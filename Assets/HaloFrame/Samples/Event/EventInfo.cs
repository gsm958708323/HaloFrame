using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class EventInfo : IReference
    {
        public Delegate Callback;
        public object Sender;
        public bool IsDestroy;

        public EventInfo() { }

        public EventInfo(Delegate callback, object sender)
        {
            Callback = callback;
            Sender = sender;
            IsDestroy = false;
        }

        public void Clear()
        {
            Callback = null;
            Sender = null;
            IsDestroy = false;
        }

        public static EventInfo Get(Delegate callback, object sender)
        {
            var eventInfo = ReferencePool.Get<EventInfo>();
            eventInfo.Callback = callback;
            eventInfo.Sender = sender;
            eventInfo.IsDestroy = false;
            return eventInfo;
        }

        public static EventInfo GetTest<TArg1>(Action<TArg1> handler, object sender)
        {
            return Get(handler, sender);
        }

        public static void Release(EventInfo eventInfo)
        {
            ReferencePool.Release(eventInfo);
        }
    }
}

