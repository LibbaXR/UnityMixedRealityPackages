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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ControllerTouchpad
    {
        public event Action TouchpadStateChanged;
        public event Action<Vector2> TouchPositionChanged;

        private Vector2 clampedTouchpadCursorPos;
        private Texture2D currentTouchpadTexture;
        private Vector2 lastMouseDownPos;
        private Texture2D touchpadCursor;

        private IMGUIContainer touchpadGUIContainer;

        private Rect touchpadRect;

        private Texture2D touchpadTexture;
        private Texture2D touchpadTextureSelected;

        public bool IsTouchPadHeld { set; get; }
        public bool IsTouchpadHeldExternally { set; private get; }
        public bool IsTouchpadHeldLock { set; private get; }
        public bool WasTouchPadHeldAfterLock { get; set; }

        public void Initialize(IMGUIContainer touchpadGUIContainer)
        {
            this.touchpadGUIContainer = touchpadGUIContainer;

            if (EditorGUIUtility.isProSkin)
            {
                touchpadTexture =
                    AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadBorder.png",
                        typeof(Texture2D)) as Texture2D;
            }
            else
            {
                touchpadTexture =
                    AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadBorder_Inverted.png",
                        typeof(Texture2D)) as Texture2D;
            }

            touchpadTextureSelected = EditorGUIUtility.isProSkin ?
                AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadBorderSelected.png",
                    typeof(Texture2D)) as Texture2D :
                    AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadBorderSelectedNoFill.png",
                    typeof(Texture2D)) as Texture2D;

            touchpadCursor = EditorGUIUtility.isProSkin ?
                AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadReticle.png", typeof(Texture2D))
                    as Texture2D :
                    AssetDatabase.LoadAssetAtPath(Constants.zifPackagePath + "/Icons/TouchpadReticleLightMode.png", typeof(Texture2D))
                    as Texture2D;

            currentTouchpadTexture = touchpadTexture;
        }

        public void OnGUI()
        {
            GetMouseInput();
            DrawTouchpad();
            DrawTouchpadCursor();
        }

        public void ReleaseTouchPad()
        {
            ReleaseTouchpadLock();
            SetTouchpadHeld(false);
        }

        public void SetCursorPosition(Vector2 normalizedPosition)
        {
            normalizedPosition = ClampToRadius(normalizedPosition);

            if (!IsTouchPadHeld)
            {
                clampedTouchpadCursorPos = new Vector2(touchpadRect.width / 2 + touchpadRect.width / 2 * normalizedPosition.x,
                    +touchpadRect.height / 2 - touchpadRect.height / 2 * normalizedPosition.y
                );
            }
        }

        private Vector2 ClampToRadius(Vector2 input)
        {
            if (input.magnitude > 1)
            {
                input.Normalize();
            }

            return input;
        }

        private void DrawTouchpad()
        {
            touchpadRect = new Rect(0, 0, touchpadGUIContainer.resolvedStyle.width, touchpadGUIContainer.resolvedStyle.height);

            currentTouchpadTexture = IsTouchpadHeldLock && WasTouchPadHeldAfterLock || IsTouchpadHeldExternally || IsTouchPadHeld
                ? touchpadTextureSelected
                : touchpadTexture;

            GUI.DrawTexture(touchpadRect, currentTouchpadTexture);
        }

        private void DrawTouchpadCursor()
        {
            if (IsTouchPadHeld || IsTouchpadHeldExternally || IsTouchpadHeldLock && WasTouchPadHeldAfterLock)
            {
                var cursorRect = new Rect(clampedTouchpadCursorPos.x - touchpadCursor.width / 2,
                    clampedTouchpadCursorPos.y - touchpadCursor.height / 2, touchpadCursor.width, touchpadCursor.height);

                GUI.DrawTexture(cursorRect, touchpadCursor);

                touchpadGUIContainer.MarkDirtyRepaint();
            }
        }

        private Vector2 GetCursorPosition(Vector2 touchPosition)
        {
            float radius = touchpadRect.width / 2;
            return new Vector2(radius + touchPosition.x * radius, radius - touchPosition.y * radius);
        }

        private void GetMouseInput()
        {
            Event currentEvent = Event.current;

            //this special behavior ensures we can register mouse up events if the cursor is dragged outside of this EditorWindow
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            switch (currentEvent.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlId;
                    SetTouchpadHeld(true);
                    if (!IsTouchpadHeldLock)
                    {
                        WasTouchPadHeldAfterLock = true;
                    }

                    lastMouseDownPos = currentEvent.mousePosition;
                    break;
                case EventType.MouseUp:
                    if (!IsTouchpadHeldLock)
                    {
                        ReleaseTouchPad();
                    }

                    SetTouchpadHeld(false);
                    break;
                case EventType.MouseDrag:
                    lastMouseDownPos = currentEvent.mousePosition;
                    break;
            }

            if (IsTouchPadHeld || WasTouchPadHeldAfterLock && !IsTouchpadHeldExternally)
            {
                Vector3 touchPosition = GetTouchPosition(lastMouseDownPos);
                clampedTouchpadCursorPos = GetCursorPosition(touchPosition);

                TouchPositionChanged?.Invoke(touchPosition);
            }
        }

        private Vector2 GetTouchPosition(Vector2 mousePosition)
        {
            float radius = touchpadRect.width / 2;
            var center = new Vector2(touchpadRect.x + radius, touchpadRect.y + radius);
            Vector2 mouseVector = mousePosition - center;
            mouseVector.y *= -1;

            var touchPosition = new Vector2(mouseVector.x / radius, mouseVector.y / radius);

            return ClampToRadius(touchPosition);
        }

        private void ReleaseTouchpadLock()
        {
            WasTouchPadHeldAfterLock = false;
            clampedTouchpadCursorPos = touchpadRect.center;
            TouchPositionChanged?.Invoke(Vector2.zero);
        }

        private void SetTouchpadHeld(bool isHeld)
        {
            IsTouchPadHeld = isHeld;
            TouchpadStateChanged?.Invoke();
        }
    }
}
