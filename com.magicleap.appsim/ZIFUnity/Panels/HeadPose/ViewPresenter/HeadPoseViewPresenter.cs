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
using ml.zi;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class HeadPoseViewPresenter : AlignableTransformViewPresenter<HeadPoseViewState>
    {
        public event Action<float> OnCurrentConfidenceChanged;
        public event Action<HeadTrackingError> OnCurrentErrorChanged;
        public event Action<HeadTrackingMode> OnCurrentModeChanged;
        public event Action<HeadTrackingMapEvents> OnTriggerHeadTrackingEvent;
        private Slider confidenceSlider;
        private EnumField error;
        private float lastRefresh;

        private ToolbarToggle lostButton;
        private EnumField mode;
        private ToolbarToggle newSessionButton;
        private ToolbarToggle recoveredButton;
        private ToolbarToggle recoveryFailedButton;

        public override void OnEnable(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HeadPose/Views/ZIHeadPoseView.uxml");

            visualTree.CloneTree(root);

            base.OnEnable(root);

            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HeadPose/Views/ZIHeadPoseDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HeadPose/Views/ZIHeadPoseLightStyle.uss");

            error.Init(HeadTrackingError.None);
            mode.Init(HeadTrackingMode.Unavailable);
        }

        public void RefreshMapEventsButtons()
        {
            if (!(Time.realtimeSinceStartup - lastRefresh > 5f))
            {
                return;
            }

            lastRefresh = Time.realtimeSinceStartup;

            lostButton.SetValueWithoutNotify(false);
            recoveredButton.SetValueWithoutNotify(false);
            recoveryFailedButton.SetValueWithoutNotify(false);
            newSessionButton.SetValueWithoutNotify(false);
        }

        public void SetHeadTrackingError(HeadTrackingError error)
        {
            this.error.SetValueWithoutNotify(error);
        }

        public void SetHeadTrackingMode(HeadTrackingMode mode)
        {
            this.mode.SetValueWithoutNotify(mode);
        }

        public void SetHybridAndDeviceVisualElements(bool status)
        {
            position.SetEnabled(status);
            orientation.SetEnabled(status);
        }

        public void SetTrackingConfidence(float confidence)
        {
            confidenceSlider.SetValueWithoutNotify(confidence);
        }

        public void SetTrackingMapEvents(HeadTrackingMapEvents events)
        {
            if (events == HeadTrackingMapEvents.None)
            {
                return;
            }

            if (events.HasFlag(HeadTrackingMapEvents.Lost))
            {
                lostButton.SetValueWithoutNotify(true);
            }

            if (events.HasFlag(HeadTrackingMapEvents.Recovered))
            {
                recoveredButton.SetValueWithoutNotify(true);
            }

            if (events.HasFlag(HeadTrackingMapEvents.RecoveryFailed))
            {
                recoveryFailedButton.SetValueWithoutNotify(true);
            }

            if (events.HasFlag(HeadTrackingMapEvents.NewSession))
            {
                newSessionButton.SetValueWithoutNotify(true);
            }
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            Assert.IsNotNull(confidenceSlider, nameof(confidenceSlider));
            Assert.IsNotNull(error, nameof(error));
            Assert.IsNotNull(mode, nameof(mode));
            Assert.IsNotNull(lostButton, nameof(lostButton));
            Assert.IsNotNull(recoveredButton, nameof(recoveredButton));
            Assert.IsNotNull(recoveryFailedButton, nameof(recoveryFailedButton));
            Assert.IsNotNull(newSessionButton, nameof(newSessionButton));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();
            confidenceSlider = Root.Q<Slider>("Confidence-slider");
            error = Root.Q<EnumField>("Error-enum");
            mode = Root.Q<EnumField>("Mode-enum");

            lostButton = Root.Q<ToolbarToggle>("Lost-button");
            recoveredButton = Root.Q<ToolbarToggle>("Recovered-button");
            recoveryFailedButton = Root.Q<ToolbarToggle>("RecoveryFailed-button");
            newSessionButton = Root.Q<ToolbarToggle>("NewSession-button");
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            confidenceSlider.RegisterValueChangedCallback(ConfidencesValueChangedCallback);
            error.RegisterValueChangedCallback(ErrorValueChangedCallback);
            mode.RegisterValueChangedCallback(ModeValueChangedCallback);
            lostButton.RegisterValueChangedCallback(LostEventValueChangedCallback);
            recoveredButton.RegisterValueChangedCallback(RecoveredEventValueChangedCallback);
            recoveryFailedButton.RegisterValueChangedCallback(RecoveryFailedEventValueChangedCallback);
            newSessionButton.RegisterValueChangedCallback(NewSessionEventValueChangedCallback);
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            confidenceSlider.UnregisterValueChangedCallback(ConfidencesValueChangedCallback);
            error.UnregisterValueChangedCallback(ErrorValueChangedCallback);
            mode.UnregisterValueChangedCallback(ModeValueChangedCallback);
            lostButton.UnregisterValueChangedCallback(LostEventValueChangedCallback);
            recoveredButton.UnregisterValueChangedCallback(RecoveredEventValueChangedCallback);
            recoveryFailedButton.UnregisterValueChangedCallback(RecoveryFailedEventValueChangedCallback);
            newSessionButton.UnregisterValueChangedCallback(NewSessionEventValueChangedCallback);
        }

        public override void ClearFields()
        {
            base.ClearFields();
            confidenceSlider.SetValueWithoutNotify(0.5f);
            error.SetValueWithoutNotify(HeadTrackingError.None);
            mode.SetValueWithoutNotify(HeadTrackingMode.Unavailable);
        }
        

        private void ConfidencesValueChangedCallback(ChangeEvent<float> evt)
        {
            OnCurrentConfidenceChanged?.Invoke(evt.newValue);
        }

        private void ErrorValueChangedCallback(ChangeEvent<Enum> evt)
        {
            OnCurrentErrorChanged?.Invoke((HeadTrackingError) evt.newValue);
        }

        private void LostEventValueChangedCallback(ChangeEvent<bool> evt)
        {
            OnTriggerHeadTrackingEvent?.Invoke(HeadTrackingMapEvents.Lost);
        }

        private void ModeValueChangedCallback(ChangeEvent<Enum> evt)
        {
            OnCurrentModeChanged?.Invoke((HeadTrackingMode) evt.newValue);
        }

        private void NewSessionEventValueChangedCallback(ChangeEvent<bool> evt)
        {
            OnTriggerHeadTrackingEvent?.Invoke(HeadTrackingMapEvents.NewSession);
        }

        private void RecoveredEventValueChangedCallback(ChangeEvent<bool> evt)
        {
            OnTriggerHeadTrackingEvent?.Invoke(HeadTrackingMapEvents.Recovered);
        }

        private void RecoveryFailedEventValueChangedCallback(ChangeEvent<bool> evt)
        {
            OnTriggerHeadTrackingEvent?.Invoke(HeadTrackingMapEvents.RecoveryFailed);
        }
    }
}
