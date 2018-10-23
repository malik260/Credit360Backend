using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.AlertMonitoring;
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
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();

                var output = (

                                from Loan in context.TBL_LOAN
                                join subSector in context.TBL_SUB_SECTOR
                                on Loan.SUBSECTORID equals subSector.SUBSECTORID into cc
                                from subSector in cc.DefaultIfEmpty()
                                join sector in context.TBL_SECTOR on subSector.SECTORID equals sector.SECTORID into dd 
                                from sector in dd.DefaultIfEmpty()
                                group Loan by new { sector.CODE, sector.NAME, sector.LOAN_LIMIT, SubSectorCode = subSector.CODE, SubSectorName = subSector.NAME } into groupedQ
                                select new SectorLimitViewModel()
                                {
                                    companyLogo = company.LOGOPATH,
                                    companyName = company.NAME,
                                    subsectorName = (groupedQ.Key.SubSectorName ?? "NOT DEFINED"),
                                    subsectorCode = (groupedQ.Key.SubSectorCode ?? "NOT DEFINED"),
                                    sectorcode = groupedQ.Key.CODE,
                                    sectorName = groupedQ.Key.NAME,
                                    limitMaximumValue = groupedQ.Key.LOAN_LIMIT ?? 0,
                                    usage = groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL),
                                }).ToList(); 

                return output;
            }
        }

        public IEnumerable<SectorLimitViewModel> GetBranchLoanAmountLimit(int branchId,int companyId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();


                var output = (

                from Loan in context.TBL_LOAN
                join branch in context.TBL_BRANCH
                on Loan.BRANCHID equals branch.BRANCHID into cc
                from branch in cc.DefaultIfEmpty()
                group Loan by new { branch.BRANCHCODE, branch.BRANCHNAME, branch.NPL_LIMIT} into groupedQ
                select new SectorLimitViewModel()
                {
                    companyLogo = company.LOGOPATH,
                    companyName = company.NAME,

                    subsectorCode = (groupedQ.Key.BRANCHCODE ?? "NOT DEFINED"),
                    sectorName = groupedQ.Key.BRANCHNAME,
                    limitMaximumValue = (decimal?)groupedQ.Key.NPL_LIMIT ?? 0,
                    usage = groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL),
                }).ToList();

                return output;
            }
        }

        public List<LoanCovenantDetailViewModel> CovenantsApproachingDueDate(DateTime startDate, DateTime endDate)
        {
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where a.NEXTCOVENANTDATE >= startDate && a.NEXTCOVENANTDATE<=endDate
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
        } //done

        public List<CollateralViewModel> CollateralPropertyRevaluation(DateTime startDate, DateTime endDate)
        {
            List<CollateralViewModel> PropertyRevaluation = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                                              join a in  context.TBL_COLLATERAL_CUSTOMER on f.COLLATERALCUSTOMERID equals a.COLLATERALCUSTOMERID
                                                    join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
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
                                                         valuationAmount = f.VALUATIONAMOUNT,
                                                        valuationCycle = e.REVALUATIONDURATION,
                                                        
                                                     }).ToList();

            return PropertyRevaluation;
        } //done

        public List<CollateralViewModel> CollateralPropertyDueForVisitation(DateTime startDate, DateTime endDate)
        {
            List<CollateralViewModel> PropertyDueForVisitation = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join v in context.TBL_COLLATERAL_VISITATION on a.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                                                     where v.VISITATIONDATE >= startDate && v.VISITATIONDATE <= endDate
                                                     && d.REQUIREVISITATION == true
                                                     select new CollateralViewModel
                                                     {
                                                         collateralTypeId = a.COLLATERALTYPEID,
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastVisitationDate = v.VISITATIONDATE,
                                                         nextVisitationDate = v.VISITATIONDATE,
                                                         visitationCycle = (int)e.VISITATIONCYCLE,
                                                         relationshipManagerId = a.CREATEDBY,
                                                         relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                                                         relationshipManagerEmail = c.EMAIL,
                                                     }).ToList();

            return PropertyDueForVisitation;
        } //to do

        public List<LoanCovenantDetailViewModel> TurnoverCovenant(DateTime startDate, DateTime endDate)
        {
            List<LoanCovenantDetailViewModel> turnoverCovenant = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join ca in context.TBL_CASA on b.CASAACCOUNTID equals ca.CASAACCOUNTID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where a.NEXTCOVENANTDATE >= startDate && a.NEXTCOVENANTDATE <= endDate && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
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
            return turnoverCovenant;
        } // to do

        public List<LoanViewModel> NPL(DateTime startDate, DateTime endDate, int classification)
        {
            if (classification!=null)
            {
                List<LoanViewModel> npl = (from a in context.TBL_LOAN_APPLICATION
                                           join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                           join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                           join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                           where b.EXT_PRUDENT_GUIDELINE_STATUSID == (int)PrudentialGuidelineTypeEnum.NonPerforming
                                           && b.BOOKINGDATE >= startDate && b.BOOKINGDATE <= endDate
                                           select new LoanViewModel
                                           {
                                               applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                               loanReferenceNumber = b.LOANREFERENCENUMBER,
                                               bookingDate = b.BOOKINGDATE,
                                               disburseDate = b.DISBURSEDATE,
                                               nplDate = (DateTime?)b.NPLDATE,
                                               outstandingInterest = b.OUTSTANDINGINTEREST,
                                               outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                                               loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                               externalPrudentialGuidelineStatus = b.TBL_LOAN_PRUDENTIALGUIDELINE.STATUSNAME,
                                               productName = d.TBL_PRODUCT.PRODUCTNAME
                                           }).ToList();
                return npl;
            }
            else
            {
                List<LoanViewModel> npl = (from a in context.TBL_LOAN_APPLICATION
                                           join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                           join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                           join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                           where b.INT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                           && b.BOOKINGDATE >= startDate && b.BOOKINGDATE <= endDate
                                           select new LoanViewModel
                                           {
                                               applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                               loanReferenceNumber = b.LOANREFERENCENUMBER,
                                               bookingDate = b.BOOKINGDATE,
                                               disburseDate = b.DISBURSEDATE,
                                               nplDate = (DateTime?)b.NPLDATE,
                                               outstandingInterest = b.OUTSTANDINGINTEREST,
                                               outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                                               loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                               externalPrudentialGuidelineStatus = b.TBL_LOAN_PRUDENTIALGUIDELINE.STATUSNAME,
                                               productName = d.TBL_PRODUCT.PRODUCTNAME
                                           }).ToList();
                return npl;
            }
          
            
        }  //done

        public List<LoanViewModel> SelfLiquidatingLoan(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> selfLiquidatingLoan = (from a in context.TBL_LOAN
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating &&
                                               a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
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
            return selfLiquidatingLoan;
        } //to do

        public List<LoanViewModel> OverDraft(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> overDraft = (from a in context.TBL_LOAN_REVOLVING
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan 
                                               && a.MATURITYDATE >= startDate && a.MATURITYDATE<= endDate
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

            return overDraft;
        } //done

        public List<LoanViewModel> BondAndGuarantee(DateTime startDate, DateTime endDate , int approvalStatus)
        {
            if (approvalStatus == (int)LoanStatusEnum.Expired)
            {
                List<LoanViewModel> bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                                        join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                        join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                        join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                        where a.ISTENORED == false && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                                        && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
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
                                                            loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS
                                                        }).ToList();
                return bondAndGuarantee;
            }
            else
            {

                List<LoanViewModel> bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                                        join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                        join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                        join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                        where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == approvalStatus
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
                                                            notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now)
                                                        }).ToList();
                return bondAndGuarantee;
            }
        } // to do

        public List<LoanViewModel> SendAlertOnAccountWithExeption_Overdrawn(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> overDue = (from a in context.TBL_LOAN_CONTINGENT
                                           join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                           join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                           join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                           where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                           &&  s.AVAILABLEBALANCE < 0m
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
            return overDue;
        }
        
        public List<LoanViewModel> PastDueObligationAccounts(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> pastDueObligation = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.MATURITYDATE >=startDate && a.MATURITYDATE <= endDate && s.AVAILABLEBALANCE < 0m
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
            return pastDueObligation;
        }

        public List<CollateralViewModel> InsuranceApprochingExpiration (DateTime startDate, DateTime endDate)
        {
            List<CollateralViewModel> insuranceApprochingExpiration = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where p.ENDDATE>= startDate && p.ENDDATE <=endDate
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
            return insuranceApprochingExpiration;
        } //to do

        public CollateralHistory getCollateralHistory(int collateralId)
        {
            var termLoanCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility),
                    c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
                .Join(context.TBL_LOAN, clc => clc.lc.LOANID, l => l.TERMLOANID, (clc, l) => new { clc, l }) // TBL_LOAN
                .Select(o => new CollateralHistoryList
                {
                    customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
                    loanRef = o.l.LOANREFERENCENUMBER,
                    expirationDate = o.l.MATURITYDATE,
                    collateralValue = o.clc.c.COLLATERALVALUE,
                    outstandingPrincipal = o.l.OUTSTANDINGPRINCIPAL,
                    runningPrincipal = o.l.PRINCIPALAMOUNT,
                    dateUsed = o.clc.lc.DATETIMECREATED,
                    haircut = o.clc.c.HAIRCUT,
                    exchangeRate = o.l.EXCHANGERATE,
                    approvedLoanAmount = o.l.PRINCIPALAMOUNT,
                });

            var odCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility),
                    c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
                .Join(context.TBL_LOAN_REVOLVING, clc => clc.lc.LOANID, l => l.REVOLVINGLOANID, (clc, l) => new { clc, l }) // TBL_LOAN_REVOLVING
                .Select(o => new CollateralHistoryList
                {
                    customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
                    loanRef = o.l.LOANREFERENCENUMBER,
                    expirationDate = o.l.MATURITYDATE,
                    collateralValue = o.clc.c.COLLATERALVALUE,
                    outstandingPrincipal = o.l.OVERDRAFTLIMIT,
                    runningPrincipal = o.l.OVERDRAFTLIMIT,
                    dateUsed = o.clc.lc.DATETIMECREATED,
                    haircut = o.clc.c.HAIRCUT,
                    exchangeRate = o.l.EXCHANGERATE,
                    approvedLoanAmount = o.l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                });

            var collaterals = new CollateralHistory();

            collaterals.usage = termLoanCollaterals.Union(odCollaterals);
            collaterals.totalAmountUsedByOutstanding = collaterals.usage.Sum(x => x.outstandingPrincipal);
            collaterals.totalAmountUsedByPrincipal = collaterals.usage.Sum(x => x.approvedLoanAmount);
            collaterals.collateralValue = collaterals.usage.Any() ? collaterals.usage.Max(x => x.collateralValue) : 0;
            collaterals.availableValueByPrincipal = collaterals.collateralValue - collaterals.totalAmountUsedByPrincipal;
            collaterals.availableValueByOutstanding = collaterals.collateralValue - collaterals.totalAmountUsedByOutstanding;

            /*
            var testL = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false).ToList();// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
            var test = termLoanCollaterals.Union(odCollaterals).ToList();
            var testTL = termLoanCollaterals.ToList();
            var testOD = odCollaterals.ToList();
            */

            return collaterals;
        }


        public IEnumerable<SLANotificationViewModel> SLAMonitoring(DateTime startDate, DateTime endDate, int approvalStatus,int operationId)
        {

            int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CAM, (int)OperationsEnum.ContigentLoanBooking ,
           (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
            (int)OperationsEnum.CommercialLoanBooking};

            var list = new List<SLANotificationViewModel>();
            var notificationList = from a in context.TBL_APPROVAL_TRAIL
                                   join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                                   join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
                                   join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                   where a.SYSTEMARRIVALDATETIME >= startDate && a.SYSTEMARRIVALDATETIME <= endDate 
                                   && ( a.APPROVALSTATUSID == approvalStatus || approvalStatus == 0)
                                   && (a.OPERATIONID == operationId || operationId ==0)
                                   
                                   //&& a.RESPONSESTAFFID == null
                                   //&& a.TOSTAFFID != null
                                   //&& b.SLAINTERVAL > 0

                                   select new SLANotificationViewModel
                                   {
                                       approvalTrailId = a.APPROVALTRAILID,
                                       arrivalDate = a.ARRIVALDATE,
                                       fromApprovalLevelId = a.FROMAPPROVALLEVELID,
                                       operationId = a.OPERATIONID,
                                       requestStaffId = a.REQUESTSTAFFID,
                                       salInterval = b.SLAINTERVAL,
                                       systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                       ressponseTime = a.SYSTEMRESPONSEDATETIME,
                                       systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                       targetId = a.TARGETID,
                                       toApprovalLevelId = a.TOAPPROVALLEVELID,
                                       toStaffId = a.TOSTAFFID,
                                       staffEmail = s.EMAIL,
                                       operationName = o.OPERATIONNAME,
                                       slaNotificationInterval = b.SLANOTIFICATIONINTERVAL,
                                       approvalStatusId = a.APPROVALSTATUSID
                                   };


            var data = new SLANotificationViewModel();

            foreach (var x in notificationList)
            {
                if (operations.Contains(x.operationId))
                {
                    data = new SLANotificationViewModel
                    {
                        approvalTrailId = x.approvalTrailId,
                        arrivalDate = x.arrivalDate,
                        fromApprovalLevelId = x.fromApprovalLevelId,
                        operationId = x.operationId,
                        requestStaffId = x.requestStaffId,
                        salDateLine = x.systemArrivalDate.AddHours(x.salInterval),
                        slaNotificationDate = x.systemArrivalDate.AddHours(x.slaNotificationInterval),
                        salInterval = x.salInterval,
                        systemArrivalDate = x.systemArrivalDate,
                        systemResponseDate = x.systemResponseDate,
                        targetId = x.targetId,
                        toApprovalLevelId = x.toApprovalLevelId,
                        toStaffId = x.toStaffId,
                        staffEmail = x.staffEmail,
                        operationName = x.operationName,
                        responseDefaultTime = x.systemResponseDate == null ? 0 : x.systemArrivalDate.AddHours(x.salInterval).Subtract(x.systemResponseDate.Value).TotalHours,
                        // responseDefaultTime = x.salDateLine == null ? 0 : x.salDateLine.Value.Subtract(x.systemArrivalDate).TotalHours,
                        slaNotificationInterval = x.slaNotificationInterval,
                        requestFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                        emailFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.EMAIL).FirstOrDefault(),
                        requestTo = context.TBL_STAFF.Where(s => s.STAFFID == x.toStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                        approvalStatus = context.TBL_APPROVAL_STATUS.Where(p => p.APPROVALSTATUSID == x.approvalStatusId).Select(p => p.APPROVALSTATUSNAME).FirstOrDefault(),
                        ReferenceNumber = context.TBL_LOAN_APPLICATION.Where(o=>o.LOANAPPLICATIONID==x.targetId).Select(o=>o.APPLICATIONREFERENCENUMBER).FirstOrDefault()
                    };
                    list.Add(data);
                }
                else {
                    data = new SLANotificationViewModel
                    {
                        approvalTrailId = x.approvalTrailId,
                        arrivalDate = x.arrivalDate,
                        fromApprovalLevelId = x.fromApprovalLevelId,
                        operationId = x.operationId,
                        requestStaffId = x.requestStaffId,
                        salDateLine = x.systemArrivalDate.AddHours(x.salInterval),
                        slaNotificationDate = x.systemArrivalDate.AddHours(x.slaNotificationInterval),
                        salInterval = x.salInterval,
                        systemArrivalDate = x.systemArrivalDate,
                        systemResponseDate = x.systemResponseDate,
                        targetId = x.targetId,
                        toApprovalLevelId = x.toApprovalLevelId,
                        toStaffId = x.toStaffId,
                        staffEmail = x.staffEmail,
                        operationName = x.operationName,
                        responseDefaultTime = x.systemResponseDate == null ? 0 : x.systemArrivalDate.AddHours(x.salInterval).Subtract(x.systemResponseDate.Value).TotalHours,
                        // responseDefaultTime = x.salDateLine == null ? 0 : x.salDateLine.Value.Subtract(x.systemArrivalDate).TotalHours,
                        slaNotificationInterval = x.slaNotificationInterval,
                        requestFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                        emailFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.EMAIL).FirstOrDefault(),
                        requestTo = context.TBL_STAFF.Where(s => s.STAFFID == x.toStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                        approvalStatus = context.TBL_APPROVAL_STATUS.Where(p => p.APPROVALSTATUSID == x.approvalStatusId).Select(p => p.APPROVALSTATUSNAME).FirstOrDefault()
                    };
                    list.Add(data);
                }
                    
            }
            return list.ToList(); 
        }

        public List<Blacklist> Blacklist(DateTime startDate, DateTime endDate, string customercode)
        {
            var data = (from camsol in context.TBL_LOAN_CAMSOL
                        where DbFunctions.TruncateTime(camsol.DATE) >= DbFunctions.TruncateTime(startDate) 
                        && DbFunctions.TruncateTime(camsol.DATE) <= DbFunctions.TruncateTime(endDate) 
                        && (camsol.CUSTOMERNAME.ToLower().Contains(customercode.ToLower())
                        || camsol.CUSTOMERCODE == customercode || customercode == null ||  customercode == "")
                        select new Blacklist
                        {
                            accountName = camsol.ACCOUNTNAME,
                            accountNumber = camsol.ACCOUNTNAME,
                            balance = camsol.BALANCE,
                            canTakeLoan = camsol.CANTAKELOAN,
                            customerCode = camsol.CUSTOMERCODE,
                            customerName = camsol.CUSTOMERNAME,
                            date = camsol.DATE,
                            camsolType = context.TBL_LOAN_SYSTEM_TYPE.Where(x=>x.LOANSYSTEMTYPEID==camsol.LOANSYSTEMTYPEID).Select(x=>x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                            loanSystemType = context.TBL_LOAN_CAMSOL_TYPE.Where(x=>x.CAMSOLTYPEID==camsol.CAMSOLTYPEID).Select(x=>x.CAMSOLTYPENAME).FirstOrDefault(),
                            InterestInSuspense = camsol.INTERESTINSUSPENSE,
                            principal = camsol.PRINCIPAL,
                            remark = camsol.REMARK,
                        });

          
            return data.ToList();
        }
    }

}
