using System;

namespace Bewit;

public interface ICryptographyService
{
    string GetHash<T>(string token, DateTime expirationDate, T payload)
        where T : notnull;
}
