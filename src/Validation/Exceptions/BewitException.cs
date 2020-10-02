using System;
using System.Runtime.Serialization;

namespace Bewit.Validation.Exceptions
{
    [Serializable]
    public class BewitException : Exception
    {
        protected BewitException(string errorMessage)
            : base(errorMessage)
        {
        }

        protected BewitException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
