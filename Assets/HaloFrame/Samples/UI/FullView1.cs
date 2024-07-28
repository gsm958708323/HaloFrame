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
            OpenOneChildUi(ViewType.ChildView1);
        });

        transform.Find("ChildBtn2").GetComponent<Button>().onClick.AddListener(() =>
        {
            OpenOneChildUi(ViewType.ChildView2);
        });
    }

    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        OpenChild(ViewType.ChildView1);
    }
}
