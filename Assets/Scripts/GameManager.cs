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

    [Header("구조물  배치 세팅")]
    [SerializeField] Transform[] structureSpawnPoints;
    private GameObject[] builtStructures = new GameObject[2];

    [Header("상점 상태")]
    public bool isShopOpen = false;

    private List<BuildingController> activeBuildings = new List<BuildingController>();

    public void RegisterBuilding(BuildingController buildingController) => activeBuildings.Add(buildingController);
    public void UnregisterBuilding(BuildingController buildingController) => activeBuildings.Remove(buildingController);

    private MonsterController currentMonsterController;
    private int stageNormalKills = 0;

    private void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(gameObject);
        //초기화용
        //PlayerPrefs.DeleteAll();
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
            //몬스터 소환 시작
            SpawnMatchedNormalMonster();
            
            string currentCity = currentStageCities[currentCityIndex];
            float currentTemp = weatherManager.CurrentWeather.main.temp;

            gameUI.updateInfo(currentCity,currentTemp);
        }
        else
        {
            // 통신 실패시 강제 맑음 몬스터 소환
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
            //밤이랑 낮 구별
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
            Debug.Log("날씨에 맞는 몬스터 없음");
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
            Debug.Log("해당so 부족");
            matchedData = normalMonster[0]; //임시 사용
        }

        GameObject newMonster = Instantiate(matchedData.monsterPrefab, AppearLocation);
        //스테이지마다 점점 쎄지는 몬스터 체력
        float hpMuliplier = Mathf.Pow(1.5f, currentStage - 1); //1.5배의 제곱
        float finalHp = matchedData.maxHP * hpMuliplier;

        newMonster.GetComponent<MonsterController>().InitMonster(matchedData, finalHp);
        currentMonsterController = newMonster.GetComponent<MonsterController>();
        if (isShopOpen)
        {
            currentMonsterController.SetVisualActive(false);
        }
        if (gameUI != null)
        {
            gameUI.ResetHpBar();
            gameUI.SetTierIcon(WeatherMonsterData.MonsterTier.Normal);
        }
        //몬스터 공격하는 건물 로직
        ApplyAttackBuilding(newMonster.GetComponent<MonsterController>(), finalHp);
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
        currentMonsterController = newMonster.GetComponent<MonsterController>();
        if (isShopOpen)
        {
            currentMonsterController.SetVisualActive(false);
        }
        if (gameUI != null)
        {
            gameUI.ResetHpBar();
        }
        ApplyAttackBuilding(newMonster.GetComponent<MonsterController>(), finalHp);

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
        currentMonsterController = newMonster.GetComponent<MonsterController>();
        if (isShopOpen && currentMonsterController != null)
        {
            currentMonsterController.SetVisualActive(false);
        }

        if (gameUI != null)
        {
            gameUI.ResetHpBar();
        }
        ApplyAttackBuilding(newMonster.GetComponent<MonsterController>(), finalHp);
    }

    //몬스터 죽으면 호출 할 함수
    public void OnMonsterDie(WeatherMonsterData.MonsterTier tier, int coinReward)
    {
        //1. 골드 추가
        totalCoins += GetBonusGold(coinReward);
        if (gameUI != null) gameUI.UpdateCoinText(totalCoins);
        SaveGameData();
        //2. 죽은 몬스터가 normal일때 
        if (tier == WeatherMonsterData.MonsterTier.Normal)
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
        ClearAllStructures();
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

    public void ProcessPurchase(ShopData data, int nextLevel)
    {
        switch (data.itemType)
        {
            case ShopData.ItemType.Stat:
                //플레이어 스탯 관리
                Player.instance.UpgradeDamage(data.effectValue);
                break;

            case ShopData.ItemType.Skill:
               //보류
                break;

            case ShopData.ItemType.Structure:
                BuyStructure(data.structurePrefab, data.baseCost);
                break;
        }

        // 골드 소모 후 UI 갱신 (예시)
        // if (gameUI != null) gameUI.UpdateGoldText(totalCoins);
        SaveGameData(); // 골드 쓴 상태 저장
    }

    public void BuyStructure(GameObject structurePrefab, int cost)
    {
        if (totalCoins < cost) return;

        int emptySlotIndex = -1;
        for(int i=0; i<builtStructures.Length; i++)
        {
            if(builtStructures[i] == null)
            {
                emptySlotIndex = i;
                break;
            }
        }
        //빈자리 있을때
        if(emptySlotIndex != -1)
        {
            totalCoins -= cost;
            gameUI.UpdateCoinText(totalCoins);

            Transform targetPoint = structureSpawnPoints[emptySlotIndex];
            GameObject newBuilding = Instantiate(structurePrefab, targetPoint.position, targetPoint.rotation);
            builtStructures[emptySlotIndex] = newBuilding; // 리스트에 기록
        }
        else
        {
            // 두 자리 모두 꽉 참
            Debug.Log("자리가 없습니다");
        }
    }
    //좌,우 구조물 파괴 -> 버튼 누르는거에 따라 자동 파괴
    public void DeleteStructure(int slotIndex) 
    {
        if (builtStructures[slotIndex] != null)
        {
            Destroy(builtStructures[slotIndex]);
            builtStructures[slotIndex] = null; 
        }
    }


    private void ClearAllStructures()
    {
        foreach (GameObject structure in builtStructures)
        {
            if (structure != null) Destroy(structure);
        }
        //builtStructures.Clear(); 초기화시키는 로직 필요
        System.Array.Clear(builtStructures, 0, builtStructures.Length);
        Debug.Log("미션 실패로 초기화");
    }
    public void ToggleMonsterVisual(bool isShow)
    {
        isShopOpen = !isShow;

        if (currentMonsterController == null)
        {
            return;
        }

        currentMonsterController.SetVisualActive(isShow);
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

    public float GetBuff()
    {
        return activeBuildings.Exists(b => b.type == BuildingController.BuildingType.Buff) ? 1.05f : 1.0f;
    }

    public int GetBonusGold(int originalReward)
    {
        return activeBuildings.Exists(b => b.type == BuildingController.BuildingType.Gold) ? originalReward + 15 : originalReward;
    }

    public void ApplyAttackBuilding(MonsterController monster, float maxHp)
    {
        if (activeBuildings.Exists(b => b.type == BuildingController.BuildingType.Attack))
        {
            monster.TakeDamage(maxHp * 0.15f);
            Debug.Log("공격 건물이 몬스터 체력 15%를 즉시 공격!");
        }
    }

    public bool IsMissControlActive() => activeBuildings.Exists(b => b.type == BuildingController.BuildingType.MissControl);
    public bool IsStunControlActive() => activeBuildings.Exists(b => b.type == BuildingController.BuildingType.StunControl);

    public bool DestroyRandomStructure()
    {
        List<int> existingIndices = new List<int>();
        for (int i = 0; i < builtStructures.Length; i++)
        {
            if (builtStructures[i] != null) existingIndices.Add(i);
        }

        // 설치된 구조물이 있다면
        if (existingIndices.Count > 0)
        {
            int randomIndex = existingIndices[Random.Range(0, existingIndices.Count)];
            DeleteStructure(randomIndex);
            Debug.Log($"{randomIndex}번 구조물이 토네이도에 의해 파괴되었습니다!");
            return true;
        }

        return false;
    }

}
