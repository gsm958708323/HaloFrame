using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace HaloFrame
{
    /// <summary>
    /// Async相关方法是处理需要等待的任务，如动画播放，加载等
    /// 生命周期：LoadAsset - Awake - Start - Enable - Disable - Destroy - ReleaseUI
    /// </summary>
    public class UIView
    {
        public GameObject gameObject;
        public Transform transform;
        public RectTransform rectTransform;
        public UIConfig UIConfig;
        public UILayer UILayer;

        public UIState UIState { get; internal set; }
        public TaskCompletionSource<bool> TaskResult { get; private set; }
        public int SortingOrder { get; private set; }

        /// <summary>
        /// 存储子界面的canvas
        /// </summary>
        private Dictionary<Canvas, int> canvasDict;

        public UIView()
        {
            UIState = UIState.None;
            canvasDict = new Dictionary<Canvas, int>();
            TaskResult = new TaskCompletionSource<bool>();
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
            TaskResult = null;
            canvasDict = null;
        }

        private void ActiveGameObject(bool active)
        {
            if (gameObject != null)
            {
                gameObject.SetActiveEx(active);
            }
        }
        protected void CloseSelf()
        {
            GameManager.UI.Close(UIConfig.ViewType);
        }

        /// <summary>
        /// 这里写GameObject相关的操作
        /// </summary>
        /// <param name="go"></param>
        internal void LoadAsset(GameObject go)
        {
            gameObject = go;
            transform = go.transform;
            rectTransform = go.GetComponent<RectTransform>();
            ActiveGameObject(false);

            // 设置父节点
            go.transform.SetParentEx(UILayer.Canvas.transform);
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

        internal void SetOrder(int order)
        {
            SortingOrder = order;
            foreach (var item in canvasDict)
            {
                item.Key.sortingOrder = item.Value + order;
            }
        }

        internal void Awake()
        {
            if (UIState <= UIState.Loading)
            {
                UIState = UIState.Awake;
                OnAwake();
                ActiveGameObject(false);
            }
        }
        protected virtual void OnAwake() { }

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

        public void Start(object[] args)
        {
            if (UIState == UIState.Awake)
            {
                UIState = UIState.Start;
                ActiveGameObject(true);

                OnStart(args);
            }
        }
        protected virtual void OnStart(object[] args) { }
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
        private void Enable()
        {
            if (UIState == UIState.Start || UIState == UIState.Disable)
            {
                UIState = UIState.Enable;
                ActiveGameObject(true);

                OnEnable();
            }
        }
        protected virtual void OnEnable() { }

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
        private void Disable()
        {
            if (UIState <= UIState.Loading)
            {
                return;
            }
            if (UIState == UIState.Start || UIState == UIState.Enable)
            {
                UIState = UIState.Disable;
                OnDisable();
                ActiveGameObject(false);
            }
        }
        protected virtual void OnDisable() { }

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

        internal void Destroy()
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

        protected virtual void OnDestroy() { }

        private async Task WaitAnimation()
        {
        }
    }
}
