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
    public static HotUpdateManger HotUpdate;
    public static DownloadManager Download;

    protected override IEnumerator Start()
    {
        Dispatcher = GetManager<Dispatcher>();
        Download = GetManager<DownloadManager>();
        HotUpdate = GetManager<HotUpdateManger>();
        yield return HotUpdate.ReqRemote();
        HotUpdate.StarHotUpdate(EnterGame);
    }

    void EnterGame()
    {
        Resource = GetManager<ResourceManager>();
        Resource.Init(PathTools.DownloadABPathPrefix, PlayerPrefs.GetInt("IsEditorMode", 1) == 1);

        Driver = GetManager<DriverManager>();
        RedDot = GetManager<RedDotManager>();
        UI = GetManager<UIManager>();
    }

    /*
    GameManager.RedDotManager.AddValue(redDotItem.Key, 1); 
    上面的调用方式会走Awake方法，然后将GameManager的实例字段instance赋值，但是需要将GameManager挂载在GameObject上
    */
}
