using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源加载优先级
/// </summary>
public enum LoadPriority
{
    RES_HIGHT = 0,
    RES_MIDDLE,
    RES_LOWER,
    RES_NUM
}
public class ResourceObject
{
    public uint m_Crc = 0;
    public ResourceItem m_ResItem = null;   //物体对应的资源
    public GameObject m_GameObj = null;     //实例化出来的物体
    public bool m_IsClear = true;           //是否加载场景清除
    public bool m_Already = false;          //是否已经释放过
    public bool m_IsSceneParent = false;    //是否需要设置到SceneNode结点

    //------------------------------------
    public LoadPriority m_LoadPriority = LoadPriority.RES_LOWER;
    public OnAsyncFinishCallBack m_FinishCallBack = null;
    public object m_Param1, m_Param2, m_Param3 = null;
    public OfflineData m_OfflineData = null;

    public void Reset()
    {
        m_Crc = 0;
        m_ResItem = null;
        m_GameObj = null;
        m_IsClear = true;
        m_Already = false;
        m_LoadPriority = LoadPriority.RES_LOWER;
        m_FinishCallBack = null;
        m_Param1 = m_Param2 = m_Param3 = null;
        m_OfflineData = null;
    }
}

/// <summary>
/// 异步加载资源对象
/// </summary>
public class AsyncLoadResParam
{
    public List<AsyncCallBack> callBackList = new List<AsyncCallBack>();    //一个资源可能存在多个回调
    public uint m_Crc = 0;
    public string m_Path = "";
    public bool m_IsSprite = false;
    public LoadPriority m_Priority = LoadPriority.RES_LOWER;            //当前资源的加载优先级

    public void Reset()
    {
        callBackList.Clear();
        m_Crc = 0;
        m_Path = "";
        m_IsSprite = false;
        m_Priority = LoadPriority.RES_LOWER;
    }
}
/// <summary>
/// 异步加载回调对象
/// </summary>
public class AsyncCallBack
{
    public OnAsyncFinishCallBack finishCallBack = null;
    public OnAsyncResFinshCallBack resFinshCallBack = null;
    public ResourceObject resObj = null;
    public object param1 = null;
    public object param2 = null;
    public object param3 = null;

    public void Reset()
    {
        finishCallBack = null;
        resFinshCallBack = null;
        resObj = null;
        param1 = null;
        param2 = null;
        param3 = null;
    }
}
/// <summary>
/// 异步加载调用的回调委托
/// </summary>
public delegate void OnAsyncFinishCallBack(string path, Object obj, object param1, object param2, object param3);
/// <summary>
/// 用于ObjectManager加载ResourceManager资源后回调的委托
/// </summary>
public delegate void OnAsyncResFinshCallBack(string path, ResourceObject resObj, object param1, object param2, object param3);

public class ResourceManager:Singleton<ResourceManager>
{
    public bool isLoadFromAssetBundle = false;
    //缓存所有正在使用的资源列表
    public Dictionary<uint, ResourceItem> AssetDict { set; get; } = new Dictionary<uint, ResourceItem>();
    //缓存引用为0的资源列表，当列表数达到指定最大值后，清除其使用率最低的（就是列表尾部）
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();

    //用于开启协程的Mono脚本
    protected MonoBehaviour m_Mono;
    //需要异步加载的资源列表（等待，需要加载的列表）
    protected List<AsyncLoadResParam>[] m_LoadAsyncResList = new List<AsyncLoadResParam>[(int)LoadPriority.RES_NUM];
    //正在异步加载的资源列表（正在加载，还没加载完）
    protected Dictionary<uint, AsyncLoadResParam> m_LoadingAssetDict = new Dictionary<uint, AsyncLoadResParam>();

    protected ClassObjectPool<AsyncLoadResParam> m_AsyncLoadResPool = ObjectManager.Instance.GetOrCreateClassPool<AsyncLoadResParam>(50);
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = ObjectManager.Instance.GetOrCreateClassPool<AsyncCallBack>(100);

    public void Init(MonoBehaviour mono)
    {
        for (int i = 0; i < (int)LoadPriority.RES_NUM; i++)
        {
            m_LoadAsyncResList[i] = new List<AsyncLoadResParam>();
        }
        m_Mono = mono;
        m_Mono.StartCoroutine(LoadAssetAsync());
    }

    //同步资源加载----------------------------------------------------------------------------------

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="path"></param>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path)) return null;
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return item.m_Obj as T;
        }
        T obj = null;
#if UNITY_EDITOR
        if (!isLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.GetResourceItem(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = (T)item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadAssetByEditor<T>(path);
            }
        }

#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj as T;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);

        return obj;
    }
#if UNITY_EDITOR
    private T LoadAssetByEditor<T>(string path) where T:UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif
    /// <summary>
    /// 获取缓存的资源项
    /// </summary>
    private ResourceItem GetCacheResourceItem(uint crc,int addRefCount = 1)
    {
        ResourceItem item = null;
        if (AssetDict.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount += addRefCount;
            item.m_LastUseTime = Time.realtimeSinceStartup;
        }
        return item;
    }
    /// <summary>
    /// 用于ObjectManager获取资源对象的接口
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="isClear">是否跳转场景清除</param>
    public ResourceItem GetResourceItem(string path,bool isClear)
    {
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return item;
        }
        Object obj = null;
#if UNITY_EDITOR
        if (!isLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.GetResourceItem(crc);
            if (item != null && item.m_Obj != null)
            {
                obj = item.m_Obj;
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        item.m_IsClear = isClear;
        return item;
    }
    /// <summary>
    /// 缓存资源项
    /// </summary>
    private void CacheResourceItem(string path, ref ResourceItem item, uint crc, UnityEngine.Object obj, int addRefCount = 1)
    {
        WashOut();

        if (item == null)
        {
            Debug.LogError("item is null in path :" + path); return;
        }
        if (obj == null)
        {
            Debug.LogError("obj is null in path :" + path); return;
        }
        item.m_Obj = obj;
        item.m_GUID = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;
        AssetDict[item.m_Crc] = item;
    }
    /// <summary>
    /// 不需要实例化的资源项卸载
    /// </summary>
    public bool ReleaseResource(UnityEngine.Object obj, bool isDestory = false)
    {
        if (obj == null) return false;
        ResourceItem item = null;
        foreach (ResourceItem tempItem in AssetDict.Values)
        {
            if (tempItem.m_GUID == obj.GetInstanceID())
            {
                item = tempItem;
                break;
            }
        }
        if (item == null)
        {
            Debug.LogError("资源可能释放多次，或者不存在该资源 :" + obj.name);
            return false;
        }
        item.RefCount--;
        RecycleResourceItem(item, isDestory);
        return true;
    }
    public bool ReleaseResource(ResourceObject resObj, bool isDestory = false)
    {
        if (resObj == null) return false;
        ResourceItem item = null;
        if (!AssetDict.TryGetValue(resObj.m_Crc, out item) || item == null)
        {
            Debug.LogError("AssetDict里不存在该资源:" + resObj.m_GameObj.name + " 可能释放了多次");
        }
        GameObject.Destroy(resObj.m_GameObj);

        item.RefCount--;
        RecycleResourceItem(item, isDestory);
        return true;
    }
    /// <summary>
    /// 增加引用数
    /// </summary>
    public int IncreaseRef(uint crc, int addCount = 1)
    {
        ResourceItem item = null;
        if (AssetDict.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount += addCount;
        }
        return item == null ? 0 : item.RefCount;
    }
    /// <summary>
    /// 减去引用数
    /// </summary>
    public int DecreaseRef(uint crc, int desCount = 1)
    {
        ResourceItem item = null;
        if (AssetDict.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount -= desCount;
        }
        return item == null ? 0 : item.RefCount;
    }
    /// <summary>
    /// 按路径释放资源
    /// </summary>
    public bool ReleaseResource(string path, bool isDestory = false)
    {
        if (string.IsNullOrEmpty(path)) return false;
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = null;
        if (!AssetDict.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError("资源可能释放多次，或者不存在该资源 :" + path);
            return false;
        }
        
        item.RefCount--;
        RecycleResourceItem(item, isDestory);
        return true;
    }
    /// <summary>
    /// 清理资源缓存（策略：当运行内存>80%时）
    /// </summary>
    private void WashOut()
    {
        while (m_NoRefrenceAssetMapList.Size >= ConstConfig.MAX_CACHECOUNT)
        {
            for (int i = 0; i < ConstConfig.MAX_CACHECOUNT / 2; i++)
            {
                ResourceItem item = m_NoRefrenceAssetMapList.Pop();
                RecycleResourceItem(item, true);
            }
        }
    }
    /// <summary>
    /// 回收资源项
    /// </summary>
    /// <param name="isDestory">是否清除（是否寄存）</param>
    private bool RecycleResourceItem(ResourceItem item, bool isDestory = false)
    {
        if (item == null || item.RefCount > 0) return false;

        if (!AssetDict.Remove(item.m_Crc)) return false;
        if (!isDestory)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return true;
        }
        m_NoRefrenceAssetMapList.Remove(item);
        
        //释放AB资源
        AssetBundleManager.Instance.ReleaseAssetBundle(item);
        //清空对象池引用
        ObjectManager.Instance.ClearPoolObject(item.m_Crc);

        if (item.m_Obj != null)
        {
            item.m_Obj = null;
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
#endif
        }

        return true;
    }
    /// <summary>
    /// 清空缓存区（如界面跳转时，清空一下缓存）
    /// </summary>
    public void ClearCache()
    {
        while (m_NoRefrenceAssetMapList.Size > 0)
        {
            ResourceItem item = m_NoRefrenceAssetMapList.Pop();
            RecycleResourceItem(item, true);
        }
        m_NoRefrenceAssetMapList.Clear();
    }
    /// <summary>
    /// 清空当前正在使用的资源
    /// </summary>
    public void ClearAsset()
    {
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (ResourceItem item in AssetDict.Values)
        {
            if (item.m_IsClear)
            {
                tempList.Add(item);
            }
        }
        foreach (ResourceItem item in tempList)
        {
            RecycleResourceItem(item, item.m_IsClear);
        }
    }
    /// <summary>
    /// 预加载资源
    /// </summary>
    public void PreloadRes(string path)
    {
        if (string.IsNullOrEmpty(path)) return;
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return;
        }
        Object obj = null;
#if UNITY_EDITOR
        if (!isLoadFromAssetBundle)
        {
            item = AssetBundleManager.Instance.GetResourceItem(crc);
            if (item != null && item.m_Obj != null)
            {
                obj = item.m_Obj;
            }
            else
            {
                if (item == null)
                {
                    item = new ResourceItem();
                    item.m_Crc = crc;
                }
                obj = LoadAssetByEditor<Object>(path);
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                if (item.m_Obj != null)
                {
                    obj = item.m_Obj;
                }
                else
                {
                    obj = item.m_AssetBundle.LoadAsset<Object>(item.m_AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        ReleaseResource(obj, false);
    }

    //异步资源加载-----------------------------------------------------------------------------------
   
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="finishCallBack">完成后回调</param>
    /// <param name="loadPriority">加载优先级</param>
    /// <param name="param1">回调参数1</param>
    /// <param name="param2">回调参数2</param>
    /// <param name="param3">回调参数3</param>
    public void AsyncLoadResource(string path, OnAsyncFinishCallBack finishCallBack, LoadPriority loadPriority,bool isSprite=false,
        object param1 = null, object param2 = null, object param3 = null)
    {
        uint crc = CRC32.GetCRC32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            finishCallBack?.Invoke(path, item.m_Obj, param1, param2, param3);
            return;
        }

        //判断当前要加载的资源是否已存在加载列表中，不存在，则加入加载列表
        AsyncLoadResParam param = null;
        if (!m_LoadingAssetDict.TryGetValue(crc, out param) || param == null)
        {
            param = m_AsyncLoadResPool.Spawn();
            param.m_Crc = crc;
            param.m_Path = path;
            param.m_Priority = loadPriority;
            param.m_IsSprite = isSprite;
            m_LoadAsyncResList[(int)loadPriority].Add(param);
            m_LoadingAssetDict.Add(crc, param);
        }
        //回调列表里添加回调
        AsyncCallBack asyncCallBack = m_AsyncCallBackPool.Spawn();
        asyncCallBack.finishCallBack = finishCallBack;
        asyncCallBack.param1 = param1;
        asyncCallBack.param2 = param2;
        asyncCallBack.param3 = param3;
        param.callBackList.Add(asyncCallBack);
    }
    /// <summary>
    /// 异步加载资源
    /// 顺序：每次加载待加载列表的第一个资源，按优先级高低依次加载
    /// </summary>
    IEnumerator LoadAssetAsync()
    {
        List<AsyncCallBack> callBackList = null;
        long lastYiledTime = System.DateTime.Now.Ticks;   //上一次yield时间
        while (true)
        {
            for (int i = 0; i < (int)LoadPriority.RES_NUM; i++)
            {
                if (m_LoadAsyncResList[(int)LoadPriority.RES_HIGHT].Count > 0)
                {
                    i = (int)LoadPriority.RES_HIGHT;
                }
                else if (m_LoadAsyncResList[(int)LoadPriority.RES_MIDDLE].Count > 0)
                {
                    i = (int)LoadPriority.RES_MIDDLE;
                }

                List<AsyncLoadResParam> loadingList = m_LoadAsyncResList[i];
                if (loadingList.Count <= 0)
                    continue;

                AsyncLoadResParam loadingItem = loadingList[0];
                loadingList.RemoveAt(0);
                callBackList = loadingItem.callBackList;

                Object obj = null;
                ResourceItem item = null;
#if UNITY_EDITOR
                if (!isLoadFromAssetBundle)
                {
                    if (loadingItem.m_IsSprite)
                    {
                        obj = LoadAssetByEditor<Sprite>(loadingItem.m_Path);
                    }
                    else
                    {
                        obj = LoadAssetByEditor<Object>(loadingItem.m_Path);
                    }
                    //模拟异步加载
                    yield return new WaitForSeconds(0.5f);

                    item = AssetBundleManager.Instance.GetResourceItem(loadingItem.m_Crc);
                    if (item == null)
                    {
                        item = new ResourceItem();
                        item.m_Crc = loadingItem.m_Crc;
                    }
                }
#endif
                if (obj == null)
                {
                    item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.m_Crc);
                    if (item != null && item.m_AssetBundle != null)
                    {
                        AssetBundleRequest abRequest = null;
                        if (loadingItem.m_IsSprite)
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Sprite>(item.m_AssetName);   //由于Object不能转成Spite类型
                        }
                        else
                        {
                            abRequest = item.m_AssetBundle.LoadAssetAsync<Object>(item.m_AssetName);
                        }
                        yield return abRequest;
                        if (abRequest != null && abRequest.isDone)
                        {
                            obj = abRequest.asset;
                        }
                    }
                }
                //缓存资源项
                CacheResourceItem(loadingItem.m_Path, ref item, item.m_Crc, obj, callBackList.Count);
                //遍历执行所有回调
                foreach (AsyncCallBack callBack in callBackList)
                {
                    if (callBack != null)
                    {
                        if (callBack.resFinshCallBack != null && callBack.resObj != null)
                        {
                            ResourceObject tempObj = callBack.resObj;
                            tempObj.m_ResItem = item;
                            callBack.resFinshCallBack(loadingItem.m_Path, tempObj, tempObj.m_Param1, tempObj.m_Param2, tempObj.m_Param3);
                            callBack.resFinshCallBack = null;
                            tempObj = null;
                        }
                        if (callBack.finishCallBack != null)
                        {
                            callBack.finishCallBack(loadingItem.m_Path, obj, callBack.param1, callBack.param2, callBack.param3);
                            callBack.finishCallBack = null;
                        }
                    }
                    callBack.Reset();
                    m_AsyncCallBackPool.UnSpawn(callBack);
                }
                //清空资源
                obj = null;
                callBackList.Clear();
                m_LoadingAssetDict.Remove(loadingItem.m_Crc);

                loadingItem.Reset();
                m_AsyncLoadResPool.UnSpawn(loadingItem);

                if (System.DateTime.Now.Ticks - lastYiledTime > ConstConfig.MAX_LOADRESTIME)
                {
                    yield return null;
                    lastYiledTime = System.DateTime.Now.Ticks;
                }
            }
            if (System.DateTime.Now.Ticks - lastYiledTime > ConstConfig.MAX_LOADRESTIME)
            {
                yield return null;
                lastYiledTime = System.DateTime.Now.Ticks;
            }
        }
    }
    public bool CancelAsyncLoad(ResourceObject resObj)
    {
        AsyncLoadResParam asyncParam = null;
        //首先获取是否存在该异步加载资源
        if (m_LoadingAssetDict.TryGetValue(resObj.m_Crc, out asyncParam) && m_LoadAsyncResList[(int)resObj.m_LoadPriority].Contains(asyncParam))
        {
            for (int i = asyncParam.callBackList.Count - 1; i >= 0; i--) 
            {
                AsyncCallBack tempCallBack = asyncParam.callBackList[i];
                //这个判断是，当有多个物体A,B都需要加载同样的资源时，如果A取消了，但是B不能取消
                if (tempCallBack != null && tempCallBack.resObj == resObj)
                {
                    tempCallBack.Reset();
                    m_AsyncCallBackPool.UnSpawn(tempCallBack);
                    asyncParam.callBackList.RemoveAt(i);
                }
            }
            //在上层遍历完，如果异步回调列表空了，则代表不再需要异步加载该资源了，因此直接清空回收
            if (asyncParam.callBackList.Count <= 0)
            {
                asyncParam.Reset();
                m_LoadAsyncResList[(int)resObj.m_LoadPriority].Remove(asyncParam);
                m_LoadingAssetDict.Remove(resObj.m_Crc);
                m_AsyncLoadResPool.UnSpawn(asyncParam);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 用于ObjectManager异步加载接口
    /// </summary>
    public void AsyncLoadResObj(string path, ResourceObject resObj, OnAsyncResFinshCallBack resLoadCallBack)
    {
        ResourceItem item = GetCacheResourceItem(resObj.m_Crc);
        if (item != null)
        {
            resObj.m_ResItem = item;
            resLoadCallBack?.Invoke(path, resObj, resObj.m_Param1, resObj.m_Param2, resObj.m_Param3);
            resLoadCallBack = null;
            return;
        }

        //判断当前要加载的资源是否已存在加载列表中，不存在，则加入加载列表
        AsyncLoadResParam param = null;
        if (!m_LoadingAssetDict.TryGetValue(resObj.m_Crc, out param) || param == null)
        {
            param = m_AsyncLoadResPool.Spawn();
            param.m_Crc = resObj.m_Crc;
            param.m_Path = path;
            param.m_Priority = resObj.m_LoadPriority;
            m_LoadingAssetDict.Add(resObj.m_Crc, param);
            m_LoadAsyncResList[(int)resObj.m_LoadPriority].Add(param);
        }
        //回调列表里添加回调
        AsyncCallBack asyncCallBack = m_AsyncCallBackPool.Spawn();
        asyncCallBack.resObj = resObj;
        asyncCallBack.resFinshCallBack = resLoadCallBack;
        param.callBackList.Add(asyncCallBack);
    }
}


public class CMapList<T> where T : class, new()
{
    DoubleLinedList<T> m_DoubleLinedList = new DoubleLinedList<T>();
    Dictionary<T, DoubleLinedListNode<T>> m_FindMap = new Dictionary<T, DoubleLinedListNode<T>>();

    ~CMapList()
    {
        Clear();
    }

    public int Size { get { return m_FindMap.Count; } }

    /// <summary>
    /// 插入至头部
    /// </summary>
    public void InsertToHead(T t)
    {
        DoubleLinedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DoubleLinedList.AddToHeader(node);
            return;
        }
        m_DoubleLinedList.AddToHeader(t);
        m_FindMap.Add(t, m_DoubleLinedList.Head);
    }
    /// <summary>
    /// 插入至尾部
    /// </summary>
    public void InsertTail(T t)
    {
        DoubleLinedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DoubleLinedList.AddToTailer(node);
            return;
        }
        m_DoubleLinedList.AddToTailer(t);
        m_FindMap.Add(t, m_DoubleLinedList.Tail);
    }
    public void Remove(T t)
    {
        DoubleLinedListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null) return;
        m_DoubleLinedList.RemoveNode(node);
        m_FindMap.Remove(t);
    }
    /// <summary>
    /// 获取并弹出
    /// </summary>
    public T Pop()
    {
        T t = null;
        if (m_DoubleLinedList.Tail != null)
        {
            t = m_DoubleLinedList.Tail.t;
            Remove(t);
        }
        return t;
    }

    /// <summary>
    /// 获取头
    /// </summary>
    public T Peek()
    {
        return m_DoubleLinedList.Head == null ? null : m_DoubleLinedList.Head.t;
    }
    /// <summary>
    /// 获取尾
    /// </summary>
    public T Back()
    {
        return m_DoubleLinedList.Tail == null ? null : m_DoubleLinedList.Tail.t;
    }
    /// <summary>
    /// 是否包含
    /// </summary>
    public bool Contains(T t)
    {
        DoubleLinedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 移至头结点
    /// </summary>
    public bool MoveToHead(T t)
    {
        DoubleLinedListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            return m_DoubleLinedList.MoveToHeader(node);
        }
        return false;
    }
    /// <summary>
    /// 清理列表
    /// </summary>
    public void Clear()
    {
        while (m_DoubleLinedList.Tail != null)
        {
            Remove(m_DoubleLinedList.Tail.t);
        }
    }
}

//双向链表
public class DoubleLinedList<T> where T : class, new()
{
    //头结点
    public DoubleLinedListNode<T> Head = null;
    //尾结点
    public DoubleLinedListNode<T> Tail = null;
    //双向列表结点资源池
    protected ClassObjectPool<DoubleLinedListNode<T>> m_DoubleLinedNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinedListNode<T>>(500);
    //结点数
    protected int m_Count = 0;
    public int Count { get { return m_Count; } }

    /// <summary>
    /// 添加结点至头部
    /// </summary>
    public DoubleLinedListNode<T> AddToHeader(T t)
    {
        DoubleLinedListNode<T> pNode = m_DoubleLinedNodePool.Spawn();
        pNode.t = t;
        return AddToHeader(pNode);
    }
    /// <summary>
    /// 添加结点至头部
    /// </summary>
    public DoubleLinedListNode<T> AddToHeader(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null) return null;
        pNode.preNode = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            Head.preNode = pNode;
            pNode.nextNode = Head;
            Head = pNode;
        }
        m_Count++;
        return Head;
    }
    /// <summary>
    /// 添加结点至尾部
    /// </summary>
    public DoubleLinedListNode<T> AddToTailer(T t)
    {
        DoubleLinedListNode<T> pNode = m_DoubleLinedNodePool.Spawn();
        pNode.t = t;
        return AddToHeader(pNode);
    }
    /// <summary>
    /// 添加结点至尾部
    /// </summary>
    public DoubleLinedListNode<T> AddToTailer(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null) return null;
        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            Tail.nextNode = pNode;
            pNode.preNode = Tail;
            Tail = pNode;
        }
        m_Count++;
        return Tail;
    }
    /// <summary>
    /// 移除某个结点
    /// </summary>
    public bool RemoveNode(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null) return false;
        //if (!ContainsNode(pNode)) return false;

        if (pNode == Head)
        {
            Head = Head.nextNode;
            Head.preNode = null;
        }
        if (pNode == Tail)
        {
            Tail = Tail.preNode;
            Tail.nextNode = null;
        }

        if (pNode.preNode != null)
        {
            pNode.preNode.nextNode = pNode.nextNode;
        }
        if (pNode.nextNode != null)
        {
            pNode.nextNode.preNode = pNode.preNode;
        }

        pNode.Reset();
        m_DoubleLinedNodePool.UnSpawn(pNode);
        m_Count--;
        return true;
    }
    /// <summary>
    /// 判断结点是否在列表内
    /// </summary>
    //public bool ContainsNode(DoubleLinedListNode<T> pNode)
    //{
    //    DoubleLinedListNode<T> tempNode = Head;
    //    for (int i = 0; i < m_Count; i++)
    //    {
    //        if (pNode == tempNode)
    //        {
    //            return true;
    //        }
    //        tempNode = tempNode.nextNode;
    //    }
    //    Debug.LogError("该结点不在双向列表内，无法进行操作!");
    //    return false;
    //}
    /// <summary>
    /// 将某个结点移至头结点
    /// </summary>
    public bool MoveToHeader(DoubleLinedListNode<T> pNode)
    {
        if (pNode == null) return false;
        if (pNode.preNode == null && pNode.nextNode == null) return false;
        //if (!ContainsNode(pNode)) return false;

        if (Tail == pNode)
        {
            Tail = pNode.preNode;
        }
        if (pNode.preNode != null)
        {
            pNode.preNode.nextNode = pNode.nextNode;
        }
        if (pNode.nextNode != null)
        {
            pNode.nextNode.preNode = pNode.preNode;
        }
        pNode.nextNode = Head;
        Head.preNode = pNode;
        Head = pNode;
        pNode.preNode = null;
        //当列表只有1-2个结点时，会出现尾结点为空，这里加一个判断
        if (Tail == null)
        {
            Tail = Head;
        }
        return true;
    }
}
//双向链表结点结构
public class DoubleLinedListNode<T> where T:class,new()
{
    public DoubleLinedListNode<T> preNode = null;
    public DoubleLinedListNode<T> nextNode = null;
    public T t = null;

    public void Reset()
    {
        preNode = null;
        nextNode = null;
        t = null;
    }
}
