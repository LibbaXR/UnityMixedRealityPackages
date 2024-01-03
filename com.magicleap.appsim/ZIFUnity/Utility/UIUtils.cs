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
    internal static class UIUtils
    {
        public static bool IncrementalSelectionKey(this IMouseEvent mouseMoveEvent)
        {
#if UNITY_EDITOR_WIN
            return mouseMoveEvent.ctrlKey;
#elif UNITY_EDITOR_OSX
        return mouseMoveEvent.commandKey;
#else
        return mouseMoveEvent.ctrlKey;
#endif
        }

        public static void SetDisplay(this VisualElement element, bool enabled)
        {
            element.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetPropertyData(this TextField field, PropertyData<string> propertyData)
        {
            field.SetEnabled(propertyData.enabled);
            field.SetValueWithoutNotify(propertyData.value);
        }
    }
}
