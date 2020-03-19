using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Reflection;

public enum ShopItemType
{
    All=0,
    Diamond,
    Skin,
    Card,
    Sum_Count,
}
public enum PayType
{
    Money,
    Diamond
}

[System.Serializable]
public class ShopData : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(ShopData).GetProperties();
    }
    public override void Construction()
    {
        ShopList = new List<ShopBase>();
        ShopList.Add(new ShopBase { Id = 1, Name = "Name", IconPath = "IconPath",Des="Des" });
    }
    public override void Init()
    {
        ShopDict.Clear();
        foreach (ShopBase enemy in ShopList)
        {
            ShopDict.Add(enemy.Id, enemy);
        }
    }
    [XmlElement("ShopList")]
    public List<ShopBase> ShopList { set; get; }
    [XmlIgnore]
    public Dictionary<int, ShopBase> ShopDict = new Dictionary<int, ShopBase>();
}
[System.Serializable]
public class ShopBase : BaseData
{
    public override PropertyInfo[] AllAttributes()
    {
        return typeof(ShopBase).GetProperties();
    }
    [XmlAttribute("Id")]
    public int Id { set; get; }
    [XmlAttribute("Name")]
    public string Name { set; get; }
    [XmlAttribute("IconPath")]
    public string IconPath { set; get; }
    [XmlAttribute("ShopItemType")]
    public ShopItemType ShopItemType { set; get; }
    [XmlAttribute("PayType")]
    public PayType PayType { set; get; }
    [XmlAttribute("Payment")]
    public int Payment { set; get; }
    [XmlAttribute("Des")]
    public string Des { set; get; }
    [XmlIgnore]
    public bool IsOwn = false;      //是否拥有
}
