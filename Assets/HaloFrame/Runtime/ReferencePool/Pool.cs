/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/20 18:09:17
** dsec:  Pool 
*******************************************************/
using System;
using System.Collections.Generic;

namespace HaloFrame
{
    public interface IPool
    {
        /// <summary>
        /// 释放对象池，并将对象池实例设置为空
        /// </summary>
        void Dispose();
    }
    public interface IObject
    {
        void Release();
    }

    /// <summary>
    /// 管理所有对象池
    /// </summary>
    public static class Pool
    {
        private readonly static List<IPool> allPools = new List<IPool>();

        public static void Add(IPool pool)
        {
            allPools.Add(pool);
        }

        /// <summary>
        /// 清除所有对象池
        /// </summary>
        /// <param name="pool"></param>
        public static void DisposeAll()
        {
            foreach (var item in allPools)
            {
                item.Dispose();
            }
            allPools.Clear();
        }

        public static void Debug()
        {
            Debugger.Log($"对象池数量：{allPools.Count}", LogDomain.ReferencePool);
        }
    }

    public class ObjectPool<T> : IPool where T : new()
    {
        private Stack<T> pool;
        private static ObjectPool<T> instance;

        private static void Init()
        {
            if (instance != null)
                return;

            instance = new ObjectPool<T>
            {
                pool = new Stack<T>()
            };
            Pool.Add(instance);
        }

        public void Dispose()
        {
            if (instance == null)
                return;

            if (instance.pool != null)
            {
                instance.pool.Clear();
                instance.pool = null;
            }
            instance = null;
        }

        public static T Get()
        {
            Init();
            if (instance.pool.Count > 0)
            {
                return instance.pool.Pop();
            }
            else
            {
                return new T();
            }
        }

        public void Release(T obj)
        {
            if (obj == null || instance == null)
                return;

            if (obj is IObject iObject)
            {
                iObject.Release();
            }
            pool.Push(obj);
        }
    }

    public class ListPool<T> : IPool
    {
        private static ListPool<T> instance;
        private Stack<List<T>> pool;

        private static void Init()
        {
            if (instance != null)
                return;

            instance = new ListPool<T>()
            {
                pool = new Stack<List<T>>()
            };
            Pool.Add(instance);
        }

        public void Dispose()
        {
            if (instance == null)
                return;
            if (instance.pool != null)
            {
                instance.pool.Clear();
                instance.pool = null;
            }
            instance = null;
        }

        public static List<T> Get()
        {
            Init();
            if (instance.pool.Count > 0)
            {
                return instance.pool.Pop();
            }
            else
            {
                return new List<T>();
            }
        }

        public static void Release(List<T> list)
        {
            if (list == null || instance == null)
                return;
            list.Clear();
            instance.pool.Push(list);
        }
    }

    public class DictionaryPool<K, V> : IPool
    {
        private static DictionaryPool<K, V> instance;
        private Stack<Dictionary<K, V>> pool;

        private static void Init()
        {
            if (instance != null)
                return;

            instance = new DictionaryPool<K, V>()
            {
                pool = new Stack<Dictionary<K, V>>()
            };
            Pool.Add(instance);
        }

        public void Dispose()
        {
            if (instance == null)
                return;
            if (instance.pool != null)
            {
                instance.pool.Clear();
                instance.pool = null;
            }
            instance = null;
        }

        public static Dictionary<K, V> Get()
        {
            Init();
            if (instance.pool.Count > 0)
            {
                return instance.pool.Pop();
            }
            else
            {
                return new Dictionary<K, V>();
            }
        }

        public static void Release(Dictionary<K, V> dict)
        {
            if (dict == null || instance == null)
                return;
            dict.Clear();
            instance.pool.Push(dict);
        }
    }
}