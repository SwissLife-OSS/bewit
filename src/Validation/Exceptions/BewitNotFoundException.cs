namespace Bewit.Validation.Exceptions
{
    public sealed class BewitNotFoundException : BewitException
    {
        public BewitNotFoundException()
            : base("The given Bewit was not found.")
        {
        }
    }
}
