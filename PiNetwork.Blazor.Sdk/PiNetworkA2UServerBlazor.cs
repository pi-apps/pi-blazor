using Microsoft.Extensions.Logging;
using PiNetwork.Blazor.Sdk.Common;
using PiNetwork.Blazor.Sdk.Dto;
using PiNetwork.Blazor.Sdk.Dto.Auth;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk;

public interface IPiNetworkA2UServerBlazor
{
    Task<double> GetAccountNativeBalance(string network, string account);
    Task<AuthMeDto> MeGet(string accessToken);
    Task<SubmitTransactionResponse> SendNativeAssets(string network, string seed, TransactionA2UView data, uint fee = 100000);
}

public sealed class PiNetworkA2UServerBlazor : IPiNetworkA2UServerBlazor
{
    private readonly ILogger logger;
    private readonly HttpClient httpClient;

    public PiNetworkA2UServerBlazor(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
    {
        this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        this.httpClient = clientFactory.CreateClient(PiNetworkConstants.PiNetworkClient);
    }

    /// <summary>
    /// Get user auth information
    /// </summary>
    /// <param name="accessToken">from user authentication responce</param>
    /// <returns></returns>
    public async Task<AuthMeDto> MeGet(string accessToken)
    {
        try
        {
            this.logger.LogInformation("Method: {@Method}. AccessToken: {@accessToken}", nameof(MeGet), accessToken);

            this.httpClient.DefaultRequestHeaders.Add("Accept", $"application/json");
            this.httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var result = await this.httpClient.GetFromJsonAsync<AuthMeDto>($"me");

            return result;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. accessToken: {@AccessToken}", nameof(AuthMeDto), accessToken);
            throw;
        }
    }

    /// <summary>
    /// Get native balance 'pi network balance'
    /// </summary>
    /// <param name="network"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    public async Task<double> GetAccountNativeBalance(string network, string account)
    {
        try
        {
            Server server = GetServerAsync(network);
            KeyPair keypair;

            if (account.StartsWith("S"))
                keypair = KeyPair.FromSecretSeed(account);
            else
                keypair = KeyPair.FromAccountId(account);

            AccountResponse accountResponse = await server.Accounts.Account(keypair.AccountId);

            Balance[] balances = accountResponse.Balances;

            for (int i = 0; i < balances.Length; i++)
            {
                if (balances[i].AssetType == "native")
                    return double.Parse(balances[i].BalanceString);
            }

            return 0.0;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Network: {@Network}, Account: {@Account}", nameof(GetAccountNativeBalance), network, account);
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="network">"Pi Network" or "Pi Testnet"</param>
    /// <param name="seed">App developer seed</param>
    /// <param name="data">transaction data</param>
    /// <param name="fee">transaction fee</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<SubmitTransactionResponse> SendNativeAssets(string network, string seed, TransactionA2UView data, uint fee = 100000)
    {
        var sourceKeypair = KeyPair.FromSecretSeed(seed);
        Server server = GetServerAsync(network);
        var destinationKeyPair = KeyPair.FromAccountId(data.ToAddress);
        AccountResponse sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);
        var sourceAccount = new Account(sourceKeypair.AccountId, sourceAccountResponse.SequenceNumber);
        Asset asset = new AssetTypeNative();

        double balance = 0.0;
        for (int i = 0; i < sourceAccountResponse.Balances.Length; i++)
        {
            Balance ast = sourceAccountResponse.Balances[i];
            if (ast.AssetType == "native")
            {
                if (double.TryParse(ast.BalanceString, out balance))
                    break;
            }
        }
        if (balance < data.Amount + 0.01)
        {
            throw new Exception($"Not enough balance ({balance})");
        }

        string amount = $"{Math.Floor(data.Amount * 10000000.0) / 10000000.0:F7}";
        try
        {
            var operation = new PaymentOperation.Builder(destinationKeyPair, asset, amount)
                                                .SetSourceAccount(sourceAccount.KeyPair)
                                                .Build();

            var Identifier = string.IsNullOrEmpty(data.Memmo) ? $"" : $"{data.Memmo.Trim()}";
            var memo = new MemoText(string.IsNullOrEmpty(Identifier) ? $"" : Identifier.Substring(0, Math.Min(Identifier.Length, 28)));
            var transaction = new TransactionBuilder(sourceAccount)
                                        .AddOperation(operation)
                                        .AddMemo(memo)
                                        .SetFee(fee)
                                        .Build();

            transaction.Sign(sourceKeypair, new Network(network));

            var tx = await server.SubmitTransaction(transaction);

            return tx;
        }
        catch (Exception e)
        {
            this.logger.LogError(e, "Method: {@Method}. Network: {@Network}, Data: {@Data}", nameof(SendNativeAssets), network, data);
            throw;
        }
    }

    private static Server GetServerAsync(string network)
    {
        if (network == PiNetworkConstants.PiNetwork)
            return new Server("https://api.mainnet.minepi.com");
        if (network == PiNetworkConstants.PiTestnet)
            return new Server("https://api.testnet.minepi.com");

        throw new PiNetworkException($"Incorect server name provided: {network}");
    }
}
