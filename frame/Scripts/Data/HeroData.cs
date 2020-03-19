using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

[System.Serializable]
public class HeroData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(HeroData).GetProperties();
    }
    public override void Construction()
    {
        HeroList = new List<HeroBase>();
        HeroList.Add(new HeroBase { Id=1,Name="Name",Path="Path",IconPath="IconPath", AttackPath = "AttackPath",Des="Des" });
    }
    public override void Init()
    {
        HeroDict.Clear();
        foreach (HeroBase hero in HeroList)
        {
            HeroDict.Add(hero.Id, hero);
            CheckList(hero.SkillList);
        }
    }
    [XmlElement("HeroList")]
    public List<HeroBase> HeroList { set; get; }
    [XmlIgnore]
    public Dictionary<int, HeroBase> HeroDict = new Dictionary<int, HeroBase>();
}


[System.Serializable]
public class HeroBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(HeroBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("Path")]
    public string Path { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("AttackPath")]
    public string AttackPath { set; get; }      //远程攻击的攻击物体路径
    [XmlAttribute("RaceType")]
    public RaceType RaceType { set; get; }
    [XmlAttribute("AttackRange")]
    public float AttackRange { set; get; }
    [XmlElement("SkillList")]
    public List<int> SkillList { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }

    //不需要离线配置的数据

    //三维
    public int Strength = 0;
    public int Agility = 0;
    public int Intelligence = 0;

    //物理
    public int Attack = 0;
    public float PhyCrit = 0;
    public float PhyCritMultiply = 0;
    public float AttackRate = 0;                //攻速
    public float AttackIntervalLimit = 0;       //攻击间隔上限

    //魔法
    public float MagicDamage = 0;
    public float MagicCrit = 0;
    public float MagicCritMultiply = 0;

    //其他
    public int Level = 0;
    public int Exp = 0;
    public int NeedExp = 0;
    public int Point = 0;           //潜能点

    [XmlIgnore]
    public int ThreeAttribute { get { return Strength + Agility + Intelligence; } set { value = 0; } }
}