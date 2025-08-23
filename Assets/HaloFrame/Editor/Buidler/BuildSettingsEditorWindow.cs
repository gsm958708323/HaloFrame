using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace HaloFrame
{
    public class BuildSettingsEditorWindow : OdinMenuEditorWindow
    {
        [MenuItem("Tools/HaloFrame/打包编辑器 _F5")]
        private static void OpenWindow()
        {
            var window = GetWindow<BuildSettingsEditorWindow>();
            window.titleContent = new GUIContent("打包编辑器");
            window.minSize = new Vector2(800, 600);
            window.maxSize = new Vector2(1024, 768);
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var setting = AssetDatabase.LoadAssetAtPath<BuildSettingsSO>(PathTools.BuildSettingPath);
            if (setting == null)
            {
                setting = ScriptableObject.CreateInstance<BuildSettingsSO>();
                AssetDatabase.CreateAsset(setting, PathTools.BuildSettingPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            setting.Init();

            var tree = new OdinMenuTree(supportsMultiSelect: false)
            {
                {"打包设置", setting, EditorIcons.SettingsCog}
            };
            return tree;
        }

    }
}