using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class Serialize{


    #region XML序列化
    /// <summary>
    /// 类转换成Xml
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="path">xml保存路径</param>
    public static bool XmlSerialize(string path, System.Object obj)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    //这一步是将xml生成后的配置说明那一行取消
                    //XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    //namespaces.Add(string.Empty, string.Empty);

                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                    xmlSerializer.Serialize(sw, obj);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("序列化Xml失败，类型：" + obj.GetType() + "， " + e);
            if (File.Exists(path)) File.Delete(path);
        }
        return false;
    }
    /// <summary>
    /// （编译器模式下使用）XML转成类
    /// </summary>
    public static T XmlDeSerializeEditor<T>(string path) where T : class
    {
        //获取默认的T类型对象
        T t = default(T);
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(t.GetType());
                t = (T)xmlSerializer.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("找不到路径：" + path + "， 错误：" + e);
            return null;
        }
        return t;
    }
    public static System.Object XmlDeSerializeEditor(string path,Type type)
    {
        //获取默认的T类型对象
        System.Object obj = null;
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(type);
                obj = xmlSerializer.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("找不到路径：" + path + "， 错误：" + e);
            return null;
        }
        return obj;
    }
    /// <summary>
    /// （运行模式下使用）XML转类
    /// </summary>
    public static T XmlDeSerializeRun<T>(string path) where T : class
    {
        //获取默认的T类型对象
        T t = default(T);
        TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("找不到路径：" + path + " 对应的xml文件");
            return null;
        }

        try
        {
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
            {
                XmlSerializer xs = new XmlSerializer(t.GetType());
                t = (T)xs.Deserialize(ms);
            }
            ResourceManager.Instance.ReleaseResource(path, true);
        }
        catch (Exception e)
        {
            Debug.LogError("找不到路径：" + path + "， 错误：" + e);
        }

        return t;
    }
    #endregion

    #region Binary序列化
    /// <summary>
    /// 类转换成二进制
    /// </summary>
    public static bool BinarySerialize(string path, System.Object obj)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, obj);
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("无法完成Binary序列化：" + path + "，错误：" + e);
            if (File.Exists(path)) File.Delete(path);
        }
        return false;
    }
    /// <summary>
    /// （编译器使用）二进制转类
    /// </summary>
    public static T BinaryDeSerializeEditor<T>(string path) where T : class
    {
        T t = default(T);
        try
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                BinaryFormatter bf = new BinaryFormatter();
                t = (T)bf.Deserialize(fs);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("找不到路径：" + path + "， 错误：" + e);
        }
        return t;
    }
    /// <summary>
    /// （运行时使用）二进制转类
    /// </summary>
    public static T BinaryDeSerializeRun<T>(string path) where T : class
    {
        T t = default(T);

        TextAsset textAsset = ResourceManager.Instance.LoadResource<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError("找不到路径：" + path + " 对应的二进制文件");
            return null;
        }
        try
        {
            using (MemoryStream ms = new MemoryStream(textAsset.bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                t = (T)bf.Deserialize(ms);
            }
            ResourceManager.Instance.ReleaseResource(path, true);
        }
        catch (Exception e)
        {
            Debug.LogError("找不到路径：" + path + "， 错误：" + e);
        }
        return t;
    }
    #endregion

}
