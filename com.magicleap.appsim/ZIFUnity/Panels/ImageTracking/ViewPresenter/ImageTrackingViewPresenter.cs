// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using ml.zi;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class ImageTrackingViewPresenter : ViewPresenter<ImageTrackingViewState>
    {
        private const string ViewsPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/";
        private const string MainView = "ZIImageTrackingView.uxml";
        private const string DarkStyle = "ImageTargetTemplateDarkStyle.uss";
        private const string LightStyle = "ImageTargetTemplateLightStyle.uss";

        public event Action<string> NewMarkerTypeChanged;
        public event Action<bool> FollowHeadPoseChanged;
        public event Action<string> ImageTargetCloned;
        public event Action<string> ImageTargetRemoved;
        public event Action<string> NewImageTargetCreated;

        private DropdownField newMarkerTypeField = null;
        private Button addImageTargetButton = null;
        private Toggle followHeadToggle = null;

        private GenericListView<ImageTrackingFoldout> listPresenter = null;
        private ScrollView scrollView = null;

        public override void OnEnable(VisualElement root)
        {
            string visualTreePath = Path.Combine(ViewsPath, MainView);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);

            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(Path.Combine(ViewsPath, DarkStyle), Path.Combine(ViewsPath, LightStyle));
        }

        public void OnFollowHeadPoseUpdated(bool enabled)
        {
            followHeadToggle.SetValueWithoutNotify(enabled);
        }

        public void ActiveOnDeviceChanged(bool activeOnDevice)
        {
            followHeadToggle.SetEnabled(!activeOnDevice);
        }

        public void OnImageTargetModelUpdated(string id)
        {
            ImageTrackingFoldout visual = listPresenter.ObjectViews.First(x => x.BondedObject.NodeId == id);
            visual.SynchronizeViewWithStateExposer();
        }

        public void OnImageTargetsCollectionChanged(NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnCollectionElementsAdded(eventArgs.NewItems);
                break;
                case NotifyCollectionChangedAction.Remove:
                    OnCollectionElementRemoved(eventArgs.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    listPresenter.Clear();
                    break;
            }
        }

        private void OnCollectionElementsAdded(IList newItems)
        {
            // Create an visual representation for each new element.
            foreach (ImageTargetBuilderWrapper imageTarget in newItems)
            {
                ImageTrackingFoldout template = new ImageTrackingFoldout(imageTarget, CloneImageTarget, RemoveImageTarget);
                listPresenter.AddToView(template);
            }
        }

        private void OnCollectionElementRemoved(IList oldItems)
        {
            // Remove items which were deleted.
            foreach (ImageTargetBuilderWrapper imageTarget in oldItems)
            {
                ImageTrackingFoldout removedFoldout = listPresenter.ObjectViews.First(x => x.BondedObject == imageTarget);
                listPresenter.RemoveFromView(removedFoldout);
            }
        }
        
        public void SetEnabled(bool state)
        {
            newMarkerTypeField.SetEnabled(state);
            addImageTargetButton.SetEnabled(state);
            listPresenter.SetEnabled(state);
            followHeadToggle.SetEnabled(state);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(newMarkerTypeField, nameof(newMarkerTypeField));
            Assert.IsNotNull(addImageTargetButton, nameof(addImageTargetButton));
            Assert.IsNotNull(followHeadToggle, nameof(followHeadToggle));
            Assert.IsNotNull(scrollView, nameof(scrollView));
            Assert.IsNotNull(listPresenter.VisualElement, nameof(listPresenter.VisualElement));
        }

        protected override void BindUIElements()
        {
            newMarkerTypeField = Root.Q<DropdownField>("NewMarkerType-dropdown");
            addImageTargetButton = Root.Q<Button>("AddTarget-button");
            followHeadToggle = Root.Q<Toggle>("FollowHead-toggle");
            scrollView = Root.Q<ScrollView>("ScrollView");
            listPresenter = new GenericListView<ImageTrackingFoldout>(scrollView);
        }

        protected override void RegisterUICallbacks()
        {
            newMarkerTypeField.RegisterValueChangedCallback(NewMarkerTypeChangedCallback);
            addImageTargetButton.clicked += OnAddImageTargetButtonDown;
            followHeadToggle.RegisterValueChangedCallback(FollowHeadToggleCallback);
        }

        protected override void SynchronizeViewWithState()
        {
            newMarkerTypeField.choices = CreateTypeFieldChoiceList();
            newMarkerTypeField.index = 0;

            List<string> CreateTypeFieldChoiceList()
            {
                Type choiceType = typeof(ImageTargetType);
                List<string> choiceList = new List<string>(Enum.GetNames(choiceType));
                choiceList.Remove(Enum.GetName(choiceType, ImageTargetType.Unknown));
                choiceList.Remove(Enum.GetName(choiceType, ImageTargetType.Image));
                return choiceList;
            }
        }

        protected override void UnregisterUICallbacks()
        {
            newMarkerTypeField.UnregisterValueChangedCallback(NewMarkerTypeChangedCallback);
            addImageTargetButton.clicked -= OnAddImageTargetButtonDown;
            followHeadToggle.UnregisterValueChangedCallback(FollowHeadToggleCallback);
        }

        private void NewMarkerTypeChangedCallback(ChangeEvent<string> evt)
        {
            NewMarkerTypeChanged?.Invoke(evt.newValue);
        }

        private void FollowHeadToggleCallback(ChangeEvent<bool> toggleEvent)
        {
            FollowHeadPoseChanged?.Invoke(toggleEvent.newValue);
        }

        private void OnAddImageTargetButtonDown()
        {
            NewImageTargetCreated?.Invoke(newMarkerTypeField.value);
        }

        private void CloneImageTarget(string nodeId)
        {
            ImageTargetCloned?.Invoke(nodeId);
        }

        private void RemoveImageTarget(string nodeId)
        {
            ImageTargetRemoved?.Invoke(nodeId);
        }
    }
}
