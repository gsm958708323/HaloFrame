using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HaloFrame
{
    public class UIView
    {
        public GameObject gameObject;
        public Transform transform;
        public RectTransform rectTransform;
        public UIConfig UIConfig;

        public UIState UIState { get; internal set; }

        public void Clear()
        {
            gameObject = null;
            transform = null;
            rectTransform = null;
            UIConfig = null;
        }

        internal void Destroy()
        {
            throw new NotImplementedException();
        }

        internal async Task DestroyAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task DisableAsync()
        {
            throw new NotImplementedException();
        }

        internal async Task EnableAsync()
        {
            throw new NotImplementedException();
        }

        internal void SetOrder(int order)
        {
            throw new NotImplementedException();
        }

        internal async Task StartAsync(object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
