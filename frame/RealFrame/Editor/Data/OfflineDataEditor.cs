using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        AssetBindOfflineData<OfflineData>();
    }
    [MenuItem("Assets/生成UI离线数据")]
    public static void AssetCreateUIOfflineData()
    {
        AssetBindOfflineData<UIOfflineData>();
    }
    [MenuItem("Assets/生成Effect离线数据")]
    public static void AssetCreateEffectOfflineData()
    {
        AssetBindOfflineData<EffectOfflineData>();
    }

    private static void AssetBindOfflineData<T>() where T : OfflineData
    {
        GameObject[] objs = Selection.gameObjects;
        for (int i = 0; i < objs.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改" + objs[i].name + " prefab.", i * 1.0f / objs.Length);
            BindOfflineData<T>(objs[i]);
        }
        EditorUtility.ClearProgressBar();
    }
    private static void BindOfflineData<T>(GameObject obj) where T : OfflineData
    {
        OfflineData offlineData = obj.GetComponent<T>();
        if (offlineData == null)
        {
            offlineData = obj.AddComponent<T>();
        }
        offlineData.BindData();
        //保存当前预制体
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了：" + obj.name + " prefab!");
        Resources.UnloadUnusedAssets(); //卸载未使用的资源
        AssetDatabase.Refresh();        //更新编辑器
    }
}
