using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoWeatherAPI.Models.Filters
{
    public class WeatherDataIdFilter
    {
        public string ObjId { get; set; }
    }

}
