using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bewit.Extensions.HotChocolate.Tests.Integration
{
    public class GraphQLClient
    {
        private readonly HttpClient _client;
        private const string ContentType = "application/json";

        public GraphQLClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<QueryResponse<T>> QueryAsync<T>(
            QueryRequest request,
            CancellationToken cancellationToken)
        {
            using (HttpClient client = _client)
            {
                var encodedData = JsonConvert.SerializeObject(request,
                    new JsonSerializerSettings
                    {
                        ContractResolver =
                            new CamelCasePropertyNamesContractResolver()
                    });

                HttpResponseMessage httpResult = await client.PostAsync(
                    string.Empty,
                    new StringContent(
                        encodedData,
                        Encoding.UTF8,
                        ContentType), cancellationToken);

                var res = await httpResult.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<QueryResponse<T>>(res);
            }
        }
    }
}
