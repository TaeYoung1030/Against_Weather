using System.Collections;
using UnityEngine;

public class EpicMonster : MonsterController
{
    public enum EpicType { Heat, Cold, Rain}

    [Header("에픽 보스 세팅")]
    [SerializeField] EpicType epicBossType;

    [Header("폭염 스킬 세팅")]
    [SerializeField] float recoverHP = 0.02f;

    [Header("한파 스킬 세팅")]
    [SerializeField] float damageReduce = 0.5f;

    [Header("호우 스킬 세팅")]
    [SerializeField] float missChance = 0.3f;

    protected override void ActivateEpicSkill()
    {
        switch(epicBossType)
        {
            //각 디버프 시작시 ui 에 알리기
            case EpicType.Heat:
                StartCoroutine(HeatWaveRegenRoutine());
                break;
            case EpicType.Cold:
                break;
            case EpicType.Rain:
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
            yield return new WaitForSeconds(1f); // 1초마다 회복
        }
    }


    protected override float ApplyPlayerDebuffToDamage(float incomingDamage)
    {
        // 스킬이 발동된 상태(체력 20% 이하)일 때만 디버프를 계산합니다.
        if (!isEpicSkillActivated) return incomingDamage;

        switch (epicBossType)
        {
            case EpicType.Cold:
                // [한파] 데미지를 반토막 냅니다.
                Debug.Log("한파 스킬로 공격력 감소");
                return incomingDamage * (1f - damageReduce);

            case EpicType.Rain:
                // [호우] 무작위 확률을 계산해 빗나감을 구현합니다.
                if (Random.value < missChance)
                {
                    Debug.Log(" 호우로 인해 플레이어의 공격이 빗나갔습니다! (MISS)");
                    // 필요 시 UI로 "MISS!" 텍스트를 띄우는 함수를 부를 수 있습니다.
                    return 0f; // 데미지 0 처리
                }
                break;
        }

        return incomingDamage;
    }

}
