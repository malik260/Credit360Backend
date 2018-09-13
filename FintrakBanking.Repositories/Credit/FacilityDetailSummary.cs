using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class FacilityDetailSummary : IFacilityDetailSummary
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private ICasaLienRepository casaLien;
        private ICustomerRepository customers;
        private IOverRideRepository overrider;

        public FacilityDetailSummary(

            FinTrakBankingContext _context,
        IGeneralSetupRepository _generalSetup,
        IAuditTrailRepository _auditTrail,
        ILoanScheduleRepository _loanSchedule,
        ILoanCovenantRepository _loanCovenant,
        IFinanceTransactionRepository _financeTransaction,
        ICasaLienRepository _casaLien,
        ICustomerRepository _customers,
        IOverRideRepository _overrider
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            loanSchedule = _loanSchedule;
            loanCovenant = _loanCovenant;
            financeTransaction = _financeTransaction;
        }
        public List<CollateralViewModel> Collateral(int loanId)
        {
            var data = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                        join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                        join ct in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                        join cs in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                        where x.LOANID == loanId
                        select new CollateralViewModel
                        {
                            collateralType = ct.COLLATERALTYPENAME,
                            collateralSubTypeName = cs.COLLATERALSUBTYPENAME,
                            collateralCode = c.COLLATERALCODE,
                            collateralValue = c.COLLATERALVALUE,
                            haircut = c.HAIRCUT

                        }).ToList();
            return data;
        }

        public LoanViewModel FacilityDetail(int loanId)
        {
            return GetDisbursedLoanByLoan(loanId);
        }

        public LoanViewModel LMSFacilityDetail(int loanId)
        {
            return GetLMSLoanByLoan(loanId);
        }
        public LoanViewModel FacilityDetailArchive(int archiveId)
        {
            return GetLoanArchive(archiveId);
        }
        public LoanViewModel OverdraftFacilityDetail(int loanId)
        {
            return GetDisbursedODByODId(loanId);
        }
        public LoanViewModel OverdraftFacilityDetailArchive(int archiveId)
        {
            return GetODByODArchive(archiveId);
        }
        public LoanViewModel OverdraftLMSFacilityDetailArchive(int archiveId)
        {
            return GetODByODArchive(archiveId);
        }

        public LoanViewModel ContingentFacilityDetail(int loanId)
        {
            return GetDisbursedContingent(loanId);
        }

        public LoanViewModel ContingentLMSFacilityDetail(int loanId)
        {
            return GetLMSContingent(loanId);
        }
        public List<LoanChargeFeeViewModel> GuarantorDetail(int loanId)
        {
            throw new NotImplementedException();
        }

        public List<LoanChargeFeeViewModel> LoanChargeFee(int loanId)
        {
            var data = (from c in context.TBL_LOAN_FEE
                        where c.LOANID == loanId //&& c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            loanChargeFeeId = c.LOANCHARGEFEEID,
                            loanId = c.LOANID,
                            chargeFeeId = c.CHARGEFEEID,
                            chargeFeeName = c.TBL_CHARGE_FEE.CHARGEFEENAME,
                            feeRateValue = c.FEERATEVALUE,
                            feeDependentAmount = c.FEEDEPENDENTAMOUNT,
                            feeAmount = c.FEEAMOUNT,
                            feeIntervalId = c.TBL_CHARGE_FEE.FEEINTERVALID,
                            feeIntervalName = c.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                            feeRate = c.FEERATEVALUE
                        }).ToList();
            return data;
        }

        public List<LoanCovenantDetailViewModel> LoanCovenantDetail(int loanId)
        {
            var data = (from a in context.TBL_LOAN_COVENANT_DETAIL
                        where a.LOANID == loanId && a.DELETED == false
                        select new LoanCovenantDetailViewModel
                        {
                            loanCovenantDetailId = a.LOANCOVENANTDETAILID,
                            covenantDetail = a.COVENANTDETAIL,
                            loanId = a.LOANID,
                            covenantTypeId = (short)a.COVENANTTYPEID,
                            frequencyTypeId = (short)a.FREQUENCYTYPEID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE,
                            casaAccountId = a.CASAACCOUNTID
                        }).ToList();
            return data;
        }

        public List<LoanPaymentSchedulePeriodicViewModel> LoanSchedule(int loanId)
        {
            var loanSchedule = (from sch in context.TBL_LOAN_SCHEDULE_PERIODIC
                                where sch.LOANID == loanId
                                select new LoanPaymentSchedulePeriodicViewModel
                                {
                                    loanId = sch.LOANID,
                                    paymentNumber = sch.PAYMENTNUMBER,
                                    paymentDate = sch.PAYMENTDATE,
                                    startPrincipalAmount = (double)sch.STARTPRINCIPALAMOUNT,
                                    periodPaymentAmount = (double)sch.PERIODPAYMENTAMOUNT,
                                    periodInterestAmount = (double)sch.PERIODINTERESTAMOUNT,
                                    periodPrincipalAmount = (double)sch.PERIODPRINCIPALAMOUNT,
                                    endPrincipalAmount = (double)sch.ENDPRINCIPALAMOUNT,
                                    interestRate = sch.INTERESTRATE,
                                    amortisedStartPrincipalAmount = (double)sch.AMORTISEDSTARTPRINCIPALAMOUNT,
                                    amortisedPeriodPaymentAmount = (double)sch.AMORTISEDPERIODPAYMENTAMOUNT,
                                    amortisedPeriodInterestAmount = (double)sch.AMORTISEDPERIODINTERESTAMOUNT,
                                    amortisedPeriodPrincipalAmount = (double)sch.AMORTISEDPERIODPRINCIPALAMOUNT,
                                    amortisedEndPrincipalAmount = (double)sch.AMORTISEDENDPRINCIPALAMOUNT,
                                    effectiveInterestRate = sch.EFFECTIVEINTERESTRATE
                                }).ToList();
            return loanSchedule;
        }
        public List<LoanPaymentSchedulePeriodicViewModel> ArchivedLoanSchedule(LoanPaymentSchedulePeriodicViewModel data)
        {
            var loanSchedule = (from sch in context.TBL_LOAN_SCHEDULE_PERIODIC_ARC
                                where sch.LOANID == data.loanId && sch.ARCHIVEBATCHCODE == data.archiveCode
                                select new LoanPaymentSchedulePeriodicViewModel
                                {
                                    loanId = sch.LOANID,
                                    archiveCode = sch.ARCHIVEBATCHCODE,
                                    paymentNumber = sch.PAYMENTNUMBER,
                                    paymentDate = sch.PAYMENTDATE,
                                    startPrincipalAmount = (double)sch.STARTPRINCIPALAMOUNT,
                                    periodPaymentAmount = (double)sch.PERIODPAYMENTAMOUNT,
                                    periodInterestAmount = (double)sch.PERIODINTERESTAMOUNT,
                                    periodPrincipalAmount = (double)sch.PERIODPRINCIPALAMOUNT,
                                    endPrincipalAmount = (double)sch.ENDPRINCIPALAMOUNT,
                                    interestRate = sch.INTERESTRATE,
                                    amortisedStartPrincipalAmount = (double)sch.AMORTISEDSTARTPRINCIPALAMOUNT,
                                    amortisedPeriodPaymentAmount = (double)sch.AMORTISEDPERIODPAYMENTAMOUNT,
                                    amortisedPeriodInterestAmount = (double)sch.AMORTISEDPERIODINTERESTAMOUNT,
                                    amortisedPeriodPrincipalAmount = (double)sch.AMORTISEDPERIODPRINCIPALAMOUNT,
                                    amortisedEndPrincipalAmount = (double)sch.AMORTISEDENDPRINCIPALAMOUNT,
                                    effectiveInterestRate = sch.EFFECTIVEINTERESTRATE,
                                    principalAmount = context.TBL_LOAN_ARCHIVE.Where(x=>x.LOANID==sch.LOANID).Select(x=>x.PRINCIPALAMOUNT).FirstOrDefault(),
                                    interestRateArc = context.TBL_LOAN_ARCHIVE.Where(x=>x.LOANID==sch.LOANID).Select(x=>x.INTERESTRATE).FirstOrDefault(),
                                    effectiveDate = context.TBL_LOAN_ARCHIVE.Where(x=>x.LOANID==sch.LOANID).Select(x=>x.EFFECTIVEDATE).FirstOrDefault(),
                                    maturityDate = context.TBL_LOAN_ARCHIVE.Where(x=>x.LOANID==sch.LOANID).Select(x=>x.MATURITYDATE).FirstOrDefault(),
                                    scheduleTypeName = context.TBL_LOAN_SCHEDULE_TYPE.Where(x=>x.SCHEDULETYPEID==sch.PERIODICSCHEDULEID).Select(x=>x.SCHEDULETYPENAME).FirstOrDefault(),
                                  //  effectiveInterestRate = context.TBL_LOAN_ARCHIVE.Where(x=>x.LOANID==sch.LOANID).Select(x=>x.eff).FirstOrDefault(),
                                    

                                }).ToList();
            return loanSchedule;
        }


        private LoanViewModel GetDisbursedODByODId(int loanId)
        {

            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.REVOLVINGLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.REVOLVINGLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   revolvingType = context.TBL_LOAN_REVOLVING_TYPE.Where(x => x.REVOLVINGTYPEID == a.REVOLVINGTYPEID).Select(x => x.REVOLVINGTYPENAME).FirstOrDefault(),
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   penalChargeAmount = a.PENALCHARGEAMOUNT,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   scheduleDayCountConvention = context.TBL_DAY_COUNT_CONVENTION.Where(x => x.DAYCOUNTCONVENTIONID == a.DAYCOUNTCONVENTIONID).Select(x => x.DAYSINAYEAR).FirstOrDefault(),
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                               }).FirstOrDefault();
            return loanDetails;
        }
        private LoanViewModel GetODByODArchive(int archiveId)
        {

            var loanDetails = (from a in context.TBL_LOAN_REVOLVING_ARCHIVE
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.REVOLVINGLOAN_ARCHIVE_ID == archiveId //&& a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.REVOLVINGLOAN_ARCHIVE_ID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   revolvingType = context.TBL_LOAN_REVOLVING_TYPE.Where(x => x.REVOLVINGTYPEID == a.REVOLVINGTYPEID).Select(x => x.REVOLVINGTYPENAME).FirstOrDefault(),
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   penalChargeAmount = a.PENALCHARGEAMOUNT,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   scheduleDayCountConvention = context.TBL_DAY_COUNT_CONVENTION.Where(x => x.DAYCOUNTCONVENTIONID == a.DAYCOUNTCONVENTIONID).Select(x => x.DAYSINAYEAR).FirstOrDefault(),
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),


                               }).FirstOrDefault();
            return loanDetails;
        }

        private LoanViewModel GetDisbursedContingent(int loanId)
        {

            var loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.CONTINGENTLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.CONTINGENTLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.CONTINGENTAMOUNT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   istenored = a.ISTENORED ? "Yes" : "No",
                                   isbankFormat = a.ISBANKFORMAT ? "Yes" : "No",
                                   productPriceIndex = ld.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == ld.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

                               }).FirstOrDefault();
            return loanDetails;
        }

        private LoanViewModel GetDisbursedLoanByLoan(int loanId)
        {
            var loanDetails = (from a in context.TBL_LOAN
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                               join f in context.TBL_PRODUCT on a.PRODUCTID equals f.PRODUCTID
                               join pt in context.TBL_PRODUCT_TYPE on f.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                               //let fpp = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).Select(x => x.FIRSTPRINCIPALPAYMENTDATE).FirstOrDefault()
                               //let ipp = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).Select(x => x.FIRSTINTERESTPAYMENTDATE).FirstOrDefault()
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               join cur in context.TBL_CURRENCY on a.CURRENCYID equals cur.CURRENCYID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ro in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals ro.STAFFID
                               join rm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals rm.STAFFID
                               where a.TERMLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                   productTypeId = f.PRODUCTTYPEID,
                                   productName = f.PRODUCTNAME,
                                   productTypeName = pt.PRODUCTTYPENAME,
                                   principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                   interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = ro.FIRSTNAME + " " + ro.MIDDLENAME + " " + ro.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = rm.FIRSTNAME + " " + rm.MIDDLENAME + " " + rm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE ,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.PRINCIPALAMOUNT,
                                   principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                   interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvedBy = a.APPROVEDBY,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   scheduleTypeId = a.SCHEDULETYPEID,
                                   scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "Yes" : "No",
                                   disbursedBy = a.DISBURSEDBY,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   customerGroupId = e.CUSTOMERGROUPID,
                                   loanTypeId = e.LOANAPPLICATIONTYPEID,
                                   //loanTypeName = e.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   equityContribution = a.EQUITYCONTRIBUTION,
                                   firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                   firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                   principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                   principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                   fixedPrincipal = a.FIXEDPRINCIPAL,
                                   profileLoan = a.PROFILELOAN,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   //   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x=>x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x=>x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                                   nostroAccountId =  a.NOSTROACCOUNTID,
                                   nostroRateCode = context.TBL_CURRENCY_RATECODE.Where(x =>x.RATECODEID== a.NOSTRORATECODEID).Select(x=>x.RATECODE).FirstOrDefault(),
                                   nostroRateAmount = a.NOSTRORATEAMOUNT,
                                   notstroCurrency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == a.NOSTROCURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                                   productPriceIndex = d.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

                               }).FirstOrDefault();

            return loanDetails;

        }
        private LoanViewModel GetLoanArchive(int archiveId)
        {
            var loanDetails = (from a in context.TBL_LOAN_ARCHIVE
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                               join f in context.TBL_PRODUCT on a.PRODUCTID equals f.PRODUCTID
                               join pt in context.TBL_PRODUCT_TYPE on f.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               join cur in context.TBL_CURRENCY on a.CURRENCYID equals cur.CURRENCYID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ro in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals ro.STAFFID
                               join rm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals rm.STAFFID
                               where a.LOANARCHIVEID == archiveId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.LOANARCHIVEID,
                                   loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   interestFrequencyTypeName = context.TBL_FREQUENCY_TYPE.Where(x=>x.FREQUENCYTYPEID==a.INTERESTFREQUENCYTYPEID).Select(x=>x.MODE).FirstOrDefault(),
                                   productTypeId = f.PRODUCTTYPEID,
                                   productName = f.PRODUCTNAME,
                                   productTypeName = pt.PRODUCTTYPENAME,
                                   principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                   interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = ro.FIRSTNAME + " " + ro.MIDDLENAME + " " + ro.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = rm.FIRSTNAME + " " + rm.MIDDLENAME + " " + rm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.PRINCIPALAMOUNT,
                                   principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                   interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvedBy = a.APPROVEDBY,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   scheduleTypeId = a.SCHEDULETYPEID,
                                   scheduleTypeName = context.TBL_LOAN_SCHEDULE_TYPE.Where(x=>x.SCHEDULETYPEID==a.SCHEDULETYPEID).Select(x=>x.SCHEDULETYPENAME).FirstOrDefault(),
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disbursedBy = a.DISBURSEDBY,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   customerGroupId = e.CUSTOMERGROUPID,
                                   loanTypeId =  e.LOANAPPLICATIONTYPEID,
                                   loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(x=>x.LOANAPPLICATIONTYPEID==e.LOANAPPLICATIONTYPEID).Select(x=>x.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                   equityContribution = a.EQUITYCONTRIBUTION,
                                   firstPrincipalPaymentDate1 = (DateTime)a.FIRSTPRINCIPALPAYMENTDATE ,
                                   firstInterestPaymentDate1 = (DateTime)a.FIRSTINTERESTPAYMENTDATE,
                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                   principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                   principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                   fixedPrincipal = a.FIXEDPRINCIPAL,
                                   profileLoan = a.PROFILELOAN,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.LOANARCHIVEID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                   lastRestructureDate=a.LASTRESTRUCTUREDATE,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x => x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                                   productPriceIndex = d.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

                               }).FirstOrDefault();

            return loanDetails;

        }

        public List<LoanViewModel> LoanSearch(int loanSystemTypeId, string searchQuery)
        {

            // var applicationDate = generalSetup.GetApplicationDate();

            try
            {
                List<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        allFilteredLoan = SearchTermLoan(searchQuery);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        allFilteredLoan = SearchRevolvingLoan(searchQuery);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                    {
                        allFilteredLoan = SearchContigentLoan(searchQuery);
                    }
                }

                //var x = allFilteredLoan.ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<LoanViewModel> LMSLoanSearch(int loanSystemTypeId, string searchQuery)
        {

            // var applicationDate = generalSetup.GetApplicationDate();

            try
            {
                List<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        allFilteredLoan = SearchLMSLoan(searchQuery);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        allFilteredLoan = SearchRevolvingLMSLoan(searchQuery);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                    {
                        allFilteredLoan = SearchContigentLMSLoan(searchQuery);
                    }
                }

                //var x = allFilteredLoan.ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<LoanViewModel> SearchTermLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   
                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                      // applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                     //  productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = context.TBL_PRODUCT.Where(x=>x.PRODUCTID==a.PRODUCTID).Select(x=>x.PRODUCTNAME).FirstOrDefault(),
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                       outstandingPrincipal = a.OUTSTANDINGPRINCIPAL

                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchLMSLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join l in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals l.LOANID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))

                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       // applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       //  productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                       outstandingPrincipal = a.OUTSTANDINGPRINCIPAL

                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchContigentLMSLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join l in context.TBL_LMSR_APPLICATION_DETAIL on a.CONTINGENTLOANID equals l.LOANID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = 1,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchRevolvingLMSLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join l in context.TBL_LMSR_APPLICATION_DETAIL on a.REVOLVINGLOANID equals l.LOANID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> RelatedFacilities(string relaltedLoanRefNo, string loanRefNo)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.RELATED_LOAN_REFERENCE_NUMBER == relaltedLoanRefNo
                                   && a.LOANREFERENCENUMBER != loanRefNo
                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                       interestRate = a.INTERESTRATE,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   });
            return allFilteredLoan.ToList();
        }
        public List<LoanViewModel> ArchiveLoanFacilityDetail(int loanId)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_ARCHIVE
                                   where a.LOANID == loanId
                                   select new LoanViewModel
                                   {
                                       archiveCode = a.ARCHIVEBATCHCODE,
                                       loadArchiveId = a.LOANARCHIVEID,
                                       loanId = a.LOANID,
                                     //  archiveCode =a.ARCHIVEBATCHCODE,
                                     lastRestructureDate = a.LASTRESTRUCTUREDATE,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       isTermLoam = true,
                                       isOD = false,
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchRevolvingLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> RelatedRevolvingLoan(string relatedLoanFreNo, string loanRefNo)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.RELATED_LOAN_REFERENCE_NUMBER == relatedLoanFreNo
                                   && a.LOANREFERENCENUMBER != loanRefNo
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       interestRate = a.INTERESTRATE,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchContigentLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = 1,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> RelatedContigentLoan(string relatedLoanRef, string loanRefNo)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.RELATED_LOAN_REFERENCE_NUMBER == relatedLoanRef
                                   && a.LOANREFERENCENUMBER != loanRefNo
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       interestRate = 1,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                      loanSystemTypeId=a.LOANSYSTEMTYPEID
                                   });
            return allFilteredLoan.ToList();
        }

        public List<ProductType> ProductType()
        {
            //  List<int> productTypeId = new List<int> { (int)LoanProductTypeEnum.TermLoan, (int)LoanProductTypeEnum.RevolvingLoan, (int)LoanProductTypeEnum.ContingentLiability };
            return (from x in context.TBL_LOAN_SYSTEM_TYPE
                        // where productTypeId.Contains(x.LOANSYSTEMTYPEID)
                    select new ProductType
                    {
                        loanSystemTypeId = x.LOANSYSTEMTYPEID,
                        loanSystemTypeName = x.LOANSYSTEMTYPENAME
                    }).ToList();
        }

        public List<LoanViewModel> SearchAllRevolvingLoan(int loanId)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING_ARCHIVE
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.REVOLVINGLOANID == loanId
                                   select new LoanViewModel
                                   {
                                       loadArchiveId = a.REVOLVINGLOAN_ARCHIVE_ID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       isTermLoam = false,
                                       isOD = true,
                                   });
            return allFilteredLoan.ToList();
        }

        public List<LoanViewModel> ArchiveRevolvingLoanFacilityDetail(int loanId)
        {
            throw new NotImplementedException();
        }

        public List<LoanViewModel> RelatedFacility(int loanSystemTypeId, string relatedLoanRefNo, string loanRefN)
        {
            // var applicationDate = generalSetup.GetApplicationDate();

            try
            {
                List<LoanViewModel> allFilteredLoan = null;

                if (!string.IsNullOrWhiteSpace(relatedLoanRefNo.Trim()))
                {
                    if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        allFilteredLoan = RelatedFacilities(relatedLoanRefNo, loanRefN);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        allFilteredLoan = RelatedRevolvingLoan(relatedLoanRefNo, loanRefN);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                    {
                        allFilteredLoan = RelatedContigentLoan(relatedLoanRefNo, loanRefN);
                    }
                }

                //var x = allFilteredLoan.ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public LoanViewModel RelatedFacilityDetail(string relatedLaonRefNo)
        {
            var loanDetails = (from a in context.TBL_LOAN
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                               join f in context.TBL_PRODUCT on a.PRODUCTID equals f.PRODUCTID
                               join pt in context.TBL_PRODUCT_TYPE on f.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               //let fpp = context.TBL_LOAN.Where(x => x.RELATED_LOAN_REFERENCE_NUMBER == relatedLaonRefNo).Select(x => x.FIRSTPRINCIPALPAYMENTDATE).FirstOrDefault()
                               //let ipp = context.TBL_LOAN.Where(x => x.RELATED_LOAN_REFERENCE_NUMBER == relatedLaonRefNo).Select(x => x.FIRSTINTERESTPAYMENTDATE).FirstOrDefault()
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               join cur in context.TBL_CURRENCY on a.CURRENCYID equals cur.CURRENCYID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ro in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals ro.STAFFID
                               join rm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals rm.STAFFID
                               where a.RELATED_LOAN_REFERENCE_NUMBER == relatedLaonRefNo && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                   productTypeId = f.PRODUCTTYPEID,
                                   productName = f.PRODUCTNAME,
                                   productTypeName = pt.PRODUCTTYPENAME,
                                   principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                   interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = ro.FIRSTNAME + " " + ro.MIDDLENAME + " " + ro.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = rm.FIRSTNAME + " " + rm.MIDDLENAME + " " + rm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.PRINCIPALAMOUNT,
                                   principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                   interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvedBy = a.APPROVEDBY,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   scheduleTypeId = a.SCHEDULETYPEID,
                                   scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "Yes" : "No",
                                   disbursedBy = a.DISBURSEDBY,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   customerGroupId = e.CUSTOMERGROUPID,
                                   loanTypeId = e.LOANAPPLICATIONTYPEID,
                                   //loanTypeName = e.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   equityContribution = a.EQUITYCONTRIBUTION,
                                   firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                   firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                   principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                   principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                   fixedPrincipal = a.FIXEDPRINCIPAL,
                                   profileLoan = a.PROFILELOAN,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   //   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x=>x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x=>x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                               }).FirstOrDefault();

            return loanDetails;

        }

        public LoanViewModel RelatedOverdraftFacilityDetail(string RelatedLoanRefNo)
        {
            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.RELATED_LOAN_REFERENCE_NUMBER== RelatedLoanRefNo
                               select new LoanViewModel
                               {
                                   loanId = a.REVOLVINGLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   revolvingType = context.TBL_LOAN_REVOLVING_TYPE.Where(x => x.REVOLVINGTYPEID == a.REVOLVINGTYPEID).Select(x => x.REVOLVINGTYPENAME).FirstOrDefault(),
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   penalChargeAmount = a.PENALCHARGEAMOUNT,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   scheduleDayCountConvention = context.TBL_DAY_COUNT_CONVENTION.Where(x => x.DAYCOUNTCONVENTIONID == a.DAYCOUNTCONVENTIONID).Select(x => x.DAYSINAYEAR).FirstOrDefault(),
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                               }).FirstOrDefault();
            return loanDetails;
        }

        public LoanViewModel RelatedContingentFacilityDetail(string RelatedLoanRefNo)
        {
            var loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.RELATED_LOAN_REFERENCE_NUMBER == RelatedLoanRefNo && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.CONTINGENTLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.CONTINGENTAMOUNT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   istenored = a.ISTENORED ? "Yes" : "No",
                                   isbankFormat = a.ISBANKFORMAT ? "Yes" : "No",

                               }).FirstOrDefault();
            return loanDetails;
        }

        public List<LoanViewModel> TransactionDetail(string loanRefNo)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                List<LoanViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                                   join l in context.TBL_LOAN on a.SOURCEREFERENCENUMBER equals l.LOANREFERENCENUMBER
                                                   join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                                   join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                                   where a.SOURCEREFERENCENUMBER== loanRefNo         
                                                   && a.TBL_CHART_OF_ACCOUNT.GLCLASSID == (short)GLClassEnum.CASA
                                                   orderby a.POSTEDDATE, a.TRANSACTIONID descending

                                                   select new LoanViewModel()
                                                   {
                                                       postedByStaffId = a.POSTEDBY,
                                                       branchId = a.SOURCEBRANCHID,
                                                       branchName = a.TBL_BRANCH.BRANCHNAME,
                                                       batchNo = a.BATCHCODE,
                                                       companyName = a.TBL_COMPANY.NAME,
                                                       creditAmount = a.CREDITAMOUNT,
                                                       debitAmount = a.DEBITAMOUNT,
                                                       description = a.DESCRIPTION,
                                                       valueDate = a.VALUEDATE,
                                                       postedDate = a.POSTEDDATE,
                                                       postedTime = a.POSTEDDATETIME,
                                                       postCurrency = a.TBL_CURRENCY.CURRENCYNAME,
                                                       currencyRate = a.CURRENCYRATE,
                                                       productName = p.PRODUCTNAME,
                                                       customerCode = c.CUSTOMERCODE,
                                                       sourceReferenceNumber = a.SOURCEREFERENCENUMBER
                                                   }).ToList();
                return data;
            }

        }
        public List<LoanViewModel> ContingentUtilization(int contingentId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                List<LoanViewModel> data = (from a in context.TBL_LOAN_CONTINGENT_USAGE
                                            join c in context.TBL_LOAN_CONTINGENT on a.CONTINGENTLOANID equals c.CONTINGENTLOANID
                                            where a.CONTINGENTLOANID == contingentId
                                            select new LoanViewModel()
                                            {
                                                loanReferenceNumber = c.LOANREFERENCENUMBER,
                                                requestedAmount = a.AMOUNTREQUESTED,
                                                ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID==a.APPROVALSTATUSID).Select(x=>x.APPROVALSTATUSNAME).FirstOrDefault(),
                                                remark = a.REMARK,
                                                dateTimeCreated = a.DATETIMECREATED
                                            }).ToList();
                return data;
            }

        }


        public List<LoanViewModel> DailyInterestAccrual(DateTime startDate, DateTime endDate, string loanReferenceNumber)
        {
            List<LoanViewModel> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = (from a in context.TBL_DAILY_ACCRUAL
                        where a.DATE >= startDate && a.DATE <= endDate
                        && a.REFERENCENUMBER==loanReferenceNumber
                        orderby a.DAILYACCURALID descending
                        select new LoanViewModel()
                        {
                            baseReferenceNumber = a.BASEREFERENCENUMBER,
                           // categoryName = context.TBL_DAILY_ACCRUAL_CATEGORY.Where(x => x.CATEGORYID == a.CATEGORYID).Select(x => x.CATEGORYNAME).FirstOrDefault(),
                            currencyName = context.TBL_CURRENCY.Where(x => x.CURRENCYID == a.CURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                            dailyAccrualAmount = a.DAILYACCURALAMOUNT,
                            date = a.DATE,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            mainAmount = a.MAINAMOUNT,
                            loanReferenceNumber = a.REFERENCENUMBER,
                        }).ToList();
            }
            return data;
        }





        #region

        private LoanViewModel GetLMSLoanByLoan(int loanId)
        {
            var loanDetails = (from a in context.TBL_LOAN
                               join d in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals d.LOANID
                               join e in context.TBL_LMSR_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                               join f in context.TBL_PRODUCT on a.PRODUCTID equals f.PRODUCTID
                               join pt in context.TBL_PRODUCT_TYPE on f.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               join cur in context.TBL_CURRENCY on a.CURRENCYID equals cur.CURRENCYID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ro in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals ro.STAFFID
                               join rm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals rm.STAFFID
                               where a.TERMLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   loanApplicationId = d.LOANREVIEWAPPLICATIONID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                   productTypeId = f.PRODUCTTYPEID,
                                   productName = f.PRODUCTNAME,
                                   productTypeName = pt.PRODUCTTYPENAME,
                                   principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                   interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = ro.FIRSTNAME + " " + ro.MIDDLENAME + " " + ro.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = rm.FIRSTNAME + " " + rm.MIDDLENAME + " " + rm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.PRINCIPALAMOUNT,
                                   principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                   interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvedBy = a.APPROVEDBY,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   scheduleTypeId = a.SCHEDULETYPEID,
                                   scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "Yes" : "No",
                                   disbursedBy = a.DISBURSEDBY,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   customerGroupId = e.CUSTOMERGROUPID,
                                   //loanTypeId = e.LOANAPPLICATIONTYPEID,
                                   //loanTypeName = e.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   equityContribution = a.EQUITYCONTRIBUTION,
                                   firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                   firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                   principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                   principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                   fixedPrincipal = a.FIXEDPRINCIPAL,
                                   profileLoan = a.PROFILELOAN,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   //   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x=>x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x=>x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                                   nostroAccountId = a.NOSTROACCOUNTID,
                                   nostroRateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == a.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault(),
                                   nostroRateAmount = a.NOSTRORATEAMOUNT,
                                   notstroCurrency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == a.NOSTROCURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                               }).FirstOrDefault();

            return loanDetails;

        }


        private LoanViewModel GetLMSContingent(int loanId)
        {

            var loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LMSR_APPLICATION_DETAIL on a.CONTINGENTLOANID equals ld.LOANID
                               join lp in context.TBL_LMSR_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               //join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.CONTINGENTLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.CONTINGENTLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.CONTINGENTAMOUNT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   //loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   //loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   istenored = a.ISTENORED ? "Yes" : "No",
                                   isbankFormat = a.ISBANKFORMAT ? "Yes" : "No",

                               }).FirstOrDefault();
            return loanDetails;
        }

        private LoanViewModel GetOverDrftLMSODetails(int archiveId)
        {

            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LMSR_APPLICATION_DETAIL on a.REVOLVINGLOANID equals ld.LOANID
                               join lp in context.TBL_LMSR_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               //join at in context.TBL_LOAN_APPLICATION_TYPE on lp.la equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.REVOLVINGLOANID == archiveId //&& a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.REVOLVINGLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                  // loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                  // loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   //outstandingInterest = a.OUTSTANDINGINTEREST,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,

                                   revolvingType = context.TBL_LOAN_REVOLVING_TYPE.Where(x => x.REVOLVINGTYPEID == a.REVOLVINGTYPEID).Select(x => x.REVOLVINGTYPENAME).FirstOrDefault(),
                                   RelatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   pastDuePrincipal = a.PASTDUEPRINCIPAL,
                                   pastDueInterest = a.PASTDUEINTEREST,
                                   interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                   interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                   penalChargeAmount = a.PENALCHARGEAMOUNT,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   scheduleDayCountConvention = context.TBL_DAY_COUNT_CONVENTION.Where(x => x.DAYCOUNTCONVENTIONID == a.DAYCOUNTCONVENTIONID).Select(x => x.DAYSINAYEAR).FirstOrDefault(),
                                   externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   // internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                                   userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),


                               }).FirstOrDefault();
            return loanDetails;
        }

        public List<LoanCovenantDetailViewModel> LMSLoanCovenantDetail(int loanId)
        {
            var data = (from a in context.TBL_LMSR_APPLICATION_COVENANT
                        where a.LOANREVIEWAPPLICATIONID == loanId && a.DELETED == false
                        select new LoanCovenantDetailViewModel
                        {
                            loanCovenantDetailId = a.LOANCOVENANTDETAILID,
                            covenantDetail = a.COVENANTDETAIL,
                            loanId = a.LOANREVIEWAPPLICATIONID,
                            covenantTypeId = (short)a.COVENANTTYPEID,
                            frequencyTypeId = (short)a.FREQUENCYTYPEID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE,
                            casaAccountId = a.CASAACCOUNTID
                        }).ToList();
            return data;
        }
        #endregion
    }
}
