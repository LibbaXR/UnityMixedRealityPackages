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

namespace MagicLeap.ZI
{
    internal class AsyncEventQueue
    {
        private static readonly Queue<AsyncEvent> eventQueue = new();

        public AsyncEvent PollEvent()
        {
            lock (eventQueue)
            {
                if (eventQueue.Count > 0)
                {
                    return eventQueue.Dequeue();
                }
            }

            return null;
        }

        public void QueueEvent(AsyncEventType type, string message, float percentDone)
        {
            QueueEvent(new AsyncEvent(type, message, percentDone));
        }

        public void QueueEvent(AsyncEvent evt)
        {
            lock (eventQueue)
            {
                eventQueue.Enqueue(evt);
            }
        }
    }
}
