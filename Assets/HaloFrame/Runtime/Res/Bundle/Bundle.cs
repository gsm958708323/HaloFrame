using System;
using System.IO;
using UnityEngine;

namespace HaloFrame
{
    public class Bundle : ABundle
    {
        internal override void LoadAB()
        {
            if (assetBundle)
            {
                Debugger.LogError($"重复加载Bundle {url}", LogDomain.Res);
                return;
            }

            var file = BundleManager.Instance.GetFileUrl(url);
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!File.Exists(file))
            {
                Debugger.LogError($"资源路径不存在 {url}", LogDomain.Res);
                return;
            }
#endif
            assetBundle = AssetBundle.LoadFromFile(file, 0, BundleManager.Instance.offset);
            isScene = assetBundle.isStreamedSceneAssetBundle;
            done = true;
        }

        internal override void UnloadAB()
        {
            if (assetBundle)
            {
                assetBundle.Unload(true);
            }
            assetBundle = null;
            done = false;
            reference = 0;
            isScene = false;
        }

        internal override UnityEngine.Object LoadAsset(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debugger.LogError($"资源名字不存在 {url}", LogDomain.Res);
                return null;
            }
            if (assetBundle is null)
            {
                Debugger.LogError($"ab资源不存在 {url} {assetName}", LogDomain.Res);
                return null;
            }

            return assetBundle.LoadAsset(assetName, type);
        }

        internal override AssetBundleRequest LoadAssetAsync(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debugger.LogError($"资源名字不存在 {url}", LogDomain.Res);
                return null;
            }
            if (assetBundle is null)
            {
                Debugger.LogError($"ab资源不存在 {url} {assetName}", LogDomain.Res);
                return null;
            }
            return assetBundle.LoadAssetAsync(assetName, type);
        }
    }
}
