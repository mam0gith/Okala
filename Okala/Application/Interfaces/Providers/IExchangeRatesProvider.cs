namespace Okala.Application.Interfaces.Providers
{
    public interface IExchangeRatesProvider
    {
        Task<Dictionary<string, decimal>> GetRatesAgainstEURAsync(string[] symbols);

    }
}
