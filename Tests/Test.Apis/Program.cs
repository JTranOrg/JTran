using System.Reflection;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using MondoCore.Common;
using MondoCore.Rest;

using JTran;

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
                                    options.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
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
            return services.AddTransformer<List<Municipality>>("Municipalities")
                           .AddTransformer<Municipality>("Municipality")
                           .AddTransformer<Movie>("Movies")
                           .AddTransformer<History>("History")
                           .AddRestApi<Municipality>("madrid", "https://datos.comunidad.madrid/catalogo/dataset/05a7750f-0215-43a0-ac8b-4f78dc458f78/resource/57d61071-7329-448a-859b-a27de284f3a5/download/cm.json")
                           .AddRestApi<Movie>("movies", "https://showtimes.everyday.in.th/api/v2/movie/")
                           .AddRestApi<History>("history", "https://www.vizgr.org/historical-events/search.php");
        }

        private static IServiceCollection AddTransformer<T>(this IServiceCollection services, string name)
        {
            return services.AddScoped<ITransformer<T>>( p=> 
            {
                return TransformerBuilder.FromString(LoadTransform(name))
                                         .Build<T>();
            });
        }

        private static string LoadTransform(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"Test.Apis.Transforms.{name}.jtran");

            return stream.ReadString();
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
