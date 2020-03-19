using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : SingletonMono<EnemyManager>
{
    private EnemyData m_EnemyData;
    private WaveData m_WaveData;

    private void Awake()
    {
        m_EnemyData = ConfigManager.Instance.FindData<EnemyData>(Consts.Config_EnemyData);
        m_WaveData = ConfigManager.Instance.FindData<WaveData>(Consts.Config_WaveData);
    }
    private void SpawnEnemy(int id,Vector3 pos,WaveBase wave)
    {
        EnemyBase enemyBase = Tools.Clone(m_EnemyData.EnemyDict[id]);
        Enemy enemy = ObjectManager.Instance.InstantiateObject(enemyBase.Path).GetComponent<Enemy>();

        //HpSlider
        HpSlider hpSlider = ObjectManager.Instance.InstantiateObject(Consts.UI_HpSlider).GetComponent<HpSlider>();
        hpSlider.Init(enemy.gameObject);

        enemy.Init(enemyBase, hpSlider, wave);
        enemy.transform.position = pos;
    }
    /// <summary>
    /// 开始生成怪物
    /// </summary>
    public void Spawn(int wave, System.Action action)
    {
        StartCoroutine(StartSpawn(wave, action));
    }
    IEnumerator StartSpawn(int wave,System.Action action)
    {
        WaveBase waveBase = m_WaveData.WaveDict[wave];
        yield return new WaitForSeconds(ModelManager.Instance.WaitingTime);
        int count = waveBase.Count;
        while (count > 0)
        {
            count--;
            //SpawnEnemy(wave.EnemyId, Vector3.zero, wave);
            yield return new WaitForSeconds(waveBase.Interval);
        }
        //结束回调
        if (wave % 5 == 0)
        {
            SpawnWelfare(wave, action);
        }
        else
        {
            action?.Invoke();
        }
    }
    /// <summary>
    /// 开始生成福利怪
    /// </summary>
    private void SpawnWelfare(int wave, System.Action action)
    {
        StartCoroutine(StartSpawnWelfare(wave, action));
    }
    IEnumerator StartSpawnWelfare(int wave, System.Action action)
    {
        yield return new WaitForSeconds(ModelManager.Instance.WaitingTime);

        SpawnEnemy(0, Vector3.zero, m_WaveData.WaveDict[wave]);

        yield return new WaitForSeconds(ModelManager.Instance.WelfareTime);
        //结束回调
        action?.Invoke();
    }
}
