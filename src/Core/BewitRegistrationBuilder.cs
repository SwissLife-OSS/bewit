using System.Collections.Generic;

namespace Bewit
{
    public class BewitRegistrationBuilder
    {
        private readonly List<BewitPayloadContext> _payloads = new List<BewitPayloadContext>();

        internal IReadOnlyList<BewitPayloadContext> Payloads => _payloads;

        public BewitPayloadContext AddPayload<T>()
        {
            var payload = new BewitPayloadContext(typeof(T));
            _payloads.Add(payload);

            return payload;
        }
    }
}
