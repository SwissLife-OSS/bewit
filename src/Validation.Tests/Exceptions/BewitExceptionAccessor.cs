using System;
using System.Runtime.Serialization;
using Bewit.Validation.Exceptions;

namespace Bewit.Validation.Tests.Exceptions
{
    [Serializable]
    public class BewitExceptionAccessor: BewitException
    {
        public BewitExceptionAccessor(string errorMessage) 
            : base(errorMessage)
        {
        }

        public BewitExceptionAccessor(
            SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
