using System;
using System.IO;
using UnityEngine;

namespace HaloFrame
{
    public class BundleAsync : ABundleAsync
    {
        /// <summary>
        /// 异步加载资源的Request对象
        /// </summary>
        AssetBundleCreateRequest bundleCreateRequest;

        /// <summary>assetBundleCreateRequest
        /// 资源加载，依赖资源全部加载完成之后才为true
        /// </summary>
        /// <returns></returns>
        public override bool Tick()
        {
            if (done)
                return true;

            // 检查依赖资源加载进度
            if (dependencies != null)
            {
                foreach (var item in dependencies)
                {
                    if (!item.done)
                        return false;
                }
            }

            // 检查自己加载进度
            if (!bundleCreateRequest.isDone)
                return false;

            done = true;
            assetBundle = bundleCreateRequest.assetBundle;
            isScene = assetBundle.isStreamedSceneAssetBundle;

            // 加载完成之后已经没有引用，则尝试卸载
            if (reference == 0)
            {
                UnloadAB();
            }
            return true;
        }

        internal override void LoadAB()
        {
            if (bundleCreateRequest != null)
            {
                Debugger.LogError($"重复加载Bundle {url}", LogDomain.Res);
                return;
            }
            var file = BundleManager.Instance.GetFileUrl(url);
#if UNITY_EDITOR || UNITY_STANDALONE
            if (!File.Exists(file))
            {
                Debugger.LogError($"资源路径不存在 {url}", LogDomain.Res);
            }
#endif

            bundleCreateRequest = AssetBundle.LoadFromFileAsync(file, 0, BundleManager.Instance.offset);
        }

        internal override void UnloadAB()
        {
            // todo 会不会内容泄漏
            // https://docs.unity3d.com/cn/2019.4/Manual/AssetBundles-Native.html
            if (bundleCreateRequest != null)
            {
                assetBundle = bundleCreateRequest.assetBundle;
            }
            if (assetBundle)
            {
                assetBundle.Unload(true);
            }

            bundleCreateRequest = null;
            done = false;
            reference = 0;
            assetBundle = null;
            isScene = false;
        }

        internal override UnityEngine.Object LoadAsset(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debugger.LogError($"资源名字不存在 {url} {assetName}", LogDomain.Res);
                return null;
            }
            if (bundleCreateRequest == null)
            {
                Debugger.LogError($"bundleCreateRequest为nil {url} {assetName}", LogDomain.Res);
                return null;
            }

            if (assetBundle == null)
            {
                assetBundle = bundleCreateRequest.assetBundle;
            }
            return assetBundle.LoadAsset(assetName, type);
        }

        internal override AssetBundleRequest LoadAssetAsync(string assetName, Type type)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Debugger.LogError($"资源名字不存在 {url} {assetName}", LogDomain.Res);
                return null;
            }
            if (bundleCreateRequest == null)
            {
                Debugger.LogError($"bundleCreateRequest为nil {url} {assetName}", LogDomain.Res);
                return null;
            }

            return assetBundle.LoadAssetAsync(assetName, type);
        }

    }
}
