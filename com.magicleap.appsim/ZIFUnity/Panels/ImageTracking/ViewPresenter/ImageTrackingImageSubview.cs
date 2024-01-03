// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ImageTrackingImageSubview : ImageTrackingSubView
    {
        private const string SubviewPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/ImageTargetImageTemplate.uxml";

        private EnumField texture = null;
        private Label textureH = null;
        private Label textureW = null;

        public new ImageTargetBuilderWrapper BondedObject => base.BondedObject;

        public ImageTrackingImageSubview(ImageTargetBuilderWrapper bondedObject) : base(bondedObject)
        {
        }

        public override void SetEnabled(bool status)
        {
            texture.SetEnabled(status);
        }


        protected override void AssertFields()
        {
            Assert.IsNotNull(texture, nameof(texture));
            Assert.IsNotNull(textureW, nameof(textureW));
            Assert.IsNotNull(textureH, nameof(textureH));
        }

        protected override void BindUIElements()
        {
            texture = Root.Q<EnumField>("Texture");
            textureW = Root.Q<Label>("W");
            textureH = Root.Q<Label>("H");
        }

        protected override void Initialize()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SubviewPath);
            Root = template.Instantiate();

            base.Initialize();
        }

        protected override void RegisterUICallbacks()
        {
            texture.RegisterCallback<MouseDownEvent>(OnTextureFieldClick);
        }

        protected override void SynchronizeViewWithState()
        {
            texture.Q<TextElement>(className: "unity-enum-field__text").text = BondedObject.TextureName;
            textureW.text = $"W: {BondedObject.PhysicalWidth}";
            textureH.text = $"H: {BondedObject.PhysicalHeight}";
        }

        protected override void UnregisterUICallbacks()
        {
            texture.UnregisterCallback<MouseDownEvent>(OnTextureFieldClick);
        }

        private void OnTextureFieldClick(MouseDownEvent evt)
        {
            ShowTexturesDropdown(LoadedImageTextures.GetTextureNames(), BondedObject.TextureName);
        }

        private void ShowTexturesDropdown(string[] values, string picked)
        {
            if (values.Length <= 0)
            {
                Debug.LogWarning("There is any image uploaded to assign.");
                return;
            }

            var genericMenu = new GenericMenu();
            int num = Array.IndexOf(values, picked);
            for (int i = 0; i < values.Length; ++i)
            {
                bool on = num == i;
                int closureIndex = i;
                genericMenu.AddItem(new GUIContent(values[i]), on, value => TextureChangeCallback(values[closureIndex]), values[closureIndex]);
            }

            Vector2 world = Vector2.zero;
            ref Vector2 local = ref world;
            Rect layout = texture.Q<VisualElement>(className: "unity-enum-field__input").layout;
            double xMin = layout.xMin;
            layout = texture.layout;
            double height = layout.height;
            local = new Vector2((float)xMin, (float)height);
            world = texture.LocalToWorld(world);
            var position = new Rect(world, Vector2.zero);
            genericMenu.DropDown(position);
        }

        private void TextureChangeCallback(string textureName)
        {
            BondedObject.SetTextureBasedOnName(textureName);
        }
    }
}
