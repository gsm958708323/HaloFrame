using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.IO;
using System;
using UnityEditor;

namespace HaloFrame
{
    [CreateAssetMenu(fileName = "BuildSettings", menuName = "HaloFrame/Build Settings", order = 1)]
    public class BuildSettingsSO : SerializedScriptableObject
    {
        [BoxGroup("基本设置")]
        [LabelText("项目名称")]
        public string projectName = "HaloFrame";

        [BoxGroup("基本设置")]
        [LabelText("版本号")]
        public string version = "1.0.0";

        [BoxGroup("基本设置")]
        [FolderPath]
        [LabelText("打包文件的目标文件夹")]
        public string buildRoot = "Assets/AssetBundle";


        [BoxGroup("热更新设置", ShowLabel = true)]
        [LabelText("热更新地址")]
        public string remoteAddress = "http://127.0.0.1:8080";

        [HorizontalGroup("热更新设置/选项")]
        [VerticalGroup("热更新设置/选项/左")]
        [LabelText("启用热更新")]
        public bool openHotUpdate = true;

        [VerticalGroup("热更新设置/选项/右")]
        [LabelText("启用分包")]
        public bool enablePackage = false;

        [BoxGroup("打包配置", ShowLabel = true)]
        [ListDrawerSettings(ShowFoldout = true, ShowItemCount = true, DraggableItems = true)]
        [LabelText("打包规则")]
        public List<BuildItem> items = new List<BuildItem>();

        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第一行")]
        [Button("根据当前平台打包游戏")]
        public void Build()
        {
            Builder.Build();
        }

        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第一行")]
        [Button("构建热更包")]
        public void BuildHot()
        {
            Builder.BuildUpdate();
        }

        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第二行")]
        [Button("打开输出目录")]
        public void OpenOutPath()
        {
            System.Diagnostics.Process.Start(buildRoot);
        }

        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第二行")]
        [Button("打开沙盒目录")]
        public void OpenPersistentPath()
        {
            System.Diagnostics.Process.Start(Application.persistentDataPath);
        }

        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第二行")]
        [Button("删除输出目录")]
        public void DeleteOutPath()
        {
            FileTools.SafeDeleteDir(buildRoot);
        }
        [BoxGroup("按钮")]
        [HorizontalGroup("按钮/第二行")]
        [Button("删除沙盒目录")]
        public void DeletePersistentPath()
        {
            FileTools.SafeDeleteDir(Application.persistentDataPath);
        }

        [HideInInspector]
        public Dictionary<string, BuildItem> itemDic = new();
        public void Init()
        {
            buildRoot = Path.GetFullPath(buildRoot).Replace("\\", "/");

            itemDic.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem = items[i];

                if (buildItem.bundleType == EBundleType.Rule || buildItem.bundleType == EBundleType.Directory)
                {
                    if (!Directory.Exists(buildItem.assetPath))
                    {
                        throw new Exception($"不存在资源路径:{buildItem.assetPath}");
                    }
                }

                //处理后缀
                string[] prefixes = buildItem.suffix.Split('|');
                for (int ii = 0; ii < prefixes.Length; ii++)
                {
                    string prefix = prefixes[ii].Trim();
                    if (!string.IsNullOrEmpty(prefix))
                        buildItem.suffixes.Add(prefix);
                }

                if (itemDic.ContainsKey(buildItem.assetPath))
                {
                    throw new Exception($"重复的资源路径:{buildItem.assetPath}");
                }
                itemDic.Add(buildItem.assetPath, buildItem);
            }
        }

        /// <summary>
        /// 根据规则自动搜集所有文件
        /// </summary>
        /// <returns></returns>
        internal HashSet<string> Collect()
        {
            float min = Builder.collectRuleFileProgress.x;
            float max = Builder.collectRuleFileProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源", min);

            //规则1 Assets/Resources，规则2 Assets/Resources/UI，规则2是规则1的子目录，规则1在搜集文件时会忽略规则2，防止资源重复打包
            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem_i = items[i];

                if (buildItem_i.resourceType != EResourceType.Direct)
                    continue;

                buildItem_i.ignorePaths.Clear();
                for (int j = 0; j < items.Count; j++)
                {
                    BuildItem buildItem_j = items[j];
                    if (i != j && buildItem_j.resourceType == EResourceType.Direct)
                    {
                        // 如果j是i的子目录，则i把j添加到忽略列表中
                        if (buildItem_j.assetPath.StartsWith(buildItem_i.assetPath, StringComparison.InvariantCulture))
                        {
                            buildItem_i.ignorePaths.Add(buildItem_j.assetPath);
                        }
                    }
                }
            }

            //存储被规则分析到的所有文件
            HashSet<string> files = new HashSet<string>();

            for (int i = 0; i < items.Count; i++)
            {
                BuildItem buildItem = items[i];

                EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包规则资源", min + (max - min) * ((float)i / (items.Count - 1)));

                if (buildItem.resourceType != EResourceType.Direct)
                    continue;

                List<string> tempFiles = Builder.GetFiles(buildItem.assetPath, null, buildItem.suffixes.ToArray());
                for (int j = 0; j < tempFiles.Count; j++)
                {
                    string file = tempFiles[j];

                    //过滤被忽略的
                    if (IsIgnore(buildItem.ignorePaths, file))
                        continue;

                    files.Add(file);
                }

                EditorUtility.DisplayProgressBar($"{nameof(Collect)}", "搜集打包设置资源", (float)(i + 1) / items.Count);
            }

            return files;
        }

        /// <summary>
        /// 文件是否在忽略列表
        /// </summary>
        /// <param name="ignoreList">忽略路径列表</param>
        /// <param name="file">文件路径</param>
        /// <returns></returns>
        public bool IsIgnore(List<string> ignoreList, string file)
        {
            for (int i = 0; i < ignoreList.Count; i++)
            {
                string ignorePath = ignoreList[i];
                if (string.IsNullOrEmpty(ignorePath))
                    continue;
                if (file.StartsWith(ignorePath, StringComparison.InvariantCulture))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// 通过资源获取打包选项
        /// </summary>
        /// <param name="assetUrl">资源路径</param>
        /// <returns>打包选项</returns>
        public BuildItem GetBuildItem(string assetUrl)
        {
            BuildItem item = null;
            for (int i = 0; i < items.Count; ++i)
            {
                BuildItem tempItem = items[i];
                //前面是否匹配
                if (assetUrl.StartsWith(tempItem.assetPath, StringComparison.InvariantCulture))
                {
                    //找到优先级最高的Rule,路径越长说明优先级越高
                    if (item == null || item.assetPath.Length < tempItem.assetPath.Length)
                    {
                        item = tempItem;
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// 获取BundleName
        /// </summary>
        /// <param name="assetUrl">资源路径</param>
        /// <param name="resourceType">资源类型</param>
        /// <returns>BundleName</returns>
        public string GetBundleName(string assetUrl, EResourceType resourceType)
        {
            BuildItem buildItem = GetBuildItem(assetUrl);

            if (buildItem is null)
            {
                return null;
            }

            //依赖类型一定要匹配后缀
            if (resourceType == EResourceType.Dependency)
            {
                string extension = Path.GetExtension(assetUrl).ToLower();
                bool exist = false;
                for (int i = 0; i < buildItem.suffixes.Count; i++)
                {
                    if (buildItem.suffixes[i] == extension)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    return null;
                }
            }

            string name;
            switch (buildItem.bundleType)
            {
                case EBundleType.Rule:
                    name = buildItem.assetPath;
                    if (buildItem.assetPath[buildItem.assetPath.Length - 1] == '/')
                        name = buildItem.assetPath.Substring(0, buildItem.assetPath.Length - 1);
                    name = $"{name}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                case EBundleType.Directory:
                    name = $"{assetUrl.Substring(0, assetUrl.LastIndexOf('/'))}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                case EBundleType.File:
                    name = $"{assetUrl}{Builder.BUNDLE_SUFFIX}".ToLowerInvariant();
                    break;
                default:
                    throw new Exception($"无法获取{assetUrl}的BundleName");
            }

            buildItem.Count += 1;

            return name;
        }

    }
}