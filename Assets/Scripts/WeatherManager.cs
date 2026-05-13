using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    private string apiKey = "0f2a2a069248bebbe4805b8dca9901c8";

    public string[] countries = { "Seoul", "Tokyo", "Bangkok" };
    private int currentCountryIndex = 0;

    private void Start()
    {
        FetchWeatherForCurrentCountry();
    }

    public void FetchWeatherForCurrentCountry()
    {
        string cityName = countries[currentCountryIndex];
        StartCoroutine(GetWeatherCoroutine(cityName));
    }

    IEnumerator GetWeatherCoroutine(string city)
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if(request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                WeatherData data = JsonUtility.FromJson<WeatherData>(json);

                Debug.Log($"{data.name}의 온도: {data.main.temp}°C, 상태 : {data.weather[0].main}");

            }
            else
            {
                Debug.LogError("날씨 데이터 로드 실패 : " + request.error);
            }
        }
    }

    public void GoToNextCounty()
    {

    }
}
