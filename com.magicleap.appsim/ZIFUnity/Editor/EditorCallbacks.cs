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
            if (ZIBridge.Instance.IsConnected)
            {
                if (Settings.Instance.DirtySessionPrompt == Settings.DirtySessionState.Prompt || Settings.Instance.ShowCloseDialog)
                {
#pragma warning disable 4014  // execution of awaited call without await operator
                    AsyncCloseSession();
#pragma warning restore 4014
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

        private static async Task AsyncCloseSession()
        {
            if (ZIBridge.Instance.SessionSaveStatus.RequiresSave)
            {
                bool? result = null;

                switch (Settings.Instance.DirtySessionPrompt)
                {
                    case Settings.DirtySessionState.Prompt:
                        result = await SavingDirtyScenePrompt.AsyncShowPrompt(ZIBridge.Instance.SessionSaveStatus);
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

                if (result == null)
                    return;

                if (result == true)
                {
                    await ZIBridge.Instance.AsyncSaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path);
                }
            }

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
                await CheckboxDialog.AsyncShowDialog(dialogSettings);

                if (userInput)
                {
                    await ZIBridge.Instance.AsyncStopSessionOnThread();
                }

                ExitUnity();
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
