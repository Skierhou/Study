using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class WaveData:BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(WaveData).GetProperties();
    }
    public override void Construction()
    {
        WaveList = new List<WaveBase>();
        WaveList.Add(new WaveBase { Id=1});
    }
    public override void Init()
    {
        WaveDict.Clear();
        foreach (WaveBase wave in WaveList)
        {
            WaveDict.Add(wave.Id, wave);
        }
    }

    [XmlElement("WaveList")]
    public List<WaveBase> WaveList { set; get; }

    [XmlIgnore]
    public Dictionary<int, WaveBase> WaveDict = new Dictionary<int, WaveBase>();

}

[System.Serializable]
public class WaveBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(WaveBase).GetProperties();
    }

    [XmlAttribute("Id")]
    public int Id { set; get; }             //波数
    [XmlAttribute("EnemyId")]
    public int EnemyId { set; get; }        //怪物ID
    [XmlAttribute("Coin")]
    public int Coin { set; get; }           //每只金币
    [XmlAttribute("Exp")]
    public int Exp { set; get; }            //每只经验
    [XmlAttribute("Count")]
    public int Count { set; get; }          //数量
    [XmlAttribute("Interval")]
    public float Interval { set; get; }     //每只间隔
    [XmlAttribute("Hp")]
    public int Hp { set; get; }             //生命
    [XmlAttribute("MoveSpeed")]
    public float MoveSpeed { set; get; }    //移速
    [XmlAttribute("Defense")]
    public int Defense { set; get; }        //防御
    [XmlAttribute("MagicDefense")]
    public int MagicDefense { set; get; }   //魔法防御
}
