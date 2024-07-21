/*******************************************************
** auth:  https://github.com/gsm958708323
** date:  2024/07/20 22:29:33
** dsec:  UIConfig 
界面配置
界面关联了子界面，则会把子界面统一管理
界面A可以关联子界面SubA，界面B也可以管理SubA
*******************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaloFrame
{
    [System.Serializable]
    public class UIConfig
    {
        /// <summary>
        /// 界面唯一id
        /// </summary>
        public ViewType ViewType;
        /// <summary>
        /// 界面所属层级
        /// </summary>
        public LayerType LayerType;
        /// <summary>
        /// 界面生成方式
        /// </summary>
        public ResType ResType = ResType.Dynamic;
        /// <summary>
        /// 是弹窗还是全屏界面
        /// </summary>
        public bool IsPopup; // 是否为弹窗
        /// <summary>
        /// 预制体路径
        /// </summary>
        public string Prefab;
        public List<ViewType> ChildList;
        public bool LoadWithParent = false;
    }
}
