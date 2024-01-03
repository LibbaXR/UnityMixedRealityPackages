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
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class SceneGraphNodeData
    {
        public readonly List<SceneGraphNodeData> Children = new();

        public readonly string Id;
        public readonly string Name;
        public readonly SceneNodeType Type;
        private static readonly Dictionary<string, Texture2D> cachedTextures = new();

        public bool HasChildren => Children.Count > 0;
        public bool HasParent => Parent != null;
        public Texture2D Icon => GetIcon();

        public SceneGraphNodeData Parent { private set; get; }

        public SceneGraphNodeData(string id, string name, SceneNodeType nodeType, SceneGraphNodeData parent)
        {
            Parent = parent;
            Id = id;
            Name = name;
            Type = nodeType;
        }

        private Texture2D GetIcon()
        {
            if (Type == SceneNodeType.RoomNode)
            {
                return GetIconTexture("Room");
            }

            if (HasChildren)
            {
                return GetIconTexture("NodeGroup");
            }

            return GetIconTexture("Ellipse");
        }

        private Texture2D GetIconTexture(string iconName)
        {
            if (!cachedTextures.TryGetValue(iconName, out Texture2D icon))
            {
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/com.magicleap.appsim/Icons/{iconName}.png");
                cachedTextures[iconName] = icon;
            }

            return icon;
        }
    }
}
