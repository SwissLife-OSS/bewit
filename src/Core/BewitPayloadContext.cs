using System;

namespace Bewit
{
    public class BewitPayloadContext
    {
        public BewitPayloadContext(Type payloadType)
        {
            Type = payloadType;
        }

        internal Type Type { get; }

        internal Func<INonceRepository> CreateRepository { get; private set; }
        public BewitPayloadContext SetRepository(Func<INonceRepository> create)
        {
            CreateRepository = create;
            return this;
        }

        internal Func<ICryptographyService> CreateCryptographyService { get; private set; }
        public BewitPayloadContext SetCryptographyService(Func<ICryptographyService> create)
        {
            CreateCryptographyService = create;
            return this;
        }

        internal Func<IVariablesProvider> CreateVariablesProvider { get; private set; }
        public BewitPayloadContext SetVariablesProvider(Func<IVariablesProvider> create)
        {
            CreateVariablesProvider = create;
            return this;
        }
    }
}
