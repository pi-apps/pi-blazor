## LICENSE
See LICENSE.md file for more details

## INSTALATION

Clone project, add to your solution.
This setup is for server side Blazor, but for WASM blazor (client side) would be with minimal changes.

Add lines to appsettings.json

    "PiNetwork": {
        "ApiKey": "YourApiKey",
        "BaseUrl": "https://api.minepi.com/v2"
    }


Add reference to your Server side blazor project to PiNetwork.Blazor.Sdk

Modify Startup.cs file:

    public void ConfigureServices(IServiceCollection services)
    {
    ...your rest code
    services.AddScoped<IPiNetworkClientBlazor, ServicesPiNetworkFacade>();
    services.AddScoped<IPiNetworkServerBlazor, PiNetworkServerBlazor>();
    ...your rest code

    services.AddPiNetwork(options =>
    {
            options.ApyKey = this.Configuration["PiNetwork:ApiKey"];
            options.BaseUrl = this.Configuration["PiNetwork:BaseUrl"];
    });

    services.AddBlazoredSessionStorage();
    services.AddMemoryCache();

    ...your rest code
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
    ... your rest code
    app.UseRouting();
    app.UseCookiePolicy(
            new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.None,
                HttpOnly = HttpOnlyPolicy.Always,
                Secure = CookieSecurePolicy.Always
            }
    );
    app.UseAuthentication();
    app.UseAuthorization();
    ... your rest code
    }

Now you will need to create some class to deal with PiNetwork callbacks. This class must be extended from abstract class PiNetworkClientBlazor. You will need to override methods to your needs. In example bellow your bussiness logic handler is IOrderServices.

Put this new class to Server side Blazor project. Lets call it ServicesPiNetworkFacade

    public class ServicesPiNetworkFacade : PiNetworkClientBlazor
    {
        private readonly ILogger logger;
        private readonly IPiNetworkServerBlazor server;
        private readonly IOrderServices orderServices;
        
        //If you don't need logging use constructor without ILoggerFactory
        public ServicesPiNetworkFacade(ILoggerFactory loggerFactory,
                                    NavigationManager navigationManager,
                                    IJSRuntime jsRuntime,
                                    ISessionStorageService sessionStorage,
                                    IPiNetworkServerBlazor server,
                                    IOrderServices orderServices)
                : base(navigationManager, jsRuntime, sessionStorage, loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
            this.server = server;
            this.orderServices = orderServices; //your bussiness logic is here.
        }

        public override async Task CreatePaymentOnReadyForServerApprovalCallBack(string paymentId)
        {
            var result = await this.server.PaymentApprove(paymentId); //don't change this row.
            await this.orderServices.SavePaymentStatusPiNetwork(result.Metadata.OrderId, Enums.PaymentStatus.Waiting, null);
        }

        public override async Task CreatePaymentOnReadyForServerCompletionCallBack(string paymentId, string txid)
        {
            var result = await this.server.PaymentComplete(paymentId, txid); //don't change this row
            await this.orderServices.SavePaymentStatusPiNetwork(result.Metadata.OrderId, Enums.PaymentStatus.AdditionalSuccess, txid);
            this.navigationManager.NavigateTo($"myorders/{result.Metadata.OrderId}", forceLoad: true);
        }

        public override Task AuthenticateOnErrorCallBack(object error)
        {
        }

        public override Task CreatePaymentOnCancelCallBack(string paymentId)
        {
        }

        public override Task CreatePaymentOnErrorCallBack(object error, PaymentDto payment)
        {
        }

        public override Task OpenShareDialog(string title, string message)
        {
        }
    }

## SEE EXAMPLE FROM REAL PROJECT
See ServicesPiNetworkFacadeExample.md

## RETRIES
By default if something fails PiNetworkClientBlazor will try to retry (if it is reasonable to do so) up to 10 times with 1000 milliseconds interval.
You can override this values in ServicesPiNetworkFacade:

    public virtual int Retries { get; set; } = 10;

    public virtual int RetryDelay { get; set; } = 1000;

## USE ENUMS FOR MESSAGES IN PiNetworkConstantsEnums.cs
See PiNetwork.Blazor.Sdk.ConstantsEnums for messages to pass to front side
    AuthenticationError = 0,
    PaymentError = 1,
    AuthenticationSuccess = 2,
    PaymentSuccess = 3


## USE CONSTANTS IN PiNetworkConstantsEnums.cs
    public const string PiNetworkSdkCallBackError = "PiNetworkSdkCallBackError";
    public const string PiNetworkDoNotRedirect = "PiNetworkDoNotRedirect";
    public const string IsPiNetworkBrowser = "IsPiNetworkBrowser";

If you don't need to redirect use constant "PiNetworkDoNotRedirect" this is used in AuthenticateOnSuccessCallBack(AuthResultDto auth, string redirectUri).
See ServicesPiNetworkFacadeExample.md.

## CHECK IF BROWSER IS PI NETWORK BROWSER
PiNetworkClientBlazor.IsPiNetworkBrowser() this method is not from PI SDK.




