using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : SingletonMono<HeroManager>
{
    private HeroData m_HeroData;
    private Dictionary<RaceType, List<HeroBase>> m_RaceTypeDict = new Dictionary<RaceType, List<HeroBase>>();

    private void Awake()
    {
        m_HeroData = ConfigManager.Instance.FindData<HeroData>(Consts.Config_HeroData);
        foreach (HeroBase hero in m_HeroData.HeroList)
        {
            if (!m_RaceTypeDict.ContainsKey(hero.RaceType))
            {
                m_RaceTypeDict.Add(hero.RaceType, new List<HeroBase>());
            }
            m_RaceTypeDict[hero.RaceType].Add(hero);
        }
    }

    public Hero SpawnHero(int id,Vector3 pos)
    {
        HeroBase heroBase = Tools.Clone(m_HeroData.HeroDict[id]);
        Hero hero = ObjectManager.Instance.InstantiateObject(heroBase.Path).GetComponent<Hero>();
        hero.Init(heroBase);
        hero.transform.position = pos;
        return hero;
    }
    public Hero SpawnHero(RaceType raceType, Vector3 pos)
    {
        List<HeroBase> heroList = m_RaceTypeDict[raceType];
        int ran = Random.Range(1,heroList.Count);
        return SpawnHero(ran, pos);
    }

    public List<HeroBase> GetRaceHeroList(RaceType raceType)
    {
        List<HeroBase> list = null;
        m_RaceTypeDict.TryGetValue(raceType, out list);
        return list;
    }
}
