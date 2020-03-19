using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ABBuildTools {

    private static string ABCONFIGPATH = "Assets/RealFrame/Editor/ABConfig.asset";
    private static string BUNDLETARGERPATH = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget;
    private static string ABCONFIGBINNERPATH = EditorConfig.GetEditorConfig().ABConfigBinaryPath;

    //key:ab包名，value:文件路径 , 存贮所有文件夹AB路径
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();

    //key:ab包名，value：所有依赖项和自己路径，存储prefab所有项路径
    public static Dictionary<string, List<string>> m_AllPrefabABDir = new Dictionary<string, List<string>>();

    //过滤用的List
    public static List<string> m_AllFileABList = new List<string>();

    //配置表过滤用的List
    private static List<string> m_ConfigABList = new List<string>();

    [MenuItem("Tools/AB包/打AB包")]
    public static void BuildAB()
    {
        m_AllFileABList.Clear();
        m_AllFileDir.Clear();
        m_AllPrefabABDir.Clear();
        m_ConfigABList.Clear();

        ABConfig aBConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);

        //先打包文件夹，再打包prefab
        //获取需要打包的文件夹
        foreach (ABConfig.FileDirABName fileDir in aBConfig.m_AllFileDirABList)
        {
            if (m_AllFileDir.ContainsKey(fileDir.AbName))
            {
                Debug.LogError("AB包名重复，请检查！");
            }
            else
            {
                m_AllFileDir.Add(fileDir.AbName, fileDir.FilePath);
                m_AllFileABList.Add(fileDir.FilePath);
                m_ConfigABList.Add(fileDir.FilePath);
            }
        }

        //获取需要打包的Prefab
        string[] allStr = AssetDatabase.FindAssets("t:prefab", aBConfig.m_AllPrefabPathList.ToArray());
        for (int i = 0; i < allStr.Length; i++)
        {
            //FindAssets方法找到的是GUID，通过下面方法转换成path
            string path = AssetDatabase.GUIDToAssetPath(allStr[i]);
            m_ConfigABList.Add(path);
            //显示进度条
            EditorUtility.DisplayProgressBar("查找Prefab", "Prefab: " + path, i * 1.0f / allStr.Length);
            if (!ContainAllFileAB(path))
            {
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                //获取当前prefab的所有依赖项路径
                string[] allDependPaths = AssetDatabase.GetDependencies(path);

                List<string> dependList = new List<string>();
                for (int j = 0; j < allDependPaths.Length; j++)
                {
                    //不包含该文件，且该文件不是.cs文件
                    if (!ContainAllFileAB(allDependPaths[j]) && !allDependPaths[j].EndsWith(".cs"))
                    {
                        m_AllFileABList.Add(allDependPaths[j]); //加入过滤List中
                        dependList.Add(allDependPaths[j]);
                    }
                }
                if (m_AllPrefabABDir.ContainsKey(go.name))
                {
                    Debug.LogError("存在相同名字的Prefab :" + go.name);
                }
                else
                {
                    m_AllPrefabABDir.Add(go.name, dependList);
                }
            }
        }

        //设置AB包名
        foreach (string name in m_AllFileDir.Keys)
        {
            SetABName(name, m_AllFileDir[name]);
        }
        foreach (string name in m_AllPrefabABDir.Keys)
        {
            SetABName(name, m_AllPrefabABDir[name]);
        }

        BuildAssetBundle();

        //清除AB包，作用：文件AB包名更改后会更改其.meta文件，这样打包后可能会导致各种各样的问题
        //在每次打完AB包后，清除所有AB包名，可以避免一些意外的错误。
        string[] oldABNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldABNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldABNames[i], true);
            EditorUtility.DisplayProgressBar("清除AB包", "名字：" + oldABNames[i], i * 1.0f / oldABNames.Length);
        }

        AssetDatabase.Refresh();
        //清除进度条
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 设置AB名字（同编译器设置名字，用代码完成）
    /// </summary>
    static void SetABName(string name, string path)
    {
        AssetImporter assetImporter = AssetImporter.GetAtPath(path);
        if (assetImporter == null)
        {
            Debug.LogError("不存在该路径：" + path);
        }
        else
        {
            assetImporter.assetBundleName = name;
        }
    }
    static void SetABName(string name, List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            SetABName(name, paths[i]);
        }
    }
    /// <summary>
    /// 打包过程
    /// </summary>
    static void BuildAssetBundle()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();    //获取所有包名
        //key为全路径，Value为包名  这个字典是所有打包的全路径和包名，用于设置自己的配置表
        Dictionary<string, string> resultPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allBundleNames.Length; i++)
        {
            string[] allBundlePaths = AssetDatabase.GetAssetPathsFromAssetBundle(allBundleNames[i]); //获取该包名下的所有路径
            for (int j = 0; j < allBundlePaths.Length; j++)
            {
                if (allBundlePaths[j].EndsWith(".cs")) continue;

                Debug.Log("AB包：" + allBundleNames[i] + " 包含资源路径：" + allBundlePaths[j]);

                resultPathDic.Add(allBundlePaths[j], allBundleNames[i]);
            }
        }

        if (!Directory.Exists(BUNDLETARGERPATH))
        {
            Directory.CreateDirectory(BUNDLETARGERPATH);
        }

        //删除不存在或者已经改名的AB包
        DeleteAB();

        //自己的配置表生成
        CreateConfigTable(resultPathDic);

        //打包
        BuildPipeline.BuildAssetBundles(BUNDLETARGERPATH, BuildAssetBundleOptions.ChunkBasedCompression,
            EditorUserBuildSettings.activeBuildTarget);

    }

    /// <summary>
    /// 创建配置表
    /// </summary>
    static void CreateConfigTable(Dictionary<string,string> resultPathDic)
    {
        ABBundleConfig bundleConfig = new ABBundleConfig();
        bundleConfig.ABList = new List<ABBase>();
        foreach (string path in resultPathDic.Keys)
        {
            if (!ValidPath(path))
                continue;

            ABBase abBase = new ABBase();
            abBase.Path = path;
            abBase.Crc = CRC32.GetCRC32(path);
            abBase.ABName = resultPathDic[path];
            abBase.AssetName = path.Remove(0, path.LastIndexOf("/") + 1);  //资源名是全路径名去掉最后一个'/'前面的字符串
            abBase.ABDependce = new List<string>();
            string[] allDependce = AssetDatabase.GetDependencies(path);
            for (int i = 0; i < allDependce.Length; i++)
            {
                string tempPath = allDependce[i];
                if (tempPath == path || path.EndsWith(".cs")) continue;

                string abName = "";
                if (resultPathDic.TryGetValue(tempPath, out abName))
                {
                    if (abName == resultPathDic[path]) continue;

                    if (!abBase.ABDependce.Contains(abName))
                    {
                        abBase.ABDependce.Add(abName);
                    }
                }
            }
            bundleConfig.ABList.Add(abBase);
        }

        //写入xml
        string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
        if (File.Exists(xmlPath)) File.Delete(xmlPath);
        FileStream fs1 = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw1 = new StreamWriter(fs1, System.Text.Encoding.UTF8);
        XmlSerializer xmlSerializer = new XmlSerializer(bundleConfig.GetType());
        xmlSerializer.Serialize(sw1, bundleConfig);
        sw1.Close();
        fs1.Close();

        //写入二进制

        if (File.Exists(ABCONFIGBINNERPATH)) File.Delete(ABCONFIGBINNERPATH);
        FileStream fs2 = new FileStream(ABCONFIGBINNERPATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf2 = new BinaryFormatter();
        bf2.Serialize(fs2, bundleConfig);
        fs2.Close();
        AssetDatabase.Refresh();
        SetABName("assetbundleconfig", ABCONFIGBINNERPATH);
    }

    /// <summary>
    /// 删除无用AB包
    /// </summary>
    static void DeleteAB()
    {
        string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo directoryInfo = new DirectoryInfo(BUNDLETARGERPATH);
        FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            if (ContainABName(files[i].Name, allBundleNames) || files[i].Name.EndsWith(".meta") || files[i].Name.EndsWith(".manifest"))
            {
                continue;
            }
            else
            {
                Debug.Log("此AB包已经被删除或者改名了：" + files[i].Name);
                if (File.Exists(files[i].FullName))
                {
                    File.Delete(files[i].FullName);
                }
            }
        }
    }

    /// <summary>
    /// 是否包含相同的AB包名
    /// </summary>
    static bool ContainABName(string name, string[] allBundleNames)
    {
        for (int i = 0; i < allBundleNames.Length; i++)
        {
            if (name == allBundleNames[i])
                return true;
        }
        return false;
    }

    /// <summary>
    /// 用于冗余剔除，判断是否包含该AB包
    /// </summary>
    static bool ContainAllFileAB(string path)
    {
        for (int i = 0; i < m_AllFileABList.Count; i++)
        {
            if (path == m_AllFileABList[i] || (path.Contains(m_AllFileABList[i]) && (path.Replace(m_AllFileABList[i], "")[0] == '/')))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断有效路径，作用：配置表时不加入依赖项的数据
    /// </summary>
    static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigABList.Count; i++)
        {
            if (path.Contains(m_ConfigABList[i]))
                return true;
        }
        return false;
    }

}
