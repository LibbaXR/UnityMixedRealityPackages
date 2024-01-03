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

namespace MagicLeap.ZI
{
    internal class AsyncTaskState<T>
    {
        public readonly AsyncEventQueue queue = new();

        public bool Cancel = false;
        public bool Complete = false;
        public T Result = default;

        public void EventCallback(AsyncEventType type, string message, float percentDone, EventUserMessageType userMessageType)
        {
            if (type == AsyncEventType.UserMessage)
            {
                queue.QueueEvent(new AsyncEvent(message, userMessageType));
            }
            else
            {
                queue.QueueEvent(type, message, percentDone);
            }
        }
    }
}
