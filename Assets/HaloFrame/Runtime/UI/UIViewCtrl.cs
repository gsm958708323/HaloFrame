using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UIViewCtrl : IReference
    {
        UIConfig uIConfig;
        UIView uiView;
        UILayer uILayer;
        public int Order;
        public UIViewCtrl() { }

        internal static UIViewCtrl Get(UIConfig item, UIView uIView, UILayer uILayer)
        {
            var ctrl = ReferencePool.Get<UIViewCtrl>();
            ctrl.uIConfig = item;
            ctrl.uiView = uIView;
            ctrl.uILayer = uILayer;
            return ctrl;
        }

        public void Clear()
        {
            uIConfig = null;
            uiView = null;
            uILayer = null;
        }

        internal void Open(object data = null, Action action = null)
        {
            GameObject go = GameManager.ResourceManager.LoadAsset<GameObject>(uIConfig.Prefab);
            if (go == null)
            {
                Debugger.LogError($"加载预制体失败 {uIConfig.Prefab}", LogDomain.UI);
                return;
            }

            go.transform.SetParentEx(uILayer.Canvas.transform);
            uiView.OnInit(go, this); // todo 组件绑定
            
        }
    }
}
