using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace HaloFrame
{
    public class UIManager : IManager
    {
        private Dictionary<LayerType, UILayer> layerDict;
        private Camera worldCamera, uiCamera;
        public int width = 1920;
        public int height = 1080;

        public override void Init()
        {
            base.Init();
            layerDict = new();

            InitConfig();
            InitUI();
        }

        private void InitConfig()
        {
            ResConfigSO.Init();
            UIConfigSO.Init();
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
        }

        public void Open<T>(Action action = null, params object[] args)
        {
            var type = typeof(T);
            var config = UIConfigSO.Get(type);
            if (config == null)
                return;
            if (!layerDict.TryGetValue(config.LayerType, out var layer))
            {
                Debugger.LogError($"界面层级不存在 {config.LayerType}", LogDomain.UI);
                return;
            }

            layer.Open(type, action, args);
        }

        public void Close(Type type)
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

        internal UIView CreateUI(Type type, UILayer layer)
        {
            var config = UIConfigSO.Get(type);
            if (config == null)
                return null;

            var view = (UIGameView)AssemblyTools.CreateInstance(type);
            view.Bind(config, layer);

            if (config.ChildList != null)
            {
                foreach (var name in config.ChildList)
                {
                    var subType = AssemblyTools.GetType(name);
                    if (subType == null)
                        continue;

                    var subConfig = UIConfigSO.Get(subType);
                    if (subConfig == null)
                        continue;

                    UISubView subView = (UISubView)AssemblyTools.CreateInstance(subType);
                    subView.Bind(subConfig, layer);
                    view.AddChild(subType, subView);
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
                foreach (var name in view.UIConfig.ChildList)
                {
                    var child = gameView.FindChild(name);
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
                view.UIState = UIState.Loading;
                LoadAsset(view);
            }
            return view.LoadTask.Task; 
        }

        private async void LoadAsset(UIView view)
        {
            if (view is UISubView subView)
            {
                // 支持两种加载方式：动态加载 | 查找节点
                GameObject childGo;
                Transform parent;
                if (subView.ResType == ResType.Dynamic)
                {
                    var awaiter = GameManager.Resource.LoadWithAwaiter(subView.UIConfig.ResId);
                    await awaiter;
                    childGo = awaiter.GetResult().Instantiate(true);
                    parent = subView.UILayer.Canvas.transform;
                }
                else
                {
                    if (subView.Parent == null)
                    {
                        Debugger.LogError($"子界面没有Parent {subView}");
                        return;
                    }
                    childGo = subView.Parent.gameObject.FindEx(subView.ToString());
                    parent = subView.Parent.UILayer.Canvas.transform;
                }

                if (childGo == null)
                {
                    Debugger.LogError($"子节点不存在 {subView}");
                    return;
                }
                subView.OnLoadAsset(childGo, parent);
            }
            else if (view is UIGameView gameView)
            {
                if (view.UIConfig.ResId == 0)
                {
                    Debugger.LogError($"界面的ResId为0 {gameView}", LogDomain.UI);
                    return;
                }
                var awaiter = GameManager.Resource.LoadWithAwaiter(view.UIConfig.ResId);
                await awaiter;
                var go = awaiter.GetResult().Instantiate(true);
                var parent = view.UILayer.Canvas.transform;
                view.OnLoadAsset(go, parent);
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

            if (view.gameObject != null)
            {
                // AutoUnload会自动卸载 
                GameObject.Destroy(view.gameObject);
            }
        }
    }
}
