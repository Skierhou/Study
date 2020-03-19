using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private EnemyBase m_Data;
    public EnemyBase Data { get { return m_Data; } }
    private HpSlider m_HpSlider;

    private bool m_IsDie = false;
    public bool IsDie { get { return m_IsDie; } }

    public void Init(EnemyBase enemyBase, HpSlider hpSlider, WaveBase wave)
    {
        m_Data = enemyBase;
        m_HpSlider = hpSlider;

        //更新数据
        m_Data.Coin = wave.Coin;
        m_Data.CurrentHp = wave.Hp;
        m_Data.TotalHp = wave.Hp;
        m_Data.MoveSpeed = wave.MoveSpeed;
        m_Data.MagicDefense = wave.MagicDefense;
        m_Data.Defense = wave.Defense;
    }

    public void TakeDamage(int damage, bool isMagic = false, float beatflyTime = 0, float vertigoTime = 0, bool isCrit = false)
    {

    }
}
