using System;

namespace Bartendro.Application.Models.Weather
{
    public class WeatherForecastModel
    {
        public DateTime Date {get;set;}
        public int TemperatureC {get;set;}
        public float TemperatureF => 32F + TemperatureC / (5F / 9F);
        public string Summary {get;set;}
    }
}