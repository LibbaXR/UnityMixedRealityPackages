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
using UnityEngine;

namespace MagicLeap.ZI
{
    internal abstract class ViewController<TModel, TViewPresenter> : PanelWindow where TModel : ViewModel, new() where TViewPresenter : CustomView, new()
    {
        protected TModel Model;
        protected TViewPresenter Presenter;

        protected virtual void Update()
        {
            try
            {
                Model.Update();
            }
            catch (ResultIsErrorException e)
            {
                //This try catch is required due to Unity error which block propagation of remaining methods
                //subscribed to EditorApplication.update if it encounters a crash.
                if (e.Result != Result.HandleNotConnected)
                {
                    Debug.LogError(e);
                }
            }
        }

        protected void OnEnable()
        {
            Model = new TModel();
            Presenter = new TViewPresenter();

            Initialize();
        }

        protected virtual void Initialize()
        {
            Model.Initialize();
            Model.OnSessionStopped += Presenter.ClearFields;
        }
    }
}
