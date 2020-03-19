using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstConfig {

    //用于ResourceManager的配置:
    public const long MAX_LOADRESTIME = 200000;     //最大等待加载时间,单位：微秒
    public const long MAX_CACHECOUNT = 500;         //最大缓存个数

    //用于UIManager的配置：
    public const string UIPREFABPATH = "Assets/GameData/Prefabs/UIPanel/";     //加载Panel的根目录

    //用于AssetBundleMananger的配置：
    public static string ASSETBUNDLEPATH = Application.streamingAssetsPath;     //加载AB包的根目录
}
