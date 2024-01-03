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
using NUnit.Framework;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class GenericListView<TObjectView> : ObjectView where TObjectView : ObjectView
    {
        public readonly List<TObjectView> ObjectViews = new();
        protected readonly VisualElement Content;
        protected readonly ScrollView ScrollView;

        public GenericListView(ScrollView scrollView)
        {
            ScrollView = scrollView;
            Content = scrollView.contentContainer;
        }

        public void AddToView(TObjectView element)
        {
            ObjectViews.Add(element);
            Content.Add(element.VisualElement);
        }

        public void Clear()
        {
            for (int i = ObjectViews.Count - 1; i >= 0; i--)
            {
                RemoveFromView(ObjectViews[i]);
            }
        }

        public void RemoveFromView(TObjectView element)
        {
            ObjectViews.Remove(element);
            Content.Remove(element.VisualElement);
        }

        public override void SetEnabled(bool state)
        {
            foreach (TObjectView objectView in ObjectViews)
            {
                objectView.SetEnabled(state);
            }
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(ScrollView, nameof(ScrollView));
            Assert.IsNotNull(Content, nameof(Content));
        }

        protected override void BindUIElements()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            Root = ScrollView;
        }

        protected override void RegisterUICallbacks()
        {
        }

        protected override void SynchronizeViewWithState()
        {
        }

        protected override void UnregisterUICallbacks()
        {
        }
    }
}
