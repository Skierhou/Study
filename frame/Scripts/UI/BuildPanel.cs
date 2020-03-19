using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanel : BasePanel
{
    private Transform m_RaceGrid;
    private Transform m_HeroGrid;
    private Text m_NameText;
    private Text m_DesText;
    private Text m_CoinText;
    private Button m_BuyBtn;
    private Button m_CloseBtn;

    private BuildData m_BuildData;
    private BuildBase m_CurentBuild;

    private List<GameObject> m_HeroIconList = new List<GameObject>();

    private BuildPlace m_BuildPlace;

    private void Awake()
    {
        m_BuildData = ConfigManager.Instance.FindData<BuildData>(Consts.Config_BuildData);

        m_RaceGrid = transform.Find("Bg/RaceGrid/Viewport/Content");
        m_HeroGrid = transform.Find("Bg/HeroGrid/ScrollView/ViewPort/Grid");
        m_NameText = transform.Find("Bg/BuyTip/Name").GetComponent<Text>();
        m_DesText = transform.Find("Bg/BuyTip/Des").GetComponent<Text>();
        m_BuyBtn = transform.Find("Bg/BuyTip/BuyBtn").GetComponent<Button>();
        m_CoinText = m_BuyBtn.GetComponentInChildren<Text>();
        m_CloseBtn = GetComponent<Button>();

        m_BuyBtn.onClick.AddListener(BuyBtnClick);
        m_CloseBtn.onClick.AddListener(CloseBtnClick);

        foreach (BuildBase build in m_BuildData.BuildList)
        {
            GameObject go = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            go.transform.SetParent(m_RaceGrid);
            go.GetComponent<GridItem>().Init(Tools.LoadSprite(build.IconPath), ()=> {
                BuildClick(build);
            });
        }
    }
    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
        if (paramList != null && paramList.Length > 0)
        {
            m_BuildPlace = (BuildPlace)paramList[0];
        }
        GameManager.Instance.SelectGrid(m_BuildPlace.transform.position);
        BuildClick(m_BuildData.BuildList[0]);
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
        GameManager.Instance.IsClickBuilcPlace = false;
        GameManager.Instance.CancelSelect();
    }
    public override void OnPause()
    {
        gameObject.SetActive(false);
    }
    public override void OnResume()
    {
        gameObject.SetActive(true);
    }
    void BuildClick(BuildBase build)
    {
        m_CurentBuild = build;
        m_NameText.text = build.Name;
        m_DesText.text = build.Des;
        m_CoinText.text = build.Coin.ToString();
        //更新可以随机的英雄图标
        foreach (GameObject go in m_HeroIconList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_HeroIconList.Clear();
        List<HeroBase> heroBases = HeroManager.Instance.GetRaceHeroList(build.RaceType);
        foreach (HeroBase hero in heroBases)
        {
            GameObject heroGo = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            heroGo.transform.SetParent(m_HeroGrid);
            heroGo.GetComponent<GridItem>().Init(Tools.LoadSprite(hero.IconPath), ()=>
            {
                uiManager.PushPanel(UIPanelType.HeroInfoPanel, paramList: new object[] { hero }); 
            });

            m_HeroIconList.Add(heroGo);
        }
    }
    
    void BuyBtnClick()
    {
        if (m_CurentBuild != null)
        {
            if (GameManager.Instance.GamePanel.Coin >= m_CurentBuild.Coin)
            {
                GameManager.Instance.GamePanel.Coin -= m_CurentBuild.Coin;
                Hero hero = HeroManager.Instance.SpawnHero(m_CurentBuild.RaceType, Vector3.zero);
                m_BuildPlace.SetHero(hero);
                uiManager.PopPanel();
            }
            else
            {
                Debug.Log("123");
            }
        }
    }
    void CloseBtnClick()
    {
        uiManager.PopPanel();
    }
}
