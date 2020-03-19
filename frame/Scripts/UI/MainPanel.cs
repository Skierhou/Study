using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : BasePanel
{
    private Text m_NameText;
    private Text m_DiamondText;
    private Button m_AboutUsBtn;
    private Button m_NewGameBtn;
    private Button m_EvolutionBtn;
    private Button m_AchievementBtn;
    private Button m_ShopBtn;
    private Button m_SettingBtn;

    private void Awake()
    {
        m_NameText = transform.Find("Top/Head/NameText").GetComponent<Text>();
        m_DiamondText = transform.Find("Top/Diamond/DiamondText").GetComponent<Text>();
        m_AboutUsBtn = transform.Find("Bottom/AboutUs").GetComponent<Button>();
        m_NewGameBtn = transform.Find("Bottom/NewGame").GetComponent<Button>();
        m_EvolutionBtn = transform.Find("Bottom/Evolution").GetComponent<Button>();
        m_AchievementBtn = transform.Find("Bottom/Achievement").GetComponent<Button>();
        m_ShopBtn = transform.Find("Bottom/Shop").GetComponent<Button>();
        m_SettingBtn = transform.Find("Bottom/Setting").GetComponent<Button>();

        m_AboutUsBtn.onClick.AddListener(AboutUsBtnClick);
        m_NewGameBtn.onClick.AddListener(NewGameBtnClick);
        m_EvolutionBtn.onClick.AddListener(EvolutionBtnClick);
        m_AchievementBtn.onClick.AddListener(AchievementBtnClick);
        m_ShopBtn.onClick.AddListener(ShopBtnClick);
        m_SettingBtn.onClick.AddListener(SettingBtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }
    public override void OnPause()
    {
        OnExit();
    }
    public override void OnResume()
    {
        OnEnter();
    }

    void AboutUsBtnClick()
    { }
    void NewGameBtnClick()
    {
        GamePanel gamePanel = (GamePanel)uiManager.PushPanel(UIPanelType.GamePanel);
        GameManager.Instance.StartGame(gamePanel);
    }
    void EvolutionBtnClick()
    {
        uiManager.PushPanel(UIPanelType.EvolutionPanel);
    }
    void AchievementBtnClick()
    {
        uiManager.PushPanel(UIPanelType.AchievementPanel);
    }
    void ShopBtnClick()
    {
        uiManager.PushPanel(UIPanelType.ShopPanel);
    }
    void SettingBtnClick()
    {
        uiManager.PushPanel(UIPanelType.SettingPanel);
    }
}
