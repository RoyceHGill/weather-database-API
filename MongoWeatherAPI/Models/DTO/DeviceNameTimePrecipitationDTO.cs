namespace MongoWeatherAPI.Models.DTO
{
    public class DeviceNameTimePrecipitationDTO
    {
        public string DeviceName { get; set; }
        public DateTime Time { get; set; }
        public double? PrecipitationMMH { get; set; }
    }
}
