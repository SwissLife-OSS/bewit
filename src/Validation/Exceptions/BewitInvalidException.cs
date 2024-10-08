namespace Bewit.Validation.Exceptions
{
    public sealed class BewitInvalidException : BewitException
    {
        public BewitInvalidException()
            : base("The given Bewit is invalid.")
        {
        }
    }
}
