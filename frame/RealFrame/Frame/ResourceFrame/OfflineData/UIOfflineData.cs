using System;
using System.Collections.Generic;
using UnityEngine;

public class UIOfflineData:OfflineData
{
    public Vector2[] m_AnchorMin;
    public Vector2[] m_AnchorMax;
    public Vector3[] m_AnchorPos;
    public Vector2[] m_SizeDelta;
    public Vector2[] m_Pivot;
    public ParticleSystem[] m_ParticleSys;

    public override void ResetProp()
    {
        int allCount = m_AllChild.Length;
        for (int i = 0; i < allCount; i++)
        {
            RectTransform tempTrans = m_AllChild[i] as RectTransform;
            if (tempTrans != null)
            {
                tempTrans.localPosition = m_AllChildPos[i];
                tempTrans.localScale = m_AllChildScale[i];
                tempTrans.localRotation = m_AllChildRotation[i];
                tempTrans.anchorMin = m_AnchorMin[i];
                tempTrans.anchorMax = m_AnchorMax[i];
                tempTrans.anchoredPosition = m_AnchorPos[i];
                tempTrans.sizeDelta = m_SizeDelta[i];
                tempTrans.pivot = m_Pivot[i];

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

        int particleCount = m_ParticleSys.Length;
        for (int i = 0; i < particleCount; i++)
        {
            m_ParticleSys[i].Clear(true);
            m_ParticleSys[i].Play();
        }
    }
    public override void BindData()
    {
        gameObject.layer = LayerMask.GetMask("UI");
        Transform[] tempTrans = gameObject.GetComponentsInChildren<Transform>(true);
        int length = tempTrans.Length;
        for (int i = 0; i < length; i++)
        {
            if (!(tempTrans[i] is RectTransform))
            {
                tempTrans[i].gameObject.AddComponent<RectTransform>();
            }
        }
        m_AllChild = gameObject.GetComponentsInChildren<RectTransform>(true);
        m_ParticleSys = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        m_AllChildCount = new int[length];
        m_AllChildActive = new bool[length];
        m_AllChildPos = new Vector3[length];
        m_AllChildScale = new Vector3[length];
        m_AllChildRotation = new Quaternion[length];
        m_AnchorMin = new Vector2[length];
        m_AnchorMax = new Vector2[length];
        m_AnchorPos = new Vector3[length];
        m_SizeDelta = new Vector2[length];
        m_Pivot = new Vector2[length];
        for (int i = 0; i < length; i++)
        {
            RectTransform temp = m_AllChild[i] as RectTransform;
            m_AllChildCount[i] = temp.childCount;
            m_AllChildActive[i] = temp.gameObject.activeSelf;
            m_AllChildPos[i] = temp.localPosition;
            m_AllChildScale[i] = temp.localScale;
            m_AllChildRotation[i] = temp.localRotation;
            m_AnchorMin[i] = temp.anchorMin;
            m_AnchorMax[i] = temp.anchorMax;
            m_AnchorPos[i] = temp.anchoredPosition;
            m_SizeDelta[i] = temp.sizeDelta;
            m_Pivot[i] = temp.pivot;
        }
    }
}