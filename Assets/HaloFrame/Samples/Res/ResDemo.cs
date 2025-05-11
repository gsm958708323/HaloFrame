using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HaloFrame;
using UnityEngine;

public class ResDemo : MonoBehaviour
{
    void Start()
    {
        GameManager.Resource.LoadWithCallback("Assets/Res/UI/FullView.prefab", true, uiRootRes =>
        {
            uiRootRes.Instantiate();
        });
    }
}
