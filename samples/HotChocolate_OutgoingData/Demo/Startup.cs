using HotChocolate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Demo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLServer()
                .AddQueryType<QueryType>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapGraphQL("/");
            });
        }
    }
}
