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

namespace HaloFrame
{
    [System.Serializable]
    public class UIConfig
    {
        /// <summary>
        /// 界面所属层级
        /// </summary>
        public LayerType LayerType;
        /// <summary>
        /// 是弹窗还是全屏界面
        /// </summary>
        public bool IsPopup;
        public int ResId;
        public List<string> ChildList;
        public bool LoadWithParent = false;
    }

    [System.Serializable]
    public class ResConfig
    {
        public int ResId;
        public string ResPath;
    }
}
