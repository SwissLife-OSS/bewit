using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using Bewit;
using System.Text;
using Bewit.Http.Endpoint;
using Bewit.Generation;

namespace Host
{

    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var bewitOptions = new BewitOptions(secret: "ax54Z$tgs87454")
            {
                TokenDuration = TimeSpan.FromMinutes(5)
            };

            // Add support for generating bewits
            services.AddBewitGeneration<string>(bewitOptions);

            // Add support for endpoint authorization
            services.AddBewitEndpointAuthorization(bewitOptions);

            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseMiddleware<BewitEndpointMiddleware>()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/download/{id:int}", async c =>
                    {
                        var bytes = Encoding.UTF8.GetBytes("hello world");
                        await c.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    })
                    .RequireBewitUrlAuthorization();

                    endpoints.MapGet("/opensesame/{id:int}", async c =>
                    {
                        IBewitTokenGenerator<string> generator =
                            c.RequestServices.GetRequiredService<IBewitTokenGenerator<string>>();

                        var id = c.Request.RouteValues.GetValueOrDefault("id");

                        BewitToken<string> token =
                            await generator.GenerateBewitTokenAsync($"/download/{id}", default);

                        string html = @$"<html><a href=""/download/{id}?bewit={token}"">download</a>
                                        <br>{(string)token}</html>";

                        var bytes = Encoding.UTF8.GetBytes(html);
                        await c.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    });
                });
        }
    }
}
