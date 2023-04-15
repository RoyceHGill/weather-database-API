using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoWeatherAPI.Models
{
    /// <summary>
    /// User for the Api
    /// </summary>
    public class ApiUser
    {
        /// <summary>
        /// MongoDB ObjectId for document
        /// </summary>
        [JsonIgnore]
        public ObjectId _id { get; set; }

        public string ObjId => _id.ToString();
        /// <summary>
        /// "User Name" Key for User Collection of the MongoDb Database
        /// </summary>
        [BsonElement("User Name")]
        public string? UserName { get; set; }

        /// <summary>
        /// "Password Hash" Key for User Collection of the MongoDb Database
        /// </summary>
        [BsonElement("Password Hash")]
        public string PasswordHash { get; set; }

        /// <summary>
        /// "User Role" Key for User Collection of the MongoDb Database
        /// </summary>
        [BsonElement("User Role")]
        public string UserRole { get; set; } = "Student";

        /// <summary>
        /// "Last Login" Key for User Collection of the MongoDb Database
        /// Should be updated every successful login.
        /// </summary>
        [BsonElement("Last Login")]
        [JsonIgnore]
        public DateTime LastLogin { get; set; }

        /// <summary>
        /// "Created" Key for User Collection of the MongoDb Database,
        /// Marks when the User was created. Should not be modified. 
        /// </summary>
        [JsonIgnore]
        public DateTime Created { get; set; }

        
        public string? ApiKey { get; set; }

        [JsonIgnore]
        public DateTime? Expiry { get; set; }
    }
}
