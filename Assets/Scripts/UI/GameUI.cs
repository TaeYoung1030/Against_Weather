using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.MTP;

public class GameUI : MonoBehaviour
{
    [Header("국가와 날씨")]
    [SerializeField] TextMeshProUGUI cityTxt;
    [SerializeField] TextMeshProUGUI tempTxt;

    [Header("몬스터 스탯")]
    [SerializeField] TextMeshProUGUI monName;
    [SerializeField] TextMeshProUGUI monHpTxt;
    [SerializeField] Slider mainHpSlider;
    [SerializeField] float hpLerpSpeed = 10f;

    [Header("현재 보유한 골드")]
    [SerializeField] TextMeshProUGUI goldTxt;

    [Header("실패시 메시지")]
    [SerializeField] TextMeshProUGUI failMessage;
    [SerializeField] float messageDuration = 2f;

    [Header("현 스테이지")]
    [SerializeField] TextMeshProUGUI stageTxt;

    [Header("몬스터 처치 제한 시간")]
    [SerializeField] TextMeshProUGUI catchTimeTxt;

    [Header("디버프 상시 유지 UI")]
    [SerializeField] GameObject heatRegenIcon;
    [SerializeField] GameObject coldDebuffIcon;
    [SerializeField] GameObject redMoonPassiveIcon;

    [Header("기절/빗나감 UI")]
    [SerializeField] GameObject missNoticeObj;
    [SerializeField] GameObject stunNoticeObj;
    [SerializeField] float duration = 0.5f;

    [Header("스킬 UI")]
    [SerializeField] GameObject LegendskillAwakeNoticeObj;
    [SerializeField] GameObject DebuffNoticeObj;
    [SerializeField] float skillDuration = 1.5f;

    private Coroutine missCoroutine;
    private Coroutine stunCoroutine;
    private Coroutine awakeCoroutine;

    [Header("미니언 UI")]
    [SerializeField] GameObject[] minionPanels;
    [SerializeField] Slider[] minionHpSliders;
    [SerializeField] TextMeshProUGUI[] minionHpTexts;

    [Header("몬스터 이름 UI")]
    [SerializeField] GameObject motionTitlePrefab;
    [SerializeField] TextItem[] titleTexts;
    [SerializeField] float titleDisplayTime = 2.5f;

    [Header("배경 이미지")]
    [SerializeField] Image backgroundImage;

    [Header("보스 전용 상단 ui")]
    [SerializeField] GameObject epicIcon;
    [SerializeField] GameObject legendIcon;


    private float[] minionTargetHpRatios = new float[2] { 1f, 1f };

    private float targetHpRatio = 1f;
    private Coroutine failMessageCoroutine; //광클 방지용 
    private Coroutine titleCoroutine;

    private void Start()
    {
        if (failMessage != null)
        {
            failMessage.gameObject.SetActive(false);
        }

        if (motionTitlePrefab != null)
        {
            motionTitlePrefab.SetActive(false);
        }

        ResetAllDebuffUI();
    }

    public void ResetAllDebuffUI()
    {
        if (heatRegenIcon != null) heatRegenIcon.SetActive(false);
        if (coldDebuffIcon != null) coldDebuffIcon.SetActive(false);
        if (redMoonPassiveIcon != null) redMoonPassiveIcon.SetActive(false);
        if (missNoticeObj != null) missNoticeObj.SetActive(false);
        if (stunNoticeObj != null) stunNoticeObj.SetActive(false);
        if (LegendskillAwakeNoticeObj != null) LegendskillAwakeNoticeObj.SetActive(false);
        if(DebuffNoticeObj != null) DebuffNoticeObj.SetActive(false);
    }

    public void ActiveICon(string debuff, bool isActive)
    {
        if (debuff == "Heat" && heatRegenIcon != null) heatRegenIcon.SetActive(isActive);
        if (debuff == "Cold" && coldDebuffIcon != null) coldDebuffIcon.SetActive(isActive);
        if (debuff == "RedMoon" && redMoonPassiveIcon != null) redMoonPassiveIcon.SetActive(isActive);
    }

    public void MissUI()
    {
        if (missNoticeObj == null) return;
        if (missCoroutine != null) StopCoroutine(missCoroutine);

        missCoroutine = StartCoroutine(changeUI(missNoticeObj, duration));

    }

    public void StunUI()
    {
        if (stunNoticeObj == null) return;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(changeUI(stunNoticeObj, duration));
    }

    public void LegendskillUI()
    {
        if (LegendskillAwakeNoticeObj == null) return;

        if (awakeCoroutine != null) StopCoroutine(awakeCoroutine);
        awakeCoroutine = StartCoroutine(changeUI(LegendskillAwakeNoticeObj, skillDuration));
    }

    public void DebuffskillUI()
    {
        if (DebuffNoticeObj == null) return;

        if (awakeCoroutine != null) StopCoroutine(awakeCoroutine);
        awakeCoroutine = StartCoroutine(changeUI(DebuffNoticeObj, skillDuration));
    }


    private IEnumerator changeUI(GameObject ui, float duration)
    {
        ui.SetActive(false);
        ui.SetActive(true);
        yield return new WaitForSeconds(duration);
        ui.SetActive(false);
    }

    public void ChangeBackground(Sprite newBg)
    {
        if(backgroundImage != null && newBg != null)
        {
            backgroundImage.sprite = newBg;
        }
    }

    public void updateInfo(string city, float temp)
    {
        //처치할떄마다 바뀐 국가와 날씨 정보를 가져와서 text에 삽입
        cityTxt.text = city;
        tempTxt.text = $"{temp:F1}°C"; 
        
    }

    public void UpdateCoinText(int currentCoins)
    {
        if(goldTxt != null)
        {
            goldTxt.text = currentCoins.ToString();
        }
    }

    private void Update()
    {
        if (mainHpSlider != null)
        {
            mainHpSlider.value = Mathf.Lerp(mainHpSlider.value, targetHpRatio, Time.deltaTime * hpLerpSpeed);
        }

        for (int i = 0; i < minionHpSliders.Length; i++)
        {
            if (minionHpSliders[i] != null && minionPanels[i] != null && minionPanels[i].activeSelf)
            {
                minionHpSliders[i].value = Mathf.Lerp(minionHpSliders[i].value, minionTargetHpRatios[i], Time.deltaTime * hpLerpSpeed);
            }
        }
    }

    public void ShowMonsterName(string monsterName)
    {
        if (motionTitlePrefab == null) return;

        if (titleCoroutine != null)
        {
            StopCoroutine(titleCoroutine);
        }
        foreach (var txtItem in titleTexts)
        {
            if (txtItem != null)
            {
                txtItem.text = monsterName; 
                txtItem.UpdateText();     
            }
        }
        motionTitlePrefab.SetActive(false);
        motionTitlePrefab.SetActive(true);

        titleCoroutine = StartCoroutine(HideTitleRoutine());
    }

    private IEnumerator HideTitleRoutine()
    {
        yield return new WaitForSeconds(titleDisplayTime);

        if (motionTitlePrefab != null)
        {
            motionTitlePrefab.SetActive(false);
        }

        titleCoroutine = null;
    }

    public void ResetHpBar()
    {
        targetHpRatio = 1f;
        if (mainHpSlider != null)
        {
            mainHpSlider.value = 1f;
        }
    }

    public void UpdateHpBar(float currentHp, float maxHp)
    {
        // 0 이하로 안 내려가게 방어하고 비율(0.0 ~ 1.0) 계산
        targetHpRatio = Mathf.Max(0, currentHp) / maxHp;

        if (monHpTxt != null)
        {
            monHpTxt.text = $"{Mathf.RoundToInt(Mathf.Max(0, currentHp))} / {Mathf.RoundToInt(maxHp)}";
        }
    }

    public void UpdateFailMessage()
    {
        if (failMessageCoroutine != null)
        {
            StopCoroutine(failMessageCoroutine);
        }
        failMessageCoroutine = StartCoroutine(ShowAndHideMessageRoutine());
        Debug.Log("미션 실패");
    }

    private IEnumerator ShowAndHideMessageRoutine()
    {
        failMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        failMessage.gameObject.SetActive(false);

        failMessageCoroutine = null;
    }

    public void UpdateStage(int stage)
    {
        if (stageTxt != null)
        {
            stageTxt.text = $"STAGE {stage}";
        }
    }

    public void UpdateCatchTime(float time, bool NotNormal)
    {
        if (catchTimeTxt == null) return;

        catchTimeTxt.gameObject.SetActive(NotNormal);

        if (NotNormal)
        {
            float displayTime = Mathf.Max(0f, time);

            int seconds = Mathf.FloorToInt(displayTime);
            int milliseconds = Mathf.FloorToInt((displayTime % 1f) * 100f);

            catchTimeTxt.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
        }
    }

    public void ToggleCityWeatherUI(bool  isNormalMonster)
    {
        if (cityTxt != null) cityTxt.gameObject.SetActive(isNormalMonster);
        if (tempTxt != null) tempTxt.gameObject.SetActive(isNormalMonster);
    }

    //미니언들 관련 ui함수
    public void InitMinionUI(int index, float maxHp)
    {
        if (index < 0 || index >= minionPanels.Length) return;

        minionPanels[index].SetActive(true);
        minionTargetHpRatios[index] = 1f;

        if (minionHpSliders[index] != null) minionHpSliders[index].value = 1f;

        UpdateMinionHpText(index, maxHp, maxHp);
    }

    public void UpdateMinionHpBar(int index, float currentHp, float maxHp)
    {
        if (index < 0 || index >= minionPanels.Length) return;

        minionTargetHpRatios[index] = Mathf.Max(0, currentHp) / maxHp;
        UpdateMinionHpText(index, currentHp, maxHp);
    }

    private void UpdateMinionHpText(int index, float currentHp, float maxHp)
    {
        if (minionHpTexts[index] != null)
        {
            minionHpTexts[index].text = $"{Mathf.Max(0, currentHp):F0} / {maxHp:F0}";
        }
    }

    public void HideMinionUI(int index)
    {
        if (index < 0 || index >= minionPanels.Length) return;
        minionPanels[index].SetActive(false);
    }

    public void SetTierIcon(WeatherMonsterData.MonsterTier tier)
    {
        if (epicIcon != null) epicIcon.SetActive(false);
        if (legendIcon != null) legendIcon.SetActive(false);

        switch (tier)
        {
            case WeatherMonsterData.MonsterTier.Epic:
                if (epicIcon != null) epicIcon.SetActive(true);
                break;
            case WeatherMonsterData.MonsterTier.Legend:
                if (legendIcon != null) legendIcon.SetActive(true);
                break;
            case WeatherMonsterData.MonsterTier.Normal:
                break;
        }
    }

}
