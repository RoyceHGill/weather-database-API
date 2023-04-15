using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoWeatherAPI.Settings;
using ZstdSharp.Unsafe;

namespace MongoWeatherAPI.Services
{
    public class MongoConnection
    {
        private readonly IOptions<MongoDbConnectionSettings> _options;

        public MongoConnection(IOptions<MongoDbConnectionSettings> options)
        {
            _options = options;
        }



        /// <summary>
        /// Gets the Database object from the Database
        /// </summary>
        /// <returns></returns>
        public IMongoDatabase GetDatabase() 
        {
            var client = new MongoClient(_options.Value.ConnectionString);
            return client.GetDatabase(_options.Value.DatabaseName);

        }

        /// <summary>
        /// Gets the Database object from the Database
        /// </summary>
        /// <param name="database">The string name of the database that can be entered to specify which database to connect to</param>
        /// <returns></returns>
        public IMongoDatabase GetDatabase(string database)
        {
            var client = new MongoClient(_options.Value.ConnectionString);
            return client.GetDatabase(database);
        }

        /// <summary>
        /// Gets the Database object from the Database
        /// </summary>
        /// <param name="connectionString">The string name of the connection string that can be entered to specify how to connect to the database</param>
        /// <param name="database">The string name of the database that can be entered to specify which database to connect to</param>
        /// <returns></returns>
        public IMongoDatabase GetDatabase(string connectionString, string database)
        {
            var client = new MongoClient(connectionString);
            return client.GetDatabase(database);
        }
    }
}
