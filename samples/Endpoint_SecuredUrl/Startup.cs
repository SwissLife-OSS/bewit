using System;
using System.Text;
using Bewit;
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
                bewit.UseSigningKey(
                    "sample", "sample-signing-key-with-at-least-32-bytes");
                bewit.AddToken<string>("download", token => token
                    .Configure(options => options.Lifetime = TimeSpan.FromMinutes(5)));
            });

            services.AddBewitAspNetCore();
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet(
                        "/download/{id:int}",
                        () => Results.Text("hello world"))
                        .AddBewitAuthorization<string>();

                    endpoints.MapGet("/opensesame/{id:int}", async c =>
                    {
                        IBewitTokenGenerator<string> generator =
                            c.RequestServices.GetRequiredService<IBewitTokenGenerator<string>>();

                        var id = c.Request.RouteValues.GetValueOrDefault("id");

                        BewitToken<string> token =
                            await generator.GenerateAsync(
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
