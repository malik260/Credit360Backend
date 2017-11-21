using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;
using System.Collections.Generic;
using System;
using FintrakBanking.Common;
using System.IO;
using System.Web.Hosting;

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
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                   from c in cc.DefaultIfEmpty()
                                   join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                   from d in cg.DefaultIfEmpty()
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

        public static OfferLetterTemplateViewModel PrepareOfferLetterTemplate(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

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

            var customerName = offerLetterDetails.customerName;

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
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

            var totalLoanAmount = loanDetails.FirstOrDefault().baseCurrencyLoanAmount;

            var currency = loanDetails.FirstOrDefault().currencyName;

            var interestRate = loanDetails.Sum(x => x.interestRate);

            var loanDetailsTable = string.Empty;

            loanDetailsTable = "<table><tr><th>Product</th><th>Loan Amount</th><th>Tenor</th><th>Interest Rate</th></tr>";

            foreach (var item in loanDetails)
            {

                loanDetailsTable = loanDetailsTable +
                    $"<tr><td>{item.productName}</td><td>{item.loanAmount:f}</td><td>{item.tenor:f} days</td><td>{item.interestRate:f}</td></tr>";
            }

            var finalLoanDetails = loanDetailsTable + "</table>";

            var conditionPrecedent = (from a in context.TBL_LOAN_APPLICATION
                                      join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISEXTERNAL == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.LOANAPPLICATIONID
                                      }).ToList();

            var conditions = string.Empty;

            conditions = "<table><tr>Conditions</tr>";

            foreach (var item in conditionPrecedent)
            {
                conditions = conditions +
                    $"<tr><td>{item.conditionPrecident}</td></tr>";
            }

            var finalConditions = conditions + "</table>";

            var preparedTemplate = PopulateOfferLetterPlaceholders(applDate.ToShortDateString(), customerName,
                totalLoanAmount.ToString(), currency, finalLoanDetails, finalConditions, interestRate.ToString());

            if (preparedTemplate != null)
            {
                return new OfferLetterTemplateViewModel { documentTemplate = preparedTemplate };

            }

            return new OfferLetterTemplateViewModel { };
        }

        private static string PopulateOfferLetterPlaceholders(string applDate, string obligorName, string loanAmount, string currency, string loanDetails, string conditionPrecendent, string interestRate)
        {
            string body;

            string templateLink = "~/EmailTemplates/OfferLetter.html";

            using (var reader = new StreamReader(HostingEnvironment.MapPath(templateLink) ?? throw new InvalidOperationException()))
            {
                body = reader.ReadToEnd();
            }

            body = body.Replace("{CurrentApplDate}", applDate);
            body = body.Replace("{ObligorName}", obligorName);
            body = body.Replace("{TotalLoanAmount}", loanAmount);
            body = body.Replace("{Currency}", currency);
            body = body.Replace("{ConditionPrecedent}", loanDetails);
            body = body.Replace("{InterestRate}", interestRate);

            return body;
        }

    }
}