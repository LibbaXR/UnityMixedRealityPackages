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
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal partial class EyeGazeViewModel
    {
        private EyeTracking GazeHandle => EyeTracking.Handle;
        public event Action<float> GazeVelocityChanged;
        public event Action<float> GazeAmplitudeChanged;
        public event Action<float> GazeDurationChanged;
        public event Action<float> GazeDirectionChanged;
        public event Action<float> GazeOnSetChanged;
        public event Action<GazeRecognitionBehavior> GazeBehaviorChanged;
        public event Action<GazeRecognitionError> GazeErrorChanged;

        public event Action<Vector2> GazeLeftPupilPositionChanged;
        public event Action<Vector2> GazeRightPupilPositionChanged;

        private void CheckGazeRecognitionChanges(EyeTrackingChanges changes)
        {
            if (changes.HasFlag(EyeTrackingChanges.GazeAmplitude))
            {
                GazeAmplitudeChanged?.Invoke(GetGazeAmplitude());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeVelocity))
            {
                GazeVelocityChanged?.Invoke(GetGazeVelocity());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeDirection))
            {
                GazeDirectionChanged?.Invoke(GetGazeDirection());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeBehavior))
            {
                GazeBehaviorChanged?.Invoke(GetGazeBehavior());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeDuration))
            {
                GazeDurationChanged?.Invoke(GetGazeDuration());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeError))
            {
                GazeErrorChanged?.Invoke(GetGazeError());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeOnset))
            {
                GazeOnSetChanged?.Invoke(GetGazeOnset());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeBehavior))
            {
                GazeBehaviorChanged?.Invoke(GetGazeBehavior());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeLeftPupilPosition))
            {
                GazeLeftPupilPositionChanged?.Invoke(GetGazeLeftPupilPosition());
            }

            if (changes.HasFlag(EyeTrackingChanges.GazeRightPupilPosition))
            {
                GazeRightPupilPositionChanged?.Invoke(GetGazeRightPupilPosition());
            }
        }

        #region getters

        public float GetGazeAmplitude()
        {
            return GazeHandle.GetGazeAmplitude();
        }

        public float GetGazeVelocity()
        {
            return GazeHandle.GetGazeVelocity();
        }

        public float GetGazeOnset()
        {
            return GazeHandle.GetGazeOnset();
        }

        public float GetGazeDirection()
        {
            return GazeHandle.GetGazeDirection();
        }

        public float GetGazeDuration()
        {
            return GazeHandle.GetGazeDuration();
        }

        public GazeRecognitionBehavior GetGazeBehavior()
        {
            return GazeHandle.GetGazeBehavior();
        }

        public GazeRecognitionError GetGazeError()
        {
            return GazeHandle.GetGazeError();
        }

        public Vector2 GetGazeLeftPupilPosition()
        {
            return GazeHandle.GetGazeLeftPupilPosition().ToVec2();
        }

        public Vector2 GetGazeRightPupilPosition()
        {
            return GazeHandle.GetGazeRightPupilPosition().ToVec2();
        }

        #endregion

        #region setters

        public void SetGazeLeftPupilPosition(Vector2 value)
        {
            GazeHandle.SetGazeLeftPupilPosition(value.ToVec2f());
        }

        public void SetGazeRightPupilPosition(Vector2 value)
        {
            GazeHandle.SetGazeRightPupilPosition(value.ToVec2f());
        }

        public void SetGazeOnset(float value)
        {
            GazeHandle.SetGazeOnset(value);
        }

        public void SetGazeDuration(float value)
        {
            GazeHandle.SetGazeDuration(value);
        }

        public void SetGazeDirection(float value)
        {
            GazeHandle.SetGazeDirection(value);
        }

        public void SetGazeAmplitude(float value)
        {
            GazeHandle.SetGazeAmplitude(value);
        }

        public void SetGazeVelocity(float value)
        {
            GazeHandle.SetGazeVelocity(value);
        }

        public void SetGazeBehavior(GazeRecognitionBehavior behavior)
        {
            GazeHandle.SetGazeBehavior(behavior);
        }

        public void SetGazeError(GazeRecognitionError error)
        {
            GazeHandle.SetGazeError(error);
        }

        #endregion
    }
}
