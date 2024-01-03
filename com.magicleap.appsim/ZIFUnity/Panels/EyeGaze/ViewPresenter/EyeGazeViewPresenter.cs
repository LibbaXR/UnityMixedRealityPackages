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
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class EyeGazeViewPresenter : ResettableTransformViewPresenter<EyeGazeViewState>
    {
        private EyeGazeSubPanels eyeGazeSubPanels;
        public EyeTrackingSubView EyeTrackingSubView { get; } = new();

        public GazeRecognitionSubView GazeRecognitionSubView { get; } = new();

        public override void OnEnable(VisualElement root)
        {
            eyeGazeSubPanels = new EyeGazeSubPanels(new List<ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>>
            {
                EyeTrackingSubView,
                GazeRecognitionSubView
            });
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.magicleap.appsim/ZIFUnity/Panels/EyeGaze/Views/ZIEyeGazeView.uxml");
            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/EyeGaze/Views/ZIEyeGazeDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/EyeGaze/Views/ZIEyeGazeLightStyle.uss");
            eyeGazeSubPanels.Panel = this;
            eyeGazeSubPanels.Root = Root;
            eyeGazeSubPanels.State = State;
        }

        public void SetPanelActive(bool isEnabled)
        {
            resetButton.SetEnabled(isEnabled);
            eyeGazeSubPanels.SetPanelActive(isEnabled);
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            eyeGazeSubPanels.AssertFields();
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();
            eyeGazeSubPanels.Root = Root;
            eyeGazeSubPanels.BindUIElements();
        }

        protected override void Serialize()
        {
            eyeGazeSubPanels.State = State;
            eyeGazeSubPanels.Panel = this;
            eyeGazeSubPanels.Serialize();
            base.Serialize();
        }

        protected override void SynchronizeViewWithState()
        {
            eyeGazeSubPanels.State = State;
            eyeGazeSubPanels.SynchronizeViewWithState();
        }

        public override void ClearFields()
        {
            eyeGazeSubPanels.ClearFields();
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            eyeGazeSubPanels.RegisterUICallbacks();
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            eyeGazeSubPanels.UnRegisterUICallbacks();
        }
    }
}
