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
using System.Globalization;
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal partial class EyeGazeViewModel
    {
        private const string PupilDistanceKey = "EyeFixationGizmoIPD";
        public event Action<EyeTrackingCalibrationStatus> CalibrationStatusChanged;
        public event Action<EyeTrackingError> ErrorStateChanged;
        public event Action<float> FixationConfidenceChanged;
        public event Action<Vector3> FixationPositionChanged;

        public event Action<bool> FollowHeadPoseChanged;
        public event Action<bool> LeftEyeBlinkChanged;
        public event Action<float> LeftEyeConfidenceChanged;
        public event Action<Vector3> LeftEyePositionChanged;
        public event Action<int> PupilDistanceChanged;
        public event Action<float> PupilLeftSizeChanged;
        public event Action<float> PupilRightSizeChanged;
        public event Action<bool> RightEyeBlinkChanged;
        public event Action<float> RightEyeConfidenceChanged;
        public event Action<Vector3> RightEyePositionChanged;
        public event Action<bool> ActiveOnDeviceChanged;

        private void CheckEyeTrackingChanges(EyeTrackingChanges changes)
        {
            if (changes.HasFlag(EyeTrackingChanges.ActiveOnDevice))
            {
                ActiveOnDeviceChanged?.Invoke(GetActiveOnDevice());
            }

            if (changes.HasFlag(EyeTrackingChanges.FollowHeadpose))
            {
                FollowHeadPoseChanged?.Invoke(GetFollowHeadPose());
            }

            if (changes.HasFlag(EyeTrackingChanges.FixationPosition))
            {
                FixationPositionChanged?.Invoke(GetFixationPosition());
            }

            if (changes.HasFlag(EyeTrackingChanges.LeftCenterPosition))
            {
                LeftEyePositionChanged?.Invoke(GetLeftEyePosition());
            }

            if (changes.HasFlag(EyeTrackingChanges.RightCenterPosition))
            {
                RightEyePositionChanged?.Invoke(GetRightEyePosition());
            }

            if (changes.HasFlag(EyeTrackingChanges.LeftBlink))
            {
                LeftEyeBlinkChanged?.Invoke(GetLeftEyeBlink());
            }

            if (changes.HasFlag(EyeTrackingChanges.RightBlink))
            {
                RightEyeBlinkChanged?.Invoke(GetRightEyeBlink());
            }

            if (changes.HasFlag(EyeTrackingChanges.LeftPupilDiameter))
            {
                PupilLeftSizeChanged?.Invoke(GetLeftPupilSize());
            }

            if (changes.HasFlag(EyeTrackingChanges.RightPupilDiameter))
            {
                PupilRightSizeChanged?.Invoke(GetRightPupilSize());
            }

            if (changes.HasFlag(EyeTrackingChanges.FixationConfidence))
            {
                FixationConfidenceChanged?.Invoke(GetFixationConfidence());
            }

            if (changes.HasFlag(EyeTrackingChanges.LeftCenterConfidence))
            {
                LeftEyeConfidenceChanged?.Invoke(GetLeftEyeConfidence());
            }

            if (changes.HasFlag(EyeTrackingChanges.RightCenterConfidence))
            {
                RightEyeConfidenceChanged?.Invoke(GetRightEyeConfidence());
            }

            if (changes.HasFlag(EyeTrackingChanges.Error))
            {
                ErrorStateChanged?.Invoke(GetErrorState());
            }

            if (changes.HasFlag(EyeTrackingChanges.CalibrationStatus))
            {
                CalibrationStatusChanged?.Invoke(GetCalibrationStatus());
            }
        }

        public void ResetEyeFixation()
        {
            EyeTracking.Handle.ResetEyeFixation();
        }

        #region Getters

        public EyeTrackingCalibrationStatus GetCalibrationStatus()
        {
            return EyeTracking.Handle.GetCalibrationStatus();
        }

        public EyeTrackingError GetErrorState()
        {
            return EyeTracking.Handle.GetError();
        }

        public float GetFixationConfidence()
        {
            return EyeTracking.Handle.GetFixationConfidence();
        }

        public Vector3 GetFixationPosition()
        {
            return EyeTracking.Handle.GetFixationPosition().ToVec3();
        }

        private bool GetActiveOnDevice()
        {
            return EyeTracking.Handle.GetActiveOnDevice();
        }

        public bool GetFollowHeadPose()
        {
            return EyeTracking.Handle.GetFollowHeadpose();
        }

        public bool GetLeftEyeBlink()
        {
            return EyeTracking.Handle.GetLeftBlink();
        }

        public float GetLeftEyeConfidence()
        {
            return EyeTracking.Handle.GetLeftCenterConfidence();
        }

        public Vector3 GetLeftEyePosition()
        {
            return EyeTracking.Handle.GetLeftCenterPosition().ToVec3();
        }

        public float GetLeftPupilSize()
        {
            return EyeTracking.Handle.GetLeftPupilDiameter();
        }

        public int GetPupilDistance()
        {
            if (ZIBridge.IsDeviceMode)
            {
                return 0;
            }

            var valueJson =
                ConfigurationSettings.Handle.GetValueJson(Constants.zifSimulatorDeviceName, PupilDistanceKey );
            int.TryParse(valueJson, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
            return result;
        }
        public bool GetRightEyeBlink()
        {
            return EyeTracking.Handle.GetRightBlink();
        }

        public float GetRightEyeConfidence()
        {
            return EyeTracking.Handle.GetRightCenterConfidence();
        }

        public Vector3 GetRightEyePosition()
        {
            return EyeTracking.Handle.GetRightCenterPosition().ToVec3();
        }

        public float GetRightPupilSize()
        {
            return EyeTracking.Handle.GetRightPupilDiameter();
        }

        #endregion

        #region Setters

        public void SetCalibrationStatus(EyeTrackingCalibrationStatus calibrationStatus)
        {
            EyeTracking.Handle.SetCalibrationStatus(calibrationStatus);
        }

        public void SetErrorState(EyeTrackingError error)
        {
            EyeTracking.Handle.SetError(error);
        }

        public void SetFixationConfidence(float confidence)
        {
            EyeTracking.Handle.SetFixationConfidence(confidence);
        }

        public void SetFixationPosition(Vector3 fixation)
        {
            EyeTracking.Handle.SetFixationPosition(fixation.ToVec3f(), true);
        }

        public void SetFollowHeadPose(bool followHeadPose)
        {
            EyeTracking.Handle.SetFollowHeadpose(followHeadPose);
        }

        public void SetLeftEyeBlink(bool leftEyeBlink)
        {
            EyeTracking.Handle.SetLeftBlink(leftEyeBlink);
        }

        public void SetLeftEyeConfidence(float confidence)
        {
            EyeTracking.Handle.SetLeftCenterConfidence(confidence);
        }

        public void SetLeftEyePosition(Vector3 eyePosition)
        {
            EyeTracking.Handle.SetLeftCenterPosition(eyePosition.ToVec3f(), true);
        }

        public void SetLeftPupilSize(float size)
        {
            EyeTracking.Handle.SetLeftPupilDiameter(size);
        }

        public void SetPupilDistance(int value)
        {
            ConfigurationSettings.Handle.SetValueInt(Constants.zifSimulatorDeviceName, PupilDistanceKey, value);
        }

        public void SetRightEyeBlink(bool rightEyeBlink)
        {
            EyeTracking.Handle.SetRightBlink(rightEyeBlink);
        }

        public void SetRightEyeConfidence(float confidence)
        {
            EyeTracking.Handle.SetRightCenterConfidence(confidence);
        }

        public void SetRightEyePosition(Vector3 eyePosition)
        {
            EyeTracking.Handle.SetRightCenterPosition(eyePosition.ToVec3f(), true);
        }

        public void SetRightPupilSize(float size)
        {
            EyeTracking.Handle.SetRightPupilDiameter(size);
        }

        #endregion
    }
}
