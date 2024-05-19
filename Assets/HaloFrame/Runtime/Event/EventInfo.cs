using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class EventInfo : IReference
    {
        public Delegate Callback;
        public object Owner;
        public bool IsRelease;

        public EventInfo() { }

        public EventInfo(Delegate callback, object sender)
        {
            Callback = callback;
            Owner = sender;
            IsRelease = false;
        }

        public void Clear()
        {
            Callback = null;
            Owner = null;
            IsRelease = false;
        }

        public static EventInfo Get(Delegate callback, object sender)
        {
            var eventInfo = ReferencePool.Get<EventInfo>();
            eventInfo.Callback = callback;
            eventInfo.Owner = sender;
            eventInfo.IsRelease = false;
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

