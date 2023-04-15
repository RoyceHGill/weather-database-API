namespace MongoWeatherAPI.Models.DTO
{
    public class DeviceNameTimeTempuratureDTO
    {
        public string? DeviceName { get; set; }
        public DateTime? Time { get; set; }
        public double? TemperatureC { get; set; }

    }
}
