// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ImageTrackingFoldout : ObjectView<ImageTargetBuilderWrapper>
    {
        private const string TemplatePath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/ImageTargetTemplate.uxml";

        private event Action<string> OnCloneClicked = null;
        private event Action<string> OnDeleteClicked = null;

        private VisualElement container = null;
        private Button cloneButton = null;
        private Button deleteButton = null;
        private DropdownField typeField = null;
        private bool foldOut = false;
        private VisualElement icon = null;
        private Label name = null;
        private Vector3Field orientation = null;
        private Vector3Field position = null;

        private Toggle toggle = null;
        private VisualElement trackingStateIndicator = null;

        private VisualElement typeSubview = null;
        
        public new ImageTargetBuilderWrapper BondedObject => base.BondedObject;

        private ImageTrackingSubView imageTrackingSubView;

        public ImageTrackingFoldout(
            ImageTargetBuilderWrapper bondedObject,
            Action<string> onCloneClicked = null,
            Action<string> onDeleteClicked = null) : base(bondedObject)
        {
            OnCloneClicked = onCloneClicked;
            OnDeleteClicked = onDeleteClicked;
        }

        public override void SetEnabled(bool status)
        {
            cloneButton.SetEnabled(status);
            deleteButton.SetEnabled(status);
            typeField.SetEnabled(status);
            position.SetEnabled(status);
            orientation.SetEnabled(status);
        }

        public void SynchronizeViewWithStateExposer()
        {
            SynchronizeViewWithState();
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(name, nameof(name));
            Assert.IsNotNull(cloneButton, nameof(cloneButton));
            Assert.IsNotNull(deleteButton, nameof(deleteButton));
            Assert.IsNotNull(toggle, nameof(toggle));
            Assert.IsNotNull(container, nameof(container));
            Assert.IsNotNull(icon, nameof(icon));
            Assert.IsNotNull(typeField, nameof(typeField));
            Assert.IsNotNull(position, nameof(position));
            Assert.IsNotNull(orientation, nameof(orientation));
            Assert.IsNotNull(trackingStateIndicator, nameof(trackingStateIndicator));
        }

        protected override void BindUIElements()
        {
            name = Root.Q<Label>("Name");
            cloneButton = Root.Q<Button>("Clone-button");
            deleteButton = Root.Q<Button>("Delete-button");
            toggle = Root.Q<Toggle>("toggle");
            container = Root.Q<VisualElement>("unity-content");
            icon = Root.Q<VisualElement>("Icon");
            typeField = Root.Q<DropdownField>("Type");
            position = Root.Q<Vector3Field>("Position");
            orientation = Root.Q<Vector3Field>("Orientation");
            trackingStateIndicator = Root.Q<VisualElement>("TrackingState");
            typeSubview = Root.Q<VisualElement>("Subview");
        }

        protected override void Initialize()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);
            Root = template.Instantiate();

            base.Initialize();

            RenderSubview(typeField.value);
        }

        protected override void RegisterUICallbacks()
        {
            cloneButton.clicked += OnCloneButtonClicked;
            deleteButton.clicked += OnDeleteButtonClicked;
            position.RegisterValueChangedCallback(PositionChangeCallback);
            orientation.RegisterValueChangedCallback(OrientationChangeCallback);
            typeField.RegisterValueChangedCallback(ImageTypeChangeCallback);
            toggle.RegisterValueChangedCallback(OnFoldoutToggleChanged);
            BondedObject.OnImageTargetTypeChanged += SetImageTargetType;
        }

        protected override void SynchronizeViewWithState()
        {
            name.text = BondedObject.Name;
            typeField.choices = CreateTypeFieldChoiceList();
            typeField.SetValueWithoutNotify(BondedObject.ImageTargetType.ToString());
            position.SetValueWithoutNotify(BondedObject.Position.RoundToDisplay());
            orientation.SetValueWithoutNotify(BondedObject.Euler.RoundToDisplay());

            if (BondedObject.HasTexture)
            {
                Texture2D texture = BondedObject.Texture as Texture2D;
                if (texture != null)
                {
                    icon.style.backgroundImage = texture;
                    float aspectRatio = (float)texture.width / (float)texture.height;
                    var width = new StyleLength(new Length(aspectRatio < 1f ? aspectRatio * 100f : 100f, LengthUnit.Percent));
                    var height = new StyleLength(new Length(aspectRatio > 1f ? 1f / aspectRatio * 100f : 100f, LengthUnit.Percent));
                    icon.style.width = width;
                    icon.style.height = height;
                }
            }

            trackingStateIndicator.RemoveFromClassList(trackingStateIndicator.GetClasses()
                                                                             .FirstOrDefault(x => x.StartsWith("trackingState__")));
            trackingStateIndicator.AddToClassList($"trackingState__{BondedObject.TrackingStatus}");
            trackingStateIndicator.tooltip = BondedObject.GetTrackingStatus();

            container.SetDisplay(foldOut);
            
            List<string> CreateTypeFieldChoiceList()
            {
                Type choiceType = typeof(ImageTargetType);
                List<string> choiceList = new List<string>(Enum.GetNames(choiceType));
                choiceList.Remove(Enum.GetName(choiceType, ImageTargetType.Unknown));
                choiceList.Remove(Enum.GetName(choiceType, ImageTargetType.Image));
                return choiceList;
            }
            imageTrackingSubView?.SynchronizeStateExposer();
        }

        protected override void UnregisterUICallbacks()
        {
            cloneButton.clicked -= OnCloneButtonClicked;
            deleteButton.clicked -= OnDeleteButtonClicked;
            position.UnregisterValueChangedCallback(PositionChangeCallback);
            orientation.UnregisterValueChangedCallback(OrientationChangeCallback);
            typeField.UnregisterValueChangedCallback(ImageTypeChangeCallback);
            toggle.UnregisterValueChangedCallback(OnFoldoutToggleChanged);
            BondedObject.OnImageTargetTypeChanged -= SetImageTargetType;
        }

        private void ImageTypeChangeCallback(ChangeEvent<string> evt)
        {
            BondedObject.ImageTargetType = Enum.Parse<ImageTargetType>(evt.newValue);
        }

        private void RenderSubview(string type)
        {
            ImageTargetType targetType = ImageTargetType.Unknown;
            try
            {
                targetType = Enum.Parse<ImageTargetType>(type);

                ClearSubview();
                BondedObject.ImageTargetType = targetType;
                VisualElement subview = default;
                imageTrackingSubView = null;

                switch (targetType)
                {
                    case ImageTargetType.Aruco:
                        imageTrackingSubView = new ImageTrackingArucoSubview(BondedObject);
                        break;
                    case ImageTargetType.Barcode_EAN_13:
                        imageTrackingSubView = new ImageTrackingBarcodeEAN13Subview(BondedObject);
                        break;
                    case ImageTargetType.Barcode_UPC_A:
                        imageTrackingSubView = new ImageTrackingBarcodeUPCASubview(BondedObject);
                        break;
                    case ImageTargetType.Image:
                        imageTrackingSubView = new ImageTrackingImageSubview(BondedObject);
                        break;
                    case ImageTargetType.QRCode:
                        imageTrackingSubView = new ImageTrackingQRSubview(BondedObject);
                        break;
                }

                if (imageTrackingSubView != null)
                {
                    imageTrackingSubView.Init();
                    subview = imageTrackingSubView.Root.Q<VisualElement>("ImageSubview");
                    typeSubview.Add(subview);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Image Target Type parsing failed with exception {exception}");
                throw;
            }
        }

        private void OnCloneButtonClicked()
        {
            OnCloneClicked?.Invoke(BondedObject.NodeId);
        }

        private void OnDeleteButtonClicked()
        {
            OnDeleteClicked?.Invoke(BondedObject.NodeId);
            OnDeleteClicked = null;
        }

        private void OnFoldoutToggleChanged(ChangeEvent<bool> evt)
        {
            foldOut = evt.newValue;
            container.SetDisplay(foldOut);
        }

        private void OrientationChangeCallback(ChangeEvent<Vector3> evt)
        {
            BondedObject.Euler = evt.newValue;
        }

        private void PositionChangeCallback(ChangeEvent<Vector3> evt)
        {
            BondedObject.Position = evt.newValue;
        }

        private void ClearSubview()
        {
            VisualElement subview = typeSubview.Q<VisualElement>("ImageSubview");
            if (subview != null)
            {
                subview.RemoveFromHierarchy();
            }
            typeSubview.Clear();
            imageTrackingSubView = null;
        }

        private void SetImageTargetType(string _)
        {
            string targetType = BondedObject.ImageTargetType.ToString();
            typeField.SetValueWithoutNotify(targetType);
            RenderSubview(targetType);
        }
    }
}
