namespace Bewit.HotChocolate.Tests.Integration
{
    public class QueryResponse<T>
    {
        public T Data { get; set; }
        public string Errors { get; set; }
    }
}
