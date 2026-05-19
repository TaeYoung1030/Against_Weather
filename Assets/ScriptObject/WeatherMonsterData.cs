using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData")]
public class WeatherMonsterData : ScriptableObject
{
    [Header("몬스터 세팅")]
    public string weatherType;
    public string monsterName;
    public GameObject monsterPrefab;
    [Header("몬스터 특성")]
    public float maxHP;
    public int coinReward;

    public enum SpawnTimeType
    {
        Day,
        Night,
        Any
    }

    public enum MonsterTier
    {
        Normal,
        Epic,
        Legend
    }

    [Header("낮과 밤 구별")]
    public SpawnTimeType spawnTime = SpawnTimeType.Any;
    [Header("몬스터 희귀도")]
    public MonsterTier monsterTier = MonsterTier.Normal;
}
