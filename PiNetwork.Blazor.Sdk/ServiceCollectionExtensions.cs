using Microsoft.Extensions.DependencyInjection;
using PiNetwork.Blazor.Sdk.ConstantsEnums;
using Polly;
using System;
using System.Net.Http.Headers;

namespace PiNetwork.Blazor.Sdk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPiNetwork(this IServiceCollection services, Action<PiNetworkOptions> configure)
    {
        try
        {
            if (configure is null)
                throw new PiNetworkException("PiNetwork configuration can't be null");

            PiNetworkOptions item = new();
            configure.Invoke(item);

            if (item is null)
                throw new PiNetworkException("PiNetwork configuration can't be null");

            if (string.IsNullOrEmpty(item.ApyKey))
                throw new PiNetworkException("PiNetwork configuration ApiKey can't be null");

            if (string.IsNullOrEmpty(item.BaseUrl))
                throw new PiNetworkException("PiNetwork configuration BaseUrl can't be null");

            string url = GetUrlFull(item.BaseUrl);

            services.AddHttpClient(PiNetworkConstants.PiNetworkClient, client =>
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", item.ApyKey);
            }).AddTransientHttpErrorPolicy(s => s.WaitAndRetryAsync(3, times => TimeSpan.FromSeconds(times * 1)));

            if (!string.IsNullOrEmpty(item.BaseUrlFallback))
            {
                url = GetUrlFull(item.BaseUrlFallback);

                services.AddHttpClient(PiNetworkConstants.PiNetworkClientFallback, client =>
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", item.ApyKey);
                }).AddTransientHttpErrorPolicy(s => s.WaitAndRetryAsync(3, times => TimeSpan.FromMilliseconds(times * 300)));
            }

            return services;
        }
        catch (PiNetworkException)
        {
            throw;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public static string GetUrlFull(string urlOriginal)
    {
        ReadOnlySpan<char> url = urlOriginal;

        if (url.Slice(url.Length - 1) == "/")
            return urlOriginal;

        return $"{urlOriginal}/";
    }
}