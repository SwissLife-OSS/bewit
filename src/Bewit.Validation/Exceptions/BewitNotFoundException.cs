using System;
using System.Runtime.Serialization;

namespace Bewit.Validation.Exceptions
{
    [Serializable]
    public sealed class BewitNotFoundException : BewitException
    {
        public BewitNotFoundException()
            : base("The given Bewit was not found.")
        {
        }

        private BewitNotFoundException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
