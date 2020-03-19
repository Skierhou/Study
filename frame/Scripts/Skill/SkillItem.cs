using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillItem : MonoBehaviour
{
    protected SkillBase m_Skill;
    protected GameObject m_Owner;
    protected GameObject m_Target;
    protected Hero m_Hero;
    protected Enemy m_Enemy;

    public void Init(SkillBase skill,GameObject owner,GameObject target)
    {
        m_Skill = skill;
        m_Owner = owner;
        m_Target = target;
        m_Hero = owner.GetComponent<Hero>();
        m_Enemy = target.GetComponent<Enemy>();
        transform.position = owner.transform.position;
    }
    public abstract void ReleaseSkill();
}
