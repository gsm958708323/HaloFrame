using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class Dispatcher : DispatcherBase
    {
        public void AddListener(int eventId, Action callback, object owner)
        {
            base.AddListener(eventId, callback, owner);
        }
        public void AddListener<T1>(int eventId, Action<T1> callback, object owner)
        {
            base.AddListener(eventId, callback, owner);
        }

        public void RemoveListener(int eventId, Action callback)
        {
            base.RemoveListener(eventId, callback);
        }
        public void RemoveListener<T1>(int eventId, Action<T1> callback)
        {
            base.RemoveListener(eventId, callback);
        }
    }
}

/*
以下写法会有装箱拆箱操作

public void SendEvent(int eventId)
{
    SendEventInternal(eventId, null);
}

public void SendEvent<TArg1>(int eventId, TArg1 arg1)
{
    SendEventInternal(eventId, arg1);
}

private void SendEventInternal(int eventId, params object[] args)
{
    if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
    {
        _processEventList.Add(eventId);

        var count = listListener.Count;
        for (int i = 0; i < count; i++)
        {
            var node = listListener[i];
            if (node.IsDeleted)
            {
                continue;
            }

            var callback = listListener[i].Callback;
            if (callback != null && callback.GetType().IsGenericType)
            {
                var callbackParams = callback.GetType().GetGenericArguments();
                if (callbackParams.Length == args.Length)
                {
                    callback.DynamicInvoke(args);
                }
                else
                {
                    Log.Fatal("Invalid number of event parameters: {0}. Expected: {1}, Actual: {2}", eventId, callbackParams.Length, args.Length);
                }
            }
            else
            {
                Log.Fatal("Invalid event data type: {0}", eventId);
            }
        }

        _processEventList.Remove(eventId);
        CheckDelayDelete(eventId);
    }
}
*/