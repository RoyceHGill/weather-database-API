using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using MongoDB.Driver;
using MongoWeatherAPI.Attributes;
using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Filters;
using MongoWeatherAPI.Models.Operations;
using MongoWeatherAPI.Repository.Interfaces;
using SharpCompress.Common;
using System.Text.RegularExpressions;

namespace MongoWeatherAPI.Controllers
{

    /// <summary>
    /// The Weather Data Route of the API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherDataController : ControllerBase
    {
        private readonly IWeatherDataRepository _weatherDataRepository;
        private readonly IApiUserRepository _userRepository;

        /// <summary>
        /// Constructor. Inject weather data methods repository.
        /// </summary>
        /// <param name="weatherDataRepository">The Database Repository Context</param
        public WeatherDataController(IWeatherDataRepository weatherDataRepository, IApiUserRepository apiUserRepository)
        {
            _weatherDataRepository = weatherDataRepository;
            _userRepository = apiUserRepository;
        }

        /// <summary>
        /// Create a query against all data.
        /// </summary>
        /// <param name="filter"> contains filter parameters for a query. </param>
        /// <returns> Collection of Weather data. </returns>
        /// <remarks>
        /// Using a start and end DateTime and or Device Name get all entities that match the filter.
        /// 
        /// Body Parameters:
        /// 
        /// Device Name: string
        /// 
        /// Time Range: from - to
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Student
        /// </remarks>
        [HttpGet]
        public IEnumerable<WeatherData> Get([FromQuery] WeatherDataNameTimeFilter filter)
        {
            HttpContext.Response.Headers.Add("x-customer-data-header", DateTime.Now.ToString());
            var weatherData = _weatherDataRepository.GetWeatherData(filter);
            return weatherData;
        }

        // Get api/<WeatherDataController>/640d692fd3341cc30e082455
        /// <summary>
        /// Get the Weather data by Id.
        /// </summary>
        /// <param name="id"> Database ID </param>
        /// <returns> Single Weather reading. </returns>
        /// <remarks>
        /// Using a Database ID get one Weather reading.
        ///        
        /// Body Parameters:
        /// 
        /// ID: String
        /// 
        /// Authorization:
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Student
        /// </remarks>
        [HttpGet("{id}")]
        [ApiKey(role: "Student")]
        public WeatherData Get(string id, [FromHeader] string apiKey)
        {
            HttpContext.Response.Headers.Add("x-customer-data-header", DateTime.Now.ToString());
            var weatherData = _weatherDataRepository.GetWeatherData(id);
            _userRepository.UpdateUserLastLogin(apiKey);
            return weatherData;
        }

        // POST api/<WeatherDataController>
        /// <summary>
        /// Create many Weather readings at once. 
        /// </summary>
        /// <param name="weatherData"> A Json body of weather reading DTO objects.  </param>
        /// <returns> Status code 201 if successful, 500 if unsuccessful. </returns>
        /// <remarks>
        /// Insert Many Weather readings together. 
        ///        
        /// Body Parameters:
        /// 
        /// List of: 
        /// 
        /// DeviceName: String
        /// 
        /// Precipitation: double
        /// 
        /// Time : DateTime 
        /// 
        /// Latitude: double
        /// 
        /// Longitude: double
        ///  
        /// TemperatureC: double
        /// 
        /// AtmosphericPressureKPA: double
        /// 
        /// MaxWindSpeedMS: double
        /// 
        /// SolarRadiationWM2: double
        /// 
        /// VaporPressureKPA: double
        /// 
        /// HumidityPercetage: double
        /// 
        /// WindDirection: double
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [ApiKey(role: "Student")]
        [HttpPost("CreateMany")]
        public ActionResult PostMany([FromBody] List<WeatherDataCreateDTO> weatherData, [FromHeader] string apiKey)
        {
            try
            {
                _weatherDataRepository.CreateManyWeatherReadings(weatherData);
                _userRepository.UpdateUserLastLogin(apiKey);

                return CreatedAtAction("PostMany", null);
            }
            catch (Exception e)
            {

                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        // POST api/<WeatherDataController>
        /// <summary>
        /// Create a new Reading Entry.
        /// </summary>
        /// <param name="newdata"> Data for creating a new weather reading. </param>
        /// <returns>Status code 200 if successful, 500 if failed.</returns>
        /// <remarks>
        /// Insert One Weather reading. 
        ///        
        /// Body Parameters:
        /// 
        /// DeviceName: String
        /// 
        /// Precipitation: double
        /// 
        /// Time : DateTime 
        /// 
        /// Latitude: double
        /// 
        /// Longitude: double
        ///  
        /// TemperatureC: double
        /// 
        /// AtmosphericPressureKPA: double
        /// 
        /// MaxWindSpeedMS: double
        /// 
        /// SolarRadiationWM2: double
        /// 
        /// VaporPressureKPA: double
        /// 
        /// HumidityPercetage: double
        /// 
        /// WindDirection: double
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPost]
        [ApiKey(role: "Teacher")]
        public ActionResult Post([FromBody] WeatherDataCreateDTO newdata, [FromHeader] string apiKey)
        {
            try
            {
                _weatherDataRepository.CreateWeatherData(newdata);
                _userRepository.UpdateUserLastLogin(apiKey);

                return CreatedAtAction("Post", null);
            }
            catch (Exception e)
            {

                return Problem(detail: e.Message, statusCode: 500);
            }

        }

        // PUT api/<WeatherDataController>/640d692fd3341cc30e082455
        /// <summary>
        /// Replace some or all data in a weather reading.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns>Status code 200, if successful, 500 if unsuccessful.</returns>
        /// <remarks>
        /// Update one Weather readings together. 
        /// 
        /// Path Parameters:
        /// 
        /// ID: string
        /// 
        /// Body Parameters:
        /// 
        /// DeviceName: String
        /// 
        /// Precipitation: double
        /// 
        /// Time : DateTime 
        /// 
        /// Latitude: double
        /// 
        /// Longitude: double
        ///  
        /// TemperatureC: double
        /// 
        /// AtmosphericPressureKPA: double
        /// 
        /// MaxWindSpeedMS: double
        /// 
        /// SolarRadiationWM2: double
        /// 
        /// VaporPressureKPA: double
        /// 
        /// HumidityPercetage: double
        /// 
        /// WindDirection: double
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPut("{id}")]
        [ApiKey(role: "Teacher")]
        public ActionResult Put(string id, [FromBody] WeatherData data, [FromHeader] string apiKey)
        {
            try
            {
                var result = _weatherDataRepository.UpdateWeatherData(id, data);
                _userRepository.UpdateUserLastLogin(apiKey);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        // Patch api/<WeatherDataController>/640d692fd3341cc30e082455/Precipitation/0.75
        /// <summary>
        /// Update the precipitation value of the specified Reading.
        /// </summary>
        /// <param name="id">Database ID</param>
        /// <param name="value">new Precipitation value</param>
        /// <returns>status code if 200 if successful, 500 if unsuccessful. </returns>
        /// <remarks>
        /// Patch one Weather reading. 
        /// 
        /// Path Parameters:
        /// 
        /// ID : string
        /// 
        /// Body Parameters:
        /// 
        /// Value: string
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPatch("Precipitation/{id}")]
        [ApiKey(role: "Teacher")]
        public ActionResult PatchPrecipitation(string id, [FromBody] string value, [FromHeader] string apiKey)
        {
            try
            {
                _weatherDataRepository.UpdateWeatherDataPrecipitation(id, value);
                _userRepository.UpdateUserLastLogin(apiKey);

                return Ok("Successfully patched the Precipitation value.");
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// Patch the specified Property with the specified Value.
        /// </summary>
        /// <param name="patchRequest">Property and value holder</param>
        /// <returns>status code if 200 if successful, 500 if unsuccessful.</returns>
        /// <remarks>
        /// Patch one Weather reading. 
        ///        
        /// Body Parameters:
        /// 
        /// Property
        /// 
        /// DeviceName: String
        /// 
        /// Precipitation: double
        /// 
        /// Time : DateTime 
        /// 
        /// Latitude: double
        /// 
        /// Longitude: double
        ///  
        /// TemperatureC: double
        /// 
        /// AtmosphericPressureKPA: double
        /// 
        /// MaxWindSpeedMS: double
        /// 
        /// SolarRadiationWM2: double
        /// 
        /// VaporPressureKPA: double
        /// 
        /// HumidityPercetage: double
        /// 
        /// WindDirection: double
        /// 
        /// Value: string
        /// 
        /// Property is used to identify the field you want to patch and the value if entered as a string,
        /// 
        /// is convented to the DataType required
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpPatch]
        [ApiKey(role: "Teacher")]
        public ActionResult PatchMany([FromBody] WeatherDataPatchRequestObject patchRequest, [FromHeader] string apiKey)
        {
            OperationResult<WeatherData> result;
            try
            {
                switch (patchRequest.PropertyName)
                {
                    case "deviceName":
                        result = _weatherDataRepository.UpdateManyDeviceName(patchRequest);
                        break;
                    case "precipitationMMH":
                        result = _weatherDataRepository.UpdateManyPrecipitation(patchRequest);
                        break;
                    case "time":
                        result = _weatherDataRepository.UpdateManyTime(patchRequest);
                        break;
                    case "latitude":
                        result = _weatherDataRepository.UpdateManyLatitude(patchRequest);
                        break;
                    case "longitude":
                        result = _weatherDataRepository.UpdateManyLongitude(patchRequest);
                        break;
                    case "temperatureC":
                        result = _weatherDataRepository.UpdateManyTemperature(patchRequest);
                        break;
                    case "atmosphericPressureKPA":
                        result = _weatherDataRepository.UpdateManyAtmospherePressure(patchRequest);
                        break;
                    case "maxWindSpeedMS":
                        result = _weatherDataRepository.UpdateManyMaxWindSpeed(patchRequest);
                        break;
                    case "solarRadiationWM2":
                        result = _weatherDataRepository.UpdateManySolarRadiation(patchRequest);
                        break;
                    case "vaporPressureKPA":
                        result = _weatherDataRepository.UpdateManyVaporPressure(patchRequest);
                        break;
                    case "humidityPercetage":
                        result = _weatherDataRepository.UpdateManyHumidityPercent(patchRequest);
                        break;
                    case "windDirection":
                        result = _weatherDataRepository.UpdateManyWindDirection(patchRequest);
                        break;
                    default:
                        return BadRequest("No Properties Matched");
                }
            }
            catch (Exception e)
            {

                return Problem(detail: e.Message, statusCode: 500);
            }
            _userRepository.UpdateUserLastLogin(apiKey);

            return Ok($"Successfully patched {patchRequest.PropertyName} for {result.RecordsAffected}");
        }

        // DELETE api/<WeatherDataController>/640d692fd3341cc30e082455
        /// <summary>
        /// Deletes a Specified Reading.
        /// </summary>
        /// <param name="id">Database ID</param>
        /// <returns>status code 200 if successful, 500 if unsuccessful. </returns>
        /// <remarks>
        /// Using a database ID, search and destroy the entity from the database.
        /// 
        /// Path Parameters: 
        /// 
        /// ID: string
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Teacher
        /// </remarks>
        [HttpDelete("Delete/{id}")]
        [ApiKey(role: "Teacher")]
        public ActionResult Delete(string id, [FromHeader] string apiKey)
        {
            try
            {
                var result = _weatherDataRepository.DeleteWeatherData(id);
                _userRepository.UpdateUserLastLogin(apiKey);

                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception e)
            {
                return Problem(detail: e.Message, statusCode: 500);
            }
        }

        /// <summary>
        /// Get the maximum Temperature Reading for each station in a time frame. 
        /// </summary>
        /// <param name="from">Start of time frame</param>
        /// <param name="to">End of time frame</param>
        /// <param name="apiKey">API key</param>
        /// <returns> </returns>
        /// <remarks>
        /// Get a List of readings that is as long as the number of stations,
        /// 
        /// with the highest recorded temperature for that station,
        /// 
        /// and the time of the reading.
        /// 
        /// Path Parameters: 
        /// 
        /// from: DateTime
        /// 
        /// to: DateTime
        /// 
        /// APIKey: String (Header)
        /// 
        /// Role: >= Student
        /// </remarks>
        [HttpGet("MaxTempForStation")]
        [ApiKey(role: "Student")]
        public List<DeviceNameTimeTempuratureDTO> GetMaxTempForStaions([FromQuery] DateTime from, DateTime to, [FromHeader] string apiKey)
        {
            _userRepository.UpdateUserLastLogin(apiKey);

            return _weatherDataRepository.GetMaxTempReadingForEachStation(from, to);
        }

        /// <summary>
        /// Get A set of Environmental Readings for the specified hour. 
        /// </summary>
        /// <param name="hour">Provide a date time for the hour to be specified. </param>
        /// <param name="apiKey">API Key</param>
        /// <returns>List of Weather reading with fewer properties.</returns>
        /// <remarks>
        /// Get a List of readings with:
        /// 
        /// DeviceName
        /// 
        /// TemperatureC
        /// 
        /// AtmosphericPressure
        /// 
        /// Radiation
        /// 
        /// Precipitation
        /// 
        /// Time
        /// 
        /// for the hour the dateTime lands in.  
        /// 
        /// Path Parameters: 
        /// 
        /// hour: DateTime
        ///
        /// APIKey: String (Header)
        /// 
        /// Role: >= Student
        /// </remarks>
        [HttpGet("EnvironmentForhour")]
        [ApiKey(role: "Student")]
        public List<EnvironmentalReadingDTO> GetEnvironmentForhour([FromQuery] DateTime hour, [FromHeader] string apiKey)
        {
            _userRepository.UpdateUserLastLogin(apiKey);

            return _weatherDataRepository.GetEnvironmentDetailsForHour(hour);
        }


        /// <summary>
        /// Get the Highest Precipitation value for a station in the past 5 months.
        /// </summary>
        /// <param name="deviceName">Device name</param>
        /// <param name="apiKey">API Key</param>
        /// <returns>Device name, Time, Precipitation</returns>
        /// <remarks>
        /// 
        /// Get a single reading with:
        /// 
        /// DeviceName
        /// 
        /// Precipitation
        /// 
        /// Time
        /// 
        /// The highest Precipitation value for a station and the reading date for the value.
        /// 
        /// Path Parameters: 
        /// 
        /// Device Name: string 
        ///
        /// APIKey: String (Header)
        /// 
        /// Role: >= Student
        /// </remarks>
        [HttpGet("MaxPrecipitation5MT")]
        [ApiKey(role: "Student")]
        public DeviceNameTimePrecipitationDTO GetMaxPrecipitation5MT([FromQuery] string deviceName, [FromHeader] string apiKey)
        {
            _userRepository.UpdateUserLastLogin(apiKey);

            return _weatherDataRepository.GetMaxPrecipitation5MT(deviceName);
        }
    }
}
