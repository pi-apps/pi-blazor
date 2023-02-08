namespace PiNetwork.Blazor.Sdk
{
    public sealed class PiNetworkOptions
    {
        /// <summary>
        /// PiNetwork your application Apykey, must be provided
        /// </summary>
        public string ApyKey { get; set; }

        /// <summary>
        /// PiNetwrok BaseUrl for requests, must be provided
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Not yet supported. PiNetwrok BaseUrl fallback in case BaseUrl fails to response this url will be tried, it's optional
        /// </summary>
        public string BaseUrlFallback { get; set; }
    }
}