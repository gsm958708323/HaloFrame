using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HaloFrame;
using UnityEngine;

public class ResDemo : MonoBehaviour
{
    string url = "Assets/HaloFrame/Samples/UI/Prefabs/FullView1.prefab";

    void Start()
    {
        AwaitTest();
    }

    async void AwaitTest()
    {
        var awaiter = GameManager.Resource.LoadWithAwaiter(url);
        await awaiter;
        awaiter.GetResult().Instantiate();
    }

    void CallbackTest()
    {
        GameManager.Resource.LoadWithCallback(url, true, uiRootRes =>
        {
            uiRootRes.Instantiate();
        });
    }

    void CoroutineTest()
    {
        StartCoroutine(CoroutineLoad());
    }

    private IEnumerator CoroutineLoad()
    {
        var uiRootRes = GameManager.Resource.Load(url, true);
        yield return uiRootRes;
        uiRootRes.Instantiate();
    }
}
