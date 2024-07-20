using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaloFrame;

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
        Driver = GetManager<DriverManager>();
        RedDot = GetManager<RedDotManager>();
        UI = GetManager<UIManager>();
    }

    /*
    GameManager.RedDotManager.AddValue(redDotItem.Key, 1); 
    上面的调用方式会走Awake方法，然后将GameManager的实例字段instance赋值，但是需要将GameManager挂载在GameObject上
    */
}
