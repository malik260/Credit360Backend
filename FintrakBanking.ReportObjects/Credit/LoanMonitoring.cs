using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.ReportObjects.Credit
{
    public class LoanMonitoring
    {
        public static IEnumerable<CollateralViewModel> CollateralPropertyRevaluation(int companyId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_COLLATERAL_CUSTOMER
                        join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                        join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                        join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                        join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                        join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f
                            .COLLATERALCUSTOMERID
                        where (DbFunctions.DiffDays(DbFunctions.AddDays(f.LASTVALUATIONDATE, a.VALUATIONCYCLE), applDate) <= 30)
                        && a.COMPANYID == companyId
                        select new CollateralViewModel
                        {
                            collateralTypeId = a.COLLATERALTYPEID,
                            collateralType = d.COLLATERALTYPENAME,
                            collateralCode = a.COLLATERALCODE,
                            collateralSubType = e.COLLATERALSUBTYPENAME,
                            customerName = b.FIRSTNAME + " " + b.LASTNAME,
                            propertyName = f.PROPERTYNAME,
                            lastValuationDate = f.LASTVALUATIONDATE,
                            relationshipManagerId = a.CREATEDBY,
                            relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                            relationshipManagerEmail = c.EMAIL
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<CollateralViewModel>();
        }

        public static IEnumerable<LoanCovenantDetailViewModel> CovenantsApproachingDueDate(int companyId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            var data = (from a in context.TBL_LOAN_COVENANT_DETAIL
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                        join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                        join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                        join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals f.FREQUENCYTYPEID
                        join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                        where DbFunctions.DiffDays(applDate, a.NEXTCOVENANTDATE) <= 10
                        && a.COMPANYID == companyId
                        select new LoanCovenantDetailViewModel
                        {
                            companyId = a.COMPANYID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE,
                            dueDate = a.NEXTCOVENANTDATE,
                            covenantDetail = a.COVENANTDETAIL,
                            covenantTypeId = a.COVENANTTYPEID,
                            covenantTypeName = g.COVENANTTYPENAME,
                            frequencyTypeId = a.FREQUENCYTYPEID,
                            frequencyTypeName = f.MODE,
                            loanId = a.LOANID,
                            loanRefNumber = e.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                            managerEmail = c.EMAIL,
                            relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                            officerEmail = d.EMAIL,
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanCovenantDetailViewModel>();
        }

        public static IEnumerable<LoanViewModel> NplLoanMonitoring(int companyId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                        where b.INT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                        && b.COMPANYID == companyId
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = b.LOANREFERENCENUMBER,
                            bookingDate = b.BOOKINGDATE,
                            disburseDate = b.DISBURSEDATE,
                            maturityDate = b.MATURITYDATE,
                            productName = b.TBL_PRODUCT.PRODUCTNAME,
                            nplDate = b.NPLDATE.Value,
                            outstandingInterest = b.OUTSTANDINGINTEREST,
                            outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                            productTypeName = b.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            loanTypeName = b.TBL_LOAN_TYPE.LOANTYPENAME,
                            relationshipManagerId = b.RELATIONSHIPOFFICERID,
                            relationshipManagerName = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                            relationshipManagerEmail = b.TBL_STAFF.EMAIL,
                            relationshipOfficerId = b.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = b.TBL_STAFF1.FIRSTNAME + " " + b.TBL_STAFF1.LASTNAME,
                            relationshipOfficerEmail = b.TBL_STAFF1.EMAIL
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanViewModel>();
        }

        public IEnumerable<LoanViewModel> SelfLiquidatingLoanExpiry(int companyId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_LOAN
                        join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                        join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating &&
                        DbFunctions.DiffDays(a.MATURITYDATE, applDate) <= 30
                        && a.COMPANYID == companyId
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            bookingDate = a.BOOKINGDATE,
                            disburseDate = a.DISBURSEDATE,
                            maturityDate = a.MATURITYDATE,
                            productName = b.PRODUCTNAME,
                            nplDate = a.NPLDATE,
                            outstandingInterest = a.OUTSTANDINGINTEREST,
                            outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            relationshipManagerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipManagerEmail = a.TBL_STAFF.EMAIL,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                            relationshipOfficerEmail = a.TBL_STAFF1.EMAIL
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanViewModel>();
        }

        public IEnumerable<CreditBereauViewModel> CreditBereauDetails(int companyId)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<LoanViewModel> ExpiredOverDraftLoans(int companyId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_LOAN_REVOLVING
                        join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                        join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan && a.COMPANYID == companyId
                        && DbFunctions.DiffDays(a.MATURITYDATE, applDate) <= 90
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            bookingDate = a.BOOKINGDATE,
                            disburseDate = a.DISBURSEDATE,
                            maturityDate = a.MATURITYDATE,
                            productName = b.PRODUCTNAME,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            overdraftLimit = a.OVERDRAFTLIMIT,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                            relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                        }).ToList();

            if (data != null)
            {
                return data;
            }

            return new List<LoanViewModel>();
        }
    }
}