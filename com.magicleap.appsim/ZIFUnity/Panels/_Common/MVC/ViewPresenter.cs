// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal abstract class ViewPresenter<TState> : CustomView where TState : IViewState, new()
    {
        public TState State { get; protected set; }

        internal string PlayerPrefsKey => "ZI_ViewState_" + GetType().Name;

        public void CleanData()
        {
            EditorPrefs.DeleteKey(PlayerPrefsKey);
        }

        public virtual void OnDisable()
        {
            Serialize();
            UnregisterUICallbacks();
        }

        public virtual void OnEnable(VisualElement root)
        {
            Root = root;

            DeSerialize();
            Initialize();
        }

        protected virtual void DeSerialize()
        {
            if (EditorPrefs.HasKey(PlayerPrefsKey))
            {
                string savedJson = EditorPrefs.GetString(PlayerPrefsKey);
                if (savedJson != "")
                {
                    State = JsonUtility.FromJson<TState>(savedJson);
                    return;
                }

                CleanData();
            }

            State = new TState();
            State.SetDefaultValues();
        }

        protected virtual void Serialize()
        {
            EditorPrefs.SetString(PlayerPrefsKey, JsonUtility.ToJson(State));
        }

        protected void AddThemeStyleSheetToRoot(string darkModeStylePath, string lightModeStylePath)
        {
            string styleSheetPath = EditorGUIUtility.isProSkin ? darkModeStylePath : lightModeStylePath;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            if (styleSheet == null)
            {
                Debug.LogErrorFormat("Couldn't load style sheet in path: {0}", styleSheetPath);
                return;
            }
            AddStyleSheet(styleSheet);
        }

        private void AddStyleSheet(StyleSheet styleSheet)
        {
            VisualElementStyleSheetSet styleSheets = Root.styleSheets;
            bool hasThisStyle = styleSheets.Contains(styleSheet);
            if (!hasThisStyle)
            {
                styleSheets.Add(styleSheet);
            }
        }
    }
}
