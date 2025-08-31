using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace HaloFrame
{
    /// <summary>
    /// 游戏入口
    /// </summary>
    public class GameManagerBase : MonoSingleton<GameManagerBase>
    {
        /// <summary>
        /// 管理器组成的链表，优先级高的排在前面
        /// </summary>
        private LinkedList<IManager> managerLinked;
        public readonly int TargetFrameRate = 60;

        float cacheTime;
        public int CurFrame;
        public float FrameInterval;

        protected override void Awake()
        {
            base.Awake();
            managerLinked = new();
            InitFrame();
            InitManager();
        }

        protected virtual IEnumerator Start() {
            yield break;
        }

        void InitFrame()
        {
            Application.targetFrameRate = TargetFrameRate;
            FrameInterval = 1.0f / TargetFrameRate;
            CurFrame = 1;
            cacheTime = 0;
        }

        protected virtual void InitManager()
        {
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            //更新：优先级高的先更新
            foreach (var item in managerLinked)
            {
                item.Update(deltaTime);
            }

            cacheTime += FrameInterval;
            while (cacheTime > FrameInterval)
            {
                foreach (var item in managerLinked)
                {
                    item.Tick(FrameInterval);
                }
                CurFrame += 1;
                cacheTime -= FrameInterval;
            }
        }

        protected override void OnDestroy()
        {
            // 销毁：优先级低的先销毁
            for (var current = managerLinked.Last; current != null; current = current.Previous)
            {
                current.Value.Exit();
            }
            managerLinked.Clear();
            base.OnDestroy();
        }

        public T GetManager<T>() where T : IManager
        {
            var type = typeof(T);
            foreach (var item in managerLinked)
            {
                if (item.GetType() == type)
                    return item as T;
            }

            return AddManager(type) as T;
        }

        IManager AddManager(Type type)
        {
            IManager manager = (IManager)Activator.CreateInstance(type, true);
            if (manager == null)
            {
                Debugger.LogError($"[管理器] 创建管理器失败 {type.FullName}", LogDomain.Manager);
                return null;
            }

            LinkedListNode<IManager> current = managerLinked.First;
            while (current != null)
            {
                if (manager.Priority > current.Value.Priority)
                    break;

                current = current.Next;
            }

            if (current != null)
            {
                //找到一个比新节点优先级小的节点，把新节点放在它前面
                managerLinked.AddBefore(current, manager);
            }
            else
            {
                //否则，新新节点优先级最低，放在最后
                managerLinked.AddLast(manager);
            }

            manager.Init();
            manager.Enter();
            return manager;
        }
    }
}
