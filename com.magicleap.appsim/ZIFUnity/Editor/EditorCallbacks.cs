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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEditor;

namespace MagicLeap.ZI
{
    [InitializeOnLoad]
    public class EditorCallbacks
    {
        static EditorCallbacks()
        {
            EditorApplication.wantsToQuit += WantsToQuit;
        }

        private static bool WantsToQuit()
        {
            if (ZIBridge.Instance.IsServerRunning)
            {
                if (Settings.Instance.DirtySessionPrompt == Settings.DirtySessionState.Prompt || Settings.Instance.ShowCloseDialog)
                {
                    QuitUnityWithConfirmation();
                    return false;
                }

                if (Settings.Instance.CloseZISessionOnQuit)
                {
                    CloseSession();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Close the session and exit unity editor, prompting user with necessary dialogs
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void QuitUnityWithConfirmation()
        {
            if (ZIBridge.Instance.SessionSaveStatus.RequiresSave)
            {
                bool? result = null;

                switch (Settings.Instance.DirtySessionPrompt)
                {
                    case Settings.DirtySessionState.Prompt:
                        SavingDirtyScenePrompt.ShowPrompt(ZIBridge.Instance.SessionSaveStatus, (res) =>
                        {
                            if (res.GetValueOrDefault())
                            {
                                ZIBridge.Instance.SaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path, false, (success) =>
                                {
                                    if (success)
                                    {
                                        QuitUnityWithSessionDialog();
                                    }
                                });
                            } 
                            else if (res.HasValue)  // user answered "no" to saving
                            {
                                QuitUnityWithSessionDialog();
                            }
                        });
                        break;
                    case Settings.DirtySessionState.SaveSession:
                        result = true;
                        break;
                    case Settings.DirtySessionState.DiscardChanges:
                        result = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (result.GetValueOrDefault())
                {
                    ZIBridge.Instance.SaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path, false, (success) =>
                    {
                        if (success)
                        {
                            QuitUnityWithSessionDialog();
                        }
                    });
                }
                else if (result.HasValue)  // user auto-answered "no" to saving
                {
                    QuitUnityWithSessionDialog();
                }
            }
            else
            {
                QuitUnityWithSessionDialog();
            }
        }

        private static void QuitUnityWithSessionDialog()
        {
            if (Settings.Instance.ShowCloseDialog)
            {
                bool userInput = false;
                CheckboxDialogSettings dialogSettings = new()
                {
                    Name = "Quitting Unity",
                    Description = "Do you want to stop the Magic Leap App Simulator session?",
                    YesAction = dialogPref =>
                    {
                        userInput = true;
                        if (dialogPref)
                        {
                            Settings.Instance.ShowCloseDialog = false;
                            Settings.Instance.CloseZISessionOnQuit = true;
                        }
                    },
                    NoAction = dialogPref =>
                    {
                        userInput = false;
                        if (!dialogPref)
                        {
                            Settings.Instance.ShowCloseDialog = true;
                            Settings.Instance.CloseZISessionOnQuit = false;
                        }
                    }
                };
                CheckboxDialog.ShowDialog(dialogSettings, () =>
                {
                    if (userInput)
                    {
                        CloseSession();
                    }
                    else
                    {
                        ExitUnity();
                    }
                });
            }
            else // if (!Settings.Instance.ShowCloseDialog)
            {
                if (Settings.Instance.CloseZISessionOnQuit)
                {
                    CloseSession();
                }
                else
                {
                    ExitUnity();
                }
            }
        }

        private static void CloseSession()
        {
            EditorApplication.isPlaying = false;
            ZIBridge.Instance.StopSessionOnThread(OnSessionClosed);
        }

        private static void OnSessionClosed(bool isDisconnected)
        {
            if (isDisconnected)
            {
                ExitUnity();
            }
        }

        private static void ExitUnity() => EditorApplication.Exit(0);
    }
}
