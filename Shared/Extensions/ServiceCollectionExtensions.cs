using Microsoft.Extensions.DependencyInjection;
using Shared.Http;
using System.Diagnostics;

namespace Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpClientService(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }

    public static IServiceCollection AddHttpClientService<TClient>(this IServiceCollection services, string baseAddress) 
        where TClient : class
    {
        services.AddHttpClient<TClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}

