using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

public enum SkillType
{
    Active,         //主动
    AttackPassive,  //攻击被动（普通攻击时几率触发）
    DiePassive,     //死亡被动（怪物死亡时几率触发）
    MagicPassive,   //魔法被动（释放魔法是几率触发）
    Buff,           //加成Buff（技能Buff算在主动技能里面）
}
public enum ReleaseType
{
    Instantaneous,  //直接释放
    Continued,      //持续释放
}

[System.Serializable]
public class SkillData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(SkillData).GetProperties();
    }
    public override void Construction()
    {
        SkillList = new List<SkillBase>();
        SkillList.Add(new SkillBase { Id = 1, Name = "Name", Path = "Path", IconPath = "IconPath", EffectPath = "EffectPath" });
    }
    public override void Init()
    {
        SkillDict.Clear();
        foreach (SkillBase skill in SkillList)
        {
            SkillDict.Add(skill.Id,skill);
        }
    }
    [XmlElement("SkillList")]
    public List<SkillBase> SkillList { set; get; }
    [XmlIgnore]
    public Dictionary<int, SkillBase> SkillDict = new Dictionary<int, SkillBase>();
}

[System.Serializable]
public class SkillBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(SkillBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("Path")]
    public string Path { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("EffectPath")]
    public string EffectPath { set; get; }

    //如何造成伤害
    [XmlAttribute("SkillType")]
    public SkillType SkillType { set; get; }
    [XmlAttribute("ReleaseType")]
    public ReleaseType ReleaseType { set; get; }
    [XmlAttribute("CD")]
    public float CD { set; get; }
    [XmlAttribute("Duration")]
    public float Duration { set; get; }             //持续时间
    [XmlAttribute("TriggerRate")]
    public float TriggerRate { set; get; }          //触发几率
    [XmlAttribute("BasicAddition")]
    public int BasicAddition { set; get; }          //基础加成
    [XmlAttribute("AdditionType")]
    public AdditionType AdditionType { set; get; }  //额外加成类型
    [XmlAttribute("AdditionRate")]
    public float AdditionRate { set; get; }         //额外加成比率
    [XmlAttribute("Des")]
    public string Des { set; get; }


    public float CdTimer = 0;
}
