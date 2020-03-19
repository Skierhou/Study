using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectOfflineData:OfflineData
{
    public ParticleSystem[] m_ParticleSystems;
    public TrailRenderer[] m_TrailRenderers;

    public override void ResetProp()
    {
        base.ResetProp();
        foreach (ParticleSystem partSys in m_ParticleSystems)
        {
            partSys.Clear(true);
            partSys.Play();
        }
        foreach (TrailRenderer trail in m_TrailRenderers)
        {
            trail.Clear();
        }
    }
    public override void BindData()
    {
        base.BindData();
        m_ParticleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
        m_TrailRenderers = gameObject.GetComponentsInChildren<TrailRenderer>();
    }
}