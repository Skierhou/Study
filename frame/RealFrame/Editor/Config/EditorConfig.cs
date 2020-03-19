using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Assets/RealFrame/Editor/EditorConfig", menuName = "CreateEditorConfig", order = 0)]
[System.Serializable]
public class EditorConfig : ScriptableObject
{
    //ABConfig配置表的二进制路径
    public string ABConfigBinaryPath;
    //打包的APP名字
    public string APPName;
    //Xml与二进制转换的Xml文件目录
    public string XmlPath;
    //Xml与二进制转换的二进制文件目录
    public string BinaryPath;
    //Xml与二进制转换的数据类文件目录
    public string ScriptPath;

    public static EditorConfig GetEditorConfig()
    {
        string editorConfigPath = "Assets/RealFrame/Editor/EditorConfig.asset";
        return AssetDatabase.LoadAssetAtPath<EditorConfig>(editorConfigPath);
    }
}
