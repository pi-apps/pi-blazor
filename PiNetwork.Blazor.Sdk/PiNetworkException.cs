using System;

namespace PiNetwork.Blazor.Sdk;

public sealed class PiNetworkException : Exception
{
    public PiNetworkException(string message) : base(message)

    {
    }
}