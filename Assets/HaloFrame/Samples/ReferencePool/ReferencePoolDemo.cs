using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HaloFrame;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class ReferencePoolDemo : MonoBehaviour
{
    void Start()
    {
        //TestReferencePool();

        TestListPool();
    }

    void TestReferencePool()
    {
        TestGet();
        TestGetMultiple();
        TestRelease();
        TestAdd();
        TestAddMultiple();
        TestRemove();
        TestRemoveAll();
    }

    private void TestListPool()
    {
        Profiler.BeginSample("TestListPool");
        // 1.3kb
        for (int i = 0; i < 10000; i++)
        {
            var list = ListPool<int>.Get();
            list.Add(1);
            list.Add(2);
            ListPool<int>.Release(list);

            var dict = DictionaryPool<int, int>.Get();
            dict.Add(1, 1);
            dict.Add(2, 2);
            DictionaryPool<int, int>.Release(dict);
        }

        // Pool.Debug();
        // Pool.DisposeAll();
        // Pool.Debug();

        // 28.6kb
        // for (int i = 0; i < 100; i++)
        // {
        //     var list = new List<int>() { 1, 2 };
        //     var dict = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };
        // }
        Profiler.EndSample();
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

            var collect = ReferencePool.GetRefCollection(typeof(EventInfo));
            print(collect.ToString());
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            ReferencePool.ClearAll();
            var collect = ReferencePool.GetRefCollection(typeof(EventInfo));
            print(collect.ToString());
        }
    }
    public void OnTest(int value)
    {

    }

    public void TestGet()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        var reference = referenceCollection.Get<MyReference>();
        Assert.IsNotNull(reference);
        Assert.AreEqual(1, referenceCollection.GetsRefCount);
        Assert.AreEqual(1, referenceCollection.UsingRefCount);
        print(referenceCollection.ToString());
    }

    public void TestGetMultiple()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        var references = Enumerable.Range(0, 5).Select(x => referenceCollection.Get<MyReference>()).ToList();
        Assert.AreEqual(5, referenceCollection.GetsRefCount);
        Assert.AreEqual(5, referenceCollection.UsingRefCount);
        print(referenceCollection.ToString());

    }

    public void TestRelease()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        var reference = referenceCollection.Get<MyReference>();
        referenceCollection.Release(reference);
        Assert.AreEqual(0, referenceCollection.UsingRefCount);
        Assert.AreEqual(1, referenceCollection.ReleaseRefCount);
        print(referenceCollection.ToString());
    }

    public void TestAdd()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        referenceCollection.Add<MyReference>(5);
        Assert.AreEqual(5, referenceCollection.AddRefCount);
        Assert.AreEqual(0, referenceCollection.UsingRefCount);
        print(referenceCollection.ToString());

    }

    public void TestAddMultiple()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        referenceCollection.Add<MyReference>(5);
        referenceCollection.Add<MyReference>(3);
        Assert.AreEqual(8, referenceCollection.AddRefCount);
        Assert.AreEqual(0, referenceCollection.UsingRefCount);
        print(referenceCollection.ToString());
    }

    public void TestRemove()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        referenceCollection.Add<MyReference>(5);
        referenceCollection.Remove(3);
        print(referenceCollection.ToString());
    }

    public void TestRemoveAll()
    {
        var referenceCollection = new ReferenceCollection(typeof(MyReference));
        referenceCollection.Add<MyReference>(5);
        referenceCollection.RemoveAll();
        Assert.AreEqual(5, referenceCollection.RemoveRefCount);
        Assert.AreEqual(0, referenceCollection.UsingRefCount);
        print(referenceCollection.ToString());
    }

    public class MyReference : IReference
    {
        public void Clear()
        {
            // Do nothing
        }
    }
}
