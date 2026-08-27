using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EpicMonster : MonsterController
{
    public enum EpicType { Heat, Cold, Rain, Thunder}

    [Header("에픽 보스 세팅")]
    [SerializeField] EpicType epicBossType;

    [Header("폭염 스킬 세팅")]
    [SerializeField] float recoverHP = 0.02f;

    [Header("한파 스킬 세팅")]
    [SerializeField] float damageReduce = 0.5f;

    [Header("호우 스킬 세팅")]
    [SerializeField] float missChance = 0.3f;

    [Header("천둥 스킬 세팅")]
    [SerializeField] float stunChance = 0.15f; //기절 확률
    [SerializeField] float stunDuration = 2f; //기절시간

    private bool isPlayerStunned = false;


    public override void InitMonster(WeatherMonsterData data, float Hp)
    {
        base.InitMonster(data, Hp);

        if (gameUI != null)
        {
            gameUI.SetTierIcon(WeatherMonsterData.MonsterTier.Epic);
        }
    }
    protected override void ActivateEpicSkill()
    {
        switch(epicBossType)
        {
            case EpicType.Heat:
                gameUI.ActiveICon("Heat", true);
                StartCoroutine(HeatWaveRegenRoutine());
                break;
            case EpicType.Cold:
                gameUI.ActiveICon("Cold", true);
                break;
            case EpicType.Rain:
            case EpicType.Thunder:
                break;
        }
    }

    private IEnumerator HeatWaveRegenRoutine()
    {
        while (!isDead)
        {
            if (currentHP < maxHP)
            {
                float regenAmount = maxHP * recoverHP;
                currentHP = Mathf.Min(maxHP, currentHP + regenAmount);
                GameManager.instance.OnMonsterTakeDamage(currentHP, maxHP);
                Debug.Log($"[폭염 효과] 체력 재생 완료. 현재 체력: {currentHP}");
            }
            yield return new WaitForSeconds(1f);
        }
    }


    protected override float ApplyPlayerDebuffToDamage(float incomingDamage)
    {
        if (!isEpicSkillActivated) return incomingDamage;

        if (isPlayerStunned)
        {
            return 0f;
        }

        switch (epicBossType)
        {
            case EpicType.Cold:
                //데미지 감소 구현
                Debug.Log("공격력 감소");
                return incomingDamage * (1f - damageReduce);

            case EpicType.Rain:
                //빗나감 구현, 빗나감 무시 구현
                if (GameManager.instance.IsMissControlActive())
                {
                    Debug.Log("빗나감 방어");
                    return incomingDamage;
                }
                if (Random.value < missChance)
                {
                    Debug.Log("빗나감");
                    gameUI.MissUI();
                    return 0f;
                }
                break;
            case EpicType.Thunder:
                //기절 방어 및 기절 효과
                if (GameManager.instance.IsStunControlActive())
                {
                    Debug.Log("기절 방어");
                    return incomingDamage;
                }
                if (Random.value < stunChance)
                {
                    StartCoroutine(PlayerStunRoutine());
                    gameUI.StunUI();
                    return 0f;
                }
                break;
        }

        return incomingDamage;
    }

    private IEnumerator PlayerStunRoutine()
    {
        isPlayerStunned = true;
        //gameUI.ToggleStunUI(true);
        yield return new WaitForSeconds(stunDuration);
        isPlayerStunned = false;
        //gameUI.ToggleStunUI(false);
    }

}
