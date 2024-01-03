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

namespace MagicLeap.ZI
{
    [Serializable]
    internal class EyeGazeViewState : IViewState
    {
        public EyeTrackingState eyeTrackingState = new();
        public GazeRecognitionState gazeRecognitionState = new();

        public void SetDefaultValues()
        {
            eyeTrackingState.Reset();
            gazeRecognitionState.Reset();
        }

        [Serializable]
        public class EyeTrackingState
        {
            public bool IsPositionTabOpened;
            public bool IsConfidenceTabOpened;
            public bool IsStateTabOpened;
            public bool IsEyeBlinkLinked;
            public bool IsHoldEnabled;

            public void Reset()
            {
                IsPositionTabOpened = true;
                IsConfidenceTabOpened = true;
                IsStateTabOpened = true;
                IsEyeBlinkLinked = false;
                IsHoldEnabled = false;
            }
        }

        [Serializable]
        public class GazeRecognitionState
        {
            public bool IsHoldEnabled;
            public bool IsBehaviorFoldoutOpened;
            public bool IsGazeRecognitionFoldoutOpened;

            public void Reset()
            {
                IsBehaviorFoldoutOpened = true;
                IsGazeRecognitionFoldoutOpened = true;
                IsHoldEnabled = false;
            }
        }
    }
}
