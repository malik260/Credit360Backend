using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;
using System.Collections.Generic;

namespace FintrakBanking.ReportObjects
{
    public class OfferLetterInfo
    {
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var offerLetterDetails = (from a in context.tbl_Loan_Application
                                      join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                      join c in context.tbl_Customer_Address on b.CustomerId equals c.CustomerId
                                      where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() &&
                                      a.ApprovalStatusId == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.tbl_Company.FirstOrDefault(x => x.CompanyId == a.CompanyId).Name,
                                          customerId = (int)a.CustomerId,
                                          customerName = b.Title + " " + b.FirstName + " " + b.LastName,
                                          customerAddress = c.Address ?? string.Empty,
                                          applicationDate = a.ApplicationDate
                                      }).FirstOrDefault();

            if (offerLetterDetails != null)
            {
                return offerLetterDetails;
            }

            GetLoanApplicationDetail(applicationRefNumber);

            GetLoanApplicationConditionPrecident(applicationRefNumber);

            return new OfferLetterViewModel();
        }

        public static List<OfferLetterDetailViewModel> GetLoanApplicationDetail(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanDetails = (from a in context.tbl_Loan_Application
                               join b in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals b.LoanApplicationId
                               join c in context.tbl_Product on b.ApprovedProductId equals c.ProductId
                               join d in context.tbl_Customer on b.CustomerId equals d.CustomerId
                               where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() &&
                                     b.StatusId == (int)ApprovalStatusEnum.Approved
                               select new OfferLetterDetailViewModel()
                               {
                                   productName = c.ProductName,
                                   customerName = d.FirstName + ' ' + d.LastName,
                                   currencyName = b.tbl_Currency.CurrencyName,
                                   tenor = b.ApprovedTenor,
                                   interestRate = b.ApprovedInterestRate,
                                   loanAmount = b.ApprovedAmount,
                                   exchangeRate = (float)b.ExchangeRate
                               }).ToList();

            if (loanDetails != null)
            {
                return loanDetails;
            }

            return new List<OfferLetterDetailViewModel>();
        }

        public static List<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionPrecedent = (from a in context.tbl_Loan_Application
                                      join b in context.tbl_Loan_Condition_Precedent on a.LoanApplicationId equals b.LoanApplicationId
                                      where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() && b.IsExternal == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.Condition,
                                          loanApplicationId = b.LoanApplicationId
                                      }).ToList();

            if (conditionPrecedent != null)
            {
                return conditionPrecedent;
            }
            return new List<OfferLetterConditionPrecidentViewModel>();
        }

    }
}