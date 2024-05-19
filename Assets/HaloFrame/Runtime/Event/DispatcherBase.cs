using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class DispatcherBase : IManager, IReference
    {
        private readonly Dictionary<int, List<EventInfo>> eventDict;
        private readonly HashSet<int> processSet;
        private readonly HashSet<int> deleteSet;

        public DispatcherBase()
        {
            eventDict = new Dictionary<int, List<EventInfo>>();
            processSet = new HashSet<int>();
            deleteSet = new HashSet<int>();
        }

        public void Clear()
        {
            foreach (var item in eventDict)
            {
                foreach (var eventInfo in item.Value)
                {
                    EventInfo.Release(eventInfo);
                }
            }

            eventDict.Clear();
            processSet.Clear();
            deleteSet.Clear();
        }

        /// <summary>
        /// 添加事件监听
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="callback"></param>
        /// <param name="owner"></param>
        protected void AddListener(int eventId, Delegate callback, object owner)
        {
            if (!eventDict.TryGetValue(eventId, out var listeners))
            {
                listeners = new List<EventInfo>();
                eventDict.Add(eventId, listeners);
            }

            // 同一个object，同一个事件，不允许多次监听
            foreach (var item in listeners)
            {
                if (item.Callback == callback && item.Owner == owner)
                {
                    if (item.IsRelease)
                    {
                        // 如果被标记回收，但是还没回收，需要重新激活
                        item.IsRelease = false;
                    }
                    Debugger.Log($"重复添加监听事件：{eventId} {owner.GetType().Name} {callback.Method.Name}", LogDomain.Event);
                    return; // 已经存在，直接返回
                }
            }

            // 不存在才添加
            listeners.Add(EventInfo.Get(callback, owner));
        }

        /// <summary>
        /// 移除object身上所有监听的事件
        /// </summary>
        /// <param name="owner"></param>
        public void OwnerRemoveAll(object owner)
        {
            foreach (var item in eventDict)
            {
                var eventId = item.Key;
                var listeners = item.Value;
                var isProcess = processSet.Contains(eventId);

                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    var eventInfo = listeners[i];
                    if (eventInfo.Owner == owner)
                    {
                        if (isProcess)
                        {
                            // 有正在处理的事件，不能直接移除
                            eventInfo.IsRelease = true;
                            AddWaitDelEvent(eventId);
                        }
                        else
                        {
                            EventInfo.Release(eventInfo);
                            listeners.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 移除事件监听
        /// 如果事件正在处理中，不能直接移除，需要等待处理完毕后移除
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="listener"></param>
        public void RemoveListener(int eventId, Delegate listener)
        {
            if (!eventDict.TryGetValue(eventId, out var listeners))
                return;

            var isProcess = processSet.Contains(eventId);
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                var eventInfo = listeners[i];
                if (eventInfo.Callback == listener)
                {
                    if (isProcess)
                    {
                        // 事件正在处理中，不能直接移除
                        eventInfo.IsRelease = true;
                        AddWaitDelEvent(eventId);
                    }
                    else
                    {
                        EventInfo.Release(eventInfo);
                        listeners.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 在移除事件时，如果此事件正在执行中，会添加到这里
        /// </summary>
        /// <param name="eventId"></param>
        private void AddWaitDelEvent(int eventId)
        {
            if (deleteSet.Contains(eventId))
                return;

            deleteSet.Add(eventId);
        }

        private void CheckWaitDelEvent(int eventId)
        {
            if (!deleteSet.Contains(eventId))
                return;
            deleteSet.Remove(eventId);

            if (eventDict.TryGetValue(eventId, out var listeners))
            {
                for (int i = listeners.Count - 1; i >= 0; i--)
                {
                    var eventInfo = listeners[i];
                    if (eventInfo.IsRelease)
                    {
                        Debugger.Log($"延时移除事件 {eventId} {eventInfo.Callback.Method}", LogDomain.Event);
                        EventInfo.Release(eventInfo);
                        listeners.RemoveAt(i);
                    }
                }
            }
        }

        public void Notify(int eventId)
        {
            if (!eventDict.TryGetValue(eventId, out var listeners))
                return;

            // 执行过程中可能会取消监听或者增加监听，这里需要加一个标记
            processSet.Add(eventId);
            var count = listeners.Count;
            for (int i = 0; i < count; i++)
            {
                var info = listeners[i];
                if (info.IsRelease)
                    continue;

                if (info.Callback is Action callBack)
                {
                    callBack.Invoke();
                }
            }

            // 执行完毕之后移除标记
            processSet.Remove(eventId);
            CheckWaitDelEvent(eventId);
        }

        public void Notify<T1>(int eventId, T1 arg1)
        {
            if (!eventDict.TryGetValue(eventId, out var listeners))
                return;

            // 执行过程中可能会取消监听或者增加监听，这里需要加一个标记
            processSet.Add(eventId);
            var count = listeners.Count;
            for (int i = 0; i < count; i++)
            {
                var info = listeners[i];
                if (info.IsRelease)
                    continue;

                if (info.Callback is Action<T1> callBack)
                {
                    callBack.Invoke(arg1);
                }
            }

            // 执行完毕之后移除标记
            processSet.Remove(eventId);
            CheckWaitDelEvent(eventId);
        }
    }
}
