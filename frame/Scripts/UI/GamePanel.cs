using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    private Text m_CoinText;
    private Text m_WaveText; 
    private Text m_LimitText;
    private Text m_KillText;
    private Text m_PopulationText;
    private Button m_AcclerateBtn;
    private Text m_AcclerateText;
    private GameObject m_StartTip;
    private Button m_StartBtn;
    private Button m_SettingBtn;
    private Text m_DifficultText;
    private Text m_TipText;

    private PlayableDirector m_CoinAnim;
    private PlayableDirector m_WaveAnim;
    private PlayableDirector m_LimitAnim;
    private PlayableDirector m_KillAnim;
    private PlayableDirector m_PopulationAnim;

    private int m_Coin;
    public int Coin {
        set {
            m_Coin = value;
            m_CoinText.text = m_Coin.ToString();
            m_CoinAnim.Play();
        }
        get { return m_Coin; }
    }
    private int m_Wave;
    public int Wave
    {
        set
        {
            m_Wave = value;
            m_WaveText.text = m_Wave.ToString();
            m_WaveAnim.Play();
        }
        get { return m_Wave; }
    }
    private int m_Limit;
    public int Limit
    {
        set
        {
            m_Limit = value;
            m_LimitText.text = m_Limit + " / " + ModelManager.Instance.MaxLimit;
            m_LimitAnim.Play();
        }
        get { return m_Limit; }
    }
    private int m_Kill;
    public int Kill
    {
        set
        {
            m_Kill = value;
            m_KillText.text = m_Kill.ToString();
            m_KillAnim.Play();
        }
        get { return m_Kill; }
    }
    private int m_Population;
    public int Population
    {
        set
        {
            m_Population = value;
            m_PopulationText.text = m_Population+" / " +ModelManager.Instance.MaxPopulation;
            m_PopulationAnim.Play();
        }
        get { return m_Population; }
    }

    private int acclerate = 0;

    private void Awake()
    {
        m_CoinText = transform.Find("Coin/Text").GetComponent<Text>();
        m_WaveText = transform.Find("Wave/Text").GetComponent<Text>();
        m_LimitText = transform.Find("Limit/Text").GetComponent<Text>();
        m_KillText = transform.Find("Kill/Text").GetComponent<Text>();
        m_PopulationText = transform.Find("Population/Text").GetComponent<Text>();
        m_DifficultText = transform.Find("DifficultText").GetComponent<Text>();
        m_TipText = transform.Find("TipText").GetComponent<Text>();
        m_AcclerateBtn = transform.Find("AcclerateBtn").GetComponent<Button>();
        m_AcclerateText = transform.Find("AcclerateBtn/Text").GetComponent<Text>();
        m_StartBtn = transform.Find("StartTip").GetComponentInChildren<Button>();
        m_SettingBtn = transform.Find("SettingBtn").GetComponent<Button>();
        m_StartTip = transform.Find("StartTip").gameObject;
        m_CoinAnim = transform.Find("Coin").GetComponent<PlayableDirector>();
        m_WaveAnim = transform.Find("Wave").GetComponent<PlayableDirector>();
        m_LimitAnim = transform.Find("Limit").GetComponent<PlayableDirector>();
        m_KillAnim = transform.Find("Kill").GetComponent<PlayableDirector>();
        m_PopulationAnim = transform.Find("Population").GetComponent<PlayableDirector>();


        m_AcclerateBtn.onClick.AddListener(AcclerateBtnClick);
        m_SettingBtn.onClick.AddListener(SettingBtnClick);
        m_StartBtn.onClick.AddListener(StartGameBtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        //更新显示
        Coin = ModelManager.Instance.StartCoin;
        Wave = 0;
        Limit = 0; 
        Population = 0;
        Kill = 0;
        m_StartTip.SetActive(true);
        m_TipText.text = "";
    }
    IEnumerator StartWave()
    {
        float time = ModelManager.Instance.WaitingTime;
        Wave++;
        EnemyManager.Instance.Spawn(Wave, NextWave);
        while (time > 0)
        {
            time -= Time.deltaTime;
            m_TipText.text = "下一波怪物\n" + time.ToString("f0") + "S\n后出现";
            yield return null;
        }
    }
    void NextWave()
    {
        StartCoroutine(StartWave());
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }
   
    void AcclerateBtnClick()
    {
        if (acclerate < ModelManager.Instance.MaxAcclerate)
        {
            acclerate++;
            m_AcclerateText.text = acclerate.ToString();
        }
        else
        {
            acclerate = 0;
            m_AcclerateText.text = "I I";
        }
        Time.timeScale = acclerate;
    }
    void SettingBtnClick()
    {

    }
    void StartGameBtnClick()
    {
        m_StartTip.SetActive(false);
        StartCoroutine(StartWave());
    }
}
