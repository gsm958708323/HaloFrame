using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace HaloFrame
{
    public class UIManager : IManager
    {
        /// <summary>
        /// 存储UI界面对应的类
        /// </summary>
        private Dictionary<ViewType, Type> uiDict;
        private Dictionary<LayerType, UILayer> layerDict;
        private HashSet<ViewType> openSet;
        private Camera worldCamera, uiCamera;
        public int width = 1920;
        public int height = 1080;

        public override void Init()
        {
            base.Init();
            layerDict = new();
            uiDict = new();
            openSet = new();

            InitUI();
        }

        void InitUI()
        {
            worldCamera = Camera.main;
            worldCamera.cullingMask &= int.MaxValue ^ (1 << Layer.UI);
            var root = GameObject.Find("UIRoot");
            if (root == null)
                root = new GameObject("UIRoot");

            root.layer = Layer.UI;
            GameObject.DontDestroyOnLoad(root);

            var camera = GameObject.Find("UICamera");
            if (camera == null)
                camera = new GameObject("UICamera");
            uiCamera = camera.GetOrAddComponent<Camera>();
            uiCamera.cullingMask = 1 << Layer.UI;
            uiCamera.transform.SetParent(root.transform);
            uiCamera.orthographic = true;
            uiCamera.clearFlags = CameraClearFlags.Depth;

            // 创建layer层级对象，指定UI相机
            var layers = Enum.GetValues(typeof(LayerType));
            foreach (LayerType layer in layers)
            {
                bool is3d = layer == LayerType.SceneLayer;
                Canvas canvas = UIExtension.CreateLayerCanvas(layer, is3d, root.transform, is3d ? worldCamera : uiCamera, UIDefine.WIDTH, UIDefine.HEIGHT);
                var uILayer = new UILayer(layer, canvas);
                layerDict.Add(layer, uILayer);
            }

            // 根据脚本名称找到对应界面类
            Assembly assembly = Assembly.GetExecutingAssembly();
            var configDict = UIConfigSO.GetAll();
            foreach (var item in configDict)
            {
                if (!layerDict.ContainsKey(item.Value.LayerType))
                {
                    Debugger.LogError($"界面层级不存在 {item.Value.LayerType}", LogDomain.UI);
                    continue;
                }

                Type type = assembly.GetType(item.Key.ToString());
                if (type == null)
                {
                    Debugger.LogError($"界面对象不存在 {item.Key}", LogDomain.UI);
                    continue;
                }
                uiDict.Add(item.Key, type); // 打开时才做实例化操作
            }
        }

        public void Open(ViewType type, Action action = null, params object[] args)
        {
            var config = UIConfigSO.Get(type);
            if (config == null)
                return;
            if (!uiDict.TryGetValue(type, out var uiView))
            {
                Debugger.LogError($"界面对象不存在 {type}", LogDomain.UI);
                return;
            }
            if (!layerDict.TryGetValue(config.LayerType, out var layer))
            {
                Debugger.LogError($"界面层级不存在 {config.LayerType}", LogDomain.UI);
                return;
            }

            layer.Open(type, action, args);
        }

        public void Close(ViewType type)
        {
            var config = UIConfigSO.Get(type);
            if (config == null)
                return;
            if (!layerDict.TryGetValue(config.LayerType, out var layer))
            {
                Debugger.LogError($"界面层级不存在 {config.LayerType}", LogDomain.UI);
                return;
            }

            layer.Pop();
        }

        internal UIView CreateUI(ViewType type, UILayer layer)
        {
            if (!uiDict.TryGetValue(type, out var viewType))
            {
                Debugger.LogError($"界面对象不存在 {type}", LogDomain.UI);
                return null;

            }
            var config = UIConfigSO.Get(type);
            if (config == null)
                return null;

            var view = (UIGameView)Activator.CreateInstance(viewType);
            view.Bind(config, layer);

            if (config.ChildList != null)
            {
                foreach (var item in config.ChildList)
                {
                    var subConfig = UIConfigSO.Get(item);
                    if (subConfig == null)
                        continue;

                    UISubView subView = (UISubView)Activator.CreateInstance(viewType);
                    subView.Bind(subConfig, layer);
                    view.AddChild(subConfig.ViewType, subView);
                }
            }
            return view;
        }

        internal async Task LoadUIAsync(UIView view)
        {
            await LoadUITask(view);
            await LoadChildIUI(view);
            if (view.UIState == UIState.Loading)
            {
                view.Awake();
            }
        }

        private async Task LoadChildIUI(UIView view)
        {
            // 有些子界面跟随父界面一起加载
            if (view.UIConfig.ChildList.Count > 0 && view.UIConfig.LoadWithParent && view is UIGameView gameView)
            {
                foreach (var childType in view.UIConfig.ChildList)
                {
                    var child = gameView.FindChild(childType);
                    if (child != null)
                    {
                        await LoadUIAsync(child);
                    }
                }
            }
        }

        private Task LoadUITask(UIView view)
        {
            if (view.UIState == UIState.None)
            {
                LoadAsset(view);
            }
            return view.LoadTask.Task;
        }

        private void LoadAsset(UIView view)
        {
            // 支持两种加载方式：动态加载，代码指定Gameobject
            view.UIState = UIState.Loading;
            if (view.UIConfig.ResType == ResType.Dynamic)
            {
                GameObject prefab = GameManager.Resource.LoadAsset<GameObject>(view.UIConfig.Prefab);
                var go = GameObject.Instantiate(prefab);
                view.OnLoadAsset(go);
            }
            else
            {
                // 子界面 和 Item都需要有Parent
                if (view is UISubView subView)
                {
                    if (subView.Parent == null)
                    {
                        Debugger.LogError($"子界面没有Parent {subView.UIConfig.ViewType}");
                        return;
                    }
                    subView.UIState = UIState.Loading;
                    var childGo = subView.Parent.gameObject.FindEx(subView.UIConfig.ViewType.ToString());
                    if (childGo == null)
                    {
                        Debugger.LogError($"子节点不存在 {subView.UIConfig.ViewType} {subView.Parent.UIConfig.ViewType}");
                        return;
                    }
                    subView.OnLoadAsset(childGo);
                }
                else
                {
                    Debugger.LogError($"查找已有节点作为界面资源，必须是子界面 {view.UIConfig.ViewType}");
                }
            }


            if (view.LoadTask != null)
            {
                view.LoadTask.SetResult(true);
            }
        }

        internal void ReleaseUI(UIView view)
        {
            if (view.UIState == UIState.None || view.UIState == UIState.Release)
            {
                return;
            }
            view.UIState = UIState.Release;

            // todo 子界面不销毁
            if (view.gameObject != null)
            {
                GameManager.Resource.UnloadAsset(view.gameObject);
            }
        }
    }
}
