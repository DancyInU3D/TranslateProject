using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;



public partial class ConfigFile
{
    public string ConvertToAsset()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Assembly assembly = Assembly.Load("HotFix");//加载程序集
        Type dbType = assembly.GetType(ConfigClassName);//通过反射找到这个类
        if (dbType == null)
            throw new InvalidOperationException(string.Format("c# class {0} is not exists", ConfigClassName));//如果这个类没生成呢，那就先生成

        string assetPath = string.Format("{0}{1}.asset", AssetePath, m_fileName);
        var asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);//先尝试加载asset文件
        if (asset == null)
        {
            var obj = ScriptableObject.CreateInstance(ConfigClassName);
            CreateDirectory(assetPath);
            AssetDatabase.CreateAsset(obj, assetPath);
            asset = AssetDatabase.LoadAssetAtPath(assetPath, dbType);
        }

        SetAssetData(assembly, asset);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        return "配置转换成功，耗时：" + stopwatch.ElapsedMilliseconds;
    }

    private void SetAssetData(Assembly assembly, object asset)
    {
        ArrayList arrayList = new ArrayList();
        Type itemType = assembly.GetType(ItemClassName);
        var itemConstructor = itemType.GetConstructor(Type.EmptyTypes);//创建构造函数

        for (int r = 0; r < m_valueLines.Length; r++)
        {
            string[] line = m_valueLines[r];//这一行的所有数据
            var itemObj = itemConstructor.Invoke(null);
            for (int c = 0; c < line.Length; c++)
            {
                string name = m_fieldNames[c];
                string srcValue = line[c];
                var field = itemType.GetField(name);//通过变量名称，获得 FieldInfo 类型
                if (field != null)
                {
                    object valueObj = GetCSharpValue(srcValue, field.FieldType);
                    field.SetValue(itemObj, valueObj);//设置这一个变量的value
                }
                else
                    UnityEngine.Debug.LogWarning(string.Format("field {0}.{1} is not exists", ItemClassName, name));
            }
            arrayList.Add(itemObj);
        }

        Type dbType = assembly.GetType(ConfigClassName);
        FieldInfo info = dbType.GetField("Array");
        info.SetValue(asset, arrayList.ToArray(itemType));
    }

    private object GetCSharpValue(string srcValue, Type type)
    {
        object target;
        switch (type.ToString())
        {
            case "System.UInt64":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(ulong);
                else
                {
                    ulong ul;
                    if (!ulong.TryParse(srcValue, out ul))
                        throw new InvalidOperationException(srcValue + " is not a ulong");
                    target = ul;
                }
                break;
            case "System.Int32":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(int);
                else
                {
                    int i;
                    if (!int.TryParse(srcValue, out i))
                        throw new InvalidOperationException(srcValue + " is not a int");
                    target = i;
                }
                break;
            case "System.UInt32":
                srcValue = srcValue.Replace(" ", "");
                if (string.IsNullOrEmpty(srcValue))
                    target = default(uint);
                else
                {
                    uint i;
                    if (!uint.TryParse(srcValue, out i))
                        throw new InvalidOperationException(srcValue + " is not a int");
                    target = i;
                }
                break;
            case "System.String":
                target = srcValue.TrimStart('"').TrimEnd('"');
                break;
            default:
                target = srcValue;
                throw new InvalidOperationException("Unexpect C# type " + type.ToString());
        }
        return target;
    }

    private void CreateDirectory(string path)
    {
        path = Application.dataPath + path;
        if (path.Contains("."))//筛选出来文件夹，而不是文件名
        {
            string[] strs = path.Split('/');
            string name = strs[strs.Length - 1];
            path = path.Replace(name, "");
        }
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
    }
}