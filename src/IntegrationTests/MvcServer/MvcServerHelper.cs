using Bewit.Mvc.Filter;
using Bewit.Storage.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.IntegrationTests.MvcServer
{
    internal static class MvcServerHelper
    {
        internal static TestServer CreateMvcServer(
            string secret,
            string connectionString,
            string databaseName)
        {
            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvc();

                    //for url protection
                    services
                        .AddBewitUrlAuthorizationFilter(
                            new BewitOptions
                            {
                                Secret = secret
                            },
                            pb => pb.UseMongoPersistence(
                                new MongoNonceOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                }));

                    //for payload protection, the payload can be injected into an action's parameter through the [FromBewit] Attribute
                    services
                        .AddBewitFilter(
                            new BewitOptions
                            {
                                Secret = secret
                            },
                            pb => pb.UseMongoPersistence(
                                new MongoNonceOptions
                                {
                                    ConnectionString = connectionString,
                                    DatabaseName = databaseName
                                }));
                })
                .Configure(app => app.UseRouting().UseEndpoints(c => c.MapControllers()));

            return new TestServer(hostBuilder);
        }
    }
}
