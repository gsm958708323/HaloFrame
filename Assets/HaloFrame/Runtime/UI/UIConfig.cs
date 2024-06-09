using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    [System.Serializable]
    public class UIConfig
    {
        public UIViewType ViewType;
        public string Prefab;
        public UILayerType LayerType;
        public bool isWindow;
    }
}
