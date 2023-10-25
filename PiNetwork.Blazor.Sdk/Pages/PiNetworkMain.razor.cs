using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk.Pages;

public partial class PiNetworkMain : ComponentBase
{
    private readonly ILogger logger;
    private readonly NavigationManager navigationManager;
    private readonly IPiNetworkClientBlazor sdk;
    private readonly ISessionStorageService sessionStorage;

    public PiNetworkMain(NavigationManager navigationManager, IPiNetworkClientBlazor sdk, ISessionStorageService sessionStorage)
    {
        this.navigationManager = navigationManager;
        this.sdk = sdk;
        this.sessionStorage = sessionStorage;
    }

    public PiNetworkMain(NavigationManager navigationManager, IPiNetworkClientBlazor sdk, ISessionStorageService sessionStorage, ILoggerFactory loggerFactory)
        : this(navigationManager, sdk, sessionStorage)
    {
        this.logger = loggerFactory.CreateLogger(this.GetType().Name);
    }

    [JSInvokable]
    public async Task AuthenticateOnSuccess(string userString, string redirectUri)
    {
        if (this.logger is { })
            this.logger.LogInformation("Method: {@Method}. Authentication: {@Authentication}. RedirectUri: {RedirectUri}", nameof(AuthenticateOnSuccess), userString, redirectUri);

        await this.sdk.AuthenticateOnSuccessCallBack(userString, redirectUri);
    }

    [JSInvokable]
    public async void AuthenticateOnError(string error, string redirectUri)
    {
        if (this.logger is { })
        {
            if (string.IsNullOrEmpty(error) || error.Equals("{}"))
                this.logger.LogWarning("Method: {@Method}. Error: {@error}. Redirect: {@redirect}", nameof(AuthenticateOnError), error, redirectUri);
            else
                this.logger.LogError("Method: {@Method}. Error: {@error}. Redirect: {@redirect}", nameof(AuthenticateOnError), error, redirectUri);
        }

        await this.sdk.AuthenticateOnErrorCallBack(error, redirectUri, 0);
    }

    [JSInvokable]
    public async Task CreatePaymentOnIncopletePaymentFound(string identifier, string txid)
    {
        if (this.logger is { })
            this.logger.LogInformation("Method: {@Method}. Identifier: {@Identifier}. TxId: {@TxId}", nameof(CreatePaymentOnIncopletePaymentFound), identifier, txid);

        await this.sdk.CreatePaymentOnReadyForServerCompletionCallBack(identifier, txid);
    }

    [JSInvokable]
    public async Task CreatePaymentOnReadyForServerApproval(string paymentId)
    {
        if (this.logger is { })
            this.logger.LogInformation("Method: {@Method}. PaymentId: {PaymentId}", nameof(CreatePaymentOnReadyForServerApproval), paymentId);

        await this.sdk.CreatePaymentOnReadyForServerApprovalCallBack(paymentId);
    }

    [JSInvokable]
    public async Task CreatePaymentOnReadyForServerCompletion(string paymentId, string txid)
    {
        if (this.logger is { })
            this.logger.LogInformation("Method: {@Method}. PaymentId: {@PaymentId}", nameof(CreatePaymentOnReadyForServerCompletion), paymentId);

        await this.sdk.CreatePaymentOnReadyForServerCompletionCallBack(paymentId, txid);
    }

    [JSInvokable]
    public async Task CreatePaymentOnCancel(string paymentId)
    {
        if (this.logger is { })
            this.logger.LogInformation("Method: {@Method}. PaymentId: {PaymentId}", nameof(CreatePaymentOnCancel), paymentId);

        await this.sdk.CreatePaymentOnCancelCallBack(paymentId);
    }

    [JSInvokable]
    public async Task CreatePaymentOnError(string identifier, string txid)
    {
        if (this.logger is { })
            this.logger.LogWarning("Method: {@Method}. Identifier: {@identifier}. TxId: {@txid}", nameof(CreatePaymentOnError), identifier, txid);

        await this.sdk.CreatePaymentOnErrorCallBack(identifier, txid);
    }

    [JSInvokable]
    public async Task IsPiNetworkBrowser()
    {
        /*
                if (this.logger is { })
                    this.logger.LogInformation("Method: {@Method}. {@Value}", nameof(IsPiNetworkBrowser), true);
        */
        await this.sessionStorage.SetItemAsStringAsync(Common.PiNetworkConstants.IsPiNetworkBrowser, "1");
    }
}