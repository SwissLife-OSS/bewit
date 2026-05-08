using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Bewit.Storage.MongoDB;

public sealed class BewitMongoOptions
{
    [Required]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    public string DatabaseName { get; set; } = string.Empty;

    public string CollectionName { get; set; } = "bewit_nonces";

    public MongoAuthType AuthType { get; set; } = MongoAuthType.Password;

    public List<string>? OidcScopes { get; set; }

    public NonceUsage NonceUsage { get; set; } = NonceUsage.OneTime;

    public int RecordExpireAfterDays { get; set; } = 730;
}

[OptionsValidator]
public partial class BewitMongoOptionsValidator : IValidateOptions<BewitMongoOptions>;
