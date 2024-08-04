using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HaloFrame
{
    // 临时
    public class ResourceManager : IManager
    {
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
