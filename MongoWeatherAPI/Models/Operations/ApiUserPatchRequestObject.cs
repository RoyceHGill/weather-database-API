using MongoWeatherAPI.Models.Filters;

namespace MongoWeatherAPI.Models.Operations
{
    /// <summary>
    ///  Used to collect data for a patch object from an API call.
    /// </summary>
    public class ApiUserPatchRequestObject
    {
        /// <summary>
        /// Holds a filter Object for APIUsers
        /// </summary>
        public ApiUserFilter? Filter { get; set; }

        /// <summary>
        /// A string that refers to the key of the MongoDb Database
        /// </summary>
        public string? PropertyName { get; set; }

        /// <summary>
        /// The new value you wish to store. 
        /// </summary>
        public string? PropertyValue { get; set; }

    }
}
