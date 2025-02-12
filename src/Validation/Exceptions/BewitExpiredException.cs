namespace Bewit.Validation.Exceptions
{
    public sealed class BewitExpiredException : BewitException
    {
        public BewitExpiredException()
            : base("The given Bewit has expired.")
        {
        }
    }
}
