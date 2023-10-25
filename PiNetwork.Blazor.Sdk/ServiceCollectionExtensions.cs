using Microsoft.Extensions.DependencyInjection;
using PiNetwork.Blazor.Sdk.Common;
using Polly;
using System;
using System.Net.Http.Headers;

namespace PiNetwork.Blazor.Sdk;

public sealed class PiNetworkOptions
{
    /// <summary>
    /// PiNetwork your application Apykey, must be provided
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// PiNetwrok BaseUrl for requests, must be provided
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Not yet supported. PiNetwrok BaseUrl fallback in case BaseUrl fails to response this url will be tried, it's optional
    /// </summary>
    public string BaseUrlFallback { get; set; }
}

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

            if (string.IsNullOrEmpty(item.ApiKey))
                throw new PiNetworkException("PiNetwork configuration ApiKey can't be null");

            if (string.IsNullOrEmpty(item.BaseUrl))
                throw new PiNetworkException("PiNetwork configuration BaseUrl can't be null");

            string url = GetUrlFull(item.BaseUrl);

            services.AddHttpClient(PiNetworkConstants.PiNetworkClient, client =>
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", item.ApiKey);
            }).AddTransientHttpErrorPolicy(s => s.WaitAndRetryAsync(3, times => TimeSpan.FromSeconds(times * 1)));

            //this linko not supported yet
            if (!string.IsNullOrEmpty(item.BaseUrlFallback))
            {
                url = GetUrlFull(item.BaseUrlFallback);

                services.AddHttpClient(PiNetworkConstants.PiNetworkClientFallback, client =>
                {
                    client.BaseAddress = new Uri(url);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", item.ApiKey);
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