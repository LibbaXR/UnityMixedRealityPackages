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
    internal class AsyncEvent
    {
        public string Message; // optional
        public float PercentDone = -1.0f; // optional

        public AsyncEventType Type;
        public EventUserMessageType UserMessageType = EventUserMessageType.Unknown; // optional

        public AsyncEvent(AsyncEventType type, string message, float percentDone)
        {
            Type = type;
            Message = message;
            PercentDone = percentDone;
        }

        public AsyncEvent(string message, EventUserMessageType userMessageType)
        {
            Type = AsyncEventType.UserMessage;
            Message = message;
            UserMessageType = userMessageType;
        }
    }
}
