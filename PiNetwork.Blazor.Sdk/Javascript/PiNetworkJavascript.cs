using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PiNetwork.Blazor.Sdk.Javascript
{
    public sealed class PiNetworkJavascript
    {
        public static ValueTask Authenticate(IJSRuntime jsRuntime, DotNetObjectReference<Pages.PiNetworkMain> reference, string redirectUri, int retries)
            => jsRuntime.InvokeVoidAsync("PiNetworkBlazorSdk.Authenticate", reference, redirectUri, retries);

        public static ValueTask CreatePayment(IJSRuntime jsRuntime, DotNetObjectReference<Pages.PiNetworkMain> reference, decimal amount, string memo, int orderId)
            => jsRuntime.InvokeVoidAsync("PiNetworkBlazorSdk.CreatePayment", reference, amount, memo, orderId);

        public static ValueTask OpenShareDialog(IJSRuntime jsRuntime, string title, string message)
            => jsRuntime.InvokeVoidAsync("PiNetworkBlazorSdk.OpenShareDialog", title, message);

        public static ValueTask IsPiNetworkBrowser(IJSRuntime jsRuntime, DotNetObjectReference<Pages.PiNetworkMain> reference)
            => jsRuntime.InvokeVoidAsync("Browser.IsPiNetworkBrowser", reference);

        public static ValueTask<string> Test(IJSRuntime jsRuntime)
            => jsRuntime.InvokeAsync<string>("PiNetworkBlazorSdk.Test");
    }
}