using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HaloFrame
{
    public class BundleManager : Singleton<BundleManager>
    {
        /// <summary>
        /// ab资源的根目录
        /// </summary>
        private string bundleRootDir;
        /// <summary>
        /// 偏移量，默认为0（可以跳过自定义加密数据）
        /// </summary>
        public ulong offset;
        private AssetBundleManifest manifestAB;

        /// <summary>
        /// 当前存在的bundle
        /// </summary>
        private Dictionary<string, ABundle> bundleDict;
        /// <summary>
        /// 等待被移除的bundle
        /// </summary>
        private LinkedList<ABundle> waitUnloadList;
        /// <summary>
        /// 需要异步加载的bundle
        /// </summary>
        private List<ABundleAsync> asyncList;

        private BundleManager()
        {
            bundleDict = new();
            waitUnloadList = new();
            asyncList = new();
        }

        public void Init(string bundleRootDir, ulong offset)
        {
            this.offset = offset;
            this.bundleRootDir = bundleRootDir;
            string manifestFile = PathTools.Combine(bundleRootDir, Path.GetFileNameWithoutExtension(PathTools.MainManifestFile));
            AssetBundle assetBundle = AssetBundle.LoadFromFile(manifestFile);
            var objests = assetBundle.LoadAllAssets();
            if (objests.Length == 0)
            {
                Debugger.LogError($"{manifestFile} 加载错误");
                return;
            }
            manifestAB = objests[0] as AssetBundleManifest;
        }

        internal ABundle Load(string url)
        {
            return LoadInternal(url, false);
        }
        internal ABundle LoadAsync(string url)
        {
            return LoadInternal(url, true);
        }

        ABundle LoadInternal(string url, bool async)
        {
            ABundle bundle;
            //此bundle是否创建过
            if (bundleDict.TryGetValue(url, out bundle))
            {
                //此bunlde重新被使用，从移除列表中删除
                if (bundle.reference == 0)
                {
                    waitUnloadList.Remove(bundle);
                }
                bundle.AddReference();
                return bundle;
            }

            //创建新的bundle
            if (async)
            {
                bundle = new BundleAsync();
                asyncList.Add(bundle as ABundleAsync);
            }
            else
            {
                bundle = new Bundle();
            }
            bundle.url = url;
            bundleDict.Add(url, bundle);
            //加载依赖
            var dependencies = manifestAB.GetDirectDependencies(url);
            bundle.dependencies = new(dependencies.Length);
            foreach (var dependencyUrl in dependencies)
            {
                ABundle dependencyBundle = LoadInternal(dependencyUrl, async);
                bundle.dependencies.Add(dependencyBundle);
            }
            bundle.AddReference();
            bundle.LoadAB();
            return bundle;
        }

        public void Unload(ABundle bundle)
        {
            if (bundle == null)
            {
                Debugger.LogError("bundle is null", LogDomain.Res);
                return;
            }

            bundle.ReduceReference();
            // 引用计数为0，直接释放
            if (bundle.reference == 0)
            {
                waitUnloadList.AddLast(bundle);
            }
        }

        /// <summary>
        /// 检查是否加载完成
        /// </summary>
        public void CheckLoad()
        {
            for (int i = 0; i < asyncList.Count; i++)
            {
                var bunlde = asyncList[i];
                var laodFinsh = bunlde.Tick();
                if (laodFinsh)
                {
                    asyncList.RemoveAt(i);
                    //移除之后防止跳过元素
                    i--;
                }
            }
        }

        /// <summary>
        /// 卸载不需要的bunlde
        /// </summary>
        public void CheckUnload()
        {
            if (waitUnloadList.Count == 0)
                return;

            while (waitUnloadList.Count > 0)
            {
                ABundle bundle = waitUnloadList.First.Value;
                waitUnloadList.RemoveFirst();
                if (bundle == null)
                    continue;

                bundleDict.Remove(bundle.url);
                var bundleAsync = bundle as ABundleAsync;
                if (asyncList.Contains(bundleAsync))
                {
                    asyncList.Remove(bundleAsync);
                }

                bundle.UnloadAB();
                if (bundle.dependencies != null)
                {
                    foreach (var dependencie in bundle.dependencies)
                    {
                        Unload(dependencie);
                    }
                }
            }
        }

        /// <summary>
        /// 获取bundle的绝对路径
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        internal string GetFileUrl(string url)
        {
            return PathTools.Combine(bundleRootDir, url);
        }
    }
}
