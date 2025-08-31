using System;
using System.Collections.Generic;

namespace HaloFrame
{
    public class DownloadManager : IManager
    {
        public bool DeleteFileOnAbort
        {
            get { return DownloadDataProxy.DeleteFileOnAbort; }
            set { DownloadDataProxy.DeleteFileOnAbort = value; }
        }

        /// <summary>
        /// 下载器；
        /// </summary>
        Dictionary<string, IDownloader> downloadDict = new();

        /// <summary>
        /// 下载请求器，专门用于获取文件大小；
        /// </summary>
        IDownloadRequester downloadRequester;

        /// <summary>
        /// 下载资源地址帮助体；用于解析URL中的文件列表；
        /// 支持localhost地址与http地址；
        /// </summary>
        IDownloadUrlHelper downloadUrlHelper;

        /// <summary>
        /// 设置下载资源地址帮助体。
        /// </summary>
        /// <param name="helper">要设置的下载资源地址帮助体。</param>
        public void SetUrlHelper(IDownloadUrlHelper helper)
        {
            this.downloadUrlHelper = helper;
        }

        /// <summary>
        /// 设置下载请求器帮助体。
        /// </summary>
        /// <param name="helper">要设置的下载请求器帮助体。</param>
        public void SetRequesterHelper(IDownloadRequester helper)
        {
            this.downloadRequester = helper;
        }

        /// <summary>
        /// 异步获取URI文件大小。
        /// </summary>
        /// <param name="downloadUri">下载资源的URI。</param>
        /// <param name="callback">获取完成后的回调函数。</param>
        public void GetUriFileSizeAsync(string downloadUri, Action<long> callback)
        {
            Utility.Text.IsStringValid(downloadUri, "URI is invalid !");
            if (callback == null)
                throw new ArgumentNullException("Callback is invalid !");
            downloadRequester.GetUriFileSizeAsync(downloadUri, callback);
        }

        /// <summary>
        /// 异步获取URL中文件的总大小。
        /// </summary>
        /// <param name="downloadUrl">下载资源的URL。</param>
        /// <param name="callback">获取完成后的回调函数。</param>
        public void GetUrlFilesSizeAsync(string downloadUrl, Action<long> callback)
        {
            Utility.Text.IsStringValid(downloadUrl, "URI is invalid !");
            var relUris = downloadUrlHelper.ParseUrlToRelativeUris(downloadUrl);
            downloadRequester.GetUriFilesSizeAsync(relUris, callback);
        }

        public void OnInit(object createParam)
        {
            downloadUrlHelper = new DefaultDownloadUrlHelper();
            downloadRequester = new DefaultDownloadRequester();
        }

        /// <summary>
        /// 创建下载器
        /// </summary>
        /// <param name="downloaderName"></param>
        /// <param name="_downloader"></param>
        public Downloader CreateDownloader(string downloaderName)
        {
            if (downloadDict.ContainsKey(downloaderName))
            {
                return downloadDict[downloaderName] as Downloader;
            }

            var downloader = new Downloader();
            downloadDict.Add(downloaderName, downloader);
            return downloader;
        }

        public void CancelDownload()
        {
            foreach (var downloader in downloadDict.Values)
            {
                downloader.CancelDownload();
            }
        }
    }
}