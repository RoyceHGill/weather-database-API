namespace MongoWeatherAPI.Models.Operations
{
    /// <summary>
    /// Object to catch a response from the database.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperationResult<T>
    {
        /// <summary>
        /// The message from the database
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Was it successful true or false.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The objects the query will return.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// The number of entities effected by the query.
        /// </summary>
        public int RecordsAffected { get; set; }
    }
}
