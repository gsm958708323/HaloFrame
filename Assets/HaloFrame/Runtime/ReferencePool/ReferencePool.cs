using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    public class ReferencePool
    {
        private static readonly Dictionary<Type, ReferenceCollection> refColDict = new Dictionary<Type, ReferenceCollection>();

        public static int Count
        {
            get
            {
                return refColDict.Count;
            }
        }

        /// <summary>
        /// 从引用池中获取引用对象
        /// </summary>
        public static T Get<T>() where T : class, IReference, new()
        {
            return GetRefCollection(typeof(T)).Get<T>();
        }

        /// <summary>
        /// 从引用池中获取引用对象
        /// </summary>
        public static IReference Get(Type type)
        {
            CheckRefType(type);
            return GetRefCollection(type).Get();
        }

        /// <summary>
        /// 释放引用对象
        /// </summary>
        /// <param name="reference"></param>
        public static void Release(IReference reference)
        {
            if (reference == null)
            {
                Debugger.LogError("引用类型错误", LogDomain.ReferencePool);
                return;
            }

            Type referenceType = reference.GetType();
            CheckRefType(referenceType);
            GetRefCollection(referenceType).Release(reference);
        }

        /// <summary>
        /// 清除所有引用池。
        /// todo 切场景时调用？？
        /// </summary>
        public static void ClearAll()
        {
            lock (refColDict)
            {
                foreach (var item in refColDict)
                {
                    item.Value.RemoveAll();
                }

                refColDict.Clear();
            }
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            GetRefCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public static void Add(Type referenceType, int count)
        {
            CheckRefType(referenceType);
            GetRefCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            GetRefCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public static void Remove(Type referenceType, int count)
        {
            CheckRefType(referenceType);
            GetRefCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetRefCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll(Type referenceType)
        {
            CheckRefType(referenceType);
            GetRefCollection(referenceType).RemoveAll();
        }

        /// <summary>
        /// 获取指定类型的引用对象集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ReferenceCollection GetRefCollection(Type type)
        {
            if (type == null)
            {
                Debugger.LogError("引用类型错误", LogDomain.ReferencePool);
                return null;
            }

            ReferenceCollection refCol = null;
            lock (refColDict)
            {
                if (!refColDict.TryGetValue(type, out refCol))
                {
                    refCol = new ReferenceCollection(type);
                    refColDict.Add(type, refCol);
                }
            }
            return refCol;
        }

        static void CheckRefType(Type refType)
        {
            if (refType == null)
            {
                throw new HaloException("Reference type is invalid.");
            }

            if (!refType.IsClass || refType.IsAbstract)
            {
                throw new HaloException("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(refType))
            {
                throw new HaloException($"Reference type '{refType.FullName}' is invalid.");
            }
        }
    }
}
