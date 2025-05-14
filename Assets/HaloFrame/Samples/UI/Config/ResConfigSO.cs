using System;
using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

[CreateAssetMenu(fileName = "ResConfig", menuName = "UI/ResConfig", order = 1)]
public class ResConfigSO : ScriptableObject
{
    public List<ResConfig> resConfigs;
    private static Dictionary<int, ResConfig> cache;

    public static void Init()
    {
        var path = "Assets/HaloFrame/Samples/UI/Config/ResConfig.asset";
        var resource = GameManager.Resource.Load(path);
        var so = resource.GetAsset<ResConfigSO>();
        if (so == null)
            return;

        cache = new();
        foreach (var item in so.resConfigs)
        {
            cache.Add(item.ResId, item);
        }
    }

    public static ResConfig Get(int resId)
    {
        if (!cache.ContainsKey(resId))
        {
            Debugger.LogError($"界面资源不存在 {resId}", LogDomain.UI);
            return null;
        }

        return cache[resId];
    }
}
