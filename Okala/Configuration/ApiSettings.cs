namespace CryptoRateApp.Configuration
{
    public class CoinMarketCapSettings
    {
        public string ApiKey { get; set; }
    }

    public class ExchangeRatesSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
    }
}