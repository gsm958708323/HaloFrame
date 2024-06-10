using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace HaloFrame
{
    public class UIManager : IManager
    {
        private Dictionary<UIViewType, UIViewCtrl> ctrlDict;
        private Dictionary<UILayerType, UILayer> layerDict;
        private HashSet<UIViewType> openSet;
        private Camera worldCamera, uiCamera;
        public int width = 1080;
        public int height = 1920;

        public override void Init()
        {
            base.Init();
            ctrlDict = new();
            layerDict = new();
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
                Canvas canvas = UIExtension.CreateLayerCanvas(layer, is3d, root.transform, is3d ? worldCamera : uiCamera, width, height);
                var uILayer = UILayer.Get(layer, canvas);
                layerDict.Add(layer, uILayer);
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            // 初始化界面配置 todo 优化反射？在编辑器上生成
            var configList = UIConfigSO.Get();
            foreach (var item in configList)
            {
                if (ctrlDict.ContainsKey(item.ViewType))
                {
                    Debugger.LogError($"界面类型重复 {item.ViewType}", LogDomain.UI);
                    continue;
                }
                if (!layerDict.ContainsKey(item.LayerType))
                {
                    Debugger.LogError($"界面层级不存在 {item.LayerType}", LogDomain.UI);
                    continue;
                }

                Type type = assembly.GetType(item.ViewType.ToString());
                if (type == null)
                {
                    Debugger.LogError($"界面对象不存在 {item.ViewType}", LogDomain.UI);
                    continue;
                }
                // todo 只有UIView能够复用，但是有内存泄漏的风险，考虑去掉？
                UIViewCtrl viewCtrl = UIViewCtrl.Get(item, UIView.Get(type), layerDict[item.LayerType]);
                ctrlDict.Add(item.ViewType, viewCtrl);
            }
        }

        public void Open(UIViewType type, object data = null, Action action = null)
        {
            if (!ctrlDict.ContainsKey(type))
            {
                Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
                return;
            }

            openSet.Add(type);
            ctrlDict[type].Open(data, action);
        }

        public void Close(UIViewType type, Action action = null)
        {
            if (!ctrlDict.ContainsKey(type))
            {
                Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
                return;
            }

            openSet.Remove(type);
            ctrlDict[type].Close(action);
        }
    }
}
