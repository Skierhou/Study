using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameManager:SingletonMono<GameManager>
{
    public GamePanel GamePanel;

    public bool IsClickBuilcPlace = false;

    private GameObject m_CircleGo;

    public void StartGame(GamePanel gamePanel)
    {
        GamePanel = gamePanel;
    }

    public void SelectGrid(Vector3 pos)
    {
        if (m_CircleGo == null)
        {
            m_CircleGo = ObjectManager.Instance.InstantiateObject(Consts.Item_Circle);
        }
        m_CircleGo.SetActive(true);
        m_CircleGo.transform.position = pos;
    }
    public void CancelSelect()
    {
        m_CircleGo.SetActive(false);
    }
}