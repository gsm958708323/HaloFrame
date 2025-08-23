using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame
{
    public class ResourceAsync : AResourceAsync
    {
        private AssetBundleRequest assetBundleRequest;
        internal override Object asset
        {
            get
            {
                if (done)
                    return base.asset;

                FreshAsyncAsset();
                return base.asset;
            }
            set
            {
                base.asset = value;
            }
        }

        /// <summary>
        /// 加载AB
        /// </summary>
        public override void Load()
        {
            if (string.IsNullOrEmpty(url))
            {
                Debugger.LogError($"加载的资源url不存在 {url}", LogDomain.Res);
                return;
            }

            if (bundle is not null)
            {
                Debugger.LogError($"资源已经加载过了 {url}", LogDomain.Res);
                return;
            }

            var assetInfo = GameManager.Resource.GetAssetInfo(url);
            if (assetInfo is null)
            {
                Debugger.LogError($"资源没有找到对应的AssetInfo {url}", LogDomain.Res);
                return;
            }
            var bundleUrl = assetInfo.ABUrl;
            if (string.IsNullOrEmpty(bundleUrl))
            {
                Debugger.LogError($"资源没有找到对应的bundle路径 {url}", LogDomain.Res);
                return;
            }
            bundle = BundleManager.Instance.LoadAsync(bundleUrl);
        }

        public override void Unload()
        {
            if (bundle is null)
            {
                Debugger.LogError($"卸载的资源不存在 {url}", LogDomain.Res);
                return;
            }

            // 先卸载资源
            if (asset && asset is not GameObject)
            {
                Resources.UnloadAsset(asset);
                asset = null;
            }

            assetBundleRequest = null;
            // 再卸载bundle
            BundleManager.Instance.Unload(bundle);
            bundle = null;
            awaiter = null;
            finishCB = null;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        public override void LoadAsset()
        {
            if (bundle is null)
            {
                Debugger.LogError($"加载资源时bundle不存在 {url}", LogDomain.Res);
                return;
            }

            if (!bundle.isScene)
            {
                if (assetBundleRequest is not null)
                {
                    // 在isDone为true之前访问，将导致加载过程停止
                    asset = assetBundleRequest.asset;
                }
                else
                {
                    asset = bundle.LoadAsset(url, typeof(Object));
                }
            }

            done = true;
            finishCB?.Invoke(this);
            finishCB = null;
        }

        public override void LoadAssetAsync()
        {
            if (bundle is null)
            {
                Debugger.LogError($"加载资源时bundle不存在 {url}", LogDomain.Res);
                return;
            }

            assetBundleRequest = bundle.LoadAssetAsync(url, typeof(Object));
        }

        public override T GetAsset<T>()
        {
            Type type = typeof(T);
            if (type == typeof(Sprite))
            {
                if (asset is Sprite)
                {
                    return asset as T;
                }
                else
                {
                    if (asset && asset is not GameObject)
                    {
                        Resources.UnloadAsset(asset);
                    }

                    asset = bundle.LoadAsset(url, type);
                    return asset as T;
                }
            }
            else
            {
                return asset as T;
            }
        }

        public override bool Tick()
        {
            // 检查自己加载进度
            if (done)
                return true;

            // 检查依赖加载进度
            if (dependencies != null)
            {
                foreach (var item in dependencies)
                {
                    if (!item.done)
                        return false;
                }
            }

            // 加载bundle加载进度
            if (!bundle.done)
                return false;

            if (assetBundleRequest is null)
            {
                LoadAssetAsync();
            }
            // 检查资源加载进度
            if (assetBundleRequest is not null && !assetBundleRequest.isDone)
                return false;

            LoadAsset();
            return true;
        }
    }
}
