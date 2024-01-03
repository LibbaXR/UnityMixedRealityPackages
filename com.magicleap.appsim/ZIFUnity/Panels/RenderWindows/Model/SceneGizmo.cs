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

namespace MagicLeap.ZI
{
    [Serializable]
    internal struct SceneGizmo
    {
        public string Category;
        public bool Default;
        public string Description;
        public string Label;
        public string Name;
        public int Priority;
        public string Type;
        public bool UserVisible;
        public bool Value;
    }
}
