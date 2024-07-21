using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    public enum LayerType
    {
        SceneLayer = 1000, // 用于显示3DUI
        BackgroundLayer = 2000, // 用于显示UI背景，如主界面，黑边图
        NormalLayer = 3000, // 普通UI
        InfoLayer = 4000, // 需要显示在普通UI上面的UI
        TopLayer = 5000, // 顶层UI，如Loading界面
        TipsLayer = 6000, // 全部提示，弹窗UI
        BlackMaskLayer = 7000, // 最上层黑色遮罩
    }

    // unity自带的层级
    public static class Layer
    {
        public const int Default = 0;
        public const int TransparentFX = 1;
        public const int IgnoreRaycast = 2;
        public const int Water = 4;
        public const int UI = 5;
        public const int UIRenderToTarget = 6;
    }

    public enum ResType {
        Dynamic, // 动态实例化，默认
        Static, // 查找节点作为界面资源
    }

    /// <summary>
    /// UI状态
    /// </summary>
    public enum UIState
    {
        None,
        Loading,
        Awake,
        Start,
        Enable,
        Disable,
        Destroy,
        Release,
    }
}
