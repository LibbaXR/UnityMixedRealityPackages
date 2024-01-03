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
    internal abstract class CustomView
    {
        public virtual VisualElement Root { get; protected set; }

        protected virtual void AssertFields()
        {
        }

        protected virtual void BindUIElements()
        {
        }

        protected virtual void Initialize()
        {
            BindUIElements();
            AssertFields();
            RegisterUICallbacks();
            SynchronizeViewWithState();
        }

        protected virtual void RegisterUICallbacks()
        {
        }

        protected virtual void SynchronizeViewWithState()
        {
        }

        protected virtual void UnregisterUICallbacks()
        {
        }

        public virtual void ClearFields()
        {
        }
    }
}
