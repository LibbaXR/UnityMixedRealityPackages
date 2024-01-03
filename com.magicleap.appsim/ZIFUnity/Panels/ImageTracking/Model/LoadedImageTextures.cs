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
using System.Linq;

namespace MagicLeap.ZI
{
    internal static class LoadedImageTextures
    {
        private static readonly List<ImageTextureWrapper> textures = new();

        public static void Add(ImageTextureWrapper imageTexture)
        {
            textures.Add(imageTexture);
        }

        public static void Clear()
        {
            textures.Clear();
        }

        public static bool TryGetTextureById(string id, out ImageTextureWrapper wrapper)
        {
            wrapper = textures.FirstOrDefault(x => x.Id == id);
            return wrapper != null;
        }

        public static ImageTextureWrapper GetTextureByName(string name)
        {
            return textures.First(x => x.Name == name);
        }

        public static string[] GetTextureNames()
        {
            return textures.Select(x => x.Name).ToArray();
        }

        public static IEnumerable<string> GetTextureIds()
        {
            return textures.Select(x => x.Id);
        }

        public static bool RemoveById(string textureId)
        {
            int index = textures.FindIndex(x => x.Id == textureId);
            if (index >= 0)
            {
                textures.RemoveAt(index);
                return true;
            }
            return false;
        }
    }
}
