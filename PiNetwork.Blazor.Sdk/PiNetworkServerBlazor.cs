using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PiNetwork.Blazor.Sdk.Dto.Payment;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk
{
    public sealed class PiNetworkServerBlazor : IPiNetworkServerBlazor
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;

        public PiNetworkServerBlazor(IConfiguration configuration, IHttpClientFactory clientFactory, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
            this.configuration = configuration;
            this.httpClient = clientFactory.CreateClient("PiNetworkClient");
        }

        public async Task<PaymentDto> PaymentGet(string id)
        {
            try
            {
                if (this.logger is { })
                    this.logger.LogInformation("Method: {@Method}. Id: {Id}", nameof(PaymentGet), id);

                var result = await this.httpClient.GetFromJsonAsync<PaymentDto>($"payments/{id}");
                return result;
            }
            catch (Exception e)
            {
                if (this.logger is { })
                    this.logger.LogError(e, "Method: {@Method}. Id: {Id}", nameof(PaymentGet), id);
                throw;
            }
        }

        public async Task<PaymentDto> PaymentApprove(string id)
        {
            try
            {
                if (this.logger is { })
                    this.logger.LogInformation("Method: {@Method}. Id: {Id}", nameof(PaymentApprove), id);

                var httpResponse = await this.httpClient.PostAsync($"payments/{id}/approve", new StringContent("application/json"));
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
                if (this.logger is { })
                    this.logger.LogError(e, "Method: {@Method}. Id: {Id}", nameof(PaymentApprove), id);
                throw;
            }
        }

        public async Task<PaymentDto> PaymentComplete(string id, string txid)
        {
            try
            {
                if (this.logger is { })
                    this.logger.LogInformation("Method: {@Method}. Id: {Id}", nameof(PaymentComplete), id);

                var httpResponse = await this.httpClient.PostAsJsonAsync($"payments/{id}/complete", new PaymentCompleteDto { Txid = txid });
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
                if (this.logger is { })
                    this.logger.LogError(e, "Method: {@Method}. Id: {Id}", nameof(PaymentComplete), id);
                throw;
            }
        }
    }
}