#Publishing nuget package to AVSoft

nuget.exe push -Source "https://aarrtturas.pkgs.visualstudio.com/_packaging/AVSoft/nuget/v3/index.json" -ApiKey az "C:\Users\ArturasValincius\Desktop\Challenge\Clients\PiNetwork.Blazor.Sdk\bin\Release\PiNetwork.Blazor.Sdk.1.0.0.nupkg"



#INSTALATION
Clone project, add to your solution
Modyfy Startup.cs file:

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

   ...your rest code
}


It's important to set CookiePolicyOptions to MinimumSameSitePolicy = SameSiteMode.None

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
   ... your rest code
   app.UseRouting();
   app.UseCookiePolicy(
         new CookiePolicyOptions
         {
            MinimumSameSitePolicy = SameSiteMode.None
         }
   );
   app.UseAuthentication();
   app.UseAuthorization();
   ... your rest code
}

At this repository we have Pages folder. Copy it:

a) Copy to your Server Side Blazor project folder 'Pages' content.

b) Copy to your **WASM Blazor Server part (not to client part)** project folder 'Pages' content


Don't change path 'Pages/api/pinetwork/pinetworksignin'!. This end point will be called after Authentication and authentication cookie will be created.


Settings is done!

Now you will need to create some class to deal with PiNetwork callbacks. 
This class mus be extended from abstract class PiNetworkClientBlazor.
You will need to override at least two methods.
In example bellow your bussiness logic handler is IOrderServices, the rest is out of the box, and you don't need to change.

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


