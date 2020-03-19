using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipGridItem : MonoBehaviour
{
    private Image m_IconImg;
    private Text m_NameTxt;
    private Text m_CoinTxt;
    private Button m_Btn;

    private EquipBase m_Equip;
    private System.Action m_Action;

    private void Awake()
    {
        m_IconImg = transform.Find("Icon").GetComponent<Image>();
        m_NameTxt = transform.Find("NameText").GetComponent<Text>();
        m_CoinTxt = transform.Find("CoinText").GetComponent<Text>();
        m_Btn = GetComponent<Button>();
        m_Btn.onClick.AddListener(BtnClick);
    }
    public void Init(EquipBase equip,System.Action action)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        m_Equip = equip;
        m_IconImg.sprite = Tools.LoadSprite(equip.IconPath);
        m_NameTxt.text = equip.Name;
        m_CoinTxt.text = equip.TotalCoin.ToString();
        m_Action = action;
    }
    void BtnClick()
    {
        m_Action?.Invoke();
    }
}
