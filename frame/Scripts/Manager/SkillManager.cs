using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : SingletonMono<SkillManager>
{
    private SkillData m_SkillData;
    private void Awake()
    {
        m_SkillData = ConfigManager.Instance.FindData<SkillData>(Consts.Config_SkillData);
    }

    public SkillBase GetSkill(int id)
    {
        SkillBase skill = null;
        m_SkillData.SkillDict.TryGetValue(id, out skill);
        return skill;
    }
    public void ReleaseSkill(int id, GameObject owner, GameObject target)
    {
        ReleaseSkill(GetSkill(id), owner, target);
    }
    public void ReleaseSkill(SkillBase skill, GameObject owner, GameObject target)
    {
        GameObject skillGo = ObjectManager.Instance.InstantiateObject(skill.Path);
        SkillItem skillItem = skillGo.GetComponent<SkillItem>();
        skillItem.Init(skill,owner,target);
        skillItem.ReleaseSkill();
    }
}
