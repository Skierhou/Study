using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

public enum RaceType
{
    None,
    Human,
    Orc,
    Undead,
    Dragon,
    Neutral
}

[System.Serializable]
public class BuildData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(BuildData).GetProperties();
    }
    public override void Construction()
    {
        BuildList = new List<BuildBase>
        {
            new BuildBase { Id = 1, IconPath = "IconPath", Name = "Name", Des = "Des" }
        };
    }
    public override void Init()
    {
        BuildDict.Clear();
        foreach (BuildBase build in BuildList)
        {
            BuildDict.Add(build.Id,build);
        }
    }
    [XmlElement("BuildList")]
    public List<BuildBase> BuildList { set; get; }
    [XmlIgnore]
    public Dictionary<int, BuildBase> BuildDict = new Dictionary<int, BuildBase>();
}
[System.Serializable]
public class BuildBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(BuildBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("RaceType")]
    public RaceType RaceType { set; get; }
    [XmlAttribute("Coin")]
    public int Coin { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }
}

