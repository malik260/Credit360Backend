using FintrakBanking.ViewModels.ThridPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Admin;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IIntegrationWithFinacle
    {
        ResponseMessageViewModel OverDraftNormal(OverDraftNormalViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        //ResponseMessageViewModel OverDraftNormal(OverDraftNormalViewModel model);
        ResponseMessageViewModel OverDraftTopUp(OverDraftTopUpAndRenewViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        ResponseMessageViewModel OverDraftExtend(OverDraftExtendViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        ResponseMessageViewModel OverDraftRenew(OverDraftTopUpAndRenewViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);

        ResponseMessageViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        ResponseMessageViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);
        ResponseMessageViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model, TwoFactorAutheticationViewModel twoFADetails = null);

        bool GetExposePersonStatus(string customerCode);
        BVNCustomerDetailsViewModel BVNCustomerDetails(string customerCode);
        GLAccountDetailsViewModel ValidateGLNumber(string glNumber);
        TDAccountRecordViewModel ValidateTDAccountNumber(string teamDepositAccountNumber);
        PostingResult PostTransactions(List<FinanceTransactionViewModel> model);

        CasaBalanceViewModel GetCustomerAccountBalance(string customerAccoun);
        List<CustomerViewModels> GetCustomerByAccountsNumber(string customerAccount);
        List<CasaViewModel> GetCustomerAccountsBalanceByCustomerCode(string customerCode);

        List<CustomerTurnoverViewModel> GetCustomerAccountTurnover(string customerCode, int durationInMonths);
        List<CustomerTurnoverViewModel> GetCustomerAccountInterestTransactions(string customerCode, int durationInMonths);

        string GetGlAccountCode(int glAccountId, int currencyId, int branchId);
        CurrencyExchangeRateViewModel GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode);
        AccountCreationResponseMessageViewModel CreateForeignAccount(CreateAccountViewModel entity);
        bool ChangeOverDraftInterestRate(InterestRateInquiryViewModel model, string accountType, TwoFactorAutheticationViewModel twoFADetails = null);
        InterestRateInquiryViewModel GetInterestRateInquiry(string accountNumber, string accountType);
        bool AddCustomerAccounts(string customerCode);
        bool AddCustomerAccounts(int customerId, string customerCode);
        Users GetUserRoleFinacle(string staffCode);

    }
}
