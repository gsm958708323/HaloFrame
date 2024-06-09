using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaloFrame;

public class GameManager : GameManagerBase
{
    public static DriverManager DriverManager;
    public static RedDotManager RedDotManager;
    public static Dispatcher Dispatcher;
    public static UIManager UIManager;
    public static ResourceManager ResourceManager;

    protected override void InitManager()
    {
        base.InitManager();

        Dispatcher = GetManager<Dispatcher>();
        ResourceManager = GetManager<ResourceManager>();
        DriverManager = GetManager<DriverManager>();
        RedDotManager = GetManager<RedDotManager>();
        UIManager = GetManager<UIManager>();
    }

    /*
    GameManager.RedDotManager.AddValue(redDotItem.Key, 1); 
    上面的调用方式会走Awake方法，然后将GameManager的实例字段instance赋值，但是需要将GameManager挂载在GameObject上
    */
}
