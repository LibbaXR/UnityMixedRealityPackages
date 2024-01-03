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
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        private static readonly ProgressMonitor virtualRoomLoadSessionMonitor = ProgressMonitorDisplay.Show("Loading session.", "Magic Leap App Simulator");
        private static readonly ProgressMonitor virtualRoomSaveSessionMonitor = ProgressMonitorDisplay.Show("Saving session.", "Magic Leap App Simulator");
        
        public ModuleWrapper<VirtualRoom, VirtualRoomChanges> VirtualRoom = new();

        private bool LoadSession(string sessionPath)
        {
            ReturnedResultString loadResult = VirtualRoom.Handle.LoadSession(sessionPath, virtualRoomLoadSessionMonitor);
                
            bool isResultError = ZIFGen.ResultIsError(loadResult.first);
            if (isResultError)
            {
                Debug.LogError(ZIFGen.ResultGetString(loadResult.first));
            }

            return !isResultError;
        }

        private bool SaveSession(string sessionPath)
        {
            ReturnedResultString saveResult = VirtualRoom.Handle.SaveSession(sessionPath, virtualRoomSaveSessionMonitor);
                
            bool isResultError = ZIFGen.ResultIsError(saveResult.first);
            if (isResultError)
            {
                Debug.LogError(ZIFGen.ResultGetString(saveResult.first));
            }
            
            return !isResultError;
        }
    }
}
