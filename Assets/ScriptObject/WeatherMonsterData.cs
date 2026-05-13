using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "ScriptableObjects/MonsterData")]
public class WeatherMonsterData : ScriptableObject
{
    public string weatherType;
    public string monsterName;
    public GameObject monsterPrefab;
    public float maxHP;
    public float coinReward;
}
