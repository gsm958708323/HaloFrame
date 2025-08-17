using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace HaloFrame
{
    [System.Serializable]
    public class BuildItem
    {
        [BoxGroup("打包规则")]
        [FolderPath]
        [LabelText("资源路径")]
        public string assetPath;

        [BoxGroup("打包规则")]
        [LabelText("资源类型")]
        [SerializeField]
        public EResourceType resourceType = EResourceType.Direct;

        [BoxGroup("打包规则")]
        [LabelText("ab粒度类型")]
        [SerializeField]
        public EBundleType bundleType = EBundleType.File;

        [BoxGroup("打包规则")]
        [LabelText("资源后缀(多个后缀用|分割)")]
        [SerializeField]
        public string suffix = ".prefab";

        [HideInInspector]
        public List<string> ignorePaths = new List<string>();

        [HideInInspector]
        public List<string> suffixes = new List<string>();
        /// <summary>
        /// 匹配该打包设置的个数
        /// </summary>
        public int Count { get; set; }
    }
}