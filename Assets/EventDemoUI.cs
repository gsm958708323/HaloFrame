using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDemoUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Dispatcher.AddListener<int>(1, OnEvent, this);
    }

    private void OnDestroy() {
        GameManager.Dispatcher.RemoveListener<int>(1, OnEvent);
    }

    private void OnEvent(int obj)
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
