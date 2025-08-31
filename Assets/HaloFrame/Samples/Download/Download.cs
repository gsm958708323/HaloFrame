using System.Collections;
using System.Collections.Generic;
using System.IO;
using HaloFrame;
using UnityEngine;

public class Download : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var download = GameManager.Download.CreateDownloader("Test");
        // 设置热更下载器回调
        download.OnDownloadSuccess += (eventArgs) =>
        {
            Debugger.Log($"获取热更资源完成！：{eventArgs.DownloadInfo.DownloadUrl}");
        };
        download.OnDownloadFailure += (eventArgs) =>
        {
            Debugger.LogError($"获取热更资源失败。：{eventArgs.DownloadInfo.DownloadUrl}\n{eventArgs.ErrorMessage}");
        };
        download.OnDownloadStart += (eventArgs) =>
        {
            Debugger.Log($"开始获取热更资源...：{eventArgs.DownloadInfo.DownloadUrl}");
        };
        download.OnDownloadOverallProgress += (eventArgs) =>
        {
            float currentTaskIndex = (float)eventArgs.CurrentDownloadTaskIndex;
            float taskCount = (float)eventArgs.DownloadTaskCount;

            // 计算进度百分比
            float progress = currentTaskIndex / taskCount * 100f;
            Debugger.Log($"下载进度：{progress}");
        };
        download.OnAllDownloadTaskCompleted += (eventArgs) =>
        {
            Debugger.Log($"所有热更资源获取完成！，用时：{eventArgs.TimeSpan}");
        };


        var file = "assets/haloframe/samples/ui/prefabs/buttonitem.prefab.ab";
        var path1 = "http://127.0.0.1:8088/AssetBunlde";
        var version = "HotUpdate_1.0.0";
        var url = PathTools.Combine(path1, PathTools.Platform, version, file);

        var downPath = PathTools.Combine(Application.persistentDataPath, "HotUpdate", file);
        Debugger.Log($"下载位置：{downPath}");
        download.AddDownloadTask(url, downPath);
        download.LaunchDownload();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
