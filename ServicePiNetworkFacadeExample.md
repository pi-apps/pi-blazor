    using AVSoft.Logging;
    using Blazored.SessionStorage;
    using Chall.Blazor.Pages.Infrastructure;
    using Chall.Enumerators;
    using Chall.Services;
    using Chall.Services.Interfaces;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.JSInterop;
    using PiNetwork.Blazor.Sdk;
    using PiNetwork.Blazor.Sdk.Dto.Auth;
    using PiNetwork.Blazor.Sdk.ConstantsEnums;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    namespace Chall.Server.Side.Infrastructure
    {
        public sealed class ServicesPiNetworkFacade : PiNetworkClientBlazor
        {
            private readonly ILogger logger;
            private readonly ISessionStorageService sessionStorage;
            private readonly IPiNetworkServerBlazor server;
            private readonly IOrderService orderServices;
            private readonly IUserInfoService userInfoService;
            private readonly IMailingService mailingService;
            private readonly IMemoryCache cache;
            private readonly IPiNetworkAuthStateProvider authenticationState;

            public ServicesPiNetworkFacade(ILoggerFactory loggerFactory,
                                        NavigationManager navigationManager,
                                        IJSRuntime jsRuntime,
                                        ISessionStorageService sessionStorage,
                                        IPiNetworkServerBlazor server,
                                        IOrderService orderServices,
                                        IUserInfoService userInfoService,
                                        IMailingService mailingService,
                                        IMemoryCache cache,
                                        IPiNetworkAuthStateProvider authenticationState)
                : base(navigationManager, jsRuntime, sessionStorage, loggerFactory)
            {
                this.logger = loggerFactory.CreateLogger(this.GetType().Name);
                this.sessionStorage = sessionStorage;
                this.server = server;
                this.orderServices = orderServices;
                this.userInfoService = userInfoService;
                this.mailingService = mailingService;
                this.cache = cache;
                this.authenticationState = authenticationState;
            }

            public override int Retries { get; set; } = 8;

            public override async Task AuthenticateOnErrorCallBack(string error, string redirectUri, int retries = 0)
            {
                if (retries <= base.Retries && (string.IsNullOrEmpty(error) || error.Equals("{}")))
                {
                    this.logger.LogWarning("Method: {@Method}. Error: {@Error}", nameof(AuthenticateOnErrorCallBack), error);
                    await Authenticate(redirectUri, retries + 1);
                }
                else
                    this.logger.LogError("Method: {@Method}. Error: {@Error}", nameof(AuthenticateOnErrorCallBack), error);

                await this.sessionStorage.SetItemAsync(ConstantsEnums.PiNetworkSdkCallBackError, Messages.AuthenticationError);

                this.navigationManager.NavigateTo($"/", forceLoad: false);
            }

            public override async Task AuthenticateOnSuccessCallBack(AuthResultDto auth, string redirectUri)
            {
                try
                {
                    if (this.logger is { })
                        this.logger.LogInformation("Method: {@Method}. Authentication: {@Authentication} RedirectUri: {RedirectUri}", nameof(AuthenticateOnSuccessCallBack), auth, redirectUri);

                    if (string.IsNullOrEmpty(auth.User.Username))
                    {
                        if (this.logger is { })
                            this.logger.LogError("Method: {@Method}. Pi network SDK not returning 'username', it must be returned, to properly authenticate. Authentication: {@Authentication} RedirectUri: {RedirectUri}", nameof(AuthenticateOnSuccessCallBack), auth, redirectUri);

                        this.navigationManager.NavigateTo("signin/error");
                    }
                    else if (!redirectUri.Equals(ConstantsEnums.PiNetworkDoNotRedirect))
                    {
                        var user = await this.userInfoService.Get(auth.User.Username);

                        if (user is { })
                            await this.authenticationState.LoginAsync(user);
                        else
                        {
                            user = new Domains.View.UserInfoView { PiNetworkId = auth.User.Username, IsAuthenticated = true };
                            await this.authenticationState.LoginAsync(user);
                        }

                        this.navigationManager.NavigateTo($"{redirectUri}", true);
                    }
                }
                catch (Exception e)
                {
                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. Authentication: {@Authentication}. Redirect: {@Redirect}", nameof(AuthenticateOnSuccessCallBack), auth, redirectUri);
                    throw;
                }
            }

            public override async Task CreatePaymentOnReadyForServerApprovalCallBack(string paymentId, int retries = 0)
            {
                try
                {
                    string key = $"CreatePaymentOnReadyForServerApprovalCallBack-{paymentId}";

                    if (!this.cache.TryGetValue(key, out bool exists))
                    {
                        this.cache.Set(key, true, TimeSpan.FromSeconds(60));

                        if (this.logger is { })
                            this.logger.LogInformation("Method: {@Method}. PaymentId: {@PaymentId}", nameof(CreatePaymentOnReadyForServerApprovalCallBack), paymentId);

                        var result = await this.server.PaymentApprove(paymentId);

                        if (this.logger is { })
                            this.logger.LogInformation("Method: {@Method}. Result: {@Result}", nameof(CreatePaymentOnReadyForServerApprovalCallBack), result);

                        await this.orderServices.SavePaymentStatus(result.Metadata.OrderId, Enums.PaymentStatus.Waiting, null);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        string textToSearch = "NotFound";
                        if (CreatePaymentExceptionMessageValidation(e, textToSearch) && retries <= base.Retries)
                        {
                            await Task.Delay(base.RetryDelay);
                            await CreatePaymentOnReadyForServerApprovalCallBack(paymentId, retries + 1);
                        }
                        else
                            await CreatePaymentExceptionProccess(e, textToSearch);
                    }

                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. PaymentId: {@paymentId}", nameof(CreatePaymentOnReadyForServerApprovalCallBack), paymentId);

                    this.navigationManager.NavigateTo($"/", forceLoad: false);
                }
            }

            public override async Task CreatePaymentOnReadyForServerCompletionCallBack(string paymentId, string txid, int retries = 0)
            {
                try
                {
                    string key = $"CreatePaymentOnReadyForServerCompletionCallBack-{paymentId}";

                    if (!this.cache.TryGetValue(key, out bool exists))
                    {
                        this.cache.Set(key, true, TimeSpan.FromSeconds(60));

                        if (this.logger is { })
                            this.logger.LogInformation("Method: {@Method}. PaymentId: {@PaymentId}. TxId: {@TxId}", nameof(CreatePaymentOnReadyForServerCompletionCallBack), paymentId, txid);

                        var result = await this.server.PaymentComplete(paymentId, txid);

                        if (this.logger is { })
                            this.logger.LogInformation("Method: {@Method}. Result: {@Result}", nameof(CreatePaymentOnReadyForServerCompletionCallBack), result);

                        await this.orderServices.SavePaymentStatus(result.Metadata.OrderId, Enums.PaymentStatus.AdditionalSuccess, txid);
                        await OrderStateChange();

                        var orderOriginal = await this.orderServices.GetAsync(result.Metadata.OrderId);
                        var userOriginal = await this.userInfoService.GetAsync(orderOriginal.UserInfoId);

                        await this.mailingService.OnOrderFilledEmailAsync(userOriginal.Email, orderOriginal, orderOriginal.Id);

                        this.navigationManager.NavigateTo($"myorders/{result.Metadata.OrderId}", forceLoad: true);
                    }
                }
                catch (Exception e)
                {
                    if (e is HttpRequestException)
                    {
                        string textToSearch = "NotFound";
                        if (CreatePaymentExceptionMessageValidation(e, textToSearch) && retries <= base.Retries)
                        {
                            await Task.Delay(base.RetryDelay);
                            await CreatePaymentOnReadyForServerCompletionCallBack(paymentId, txid, retries + 1);
                        }
                        else
                            await CreatePaymentExceptionProccess(e, textToSearch);
                    }

                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. PaymentId: {@PaymentId} TxId: {@TxId}", nameof(CreatePaymentOnReadyForServerCompletionCallBack), paymentId, txid);

                    this.navigationManager.NavigateTo($"/", forceLoad: true);
                }
            }

            public override async Task CreatePaymentOnCancelCallBack(string paymentId)
            {
                string key = $"CreatePaymentOnCancelCallBack-{paymentId}";

                try
                {
                    if (!this.cache.TryGetValue(key, out bool exists))
                    {
                        this.cache.Set(key, true, TimeSpan.FromSeconds(60));

                        if (this.logger is { })
                            this.logger.LogInformation("Method: {@Method}. PaymentId: {@PaymentId}", nameof(CreatePaymentOnCancelCallBack), paymentId);
                        await OrderStateChange();

                        this.navigationManager.NavigateTo($"/", forceLoad: true);
                    }
                }
                catch (Exception e)
                {
                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. PaymentId: {@PaymentId}", nameof(CreatePaymentOnCancelCallBack), paymentId);
                    throw;
                }
            }

            public override async Task CreatePaymentOnErrorCallBack(string paymentId, string txId)
            {
                string key = $"CreatePaymentOnErrorCallBack-{paymentId}";

                try
                {
                    if (!this.cache.TryGetValue(key, out bool exists))
                    {
                        this.cache.Set(key, true, TimeSpan.FromSeconds(60));

                        if (this.logger is { })
                            this.logger.LogWarning("Method: {@Method}. Id: {@Id} TxId: {@TxId}", nameof(CreatePaymentOnErrorCallBack), paymentId, txId);

                        if (!string.IsNullOrEmpty(paymentId) && !string.IsNullOrEmpty(txId))
                        {
                            await CreatePaymentOnReadyForServerCompletionCallBack(paymentId, txId);
                        }
                        else
                        {
                            await OrderStateChange();
                        }

                        await this.sessionStorage.SetItemAsync(ConstantsEnums.PiNetworkSdkCallBackError, Messages.PaymentError);

                        this.navigationManager.NavigateTo($"/", forceLoad: false);
                    }
                }
                catch (Exception e)
                {
                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. PaymentId: {@PaymentId}", nameof(CreatePaymentOnErrorCallBack), paymentId);
                    throw;
                }
            }

            private async Task OrderStateChange()
            {
                try
                {
                    var orderState = new OrderState();
                    await this.sessionStorage.SetItemAsync(Constants.SessionOrderState, orderState);
                    await this.sessionStorage.SetItemAsync(Constants.SessionBasketCounter, 0);
                }
                catch (Exception e)
                {
                    if (this.logger is { })
                        this.logger.LogErrorMy(e, "Method: {@Method}. ", nameof(OrderStateChange));
                    throw;
                }
            }
        }
    }