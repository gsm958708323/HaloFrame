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
        // Normal层
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameManager.UI.Open(ViewType.FullView1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameManager.UI.Open(ViewType.TipsView1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.UI.Open(ViewType.FullView2);
        }


        // Top层
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.UI.Open(ViewType.FullView2_Top);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameManager.UI.Open(ViewType.TipsView2);
        }
    }
}
