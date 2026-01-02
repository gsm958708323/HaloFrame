using System;
using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UIConfigSO", order = 1)]
public class UIConfigSO : SerializedScriptableObject
{
    public Dictionary<string, UIConfig> uiConfigs = new();
    private static Dictionary<Type, UIConfig> cache;

    public static void Init()
    {
        var path = "Assets/HaloFrame/Samples/UI/Config/UIConfig.asset";
        var resource = GameManager.Resource.Load(path);
        var so = resource.GetAsset<UIConfigSO>();
        if (so == null)
            return;

        cache = new();
        foreach (var item in so.uiConfigs)
        {
            Type type = AssemblyTools.GetType(item.Key.ToString());
            if (cache.ContainsKey(type))
            {
                Debugger.LogError($"界面类型重复 {type}", LogDomain.UI);
                continue;
            }

            cache.Add(type, item.Value);
        }
    }

    public static Dictionary<Type, UIConfig> GetAll()
    {
        return cache;
    }

    public static UIConfig Get(Type type)
    {
        var all = GetAll();
        if (!all.ContainsKey(type))
        {
            Debugger.LogError($"界面配置不存在 {type}", LogDomain.UI);
            return null;
        }

        return all[type];
    }
}
