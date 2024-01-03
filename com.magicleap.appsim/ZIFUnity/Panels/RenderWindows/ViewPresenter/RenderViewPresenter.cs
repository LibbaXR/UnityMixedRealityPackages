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
using UnityEngine;
using UnityEngine.UIElements;
using Event = UnityEngine.Event;
using EventType = UnityEngine.EventType;
using KeyCode = ml.zi.KeyCode;
using MouseButton = ml.zi.MouseButton;

namespace MagicLeap.ZI
{
    internal abstract class RenderViewPresenter<TState> : ViewPresenter<TState> where TState : RenderViewStateBase, new()
    {
        public event Action<bool, KeyCode, KeyModifiers> OnKeyPressed;
        public event Action<bool, MouseButton, KeyModifiers> OnMouseButton;

        public event Action<float, float, KeyModifiers> OnMouseMove;
        public event Action<float, float, KeyModifiers> OnMouseScroll;

        protected Label connectionStatusLabel;

        private readonly List<int> heldMouseButtons = new();
        private ColorSpace colorSpaceForRender = ColorSpace.Uninitialized;
        private bool hasFocus;
        private KeyModifiers heldModifierKeys = KeyModifiers.None;
        private bool isRendering;

        private Rect lastRenderRect = Rect.zero;
        private IMGUIContainer renderGUIContainer;
        public abstract PeripheralInputSource InputSource { get; }
        public bool IsWindowDocked { get; set; }
        private bool AnyMouseButtonDown => heldMouseButtons.Count > 0;
        private ZIBridge.ModuleWrapper<Peripheral, PeripheralChanges> Peripheral => ZIBridge.Instance.Peripheral;
        public override void OnEnable(VisualElement root)
        {
            base.OnEnable(root);

            ToggleToolbarButtonsEnabled(false);
            ToggleConnectionStatusPanelDisplay(true);

            Root = root;
        }

        public void QueueRepaintEvent()
        {
            renderGUIContainer?.MarkDirtyRepaint();
        }

        public void ReceiveFocus()
        {
            hasFocus = true;
        }

        public void ReleaseFocus()
        {
            hasFocus = false;
            heldModifierKeys = KeyModifiers.None;
        }

        public void StartRendering()
        {
            isRendering = true;
        }

        public void StopRendering()
        {
            isRendering = false;
        }

        public abstract void ToggleConnectionStatusPanelDisplay(bool enablePanel);

        public abstract void SwitchConnectionStatusMessage(bool isDeviceMode);

        public abstract void ToggleToolbarButtonsEnabled(bool enableButtons);

        protected override void AssertFields()
        {
            base.AssertFields();

            Assert.IsNotNull(renderGUIContainer, nameof(renderGUIContainer));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();

            connectionStatusLabel = Root.Q<Label>("ConnectionStatusLabel");
            renderGUIContainer = Root.Q<IMGUIContainer>("RenderGUI");
        }

        protected void MarkDirtyRepaint()
        {
            renderGUIContainer.MarkDirtyRepaint();
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();

            renderGUIContainer.onGUIHandler += OnViewRender;
        }

        protected void SetPickingMode(PickingMode pickingMode)
        {
            renderGUIContainer.pickingMode = pickingMode;
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();

            renderGUIContainer.onGUIHandler -= OnViewRender;
        }

        private void BlitScreen()
        {
            GL.IssuePluginEvent(ZIBridge.Instance.RenderEventPtr, (int) InputSource);
        }

        /// <summary>
        ///     Checks if the target mode should be updated on getting focus by renderer. Target mode if
        ///     should be Preserved if current target mode is set as hands/eyes/input and you right-click on a view.
        /// </summary>
        /// <param name="guiEventButton">Mouse button index.</param>
        private void FocusTargetMode(int guiEventButton)
        {
            // Left click 
            if (guiEventButton == 0)
            {
                SwitchTargetMode();
            }
            //Right click
            else
            {
                if (!Peripheral.IsHandleConnected)
                    return;
                
                PeripheralTargetMode currentTargetMode = Peripheral.Handle.GetTargetMode();

                if (currentTargetMode is PeripheralTargetMode.Control1
                    or PeripheralTargetMode.HandLeft
                    or PeripheralTargetMode.HandRight
                    or PeripheralTargetMode.EyeFixation)
                {
                    return;
                }

                SwitchTargetMode();
            }
        }

        private void HandleInputEvents(Event guiEvent)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            if (guiEvent.GetTypeForControl(controlId) == EventType.MouseDown)
            {
                // post mouse Down event
                GUIUtility.hotControl = controlId;
                heldMouseButtons.Add(guiEvent.button);
                OnMouseButton(true, Utils.ConvertMouseButton(guiEvent.button), heldModifierKeys);
                guiEvent.Use();

                // Preserve the target mode if target mode it’s hands/eyes/input and you right-click on a view.
                FocusTargetMode(guiEvent.button);
            }

            // events to check if window has focus
            switch (guiEvent.type)
            {
                case EventType.KeyDown:

                    // Post key up event only for existing keys
                    if (guiEvent.keyCode == UnityEngine.KeyCode.None)
                    {
                        return;
                    }

                    OnKeyPressed(true, Utils.ConvertKeyCode(guiEvent.keyCode), heldModifierKeys);
                    guiEvent.Use();
                    break;

                case EventType.KeyUp:

                    // Post key up event only for existing keys
                    if (guiEvent.keyCode == UnityEngine.KeyCode.None)
                    {
                        return;
                    }

                    OnKeyPressed(false, Utils.ConvertKeyCode(guiEvent.keyCode), heldModifierKeys);
                    guiEvent.Use();
                    break;

                case EventType.MouseUp:
                    // post mouse Down event
                    GUIUtility.hotControl = 0;
                    heldMouseButtons.Remove(guiEvent.button);
                    OnMouseButton(false, Utils.ConvertMouseButton(guiEvent.button), heldModifierKeys);
                    guiEvent.Use();
                    break;

                case EventType.ScrollWheel:
                    // post mouse scroll
                    OnMouseScroll(guiEvent.delta.x, guiEvent.delta.y, heldModifierKeys);
                    guiEvent.Use();
                    break;
            }

            if (IsPointerOverRender() || GUIUtility.hotControl == controlId)
            {
                if (hasFocus || !AnyMouseButtonDown)
                {
                    OnMouseMove(guiEvent.mousePosition.x, guiEvent.mousePosition.y, heldModifierKeys);
                    if (guiEvent.type != EventType.Layout)
                    {
                        guiEvent.Use();
                    }
                }
            }
        }

        private bool IsPointerOverRender()
        {
            if (!isRendering)
            {
                return false;
            }

            if (!hasFocus)
            {
                return false;
            }

            Event guiEvent = Event.current;

            if (guiEvent.type == EventType.Repaint)
            {
                return false;
            }

            VisualElement elementUnderPointer = Root.panel.Pick(guiEvent.mousePosition);
            return elementUnderPointer != null && elementUnderPointer == renderGUIContainer;
        }

        private void OnViewRender()
        {
            var guiRect = new Rect(renderGUIContainer.worldBound.x, renderGUIContainer.worldBound.y,
                renderGUIContainer.worldBound.width + (IsWindowDocked ? 1 : 0),
                renderGUIContainer.worldBound.height + (IsWindowDocked ? 2 : 0));

            Rect rect = GUIUtility.GUIToScreenRect(guiRect);
            Render(rect);
        }

        private void Render(Rect guiRect)
        {
            if (!isRendering)
            {
                return;
            }

            Event guiEvent = Event.current;
            float scale = EditorGUIUtility.pixelsPerPoint;

            heldModifierKeys = Utils.ConvertEventModifiers(guiEvent);

            if (guiEvent.type != EventType.Repaint)
            {
                if (hasFocus)
                {
                    HandleInputEvents(guiEvent);
                }

                UpdateRenderDimensions(guiRect, scale);
            }
            else
            {
                BlitScreen();
            }
        }

        private void SwitchTargetMode()
        {
            if (!Peripheral.IsHandleConnected)
                return;
            
            switch (InputSource)
            {
                case PeripheralInputSource.SceneView:
                    Peripheral.Handle.SetTargetMode(PeripheralTargetMode.SceneView);
                    break;
                case PeripheralInputSource.DeviceView:
                    Peripheral.Handle.SetTargetMode(PeripheralTargetMode.Headpose);
                    break;
            }
        }

        private void UpdateRenderDimensions(Rect screenRect, float scale)
        {
            if (screenRect != lastRenderRect)
            {
                ZIBridge.Instance.ResizeRendererViewport(InputSource, (int) screenRect.width, (int) screenRect.height, scale);
                lastRenderRect = screenRect;
            }

            if (PlayerSettings.colorSpace != colorSpaceForRender)
            {
                ZIBridge.Instance.SetUsingLinearColorSpace(PlayerSettings.colorSpace == ColorSpace.Linear);
                colorSpaceForRender = PlayerSettings.colorSpace;
            }
        }
    }
}
