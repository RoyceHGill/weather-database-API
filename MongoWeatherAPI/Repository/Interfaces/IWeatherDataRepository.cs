using MongoWeatherAPI.Models;
using MongoWeatherAPI.Models.DTO;
using MongoWeatherAPI.Models.Filters;
using MongoWeatherAPI.Models.Operations;

namespace MongoWeatherAPI.Repository.Interfaces
{
    public interface IWeatherDataRepository
    {
        public IEnumerable<WeatherData> GetWeatherData(WeatherDataNameTimeFilter filter);

        public WeatherData GetWeatherData(string id);

        public void CreateWeatherData(WeatherDataCreateDTO newData);

        public void CreateManyWeatherReadings(List<WeatherDataCreateDTO> newReadins);

        public OperationResult<WeatherData> UpdateWeatherData(string id, WeatherData updatedData);

        public OperationResult<WeatherData> DeleteWeatherData(string id);

        public OperationResult<WeatherData> UpdateWeatherDataPrecipitation(string id, string value);

        public OperationResult<WeatherData> UpdateManyDeviceName(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyPrecipitation(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyTime(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyLatitude(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyLongitude(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyTemperature(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyAtmospherePressure(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyMaxWindSpeed(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManySolarRadiation(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyVaporPressure(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyHumidityPercent(WeatherDataPatchRequestObject patchRequest);

        public OperationResult<WeatherData> UpdateManyWindDirection(WeatherDataPatchRequestObject patchRequest);

        public DeviceNameTimePrecipitationDTO GetMaxPrecipitation5MT(string deviceName);

        public List<EnvironmentalReadingDTO> GetEnvironmentDetailsForHour(DateTime hour);

        public List<DeviceNameTimeTempuratureDTO> GetMaxTempReadingForEachStation(DateTime from, DateTime to);
    }
}
