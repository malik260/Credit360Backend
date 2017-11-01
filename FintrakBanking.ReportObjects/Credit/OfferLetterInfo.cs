using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;
using System.Collections.Generic;
using System;

namespace FintrakBanking.ReportObjects.Credit
{
    public class OfferLetterInfo
    {
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var offerLetterDetails = (from a in context.TBL_LOAN_APPLICATION
                                      join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                      join b in context.TBL_CUSTOMER on d.CUSTOMERID equals b.CUSTOMERID into cc
                                      from b in cc.DefaultIfEmpty()
                                      join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID into cg
                                      from c in cg.DefaultIfEmpty()
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                      a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                          //customerId = b.CustomerId,
                                          customerName = a.LOANTYPEID != 3 ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME + " - " + c.GROUPCODE,
                                          customerGroupName = c.GROUPNAME + " - " + c.GROUPCODE,
                                          customerAddress = a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                          applicationDate = a.APPLICATIONDATE
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
                var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                                   join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc from c in cc.DefaultIfEmpty()
                                   join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg from d in cg.DefaultIfEmpty()
                                   where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower() &&
                                         b.STATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                       customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                       currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                                       tenor = b.APPROVEDTENOR,
                                       interestRate = b.APPROVEDINTERESTRATE,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       exchangeRate = b.EXCHANGERATE
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

            var conditionPrecedent = (from a in context.TBL_LOAN_APPLICATION
                                      join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISEXTERNAL == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.LOANAPPLICATIONID
                                      }).ToList();

            if (conditionPrecedent != null)
            {
                return conditionPrecedent;
            }
            return new List<OfferLetterConditionPrecidentViewModel>();
        }

    }
}