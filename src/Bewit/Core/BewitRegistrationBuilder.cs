using System;

namespace Bewit.Core
{
    public class BewitRegistrationBuilder
    {
        private Func<BewitOptions, ICryptographyService> _getCryptographyService;

        public Func<INonceRepository> GetRepository { get; set; }

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
