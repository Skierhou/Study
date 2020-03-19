using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static T Clone<T>(T tIn)
    {
        T tOut = Activator.CreateInstance<T>();
        var tInType = tIn.GetType();
        foreach (var itemOut in tOut.GetType().GetProperties())
        {
            var itemIn = tInType.GetProperty(itemOut.Name);
            if (itemIn != null)
            {
                itemOut.SetValue(tOut, itemIn.GetValue(tIn));
            }
        }
        foreach (var itemOut in tOut.GetType().GetFields())
        {
            var itemIn = tInType.GetField(itemOut.Name);
            if (itemIn != null)
            {
                itemOut.SetValue(tOut, itemIn.GetValue(tIn));
            }
        }
        return tOut;
    }
    public static Sprite LoadSprite(string path)
    {
        Texture2D texture2d = ResourceManager.Instance.LoadResource<Texture2D>(path);//创建一个图片变量
        
        Sprite sp = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));//注意居中显示采用0.5f值  //创建一个精灵(图片，纹理，二维浮点型坐标)
        return sp;
    }
}
