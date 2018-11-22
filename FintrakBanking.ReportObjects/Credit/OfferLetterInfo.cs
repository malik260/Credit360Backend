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

           
                var customerExist = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).CUSTOMERID;

                var offerLetterDetails = (from a in context.TBL_LOAN_APPLICATION
                                          join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                          join b in context.TBL_CUSTOMER on d.CUSTOMERID equals b.CUSTOMERID into cc
                                          from b in cc.DefaultIfEmpty()
                                          join c in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals c.CUSTOMERGROUPID into cg
                                          from c in cg.DefaultIfEmpty()
                                          join e in context.TBL_CUSTOMER_ADDRESS on d.CUSTOMERID equals e.CUSTOMERID into dg
                                          from e in dg.DefaultIfEmpty()
                                          join g in context.TBL_CUSTOMER_PHONECONTACT on d.CUSTOMERID equals g.CUSTOMERID into gg
                                          from g in gg.DefaultIfEmpty()
                                          join h in context.TBL_OFFERLETTER on a.APPLICATIONREFERENCENUMBER equals h.APPLICATIONREFERENCENUMBER into hh
                                          from h in hh.DefaultIfEmpty()
                                              //join i in context.TBL_CUSTOMER_GROUP_MAPPING on b.CUSTOMERID equals i.CUSTOMERID into ii
                                              //from i in ii.DefaultIfEmpty()
                                              //join j in context.TBL_CUSTOMER_GROUP_MAPPING on c.CUSTOMERGROUPID equals j.CUSTOMERGROUPID into jj
                                              //from j in jj.DefaultIfEmpty()
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                      a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      && d.STATUSID == (int)ApprovalStatusEnum.Approved
                                          select new OfferLetterViewModel
                                          {
                                              companyName = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == a.COMPANYID).NAME,
                                              customerName = customerExist != null ? b.TITLE + " " + b.FIRSTNAME + " " + b.LASTNAME : c.GROUPNAME,
                                              customerAddress = e.ADDRESS ?? " ",
                                              customerEmailAddress = b.EMAILADDRESS,
                                              customerPhoneNumber = g.PHONENUMBER,
                                              isFinal = h.ISFINAL,
                                              producyClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                                              loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,

                                          }).FirstOrDefault();

            var isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(x => x.APPLICATIONREFERENCENUMBER == applicationRefNumber).Any();
            if (isOfferLetterAvailable == true)
            {
                if (offerLetterDetails.producyClassProcessId == (int)ProductClassProcessEnum.ProductBased)
                {
                    offerLetterDetails.isFinal = true;
                }
            }
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
                            join e in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals e.PRODUCTID
                            where d.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                  //&& b.STATUSID == (int)ApprovalStatusEnum.Approved && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                  && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                            && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                             && b.STATUSID == (int)ApprovalStatusEnum.Approved
                            select new ProductFeeViewModel()
                            {
                                feeName = c.CHARGEFEENAME,
                                rateValue = a.RECOMMENDED_FEERATEVALUE,
                                productName = e.PRODUCTNAME
                            }).ToList();

                if (fees != null)
                {
                    return fees;
                }

                
                //var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                //            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                //            join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                //            join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                //            where d.APPLICATIONREFERENCENUMBER == applicationRefNumber
                //            && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                //            && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                //            select new ProductFeeViewModel()
                //            {
                //                feeName = c.CHARGEFEENAME,
                //                rateValue = a.RECOMMENDED_FEERATEVALUE
                //            }).ToList();

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
                                   join h in context.TBL_CURRENCY on b.CURRENCYID equals h.CURRENCYID into hh
                                   from h in hh.DefaultIfEmpty()
                                   where a.APPLICATIONREFERENCENUMBER == applicationRefNumber &&
                                         b.STATUSID == (int)ApprovalStatusEnum.Approved
                                   select new OfferLetterDetailViewModel()
                                   {
                                       productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                       //customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       //customerGroupName = d.GROUPNAME + " - " + d.GROUPCODE,
                                       currencyName = h.CURRENCYCODE,//b.TBL_CURRENCY.CURRENCYNAME,
                                       tenor = b.APPROVEDTENOR,
                                       interestRate = b.APPROVEDINTERESTRATE,
                                       loanAmount = b.APPROVEDAMOUNT,
                                       exchangeRate = b.EXCHANGERATE,
                                       currencyId = b.CURRENCYID,
                                       companyName = context.TBL_COMPANY.Where(x => x.COMPANYID == a.COMPANYID).Select(x=>x.NAME).FirstOrDefault(),
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
                                       productPriceIndex = b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x=>x.PRODUCTPRICEINDEXID==b.PRODUCTPRICEINDEXID).Select(x=>x.PRICEINDEXNAME).FirstOrDefault() : "",
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

            var conditionPrecedentData = (from a in context.TBL_LOAN_APPLICATION
                                          join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                          join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                          && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                          && (b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived 
                                          || b.CHECKLISTSTATUSID == null)
                                          && b.ISSUBSEQUENT == false && b.ISEXTERNAL == true
                                          && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                          select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

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
                                               && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                               && b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                               && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                           select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionPrecedentDeferralData = (from a in context.TBL_LOAN_APPLICATION
                                          join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                          join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                          join d in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals d.LOANCONDITIONID
                                          where a.APPLICATIONREFERENCENUMBER == applicationRefNumber && b.ISSUBSEQUENT == false && b.ISEXTERNAL == true
                                               && c.STATUSID == (int)ApprovalStatusEnum.Approved && b.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred 
                                               && d.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                               && b.CHECKLISTSTATUSID != (short)CheckListStatusEnum.Waived
                                               && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                                  select new OfferLetterConditionPrecidentViewModel()
                                          {
                                              conditionPrecident = b.CONDITION,
                                              loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                              isExternal = b.ISEXTERNAL,
                                              productName = c.TBL_PRODUCT.PRODUCTNAME
                                          }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();


            var forDebugging = conditionSubsequentData.ToList().Union(conditionPrecedentDeferralData.ToList());
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
                                       && d.STATUSID == (int)ApprovalStatusEnum.Approved
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
                                   productPriceIndex = b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == b.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

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
                                       && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

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

        #region FORM3800B LOS report
       public int count = 0;
        public List<CamProcessedLoanViewModel> Los_LoanDetail(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CURRENCY on b.CURRENCYID equals e.CURRENCYID

                               where a.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNumber.ToLower()
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                               && b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new CamProcessedLoanViewModel()
                               {
                                   
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   tenor = b.APPROVEDTENOR,
                                   interestRate = b.APPROVEDINTERESTRATE,
                                   purpose = b.LOANPURPOSE,
                                   applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                                   approvedAmountCurrency = e.CURRENCYNAME + " " + b.APPROVEDAMOUNT + " % p.a ",
                                   productPriceIndex = b.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(s => s.PRODUCTPRICEINDEXID == b.PRODUCTPRICEINDEXID).Select(s => s.PRICEINDEXNAME).FirstOrDefault() : "",
                                   approvedDate = a.APPROVEDDATE,
                                   newApplicationDate = a.APPLICATIONDATE,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                               }).ToList();

            return loanDetails;


        }

        public List<OfferLetterConditionPrecidentViewModel> Los_ConditionPrecedents(string applicationRefNumber)
        {
            count = 1;
            FinTrakBankingContext context = new FinTrakBankingContext();

            //var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
            //                           join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
            //                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
            //                           where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
            //                           && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
            //                           && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
            //                           && b.ISSUBSEQUENT == false
            //                           select new { b, c }).ToList();

            var conditionPrecedents = (from a in context.TBL_LOAN_APPLICATION
                                       join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                       join b in context.TBL_LOAN_CONDITION_PRECEDENT on c.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                       where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                       && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                       && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                       && (b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived || b.CHECKLISTSTATUSID == null)
                                       && b.ISSUBSEQUENT == false
                                       && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                       select new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionId = b.CONDITIONID,
                                           conditionPrecident = b.CONDITION,
                                           loanApplicationId = a.LOANAPPLICATIONID,
                                           isExternal = b.ISEXTERNAL,
                                           productName = c.TBL_PRODUCT.PRODUCTNAME
                                       }).Distinct().ToList();

            var externalCondition =  conditionPrecedents.Where(x => x.isExternal == true).Select(x=> new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = x.conditionPrecident,
                loanApplicationId = x.loanApplicationId,
                isExternal = x.isExternal,
                productName = x.productName,
                sortOrder = "A"
            }).ToList(); ;

           var internalCondition = conditionPrecedents.Where(x => x.isExternal == false).Select((x,index)=> new OfferLetterConditionPrecidentViewModel()
                                       {
                                           conditionPrecident = x.conditionPrecident,
                                           loanApplicationId = x.loanApplicationId,
                                           isExternal = x.isExternal,
                                           productName = x.productName,
               sortOrder = "B"
           }).ToList();

            return externalCondition.Union(internalCondition).ToList();
        }

        public List<OfferLetterConditionPrecidentViewModel> Internal_ConditionPrecedents(string applicationRefNumber)
        {
            return Los_ConditionPrecedents(applicationRefNumber).ToList();
           // return ConditionsPrecedents.Where(x => x.isExternal == false).ToList();
        }
        public List<OfferLetterConditionPrecidentViewModel> External_ConditionPrecedents(string applicationRefNumber)
        {
            var ConditionsPrecedents = Los_ConditionPrecedents(applicationRefNumber);
            return ConditionsPrecedents.Where(x => x.isExternal == true).ToList();
        }
        public List<OfferLetterConditionPrecidentViewModel> External_ConditionSubsequents(string applicationRefNumber)
        {
            var conditionSubsequents = Los_ConditionSubsequents(applicationRefNumber);
            return conditionSubsequents.Where(x => x.isExternal == true).ToList();
        }
        public List<OfferLetterConditionPrecidentViewModel> Internal_ConditionSubsequents(string applicationRefNumber)
        {
            return Los_ConditionSubsequents(applicationRefNumber).ToList();
           // return conditionSubsequents.Where(x => x.isExternal == false).ToList();
        }

       
        public List<OfferLetterConditionPrecidentViewModel> Los_ConditionSubsequents(string applicationRefNumber)
        {
            
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequents = (from a in context.TBL_LOAN_APPLICATION
                                        join c in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                        join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONID equals b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID
                                        where a.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                        && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                        && b.CHECKLISTSTATUSID != (int)CheckListStatusEnum.Waived
                                       && c.STATUSID == (int)ApprovalStatusEnum.Approved
                                        && b.ISSUBSEQUENT == true
                                        select new { b, c }).ToList();

            var externalCondition = conditionSubsequents.Where(x => x.b.ISEXTERNAL == true).Select(x => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = x.b.CONDITION,
                loanApplicationId = x.b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                isExternal = x.b.ISEXTERNAL,
                productName = x.c.TBL_PRODUCT.PRODUCTNAME
            }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList(); ;

            var internalCondition = conditionSubsequents.Where(x => x.b.ISEXTERNAL == false).Select((x, index) => new OfferLetterConditionPrecidentViewModel()
            {
                conditionPrecident = x.b.CONDITION,
                loanApplicationId = x.b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                isExternal = x.b.ISEXTERNAL,
                productName = x.c.TBL_PRODUCT.PRODUCTNAME
            }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            return externalCondition.Union(internalCondition).ToList();
        }


        public List<ProductFeeViewModel> Los_Fee(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        where d.APPLICATIONREFERENCENUMBER == applicationRefNumber
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        && d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        && b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new ProductFeeViewModel()
                        {
                            SN = +count,
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE,
                            productName=b.TBL_PRODUCT.PRODUCTNAME
                        }).ToList();

            return fees;
        }


        public List<TransactionDynamicsViewModel> Los_ConditionDynamics(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var transactionDynamicsDetails = (from a in context.TBL_LOAN_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                              //join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                                              //from c in cc.DefaultIfEmpty()
                                              //join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                                              //from d in cg.DefaultIfEmpty()
                                              where c.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                              && c.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                              && c.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                              && b.STATUSID == (int)ApprovalStatusEnum.Approved
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  SN = +count,
                                                  dynamics = a.DYNAMICS,
                                                  productName = b.TBL_PRODUCT.PRODUCTNAME
                                              }).Distinct().ToList();

            return transactionDynamicsDetails;
        }

        public List<LoanApplicationCollateralViewModel> Collateral(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var loanCollaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                                   join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                   where y.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                   && y.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                   select new LoanApplicationCollateralViewModel()
                                   {
                                       SN = +count,
                                       collateralDetail = x.COLLATERALDETAIL,
                                       collateralValue = x.COLLATERALVALUE,
                                       stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                                       facilityAmount = y.APPROVEDAMOUNT
                                   }).ToList();

            return loanCollaterals;
        }

        public List<MonitoringTriggersViewModel> Los_loanMonitoringTriggers(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var loanMonitoringTriggers = (from x in context.TBL_LOAN_APPLICATN_DETL_MTRIG
                                          join y in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals y.LOANAPPLICATIONDETAILID
                                          join z in context.TBL_LOAN_APPLICATION on y.LOANAPPLICATIONID equals z.LOANAPPLICATIONID
                                          where z.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                          && z.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                          && z.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                          && y.STATUSID == (int)ApprovalStatusEnum.Approved
                                          select new MonitoringTriggersViewModel()
                                          {
                                              SN = +count,
                                              monitoringTrigger = x.MONITORING_TRIGGER,
                                              productName = y.TBL_PRODUCT.PRODUCTNAME
                                          }).Distinct().ToList();

            return loanMonitoringTriggers;
        }

        public List<LoanApplicationCommentViewModel> LoanComments(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var loanComments = (from x in context.TBL_LOAN_APPLICATION_COMMENT
                                join y in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals y.LOANAPPLICATIONID
                                where y.APPLICATIONREFERENCENUMBER == applicationRefNumber && x.OPERATIONID == (int)CommentsTypeEnum.LOS
                                select new LoanApplicationCommentViewModel()
                                {
                                    SN = +count,
                                    comments = x.COMMENTS,
                                }).ToList();

            return loanComments;


        }

        public List<CusotmerInfoViewModel> Los_Customer(string applicationRefNumber)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var loanComments = (from x in context.TBL_LOAN_APPLICATION
                                join y in context.TBL_CUSTOMER on x.CUSTOMERID equals y.CUSTOMERID
                                join b in context.TBL_BRANCH on y.BRANCHID equals b.BRANCHID
                                where x.APPLICATIONREFERENCENUMBER == applicationRefNumber
                                select new CusotmerInfoViewModel()
                                {
                                    customer = y.LASTNAME + " " + y.FIRSTNAME + " " + y.MIDDLENAME,
                                    date = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                                    branch = b.BRANCHNAME
                                }).ToList();

            return loanComments;
        }

        #endregion


    }
}