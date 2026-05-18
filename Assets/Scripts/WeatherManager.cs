using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    private string apiKey = "0f2a2a069248bebbe4805b8dca9901c8";

    public WeatherData CurrentWeather {  get; private set; }

    private int currentCountryIndex = 0;

    private void Start()
    {
        
    }

    public void FetchWeather(string cityName, Action<bool> onCompleted )
    {
        
        StartCoroutine(GetWeatherCoroutine(cityName, onCompleted));
    }

    IEnumerator GetWeatherCoroutine(string city, Action<bool> onCompleted)
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            // 2. 예외 처리: 통신 실패 시
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"[{city}] 날씨 데이터를 가져오지 못했습니다: {request.error}");
                // 실패했다고 알려줌
                onCompleted?.Invoke(false);
            }
            // 3. 통신 성공 시
            else
            {
                string json = request.downloadHandler.text;

                // 받아온 텍스트를 WeatherData 클래스로 변환해서 캐싱(저장)
                CurrentWeather = JsonUtility.FromJson<WeatherData>(json);

                Debug.Log($"[{city}] 날씨 로드 완료! (상태: {CurrentWeather.weather[0].main}, 온도: {CurrentWeather.main.temp}도)");

                // 성공했다고 알려줌 -> 이 신호를 받으면 GameManager가 몬스터를 소환함!
                onCompleted?.Invoke(true);
            }
        }
    }

}
