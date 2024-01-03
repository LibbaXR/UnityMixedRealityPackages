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
using System.Linq;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class EyeGazeSubPanels : ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>
    {
        private readonly List<ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>> subPanels;

        public EyeGazeSubPanels(IEnumerable<ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>> panels)
        {
            subPanels = panels.ToList();
        }

        public EyeGazeViewPresenter Panel
        {
            set
            {
                foreach (var subPanel in subPanels)
                {
                    subPanel.Panel = value;
                }
            }
        }

        public EyeGazeViewState State
        {
            set
            {
                foreach (var subPanel in subPanels)
                {
                    subPanel.State = value;
                }
            }
        }

        public VisualElement Root
        {
            set
            {
                foreach (var subPanel in subPanels)
                {
                    subPanel.Root = value;
                }
            }
        }

        public void RegisterUICallbacks()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.RegisterUICallbacks();
            }
        }

        public void UnRegisterUICallbacks()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.UnRegisterUICallbacks();
            }
        }

        public void BindUIElements()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.BindUIElements();
            }
        }

        public void AssertFields()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.AssertFields();
            }
        }

        public void SynchronizeViewWithState()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.SynchronizeViewWithState();
            }
        }

        public void SetPanelActive(bool isEnabled)
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.SetPanelActive(isEnabled);
            }
        }

        public void Serialize()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.Serialize();
            }
        }

        public void ClearFields()
        {
            foreach (var subPanel in subPanels)
            {
                subPanel.ClearFields();
            }
        }
    }
}
