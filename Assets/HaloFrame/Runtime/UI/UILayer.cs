using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace HaloFrame
{
    public class UILayer : IReference
    {
        private UILayerType layerType;
        public Canvas Canvas;
        private int maxOrder; // 当前层级的最大order值
        private HashSet<int> orderSet; // 当前已分配的order值
        public Stack<UIViewCtrl> viewStack;
        private List<UIViewCtrl> tempList;

        public UILayer()
        {
            orderSet = new();
            viewStack = new();
            tempList = new();
        }

        public void Clear()
        {
            layerType = 0;
            Canvas = null;
            orderSet.Clear();
            viewStack.Clear();
            tempList.Clear();
        }

        public static UILayer Get(UILayerType layerType, Canvas canvas)
        {
            var layer = ReferencePool.Get<UILayer>();
            layer.layerType = layerType;
            layer.Canvas = canvas;
            return layer;
        }

        public UIViewCtrl GetTop()
        {
            viewStack.TryPeek(out UIViewCtrl topCtrl);
            return topCtrl;
        }

        internal void OpenUI(UIViewCtrl curCtrl)
        {
            if (curCtrl.Order == 0)
            {
                curCtrl.Order = PopOrder(curCtrl);
            }

            foreach (var ctrl in viewStack)
            {
                // 筛选出order值小于当前界面的，设置为暂停状态 
                if (ctrl != curCtrl && ctrl.Order < curCtrl.Order && ctrl.IsActive)
                {
                    if (!ctrl.IsPause)
                    {
                        // 这里不会触发界面的显示隐藏
                        ctrl.IsPause = true;
                        ctrl.UIView.OnPause();
                    }

                    if (!curCtrl.UIConfig.IsWindow)
                    {
                        // 这里才会触发界面的显示隐藏
                        ctrl.AddTopViewNum(1);
                    }
                }
            }
        }

        internal void CloseUI(UIViewCtrl curCtrl)
        {
            int order = curCtrl.Order;
            PushOrder(curCtrl); // 归还不再使用的order
            curCtrl.Order = 0;

            if (viewStack.Count > 0)
            {
                var topView = viewStack.Peek();
                if (topView.IsPause)
                {
                    topView.IsPause = false;
                    topView.UIView.OnResume();
                }

                if (!curCtrl.UIConfig.IsWindow)
                {
                    foreach (var ctrl in viewStack)
                    {
                        if (ctrl != curCtrl && ctrl.Order < order && ctrl.IsOpen)
                        {
                            ctrl.AddTopViewNum(-1);
                        }
                    }
                }
            }
        }

        private void PushOrder(UIViewCtrl curCtrl)
        {
            int order = curCtrl.Order;
            if (!orderSet.Contains(order))
                return;

            orderSet.Remove(order);

            tempList.Clear();
            // 栈无法直接从中间移除，操作当前界面到栈顶中间的所有界面 todo 把栈改成list？
            while (viewStack.Count > 0)
            {
                var ctrl = viewStack.Pop();
                if (ctrl != curCtrl)
                {
                    tempList.Add(ctrl);
                }
                else
                {
                    break;
                }
            }

            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                viewStack.Push(tempList[i]);
            }
        }

        internal int PopOrder(UIViewCtrl curCtrl)
        {
            maxOrder += 10; // 每个界面给10个层级
            orderSet.Add(maxOrder);
            viewStack.Push(curCtrl);
            return maxOrder;
        }
    }
}
