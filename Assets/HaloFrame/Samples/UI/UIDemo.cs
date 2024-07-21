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
            GameManager.UI.Open(ViewType.UITestFullView1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameManager.UI.Open(ViewType.UITestFullView2);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.UI.Open(ViewType.UITestTipsView1);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.UI.Open(ViewType.UITestTipsView2);
        }
    }
}
