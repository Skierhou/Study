using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionPanel : BasePanel
{
    private Transform m_RaceGrid;
    private RectTransform m_HeroGrid;
    private Image m_HeroImg;
    private Text m_NameTxt;
    private Text m_DesTxt;
    private RectTransform m_ItemGrid;
    private Button m_CloseBtn;

    private BuildData m_BuildData;
    private EvolutionData m_EvolutionData;
    private Dictionary<RaceType, List<EvolutionBase>> m_RaceEvoluDict = new Dictionary<RaceType, List<EvolutionBase>>();
    //key:HeroId
    private Dictionary<int, List<EvolutionBase>> m_HeroEvoluDict = new Dictionary<int, List<EvolutionBase>>();

    private List<GameObject> m_HeroUIList = new List<GameObject>();
    private List<GameObject> m_EvoluUIList = new List<GameObject>();

    private Transform m_RaceHide;
    private Transform m_HeroHide;

    private void Awake()
    {
        m_BuildData = ConfigManager.Instance.FindData<BuildData>(Consts.Config_BuildData);
        m_EvolutionData = ConfigManager.Instance.FindData<EvolutionData>(Consts.Config_EvolutionData);
        foreach (EvolutionBase evolution in m_EvolutionData.EvolutionList)
        {
            if (!m_RaceEvoluDict.ContainsKey(evolution.RaceType))
            {
                m_RaceEvoluDict.Add(evolution.RaceType, new List<EvolutionBase>());
            }
            if (evolution.HeroId == 0)
            {
                m_RaceEvoluDict[evolution.RaceType].Add(evolution);
            }
            else
            {
                if (!m_HeroEvoluDict.ContainsKey(evolution.HeroId))
                {
                    m_HeroEvoluDict.Add(evolution.HeroId, new List<EvolutionBase>());
                }
                m_HeroEvoluDict[evolution.HeroId].Add(evolution);
            }
        }
        m_RaceGrid = transform.Find("Bg/RaceGrid");
        m_HeroGrid = transform.Find("Bg/HeroRectView/ViewPort/Grid") as RectTransform;
        m_HeroImg = transform.Find("Bg/Icon").GetComponent<Image>();
        m_NameTxt = transform.Find("Bg/Name").GetComponent<Text>();
        m_DesTxt = transform.Find("Bg/Des").GetComponent<Text>();
        m_ItemGrid = transform.Find("Bg/RectView/ViewPort/Grid") as RectTransform;
        m_CloseBtn = transform.Find("Bg/CloseBtn").GetComponent<Button>();

        m_CloseBtn.onClick.AddListener(CloseBtnClick);
    }
    private void Start()
    {
        foreach (BuildBase build in m_BuildData.BuildList)
        {
            GameObject gridItem = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            gridItem.transform.SetParent(m_RaceGrid);
            gridItem.GetComponent<GridItem>().Init(Tools.LoadSprite(build.IconPath), () => {
                RaceBtnClick(gridItem, build);
            });
        }
    }

    public override void OnEnter(params object[] paramList)
    {
        gameObject.SetActive(true);
    }
    public override void OnExit()
    {
        gameObject.SetActive(false);
    }
    void RaceBtnClick(GameObject itemGo,BuildBase build)
    {
        ImgHide(m_RaceHide, itemGo.transform);
        if (m_HeroHide != null)
        {
            m_HeroHide.gameObject.SetActive(false);
        }
        ChangeUI(build.Name, build.Des, Tools.LoadSprite(build.IconPath));
        //更新英雄
        foreach (GameObject go in m_HeroUIList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_HeroUIList.Clear();
        List<HeroBase> heroList = HeroManager.Instance.GetRaceHeroList(build.RaceType);
        foreach (HeroBase hero in heroList)
        {
            GameObject gridItem = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            gridItem.transform.SetParent(m_HeroGrid);
            gridItem.GetComponent<GridItem>().Init(Tools.LoadSprite(hero.IconPath), () => {
                HeroBtnClick(gridItem,hero);
            });
            m_HeroUIList.Add(gridItem);
        }
        float width = heroList.Count * 40 > ((RectTransform)m_HeroGrid.parent).rect.width ? heroList.Count * 40 : ((RectTransform)m_HeroGrid.parent).rect.width;
        m_HeroGrid.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        //更新升级项
        ChangeEvolutionItem(m_RaceEvoluDict[build.RaceType]);
    }
    void HeroBtnClick(GameObject itemGo, HeroBase heroBase)
    {
        ImgHide(m_HeroHide, itemGo.transform);
        ChangeUI(heroBase.Name, heroBase.Des, Tools.LoadSprite(heroBase.IconPath));
        ChangeEvolutionItem(m_HeroEvoluDict[heroBase.Id]);
    }
    void ChangeUI(string name, string des, Sprite sprite)
    {
        m_NameTxt.text = name;
        m_DesTxt.text = des;
        m_HeroImg.sprite = sprite;
    }
    /// <summary>
    /// 更改进化项显示
    /// </summary>
    void ChangeEvolutionItem(List<EvolutionBase> list)
    {
        foreach (GameObject go in m_EvoluUIList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_EvoluUIList.Clear();
        foreach (EvolutionBase evolution in list)
        {
            GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_EvolutionItem);
            itemGo.transform.SetParent(m_ItemGrid);
            itemGo.GetComponent<EvolutionItem>().Init(evolution, () => {
                //升级按钮点击
                if (ModelManager.Instance.Diamond >= evolution.Payment)
                {
                    if (ModelManager.Instance.LevelUpEvolution(evolution))
                    {
                        ModelManager.Instance.Diamond -= evolution.Payment;
                        //更新UI显示
                    }
                }
            });
            m_EvoluUIList.Add(itemGo);
        }
        float height = list.Count * 70 > ((RectTransform)m_ItemGrid.parent).rect.height ? list.Count * 70 : ((RectTransform)m_ItemGrid.parent).rect.height;
        m_ItemGrid.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
    void ImgHide(Transform old,Transform parent)
    {
        if (old == null)
        {
            old = ObjectManager.Instance.InstantiateObject(Consts.UI_ImgHide).transform;
        }
        old.SetParent(parent);
        old.gameObject.SetActive(true);
        old.localPosition = Vector3.zero;
        old.localScale = Vector3.one;
    }
    
    void CloseBtnClick()
    {
        uiManager.PopPanel();
    }
}
