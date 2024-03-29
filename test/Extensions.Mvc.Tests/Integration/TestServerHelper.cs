using System;
using Bewit.Mvc.Filter;
using Bewit.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Bewit.Extensions.Mvc.Tests.Integration
{
    internal static class TestServerHelper
    {
        internal class MockedVariablesProvider : IVariablesProvider
        {
            public DateTime UtcNow =>
                new DateTime(2017, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

            public Guid NextToken =>
                new Guid("724e7acc-be57-49a1-8195-46a03c6271c6");
        }

        public static INonceRepository NonceRepository { get; set; }
        public static IVariablesProvider VariablesProvider { get; set; }

        internal static TestServer CreateServer<T>(BewitOptions options)
        {
            NonceRepository = new DefaultNonceRepository();
            VariablesProvider = new MockedVariablesProvider();

            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddMvc();
                    services
                        .AddTransient<BewitAttribute>()
                        .AddSingleton(options)
                        .AddBewitValidation(options, b => b.AddPayload<T>()
                            .SetVariablesProvider(() => VariablesProvider));
                })
                .Configure(app => app.UseRouting().UseEndpoints(c => c.MapControllers()));
            var server = new TestServer(hostBuilder);
            return server;
        }
    }
}
