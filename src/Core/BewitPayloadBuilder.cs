using System;

namespace Bewit.Core
{
    public class BewitPayloadBuilder
    {
        private Type _payload;
        internal Func<INonceRepository> CreateRepository { get; set; }

        public BewitPayloadBuilder SetRepository(Func<INonceRepository> create)
        {
            CreateRepository = create;
            return this;
        }

        public BewitPayloadBuilder AddPayload<T>()
        {
            _payload = typeof(T);
            return this;
        }
    }
}
