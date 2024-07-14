using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UIView
    {
        public GameObject gameObject;
        public Transform transform;
        public RectTransform rectTransform;
        UIViewCtrl uiViewCtrl;

        public void Clear()
        {
            gameObject = null;
            transform = null;
            rectTransform = null;
            uiViewCtrl = null;
        }

        public void CloseSelf()
        {
            GameManager.UIManager.Close(uiViewCtrl.UIConfig.ViewType);
        }

        public virtual void OnInit(GameObject go, UIViewCtrl ctrl)
        {
            uiViewCtrl = ctrl;
            gameObject = go;
            transform = go.transform;
            rectTransform = go.GetComponent<RectTransform>();
        }

        public virtual void OnOpen(object data)
        {
            Debugger.Log($"OnOpen {uiViewCtrl.UIConfig.ViewType}", LogDomain.UI);
        }

        /// <summary>
        /// 界面被恢复时调用
        /// </summary>
        public virtual void OnResume()
        {
            Debugger.Log($"OnResume {uiViewCtrl.UIConfig.ViewType}", LogDomain.UI);

        }

        /// <summary>
        /// 界面被覆盖时调用，被非全屏和全屏界面覆盖时都会调用。
        /// 暂停和恢复不影响其是否被显示隐藏，只要不是最上层UI都应该标记暂停状态
        /// </summary>
        public virtual void OnPause()
        {
            Debugger.Log($"OnPause {uiViewCtrl.UIConfig.ViewType}", LogDomain.UI);

        }

        public virtual void OnClose()
        {
            Debugger.Log($"OnClose {uiViewCtrl.UIConfig.ViewType}", LogDomain.UI);

        }
    }
}
