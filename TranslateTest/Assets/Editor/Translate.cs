using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Translante
{
    public partial class Translate : EditorWindow
    {
        static List<UnityEngine.Object> m_list;
        int? m_removeIndex;
        [MenuItem("翻译/检查翻译文件key是否重复")]
        public static void CheckKeyRepeat()
        {
            DoCheckKeyRepeat();
        }

        [MenuItem("翻译/提取/预制的中文")]
        public static void GetPrefabChinese()
        {
            DoGetFromPrefab();
        }

        //[MenuItem("翻译/提取/单个预制的中文")]
        //public static void GetSinglePrefab()
        //{
        //    Translate wnd = GetWindow<Translate>("提取单个预制的中文");
        //    wnd.Init();
        //}

        //[MenuItem("翻译/提取/代码的中文")]
        //public static void GetCodeChinese()
        //{
        //    ExcuteGetMonoScript();
        //}

        [MenuItem("翻译/导入/预制翻译表格")]
        public static void ImportChinese()
        {
            ImportPrefabTranslate();
        }

        void Init()
        {
            m_list = new List<UnityEngine.Object>() { null };
            m_removeIndex = null;
        }

        private void OnGUI()
        {
            GUILayout.Label("提取单个预制翻译");
            for (int i = 0; i < m_list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                m_list[i] = EditorGUILayout.ObjectField((i + 1).ToString(), m_list[i], typeof(UnityEngine.Object), GUILayout.Width(400));//可以拖东西的框
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    m_removeIndex = i;
                }

                GUILayout.EndHorizontal();
            }

            // remove asset
            if (m_removeIndex != null)
            {
                m_list.RemoveAt(m_removeIndex.Value);
                m_removeIndex = null;
            }

            // add asset
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                m_list.Add(null);
            }

            // execute
            GUILayout.Space(10);
            if (GUILayout.Button("提取预制翻译", GUILayout.Width(200)))
            {
                ExecuteGetSingle();
            }

            else if (GUILayout.Button("提取Code翻译", GUILayout.Width(200)))
            {
                ExcuteGetMonoScript();
            }
            //}
        }


        public static void DoCheckKeyRepeat()
        {
            string allPath = EditorUtility.OpenFilePanel("请选择导出的本地化文件", "", "");
            List<string> existList = new List<string>();
            int count = 0;
            using (var fileStream = new FileStream(allPath, FileMode.Open, FileAccess.Read))
            {
                using (var excel = new ExcelPackage(fileStream))
                {
                    var sheet = excel.Workbook.Worksheets[1];
                    if (sheet != null && sheet.Dimension != null)
                    {
                        for (int r = 2; r <= sheet.Dimension.End.Row; r++)
                        {
                            string key = sheet.Cells[r, 1].Value.ToString();
                            if (existList.Contains(key))
                            {
                                UnityEngine.Debug.Log("这个key重复了: " + key);
                                count++;
                            }
                            else
                                existList.Add(key);
                        }
                    }
                }

            }
            Debug.Log("检查完毕，重复key的个数为" + count);
        }


        private static void WriteExcel(string folder, List<string> list)
        {
            using (FileStream stream = new FileStream(folder, FileMode.Create, FileAccess.Write))
            {
                using (ExcelPackage excel = new ExcelPackage(stream))
                {
                    var sheet = excel.Workbook.Worksheets.Add("UnlocalizedText");
                    var row = 2;
                    sheet.Cells[1, 1].Value = "Chinese";
                    sheet.Cells[1, 2].Value = "Translation";
                    for (int i = 0; i < list.Count; i++)
                    {
                        sheet.Cells[i + 2, 1].Value = list[i];
                        sheet.Cells[i + 2, 2].Value = " ";
                        EditorUtility.DisplayProgressBar("正在导出翻译文件", "loading...", i / list.Count);

                    }
                    excel.Save();
                }
            }
            EditorUtility.ClearProgressBar();

        }

        private static void WriteExcel(string folder, Dictionary<string, string> dic)
        {
            using (FileStream stream = new FileStream(folder, FileMode.Create, FileAccess.Write))
            {
                using (ExcelPackage excel = new ExcelPackage(stream))
                {
                    var sheet = excel.Workbook.Worksheets.Add("UnlocalizedText");
                    var row = 2;
                    sheet.Cells[1, 1].Value = "Chinese";
                    sheet.Cells[1, 2].Value = "Translation";
                    int sum = dic.Count;
                    int index = 0;
                    foreach (var item in dic)
                    {
                        sheet.Cells[index + 2, 1].Value = item.Key;
                        sheet.Cells[index + 2, 2].Value = item.Value;
                        EditorUtility.DisplayProgressBar("正在导出翻译文件", "loading...", index / sum);
                        index++;
                    }
                    excel.Save();
                }
                EditorUtility.ClearProgressBar();
            }
        }

        private static void FindCompents<T>(string subFolder, Action<T> action)
        {
            var gameobj = AssetDatabase.LoadAssetAtPath<GameObject>(subFolder);
            var uiLabels = gameobj.GetComponentsInChildren<T>(true);
            foreach (var uiLabel in uiLabels)
            {
                action.Invoke(uiLabel);
            }
        }


        /// <summary>
        /// 获取所有组件并执行委托
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        private static void GetComponentsForPath<T>(string path, string filter, Action<T> action)
        {
            var guids = AssetDatabase.FindAssets(filter, new[] { path });
            int sum = guids.Length;
            int index = 0;
            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var uiLabels = obj.GetComponentsInChildren<T>(true);
                foreach (var uiLabel in uiLabels)
                {
                    action.Invoke(uiLabel);
                }
                EditorUtility.DisplayProgressBar("正在提取预制中文", prefabPath, index / sum);
                index++;
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 格式化文本
        /// </summary>
        /// <param name="text"></param>
        /// <returns>第一个：格式完, 第二个：格式前</returns>
        public static (string, string) FormatTexts(string text)
        {
            if (!text.Contains(",") && !text.Contains("\"")) return (text, text);
            var strTemp = text;
            while (strTemp.Contains("\"\""))
            {
                strTemp = strTemp.Replace("\"\"", "\"");
            }

            strTemp = strTemp.Replace("\"", "\"\"");
            text = string.Format("\"{0}\"", strTemp);

            return (text, strTemp);
        }

        /// <summary>
        /// 文本是否含有中文
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            const string pattern = "[\u4e00-\u9fbb]";
            return Regex.IsMatch(text, pattern);
        }
    }
}

