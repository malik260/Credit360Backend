using FintrakBanking.ViewModels.Finance;

namespace FintrakBanking.Interfaces.Finance
{
    public interface IFinanceTransactionRepository
    {
        CasaLienViewModel AddCollateralSearchLien(CasaLienViewModel model);
        CasaLienViewModel PostCollateralSearch(CasaLienViewModel model);

       string PostTransaction(FinanceTransactionViewModel transaction);
    }
}