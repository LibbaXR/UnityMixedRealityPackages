// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal interface ISubPanelView<in TMainPanel, in TViewState> where TViewState : IViewState
    {
        public TMainPanel Panel { set; }

        public TViewState State { set; }

        public VisualElement Root { set; }

        public void RegisterUICallbacks();

        public void UnRegisterUICallbacks();

        public void BindUIElements();

        public void AssertFields();

        public void SynchronizeViewWithState();

        public void SetPanelActive(bool isEnabled);

        public void Serialize();

        public void ClearFields();
    }
}
