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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal abstract class PanelWindow : EditorWindow, IHasCustomMenu
    {
        // Order of menu items in "Window=>Magic Leap App Simulator" menu.
        // It seems that items with priorities within a section of 20 are put in the same group.
        //
        public const int MenuItemPriority_LoadDefaultLayout = -11;
        protected const int MenuItemPriority_Target = 0;
        protected const int MenuItemPriority_DeviceView = 11;
        protected const int MenuItemPriority_SceneView = 12;
        protected const int MenuItemPriority_HeadPose = 31;
        protected const int MenuItemPriority_SceneCamera = 32;
        protected const int MenuItemPriority_Controller = 51;
        protected const int MenuItemPriority_HandTracking = 52;
        protected const int MenuItemPriority_EyeGaze = 53;
        protected const int MenuItemPriority_MarkerTracking = 54;
        protected const int MenuItemPriority_Permission = 71;
        protected const int MenuItemPriority_SystemEvents = 72;
        protected const int MenuItemPriority_SceneGraph = 91;
        protected const int MenuItemPriority_Rendering = 92;

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            Type currentWindowType = GetType();
            string menuPath = "Add Tab/";
            IEnumerable<Type> windowTypes = GetAllWindowTypes();

            foreach (Type windowType in windowTypes)
            {
                GUIContent content = new GUIContent(menuPath + GetWindowNameOfType(windowType));
                menu.AddItem(content, false, () => GetWindowDocked(windowType, new[] {currentWindowType}, false));
            }

            menu.AddSeparator(menuPath);
        }

        private PanelWindow GetWindowDocked(Type windowType, Type[] dockWindowType = null, bool focus = true)
        {
            if (!windowType.IsSubclassOf(typeof(PanelWindow)))
            {
                Debug.LogError($"{windowType} is not sub class of Panel Window!");
                return null;
            }

            Type[] parameterTypes =
            {
                typeof(string),
                typeof(bool),
                typeof(Type[])
            };

            Type editorType = typeof(EditorWindow);
            MethodInfo genericMethodInfo = editorType.GetRuntimeMethod("GetWindow", parameterTypes).MakeGenericMethod(windowType);
            string windowName = GetWindowNameOfType(windowType);

            object[] parametersArray = {windowName, focus, dockWindowType};

            return (PanelWindow) genericMethodInfo?.Invoke(null, parametersArray);
        }

        private IEnumerable<Type> GetAllWindowTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(PanelWindow)) && t != GetType() && !t.IsAbstract)
                .Where(t => !Attribute.IsDefined(t, typeof(ObsoleteAttribute)));
        }

        private string GetWindowNameOfType(Type windowType)
        {
            return (string) windowType.GetField("windowName", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
        }
    }
}
