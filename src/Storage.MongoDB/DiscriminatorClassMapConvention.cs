using System;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Bewit.Storage.MongoDB
{
    internal class DiscriminatorClassMapConvention : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            var discriminator = GetDiscriminatorName(classMap.ClassType);
            classMap.SetDiscriminator(discriminator);
        }

        private string GetDiscriminatorName(Type type)
        {
            var name = $"{type.Namespace}.{type.Name}";
            if (type.IsGenericType)
            {
                var builder = new StringBuilder();
                builder.Append($"{name}[[");

                for (var i = 0; i < type.GenericTypeArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append("],[");
                    }

                    Type typeArgument = type.GenericTypeArguments[i];
                    builder.Append(GetDiscriminatorName(typeArgument));
                }

                builder.Append("]]");
                name = builder.ToString();
            }

            return name;
        }
    }
}
