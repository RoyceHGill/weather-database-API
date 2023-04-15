using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace MongoWeatherAPI.Models
{
    public class WeatherData
    {
        [JsonIgnore]
        public ObjectId _id { get; set; }

        // Converts the object id to a string
        public string ObjId => _id.ToString();

        [BsonElement("Device Name")]
        public string? DeviceName { get; set; }

        [BsonElement("Precipitation mm/h")]
        public double? PrecipitationMMH { get; set; }

        public DateTime Time { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [BsonElement("Temperature (°C)")]
        public double? TemperatureC { get; set; }

        [BsonElement("Atmospheric Pressure (kPa)")]
        public double? AtmosphericPressureKPA { get; set; }

        [BsonElement("Max Wind Speed (m/s)")]
        public double? MaxWindSpeedMS { get; set; }

        [BsonElement("Solar Radiation (W/m2)")]
        public double? SolarRadiationWM2 { get; set; }

        [BsonElement("Vapor Pressure (kPa)")]
        public double? VaporPressureKPA { get; set; }

        [BsonElement("Humidity (%)")]
        public double? HumidityPercetage { get; set; }

        [BsonElement("Wind Direction (°)")]
        public double? WindDirection { get; set; }

        public string? Location { get; set; }
    }
}
