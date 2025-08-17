using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HaloFrame.Editor
{
    [InitializeOnLoad]
    public class EditorTools
    {
        static EditorTools()
        {
            // 编辑器加载时初始化菜单状态
            InitializeMenuState();
        }

        [MenuItem("Tools/HaloFrame/资源加载模式/编辑器模式")]
        public static void SwitchResLoad1()
        {
            PlayerPrefs.SetInt("IsEditorMode", 1);
            UpdateMenuCheckState();
        }

        [MenuItem("Tools/HaloFrame/资源加载模式/AB模式")]
        public static void SwitchResLoad2()
        {
            PlayerPrefs.SetInt("IsEditorMode", 0);
            UpdateMenuCheckState();
        }

        // 根据 IsEditorMode 的值设置 MenuItem 的选中状态
        private static void UpdateMenuCheckState()
        {
            bool isEditorMode = PlayerPrefs.GetInt("IsEditorMode", 1) == 1;
            Menu.SetChecked("Tools/HaloFrame/资源加载模式/编辑器模式", isEditorMode);
            Menu.SetChecked("Tools/HaloFrame/资源加载模式/AB模式", !isEditorMode);
        }

        // 初始化菜单状态
        private static void InitializeMenuState()
        {
            UpdateMenuCheckState();
        }
    }
}