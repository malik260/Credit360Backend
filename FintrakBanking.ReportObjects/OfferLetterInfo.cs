using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;

namespace FintrakBanking.ReportObjects
{
    public class OfferLetterInfo
    {
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var offerLetterDetails = (from a in context.tbl_Loan_Application
                                      join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                      join c in context.tbl_Credit_Appraisal_Memorandum on a.LoanApplicationId equals c.LoanApplicationId
                                      join d in context.tbl_Credit_Appraisal_Memorandum_Loan_Detail on c.AppraisalMemorandumId equals d.AppraisalMemorandumId
                                      join e in context.tbl_Loan_Condition_Precedent on a.LoanApplicationId equals e.LoanApplicationId into condPrec
                                      from e in condPrec.DefaultIfEmpty()
                                      where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() &&
                                      a.ApprovalStatusId == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.tbl_Company.FirstOrDefault(x => x.CompanyId == a.CompanyId).Name,
                                          customerId = (int)a.CustomerId,
                                          customerName = b.Title + " " + b.FirstName + " " + b.LastName,
                                          customerAddress = context.tbl_Customer_Address.FirstOrDefault(cAddr => cAddr.CustomerId == b.CustomerId).Address ?? string.Empty,
                                          loanAmount = d.PrincipalAmount,
                                          interestRate = d.InterestRate,
                                          tenor = d.Tenor,
                                          applicationDate = d.DateTimeCreated,
                                          condition = e.Condition
                                      }).FirstOrDefault();

            if (offerLetterDetails != null)
            {
                return offerLetterDetails;
            }

            return new OfferLetterViewModel();
        }
    }
}