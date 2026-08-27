using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructureItemUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI descTxt;
    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private Button buyBtn;

    [Header("이 슬롯의 구조물 데이터")]
    [SerializeField] private ShopData myStructureData;

    private void Start()
    {
        if (myStructureData != null)
        {
            iconImg.sprite = myStructureData.icon;
            nameTxt.text = myStructureData.itemName;
            descTxt.text = myStructureData.description;
            costTxt.text = $"{myStructureData.baseCost} G";

            // 구매 버튼에 TryBuy 함수 연결
            buyBtn.onClick.AddListener(TryBuy);
        }
    }

    private void Update()
    {
        if (myStructureData != null)
        {
            buyBtn.interactable = GameManager.instance.totalCoins >= myStructureData.baseCost;
        }
    }

    private void TryBuy()
    {
        GameManager.instance.ProcessPurchase(myStructureData, 1);
    }
}
