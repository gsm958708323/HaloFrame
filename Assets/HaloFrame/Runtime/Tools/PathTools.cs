using System;
using System.IO;
using UnityEngine;

namespace HaloFrame
{
    public class PathTools
    {
        public const string AssetBundlesDir = "AssetBundles";
        public static string HotUpdateDir = "HotUpdate";
        public static string BuildSettingPath = "Assets/Resources/BuildSetting.asset";
        public static string AssetMapFileName = "AssetMap";

        private static string assetMapPath;
        public static string AssetMapPath
        {
            get
            {
                if (string.IsNullOrEmpty(assetMapPath))
                {
                    assetMapPath = Path.Combine(Application.dataPath, "Resources", $"{AssetMapFileName}.json");
                }
                return assetMapPath;
            }
        }

        private static string streamingAssetsPath;
        public static string StreamingAssetsPath
        {
            get
            {
                if (string.IsNullOrEmpty(streamingAssetsPath))
                {
                    streamingAssetsPath = Path.Combine(Application.streamingAssetsPath, AssetBundlesDir, Platform).Replace("\\", "/");
                }
                return streamingAssetsPath;
            }
        }

        private static string remoteAddress;
        public static string RemoteAddress
        {
            get
            {
                if (string.IsNullOrEmpty(remoteAddress))
                {
                    remoteAddress = Path.Combine(GameConfig.LocalVersion.AssetRemoteAddress, AssetBundlesDir, Platform).Replace("\\", "/");
                }
                return remoteAddress;
            }
        }

        private static string hotUpdatePath;
        public static string HotUpdatePath
        {
            get
            {
                if (string.IsNullOrEmpty(hotUpdatePath))
                {
                    hotUpdatePath = Path.Combine(Application.persistentDataPath, HotUpdateDir, AssetBundlesDir, Platform).Replace("\\", "/");
                }
                return hotUpdatePath;
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

    }
}
