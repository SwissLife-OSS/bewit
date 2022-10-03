namespace Bewit.IntegrationTests.HotChocolateServer
{
    public class QueryResponse<T>
    {
        public T Data { get; set; }
        public string Errors { get; set; }
    }
}
