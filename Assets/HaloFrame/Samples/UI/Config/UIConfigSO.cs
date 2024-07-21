using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UIConfig", order = 1)]
public class UIConfigSO : ScriptableObject
{
    public List<UIConfig> uiConfigs;

    private static Dictionary<ViewType, UIConfig> configDict;
    public static Dictionary<ViewType, UIConfig> GetAll()
    {
        if (configDict == null)
        {
            var path = "Assets/HaloFrame/Samples/UI/Config/UIConfig.asset";
            var so = GameManager.Resource.LoadAsset<UIConfigSO>(path);
            if (so == null)
                return null;

            configDict = new Dictionary<ViewType, UIConfig>();
            foreach (var item in so.uiConfigs)
            {
                if (configDict.ContainsKey(item.ViewType))
                {
                    Debugger.LogError($"界面类型重复 {item.ViewType}", LogDomain.UI);
                    continue;
                }

                configDict.Add(item.ViewType, item);
            }
        }

        return configDict;
    }

    public static UIConfig Get(ViewType viewType)
    {
        var all = GetAll();
        if (all == null)
        {
            Debugger.LogError($"界面配置不存在 {viewType}", LogDomain.UI);
            return null;
        }

        return all[viewType];
    }
}
