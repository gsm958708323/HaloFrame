using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace HaloFrame
{
    public class HotUpdateManger : IManager
    {
        Downloader downloader;

        public override void Init()
        {
            downloader = GameManager.Download.CreateDownloader("HotUpdate");
            // 设置热更下载器回调
            downloader.OnDownloadSuccess += (eventArgs) =>
            {
                Debugger.Log($"获取热更资源完成！：{eventArgs.DownloadInfo.DownloadUrl}", LogDomain.HotUpdate);
            };
            downloader.OnDownloadFailure += (eventArgs) =>
            {
                Debugger.LogError($"获取热更资源失败。：{eventArgs.DownloadInfo.DownloadUrl}\n{eventArgs.ErrorMessage}", LogDomain.HotUpdate);
            };
            downloader.OnDownloadStart += (eventArgs) =>
            {
                Debugger.Log($"开始获取热更资源...：{eventArgs.DownloadInfo.DownloadUrl}", LogDomain.HotUpdate);
            };
            downloader.OnDownloadOverallProgress += (eventArgs) =>
            {
                float currentTaskIndex = eventArgs.CurrentDownloadTaskIndex;
                float taskCount = eventArgs.DownloadTaskCount;

                // 计算进度百分比
                float progress = currentTaskIndex / taskCount * 100f;
                Debugger.Log($"下载进度：{eventArgs.DownloadInfo.DownloadPath} {progress}", LogDomain.HotUpdate);
            };
            downloader.OnAllDownloadTaskCompleted += (eventArgs) =>
            {
                Debugger.Log($"所有热更资源获取完成！，用时：{eventArgs.TimeSpan}", LogDomain.HotUpdate);
                UpdateRemoteToLocal();
            };
        }

        public override void Enter()
        {
            // 尝试从本地沙盒目录获取本地版本,没有则读取resources下的版本
            var versionFile = PathTools.Combine(Application.persistentDataPath, PathTools.Platform, PathTools.GameVersionFile);
            GameVersion gameVersion;
            if (File.Exists(versionFile))
            {
                var version = JsonTools.ToObject<GameVersion>(FileTools.SafeReadAllText(versionFile));
                gameVersion = version;
            }
            else
            {
                var versionAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(PathTools.GameVersionFile));
                gameVersion = JsonTools.ToObject<GameVersion>(versionAsset.ToString());
            }
            GameConfig.LocalVersion = gameVersion.Version;
            GameConfig.HotUpdateAddress = gameVersion.AssetRemoteAddress;

            var assetMapFile = PathTools.Combine(Application.persistentDataPath, PathTools.Platform, PathTools.AssetMapFile);
            if (File.Exists(assetMapFile))
            {
                var assetMap = JsonTools.ToObject<Dictionary<string, AssetInfo>>(FileTools.SafeReadAllText(assetMapFile));
                GameConfig.LocalAssetMap = assetMap;
            }
            else
            {
                var assetMap = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(PathTools.AssetMapFile));
                GameConfig.LocalAssetMap = JsonTools.ToObject<Dictionary<string, AssetInfo>>(assetMap.ToString());
            }
        }

        public IEnumerator ReqRemote()
        {
            var remoteVersionFile = PathTools.Combine(GameConfig.HotUpdateAddress, PathTools.Platform, PathTools.GameVersionFile);
            Debugger.Log($"请求远端资源版本 {remoteVersionFile}", LogDomain.HotUpdate);
            using (UnityWebRequest request = UnityWebRequest.Get(remoteVersionFile))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debugger.LogError($"初始化资源版本失败 {remoteVersionFile} {request.error}", LogDomain.HotUpdate);
                    yield break;
                }
                GameVersion gameVersion = JsonTools.ToObject<GameVersion>(request.downloadHandler.text);
                GameConfig.RemoteVersion = gameVersion.Version;
            }

            var result = CompareVersion(GameConfig.LocalVersion, GameConfig.RemoteVersion);
            // 如果当前版本相同，则不进行更新
            if (result == 0)
            {
                Debugger.Log("版本号相同，不要更新", LogDomain.HotUpdate);
                yield break;
            }
            var remoteAssetMapFile = PathTools.Combine(GameConfig.HotUpdateAddress, PathTools.Platform, PathTools.AssetMapFile);
            Debugger.Log($"请求远端资源映射文件 {remoteAssetMapFile}", LogDomain.HotUpdate);
            using (UnityWebRequest request = UnityWebRequest.Get(remoteAssetMapFile))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debugger.LogError($"初始化资源映射失败 {remoteAssetMapFile} {request.error}", LogDomain.HotUpdate);
                    yield break;
                }

                var assetMap = JsonTools.ToObject<Dictionary<string, AssetInfo>>(request.downloadHandler.text);
                GameConfig.RemoteAssetMap = assetMap;
            }
        }

        public Tuple<HashSet<AssetInfo>, long> CheckHotUpdate()
        {
            if (GameConfig.RemoteAssetMap is null)
            {
                Debugger.LogError($"热更失败：资源映射文件不存在");
                return null;
            }
            if (GameConfig.RemoteAssetMap.Count == 0)
            {
                Debugger.LogError($"热更失败：资源映射文件内容为空");
                return null;
            }

            var hotSet = new HashSet<AssetInfo>();
            long hotSize = 0;
            var result = CompareVersion(GameConfig.LocalVersion, GameConfig.RemoteVersion);
            if (result == 0)
            {
                return Tuple.Create(hotSet, hotSize);
            }
            foreach (var item in GameConfig.RemoteAssetMap)
            {
                GameConfig.LocalAssetMap.TryGetValue(item.Key, out var localInfo);
                var remoteInfo = item.Value;
                if (localInfo is null || localInfo.Md5 != remoteInfo.Md5)
                {
                    hotSet.Add(remoteInfo);
                    hotSize += remoteInfo.Size;
                }
            }
            return Tuple.Create(hotSet, hotSize);
        }

        /// <summary>
        /// 判断版本号大小，-1表示版本过低,1表示版本过高,0表示版本号相同
        /// </summary>

        int CompareVersion(string version1, string version2)
        {
            var arr1 = version1.Split('.');
            var arr2 = version2.Split('.');

            int maxLength = Math.Max(arr1.Length, arr2.Length);
            for (int i = 0; i < maxLength; i++)
            {
                int v1 = i < arr1.Length ? int.Parse(arr1[i]) : 0;
                int v2 = i < arr2.Length ? int.Parse(arr2[i]) : 0;

                if (v1 < v2)
                {
                    return -1;
                }
                else if (v1 > v2)
                {
                    return 1;
                }
            }
            return 0;
        }

        public void StarHotUpdate()
        {
            var info = CheckHotUpdate();
            if (info is null)
            {
                return;
            }

            var assetInfoSet = info.Item1;
            var size = info.Item2;
            foreach (var item in assetInfoSet)
            {
                var remoteUrl = PathTools.Combine(PathTools.RemoteABUrlPrefix, item.ABUrl);
                var downPath = PathTools.Combine(PathTools.DownloadABPathPrefix, item.ABUrl);
                downloader.AddDownloadTask(remoteUrl, downPath);
            }

            downloader.LaunchDownload();
        }

        void UpdateRemoteToLocal()
        {

        }
    }
}
