using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UIView : IReference
    {
        protected GameObject gameObject;
        protected Transform transform;
        UIViewCtrl uiViewCtrl;

        public static UIView Get(Type type)
        {
            var ui = ReferencePool.Get(type);
            return ui as UIView;
        }

        public void Clear()
        {
            gameObject = null;
            transform = null; ;
            uiViewCtrl = null;
        }

        internal void OnInit(GameObject go, UIViewCtrl ctrl)
        {
            gameObject = go;
            transform = go.transform;
            uiViewCtrl = ctrl;
        }
    }
}
