// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Texture = ml.zi.Texture;


namespace MagicLeap.ZI
{
    internal class ImageTrackingViewModel : ViewModel
    {
        public event Action<bool> FollowHeadPoseUpdated;
        public event Action<string> ImageTargetModelUpdated;
        public event Action<bool> ActiveOnDeviceUpdated;

        private const double CacheUpdateFrequency = 1 / 10f; // 10Hz
        private const ulong ProgressMonitorTimeout = 15000;

        public readonly ObservableCollection<ImageTargetBuilderWrapper> ImageTargets = new();
        private readonly List<ImageTargetModelQueuedChanges> QueriedChanges = new();

        private double timeLastCacheUpdated;

        private static ZIBridge.ImageTrackingModule ImageTracking => Bridge.ImageTracking;

        private bool insideSyncModelData = false;  // true when receiving updated model state data from external source

        public void ClearAllTargets()
        {
            if (IsSessionRunning)
            {
                ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
                ImageTracking.Handle.DestroyAllImageTargets(pm);
            }
        }

        public void CreateNewImageTarget(string markerType)
        {
            ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
            var result = ImageTracking.Handle.CreateImageTarget(CreateImageTarget(Enum.Parse<ImageTargetType>(markerType)), pm);
            if (ZIFGen.ResultIsError(result.first))
            {
                Debug.LogError($"App Sim error: {result.second}");
            }
        }

        public void SetFollowHeadPose(bool followEnabled)
        {
            ImageTracking.Handle.SetFollowHeadpose(followEnabled);
        }

        public bool GetFollowHeadPose()
        {
            return ImageTracking.Handle.GetFollowHeadpose();
        }

        public bool GetActiveOnDevice()
        {
            return ImageTracking.Handle.GetActiveOnDevice();
        }

        public override void Initialize()
        {
            ImageTracking.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            ImageTracking.OnTakeChanges += ImageTrackingChanged;
            ImageTracking.StartListening(this);
        }

        public override void UnInitialize()
        {
            ImageTracking.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            ImageTracking.OnTakeChanges -= ImageTrackingChanged;
            ImageTracking.StopListening(this);
        }

        protected override void SessionStarted()
        {
            base.SessionStarted();

            if (!ImageTracking.Handle.GetActiveOnDevice())
            {
                ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
                ReturnedResultString result = ImageTracking.Handle.TestMarkerImageSupportIsInstalled(pm);
                if (ZIFGen.ResultIsError(result.first))
                {
                    result = ImageTracking.Handle.InstallMarkerImageSupport(pm);
                    if (ZIFGen.ResultIsError(result.first))
                    {
                        Debug.LogError($"App Sim error: {result.second}");
                    }
                }

                UpdateLoadedTextures();
                UpdateImageTargets();
            }
        }

        protected override void SessionStopped()
        {
            base.SessionStopped();
            ImageTargets.Clear();
        }

        public void CloneImageTarget(string nodeId)
        {
            ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
            var target = ImageTargets.Where(x => x.NodeId == nodeId).FirstOrDefault();
            ReturnedResultString result = ImageTracking.Handle.CreateImageTarget(CreateImageTarget(target), pm);
            if (ZIFGen.ResultIsError(result.first))
            {
                Debug.LogError($"App Sim error: {result.second}");
            }
        }

        public void RemoveImageTarget(string nodeId)
        {
            ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
            ImageTracking.Handle.DestroyImageTarget(nodeId, pm);
        }

        public override void Update()
        {
            if (IsSessionRunning)
            {
                CheckCache();
            }
        }

        internal void RefreshMarkers()
        {
            if (IsSessionRunning)
            {
                UpdateLoadedTextures();
                UpdateImageTargets();
            }
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ImageTracking.IsHandleConnected;
        }

        private ImageTargetBuilder CreateImageTarget(ImageTargetType targetType)
        {
            return CreateImageTarget(new ImageTargetBuilderWrapper(targetType));
        }

        private ImageTargetBuilder CreateImageTarget(ImageTargetBuilderWrapper wrapper)
        {
            ImageTargetBuilder result = ImageTargetBuilder.Alloc();
            result.SetImageTargetType(wrapper.ImageTargetType);

            switch (result.ImageTargetType)
            {
                case ImageTargetType.Aruco:
                    result.SetArucoDictionary(wrapper.ArucoDictionary);
                    result.SetArucoId((uint)wrapper.ArucoId);
                    result.SetMarkerLength((uint)wrapper.MarkerLength);
                    break;
                case ImageTargetType.QRCode:
                    result.SetTextData(wrapper.Text);
                    result.SetMarkerLength((uint)wrapper.MarkerLength);
                    break;
                case ImageTargetType.Barcode_EAN_13:
                case ImageTargetType.Barcode_UPC_A:
                    result.SetTextData(wrapper.Text);
                    break;
            }

            return result;
        }

        private void ApplyChangesToImageBuilder(ref ImageTargetBuilder trackerBuilder, ImageTargetBuilderWrapper wrapper,
                                                ImageTargetModelQueuedChanges changes)
        {
            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.Position))
            {
                trackerBuilder.Transform.SetPosition(wrapper.MLPosition);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.Orientation))
            {
                trackerBuilder.Transform.SetOrientation(wrapper.MLOrientation);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.Scale))
            {
                trackerBuilder.Transform.SetScale(wrapper.MLScale);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.ImageType))
            {
                trackerBuilder.SetImageTargetType(wrapper.ImageTargetType);
                switch (wrapper.ImageTargetType)
                {
                    case ImageTargetType.Aruco:
                        trackerBuilder.SetArucoDictionary(wrapper.ArucoDictionary);
                        trackerBuilder.SetArucoId((uint)wrapper.ArucoId);
                        trackerBuilder.SetMarkerLength((uint)wrapper.MarkerLength);
                        break;
                    case ImageTargetType.QRCode:
                        trackerBuilder.SetTextData(wrapper.Text);
                        trackerBuilder.SetMarkerLength((uint)wrapper.MarkerLength);
                        break;
                    case ImageTargetType.Barcode_EAN_13:
                    case ImageTargetType.Barcode_UPC_A:
                        trackerBuilder.SetTextData(wrapper.Text);
                        break;
                }
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.AssignedImage))
            {
                trackerBuilder.SetTextureId(wrapper.TextureId);
                trackerBuilder.SetTextureName(wrapper.TextureName);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.ArucoDictionary))
            {
                trackerBuilder.SetArucoDictionary(wrapper.ArucoDictionary);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.ArucoId))
            {
                trackerBuilder.SetArucoId((uint)wrapper.ArucoId);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.MarkerLength))
            {
                trackerBuilder.SetMarkerLength((uint)wrapper.MarkerLength);
            }

            if (changes.QueuedChanges.HasFlag(ImageTargetModelQueuedChanges.ChangesType.Text))
            {
                trackerBuilder.SetTextData(wrapper.Text);
            }
        }

        private void CheckCache()
        {
            if (QueriedChanges.Count <= 0)
            {
                return;
            }

            if (!(EditorApplication.timeSinceStartup - timeLastCacheUpdated > CacheUpdateFrequency))
            {
                return;
            }

            PushCacheChanges();
            timeLastCacheUpdated = EditorApplication.timeSinceStartup;
        }

        private void ImageTrackingChanged(ImageTrackingChanges changes)
        {
            if (changes.HasFlag(ImageTrackingChanges.ActiveOnDevice))
            {
                ActiveOnDeviceUpdated?.Invoke(GetActiveOnDevice());
            }
            if (changes.HasFlag(ImageTrackingChanges.TargetTexturesChanged))
            {
                using TextureList textureList = TextureList.Alloc();
                ImageTracking.Handle.TakeChangedTargetTextures(textureList);
                UpdateLoadedTextures(textureList);
            }
            if (changes.HasFlag(ImageTrackingChanges.TargetsDestroyed))
            {
                using StringList idList = StringList.Alloc();
                ImageTracking.Handle.TakeDestroyedImageTargets(idList);
                for (uint i = 0; i < idList.GetSize(); ++i)
                {
                    OnImageTargetRemoved(idList.Get(i));
                }
            }
            if (changes.HasFlag(ImageTrackingChanges.TargetsAdded))
            {
                using ImageTargetList imageTargetsList = ImageTargetList.Alloc();
                ImageTracking.Handle.TakeAddedImageTargets(imageTargetsList);
                for (uint i = 0; i < imageTargetsList.GetSize(); ++i)
                {
                    ImageTargetBuilder imageTarget = imageTargetsList.Get(i);
                    string nodeId = imageTarget.NodeId;
                    if (ImageTargets.FirstOrDefault(x => x.NodeId == nodeId) == null)
                    {
                        OnImageTargetCreated(imageTarget);
                    }
                }
            }
            if (changes.HasFlag(ImageTrackingChanges.TargetsChanged))
            {
                using ImageTargetList imageTargetsList = ImageTargetList.Alloc();
                ImageTracking.Handle.TakeChangedImageTargets(imageTargetsList);
                for (uint i = 0; i < imageTargetsList.GetSize(); ++i)
                {
                    ImageTargetBuilder imageTarget = imageTargetsList.Get(i);
                    string nodeId = imageTarget.NodeId;
                    ImageTargetBuilderWrapper wrapper = ImageTargets.FirstOrDefault(x => x.NodeId == nodeId);
                    if (wrapper != null)
                    {
                        wrapper.Name = GetImageTargetName(nodeId);
                        SyncModelData(wrapper, imageTarget);
                        ImageTargetModelUpdated?.Invoke(nodeId);
                    }
                }
            }
            if (changes.HasFlag(ImageTrackingChanges.TargetTexturesChanged) || changes.HasFlag(ImageTrackingChanges.TargetsDestroyed))
            {
                RemoveUnreferencedTextures();
            }

            if (changes.HasFlag(ImageTrackingChanges.FollowHeadpose))
            {
                FollowHeadPoseUpdated?.Invoke(ImageTracking.Handle.GetFollowHeadpose());
            }
        }

        private void OnImageTargetCreated(ImageTargetBuilder imageTarget)
        {
            var wrapper = new ImageTargetBuilderWrapper(imageTarget.NodeId, imageTarget.GetBuilder());

            SyncModelData(wrapper, imageTarget);
            wrapper.Name = GetImageTargetName(imageTarget.NodeId);

            wrapper.OnPositionChanged += QueuePositionChange;
            wrapper.OnOrientationChanged += QueueOrientationChange;
            wrapper.OnScaleChanged += QueueScaleChange;
            wrapper.OnImageTargetTypeChanged += QueueImageTargetTypeChange;
            wrapper.OnTextureChanged += QueueAssignedImageChange;
            wrapper.OnArucoDictionaryChanged += QueueArucoDictionaryChange;
            wrapper.OnArucoIdChanged += QueueArucoIdChange;
            wrapper.OnMarkerLengthChanged += QueueMarkerLengthChange;
            wrapper.OnTextChanged += QueueTextChange;

            ImageTargets.Add(wrapper);
        }

        private void OnImageTargetRemoved(string removedId)
        {
            ImageTargetBuilderWrapper wrapper = ImageTargets.FirstOrDefault(x => x.NodeId == removedId);
            if (wrapper != null)
            {
                ImageTargets.Remove(wrapper);
                wrapper.Dispose();
            }
        }

        private void PushCacheChanges()
        {
            using ImageTargetList inImageTargets = ImageTargetList.Alloc();

            foreach (ImageTargetBuilderWrapper wrapper in ImageTargets)
            {
                ImageTargetModelQueuedChanges changes = QueriedChanges.Find(x => x.NodeId == wrapper.NodeId);

                if (changes.IsEmpty)
                {
                    continue;
                }

                try
                {
                    ImageTargetBuilder trackerBuilder = ImageTargetBuilder.Alloc();

                    ImageTracking.Handle.GetImageTarget(wrapper.NodeId, trackerBuilder);

                    ApplyChangesToImageBuilder(ref trackerBuilder, wrapper, changes);

                    inImageTargets.Append(trackerBuilder);
                }
                catch (ResultIsErrorException e)
                {
                    // no image target?
                    Debug.LogWarning(e.Message);
                }
            }

            using ProgressMonitor pm = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
            using StringList sl = StringList.Alloc();

            try
            {
                var result = ImageTracking.Handle.ModifyImageTargets(inImageTargets, sl, pm);
                if (ZIFGen.ResultIsError(result.first))
                {
                    Debug.LogError($"App Sim error: {result.second}");
                }
            }
            catch (ResultIsErrorException e)
            {
                // no image target?
                Debug.LogWarning(e.Message);
            }
            finally
            {
                QueriedChanges.Clear();
            }
        }

        private void QueueAssignedImageChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.ImageType);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.AssignedImage(nodeId));
            }
        }

        private void QueueImageTargetTypeChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.ImageType);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.ImageType(nodeId));
            }
        }

        private void QueueOrientationChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.Orientation);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.Orientation(nodeId));
            }
        }

        private void QueuePositionChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.Position);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.Position(nodeId));
            }
        }

        private void QueueScaleChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.Scale);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.Scale(nodeId));
            }
        }

        private void QueueArucoDictionaryChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.ArucoDictionary);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.ArucoDictionary(nodeId));
            }
        }

        private void QueueArucoIdChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.ArucoId);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.ArucoId(nodeId));
            }
        }

        private void QueueMarkerLengthChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.MarkerLength);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.MarkerLength(nodeId));
            }
        }

        private void QueueTextChange(string nodeId)
        {
            if (insideSyncModelData) { return; }
            ImageTargetModelQueuedChanges queuedChange = QueriedChanges.FirstOrDefault(x => x.NodeId == nodeId);

            if (!queuedChange.IsEmpty)
            {
                queuedChange.Merge(ImageTargetModelQueuedChanges.ChangesType.Text);
            }
            else
            {
                QueriedChanges.Add(ImageTargetModelQueuedChanges.Text(nodeId));
            }
        }

        private void SyncModelData(ImageTargetBuilderWrapper wrapper, ImageTargetBuilder imageTarget)
        {
            if (insideSyncModelData) { Debug.LogError("Reentrancy detected inside SyncModelData."); }
            insideSyncModelData = true;
            try
            {

                wrapper.MLPosition = imageTarget.Transform.Position;
                wrapper.MLOrientation = imageTarget.Transform.Orientation;
                wrapper.MLScale = imageTarget.Transform.Scale;
                wrapper.ImageTargetType = imageTarget.ImageTargetType;
                wrapper.TextureId = imageTarget.TextureId;
                wrapper.TextureName = imageTarget.TextureName;
                wrapper.TrackingStatus = imageTarget.TargetStatus;
                wrapper.PhysicalHeight = imageTarget.GetPhysicalHeight();
                wrapper.PhysicalWidth = imageTarget.GetPhysicalWidth();
                wrapper.MarkerLength = (int)imageTarget.MarkerLength;
                switch (wrapper.ImageTargetType)
                {
                    case ImageTargetType.Aruco:
                        wrapper.ArucoDictionary = imageTarget.ArucoDictionary;
                        wrapper.ArucoId = (int)imageTarget.ArucoId;
                        break;
                    case ImageTargetType.Barcode_EAN_13:
                    case ImageTargetType.Barcode_UPC_A:
                    case ImageTargetType.QRCode:
                        wrapper.Text = imageTarget.TextData;
                        break;
                }
                if (LoadedImageTextures.TryGetTextureById(wrapper.TextureId, out ImageTextureWrapper textureData))
                {
                    wrapper.Texture = textureData.Texture;
                }
            }
            finally
            {
                insideSyncModelData = false;
            }
        }

        private void UpdateImageTargets()
        {
            using ImageTargetList imageTargetsList = ImageTargetList.Alloc();

            ImageTracking.Handle.GetAllImageTargets(imageTargetsList);

            uint size = imageTargetsList.GetSize();
            var newAllocatedImageTargets = new ImageTargetBuilder[(int)size];
            for (uint i = 0; i < size; i++)
            {
                newAllocatedImageTargets[i] = imageTargetsList.Get(i);
            }

            string[] newIds = newAllocatedImageTargets.Select(x => x.NodeId).ToArray();
            string[] existingIds = ImageTargets.Select(x => x.NodeId).ToArray();

            List<string> removed = existingIds.Except(newIds).ToList();
            List<string> created = newIds.Except(existingIds).ToList();
            List<string> commons = existingIds.Intersect(newIds).ToList();

            foreach (string removedId in removed)
            {
                OnImageTargetRemoved(removedId);
            }

            foreach (string addedId in created)
            {
                ImageTargetBuilder imageTarget = newAllocatedImageTargets.First(x => x.NodeId == addedId);
                OnImageTargetCreated(imageTarget);
            }

            for (int i = 0; i < commons.Count; i++)
            {
                string commonId = commons[i];
                ImageTargetBuilderWrapper wrapper = ImageTargets.First(x => x.NodeId == commonId);
                ImageTargetBuilder imageTarget = newAllocatedImageTargets.First(x => x.NodeId == commonId);

                wrapper.Name = GetImageTargetName(wrapper.NodeId);
                SyncModelData(wrapper, imageTarget);

                ImageTargetModelUpdated?.Invoke(commonId);
            }
        }

        private void UpdateLoadedTextures(TextureList textureList)
        {
            var idSet = new HashSet<string>();
            uint size = textureList.GetSize();
            for (uint i = 0; i < size; i++)
            {
                Texture texture = textureList.Get(i);

                if (string.IsNullOrEmpty(texture.GetTextureId()) || idSet.Add(texture.GetTextureId()) == false)
                {
                    continue;
                }

                var t = new Texture2D(
                    (int)texture.GetWidth(),
                    (int)texture.GetHeight(),
                    (UnityEngine.TextureFormat)texture.GetFormat(),
                    false);
                byte[] memoryDump = new byte[texture.GetMemorySize()];
                texture.GetMemoryContent(memoryDump);
                t.LoadRawTextureData(memoryDump);
                t.Apply();

                ImageTextureWrapper imageTexture;
                if (LoadedImageTextures.TryGetTextureById(texture.GetTextureId(), out imageTexture) == false)
                {
                    imageTexture = new ImageTextureWrapper
                    {
                        Id = texture.GetTextureId(),
                        Name = texture.GetTextureName(),
                        Width = texture.GetWidth(),
                        Height = texture.GetHeight(),
                        Texture = t
                    };
                    LoadedImageTextures.Add(imageTexture);
                }
                else
                {
                    imageTexture.Name = texture.GetTextureName();
                    imageTexture.Width = texture.GetWidth();
                    imageTexture.Height = texture.Height;
                    imageTexture.Texture = t;
                }
            }

            UpdateModelTextures(idSet);
        }

        private void UpdateLoadedTextures()
        {
            using TextureList textureList = TextureList.Alloc();
            ImageTracking.Handle.GetAllTextures(textureList);
            UpdateLoadedTextures(textureList);
        }

        private void UpdateModelTextures(ICollection<string> textureIds)
        {
            foreach (ImageTargetBuilderWrapper wrapper in ImageTargets)
            {
                if (textureIds.Contains(wrapper.TextureId) &&
                    LoadedImageTextures.TryGetTextureById(wrapper.TextureId, out ImageTextureWrapper textureData))
                {
                    if (false == object.ReferenceEquals(wrapper.Texture, textureData.Texture))
                    {
                        wrapper.Texture = textureData.Texture;
                        ImageTargetModelUpdated?.Invoke(wrapper.NodeId);
                    }
                }
            }
        }

        private int RemoveUnreferencedTextures()
        {
            int cntRemoved = 0;
            var textureIds = new HashSet<string>(LoadedImageTextures.GetTextureIds());
            foreach (ImageTargetBuilderWrapper wrapper in ImageTargets)
            {
                textureIds.Remove(wrapper.TextureId);
            }
            foreach (string textureId in textureIds)
            {
                if (LoadedImageTextures.RemoveById(textureId))
                {
                    ++cntRemoved;
                }
            }
            return cntRemoved;
        }
        
        private string GetImageTargetName(string id)
        {
            SceneNodeBuilder newNode = SceneNodeBuilder.Alloc();
            string name = string.Empty;

            try
            {
                ProgressMonitor progressMonitor = ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout);
                Bridge.SceneGraph.Handle.GetSceneNodeById(id, newNode, false, progressMonitor);
                name = newNode.Name;
            }
            catch (ResultIsErrorException exception)
            {
                if (exception.Result != Result.DoesNotExist)
                {
                    throw;
                }
            }
            finally
            {
                newNode.Dispose();
            }

            return name;
        }
    }
}
