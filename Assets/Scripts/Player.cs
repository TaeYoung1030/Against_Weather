using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] float basicDamage = 10;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
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

            // 4. 스크립트가 존재한다면 (즉, 맞은 게 몬스터가 확실하다면)
            if (targetMonster != null)
            {
                // 몬스터의 TakeDamage 함수를 불러와서 내 데미지를 넘겨줍니다!
                targetMonster.TakeDamage(basicDamage);

                // (선택) 부딪힌 위치(hit.point)에 타격 파티클 이펙트를 생성할 수도 있습니다.
            }
        }
    }

    //시간초과로 플레이어가 죽었을떄 -> GM의 StartNewStage로 이동하는 로직
}
