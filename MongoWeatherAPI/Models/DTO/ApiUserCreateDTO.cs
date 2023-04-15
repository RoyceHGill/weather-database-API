using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoWeatherAPI.Models.DTO
{
    public class ApiUserCreateDTO
    {
        [JsonIgnore]
        public ObjectId _id { get; set; } 

        public string UserName { get; set; }

        public string Password { get; set; }

        public string UserRole { get; set; }

    }
}
