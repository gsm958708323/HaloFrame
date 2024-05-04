using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.UI;

public class RedDotDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var redList = GetComponentsInChildren<RedDotItem>();
        var node2 = redList[0].Bind("2");
        var node2_1 = redList[1].Bind("2.1");
        node2_1.SetValue(1);
        var node2_2 = redList[2].Bind("2.2");
        node2_2.SetValue(1);

        var btnList = GetComponentsInChildren<Button>();
        btnList[1].onClick.AddListener(() => node2_1.SetValue(0));
        btnList[2].onClick.AddListener(() => node2_2.SetValue(0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
