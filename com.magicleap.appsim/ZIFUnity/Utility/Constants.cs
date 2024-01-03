// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.IO;

namespace MagicLeap.ZI
{
    internal static class Constants
    {
        public static readonly string zifPackageName = "com.magicleap.appsim";
        public static readonly string zifPackagePath = Path.Combine("Packages", zifPackageName).Replace("\\", "/");
        public static readonly string zifPluginName = "com.magicleap.zif.dll";
        public static readonly string zifSimulatorDeviceName = "SimulatorManagerSparseDevice";
        public static readonly string zifEditorPath = Path.Combine(zifPackagePath, "ZIFUnity/Editor").Replace("\\", "/");
        public static readonly string zifPluginUserName = "Magic Leap App Sim";
    }
}
