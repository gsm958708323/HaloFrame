using UnityEditor;
using System.IO;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;

namespace HaloFrame
{
    public static class Builder
    {
        public static readonly Vector2 collectRuleFileProgress = new Vector2(0, 0.2f);
        private static readonly Vector2 ms_GetDependencyProgress = new Vector2(0.2f, 0.4f);
        private static readonly Vector2 ms_CollectBundleInfoProgress = new Vector2(0.4f, 0.5f);
        private static readonly Vector2 ms_GenerateBuildInfoProgress = new Vector2(0.5f, 0.6f);
        private static readonly Vector2 ms_BuildBundleProgress = new Vector2(0.6f, 0.9f);
        private static readonly Vector2 ms_ClearBundleProgress = new Vector2(0.9f, 1f);

        private static readonly CustomProfiler ms_BuildProfiler = new CustomProfiler(nameof(Builder));
        private static readonly CustomProfiler ms_LoadBuildSettingProfiler = ms_BuildProfiler.CreateChild(nameof(LoadSettingSO));

        private static readonly CustomProfiler ms_CollectProfiler = ms_BuildProfiler.CreateChild(nameof(Collect));
        private static readonly CustomProfiler ms_CollectBuildSettingFileProfiler = ms_CollectProfiler.CreateChild("CollectBuildSettingFile");
        private static readonly CustomProfiler ms_CollectDependencyProfiler = ms_CollectProfiler.CreateChild(nameof(CollectDependency));
        private static readonly CustomProfiler ms_CollectBundleProfiler = ms_CollectProfiler.CreateChild(nameof(CollectBundleSO));
        private static readonly CustomProfiler ms_GenerateManifestProfiler = ms_CollectProfiler.CreateChild(nameof(GenerateResMap));
        private static readonly CustomProfiler ms_BuildBundleProfiler = ms_BuildProfiler.CreateChild(nameof(BuildBundle));
        private static readonly CustomProfiler ms_ClearBundleProfiler = ms_BuildProfiler.CreateChild(nameof(ClearAssetBundle));
        private static string PLATFORM = PathTools.Platform;
        //bundle后缀
        public const string BUNDLE_SUFFIX = ".ab";
        public const string BUNDLE_MANIFEST_SUFFIX = ".manifest";

        public static readonly ParallelOptions ParallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount * 2
        };

        //bundle打包Options
        public readonly static BuildAssetBundleOptions BuildAssetBundleOptions = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

        /// <summary>
        /// ScriptableObject打包设置
        /// </summary>
        public static BuildSettingsSO buildSettingsSO { get; private set; }

        #region Path

        /// <summary>
        /// 打包目录
        /// </summary>
        public static string buildPath;
        /// <summary>
        /// 热更包目录
        /// </summary>
        public static string hotUpdateBuildPath;

        #endregion

        /// <summary>
        /// 加载ScriptableObject打包配置
        /// </summary>
        /// <param name="settingPath">打包配置资源路径</param>
        private static BuildSettingsSO LoadSettingSO(string settingPath)
        {
            buildSettingsSO = AssetDatabase.LoadAssetAtPath<BuildSettingsSO>(settingPath);
            if (buildSettingsSO == null)
            {
                throw new Exception($"Load buildSettingsSO failed,SettingPath:{settingPath}.");
            }

            buildPath = PathTools.Combine(buildSettingsSO.buildRoot, PLATFORM);
            hotUpdateBuildPath = PathTools.Combine(buildPath, $"{PathTools.HotUpdateDir}_{buildSettingsSO.version}/");

            return buildSettingsSO;
        }

        public static void Build()
        {
            ms_BuildProfiler.Start();

            ms_LoadBuildSettingProfiler.Start();
            buildSettingsSO = LoadSettingSO(PathTools.BuildSettingPath);
            ms_LoadBuildSettingProfiler.Stop();

            //搜集bundle信息
            ms_CollectProfiler.Start();
            Dictionary<string, List<string>> bundleDic = Collect();
            ms_CollectProfiler.Stop();

            //打包assetbundle
            ms_BuildBundleProfiler.Start();
            BuildBundle(hotUpdateBuildPath, bundleDic);
            ms_BuildBundleProfiler.Stop();

            //清空多余文件
            ms_ClearBundleProfiler.Start();
            ClearAssetBundle(hotUpdateBuildPath, bundleDic);
            ms_ClearBundleProfiler.Stop();

            EditorUtility.ClearProgressBar();

            ms_BuildProfiler.Stop();

            Debug.Log($"打包完成{ms_BuildProfiler}");
        }

        public static void BuildUpdate()
        {
            var path = PathTools.Combine(buildSettingsSO.buildRoot, PathTools.AssetMapFile);
            if (!File.Exists(path))
            {
                Debugger.LogError("未生成资源文件，请先打包游戏！");
                return;
            }


            ms_BuildProfiler.Start();
            ms_LoadBuildSettingProfiler.Start();
            buildSettingsSO = LoadSettingSO(PathTools.BuildSettingPath);
            ms_LoadBuildSettingProfiler.Stop();

            //搜集bundle信息
            ms_CollectProfiler.Start();
            Dictionary<string, List<string>> bundleDic = Collect(true);
            ms_CollectProfiler.Stop();

            //打包assetbundle
            ms_BuildBundleProfiler.Start();
            BuildBundle(hotUpdateBuildPath, bundleDic);
            ms_BuildBundleProfiler.Stop();

            //清空多余文件
            ms_ClearBundleProfiler.Start();
            ClearAssetBundle(hotUpdateBuildPath, bundleDic);
            ms_ClearBundleProfiler.Stop();

            EditorUtility.ClearProgressBar();

            ms_BuildProfiler.Stop();

            Debug.Log($"热更包构建完成{ms_BuildProfiler}");
        }

        /// <summary>
        /// 搜集打包bundle的信息
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, List<string>> Collect(bool buildHotUpdate = false)
        {
            //获取所有在打包设置的文件列表
            ms_CollectBuildSettingFileProfiler.Start();
            HashSet<string> files = buildSettingsSO.Collect();
            ms_CollectBuildSettingFileProfiler.Stop();

            //搜集所有文件的依赖关系
            ms_CollectDependencyProfiler.Start();
            Dictionary<string, List<string>> dependencyDic = CollectDependency(files);
            ms_CollectDependencyProfiler.Stop();

            //标记所有资源的信息
            Dictionary<string, EResourceType> assetDic = new Dictionary<string, EResourceType>();

            //被打包配置分析到的直接设置为Direct
            foreach (string url in files)
            {
                assetDic.Add(url, EResourceType.Direct);
            }

            //依赖的资源标记为Dependency，已经存在的说明是Direct的资源
            foreach (string url in dependencyDic.Keys)
            {
                if (!assetDic.ContainsKey(url))
                {
                    assetDic.Add(url, EResourceType.Dependency);
                }
            }

            //该字典保存bundle对应的资源集合
            ms_CollectBundleProfiler.Start();
            //此bundle被哪些资源使用
            Dictionary<string, List<string>> bundleDic = CollectBundleSO(buildSettingsSO, assetDic, dependencyDic);
            ms_CollectBundleProfiler.Stop();

            //生成资源映射文件
            ms_GenerateManifestProfiler.Start();
            GenerateResMap(assetDic, bundleDic, dependencyDic, buildHotUpdate);
            ms_GenerateManifestProfiler.Stop();

            return bundleDic;
        }

        /// <summary>
        /// 收集指定文件集合所有的依赖信息
        /// </summary>
        /// <param name="files">文件集合</param>
        /// <returns>依赖信息</returns>
        private static Dictionary<string, List<string>> CollectDependency(ICollection<string> files)
        {
            float min = ms_GetDependencyProgress.x;
            float max = ms_GetDependencyProgress.y;

            Dictionary<string, List<string>> dependencyDic = new Dictionary<string, List<string>>();

            //声明fileList后，就不需要递归了
            List<string> fileList = new List<string>(files);

            for (int i = 0; i < fileList.Count; i++)
            {
                string assetUrl = fileList[i];

                if (dependencyDic.ContainsKey(assetUrl))
                    continue;

                if (i % 10 == 0)
                {
                    //只能大概模拟进度
                    float progress = min + (max - min) * ((float)i / (files.Count * 3));
                    EditorUtility.DisplayProgressBar($"{nameof(CollectDependency)}", "搜集依赖信息", progress);
                }

                string[] dependencies = AssetDatabase.GetDependencies(assetUrl, false);
                List<string> dependencyList = new List<string>(dependencies.Length);

                //过滤掉不符合要求的asset
                for (int ii = 0; ii < dependencies.Length; ii++)
                {
                    string tempAssetUrl = dependencies[ii];
                    string extension = Path.GetExtension(tempAssetUrl).ToLower();
                    if (string.IsNullOrEmpty(extension) || extension == ".cs" || extension == ".dll")
                        continue;
                    dependencyList.Add(tempAssetUrl);
                    if (!fileList.Contains(tempAssetUrl))
                        fileList.Add(tempAssetUrl);
                }

                dependencyDic.Add(assetUrl, dependencyList);
            }

            return dependencyDic;
        }


        /// <summary>
        /// 搜集bundle对应的ab名字
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="assetDic">资源列表</param>
        /// <param name="dependencyDic">资源依赖信息</param>
        /// <returns>一个bundle被哪些资源使用（bundleUrl: AssetUrlList）</returns>
        private static Dictionary<string, List<string>> CollectBundleSO(BuildSettingsSO buildSettings, Dictionary<string, EResourceType> assetDic, Dictionary<string, List<string>> dependencyDic)
        {
            float min = ms_CollectBundleInfoProgress.x;
            float max = ms_CollectBundleInfoProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(CollectBundleSO)}", "搜集bundle信息", min);

            Dictionary<string, List<string>> bundleDic = new Dictionary<string, List<string>>();
            //外部资源
            List<string> notInRuleList = new List<string>();

            int index = 0;
            foreach (KeyValuePair<string, EResourceType> pair in assetDic)
            {
                index++;
                string assetUrl = pair.Key;
                string bundleName = buildSettings.GetBundleName(assetUrl, pair.Value);

                //没有bundleName的资源为外部资源
                if (bundleName == null)
                {
                    notInRuleList.Add(assetUrl);
                    continue;
                }

                List<string> list;
                if (!bundleDic.TryGetValue(bundleName, out list))
                {
                    list = new List<string>();
                    bundleDic.Add(bundleName, list);
                }

                list.Add(assetUrl);

                EditorUtility.DisplayProgressBar($"{nameof(CollectBundleSO)}", "搜集bundle信息", min + (max - min) * ((float)index / assetDic.Count));
            }

            //todo...  外部资源
            if (notInRuleList.Count > 0)
            {
                string massage = string.Empty;
                for (int i = 0; i < notInRuleList.Count; i++)
                {
                    massage += "\n" + notInRuleList[i];
                }
                EditorUtility.ClearProgressBar();
                throw new Exception($"资源不在打包规则,或者后缀不匹配！！！{massage}");
            }

            //排序
            foreach (List<string> list in bundleDic.Values)
            {
                list.Sort();
            }

            return bundleDic;
        }


        /// <summary>
        /// 生成资源描述文件
        /// <param name="assetDic">资源列表</param>
        /// <param name="bundleDic">bundle包信息</param>
        /// <param name="dependencyDic">资源依赖信息</param>
        /// </summary>
        private static void GenerateResMap(Dictionary<string, EResourceType> assetDic, Dictionary<string, List<string>> bundleDic, Dictionary<string, List<string>> dependencyDic, bool buildHotUpdate = false)
        {
            float min = ms_GenerateBuildInfoProgress.x;
            float max = ms_GenerateBuildInfoProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(GenerateResMap)}", "生成打包信息", min);

            var resUrl2AB = new Dictionary<string, string>();
            foreach (var item1 in bundleDic)
            {
                foreach (var resUrl in item1.Value)
                {
                    resUrl2AB.Add(resUrl, item1.Key);
                }
            }
            var assetMap = new Dictionary<string, AssetInfo>();
            foreach (var item in assetDic)
            {
                var resName = item.Key;
                resUrl2AB.TryGetValue(resName, out var abName);
                dependencyDic.TryGetValue(resName, out var dependency);
                var info = new AssetInfo
                {
                    ResUrl = resName,
                    ABUrl = abName,
                    Dependency = dependency,
                    Version = buildSettingsSO.version,
                    Md5 = FileTools.CreateMd5ForFile(resName),
                    Size = FileTools.GetFileSize(resName)
                };
                assetMap.Add(resName, info);
            }
            UpdateResMap(assetMap, buildHotUpdate);


            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar($"{nameof(GenerateResMap)}", "生成打包信息", max);

            EditorUtility.ClearProgressBar();
        }

        public static void UpdateResMap(Dictionary<string, AssetInfo> assetMap, bool buildHotUpdate = false)
        {
            if (!buildHotUpdate)
            {
                var assetMapJson = JsonTools.ToJson(assetMap);
                FileTools.SafeWriteAllText(PathTools.Combine(buildPath, PathTools.AssetMapFile), assetMapJson);
            }
            else
            {
                var path = PathTools.Combine(buildPath, PathTools.AssetMapFile);
                var oldAssetMap = JsonTools.ToObject<Dictionary<string, AssetInfo>>(FileTools.SafeReadAllText(path));
                foreach (var mapItem in assetMap)
                {
                    oldAssetMap.TryGetValue(mapItem.Key, out var oldAssetInfo);
                    var assetInfo = mapItem.Value;
                    if (oldAssetInfo is null || oldAssetInfo.Md5 != assetInfo.Md5)
                    {
                        // 新增或md5不一致则需要热更
                        assetInfo.Update = true;
                    }
                }

                var assetMapJson = JsonTools.ToJson(assetMap);
                FileTools.SafeWriteAllText(PathTools.Combine(buildPath, PathTools.AssetMapFile), assetMapJson);
            }
        }

        /// <summary>
        /// 打包AssetBundle
        /// <param name="assetDic">资源列表</param>
        /// <param name="bundleDic">bundle包信息</param>
        /// <param name="dependencyDic">资源依赖信息</param>
        /// </summary>
        private static AssetBundleManifest BuildBundle(string buildPath, Dictionary<string, List<string>> bundleDic)
        {
            float min = ms_BuildBundleProgress.x;
            float max = ms_BuildBundleProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(BuildBundle)}", "打包AssetBundle", min);

            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);

            // 此接口的路径最后必须以'/'结尾，否则不会生成ab文件
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildPath, GetBuilds(bundleDic), BuildAssetBundleOptions, EditorUserBuildSettings.activeBuildTarget);

            EditorUtility.DisplayProgressBar($"{nameof(BuildBundle)}", "打包AssetBundle", max);

            return manifest;
        }

        /// <summary>
        /// 获取所有需要打包的AssetBundleBuild
        /// </summary>
        /// <param name="bundleTable">bunlde信息</param>
        /// <returns></returns>
        private static AssetBundleBuild[] GetBuilds(Dictionary<string, List<string>> bundleTable)
        {
            int index = 0;
            AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[bundleTable.Count];
            foreach (KeyValuePair<string, List<string>> pair in bundleTable)
            {
                assetBundleBuilds[index++] = new AssetBundleBuild()
                {
                    assetBundleName = pair.Key,
                    assetNames = pair.Value.ToArray(),
                };
            }

            return assetBundleBuilds;
        }

        /// <summary>
        /// 清空多余的assetbundle，没有在bundleDict中的文件都会被删除掉
        /// </summary>
        /// <param name="path">打包路径</param>
        /// <param name="bundleDic"></param>
        private static void ClearAssetBundle(string path, Dictionary<string, List<string>> bundleDic)
        {
            float min = ms_ClearBundleProgress.x;
            float max = ms_ClearBundleProgress.y;

            EditorUtility.DisplayProgressBar($"{nameof(ClearAssetBundle)}", "清除多余的AssetBundle文件", min);
            RenameMainFile(path);
            List<string> fileList = GetFiles(path, null, null);
            HashSet<string> fileSet = new HashSet<string>(fileList);

            foreach (string bundle in bundleDic.Keys)
            {
                fileSet.Remove($"{path}{bundle}");
                fileSet.Remove($"{path}{bundle}{BUNDLE_MANIFEST_SUFFIX}");
            }

            fileSet.Remove($"{path}{Path.GetFileNameWithoutExtension(PathTools.MainManifestFile)}");
            fileSet.Remove($"{path}{PathTools.MainManifestFile}");

            Parallel.ForEach(fileSet, ParallelOptions, File.Delete);

            EditorUtility.DisplayProgressBar($"{nameof(ClearAssetBundle)}", "清除多余的AssetBundle文件", max);
        }

        /// <summary>
        /// mainManifest文件会根据文件夹自动命名，这里修改
        /// </summary>
        /// <param name="path"></param>
        static void RenameMainFile(string path)
        {
            var prefix = Path.GetFileNameWithoutExtension(PathTools.MainManifestFile);
            var suffix = Path.GetExtension(PathTools.MainManifestFile);

            var dirName = $"{PathTools.HotUpdateDir}_{buildSettingsSO.version}";
            var file1 = Path.Combine(path, dirName);
            FileTools.SafeRenameFile(file1, Path.Combine(path, prefix));

            var file2 = Path.Combine(path, dirName + suffix);
            FileTools.SafeRenameFile(file2, Path.Combine(path, PathTools.MainManifestFile));
        }

        /// <summary>
        /// 获取指定路径的文件
        /// </summary>
        /// <param name="path">指定路径</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffixes">后缀集合</param>
        /// <returns>文件列表</returns>
        public static List<string> GetFiles(string path, string prefix, params string[] suffixes)
        {
            string[] files = Directory.GetFiles(path, $"*.*", SearchOption.AllDirectories);
            List<string> result = new List<string>(files.Length);

            for (int i = 0; i < files.Length; ++i)
            {
                string file = files[i].Replace('\\', '/');

                if (prefix != null && !file.StartsWith(prefix, StringComparison.InvariantCulture))
                {
                    continue;
                }

                if (suffixes != null && suffixes.Length > 0)
                {
                    bool exist = false;

                    for (int ii = 0; ii < suffixes.Length; ii++)
                    {
                        string suffix = suffixes[ii];
                        if (file.EndsWith(suffix, StringComparison.InvariantCulture))
                        {
                            exist = true;
                            break;
                        }
                    }

                    if (!exist)
                        continue;
                }

                result.Add(file);
            }

            return result;
        }
    }
}
