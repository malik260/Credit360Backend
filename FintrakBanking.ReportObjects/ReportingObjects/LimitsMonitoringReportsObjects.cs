using EmailMessageLogger.Enum;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public  class LimitsMonitoringReportsObjects
    {
        FinTrakBankingContext context = new FinTrakBankingContext();
        public IEnumerable<SectorLimitViewModel> GetSectorLoanAmountLimit(int companyId, int operationId)
        {
            
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId ).FirstOrDefault();

                var output = (

                    from B in context.TBL_SUB_SECTOR
                    select new SectorLimitViewModel()
                    {
                        companyLogo = company.LOGOPATH ,
                        companyName = company.NAME,
                        sectorcode = B.CODE,
                        sectorName = B.TBL_SECTOR.NAME,
                        subsectorName = (B.NAME ?? "NOT DEFINED"),
                        subsectorCode = (B.CODE ?? "NOT DEFINED"),
                        limitMaximumValue = ((System.Decimal?)((Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) > 0 ? (System.Decimal?)
                        ((from D in context.TBL_LIMIT_DETAIL
                          where D.LIMITTYPEID == 2 && D.TARGETID == (Int32)B.SUBSECTORID
                          select new { D.MAXIMUMVALUE }).FirstOrDefault().MAXIMUMVALUE) : (Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) == 0 ? (System.Decimal?)0 : null) ?? (System.Decimal?)0),
                        usage = ((System.Decimal?)((Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) > 0 ? (System.Decimal?)
                        (from C in context.TBL_LOAN
                         where C.SUBSECTORID == B.SUBSECTORID
                         select new
                         { C.OUTSTANDINGPRINCIPAL }).Sum(p => p.OUTSTANDINGPRINCIPAL) : null) ?? (System.Decimal?)0)
                    }).ToList();


                //from Loan in context.tbl_Loan
                //           join LimitDetail in context.tbl_Limit_Detail
                //                 on new { SubSectorId = (int)Loan.SubSectorId, LimitTypeId =(int) LimitType.Sector }
                //             equals new { SubSectorId = LimitDetail.TargetId, LimitDetail.LimitTypeId } into LimitDetail_join
                //           from LimitDetail in LimitDetail_join.DefaultIfEmpty()

                //           group new { Loan.tbl_Sub_Sector.tbl_Sector, Loan.tbl_Sub_Sector, LimitDetail, Loan } by new
                //           {
                //               SectorId = Loan.tbl_Sub_Sector.tbl_Sector.SectorId,
                //               Sector = Loan.tbl_Sub_Sector.tbl_Sector.Name,
                //               Subsector = Loan.tbl_Sub_Sector.Name,
                //               MaximumValue = LimitDetail.MaximumValue,
                //               CompanyName = Loan.tbl_Company.Name,
                //               subsectorCode = Loan.tbl_Sub_Sector.Code,
                //               sectorCode = Loan.tbl_Sub_Sector.tbl_Sector.Code
                //           } into groupedQ
                //           select new SectorLimitViewModel()
                //           {
                //               companyName = company.Name,
                //               Id = groupedQ.Key.SectorId,
                //               Code = groupedQ.Key.sectorCode,
                //               Name = groupedQ.Key.Sector,
                //               Limit = (decimal?) groupedQ.Key.MaximumValue ?? 0,
                //               Usage = (decimal?) groupedQ.Sum(i => i.Loan.OutstandingPrincipal) ?? 0,
                //              // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.Loan.OutstandingPrincipal)
                //           }).ToList();

                return output;
            }
        }

        public IEnumerable<SectorLimitViewModel> GetBranchLoanAmountLimit(int branchId,int companyId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();

                var output = (from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_BRANCH on a.TARGETID equals b.BRANCHID
                              join c in context.TBL_LOAN on a.TARGETID equals c.BRANCHID
                              where a.LIMITTYPEID == (int)LimitType.Sector && c.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  && a.TBL_LIMIT.TBL_LIMIT_METRIC.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount && c.COMPANYID == companyId && c.BRANCHID == branchId
                              group new { a, b, c } by new
                              {
                                  a.MAXIMUMVALUE,
                                  b.BRANCHNAME,
                                  b.BRANCHCODE,
                                  b.BRANCHID
                              } into groupedQ
                              select new  SectorLimitViewModel

                              {
                                  companyName = company.NAME,
                                   limitMaximumValue = groupedQ.Key.MAXIMUMVALUE,
                                   usage = groupedQ.Sum(p => p.c.OUTSTANDINGPRINCIPAL),
                                   sectorName = groupedQ.Key.BRANCHNAME,
                                   subsectorCode = groupedQ.Key.BRANCHCODE,
                                  // Id = groupedQ.Key.BranchId,
                                 // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.c.OutstandingPrincipal)
                                   
            }).ToList();

                return output;
            }
        }

        public List<LoanCovenantDetailViewModel> CovenantsApproachingDueDate(DateTime startDate , DateTime endDate)
        {
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where a.NEXTCOVENANTDATE >= startDate && a.NEXTCOVENANTDATE <= endDate 
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
                                                                 relationshipManager = d.FIRSTNAME + " " + d.LASTNAME,
                                                                 relationshipManagerId = d.STAFFID,
                                                                 managerEmail = d.EMAIL,
                                                                 relationshipOfficerId = d.STAFFID,
                                                                 relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                                                                 officerEmail = d.EMAIL,
                                                             }).ToList();
           
            return loanDetails;
        }

        public List<CollateralViewModel> CollateralPropertyApproachingRevaluation(DateTime startDate, DateTime endDate)
        {
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     where f.LASTVALUATIONDATE >= startDate && f.LASTVALUATIONDATE <= endDate
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
                                                         relationshipManagerEmail = c.EMAIL,
                                                     }).ToList();
            return loanDetails;
        }

        public List<LoanViewModel> SendAlertsForLoanNplMonitoring()
        {
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_APPLICATION
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                               join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                               join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                               //   where (object?)b.INT_PRUDENT_GUIDELINE_STATUSID != (object?)(int?)1
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                                   loanReferenceNumber = b.LOANREFERENCENUMBER,
                                                   bookingDate = b.BOOKINGDATE,
                                                   disburseDate = b.DISBURSEDATE,
                                                   nplDate = (DateTime?)b.NPLDATE.Value,
                                                   outstandingInterest = b.OUTSTANDINGINTEREST,
                                                   outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                                                   loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = b.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = b.TBL_STAFF1.FIRSTNAME + " " + b.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = b.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = b.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = b.TBL_STAFF.EMAIL
                                               }).ToList();
            
            return loanDetails;
        }

        public List<LoanViewModel> SelfLiquidatingLoanExpiry(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating && a.MATURITYDATE >=startDate && a.MATURITYDATE <= endDate
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   productName = b.PRODUCTNAME,
                                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                                               }).ToList();
            
            return loanDetails;
        }

        public List<LoanViewModel> OverDraftLoansAlmostDue(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   productName = b.PRODUCTNAME,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                                   overdraftLimit = a.OVERDRAFTLIMIT,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                                               }).ToList();
        
            return loanDetails;
        }

        public List<LoanViewModel> CASAwithPND(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where s.HASLIEN == true && (s.POSTNOSTATUSID == (int)LoanStatusEnum.Suspended || s.POSTNOSTATUSID == (int)LoanStatusEnum.Terminated)
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.PRINCIPALAMOUNT,
                                                   interestRate = a.INTERESTRATE,
                                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME
                                               }).ToList();
            
            return loanDetails;
        }

        public List<LoanViewModel> ActiveBondAndGuarantee(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                               }).ToList();
            
            return loanDetails;
        }

        public List<LoanViewModel> ExpiredActiveBondAndGuarantee(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.ISTENORED == false && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME
                                               }).ToList();
            
            return loanDetails;
        }

        public List<LoanViewModel> AccountWithExeption_Overdrawn(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.MATURITYDATE >=startDate && a.MATURITYDATE <= endDate && s.AVAILABLEBALANCE < 0
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                               }).ToList();

           
            return loanDetails;
        }
        public List<LoanViewModel> AccountWithExeption_Watchist(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.INT_PRUDENT_GUIDELINE_STATUSID == (int)LoanPrudentialStatusEnum.WatchList
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = (decimal)s.OVERDRAFTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                               }).ToList();

           
            return loanDetails;
        }
        public List<LoanViewModel> AccountWithExeption_Unauthorized(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.MATURITYDATE  >= startDate && a.MATURITYDATE >= endDate && s.AVAILABLEBALANCE < 0m
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = (decimal)s.OVERDRAFTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                               }).ToList();
         
            return loanDetails;
        }

        public List<LoanViewModel> PastDueObligationAccounts(DateTime startDate, DateTime endDate)
        {

            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && s.AVAILABLEBALANCE < 0m
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                               }).ToList();
          
            return loanDetails;
        }

        public List<CollateralViewModel> InsuranceApprochingExpiration(DateTime startDate, DateTime endDate)
        {

            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where p.ENDDATE >= startDate && p.ENDDATE <= endDate
                                                     select new CollateralViewModel
                                                     {
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastValuationDate = f.LASTVALUATIONDATE,
                                                         collateralValue = (decimal?)a.COLLATERALVALUE,
                                                         valuationCycle = a.VALUATIONCYCLE,
                                                         insuranceCompany = p.INSURANCECOMPANYNAME,
                                                         startDate = (DateTime?)p.STARTDATE,
                                                         endDate = (DateTime?)p.ENDDATE,
                                                     }).ToList();
         
            return loanDetails;
        }

        public List<CollateralViewModel> ExpiredInsurance(DateTime startDate, DateTime endDate)
        {

            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where p.ENDDATE >= startDate && p.ENDDATE <= endDate && p.HASEXPIRED == true
                                                     select new CollateralViewModel
                                                     {
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastValuationDate = f.LASTVALUATIONDATE,
                                                         collateralValue = (decimal?)a.COLLATERALVALUE,
                                                         valuationCycle = a.VALUATIONCYCLE,
                                                         insuranceCompany = p.INSURANCECOMPANYNAME,
                                                         startDate = (DateTime?)p.STARTDATE,
                                                         endDate = (DateTime?)p.ENDDATE,
                                                     }).ToList();
            return loanDetails;
        }

        public List<LoanCovenantDetailViewModel> TurnoverCovenant(DateTime startDate, DateTime endDate)
        {

            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join ca in context.TBL_CASA on b.CASAACCOUNTID equals ca.CASAACCOUNTID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where a.NEXTCOVENANTDATE >= startDate && a.NEXTCOVENANTDATE   <= endDate  && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
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
           
            return loanDetails;
        }


    }
}
