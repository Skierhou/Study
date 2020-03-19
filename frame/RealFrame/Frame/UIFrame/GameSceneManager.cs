using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : Singleton<GameSceneManager>
{
    private MonoBehaviour m_Mono;

    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
    }
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName">场景名</param>
    public void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogError("当前出于该场景下：" + sceneName + " 无法重复加载！");
            return;
        }
        SceneManager.LoadScene(sceneName);
        ClearCache();
    }
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName">场景名</param>
    /// <param name="panelType">加载后第一个加载的panel</param>
    /// <param name="paramList">panel需要的参数</param>
    public BasePanel LoadScene(string sceneName, UIPanelType panelType = UIPanelType.None, params object[] paramList)
    {
        LoadScene(sceneName);
        return UIManager.Instance.PushPanel(panelType, paramList: paramList);
    }
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="finishCallBack">加载完成后回调</param>
    /// <param name="sceneName">场景名</param>
    /// <param name="panelType">加载后第一个加载的panel</param>
    /// <param name="paramList">panel需要的参数</param>
    public void LoadSceneAsync(string sceneName, OnAsyncLoadUIFinish finishCallBack, UIPanelType panelType = UIPanelType.None, params object[] paramList)
    {
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogError("当前出于该场景下："+sceneName+" 无法重复加载！");
            return;
        }
        m_Mono.StartCoroutine(StartLoading(sceneName, finishCallBack, panelType, paramList));
    }
    IEnumerator StartLoading(string sceneName, OnAsyncLoadUIFinish finishCallBack, UIPanelType panelType = UIPanelType.None, params object[] paramList)
    {
        BasePanel panel = UIManager.Instance.PushPanel(UIPanelType.AsyncLoadingPanel);
        int displayProgress = 0;
        int toProgress = 0;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
        {
            toProgress = (int)op.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                SetLoadingPercentage(panel,displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }

        toProgress = 100;
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            SetLoadingPercentage(panel, displayProgress);
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = true;
        UIManager.Instance.PopPanel();

        BasePanel basePanel = null;
        if (panelType != UIPanelType.None)
        {
            basePanel = UIManager.Instance.PushPanel(panelType, paramList: paramList);
        }
        ClearCache();
        finishCallBack?.Invoke(basePanel);
    }
    void SetLoadingPercentage(BasePanel panel, float v)
    {
        //if (panel == null) return;
        //panel.ProgressSlider.value = v * 1.0f / 100;
        //panel.ProgressText.text = (int)v + "%";
    }
    private void ClearCache()
    {
        ResourceManager.Instance.ClearAsset();
        ResourceManager.Instance.ClearCache();
    }
}