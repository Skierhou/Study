using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GameStart : MonoBehaviour {

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        AssetBundleManager.Instance.LoadAllAssetBundle();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("UnSpawnNode"), transform.Find("SceneNode"));
        UIManager.Instance.Init(transform.Find("UIRoot") as RectTransform, transform.Find("UIRoot/PanelRoot") as RectTransform,
            transform.Find("UIRoot/UICamera").GetComponent<Camera>(), null, this);
        GameSceneManager.Instance.Init(this);
        LoadConfigData();
    }
    private void Start()
    {
        gameObject.AddComponent<GameManager>();
        gameObject.AddComponent<EnemyManager>();
        gameObject.AddComponent<HeroManager>();
        gameObject.AddComponent<SkillManager>();
        gameObject.AddComponent<ModelManager>();

        UIManager.Instance.PushPanel(UIPanelType.MainPanel);
    }
    void LoadConfigData()
    {
        ConfigManager.Instance.LoadData<EnemyData>(Consts.Config_EnemyData);
        ConfigManager.Instance.LoadData<EquipData>(Consts.Config_EquipData);
        ConfigManager.Instance.LoadData<HeroData>(Consts.Config_HeroData);
        ConfigManager.Instance.LoadData<BuildData>(Consts.Config_BuildData);
        ConfigManager.Instance.LoadData<SkillData>(Consts.Config_SkillData);
        ConfigManager.Instance.LoadData<WaveData>(Consts.Config_WaveData);
        ConfigManager.Instance.LoadData<EvolutionData>(Consts.Config_EvolutionData);
        ConfigManager.Instance.LoadData<AchievementData>(Consts.Config_AchievementData);
        ConfigManager.Instance.LoadData<ShopData>(Consts.Config_ShopData);
    }

}
