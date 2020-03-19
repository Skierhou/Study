using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

/// <summary>
/// 成就的领取状态
/// </summary>
public enum AchievementStatus
{
    NotReceive = 0,
    Receive,
    AlreadyReceive
}
public enum AchievementType
{
    Kill,
    Coin,
    Diamond,
    Boss,
    Welfare,
    Wave,
    Damage,
    WelfareDamage,
    Time
}

[System.Serializable]
public class AchievementData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(AchievementData).GetProperties();
    }
    public override void Construction()
    {
        AchievementList = new List<AchievementBase>();
        AchievementList.Add(new AchievementBase { Id = 1, Name = "Name",  IconPath = "IconPath",Des="Des" });
    }
    public override void Init()
    {
        AchievementDict.Clear();
        foreach (AchievementBase enemy in AchievementList)
        {
            AchievementDict.Add(enemy.Id, enemy);
        }
    }
    [XmlElement("AchievementList")]
    public List<AchievementBase> AchievementList { set; get; }
    [XmlIgnore]
    public Dictionary<int, AchievementBase> AchievementDict = new Dictionary<int, AchievementBase>();
}
[System.Serializable]
public class AchievementBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(AchievementBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }
    [XmlAttribute("AchievementType")]
    public AchievementType AchievementType { set; get; }
    [XmlAttribute("NeedCount")]
    public int NeedCount { set; get; }          //达到条件的个数
    [XmlAttribute("Reward")]
    public int Reward { set; get; }         //获得的奖励

    [XmlIgnore]
    public AchievementStatus AchievementStatus;
}
