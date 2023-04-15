namespace MongoWeatherAPI.Models.DTO
{
    public class EnvironmentalReadingDTO
    {
        public string DeviceName { get; set; }
        public double? TemperatureC { get; set; }
        public double? AtmosphericPressure { get; set; }
        public double? Radiation { get; set; }
        public double? Precipitation { get; set; }
        public DateTime? Time { get; set; }
    }
}
