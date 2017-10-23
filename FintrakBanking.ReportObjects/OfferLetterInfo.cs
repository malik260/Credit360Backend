using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;
using System.Collections.Generic;
using System;

namespace FintrakBanking.ReportObjects
{
    public class OfferLetterInfo
    {
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var offerLetterDetails = (from a in context.tbl_Loan_Application
                                      where a.ApplicationReferenceNumber == applicationRefNumber &&
                                      a.ApprovalStatusId == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.tbl_Company.FirstOrDefault(x => x.CompanyId == a.CompanyId).Name,
                                          customerId = (int)a.CustomerId,
                                          customerName = a.tbl_Customer.Title + " " + a.tbl_Customer.FirstName + " " + a.tbl_Customer.LastName,
                                          customerGroupName = a.tbl_Customer_Group.GroupName + " - " + a.tbl_Customer_Group.GroupCode,
                                          customerAddress = a.tbl_Customer.tbl_Customer_Address.FirstOrDefault().Address ?? string.Empty,
                                          applicationDate = a.ApplicationDate
                                      }).FirstOrDefault();

            if (offerLetterDetails != null)
            {
                return offerLetterDetails;
            }

            return new OfferLetterViewModel();
        }

        public static List<OfferLetterDetailViewModel> GetLoanApplicationDetail(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            try
            {
                var loanDetails = (from a in context.tbl_Loan_Application
                                   join b in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals b.LoanApplicationId
                                   where a.ApplicationReferenceNumber.ToLower() == applicationRefNumber.ToLower() &&
                                         b.StatusId == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.tbl_Product.FirstOrDefault(x => x.ProductId == b.ApprovedProductId).ProductName,
                                       customerName = a.tbl_Customer.FirstName + " " + b.tbl_Customer.LastName,
                                       customerGroupName = a.tbl_Customer_Group.GroupName + " - " + a.tbl_Customer_Group.GroupCode,
                                       currencyName = b.tbl_Currency.CurrencyName,
                                       tenor = b.ApprovedTenor,
                                       interestRate = b.ApprovedInterestRate,
                                       loanAmount = b.ApprovedAmount,
                                       exchangeRate = b.ExchangeRate
                                   }).ToList();

                if (loanDetails != null)
                {
                    return loanDetails;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new List<OfferLetterDetailViewModel>();


        }

        public static List<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionPrecedent = (from a in context.tbl_Loan_Application
                                      join b in context.tbl_Loan_Condition_Precedent on a.LoanApplicationId equals b.LoanApplicationId
                                      where a.ApplicationReferenceNumber == applicationRefNumber && b.IsExternal == true
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