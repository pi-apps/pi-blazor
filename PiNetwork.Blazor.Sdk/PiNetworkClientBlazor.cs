using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using PiNetwork.Blazor.Sdk.Dto.Auth;
using PiNetwork.Blazor.Sdk.Javascript;
using PiNetwork.Blazor.Sdk.Pages;
using System;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk
{
    public abstract class PiNetworkClientBlazor : IPiNetworkClientBlazor
    {
        private readonly ILoggerFactory loggerFactory;

        private readonly ILogger logger;
        protected readonly NavigationManager navigationManager;
        protected readonly IJSRuntime jsRuntime;
        private readonly ISessionStorageService sessionStorage;

        /// <summary>
        /// Number of retries to make if error / message content is usafull for making retry.
        /// </summary>
        public virtual int Retries { get; set; } = 10;

        /// <summary>
        /// Delay in milliseconds
        /// </summary>
        public virtual int RetryDelay { get; set; } = 1000;

        public PiNetworkClientBlazor(NavigationManager navigationManager, IJSRuntime jsRuntime, ISessionStorageService sessionStorage)
        {
            this.jsRuntime = jsRuntime;
            this.navigationManager = navigationManager;
            this.sessionStorage = sessionStorage;
        }

        public PiNetworkClientBlazor(NavigationManager navigationManager, IJSRuntime jsRuntime, ISessionStorageService sessionStorage, ILoggerFactory loggerFactory)
            : this(navigationManager, jsRuntime, sessionStorage)
        {
            this.loggerFactory = loggerFactory;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }

        public virtual async Task Authenticate(string redirectUri, int retries = 0)
        {
            if (this.logger is { })
                this.logger.LogInformation("Method: {@Method}", nameof(Authenticate));

            DotNetObjectReference<PiNetworkMain> objRef;

            if (this.logger is { })
                objRef = DotNetObjectReference.Create(new PiNetworkMain(this.navigationManager, this, this.sessionStorage, this.loggerFactory));
            else
                objRef = DotNetObjectReference.Create(new PiNetworkMain(this.navigationManager, this, this.sessionStorage));

            try
            {
                await PiNetworkJavascript.Authenticate(jsRuntime, objRef, redirectUri, retries);
            }
            catch (Exception e)
            {
                if (this.logger is { })
                    this.logger.LogError(e, "Method: {@Method}. Message: {Message}", nameof(Authenticate), e.Message);

                await this.sessionStorage.SetItemAsync(ConstantsEnums.ConstantsEnums.PiNetworkSdkCallBackError, ConstantsEnums.Messages.AuthenticationError);

                this.navigationManager.NavigateTo($"/", forceLoad: true);
            }
        }

        public abstract Task AuthenticateOnErrorCallBack(string error, string redirectUri, int retries = 0);

        public abstract Task AuthenticateOnSuccessCallBack(AuthResultDto auth, string redirectUri);

        /// <summary>
        /// Make payment
        /// </summary>
        /// <param name="amount">amount to charge</param>
        /// <param name="memo">text to display. Limit to 25 characters</param>
        /// <param name="orderId">your order id</param>
        /// <param name="retries">0 for first attempt</param>
        /// <returns></returns>
        public virtual async Task CreatePayment(decimal amount, string memo, int orderId, int retries = 0)
        {
            DotNetObjectReference<PiNetworkMain> objRef;

            if (this.logger is { })
                objRef = DotNetObjectReference.Create(new PiNetworkMain(this.navigationManager, this, this.sessionStorage, this.loggerFactory));
            else
                objRef = DotNetObjectReference.Create(new PiNetworkMain(navigationManager, this, sessionStorage));

            try
            {
                await PiNetworkJavascript.CreatePayment(jsRuntime, objRef, amount, memo, orderId);
            }
            catch (TaskCanceledException e)
            {
                if (this.logger is { })
                    this.logger.LogWarning(e, "Method: {@Method}. Message: {Message}", nameof(CreatePayment), e.Message);
            }
            catch (Exception e)
            {
                //Some exceptions can be practical to retrie. They can be added here. Max 3 retries for client side.
                if (e is JSException)
                {
                    string textToSearch = "Cannot create a payment without \"payments\" scope";

                    if (CreatePaymentExceptionMessageValidation(e, textToSearch) && retries <= this.Retries)
                    {
                        await Task.Delay(this.RetryDelay);
                        await CreatePayment(amount, memo, orderId, retries + 1);
                    }
                    else
                        await CreatePaymentExceptionProccess(e, textToSearch);
                }
                else
                {
                    if (this.logger is { })
                        this.logger.LogError(e, "Method: {@Method}. Message: {Message}", nameof(CreatePayment), e.Message);

                    await this.sessionStorage.SetItemAsync(ConstantsEnums.ConstantsEnums.PiNetworkSdkCallBackError, ConstantsEnums.Messages.PaymentError);

                    this.navigationManager.NavigateTo($"/", forceLoad: true);
                }
            }
        }

        public abstract Task CreatePaymentOnReadyForServerApprovalCallBack(string paymentId, int reties = 0);

        public abstract Task CreatePaymentOnReadyForServerCompletionCallBack(string paymentId, string txid, int retries = 0);

        public abstract Task CreatePaymentOnCancelCallBack(string paymentId);

        public abstract Task CreatePaymentOnErrorCallBack(string paymentId, string txid);

        public virtual async Task OpenShareDialog(string title, string message)
        {
            await PiNetworkJavascript.OpenShareDialog(jsRuntime, title, message);
        }

        /// <summary>
        /// This method is not from Pi network SDK
        /// </summary>
        /// <returns></returns>
        public virtual async Task IsPiNetworkBrowser()
        {
            DotNetObjectReference<PiNetworkMain> objRef;

            if (this.logger is { })
                objRef = DotNetObjectReference.Create(new PiNetworkMain(this.navigationManager, this, this.sessionStorage, this.loggerFactory));
            else
                objRef = DotNetObjectReference.Create(new PiNetworkMain(navigationManager, this, sessionStorage));

            await PiNetworkJavascript.IsPiNetworkBrowser(jsRuntime, objRef);
        }

        protected static bool CreatePaymentExceptionMessageValidation(Exception e, string text) =>
            (!string.IsNullOrEmpty(e.Message) && e.Message.Contains(text)) ||
            (e.InnerException is { } && !string.IsNullOrEmpty(e.InnerException.Message) && e.Message.Contains(text)) ||
            (!string.IsNullOrEmpty(e.StackTrace) && e.StackTrace.Contains(text));

        protected async Task CreatePaymentExceptionProccess(Exception e, string text)
        {
            if (this.logger is { } && CreatePaymentExceptionMessageValidation(e, text))
                this.logger.LogWarning(e, "Method: {@Method}. Message: {Message}", nameof(CreatePayment), e.Message);
            else if (this.logger is { })
                this.logger.LogError(e, "Method: {@Method}. Message: {Message}", nameof(CreatePayment), e.Message);

            await this.sessionStorage.SetItemAsync(ConstantsEnums.ConstantsEnums.PiNetworkSdkCallBackError, ConstantsEnums.Messages.PaymentError);

            this.navigationManager.NavigateTo($"/", forceLoad: false);
        }
    }
}