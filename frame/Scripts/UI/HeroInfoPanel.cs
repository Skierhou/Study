using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoPanel : BasePanel
{
    private Image m_IconImg;
    private Text m_NameTxt;
    private Transform m_SkillGrid;
    private Text m_DesTxt;
    private Button m_Btn;

    private void Awake()
    {
        m_IconImg = transform.Find("Bg/Icon").GetComponent<Image>();
        m_NameTxt = transform.Find("Bg/Name").GetComponent<Text>();
        m_SkillGrid = transform.Find("Bg/Skill/SkillGrid");
        m_DesTxt = transform.Find("Bg/Des/Text").GetComponent<Text>();
        m_Btn = GetComponent<Button>();
        m_Btn.onClick.AddListener(CloseBtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        HeroBase heroBase = (HeroBase)paramList[0];
        m_IconImg.sprite = Tools.LoadSprite(heroBase.IconPath);
        m_NameTxt.text = heroBase.Name;
        foreach (int skillId in heroBase.SkillList)
        {
            SkillBase skill = SkillManager.Instance.GetSkill(skillId);
            GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            itemGo.transform.SetParent(m_SkillGrid);
            itemGo.GetComponent<GridItem>().Init(Tools.LoadSprite(skill.IconPath),()=> {
                uiManager.PushPanel(UIPanelType.SkillInfoPanel,paramList:new object[]{ skill });
            });
        }
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }

    void CloseBtnClick()
    {
        uiManager.PopPanel();
    }
}
