using System;
using System.IO;
using System.Text;
using UnityEngine;

public partial class ConfigFile
{
    public void ConvertToCS()
    {
        string itemClassString = GetCSharpString();

        string assetPath = string.Format("{0}{1}.cs", ConfigTextPath, ConfigClassName);
        string filePath = GetFullName(assetPath);
        if (File.Exists(filePath))
        {
            string oldText = File.ReadAllText(filePath);
            if (oldText == itemClassString) return;
        }

        if (!Directory.Exists(Application.dataPath + ConfigTextPath))
            Directory.CreateDirectory(Application.dataPath + ConfigTextPath);
        File.WriteAllText(filePath, itemClassString);
        Debug.Log("转换完成");
    }


    public string GetCSharpString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("//本文件中的代码为生成的代码，不允许手动修改");
        sb.AppendLine("using System;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine();

        //itemClass
        sb.AppendLine("[Serializable]");
        sb.AppendFormat("public partial class {0}", ItemClassName);
        sb.AppendLine();
        sb.AppendLine("{");


        for (int i = 0; i < m_types.Length; i++) //所有的列
        {
            string type = m_types[i];//先拿到类型
            string name = m_fieldNames[i];//拿到变量名
            string annotation = m_annotations[i];//拿到注释
            string cSharpType = null;
            string[] values = new string[m_valueLines.Length];//这一列,同一个变量类型的全部value
            for (int j = 0; j < m_valueLines.Length; j++)//所有的行
            {
                values[j] = m_valueLines[j][i];//第几行j，第几列i
            }
            cSharpType = GetCSharpType(type, values);
            sb.AppendFormat("\t///<summary> \n\t ///</summary> \n\t public {0} {1};", cSharpType, name, annotation);
            sb.AppendLine();
        }

        sb.AppendLine("}");
        sb.AppendFormat(@"
public partial class {0} : ScriptableObject
{{
    public {1}[] Array;

    public static {0} Singleton {{ get; private set; }}

    public static void Init()
    {{
#if UNITY_EDITOR
        //if (GameConfig.UseLocalAsset)
            LoadFromLocal();
        //else
            //LoadFromBundle();
#else
            LoadFromBundle();
#endif
    }}
/*
    static void LoadFromBundle()
    {{
        var item = new NormalAssetItem(""data/{2}.u"");
        item.Load(() =>
        {{
            Singleton = item.AssetObj as {0};
            item.Release();
        }});
    }}
*/
#if UNITY_EDITOR
    static void LoadFromLocal()
    {{
        string path = ""Assets/Config/Asset/{2}.asset"";
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<{0}>(path);
        Singleton = obj as {0};
    }}
#endif

}}", ConfigClassName, ItemClassName, m_fileName);

        return sb.ToString();
    }

    private string GetCSharpType(string type, string[] values)
    {
        type = type.Trim();

        string target;
        string typeLower = type.ToLowerInvariant();
        switch (typeLower)
        {
            case "uint32":
                target = "uint";
                break;
            case "int32":
                target = "int";
                break;
            case "string":
                target = "string";
                break;
            default:
                throw new InvalidOperationException("Unkown type : " + type);
        }
        return target;
    }
}
