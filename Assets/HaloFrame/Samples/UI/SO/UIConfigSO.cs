using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UIConfig", order = 1)]
public class UIConfigSO : ScriptableObject
{
    public List<UIConfig> uiConfigs;

    public static List<UIConfig> Get()
    {
        var path = "Assets/HaloFrame/Samples/UI/SO/UIConfig.asset";
        var so = GameManager.ResourceManager.LoadAsset<UIConfigSO>(path);
        return so.uiConfigs;
    }
}
