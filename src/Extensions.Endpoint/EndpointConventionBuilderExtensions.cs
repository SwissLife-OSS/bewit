using System;
using Microsoft.AspNetCore.Builder;

namespace Bewit.Endpoint
{
    public static class EndpointConventionBuilderExtensions
    {
        public static TBuilder RequireBewitUrlAuthorization<TBuilder>(
            this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Add(endpointBuilder =>
            {
                endpointBuilder.Metadata.Add(new BewitEndpointAttribute());
            });

            return builder;
        }
    }
}
