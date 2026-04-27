
#nullable enable
namespace Bewit.Storage.MongoDB
{
    public class MongoNonceOptions
    {
        /// <summary>
        /// Connection string for the MongoDB instance.
        /// Required only when using connection-string based UseMongoPersistence overloads.
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Name of the Database in the MongoDB instance.
        /// Mandatory for all UseMongoPersistence overloads.
        /// </summary>
        public string DatabaseName { get; set; } = null!;
        /// <summary>
        /// Name of the Bewit Collection in the Mongo database.
        /// Optional.
        /// </summary>
        public string CollectionName { get; set; } = nameof(Token);

        /// <summary>
        /// Usage strategy for nonce. Default is OneTime which will remove the nonce from storage.
        /// ReUse will keep it in the storage.
        /// </summary>
        public NonceUsage NonceUsage { get; set; } = NonceUsage.OneTime;

        public int RecordExpireAfterDays { get; set; } = 365 * 2;
    }
}
