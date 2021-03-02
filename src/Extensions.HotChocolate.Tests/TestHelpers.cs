using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bewit.Core;
using Bewit.Extensions.HotChocolate.Validation;
using Bewit.Generation;
using Bewit.Storage.MongoDB;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Moq;
using Squadron;

namespace Bewit.Extensions.HotChocolate.Tests
{
    public static class TestHelpers
    {
        public static async Task<string> CreateToken<T>(
            IServiceProvider serviceProvider, T payload)
        {
            IBewitTokenGenerator<T> bewitGenerator = serviceProvider
                .GetService<IBewitTokenGenerator<T>>();

            return (await bewitGenerator
                    .GenerateBewitTokenAsync(payload, default))
                .ToString();
        }

        public static async Task<string> CreateBadToken()
        {
            var bewitOptions = new BewitOptions
            { Secret = "badSecret", TokenDuration = TimeSpan.FromMinutes(5) };

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddBewitGeneration(bewitOptions, b => b.AddPayload<string>())
                .BuildServiceProvider();

            IBewitTokenGenerator<string> bewitGenerator = serviceProvider
                .GetService<IBewitTokenGenerator<string>>();

            return (await bewitGenerator
                    .GenerateBewitTokenAsync("badPayload", default))
                .ToString();
        }

        public static async Task<IExecutionResult> ExecuteQuery(
            IServiceProvider services, string token = null)
        {
            IQueryRequestBuilder requestBuilder =
                QueryRequestBuilder.New()
                    .SetQuery("{ foo }");

            if (token != null)
            {
                HttpContext httpContext = services.GetRequiredService<HttpContext>();
                httpContext.Request.Headers.Add(BewitTokenHeader.Value, token);
            }

            return await services.ExecuteRequestAsync(requestBuilder.Create());
        }

        public static IServiceProvider CreateSchema<TPayload>()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Bewit:Secret", "secret"),
                    new KeyValuePair<string, string>("Bewit:TokenDuration", "0:00:05:00")
                })
                .Build();

            var httpContext = new DefaultHttpContext();
            var httpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
            httpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContext);

            return new ServiceCollection()
                .AddSingleton<HttpContext>(httpContext)
                .AddSingleton(httpContextAccessor.Object)
                .AddBewitGeneration(configuration, b => b.AddPayload<TPayload>())
                .AddGraphQLServer()
                .UseBewitAuthorization<TPayload>(configuration)
                .AddQueryType(c =>
                    c.Name("Query")
                        .Field("foo")
                        .Type<StringType>()
                        .Resolver("bar")
                        .AuthorizeBewit<TPayload>())
                .UseDefaultPipeline()
                .Services
                .BuildServiceProvider();
        }
    }
}
