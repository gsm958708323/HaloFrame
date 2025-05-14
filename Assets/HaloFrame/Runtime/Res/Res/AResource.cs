using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame
{
    public abstract class AResource : CustomYieldInstruction, IResource
    {
        public string url { get; set; }
        internal virtual Object asset { get; set; }
        internal ABundle bundle { get; set; }
        internal List<AResource> dependencies { get; set; }
        internal int reference { get; set; }
        internal bool done { get; set; }
        internal ResourceAwaiter awaiter { get; set; }
        internal Action<AResource> finishCB { get; set; }
        public override bool keepWaiting => !done;

        /// <summary>
        /// 增加引用
        /// </summary>
        internal void AddReference()
        {
            ++reference;
        }

        /// <summary>
        /// 减少引用
        /// </summary>
        internal void ReduceReference()
        {
            --reference;
            if (reference < 0)
            {
                Debugger.LogError($"{url} 引用计数小于0", LogDomain.Res);
            }
        }

        public abstract void Load();
        public abstract void Unload();
        public abstract void LoadAsset();

        /// <summary>
        /// 刷新异步资源（当同步资源的依赖包含异步时，需要立即刷新返回）
        /// </summary>
        public void FreshAsyncAsset()
        {
            if (done)
                return;
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    dependency.FreshAsyncAsset();
                }
            }
            if (this is AResourceAsync)
            {
                LoadAsset();
            }
        }

        public Object GetAsset()
        {
            return asset;
        }

        public abstract T GetAsset<T>() where T : Object;

        public GameObject Instantiate()
        {
            if (!asset)
                return null;
            if (asset is not GameObject)
                return null;
            return Object.Instantiate(asset) as GameObject;
        }

        public GameObject Instantiate(bool autoUnload)
        {
            var go = Instantiate();
            if (autoUnload && go)
            {
                var comp = go.AddComponent<AutoUnload>();
                comp.resource = this;
            }
            return go;
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation)
        {
            if (!asset)
                return null;
            if (asset is not GameObject)
                return null;
            return Object.Instantiate(asset, position, rotation) as GameObject;
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation, bool autoUnload)
        {
            GameObject go = Instantiate(position, rotation);
            if (autoUnload && go)
            {
                AutoUnload temp = go.AddComponent<AutoUnload>();
                temp.resource = this;
            }

            return go;
        }

        public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace)
        {
            if (!asset)
                return null;
            if (asset is not GameObject)
                return null;
            return Object.Instantiate(asset, parent, instantiateInWorldSpace) as GameObject;
        }

        public GameObject Instantiate(Transform parent, bool instantiateInWorldSpace, bool autoUnload)
        {
            GameObject go = Instantiate(parent, instantiateInWorldSpace);
            if (autoUnload && go)
            {
                AutoUnload temp = go.AddComponent<AutoUnload>();
                temp.resource = this;
            }

            return go;
        }
    }
}

