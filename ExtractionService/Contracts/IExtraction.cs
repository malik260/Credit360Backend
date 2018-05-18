using System.Threading.Tasks;

namespace ExtractionService
{
    namespace Contracts
    {
        public interface IExtraction
        {
            Task CurrencyExchangeRateExtraction();
           // Task CustomerAccountBalances();
            Task CustomerAccountExtraction();
            Task ProductPricingExtraction();
            
        }
    }
}