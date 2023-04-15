namespace MongoWeatherAPI.Models.DTO
{
    public class WeatherDataCreateDTO
    {
        public string DeviceName { get; set; }
        public double PrecipitatonMMH { get; set; }
        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TemperatureC { get; set; }
        public double AtmosphericPressureKPA { get; set; }
        public double MaxWindSpeedMS { get; set; }
        public double SolarRadiationWM2 { get; set; }
        public double VaporPressureKPA { get; set; }
        public double HumidityPercetage { get; set; }
        public double WindDirection { get; set; }
    }
}
