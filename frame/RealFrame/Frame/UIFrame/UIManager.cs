using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public delegate void OnAsyncLoadUIFinish(BasePanel basePanel);

public class AsyncLoadPanelParam
{
    public UIPanelType m_PanelType;
    public OnAsyncLoadUIFinish m_FinishCallBack;
    public object[] paramList;

    public void Reset()
    {
        m_PanelType = UIPanelType.None;
        m_FinishCallBack = null;
        paramList = null;
    }
}

public class UIManager : Singleton<UIManager>
{
    public RectTransform m_UIRoot;
    private RectTransform m_PanelRoot;
    private Camera m_UICamera;
    private EventSystem m_EventSystem;
    //屏幕高宽比
    private float m_CanvasRate = 0;
    private MonoBehaviour m_Mono;

    private Dictionary<UIPanelType, BasePanel> m_PanelDict;//保存所有实例化面板的游戏物体身上的BasePanel组件
    private Stack<BasePanel> m_PanelStack;
    //异步加载Panel列表
    private List<AsyncLoadPanelParam> m_AsyncLoadPanelParamList = new List<AsyncLoadPanelParam>();
    //需要pop的数量
    private int m_PopCount = 0;
    //异步加载Panel参数类池
    private ClassObjectPool<AsyncLoadPanelParam> m_AsyncPanelParamPool = ObjectManager.Instance.GetOrCreateClassPool<AsyncLoadPanelParam>(50);

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uiRoot">UI根目录</param>
    /// <param name="panelRoot">Panel根目录</param>
    /// <param name="uiCamera">UI相机</param>
    /// <param name="eventSystem"></param>
    public void Init(RectTransform uiRoot, RectTransform panelRoot, Camera uiCamera, EventSystem eventSystem,MonoBehaviour mono)
    {
        m_UIRoot = uiRoot;
        m_PanelRoot = panelRoot;
        m_UICamera = uiCamera;
        m_EventSystem = eventSystem;
        m_CanvasRate = Screen.height / (m_UICamera.orthographicSize * 2);
        m_Mono = mono;
        m_Mono.StartCoroutine(AysncPushPanel());
    }

    /// <summary>
    /// 异步加载面板
    /// </summary>
    /// <param name="panelType">panel类型</param>
    /// <param name="finishCallBack">panel加载完成后回调</param>
    /// <param name="paramList">panel加载需要携带的参数</param>
    public void PushPanelSync(UIPanelType panelType,OnAsyncLoadUIFinish finishCallBack,params object[] paramList)
    {
        if (panelType == UIPanelType.None) return;
        AsyncLoadPanelParam param = m_AsyncPanelParamPool.Spawn();
        if (param != null)
        {
            param.m_PanelType = panelType;
            param.m_FinishCallBack = finishCallBack;
            param.paramList = paramList;
            m_AsyncLoadPanelParamList.Add(param);
        }
    }
    /// <summary>
    /// 异步加载面板协程
    /// </summary>
    IEnumerator AysncPushPanel()
    {
        while (true)
        {
            while (m_AsyncLoadPanelParamList.Count > 0)
            {
                AsyncLoadPanelParam param = m_AsyncLoadPanelParamList[0];
                m_AsyncLoadPanelParamList.RemoveAt(0);

                BasePanel basePanel = PushPanel(param.m_PanelType, paramList: param.paramList);
                param.m_FinishCallBack.Invoke(basePanel);

                param.Reset();
                m_AsyncPanelParamPool.UnSpawn(param);

                yield return null;
            }
            while (m_PopCount > 0)
            {
                m_PopCount--;
                PopPanel();
                yield return null;
            }
            m_AsyncLoadPanelParamList.Clear();
            yield return null;
        }
    }

    /// <summary>
    /// 把某个页面入栈，把某个页面显示在界面上
    /// </summary>
    public BasePanel PushPanel(UIPanelType panelType, bool isTop = true, params object[] paramList)
    {
        if (panelType == UIPanelType.None) return null;
        if (m_PanelStack == null)
            m_PanelStack = new Stack<BasePanel>();

        //判断一下栈里面是否有页面
        if (m_PanelStack.Count > 0)
        {
            BasePanel topPanel = m_PanelStack.Peek();
            topPanel.OnPause();
        }

        BasePanel panel = GetPanel(panelType);
        if (isTop)
        {
            panel.transform.SetAsLastSibling();
        }
        panel.OnEnter(paramList);
        m_PanelStack.Push(panel);
        return panel;
    }
    /// <summary>
    /// 异步出栈,弹出面板
    /// </summary>
    public void PopPanelSync()
    {
        m_PopCount++;
    }
    /// <summary>
    /// 同步出栈,把页面从界面上移除
    /// </summary>
    public void PopPanel()
    {
        if (m_PanelStack == null)
            m_PanelStack = new Stack<BasePanel>();

        if (m_PanelStack.Count <= 0) return;

        //关闭栈顶页面的显示
        BasePanel topPanel = m_PanelStack.Pop();
        topPanel.OnExit();

        if (m_PanelStack.Count <= 0) return;
        BasePanel topPanel2 = m_PanelStack.Peek();
        topPanel2.OnResume();
    }

    /// <summary>
    /// 根据面板类型 得到实例化的面板
    /// </summary>
    /// <returns></returns>
    private BasePanel GetPanel(UIPanelType panelType)
    {
        if (m_PanelDict == null)
        {
            m_PanelDict = new Dictionary<UIPanelType, BasePanel>();
        }

        BasePanel panel = null;
        if (!m_PanelDict.TryGetValue(panelType, out panel) || panel == null)
        {
            //如果找不到，那么就找这个面板的prefab的路径，然后去根据prefab去实例化面板
            string path = ConstConfig.UIPREFABPATH + panelType.ToString() + ".prefab";
            //GameObject instPanel = GameObject.Instantiate(Resources.Load(path)) as GameObject;
            GameObject instPanel = ObjectManager.Instance.InstantiateObject(path, isClear: false);

            if (instPanel != null)
            {
                instPanel.transform.SetParent(m_PanelRoot, false);
                instPanel.GetComponent<BasePanel>().uiManager = this;
                //instPanel.GetComponent<BasePanel>().gameFacade = gameFacade;
                m_PanelDict.Add(panelType, instPanel.GetComponent<BasePanel>());
                return instPanel.GetComponent<BasePanel>();
            }
            else
            {
                Debug.LogError("未找到panel，路径：" + path);
                return null;
            }
        }
        else
        {
            return panel;
        }
    }

    //public void ShowMessage(string content)
    //{
    //    MessagePanel messagePanel = (MessagePanel)panelDict.TryGet(UIPanelType.MessagePanel);
    //    if (messagePanel == null)
    //    {
    //        Debug.LogError("MessagePanel为空!"); return;
    //    }
    //    messagePanel.ShowMessage(content);
    //}
    //public void ShowMessageSync(string content)
    //{
    //    MessagePanel messagePanel = (MessagePanel)panelDict.TryGet(UIPanelType.MessagePanel);
    //    if (messagePanel == null)
    //    {
    //        Debug.LogError("MessagePanel为空!"); return;
    //    }
    //    messagePanel.ShowMessageSync(content);
    //}

}
