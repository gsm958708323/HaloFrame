using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame
{
    // 临时
    public class ResourceManager : IManager
    {
        private bool isEditor;
        /// <summary>
        /// 资源对应bundle路径
        /// </summary>
        Dictionary<string, string> assetBundleDict;
        Dictionary<string, List<string>> dependencyDict;
        Dictionary<string, AResource> resourceDict;
        Dictionary<ushort, string> assetUrlDict;
        List<AResourceAsync> asyncList;
        LinkedList<AResource> waitUnloadList;

        private const string MANIFEST_BUNDLE = "manifest.ab";
        private const string RESOURCE_ASSET_NAME = "Assets/Temp/Resource.bytes";
        private const string BUNDLE_ASSET_NAME = "Assets/Temp/Bundle.bytes";
        private const string DEPENDENCY_ASSET_NAME = "Assets/Temp/Dependency.bytes";

        public void Init(string platform, Func<string, string> getFileCB, bool isEditor = false, ulong offset = 0)
        {
            this.isEditor = isEditor;
            if (isEditor)
                return;
            BundleManager.Instance.Init(platform, getFileCB, offset);

            string manifestFile = getFileCB.Invoke(MANIFEST_BUNDLE);
            AssetBundle manifestAB = AssetBundle.LoadFromFile(manifestFile);
            var resourceAsset = manifestAB.LoadAsset(RESOURCE_ASSET_NAME) as TextAsset;
            var bundleAsset = manifestAB.LoadAsset(BUNDLE_ASSET_NAME) as TextAsset;
            var dependencyAsset = manifestAB.LoadAsset(DEPENDENCY_ASSET_NAME) as TextAsset;
            byte[] resourceBytes = resourceAsset.bytes;
            byte[] bundleBytes = bundleAsset.bytes;
            byte[] dependencyBytes = dependencyAsset.bytes;
            manifestAB.Unload(true);
            manifestAB = null;

            ReadAssetUrl(resourceBytes);
            ReadBundle(bundleBytes);
            ReadDependency(dependencyBytes);
        }

        public override void Tick(float deltaTime)
        {
            BundleManager.Instance.CheckLoad();
            CheckLoad();
            CheckUnload();
            BundleManager.Instance.CheckUnload();
        }

        void CheckLoad()
        {
            for (int i = 0; i < asyncList.Count; i++)
            {
                var resourceAsync = asyncList[i];
                if (resourceAsync.Tick())
                {
                    asyncList.RemoveAt(i);
                    i--;

                    resourceAsync.awaiter?.SetResult(resourceAsync);
                }
            }
        }
        void CheckUnload()
        {
            if (waitUnloadList.Count == 0)
                return;

            while (waitUnloadList.Count > 0)
            {
                var resource = waitUnloadList.First.Value;
                waitUnloadList.RemoveFirst();
                if (resource is null)
                    continue;

                // 先卸载自己
                resourceDict.Remove(resource.url);
                resource.Unload();

                // 再卸载依赖
                if (resource.dependencies is not null)
                {
                    foreach (var item in resource.dependencies)
                    {
                        Unload(item);
                    }
                }
            }
        }

        public IResource Load(string url, bool async)
        {
            return LoadInternal(url, async);
        }

        private AResource LoadInternal(string url, bool async)
        {
            AResource resource;
            if (resourceDict.TryGetValue(url, out resource))
            {
                if (resource.reference == 0)
                {
                    waitUnloadList.Remove(resource);
                }
                resource.AddReference();
                return resource;
            }

            if (isEditor)
            {
                resource = new EditorResource();
            }
            else
            {
                resource = new Resource();
            }
            resource.url = url;
            resourceDict.Add(url, resource);

            List<string> dependencies;
            if (dependencyDict.TryGetValue(url, out dependencies))
            {
                resource.dependencies = new(dependencies.Count);
                foreach (var depUrl in dependencies)
                {
                    var depResource = LoadInternal(depUrl, async);
                    resource.dependencies.Add(depResource);
                }
            }
            resource.AddReference();
            resource.Load();

            return resource;
        }

        public void Unload(IResource resource)
        {
            if (resource is null)
            {
                Debugger.LogError($"卸载的资源不存在", LogDomain.Res);
                return;
            }

            var aResource = resource as AResource;
            aResource.ReduceReference();
            if (aResource.reference == 0)
            {
                waitUnloadList.AddLast(aResource);
            }
        }

        public void Unload(string assetUrl)
        {
            if (string.IsNullOrEmpty(assetUrl))
            {
                Debugger.LogError($"卸载的资源url不存在", LogDomain.Res);
                return;
            }

            AResource resource;
            if (!resourceDict.TryGetValue(assetUrl, out resource))
            {
                Debugger.LogError($"卸载的资源url对应资源不存在 {assetUrl}", LogDomain.Res);
                return;
            }

            Unload(resource);
        }

        private void ReadDependency(byte[] dependencyBytes)
        {
            dependencyDict = new();
            var stream = new MemoryStream(dependencyBytes);
            var reader = new BinaryReader(stream);
            ushort dependencyCount = reader.ReadUInt16();
            for (int i = 0; i < dependencyCount; i++)
            {
                ushort resourceCount = reader.ReadUInt16();
                ushort assetId = reader.ReadUInt16();
                string assetUrl = assetUrlDict[assetId];
                var dependencieList = new List<string>(resourceCount);
                for (int j = 0; j < resourceCount; j++)
                {
                    ushort dependencyAssetId = reader.ReadUInt16();
                    string dependencyUrl = assetUrlDict[dependencyAssetId];
                    dependencieList.Add(dependencyUrl);
                }
                dependencyDict.Add(assetUrl, dependencieList);
            }
        }

        /// <summary>
        /// 读取所有资源路径对应ab路径
        /// </summary>
        /// <param name="bundleBytes"></param>
        private void ReadBundle(byte[] bundleBytes)
        {
            assetBundleDict = new();
            var stream = new MemoryStream(bundleBytes);
            var reader = new BinaryReader(stream);
            // bundle数量
            ushort bundleCount = reader.ReadUInt16();
            for (ushort i = 0; i < bundleCount; i++)
            {
                string bundleUrl = reader.ReadString();
                // 一个bundle依赖的资源数量
                ushort resourceCount = reader.ReadUInt16();
                for (ushort j = 0; j < resourceCount; j++)
                {
                    var assetId = reader.ReadUInt16();
                    var assetUrl = assetUrlDict[assetId];
                    assetBundleDict.Add(assetUrl, bundleUrl);
                }
            }
        }

        public string GetAssetBundleUrl(string assetUrl)
        {
            assetBundleDict.TryGetValue(assetUrl, out string bundleUrl);
            return bundleUrl;
        }

        /// <summary>
        /// 读取所有资源的路径
        /// </summary>
        /// <param name="resourceBytes"></param>
        void ReadAssetUrl(byte[] resourceBytes)
        {
            assetUrlDict = new();
            var stream = new MemoryStream(resourceBytes);
            var reader = new BinaryReader(stream);
            ushort count = reader.ReadUInt16();
            for (ushort i = 0; i < count; i++)
            {
                var assetUrl = reader.ReadString();
                assetUrlDict.Add(i, assetUrl);
            }
        }

        public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public T LoadAsset<T>(int resId) where T : UnityEngine.Object
        {
            var config = ResConfigSO.Get(resId);
            return LoadAsset<T>(config.ResPath);
        }

        public void UnloadAsset(GameObject asset)
        {
            Object.Destroy(asset);
        }
    }
}
