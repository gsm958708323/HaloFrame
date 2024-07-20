using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace HaloFrame
{
    public class UIManager : IManager
    {
        /// <summary>
        /// 存储UI界面对应的类
        /// </summary>
        private Dictionary<UIViewType, Type> uiDict;
        private Dictionary<UILayerType, UILayer> layerDict;
        private HashSet<UIViewType> openSet;
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
            var layers = Enum.GetValues(typeof(UILayerType));
            foreach (UILayerType layer in layers)
            {
                bool is3d = layer == UILayerType.SceneLayer;
                Canvas canvas = UIExtension.CreateLayerCanvas(layer, is3d, root.transform, is3d ? worldCamera : uiCamera, UIDefine.WIDTH, UIDefine.HEIGHT);
                var uILayer = new UILayer(layer, canvas);
                layerDict.Add(layer, uILayer);
            }

            // 根据脚本名称找到对应界面类
            Assembly assembly = Assembly.GetExecutingAssembly();
            var configDict = UIConfigSO.Get();
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

        public void Open(UIViewType type, Action action = null, params object[] args)
        {
            var configDict = UIConfigSO.Get();
            if (!configDict.TryGetValue(type, out var config))
            {
                Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
                return;
            }
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

        public void Close(UIViewType type)
        {
            var configDict = UIConfigSO.Get();
            if (!configDict.TryGetValue(type, out var config))
            {
                Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
                return;
            }
            if (!layerDict.TryGetValue(config.LayerType, out var layer))
            {
                Debugger.LogError($"界面层级不存在 {config.LayerType}", LogDomain.UI);
                return;
            }

            layer.Pop();
        }

        internal UIView CreateUI(UIViewType type, UILayer layer)
        {
            if (!uiDict.TryGetValue(type, out var viewType))
            {
                Debugger.LogError($"界面对象不存在 {type}", LogDomain.UI);
                return null;

            }
            var configDict = UIConfigSO.Get();
            if (!configDict.TryGetValue(type, out var config))
            {
                Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
                return null;
            }

            var view = (UIView)Activator.CreateInstance(viewType);
            view.Bind(config, layer);
            // todo 子界面
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

        }

        private Task LoadUITask(UIView view)
        {
            if (view.UIState == UIState.None)
            {
                LoadAsset(view);
            }
            return view.TaskResult.Task;
        }

        private void LoadAsset(UIView view)
        {
            // todo 支持两种加载方式：动态加载，代码指定Gameobject
            view.UIState = UIState.Loading;
            GameObject prefab = GameManager.Resource.LoadAsset<GameObject>(view.UIConfig.Prefab);
            var go = GameObject.Instantiate(prefab);
            view.LoadAsset(go);

            if (view.TaskResult != null)
            {
                view.TaskResult.SetResult(true);
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
