namespace MongoWeatherAPI.Models.Filters
{
    public class WeatherDataNameTimeFilter
    {
        /// <summary>
        /// String value, can be full or partial value.
        /// </summary>
        public string? DeviceNamePartial { get; set; }
        /// <summary>
        /// Starting DateTime of filter.
        /// </summary>
        public DateTime? TimeFrom { get; set; }
        /// <summary>
        /// Ending DateTime filter.
        /// </summary>
        public DateTime? TimeTo { get; set; }
    }
}
