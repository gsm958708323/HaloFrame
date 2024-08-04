/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/20 22:14:21
** dsec:  UIView_LifeCycle 
管理View的生命周期
Async相关方法是处理需要等待的任务，如动画播放，加载等
生命周期：LoadAsset - Awake - Start - Enable - Disable - Destroy - ReleaseUI
UIView 
    - UIGameView 游戏界面
    - UISubView 子界面
*******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HaloFrame
{
    public partial class UIView
    {
        public GameObject gameObject;
        public Transform transform;
        public RectTransform rectTransform;
        public UIConfig UIConfig;
        public UILayer UILayer;

        public UIState UIState { get; internal set; }
        public TaskCompletionSource<bool> LoadTask { get; private set; }
        public int SortingOrder { get; private set; }

        /// <summary>
        /// 存储子界面的canvas
        /// </summary>
        private Dictionary<Canvas, int> canvasDict;

        public UIView()
        {
            UIState = UIState.None;
            canvasDict = new Dictionary<Canvas, int>();
            LoadTask = new TaskCompletionSource<bool>();
        }

        public void Bind(UIConfig config, UILayer layer)
        {
            UIConfig = config;
            UILayer = layer;
        }

        public void Clear()
        {
            gameObject = null;
            transform = null;
            rectTransform = null;
            UIConfig = null;
            UILayer = null;
            LoadTask = null;
            canvasDict = null;
        }

        public void SetActive(bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActiveCanvas(active);
            }
        }


        /// <summary>
        /// 这里写GameObject相关的操作
        /// </summary>
        /// <param name="go"></param>
        internal void OnLoadAsset(GameObject go, Transform parent)
        {
            gameObject = go;
            transform = go.transform;
            rectTransform = go.GetComponent<RectTransform>();
            SetActive(false);

            // 设置父节点
            go.transform.SetParentEx(parent);
            go.transform.SetAsLastSibling();

            // 子节点canvas信息初始化
            var canvases = gameObject.GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                var childCanvas = canvases[i];
                canvasDict.Add(childCanvas, childCanvas.sortingOrder);
            }

            // 动画信息初始化
        }

        internal void SetCanvasOrder(int order)
        {
            SortingOrder = order;
            // 这里会包含自己和所有子节点的canvas
            foreach (var item in canvasDict)
            {
                item.Key.sortingOrder = item.Value + order;
            }
        }

        internal virtual void Awake()
        {
            if (UIState <= UIState.Loading)
            {
                UIState = UIState.Awake;
                OnAwake();
                SetActive(false);
            }
        }
        protected virtual void OnAwake()
        {
            Debugger.Log($"{this.GetType().Name} OnAwake", LogDomain.UI);
        }

        internal async Task StartAsync(object[] args)
        {
            if (UIState <= UIState.Loading)
                return;

            if (UIState == UIState.Awake)
            {
                Start(args);
                Enable();

                await WaitAnimation();
            }
        }

        internal void Start(object[] args)
        {
            if (UIState == UIState.Awake)
            {
                UIState = UIState.Start;
                SetActive(true);

                OnStart(args);
            }
        }
        protected virtual void OnStart(object[] args)
        {
            Debugger.Log($"{this.GetType().Name} OnStart", LogDomain.UI);
        }
        internal async Task EnableAsync()
        {
            if (UIState <= UIState.Loading)
            {
                return;
            }
            if (UIState == UIState.Start || UIState == UIState.Disable)
            {
                Enable();

                await WaitAnimation();
            }
        }
        internal virtual void Enable()
        {
            if (UIState == UIState.Start || UIState == UIState.Disable)
            {
                UIState = UIState.Enable;
                SetActive(true);

                OnEnable();
            }
        }
        protected virtual void OnEnable()
        {
            Debugger.Log($"{this.GetType().Name} OnEnable", LogDomain.UI);
        }

        internal async Task DisableAsync()
        {
            if (UIState <= UIState.Loading)
            {
                return;
            }
            if (UIState == UIState.Start || UIState == UIState.Enable)
            {
                await WaitAnimation();
                Disable();
            }
        }
        internal virtual void Disable()
        {
            if (UIState <= UIState.Loading)
            {
                return;
            }
            if (UIState == UIState.Start || UIState == UIState.Enable)
            {
                UIState = UIState.Disable;
                OnDisable();
                SetActive(false);
            }
        }
        protected virtual void OnDisable()
        {
            Debugger.Log($"{this.GetType().Name} OnDisable", LogDomain.UI);
        }

        internal async Task DestroyAsync()
        {
            if (UIState <= UIState.Loading)
            {
                return; // todo 测试没加载完成，直接被卸载了
            }

            if (UIState == UIState.Start || UIState == UIState.Enable)
            {
                await WaitAnimation();
                Disable();
            }
        }

        internal virtual void Destroy()
        {
            if (UIState <= UIState.Loading)
            {
                return;
            }
            Disable();

            if (UIState < UIState.Destroy)
            {
                UIState = UIState.Destroy;
                OnDestroy();
                if (gameObject != null)
                {
                    GameObject.Destroy(this.gameObject);
                }

                Clear();
            }
        }

        protected virtual void OnDestroy()
        {
            Debugger.Log($"{this.GetType().Name} OnDestroy", LogDomain.UI);
        }

        private async Task WaitAnimation()
        {
        }
    }
}
