using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class ABBundleConfig{

    [XmlElement("ABList")]
    public List<ABBase> ABList = new List<ABBase>();
}

[System.Serializable]
public class ABBase   //用于配置表最小单位
{
    [XmlAttribute("Path")]      
    public string Path { get; set; }        //资源全路径
    [XmlAttribute("Crc")]
    public uint Crc { get; set; }         //crc校验，将路径转成crc数据用于内部传输，属于一个文件的唯一标识
    [XmlAttribute("ABName")]
    public string ABName { get; set; }      //AB包名
    [XmlAttribute("AssetName")]
    public string AssetName { get; set; }   //资源名
    [XmlElement("ABDependce")]
    public List<string> ABDependce { get; set; }    //所有依赖项路径
}
