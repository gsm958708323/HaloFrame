using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame
{
    public class Resource : AResource
    {
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
            if (string.IsNullOrEmpty(assetInfo.ABUrl))
            {
                Debugger.LogError($"资源没有找到对应的bundle路径 {url}", LogDomain.Res);
                return;
            }

            bundle = BundleManager.Instance.Load(bundleUrl);
            LoadAsset();
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
            // 再卸载bundle
            BundleManager.Instance.Unload(bundle);
            bundle = null;
            awaiter = null;
            finishCB = null;
        }


        public override void LoadAsset()
        {
            if (bundle is null)
            {
                Debugger.LogError($"加载资源时bundle不存在 {url}", LogDomain.Res);
                return;
            }

            // 异步转同步
            FreshAsyncAsset();

            asset = bundle.LoadAsset(url, typeof(Object));
            done = true;
            finishCB?.Invoke(this);
            finishCB = null;
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
    }
}
