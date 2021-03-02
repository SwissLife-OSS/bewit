using System;
using System.Collections.Generic;

namespace Bewit.Core
{
    public class BewitRegistrationBuilder
    {
        private Func<BewitOptions, ICryptographyService> _getCryptographyService;
        private readonly List<BewitPayloadBuilder> _payloadBuilders =
            new List<BewitPayloadBuilder>();

        public Func<BewitOptions, ICryptographyService> GetCryptographyService
        {
            get
            {
                if(_getCryptographyService == default)
                {
                    _getCryptographyService = (BewitOptions options) 
                        => new HmacSha256CryptographyService(options.Secret);
                }

                return _getCryptographyService;
            }
            set => _getCryptographyService = value;
        }

        internal IReadOnlyList<BewitPayloadBuilder> PayloadBuilders => _payloadBuilders;

        public BewitPayloadBuilder AddPayload<T>()
        {
            BewitPayloadBuilder payloadBuilder = new BewitPayloadBuilder().AddPayload<T>();
            _payloadBuilders.Add(payloadBuilder);

            return payloadBuilder;
        }
    }
}
