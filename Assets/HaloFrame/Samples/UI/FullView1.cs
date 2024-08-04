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
            OpenOneChildUI<ChildView1>(ResType.Static);
        });

        transform.Find("ChildBtn2").GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenOneChildUI<ChildView2>();
        });
    }

    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        var item2 = CreateItem<ButtonItem>();
        item2.Start(new object[] { 2 });

        var item = CreateItem<ButtonItem>(2001);
        item.Start(new object[] { 1 });
    }
}
