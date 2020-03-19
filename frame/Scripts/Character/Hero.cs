using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    private HeroBase m_Data;
    public HeroBase Data { get { return m_Data; } }
    private ExtraData m_ExtraData;
    public ExtraData ExtarData { get { return m_ExtraData; } }

    public ClassObjectPool<ExtraData> m_ExtraDataPool = ObjectManager.Instance.GetOrCreateClassPool<ExtraData>(50);

    private Dictionary<SkillType, List<SkillBase>> m_SkillDict = new Dictionary<SkillType, List<SkillBase>>();
    public List<SkillBase> ActiveSkillList
    {
        get
        {
            if (!m_SkillDict.ContainsKey(SkillType.Active))
            {
                m_SkillDict.Add(SkillType.Active, new List<SkillBase>());
            }
            return m_SkillDict[SkillType.Active];
        }
    }
    public List<SkillBase> AttackPassiveList
    {
        get
        {
            if (!m_SkillDict.ContainsKey(SkillType.AttackPassive))
            {
                m_SkillDict.Add(SkillType.AttackPassive, new List<SkillBase>());
            }
            return m_SkillDict[SkillType.AttackPassive];
        }
    }
    public List<SkillBase> MagicPassiveList
    {
        get
        {
            if (!m_SkillDict.ContainsKey(SkillType.MagicPassive))
            {
                m_SkillDict.Add(SkillType.MagicPassive, new List<SkillBase>());
            }
            return m_SkillDict[SkillType.MagicPassive];
        }
    }
    public List<SkillBase> DiePassiveList
    {
        get
        {
            if (!m_SkillDict.ContainsKey(SkillType.DiePassive))
            {
                m_SkillDict.Add(SkillType.DiePassive,new List<SkillBase>());
            }
            return m_SkillDict[SkillType.DiePassive];
        }
    }
    public List<SkillBase> BuffList
    {
        get
        {
            if (!m_SkillDict.ContainsKey(SkillType.Buff))
            {
                m_SkillDict.Add(SkillType.Buff, new List<SkillBase>());
            }
            return m_SkillDict[SkillType.Buff];
        }
    }

    public List<EquipBase> EquipList = new List<EquipBase>();
    private bool m_IsAttacking = false;

    private Animator m_Anim;
    private Enemy m_Target;
    private SkillBase m_CurSkill;

    [Header("动画播放时间")]
    public float AttackTime;
    public float CritTime;
    public float ContinuedTime;
    public float InstantTime;

    private const string AttackAnim = "Attack";
    private const string CritAnim = "Crit";
    private const string ContinuedAnim = "Continued";
    private const string InstantAnim = "InStant";

    private void Awake()
    {
        m_Anim = GetComponent<Animator>();
    }

    public void Init(HeroBase data)
    {
        m_Data = data;
        m_ExtraData = m_ExtraDataPool.Spawn();
        m_SkillDict.Clear();
        foreach (int id in m_Data.SkillList)
        {
            SkillBase skill = SkillManager.Instance.GetSkill(id);
            if (!m_SkillDict.ContainsKey(skill.SkillType))
            {
                m_SkillDict.Add(skill.SkillType,new List<SkillBase>());
            }
            m_SkillDict[skill.SkillType].Add(skill);
        }
    }
    /// <summary>
    /// 攻击
    /// </summary>
    public void Attack()
    {
        if (!m_IsAttacking)
        {
            bool isRleaseSkill = false;
            foreach (SkillBase skill in ActiveSkillList)
            {
                if (skill.CdTimer >= skill.CD)
                {
                    m_CurSkill = skill;
                    isRleaseSkill = true;
                    skill.CdTimer = 0;
                    switch (skill.ReleaseType)
                    {
                        case ReleaseType.Instantaneous:
                            PlayAnim(InstantAnim, InstantTime, skill.Duration);
                            break;
                        case ReleaseType.Continued:
                            PlayAnim(ContinuedAnim,ContinuedTime);
                            break;
                    }
                    break;
                }
            }
            if (!isRleaseSkill)
            {
                if (Random.value <= m_Data.PhyCrit)
                {
                    PlayAnim(CritAnim, CritTime);
                }
                else
                {
                    PlayAnim(AttackAnim, AttackTime);
                }
            }
        }
    }
    void AttackPassiveSkill()
    {
        foreach (SkillBase skill in AttackPassiveList)
        {
            if (Random.value <= skill.TriggerRate)
            {
                SkillManager.Instance.ReleaseSkill(skill, gameObject, m_Target.gameObject);
            }
        }
    }
    protected virtual void NormalAttack()
    {
        if (m_Target != null && !m_Target.IsDie)
        {
            AttackPassiveSkill();
            int damage = m_Data.Attack + m_ExtraData.Attack;
            m_Target.TakeDamage(damage);
        }
    }
    protected virtual void CritAttack()
    {
        if (m_Target != null && !m_Target.IsDie)
        {
            AttackPassiveSkill();
            int damage = m_Data.Attack + m_ExtraData.Attack;
            damage = (int)(damage * m_Data.MagicCritMultiply);
            m_Target.TakeDamage(damage, isCrit: true);
        }
    }
    protected virtual void SkillAttack()
    {
        SkillManager.Instance.ReleaseSkill(m_CurSkill, gameObject, m_Target.gameObject);
        //判断是否存在魔法施放被动
        foreach (SkillBase passiveSkill in MagicPassiveList)
        {
            if (Random.value <= passiveSkill.TriggerRate)
            {
                SkillManager.Instance.ReleaseSkill(passiveSkill, gameObject, m_Target.gameObject);
            }
        }
    }
    void PlayAnim(string name, float animTime, float duration = 0)
    {
        float speed = 0;
        if (duration == 0)
        {
            float interval = 1 / (m_Data.AttackRate + m_ExtraData.AttackRate);
            interval = interval < m_Data.AttackIntervalLimit ? m_Data.AttackIntervalLimit : interval;
            speed = animTime / interval;
        }
        else
        {
            speed = duration / animTime;
        }
        m_Anim.speed = speed;
        m_Anim.SetTrigger(name);
    }

    public void Remove()
    {
        m_ExtraDataPool.UnSpawn(m_ExtraData);
    }
}
