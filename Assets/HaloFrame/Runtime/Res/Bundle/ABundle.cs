using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public abstract class ABundle
    {
        /// <summary>
        /// ab资源
        /// </summary>
        internal AssetBundle assetBundle;
        /// <summary>
        /// 是否为场景资源
        /// </summary>
        internal bool isScene;

        /// <summary>
        /// 引用计数
        /// </summary>
        internal int reference;
        /// <summary>
        /// 资源地址
        /// </summary>
        internal string url;
        /// <summary>
        /// 依赖列表（我使用别人）
        /// </summary>
        internal List<ABundle> dependencies;
        /// <summary>
        /// 是否加载完成
        /// </summary>
        internal bool done;

        /// <summary>
        /// 添加引用计数（别人使用我）
        /// </summary>
        internal void AddReference()
        {
            ++reference;
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        internal void ReduceReference()
        {
            --reference;
            if(reference < 0)
            {
                Debugger.LogError($"{url} 引用计数小于0", LogDomain.Res);
            }
        }

        internal abstract void LoadAB();
        internal abstract void UnloadAB();
        internal abstract AssetBundleRequest LoadAssetAsync(string assetName, Type type);
        internal abstract UnityEngine.Object LoadAsset(string assetName, Type type);
    }
}
