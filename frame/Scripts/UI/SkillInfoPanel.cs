using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoPanel : BasePanel
{
    private Button m_Btn;
    private Text m_CdText;
    private Image m_IconImg;
    private Text m_DesText;

    private SkillBase m_Skill;

    private void Awake()
    {
        m_Btn = GetComponent<Button>();
        m_CdText = transform.Find("Bg/CD/Text").GetComponent<Text>();
        m_IconImg = transform.Find("Bg/Icon").GetComponent<Image>();
        m_DesText = transform.Find("Bg/DesText").GetComponent<Text>();

        m_Btn.onClick.AddListener(OnBtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        if (paramList != null && paramList.Length > 0)
        {
            m_Skill = (SkillBase)paramList[0];
        }
        m_CdText.text = m_Skill.CD.ToString();
        m_IconImg.sprite = Tools.LoadSprite(m_Skill.IconPath);
        m_DesText.text = m_Skill.Des;
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }
    void OnBtnClick()
    {
        uiManager.PopPanel();
    }
}
