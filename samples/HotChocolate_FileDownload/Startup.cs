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
using System.Reflection;
using System.IO;
using System.Linq;
using Bewit;

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
                            ReadEmbeddedResource("Host.Files.hello_world.pdf"),
                            "application/pdf")
                    }));

            services.AddBewit(bewit =>
            {
                bewit.ConfigureOptions(o =>
                {
                    o.Secret = "ax54Z$tgs87454";
                    o.TokenDuration = TimeSpan.FromMinutes(5);
                });

                bewit.AddPayload<string>();
            });

            services.AddBewitGeneration<string>();
            services.AddBewitValidation<string>();
            services.AddHttpContextAccessor();

            services
                .AddGraphQLServer()
                .AddQueryType<Query>()
                .AddMutationType<MutationType>()
                .AddType<DocumentType>();

            services.AddControllers();
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
                    endpoints.MapControllers();
                });
        }

        private byte[] ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is { })
            {
                byte[] ba = new byte[stream.Length];
                int valuesRead = stream.Read(ba, 0, ba.Length);
                return ba.Take(valuesRead).ToArray();
            }

            return Array.Empty<byte>();
        }
    }
}
