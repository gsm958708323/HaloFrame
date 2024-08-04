using System.Collections;
using System.Collections.Generic;
using HaloFrame;
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
            GameManager.UI.Open<FullView1>();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            GameManager.UI.Open<TipsView1>();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            GameManager.UI.Open<FullView2>();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameManager.UI.Open<ChildView2>();
        }

        // Top层
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.UI.Open<FullView2_Top>();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameManager.UI.Open<TipsView2>();
        }
    }
}
