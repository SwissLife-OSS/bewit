using System;
using System.Text;
using Bewit;
using Bewit.Generation;
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
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseBewitEndpointAuthorization<string>()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/download/{id:int}", async c =>
                    {
                        var bytes = Encoding.UTF8.GetBytes("hello world");
                        await c.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    });

                    endpoints.MapGet("/opensesame/{id:int}", async c =>
                    {
                        IBewitTokenGenerator<string> generator =
                            c.RequestServices.GetRequiredService<IBewitTokenGenerator<string>>();

                        var id = c.Request.RouteValues.GetValueOrDefault("id");

                        BewitToken<string> token =
                            await generator.GenerateBewitTokenAsync(
                                $"/download/{id}", default);

                        string html = @$"<html><a href=""/download/{id}?bewit={token}"">download</a>
                                        <br>{(string)token}</html>";

                        var bytes = Encoding.UTF8.GetBytes(html);
                        await c.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    });
                });
        }
    }
}
