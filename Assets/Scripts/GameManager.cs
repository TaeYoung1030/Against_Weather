using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public WeatherManager weatherManager;

    [Header("몬스터 so")]
    [SerializeField] List<WeatherMonsterData> normalMonster;
    [SerializeField] List<WeatherMonsterData> epicMonster;
    [SerializeField] List<WeatherMonsterData> legendMonster;
    [SerializeField] Transform AppearLocation;

    [Header("나라 세팅")]
    [SerializeField] List<string> allCites = new List<string> { "Tokyo", "Washington", "London", "Paris","Cairo","Moscow" };
    private List<string> currentStageCities = new List<string>();
    private int currentCityIndex = 0;

    [Header("Save Data (저장되는 값들)")]
    public int currentStage = 1;
    public int totalCoins = 0;

    [Header("몬스터 등장 스테이지")]
    [SerializeField] int targetKillsForEpic = 3;
    [SerializeField] int targetKillsForLegend = 6;
    [Header("UI매니저")]
    [SerializeField] GameUI gameUI;

    private int stageNormalKills = 0;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);

        LoadGameData();
    }

    private void Start()
    {
        StartNewStage();
    }

    void StartNewStage()
    {
        currentStageCities.Clear();
        //처음은 서울로 고정
        currentStageCities.Add("Seoul");

        //추후 리스트에서 무작위 나라들만 몇 개 뽑아서 add할 수 있도록
        //리스트 무작위로 섞기
        for(int i =0; i<allCites.Count; i++)
        {
            string temp = allCites[i];
            int randomIndex = Random.Range(i, allCites.Count);
            allCites[i] = allCites[randomIndex];
            allCites[randomIndex] = temp;
        }

        //섞은 리스트에서 5개 무작위 뽑기
        for(int i=0; i<5;i++)
        {
            currentStageCities.Add(allCites[i]);
        }

        currentCityIndex = 0;
        stageNormalKills = 0;

        if (gameUI != null)
        {
            gameUI.UpdateStage(currentStage);
        }

        EnterCity(currentStageCities[currentCityIndex]);

    }

    void EnterCity(string city)
    {
        weatherManager.FetchWeather(city, OnWeatherLoaded);
    }

    void OnWeatherLoaded(bool isSuccess)
    {
        if (isSuccess)
        {
            // 통신 성공! 몬스터 소환 시작
            SpawnMatchedNormalMonster();
            
            string currentCity = currentStageCities[currentCityIndex];
            float currentTemp = weatherManager.CurrentWeather.main.temp;

            gameUI.updateInfo(currentCity,currentTemp);
        }
        else
        {
            // 통신 실패 (인터넷 끊김 등). 강제로 맑음(Clear) 몬스터 소환
            Debug.LogWarning("통신 실패. 강제로 기본 몬스터를 소환합니다.");
            SpawnMonsterByWeather("Clear", true);
        }
    }

    void SpawnMatchedNormalMonster()
    {
        string currentWeatherMain = weatherManager.CurrentWeather.weather[0].main;
        string currentIcon = weatherManager.CurrentWeather.weather[0].icon;

        //d로 끝나면 낮, 아니면 밤
        bool isDayTime = currentIcon.EndsWith("d");

        Debug.Log($"현재 날씨 : {currentWeatherMain}");
        SpawnMonsterByWeather(currentWeatherMain, isDayTime);
    }

    void SpawnMonsterByWeather(string name, bool isDay)
    {
        WeatherMonsterData matchedData = null;

        foreach(var data in normalMonster)
        {
            bool isTimeMatched = (data.spawnTime == WeatherMonsterData.SpawnTimeType.Any) ||
                                 (isDay && data.spawnTime == WeatherMonsterData.SpawnTimeType.Day) ||
                                 (!isDay && data.spawnTime == WeatherMonsterData.SpawnTimeType.Night);

            if (data.weatherType == name && isTimeMatched)
            {
                matchedData = data;
                break;
            }
        }
        //예비 검색
        if (matchedData == null)
        {
            Debug.LogWarning($"[{name}] 날씨에 딱 맞는 몬스터가 없습니다! 시간대 무시하고 찾아봅니다.");
            foreach (var data in normalMonster)
            {
                if (data.weatherType == name)
                {
                    matchedData = data;
                    break;
                }
            }
        }

        if (matchedData == null)
        {
            Debug.Log("해당하는 so가 없습니다.");
            matchedData = normalMonster[0]; //임시 사용
        }

        GameObject newMonster = Instantiate(matchedData.monsterPrefab, AppearLocation);

        float hpMuliplier = Mathf.Pow(1.5f, currentStage - 1); //1.5배의 제곱
        float finalHp = matchedData.maxHP * hpMuliplier;

        newMonster.GetComponent<MonsterController>().InitMonster(matchedData, finalHp);
        if (gameUI != null)
        {
            gameUI.ResetHpBar();
        }
    }

    void SpawnEpicMonster()
    {
        if(epicMonster.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, epicMonster.Count);
        WeatherMonsterData selectedMon = epicMonster[randomIndex];

        GameObject newMonster = Instantiate(selectedMon.monsterPrefab, AppearLocation);

        
        float hpMultiplier = Mathf.Pow(1.7f, currentStage - 1);
        float finalHp = selectedMon.maxHP * hpMultiplier;

        newMonster.GetComponent<MonsterController>().InitMonster(selectedMon, finalHp);
        if (gameUI != null)
        {
            gameUI.ResetHpBar();
        }

    }

    void SpawnLegendMonster()
    {
        if(legendMonster.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, legendMonster.Count);
        WeatherMonsterData selectedMon = legendMonster[randomIndex];

        GameObject newMonster = Instantiate(selectedMon.monsterPrefab, AppearLocation);

        
        float hpMultiplier = Mathf.Pow(2f, currentStage - 1);
        float finalHp = selectedMon.maxHP * hpMultiplier;

        newMonster.GetComponent<MonsterController>().InitMonster(selectedMon, finalHp);
        if (gameUI != null)
        {
            gameUI.ResetHpBar();
        }
    }

    //몬스터 죽으면 호출 할 함수
    public void OnMonsterDie(WeatherMonsterData.MonsterTier tier)
    {
        //1. 골드 추가

        //2. 죽은 몬스터가 normal일때 
        if(tier == WeatherMonsterData.MonsterTier.Normal)
        {
            stageNormalKills++;
            //처치 횟수가 x마리 일때 -> epic소환 
           if(stageNormalKills == targetKillsForEpic)
           {
                SpawnEpicMonster();
           }
            //처치 횟수가 x마리 일때 -> legend 소환
           else if(stageNormalKills == targetKillsForLegend)
           {
                SpawnLegendMonster();
           }
            //그외 -> normal 다시 소환
           else
           {
                GoToNextCity();
           }
        }
        else if(tier == WeatherMonsterData.MonsterTier.Epic)
        {
            GoToNextCity();
        }
        else if(tier == WeatherMonsterData.MonsterTier.Legend)
        {
            ClearStage();
        }
        //3. 죽은 몬스터가 epic일때 -> 그 다음은 다시 normal이 나오게
        //4. 죽은 몬스터가 legend일때 -> 1스테이지가 끝났으니 다시 재시작 : 서울부터 재시작
        //현재 normal 몬스터가 나올때 나라도 무작위로 선정돼서 entercity같은걸로 api가져와야함
    }

    void GoToNextCity()
    {
        currentCityIndex++;
        if(currentCityIndex < currentStageCities.Count)
        {
            EnterCity(currentStageCities[currentCityIndex]);
        }
    }

    void ClearStage()
    {
        currentStage++;
        SaveGameData();

        StartNewStage();
    }

    public void FailMission()
    {
        //여기서 실패 ui 제출
        gameUI.UpdateFailMessage();
        currentStage = 1;
        SaveGameData();
        StartNewStage();
    }

    public void OnMonsterTakeDamage(float currentHp, float maxHp)
    {
        if (gameUI != null)
        {
            gameUI.UpdateHpBar(currentHp, maxHp);
        }
    }

    public void SaveGameData()
    {
        PlayerPrefs.SetInt("Stage", currentStage);
        PlayerPrefs.SetInt("Coins", totalCoins);
        PlayerPrefs.Save();
        Debug.Log("게임 저장 완료!");

    }
    void LoadGameData()
    {
        currentStage = PlayerPrefs.GetInt("Stage", 1);
        totalCoins = PlayerPrefs.GetInt("Coins", 0);
    }

}
