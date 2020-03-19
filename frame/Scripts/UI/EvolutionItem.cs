using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionItem : MonoBehaviour
{
    private Button m_UpBtn;
    private Text m_NameTxt;
    private Text m_DesTxt;
    private Transform m_LevelGrid;
    private Text m_CoinTxt;
    private Image m_CoinImg;
    private System.Action m_Action;
    private List<GameObject> m_LevelGoList = new List<GameObject>();

    private void Awake()
    {
        m_NameTxt = transform.Find("NameText").GetComponent<Text>();
        m_DesTxt = transform.Find("DesText").GetComponent<Text>();
        m_LevelGrid = transform.Find("LevelGrid");
        m_CoinImg = transform.Find("Coin").GetComponentInChildren<Image>();
        m_CoinTxt = transform.Find("Coin").GetComponentInChildren<Text>();

        m_UpBtn = transform.Find("UpBtn").GetComponent<Button>();
        m_UpBtn.onClick.AddListener(UpBtnClick);
    }
    public void Init(EvolutionBase evolution, System.Action action)
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        m_NameTxt.text = evolution.Name;
        m_DesTxt.text = evolution.Des;
        m_CoinImg.sprite = Tools.LoadSprite(Consts.Texture_Diamond);
        m_CoinTxt.text = evolution.Payment.ToString();

        foreach (GameObject go in m_LevelGoList)
        {
            ObjectManager.Instance.ReleaseObject(go);
        }
        m_LevelGoList.Clear();
        int level = ModelManager.Instance.EvolutionLevelDict[evolution.Id];
        for (int i = 0; i < evolution.MaxLevel; i++)
        {
            GameObject itemGo = ObjectManager.Instance.InstantiateObject(Consts.UI_GridItem);
            if (i < level)
            {
                itemGo.GetComponentInChildren<Image>().color = Color.black;
            }
            itemGo.transform.SetParent(m_LevelGrid);
            itemGo.GetComponent<GridItem>().Init();
            m_LevelGoList.Add(itemGo);
        }
        m_Action = action;
    }

    void UpBtnClick()
    {
        m_Action?.Invoke();
    }
}
