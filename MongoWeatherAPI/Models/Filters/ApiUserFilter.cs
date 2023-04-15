using MongoDB.Bson;

namespace MongoWeatherAPI.Models.Filters
{
    /// <summary>
    /// Object used to build a Filter Definition.
    /// </summary>
    public class ApiUserFilter
    {
        /// <summary>
        /// A specified MongoDB Object ID
        /// </summary>
        public ObjectId? _id { get; set; }

        /// <summary>
        /// Earliest Point of a time frame filter.
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Latest point of a time frame filter.
        /// </summary>
        public DateTime? CreatedTo { get; set; }
    }
}
