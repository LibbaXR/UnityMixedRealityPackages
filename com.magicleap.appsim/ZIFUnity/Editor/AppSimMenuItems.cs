// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2023 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Threading;
using ml.zi;
using UnityEditor;

namespace MagicLeap.ZI
{
    internal class AppSimMenuItems
    {
        private static bool isKillingProcesses = false;
        // Result message of killing processes.
        private static string resultMessage;

        [MenuItem("Magic Leap/App Simulator/Kill Backend Processes...")]
        static void KillProcesses()
        {
            Result r = ml.zi.Environment.TestAnyProcessRunning();
            if (r == Result.DoesNotExist)
            {
                EditorUtility.DisplayDialog(Constants.zifPluginUserName, "There is no App Sim backend process to kill", "Ok");
                return;
            }

            string msg = "Force kill all App Sim backend processes including zombies if any.";

            if (ZIBridge.IsSessionConnected)
            {
                msg +=
                    "\n\nAn App Sim session is running. You should stop the session by a normal way, " +
                    "e.g. by clicking 'Stop' button in 'App Sim Target' panel. " +
                    "This command should be taken as a last resort.";
            }
            msg += "\n\nDo you want to continue?";
            bool ok = EditorUtility.DisplayDialog(Constants.zifPluginUserName, msg, "Ok", "Cancel");
            if (!ok)
                return;

            isKillingProcesses = true;
            resultMessage = "";
            EditorApplication.update += ShowProgressForKillProcesses;

            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    ml.zi.Environment.KillProcesses();
                    resultMessage = "Finished killing App Sim backend processes.";
                }
                catch (ResultIsErrorException e)
                {
                    if (e.Result == Result.DoesNotExist)
                    {
                        resultMessage = "There is no App Sim backend process to kill.";
                    }
                    else
                    {
                        // Should not happen.
                        resultMessage = "Failed to kill some processes: " + e.Message;
                    }
                }
                finally
                {
                    isKillingProcesses = false;
                }
            }));
            thread.Start();
        }

        [MenuItem("Magic Leap/App Simulator/Purge Generated Files...")]
        static void PurgeFiles()
        {
            string prompt = "Delete files generated by App Sim such as log files and other temporary files.\n";
            if (ZIBridge.IsSessionConnected)
            {
                prompt = "An App Sim session is running. Please stop the session first.";
                EditorUtility.DisplayDialog(Constants.zifPluginUserName, prompt, "Ok");
                return;
            }
            else
            {
                prompt += "Do you want to continue?";
                bool ok = EditorUtility.DisplayDialog(Constants.zifPluginUserName, prompt, "Ok", "Cancel");
                if (!ok)
                    return;
            }

            prompt = "Finished purging generated files.";
            try
            {
                Environment.PurgeFiles();
            }
            catch (ResultIsErrorException)
            {
                // This message should be sufficient for user
                prompt += "\n\nSome files are not deleted as they are in use.";
                // for advanced users, see logfile for which file deletion failed.
            }

            EditorUtility.DisplayDialog(Constants.zifPluginUserName, prompt, "Ok");
        }

        private static void ShowProgressForKillProcesses()
        {
            if (isKillingProcesses)
            {
                EditorUtility.DisplayProgressBar(Constants.zifPluginUserName, "Killing backend processes...", 0.5f);
            }
            else
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= ShowProgressForKillProcesses;
                EditorUtility.DisplayDialog(Constants.zifPluginUserName, resultMessage, "Ok");
            }
        }
    }
}