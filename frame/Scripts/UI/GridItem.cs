using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridItem : MonoBehaviour
{
    private Image m_Img;
    private Button m_Btn;
    private System.Action m_Action;

    private void Awake()
    {
        m_Img = GetComponent<Image>();
        m_Btn = GetComponent<Button>();
        m_Btn.onClick.AddListener(BtnClick);
    }
    public void Init()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
    }
    public void Init(Sprite sprite,System.Action action)
    {
        Init();
        m_Img.sprite = sprite;
        m_Action = action;
    }

    void BtnClick()
    {
        m_Action?.Invoke();
    }
}
