// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ml.zi;
using UnityEngine;
using Event = UnityEngine.Event;
using KeyCode = ml.zi.KeyCode;

namespace MagicLeap.ZI
{
    internal static class Utils
    {
        public static KeyModifiers ConvertEventModifiers(Event e)
        {
            var ziModifiers = KeyModifiers.None;
            if (e.shift)
            {
                ziModifiers |= KeyModifiers.Shift;
            }

            if (e.alt)
            {
                ziModifiers |= KeyModifiers.Alt;
            }

            if (e.control)
            {
                ziModifiers |= KeyModifiers.Control;
            }

            if (e.command)
            {
                ziModifiers |= KeyModifiers.Super;
            }

            return ziModifiers;
        }

        public static KeyCode ConvertKeyCode(UnityEngine.KeyCode key)
        {
            switch (key)
            {
                case UnityEngine.KeyCode.None:
                    return KeyCode.None;
                case UnityEngine.KeyCode.Backspace:
                    return KeyCode.Backspace;
                case UnityEngine.KeyCode.Delete:
                    return KeyCode.Delete;
                case UnityEngine.KeyCode.Tab:
                    return KeyCode.Tab;
                case UnityEngine.KeyCode.Return:
                    return KeyCode.Enter;
                case UnityEngine.KeyCode.Pause:
                    return KeyCode.Pause;
                case UnityEngine.KeyCode.Escape:
                    return KeyCode.Escape;
                case UnityEngine.KeyCode.Space:
                    return KeyCode.ASCII_32;
                case UnityEngine.KeyCode.Keypad0:
                    return KeyCode.KP_0;
                case UnityEngine.KeyCode.Keypad1:
                    return KeyCode.KP_1;
                case UnityEngine.KeyCode.Keypad2:
                    return KeyCode.KP_2;
                case UnityEngine.KeyCode.Keypad3:
                    return KeyCode.KP_3;
                case UnityEngine.KeyCode.Keypad4:
                    return KeyCode.KP_4;
                case UnityEngine.KeyCode.Keypad5:
                    return KeyCode.KP_5;
                case UnityEngine.KeyCode.Keypad6:
                    return KeyCode.KP_6;
                case UnityEngine.KeyCode.Keypad7:
                    return KeyCode.KP_7;
                case UnityEngine.KeyCode.Keypad8:
                    return KeyCode.KP_8;
                case UnityEngine.KeyCode.Keypad9:
                    return KeyCode.KP_9;
                case UnityEngine.KeyCode.KeypadPeriod:
                    return KeyCode.KP_Decimal;
                case UnityEngine.KeyCode.KeypadDivide:
                    return KeyCode.KP_Divide;
                case UnityEngine.KeyCode.KeypadMultiply:
                    return KeyCode.KP_Multiply;
                case UnityEngine.KeyCode.KeypadMinus:
                    return KeyCode.KP_Subtract;
                case UnityEngine.KeyCode.KeypadPlus:
                    return KeyCode.KP_Add;
                case UnityEngine.KeyCode.KeypadEnter:
                    return KeyCode.KP_Enter;
                case UnityEngine.KeyCode.KeypadEquals:
                    return KeyCode.KP_Equal;
                case UnityEngine.KeyCode.UpArrow:
                    return KeyCode.Up;
                case UnityEngine.KeyCode.DownArrow:
                    return KeyCode.Down;
                case UnityEngine.KeyCode.RightArrow:
                    return KeyCode.Right;
                case UnityEngine.KeyCode.LeftArrow:
                    return KeyCode.Left;
                case UnityEngine.KeyCode.Insert:
                    return KeyCode.Insert;
                case UnityEngine.KeyCode.Home:
                    return KeyCode.Home;
                case UnityEngine.KeyCode.End:
                    return KeyCode.End;
                case UnityEngine.KeyCode.PageUp:
                    return KeyCode.PageUp;
                case UnityEngine.KeyCode.PageDown:
                    return KeyCode.PageDown;
                case UnityEngine.KeyCode.F1:
                    return KeyCode.F1;
                case UnityEngine.KeyCode.F2:
                    return KeyCode.F2;
                case UnityEngine.KeyCode.F3:
                    return KeyCode.F3;
                case UnityEngine.KeyCode.F4:
                    return KeyCode.F4;
                case UnityEngine.KeyCode.F5:
                    return KeyCode.F5;
                case UnityEngine.KeyCode.F6:
                    return KeyCode.F6;
                case UnityEngine.KeyCode.F7:
                    return KeyCode.F7;
                case UnityEngine.KeyCode.F8:
                    return KeyCode.F8;
                case UnityEngine.KeyCode.F9:
                    return KeyCode.F9;
                case UnityEngine.KeyCode.F10:
                    return KeyCode.F10;
                case UnityEngine.KeyCode.F11:
                    return KeyCode.F11;
                case UnityEngine.KeyCode.F12:
                    return KeyCode.F12;
                case UnityEngine.KeyCode.F13:
                    return KeyCode.F13;
                case UnityEngine.KeyCode.F14:
                    return KeyCode.F14;
                case UnityEngine.KeyCode.F15:
                    return KeyCode.F15;
                case UnityEngine.KeyCode.Alpha0:
                    return KeyCode.ASCII_48;
                case UnityEngine.KeyCode.Alpha1:
                    return KeyCode.ASCII_49;
                case UnityEngine.KeyCode.Alpha2:
                    return KeyCode.ASCII_50;
                case UnityEngine.KeyCode.Alpha3:
                    return KeyCode.ASCII_51;
                case UnityEngine.KeyCode.Alpha4:
                    return KeyCode.ASCII_52;
                case UnityEngine.KeyCode.Alpha5:
                    return KeyCode.ASCII_53;
                case UnityEngine.KeyCode.Alpha6:
                    return KeyCode.ASCII_54;
                case UnityEngine.KeyCode.Alpha7:
                    return KeyCode.ASCII_55;
                case UnityEngine.KeyCode.Alpha8:
                    return KeyCode.ASCII_56;
                case UnityEngine.KeyCode.Alpha9:
                    return KeyCode.ASCII_57;
                case UnityEngine.KeyCode.Quote:
                    return KeyCode.ASCII_39;
                case UnityEngine.KeyCode.Comma:
                    return KeyCode.ASCII_44;
                case UnityEngine.KeyCode.Minus:
                    return KeyCode.ASCII_45;
                case UnityEngine.KeyCode.Period:
                    return KeyCode.ASCII_46;
                case UnityEngine.KeyCode.Slash:
                    return KeyCode.ASCII_47;
                case UnityEngine.KeyCode.Semicolon:
                    return KeyCode.ASCII_59;
                case UnityEngine.KeyCode.LeftBracket:
                    return KeyCode.ASCII_91;
                case UnityEngine.KeyCode.Backslash:
                    return KeyCode.ASCII_92;
                case UnityEngine.KeyCode.RightBracket:
                    return KeyCode.ASCII_93;
                case UnityEngine.KeyCode.BackQuote:
                    return KeyCode.ASCII_96;
                case UnityEngine.KeyCode.A:
                    return KeyCode.ASCII_65;
                case UnityEngine.KeyCode.B:
                    return KeyCode.ASCII_66;
                case UnityEngine.KeyCode.C:
                    return KeyCode.ASCII_67;
                case UnityEngine.KeyCode.D:
                    return KeyCode.ASCII_68;
                case UnityEngine.KeyCode.E:
                    return KeyCode.ASCII_69;
                case UnityEngine.KeyCode.F:
                    return KeyCode.ASCII_70;
                case UnityEngine.KeyCode.G:
                    return KeyCode.ASCII_71;
                case UnityEngine.KeyCode.H:
                    return KeyCode.ASCII_72;
                case UnityEngine.KeyCode.I:
                    return KeyCode.ASCII_73;
                case UnityEngine.KeyCode.J:
                    return KeyCode.ASCII_74;
                case UnityEngine.KeyCode.K:
                    return KeyCode.ASCII_75;
                case UnityEngine.KeyCode.L:
                    return KeyCode.ASCII_76;
                case UnityEngine.KeyCode.M:
                    return KeyCode.ASCII_77;
                case UnityEngine.KeyCode.N:
                    return KeyCode.ASCII_78;
                case UnityEngine.KeyCode.O:
                    return KeyCode.ASCII_79;
                case UnityEngine.KeyCode.P:
                    return KeyCode.ASCII_80;
                case UnityEngine.KeyCode.Q:
                    return KeyCode.ASCII_81;
                case UnityEngine.KeyCode.R:
                    return KeyCode.ASCII_82;
                case UnityEngine.KeyCode.S:
                    return KeyCode.ASCII_83;
                case UnityEngine.KeyCode.T:
                    return KeyCode.ASCII_84;
                case UnityEngine.KeyCode.U:
                    return KeyCode.ASCII_85;
                case UnityEngine.KeyCode.V:
                    return KeyCode.ASCII_86;
                case UnityEngine.KeyCode.W:
                    return KeyCode.ASCII_87;
                case UnityEngine.KeyCode.X:
                    return KeyCode.ASCII_88;
                case UnityEngine.KeyCode.Y:
                    return KeyCode.ASCII_89;
                case UnityEngine.KeyCode.Z:
                    return KeyCode.ASCII_90;
                case UnityEngine.KeyCode.Numlock:
                    return KeyCode.NumLock;
                case UnityEngine.KeyCode.CapsLock:
                    return KeyCode.CapsLock;
                case UnityEngine.KeyCode.ScrollLock:
                    return KeyCode.ScrollLock;
                case UnityEngine.KeyCode.RightShift:
                    return KeyCode.Right_Shift;
                case UnityEngine.KeyCode.LeftShift:
                    return KeyCode.Left_Shift;
                case UnityEngine.KeyCode.RightControl:
                    return KeyCode.Right_Control;
                case UnityEngine.KeyCode.LeftControl:
                    return KeyCode.Left_Control;
                case UnityEngine.KeyCode.RightAlt:
                    return KeyCode.Right_Alt;
                case UnityEngine.KeyCode.LeftAlt:
                    return KeyCode.Left_Alt;
                case UnityEngine.KeyCode.RightCommand:
                    return KeyCode.Right_Super;
                case UnityEngine.KeyCode.RightWindows:
                    return KeyCode.Right_Super;
                case UnityEngine.KeyCode.LeftCommand:
                    return KeyCode.Left_Super;
                case UnityEngine.KeyCode.LeftWindows:
                    return KeyCode.Left_Super;
                case UnityEngine.KeyCode.Print:
                    return KeyCode.PrintScreen;
                case UnityEngine.KeyCode.Menu:
                    return KeyCode.Menu;
                default:
                    Debug.LogError("Invalid ZI KeyCode mapping");
                    return KeyCode.None;
            }
        }

        public static MouseButton ConvertMouseButton(int button)
        {
            switch (button)
            {
                case 0:
                    return MouseButton.Left;
                case 1:
                    return MouseButton.Right;
                case 2:
                    return MouseButton.Middle;
                default:
                    return (MouseButton) button + 1;
            }
        }

        public static Vector3 RoundToDisplay(this Vector3 vector3, int decimalPlaces = 4)
        {
            return new Vector3((float)System.Math.Round(vector3.x, decimalPlaces),
            (float)System.Math.Round(vector3.y, decimalPlaces),
            (float)System.Math.Round(vector3.z, decimalPlaces));
        }

        public static Vector3 ToMLCoordinates(Vector3 unityPosition)
        {
            return ChangeBasis(unityPosition);
        }

        public static Quaternion ToMLCoordinates(Quaternion unityOrientation)
        {
            return ChangeBasis(unityOrientation);
        }

        public static Vector3 ToUnityCoordinates(Vector3 mlPosition)
        {
            return ChangeBasis(mlPosition);
        }

        public static Quaternion ToUnityCoordinates(Quaternion mlOrientation)
        {
            return ChangeBasis(mlOrientation);
        }

        private static Vector3 ChangeBasis(Vector3 point)
        {
            Matrix4x4 mat = Matrix4x4.identity;
            return mat.MultiplyPoint3x4(point);
        }

        private static Quaternion ChangeBasis(Quaternion rotation)
        {
            // for only rotations (in the case of a quaternion) it is safe to directly
            // convert the quaternion's rotation direction
            // Doing so prevents floating point error from change of basis conversion
            Quaternion newRotation = rotation;
            newRotation.z = -rotation.z;
            newRotation.w = -rotation.w;
            return newRotation;
        }

        public static List<string> ToStringList(this StringList stringList)
        {
            List<string> result = new List<string>();

            if (stringList == null)
                return result;
            
            for (uint i = 0; i < stringList.GetSize(); i++)
            {
                result.Add(stringList.Get(i));
            }

            return result;
        }

        /// <summary>
        /// Uses reflection and some other magic to get the unity editor's screen position.
        /// Lifted from https://forum.unity.com/threads/issue-setting-the-position-of-an-editorwindow-opened-with-showutility.1069304/
        /// </summary>
        public static Rect GetUnityCurrentMonitorRect()
        {
            IEnumerable<Type> derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ScriptableObject)));
            Type containerWinType = derivedTypes.FirstOrDefault(t => "ContainerWindow".Equals(t.Name));
            if (containerWinType == null)
            {
                Debug.LogError("Can't find ContainerWindow");
            }
            FieldInfo showModeField = containerWinType.GetField("m_ShowMode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            PropertyInfo rootViewProperty = containerWinType.GetProperty("rootView",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || rootViewProperty == null)
            {
                Debug.LogError("Can't find 'm_ShowMode' or 'position'");
            }
            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (UnityEngine.Object win in windows)
            {
                var showMode = (int)showModeField.GetValue(win);
                if (showMode != 4)
                {
                    continue;
                }
                object view = rootViewProperty.GetValue(win, null);
                PropertyInfo screenPosProperty = view.GetType().GetProperty("screenPosition");
                if (screenPosProperty == null)
                {
                    Debug.LogError("Can't find 'screenPosition'.");
                }
                var screenPos = (Rect)screenPosProperty.GetValue(view, null);
                return screenPos;
            }
            return new Rect();
        }

    }
}
