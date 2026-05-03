using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotChocolate.AspNetCore;
using Host.Data;
using Host.Types;
using System;
using Host.Models;
using System.Collections.Generic;
using Bewit;
using Bewit.Storage.MongoDB;

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
                bewit.ConfigureOptions(o =>
                {
                    o.Secret = "ax54Z$tgs87454";
                    o.TokenDuration = TimeSpan.FromMinutes(5);
                });

                bewit.AddPayload<FooPayload>();

                bewit.AddPayload<BarPayload>(p =>
                {
                    p.ConfigureOptions(o =>
                    {
                        o.ExpiryMode = ExpiryMode.ServerControlled;
                    });

                    p.UseMongoDb(m =>
                    {
                        m.ConnectionString = "mongodb://localhost:27017";
                        m.DatabaseName = "bewit_secured_argument";
                    });
                });

                bewit.AddPayload<BazPayload>(p =>
                {
                    p.ConfigureOptions(o =>
                    {
                        o.ExpiryMode = ExpiryMode.ServerControlled;
                    });

                    p.UseMongoDb(m =>
                    {
                        m.ConnectionString = "mongodb://localhost:27017";
                        m.DatabaseName = "bewit_secured_argument";
                    });
                });
            });

            services.AddBewitGeneration<FooPayload>();
            services.AddBewitGeneration<BarPayload>();
            services.AddBewitGeneration<BazPayload>();
            services.AddBewitValidation<FooPayload>();
            services.AddBewitValidation<BarPayload>();
            services.AddHttpContextAccessor();

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<Mutation>()
                .AddType<DocumentType>()
                .AddMutationConventions()
                .InitializeOnStartup()
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
                .UseBewitTokenHeaderExtraction()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGraphQL(path: "/")
                        .WithOptions(new GraphQLServerOptions { EnableSchemaRequests = true });
                });
        }
    }
}
