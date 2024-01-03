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
    internal sealed partial class ControllerViewPresenter : ResettableTransformViewPresenter<ControllerViewState>
    {
        public event Action<InputControllerButton, bool> ActionButtonStateChanged;

        public event Action<bool> ControllerFollowHeadPoseChanged;
        public event Action<Vector3> ControllerOrientationChanged;
        public event Action<Vector3> ControllerPositionChanged;

        public event Action<InputControllerType> ControllerTypeChanged;
        public event Action<float> GestureAngleChanged;
        public event Action<InputControllerTouchpadGestureDirection> GestureDirectionChanged;
        public event Action<float> GestureDistanceChanged;
        public event Action<Vector3> GesturePositionChanged;
        public event Action<float> GestureRadiusChanged;
        public event Action<float> GestureSpeedChanged;
        public event Action<InputControllerTouchpadGestureState> GestureStateChanged;
        public event Action<InputControllerTouchpadGestureType> GestureTypeChanged;

        public event Action<bool> InputConnectionChanged;
        public event Action<InputControllerSnapToHandposeMode> SnapToHandPoseChanged;
        public event Action<Vector3> TouchpadPositionAndForceChanged;

        public event Action<bool> TouchpadStateChanged;

        public event Action<float> TriggerValueChanged;
        
        private VisualElement actionsContent;
        private Foldout actionsFoldout;

        private FloatField angleField;

        private Slider appliedForceSlider;
        private VisualElement backButton;
        private Label backText;
        private VisualElement bumperButton;
        private Label bumperText;
        private EnumField controlDoFSelector;
        private Vector3Field controllerOrientationField;

        private Vector3Field controllerPositionField;
        private EnumField controlTypeSelector;
        private FloatField distanceField;

        private Toggle followHeadPoseToggle, touchpadHoldToggle, actionHoldToggle, connectedToggle;
        private EnumField gestureDirectionSelector;
        private Vector3Field gesturePositionField;
        private VisualElement gesturesContent;
        private Foldout gesturesFoldout;
        private EnumField gestureStateSelector;
        private EnumField gestureTypeSelector;
        private VisualElement menuButton;
        private Label menuText;
        private Slider radiusSlider;

        private EnumField snapToHandPoseSelector;
        private FloatField speedField;

        private ControllerTouchpad touchPad;
        private VisualElement touchpadContent;
        private Foldout touchpadFoldout;

        private IMGUIContainer touchpadGUIContainer;
        private Vector3Field touchpadPositionField;

        private VisualElement transformContent;

        private Foldout transformFoldout;

        private VisualElement triggerButton;
        private Slider triggerSlider;

        private Label triggerText;
        private bool IsTouchPadHeld => touchpadHoldToggle.value || touchPad.IsTouchPadHeld;

        private Vector3 TouchpadPosAndForce
        {
            get
            {
                return new Vector3(touchpadPositionField.value.x, touchpadPositionField.value.y, IsTouchPadHeld ? appliedForceSlider.value : 0);
            }
        }

        public void DisableAllControls()
        {
            SetInteractable(false);
        }

        public override void OnEnable(VisualElement root)
        {
            touchPad = new ControllerTouchpad();

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Controller/Views/ZIControllerView.uxml");

            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Controller/Views/ZIControllerDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Controller/Views/ZIControllerLightStyle.uss");

            InitEnums();
            DisableAllControls();
        }

        public void SetBumperButtonState(bool isHeld)
        {
            State.IsBumperButtonHeld = isHeld;
            SetButtonSelected(bumperButton, isHeld);
        }

        public void SetControllerOrientation(Quaternion orientation)
        {
            controllerOrientationField.SetValueWithoutNotify(orientation.eulerAngles.RoundToDisplay());
        }

        public void SetControllerPosition(Vector3 position)
        {
            controllerPositionField.SetValueWithoutNotify(position.RoundToDisplay());
        }

        public void SetControllerType(InputControllerType type)
        {
            controlTypeSelector.SetValueWithoutNotify(type);
        }

        public void SetControllerTypeEnabled(bool isEnabled)
        {
            controlTypeSelector.SetEnabled(isEnabled);
        }

        public void SetFollowHeadPose(bool value)
        {
            followHeadPoseToggle.SetValueWithoutNotify(value);
        }

        public void SetGestureAngle(float angle)
        {
            angleField.SetValueWithoutNotify(angle);
        }

        public void SetGestureDirection(InputControllerTouchpadGestureDirection direction)
        {
            gestureDirectionSelector.SetValueWithoutNotify(direction);
        }

        public void SetGestureDistance(float distance)
        {
            distanceField.SetValueWithoutNotify(distance);
        }

        public void SetGesturePositionAndForce(Vector3 positionAndForce)
        {
            gesturePositionField.SetValueWithoutNotify(positionAndForce.RoundToDisplay());
        }

        public void SetGestureRadius(float radius)
        {
            radiusSlider.SetValueWithoutNotify(radius);
        }

        public void SetGestureSpeed(float speed)
        {
            speedField.SetValueWithoutNotify(speed);
        }

        public void SetGestureState(InputControllerTouchpadGestureState state)
        {
            gestureStateSelector.SetValueWithoutNotify(state);
        }

        public void SetGestureType(InputControllerTouchpadGestureType type)
        {
            gestureTypeSelector.SetValueWithoutNotify(type);
        }

        public void SetInputConnection(bool isConnected)
        {
            connectedToggle.SetValueWithoutNotify(isConnected);
            SetControllerTypeEnabled(!isConnected);
        }

        public void SetMenuButtonState(bool isHeld)
        {
            State.IsMenuButtonHeld = isHeld;
            SetButtonSelected(menuButton, isHeld);
        }

        public void SetInteractable(bool isActive)
        {
            connectedToggle.SetEnabled(isActive);
            transformContent.SetEnabled(isActive);
            touchpadContent.SetEnabled(isActive);
            actionsContent.SetEnabled(isActive);
            gesturesContent.SetEnabled(isActive);
            controlTypeSelector.SetEnabled(isActive);
            controlDoFSelector.SetEnabled(isActive);
            triggerButton.SetEnabled(isActive);
            bumperButton.SetEnabled(isActive);
            menuButton.SetEnabled(isActive);
            backButton.SetEnabled(isActive);
            triggerText.SetEnabled(isActive);
            bumperText.SetEnabled(isActive);
            menuText.SetEnabled(isActive);
            backText.SetEnabled(isActive);
            touchpadGUIContainer.SetEnabled(isActive);
            touchpadHoldToggle.SetEnabled(isActive);
            actionHoldToggle.SetEnabled(isActive);
            controllerPositionField.SetEnabled(isActive);
            controllerOrientationField.SetEnabled(isActive);
            touchpadPositionField.SetEnabled(isActive);
            gesturePositionField.SetEnabled(isActive);
            snapToHandPoseSelector.SetEnabled(isActive);
            gestureTypeSelector.SetEnabled(isActive);
            gestureStateSelector.SetEnabled(isActive);
            gestureDirectionSelector.SetEnabled(isActive);
            appliedForceSlider.SetEnabled(isActive);
            triggerSlider.SetEnabled(isActive);
            speedField.SetEnabled(isActive);
            distanceField.SetEnabled(isActive);
            radiusSlider.SetEnabled(isActive);
            angleField.SetEnabled(isActive);
        }

        public void DisableDeviceFields(bool isDeviceMode)
        {
            if (!isDeviceMode)
                return;
            
            snapToHandPoseSelector.SetEnabled(false);
            followHeadPoseToggle.SetEnabled(false);
        }

        public void SetControllerEnabled(bool isEnabled)
        {
            followHeadPoseToggle.SetEnabled(isEnabled);
            controllerPositionField.SetEnabled(isEnabled);
            controllerOrientationField.SetEnabled(isEnabled);
        }

        public void SetSnapToHandPoseEnabled(bool isEnabled)
        {
            snapToHandPoseSelector.SetEnabled(isEnabled);
        }

        public void SetSnapToHandPose(InputControllerSnapToHandposeMode mode)
        {
            snapToHandPoseSelector.SetValueWithoutNotify(mode);
        }

        public void SetTouchpadPositionAndForce(Vector3 positionAndForce)
        {
            touchpadPositionField.SetValueWithoutNotify(positionAndForce.RoundToDisplay());
            touchPad.SetCursorPosition(new Vector2(touchpadPositionField.value.x, touchpadPositionField.value.y));
            touchPad.IsTouchpadHeldExternally = positionAndForce.z != 0;
        }

        public void SetTriggerValue(float value)
        {
            triggerSlider.SetValueWithoutNotify(value);
            SetTriggerButtonState(value == 1);
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            Assert.IsNotNull(transformContent, nameof(transformContent));
            Assert.IsNotNull(touchpadContent, nameof(touchpadContent));
            Assert.IsNotNull(actionsContent, nameof(actionsContent));
            Assert.IsNotNull(gesturesContent, nameof(gesturesContent));
            Assert.IsNotNull(followHeadPoseToggle, nameof(followHeadPoseToggle));
            Assert.IsNotNull(touchpadHoldToggle, nameof(touchpadHoldToggle));
            Assert.IsNotNull(actionHoldToggle, nameof(actionHoldToggle));
            Assert.IsNotNull(connectedToggle, nameof(connectedToggle));
            Assert.IsNotNull(transformFoldout, nameof(transformFoldout));
            Assert.IsNotNull(touchpadFoldout, nameof(touchpadFoldout));
            Assert.IsNotNull(actionsFoldout, nameof(actionsFoldout));
            Assert.IsNotNull(gesturesFoldout, nameof(gesturesFoldout));
            Assert.IsNotNull(controllerPositionField, nameof(controllerPositionField));
            Assert.IsNotNull(controllerOrientationField, nameof(controllerOrientationField));
            Assert.IsNotNull(touchpadPositionField, nameof(touchpadPositionField));
            Assert.IsNotNull(gesturePositionField, nameof(gesturePositionField));
            Assert.IsNotNull(snapToHandPoseSelector, nameof(snapToHandPoseSelector));
            Assert.IsNotNull(gestureTypeSelector, nameof(gestureTypeSelector));
            Assert.IsNotNull(gestureStateSelector, nameof(gestureStateSelector));
            Assert.IsNotNull(gestureDirectionSelector, nameof(gestureDirectionSelector));
            Assert.IsNotNull(controlTypeSelector, nameof(controlTypeSelector));
            Assert.IsNotNull(controlDoFSelector, nameof(controlDoFSelector));
            Assert.IsNotNull(appliedForceSlider, nameof(appliedForceSlider));
            Assert.IsNotNull(triggerSlider, nameof(triggerSlider));
            Assert.IsNotNull(speedField, nameof(speedField));
            Assert.IsNotNull(distanceField, nameof(distanceField));
            Assert.IsNotNull(radiusSlider, nameof(radiusSlider));
            Assert.IsNotNull(angleField, nameof(angleField));
            Assert.IsNotNull(triggerButton, nameof(triggerButton));
            Assert.IsNotNull(bumperButton, nameof(bumperButton));
            Assert.IsNotNull(menuButton, nameof(menuButton));
            Assert.IsNotNull(backButton, nameof(backButton));
            Assert.IsNotNull(triggerText, nameof(triggerText));
            Assert.IsNotNull(bumperText, nameof(bumperText));
            Assert.IsNotNull(menuText, nameof(menuText));
            Assert.IsNotNull(backText, nameof(backText));
            Assert.IsNotNull(touchpadGUIContainer, nameof(touchpadGUIContainer));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();
            followHeadPoseToggle = Root.Q<Toggle>("FollowHeadPose");
            touchpadHoldToggle = Root.Q<Toggle>("HoldToggle");
            actionHoldToggle = Root.Q<Toggle>("ActionHoldToggle");
            connectedToggle = Root.Q<Toggle>("Connected");

            transformFoldout = Root.Q<Foldout>("Foldout-Transform");
            touchpadFoldout = Root.Q<Foldout>("Foldout-Touchpad");
            actionsFoldout = Root.Q<Foldout>("Foldout-Actions");
            gesturesFoldout = Root.Q<Foldout>("Foldout-Gestures");

            transformContent = transformFoldout.Q<VisualElement>("unity-content");
            touchpadContent = touchpadFoldout.Q<VisualElement>("unity-content");
            actionsContent = actionsFoldout.Q<VisualElement>("unity-content");
            gesturesContent = gesturesFoldout.Q<VisualElement>("unity-content");

            controllerPositionField = Root.Q<Vector3Field>("TransformPosition");
            controllerOrientationField = Root.Q<Vector3Field>("TransformOrientation");
            touchpadPositionField = Root.Q<Vector3Field>("TouchPosAndForceFields");
            gesturePositionField = Root.Q<Vector3Field>("PositionAndForceFields");

            snapToHandPoseSelector = Root.Q<EnumField>("SnapToHandPose");
            gestureTypeSelector = Root.Q<EnumField>("GestureType");
            gestureStateSelector = Root.Q<EnumField>("GestureState");
            gestureDirectionSelector = Root.Q<EnumField>("GestureDirection");
            controlTypeSelector = Root.Q<EnumField>("TypeSelector");
            controlDoFSelector = Root.Q<EnumField>("DoFSelector");

            appliedForceSlider = Root.Q<Slider>("AppliedForce");
            triggerSlider = Root.Q<Slider>("TriggerSlider");
            radiusSlider = Root.Q<Slider>("Radius");

            speedField = Root.Q<FloatField>("Speed");
            distanceField = Root.Q<FloatField>("Distance");
            angleField = Root.Q<FloatField>("Angle");

            triggerButton = Root.Q<VisualElement>("TriggerButton");
            bumperButton = Root.Q<VisualElement>("BumperButton");
            menuButton = Root.Q<VisualElement>("MenuButton");
            backButton = Root.Q<VisualElement>("BackButton");

            triggerText = Root.Q<Label>("TriggerText");
            bumperText = Root.Q<Label>("BumperText");
            menuText = Root.Q<Label>("MenuText");
            backText = Root.Q<Label>("BackText");

            touchpadGUIContainer = Root.Q<IMGUIContainer>("TouchpadGUI");

            touchPad.Initialize(touchpadGUIContainer);

            touchpadGUIContainer.onGUIHandler = touchPad.OnGUI;
        }

        protected override void Serialize()
        {
            State.IsTransformOpen = transformFoldout.value;
            State.IsTouchpadOpen = touchpadFoldout.value;
            State.IsActionsOpen = actionsFoldout.value;
            State.IsGesturesOpen = gesturesFoldout.value;
            State.TouchPadForce = appliedForceSlider.value;
            base.Serialize();
        }

        protected override void SynchronizeViewWithState()
        {
            transformFoldout.value = State.IsTransformOpen;
            touchpadFoldout.value = State.IsTouchpadOpen;
            actionsFoldout.value = State.IsActionsOpen;
            gesturesFoldout.value = State.IsGesturesOpen;
            appliedForceSlider.value = State.TouchPadForce;

            actionHoldToggle.value = State.IsActionHoldToggled;
            SetTriggerButtonState(State.IsTriggerButtonHeld);
            SetMenuButtonState(State.IsMenuButtonHeld);
            SetBumperButtonState(State.IsBumperButtonHeld);
            SetBackButtonState(State.IsBackButtonHeld);
        }

        public override void ClearFields()
        {
            snapToHandPoseSelector.SetValueWithoutNotify(InputControllerSnapToHandposeMode.Disabled);
            gestureTypeSelector.SetValueWithoutNotify(InputControllerTouchpadGestureType.None);
            gestureStateSelector.SetValueWithoutNotify(InputControllerTouchpadGestureState.End);
            gestureDirectionSelector.SetValueWithoutNotify(InputControllerTouchpadGestureDirection.None);
            controlTypeSelector.SetValueWithoutNotify(InputControllerType.Device);
            
            controllerPositionField.SetValueWithoutNotify(Vector3.zero);
            controllerOrientationField.SetValueWithoutNotify(Vector3.zero);
            touchpadPositionField.SetValueWithoutNotify(Vector3.zero);
            gesturePositionField.SetValueWithoutNotify(Vector3.zero);
            triggerSlider.SetValueWithoutNotify(0);
            appliedForceSlider.SetValueWithoutNotify(0);
            
            speedField.SetValueWithoutNotify(0);
            distanceField.SetValueWithoutNotify(0);
            radiusSlider.SetValueWithoutNotify(0);
            angleField.SetValueWithoutNotify(0);
        }

        private void InitEnums()
        {
            snapToHandPoseSelector.Init(InputControllerSnapToHandposeMode.Disabled);
            gestureTypeSelector.Init(InputControllerTouchpadGestureType.None);
            gestureStateSelector.Init(InputControllerTouchpadGestureState.End);
            gestureDirectionSelector.Init(InputControllerTouchpadGestureDirection.None);
            controlTypeSelector.Init(InputControllerType.Device);
        }

        private void SetBackButtonState(bool isHeld)
        {
            State.IsBackButtonHeld = isHeld;
            SetButtonSelected(backButton, isHeld);
        }

        private void SetButtonSelected(VisualElement button, bool isHeld)
        {
            if (isHeld)
            {
                button.AddToClassList("gradientBackground");
                return;
            }

            button.RemoveFromClassList("gradientBackground");
        }

        private void SetTriggerButtonState(bool isHeld)
        {
            State.IsTriggerButtonHeld = isHeld;
            SetButtonSelected(triggerButton, isHeld);
        }
    }
}
