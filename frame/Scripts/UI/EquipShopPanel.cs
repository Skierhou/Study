using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipShopPanel : BasePanel
{
    private Text m_CoinTxt;
    private Button m_AllBtn;
    private Button m_AttackBtn;
    private Button m_AttackRateBtn;
    private Button m_MagicBtn;
    private Button m_AssistBtn;
    private Text m_NameTxt;
    private Text m_DesTxt;
    private Text m_NeedTxt;
    private Button m_BuyBtn;
    private Button m_SellBtn;
    private Button m_CloseBtn;
    private Text m_BuyCoinTxt;
    private Text m_SellCoinTxt;

    private Transform m_LowerGrid;
    private Transform m_MiddleGrid;
    private Transform m_HighGrid;
    private Transform m_SuperGrid;

    private Transform m_EquipGrid;

    private Hero m_Hero;
    private EquipData m_EquipData;
    private List<GameObject> m_EquipGoList = new List<GameObject>();
    private List<GameObject> m_ShowEquipList = new List<GameObject>();
    private Dictionary<EquipType, List<EquipBase>> m_EquipDict = new Dictionary<EquipType, List<EquipBase>>();
    private EquipBase m_CurEquip;

    private void Awake()
    {
        m_EquipData = ConfigManager.Instance.FindData<EquipData>(Consts.Config_EquipData);
        for (int i = 0; i < (int)EquipType.Sum_Count; i++)
        {
            m_EquipDict.Add((EquipType)i,new List<EquipBase>());
        }
        foreach (EquipBase equip in m_EquipData.EquipList)
        {
            m_EquipDict[equip.EquipType].Add(equip);
        }
        m_EquipDict[EquipType.None] = m_EquipData.EquipList;

        m_CloseBtn = GetComponent<Button>();
        m_CoinTxt = transform.Find("Bg/Title/Coin/Text").GetComponent<Text>();
        m_AllBtn = transform.Find("Bg/SelectGrid/AllBtn").GetComponent<Button>();
        m_AttackBtn = transform.Find("Bg/SelectGrid/AttackBtn").GetComponent<Button>();
        m_AttackRateBtn = transform.Find("Bg/SelectGrid/AttackRateBtn").GetComponent<Button>();
        m_MagicBtn = transform.Find("Bg/SelectGrid/MagicBtn").GetComponent<Button>();
        m_AssistBtn = transform.Find("Bg/SelectGrid/AssistBtn").GetComponent<Button>();
        m_NameTxt = transform.Find("Bg/Tip/NameText").GetComponent<Text>();
        m_DesTxt = transform.Find("Bg/Tip/DesText").GetComponent<Text>();
        m_NeedTxt = transform.Find("Bg/Tip/NeedText").GetComponent<Text>();
        m_BuyBtn = transform.Find("Bg/Tip/BuyBtn").GetComponent<Button>();
        m_SellBtn = transform.Find("Bg/Tip/SellBtn").GetComponent<Button>();
        m_LowerGrid = transform.Find("Bg/ScrollRect/ViewPort/Content/LowerGrid");
        m_MiddleGrid = transform.Find("Bg/ScrollRect/ViewPort/Content/MiddleGrid");
        m_HighGrid = transform.Find("Bg/ScrollRect/ViewPort/Content/HighGrid");
        m_SuperGrid = transform.Find("Bg/ScrollRect/ViewPort/Content/SuperGrid");
        m_EquipGrid = transform.Find("Bg/Equip/Grid");
        m_BuyCoinTxt = transform.Find("Bg/Tip/BuyBtn/Text").GetComponent<Text>();
        m_SellCoinTxt = transform.Find("Bg/Tip/SellBtn/Text").GetComponent<Text>();

        m_CloseBtn.onClick.AddListener(CloseBtnClick);
        m_BuyBtn.onClick.AddListener(BuyBtnClick);
        m_SellBtn.onClick.AddListener(SellBtnClick);
        m_AttackBtn.onClick.AddListener(AttackBtnClick);
        m_AttackRateBtn.onClick.AddListener(AttackRateBtnClick);
        m_MagicBtn.onClick.AddListener(MagicBtnClick);
        m_AssistBtn.onClick.AddListener(AssistBtnClick);
        m_AllBtn.onClick.AddListener(AllBtnClick);
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        if (paramList != null && paramList.Length > 0)
        {
            m_Hero = (Hero)paramList[0];
        }
        foreach (GameObject go in m_EquipGoList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_EquipGoList.Clear();
        foreach (EquipBase equip in m_Hero.EquipList)
        {
            GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            itemGo.transform.SetParent(m_EquipGrid);
            itemGo.GetComponent<GridItem>().Init(Tools.LoadSprite(equip.IconPath), ()=> {
                EquipBtnClick(equip);
                m_SellBtn.gameObject.SetActive(true);
                m_SellCoinTxt.text = equip.SellCoin.ToString();
            });
            m_EquipGoList.Add(itemGo);
        }
        AllBtnClick();
    }
    void EquipBtnClick(EquipBase equip)
    {
        m_CurEquip = equip;
        m_NameTxt.text = equip.Name;
        m_DesTxt.text = equip.Des;
        string needStr = "";
        int coin = equip.TotalCoin;
        foreach (int id in equip.PreList)
        {
            EquipBase preEquip = m_EquipData.EquipDict[id];
            if (m_Hero.EquipList.Contains(preEquip))
            {
                needStr += "<color='#ADFF2F'>" + preEquip.Name + "(" + preEquip.Coin + ")√</color>";
                coin -= preEquip.TotalCoin;
            }
            else
            {
                needStr += preEquip.Name + "(" + preEquip.Coin + ")";
            }
        }
        m_NeedTxt.text = needStr;
        m_BuyCoinTxt.text = coin.ToString();
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }

    void CloseBtnClick()
    {
        uiManager.PopPanel();
    }
    void BuyBtnClick()
    { }
    void SellBtnClick()
    { }
    void AllBtnClick()
    {
        ShowEquipGrid(EquipType.None);
    }
    void AttackBtnClick()
    {
        ShowEquipGrid(EquipType.Attack);
    }
    void AttackRateBtnClick()
    {
        ShowEquipGrid(EquipType.AttackRate);
    }
    void MagicBtnClick()
    {
        ShowEquipGrid(EquipType.Magic);
    }
    void AssistBtnClick()
    {
        ShowEquipGrid(EquipType.Auxiliary);
    }
    void ShowEquipGrid(EquipType equipType)
    {
        foreach (GameObject go in m_ShowEquipList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_ShowEquipList.Clear();
        foreach (EquipBase equip in m_EquipDict[equipType])
        {
            GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_EquipGridItem);
            switch (equip.EquipLevel)
            {
                case EquipLevel.Lower:
                    itemGo.transform.SetParent(m_LowerGrid);
                    break;
                case EquipLevel.Middle:
                    itemGo.transform.SetParent(m_MiddleGrid);
                    break;
                case EquipLevel.High:
                    itemGo.transform.SetParent(m_HighGrid);
                    break;
                case EquipLevel.Super:
                    itemGo.transform.SetParent(m_SuperGrid);
                    break;
            }
            itemGo.GetComponent<EquipGridItem>().Init(equip, () => {
                EquipBtnClick(equip);
                m_SellBtn.gameObject.SetActive(false);
            });
            m_ShowEquipList.Add(itemGo);
        }
    }
}
