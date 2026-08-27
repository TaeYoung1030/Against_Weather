using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour
{
    [Header("등급 정보")]
    [SerializeField] WeatherMonsterData.MonsterTier myTier;

    [Header("타격감 세팅")]
    [SerializeField] float knockbackDistance = 0.7f;
    [SerializeField] float hitAnimDuration = 0.15f;
    [SerializeField] float swaySpeed = 5f;
    [SerializeField] float swayAngle = 5f;
    [SerializeField] protected ParticleSystem hitParticle;

    [Header("외형 설정")]
    [SerializeField] private GameObject modelingChild;
    [SerializeField] private GameObject child1;
    [SerializeField] private GameObject child2;

    protected float timeLimit;
    protected float currentTimer;
    protected bool isTimerRunning = false;

    protected bool isEpicSkillActivated = false;
    protected bool isLegendSkillActivated = false;


    protected float targetHpRatio = 1f;

    protected float maxHP;
    protected float currentHP;
    protected int dropCoin;
    protected bool isDead = false;

    protected Vector3 originPos;
    protected Vector3 originScale;

    protected WeatherMonsterData myData;

    private Coroutine hitCoroutine;

    protected GameManager gameManager;
    protected GameUI gameUI;

    public virtual void InitMonster(WeatherMonsterData data, float Hp)
    {
        myData = data;
        maxHP = Hp;
        currentHP = maxHP;

        dropCoin = myData.coinReward;
        timeLimit = myData.catchTime;

        originPos = transform.position;
        originScale = transform.localScale;

        isDead = false;
        isEpicSkillActivated = false;
        isLegendSkillActivated = false;

        gameUI.ShowMonsterName(myData.monsterName);
        gameUI.ResetAllDebuffUI();

        gameUI.ChangeBackground(myData.bgImage);

        StartCoroutine(SpawnAnimation());

        if (myTier == WeatherMonsterData.MonsterTier.Epic || myTier == WeatherMonsterData.MonsterTier.Legend)
        {
            StartTimer();
            gameUI.ToggleCityWeatherUI(false);
        }
        else
        {
            gameUI.UpdateCatchTime(0f,false);
            gameUI.ToggleCityWeatherUI(true);
        }
    }

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        gameUI = FindFirstObjectByType<GameUI>();
    }

    protected virtual void Update()
    {
        float angle = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        if (isTimerRunning)
        {
            currentTimer -= Time.deltaTime;
            gameUI.UpdateCatchTime(currentTimer, isTimerRunning);
            if (currentTimer <= 0)
            {
                TimeOutFail();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if(isDead) return;
        float finalDamage = ApplyPlayerDebuffToDamage(damage);
        if (finalDamage <= 0) return;
        currentHP -= finalDamage;

        GameManager.instance.OnMonsterTakeDamage(currentHP, maxHP);
        Debug.Log($"남은 체력 : {currentHP}");
        OnHitEffect();

        if (myTier == WeatherMonsterData.MonsterTier.Epic && currentHP / maxHP <= 0.4f && !isEpicSkillActivated)
        {
            isEpicSkillActivated = true;
            gameUI.DebuffskillUI();
            ActivateEpicSkill();
        }

        if (myTier == WeatherMonsterData.MonsterTier.Legend && currentHP / maxHP <= 0.5f && !isLegendSkillActivated)
        {
            isLegendSkillActivated = true;
            gameUI.LegendskillUI();
            ActivateLegendSkill();
        }

        if (currentHP <= 0)
        {
            isDead = true;
            Die();
        }

        TriggerHitEffect();
    }

    private void StartTimer()
    {
        currentTimer = timeLimit;
        isTimerRunning = true;
    }

    protected void TimeOutFail()
    {
        isTimerRunning = false;
        isDead = true;

        Debug.Log("시간 제한 초과! 보스 공략 실패!");
        gameManager.FailMission();

        Destroy(gameObject);
    }

    protected void Die()
    {
        isTimerRunning = false;
        OnDeathEffect();
        GameManager.instance.OnMonsterDie(myTier,myData.coinReward);
        Destroy(gameObject);
    }

    protected virtual IEnumerator SpawnAnimation()
    {
        Vector3 startPos = originPos + Vector3.up * 8f;
        // 반동 주기 위한 용도
        Vector3 bounceDipPos = originPos + Vector3.down * 0.5f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            transform.position = Vector3.Lerp(startPos, bounceDipPos, t);
            yield return null;
        }
        //반동
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            transform.position = Vector3.Lerp(bounceDipPos, originPos, t);
            yield return null;
        }

        transform.position = originPos;
    }

    void TriggerHitEffect()
    {
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }

        hitCoroutine = StartCoroutine(HitAnimation());
    }

    protected virtual IEnumerator HitAnimation()
    {
        Vector3 squashedScale = new Vector3(originScale.x * 1.3f, originScale.y * 0.7f, originScale.z * 1.3f);

        Vector3 knockedPos = originPos + Vector3.forward * knockbackDistance;

        transform.localScale = squashedScale;
        transform.position = knockedPos;

        float elapsed = 0f;
        while (elapsed < hitAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hitAnimDuration;

            transform.localScale = Vector3.Lerp(squashedScale, originScale, t);
            transform.position = Vector3.Lerp(knockedPos, originPos, t);
            yield return null;
        }

        transform.localScale = originScale;
        transform.position = originPos;
    }
    protected virtual void OnHitEffect() 
    {
        if(hitParticle != null)
        {
            hitParticle.Stop();
            hitParticle.Play();
        }
    }

    public void SetVisualActive(bool isActive)
    {
        if (modelingChild != null)
        {
            modelingChild.SetActive(isActive);
        }

        if(child1 != null)
        {
            child1.SetActive(isActive);
        }

        if(child2 != null)
        {
            child2.SetActive(isActive);
        }
    }


    protected virtual void ActivateEpicSkill() { }
    protected virtual float ApplyPlayerDebuffToDamage(float incomingDamage) { return incomingDamage; }
    protected virtual void ActivateLegendSkill() { }
    protected virtual void OnDeathEffect() { }
  
}
