using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class UITestTipsView1 : UIView
{
    protected override void OnAwake()
    {
        transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseSelf();
        });
    }
}
