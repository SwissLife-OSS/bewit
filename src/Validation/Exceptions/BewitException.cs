using System;

namespace Bewit.Validation.Exceptions
{
    public class BewitException : Exception
    {
        protected BewitException(string errorMessage)
            : base(errorMessage)
        {
        }
    }
}
