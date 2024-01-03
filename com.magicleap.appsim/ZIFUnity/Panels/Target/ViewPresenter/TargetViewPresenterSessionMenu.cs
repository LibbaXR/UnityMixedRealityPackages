// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class TargetViewPresenter
    {
        private class SessionMenuItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        private GenericMenu sessionMenu;

        public void UpdateSessionDirtyIndicator(bool sessionIsDirty, bool isSessionSaved)
        {
            if (sessionIsDirty && isSessionSaved)
            {
                currentSessionLabel.AddToClassList("sessionDirty");
                currentSessionLabel.text = $"* {currentSessionLabel.text}";
            }
            else
            {
                currentSessionLabel.RemoveFromClassList("sessionDirty");
                currentSessionLabel.text = currentSessionLabel.text.TrimStart('*', ' ');
            }
        }

        public void UpdateSessionLabel(string currentSessionSavePath)
        {
            if (string.IsNullOrEmpty(currentSessionSavePath))
            {
                currentSessionLabel.text = "[Unsaved Session]";
            }
            else
            {
                currentSessionLabel.text = Path.GetFileNameWithoutExtension(currentSessionSavePath);
            }
        }

        public void UpdateSessionMenu(string[] actualRecentSessionPaths)
        {
            sessionMenu = new GenericMenu();
            sessionMenu.AddItem(new GUIContent("Open Session..."), false, OpenSessionSelected);
            sessionMenu.AddItem(new GUIContent("Save Session"), false, SaveSessionSelected);
            sessionMenu.AddItem(new GUIContent("Save As..."), false, SaveSessionAsSelected);
            
            if (actualRecentSessionPaths.Length > 0)
            {
                sessionMenu.AddSeparator("");
            }

            foreach (string sessionPath in actualRecentSessionPaths)
            {
                if (!string.IsNullOrEmpty(sessionPath))
                {
                    var menuItem = new SessionMenuItem
                    {
                        Name = Path.GetFileNameWithoutExtension(sessionPath),
                        Path = sessionPath
                    };
                    sessionMenu.AddItem(new GUIContent(menuItem.Name), false, OnRecentSessionSelected, menuItem);
                }
            }
        }

        private void OpenSessionSelected()
        {
            OnOpenSessionSelected?.Invoke();
        }

        private void OnRecentSessionSelected(object session)
        {
            SessionMenuItem menuItem = session as SessionMenuItem;

            if (session == null || menuItem == null)
            {
                return;
            }

            if (File.Exists(menuItem.Path))
            {
                OnRecentSessionChanged?.Invoke(menuItem.Path);
            }
            else if (EditorUtility.DisplayDialog("Session file not found", "Could not find session file " + menuItem.Path
              + ".\n\nRemove " + menuItem.Name + " from the list of recent sessions?", "Yes", "No"))
            {
                RemoveSessionFromRecentListSelected(menuItem.Path);
            }
            else
            {
                Debug.LogErrorFormat("Could not load missing session file: {0}", menuItem.Path);
            }
        }
        
        private void RemoveSessionFromRecentListSelected(string path)
        {
            OnRemoveSessionFromRecentListSelected?.Invoke(path);
        }
        
        private void SaveSessionAsSelected()
        {
            OnSaveSessionAsSelected?.Invoke();
        }

        private void SaveSessionSelected()
        {
            OnSaveSessionSelected?.Invoke();
        }
    }
}
