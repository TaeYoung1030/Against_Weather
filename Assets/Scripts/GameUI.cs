using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    private float targetHpRatio = 1f;
    private Coroutine failMessageCoroutine; //광클 방지용 

    private void Start()
    {
        if (failMessage != null)
        {
            failMessage.gameObject.SetActive(false);
        }
    }


    public void updateInfo(string city, float temp)
    {
        //처치할떄마다 바뀐 국가와 날씨 정보를 가져와서 text에 삽입
        cityTxt.text = city;
        tempTxt.text = $"{temp:F1}°C"; 
        
    }

    private void Update()
    {
        if (mainHpSlider != null)
        {
            mainHpSlider.value = Mathf.Lerp(mainHpSlider.value, targetHpRatio, Time.deltaTime * hpLerpSpeed);
        }
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
            // 원하신다면 "스테이지 {stage}" 형태로 변경하셔도 됩니다.
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

}
