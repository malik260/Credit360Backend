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
using FintrakBanking.Entities.StagingModels;

namespace FintrakBanking.ReportObjects.Credit
{
    public class OfferLetterInfoLMSR
    {
        public static OfferLetterViewModel GenerateOfferLetter(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            FinTrakBankingStagingContext staggingCon = new FinTrakBankingStagingContext();

            var customerExist = context.TBL_LMSR_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x => x.CUSTOMERID).FirstOrDefault();
            var loanDetail = context.TBL_LMSR_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).Select(x => x).FirstOrDefault();

            var offerLetterDetails = (from a in context.TBL_LMSR_APPLICATION
                                      join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                      join b in context.TBL_CUSTOMER on d.CUSTOMERID equals b.CUSTOMERID into cc
                                      from b in cc.DefaultIfEmpty()
                                      join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID into cg
                                      from c in cg.DefaultIfEmpty()
                                      join e in context.TBL_CUSTOMER_ADDRESS on d.CUSTOMERID equals e.CUSTOMERID into dg
                                      from e in dg.DefaultIfEmpty()
                                      join g in context.TBL_CUSTOMER_PHONECONTACT on d.CUSTOMERID equals g.CUSTOMERID into gg
                                      from g in gg.DefaultIfEmpty()
                                      join h in context.TBL_LOAN_OFFER_LETTER on a.LOANAPPLICATIONID equals h.LOANAPPLICATIONID into hh
                                      from h in hh.DefaultIfEmpty()
                                          //join i in context.TBL_CUSTOMER_GROUP_MAPPING on b.CUSTOMERID equals i.CUSTOMERID into ii
                                          //from i in ii.DefaultIfEmpty()
                                          //join j in context.TBL_CUSTOMER_GROUP_MAPPING on c.CUSTOMERGROUPID equals j.CUSTOMERGROUPID into jj
                                          //from j in jj.DefaultIfEmpty()
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                      && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                       && d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                      select new OfferLetterViewModel
                                      {
                                          companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                          customerName = customerExist != null ? context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerExist).Select(f => f.TITLE + " " + f.FIRSTNAME + " " + f.LASTNAME).FirstOrDefault() : c.GROUPNAME,
                                          customerAddress = customerExist != null ? context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == customerExist).Select(f => f.ADDRESS).FirstOrDefault() : e.ADDRESS ?? " ",
                                          loanApplicationId = a.LOANAPPLICATIONID,
                                          customerEmailAddress = customerExist != null ? context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerExist).Select(f => f.EMAILADDRESS).FirstOrDefault() : b.EMAILADDRESS,
                                          customerPhoneNumber = customerExist != null ? context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customerExist).Select(f => f.PHONENUMBER).FirstOrDefault() : g.PHONENUMBER,
                                          isFinal = h.ISFINAL.Equals(null) ? false : h.ISFINAL,
                                          customerId = b.CUSTOMERID,
                                          operationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == a.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                          ////  producyClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                                          ////customerName = customerExist != null ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME,
                                          ////offerLetterTitle = b.OFFERLETTERTITLE,
                                          ////offerLetterSalutation = b.OFFERLETTERSALUTATION,
                                          //// offerLetteracceptance = context.TBL_LOAN_OFFER_LETTER.Where(x => x.LOANAPPLICATIONID == a.LOANAPPLICATIONID).Select(x => x.OFFERLETTERACCEPTANCE).FirstOrDefault(),
                                          ////offerLetterClauses = context.TBL_LOAN_OFFER_LETTER.Where(x => x.LOANAPPLICATIONID == a.LOANAPPLICATIONID).Select(x => x.OFFERLETTERCLAUSES).FirstOrDefault(),


                                      }).FirstOrDefault();


            //if (offerLetterDetails.producyClassProcessId == (int)ProductClassProcessEnum.ProductBased)
            //{
            offerLetterDetails.isFinal = context.TBL_LOAN_OFFER_LETTER.Where(o => o.LOANAPPLICATIONID == loanDetail.LOANAPPLICATIONID).Select(o => o.ISFINAL).FirstOrDefault();
            offerLetterDetails.offerLetteracceptance = context.TBL_LOAN_OFFER_LETTER.Where(x => x.LOANAPPLICATIONID == loanDetail.LOANAPPLICATIONID).Select(x => x.OFFERLETTERACCEPTANCE).FirstOrDefault();
            offerLetterDetails.offerLetterClauses = context.TBL_LOAN_OFFER_LETTER.Where(x => x.LOANAPPLICATIONID == loanDetail.LOANAPPLICATIONID).Select(x => x.OFFERLETTERCLAUSES).FirstOrDefault();
            offerLetterDetails.offerLetterTitle = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == loanDetail.CUSTOMERID).Select(o => o.OFFERLETTERTITLE).FirstOrDefault();
            offerLetterDetails.offerLetterSalutation = context.TBL_CUSTOMER.Where(o => o.CUSTOMERID == loanDetail.CUSTOMERID).Select(o => o.OFFERLETTERSALUTATION).FirstOrDefault();

            //}

            //if (offerLetterDetails != null)
            //{
            //    return offerLetterDetails;
            //}

            return offerLetterDetails;
        }

        public static OfferLetterViewModel GetManagementPosition(int loanId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            //var managementPosition = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanId).Select(x=>x.MANAGEMENTPOSITION).FirstOrDefault();
            var managementPosition = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                                      where a.LOANAPPLICATIONID == loanId
                                      select new OfferLetterViewModel
                                      {
                                          managementPosition = a.MANAGEMENTPOSITION,
                                      }).FirstOrDefault();
            return managementPosition;
        }

        public static List<SignatoryViewModel> GetLoanApplicationSignatory(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            try
            {
                var signatory = (from a in context.TBL_APPROVAL_TRAIL
                                 join b in context.TBL_STAFF on a.REQUESTSTAFFID equals b.STAFFID
                                 join c in context.TBL_LMSR_APPLICATION on a.TARGETID equals c.LOANAPPLICATIONID
                                 where c.APPLICATIONREFERENCENUMBER == applicationRefNumber && a.FROMAPPROVALLEVELID != null
                                 // && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                 orderby (a.APPROVALTRAILID)
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
                var collateral = (from x in context.TBL_LMSR_APPLICATION_COLLATRL2
                                  join b in context.TBL_LMSR_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                  where b.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                   && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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

        public static List<ProductFeeViewModel> GetLoanApplicationFee(string applicationRefNumber)
        {
            if (applicationRefNumber == null) new List<ProductFeeViewModel>();

            FinTrakBankingContext context = new FinTrakBankingContext();
            var targetAppl = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber);

            try
            {
                var fees = (from a in context.TBL_LMSR_APPLICATION_DETL_FEE
                            join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                            join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                            join d in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                            where d.APPLICATIONREFERENCENUMBER == targetAppl.APPLICATIONREFERENCENUMBER
                            && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                            && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            select new ProductFeeViewModel()
                            {
                                productName = b.TBL_PRODUCT.PRODUCTNAME,
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

                var loanDetails = (from a in context.TBL_LMSR_APPLICATION
                                   join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                   join e in context.TBL_LOAN on b.LOANID equals e.TERMLOANID
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                   into cc
                                   from c in cc.DefaultIfEmpty()
                                   join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                                   into cg
                                   from d in cg.DefaultIfEmpty()
                                   join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                                   where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                                   && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                                   && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                       tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                       interestRate = e.INTERESTRATE,
                                       purpose = b.REVIEWDETAILS,
                                       applicationDate = a.APPLICATIONDATE,
                                       approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       approvedDate = a.APPROVEDDATE,
                                       customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                       //customerPhoneNumber = g.PHONENUMBER,
                                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                       newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                                       operarionId = (short)b.OPERATIONID,
                                   }).ToList();

                if (loanDetails.Count == 0)
                {
                    loanDetails = (from a in context.TBL_LMSR_APPLICATION
                                   join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                   join e in context.TBL_LOAN_CONTINGENT on b.LOANID equals e.CONTINGENTLOANID
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                   into cc
                                   from c in cc.DefaultIfEmpty()
                                   join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                                   into cg
                                   from d in cg.DefaultIfEmpty()
                                   join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                                   where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                                   && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                                   && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                       tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                       //interestRate = e.INTERESTRATE,
                                       purpose = b.REVIEWDETAILS,
                                       applicationDate = a.APPLICATIONDATE,
                                       approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       approvedDate = a.APPROVEDDATE,
                                       customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                       //customerPhoneNumber = g.PHONENUMBER,
                                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                       newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,//a.APPLICATIONDATE,
                                       operarionId = (short)b.OPERATIONID,
                                   }).ToList();
                }
                if (loanDetails.Count == 0)
                {
                    loanDetails = (from a in context.TBL_LMSR_APPLICATION
                                   join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                   join e in context.TBL_LOAN_REVOLVING on b.LOANID equals e.REVOLVINGLOANID
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                   into cc
                                   from c in cc.DefaultIfEmpty()
                                   join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                                   into cg
                                   from d in cg.DefaultIfEmpty()
                                   join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                                   where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                                   && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                                   && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                       tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                       interestRate = e.INTERESTRATE,
                                       purpose = b.REVIEWDETAILS,
                                       applicationDate = a.APPLICATIONDATE,
                                       approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       approvedDate = a.APPROVEDDATE,
                                       customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                       //customerPhoneNumber = g.PHONENUMBER,
                                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                       newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,//a.APPLICATIONDATE,
                                       operarionId = (short)b.OPERATIONID,
                                       offerLetterIntroduction = "",
                                       isRenewal = true,
                                       // productPriceIndex = b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == b.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

                                   }).ToList();
                }
                foreach (var x in loanDetails)
                {
                    if (CommonHelpers.GetRolloverOperations().Contains(x.operarionId))
                    {
                        x.offerLetterIntroduction = "We refer to your application for a Credit Facility and are pleased to advise approval of same under the following Terms and Conditions:";
                        x.isRenewal = true;
                    }
                    else if (CommonHelpers.GetRestructureOperations().Contains(x.operarionId))
                    {
                        x.offerLetterIntroduction = "We refer to your application for renewal/enhancement of your Credit Facilities and are pleased to advise approval of same under the following Terms and Conditions:";
                        x.isRenewal = false;
                    }

                }
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

        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionPrecident(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequentData = (from a in context.TBL_LMSR_APPLICATION
                                           join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                           where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false && b.ISEXTERNAL == true
                                           && (b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                                    || b.CHECKLISTSTATUSID == null)
                                 && b.ISSUBSEQUENT == false && b.ISEXTERNAL == true
                                          && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                           select new OfferLetterConditionPrecidentViewModel()
                                           {
                                               conditionPrecident = b.CONDITION,
                                               loanApplicationId = b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                               isExternal = b.ISEXTERNAL,
                                               productName = c.TBL_PRODUCT.PRODUCTNAME
                                           }).ToList();


            var forDebugging = conditionSubsequentData.ToList();
            return conditionSubsequentData;
        }

        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionSubsequent(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequentData = (from a in context.TBL_LMSR_APPLICATION
                                           join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                           where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                           && b.ISSUBSEQUENT == true && b.ISEXTERNAL == true
                                           && b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                           && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                           select new OfferLetterConditionPrecidentViewModel()
                                           {
                                               conditionPrecident = b.CONDITION,
                                               loanApplicationId = b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
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

            var obligorDetails = (from a in context.TBL_LMSR_APPLICATION
                                  join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                  join b in context.TBL_CUSTOMER on d.CUSTOMERID equals b.CUSTOMERID into cc
                                  from b in cc.DefaultIfEmpty()
                                  join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID into cg
                                  from c in cg.DefaultIfEmpty()
                                  where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                  // && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                  //   && d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                  select new OfferLetterViewModel
                                  {
                                      companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                      //customerId = b.CustomerId,
                                      //  customerName = a.LOANAPPLICATIONTYPEID != 3 ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerGroupName = c.GROUPNAME + " - " + c.GROUPCODE,
                                      customerAddress = a.TBL_CUSTOMER.TBL_CUSTOMER_ADDRESS.FirstOrDefault().ADDRESS ?? string.Empty,
                                      customerEmailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
                                      customerPhoneNumber = a.TBL_CUSTOMER.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault().PHONENUMBER,
                                      applicationDate = a.APPLICATIONDATE
                                  }).FirstOrDefault();

            var applicant = obligorDetails.customerName;
            var applicantDetails = $"{obligorDetails.customerEmailAddress}. {obligorDetails.customerEmailAddress}. {obligorDetails.customerPhoneNumber}";
            var applicantBeneficiaryDetails = string.Empty;

            var loanDetails = (from a in context.TBL_LMSR_APPLICATION
                               join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               //  && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                               select new OfferLetterDetailViewModel()
                               {
                                   productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == b.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                   // currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   loanAmount = b.APPROVEDAMOUNT,
                                   // exchangeRate = b.EXCHANGERATE,
                                   // loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                               }).ToList();

            var facilityType = loanDetails.Select(x => x.loanTypeName).FirstOrDefault();

            var totalLoanAmount = $"{loanDetails.Sum(x => x.baseCurrencyLoanAmount):f}";

            var currency = loanDetails.Select(x => x.currencyName).FirstOrDefault();

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

            var conditionPrecedent = (from a in context.TBL_LMSR_APPLICATION
                                      join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                      where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISEXTERNAL == true
                                      //  && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                      select new OfferLetterConditionPrecidentViewModel()
                                      {
                                          conditionPrecident = b.CONDITION,
                                          loanApplicationId = b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID
                                      }).ToList();

            var conditions = string.Empty;
            conditions = "<table><tr>Conditions</tr>";

            foreach (var item in conditionPrecedent)
            {
                conditions = conditions +
                    $"<tr><td>{item.conditionPrecident}</td></tr>";
            }

            var finalConditions = conditions + "</table>";


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

        #region FORM3800B MLSR report

        public List<CamProcessedLoanViewModel> Lmsr_LoanDetail(string applicationRefNumber)
        {

            FinTrakBankingContext context = new FinTrakBankingContext();
            var k = "";
            var loanDetails = (from a in context.TBL_LMSR_APPLICATION
                               join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join e in context.TBL_LOAN on b.LOANID equals e.TERMLOANID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                               into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                               into cg
                               from d in cg.DefaultIfEmpty()
                               join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                               && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                   interestRate = e.INTERESTRATE,
                                   purpose = b.REVIEWDETAILS,
                                   applicationDate = a.APPLICATIONDATE,
                                   approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   approvedAmount = b.APPROVEDAMOUNT,
                                   approvedDate = a.APPROVEDDATE,
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                   //customerPhoneNumber = g.PHONENUMBER,
                                   loanApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,//a.APPLICATIONDATE,
                                   currencyCode = g.CURRENCYNAME

                               }).ToList();

            if (loanDetails.Count == 0)
            {
                loanDetails = (from a in context.TBL_LMSR_APPLICATION
                               join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join e in context.TBL_LOAN_CONTINGENT on b.LOANID equals e.CONTINGENTLOANID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                               into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                               into cg
                               from d in cg.DefaultIfEmpty()
                               join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                               && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                                           //interestRate = e.INTERESTRATE,
                                   purpose = b.REVIEWDETAILS,
                                   applicationDate = a.APPLICATIONDATE,
                                   approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   //approvedAmount = b.APPROVEDAMOUNT
                                   approvedDate = a.APPROVEDDATE,
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                   //customerPhoneNumber = g.PHONENUMBER,
                                   loanApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,//a.APPLICATIONDATE,
                               }).ToList();
            }
            if (loanDetails.Count == 0)
            {
                loanDetails = (from a in context.TBL_LMSR_APPLICATION
                               join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join e in context.TBL_LOAN_REVOLVING on b.LOANID equals e.REVOLVINGLOANID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                               into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID
                               into cg
                               from d in cg.DefaultIfEmpty()
                               join g in context.TBL_CURRENCY on e.CURRENCYID equals g.CURRENCYID
                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && b.LOANSYSTEMTYPEID == e.LOANSYSTEMTYPEID
                               && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == e.PRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,//(int)(e.MATURITYDATE - e.EFFECTIVEDATE).TotalDays,
                                   interestRate = e.INTERESTRATE,
                                   purpose = b.REVIEWDETAILS,
                                   applicationDate = a.APPLICATIONDATE,
                                   approvedAmountCurrency = g.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   //approvedAmount = b.APPROVEDAMOUNT
                                   approvedDate = a.APPROVEDDATE,
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x => x.NAME).FirstOrDefault(),
                                   //customerPhoneNumber = g.PHONENUMBER,
                                   loanApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   newApplicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,//a.APPLICATIONDATE,
                               }).ToList();
            }



            foreach (var x in loanDetails)
            {
                var fees = Lms_Fee(x.loanApplicationDetailId);
                var feeVal = "";
                foreach (var fee in fees)
                {
                    feeVal = feeVal + "  " + fee.feeName + ": " + fee.rateValue + "% flat,";
                }

                x.interestRateAndFees = "Interest Rate: " + x.interestRate + "% p/a, " + feeVal;
                x.interestRateAndFees.Remove(x.interestRateAndFees.Length - 1);
            }

            return loanDetails;


        }

        private List<ProductFeeViewModel> Lms_Fee(int applicationDeatailId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var fees = (from a in context.TBL_LMSR_APPLICATION_DETL_FEE
                        join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        where b.LOANREVIEWAPPLICATIONID == applicationDeatailId
                        select new ProductFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE,
                            productName = b.TBL_PRODUCT.PRODUCTNAME
                        }).ToList();

            return fees;
        }

        public List<OfferLetterConditionPrecidentViewModel> Lmsr_ConditionPrecedents(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionPrecedents = (from a in context.TBL_LMSR_APPLICATION
                                       join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                       where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                       && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                       && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                       && (b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived || b.CHECKLISTSTATUSID == null)
                                       && b.ISSUBSEQUENT == false
                                       && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionId = b.CONDITIONID,
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = a.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME

                                       }).Distinct().ToList();


            var externalCondition = conditionPrecedents.Where(x => x.isExternal == true).Select(x => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = x.conditionPrecident,
                loanApplicationId = x.loanApplicationId,
                isExternal = x.isExternal,
                productName = x.productName,
                sortOrder = "A"
            }).ToList(); ;

            var internalCondition = conditionPrecedents.Where(x => x.isExternal == false).Select((x, index) => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = x.conditionPrecident,
                loanApplicationId = x.loanApplicationId,
                isExternal = x.isExternal,
                productName = x.productName,
                sortOrder = "B"
            }).ToList();

            return externalCondition.Union(internalCondition).ToList();
        }

        //public List<OfferLetterConditionPrecidentViewModel> Lmsr_Internal_ConditionPrecedents(string applicationRefNumber)
        //{
        //    var ConditionsPrecedents = Lmsr_ConditionPrecedents(applicationRefNumber);
        //    return ConditionsPrecedents.Where(x => x.isExternal == false).ToList();
        //}
        //public List<OfferLetterConditionPrecidentViewModel> Lmsr_External_ConditionPrecedents(string applicationRefNumber)
        //{
        //    var ConditionsPrecedents = Lmsr_ConditionPrecedents(applicationRefNumber);
        //    return ConditionsPrecedents.Where(x => x.isExternal == true).ToList();
        //}
        //public List<OfferLetterConditionPrecidentViewModel> Lmsr_External_ConditionSubsequents(string applicationRefNumber)
        //{
        //    var conditionSubsequents = Lmsr_ConditionSubsequents(applicationRefNumber);
        //    return conditionSubsequents.Where(x => x.isExternal == true).ToList();
        //}
        //public List<OfferLetterConditionPrecidentViewModel> Lmsr_Internal_ConditionSubsequents(string applicationRefNumber)
        //{
        //    var conditionSubsequents = Lmsr_ConditionSubsequents(applicationRefNumber);
        //    return conditionSubsequents.Where(x => x.isExternal == false).ToList();
        //}

        public List<OfferLetterConditionPrecidentViewModel> Lmsr_ConditionSubsequents(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequents = (from a in context.TBL_LMSR_APPLICATION
                                        join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                        && b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived
                                       && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                        && b.ISSUBSEQUENT == true
                                        select new { b, c }).ToList();

            var externalCondition = conditionSubsequents.Where(o => o.b.ISEXTERNAL == true).Select(o => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = o.b.CONDITION,
                loanApplicationId = o.b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                isExternal = o.b.ISEXTERNAL,
                productName = o.c.TBL_PRODUCT.PRODUCTNAME
            }).GroupBy(o => o.conditionPrecident).Select(y => y.FirstOrDefault()).ToList(); ;

            var internalCondition = conditionSubsequents.Where(o => o.b.ISEXTERNAL == false).Select((o, index) => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = o.b.CONDITION,
                loanApplicationId = o.b.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                isExternal = o.b.ISEXTERNAL,
                productName = o.c.TBL_PRODUCT.PRODUCTNAME
            }).GroupBy(o => o.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            return externalCondition.Union(internalCondition).ToList();



        }

        public List<TransactionDynamicsViewModel> Lmsr_ConditionDynamics(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var transactionDynamicsDetails = (from a in context.TBL_LMSR_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where b.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                              && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  dynamics = a.DYNAMICS,
                                              }).Distinct().ToList();


            return transactionDynamicsDetails;
        }

        public List<MonitoringTriggersViewModel> Lmsr_loanMonitoringTriggers(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanMonitoringTriggers = (from x in context.TBL_LMSR_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANREVIEWAPPLICATIONID equals y.LOANREVIEWAPPLICATIONID
                                          where y.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                           && y.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                          select new MonitoringTriggersViewModel()
                                          {
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                          }).Distinct().ToList();

            return loanMonitoringTriggers;
        }

        public List<LoanApplicationCollateralViewModel> Lmsr_Collateral(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanCollaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                                   join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                   where y.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                                       facilityAmount = y.APPROVEDAMOUNT
                                   }).ToList();

            return loanCollaterals;
        }

        public List<LoanApplicationCommentViewModel> Lmsr_LoanComments(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanComments = (from x in context.TBL_LOAN_APPLICATION_COMMENT
                                join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                where y.APPLICATIONREFERENCENUMBER == applicationRefNumber && x.OPERATIONID == (int)CommentsTypeEnum.LMS
                                select new LoanApplicationCommentViewModel()
                                {
                                    comments = x.COMMENTS,
                                }).ToList();

            return loanComments;
        }

        public List<CusotmerInfoViewModel> Lmsr_Customer(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            FinTrakBankingStagingContext staggingCon = new FinTrakBankingStagingContext();

            var customers = (from x in context.TBL_LMSR_APPLICATION
                             join y in context.TBL_CUSTOMER on x.CUSTOMERID equals y.CUSTOMERID
                             join b in context.TBL_BRANCH on y.BRANCHID equals b.BRANCHID
                             where x.APPLICATIONREFERENCENUMBER == applicationRefNumber
                             select new CusotmerInfoViewModel()
                             {
                                 customer = y.LASTNAME + " " + y.FIRSTNAME + " " + y.MIDDLENAME,
                                 date = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                                 branch = b.BRANCHNAME,
                                 rmId = x.CREATEDBY,

                             }).ToList();

            foreach (var x in customers)
            {
                var staffcode = context.TBL_STAFF.Where(o => o.STAFFID == x.rmId).Select(o => o.STAFFCODE).FirstOrDefault();
                //x.groupHead = staggingCon.STG_STAFFMIS.Where(m => m.USERNAME == staffcode).Select(m => m.GROUP_HUB).FirstOrDefault();
            }
            return customers;
        }

        public OfferLetterViewModel offerLetterClauses(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var clause = (from x in context.TBL_LOAN_APPLICATION
                          join y in context.TBL_LOAN_OFFER_LETTER on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                          where x.APPLICATIONREFERENCENUMBER == applicationRefNumber
                          select new OfferLetterViewModel
                          {
                              offerLetteracceptance = y.OFFERLETTERACCEPTANCE,
                              offerLetterClauses = y.OFFERLETTERCLAUSES
                          }).FirstOrDefault();
            return clause;
        }

        #endregion

    }
}
