using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("국가와 날씨")]
    [SerializeField] TextMeshProUGUI cityTxt;
    [SerializeField] TextMeshProUGUI tempTxt;

    [Header("몬스터 스탯")]
    [SerializeField] TextMeshProUGUI monName;
    [SerializeField] TextMeshProUGUI monHpTxt;

    [Header("현재 보유한 골드")]
    [SerializeField] TextMeshProUGUI goldTxt;


    public void updateInfo(string city, string degree)
    {
        //처치할떄마다 바뀐 국가와 날씨 정보를 가져와서 text에 삽입
        cityTxt.text = city;
        tempTxt.text = degree; //float로 받아와야할듯
        //-> 아니면 국가랑 날씨 정보를 따로 받아오기
    }
    
}
