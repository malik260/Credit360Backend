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
        bool Init(int operationId, int targetId, bool isDrawdwon = false);
        string Replace(string content);

        string GetDrawdownMemoHtml(int staffId, int operationId, int targetId);
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
                
    }
}
