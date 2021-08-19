 using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
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

        FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext();
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
                group Loan by new { branch.BRANCHCODE, branch.BRANCHNAME, branch.NPL_LIMIT } into groupedQ
                select new SectorLimitViewModel()
                {
                    companyLogo = company.LOGOPATH,
                    companyName = company.NAME,
                    balances = groupedQ.Sum(z => z.OUTSTANDINGPRINCIPAL + z.PASTDUEPRINCIPAL),
                    subsectorCode = (groupedQ.Key.BRANCHCODE ?? "NOT DEFINED"),
                    sectorName = groupedQ.Key.BRANCHNAME,
                    limitMaximumValue = (decimal?)groupedQ.Key.NPL_LIMIT ?? 0,
                    usage = groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL),
                    
                }).ToList();

                return output;
            }
        }

        public List<LoanCovenantDetailViewModel> CovenantsApproachingDueDate(DateTime startDate, DateTime endDate, int companyId)
        {
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                            // where DbFunctions.TruncateTime(a.NEXTCOVENANTDATE ) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.NEXTCOVENANTDATE)<= DbFunctions.TruncateTime(endDate)
                                                             where DbFunctions.TruncateTime(a.COVENANTDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.COVENANTDATE) <= DbFunctions.TruncateTime(endDate)
                                                             && a.COMPANYID == companyId
                                                             orderby a.NEXTCOVENANTDATE descending
                                                             select new LoanCovenantDetailViewModel
                                                             {
                                                                 //companyId = a.COMPANYID,
                                                                 covenantAmount = a.COVENANTAMOUNT,
                                                                 covenantDate = a.COVENANTDATE,
                                                                 dueDate = a.NEXTCOVENANTDATE,
                                                                 covenantDetail = a.COVENANTDETAIL,
                                                                 //covenantTypeId = a.COVENANTTYPEID,
                                                                 covenantTypeName = g.COVENANTTYPENAME,
                                                                 //frequencyTypeId = a.FREQUENCYTYPEID,
                                                                 frequencyTypeName = f.MODE,
                                                                 //loanId = a.LOANID,
                                                                // loanRefNumber = e.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                                 loanRefNumber = b.LOANREFERENCENUMBER,
                                                                 //relationshipManager = d.FIRSTNAME + " " + d.LASTNAME,
                                                                 //relationshipManagerId = d.STAFFID,
                                                                 //managerEmail = d.EMAIL,
                                                                 //relationshipOfficerId = d.STAFFID,
                                                                 //relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                                                                 //officerEmail = d.EMAIL,
                                                             }).Distinct().ToList();

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
                                                             orderby f.LASTVALUATIONDATE descending

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

        public List<CollateralVisitationViewModel> CollateralPropertyDueForVisitation(DateTime startDate, DateTime endDate)
        {
            List<CollateralVisitationViewModel> PropertyDueForVisitation = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join v in context.TBL_COLLATERAL_VISITATION on a.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                                                     where DbFunctions.TruncateTime(v.VISITATIONDATE).Value >= DbFunctions.TruncateTime(startDate).Value && DbFunctions.TruncateTime(v.VISITATIONDATE).Value <= DbFunctions.TruncateTime(endDate).Value
                                                           && d.REQUIREVISITATION == true
                                                     orderby v.VISITATIONDATE descending

                                                     select new CollateralVisitationViewModel
                                                     {
                                                         collateralTypeId = a.COLLATERALTYPEID,
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastVisitationDate = v.VISITATIONDATE,
                                                         nextVisitationDate = v.VISITATIONDATE,
                                                         visitationCycle = e.VISITATIONCYCLE,
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
                                                            // where a.NEXTCOVENANTDATE >= startDate && a.NEXTCOVENANTDATE <= endDate && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
                                                             where a.COVENANTDATE >= startDate && a.COVENANTDATE <= endDate && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
                                                                  orderby a.COVENANTDATE descending

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
                                                              // loanRefNumber = e.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                                 loanRefNumber = b.LOANREFERENCENUMBER,
                                                                 relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                                                                 managerEmail = c.EMAIL,
                                                                 relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                                                                 officerEmail = d.EMAIL,
                                                             }).ToList();
            return turnoverCovenant;
        } // to do

        //public List<LoanViewModel> NPL(DateTime startDate, DateTime endDate, int classification)
        //{
            
        //        var staffmis = (from m in stagecontext.STG_STAFFMIS select new STG_STAFFMIS { REGION = m.REGION, STAFFCODE = m.STAFFCODE, BRANCHCODE = m.BRANCHCODE, USERNAME = m.USERNAME }).ToList();
        //    List<LoanViewModel> termloans = (from a in context.TBL_LOAN_APPLICATION
        //                                     join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
        //                                     join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
        //                                     join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on b.USER_PRUDENTIAL_GUIDE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID //b.EXT_PRUDENT_GUIDELINE_STATUSID
        //                                     join cu in context.TBL_CUSTOMER on b.CUSTOMERID equals cu.CUSTOMERID
        //                                     //join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
        //                                     where e.PRUDENTIALGUIDELINETYPEID == (int)PrudentialGuidelineTypeEnum.NonPerforming //b.EXT_PRUDENT_GUIDELINE_STATUSID
        //                                     && DbFunctions.TruncateTime(b.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate)
        //                                     orderby b.EFFECTIVEDATE descending

        //                                     select new LoanViewModel
        //                                     {
        //                                         applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //                                         loanReferenceNumber = b.LOANREFERENCENUMBER,
        //                                         bookingDate = b.BOOKINGDATE,
        //                                         disburseDate = b.DISBURSEDATE,
        //                                         nplDate = (DateTime?)b.NPLDATE,
        //                                         loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                                         externalPrudentialGuidelineStatus = e.STATUSNAME,
        //                                         productName = d.TBL_PRODUCT.PRODUCTNAME,
        //                                         customerName = cu.FIRSTNAME + " " + cu.LASTNAME,
        //                                         outstandingInterest = b.OUTSTANDINGINTEREST,
        //                                         outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
        //                                         //  businessUnit = staffmis.Where(x => x.STAFFCODE == a.TBL_STAFF.STAFFCODE)


        //                                     }).ToList();//.Select(x =>
        //                                     //{

        //                                     //    var getBudetails = staffmis.Where(z => z.STAFFCODE == x.staffCode).Select(z => z.REGION).FirstOrDefault();

        //                                     //    if (getBudetails != null)
        //                                     //    {
        //                                     //        x.businessUnit = getBudetails;
        //                                     //    }
        //                                     //    else if (getBudetails != null)
        //                                     //    {
        //                                     //        x.businessUnit = "";
        //                                     //    }
        //                                     //    return x;
        //                                     //}).ToList();
                                

        //       //List<LoanViewModel> overdraft = (from a in context.TBL_LOAN_APPLICATION
        //       //                              join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
        //       //                                 //join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
        //       //                                 join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
        //       //                                 join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on c.EXT_PRUDENT_GUIDELINE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID                                             
        //       //                              where e.PRUDENTIALGUIDELINETYPEID == (int)PrudentialGuidelineTypeEnum.NonPerforming //b.EXT_PRUDENT_GUIDELINE_STATUSID
        //       //                              && DbFunctions.TruncateTime(c.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(c.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate)
        //       //                              select new LoanViewModel
        //       //                              {
        //       //                                  applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //       //                                  loanReferenceNumber = c.LOANREFERENCENUMBER,
        //       //                                  bookingDate = c.BOOKINGDATE,
        //       //                                  disburseDate = c.DISBURSEDATE,
        //       //                                  nplDate = (DateTime?)c.NPLDATE,
        //       //                                  //outstandingInterest = b.OUTSTANDINGINTEREST,
        //       //                                  outstandingPrincipal = c.OVERDRAFTLIMIT,
        //       //                                  loanTypeName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //       //                                  externalPrudentialGuidelineStatus = c.TBL_LOAN_PRUDENTIALGUIDELINE.STATUSNAME,
        //       //                                  productName = d.TBL_PRODUCT.PRODUCTNAME
        //       //                              }).ToList();

        //    return termloans;
        //    //}
        //    //else
        //    //{
        //    //    List<LoanViewModel> npl = (from a in context.TBL_LOAN_APPLICATION
        //    //                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
        //    //                               join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
        //    //                               join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on b.EXT_PRUDENT_GUIDELINE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID
        //    //                               //join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
        //    //                               where e.PRUDENTIALGUIDELINETYPEID != (int)LoanPrudentialStatusEnum.Performing //b.INT_PRUDENT_GUIDELINE_STATUSID
        //    //                               && b.BOOKINGDATE >= startDate && b.BOOKINGDATE <= endDate
        //    //                               select new LoanViewModel
        //    //                               {
        //    //                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //    //                                   loanReferenceNumber = b.LOANREFERENCENUMBER,
        //    //                                   bookingDate = b.BOOKINGDATE,
        //    //                                   disburseDate = b.DISBURSEDATE,
        //    //                                   nplDate = (DateTime?)b.NPLDATE,
        //    //                                   outstandingInterest = b.OUTSTANDINGINTEREST,
        //    //                                   outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
        //    //                                   loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //    //                                   externalPrudentialGuidelineStatus = b.TBL_LOAN_PRUDENTIALGUIDELINE.STATUSNAME,
        //    //                                   productName = d.TBL_PRODUCT.PRODUCTNAME
        //    //                               }).ToList();
        //    //    return npl;
        //    //}
          
            
        //}  //done

        public List<LoanViewModel> NPL(DateTime startDate, DateTime endDate, int classification)
        {

            List<SubHead> staffmisi = new List<SubHead>();
            using (FinTrakBankingStagingContext stagingContext = new FinTrakBankingStagingContext())
            {
                staffmisi = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    List<LoanViewModel> termloans = (from a in context.TBL_LOAN_APPLICATION
                                                     join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                                     join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                                     join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on b.USER_PRUDENTIAL_GUIDE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID //b.EXT_PRUDENT_GUIDELINE_STATUSID
                                                     join cu in context.TBL_CUSTOMER on b.CUSTOMERID equals cu.CUSTOMERID
                                                     //join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                                     where e.PRUDENTIALGUIDELINETYPEID == (int)PrudentialGuidelineTypeEnum.NonPerforming //b.EXT_PRUDENT_GUIDELINE_STATUSID
                                                     && DbFunctions.TruncateTime(b.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate)
                                                     && b.OUTSTANDINGINTEREST >= 0
                                                     orderby b.EFFECTIVEDATE descending

                                                     select new LoanViewModel
                                                     {
                                                         applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                                         loanReferenceNumber = b.LOANREFERENCENUMBER,
                                                         bookingDate = b.BOOKINGDATE,
                                                         disburseDate = b.DISBURSEDATE,
                                                         nplDate = (DateTime?)b.NPLDATE,
                                                         loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                         externalPrudentialGuidelineStatus = e.STATUSNAME,
                                                         productName = d.TBL_PRODUCT.PRODUCTNAME,
                                                         customerName = cu.FIRSTNAME + " " + cu.LASTNAME,
                                                         outstandingInterest = Math.Round(b.OUTSTANDINGINTEREST,2),
                                                         outstandingPrincipal = Math.Round(b.OUTSTANDINGPRINCIPAL,2),
                                                         //  businessUnit = staffmis.Where(x => x.STAFFCODE == a.TBL_STAFF.STAFFCODE)


                                                     }).ToList().Select(x =>
                                                     {

                                                         var getBudetails = staffmisi.Where(z => z.staffCode == x.staffCode).Select(z => z.region).FirstOrDefault();

                                                         if (getBudetails != null)
                                                         {
                                                             x.businessUnit = getBudetails;
                                                         }
                                                         else if (getBudetails != null)
                                                         {
                                                             x.businessUnit = "";
                                                         }
                                                         return x;
                                                     }).ToList();

                    return termloans;   
                }
                
                
            }
            
        }



        //}
        //else
        //{
        //    List<LoanViewModel> npl = (from a in context.TBL_LOAN_APPLICATION
        //                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
        //                               join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
        //                               join e in context.TBL_LOAN_PRUDENTIALGUIDELINE on b.EXT_PRUDENT_GUIDELINE_STATUSID equals e.PRUDENTIALGUIDELINESTATUSID
        //                               //join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
        //                               where e.PRUDENTIALGUIDELINETYPEID != (int)LoanPrudentialStatusEnum.Performing //b.INT_PRUDENT_GUIDELINE_STATUSID
        //                               && b.BOOKINGDATE >= startDate && b.BOOKINGDATE <= endDate
        //                               select new LoanViewModel
        //                               {
        //                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //                                   loanReferenceNumber = b.LOANREFERENCENUMBER,
        //                                   bookingDate = b.BOOKINGDATE,
        //                                   disburseDate = b.DISBURSEDATE,
        //                                   nplDate = (DateTime?)b.NPLDATE,
        //                                   outstandingInterest = b.OUTSTANDINGINTEREST,
        //                                   outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
        //                                   loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                                   externalPrudentialGuidelineStatus = b.TBL_LOAN_PRUDENTIALGUIDELINE.STATUSNAME,
        //                                   productName = d.TBL_PRODUCT.PRODUCTNAME
        //                               }).ToList();
        //    return npl;
        //}





        public List<LoanViewModel> SelfLiquidatingLoan(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> selfLiquidatingLoan = (from a in context.TBL_LOAN
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating &&
                                               a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                                       orderby a.MATURITYDATE descending

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

            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;


            List<LoanViewModel> overDraft = (from a in context.TBL_LOAN_REVOLVING
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               //join l in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan
                                               && DbFunctions.TruncateTime(a.MATURITYDATE) >= DbFunctions.TruncateTime(startDate) 
                                               && DbFunctions.TruncateTime(a.MATURITYDATE) <= DbFunctions.TruncateTime(endDate)
                                               //&& a.MATURITYDATE < applDate// && a.MATURITYDATE<= endDate
                                             orderby a.MATURITYDATE descending

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
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   sanctionLimit = a.OVERDRAFTLIMIT
                                                   
                                                  // outstandingInterest = l.OUTSTANDINGINTEREST,
                                                  // outstandingPrincipal = l.OUTSTANDINGPRINCIPAL
                                               }).ToList();

            return overDraft;
        } //done

        //public List<LoanViewModel> BondAndGuarantee(DateTime startDate, DateTime endDate , int approvalStatus)
        //{
        //    List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
        //    if (approvalStatus == (int)LoanStatusEnum.Expired)
        //    {
        //        bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
        //                            join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
        //                            join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
        //                            join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
        //                            join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
        //                            join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
        //                            where a.ISTENORED == false && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
        //                            && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
        //                            orderby a.MATURITYDATE descending

        //                            select new LoanViewModel
        //                            {

        //                                applicationReferenceNumber = a.LOANREFERENCENUMBER,
        //                                loanReferenceNumber = a.LOANREFERENCENUMBER,
        //                                bookingDate = a.BOOKINGDATE,
        //                                disburseDate = a.DISBURSEDATE,
        //                                maturityDate = a.MATURITYDATE,
        //                                principalAmount = a.CONTINGENTAMOUNT,
        //                                exchangeRate = a.EXCHANGERATE,
        //                                loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                                relationshipManagerId = a.RELATIONSHIPMANAGERID,
        //                                relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
        //                                relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
        //                                relationshipOfficerId = a.RELATIONSHIPOFFICERID,
        //                                relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
        //                                relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
        //                                branchId = a.BRANCHID,
        //                                branchName = br.BRANCHNAME,
        //                                customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
        //                                loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
        //                                productName = p.PRODUCTNAME,
        //                                productTypeName = pt.PRODUCTTYPENAME,
        //                                outstandingPrincipal = a.CONTINGENTAMOUNT





        //                                                }).ToList();

        //        return bondAndGuarantee;
        //    }
        //    else
        //    {

        //         bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
        //                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
        //                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
        //                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
        //                                                where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == approvalStatus
        //                                                orderby a.MATURITYDATE descending

        //                                                select new LoanViewModel
        //                                                {
        //                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
        //                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
        //                                                    bookingDate = a.BOOKINGDATE,
        //                                                    disburseDate = a.DISBURSEDATE,
        //                                                    maturityDate = a.MATURITYDATE,
        //                                                    principalAmount = a.CONTINGENTAMOUNT,
        //                                                    exchangeRate = a.EXCHANGERATE,
        //                                                    loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                                                    relationshipManagerId = a.RELATIONSHIPMANAGERID,
        //                                                    relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
        //                                                    relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
        //                                                    relationshipOfficerId = a.RELATIONSHIPOFFICERID,
        //                                                    relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
        //                                                    relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
        //                                                    branchId = a.BRANCHID,
        //                                                    branchName = br.BRANCHNAME,
        //                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
        //                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now)
        //                                                }).ToList();
        //        return bondAndGuarantee;
        //    }
        //} // to do


        public List<LoanViewModel> BondAndGuarantee(DateTime startDate, DateTime endDate, int facilityStatusId)
        {
            List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
            if (facilityStatusId == -1) // expired facility
            {
                bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                    join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                    join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                    join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                    join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                    join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                    where a.ISTENORED == false && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                    && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
                                    orderby a.MATURITYDATE descending

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
                                        loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                        productName = p.PRODUCTNAME,
                                        productTypeName = pt.PRODUCTTYPENAME,
                                        outstandingPrincipal = a.CONTINGENTAMOUNT





                                    }).ToList();

                return bondAndGuarantee;
            }
            else
            {

                bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                    join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                    join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                    join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                    where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == facilityStatusId
                                    orderby a.MATURITYDATE descending

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


        public List<LoanViewModel> BandGReport(DateTime startDate, DateTime endDate, int facilityStatusId)
        {
            List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
            if (facilityStatusId == -1) // expired facility
            {
                bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                    join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                    join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                    join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                    join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                    join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                    where a.ISTENORED == false && a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                    && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty

                                    orderby a.MATURITYDATE descending
                                    select new LoanViewModel
                                    {
                                        customerId = a.CUSTOMERID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        loanReferenceNumber = a.LOANREFERENCENUMBER,
                                        bookingDate = a.BOOKINGDATE,
                                        issueDate = a.DISBURSEDATE,
                                        expiryDate = a.MATURITYDATE,
                                        sbu = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == a.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                        guaranteeType = context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                        customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                        //customerTier = getContractorTieringByApplicationAndCustomerBandG(a.LOANAPPLICATIONDETAILID, a.CUSTOMERID),
                                        apgAccountNumber = s.PRODUCTACCOUNTNUMBER,
                                        projectDetails = context.TBL_PROJECT_RISK_RATING.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(x => x.PROJECTDETAILS).FirstOrDefault(),
                                        projectLocation = context.TBL_PROJECT_RISK_RATING.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(x => x.PROJECTLOCATION).FirstOrDefault(),
                                        contractEmployer = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(x=>x.CUSTOMERID == a.CUSTOMERID).Select(x=>x.EMPLOYERNAME).FirstOrDefault(),
                                        //projectRiskRatings = getProjectRiskRatingByBandG(a.LOANAPPLICATIONDETAILID),
                                        ccy = context.TBL_CURRENCY.Where(x=>x.CURRENCYID == a.CURRENCYID).Select(x=>x.CURRENCYNAME).FirstOrDefault(),
                                        guaranteeAmount = a.CONTINGENTAMOUNT,
                                        exposureOnGuarantee = context.TBL_LOAN_REVIEW_OPERATION.Where(x=>x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook).Sum(x=>x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                        accountOfficer = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                        relationshipTeam = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == a.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault() + " " + br.BRANCHNAME,
                                    }).ToList();
                var data = bondAndGuarantee.GroupBy(r => r.loanApplicationDetailId)
                               .Select(r => r.FirstOrDefault()).ToList();
                foreach (var rec in data)
                {
                    var loanApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(rec.loanApplicationDetailId).LOANAPPLICATIONID;
                    var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                             where a.LOANAPPLICATIONID == loanApplication && a.CUSTOMERID == rec.customerId
                                             select new
                                             {
                                                 contractorTierId = a.CONTRACTORTIERID,
                                                 loanApplicationId = a.LOANAPPLICATIONID,
                                                 customerId = a.CUSTOMERID,
                                                 actualValue = a.ACTUALVALUE
                                             }).AsEnumerable().Select(a => new ContractorTieringViewModel
                                             {
                                                 contractorTierId = a.contractorTierId,
                                                 loanApplicationId = a.loanApplicationId,
                                                 customerId = a.customerId,
                                                 actualValue = a.actualValue
                                             }).ToList();

                    var result = contractorTiering.Select(a => new ContractorTieringViewModel
                    {
                        contractorTierId = a.contractorTierId,
                        loanApplicationId = a.loanApplicationId,
                        customerId = a.customerId,
                        criteria = a.criteria,
                        actualValue = a.actualValue,
                        computation = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == a.loanApplicationId).Sum(d => d.ACTUALVALUE),
                    }).ToList();

                    foreach (var check in result)
                    {
                        if (check.computation >= 80)
                        {
                            rec.customerTier = "Tier 1";
                        }
                        if (check.computation >= 60 && check.computation <= 79)
                        {
                            rec.customerTier = "Tier 2";
                        }
                        if (check.computation >= 1 && check.computation <= 59)
                        {
                            rec.customerTier = "Tier 3";
                        }
                        if (check.computation <= 0)
                        {
                            rec.customerTier = "N/A";
                        }
                    }

                    var customerTier1 = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == loanApplication).ToList();
                    var projectRiskRating = (from a in context.TBL_PROJECT_RISK_RATING
                                             join c in context.TBL_PROJECT_RISK_RATING_CATEGORY on a.CATEGORYID equals c.CATEGORYID
                                             where a.LOANAPPLICATIONID == loanApplication && a.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId
                                             select new
                                             {
                                                 loanApplicationId = a.LOANAPPLICATIONID,
                                                 loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                                 loanBookingRequestId = a.LOANBOOKINGREQUESTID,
                                                 categoryName = c.CATEGORYNAME,
                                                 categoryValue = a.CATEGORYVALUE,
                                                 projectLocation = a.PROJECTLOCATION,
                                                 projectDetails = a.PROJECTDETAILS
                                             }).AsEnumerable().Select(a => new ProjectRiskRatingViewModel
                                             {
                                                 loanApplicationId = a.loanApplicationId,
                                                 loanApplicationDetailId = a.loanApplicationDetailId,
                                                 loanBookingRequestId = a.loanBookingRequestId,
                                                 categoryName = a.categoryName,
                                                 categoryValue = a.categoryValue,
                                                 projectLocation = a.projectLocation,
                                                 projectDetails = a.projectDetails
                                             }).ToList();

                    var result2 = projectRiskRating.Select(a => new ProjectRiskRatingViewModel
                    {
                        loanApplicationId = a.loanApplicationId,
                        loanApplicationDetailId = a.loanApplicationDetailId,
                        loanBookingRequestId = a.loanBookingRequestId,
                        categoryName = a.categoryName,
                        categoryValue = a.categoryValue,
                        projectLocation = a.projectLocation,
                        projectDetails = a.projectDetails,
                        computation = context.TBL_PROJECT_RISK_RATING.Where(d => d.LOANAPPLICATIONDETAILID == a.loanApplicationDetailId).Sum(d => d.CATEGORYVALUE),
                    }).ToList();

                    foreach (var i in result2)
                    {
                        int rating = 0;
                        var overRallTotal = i.computation + rating;
                        if (overRallTotal >= 81 && overRallTotal <= 100)
                        {
                            rec.projectRiskRatings = "LOW";
                        }
                        if (overRallTotal >= 66 && overRallTotal < 81)
                        {
                            rec.projectRiskRatings = "MODERATE";
                        }
                        if (overRallTotal >= 51 && overRallTotal < 66)
                        {
                            rec.projectRiskRatings = "ABOVE AVERAGE";
                        }
                        if (overRallTotal >= 1 && overRallTotal < 51)
                        {
                            rec.projectRiskRatings = "HIGH";
                        }
                        if (overRallTotal <=0)
                        {
                            rec.projectRiskRatings = "N/A";
                        }
                    }
                }
                return bondAndGuarantee;
            }
            else
            {

                bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                    join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                    join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                    join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                    where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate && a.LOANSTATUSID == facilityStatusId
                                    orderby a.MATURITYDATE descending

                                    select new LoanViewModel
                                    {
                                        customerId = a.CUSTOMERID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        loanReferenceNumber = a.LOANREFERENCENUMBER,
                                        bookingDate = a.BOOKINGDATE,
                                        issueDate = a.DISBURSEDATE,
                                        expiryDate = a.MATURITYDATE,
                                        sbu = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == a.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                        guaranteeType = context.TBL_PRODUCT.Where(x=>x.PRODUCTID == a.PRODUCTID).Select(x=>x.PRODUCTNAME).FirstOrDefault(),
                                        customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                       // customerTier = getContractorTieringByApplicationAndCustomerBandG(a.LOANAPPLICATIONDETAILID, a.CUSTOMERID),
                                        apgAccountNumber = s.PRODUCTACCOUNTNUMBER,
                                        projectDetails = context.TBL_PROJECT_RISK_RATING.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(x => x.PROJECTDETAILS).FirstOrDefault(),
                                        projectLocation = context.TBL_PROJECT_RISK_RATING.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(x => x.PROJECTLOCATION).FirstOrDefault(),
                                        contractEmployer = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(x => x.CUSTOMERID == a.CUSTOMERID).Select(x => x.EMPLOYERNAME).FirstOrDefault(),
                                       // projectRiskRatings = getProjectRiskRatingByBandG(a.LOANAPPLICATIONDETAILID),
                                        ccy = context.TBL_CURRENCY.Where(x => x.CURRENCYID == a.CURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                                        guaranteeAmount = a.CONTINGENTAMOUNT,
                                        exposureOnGuarantee = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                        accountOfficer = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                        relationshipTeam = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == a.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault() +" " + br.BRANCHNAME,
                                    }).ToList();
                var data = bondAndGuarantee.GroupBy(r => r.loanApplicationDetailId)
                               .Select(r => r.FirstOrDefault()).ToList();
                foreach (var rec in data)
                {
                    var loanApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(rec.loanApplicationDetailId).LOANAPPLICATIONID;
                    var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                             where a.LOANAPPLICATIONID == loanApplication && a.CUSTOMERID == rec.customerId
                                             select new
                                             {
                                                 contractorTierId = a.CONTRACTORTIERID,
                                                 loanApplicationId = a.LOANAPPLICATIONID,
                                                 customerId = a.CUSTOMERID,
                                                 actualValue = a.ACTUALVALUE
                                             }).AsEnumerable().Select(a => new ContractorTieringViewModel
                                             {
                                                 contractorTierId = a.contractorTierId,
                                                 loanApplicationId = a.loanApplicationId,
                                                 customerId = a.customerId,
                                                 actualValue = a.actualValue
                                             }).ToList();

                    var result = contractorTiering.Select(a => new ContractorTieringViewModel
                    {
                        contractorTierId = a.contractorTierId,
                        loanApplicationId = a.loanApplicationId,
                        customerId = a.customerId,
                        criteria = a.criteria,
                        actualValue = a.actualValue,
                        computation = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == a.loanApplicationId).Sum(d => d.ACTUALVALUE),
                    }).ToList();

                    foreach (var check in result)
                    {
                        if (check.computation >= 80)
                        {
                            rec.customerTier = "Tier 1";
                        }
                        if (check.computation >= 60 && check.computation <= 79)
                        {
                            rec.customerTier = "Tier 2";
                        }
                        if (check.computation >= 1 && check.computation <= 59)
                        {
                            rec.customerTier = "Tier 3";
                        }
                        if (check.computation <= 0)
                        {
                            rec.customerTier = "N/A";
                        }
                    }

                    var customerTier1 = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == loanApplication).ToList();
                    var projectRiskRating = (from a in context.TBL_PROJECT_RISK_RATING
                                             join c in context.TBL_PROJECT_RISK_RATING_CATEGORY on a.CATEGORYID equals c.CATEGORYID
                                             where a.LOANAPPLICATIONID == loanApplication && a.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId
                                             select new
                                             {
                                                 loanApplicationId = a.LOANAPPLICATIONID,
                                                 loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                                 loanBookingRequestId = a.LOANBOOKINGREQUESTID,
                                                 categoryName = c.CATEGORYNAME,
                                                 categoryValue = a.CATEGORYVALUE,
                                                 projectLocation = a.PROJECTLOCATION,
                                                 projectDetails = a.PROJECTDETAILS
                                             }).AsEnumerable().Select(a => new ProjectRiskRatingViewModel
                                             {
                                                 loanApplicationId = a.loanApplicationId,
                                                 loanApplicationDetailId = a.loanApplicationDetailId,
                                                 loanBookingRequestId = a.loanBookingRequestId,
                                                 categoryName = a.categoryName,
                                                 categoryValue = a.categoryValue,
                                                 projectLocation = a.projectLocation,
                                                 projectDetails = a.projectDetails
                                             }).ToList();

                    var result2 = projectRiskRating.Select(a => new ProjectRiskRatingViewModel
                    {
                        loanApplicationId = a.loanApplicationId,
                        loanApplicationDetailId = a.loanApplicationDetailId,
                        loanBookingRequestId = a.loanBookingRequestId,
                        categoryName = a.categoryName,
                        categoryValue = a.categoryValue,
                        projectLocation = a.projectLocation,
                        projectDetails = a.projectDetails,
                        computation = context.TBL_PROJECT_RISK_RATING.Where(d => d.LOANAPPLICATIONDETAILID == a.loanApplicationDetailId).Sum(d => d.CATEGORYVALUE),
                    }).ToList();

                    foreach (var i in result2)
                    {
                        int rating = 0;
                        var overRallTotal = i.computation + rating;
                        if (overRallTotal >= 81 && overRallTotal <= 100)
                        {
                            rec.projectRiskRatings = "LOW";
                        }
                        if (overRallTotal >= 66 && overRallTotal < 81)
                        {
                            rec.projectRiskRatings = "MODERATE";
                        }
                        if (overRallTotal >= 51 && overRallTotal < 66)
                        {
                            rec.projectRiskRatings = "ABOVE AVERAGE";
                        }
                        if (overRallTotal >= 1 && overRallTotal < 51)
                        {
                            rec.projectRiskRatings = "HIGH";
                        }
                        if (overRallTotal <= 0)
                        {
                            rec.projectRiskRatings = "N/A";
                        }
                    }
                }
                return bondAndGuarantee;
            }
        }


        //rebook report
        public List<LoanViewModel> ContingentRebookingReport(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
                bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                    join b in context.TBL_LOAN_REVIEW_OPERATION on a.CONTINGENTLOANID equals b.LOANID
                                    join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                    join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                    join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                    join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                    join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                    where (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                    && b.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook
                                    && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                    orderby a.MATURITYDATE descending

                                    select new LoanViewModel
                                    {
                                        loanReferenceNumber = a.LOANREFERENCENUMBER,
                                        customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                        facilityType = p.PRODUCTNAME +"-"+pt.PRODUCTTYPENAME,
                                        guaranteeAmount = a.CONTINGENTAMOUNT,
                                        rebookedAmount = b.CONTINGENTOUTSTANDINGPRINCIPAL,
                                        dateRebooked = b.REBOOKDATE,
                                        currentExposure = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && (DbFunctions.TruncateTime(b.REBOOKDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.REBOOKDATE) <= DbFunctions.TruncateTime(endDate))).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                        previousExposure = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && (DbFunctions.TruncateTime(b.REBOOKDATE) < DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.REBOOKDATE) > DbFunctions.TruncateTime(endDate))).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL) ==null ? (decimal)0.01 : context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && (DbFunctions.TruncateTime(b.REBOOKDATE) < DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.REBOOKDATE) > DbFunctions.TruncateTime(endDate))).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                    }).ToList();
                
                return bondAndGuarantee;
            
        }

        public List<LoanViewModel> ContingentAmortizationReport(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
            bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                join b in context.TBL_LOAN_REVIEW_OPERATION on a.CONTINGENTLOANID equals b.LOANID
                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                where (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                && b.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityAmountReduction
                                && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                orderby a.MATURITYDATE descending

                                select new LoanViewModel
                                {
                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                    facilityType = p.PRODUCTNAME + "-" + pt.PRODUCTTYPENAME,
                                    guaranteeAmount = a.CONTINGENTAMOUNT,
                                    amortisedAmount = b.PREPAYMENT,
                                    dateAmortised = b.OPERATIONDATE,
                                    currentExposure = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && (DbFunctions.TruncateTime(b.REBOOKDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.REBOOKDATE) <= DbFunctions.TruncateTime(endDate))).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                    previousExposure = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityTerminateAndRebook && (DbFunctions.TruncateTime(b.REBOOKDATE) < DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(b.REBOOKDATE) > DbFunctions.TruncateTime(endDate))).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                }).ToList();

            return bondAndGuarantee;

        }

        public List<LoanViewModel> ContingentDischargeReport(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> bondAndGuarantee = new List<LoanViewModel>();
            bondAndGuarantee = (from a in context.TBL_LOAN_CONTINGENT
                                join b in context.TBL_LOAN_REVIEW_OPERATION on a.CONTINGENTLOANID equals b.LOANID
                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                where (a.DATETIMECREATED >= startDate && a.DATETIMECREATED <= endDate)
                                && b.OPERATIONTYPEID == (int)OperationsEnum.ContingentLiabilityAmountReduction
                                && b.OPERATIONCOMPLETED == true
                                orderby a.MATURITYDATE descending

                                select new LoanViewModel
                                {
                                    relatedloanReferenceNumber = a.LOANREFERENCENUMBER,
                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                    facilityType = p.PRODUCTNAME + "-" + pt.PRODUCTTYPENAME,
                                    guaranteeAmount = a.CONTINGENTAMOUNT,
                                    dateDischarged = b.OPERATIONDATE,
                                }).ToList();

            return bondAndGuarantee;

        }

        private string getContractorTieringByApplicationAndCustomerBandG(int loanApplicationId, int customerId)
        {
            var loanApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationId).LOANAPPLICATIONID;
            string finalResult = "";
            var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                     where a.LOANAPPLICATIONID == loanApplication && a.CUSTOMERID == customerId
                                     select new
                                     {
                                         contractorTierId = a.CONTRACTORTIERID,
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         customerId = a.CUSTOMERID,
                                         actualValue = a.ACTUALVALUE
                                     }).AsEnumerable().Select(a => new ContractorTieringViewModel
                                     {
                                         contractorTierId = a.contractorTierId,
                                         loanApplicationId = a.loanApplicationId,
                                         customerId = a.customerId,
                                         actualValue = a.actualValue
                                     }).ToList();

            var result = contractorTiering.Select(a => new ContractorTieringViewModel
            {
                contractorTierId = a.contractorTierId,
                loanApplicationId = a.loanApplicationId,
                customerId = a.customerId,
                criteria = a.criteria,
                actualValue = a.actualValue,
                computation = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == a.loanApplicationId).Sum(d => d.ACTUALVALUE),
            }).ToList();
            
            foreach(var check in result)
            {
                if(check.computation >= 80)
                {
                    finalResult = "Tier 1";
                }
                if (check.computation >= 60 && check.computation <= 79)
                {
                    finalResult = "Tier 2";
                }
                if (check.computation <= 59)
                {
                    finalResult = "Tier 3";
                }
            }
            return finalResult;
        }

        private string getProjectRiskRatingByBandG(int LOANAPPLICATIONDETAILID)
        {
            string finalResult = "";
            var loanApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(LOANAPPLICATIONDETAILID);
            var customerTier1 = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID).ToList();
            var projectRiskRating = (from a in context.TBL_PROJECT_RISK_RATING
                                     join c in context.TBL_PROJECT_RISK_RATING_CATEGORY on a.CATEGORYID equals c.CATEGORYID
                                     where a.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID && a.LOANAPPLICATIONDETAILID == LOANAPPLICATIONDETAILID
                                     select new
                                     {
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                         loanBookingRequestId = a.LOANBOOKINGREQUESTID,
                                         categoryName = c.CATEGORYNAME,
                                         categoryValue = a.CATEGORYVALUE,
                                         projectLocation = a.PROJECTLOCATION,
                                         projectDetails = a.PROJECTDETAILS
                                     }).AsEnumerable().Select(a => new ProjectRiskRatingViewModel
                                     {
                                         loanApplicationId = a.loanApplicationId,
                                         loanApplicationDetailId = a.loanApplicationDetailId,
                                         loanBookingRequestId = a.loanBookingRequestId,
                                         categoryName = a.categoryName,
                                         categoryValue = a.categoryValue,
                                         projectLocation = a.projectLocation,
                                         projectDetails = a.projectDetails
                                     }).ToList();

            var result = projectRiskRating.Select(a => new ProjectRiskRatingViewModel
            {
                loanApplicationId = a.loanApplicationId,
                loanApplicationDetailId = a.loanApplicationDetailId,
                loanBookingRequestId = a.loanBookingRequestId,
                categoryName = a.categoryName,
                categoryValue = a.categoryValue,
                projectLocation = a.projectLocation,
                projectDetails = a.projectDetails,
                computation = context.TBL_PROJECT_RISK_RATING.Where(d => d.LOANAPPLICATIONDETAILID == a.loanApplicationDetailId).Sum(d => d.CATEGORYVALUE),
            }).ToList();

            foreach (var i in result)
            {
                int rating = 0;
                i.customerTier = 0;
                if (customerTier1 != null)
                {
                    decimal compute = customerTier1.Where(x => x.LOANAPPLICATIONID == i.loanApplicationId).Sum(x => x.ACTUALVALUE);
                    if (compute >= 80) { rating = 25; }
                    if (compute >= 60 && compute <= 79) { rating = 20; }
                    if (compute <= 59) { rating = 10; }
                    i.customerTier = compute;
                }

                var overRallTotal = i.computation + rating;
                if (overRallTotal >= 81 && overRallTotal <= 100)
                {
                    finalResult = "LOW";
                }
                if (overRallTotal >= 66 && overRallTotal < 81)
                {
                    finalResult = "MODERATE";
                }
                if (overRallTotal >= 51 && overRallTotal < 66)
                {
                    finalResult = "ABOVE AVERAGE";
                }
                if (overRallTotal < 51)
                {
                    finalResult = "HIGH";
                }
            }

            return finalResult;
        }

        public List<LoanViewModel> SendAlertOnAccountWithExeption_Overdrawn(DateTime startDate, DateTime endDate)
        {
            List<LoanViewModel> overDue = (from a in context.TBL_LOAN_CONTINGENT
                                           join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                           join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                           join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                           where a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                           &&  s.AVAILABLEBALANCE < 0m
                                           orderby a.MATURITYDATE descending
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
                                                     orderby a.MATURITYDATE descending
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
                                                     //join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     //where p.ENDDATE>= startDate && p.ENDDATE <=endDate
                                                                       //orderby p.ENDDATE descending

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
                                                      //   insuranceCompany = p.INSURANCECOMPANYNAME,
                                                      //   startDate = (DateTime?)p.STARTDATE,
                                                      //   endDate = (DateTime?)p.ENDDATE,
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

        //Saheed
        public IEnumerable<SLANotificationViewModel> SLAMonitoring(DateTime startDate, DateTime endDate, int approvalStatus, int operationId)
        {
            int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CreditAppraisal, (int)OperationsEnum.ContigentLoanBooking ,
           (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
            (int)OperationsEnum.CommercialLoanBooking};

            var approvalStatusIdentity = context.TBL_APPROVAL_TRAIL.Where(x => x.APPROVALSTATUSID == approvalStatus).Select(x=> x.APPROVALSTATUSID).FirstOrDefault();

            var operationIdentity = context.TBL_OPERATIONS.Where(x => x.OPERATIONID == operationId).Select(x => x.OPERATIONID).FirstOrDefault();




            var list = new List<SLANotificationViewModel>();
            var notificationList = (from a in context.TBL_APPROVAL_TRAIL
                                     join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                                     join s in context.TBL_STAFF on a.REQUESTSTAFFID equals s.STAFFID
                                     join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
                                     where a.SYSTEMARRIVALDATETIME >= startDate && a.SYSTEMARRIVALDATETIME <= endDate
                                     orderby a.SYSTEMARRIVALDATETIME descending

                                     select new SLANotificationViewModel
                                     {
                                         approvalTrailId = a.APPROVALTRAILID,
                                         arrivalDate = a.ARRIVALDATE,
                                         fromApprovalLevelId = a.FROMAPPROVALLEVELID,
                                         operationId = a.OPERATIONID,
                                         requestStaffId = a.REQUESTSTAFFID,
                                         salDateLine = a.SYSTEMARRIVALDATETIME,//.AddHours(b.SLAINTERVAL),
                                         salInterval = b.SLAINTERVAL,
                                         systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                        // ressponseTime = a.SYSTEMRESPONSEDATETIME,
                                         systemResponseDate = a.SYSTEMRESPONSEDATETIME, 
                                         targetId = a.TARGETID,
                                         toApprovalLevelId = a.TOAPPROVALLEVELID,
                                         toStaffId = a.TOSTAFFID,
                                         staffEmail = s.EMAIL,
                                         operationName = o.OPERATIONNAME,
                                         //responseDefaultTime =a.SYSTEMRESPONSEDATETIME == null ? 0 : a.SYSTEMARRIVALDATETIME.AddHours(b.SLAINTERVAL).Subtract(a.SYSTEMRESPONSEDATETIME.Value).TotalHours,
                                         //TOSTAFFID
                                         slaNotificationInterval = b.SLANOTIFICATIONINTERVAL,
                                         approvalStatusId = a.APPROVALSTATUSID,
                                          requestFrom = context.TBL_STAFF.Where(s => s.STAFFID == a.REQUESTSTAFFID).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                                         emailFrom = context.TBL_STAFF.Where(s => s.STAFFID == a.REQUESTSTAFFID).Select(s => s.EMAIL).FirstOrDefault(),
                                          requestTo = context.TBL_STAFF.Where(s => s.STAFFID == a.TOSTAFFID).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
                                          approvalStatus = context.TBL_APPROVAL_STATUS.Where(p => p.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(p => p.APPROVALSTATUSNAME).FirstOrDefault(),
                                        ReferenceNumber = context.TBL_LOAN_APPLICATION.Where(o=>o.LOANAPPLICATIONID==a.TARGETID).Select(o=>o.APPLICATIONREFERENCENUMBER).FirstOrDefault()
                                     });

                           if(!(approvalStatusIdentity.Equals(0)) && operationIdentity.Equals(0) )
                           {
                             list = notificationList.Where(x => x.approvalStatusId.Equals(approvalStatus)).ToList();

                           }
                           else if(!(approvalStatusIdentity.Equals(0)) && !(operationIdentity.Equals(0)))
                           {
                list = notificationList.Where(x => x.approvalStatusId.Equals(approvalStatus) && x.operationId.Equals(operationId)).ToList();
                           }

                           return list.ToList(); 
        }
        //public IEnumerable<SLANotificationViewModel> SLAMonitoring(DateTime startDate, DateTime endDate, int approvalStatus,int operationId)
        //{

        //    int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CAM, (int)OperationsEnum.ContigentLoanBooking ,
        //   (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
        //    (int)OperationsEnum.CommercialLoanBooking};

           

        //    var list = new List<SLANotificationViewModel>();
                          
        //    var notificationList  = (from a in context.TBL_APPROVAL_TRAIL
        //                           join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
        //                           join s in context.TBL_STAFF on a.TOSTAFFID equals s.STAFFID
        //                           join o in context.TBL_OPERATIONS on a.OPERATIONID equals o.OPERATIONID
        //                           where a.SYSTEMARRIVALDATETIME >= startDate && a.SYSTEMARRIVALDATETIME <= endDate 
        //                           && ( a.APPROVALSTATUSID == approvalStatus || approvalStatus == 0)
        //                           && (a.OPERATIONID == operationId || operationId ==0)

        //                           orderby a.SYSTEMARRIVALDATETIME descending

        //                           //&& a.RESPONSESTAFFID == null
        //                           //&& a.TOSTAFFID != null
        //                           //&& b.SLAINTERVAL > 0

        //                           select new SLANotificationViewModel
        //                           {
        //                               approvalTrailId = a.APPROVALTRAILID,
        //                               arrivalDate = a.ARRIVALDATE,
        //                               fromApprovalLevelId = a.FROMAPPROVALLEVELID,
        //                               operationId = a.OPERATIONID,
        //                               requestStaffId = a.REQUESTSTAFFID,
        //                               salInterval = b.SLAINTERVAL,
        //                               systemArrivalDate = a.SYSTEMARRIVALDATETIME,
        //                               ressponseTime = a.SYSTEMRESPONSEDATETIME,
        //                               systemResponseDate = a.SYSTEMRESPONSEDATETIME,
        //                               targetId = a.TARGETID,
        //                               toApprovalLevelId = a.TOAPPROVALLEVELID,
        //                               toStaffId = a.TOSTAFFID,
        //                               staffEmail = s.EMAIL,
        //                               operationName = o.OPERATIONNAME,
        //                               slaNotificationInterval = b.SLANOTIFICATIONINTERVAL,
        //                               approvalStatusId = a.APPROVALSTATUSID
        //                           }).ToList();

        //    var result = notificationList;

        
        //    var data = new SLANotificationViewModel();

        //    foreach (var x in notificationList)
        //    {
        //        if (operations.Contains(x.operationId))
        //        {
        //            data = new SLANotificationViewModel
        //            {
        //                approvalTrailId = x.approvalTrailId,
        //                arrivalDate = x.arrivalDate,
        //                fromApprovalLevelId = x.fromApprovalLevelId,
        //                operationId = x.operationId,
        //                requestStaffId = x.requestStaffId,
        //                salDateLine = x.systemArrivalDate.AddHours(x.salInterval),
        //                slaNotificationDate = x.systemArrivalDate.AddHours(x.slaNotificationInterval),
        //                salInterval = x.salInterval,
        //                systemArrivalDate = x.systemArrivalDate,
        //                systemResponseDate = x.systemResponseDate,
        //                targetId = x.targetId,
        //                toApprovalLevelId = x.toApprovalLevelId,
        //                toStaffId = x.toStaffId,
        //                staffEmail = x.staffEmail,
        //                operationName = x.operationName,
        //                responseDefaultTime = x.systemResponseDate == null ? 0 : x.systemArrivalDate.AddHours(x.salInterval).Subtract(x.systemResponseDate.Value).TotalHours,
        //                // responseDefaultTime = x.salDateLine == null ? 0 : x.salDateLine.Value.Subtract(x.systemArrivalDate).TotalHours,
        //                slaNotificationInterval = x.slaNotificationInterval,
        //                requestFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
        //                emailFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.EMAIL).FirstOrDefault(),
        //                requestTo = context.TBL_STAFF.Where(s => s.STAFFID == x.toStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
        //                approvalStatus = context.TBL_APPROVAL_STATUS.Where(p => p.APPROVALSTATUSID == x.approvalStatusId).Select(p => p.APPROVALSTATUSNAME).FirstOrDefault(),
        //                ReferenceNumber = context.TBL_LOAN_APPLICATION.Where(o=>o.LOANAPPLICATIONID==x.targetId).Select(o=>o.APPLICATIONREFERENCENUMBER).FirstOrDefault()
        //            };
        //            list.Add(data);
        //        }
        //        else {
        //            data = new SLANotificationViewModel
        //            {
        //                approvalTrailId = x.approvalTrailId,
        //                arrivalDate = x.arrivalDate,
        //                fromApprovalLevelId = x.fromApprovalLevelId,
        //                operationId = x.operationId,
        //                requestStaffId = x.requestStaffId,
        //                salDateLine = x.systemArrivalDate.AddHours(x.salInterval),
        //                slaNotificationDate = x.systemArrivalDate.AddHours(x.slaNotificationInterval),
        //                salInterval = x.salInterval,
        //                systemArrivalDate = x.systemArrivalDate,
        //                systemResponseDate = x.systemResponseDate,
        //                targetId = x.targetId,
        //                toApprovalLevelId = x.toApprovalLevelId,
        //                toStaffId = x.toStaffId,
        //                staffEmail = x.staffEmail,
        //                operationName = x.operationName,
        //                responseDefaultTime = x.systemResponseDate == null ? 0 : x.systemArrivalDate.AddHours(x.salInterval).Subtract(x.systemResponseDate.Value).TotalHours,
        //                // responseDefaultTime = x.salDateLine == null ? 0 : x.salDateLine.Value.Subtract(x.systemArrivalDate).TotalHours,
        //                slaNotificationInterval = x.slaNotificationInterval,
        //                requestFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
        //                emailFrom = context.TBL_STAFF.Where(s => s.STAFFID == x.requestStaffId).Select(s => s.EMAIL).FirstOrDefault(),
        //                requestTo = context.TBL_STAFF.Where(s => s.STAFFID == x.toStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + s.MIDDLENAME).FirstOrDefault(),
        //                approvalStatus = context.TBL_APPROVAL_STATUS.Where(p => p.APPROVALSTATUSID == x.approvalStatusId).Select(p => p.APPROVALSTATUSNAME).FirstOrDefault()
        //            };
        //            list.Add(data);
        //        }
                    
        //    }
        //    return list.ToList(); 
        //}


       
            public List<Blacklist> Blacklist(DateTime startDate, DateTime endDate, string customercode)
        {
            var data = (from camsol in context.TBL_LOAN_CAMSOL
                        where DbFunctions.TruncateTime(camsol.DATE) >= DbFunctions.TruncateTime(startDate) 
                        && DbFunctions.TruncateTime(camsol.DATE) <= DbFunctions.TruncateTime(endDate) 
                        && (camsol.CUSTOMERNAME.ToLower().Contains(customercode.ToLower())
                        || camsol.CUSTOMERCODE == customercode || customercode == null ||  customercode == "")
                        && camsol.CAMSOLTYPEID == (int)CamsolTypeEnum.BlackBook
                        orderby camsol.DATE descending
                        select new Blacklist
                        {
                            accountName = camsol.ACCOUNTNAME,
                            accountNumber = camsol.ACCOUNTNUMBER,
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
