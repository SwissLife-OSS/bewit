using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotChocolate;
using HotChocolate.AspNetCore;
using Host.Data;
using Host.Types;
using Bewit.Generation;
using Bewit.Core;
using System;
using Host.Models;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using Bewit.Extensions.HotChocolate.Validation;

namespace Host
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                // Add some sample data
                .AddSingleton(new DocumentsRepository(
                    new List<Document>
                    {
                        new Document(
                            "hello_world.pdf",
                            "application/pdf")
                    }));

            var bewitOptions = new BewitOptions
            {
                TokenDuration = TimeSpan.FromMinutes(5),
                Secret = "ax54Z$tgs87454"
            };

            // Add support for generating bewits in the GraphQL Api
            services.AddBewitGeneration(
                bewitOptions,
                builder => builder.UseHmacSha256Encryption().AddPayload<string>());

             services.AddHttpContextAccessor();

            // Add GraphQL Services
            services
                .AddGraphQLServer()
                .AddQueryType<QueryType>()
                .AddType<DocumentType>()
                .UseBewitAuthorization(bewitOptions);

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                        .WithOptions(new GraphQLServerOptions { EnableSchemaRequests = true });
                });
        }

        private byte[] ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream =
                assembly.GetManifestResourceStream(resourceName))
            {
                byte[] ba = new byte[stream.Length];
                int valuesRead = stream.Read(ba, 0, ba.Length);
                return ba.Take(valuesRead).ToArray();
            }
        }
    }
}
