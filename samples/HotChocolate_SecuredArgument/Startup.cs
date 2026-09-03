using System;
using System.Collections.Generic;
using Bewit;
using Host.Data;
using Host.Models;
using Host.Types;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(new DocumentsRepository(
                    new List<Document>
                    {
                        new Document(
                            "hello_world.pdf",
                            "application/pdf")
                    }));

            services.AddBewit(bewit =>
            {
                bewit.UseSigningKey(
                    "sample", "sample-signing-key-with-at-least-32-bytes");
                bewit.UseMongoDb(mongo =>
                {
                    mongo.ConnectionString = "mongodb://localhost:27017";
                    mongo.DatabaseName = "bewit_secured_argument";
                });

                bewit.AddToken<FooPayload>("foo");
                bewit.AddToken<BarPayload>("bar", token => token
                    .UseServerControlledExpiration());
                bewit.AddToken<BazPayload>("baz", token => token
                    .UseServerControlledExpiration());
            });

            services.AddBewitAspNetCore();

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddType<DocumentType>()
                .AddMutationConventions()
                .UseDefaultPipeline();

            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGraphQL(path: "/")
                        .WithOptions(o => o.EnableSchemaRequests = true);
                });
        }
    }
}
