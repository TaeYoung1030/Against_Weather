using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/ItemData")]
public class ShopData : ScriptableObject 
{
    public enum ItemType {Stat,Skill,Structure}

    [Header("기본 정보")]
    public string itemID;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;

    [Header("가격 설정")]
    public int baseCost;
    public float costMultiplier = 1.5f;

    [Header("기능 설정")]
    public float effectValue;
    public GameObject structurePrefab;
}
