/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/21 14:53:57
** dsec:  UIGameView 
管理子界面生命周期
夫界面存在时，子界面不会销毁，只会隐藏
*******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HaloFrame
{
    public class UIGameView : UIView
    {
        /// <summary>
        /// 存储子界面的界面实例
        /// </summary>
        /// <typeparam name="ViewType"></typeparam>
        /// <typeparam name="UISubView"></typeparam>
        /// <returns></returns>
        private Dictionary<ViewType, UISubView> childDict = new Dictionary<ViewType, UISubView>();
        /// <summary>
        /// 当前正在显示的界面
        /// </summary>
        /// <typeparam name="UISubView"></typeparam>
        /// <returns></returns>
        private List<UISubView> showList = new List<UISubView>();

        internal override void Awake()
        {
            base.Awake();
            // 已经加载的子界面需要执行Awake方法
            AwakeChild();
        }

        internal override void Destroy()
        {
            DestroyChild();
            base.Destroy();
        }

        internal override void Enable()
        {
            base.Enable();
            EnableChild();
        }

        internal override void Disable()
        {
            DisableChild();
            base.Disable();
        }

        public void OpenChild(ViewType viewType, Action callback = null, params object[] args)
        {
            UISubView subVIew = FindChild(viewType);
            if (subVIew == null)
            {
                Debugger.LogError($"子界面不存在 {viewType}");
                return;
            }

            OpenAsync(subVIew, callback, args);
        }

        /// <summary>
        /// 只打开当前子界面，其他子界面关闭
        /// </summary>
        /// <param name="viewType"></param>
        /// <param name="args"></param>
        public void OpenOneChildUi(ViewType viewType, params object[] args)
        {
            OpenOneChildUiAsync(viewType, args);
        }

        private async void OpenOneChildUiAsync(ViewType viewType, object[] args)
        {
            UISubView subView = FindChild(viewType);
            if (subView == null)
            {
                Debugger.LogError($"子界面不存在 {viewType}");
                return;
            }

            await WaitChildAnimation();
            await GameManager.UI.LoadUIAsync(subView);

            var isShow = false;
            // 关闭其他界面
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var child = showList[i];
                if (child.UIConfig.ViewType != viewType)
                {
                    CloseChild(child.UIConfig.ViewType);
                }
                else
                {
                    isShow = true;
                }
            }
            await WaitChildAnimation();

            // 没有显示才重新打开
            if (!isShow)
            {
                OpenAsync(subView, null, args);
            }
        }

        private async void OpenAsync(UISubView subView, Action callback, object[] args)
        {
            if (subView == null)
                return;

            await WaitChildAnimation();
            await GameManager.UI.LoadUIAsync(subView);
            if (!showList.Contains(subView))
                showList.Add(subView);

            var order = subView.Parent.SortingOrder + showList.Count * UIDefine.ORDER_SUBVIEW_ADD;
            subView.SetCanvasOrder(order);
            if (subView.UIState == UIState.Awake)
            {
                await subView.StartAsync(args);
            }
            else if (subView.UIState == UIState.Start || subView.UIState == UIState.Disable)
            {
                await subView.EnableAsync();
            }

            callback?.Invoke();
        }

        public void CloseChild(ViewType viewType, Action callback = null)
        {
            if (!IsActive(viewType))
                return;

            if (!childDict.TryGetValue(viewType, out var subView))
                return;

            CloseAsync(viewType, callback);
        }

        private async void CloseAsync(ViewType viewType, Action callback)
        {
            await WaitChildAnimation();

            if (childDict.TryGetValue(viewType, out var childUI))
            {
                await childUI.DisableAsync();
            }

            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var tempUI = showList[i];
                if (tempUI.UIConfig.ViewType == viewType)
                {
                    showList.RemoveAt(i);
                    break;
                }
            }
            callback?.Invoke();
        }

        public void RemoveChild(ViewType viewType)
        {
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var tempUI = showList[i];
                if (tempUI.UIConfig.ViewType == viewType)
                {
                    showList.RemoveAt(i);
                    break;
                }
            }
            if (!childDict.TryGetValue(viewType, out var childUI))
                return;

            childUI.Destroy();
            GameManager.UI.ReleaseUI(childUI);
            childDict.Remove(viewType);
        }

        private void DisableChild()
        {
            foreach (var item in showList)
            {
                item.Disable();
            }
        }

        private void EnableChild()
        {
            foreach (var item in showList)
            {
                item.Enable();
            }
        }

        private void AwakeChild()
        {
            foreach (var item in childDict)
            {
                var child = item.Value;
                if (child.UIConfig.LoadWithParent && child.UIState != UIState.Awake)
                {
                    child.Awake();
                }
            }
        }

        private void DestroyChild()
        {
            showList.Clear();
            foreach (var item in childDict)
            {
                item.Value.Destroy();
                GameManager.UI.ReleaseUI(item.Value);
            }
            childDict.Clear();
        }

        public void AddChild(ViewType viewType, UISubView subView)
        {
            if (subView == null)
                return;

            if (childDict.ContainsKey(viewType))
            {
                Debugger.LogError($"重复添加子界面 {viewType}");
                return;
            }
            childDict.Add(viewType, subView);
            subView.Parent = this;
        }

        private bool IsActive(ViewType viewType)
        {
            for (int i = 0; i < showList.Count; i++)
            {
                var tempUI = showList[i];
                if (tempUI.UIConfig.ViewType == viewType)
                {
                    return true;
                }
            }
            return false;
        }

        public UISubView FindChild(ViewType viewType)
        {
            childDict.TryGetValue(viewType, out var childUI);
            return childUI;
        }

        private async Task WaitChildAnimation()
        {

        }
    }
}
