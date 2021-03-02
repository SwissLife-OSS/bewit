using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Core
{
    public class BewitPayload
    {
        public BewitPayload(IServiceCollection services, Type payloadType)
        {
            Services = services;
            Type = payloadType;
        }

        public IServiceCollection Services { get; }
        internal Type Type { get; }
    }
}
