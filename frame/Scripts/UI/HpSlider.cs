using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpSlider : MonoBehaviour
{
    private GameObject m_Target;
    private Slider m_Slider;
    private Image m_Image;

    private void Awake()
    {
        m_Slider = GetComponent<Slider>();
        m_Image = GetComponent<Image>();
    }
    public void Init(GameObject target)
    {
        m_Target = target;
        m_Slider.value = 1;
    }
    public void ChangeSlider(float value)
    {
        m_Slider.value = value;
        m_Image.color = new Color(value, value, value);
    }
    
}
