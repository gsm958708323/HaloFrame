using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;

public class ReferencePoolDemo : MonoBehaviour
{
    void Start()
    {
    }

    public void OnTest(int value)
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReferencePool.Add<EventInfo>(100);

            var list = new List<EventInfo>();
            for (int i = 0; i < 10; i++)
            {
                var info = EventInfo.GetTest<int>(OnTest, 111);
                if (i >= 5)
                    EventInfo.Release(info);  // todo 延后释放
            }

            var Collections = ReferencePool.GetRefCollection(typeof(EventInfo));
            print($"UsingRefCount {Collections.UsingRefCount} GetsRefCount {Collections.GetsRefCount} ReleaseRefCount {Collections.ReleaseRefCount}");
        }
    }
}
