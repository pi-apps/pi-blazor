using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PiNetwork.Blazor.Sdk.Common;
using PiNetwork.Blazor.Sdk.Dto.Payment;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk;

public interface IPiNetworkU2AServerBlazor
{
    Task<PaymentDto> PaymentGet(string paymentId);
    Task<PaymentDto> PaymentApprove(string paymentId);
    Task<PaymentDto> PaymentComplete(string paymentId, string txid);
    Task<PaymentDto> PaymentCancel(string paymentId);
}

public sealed class PiNetworkU2AServerBlazor : IPiNetworkU2AServerBlazor
{
    private readonly ILogger logger;
    private readonly IConfiguration configuration;
    private readonly HttpClient httpClient;

    public PiNetworkU2AServerBlazor(ILoggerFactory loggerFactory, IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        this.configuration = configuration;
        this.httpClient = clientFactory.CreateClient(PiNetworkConstants.PiNetworkClient);
    }

    public async Task<PaymentDto> PaymentGet(string paymentId)
    {
        try
        {
            this.logger.LogInformation("Method: {@Method}. Id: {@Id}", nameof(PaymentGet), paymentId);

            var result = await this.httpClient.GetFromJsonAsync<PaymentDto>($"payments/{paymentId}");
            return result;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Id: {@Id}", nameof(PaymentGet), paymentId);
            throw;
        }
    }

    public async Task<PaymentDto> PaymentApprove(string paymentId)
    {
        try
        {
            this.logger.LogInformation("Method: {@Method}. Id: {@Id}", nameof(PaymentApprove), paymentId);

            var httpResponse = await this.httpClient.PostAsync($"payments/{paymentId}/approve", new StringContent("application/json"));
            if (httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<PaymentDto>();
                return result;
            }
            else
                throw new HttpRequestException($"Method: {nameof(PaymentApprove)}. Http status code is not success: {httpResponse.StatusCode}");
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Id: {@Id}", nameof(PaymentApprove), paymentId);
            throw;
        }
    }

    public async Task<PaymentDto> PaymentComplete(string paymentId, string txid)
    {
        try
        {
            this.logger.LogInformation("Method: {@Method}. Id: {@Id}", nameof(PaymentComplete), paymentId);

            var httpResponse = await this.httpClient.PostAsJsonAsync($"payments/{paymentId}/complete", new PaymentCompleteDto { Txid = txid });
            if (httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<PaymentDto>();
                return result;
            }
            else
                throw new HttpRequestException($"Method: {nameof(PaymentComplete)}. Http status code is not success: {httpResponse.StatusCode}");
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Id: {@Id}", nameof(PaymentComplete), paymentId);
            throw;
        }
    }

    public async Task<PaymentDto> PaymentCancel(string paymentId)
    {
        try
        {
            this.logger.LogInformation("Method: {@Method}. Id: {@Id}", nameof(PaymentCancel), paymentId);

            var httpResponse = await this.httpClient.PostAsync($"payments/{paymentId}/cancel", new StringContent(String.Empty));
            if (httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadFromJsonAsync<PaymentDto>();
                return result;
            }
            else
                throw new HttpRequestException($"Method: {nameof(PaymentCancel)}. Http status code is not success: {httpResponse.StatusCode}");
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Id: {@Id}", nameof(PaymentCancel), paymentId);
            throw;
        }
    }
}