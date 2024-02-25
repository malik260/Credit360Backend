using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IMemorandumRepository
    {
        bool InitForExceptionalLoans(int operationId, int targetId);
        bool InitRecoveryDate(int accreditedConsultantId, string referenceId);
        decimal getTotalLLLImpact();
        decimal GetApprovalAmount(bool isLMS = false);
        string GetCallMemoMarkup(int id);
        bool Init(int operationId, int targetId,bool showMccStamp = false, bool showBccStamp = false, bool isDrawdwon = false);
        bool InitGenericMemo(int operationId, int targetId, int targetIdForWorkFlow, int customerId);
        bool InitForThirdpartyLoans(int operationId, int targetId);
        string Replace(string content);
        string CashBackMemoMarkupHtml(int staffId, int operationId, int targetId);
        string GetDrawdownMemoHtml(int staffId, int targetId);
        //output document function
        string MemoMarkupHtml();
        string FacilityUpgradeSupportSchemeHtml();
        string InvoiceDiscountingHtml();
        string CashCollaterizedHtml();
        string TemporaryOverdraftHtml();
        string StaffCarLoansHtml();
        string StaffMortgageLoansHtml();
        string StaffPersonalLoanAGMHtml();
        string StaffPersonalLoanHtml();
        string DocumentationDeferralWaiverFormHtml(int staffId, int operationId, int targetId);
        string UpdateEsg(string body);
        string UpdateGreenRating(string body);

    }
}
