// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class SceneCameraViewState : IViewState
    {
        public void SetDefaultValues()
        {
        }
    }

    internal class SceneCameraViewPresenter : AlignableTransformViewPresenter<SceneCameraViewState>
    {
        public override void OnEnable(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/SceneViewCamera/Views/ZISceneCameraView.uxml");

            visualTree.CloneTree(root);

            base.OnEnable(root);

            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HeadPose/Views/ZIHeadPoseDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HeadPose/Views/ZIHeadPoseLightStyle.uss");
        }
    }
}
