using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    public static Player instance;

    [Header("플레이어 스탯")]
    [SerializeField] public float basicDamage = 10;
    [Header("사운드")]
    [SerializeField] AudioClip clip;

    AudioSource asc;
    private float defaultDamage = 10f;

    private void Awake()
    {
        asc = GetComponent<AudioSource>();

        if(instance == null) instance = this;
        else Destroy(instance);

        LoadPlayerStat();
    }

    public void UpgradeDamage(float amount)
    {
        basicDamage += amount;
        SavePlayerStat();
        Debug.Log($"[플레이어] 공격력이 상승했습니다! 현재 공격력: {basicDamage}");
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            asc.PlayOneShot(clip);
            AttackMonster();
        }
    }

    private void AttackMonster()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit) )
        {
            MonsterController targetMonster = hit.collider.GetComponent<MonsterController>();

            if (targetMonster != null)
            {
                targetMonster.TakeDamage(basicDamage*GameManager.instance.GetBuff());
                return;
            }

            MinionController minion = hit.collider.GetComponent<MinionController>();
            if (minion != null)
            {
                minion.TakeDamage(basicDamage); 
                return;
            }
        }
    }
    private void SavePlayerStat()
    {
        PlayerPrefs.SetFloat("PlayerDamage",basicDamage);
        PlayerPrefs.Save();
    }

    private void LoadPlayerStat()
    {
        basicDamage = PlayerPrefs.GetFloat("PlayerDamage", defaultDamage);
    }
}
