using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HaloFrame;

public class GameManager : GameManagerBase
{
    public static DriverManager DriverManager;
    public static RedDotManager RedDotManager;
    public static Dispatcher Dispatcher;

    protected override void InitManager()
    {
        base.InitManager();

        DriverManager = GetManager<DriverManager>();
        RedDotManager = GetManager<RedDotManager>();
        Dispatcher = GetManager<Dispatcher>();
    }

    /*
    GameManager.RedDotManager.AddValue(redDotItem.Key, 1); 
    上面的调用方式会走Awake方法，然后将GameManager的实例字段instance赋值，但是需要将GameManager挂载在GameObject上
    */
}
