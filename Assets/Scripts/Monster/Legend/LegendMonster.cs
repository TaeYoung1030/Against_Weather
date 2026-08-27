using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegendMonster : MonsterController
{
    public enum LegendType { BlackMoon, RedMoon, Tornado }

    [Header("레전드 보스 세팅")]
    [SerializeField] LegendType legendBossType;

    [Header("개기일식 세팅")]
    [SerializeField] float missChance = 0.3f;
    [SerializeField] GameObject cloudVisualPrefab;
    [SerializeField] Transform cloudTransform;
    [SerializeField] float skillTime;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] Vector2 moveRange = new Vector2(2f, 3f);
    [SerializeField] GameObject model;
    private GameObject spawnedCloud;
    private bool isHiding = false;

    [Header("개기월식 세팅")]
    [SerializeField] float hpRegenPercent = 0.001f;
    [SerializeField] GameObject minion1;
    [SerializeField] GameObject minion2;
    [SerializeField] Transform[] minionSpawnPoints;
    [SerializeField] float minionHpValue;
    private List<GameObject> aliveMinions = new List<GameObject>();

    private Renderer monsterRenderer;

    private Renderer[] monsterRenderers;

    public override void InitMonster(WeatherMonsterData data, float Hp)
    {
        base.InitMonster(data, Hp);

        aliveMinions.Clear();

        monsterRenderer = GetComponent<Renderer>();

        if (gameUI != null)
        {
            gameUI.SetTierIcon(WeatherMonsterData.MonsterTier.Legend);
        }

        StartBasePassiveDebuff();
    }

    private void StartBasePassiveDebuff()
    {
        if (legendBossType == LegendType.RedMoon)
        {
            gameUI.ActiveICon("RedMoon", true);
            StartCoroutine(RedMoonRegenRoutine());
        }
    }

    protected override float ApplyPlayerDebuffToDamage(float incomingDamage)
    {      

        switch(legendBossType)
        {
            case LegendType.BlackMoon:
                if (GameManager.instance.IsMissControlActive())
                {
                    return incomingDamage;
                }
                if (Random.value < missChance)
                {
                    //gameUI.UpdateMissMessage();
                    gameUI.MissUI();
                    return 0f;
                }
                break;
            case LegendType.RedMoon:
                aliveMinions.RemoveAll(minion => minion == null);
                if (aliveMinions.Count > 0)
                {
                    return 0f;
                }
                break;
        }

        return incomingDamage;
    }


    protected override void ActivateLegendSkill()
    {
        switch (legendBossType)
        {
            case LegendType.BlackMoon:
                StartCoroutine(BlackMoonSkillRoutine());
                break;

            case LegendType.RedMoon:
                SpawnRedMoonMinions();
                break;

            case LegendType.Tornado:
                StartCoroutine(TornadoSkillRoutine());
                break;
        }
    }

    private IEnumerator BlackMoonSkillRoutine()
    {
        if (cloudVisualPrefab != null && spawnedCloud == null)
        {
            spawnedCloud = Instantiate(cloudVisualPrefab, cloudTransform.position, Quaternion.identity);
        }
        if (model != null) model.SetActive(false);

        isHiding = true;
        Coroutine moveCor = StartCoroutine(RandomMoveRoutine());

        yield return new WaitForSeconds(skillTime);

        isHiding = false;
        if (moveCor != null) StopCoroutine(moveCor);

        transform.position = originPos;

        if (model != null) model.SetActive(true);
        if (spawnedCloud != null) Destroy(spawnedCloud);
    }

    private IEnumerator RandomMoveRoutine()
    {
        while (isHiding && !isDead)
        {
            float randomX = originPos.x + Random.Range(-moveRange.x, moveRange.x);
            float randomY = originPos.y + Random.Range(-moveRange.y, moveRange.y);
            Vector3 targetPos = new Vector3(randomX, randomY, originPos.z);

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                if (!isHiding || isDead) yield break;
             
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(Random.Range(0.2f, 0.5f));
        }
    }

    private IEnumerator RedMoonRegenRoutine()
    {
        while (!isDead)
        {
            if (currentHP < maxHP)
            {
                float regenAmount = maxHP * hpRegenPercent;
                currentHP = Mathf.Min(maxHP, currentHP + regenAmount);

                GameManager.instance.OnMonsterTakeDamage(currentHP, maxHP);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator TornadoSkillRoutine()
    {
        bool isDestroyed = GameManager.instance.DestroyRandomStructure();

        if (isDestroyed)
        {
            Debug.Log("구조물이 파괴");
        }
        else
        {
            Debug.Log("파괴 실패");
        }

        yield return null;
    }

    private void SpawnRedMoonMinions()
    {
        if (minionSpawnPoints.Length < 2)
        {
            Debug.LogWarning("스폰위치 부족");
            return;
        }

        Debug.Log("부하 소환");

        GameObject[] minionPrefabs = { minion1, minion2};

        for (int i = 0; i < 2; i++)
        {
            if (minionPrefabs[i] == null || minionSpawnPoints[i] == null) continue;

            GameObject minion = Instantiate(minionPrefabs[i], minionSpawnPoints[i].position, Quaternion.identity);

            MinionController minionScript = minion.GetComponent<MinionController>();
            if (minionScript != null)
            {
                minionScript.InitMinion(maxHP * minionHpValue, i);
            }

            aliveMinions.Add(minion);
        }
        if (aliveMinions.Count > 0)
        {
            SetVisualActive(false);
            StartCoroutine(WaitForMinionsRoutine()); 
        }
    }

    private IEnumerator WaitForMinionsRoutine()
    {
        while (!isDead)
        {
            aliveMinions.RemoveAll(minion => minion == null);

            if (aliveMinions.Count == 0)
            {
                SetVisualActive(true);
                yield break; 
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    protected override void OnDeathEffect()
    {
        base.OnDeathEffect();
        if (spawnedCloud != null) Destroy(spawnedCloud);

        foreach (var minion in aliveMinions)
        {
            if (minion != null) Destroy(minion);
        }
    }


}
