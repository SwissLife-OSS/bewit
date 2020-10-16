using System;

namespace Bewit.Core
{
    public class BewitRegistrationBuilder
    {
        private Func<BewitOptions, ICryptographyService> _getCryptographyService;

        private Func<INonceRepository> _getRepository { get; set; }

        public BewitRegistrationBuilder SetRepository(Func<INonceRepository> create)
        {
            _getRepository = create;
            return this;
        }

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
    }
}
