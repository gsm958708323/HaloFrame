using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HaloFrame
{
    public class EditorResource : AResource
    {
        public override void Load()
        {
            if (string.IsNullOrEmpty(url))
            {
                Debugger.LogError($"加载的资源url不存在", LogDomain.Res);
                return;
            }

            LoadAsset();
        }

        public override void Unload()
        {
            if (asset && asset is not GameObject)
            {
                Resources.UnloadAsset(asset);
            }

            asset = null;
            awaiter = null;
            finishCB = null;
        }

        public override void LoadAsset()
        {
#if UNITY_EDITOR
            asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Object>(url);
#endif

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
#if UNITY_EDITOR
                    if (asset && asset is not GameObject)
                    {
                        Resources.UnloadAsset(asset);
                    }

                    asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(url);
#endif
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
