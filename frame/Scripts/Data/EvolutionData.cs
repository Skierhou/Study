using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

[System.Serializable]
public class EvolutionData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EvolutionData).GetProperties();
    }
    public override void Construction()
    {
        EvolutionList = new List<EvolutionBase>();
        EvolutionList.Add(new EvolutionBase { Id = 1, Name = "Name", Des="Des" });
    }
    public override void Init()
    {
        EvolutionDict.Clear();
        foreach (EvolutionBase evolution in EvolutionList)
        {
            EvolutionDict.Add(evolution.Id, evolution);
        }
    }
    [XmlElement("HeroList")]
    public List<EvolutionBase> EvolutionList { set; get; }
    [XmlIgnore]
    public Dictionary<int, EvolutionBase> EvolutionDict = new Dictionary<int, EvolutionBase>();
}

[System.Serializable]
public class EvolutionBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EvolutionBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("RaceType")]
    public RaceType RaceType { set; get; }
    [XmlAttribute("HeroId")]
    public int HeroId { set; get; }         //英雄ID为0是表示是当前种族的进化项，其余的则是英雄的进化项
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }
    [XmlAttribute("MaxLevel")]
    public int MaxLevel { set; get; }
    [XmlAttribute("Payment")]
    public int Payment { set; get; }

    //加成
    [XmlIgnore]
    public int NowLevel;
}