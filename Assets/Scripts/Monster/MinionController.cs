using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionController : MonoBehaviour
{
    [Header("타격감 세팅")]
    [SerializeField] float knockbackDistance = 0.5f;
    [SerializeField] float hitAnimDuration = 0.15f;

    private float maxHP;
    private float currentHP;
    private int myUIIndex;

    private Vector3 originPos;
    private Vector3 originScale;
    private Coroutine hitCoroutine;

    private GameUI gameui;

    public void InitMinion(float hp, int uiIndex)
    {
        maxHP = hp;
        currentHP = maxHP;
        myUIIndex = uiIndex;

        originPos = transform.position;
        originScale = transform.localScale;

        gameui.InitMinionUI(myUIIndex, maxHP);
    }

    private void Awake()
    {
        gameui = FindFirstObjectByType<GameUI>();   
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (Mathf.RoundToInt(currentHP) <= 0)
        {
            currentHP = 0f;
            gameui.UpdateMinionHpBar(myUIIndex, currentHP, maxHP);

            gameui.HideMinionUI(myUIIndex);
            Destroy(gameObject);
        }
        else
        {
            gameui.UpdateMinionHpBar(myUIIndex, currentHP, maxHP);
            TriggerHitEffect();
        }
    }

    void TriggerHitEffect()
    {
        // 광클 시 애니메이션 꼬임 방지
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }

        hitCoroutine = StartCoroutine(HitAnimation());
    }

    private IEnumerator HitAnimation()
    {
        // 찌그러짐 이펙트
        Vector3 squashedScale = new Vector3(originScale.x * 1.3f, originScale.y * 0.7f, originScale.z * 1.3f);
        // 넉백 이펙트
        Vector3 knockedPos = originPos + Vector3.forward * knockbackDistance;

        // 즉시 찌그러지고 밀려난 상태로 세팅
        transform.localScale = squashedScale;
        transform.position = knockedPos;

        // 원래 상태로 복귀
        float elapsed = 0f;
        while (elapsed < hitAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitAnimDuration;

            transform.localScale = Vector3.Lerp(squashedScale, originScale, t);
            transform.position = Vector3.Lerp(knockedPos, originPos, t);
            yield return null;
        }

        // 오차 보정
        transform.localScale = originScale;
        transform.position = originPos;
    }

}
