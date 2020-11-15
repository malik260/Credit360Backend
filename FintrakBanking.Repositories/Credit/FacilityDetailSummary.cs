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
using FintrakBanking.ViewModels.Setups.General;
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
        //private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        //private ICasaLienRepository casaLien;
        //private ICustomerRepository customers;
        //private IOverRideRepository overrider;
        private IFinanceTransactionRepository transRepo;

        public FacilityDetailSummary(

            FinTrakBankingContext _context,
        IGeneralSetupRepository _generalSetup,
        IAuditTrailRepository _auditTrail,
        ILoanScheduleRepository _loanSchedule,
        ILoanCovenantRepository _loanCovenant,
        IFinanceTransactionRepository _financeTransaction,
        ICasaLienRepository _casaLien,
        ICustomerRepository _customers,
        IOverRideRepository _overrider,
        IFinanceTransactionRepository _transRepo
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            loanSchedule = _loanSchedule;
            loanCovenant = _loanCovenant;
            financeTransaction = _financeTransaction;
            transRepo=_transRepo;
        }

        public List<CollateralViewModel> Collateral(int loanId,int loanSystemTypeId)
        {
            var data = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                        join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                        join ct in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                        join cs in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                        where x.LOANID == loanId && x.LOANSYSTEMTYPEID == loanSystemTypeId
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

        public List<CollateralViewModel> CollateralByLoanId(int loanId)
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

        public LoanViewModel ThirdPartyFacilityDetails(string loanReferenceNumber)
        {
            return GetThirdPartyLoansByReferenceNumber(loanReferenceNumber);
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

        public List<LoanChargeFeeViewModel> LoanChargeFee(int loanId, short loanSystemTypeId)
        {
            var data = (from c in context.TBL_LOAN_FEE
                        where c.LOANID == loanId && c.LOANSYSTEMTYPEID == loanSystemTypeId && c.DELETED == false
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

            var lisProdFeeViewModel = new List<LoanChargeFeeViewModel>();
            foreach (var item in data)
            {
                var chargeFeeDetail = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == item.chargeFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax).FirstOrDefault();
                if (chargeFeeDetail != null)
                {
                    var prodFeeView = new LoanChargeFeeViewModel()
                    {
                        //feeName = chargeFeeDetail.DESCRIPTION,
                        chargeFeeName = chargeFeeDetail.DESCRIPTION, // chargeFeeDetail.TBL_CHARGE_FEE.CHARGEFEENAME,
                        //loanApplicationDetailId = loanApplicationDetailId,
                        chargeFeeId = chargeFeeDetail.CHARGEFEEID,
                        //feeDependentAmount = chargeFeeDetail.,
                        feeRateValue = (decimal)chargeFeeDetail.VALUE,
                        feeAmount = (item.feeAmount * (decimal)chargeFeeDetail.VALUE) / 100,
                        feeIntervalName = chargeFeeDetail.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                        isIntegralFee = chargeFeeDetail.TBL_CHARGE_FEE.ISINTEGRALFEE,
                        feeIntervalId = chargeFeeDetail.TBL_CHARGE_FEE.FEEINTERVALID,
                        feeRate = (decimal)chargeFeeDetail.VALUE,
                        //valueBase = "Rate(%)",
                        dealTypeId = (short)ChargeFeeDetailTypeEnum.Tax
                    };

                    lisProdFeeViewModel.Add(prodFeeView);
                }
            }

            data = data.Union(lisProdFeeViewModel).OrderBy(x => x.chargeFeeName).ToList();
            return data;
        }

        public List<ProductFeeViewModel> GetLoanProductFeesByFacilityId(int loanApplicationDetailId)
        {
            var facilityDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            var loanAppProdFee = new List<ProductFeeViewModel>();

            if (facilityDetail != null)
            {
                loanAppProdFee = (from fa in context.TBL_LOAN_APPLICATION_DETL_FEE
                                      where fa.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                      && fa.DELETED == false
                                      select new ProductFeeViewModel
                                      {
                                          feeName = fa.TBL_CHARGE_FEE.CHARGEFEENAME,
                                          loanApplicationDetailId = fa.LOANAPPLICATIONDETAILID,
                                          chargeFeeId = fa.CHARGEFEEID,
                                          consessionReason = fa.CONSESSIONREASON,
                                          approvalStatusId = fa.APPROVALSTATUSID,
                                          defaultfeeRateValue = fa.DEFAULT_FEERATEVALUE,
                                          recommededFeeRateValue = fa.RECOMMENDED_FEERATEVALUE,
                                          feeRateValue = fa.RECOMMENDED_FEERATEVALUE,
                                          feeAmount = (facilityDetail.APPROVEDAMOUNT * fa.RECOMMENDED_FEERATEVALUE) / 100,
                                          feeIntervalName = fa.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                                          isIntegralFee = fa.TBL_CHARGE_FEE.ISINTEGRALFEE,
                                          isRecurring = fa.TBL_CHARGE_FEE.RECURRING,
                                          valueBase = "Rate(%)",
                                          dealTypeId = 0
                                      }).ToList();
            }
            

            var lisProdFeeViewModel = new List<ProductFeeViewModel>();
            foreach (var item in loanAppProdFee)
            {
                var chargeFeeDetail = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == item.chargeFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax).FirstOrDefault();
                if (chargeFeeDetail != null)
                {
                    var prodFeeView = new ProductFeeViewModel()
                    {
                        feeName = chargeFeeDetail.DESCRIPTION,
                        loanApplicationDetailId = loanApplicationDetailId,
                        chargeFeeId = chargeFeeDetail.CHARGEFEEID,
                        recommededFeeRateValue = (decimal)chargeFeeDetail.VALUE,
                        feeRateValue = (decimal)chargeFeeDetail.VALUE,
                        feeAmount = (item.feeAmount * (decimal)chargeFeeDetail.VALUE) / 100,
                        feeIntervalName = chargeFeeDetail.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                        isIntegralFee = chargeFeeDetail.TBL_CHARGE_FEE.ISINTEGRALFEE,
                        isRecurring = chargeFeeDetail.TBL_CHARGE_FEE.RECURRING,
                        valueBase = "Rate(%)",
                        dealTypeId = (short)ChargeFeeDetailTypeEnum.Tax
                    };

                    lisProdFeeViewModel.Add(prodFeeView);
                }
            }

            loanAppProdFee = loanAppProdFee.Union(lisProdFeeViewModel).OrderBy(x => x.feeName).ToList();

            return loanAppProdFee;
        }

        public List<LoanReviewIrregularScheduleViewModel> GetLoanIregularInput(int loanReviewOperationId)
        {
            var loanSchedule = (from sch in context.TBL_LOAN_REVIEW_OPRATN_IREG_SC
                                where sch.LOANREVIEWOPERATIONID == loanReviewOperationId
                                orderby sch.PAYMENTDATE ascending
                                select new LoanReviewIrregularScheduleViewModel
                                {
                                    LoanReviewOperationId = sch.LOANREVIEWOPERATIONID,
                                    PaymentDate = sch.PAYMENTDATE,
                                    PaymentAmount = sch.PAYMENTAMOUNT,
                                    //loan = sch.LOANID,
                                    //paymentNumber = sch.PAYMENTNUMBER,
                                    //paymentDate = sch.PAYMENTDATE,
                                    //startPrincipalAmount = (double)sch.STARTPRINCIPALAMOUNT,
                                    //periodPaymentAmount = (double)sch.PERIODPAYMENTAMOUNT,
                                    //periodInterestAmount = (double)sch.PERIODINTERESTAMOUNT,
                                    //periodPrincipalAmount = (double)sch.PERIODPRINCIPALAMOUNT,
                                    //endPrincipalAmount = (double)sch.ENDPRINCIPALAMOUNT,
                                    //interestRate = sch.INTERESTRATE,
                                    //amortisedStartPrincipalAmount = (double)sch.AMORTISEDSTARTPRINCIPALAMOUNT,
                                    //amortisedPeriodPaymentAmount = (double)sch.AMORTISEDPERIODPAYMENTAMOUNT,
                                    //amortisedPeriodInterestAmount = (double)sch.AMORTISEDPERIODINTERESTAMOUNT,
                                    //amortisedPeriodPrincipalAmount = (double)sch.AMORTISEDPERIODPRINCIPALAMOUNT,
                                    //amortisedEndPrincipalAmount = (double)sch.AMORTISEDENDPRINCIPALAMOUNT,
                                    //effectiveInterestRate = sch.EFFECTIVEINTERESTRATE
                                }).ToList();
            return loanSchedule;
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
                                orderby sch.PAYMENTDATE ascending
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
                                orderby sch.PAYMENTDATE ascending
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
            decimal overDraftLimit = 0;
            decimal availableBalance = 0;

            var odDetail = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).Select(x=>x).FirstOrDefault();
            if (odDetail!=null)
            {
                 availableBalance = transRepo.GetCASABalance(odDetail.CASAACCOUNTID).availableBalance;
                 overDraftLimit = odDetail.OVERDRAFTLIMIT;

            }

            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join lr in context.TBL_LOAN_BOOKING_REQUEST on a.LOAN_BOOKING_REQUESTID equals lr.LOAN_BOOKING_REQUESTID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.REVOLVINGLOANID == loanId 
                               //&& a.ISDISBURSED == true
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
                                   loanPurpose = ld.LOANPURPOSE,
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,

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
                                   crmsCode = lr.CRMSCODE
                               }).FirstOrDefault();

            loanDetails.availableBalance = availableBalance;

            if (availableBalance >= 0)
            {
                loanDetails.overdraftUndrawnAmount = overDraftLimit;
                loanDetails.overdraftDrawnAmount = 0;
            }
            else
            {
                //overDraftDetail.overDraft = overDraftLimit - Math.Abs(availableBalance);
                loanDetails.overdraftUndrawnAmount = overDraftLimit - Math.Abs(availableBalance);
                loanDetails.overdraftDrawnAmount = Math.Abs(availableBalance);
            }

            loanDetails.pastDueDays = loanDetails.pastDueDate != null ? DateTime.Now > loanDetails.pastDueDate ? (DateTime.Now.Subtract(loanDetails.pastDueDate.Value).Days) : 0 : 0;
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                               //join lr in context.TBL_LOAN_BOOKING_REQUEST on a.LOAN_BOOKING_REQUESTID equals lr.LOAN_BOOKING_REQUESTID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lr in context.TBL_LOAN_BOOKING_REQUEST on ld.LOANAPPLICATIONDETAILID equals lr.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.CONTINGENTLOANID == loanId 
                               //&& a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.CONTINGENTLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   loanPurpose =ld.LOANPURPOSE,
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
                                   contigentOutstandingPrincipal = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Sum(x => x.CONTINGENTOUTSTANDINGPRINCIPAL),
                                   totalPrepayment = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANID == a.CONTINGENTLOANID && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Sum(x => x.PREPAYMENT),

                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   istenored = a.ISTENORED ? "Yes" : "No",
                                   isbankFormat = a.ISBANKFORMAT ? "Yes" : "No",
                                   productPriceIndex = ld.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == ld.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",
                                   crmsCode = lr.CRMSCODE
                               }).FirstOrDefault();

            loanDetails.pastDueDays = loanDetails.pastDueDate != null ? DateTime.Now > loanDetails.pastDueDate ? (DateTime.Now.Subtract(loanDetails.pastDueDate.Value).Days) : 0 : 0;
            return loanDetails;
        }

        private LoanViewModel GetDisbursedLoanByLoan(int loanId)
        {
            var loanDetails = (from a in context.TBL_LOAN
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join lr in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals lr.LOANAPPLICATIONDETAILID
                               //join lr in context.TBL_LOAN_BOOKING_REQUEST on a.LOAN_BOOKING_REQUESTID equals lr.LOAN_BOOKING_REQUESTID
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
                               where a.TERMLOANID == loanId 
                               //&& a.ISDISBURSED == true
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
                                   loanPurpose = d.LOANPURPOSE,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   principalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
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
                                   // casaAccountNumber1 =context.TBL_CASA.Where(o=>o.CASAACCOUNTID== a.CASAACCOUNTID).Select(o=>o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                   casaAccountNumber2 = context.TBL_CASA.Where(o => o.CASAACCOUNTID == a.CASAACCOUNTID2).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   //   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x=>x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x=>x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueDate = a.PASTDUEDATE,
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
                                   productPriceIndex = d.PRODUCTPRICEINDEXID != null ? context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",
                                   //accrualedAmount = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == a.TERMLOANID && x.DATE == applicationDate).Select(aci => aci.ACCRUEDINTEREST).FirstOrDefault(),  //context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).FirstOrDefault().ACCRUEDINTEREST, //d.ACCRUEDINTEREST,
                                   //productPriceIndex = d.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",
                                   crmsCode = lr.CRMSCODE
                               }).FirstOrDefault();
            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            loanDetails.accrualedAmount = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == loanDetails.loanId && x.DATE == applicationDate).Select(aci => aci.ACCRUEDINTEREST).FirstOrDefault();
            loanDetails.totalRepayment = PresentRepayments(loanDetails.loanReferenceNumber, loanDetails.companyId);
            loanDetails.pastDueDays = loanDetails.pastDueDate != null ? DateTime.Now > loanDetails.pastDueDate ? (DateTime.Now.Subtract(loanDetails.pastDueDate.Value).Days) : 0 : 0;

            return loanDetails;

        }

        private LoanViewModel GetThirdPartyLoansByReferenceNumber(string loanReferenceNumber)
        {
            var loanDetails = (from a in context.TBL_LOAN_EXTERNAL
                              // join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               //join lr in context.TBL_LOAN_BOOKING_REQUEST on a.LOAN_BOOKING_REQUESTID equals lr.LOAN_BOOKING_REQUESTID
                               //join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
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
                               where a.LOANREFERENCENUMBER == loanReferenceNumber
                               //&& a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.EXTERNALLOANID,
                                   //loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   loanPurpose = "N/A", //d.LOANPURPOSE,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber =  "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   //principalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   //interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
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
                                   // casaAccountNumber1 =context.TBL_CASA.Where(o=>o.CASAACCOUNTID== a.CASAACCOUNTID).Select(o=>o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                   casaAccountNumber2 = context.TBL_CASA.Where(o => o.CASAACCOUNTID == a.CASAACCOUNTID2).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   //customerGroupId = e.CUSTOMERGROUPID,
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
                                   //isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                   ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                   approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                                   approvedComment = a.APPROVERCOMMENT,
                                   loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                   //   scheduleDayCountConvention = context.TBL_LOAN_SCHEDULE_DAILY.Where(x=>x.DAILYSCHEDULEID == a.SCHEDULEDAYCOUNTCONVENTIONID).Select(x=>x.BALLONAMOUNT).FirstOrDefault(),
                                   pastDueDate = a.PASTDUEDATE,
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
                                   //productPriceIndex = d.PRODUCTPRICEINDEXID != null ? context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",
                                   //accrualedAmount = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == a.TERMLOANID && x.DATE == applicationDate).Select(aci => aci.ACCRUEDINTEREST).FirstOrDefault(),  //context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).FirstOrDefault().ACCRUEDINTEREST, //d.ACCRUEDINTEREST,
                                   //productPriceIndex = d.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == d.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",
                                   //crmsCode = lr.CRMSCODE
                               }).FirstOrDefault();
            var applicationDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;
            if(loanDetails != null)
            {
                loanDetails.accrualedAmount = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == loanDetails.loanId && x.DATE == applicationDate).Select(aci => aci.ACCRUEDINTEREST).FirstOrDefault();
                loanDetails.totalRepayment = PresentRepayments(loanDetails.loanReferenceNumber, loanDetails.companyId);
                loanDetails.pastDueDays = loanDetails.pastDueDate != null ? DateTime.Now > loanDetails.pastDueDate ? (DateTime.Now.Subtract(loanDetails.pastDueDate.Value).Days) : 0 : 0;
            }

            return loanDetails;

        }

        private decimal PresentRepayments(string loanRefNo, int compoanyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                decimal repayments = 0;

                List<LoanViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                            where a.SOURCEREFERENCENUMBER == loanRefNo
                                            && a.TBL_CHART_OF_ACCOUNT.GLCLASSID == (short)GLClassEnum.CASA
                                            && a.OPERATIONID != (int)OperationsEnum.TermLoanBooking && a.DEBITAMOUNT > 0
                                            && a.COMPANYID == compoanyId

                                            select new LoanViewModel()
                                            {
                                                debitAmount = a.DEBITAMOUNT,

                                            }).ToList();
                foreach (var x in data)
                    repayments = repayments + x.debitAmount;

                return repayments;
            }

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
                                   principalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
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
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.LineFacility)
                    {
                        allFilteredLoan = SearchLoanLine(searchQuery);
                    }
                    else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ExternalFacility)
                    {
                        allFilteredLoan = SearchExternalLoan(searchQuery);
                    }
                }

                //var x = allFilteredLoan.ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                throw ex;
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
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToUpper();
            }
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where //a.ISDISBURSED == true && 
                                   (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToUpper().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToUpper().Contains(searchQuery) ||
                                   b.LASTNAME.ToUpper().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToUpper().Contains(searchQuery))

                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       // applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       loanApplicationDetailId  = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONDETAILID,
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
            try
            {
                var output = allFilteredLoan.ToList();

                return output;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private List<LoanViewModel> SearchLMSLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join l in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals l.LOANID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.ToLower().Contains(searchQuery.ToUpper()) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))

                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.ToLower().Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.ToLower().Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                   where 
                                   //a.ISDISBURSED == true && 
                                   (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       loanApplicationDetailId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONDETAILID,
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
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }
            List<short> loanStatus = new List<short>();
            loanStatus.Add((short)LoanStatusEnum.Cancelled);
            loanStatus.Add((short)LoanStatusEnum.Terminated);


            //var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
            //                       join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
            //                       join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
            //                       where a.ISDISBURSED == true && //a.LOANSTATUSID != 7 && 
            //                       (a.LOANREFERENCENUMBER.ToLower().Contains(searchQuery.Trim()) ||
            //                       b.CUSTOMERCODE.ToLower().Contains(searchQuery.Trim()) ||
            //                       b.FIRSTNAME.ToLower().Contains(searchQuery.Trim()) ||
            //                       b.LASTNAME.ToLower().Contains(searchQuery.Trim()) ||
            //                       c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery.Trim()))
            //                       && !loanStatus.Contains(a.LOANSTATUSID)

            //                       )




            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where 
                                   //a.ISDISBURSED == true && 
                                   (a.LOANREFERENCENUMBER.ToLower().Contains(searchQuery.Trim()) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery.Trim()) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery.Trim()) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery.Trim()) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery.Trim()))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       loanApplicationDetailId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONDETAILID,
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
        
        private List<LoanViewModel> SearchLoanLine(string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery)) searchQuery = searchQuery.ToUpper();

            var allFilteredLoan = (from a in context.TBL_LOAN_APPLICATION
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && d.STATUSID == 2
                                   && (a.APPLICATIONREFERENCENUMBER.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.CUSTOMERCODE.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.FIRSTNAME.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.LASTNAME.ToUpper().Contains(searchQuery.Trim())
                                   )
                                   select new LoanViewModel
                                   {
                                       loanId = d.LOANAPPLICATIONDETAILID,
                                       customerId = d.CUSTOMERID,
                                       productId = d.APPROVEDPRODUCTID,
                                       customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                       loanApplicationId = d.LOANAPPLICATIONID,
                                       loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                       interestRate = 1,
                                       principalAmount = d.APPROVEDAMOUNT,
                                       //effectiveDate = a.EFFECTIVEDATE,
                                       //maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = d.TBL_PRODUCT1.PRODUCTTYPEID, // 1
                                       productName = d.TBL_PRODUCT1.PRODUCTNAME, // 1
                                       //writtenOff = a.LOANSTATUSID == 7

                                   }).ToList();
            return allFilteredLoan;
        }

        private List<LoanViewModel> SearchExternalLoan(string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery)) searchQuery = searchQuery.ToUpper();

            var allFilteredLoan = (from a in context.TBL_LOAN_EXTERNAL
                                   //join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                   where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved //&& d.STATUSID == 2
                                   && (a.LOANREFERENCENUMBER.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.CUSTOMERCODE.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.FIRSTNAME.ToUpper().Contains(searchQuery.Trim()) ||
                                       b.LASTNAME.ToUpper().Contains(searchQuery.Trim())
                                   )
                                   select new LoanViewModel
                                   {
                                       loanId = a.EXTERNALLOANID,
                                       customerId = b.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                       loanApplicationId = a.EXTERNALLOANID,
                                       //loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                       interestRate = 1,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       //effectiveDate = a.EFFECTIVEDATE,
                                       //maturityDate = a.MATURITYDATE,
                                       //loanTypeName = a.LOANAPPLICATIONTYPENAME,
                                       productTypeId = p.PRODUCTTYPEID, // 1
                                       productName = p.PRODUCTNAME, // 1
                                       //writtenOff = a.LOANSTATUSID == 7

                                   }).ToList();
            return allFilteredLoan;
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
                                       relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                        LOANSYSTEMTYPEID = x.LOANSYSTEMTYPEID,
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
        
        public List<LoanViewModel> SearchGetotherInformation(int loanId)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVIEW_OPERATION
                                   join b in context.TBL_LOAN_REVIEW_OPRATN_IREG_SC on a.LOANREVIEWOPERATIONID equals b.LOANREVIEWOPERATIONID
                                   where a.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility && a.LOANID == loanId
                                   select new LoanViewModel
                                   {
                                       paymentDate = b.PAYMENTDATE,
                                       scheduledPrepaymentAmount = b.PAYMENTAMOUNT,
                                   });
            return allFilteredLoan.OrderBy(x=> x.paymentDate).ToList();
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

        public LoanViewModel RelatedFacilityDetail(int loanId)
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
                                   principalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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

        public LoanViewModel RelatedOverdraftFacilityDetail(int loanId)
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
                               where a.REVOLVINGLOANID== loanId
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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

        public LoanViewModel RelatedContingentFacilityDetail(int loanId)
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

                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                   principalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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

                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                                   relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
        
        #region
        private IEnumerable<CamProcessedLoanViewModel> AvailedLoanApplicationsDetails(int companyId, int staffId, int branchId, string refNo)
        {
            try
            {
                var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                            join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                            join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                            join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                            join br in context.TBL_BRANCH on m.BRANCHID equals br.BRANCHID
                            where m.COMPANYID == companyId && d.DELETED == false
                            && ((m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.AvailmentCompleted)
                            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestInitiated)
                            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestCompleted)
                            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingInProgress)
                            || (m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingCompleted))
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
                            && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                            && (m.APPLICATIONREFERENCENUMBER.Contains(refNo) || cust.FIRSTNAME.ToLower().StartsWith(refNo.ToLower())
                            || cust.LASTNAME.ToLower().StartsWith(refNo.ToLower()) || cust.MIDDLENAME.ToLower().StartsWith(refNo.ToLower()))
                            
                            orderby m.AVAILMENTDATE descending, m.DATETIMECREATED descending
                            select new CamProcessedLoanViewModel
                            {
                                approvalStatusId = (short)m.APPROVALSTATUSID,
                                loanApplicationId = m.LOANAPPLICATIONID,
                                loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                                applicationStatusId = m.APPLICATIONSTATUSID,
                                customerId = m.CUSTOMERID ?? 0,
                                customerCode = cust.CUSTOMERCODE,
                                customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                                customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                                customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                                isRelatedParty = m.ISRELATEDPARTY,
                                customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                                customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,

                                isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                                isInvestmentGrade = m.ISINVESTMENTGRADE,

                                companyId = m.COMPANYID,
                                branchId = m.BRANCHID,
                                branchName = m.TBL_BRANCH.BRANCHNAME,
                                subSectorId = d.SUBSECTORID,
                                subSectorName = d.TBL_SUB_SECTOR.NAME,
                                sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                applicationTenor = m.APPLICATIONTENOR,
                                effectiveDate = (DateTime)d.EFFECTIVEDATE,
                                expiryDate = (DateTime)d.EXPIRYDATE,
                                relationshipOfficerId = m.RELATIONSHIPOFFICERID,
                                relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
                                relationshipManagerId = m.RELATIONSHIPMANAGERID,
                                relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

                                currencyId = d.CURRENCYID,
                                currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                exchangeRate = d.EXCHANGERATE,
                                loanTypeId = m.LOANAPPLICATIONTYPEID,
                                loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault().CAMREF,
                                productId = d.APPROVEDPRODUCTID,
                                productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                                productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                productName = d.TBL_PRODUCT.PRODUCTNAME,
                                productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                                misCode = m.MISCODE,
                                teamMisCode = m.TEAMMISCODE,

                                interestRate = d.APPROVEDINTERESTRATE,
                                submittedForAppraisal = m.SUBMITTEDFORAPPRAISAL,
                                approvedAmount = d.APPROVEDAMOUNT,
                                approvedDate = m.APPROVEDDATE,
                                groupApprovedAmount = m.APPROVEDAMOUNT,
                                approvedTenor = d.APPROVEDTENOR,
                                createdBy = m.OWNEDBY,
                                newApplicationDate = m.APPLICATIONDATE,
                                dateTimeCreated = d.DATETIMECREATED,
                                availmentDate = m.AVAILMENTDATE,
                                isTemporaryOverdraft = p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault() != null ? p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault().ISTEMPORARYOVERDRAFT : false,
                                loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0
                            }).ToList();
                foreach (var item in data)
                {
                    var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                  //  var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                  //  var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                    switch (item.productTypeId)
                    {
                        case (short)LoanProductTypeEnum.TermLoan:
                            decimal customerAvailableAmount = 0;
                            foreach (var loan in loans)
                            {
                                if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount = customerAvailableAmount + loan.PRINCIPALAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount;
                            break;
                        case (short)LoanProductTypeEnum.CommercialLoan:
                            decimal customerAvailableAmount2 = 0;
                            foreach (var loan in loans)
                            {
                                if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount2 = customerAvailableAmount2 + loan.PRINCIPALAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount2;
                            break;
                        case (short)LoanProductTypeEnum.SelfLiquidating:
                            decimal customerAvailableAmount3 = 0;
                            foreach (var loan in loans)
                            {
                                if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount3 = customerAvailableAmount3 + loan.PRINCIPALAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount3;
                            break;
                        
                    }
                }
                return data;
            }
            catch (Exception ex) { throw; }
        }

        public IEnumerable<CamProcessedLoanViewModel> GetLoanFacilityUtilization(int companyId, int staffId, int branchId, string searchValue)
        {
            try
            {

                var data = AvailedLoanApplicationsDetails(companyId, staffId, branchId, searchValue);//.Where(x => x.productClassProcessId != (short)ProductClassProcessEnum.ProductBased);

                data = (from a in data where ((a.customerAvailableAmount > 0) || (a.customerAvailableAmount == null)) select a).ToList();

                foreach (var item in data)
                {


                    var requests = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);

                    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Count() > 0)
                        item.approveRequestAmount = (decimal)requests.Where(k => k.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved).Sum(s => s.AMOUNT_REQUESTED);

                    if (requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                        item.pendingRequestAmount = (decimal)requests.Where(j => j.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                    if (requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Count() > 0)
                        item.allRequestAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending).Sum(s => s.AMOUNT_REQUESTED) - item.requestedAmount;

                    item.disapprovedCount = (int)requests.Where(a => a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Count();

                    if (item.disapprovedCount > 0)
                        item.disApprovedAmount = (decimal)requests.Where(n => n.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved).Sum(s => s.AMOUNT_REQUESTED);

                    item.customerAvailableAmount = item.approvedAmount - (item.allRequestAmount - item.requestedAmount);

                    var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                    if (disbursedLoan.Any())
                    {
                        item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                    }

                }

                if (searchValue != null)
                {
                    data = data.Where(o => o.applicationReferenceNumber.Contains(searchValue) || o.customerName.ToLower().Contains(searchValue.ToLower()));
                }

                return data;
            }
            catch (Exception ex) { throw ex; }
        }

        public List<LoanViewModel> GetLoanFacilityDetail(int loanApplicationDetilId)
        {
            var loan = (from a in context.TBL_LOAN
                       where a.LOANAPPLICATIONDETAILID == loanApplicationDetilId
                       orderby a.DATETIMECREATED
                       select new LoanViewModel
                       {
                           loanId = a.TERMLOANID,
                           loanApplicationId = a.LOANAPPLICATIONDETAILID,
                           customerId = a.CUSTOMERID,
                           productId = a.PRODUCTID,
                           companyId = a.COMPANYID,
                           loanSystemTypeId = a.LOANSYSTEMTYPEID,
                           casaAccountId = a.CASAACCOUNTID,
                           loanReferenceNumber = a.LOANREFERENCENUMBER,
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
                           //crmsCode = a.CRMSCODE,
                           //crmsDate = a.CRMSDATE,
                           createdBy = a.CREATEDBY,
                           dateTimeCreated = a.DATETIMECREATED,
                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                           exchangeRate = a.EXCHANGERATE,
                           currencyId = a.CURRENCYID,
                           productPriceIndexRate = a.PRODUCTPRICEINDEXRATE,
                           relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
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
                       }).ToList();

            if (loan.Count==0)
            {
                loan = (from a in context.TBL_LOAN_CONTINGENT
                        where a.LOANAPPLICATIONDETAILID == loanApplicationDetilId
                        orderby a.DATETIMECREATED
                        select new LoanViewModel
                        {
                            loanId = a.CONTINGENTLOANID,
                            loanApplicationId = a.LOANAPPLICATIONDETAILID,
                            customerId = a.CUSTOMERID,
                            productId = a.PRODUCTID,
                            companyId = a.COMPANYID,
                            loanSystemTypeId = a.LOANSYSTEMTYPEID,
                            casaAccountId = a.CASAACCOUNTID,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            effectiveDate = a.EFFECTIVEDATE,
                            maturityDate = a.MATURITYDATE,
                            bookingDate = a.BOOKINGDATE,
                            approvalStatusId = a.APPROVALSTATUSID,
                            approvedBy = a.APPROVEDBY,
                            approverComment = a.APPROVERCOMMENT,
                            dateApproved = a.DATEAPPROVED,
                            loanStatusId = a.LOANSTATUSID,
                            isDisbursed = a.ISDISBURSED,
                            isDisbursedState = a.ISDISBURSED ? "Yes" : "No",
                            disburserComment = a.DISBURSERCOMMENT,
                            disburseDate = a.DISBURSEDATE,
                            //crmsCode = a.CRMSCODE,
                            //crmsDate = a.CRMSDATE,
                            operationId = a.OPERATIONID,
                            operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            principalAmount = a.CONTINGENTAMOUNT,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATETIMECREATED,
                            exchangeRate = a.EXCHANGERATE,
                            currencyId = a.CURRENCYID,
                            relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                            ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                            approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                            approvedComment = a.APPROVERCOMMENT,
                            loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                            productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                        }).ToList();
            }

            if (loan.Count == 0)
            {
                loan = (from a in context.TBL_LOAN_REVOLVING
                        where a.LOANAPPLICATIONDETAILID == loanApplicationDetilId
                        orderby a.DATETIMECREATED
                        select new LoanViewModel
                        {
                            loanId = a.REVOLVINGLOANID,
                            loanApplicationId = a.LOANAPPLICATIONDETAILID,
                            customerId = a.CUSTOMERID,
                            productId = a.PRODUCTID,
                            companyId = a.COMPANYID,
                            loanSystemTypeId = a.LOANSYSTEMTYPEID,
                            casaAccountId = a.CASAACCOUNTID,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            effectiveDate = a.EFFECTIVEDATE,
                            maturityDate = a.MATURITYDATE,
                            bookingDate = a.BOOKINGDATE,
                            approvalStatusId = a.APPROVALSTATUSID,
                            approvedBy = a.APPROVEDBY,
                            approverComment = a.APPROVERCOMMENT,
                            dateApproved = a.DATEAPPROVED,
                            loanStatusId = a.LOANSTATUSID,
                            isDisbursed = a.ISDISBURSED,
                            //crmsCode = a.CRMSCODE,
                            //crmsDate = a.CRMSDATE,
                            isDisbursedState = a.ISDISBURSED ? "Yes" : "No",
                            disburserComment = a.DISBURSERCOMMENT,
                            disburseDate = a.DISBURSEDATE,
                            operationId = a.OPERATIONID,
                            operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            dischargeLetter = a.DISCHARGELETTER,
                            suspendInterest = a.SUSPENDINTEREST,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATETIMECREATED,
                            exchangeRate = a.EXCHANGERATE,
                            currencyId = a.CURRENCYID,
                            relatedloanReferenceNumber = a.RELATED_LOAN_REFERENCE_NUMBER,
                            ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                            approvedByName = context.TBL_STAFF.Where(x => x.STAFFID == a.APPROVEDBY).Select(x => x.FIRSTNAME + "" + x.LASTNAME).FirstOrDefault(),
                            approvedComment = a.APPROVERCOMMENT,
                            loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                            pastDueInterest = a.PASTDUEINTEREST,
                            pastDuePrincipal = a.PASTDUEPRINCIPAL,
                            interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                            interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                            externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                            userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.Where(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).Select(x => x.STATUSNAME).FirstOrDefault(),
                            productPriceIndexName = context.TBL_PRODUCT_PRICE_INDEX.Where(q => q.PRODUCTPRICEINDEXID == context.TBL_PRODUCT.Where(x => x.PRODUCTID == a.PRODUCTID).FirstOrDefault().TBL_PRODUCT_PRICE_INDEX.PRODUCTPRICEINDEXID).Select(q => q.PRICEINDEXNAME).FirstOrDefault(),
                        }).ToList();
            }
            return loan;
        }

        #endregion
    }
}
