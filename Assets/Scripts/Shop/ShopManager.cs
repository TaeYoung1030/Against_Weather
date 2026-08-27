using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("상점 패널 설정")]
    [SerializeField] RectTransform shopPanel;
    [SerializeField] float slideSpeed = 15f;

    Vector2 closedPos = new Vector2(0, -2000f);
    Vector2 openPos = new Vector2(0, 0f);

    Coroutine slideCoroutine;
    [Header("첫 선택지 화면")]
    [SerializeField] GameObject selectionMenu;

    [Header("탭 패널 설정")]
    [SerializeField] GameObject[] detailPanels;
    [SerializeField] Button[] tabButtons;

    private void Start()
    {
        if(shopPanel != null)
        {
            shopPanel.anchoredPosition = closedPos;
        }

        for(int i=0; i<tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => OpenDetailPanel(index));
        }

    }

    public void OpenShop()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(openPos));

        GameManager.instance.ToggleMonsterVisual(false);
        BackToMenu();
    }

    public void CloseShop()
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(closedPos));

        GameManager.instance.ToggleMonsterVisual(true);
    }

    private IEnumerator SlideRoutine(Vector2 targetPos)
    {
        while (Vector2.Distance(shopPanel.anchoredPosition, targetPos) > 1f)
        {
            shopPanel.anchoredPosition = Vector2.Lerp(shopPanel.anchoredPosition, targetPos, Time.deltaTime * slideSpeed);
            yield return null;
        }
        shopPanel.anchoredPosition = targetPos;
    }

    public void OpenDetailPanel(int panelIndex)
    {
        selectionMenu.SetActive(false); // 선택지 3개 화면은 숨김

        for (int i = 0; i < detailPanels.Length; i++)
        {
            if (detailPanels[i] != null)
            {
                detailPanels[i].SetActive(i == panelIndex); // 내가 누른 패널만 켬
            }
        }
    }

    public void BackToMenu()
    {
        selectionMenu.SetActive(true); 

        for (int i = 0; i < detailPanels.Length; i++)
        {
            if (detailPanels[i] != null)
            {
                detailPanels[i].SetActive(false); 
            }
        }
    }
}
