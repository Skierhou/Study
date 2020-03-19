using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class BaseData {

    /// <summary>
    /// 编辑器下构造
    /// </summary>
    public virtual void Construction() { }
    /// <summary>
    /// 获取该类型的所有属性信息
    /// </summary>
    public virtual PropertyInfo[] AllAttributes() { return null; }
    /// <summary>
    /// 程序中加载初始化
    /// </summary>
    public virtual void Init() { }

    protected void CheckList(List<int> list)
    {
        if (list[0] == 0)
        {
            list.Clear();
        }
    }
    protected void CheckList(List<float> list)
    {
        if (list[0] == 0)
        {
            list.Clear();
        }
    }
}
