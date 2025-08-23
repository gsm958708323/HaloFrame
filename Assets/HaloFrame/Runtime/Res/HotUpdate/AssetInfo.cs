using System;
using System.Collections.Generic;
using LitJson;

namespace HaloFrame
{
    public class AssetInfo
    {
        [JsonIgnore]
        public string ResUrl;
        public List<string> Dependency;
        public string ABUrl;
        public string Version;
        public string Size;
        public string Md5;

    }
}
