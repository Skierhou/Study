using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

[System.Serializable]
public class EnemyData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EnemyData).GetProperties();
    }
    public override void Construction()
    {
        EnemyList = new List<EnemyBase>();
        EnemyList.Add(new EnemyBase { Id=1,Name="Name",Path="Path",IconPath="IconPath"});
    }
    public override void Init()
    {
        EnemyDict.Clear();
        foreach (EnemyBase enemy in EnemyList)
        {
            EnemyDict.Add(enemy.Id, enemy);
            CheckList(enemy.FearRaceList);
            CheckList(enemy.ResistRaceList);
        }
    }
    [XmlElement("EnemyList")]
    public List<EnemyBase> EnemyList { set; get; }
    [XmlIgnore]
    public Dictionary<int, EnemyBase> EnemyDict = new Dictionary<int, EnemyBase>();
}
[System.Serializable]
public class EnemyBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EnemyBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("Path")]
    public string Path { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlElement("ResistRaceList")]
    public List<int> ResistRaceList { set; get; }    //抵抗的种族(造成伤害减少30%)
    [XmlElement("FearRaceList")]
    public List<int> FearRaceList { set; get; }      //惧怕的种族(造成伤害增加30%)

    public float MoveSpeed = 0;
    public int Exp = 0;
    public int Coin = 0;
    public int TotalHp = 0;
    public int CurrentHp = 0;
    public int Defense = 0;
    public int MagicDefense = 0;
}
