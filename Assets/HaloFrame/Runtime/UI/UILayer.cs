/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/28 18:13:56
** dsec:  UILayer 
界面根据不同层级，划分到不同的UILayer中管理
UILayer使用CustomStack来管理全屏与弹窗界面
新界面如果是弹窗则不做处理，如果是全屏界面，则要关闭上一个界面
*******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public class UILayer
    {
        private CustomStack<UIView> uiStack;
        private LayerType layerType;
        public Canvas Canvas;
        private int baseOrder;

        public UILayer(LayerType layerType, Canvas canvas)
        {
            this.layerType = layerType;
            this.Canvas = canvas;
            uiStack = new CustomStack<UIView>();
            baseOrder = canvas.sortingOrder;
        }

        internal void Open(Type type, Action action, object[] args)
        {
            UIView view = GameManager.UI.CreateUI(type, this);
            if (view == null)
            {
                Debugger.LogError($"创建UI对象失败 {type}", LogDomain.UI);
                return;
            }

            OpenAsync(view, action, args);
        }

        private async void OpenAsync(UIView view, Action action, object[] args)
        {
            await GameManager.UI.LoadUIAsync(view);

            // 新界面如果是弹窗则不做处理
            // 如果是全屏界面，则要关闭上一个界面
            if (!view.UIConfig.IsPopup && uiStack.Count != 0)
            {
                var uiList = uiStack.GetList();
                for (int i = uiList.Count - 1; i >= 0; i--)
                {
                    var preView = uiList[i];
                    if (preView != null && preView.UIState == UIState.Enable)
                    {
                        await preView.DisableAsync();
                    }
                }
            }

            uiStack.Push(view);
            int order = (uiStack.Count - 1) * UIDefine.ORDER_VIEW_ADD + baseOrder;
            view.SetCanvasOrder(order);

            await view.StartAsync(args);
            action?.Invoke();
        }

        public void Pop(Action action = null)
        {
            PopAsync(action);
        }

        private async void PopAsync(Action action)
        {
            // 当前栈顶界面退出
            if (uiStack.Count != 0)
            {
                UIView topView = uiStack.Peek();
                await topView.DestroyAsync();
                uiStack.Pop();
                topView.Destroy();
                GameManager.UI.ReleaseUI(topView);
            }

            // 前一个界面显示
            if (uiStack.Count != 0)
            {
                var preView = uiStack.Peek();
                if (preView != null && preView.UIState == UIState.Disable)
                {
                    if (preView.UIConfig.IsPopup)
                    {
                        var uiList = uiStack.GetList();

                        // 前一个界面为弹窗，则要找到一个全屏界面，并将中间的所有弹窗都显出来
                        var list = new List<UIView>(); // todo 池
                        for (int i = uiList.Count - 1; i >= 0; i--)
                        {
                            var view = uiList[i];
                            if (view == null)
                                break;

                            list.Add(view);
                            if (!view.UIConfig.IsPopup)
                            {
                                break;
                            }
                        }

                        for (int i = 0; i < list.Count; i++)
                        {
                            var ui = list[i];
                            await ui.EnableAsync();
                        }
                    }
                    else
                    {
                        // 前一个界面为全屏界面，直接显示
                        await preView.EnableAsync();
                    }
                }
            }

            action?.Invoke();
        }
    }
}
