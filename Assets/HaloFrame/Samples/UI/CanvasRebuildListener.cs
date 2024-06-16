using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class CanvasRebuildListener : MonoBehaviour
{
    IList<ICanvasElement> layoutRebuild, graphicRebuild;
    void Start()
    {
        Type type = typeof(CanvasUpdateRegistry);
        FieldInfo fieldInfo1 = type.GetField("m_LayoutRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        layoutRebuild = (IList<ICanvasElement>)fieldInfo1.GetValue(CanvasUpdateRegistry.instance);

        FieldInfo fieldInfo2 = type.GetField("m_GraphicRebuildQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        graphicRebuild = (IList<ICanvasElement>)fieldInfo2.GetValue(CanvasUpdateRegistry.instance);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < layoutRebuild.Count; i++)
        {
            var rebuild = layoutRebuild[i];
            if (IsVaild(rebuild))
            {
                print($"LayoutRebuild: {rebuild.transform.name} 引起 {rebuild.transform.GetComponent<Graphic>().canvas.name} 网格重建 ");
            }
        }

        for (int i = 0; i < graphicRebuild.Count; i++)
        {
            var rebuild = graphicRebuild[i];
            if (IsVaild(rebuild))
            {
                print($"GraphicRebuild: {rebuild.transform.name} 引起 {rebuild.transform.GetComponent<Graphic>().canvas.name} 网格重建 ");
            }
        }
    }

    bool IsVaild(ICanvasElement element)
    {
        var vaild = element != null;
        var isUnityObject = element is UnityEngine.Object;
        if (isUnityObject)
        {
            vaild = (element as object) != null;
        }
        return vaild;
    }
}
