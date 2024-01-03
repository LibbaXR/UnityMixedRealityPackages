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
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    [InitializeOnLoad]
    public class EditorWindowFocusUtility
    {
        public static event Action<bool> OnUnityEditorFocus;
        public static bool AppFocused => appFocused;
        
        private static bool appFocused;
        
        static EditorWindowFocusUtility()
        {
            EditorApplication.update += Update;
        }
     
        private static void Update()
        {
            if (!appFocused && UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                OnUnityEditorFocus?.Invoke(appFocused);
            }
            else if (appFocused && !UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                appFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
                OnUnityEditorFocus?.Invoke(appFocused);
            }
        }
    }
}