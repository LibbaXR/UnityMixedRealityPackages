// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.ZI
{
    public class CheckboxDialogSettings
    {
        public string Name;
        public string Description;
        public string YesButtonText = "Yes";
        public string NoButtonText = "No";
        public string CheckboxText = "Don't ask me again";
        public IEnumerable<string> StyleSheetPaths = null;
        public Action<bool> YesAction = null;
        public Action<bool> NoAction = null;
        public Action WindowClosed = null;
        public Action LostFocus = null;
        public Vector2 Size = new Vector2(250, 120);
    }
}
