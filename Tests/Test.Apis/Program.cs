
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using MondoCore.Rest;
using Test.Apis.Classes;
using Test.Apis.Controllers;

namespace Test.Apis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer()
                            .AddSwaggerGen()
                            .AddHttpClient()
                            .AddServices()
                            .Configure<KestrelServerOptions>(options =>
                            {
                                options.ConfigureHttpsDefaults(options =>
                                { 
                                    options.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                                    options.AllowAnyClientCertificate();
                                });
                            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }

    public static class Extensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services.AddScoped<ITransformer<Municipality>>( p=> new Transformer<Municipality>("Municipality"))
                           .AddRestApi<Municipality>("madrid", "https://datos.comunidad.madrid/catalogo/dataset/032474a0-bf11-4465-bb92-392052962866/resource/301aed82-339b-4005-ab20-06db41ee7017/download/municipio_comunidad_madrid.json");
        }

        public static IServiceCollection AddRestApi<T>(this IServiceCollection services, string name, string url)
        {
            services.AddHttpClient(name: name, configureClient: (p, client) =>
                    {
                        client.BaseAddress = new Uri(url);
                        client.Timeout = TimeSpan.FromSeconds(60);
                    });

            services.AddScoped<IRestApi<T>>( p=> new RestApi<T>(p.GetRequiredService<IHttpClientFactory>(), name));

            return services;
        }
    }
}
