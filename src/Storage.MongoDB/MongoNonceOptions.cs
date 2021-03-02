
using Bewit.Core;

namespace Bewit.Storage.MongoDB
{
    public class MongoNonceOptions : NonceOptions
    {
        /// <summary>
        /// Connection string for the MongoDB instance.
        /// Mandatory.
        /// </summary>
        public string ConnectionString { get; set; } //mandatory

        /// <summary>
        /// Name of the Database in the MongoDB instance. 
        /// Mandatory.
        /// </summary>
        public string DatabaseName { get; set; } //mandatory

        /// <summary>
        /// Name of the Bewit Collection in the Mongo database. 
        /// Optional. 
        /// </summary>
        public string CollectionName { get; set; } = nameof(Token);
    }
}
