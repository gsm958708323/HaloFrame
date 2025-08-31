namespace HaloFrame
{
    public class DownloadStartEventArgs : IReference
    {
        public DownloadInfo DownloadInfo { get; private set; }
        public int CurrentDownloadTaskIndex { get; private set; }
        public int DownloadTaskCount { get; private set; }

        public void Clear()
        {
            DownloadInfo = default;
            CurrentDownloadTaskIndex = 0;
            DownloadTaskCount = 0;
        }

        public static DownloadStartEventArgs Create(DownloadInfo info, int currentTaskIndex, int taskCount)
        {
            var eventArgs = ReferencePool.Get<DownloadStartEventArgs>();
            eventArgs.DownloadInfo = info;
            eventArgs.CurrentDownloadTaskIndex = currentTaskIndex;
            eventArgs.DownloadTaskCount = taskCount;
            return eventArgs;
        }

        public static void Clear(DownloadStartEventArgs eventArgs)
        {
            ReferencePool.Release(eventArgs);
        }
    }
}