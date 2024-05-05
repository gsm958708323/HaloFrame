using System;
using System.Collections;
using System.Collections.Generic;

namespace HaloFrame
{
    /// <summary>
    /// 存储相同类型的引用对象集合。
    /// </summary>
    public class ReferenceCollection
    {
        private readonly Queue<IReference> refQue;
        private readonly Type refType;
        /// <summary>
        /// 正在使用中的数量
        /// </summary>
        public int UsingRefCount { get; private set; }
        /// <summary>
        /// 从池子中拿出的数量
        /// </summary>
        public int GetsRefCount { get; private set; }
        /// <summary>
        /// 从池子中释放的数量
        /// </summary>
        public int ReleaseRefCount { get; private set; }

        /// <summary>
        /// 新创建的引用数量，可能新添加但未使用
        /// AddRefCount >= UsingRefCount
        /// </summary>
        public int AddRefCount { get; private set; }
        /// <summary>
        /// 移除的引用数量
        /// </summary>
        /// </summary>
        public int RemoveRefCount { get; private set; }

        public ReferenceCollection(Type referenceType)
        {
            refQue = new Queue<IReference>();
            refType = referenceType;
            UsingRefCount = 0;
            GetsRefCount = 0;
            ReleaseRefCount = 0;
            AddRefCount = 0;
            RemoveRefCount = 0;
        }

        /// <summary>
        /// 创建指定泛型T类型的引用
        /// 获取可实例化的，并且继承自IReference的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : class, IReference, new()
        {
            if (typeof(T) != refType)
            {
                Debugger.LogError($"引用类型错误", LogDomain.ReferencePool);
                return null;
            }

            UsingRefCount++;
            GetsRefCount++;
            lock (refQue)
            {
                if (refQue.Count > 0)
                {
                    var refObj = refQue.Dequeue() as T;
                    return refObj;
                }
            }

            AddRefCount++;
            return new T();
        }

        /// <summary>
        /// 创建指定Type的引用
        /// </summary>
        /// <returns></returns>
        public IReference Get()
        {
            UsingRefCount++;
            GetsRefCount++;
            lock (refQue)
            {
                if (refQue.Count > 0)
                {
                    return refQue.Dequeue();
                }
            }

            AddRefCount++;
            return (IReference)Activator.CreateInstance(refType);
        }

        public void Release(IReference reference)
        {
            if (reference == null || reference.GetType() != refType)
            {
                Debugger.LogError($"引用类型错误", LogDomain.ReferencePool);
                return;
            }
            reference.Clear();
            lock (refQue)
            {
                if (refQue.Contains(reference))
                {
                    Debugger.LogError($"此引用被重复释放", LogDomain.ReferencePool);
                }
                refQue.Enqueue(reference);
            }

            UsingRefCount--;
            ReleaseRefCount++;
        }

        /// <summary>
        /// 添加指定数量的引用
        /// </summary>
        public void Add<T>(int count) where T : class, IReference, new()
        {
            if (typeof(T) != refType)
            {
                Debugger.LogError($"引用类型错误", LogDomain.ReferencePool);
                return;
            }

            lock (refQue)
            {
                for (int i = 0; i < count; i++)
                {
                    refQue.Enqueue(new T());
                    AddRefCount++;
                }
            }
        }

        public void Add(int count)
        {
            lock (refQue)
            {
                for (int i = 0; i < count; i++)
                {
                    refQue.Enqueue((IReference)Activator.CreateInstance(refType));
                    AddRefCount++;
                }
            }
        }

        public void Remove(int count)
        {
            lock (refQue)
            {
                if (count > refQue.Count)
                {
                    count = refQue.Count;
                }

                for (int i = 0; i < count; i++)
                {
                    refQue.Dequeue();
                    RemoveRefCount++;
                }
            }
        }

        public void RemoveAll()
        {
            lock (refQue)
            {
                RemoveRefCount += refQue.Count;
                refQue.Clear();
            }
        }

        public override string ToString()
        {
            return $"ReferenceCollection: Type: {refType.Name}, Using: {UsingRefCount}, Gets: {GetsRefCount}, Release: {ReleaseRefCount}, Add: {AddRefCount}, Remove: {RemoveRefCount}";
        }
    }
}

public interface IReference
{
    /// <summary>
    /// 清理引用。
    /// </summary>
    void Clear();
}

