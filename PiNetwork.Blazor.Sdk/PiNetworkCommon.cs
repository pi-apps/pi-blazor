using System;

namespace PiNetwork.Blazor.Sdk.Common;

public enum Messages
{
    AuthenticationError = 0,
    PaymentError = 1,
    AuthenticationSuccess = 2,
    PaymentSuccess = 3
}

public sealed class PiNetworkConstants
{
    public const string PiNetworkSdkCallBackError = "PiNetworkSdkCallBackError";
    public const string PiNetworkDoNotRedirect = "PiNetworkDoNotRedirect";
    public const string IsPiNetworkBrowser = "IsPiNetworkBrowser";
    public const string PiNetworkClient = "PiNetworkClient";
    public const string PiNetworkClientFallback = "PiNetworkClientFallback";
    public const string PiTestnet = "Pi Testnet";
    public const string PiNetwork = "Pi Network";
}

public sealed class PiNetworkException : Exception
{
    public PiNetworkException(string message) : base(message)

    {
    }
}