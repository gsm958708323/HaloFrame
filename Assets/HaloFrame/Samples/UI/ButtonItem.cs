using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class ButtonItem : UIItem
{
    protected override void OnStart(object[] args)
    {
        base.OnStart(args);

        int type = (int)args[0];
        gameObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            if (type == 1)
            {
                Debug.Log("这是动态创建的item");
            }
            else
            {
                Debug.Log("这是静态创建的item");
            }
        });
    }
}
