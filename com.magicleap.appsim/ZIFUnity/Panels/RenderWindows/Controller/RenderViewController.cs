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

namespace MagicLeap.ZI
{
    internal abstract class RenderViewController<TViewModel, TViewPresenter, TViewState> : ViewController<TViewModel, TViewPresenter>
        where TViewModel : RenderViewModel, new()
        where TViewPresenter : RenderViewPresenter<TViewState>, new()
        where TViewState : RenderViewStateBase, new()
    {
        private bool queueFocus;

        protected override void Update()
        {
            base.Update();
            Presenter.QueueRepaintEvent();
        }

        protected virtual void OnDisable()
        {
            Presenter.OnMouseButton -= MouseButton;
            Presenter.OnMouseMove -= MouseMove;
            Presenter.OnMouseScroll -= MouseScroll;
            Presenter.OnKeyPressed -= KeyPressed;

            Presenter.OnDisable();

            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.OnConnectionStatusChanged -= OnConnectionStatusMessageChanged;

            Model.UnInitialize();
        }

        protected void OnInspectorUpdate()
        {
            Presenter.IsWindowDocked = docked;
            if (!queueFocus)
                return;
            
            queueFocus = false;
            OnFocus();
        }

        private void OnFocus()
        {
            if (Model != null)
            {
                Presenter.ReceiveFocus();
            }
            else
            {
                queueFocus = true;
            }
        }

        private void OnLostFocus()
        {
            if (Model != null)
            {
                Presenter.ReleaseFocus();
            }
        }

        protected override void Initialize()
        {
            Presenter.OnMouseButton += MouseButton;
            Presenter.OnMouseMove += MouseMove;
            Presenter.OnMouseScroll += MouseScroll;
            Presenter.OnKeyPressed += KeyPressed;

            Presenter.OnEnable(rootVisualElement);

            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;
            Model.OnConnectionStatusChanged += OnConnectionStatusMessageChanged;
            base.Initialize();
        }

        private void KeyPressed(bool pressed, KeyCode key, KeyModifiers modifiers)
        {
            Model.PostKeyCode(pressed, key, modifiers);
        }

        private void MouseButton(bool pressed, MouseButton mouseButton, KeyModifiers modifierKeys)
        {
            Model.PostMouseButton(pressed, mouseButton, modifierKeys);
        }

        private void MouseMove(float x, float y, KeyModifiers modifierKeys)
        {
            Model.PostMouseMove(x, y, modifierKeys);
        }

        private void MouseScroll(float x, float y, KeyModifiers modifierKeys)
        {
            Model.PostMouseScroll(x, y, modifierKeys);
        }

        private void OnSessionStarted()
        {
            Presenter.StartRendering();
            Presenter.ToggleToolbarButtonsEnabled(true);
            Presenter.ToggleConnectionStatusPanelDisplay(false);
        }

        private void OnSessionStopped()
        {
            Presenter.StopRendering();
            Presenter.ToggleToolbarButtonsEnabled(false);
            Presenter.ToggleConnectionStatusPanelDisplay(true);
        }

        private void OnConnectionStatusMessageChanged()
        {
            Presenter.SwitchConnectionStatusMessage(Model.IsDeviceMode);
        }
    }
}
