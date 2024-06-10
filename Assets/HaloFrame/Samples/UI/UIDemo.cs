using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDemo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.UIManager.Open(UIViewType.UITestFullView1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameManager.UIManager.Open(UIViewType.UITestFullView2);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.UIManager.Open(UIViewType.UITestTipsView1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.UIManager.Open(UIViewType.UITestTipsView2);
        }
    }
}
