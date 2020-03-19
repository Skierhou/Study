using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : SingletonMono<ModelManager>
{
    public float WaitingTime { get; private set; } = 15;         //每波等待时间
    public float WelfareTime { get; private set; } = 25;         //福利怪持续时间
    public int MaxLimit { get; private set; } = 200;           //怪物上限
    public int MaxPopulation { get; private set; } = 5;          //人口上限
    public int MaxAcclerate { get; private set; } = 4;           //加速上限
    public int StartCoin { get; private set; } = 500;            //开始金币
    public int Diamond { get; set; } = 100;             //拥有的钻石
    public int ReCharge { set; get; } = 0;              //已经充值的金额

    //成就信息
    public int Kill { get; private set; }
    public int Coin { get; private set; }
    public int DiamondRecord { get; private set; }
    public int Boss { get; private set; }
    public int Welfare { get; private set; }
    public int Wave { get; private set; }
    public int Damage { get; private set; }
    public int WelfareDamage { get; private set; }
    public int Time { get; private set; }


    private EvolutionData m_EvolutionData;
    private AchievementData m_AchievementData;
    private ShopData m_ShopData;

    //key:id,value:level
    public Dictionary<int, int> EvolutionLevelDict { get; private set; } = new Dictionary<int, int>();
    private void Awake()
    {
        m_EvolutionData = ConfigManager.Instance.FindData<EvolutionData>(Consts.Config_EvolutionData);
        m_AchievementData = ConfigManager.Instance.FindData<AchievementData>(Consts.Config_AchievementData);
        m_ShopData = ConfigManager.Instance.FindData<ShopData>(Consts.Config_ShopData);
        LoadInfoByLocal();
    }
    /// <summary>
    /// 通过Unity的本地存储数据
    /// </summary>
    void LoadInfoByLocal()
    {
        foreach (EvolutionBase evolution in m_EvolutionData.EvolutionList)
        {
            int level = PlayerPrefs.GetInt("Evolution_" + evolution.Id);
            EvolutionLevelDict.Add(evolution.Id, level);
        }
        //获取成就信息
        Kill = PlayerPrefs.GetInt("Kill");
        Coin = PlayerPrefs.GetInt("Coin");
        DiamondRecord = PlayerPrefs.GetInt("DiamondRecord");
        Boss = PlayerPrefs.GetInt("Boss");
        Welfare = PlayerPrefs.GetInt("Welfare");
        Wave = PlayerPrefs.GetInt("Wave");
        Damage = PlayerPrefs.GetInt("Damage");
        WelfareDamage = PlayerPrefs.GetInt("WelfareDamage");
        Time = PlayerPrefs.GetInt("Time");
        //更新成就的可领取状态
        foreach (AchievementBase achievement in m_AchievementData.AchievementList)
        {
            int status = PlayerPrefs.GetInt("Achievement_" + achievement.Id);
            achievement.AchievementStatus = (AchievementStatus)status;
        }
        //更新商城商品的购买状态
        foreach (ShopBase shopBase in m_ShopData.ShopList)
        {
            int index = PlayerPrefs.GetInt("ShopItem_" + shopBase.Id);
            shopBase.IsOwn = index == 0 ? false : true;
        }
    }
    /// <summary>
    /// 进化项升级
    /// </summary>
    public bool LevelUpEvolution(EvolutionBase evolution)
    {
        int level = 0;
        if (EvolutionLevelDict.TryGetValue(evolution.Id, out level))
        {
            if (level < evolution.MaxLevel)
            {
                EvolutionLevelDict[evolution.Id]++;
            }
            else
            {
                return false;
            }
        }
        else
        {
            EvolutionLevelDict.Add(evolution.Id, level);
        }
        return true;
    }
    public void ChangeAchievementStatus(AchievementBase achievement)
    {
        PlayerPrefs.SetInt("Achievement_" + achievement.Id, (int)achievement.AchievementStatus);
    }

    public override void OnDestroy()
    {
        foreach (int id in EvolutionLevelDict.Keys)
        {
            PlayerPrefs.SetInt("Evolution_" + id, EvolutionLevelDict[id]);
        }
        base.OnDestroy();
    }
}
