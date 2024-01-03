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
    internal abstract class ObjectView<T> : ObjectView where T : class
    {
        public T BondedObject { get; }

        protected ObjectView(T bondedObject)
        {
            BondedObject = bondedObject;
        }
    }

    internal abstract class ObjectView : CustomView
    {
        public virtual VisualElement VisualElement
        {
            get
            {
                if (Root == null)
                {
                    Initialize();
                }

                return Root;
            }
        }

        public virtual void SetEnabled(bool state)
        {
        }
    }
}
