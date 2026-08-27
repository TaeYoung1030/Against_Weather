using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatUpgrade : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TextMeshProUGUI currentDmgText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button upgradeBtn;

    [Header("강화 설정")]
    [SerializeField] private int baseCost = 100;
    [SerializeField] private float costMultiplier = 1.2f; // 강화할 때마다 1.2배씩 비싸짐
    [SerializeField] private float upgradeAmount = 5f; // 한 번에 오를 공격력

    private int currentUpgradeLevel = 0;
    private int currentCost;

    private GameUI gameUI;

    private void Awake()
    {
        gameUI = FindFirstObjectByType<GameUI>();
    }

    private void Start()
    {
        // 저장된 레벨 불러오기
        currentUpgradeLevel = PlayerPrefs.GetInt("AttackUpgradeLevel", 0);
        upgradeBtn.onClick.AddListener(TryUpgrade);
        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Player.instance == null || GameManager.instance == null) return;

        // 현재 공격력 표시
        currentDmgText.text = $"현재 공격력: {Player.instance.basicDamage}";

        // 다음 강화 비용 계산
        currentCost = Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentUpgradeLevel));
        costText.text = $"{currentCost} G";

        //코인 부족시 비활성화 
        upgradeBtn.interactable = GameManager.instance.totalCoins >= currentCost;
    }

    private void TryUpgrade()
    {
        if (GameManager.instance.totalCoins >= currentCost)
        {
            GameManager.instance.totalCoins -= currentCost;
            gameUI.UpdateCoinText(GameManager.instance.totalCoins);

            Player.instance.basicDamage += upgradeAmount;
            currentUpgradeLevel++;

            PlayerPrefs.SetInt("AttackUpgradeLevel", currentUpgradeLevel);
            PlayerPrefs.SetFloat("PlayerDamage", Player.instance.basicDamage);
            PlayerPrefs.Save();

            UpdateUI();
        }
    }
}
