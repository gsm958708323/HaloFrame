using System;
using System.Collections;
using System.Collections.Generic;
using HaloFrame;
using UnityEngine;
using UnityEngine.Profiling;

public class EventDemo : MonoBehaviour
{
    Dispatcher dispatcher;
    Delegate delegateTest;

    void Start()
    {
        dispatcher = GameManager.Dispatcher;
        // Test1();
        // Test2();
        // Test3();
        // Test4();
        Test5();
    }

    private void Test5()
    {
        Profiler.BeginSample("EventDemo");
        // print函数有gc
        for (int i = 0; i < 100; i++)
        {
            // Action<T1> 到 Delegate也有 GC？
            dispatcher.AddListener<int>(1, OnEvent5_1, this);
            dispatcher.Notify(1, i);
            dispatcher.RemoveListener<int>(1, OnEvent5_1);

            // // 委托在类型转化的时候会产生GC
            // Test5_2<int>(OnEvent5_2);
            // Test5_3(1); 
            // var info = new EventInfo();
        }
        Profiler.EndSample();
    }

    public void Test5_3<T1>(T1 arg1)
    {
        if (delegateTest is Action<T1> callback)
        {
            callback.Invoke(arg1);
        }
    }

    void Test5_2<T>(Action<T> callback)
    {
        Test5_1(callback);
    }

    protected void Test5_1(Delegate callback)
    {
        delegateTest = callback;
    }


    private void OnEvent5_2(int obj)
    {
    }

    private void OnEvent5_1(int obj)
    {
    }

    public class TestCls
    {
        public int A;

        internal void Test4(int obj)
        {
            print(obj);
        }
    }

    private void Test4()
    {
        // 测试：事件触发时，监听对象被销毁了
        var demoUI = FindObjectOfType<EventDemoUI>();
        GameObject.DestroyImmediate(demoUI);
        dispatcher.Notify(1, 1);
    }

    private void Test3()
    {
        // 触发顺序
        dispatcher.AddListener<int>(1, (param) =>
        {
            print($"顺序1 {param}");
        }, this);
        dispatcher.AddListener<int>(1, (param) =>
        {
            print($"顺序2 {param}");
        }, this);
        dispatcher.AddListener<TestCls>(2, (param) =>
        {
            print($"顺序3 {param.A}");
        }, this);


        dispatcher.Notify(1, 1);
        var obj = new TestCls { A = 100 };
        dispatcher.Notify(2, obj);
    }

    private void Test2()
    {
        // 事件A触发移除了事件A的一个监听
        dispatcher.AddListener<int>(1, OnEvent2_1, this);
        dispatcher.AddListener<int>(1, OnEvent2_2, this);

        dispatcher.Notify(1, 1);
        dispatcher.Notify(1, 1);
    }

    private void OnEvent2_2(int obj)
    {
        print(obj);
    }

    private void OnEvent2_1(int obj)
    {
        print(obj);
        dispatcher.RemoveListener<int>(1, OnEvent2_2);
    }

    void Test1()
    {
        // 事件A触发移除了事件B的其中一个监听
        dispatcher.AddListener<int>(1, OnEvent1_1, this);
        dispatcher.AddListener<int>(2, OnEvent1_2, this);
        dispatcher.AddListener<int>(2, OnEvent1_2_1, this);
        dispatcher.Notify(1, 1);
        dispatcher.Notify(2, 2);
    }

    private void OnEvent1_2_1(int obj)
    {
        print(obj);
    }

    private void OnEvent1_2(int obj)
    {
        print(obj);
    }

    private void OnEvent1_1(int obj)
    {
        print(obj);
        dispatcher.RemoveListener<int>(2, OnEvent1_2);
    }
}
