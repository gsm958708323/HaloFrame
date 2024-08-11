using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class FullView1 : UIGameView
{
    private List<ButtonItem> btnList;
    private Transform layout;
    protected override void OnAwake()
    {
        base.OnAwake();

        btnList = new();

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseSelf();
        });

        layout = transform.Find("Layout");
    }

    protected override void OnDestroy()
    {
        btnList = null;

        base.OnDestroy();
    }

    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        for (int i = 0; i < 4; i++)
        {
            var item = CreateItem<ButtonItem>();
            item.transform.SetParent(layout);
            btnList.Add(item);
        }

        foreach (var item in btnList)
        {
            var index = btnList.IndexOf(item);
            item.Start(new object[] { index });
        }
    }
}
