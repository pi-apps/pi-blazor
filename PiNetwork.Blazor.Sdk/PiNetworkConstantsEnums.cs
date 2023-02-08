namespace PiNetwork.Blazor.Sdk.ConstantsEnums
{
    public enum Messages
    {
        AuthenticationError = 0,
        PaymentError = 1,
        AuthenticationSuccess = 2,
        PaymentSuccess = 3
    }

    public class ConstantsEnums
    {
        public const string PiNetworkSdkCallBackError = "PiNetworkSdkCallBackError";
        public const string PiNetworkDoNotRedirect = "PiNetworkDoNotRedirect";
        public const string IsPiNetworkBrowser = "IsPiNetworkBrowser";
    }
}