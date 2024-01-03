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
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal abstract class ResettableTransformViewPresenter<TState> : ViewPresenter<TState> where TState : IViewState, new()
    {
        protected Button resetButton;
        public event Action OnResetTransformClicked;
        private const string BUTTON_NAME = "ResetButton";

        protected override void BindUIElements()
        {
            base.BindUIElements();
            resetButton = Root.Q<Button>(BUTTON_NAME);
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            Assert.IsNotNull(resetButton, nameof(resetButton));
        }

        private void ResetClicked()
        {
            OnResetTransformClicked?.Invoke();
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            resetButton.clicked += ResetClicked;
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            resetButton.clicked -= ResetClicked;
        }
    }
}
