using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPlace : MonoBehaviour
{
    private Hero m_Hero;
    public void SetHero(Hero hero)
    {
        m_Hero = hero;
    }
    public void RemoveHero()
    {
        m_Hero = null;
    }

    private void OnMouseUp()
    {
        if (!GameManager.Instance.IsClickBuilcPlace)
        {
            GameManager.Instance.IsClickBuilcPlace = true;

            if (m_Hero == null)
            {
                UIManager.Instance.PushPanel(UIPanelType.BuildPanel, paramList: new object[] { this });
            }
            else
            {
                UIManager.Instance.PushPanel(UIPanelType.CharacterPanel, paramList: new object[] { m_Hero });
            }
        }
    }
}
