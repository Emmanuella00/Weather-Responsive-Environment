using System;



[Serializable]
public class WeatherResponse
{
    public Coord coord;
    public WeatherInfo[] weather;   
    public MainData main;
    public WindData wind;
    public CloudData clouds;
    public SysData sys;
    public long dt;                 
    public int timezone;            
    public string name;
}

[Serializable] public class Coord { public float lon; public float lat; }

[Serializable]
public class WeatherInfo
{
    public int id;                  
    public string main;             
    public string description;      
    public string icon;             
}

[Serializable] public class MainData { public float temp; public float feels_like; public int humidity; }
[Serializable] public class WindData { public float speed; public float deg; }
[Serializable] public class CloudData { public int all; }

[Serializable]
public class SysData
{
    public string country;          
    public long sunrise;            
    public long sunset;             
}



[Serializable] public class ApiError { public string message; }
