using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Core
{
    public class BewitRegistrationBuilder
    {
        private readonly List<BewitPayload> _payloads = new List<BewitPayload>();

        public BewitRegistrationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
        internal IReadOnlyList<BewitPayload> Payloads => _payloads;

        public BewitPayload AddPayload<T>()
        {
            var payload = new BewitPayload(Services, typeof(T));
            _payloads.Add(payload);

            return payload;
        }
    }
}
