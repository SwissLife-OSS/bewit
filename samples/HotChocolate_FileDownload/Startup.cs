using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                            ReadEmbeddedResource("Host.Files.hello_world.pdf"),
                            "application/pdf")
                    }));

            services.AddBewit(bewit =>
            {
                bewit.UseSigningKey(
                    "sample", "sample-signing-key-with-at-least-32-bytes");
                bewit.AddToken<string>("download-url", token => token
                    .Configure(options => options.Lifetime = TimeSpan.FromMinutes(5)));
            });

            services.AddBewitAspNetCore();

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
                        .WithOptions(o => o.EnableSchemaRequests = true);
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
