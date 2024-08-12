using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;



public partial class Config
{
    
    private static void ConvertToCSharp()
    {

    }


    private static string ConvertAsset()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();


        return "配置转换成功，耗时：" + stopwatch.ElapsedMilliseconds;
    }
}
