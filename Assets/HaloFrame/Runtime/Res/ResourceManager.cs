using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class ResourceManager : IManager
    {
        private bool isEditor;
        Dictionary<string, AResource> resourceDict;
        List<AResourceAsync> asyncList;
        LinkedList<AResource> waitUnloadList;

        public void Init(string bundleRootDir, bool isEditor = false, ulong offset = 0)
        {
            resourceDict = new();
            asyncList = new();
            waitUnloadList = new();
            this.isEditor = isEditor;
            if (isEditor)
                return;

            BundleManager.Instance.Init(bundleRootDir, offset);
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

        internal void LoadWithCallback(string url, bool async, Action<IResource> callback)
        {
            var resource = LoadInternal(url, async);
            if (resource.done)
            {
                callback?.Invoke(resource);
            }
            else
            {
                resource.finishCB += callback;
            }
        }

        internal ResourceAwaiter LoadWithAwaiter(int resId)
        {
            var config = ResConfigSO.Get(resId);
            return LoadWithAwaiter(config.ResPath);
        }

        internal ResourceAwaiter LoadWithAwaiter(string url)
        {
            AResource resource = LoadInternal(url, true);
            if (resource.done)
            {
                if (resource.awaiter is null)
                {
                    resource.awaiter = new ResourceAwaiter();
                    resource.awaiter.SetResult(resource);
                }

                return resource.awaiter;
            }
            if (resource.awaiter is null)
            {
                resource.awaiter = new ResourceAwaiter();
            }
            return resource.awaiter;
        }

        public IResource Load(int resId, bool async)
        {
            var config = ResConfigSO.Get(resId);
            return Load(config.ResPath, async);
        }

        public IResource Load(string url, bool async = true)
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
            else if (async)
            {
                var resourceAsync = new ResourceAsync();
                asyncList.Add(resourceAsync);
                resource = resourceAsync;
            }
            else
            {
                resource = new Resource();
            }
            resource.url = url;
            resourceDict.Add(url, resource);

            if (!isEditor)
            {
                var assetInfo = GetAssetInfo(url);
                if (assetInfo is not null && assetInfo.Dependency is not null)
                {
                    var dependencies = assetInfo.Dependency;
                    resource.dependencies = new(dependencies.Count);
                    foreach (var depUrl in dependencies)
                    {
                        var depResource = LoadInternal(depUrl, async);
                        resource.dependencies.Add(depResource);
                    }
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

        public AssetInfo GetAssetInfo(string assetUrl)
        {
            GameConfig.RemoteAssetMap.TryGetValue(assetUrl, out AssetInfo info);
            return info;
        }
    }
}
