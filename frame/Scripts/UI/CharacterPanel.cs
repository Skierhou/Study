using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : BasePanel
{
    //Info
    private GameObject m_Info;
    private Image m_HeadImg;
    private Slider m_ExpSlider;
    private Text m_LevelTxt;
    private Text m_ExpTxt;
    private Text m_NameTxt;
    private Text m_AttackTxt;
    private Text m_AttackRateTxt;
    private Text m_AttackRangeTxt;
    private Text m_StrengthTxt;
    private Text m_AgilityTxt;
    private Text m_IntelligenceText;
    private Button m_StrengthBtn;
    private Button m_AgilityBtn;
    private Button m_IntelligenceBtn;
    private Text m_PointTxt;
    private Transform m_BuffGrid;
    private Transform m_SkillGrid;
    private Transform m_EquipGrid;
    private Button m_EquipShopBtn;

    private Button m_InfoBtn;
    private Button m_CancelBtn;
    private Button m_Btn;

    //Character
    private GameObject m_Character;

    //Hero
    private Hero m_Hero;
    private List<GameObject> m_GridGoList = new List<GameObject>();

    private void Awake()
    {
        m_Info = transform.Find("Info").gameObject;
        m_Btn = GetComponent<Button>();
        m_HeadImg = transform.Find("Info/HeadImg/HeadImg").GetComponent<Image>();
        m_ExpSlider = transform.Find("Info/ExpSlider").GetComponent<Slider>();
        m_LevelTxt = transform.Find("Info/ExpSlider/LevelText").GetComponent<Text>();
        m_ExpTxt = transform.Find("Info/ExpSlider/ExpText").GetComponent<Text>();
        m_NameTxt = transform.Find("Info/NameText").GetComponent<Text>();
        m_AttackTxt = transform.Find("Info/Attack/Text").GetComponent<Text>();
        m_AttackRateTxt = transform.Find("Info/AttackRate/Text").GetComponent<Text>();
        m_StrengthTxt = transform.Find("Info/Strength/Text").GetComponent<Text>();
        m_AgilityTxt = transform.Find("Info/Agility/Text").GetComponent<Text>();
        m_IntelligenceText = transform.Find("Info/Intelligence/Text").GetComponent<Text>();
        m_AttackRangeTxt = transform.Find("Info/AttackRange/Text").GetComponent<Text>();
        m_PointTxt = transform.Find("Info/Point/Count").GetComponent<Text>();
        m_StrengthBtn = transform.Find("Info/StengthBtn").GetComponent<Button>();
        m_AgilityBtn = transform.Find("Info/AgilityBtn").GetComponent<Button>();
        m_IntelligenceBtn = transform.Find("Info/IntelligenceBtn").GetComponent<Button>();
        m_BuffGrid = transform.Find("Info/BuffGrid");
        m_SkillGrid = transform.Find("Info/SkillGrid");
        m_EquipGrid = transform.Find("Info/EquipGrid");

        m_InfoBtn = transform.Find("InfoBtn").GetComponent<Button>();
        m_CancelBtn = transform.Find("CancelBtn").GetComponent<Button>();
        m_EquipShopBtn = transform.Find("EquipShopBtn").GetComponent<Button>();
        m_Character = transform.Find("Character").gameObject;

        m_InfoBtn.onClick.AddListener(InfoBtnClick);
        m_CancelBtn.onClick.AddListener(CancelBtnClick);
        m_StrengthBtn.onClick.AddListener(StrengthBtnClick);
        m_AgilityBtn.onClick.AddListener(AgilityBtnClick);
        m_IntelligenceBtn.onClick.AddListener(IntelligenceBtnClick);
        m_EquipShopBtn.onClick.AddListener(EquipShopBtnClick);
        m_Btn.onClick.AddListener(BtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        if (m_Info.activeSelf)
        {
            m_Info.SetActive(false);
        }
        if (paramList != null && paramList.Length > 0)
        {
            m_Hero = (Hero)paramList[0];
        }
        m_HeadImg.sprite = Tools.LoadSprite(m_Hero.Data.IconPath);
        m_NameTxt.text = m_Hero.Data.Name;
        GameManager.Instance.SelectGrid(m_Hero.transform.position);
        
        InvokeRepeating("UpdateUI", 0, 1);
        InvokeRepeating("UpdateGrid", 0, 10);
    }
    public override void OnExit()
    {
        CancelInvoke();
        GameManager.Instance.IsClickBuilcPlace = false;
        GameManager.Instance.CancelSelect();
        gameObject.SetActive(false);
    }
    public override void OnPause()
    {
        gameObject.SetActive(false);
    }
    public override void OnResume()
    {
        gameObject.SetActive(true);
    }
    void UpdateGrid()
    {
        //删除
        foreach (GameObject go in m_GridGoList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_GridGoList.Clear();
        //添加
        foreach (SkillBase skill in m_Hero.ActiveSkillList)
        {
            CreateGridItem(skill.IconPath,m_SkillGrid, null);
        }
        foreach (SkillBase skill in m_Hero.AttackPassiveList)
        {
            CreateGridItem(skill.IconPath, m_SkillGrid, null);
        }
        foreach (SkillBase skill in m_Hero.MagicPassiveList)
        {
            CreateGridItem(skill.IconPath, m_SkillGrid, null);
        }
        foreach (SkillBase skill in m_Hero.DiePassiveList)
        {
            CreateGridItem(skill.IconPath, m_SkillGrid, null);
        }
        foreach (SkillBase skill in m_Hero.BuffList)
        {
            CreateGridItem(skill.IconPath, m_BuffGrid, null);
        }
        foreach (EquipBase equip in m_Hero.EquipList)
        {
            CreateGridItem(equip.IconPath, m_EquipGrid, null);
        }
    }
    void CreateGridItem(string path,Transform parent,System.Action action)
    {
        GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
        itemGo.transform.parent = parent;
        itemGo.GetComponent<GridItem>().Init(Tools.LoadSprite(path), action);
        m_GridGoList.Add(itemGo);
    }
    void UpdateUI()
    {
        m_ExpSlider.value = m_Hero.Data.Exp * 1.0f / m_Hero.Data.NeedExp;
        UpdateAttr(m_AttackTxt, m_Hero.Data.Attack, m_Hero.ExtarData.Attack);
        UpdateAttr(m_AttackRateTxt, m_Hero.Data.AttackRate, m_Hero.ExtarData.AttackRate);
        UpdateAttr(m_AttackRangeTxt, m_Hero.Data.AttackRange, m_Hero.ExtarData.AttackRange);
        UpdateAttr(m_StrengthTxt, m_Hero.Data.Strength, m_Hero.ExtarData.Strengthen);
        UpdateAttr(m_AgilityTxt, m_Hero.Data.Agility, m_Hero.ExtarData.Agility);
        UpdateAttr(m_IntelligenceText, m_Hero.Data.Intelligence, m_Hero.ExtarData.Intelligence);
        m_PointTxt.text = m_Hero.Data.Point.ToString();
        m_LevelTxt.text = "Lv." + m_Hero.Data.Level;
        m_ExpTxt.text = m_Hero.Data.Exp + " / " + m_Hero.Data.NeedExp;
    }
    void UpdateAttr(Text text,float basic,float extra)
    {
        if (extra == 0)
        {
            text.text = basic.ToString();
        }
        else if (extra > 0)
        {
            text.text = basic + "<color='#ADFF2F'>+" + extra + "</color>";
        }
        else
        {
            text.text = basic + "<color='#EE2C2C'>-" + extra + "</color>";
        }
    }
    void InfoBtnClick()
    {
        if (m_Info.activeSelf)
        {
            m_Info.SetActive(false);
        }
        else
        {
            m_Info.SetActive(true);
        }
    }
    void BtnClick()
    {
        if (m_Info.activeSelf)
        {
            m_Info.SetActive(false);
        }
    }
    void CancelBtnClick()
    {
        uiManager.PopPanel();
    }
    void StrengthBtnClick()
    { }
    void AgilityBtnClick()
    { }
    void IntelligenceBtnClick()
    { }
    void EquipShopBtnClick()
    {
        uiManager.PushPanel(UIPanelType.EquipShopPanel,paramList:new object[] { m_Hero });
    }

}
