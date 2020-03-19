using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>{

    //当前所有AB资源对象
    protected Dictionary<uint, ResourceItem> abResourceDict = new Dictionary<uint, ResourceItem>();
    //已加载的资源
    protected Dictionary<uint, AssetBundleItem> abItemDict = new Dictionary<uint, AssetBundleItem>();
    //ABItem类资源池
    protected ClassObjectPool<AssetBundleItem> abItemClassPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

    /// <summary>
    /// 预加载所有AssetBundle资源
    /// </summary>
    public void LoadAllAssetBundle()
    {
        abResourceDict.Clear();

        AssetBundle ab = AssetBundle.LoadFromFile(ConstConfig.ASSETBUNDLEPATH + "/assetbundleconfig");
        TextAsset textAsset = ab.LoadAsset<TextAsset>("AssetBundleConfig.bytes");
        if (textAsset == null)
        {
            Debug.LogError("Asset丢失或未找到资源");
            return;
        }
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        ABBundleConfig config = (ABBundleConfig)bf.Deserialize(stream);
        stream.Close();
        for (int i = 0; i < config.ABList.Count; i++)
        {
            ABBase abBase = config.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = abBase.Crc;
            item.m_ABName = abBase.ABName;
            item.m_AssetName = abBase.AssetName;
            item.m_ABDependce = abBase.ABDependce;
            if (abResourceDict.ContainsKey(item.m_Crc))
            {
                Debug.LogError("存在重复的Crc, AB包名：" + item.m_ABName + " 资源名：" + item.m_AssetName);
            }
            else
            {
                abResourceDict.Add(item.m_Crc, item);
            }
        }
    }

    /// <summary>
    /// 获取资源块
    /// </summary>
    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        ResourceItem item = null;
        if (!abResourceDict.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError(string.Format("Load Asset Item error: Can't find crc[{0}] in AssetBundleConfig",crc.ToString()));
            return item;
        }
        if (item.m_AssetBundle == null)
        {
            item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        }
        //加载依赖项
        if (item.m_ABDependce != null)
        {
            for (int i = 0; i < item.m_ABDependce.Count; i++)
            {
                LoadAssetBundle(item.m_ABDependce[i]);
            }
        }
        return item;
    }
    /// <summary>
    /// 加载AB资源
    /// </summary>
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (!abItemDict.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string fullPath = ConstConfig.ASSETBUNDLEPATH + "/" + name;
            //if (!File.Exists(fullPath))
            //{
            //    Debug.LogError("Can't find Asset, path:" + fullPath);
            //    return assetBundle;
            //}
            assetBundle = AssetBundle.LoadFromFile(fullPath);
            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle error :" + fullPath);
            }
            item = abItemClassPool.Spawn();
            item.m_AssetBundle = assetBundle;
            item.m_RefCount++;
            abItemDict.Add(crc, item);
        }
        else
        {
            item.m_RefCount++;
        }
        
        return item.m_AssetBundle;
    }
    /// <summary>
    /// 释放AssetBundle资源
    /// </summary>
    public void ReleaseAssetBundle(ResourceItem item)
    {
        if (item == null) return;
        if (item.m_ABDependce != null && item.m_ABDependce.Count > 0)
        {
            for (int i = 0; i < item.m_ABDependce.Count; i++)
            {
                UnLoadAssetBundle(item.m_ABDependce[i]);
            }
        }
        UnLoadAssetBundle(item.m_ABName);
    }
    /// <summary>
    /// 按资源名 卸载资源
    /// </summary>
    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (abItemDict.TryGetValue(crc, out item) && item != null)
        {
            item.m_RefCount--;
            if (item.m_RefCount <= 0 && item.m_AssetBundle != null) 
            {
                item.m_AssetBundle.Unload(true);
                item.Reset();
                abItemClassPool.UnSpawn(item);
                abItemDict.Remove(crc);
            }
        }
    }
    /// <summary>
    /// 根据Crc获取ResourceItem
    /// </summary>
    public ResourceItem GetResourceItem(uint crc)
    {
        ResourceItem item = null;
        abResourceDict.TryGetValue(crc, out item);
        return item;
    }
}

public class AssetBundleItem
{
    public AssetBundle m_AssetBundle { set; get; }
    public int m_RefCount { set; get; }         //引用个数

    public void Reset()
    {
        m_AssetBundle = null;
        m_RefCount = 0;
    }
}

public class ResourceItem
{
    public uint m_Crc = 0;
    public string m_ABName = string.Empty;
    public string m_AssetName = string.Empty;
    public List<string> m_ABDependce = null;
    public AssetBundle m_AssetBundle = null;

    //-----------------------------------------------

    public Object m_Obj = null;         //资源物体
    public int m_GUID = 0;              //GUID
    public float m_LastUseTime = 0f;    //最后使用时间
    public bool m_IsClear = true;       //是否清除
    protected int m_RefCount = 0;       //引用数
    public int RefCount {
        get { return m_RefCount; }
        set {
            if (value < 0)
            {
                Debug.LogError("ReferenceCount < 0 ," + value + "," + (m_Obj == null ? "object is null" : m_Obj.name));
            }
            m_RefCount = value;
        }
    }

}
