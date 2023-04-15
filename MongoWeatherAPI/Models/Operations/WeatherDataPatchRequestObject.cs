using MongoWeatherAPI.Models.Filters;

namespace MongoWeatherAPI.Models.Operations
{
    /// <summary>
    /// Used to collect data for a patch object from an API call.
    /// </summary>
    public class WeatherDataPatchRequestObject
    {
        /// <summary>
        /// Filters the Weather data that are effected.
        /// </summary>
        public WeatherDataFilter? Filter { get; set; }
        /// <summary>
        /// The property name to be effected.
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// the replacement value in string form. 
        /// </summary>
        public string PropertyValue { get; set; } 
    }
}
