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
    internal struct ImageTargetModelQueuedChanges
    {
        [Flags]
        public enum ChangesType
        {
            Position = 1 << 0,
            Orientation = 1 << 1,
            Scale = 1 << 2,
            ImageType = 1 << 3,
            AssignedImage = 1 << 4,
            ArucoDictionary = 1 << 5,
            ArucoId = 1 << 6,
            MarkerLength = 1 << 7,
            Text = 1 << 8
        }

        public readonly string NodeId;

        public bool IsEmpty => string.IsNullOrEmpty(NodeId);

        public ChangesType QueuedChanges { get; private set; }

        public ImageTargetModelQueuedChanges(string nodeId, ChangesType queuedChanges)
        {
            NodeId = nodeId;
            QueuedChanges = queuedChanges;
        }

        public static ImageTargetModelQueuedChanges AssignedImage(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.AssignedImage);
        }

        public static ImageTargetModelQueuedChanges ImageType(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.ImageType);
        }

        public static bool operator ==(ImageTargetModelQueuedChanges lhs, ImageTargetModelQueuedChanges rhs)
        {
            return lhs.NodeId == rhs.NodeId && lhs.QueuedChanges == rhs.QueuedChanges;
        }

        public static bool operator !=(ImageTargetModelQueuedChanges lhs, ImageTargetModelQueuedChanges rhs)
        {
            return !(lhs == rhs);
        }

        public static ImageTargetModelQueuedChanges Orientation(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.Orientation);
        }

        public static ImageTargetModelQueuedChanges Position(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.Position);
        }

        public static ImageTargetModelQueuedChanges Scale(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.Scale);
        }

        public static ImageTargetModelQueuedChanges ArucoDictionary(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.ArucoDictionary);
        }

        public static ImageTargetModelQueuedChanges ArucoId(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.ArucoId);
        }

        public static ImageTargetModelQueuedChanges MarkerLength(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.MarkerLength);
        }

        public static ImageTargetModelQueuedChanges Text(string nodeId)
        {
            return new ImageTargetModelQueuedChanges(nodeId, ChangesType.Text);
        }

        public override bool Equals(object obj)
        {
            return NodeId == ((ImageTargetModelQueuedChanges) obj).NodeId &&
                QueuedChanges == ((ImageTargetModelQueuedChanges) obj).QueuedChanges;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Merge(ChangesType changesFlag)
        {
            QueuedChanges &= changesFlag;
        }

        public void Merge(ImageTargetModelQueuedChanges changes)
        {
            QueuedChanges &= changes.QueuedChanges;
        }
    }
}
