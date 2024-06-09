using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UILayer : IReference
    {
        private UILayerType layerType;
        public Canvas Canvas;

        public UILayer() { }

        public void Clear()
        {
            layerType = 0;
            Canvas = null;
        }

        public static UILayer Get(UILayerType layerType, Canvas canvas)
        {
            var layer = ReferencePool.Get<UILayer>();
            layer.layerType = layerType;
            layer.Canvas = canvas;
            return layer;
        }
    }
}
