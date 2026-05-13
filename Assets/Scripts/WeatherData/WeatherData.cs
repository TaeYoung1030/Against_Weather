using System;
using UnityEditor.U2D.Animation;

[Serializable]
public class WeatherData
{
    public MainData main;
    public WeatherInfo[] weather;
    public CloudData clouds;
    public string name;
}

[Serializable]
public class MainData
{
    public float temp;
    public int humidity;
}

[Serializable]
public class WeatherInfo
{
    public string main;
}

[Serializable]
public class CloudData
{
    public int all;
}

