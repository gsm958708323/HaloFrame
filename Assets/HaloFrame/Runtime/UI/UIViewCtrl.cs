using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UIViewCtrl : IReference
    {
        public UIConfig UIConfig;
        public UIView UIView;
        public UILayer UILayer;
        private int topViewNum;
        public int Order;
        public bool IsLoading, IsOpen, IsPause;

        // 当前界面是否激活（预制体加载完毕）
        public bool IsActive
        {
            get { return UIView != null && UIView.gameObject != null; }
        }

        public UIViewCtrl() { }

        internal static UIViewCtrl Get(UIConfig item, UIView uIView, UILayer uILayer)
        {
            var ctrl = ReferencePool.Get<UIViewCtrl>();
            ctrl.UIConfig = item;
            ctrl.UIView = uIView;
            ctrl.UILayer = uILayer;
            return ctrl;
        }

        public void Clear()
        {
            // 回收
            UIConfig = null;
            UIView = null;
            UILayer = null;

            IsLoading = false;
            IsOpen = false;
            IsPause = false;
            Order = 0;
        }

        public void Open(object data = null, Action action = null, bool isFirst = false)
        {
            IsOpen = true;
            if (IsLoading)
                return; // 正在加载中

            if (!IsActive)
            {
                // 还没有加载过
                Load(data, action);
            }
            else
            {
                if (UILayer.GetTop() == this)
                {
                    return; // 已经在最上层，不需要再打开
                }

                // 已经加载完成
                if (isFirst == false && IsOpen && Order > 0)
                {
                    OnClose(); // 先关闭下，再重新打开  todo 验证是否去掉               
                }
                OnOpen(data, action);
            }
        }

        private void Load(object data = null, Action action = null)
        {
            IsLoading = true;

            GameObject goPrefab = GameManager.ResourceManager.LoadAsset<GameObject>(UIConfig.Prefab);
            if (goPrefab == null)
            {
                Debugger.LogError($"加载预制体失败 {UIConfig.Prefab}", LogDomain.UI);
                return;
            }
            GameObject go = GameObject.Instantiate(goPrefab);
            IsLoading = false;
            go.transform.SetParentEx(UILayer.Canvas.transform);
            go.transform.SetAsLastSibling();
            // RectTransform rectTransform = go.transform as RectTransform;
            // rectTransform.anchoredPosition = Vector2.zero;
            // rectTransform.sizeDelta = Vector2.zero;

            UIView.OnInit(go, this); // todo 组件绑定
            if (IsOpen)
            {
                Open(data, action, true);
            }
            else
            {
                Close(action);
            }
        }

        public void Close(Action action = null)
        {
            IsOpen = false;
            if (IsLoading)
                return;

            if (IsActive)
            {
                OnClose(action);
            }
        }

        private void OnOpen(object data = null, Action action = null)
        {
            UILayer.OpenUI(this);
            SetVisible(true);
            AddTopViewNum(0);
            UIView.OnOpen(data);
            UIView.OnResume();
            action?.Invoke();
        }

        /// <summary>
        /// 用来记录当前界面上还有多少个全屏界面(外部通过此方法，通过计数计算是否要显示)
        /// </summary>
        /// <param name="v"></param>
        public void AddTopViewNum(int num)
        {
            topViewNum += num;
            topViewNum = Mathf.Max(0, topViewNum);
            SetVisible(topViewNum <= 0);
        }

        private void OnClose(Action action = null)
        {
            UILayer.CloseUI(this);
            AddTopViewNum(-100000);
            SetVisible(false);
            UIView.OnPause();
            UIView.OnClose();
            action?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            if (IsActive)
            {
                UIView.gameObject.SetActiveScale(visible);
            }
        }
    }
}
