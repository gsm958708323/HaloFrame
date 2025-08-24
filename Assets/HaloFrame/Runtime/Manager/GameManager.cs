using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaloFrame;
using System.IO;

public class GameManager : GameManagerBase
{
    public static DriverManager Driver;
    public static RedDotManager RedDot;
    public static Dispatcher Dispatcher;
    public static UIManager UI;
    public static ResourceManager Resource;

    protected override void InitManager()
    {
        base.InitManager();

        Dispatcher = GetManager<Dispatcher>();
        Resource = GetManager<ResourceManager>();
        InitResource();
        Driver = GetManager<DriverManager>();
        RedDot = GetManager<RedDotManager>();
        UI = GetManager<UIManager>();
    }

    private string prefixPath;
    void InitResource()
    {
        string platform = PathTools.Platform;
        // todo 改成沙盒目录
        prefixPath = @"D:\Work\Blog\HaloFrame\Build\AssetBunlde".Replace("\\", "/");
        prefixPath += "/" + platform;
        bool isEditorMode = PlayerPrefs.GetInt("IsEditorMode", 1) == 1;
        Resource.Init(platform, GetFileUrl, isEditorMode);
    }

    private string GetFileUrl(string url)
    {
        return $"{prefixPath}/{url}";
    }

    /*
    GameManager.RedDotManager.AddValue(redDotItem.Key, 1); 
    上面的调用方式会走Awake方法，然后将GameManager的实例字段instance赋值，但是需要将GameManager挂载在GameObject上
    */
}
