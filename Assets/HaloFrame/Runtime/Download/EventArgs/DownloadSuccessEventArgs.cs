﻿using System;

namespace HaloFrame
{
    public class DownloadSuccessEventArgs : IReference
    {
        public DownloadInfo DownloadInfo { get; private set; }
        public int CurrentDownloadTaskIndex { get; private set; }
        public int DownloadTaskCount { get; private set; }
        public TimeSpan TimeSpan { get; private set; }

        public void Clear()
        {
            DownloadInfo = default;
            CurrentDownloadTaskIndex = 0;
            DownloadTaskCount = 0;
            TimeSpan = TimeSpan.Zero;
        }

        public static DownloadSuccessEventArgs Create(DownloadInfo info, int currentTaskIndex, int taskCount,
            TimeSpan timeSpan)
        {
            var eventArgs = ReferencePool.Get<DownloadSuccessEventArgs>();
            eventArgs.DownloadInfo = info;
            eventArgs.CurrentDownloadTaskIndex = currentTaskIndex;
            eventArgs.DownloadTaskCount = taskCount;
            eventArgs.TimeSpan = timeSpan;
            return eventArgs;
        }

        public static void Clear(DownloadSuccessEventArgs eventArgs)
        {
            ReferencePool.Release(eventArgs);
        }
    }
}