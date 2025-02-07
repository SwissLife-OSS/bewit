using System;
using System.Collections.Generic;
using MongoDB.Bson;

#nullable enable

namespace Bewit.Storage.MongoDB
{
    public static class TypeChecker
    {
        private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly),
            typeof(Guid),
            typeof(ObjectId),
            typeof(BsonObjectId),
            typeof(BsonDateTime)
        };

        public static bool IsPrimitiveType(object value)
        {
            if (value == null)
                return false;

            Type type = value.GetType();
            return IsPrimitiveType(type);
        }

        public static bool IsPrimitiveType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            return PrimitiveTypes.Contains(underlyingType) ||
                   underlyingType.IsEnum;
        }
    }
}
