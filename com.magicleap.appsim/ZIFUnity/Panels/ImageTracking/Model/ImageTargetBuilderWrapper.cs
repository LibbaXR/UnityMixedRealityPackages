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
using Texture = UnityEngine.Texture;

namespace MagicLeap.ZI
{
    internal class ImageTargetBuilderWrapper : IDisposable
    {
        private const ArucoDictionaryName DefaultArucoDictionary = ArucoDictionaryName.DICT_4X4_50;
        private const int DefaultArucoId = 15;
        private const int DefaultMarkerLength = 200;
        private const string DefaultQRText = "Hello World!";
        private const string DefaultEAN13Text = "012345678901";
        private const string DefaultUPCAText = "01234567890";

        public Action<string> OnImageTargetTypeChanged;
        public Action<string> OnOrientationChanged;
        public Action<string> OnPositionChanged;
        public Action<string> OnScaleChanged;
        public Action<string> OnTextureChanged;
        public Action<string> OnArucoDictionaryChanged;
        public Action<string> OnArucoIdChanged;
        public Action<string> OnMarkerLengthChanged;
        public Action<string> OnTextChanged;

        private ImageTargetType imageTargetType;
        private ArucoDictionaryName arucoDictionary = DefaultArucoDictionary;
        private int arucoId = DefaultArucoId;
        private int arucoMarkerLength = DefaultMarkerLength;
        private int qrMarkerLength = DefaultMarkerLength;
        private string qrText = DefaultQRText;
        private string ean13Text = DefaultEAN13Text;
        private string upcaText = DefaultUPCAText;

        public bool HasTexture => !string.IsNullOrEmpty(TextureName);

        public string NodeId { get; }
        public IntPtr Pointer { get; }

        public Vector3 Euler
        {
            get => Orientation.eulerAngles;
            set => Orientation = Quaternion.Euler(value);
        }

        public ImageTargetType ImageTargetType
        {
            get => imageTargetType;
            set
            {
                if (imageTargetType != value)
                {
                    imageTargetType = value;
                    OnImageTargetTypeChanged?.Invoke(NodeId);
                }
            }
        }

        public ArucoDictionaryName ArucoDictionary
        {
            get => arucoDictionary;
            set
            {
                if (arucoDictionary != value)
                {
                    arucoDictionary = value;
                    OnArucoDictionaryChanged?.Invoke(NodeId);
                }
            }
        }

        public int ArucoId
        {
            get => arucoId;
            set
            {
                if (arucoId != value)
                {
                    arucoId = value;
                    OnArucoIdChanged?.Invoke(NodeId);
                }
            }
        }

        public int MarkerLength
        {
            get
            {
                switch (ImageTargetType)
                {
                    case ImageTargetType.Aruco:
                        return arucoMarkerLength;
                    case ImageTargetType.QRCode:
                        return qrMarkerLength;
                    default:
                        return -1;
                }
            }
            set
            {
                switch (ImageTargetType)
                {
                    case ImageTargetType.Aruco:
                        SetMarkerLength(ref arucoMarkerLength, value);
                        break;
                    case ImageTargetType.QRCode:
                        SetMarkerLength(ref qrMarkerLength, value);
                        break;
                    default:
                        break;
                }

                void SetMarkerLength(ref int markerLength, int value)
                {
                    if (markerLength != value)
                    {
                        markerLength = value;
                        OnMarkerLengthChanged?.Invoke(NodeId);
                    }
                }
            }
        }

        public string Text
        {
            get
            {
                switch (ImageTargetType)
                {
                    case ImageTargetType.Barcode_EAN_13:
                        return ean13Text;
                    case ImageTargetType.Barcode_UPC_A:
                        return upcaText;
                    case ImageTargetType.QRCode:
                        return qrText;
                    default:
                        return string.Empty;
                }
            }
            set
            {
                switch (ImageTargetType)
                {
                    case ImageTargetType.Barcode_EAN_13:
                        SetText(ref ean13Text, value);
                        break;
                    case ImageTargetType.Barcode_UPC_A:
                        SetText(ref upcaText, value);
                        break;
                    case ImageTargetType.QRCode:
                        SetText(ref qrText, value);
                        break;
                    default:
                        break;
                }

                void SetText(ref string text, string value)
                {
                    if (text != value)
                    {
                        text = value;
                        OnTextChanged?.Invoke(NodeId);
                    }
                }
            }
        }

        public string Name { get; internal set; }

        public Quaternionf MLOrientation { get; internal set; } = new();

        public Vec3f MLPosition { get; internal set; } = new();

        public Vec3f MLScale { get; internal set; } = new();

        public Quaternion Orientation
        {
            get => Utils.ToUnityCoordinates(MLOrientation.ToQuat());

            set
            {
                MLOrientation = Utils.ToMLCoordinates(value).ToQuatf();

                OnOrientationChanged(NodeId);
            }
        }

        public float PhysicalHeight { get; internal set; }

        public float PhysicalWidth { get; internal set; }

        public Vector3 Position
        {
            get => Utils.ToUnityCoordinates(MLPosition.ToVec3());

            set
            {
                MLPosition = Utils.ToMLCoordinates(value).ToVec3f();
                OnPositionChanged(NodeId);
            }
        }

        public Vector3 Scale
        {
            get => Utils.ToUnityCoordinates(MLScale.ToVec3());

            set
            {
                MLScale = Utils.ToMLCoordinates(value).ToVec3f();

                OnScaleChanged(NodeId);
            }
        }

        public Texture Texture { get; internal set; }

        public string TextureId { get; internal set; } = string.Empty;

        public string TextureName { get; internal set; } = string.Empty;

        public ImageTargetStatus TrackingStatus { get; internal set; }

        public string GetTrackingStatus()
        {
            switch (TrackingStatus)
            {
                case ImageTargetStatus.NotTracked:
                    return "Not tracked";
                case ImageTargetStatus.Unreliable:
                    return "Unreliable";
                case ImageTargetStatus.Tracked:
                    return "Tracked";
                default:
                    return "Unknown";
            }
        }

        public ImageTargetBuilderWrapper(string nodeId, IntPtr pointer)
        {
            NodeId = nodeId;
            Pointer = pointer;
        }

        public ImageTargetBuilderWrapper(ImageTargetType imageTargetType)
        {
            this.imageTargetType = imageTargetType;
        }

        public void Dispose()
        {
            OnPositionChanged = null;
            OnOrientationChanged = null;
            OnScaleChanged = null;
            OnImageTargetTypeChanged = null;
            OnTextureChanged = null;
            OnArucoDictionaryChanged = null;
            OnArucoIdChanged = null;
            OnMarkerLengthChanged = null;
            OnTextChanged = null;
        }

        //TODO: This method should be rearrange probably as extension method
        //TODO: It has been extracted from name setter
        public void SetTextureBasedOnName(string name)
        {
            ImageTextureWrapper textureData = LoadedImageTextures.GetTextureByName(name);

            TextureId = textureData.Id;
            TextureName = textureData.Name;

            OnTextureChanged?.Invoke(NodeId);
        }
    }
}
