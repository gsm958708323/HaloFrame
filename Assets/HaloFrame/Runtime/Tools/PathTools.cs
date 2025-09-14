using System;
using System.IO;
using UnityEngine;

namespace HaloFrame
{
    public class PathTools
    {
        public const string AssetBundlesDir = "AssetBundles";
        public static string HotUpdateDir = "HotUpdate";
        public static string BuildSettingPath = "Assets/BuildSetting.asset";
        public static string GameVersionFile = "GameVersion.json";
        /// <summary>
        /// 资源映射文件
        /// </summary>
        public static string AssetMapFile = "AssetMap.json";
        /// <summary>
        /// ab包主文件，可以获取ab包的依赖关系
        /// </summary>
        public static string MainManifestFile = "main.manifest";

        private static string localGameVersionPath;
        public static string LocalGameVersionPath
        {
            get
            {
                if (string.IsNullOrEmpty(localGameVersionPath))
                {
                    localGameVersionPath = PathTools.Combine(Application.persistentDataPath, PathTools.Platform, PathTools.GameVersionFile);
                }
                return localGameVersionPath;
            }
        }
        private static string localAssetMapPath;
        public static string LocalAssetMapPath
        {
            get
            {
                if (string.IsNullOrEmpty(localAssetMapPath))
                {
                    localAssetMapPath = PathTools.Combine(Application.persistentDataPath, PathTools.Platform, PathTools.AssetMapFile);
                }
                return localAssetMapPath;
            }
        }

        private static string remoteABUrlPrefix;
        /// <summary>
        /// 下载abUrl的前缀
        /// </summary>
        /// <value></value>
        public static string RemoteABUrlPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(remoteABUrlPrefix))
                {
                    remoteABUrlPrefix = PathTools.Combine(GameConfig.HotUpdateAddress, PathTools.Platform, PathTools.HotUpdateVersionDir);
                }
                return remoteABUrlPrefix;
            }
        }

        private static string downloadABPathPrefix;
        /// <summary>
        /// 本地存储ab的路径（不存储多版本资源）
        /// </summary>
        /// <value></value>
        public static string DownloadABPathPrefix
        {
            get
            {
                if (string.IsNullOrEmpty(downloadABPathPrefix))
                {
                    downloadABPathPrefix = PathTools.Combine(Application.persistentDataPath, PathTools.Platform, PathTools.HotUpdateDir);
                }
                return downloadABPathPrefix;
            }
        }

        private static string hotUpdateVersionDir;
        public static string HotUpdateVersionDir
        {
            get
            {
                if (string.IsNullOrEmpty(hotUpdateVersionDir))
                {
                    hotUpdateVersionDir = $"{HotUpdateDir}_{GameConfig.RemoteVersion}";
                }
                return hotUpdateVersionDir;
            }
        }

        private static string platform;
        public static string Platform
        {
            get
            {
                if (string.IsNullOrEmpty(platform))
                {
#if UNITY_STANDALONE_WIN
                    platform = "Windows";
#elif UNITY_STANDALONE_OSX
                platform = "macOS";
#elif UNITY_STANDALONE_LINUX
                platform = "Linux";
#elif UNITY_IPHONE || UNITY_IOS
                platform = "iOS";
#elif UNITY_ANDROID
                platform = "Android";
#elif UNITY_WEBGL
                platform = "WebGL";
#else
                platform = "Unknown";
#endif
                }
                return platform;
            }
        }


        /// <summary>
        /// 拼接目录，有些unity的接口不支持"\"目录识别
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths).Replace("\\", "/");
        }
    }
}
