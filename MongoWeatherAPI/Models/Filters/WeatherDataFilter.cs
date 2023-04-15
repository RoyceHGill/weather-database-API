using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoWeatherAPI.Models.Filters
{
    public class WeatherDataFilter
    {
        public string? ObjId { get; set; }

        public string? DeviceNamePartial { get; set; }

        public double? PrecipitatonMMHMin { get; set; }
        public double? PrecipitatonMMHMax { get; set; }

        public DateTime? TimeFrom { get; set; }
        public DateTime? TimeTo { get; set; }

        public double? LatitudeMin { get; set; }
        public double? LatitudeMax { get; set; }

        public double? LongitudeMin { get; set; }
        public double? LongitudeMax { get; set; }

        public double? TemperatureCMin { get; set; }
        public double? TemperatureCMax { get; set; }

        public double? AtmosphericPressureKPAMin { get; set; }
        public double? AtmosphericPressureKPAMax { get; set; }

        public double? MaxWindSpeedMSMin { get; set; }
        public double? MaxWindSpeedMSMax { get; set; }

        public double? SolarRadiationWM2Min { get; set; }
        public double? SolarRadiationWM2Max { get; set; }

        public double? VaporPressureKPAMin { get; set; }
        public double? VaporPressureKPAMax { get; set; }

        public double? HumidityPercetageMin { get; set; }
        public double? HumidityPercetageMax { get; set; }

        public double? WindDirectionMin { get; set; }
        public double? WindDirectionMax { get; set; }
    }

}
