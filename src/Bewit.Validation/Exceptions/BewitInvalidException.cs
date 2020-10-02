using System;
using System.Runtime.Serialization;

namespace Bewit.Validation.Exceptions
{
    [Serializable]
    public sealed class BewitInvalidException : BewitException
    {
        public BewitInvalidException()
            : base("The given Bewit is invalid.")
        {
        }

        private BewitInvalidException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
