using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class FullView1 : UIGameView
{
    protected override void OnAwake()
    {
        base.OnAwake();

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseSelf();
        });

        transform.Find("ChildBtn1").GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenOneChildUI<ChildView1>();
        });

        transform.Find("ChildBtn2").GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenOneChildUI<ChildView2>();
        });

        // var item = CreateItem<TestItem>();
        // item.Start(new object[] { 1 }); // 优化参数传递
    }

    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        OpenChild<ChildView1>();
    }
}
