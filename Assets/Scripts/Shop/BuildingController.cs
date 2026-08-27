using UnityEngine;

public class BuildingController : MonoBehaviour
{
    public enum BuildingType { Buff, Gold, Attack, MissControl, StunControl }
    public BuildingType type;

    private void OnEnable()
    {
        // 설치될 때 자신 등록
        GameManager.instance.RegisterBuilding(this);
    }

    private void OnDisable()
    {
        // 철거될 때 자신 삭제
        if (GameManager.instance != null)
            GameManager.instance.UnregisterBuilding(this);
    }
}
