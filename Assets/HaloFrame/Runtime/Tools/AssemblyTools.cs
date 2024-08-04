using System;
using HaloFrame;
using System.Reflection;

public static class AssemblyTools
{
    static Assembly assembly;
    static AssemblyTools()
    {
        assembly = Assembly.GetExecutingAssembly();
    }

    /// <summary>
    /// 从域程序集中获取类。
    /// </summary>
    /// <param name="typeName">类型完全限定名</param>
    /// <returns>类型</returns>
    public static Type GetType(string typeName)
    {
        Type type = assembly.GetType(typeName);
        if (type == null)
        {
            Debugger.LogError($"找不到类型：{typeName}");
            return null;
        }
        return type;
    }

    public static object CreateInstance(Type type, params object[] args)
    {
        return Activator.CreateInstance(type, args);
    }
}
