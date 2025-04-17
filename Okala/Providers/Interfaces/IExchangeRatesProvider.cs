namespace Okala.Providers.Interfaces
{
    public interface IExchangeRatesProvider
    {
        Task<Dictionary<string, decimal>> GetRatesAgainstEURAsync(string[] symbols);

    }
}
