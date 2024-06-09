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
    }
}
