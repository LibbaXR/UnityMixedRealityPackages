// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class SettingsLoader
    {
        private readonly SettingsJson settingJson;

        public SettingsLoader(string filePath)
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);

            // Build in JsonUtility doesn't let you parse to array so it needs to have an object at root. 
            // This way handles that without modifying json.
            settingJson = JsonUtility.FromJson<SettingsJson>($"{{\"Settings\":{json}}}");
        }
        
        public IEnumerable<Setting> GetByCategory(string category)
        {
            return settingJson.Settings.Where(x => x.Category.Contains(category))
                              .SelectMany(x => x.Settings);
        }
    }
}
