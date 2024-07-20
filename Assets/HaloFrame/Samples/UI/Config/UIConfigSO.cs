using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UIConfig", order = 1)]
public class UIConfigSO : ScriptableObject
{
    public List<UIConfig> uiConfigs;

    private static Dictionary<UIViewType, UIConfig> configDict;
    public static Dictionary<UIViewType, UIConfig> Get()
    {
        if (configDict == null)
        {
            var path = "Assets/HaloFrame/Samples/UI/Config/UIConfig.asset";
            var so = GameManager.Resource.LoadAsset<UIConfigSO>(path);
            if (so == null)
                return null;

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
}
