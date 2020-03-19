using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : Singleton<ConfigManager> {

    Dictionary<string, BaseData> m_AllConfigDict = new Dictionary<string, BaseData>();

    /// <summary>
    /// 加载二进制
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="path">二进制文件路径</param>
    public T LoadData<T>(string path) where T: BaseData
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (m_AllConfigDict.ContainsKey(path))
        {
            Debug.LogError("重复加载配置表：" + path);
            return null;
        }
        T data = Serialize.BinaryDeSerializeRun<T>(path);
#if UNITY_EDITOR
        if (data == null)
        {
            Debug.Log(path + "不存在二进制文件，尝试通过xml文件加载！");
            string xmlPath = path.Replace("Binary", "Xml").Replace(".bytes", ".xml");
            data = Serialize.XmlDeSerializeRun<T>(path);
        }
#endif
        if (data != null)
        {
            data.Init();
        }
        m_AllConfigDict.Add(path,data);
        return data;
    }
    /// <summary>
    /// 找到配置资源
    /// </summary>
    public T FindData<T>(string path) where T : BaseData
    {
        if (string.IsNullOrEmpty(path)) return null;
        BaseData data = null;
        if (!m_AllConfigDict.TryGetValue(path, out data) || data == null)
        {
            Debug.LogError("无法找到配置资源：" + path);
            return null;
        }
        return data as T;
    }
}

