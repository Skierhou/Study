using System;
using System.Collections.Generic;
using UnityEngine;

public class OfflineData:MonoBehaviour
{
    public Rigidbody m_Rigidbody;
    public CharacterController m_CharacterController;
    public Collider m_Collider;
    public Transform[] m_AllChild;
    public int[] m_AllChildCount;
    public bool[] m_AllChildActive;
    public Vector3[] m_AllChildPos;
    public Vector3[] m_AllChildScale;
    public Quaternion[] m_AllChildRotation;

    /// <summary>
    /// 重置游戏物体
    /// </summary>
    public virtual void ResetProp()
    {
        int allCount = m_AllChild.Length;
        for (int i = 0; i < allCount; i++)
        {
            Transform tempTrans = m_AllChild[i];
            if (tempTrans != null)
            {
                tempTrans.localPosition = m_AllChildPos[i];
                tempTrans.localScale = m_AllChildScale[i];
                tempTrans.localRotation = m_AllChildRotation[i];

                if (m_AllChildActive[i])
                {
                    if (!tempTrans.gameObject.activeSelf)
                    {
                        tempTrans.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (tempTrans.gameObject.activeSelf)
                    {
                        tempTrans.gameObject.SetActive(false);
                    }
                }
            }
            if (tempTrans.childCount > m_AllChildCount[i])
            {
                int childCount = tempTrans.childCount;
                for (int j = m_AllChildCount[i]; j < childCount; j++)
                {
                    GameObject obj = tempTrans.GetChild(j).gameObject;
                    if (!ObjectManager.Instance.IsObjectManagerCreated(obj))
                    {
                        GameObject.Destroy(obj);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 绑定离线数据
    /// </summary>
    public virtual void BindData()
    {
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
        m_CharacterController = gameObject.GetComponent<CharacterController>();
        m_Collider = gameObject.GetComponent<Collider>();
        m_AllChild = gameObject.GetComponentsInChildren<Transform>();
        int length = m_AllChild.Length;
        m_AllChildCount = new int[length];
        m_AllChildActive = new bool[length];
        m_AllChildPos = new Vector3[length];
        m_AllChildScale = new Vector3[length];
        m_AllChildRotation = new Quaternion[length];
        for (int i = 0; i < length; i++)
        {
            Transform temp = m_AllChild[i];
            m_AllChildCount[i] = temp.childCount;
            m_AllChildActive[i] = temp.gameObject.activeSelf;
            m_AllChildPos[i] = temp.localPosition;
            m_AllChildScale[i] = temp.localScale;
            m_AllChildRotation[i] = temp.localRotation;
        }
    }

}