using UnityEngine;

public class LegendMonster : MonsterController
{
    public enum LegendType { BlackMoon, RedMoon, Tornado }

    [Header("에픽 보스 세팅")]
    [SerializeField] LegendType epicBossType;

 
}
