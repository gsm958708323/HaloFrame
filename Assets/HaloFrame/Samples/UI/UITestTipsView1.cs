using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class UITestTipsView1 : UIView
{
        public override void OnInit(GameObject go, UIViewCtrl ctrl)
    {
        base.OnInit(go, ctrl);

        transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            CloseSelf();
        });
    }
}
