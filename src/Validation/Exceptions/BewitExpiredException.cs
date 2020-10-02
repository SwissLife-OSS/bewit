using System;
using System.Runtime.Serialization;

namespace Bewit.Validation.Exceptions
{
    [Serializable]
    public sealed class BewitExpiredException : BewitException
    {
        public BewitExpiredException()
            : base("The given Bewit has expired.")
        {
        }

        private BewitExpiredException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
