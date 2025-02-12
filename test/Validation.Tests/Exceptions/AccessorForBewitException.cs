using Bewit.Validation.Exceptions;

namespace Bewit.Validation.Tests.Exceptions
{
    public class AccessorForBewitException: BewitException
    {
        public AccessorForBewitException(string errorMessage) 
            : base(errorMessage)
        {
        }
    }
}
