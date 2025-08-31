using System;
using System.Collections.Generic;

namespace HaloFrame
{
    public class GameConfig
    {
        public static string LocalVersion;
        public static string RemoteVersion;
        public static string HotUpdateAddress;

        public static Dictionary<string, AssetInfo> LocalAssetMap;
        public static Dictionary<string, AssetInfo> RemoteAssetMap;
    }
}
