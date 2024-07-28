using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class FullView2 : UIGameView
{
    protected override void OnAwake()
    {
        base.OnAwake();

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseSelf();
        });
    }
}
