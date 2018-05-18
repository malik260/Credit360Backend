using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Credit;
using System.Linq;
using FintrakBanking.Entities.Models;
using System.Collections.Generic;
using System;
using FintrakBanking.Common;
using System.IO;
using System.Web.Hosting;
using FintrakBanking.ViewModels.Setups.General;

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
                                      join e in context.TBL_CUSTOMER_ADDRESS on a.CUSTOMERID equals e.CUSTOMERID into dg
                                      from e in dg.DefaultIfEmpty()
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                      a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                          //customerId = b.CustomerId,
                                          customerName = a.LOANAPPLICATIONTYPEID != 3 ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME + " - " + c.GROUPCODE,
                                          customerGroupName = c.GROUPNAME + " - " + c.GROUPCODE,
                                          //customerAddress = e.ADDRESS ?? string.Empty, //a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                          applicationDate = a.APPLICATIONDATE
                                      }).FirstOrDefault();

            if (offerLetterDetails != null)
            {
                return offerLetterDetails;
            }
           
            return new OfferLetterViewModel();
        }


        public static List<SignatoryViewModel> GetLoanApplicationSignatory(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            try
            {
                var signatory = (from a in context.TBL_APPROVAL_TRAIL
                            join b in context.TBL_STAFF on a.REQUESTSTAFFID equals b.STAFFID
                            join c in context.TBL_LOAN_APPLICATION on a.TARGETID equals c.LOANAPPLICATIONID
                            where c.APPLICATIONREFERENCENUMBER == applicationRefNumber && a.FROMAPPROVALLEVELID != null orderby(a.APPROVALTRAILID)
                            select new SignatoryViewModel()
                            {
                                staffName = b.LASTNAME + " " + b.FIRSTNAME + " " + b.MIDDLENAME,
                            }).Take(2).ToList();

                if (signatory != null)
                {
                    signatory[0].rmStaffName = signatory[0].staffName;
                    signatory[0].bmStaffName = signatory[1].staffName;
                    return signatory;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new List<SignatoryViewModel>();


        }

        public static List<LoanApplicationCollateralViewModel> GetLoanCollateral(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            try
            {
                var collateral  = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                       join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                       where b.APPLICATIONREFERENCENUMBER == applicationRefNumber
                       select new LoanApplicationCollateralViewModel
                       {
                           collateralDetail = x.COLLATERALDETAIL,
                           collateralValue = x.COLLATERALVALUE,
                           stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT
                       }).ToList();

                if (collateral != null)
                {
                    return collateral;
                }
            }

            catch (Exception ex)
            {
                throw ex;
            }

            return new List<LoanApplicationCollateralViewModel>();

        }


        public static List<ProductFeeViewModel> GetLoanApplicationFee (string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            try
            {
                var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                            join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                            where d.APPLICATIONREFERENCENUMBER == applicationRefNumber
                            select new ProductFeeViewModel()
                            {
                                feeName = c.CHARGEFEENAME,
                                rateValue = a.RECOMMENDED_FEERATEVALUE
                            }).ToList();

                if (fees != null)
                {
                    return fees;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new List<ProductFeeViewModel>();


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
                                   join e in context.TBL_CUSTOMER_ADDRESS on a.CUSTOMERID equals e.CUSTOMERID into dg
                                   from e in dg.DefaultIfEmpty()
                                   join g in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals g.CUSTOMERID into gg
                                   from g in gg.DefaultIfEmpty()
                                   where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                         b.STATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                       //customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       //customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                       currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                                       tenor = b.APPROVEDTENOR,
                                       interestRate = b.APPROVEDINTERESTRATE,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       exchangeRate = b.EXCHANGERATE,
                                       companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                       customerName = a.LOANAPPLICATIONTYPEID != 3 ? c.TITLE + " " + c.FIRSTNAME + " " + c.LASTNAME : d.GROUPNAME + " - " + d.GROUPCODE,
                                       customerAddress = e.ADDRESS ?? " ", //a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                       applicationDate = a.APPLICATIONDATE,
                                       customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                       customerEmailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
                                       customerPhoneNumber = g.PHONENUMBER,//a.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault().PHONENUMBER,
                                       loanApplicationId = applicationRefNumber,
                                       repaymentSchedule = b.REPAYMENTSCHEDULE ?? "Not applicable",
                                       repaymentTerms = b.REPAYMENTTERMS ?? "Not applicable",
                                       purpose = b.LOANPURPOSE,
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

        //public static List<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident(string applicationRefNumber)
        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionPrecedentData  = (from a in context.TBL_LOAN_APPLICATION
                                      join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                      join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false && b.ISEXTERNAL == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                          isExternal = b.ISEXTERNAL,
                                          productName = c.TBL_PRODUCT.PRODUCTNAME
                                      }).ToList();


            var forDebugging = conditionPrecedentData.ToList();
            return conditionPrecedentData;
        }

        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionSubsequent(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequentData = (from a in context.TBL_LOAN_APPLICATION
                                          join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                          join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == true && b.ISEXTERNAL == true
                                          select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).ToList();


            var forDebugging = conditionSubsequentData.ToList();
            return conditionSubsequentData;
        }

        public static OfferLetterTemplateViewModel PrepareOfferLetterTemplate(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var obligorDetails = (from a in context.TBL_LOAN_APPLICATION
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
                                      customerName = a.LOANAPPLICATIONTYPEID != 3 ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerGroupName = c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerAddress = a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                      customerEmailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
                                      customerPhoneNumber = a.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault().PHONENUMBER,
                                      applicationDate = a.APPLICATIONDATE
                                  }).FirstOrDefault();

            var applicant = obligorDetails.customerName;
            var applicantDetails = $"{obligorDetails.customerEmailAddress}. {obligorDetails.customerEmailAddress}. {obligorDetails.customerPhoneNumber}";
            var applicantBeneficiaryDetails = string.Empty;

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
                                   exchangeRate = b.EXCHANGERATE,
                                   loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                               }).ToList();

            var facilityType = loanDetails.FirstOrDefault().loanTypeName;

            var totalLoanAmount = $"{loanDetails.Sum(x => x.baseCurrencyLoanAmount):f}";

            var currency = loanDetails.FirstOrDefault().currencyName;

            var interestRate = loanDetails.Sum(x => x.interestRate);

            var facilityPurpose = "To obtain the loan for the purpose of business expansion";

            var facilityTenor = $"{loanDetails.Sum(x => x.tenor)} days";

            var loanDetailsTable = string.Empty;

            loanDetailsTable = "<table><tr><th>Product</th><th>Loan Amount</th><th>Tenor</th><th>Interest Rate</th></tr>";

            foreach (var item in loanDetails)
            {
                loanDetailsTable = loanDetailsTable +
                    $"<tr><td>{item.productName}</td><td>{item.loanAmount:f}</td><td>{item.tenor:f} days</td><td>{item.interestRate:f}</td></tr>";
            }

            var finalLoanDetails = loanDetailsTable + "</table>";

            var conditionPrecedent = (from a in context.TBL_LOAN_APPLICATION
                                      join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISEXTERNAL == true
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                      }).ToList();

            var conditions = string.Empty;
            conditions = "<table><tr>Conditions</tr>";

            foreach (var item in conditionPrecedent)
            {
                conditions = conditions +
                    $"<tr><td>{item.conditionPrecident}</td></tr>";
            }

           var  finalConditions = conditions + "</table>";


            var preparedTemplate = PopulateOfferLetterPlaceholders(applicant, applicantDetails,
                applicantBeneficiaryDetails, facilityType, totalLoanAmount, facilityPurpose, facilityTenor);

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
            body = body.Replace("{ConditionPrecedent}", conditionPrecendent);
            body = body.Replace("{InterestRate}", interestRate);

            return body;
        }
 
    }
}