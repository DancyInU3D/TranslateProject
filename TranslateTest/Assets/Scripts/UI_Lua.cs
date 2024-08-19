using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using XLua;

/// <summary>
/// 因为使用lua的热更新，所以不放在hotfix程序集中
/// </summary>
public class UI_Lua : MonoBehaviour
{
    LuaEnv luaEnv = new LuaEnv();
    // Start is called before the first frame update
    void Start()
    {
        Fun10();
    }

    private void Fun1()
    {
        luaEnv.DoString("print('hello world')");

        luaEnv.DoString("CS.UnityEngine.Debug.Log('hello world')");
    }

    /// <summary>
    /// 通过文件加载
    /// </summary>
    private void Fun2()
    {
        TextAsset asset = Resources.Load<TextAsset>("lua01.lua");//不需要加文件后缀
        luaEnv.DoString(asset.text);
    }

    /// <summary>
    /// 通过require加载(默认通过Resources文件夹加载)
    /// </summary>
    private void Fun3()
    {
        luaEnv.DoString("require('lua01')");
    }

    /// <summary>
    /// 通过自定义目录加载
    /// </summary>
    private void Fun4()
    {
        luaEnv.AddLoader(myLoader);
        luaEnv.DoString("require('lua02')");
    }
    private byte[] myLoader(ref string filepath)
    {
        filepath = Application.dataPath + "/Scripts/Lua/" + filepath + ".lua.txt";
        return File.ReadAllBytes(filepath);
    }

    /// <summary>
    /// 新建Lua类，读取文件
    /// </summary>
    private void Fun5()
    {
        LuaEnv env = MyLuaBase.Require("lua03");
        int a = env.Global.Get<int>("a");
        string name = env.Global.Get<string>("name");
        Debug.Log(a);
        Debug.Log(name);
    }

    /// <summary>
    /// 映射到类
    /// </summary>
    private void Fun6()
    {
        LuaEnv env = MyLuaBase.Require("lua04");
        Person person = env.Global.Get<Person>("person");
        Debug.Log(person.name);
        person.name = "ppp";
        Debug.Log(person.name);
    }
    private class Person
    {
        public string name;
        public int age;
        public int id;
    }

    /// <summary>
    /// 映射到接口
    /// </summary>
    private void Fun7()
    {
        LuaEnv env = MyLuaBase.Require("lua05");
        IMyPerson per = env.Global.Get<IMyPerson>("person");
        Debug.Log(per.Name + "==" + per.Age);
        per.Talk();
        per.Work("节目：");
        per.Friend("李雪琴 ", " 郭麒麟");
    }


    [CSharpCallLua]
    public interface IMyPerson
    {
        string Name { get; set; }
        int Age { get; set; }
        void Talk();
        void Work(string name);
        void Friend(string a, string b);
    }

    /// <summary>
    /// 映射到dic和list
    /// </summary>
    private void Fun8()
    {
        LuaEnv env = MyLuaBase.Require("lua06");
        Singer ss = env.Global.Get<Singer>("Singer");
        List<int> list = ss.table1;
        foreach (var item in list)
        {
            Debug.Log(item);
        }


    }
    private class Singer
    {
        public string Name;
        public int Age;
        public List<int> table1;
        public void Work() { }
    }

    /// <summary>
    /// 通过委托调用lua方法
    /// </summary>
    private void Fun9()
    {
        LuaEnv env = MyLuaBase.Require("lua07");
        Add add = env.Global.Get<Add>("Add");
        string c;
        string res=add("a ","b", out c);
        Debug.Log(res);
        Debug.Log(c);
    }

    [CSharpCallLua]
    public delegate string Add(string a,string b,out string c);

    /// <summary>
    /// 直接用 LuaFunction 类去接收，但只能读取，不能修改
    /// </summary>
    private void Fun10()
    {
        LuaEnv env = MyLuaBase.Require("lua07");
        LuaFunction fun = env.Global.Get<LuaFunction>("Add");
        object[] res = fun.Call(2, 5);
        foreach (var item in res)
        {
            Debug.Log(item);
        }
    }
    
}