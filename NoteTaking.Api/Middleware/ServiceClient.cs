using System;

namespace NoteTaking.Api.Middleware;

internal static class ServiceClient
{
    internal static IServiceCollection ConfigureHttpClient(this IServiceCollection services)
    {
        services.AddHttpClient("AuthenticateUserClient", options =>
        {
            options.BaseAddress = new Uri("http://localhost:5195/api/Token/login");
            options.DefaultRequestHeaders.Add("Headers", "");
            options.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
