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
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class SceneGraphViewState : IViewState
    {
        public List<string> OpenedSceneObjectFoldoutsIds;

        public bool IsSceneObjectsTabOpened;
        public bool IsPropertyFoldoutOpened;

        public Vector2 SceneObjectsScrollOffset;
        public Vector2 TrackedObjectsScrollOffset;

        public IReadOnlyList<string> SelectedNodesId = new List<string>();

        public void SetDefaultValues()
        {
            SelectedNodesId = new List<string>();
            OpenedSceneObjectFoldoutsIds = new List<string>();

            SceneObjectsScrollOffset = Vector2.zero;
            TrackedObjectsScrollOffset = Vector2.zero;

            IsSceneObjectsTabOpened = true;
            IsPropertyFoldoutOpened = true;
        }
    }
}
