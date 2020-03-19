using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

/// <summary>
/// 装备等级
/// </summary>
public enum EquipLevel
{
    Lower = 0,
    Middle,
    High,
    Super,
    Sum_Count
}
/// <summary>
/// 武器类型
/// </summary>
public enum EquipType
{
    None=0,
    Attack,         //攻击
    AttackRate,     //攻速
    Magic,          //魔法
    Auxiliary,      //辅助
    Sum_Count,
}
/// <summary>
/// 加成类型
/// </summary>
public enum AdditionType
{
    Strengthen=0,
    Agility=1,
    Intelligence=2,
    ThreeAttribute=3,
    Attack=4,
    AttackRate =5,
    PhyCrit=6,
    PhyCritMultiply=7,
    MagicDamage=8,
    MagicCrit=9,
    MagicCritMultiply=10,
}

[System.Serializable]
public class EquipData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EquipData).GetProperties();
    }
    public override void Construction()
    {
        EquipList = new List<EquipBase>();
        EquipList.Add(new EquipBase { Id = 1,IconPath= "IconPath" ,Name="Name",Des="Des"});
    }
    public override void Init()
    {
        EquipDict.Clear();
        foreach (EquipBase equip in EquipList)
        {
            EquipDict.Add(equip.Id, equip);
            CheckList(equip.AdditionList);
            CheckList(equip.AdditionTypeList);
            CheckList(equip.PreList);
            CheckList(equip.SkillIdList);
        }
    }
    [XmlElement("EquipList")]
    public List<EquipBase> EquipList { set; get; }
    [XmlIgnore]
    public Dictionary<int, EquipBase> EquipDict = new Dictionary<int, EquipBase>();
}
[System.Serializable]
public class EquipBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(EquipBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("Coin")]
    public int Coin { set; get; }
    [XmlElement("PreList")]
    public List<int> PreList { set; get; }          //前置装备
    [XmlAttribute("EquipLevel")]
    public EquipLevel EquipLevel { set; get; }
    [XmlAttribute("EquipType")]
    public EquipType EquipType { set; get; }
    [XmlElement("AdditionTypeList")]
    public List<int> AdditionTypeList { set; get; }
    [XmlElement("AdditionList")]
    public List<float> AdditionList { set; get; }
    [XmlElement("SkillIdList")]
    public List<int> SkillIdList { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }

    public int SellCoin;
    public int TotalCoin;
}

