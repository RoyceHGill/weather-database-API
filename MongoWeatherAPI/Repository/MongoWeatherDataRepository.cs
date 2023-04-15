using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Filters;
using MongoWeatherAPI.Models.Operations;
using MongoWeatherAPI.Repository.Interfaces;
using MongoWeatherAPI.Services;
using System.Text.RegularExpressions;

namespace MongoWeatherAPI.Repository
{
    public class MongoWeatherDataRepository : IWeatherDataRepository
    {
        private readonly IMongoCollection<WeatherData> _weatherData;

        public MongoWeatherDataRepository(MongoConnection connection)
        {
            _weatherData = connection.GetDatabase().GetCollection<WeatherData>("WeatherCollection");
        }
        /// <summary>
        /// Creates a new Weather data document out of a Weather data Transfer object and insert it into the database
        /// </summary>
        /// <param name="newData">The Weather data DTO</param>
        public void CreateWeatherData([FromBody] WeatherDataCreateDTO newData)
        {
            _weatherData.InsertOne(new WeatherData
            {
                DeviceName = newData.DeviceName,
                PrecipitationMMH = newData.PrecipitatonMMH,
                Time = newData.Time,
                Latitude = newData.Latitude,
                Longitude = newData.Longitude,
                TemperatureC = newData.TemperatureC,
                AtmosphericPressureKPA = newData.AtmosphericPressureKPA,
                MaxWindSpeedMS = newData.MaxWindSpeedMS,
                SolarRadiationWM2 = newData.SolarRadiationWM2,
                VaporPressureKPA = newData.VaporPressureKPA,
                HumidityPercetage = newData.HumidityPercetage,
                WindDirection = newData.WindDirection,
            });
        }

        /// <summary>
        /// Convert a list of Reading Data transfer Objects into a list of WeatherData Objects,
        /// insert them into the database. 
        /// </summary>
        /// <param name="newWeatherDataDTOList"></param>
        public void CreateManyWeatherReadings(List<WeatherDataCreateDTO> newWeatherDataDTOList)
        {
            var newReadings = newWeatherDataDTOList.Select(weatherDataDTO => new WeatherData
            {
                DeviceName = weatherDataDTO.DeviceName,
                PrecipitationMMH = weatherDataDTO.PrecipitatonMMH,
                Time = weatherDataDTO.Time,
                Latitude = weatherDataDTO.Latitude,
                Longitude = weatherDataDTO.Longitude,
                TemperatureC = weatherDataDTO.TemperatureC,
                AtmosphericPressureKPA = weatherDataDTO.AtmosphericPressureKPA,
                MaxWindSpeedMS = weatherDataDTO.MaxWindSpeedMS,
                SolarRadiationWM2 = weatherDataDTO.SolarRadiationWM2,
                VaporPressureKPA = weatherDataDTO.VaporPressureKPA,
                HumidityPercetage = weatherDataDTO.HumidityPercetage,
                WindDirection = weatherDataDTO.WindDirection,
            });

            _weatherData.InsertMany(newReadings);
        }

        /// <summary>
        /// Applies the filter to the DongoDB Query and sends that query to the database. 
        /// </summary>
        /// <param name="weatherDataFilter"></param>
        /// <returns></returns>
        public IEnumerable<WeatherData> GetWeatherData([FromQuery] WeatherDataNameTimeFilter weatherDataFilter)
        {
            var filter = ProcessNameTimeFilter(weatherDataFilter);

            var weatherDataResults = _weatherData.Find(filter).ToEnumerable();
            return weatherDataResults;
        }

        /// <summary>
        /// Deletes a weather data record from the database.
        /// </summary>
        /// <param name="id">The string interpretation of the Object Id</param>
        public OperationResult<WeatherData> DeleteWeatherData(string id)
        {
            ObjectId objId = ObjectId.Parse(id);
            var filter = Builders<WeatherData>.Filter.Eq(c => c._id, objId);
            var result = _weatherData.DeleteOne(filter);

            if (result.DeletedCount > 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather Record Successfully Deleted",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.DeletedCount)
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather Record Not Deleted",
                    Success = false,
                    RecordsAffected = 0
                };
            }

        }

        /// <summary>
        /// Retrieves one document from the database by Id. 
        /// </summary>
        /// <param name="id">The string interpretation of the Object Id</param>
        /// <returns>A single Weather Data Object</returns>
        public WeatherData GetWeatherData(string id)
        {
            ObjectId objId = ObjectId.Parse(id);
            var filter = Builders<WeatherData>.Filter.Eq(c => c._id, objId);
            return _weatherData.Find(filter).FirstOrDefault();
        }

        /// <summary>
        /// Update Weather data from the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedData"></param>
        public OperationResult<WeatherData> UpdateWeatherData(string id, WeatherData updatedData)
        {
            ObjectId objId = ObjectId.Parse(id);
            var filter = Builders<WeatherData>.Filter.Eq(c => c._id, objId);
            updatedData._id = objId;

            var result = _weatherData.ReplaceOne(filter, updatedData);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather Record Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather Record Not Updated",
                    Success = false,
                    RecordsAffected = 0
                };
            }
        }

        /// <summary>
        /// Update the Precipitation Value of the selected document.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newPrecipitation"></param>
        public OperationResult<WeatherData> UpdateWeatherDataPrecipitation(string id, string value)
        {
            ObjectId objectId = ObjectId.Parse(id);
            var updateDefinition = Builders<WeatherData>.Update.Set(w => w.PrecipitationMMH, double.Parse(value));
            var result = _weatherData.UpdateOne(c => c._id.Equals(objectId), updateDefinition);

            if (result.ModifiedCount > 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather data Successfully Updated",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount)
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = "Weather Data Not Updated",
                    Success = false,
                    RecordsAffected = 0
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update ManyDevice names at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation </returns>
        public OperationResult<WeatherData> UpdateManyDeviceName(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.DeviceName, patchRequest.PropertyValue);

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Precipitation Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyPrecipitation(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.PrecipitationMMH, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Created Dates at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyTime(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.Time, DateTime.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Latitude Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyLatitude(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.Latitude, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Longitude Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyLongitude(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.Longitude, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Temperature Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyTemperature(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.TemperatureC, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Atmospheric Pressure Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyAtmospherePressure(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.AtmosphericPressureKPA, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Max Wind Speed Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyMaxWindSpeed(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.MaxWindSpeedMS, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Solar Radiation Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManySolarRadiation(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.SolarRadiationWM2, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Vapor pressure Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyVaporPressure(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.VaporPressureKPA, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Humidity Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyHumidityPercent(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.HumidityPercetage, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Method called by PatchMany in switch statement in Controller to update Many Wind Direction Values at once. 
        /// </summary>
        /// <param name="patchRequest">Contains the Property to be changed the new value and the filter parameters</param>
        /// <returns>Success confirmation</returns>
        public OperationResult<WeatherData> UpdateManyWindDirection(WeatherDataPatchRequestObject patchRequest)
        {
            var filter = ProcessFilter(patchRequest.Filter);

            var updateDefinition = Builders<WeatherData>.Update.Set(d => d.WindDirection, double.Parse(patchRequest.PropertyValue));

            var result = _weatherData.UpdateMany(filter, updateDefinition);

            if (result.ModifiedCount < 0)
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"{patchRequest.PropertyName} Successfully Updated for {result.ModifiedCount} readings.",
                    Success = true,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
            else
            {
                return new OperationResult<WeatherData>
                {
                    Message = $"No Readings were updated.",
                    Success = false,
                    RecordsAffected = Convert.ToInt32(result.ModifiedCount),
                };
            }
        }

        /// <summary>
        /// Process the a filter that will apply parameters to a patch query
        /// </summary>
        /// <param name="weatherDataFilter">Contains the search parameters</param>
        /// <returns>A filter definition to be used in the query</returns>
        private FilterDefinition<WeatherData> ProcessFilter(WeatherDataFilter weatherDataFilter)
        {
            var builder = Builders<WeatherData>.Filter;
            var filter = builder.Empty;

            if (!String.IsNullOrWhiteSpace(weatherDataFilter?.ObjId))
            {
                filter &= builder.Eq(w => w._id, ObjectId.Parse(weatherDataFilter.ObjId));
            }

            if (!String.IsNullOrEmpty(weatherDataFilter?.DeviceNamePartial))
            {
                var regexStatement = Regex.Escape(weatherDataFilter.DeviceNamePartial);
                filter &= builder.Regex(w => w.DeviceName, BsonRegularExpression.Create(regexStatement));
            }

            if (weatherDataFilter?.TimeFrom != null)
            {
                filter &= builder.Gte(t => t.Time, weatherDataFilter.TimeFrom.Value);
            }

            if (weatherDataFilter?.TimeTo != null)
            {
                filter &= builder.Lte(t => t.Time, weatherDataFilter.TimeTo.Value);
            }

            if (weatherDataFilter?.PrecipitatonMMHMin != null)
            {
                filter &= builder.Gte(p => p.PrecipitationMMH, weatherDataFilter.PrecipitatonMMHMin.Value);
            }

            if (weatherDataFilter?.PrecipitatonMMHMax != null)
            {
                filter &= builder.Lte(p => p.PrecipitationMMH, weatherDataFilter.PrecipitatonMMHMax.Value);
            }

            if (weatherDataFilter?.LatitudeMin != null)
            {
                filter &= builder.Gte(p => p.Latitude, weatherDataFilter.LatitudeMin.Value);
            }

            if (weatherDataFilter?.LatitudeMax != null)
            {
                filter &= builder.Lte(p => p.Latitude, weatherDataFilter.LatitudeMax.Value);
            }

            if (weatherDataFilter?.LongitudeMin != null)
            {
                filter &= builder.Gte(p => p.Longitude, weatherDataFilter.LongitudeMin.Value);
            }

            if (weatherDataFilter?.LongitudeMax != null)
            {
                filter &= builder.Lte(p => p.Longitude, weatherDataFilter.LongitudeMax.Value);
            }

            if (weatherDataFilter?.TemperatureCMin != null)
            {
                filter &= builder.Gte(p => p.TemperatureC, weatherDataFilter.TemperatureCMin.Value);
            }

            if (weatherDataFilter?.TemperatureCMax != null)
            {
                filter &= builder.Lte(p => p.TemperatureC, weatherDataFilter.TemperatureCMax.Value);
            }

            if (weatherDataFilter?.AtmosphericPressureKPAMin != null)
            {
                filter &= builder.Gte(p => p.AtmosphericPressureKPA, weatherDataFilter.AtmosphericPressureKPAMin.Value);
            }

            if (weatherDataFilter?.AtmosphericPressureKPAMax != null)
            {
                filter &= builder.Lte(p => p.AtmosphericPressureKPA, weatherDataFilter.AtmosphericPressureKPAMax.Value);
            }

            if (weatherDataFilter?.MaxWindSpeedMSMin != null)
            {
                filter &= builder.Gte(p => p.MaxWindSpeedMS, weatherDataFilter.MaxWindSpeedMSMin.Value);
            }

            if (weatherDataFilter?.MaxWindSpeedMSMax != null)
            {
                filter &= builder.Lte(p => p.MaxWindSpeedMS, weatherDataFilter.MaxWindSpeedMSMax.Value);
            }

            if (weatherDataFilter?.SolarRadiationWM2Min != null)
            {
                filter &= builder.Gte(p => p.SolarRadiationWM2, weatherDataFilter.SolarRadiationWM2Min.Value);
            }

            if (weatherDataFilter?.SolarRadiationWM2Max != null)
            {
                filter &= builder.Lte(p => p.SolarRadiationWM2, weatherDataFilter.SolarRadiationWM2Max.Value);
            }

            if (weatherDataFilter?.VaporPressureKPAMin != null)
            {
                filter &= builder.Gte(p => p.VaporPressureKPA, weatherDataFilter.VaporPressureKPAMin.Value);
            }

            if (weatherDataFilter?.VaporPressureKPAMax != null)
            {
                filter &= builder.Lte(p => p.VaporPressureKPA, weatherDataFilter.VaporPressureKPAMax.Value);
            }

            if (weatherDataFilter?.HumidityPercetageMin != null)
            {
                filter &= builder.Gte(p => p.HumidityPercetage, weatherDataFilter.HumidityPercetageMin.Value);
            }

            if (weatherDataFilter?.HumidityPercetageMax != null)
            {
                filter &= builder.Lte(p => p.HumidityPercetage, weatherDataFilter.HumidityPercetageMax.Value);
            }

            if (weatherDataFilter?.WindDirectionMin != null)
            {
                filter &= builder.Gte(p => p.WindDirection, weatherDataFilter.WindDirectionMin.Value);
            }

            if (weatherDataFilter?.WindDirectionMax != null)
            {
                filter &= builder.Lte(p => p.WindDirection, weatherDataFilter.WindDirectionMax.Value);
            }
            return filter;
        }

        /// <summary>
        /// Processes the Filter object into Filter Parameters to be used in a MongoDb Crud method. 
        /// </summary>
        /// <param name="weatherDataFilter"></param>
        /// <returns></returns>
        private FilterDefinition<WeatherData> ProcessNameTimeFilter(WeatherDataNameTimeFilter weatherDataFilter)
        {
            var builder = Builders<WeatherData>.Filter;
            var filter = builder.Empty;

            if (!String.IsNullOrEmpty(weatherDataFilter?.DeviceNamePartial))
            {
                var regexStatement = Regex.Escape(weatherDataFilter.DeviceNamePartial);
                filter &= builder.Regex(w => w.DeviceName, BsonRegularExpression.Create(regexStatement));
            }

            if (weatherDataFilter?.TimeFrom != null)
            {
                filter &= builder.Gte(t => t.Time, weatherDataFilter.TimeFrom.Value);
            }

            if (weatherDataFilter?.TimeTo != null)
            {
                filter &= builder.Lte(t => t.Time, weatherDataFilter.TimeTo.Value);
            }
            return filter;
        }

        /// <summary>
        /// Using a Device Name find the max precipitation value for that station in the past 5 months. 
        /// </summary>
        /// <param name="deviceNamePartial"> string, device name Partial</param>
        /// <returns>The device name , the time and the precipitation value of the reading with the highest precipitation value.</returns>
        public DeviceNameTimePrecipitationDTO GetMaxPrecipitation5MT(string deviceNamePartial)
        {
            DateTime to = DateTime.Now;
#if DEBUG
            DateTime from = to.AddMonths(-50);
#else
            DateTime from = to.AddMonths(-5);
#endif

            var result = _weatherData.AsQueryable()
                .Where(t => t.Time >= from
                && t.Time <= to
                && t.DeviceName.ToLower().Contains(deviceNamePartial.ToLower()))
                .Select(r => new DeviceNameTimePrecipitationDTO
                {
                    DeviceName = r.DeviceName,
                    PrecipitationMMH = r.PrecipitationMMH,
                    Time = r.Time

                }).OrderByDescending(p => p.PrecipitationMMH).Take(5)
                .First();

            return result;

        }

        /// <summary>
        /// Get a Collection of Environmental Readings for a the specified hour. 
        /// 
        /// A Date Time is provided, what ever hour that date time falls into is being and end of the time span.
        /// </summary>
        /// <param name="dateTime">The specified hour. </param>
        /// <returns>List of objects containing device name, Temp, Atmospheric Pressure,
        /// Precipitation, Radiation and the time the reading was made.</returns>
        public List<EnvironmentalReadingDTO> GetEnvironmentDetailsForHour(DateTime dateTime)
        {
            var result = _weatherData.AsQueryable()
                .OrderBy(t => t.Time)
                .Where(t => t.Time.Truncate(DateTimeUnit.Hour) == dateTime.Truncate(DateTimeUnit.Hour))
                .Select(r =>
                new EnvironmentalReadingDTO
                {
                    DeviceName = r.DeviceName,
                    TemperatureC = r.TemperatureC,
                    AtmosphericPressure = r.AtmosphericPressureKPA,
                    Precipitation = r.PrecipitationMMH,
                    Radiation = r.SolarRadiationWM2,
                    Time = r.Time
                })
                 .ToList();
            return result;
        }

        /// <summary>
        /// Find the highest temperature reading for a given time for each station.  
        /// </summary>
        /// <param name="from">Start time</param>
        /// <param name="to">End Time</param>
        /// <returns>Highest Temp and the Time the reading was made Device name for each station</returns>
        public List<DeviceNameTimeTempuratureDTO> GetMaxTempReadingForEachStation(DateTime from, DateTime to)
        {
            var result = _weatherData.AsQueryable()
                .Where(r => r.Time >= from && r.Time <= to)
                .GroupBy(n => n.DeviceName)
                .Select(r =>
                new DeviceNameTimeTempuratureDTO
                {
                    DeviceName = r.Key,
                    //Time = r.Where(a => a.DeviceName == r.Key && a.TemperatureC == r.Max(t => t.TemperatureC)).First().Time,
                    TemperatureC = r.Max(t => t.TemperatureC)
                }
                ).ToList();
            foreach (var t in result)
            {
                t.Time = _weatherData.AsQueryable()
                    .Where(a => a.DeviceName == t.DeviceName && a.TemperatureC == t.TemperatureC)
                    .Select(a => a.Time)
                    .First();
            }
            return result;
        }
    }
}

