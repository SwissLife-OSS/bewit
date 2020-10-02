using System;

namespace Bewit.Core
{
    public interface ICryptographyService
    {
        string GetHash<T>(
            string token, DateTime expirationDate, T payload);
    }
}
