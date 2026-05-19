using System.Collections;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [Header("등급 정보")]
    [SerializeField] WeatherMonsterData.MonsterTier myTier;

    [Header("타격감 세팅")]
    [SerializeField] float knockbackDistance = 1.5f;
    [SerializeField] float hitAnimDuration = 0.15f;
    [SerializeField] float swaySpeed = 5f;
    [SerializeField] float swayAngle = 5f;

    protected float maxHP;
    protected float currentHP;
    protected int dropCoin;
    protected bool isDead = false;

    Vector3 originPos;
    Vector3 originScale;

    protected WeatherMonsterData myData;

    private Coroutine hitCoroutine;

    public virtual void InitMonster(WeatherMonsterData data, float Hp)
    {
        myData = data;
        maxHP = Hp;
        currentHP = maxHP;

        dropCoin = myData.coinReward;

        originPos = transform.position;
        originScale = transform.localScale;

        StartCoroutine(SpawnAnimation());
    }

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void TakeDamage(float damage)
    {
        if(isDead) return;

        currentHP -= damage;
        Debug.Log($"남은 체력 : {currentHP}");
        OnHitEffect();

        if(currentHP <= 0)
        {
            isDead = true;
            Die();
        }

        TriggerHitEffect();
    }

    protected virtual void OnHitEffect()
    {

    }

    protected virtual void OnDeathEffect()
    {

    }

    protected void Die()
    {
        OnDeathEffect();
        GameManager.instance.OnMonsterDie(myTier);
        Destroy(gameObject);
    }

    IEnumerator SpawnAnimation()
    {
        // 하늘 위(Y축 +8)에서 시작
        Vector3 startPos = originPos + Vector3.up * 8f;
        // 반동을 위해 원래 위치보다 살짝 아래(Y축 -0.5)를 목표로 지정
        Vector3 bounceDipPos = originPos + Vector3.down * 0.5f;

        // 1단계: 하늘에서 아래로 빠르게 떨어짐
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f; // 떨어지는 속도
            transform.position = Vector3.Lerp(startPos, bounceDipPos, t);
            yield return null;
        }

        // 2단계: 살짝 파고들었던 곳에서 원래 위치로 튕겨 올라옴 (반동)
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f; // 복귀 속도
            transform.position = Vector3.Lerp(bounceDipPos, originPos, t);
            yield return null;
        }

        // 오차 보정 (정확히 원래 자리로)
        transform.position = originPos;
    }

    void TriggerHitEffect()
    {
        //핵심: 만약 이미 맞는 모션이 재생 중이라면 즉시 취소!
        // 이 코드가 있어야 광클 시 뒤로 계속 밀려나지 않고 버퍼링 걸린 것처럼 리셋됩니다.
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }

        // 새로운 피격 모션 시작
        hitCoroutine = StartCoroutine(HitAnimation());
    }

    IEnumerator HitAnimation()
    {
        // (요청 2-1) 찌그러짐 이펙트: 위아래로 납작해지고 옆으로 퍼짐
        Vector3 squashedScale = new Vector3(originScale.x * 1.3f, originScale.y * 0.7f, originScale.z * 1.3f);

        // (요청 2-2) 넉백 이펙트: 카메라 반대 방향(Z축)으로 훅 밀려남
        Vector3 knockedPos = originPos + Vector3.forward * knockbackDistance;

        // 맞자마자 즉시 찌그러지고 뒤로 밀려난 상태로 만듭니다. (찰진 타격감을 위해)
        transform.localScale = squashedScale;
        transform.position = knockedPos;

        // 이제 원래 상태(originPos, originScale)로 부드럽게 돌아옵니다.
        float elapsed = 0f;
        while (elapsed < hitAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitAnimDuration;

            transform.localScale = Vector3.Lerp(squashedScale, originScale, t);
            transform.position = Vector3.Lerp(knockedPos, originPos, t);
            yield return null;
        }

        // 오차 보정 (완벽하게 원래 상태로 복구)
        transform.localScale = originScale;
        transform.position = originPos;
    }
}
