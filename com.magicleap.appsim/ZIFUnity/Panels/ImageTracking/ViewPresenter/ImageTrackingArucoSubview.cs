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
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ImageTrackingArucoSubview : ImageTrackingSubView
    {
        private const string SubviewPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/ImageTargetArucoTemplate.uxml";

        private DropdownField dictionaryField = null;
        private SliderInt idSlider = null;
        private SliderInt markerLengthSlider = null;

        public new ImageTargetBuilderWrapper BondedObject => base.BondedObject;

        public ImageTrackingArucoSubview(ImageTargetBuilderWrapper bondedObject) : base(bondedObject)
        {
        }

        public override void SetEnabled(bool status)
        {
            idSlider.SetEnabled(status);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(dictionaryField, nameof(dictionaryField));
            Assert.IsNotNull(idSlider, nameof(idSlider));
            Assert.IsNotNull(markerLengthSlider, nameof(markerLengthSlider));
        }

        protected override void BindUIElements()
        {
            dictionaryField = Root.Q<DropdownField>("Dictionary-dropdown");
            idSlider = Root.Q<SliderInt>("ID-slider");
            markerLengthSlider = Root.Q<SliderInt>("MarkerLength-slider");
        }

        protected override void Initialize()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SubviewPath);
            Root = template.Instantiate();
            base.Initialize();
        }

        protected override void RegisterUICallbacks()
        {
            dictionaryField.RegisterValueChangedCallback(OnDictionaryChangedCallback);
            idSlider.RegisterValueChangedCallback(OnIDChangedCallback);
            markerLengthSlider.RegisterValueChangedCallback(OnMarkerLengthChangedCallback);
        }

        protected override void SynchronizeViewWithState()
        {
            dictionaryField.choices = CreateDictionaryFieldChoiceList();
            dictionaryField.SetValueWithoutNotify(BondedObject.ArucoDictionary.ToString());
            idSlider.SetValueWithoutNotify(BondedObject.ArucoId);
            idSlider.highValue = DictionaryToCapacity(Enum.Parse<ArucoDictionaryName>(dictionaryField.value)) - 1;
            markerLengthSlider.SetValueWithoutNotify(BondedObject.MarkerLength); 

            List<string> CreateDictionaryFieldChoiceList()
            {
                Type choiceType = typeof(ArucoDictionaryName);
                List<string> choiceList = new List<string>(Enum.GetNames(choiceType));
                choiceList.Remove(Enum.GetName(choiceType, ArucoDictionaryName.NotInitialized));
                return choiceList;
            }
        }

        protected override void UnregisterUICallbacks()
        {
            dictionaryField.UnregisterValueChangedCallback(OnDictionaryChangedCallback);
            idSlider.UnregisterValueChangedCallback(OnIDChangedCallback);
            markerLengthSlider.UnregisterValueChangedCallback(OnMarkerLengthChangedCallback);
        }

        private void OnDictionaryChangedCallback(ChangeEvent<string> evt)
        {
            BondedObject.ArucoDictionary = Enum.Parse<ArucoDictionaryName>(evt.newValue);
            idSlider.highValue = DictionaryToCapacity(Enum.Parse<ArucoDictionaryName>(dictionaryField.value)) - 1;
        }

        private void OnIDChangedCallback(ChangeEvent<int> evt)
        {
            BondedObject.ArucoId = evt.newValue;
        }

        private void OnMarkerLengthChangedCallback(ChangeEvent<int> evt)
        {
            BondedObject.MarkerLength = evt.newValue;
        }

        private int DictionaryToCapacity(ArucoDictionaryName dictionary)
        {
            switch(dictionary)
            {
                case ArucoDictionaryName.DICT_4X4_50:
                case ArucoDictionaryName.DICT_5X5_50:
                case ArucoDictionaryName.DICT_6X6_50:
                case ArucoDictionaryName.DICT_7X7_50:
                    return 50;
                case ArucoDictionaryName.DICT_4X4_100:
                case ArucoDictionaryName.DICT_5X5_100:
                case ArucoDictionaryName.DICT_6X6_100:
                case ArucoDictionaryName.DICT_7X7_100:
                    return 100;
                case ArucoDictionaryName.DICT_4X4_250:
                case ArucoDictionaryName.DICT_5X5_250:
                case ArucoDictionaryName.DICT_6X6_250:
                case ArucoDictionaryName.DICT_7X7_250:
                    return 250;
                case ArucoDictionaryName.DICT_4X4_1000:
                case ArucoDictionaryName.DICT_5X5_1000:
                case ArucoDictionaryName.DICT_6X6_1000:
                case ArucoDictionaryName.DICT_7X7_1000:
                    return 1000;
                case ArucoDictionaryName.DICT_ARUCO_ORIGINAL:
                    return 1024;
                case ArucoDictionaryName.DICT_APRILTAG_16h5:
                    return 30;
                case ArucoDictionaryName.DICT_APRILTAG_25h9:
                    return 35;
                case ArucoDictionaryName.DICT_APRILTAG_36h10:
                    return 2320;
                case ArucoDictionaryName.DICT_APRILTAG_36h11:
                    return 587;
                default:
                    return -1;
            }
        }
    }
}
