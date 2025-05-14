/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/21 14:53:57
** dsec:  UIGameView 
管理子界面生命周期
父界面存在时，子界面不会销毁，只会隐藏
*******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HaloFrame
{
    public class UIGameView : UIView
    {
        /// <summary>
        /// 存储子界面的界面实例
        /// </summary>
        private Dictionary<Type, UISubView> childDict = new();
        /// <summary>
        /// 当前正在显示的界面
        /// </summary>
        /// <typeparam name="UISubView"></typeparam>
        /// <returns></returns>
        private List<UISubView> showList = new();
        private List<UIItem> itemList = new();

        internal override void Awake()
        {
            base.Awake();
            // 已经加载的子界面需要执行Awake方法
            AwakeChild();
        }

        internal override void Destroy()
        {
            DestroyChild();
            DestroyItem();
            base.Destroy();
        }

        internal override void Enable()
        {
            base.Enable();
            EnableChild();
            EnableItem();
        }

        internal override void Disable()
        {
            DisableChild();
            DisableItem();
            base.Disable();
        }

        protected virtual void CloseSelf()
        {
            GameManager.UI.Close(GetType());
        }

        public T CreateItem<T>(int resId = 0)
                    where T : UIItem, new()
        {
            // 创建实例
            var item = (UIItem)AssemblyTools.CreateInstance(typeof(T));
            if (item == null)
                return null;

            itemList.Add(item);
            item.Parent = this;

            // 创建go
            item.UIState = UIState.Loading;
            GameObject go;
            if (resId != 0)
            {
                var resource = GameManager.Resource.Load(resId, false);
                go = resource.Instantiate(true);
            }
            else
            {
                GameObject prefab = gameObject.FindEx(item.ToString());
                go = GameObject.Instantiate(prefab);
            }
            if (go == null)
            {
                Debugger.LogError($"创建item失败 未找到对应预制体 {item}", LogDomain.UI);
                return null;
            }
            item.OnLoadAsset(go, transform);
            item.Awake();

            return item as T;
        }

        public void DestroyItem()
        {
            foreach (var item in itemList)
            {
                item.Destroy();
            }
            itemList.Clear();
        }

        private void DisableItem()
        {
            foreach (var item in itemList)
            {
                item.Disable();
            }
        }

        private void EnableItem()
        {
            foreach (var item in itemList)
            {
                item.Enable();
            }
        }

        public void OpenChild<T>(ResType resType = ResType.Dynamic, Action callback = null, params object[] args)
        {
            var viewType = typeof(T);
            UISubView subVIew = FindChild(viewType);
            if (subVIew == null)
            {
                Debugger.LogError($"子界面不存在 {viewType}");
                return;
            }

            subVIew.ResType = resType;
            OpenAsync(subVIew, callback, args);
        }

        /// <summary>
        /// 只打开当前子界面，其他子界面关闭
        /// </summary>
        /// <param name="viewType"></param>
        /// <param name="args"></param>
        public void OpenOneChildUI<T>(ResType resType = ResType.Dynamic, Action callback = null, params object[] args)
        {
            var viewType = typeof(T);
            OpenOneChildUiAsync(viewType, resType, callback, args);
        }

        private void OpenOneChildUiAsync(Type viewType, ResType resType, Action callback = null, params object[] args)
        {
            UISubView subView = FindChild(viewType);
            if (subView == null)
            {
                Debugger.LogError($"子界面不存在 {viewType}");
                return;
            }
            subView.ResType = resType;

            var isShow = false;
            // 关闭其他界面
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var child = showList[i];
                var childType = child.GetType();
                if (childType != viewType)
                {
                    CloseChild(childType);
                }
                else
                {
                    isShow = true;
                }
            }

            // 没有显示才重新打开
            if (!isShow)
            {
                OpenAsync(subView, callback, args);
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

            // 动态创建的界面需要加上父界面的order，然后和父界面同级
            var add = subView.ResType == ResType.Dynamic ? subView.Parent.SortingOrder : 0;
            var order = showList.Count * UIDefine.ORDER_SUBVIEW_ADD + add;

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

        public void CloseChild(Type viewType, Action callback = null)
        {
            if (!IsActive(viewType))
                return;

            if (!childDict.TryGetValue(viewType, out var subView))
                return;

            CloseAsync(viewType, callback);
        }

        private async void CloseAsync(Type viewType, Action callback)
        {
            await WaitChildAnimation();

            if (childDict.TryGetValue(viewType, out var childUI))
            {
                await childUI.DisableAsync();
            }

            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var tempUI = showList[i];
                if (tempUI.GetType() == viewType)
                {
                    showList.RemoveAt(i);
                    break;
                }
            }
            callback?.Invoke();
        }

        public void RemoveChild<T>()
        {
            var viewType = typeof(T);
            for (int i = showList.Count - 1; i >= 0; i--)
            {
                var tempUI = showList[i];
                if (tempUI.GetType() == viewType)
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

        public void AddChild(Type viewType, UISubView subView)
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

        private bool IsActive(Type viewType)
        {
            for (int i = 0; i < showList.Count; i++)
            {
                var tempUI = showList[i];
                if (tempUI.GetType() == viewType)
                {
                    return true;
                }
            }
            return false;
        }

        public UISubView FindChild(Type viewType)
        {
            childDict.TryGetValue(viewType, out var childUI);
            return childUI;
        }

        internal UIView FindChild(string name)
        {
            var type = AssemblyTools.GetType(name);
            if (type == null)
                return null;
            return FindChild(type);
        }

        private async Task WaitChildAnimation()
        {

        }
    }
}
