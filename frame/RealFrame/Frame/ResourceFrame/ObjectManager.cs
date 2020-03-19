using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager:Singleton<ObjectManager>
{
    //线程池内的物体存放结点（未使用的资源）
    private Transform m_UnSpawnParentNode;
    //默认场景物体结点（切换场景时不删除的资源）
    private Transform m_SceneParentNode;
    //线程池
    protected Dictionary<uint, List<ResourceObject>> m_ObjectResPoolDict = new Dictionary<uint, List<ResourceObject>>();
    protected ClassObjectPool<ResourceObject> m_ResObjPool = null;
    //对象池的资源物体，key：GUID
    protected Dictionary<int, ResourceObject> m_ResObjectDict = new Dictionary<int, ResourceObject>();
    //正在异步加载的资源，key：GUID
    protected Dictionary<int, ResourceObject> m_AsyncResObjectDict = new Dictionary<int, ResourceObject>();

    public void Init(Transform unSpawnParent,Transform sceneParentNode)
    {
        m_ResObjPool = GetOrCreateClassPool<ResourceObject>(1000);
        m_UnSpawnParentNode = unSpawnParent;
        m_SceneParentNode = sceneParentNode;
    }
    /// <summary>
    /// 从线程池加载资源
    /// </summary>
    private ResourceObject GetObjectFromPool(uint crc)
    {
        List<ResourceObject> resObjList = null;
        if (m_ObjectResPoolDict.TryGetValue(crc, out resObjList) && resObjList != null && resObjList.Count > 0)
        {
            ResourceManager.Instance.IncreaseRef(crc);
            ResourceObject resObj = resObjList[0];
            resObjList.RemoveAt(0);
            GameObject go = resObj.m_GameObj;
            if (!System.Object.ReferenceEquals(go, null)) //判断是否为空的方式，性能上比go==null更佳
            {
#if UNITY_EDITOR
                
#endif
            }
            if (resObj != null && resObj.m_OfflineData != null)
            {
                resObj.m_OfflineData.ResetProp();
            }
            return resObj;
        }
        return null;
    }
    public void PreloadObject(string path, int count, bool isClear = true)
    {
        List<GameObject> goList = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject go = InstantiateObject(path,false,isClear);
            goList.Add(go);
        }
        for (int i = 0; i < count; i++)
        {
            ReleaseObject(goList[i]);
            goList[i] = null;
        }
        goList.Clear();
    }
    /// <summary>
    /// 加载实例化物体对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isSceneParent"></param>
    /// <param name="isClear"></param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path, bool isSceneParent = false, bool isClear = true)
    {
        uint crc = CRC32.GetCRC32(path);
        ResourceObject resObj = GetObjectFromPool(crc);
        if (resObj == null)
        {
            resObj = m_ResObjPool.Spawn();
            resObj.m_Crc = crc;
            resObj.m_ResItem = ResourceManager.Instance.GetResourceItem(path, isClear);
            if (resObj.m_ResItem != null && resObj.m_ResItem.m_Obj != null)
            {
                resObj.m_GameObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj, null) as GameObject;
                resObj.m_OfflineData = resObj.m_GameObj.GetComponent<OfflineData>();
            }
        }
        else
        {
            resObj.m_Already = false;
        }
        resObj.m_IsClear = isClear;
        if (isSceneParent)
        {
            resObj.m_GameObj.transform.SetParent(m_SceneParentNode);
        }
        else
        {
            resObj.m_GameObj.transform.SetParent(null);
        }
        if (!m_ResObjectDict.ContainsKey(resObj.m_GameObj.GetInstanceID()))
        {
            m_ResObjectDict.Add(resObj.m_GameObj.GetInstanceID(), resObj);
        }
#if UNITY_EDITOR
        if (resObj.m_GameObj.name.EndsWith("(recycle)"))
        {
            resObj.m_GameObj.name = resObj.m_GameObj.name.Replace("(recycle)", "");
        }
#endif

        return resObj.m_GameObj;
    }
    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="obj">游戏物体</param>
    /// <param name="maxCacheCount">最大缓存数,小于0：不限制，等于0：清除该类型资源</param>
    /// <param name="isDestoryCache">是否清除缓存</param>
    /// <param name="recycleParent">是否回收资源</param>
    public void ReleaseObject(GameObject obj,int maxCacheCount = -1,bool isDestoryCache = false,bool recycleParent = true)
    {
        if (obj == null) return;
        int GUID = obj.GetInstanceID();
        ResourceObject resObj = null;
        if (!m_ResObjectDict.TryGetValue(GUID, out resObj))
        {
            Debug.LogWarning(obj.name + "对象不是ObjectManager创建的！");
            return;
        }
        if (resObj == null)
        {
            Debug.LogError("缓存的ResourceObject为空！");
        }
        if (resObj.m_Already)
        {
            Debug.LogError("已经释放过该资源！" + obj.name);
        }
#if UNITY_EDITOR
        obj.name += "(recycle)";
#endif
        List<ResourceObject> resObjList = null;
        if (maxCacheCount == 0) //回收资源
        {
            m_ResObjectDict.Remove(GUID);
            ResourceManager.Instance.ReleaseResource(resObj, isDestoryCache);
            resObj.Reset();
            m_ResObjPool.UnSpawn(resObj);
        }
        else //回收进对象池
        {
            if (!m_ObjectResPoolDict.TryGetValue(resObj.m_Crc, out resObjList) || resObjList == null)
            {
                resObjList = new List<ResourceObject>();
                m_ObjectResPoolDict.Add(resObj.m_Crc, resObjList);
            }
            if (resObj.m_GameObj)
            {
                if (recycleParent)
                {
                    resObj.m_GameObj.transform.SetParent(m_UnSpawnParentNode);
                }
                else
                {
                    resObj.m_GameObj.SetActive(false);
                }
            }
            if (maxCacheCount < 0 || resObjList.Count < maxCacheCount)
            {
                resObjList.Add(resObj);
                resObj.m_Already = true;
                ResourceManager.Instance.DecreaseRef(resObj.m_Crc);
            }
            else
            {
                //回收
                m_ResObjectDict.Remove(GUID);
                ResourceManager.Instance.ReleaseResource(resObj, isDestoryCache);
                resObj.Reset();
                m_ResObjPool.UnSpawn(resObj);
            }
        }
    }
    /// <summary>
    /// 清除对象池缓存
    /// </summary>
    public void ClearCache()
    {
        List<uint> needClearCrcList = new List<uint>();
        foreach (uint crc in m_ObjectResPoolDict.Keys)
        {
            List<ResourceObject> tempList = m_ObjectResPoolDict[crc];
            for (int i = tempList.Count-1; i >= 0; i--)
            {
                ResourceObject tempObj = tempList[i];
                if (!System.Object.ReferenceEquals(tempObj.m_GameObj, null) && tempObj.m_IsClear)
                {
                    tempList.RemoveAt(i);
                    m_ResObjectDict.Remove(tempObj.m_GameObj.GetInstanceID());
                    tempObj.Reset();
                    m_ResObjPool.UnSpawn(tempObj);
                }
            }
            if (tempList.Count <= 0)
            {
                needClearCrcList.Add(crc);
            }
        }
        for (int i = 0; i < needClearCrcList.Count; i++)
        {
            uint crc = needClearCrcList[i];
            if (m_ObjectResPoolDict.ContainsKey(crc))
            {
                m_ObjectResPoolDict.Remove(crc);
            }
        }
        needClearCrcList.Clear();
    }
    /// <summary>
    /// 清空对象池对该路径下的所有物体引用(用于ResourceManager释放资源的接口)
    /// </summary>
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObject> resObjList = null;

        if (!m_ObjectResPoolDict.TryGetValue(crc, out resObjList) || resObjList == null) return;
        for (int i = resObjList.Count - 1; i >= 0; i--)
        {
            ResourceObject resObj = resObjList[i];
            if (resObj.m_IsClear && resObj.m_GameObj != null)
            {
                int guid = resObj.m_GameObj.GetInstanceID();
                GameObject.Destroy(resObj.m_GameObj);
                m_ResObjectDict.Remove(guid);
                resObj.Reset();
                m_ResObjPool.UnSpawn(resObj);
                resObjList.RemoveAt(i);
            }
        }
        if (resObjList.Count <= 0)
        {
            m_ObjectResPoolDict.Remove(crc);
        }
    }
    /// <summary>
    /// 是否正在异步加载的物体
    /// </summary>
    public bool IsAsyncLoading(GameObject go)
    {
        return m_AsyncResObjectDict[go.GetInstanceID()] != null;
    }
    /// <summary>
    /// 是否是线程池创建的物体
    /// </summary>
    public bool IsObjectManagerCreated(GameObject go)
    {
        return m_ResObjectDict[go.GetInstanceID()] != null;
    }
    /// <summary>
    /// 按GUID取消异步资源加载
    /// </summary>
    /// <param name="guid"></param>
    public void CalcelAsyncLoad(int guid)
    {
        ResourceObject resObj = null;
        if (m_AsyncResObjectDict.TryGetValue(guid, out resObj) && ResourceManager.Instance.CancelAsyncLoad(resObj)) 
        {
            m_AsyncResObjectDict.Remove(guid);
            resObj.Reset();
            m_ResObjPool.UnSpawn(resObj);
        }
    }
    /// <summary>
    /// 异步加载实例化物体
    /// </summary>
    /// <param name="path">物体路径</param>
    /// <param name="finishCallBack">完成后的回调</param>
    /// <param name="loadPriority">加载优先级</param>
    /// <param name="isSceneParant">是否存放在默认场景父亲下（这里跳转场景不会清除）</param>
    /// <param name="isClear">该物体跳转场景是否清除</param>
    /// <param name="param1">回调参数1</param>
    /// <param name="param2">回调参数2</param>
    /// <param name="param3">回调参数3</param>
    public void InstantiateObjectAsync(string path, OnAsyncFinishCallBack finishCallBack,LoadPriority loadPriority, bool isSceneParant = false, bool isClear = true,
        object param1 = null, object param2 = null, object param3 = null)
    {
        if (string.IsNullOrEmpty(path)) return;
        uint crc = CRC32.GetCRC32(path);
        ResourceObject resObj = GetObjectFromPool(crc);
        if (resObj != null)
        {
            resObj.m_Already = false;
            resObj.m_IsClear = isClear;
            m_AsyncResObjectDict.Add(resObj.m_GameObj.GetInstanceID(), resObj);
            if (isSceneParant)
            {
                resObj.m_GameObj.transform.SetParent(m_SceneParentNode, false);
            }
            finishCallBack?.Invoke(path, resObj.m_GameObj, param1, param2, param3);
            return;
        }
        resObj = m_ResObjPool.Spawn();
        resObj.m_IsClear = isClear;
        resObj.m_FinishCallBack = finishCallBack;
        resObj.m_Crc = crc;
        resObj.m_IsSceneParent = isSceneParant;
        resObj.m_LoadPriority = loadPriority;
        resObj.m_Param1 = param1;
        resObj.m_Param2 = param2;
        resObj.m_Param3 = param3;
        ResourceManager.Instance.AsyncLoadResObj(path, resObj, OnAysncResObjCallBack);
    }
    void OnAysncResObjCallBack(string path, ResourceObject resObj , object param1, object param2, object param3)
    {
        if (string.IsNullOrEmpty(path)) return;
        if (resObj.m_ResItem.m_Obj == null)
        {
#if UNIYT_EDITOR
            Debug.LogError("异步加载资源为空：" + path);
#endif
        }
        else
        {
            resObj.m_GameObj = GameObject.Instantiate(resObj.m_ResItem.m_Obj) as GameObject;
            resObj.m_OfflineData = resObj.m_GameObj.GetComponent<OfflineData>();
        }
        //完成物体异步加载，移除正在加载列表
        if (m_AsyncResObjectDict.ContainsKey(resObj.m_GameObj.GetInstanceID()))
        {
            m_AsyncResObjectDict.Remove(resObj.m_GameObj.GetInstanceID());
        }
        if (resObj.m_GameObj != null && resObj.m_IsSceneParent)
        {
            resObj.m_GameObj.transform.SetParent(m_SceneParentNode, false);
        }
        if (resObj.m_FinishCallBack != null)
        {
            int GUID = resObj.m_GameObj.GetInstanceID();
            if (!m_ResObjectDict.ContainsKey(GUID))
            {
                m_ResObjectDict.Add(GUID, resObj);
            }
            resObj.m_FinishCallBack(path, resObj.m_GameObj, resObj.m_Param1, resObj.m_Param2, resObj.m_Param3);
        }
    }

#region 类对象池使用
    //所有类资源池
    protected Dictionary<Type, object> m_ClassPoolDict = new Dictionary<Type, object>(); 
    /// <summary>
    /// 获取或创建类资源池
    /// </summary>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxCount) where T : class, new()
    {
        Type type = typeof(T);
        object obj = null;
        if (!m_ClassPoolDict.TryGetValue(type, out obj) || obj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxCount);
            m_ClassPoolDict.Add(type,newPool);
            return newPool;
        }
        return obj as ClassObjectPool<T>;
    }
#endregion
}
