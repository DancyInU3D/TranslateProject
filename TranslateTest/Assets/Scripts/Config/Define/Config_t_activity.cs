//本文件中的代码为生成的代码，不允许手动修改
using System;
using UnityEngine;

[Serializable]
public partial class Item_t_activity
{
	///<summary> 
	 ///</summary> 
	 public uint f_ID;
	///<summary> 
	 ///</summary> 
	 public uint f_Type;
	///<summary> 
	 ///</summary> 
	 public string f_Name;
	///<summary> 
	 ///</summary> 
	 public string f_Relation;
	///<summary> 
	 ///</summary> 
	 public string f_Pic;
	///<summary> 
	 ///</summary> 
	 public string f_StartTime;
	///<summary> 
	 ///</summary> 
	 public string f_BeginDate;
	///<summary> 
	 ///</summary> 
	 public string f_EndDate;
	///<summary> 
	 ///</summary> 
	 public string f_Detail;
	///<summary> 
	 ///</summary> 
	 public uint f_Sort;
	///<summary> 
	 ///</summary> 
	 public uint f_Activitytype;
	///<summary> 
	 ///</summary> 
	 public string f_Remark;
	///<summary> 
	 ///</summary> 
	 public string f_IconCoordinate;
}

public partial class Config_t_activity : ScriptableObject
{
    public Item_t_activity[] Array;

    public static Config_t_activity Singleton { get; private set; }

    public static void Init()
    {
#if UNITY_EDITOR
        //if (GameConfig.UseLocalAsset)
            LoadFromLocal();
        //else
            //LoadFromBundle();
#else
            LoadFromBundle();
#endif
    }
/*
    static void LoadFromBundle()
    {
        var item = new NormalAssetItem("data/t_activity.u");
        item.Load(() =>
        {
            Singleton = item.AssetObj as Config_t_activity;
            item.Release();
        });
    }
*/
#if UNITY_EDITOR
    static void LoadFromLocal()
    {
        string path = "Assets/Config/Asset/t_activity.asset";
        var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<Config_t_activity>(path);
        Singleton = obj as Config_t_activity;
    }
#endif

}