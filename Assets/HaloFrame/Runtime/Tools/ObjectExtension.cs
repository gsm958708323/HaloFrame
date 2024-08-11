using System;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public static class ObjectExtension
    {
        private static readonly List<Transform> s_CachedTransforms = new List<Transform>();

        public static T GetOrAddComponent<T>(this Component obj) where T : Component
        {
            return obj.gameObject.GetOrAddComponent<T>();
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
                t = gameObject.AddComponent<T>();
            return t;
        }

        public static Component GetOrAddComponent(this Component obj, Type type)
        {
            return obj.gameObject.GetOrAddComponent(type);
        }

        public static Component GetOrAddComponent(this GameObject gameObject, Type type)
        {
            if (gameObject == null) return null;

            Component component = gameObject.GetComponent(type);
            if (component == null)
                component = gameObject.AddComponent(type);
            return component;
        }

        public static void SetParentEx(this Transform transform, Transform parent)
        {
            transform.SetParent(parent, false);
            // transform.localPosition = Vector3.zero;
            // transform.localRotation = Quaternion.identity;
            // transform.localScale = Vector3.one;
        }

        public static void SetActiveEx(this GameObject gameObject, bool active)
        {
            if (gameObject.activeSelf != active)
                gameObject.SetActive(active);
        }

        public static void SetActiveCanvas(this GameObject gameObject, bool active)
        {
            var canvas = gameObject.GetComponent<CanvasGroup>();
            if (canvas != null)
            {
                canvas.alpha = active ? 1 : 0;
                canvas.interactable = active;
                canvas.blocksRaycasts = active;
            }
            else
            {
                gameObject.SetActive(active);
            }
        }

        public static void SetActiveScale(this GameObject gameObject, bool active)
        {
            gameObject.transform.localScale = active ? Vector3.one : Vector3.zero;
        }

        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            gameObject.GetComponentsInChildren(true, s_CachedTransforms);
            for (int i = 0; i < s_CachedTransforms.Count; i++)
            {
                s_CachedTransforms[i].gameObject.layer = layer;
            }
            s_CachedTransforms.Clear();
        }

        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static Transform FindEx(this Transform t, string name)
        {
            if (t == null)
                return null;
            var list = ListPool<Transform>.Get();
            list.Add(t); // 添加自己
            int index = 0;
            // 递归遍历，找到则返回
            while (list.Count > index)
            {
                var transform = list[index++];
                for (int i = 0; i < transform.childCount; ++i)
                {
                    var child = transform.GetChild(i);
                    if (child.name == name)
                    {
                        ListPool<Transform>.Release(list);
                        return child;
                    }
                    list.Add(child);
                }
            }
            ListPool<Transform>.Release(list);
            return null;
        }

        public static GameObject FindEx(this GameObject go, string name)
        {
            if (go == null)
                return null;

            var target = go.transform.FindEx(name);
            return target == null ? null : target.gameObject;
        }
    }
}
