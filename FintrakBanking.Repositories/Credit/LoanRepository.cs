using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.Finance;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using FintrakBanking.ViewModels.Reports;
using System.Threading.Tasks;
using FintrakBanking.Repositories.CASA;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.Common.CustomException;
using System.Net.Sockets;
using FintrakBanking.Entities.StagingModels;
using FinTrakBanking.ThirdPartyIntegration.StagingDatabase.Finacle;
using FintrakBanking.Interfaces.Setups.Finance;
using FinTrakBanking.ThirdPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI;

namespace FintrakBanking.Repositories.Credit
{

    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private ICasaLienRepository casaLien;
        private IApprovalLevelStaffRepository level;
        private ICustomerRepository customers;
        private IWorkflow workflow;
        private IAuditTrailRepository audit;
        private IOverRideRepository overrider;
        private IChartOfAccountRepository chartOfAccount;
        private IntegrationWithFinacle integration;
        private FinTrakBankingStagingContext stgCon;
        //private ICasaRepository casaRep;

        private IIntegrationWithFinacle finacle;
        bool USE_THIRD_PARTY_INTEGRATION = false;

        public LoanRepository(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        ILoanCovenantRepository _loanCovenant, IAuditTrailRepository _audit,
                                        IFinanceTransactionRepository _financeTransaction, IApprovalLevelStaffRepository _level,
                                        ICustomerRepository _customers, IWorkflow _workflow, ICasaLienRepository _casaLien,
                                        IChartOfAccountRepository _chartOfAccount,
                                        IOverRideRepository _overrider, IntegrationWithFinacle _integration,
            IIntegrationWithFinacle finacle, FinTrakBankingStagingContext _stgCon)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.loanCovenant = _loanCovenant;
            this.audit = _audit;
            this.financeTransaction = _financeTransaction;
            this.level = _level;
            this.customers = _customers;
            this.workflow = _workflow;
            this.casaLien = _casaLien;
            this.overrider = _overrider;
            this.chartOfAccount = _chartOfAccount;
            this.integration = _integration;
            //this.casaRep = _casaRep;
            this.finacle = finacle;
            this.stgCon = _stgCon;

            var globalSetting = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = globalSetting.USE_THIRD_PARTY_INTEGRATION;


        }

        public int DateDiff(DateTime startDate, DateTime endDate)
        {
            int noofdays = (((int)(endDate - startDate).TotalDays));
            return noofdays;
        }

        public IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanTypes()
        {
            return (from data in context.TBL_LOAN_REVOLVING_TYPE
                    select new RevolvingLoanViewModel()
                    {
                        revolvingTypeId = data.REVOLVINGTYPEID,
                        revolvingTypeName = data.REVOLVINGTYPENAME,
                        isTemporaryOverdraft = data.ISTEMPORARYOVERDRAFT
                    });
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanApplicationDetailCovenantById(int applicationDetailId)
        {
            return (from data in context.TBL_LOAN_APPLICATION_COVENANT
                    where data.LOANAPPLICATIONDETAILID == applicationDetailId && data.DELETED == false
                    select new LoanCovenantDetailViewModel()
                    {
                        loanCovenantDetailId = data.LOANCOVENANTDETAILID,
                        covenantDetail = data.COVENANTDETAIL,
                        covenantTypeId = data.COVENANTTYPEID,
                        covenantTypeName = data.TBL_LOAN_COVENANT_TYPE.COVENANTTYPENAME,
                        frequencyTypeId = data.FREQUENCYTYPEID,
                        frequencyTypeName = data.TBL_FREQUENCY_TYPE.DESCRIPTION,
                        covenantAmount = data.COVENANTAMOUNT,
                        covenantDate = data.COVENANTDATE,
                        loanApplicationDetailId = data.LOANAPPLICATIONDETAILID,
                        isPercentage = data.ISPERCENTAGE,
                        nextCovenantDate = data.NEXTCOVENANTDATE,
                        casaAccountId = data.CASAACCOUNTID,
                        companyId = data.COMPANYID
                    });
        }

        public IEnumerable<RevolvingLoanViewModel> GetTemporaryOverdrafts()
        {
            return (from data in context.TBL_LOAN_REVOLVING_TYPE
                    where data.ISTEMPORARYOVERDRAFT == true
                    select new RevolvingLoanViewModel()
                    {
                         revolvingTypeId = data.REVOLVINGTYPEID,
                         revolvingTypeName = data.REVOLVINGTYPENAME,
                         isTemporaryOverdraft = data.ISTEMPORARYOVERDRAFT
                    });
        }

        /// <summary>
        /// Gets all loan types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LookupViewModel> GetLoanApplicationTypes()
        {
            return (from data in context.TBL_LOAN_APPLICATION_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.LOANAPPLICATIONTYPEID,
                        lookupName = data.LOANAPPLICATIONTYPENAME
                    });
        }


        public CasaBalanceViewModel GetCASABalanceById(int casaAccountId, int companyId)
        {
            var accountDetails = financeTransaction.GetCASABalance(casaAccountId);
            return accountDetails;
        }

        public CurrencyExchangeRateViewModel GetExchangeRate(string fromCurrencyCode, string toCurrencyCode, string rateCode)
        {
            var rate = integration.GetExchangeRate(fromCurrencyCode, toCurrencyCode, rateCode);
            return rate;
        }

        public IEnumerable<TransactionDynamicsViewModel> GetLoanTransactionDynamics(int loanApplicationDetailId)
        {
            return (from data in context.TBL_LOAN_TRANSACTION_DYNAMICS where data.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                    select new TransactionDynamicsViewModel()
                    {
                       // dynamicsId = data.DYNAMICSID,
                        dynamics = data.DYNAMICS,
                        //productId = data.TBL_TRANSACTION_DYNAMICS.PRODUCTID,
                       // loanDynamicsId = data.LOANDYNAMICSID
                    });
        }

        public string GenerateLoanReferenceNumber(int branchId, int productId, int loanSystemTypeId)
        {
            var branch = this.context.TBL_BRANCH.Find(branchId);
            var productCode = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == productId).PRODUCTCODE;
            if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                var data = ((this.context.TBL_LOAN.Count(x => x.BRANCHID == branch.BRANCHID && x.PRODUCTID == productId)) + 1);
                var reference = $"{branch.BRANCHCODE}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";

                    for (var count = data; context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == reference).Any(); count++)
                    {
                        reference = $"{branch.BRANCHCODE}-{productCode}-{CommonHelpers.GenerateZeroString(5) + count.ToString().Right(5)}";
                    };
                return reference;
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                var data = ((this.context.TBL_LOAN_REVOLVING.Count(x=> x.BRANCHID == branch.BRANCHID && x.PRODUCTID == productId)) + 1);
                return $"{branch.BRANCHCODE}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
            }
            else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                var data = ((this.context.TBL_LOAN_CONTINGENT.Count(x => x.BRANCHID == branch.BRANCHID && x.PRODUCTID == productId)) + 1);
                return $"{branch.BRANCHCODE}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
            }
            else throw new ConditionNotMetException("Loan Product Type not defined for Loan Booking");

        }

        private bool confirmCustomerAccountFunded(List<LoanChargeFeeViewModel> model, int casaAccountId, int companyId, int customerId,string loanReferenceNumber )
        {
            decimal AllfeeAmount = 0;
            foreach (var item in model) { AllfeeAmount = AllfeeAmount + item.feeAmount; }

            var casaBalance = GetCASABalanceById(casaAccountId, companyId).availableBalance;
            var customer = context.TBL_CUSTOMER.Find(customerId);

            if (AllfeeAmount > casaBalance)
            {
                int custFeeOverridable = overrider.EffectOverride(customer.CUSTOMERCODE, (short)OverrideEnum.TakeFeeAtDisbursement, loanReferenceNumber);
                if (custFeeOverridable > 0)
                {
                    return true;
                }
                else throw new ConditionNotMetException("The customer account is not funded and fee override is not enabled for this customer");
            }
            return false;
        }

        public string AddLoanBooking(LoanViewModel entity)
        {
            //...................CHECK IF THE LOAN RECORD IS TERM(SCHEDULED) LOAN..................//
            if (entity.productTypeId == (int)LoanProductTypeEnum.TermLoan || entity.productTypeId == (int)LoanProductTypeEnum.SelfLiquidating || entity.productTypeId == (int)LoanProductTypeEnum.SyndicatedTermLoan)
            {
                return this.AddTermLoan(entity);
            }
            // ...............CHECK IF THE  LOAN RECORD IS A COMMERCIAL LOAN....................//
            else if (entity.productTypeId == (int)LoanProductTypeEnum.CommercialLoan)
            {
                return AddCommercialLoan(entity);
            }
            else if (entity.productTypeId == (int)LoanProductTypeEnum.ForeignXRevolving)
            {
                return AddFXLoan(entity);
            }
            // ...............CHECK IF THE LOAN RECORD IS A NON SCHEDULED LOAN....................//
            else if (entity.productTypeId == (int)LoanProductTypeEnum.RevolvingLoan)
            {
                return addRevolvingLoan(entity);
            }
            else if (entity.productTypeId == (int)LoanProductTypeEnum.ContingentLiability)
            {
                return addContingentLiability(entity);
            }
            else
            {
                throw new ConditionNotMetException("The Product type is Invalid");
            }
        }

        private string addRevolvingLoan(LoanViewModel model)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(model.loanApplicationId);
            var revolvingLoanInput = model.revolvingLoanInput;

            var overdraftLimit = from a in context.TBL_LOAN_REVOLVING
                                 where a.LOANAPPLICATIONDETAILID == revolvingLoanInput.loanApplicationDetailId
                                 let sumLimit = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == revolvingLoanInput.loanApplicationDetailId).Sum(x => x.OVERDRAFTLIMIT)
                                 select sumLimit;

            var currentExchangeRate = financeTransaction.GetExchangeRate(DateTime.Now, (short)revolvingLoanInput.currencyId, model.companyId).sellingRate;

            var totalPreviouslyBookedAmount = overdraftLimit.FirstOrDefault();

            var totaloverdraftLimit = totalPreviouslyBookedAmount + revolvingLoanInput.overdraftLimit;

            if (totaloverdraftLimit > model.customerAvailableAmount)
                throw new ConditionNotMetException("The loan amount cannot be greater than the availiable amount");

            if (model.effectiveDate > model.maturityDate)
                throw new ConditionNotMetException("The effective cannot be greater than maturity date");

            if (model.effectiveDate > generalSetup.GetApplicationDate())
                throw new ConditionNotMetException("You effective date cannot be post-dated.");

            //var productBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == model.productId).FirstOrDefault();
            //if (productBehaviour != null && productBehaviour.ISTEMPORARYOVERDRAFT == true && context.TBL_LOAN_REVOLVING.Where(x => x.CUSTOMERID == model.customerId && x.CASAACCOUNTID == model.casaAccountId).Any())
            //    throw new ConditionNotMetException("The customer already has an existing overdraft on the selected account");

            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(model.loanBookingRequestId);
            if (request.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                throw new ConditionNotMetException("This Loan Request has already been booked by another staff");

            var product = context.TBL_PRODUCT.Find(model.productId);

            var loanReferenceNumber = GenerateLoanReferenceNumber(application.BRANCHID, model.productId, (short)LoanSystemTypeEnum.OverdraftFacility);
            //branchId, productId, serial

            if (revolvingLoanInput.revolvingTypeId == 0)
                revolvingLoanInput.revolvingTypeId = (short)LoanRevolvingTypeEnum.NormalOverdraft;

            var data = new TBL_LOAN_REVOLVING
            {
                LOAN_BOOKING_REQUESTID = model.loanBookingRequestId,
                CUSTOMERID = model.customerId,
                PRODUCTID = model.productId,
                CASAACCOUNTID = model.casaAccountId,
                BRANCHID = application.BRANCHID,
                CURRENCYID = (short)model.exchangeRate,
                EXCHANGERATE = currentExchangeRate,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                RELATED_LOAN_REFERENCE_NUMBER = loanReferenceNumber,
                SUBSECTORID = model.subSectorId,
                RELATIONSHIPOFFICERID = model.relationshipOfficerId,
                RELATIONSHIPMANAGERID = model.relationshipManagerId,
                MISCODE = model.misCode,
                TEAMMISCODE = model.teamMiscode,
                INTERESTRATE = model.interestRate,
                EFFECTIVEDATE = revolvingLoanInput.effectiveDate,
                MATURITYDATE = revolvingLoanInput.maturityDate,
                BOOKINGDATE = DateTime.Now,
                OVERDRAFTLIMIT = revolvingLoanInput.overdraftLimit,
                DAYCOUNTCONVENTIONID = revolvingLoanInput.accrualBasis,
                REVOLVINGTYPEID = revolvingLoanInput.revolvingTypeId,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                LOANSTATUSID = (int)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                OPERATIONID = model.operationId = (int)OperationsEnum.RevolvingLoanBooking,
                LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.OverdraftFacility,
                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,

                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                // LASTRESTRUCTUREDATE = revolvingLoanInput.effectiveDate,
                USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                CRMSREPAYMENTAGREEMENTID = model.crmsRepaymentAgreementTypeId
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    // ............. Checking customer balance, and fee override ......
                    model.feeOverride = confirmCustomerAccountFunded(model.loanChargeFee, model.casaAccountId, model.companyId, model.customerId, loanReferenceNumber);

                    //...................Adding Revolving Loan Record.........................
                    var loan = context.TBL_LOAN_REVOLVING.Add(data);

                    if (model.monitoringTriggers.Count > 0)
                        AddLoanMonitoringTrigger(model.monitoringTriggers, loan.REVOLVINGLOANID, (short)LoanSystemTypeEnum.OverdraftFacility);

                    //...................Update the Loan Request table.......................
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;

                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.LoanBookingInProgress;

                    //...................Adding Audit...............................
                    context.TBL_AUDIT.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = model.createdBy,
                        companyId = model.companyId,
                        applicationId = model.loanBookingRequestId,
                        comment = "Please approve this Loan",
                        amount = revolvingLoanInput.approvedAmount,
                        operationId = (int)OperationsEnum.RevolvingLoanBooking
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (LogApproval(approvalModel, (int)OperationsEnum.RevolvingLoanBooking, false, (int)ApprovalStatusEnum.Processing))
                    {
                        //............save Loan Covenant..........
                        AddLoanCovenant(model, loan.REVOLVINGLOANID, (short)LoanSystemTypeEnum.OverdraftFacility);
                        //............save Loan Fees..........
                        AddLoanFees(model.loanChargeFee, model.createdBy, loan.REVOLVINGLOANID, (short)LoanSystemTypeEnum.OverdraftFacility, model.companyId, model.feeOverride);

                        model.loanReferenceNumber = loanReferenceNumber;

                        //...................Saving Loan Collaterals Mapping.......................
                        AddLoanCollateralMapping(model.loanApplicationId, loan.REVOLVINGLOANID, (short)LoanSystemTypeEnum.OverdraftFacility);

                        //...................Saving Loan Collaterals Mapping.......................
                        //if (model.monitoringTriggers.Count > 0)
                        //    AddLoanMonitoringTrigger(model.monitoringTriggers, loan.REVOLVINGLOANID, (short)LoanSystemTypeEnum.OverdraftFacility);

                        if (!model.feeOverride) PostLoanFees(model);
                        context.SaveChanges();

                        //.....Commit transaction ............
                        trans.Commit();
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (BadLogicException be)
                {
                    trans.Rollback();
                    throw new BadLogicException(be.Message);
                }
                catch (ConditionNotMetException ce)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (TwoFactorAuthenticationException et)
                {
                    trans.Rollback();
                    throw new TwoFactorAuthenticationException(et.Message);
                }
                catch (APIErrorException ae)
                {
                    trans.Rollback();
                    throw new APIErrorException(ae.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private string addContingentLiability(LoanViewModel entity)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(entity.loanApplicationId);
            var contingentLoanInput = entity.contingentLoanInput;
            var contingentAmount = from a in context.TBL_LOAN_CONTINGENT
                                   where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                   let sumAmount = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.CONTINGENTAMOUNT)
                                   select sumAmount;

            var currentExchangeRate = financeTransaction.GetExchangeRate(DateTime.Now, (short)contingentLoanInput.currencyId, entity.companyId).sellingRate;

            var totalPreviouslyBookedAmount = contingentAmount.FirstOrDefault();

            var totalContingentAmount = totalPreviouslyBookedAmount + contingentLoanInput.contingentAmount;

            if (totalContingentAmount > entity.customerAvailableAmount)
                throw new ConditionNotMetException("The loan amount cannot be greater than the availiable amount");

            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.loanBookingRequestId);

            if (request.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                throw new ConditionNotMetException("This Loan Request has already been booked by another staff");

            if (entity.effectiveDate > entity.maturityDate)
                throw new ConditionNotMetException("The effective cannot be greater than maturity date");

            if (entity.effectiveDate > generalSetup.GetApplicationDate())
                throw new ConditionNotMetException("You effective date cannot be post-dated.");

            var bgData = context.TBL_LOAN_APPLICATION_DETL_BG.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId);

            var isTenored = false;
            var isBankFormat = false;
            if (bgData.Any())
            {
                var bgRecord = bgData.FirstOrDefault();
                if (bgRecord.ISTENORED) isTenored = true;
                if (bgRecord.ISBANKFORMAT) isBankFormat = true;
            }

            var loanReferenceNumber = GenerateLoanReferenceNumber(application.BRANCHID, entity.productId, (short)LoanSystemTypeEnum.ContingentLiability);

            var data = new TBL_LOAN_CONTINGENT
            {
                LOAN_BOOKING_REQUESTID = entity.loanBookingRequestId,
                CUSTOMERID = entity.customerId,
                PRODUCTID = entity.productId,
                CASAACCOUNTID = entity.casaAccountId,
                BRANCHID = application.BRANCHID, //entity.branchId,
                CURRENCYID = contingentLoanInput.currencyId,
                EXCHANGERATE = currentExchangeRate,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                RELATED_LOAN_REFERENCE_NUMBER = loanReferenceNumber,
                SUBSECTORID = entity.subSectorId,
                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                EFFECTIVEDATE = contingentLoanInput.effectiveDate,
                MATURITYDATE = contingentLoanInput.maturityDate,
                ISBANKFORMAT = isBankFormat,
                ISTENORED = isTenored,
                BOOKINGDATE = DateTime.Now,
                CRMSREPAYMENTAGREEMENTID = entity.crmsRepaymentAgreementTypeId,

                LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.ContingentLiability,
                CONTINGENTAMOUNT = contingentLoanInput.contingentAmount,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                LOANSTATUSID = (int)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                OPERATIONID = entity.operationId = (int)OperationsEnum.ContigentLoanBooking,
                DISCHARGELETTER = false,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    // ............. Checking customer balance, and fee override ......
                    entity.feeOverride = confirmCustomerAccountFunded(entity.loanChargeFee, entity.casaAccountId, entity.companyId, entity.customerId, loanReferenceNumber);

                    //...................Adding Contingent Loan Record.........................
                    var loan = context.TBL_LOAN_CONTINGENT.Add(data);

                    //...................Update the Loan Request and loan application table.......................
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;

                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.LoanBookingInProgress;
                    //...................Adding Audit...............................
                    context.TBL_AUDIT.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = entity.loanBookingRequestId,
                        comment = "Please approve this Loan",
                        amount = contingentLoanInput.approvedAmount,
                        operationId = (int)OperationsEnum.ContigentLoanBooking
                    };

                    if (LogApproval(approvalModel, (int)OperationsEnum.ContigentLoanBooking, false, (int)ApprovalStatusEnum.Processing))
                    {
                        //............save Loan Covenant..........
                        AddLoanCovenant(entity, loan.CONTINGENTLOANID, (short)LoanSystemTypeEnum.ContingentLiability);
                        //............save Loan Fees..........
                        AddLoanFees(entity.loanChargeFee, entity.createdBy, loan.CONTINGENTLOANID, (short)LoanSystemTypeEnum.ContingentLiability, entity.companyId, entity.feeOverride);
                        entity.loanReferenceNumber = loanReferenceNumber;

                        //...................Saving Loan Collaterals Mapping.......................
                        AddLoanCollateralMapping(entity.loanApplicationId, loan.CONTINGENTLOANID, (short)LoanSystemTypeEnum.ContingentLiability);

                        //...................Mapping Loan Monitoring Trigger.......................
                        //if (entity.monitoringTriggers.Count > 0)
                        //    AddLoanMonitoringTrigger(entity.monitoringTriggers, loan.CONTINGENTLOANID, (short)LoanSystemTypeEnum.ContingentLiability);

                        if (!entity.feeOverride) PostLoanFees(entity);

                        context.SaveChanges();

                        //.....Commit transaction ............
                        trans.Commit();
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (BadLogicException be)
                {
                    trans.Rollback();
                    throw new BadLogicException(be.Message);
                }
                catch (ConditionNotMetException ce)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (APIErrorException ae)
                {
                    trans.Rollback();
                    throw new APIErrorException(ae.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }
        private string AddTermLoan(LoanViewModel entity)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(entity.loanApplicationId);
            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);
            if (entity.loanScheduleInput.maturityDate <= entity.loanScheduleInput.effectiveDate)
                throw new ConditionNotMetException("Loan terminal date should be more than effective date");

            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.loanBookingRequestId);

            if (request.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                throw new ConditionNotMetException("This Loan Request has already been booked by another staff");

            if (entity.effectiveDate > entity.maturityDate)
                throw new ConditionNotMetException("The effective cannot be greater than maturity date");

            if (entity.effectiveDate > generalSetup.GetApplicationDate())
                throw new ConditionNotMetException("You effective date cannot be post-dated.");

            if (application.TBL_PRODUCT_CLASS != null && application.TBL_PRODUCT_CLASS.PRODUCTCLASSID == (short)ProductClassEnum.InvoiceDiscountingFacility)
            {
                if (entity.casaAccountId2 == null || entity.casaAccountId2 == 0)
                    throw new ConditionNotMetException("Specify the collection account.");
            }


            var principalAmount = from a in context.TBL_LOAN
                                  where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                  let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.PRINCIPALAMOUNT)
                                  select sumPrincipalAmount;

            var productCurrencyIndex = context.TBL_PRODUCT_PRICE_INDEX_CURNCY.Where(x => x.CURRENCYID == applicationDetail.CURRENCYID).FirstOrDefault();
            var priceIndex = (from a in context.TBL_PRODUCT_PRICE_INDEX where a.PRODUCTPRICEINDEXID == productCurrencyIndex.PRODUCTPRICEINDEXID select a).FirstOrDefault();

            var interestRate = Convert.ToDouble(entity.interestRate);
            if (priceIndex != null)
            {
                interestRate = priceIndex.PRICEINDEXRATE + interestRate;
            }

            var currentExchangeRate = financeTransaction.GetExchangeRate(DateTime.Now, (short)entity.currencyId, entity.companyId).sellingRate;

            var loanReferenceNumber = GenerateLoanReferenceNumber(application.BRANCHID, entity.productId, (short)LoanSystemTypeEnum.TermDisbursedFacility);

            var approvedAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).FirstOrDefault().APPROVEDAMOUNT;

            var totalPreviouslyBookedAmount = principalAmount.FirstOrDefault();

            var totalPrincipalAmount = (decimal)(totalPreviouslyBookedAmount + (decimal)entity.loanScheduleInput.principalAmount);

            if (totalPrincipalAmount > (decimal)approvedAmount)
                throw new ConditionNotMetException("The loan amount cannot be greater than the availiable amount");

            if (entity.loanScheduleInput.scheduleMethodId == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                entity.loanScheduleInput.principalFrequency = null;
                entity.loanScheduleInput.interestFrequency = null;
            }

            entity.casaAccountId2 = entity.casaAccountId2 != 0 ? entity.casaAccountId2 : null;
            var data = new TBL_LOAN
            {
                LOAN_BOOKING_REQUESTID = entity.loanBookingRequestId,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                RELATED_LOAN_REFERENCE_NUMBER = loanReferenceNumber,
                LOANSTATUSID = (short)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                PRINCIPALNUMBEROFINSTALLMENT = 0,
                INTERESTNUMBEROFINSTALLMENT = 0,
                SCHEDULEDPREPAYMENTAMOUNT = entity.scheduledPrepaymentAmount,
                SCH_PREPAYMENT_FREQUENCY_TYPID = null,
                PRODUCTPRICEINDEXRATE = priceIndex.PRICEINDEXRATE,
                PRODUCTPRICEINDEXID = priceIndex.PRODUCTPRICEINDEXID,

                SUBSECTORID = entity.subSectorId,
                CURRENCYID = (short)entity.currencyId,
                EXCHANGERATE = currentExchangeRate,

                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,

                CUSTOMERID = entity.customerId,
                PRODUCTID = (short)entity.productId,
                COMPANYID = entity.companyId,
                CASAACCOUNTID = entity.casaAccountId,
                CASAACCOUNTID2 = entity.casaAccountId2,
                BRANCHID = application.BRANCHID, //entity.branchId,
                SHOULD_DISBURSE = entity.loanScheduleInput.shouldDisburse,

                PRINCIPALFREQUENCYTYPEID = entity.loanScheduleInput.principalFrequency,
                INTERESTFREQUENCYTYPEID = entity.loanScheduleInput.interestFrequency,

                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                INTERESTRATE = interestRate,

                PRINCIPALINSTALLMENTLEFT = 0,
                INTERESTINSTALLMENTLEFT = 0,

                SCHEDULETYPEID = entity.loanScheduleInput.scheduleMethodId,

                PRINCIPALAMOUNT = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),

                OPERATIONID = entity.operationId = (int)OperationsEnum.TermLoanBooking,
                LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                EQUITYCONTRIBUTION = 0,
                OUTSTANDINGPRINCIPAL = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),
                PRINCIPALADDITIONCOUNT = 0,
                PRINCIPALREDUCTIONCOUNT = 0,
                FIXEDPRINCIPAL = false,
                PROFILELOAN = false,

                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,

                BOOKINGDATE = DateTime.Now,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                EFFECTIVEDATE = entity.loanScheduleInput.effectiveDate,
                MATURITYDATE = entity.loanScheduleInput.maturityDate,
                LASTRESTRUCTUREDATE = entity.loanScheduleInput.effectiveDate,
                FIRSTPRINCIPALPAYMENTDATE = entity.loanScheduleInput.principalFirstpaymentDate,
                FIRSTINTERESTPAYMENTDATE = entity.loanScheduleInput.interestFirstpaymentDate,
                ALLOWFORCEDEBITREPAYMENT = false,
                SCHEDULEDAYCOUNTCONVENTIONID = entity.loanScheduleInput.accrualBasis,
                USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                CRMSREPAYMENTAGREEMENTID = entity.crmsRepaymentAgreementTypeId,

            };

            ////Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            ////end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {

                    // ............. Checking customer balance, and fee override ......
                    entity.feeOverride = confirmCustomerAccountFunded(entity.loanChargeFee, entity.casaAccountId, entity.companyId, entity.customerId, loanReferenceNumber);

                    //...................Adding Term Loan Record.........................
                    var loan = context.TBL_LOAN.Add(data);

                    //...................Update the Loan Request and loan application table.......................
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;

                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.LoanBookingInProgress;

                    //...................Adding Audit...............................
                    var dataCount = context.SaveChanges();
                    if (dataCount > 0)
                    {
                        var approvalModel = new ForwardViewModel
                        {
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            applicationId = entity.loanBookingRequestId,
                            comment = "Please approve this Loan",
                            amount = entity.principalAmount,
                            operationId = (short)OperationsEnum.TermLoanBooking,
                        };

                        //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                        if (LogApproval(approvalModel, (int)OperationsEnum.TermLoanBooking, false, (int)ApprovalStatusEnum.Processing))
                        {
                            if (entity.loanScheduleInput.scheduleMethodId == (short)LoanScheduleTypeEnum.IrregularSchedule)
                            {
                                foreach (var irregular in entity.loanScheduleInput.irregularPaymentSchedule)
                                {
                                    var irregularRecordData = new TBL_LOAN_SCHEDULE_IREGUL_INPUT
                                    {
                                        LOANID = loan.TERMLOANID,
                                        PAYMENTAMOUNT = (decimal)irregular.paymentAmount,
                                        PAYMENTDATE = irregular.paymentDate,
                                        CREATEDBY = entity.createdBy,
                                        DATETIMECREATED = DateTime.Now,
                                    };
                                    context.TBL_LOAN_SCHEDULE_IREGUL_INPUT.Add(irregularRecordData);
                                }
                            }
                            AddLoanCovenant(entity, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);
                            AddLoanFees(entity.loanChargeFee, entity.createdBy, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility, entity.companyId, entity.feeOverride);
                            AddLoanCollateralMapping(entity.loanApplicationId, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            //if (entity.monitoringTriggers.Count > 0)
                            //    AddLoanMonitoringTrigger(entity.monitoringTriggers, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            entity.loanReferenceNumber = loan.LOANREFERENCENUMBER;
                            if (!entity.feeOverride) PostLoanFees(entity);
                            context.SaveChanges();

                            trans.Commit();
                        }
                        //.......................END OF APPROVAL LOG......................................................

                        return loanReferenceNumber;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (BadLogicException be)
                {
                    trans.Rollback();
                    throw new BadLogicException(be.Message);
                }
                catch (ConditionNotMetException ce)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (APIErrorException ae)
                {
                    trans.Rollback();
                    throw new APIErrorException(ae.Message);
                }
                catch (TwoFactorAuthenticationException fa)
                {
                    trans.Rollback();
                    throw new TwoFactorAuthenticationException(fa.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private string AddCommercialLoan(LoanViewModel entity)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(entity.loanApplicationId);
            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.loanBookingRequestId);
            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            if (entity.casaAccountId2 == null || entity.casaAccountId2 == 0)
                throw new ConditionNotMetException("Specify the recieving account.");

            if (request.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                throw new ConditionNotMetException("This Loan Request has already been booked by another staff");

            if (entity.effectiveDate > entity.maturityDate)
                throw new ConditionNotMetException("The effective cannot be greater than maturity date");

            if (entity.effectiveDate > generalSetup.GetApplicationDate())
                throw new ConditionNotMetException("You effective date cannot be post-dated.");

            if (applicationDetail.EXPIRYDATE != null && entity.maturityDate > applicationDetail.EXPIRYDATE)
                throw new ConditionNotMetException($"Commercial Loan maturity date should not exceed the line expiry date [{applicationDetail.EXPIRYDATE}]. ");

            var loans = context.TBL_LOAN.Where(x => x.LOANSTATUSID == (short)LoanStatusEnum.Active && x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).OrderByDescending(l => l.TERMLOANID);
            if (loans.Any())
            {
                if (loans.First().OUTSTANDINGPRINCIPAL > 0)
                    throw new ConditionNotMetException("The customer already have a running Commercial Loan which has not been paid down");
            }

            var principalAmount = from a in context.TBL_LOAN
                                  where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                  let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.PRINCIPALAMOUNT)
                                  select sumPrincipalAmount;

            double? priceIndex = (from a in context.TBL_PRODUCT where a.PRODUCTID == entity.productId select a.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXRATE).FirstOrDefault();

            var currentExchangeRate = financeTransaction.GetExchangeRate(DateTime.Now, (short)entity.currencyId, entity.companyId).sellingRate;

            var loanReferenceNumber = GenerateLoanReferenceNumber(application.BRANCHID, entity.productId, (short)LoanSystemTypeEnum.TermDisbursedFacility);

            var approvedAmount = applicationDetail.APPROVEDAMOUNT;

            var totalPreviouslyBookedAmount = principalAmount.FirstOrDefault();

            var totalPrincipalAmount = (decimal)(totalPreviouslyBookedAmount + (decimal)entity.principalAmount);

            if (totalPrincipalAmount > (decimal)approvedAmount)
                throw new ConditionNotMetException("The loan amount cannot be greater than the available amount");

            var data = new TBL_LOAN
            {
                LOAN_BOOKING_REQUESTID = entity.loanBookingRequestId,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                OPERATIONID = entity.operationId = (int)OperationsEnum.CommercialLoanBooking,
                LOANREFERENCENUMBER = loanReferenceNumber,
                LOANSTATUSID = (short)LoanStatusEnum.Inactive,
                SHOULD_DISBURSE = true,
                ISDISBURSED = false,
                PRINCIPALNUMBEROFINSTALLMENT = 1,
                INTERESTNUMBEROFINSTALLMENT = 1,
                SCH_PREPAYMENT_FREQUENCY_TYPID = null,
                PRODUCTPRICEINDEXRATE = (double)priceIndex,
                SUBSECTORID = applicationDetail.SUBSECTORID,
                CURRENCYID = (short)applicationDetail.CURRENCYID,
                EXCHANGERATE = currentExchangeRate,
                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,
                CUSTOMERID = entity.customerId,
                PRODUCTID = (short)applicationDetail.APPROVEDPRODUCTID,
                COMPANYID = entity.companyId,
                CASAACCOUNTID = entity.casaAccountId,
                CASAACCOUNTID2 = entity.casaAccountId2,
                BRANCHID = application.BRANCHID, //entity.branchId,
                LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                INTERESTRATE = Convert.ToInt32(applicationDetail.APPROVEDINTERESTRATE),
                ALLOWFORCEDEBITREPAYMENT = true,
                PRINCIPALINSTALLMENTLEFT = 0,
                INTERESTINSTALLMENTLEFT = 0,
                EQUITYCONTRIBUTION = 0,
                PRINCIPALADDITIONCOUNT = 0,
                PRINCIPALREDUCTIONCOUNT = 0,
                FIXEDPRINCIPAL = false,
                PROFILELOAN = false,

                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,

                SCHEDULETYPEID = (short)LoanScheduleTypeEnum.BulletPayment,
                SCHEDULEDAYCOUNTCONVENTIONID = (short)DayCountConventionEnum.Actual_Actual,
                SCHEDULEDPREPAYMENTAMOUNT = entity.scheduledPrepaymentAmount,
                PRINCIPALAMOUNT = Convert.ToDecimal(entity.loanPrincipal),
                OUTSTANDINGPRINCIPAL = Convert.ToDecimal(entity.loanPrincipal),
                CRMSREPAYMENTAGREEMENTID = entity.crmsRepaymentAgreementTypeId,

                EFFECTIVEDATE = (DateTime)applicationDetail.EFFECTIVEDATE,
                MATURITYDATE = (DateTime)applicationDetail.EXPIRYDATE,
                BOOKINGDATE = DateTime.Now,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
            };


            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AddCommercialLoanBooking,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    // ............. Checking customer balance, and fee override ......
                    entity.feeOverride = confirmCustomerAccountFunded(entity.loanChargeFee, entity.casaAccountId, entity.companyId, entity.customerId, loanReferenceNumber);

                    //...................Adding Commercial Loan Record.........................
                    var loan = context.TBL_LOAN.Add(data);

                    //request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.LoanBookingInProgress;

                    //...................Adding Audit...............................
                    var dataCount = context.SaveChanges();
                    if (dataCount > 0)
                    {
                        var approvalModel = new ForwardViewModel
                        {
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            applicationId = entity.loanBookingRequestId,
                            comment = "Please approve this Loan",
                            amount = entity.principalAmount,
                            operationId = (int)OperationsEnum.CommercialLoanBooking
                        };

                        //.....................LOG COMMERCIAL LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                        if (LogApproval(approvalModel, (int)OperationsEnum.CommercialLoanBooking, false, (int)ApprovalStatusEnum.Processing))
                        {
                            AddLoanCovenant(entity, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);
                            AddLoanFees(entity.loanChargeFee, entity.createdBy, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility, entity.companyId, entity.feeOverride);

                            //...................Saving Commercial Loan Collaterals Mapping.......................
                            AddLoanCollateralMapping(entity.loanApplicationId, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            //...................Mapping Commercial Loan Monitoring Trigger.......................
                            //if (entity.monitoringTriggers.Count > 0)
                            //    AddLoanMonitoringTrigger(entity.monitoringTriggers, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            entity.loanReferenceNumber = loan.LOANREFERENCENUMBER;
                            if (!entity.feeOverride) PostLoanFees(entity);
                            context.SaveChanges();

                            //.....Commit transaction ............
                            trans.Commit();
                        }

                        //.......................END OF APPROVAL LOG......................................................

                        return loanReferenceNumber;
                    }
                    else
                    {
                        return "";
                    }

                }
                catch (BadLogicException be)
                {
                    trans.Rollback();
                    throw new BadLogicException(be.Message);
                }
                catch (ConditionNotMetException ce)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (APIErrorException ae)
                {
                    trans.Rollback();
                    throw new APIErrorException(ae.Message);
                }
                catch (TwoFactorAuthenticationException fa)
                {
                    trans.Rollback();
                    throw new TwoFactorAuthenticationException(fa.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        
        private string AddFXLoan(LoanViewModel entity)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(entity.loanApplicationId);
            var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.loanBookingRequestId);
            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);

            if (request.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                throw new ConditionNotMetException("This Loan Request has already been booked by another staff");

            if (entity.effectiveDate > entity.maturityDate)
                throw new ConditionNotMetException("The effective cannot be greater than maturity date");

            if (entity.effectiveDate > generalSetup.GetApplicationDate())
                throw new ConditionNotMetException("You effective date cannot be post-dated.");

            if (applicationDetail.EXPIRYDATE != null && entity.maturityDate > applicationDetail.EXPIRYDATE)
                throw new ConditionNotMetException($"FX revolving loan maturity date should not exceed the line expiry date [{applicationDetail.EXPIRYDATE}]. ");

            var loans = context.TBL_LOAN.Where(x => x.LOANSTATUSID == (short)LoanStatusEnum.Active && x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).OrderByDescending(l => l.TERMLOANID);

            var principalAmount = from a in context.TBL_LOAN
                                  where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                  let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.PRINCIPALAMOUNT)
                                  select sumPrincipalAmount;

            double? priceIndex = (from a in context.TBL_PRODUCT where a.PRODUCTID == entity.productId select a.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXRATE).FirstOrDefault();

            var currentExchangeRate = financeTransaction.GetExchangeRate(DateTime.Now, (short)entity.currencyId, entity.companyId).sellingRate;

            var loanReferenceNumber = GenerateLoanReferenceNumber(application.BRANCHID, entity.productId, (short)LoanSystemTypeEnum.TermDisbursedFacility);

            var approvedAmount = applicationDetail.APPROVEDAMOUNT;

            var totalPreviouslyBookedAmount = principalAmount.FirstOrDefault();

            var totalPrincipalAmount = (decimal)(totalPreviouslyBookedAmount + (decimal)entity.principalAmount);

            if (totalPrincipalAmount > (decimal)approvedAmount)
                throw new ConditionNotMetException("The loan amount cannot be greater than the availiable amount");

            var nostroAccount = context.TBL_CUSTOM_CHART_OF_ACCOUNT.Find(entity.casaAccountId2);
            var nostroAccountNumber = nostroAccount.ACCOUNTID;
            var nostroCurrencyId = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYCODE == nostroAccount.CURRENCYCODE).CURRENCYID;

            var data = new TBL_LOAN
            {
                LOAN_BOOKING_REQUESTID = entity.loanBookingRequestId,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                OPERATIONID = entity.operationId = (int)OperationsEnum.ForeignExchangeLoanBooking,
                LOANREFERENCENUMBER = loanReferenceNumber,
                LOANSTATUSID = (short)LoanStatusEnum.Inactive,
                SHOULD_DISBURSE = true,
                ISDISBURSED = false,
                PRINCIPALNUMBEROFINSTALLMENT = 1,
                INTERESTNUMBEROFINSTALLMENT = 1,
                SCH_PREPAYMENT_FREQUENCY_TYPID = null,
                PRODUCTPRICEINDEXRATE = (double)priceIndex,
                SUBSECTORID = applicationDetail.SUBSECTORID,
                CURRENCYID = (short)applicationDetail.CURRENCYID,
                EXCHANGERATE = currentExchangeRate,
                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,
                CUSTOMERID = entity.customerId,
                PRODUCTID = (short)applicationDetail.APPROVEDPRODUCTID,
                COMPANYID = entity.companyId,
                CASAACCOUNTID = entity.casaAccountId,
                NOSTROACCOUNTID = nostroAccountNumber,
                NOSTRORATECODEID = entity.nostroRateCodeId,
                NOSTRORATEAMOUNT = entity.nostroRateAmount,
                NOSTROCURRENCYID = nostroCurrencyId,
                BRANCHID = application.BRANCHID,
                LOANSYSTEMTYPEID = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                INTERESTRATE = Convert.ToInt32(applicationDetail.APPROVEDINTERESTRATE),
                ALLOWFORCEDEBITREPAYMENT = true,
                PRINCIPALINSTALLMENTLEFT = 0,
                INTERESTINSTALLMENTLEFT = 0,
                EQUITYCONTRIBUTION = 0,
                PRINCIPALADDITIONCOUNT = 0,
                PRINCIPALREDUCTIONCOUNT = 0,
                FIXEDPRINCIPAL = false,
                PROFILELOAN = false,

                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                USER_PRUDENTIAL_GUIDE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                EXT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,
                INT_PRUDENT_GUIDELINE_STATUSID = (short)LoanPrudentialStatusEnum.Performing,

                SCHEDULETYPEID = (short)LoanScheduleTypeEnum.BulletPayment,
                SCHEDULEDAYCOUNTCONVENTIONID = (short)DayCountConventionEnum.Actual_Actual,
                SCHEDULEDPREPAYMENTAMOUNT = entity.scheduledPrepaymentAmount,
                PRINCIPALAMOUNT = Convert.ToDecimal(entity.loanPrincipal),
                OUTSTANDINGPRINCIPAL = Convert.ToDecimal(entity.loanPrincipal),
                CRMSREPAYMENTAGREEMENTID = entity.crmsRepaymentAgreementTypeId,

                EFFECTIVEDATE = (DateTime)entity.effectiveDate,
                MATURITYDATE = (DateTime)entity.maturityDate,
                BOOKINGDATE = DateTime.Now,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ForeignExchangeLoanAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Applied for FX-loan with reference number: {loanReferenceNumber}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    //...Checking customer balance, and fee override...
                    entity.feeOverride = confirmCustomerAccountFunded(entity.loanChargeFee, entity.casaAccountId, entity.companyId, entity.customerId, loanReferenceNumber);

                    //...Adding Commercial Loan Record...
                    var loan = context.TBL_LOAN.Add(data);

                    //...Update the Loan Request and loan application table...
                    request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                    application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.LoanBookingInProgress;

                    //...Adding Audit...
                    var dataCount = context.SaveChanges();
                    if (dataCount > 0)
                    {
                        var approvalModel = new ForwardViewModel
                        {
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            applicationId = entity.loanBookingRequestId,
                            comment = "Please approve this Loan",
                            amount = entity.principalAmount,
                            operationId = (int)OperationsEnum.ForeignExchangeLoanBooking
                        };

                        //.....................LOG FX LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                        if (LogApproval(approvalModel, (int)OperationsEnum.ForeignExchangeLoanBooking, false, (int)ApprovalStatusEnum.Processing))
                        {
                            AddLoanCovenant(entity, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);
                            AddLoanFees(entity.loanChargeFee, entity.createdBy, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility, entity.companyId, entity.feeOverride);

                            //...................Saving FX Loan Collaterals Mapping.......................
                            AddLoanCollateralMapping(entity.loanApplicationId, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            //...................Mapping FX Loan Monitoring Trigger.......................
                            //if (entity.monitoringTriggers.Count > 0)
                            //    AddLoanMonitoringTrigger(entity.monitoringTriggers, loan.TERMLOANID, (short)LoanSystemTypeEnum.TermDisbursedFacility);

                            if (entity.loanBeneficiary.Count > 0)
                            {
                                foreach (var item in entity.loanBeneficiary)
                                {
                                    item.loanId = loan.TERMLOANID;
                                };
                                addLoanBeneficiary(entity.loanBeneficiary);
                            }

                            entity.loanReferenceNumber = loan.LOANREFERENCENUMBER;
                            if (!entity.feeOverride) PostLoanFees(entity);
                            context.SaveChanges();

                            //.....Commit transaction ............
                            trans.Commit();
                        }

                        //.......................END OF APPROVAL LOG......................................................

                        //.....Commit transaction ............
                        trans.Commit();

                        return loanReferenceNumber;
                    }
                    else
                    {
                        return "";
                    }

                }
                catch (BadLogicException be)
                {
                    trans.Rollback();
                    throw new BadLogicException(be.Message);
                }
                catch (ConditionNotMetException ce)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (APIErrorException ae)
                {
                    trans.Rollback();
                    throw new APIErrorException(ae.Message);
                }
                catch (TwoFactorAuthenticationException fa)
                {
                    trans.Rollback();
                    throw new TwoFactorAuthenticationException(fa.Message);
                }
                catch (SecureException xe)
                {
                    trans.Rollback();
                    throw new ConditionNotMetException(xe.Message);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool addLoanBeneficiary(List<LoanDisbursementViewModel> entity)
        {
            List<TBL_LOAN_DISBURSEMENT> loanDisbursementList = new List<TBL_LOAN_DISBURSEMENT>();
            foreach(var model in entity)
            {
                var beneficiary = new TBL_LOAN_DISBURSEMENT
                {
                    AMOUNTDISBURSED = model.amountDisbursed,
                   // CURRENCYID = model.beneficiaryCurrencyId,
                    NARRATION = model.beneficiaryReason,
                   // RATECODEID = model.beneficiaryRateCodeId,
                   // RATEAMOUNT = model.beneficiaryRateAmount,
                    TERMLOANID = model.loanId,
                    DATETIMECREATED = DateTime.Now,
                    CREATEDBY = model.createdBy,
                };
                loanDisbursementList.Add(beneficiary);
                
            }
            context.TBL_LOAN_DISBURSEMENT.AddRange(loanDisbursementList);
            //return context.SaveChanges() > 0;

            return true;
        }

        public bool LogApproval(ForwardViewModel model, int operationId, bool externalInitialization, int ApprovalStatusId)
        {
            if (externalInitialization)
            {
                workflow.StaffId = model.createdBy;
                workflow.OperationId = operationId;
                workflow.TargetId = model.applicationId;
                workflow.CompanyId = model.companyId;
                workflow.Comment = model.comment;
                workflow.ExternalInitialization = externalInitialization;
                workflow.StatusId = ApprovalStatusId;
                workflow.Amount = model.amount;
            }
            

            if (!externalInitialization)
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = ApprovalStatusId;
                workflow.TargetId = model.applicationId;
                workflow.Comment = model.comment;
                workflow.OperationId = operationId;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = false;
            }

            workflow.LogActivity();

            return context.SaveChanges() >0;
        }

        public void DisburseLoan(LoanViewModel entity, TwoFactorAutheticationViewModel twoFactorAuthDetails = null)
        {
            //PostLoanDisbursment(entity);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.AddRange(BuildLoanDisbursmentPosting(entity));

            var feePostings = BuildLoanChargeFeesPosting(entity);

            //if fee has not been taken from the customer account due to override
            if (feePostings.Count() > 0)
                inputTransactions.AddRange(feePostings);

            financeTransaction.PostTransaction(inputTransactions, false, twoFactorAuthDetails);

            //if (feePostings.Count() > 0)
            //    financeTransaction.PostTransaction(feePostings);
        }

        public void PostLoanFees(LoanViewModel entity)
        {
            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                username = entity.username,
                passcode = entity.passCode
            };

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

            if (inputTransactions.Count > 0) financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);
        }

        public List<ApprovalLevelStaffViewModel> GetLoanOperationApprovers(int operation, int companyId)
        {
            return level.GetAllAssignedApprovalLevelStaff(companyId)
                                                .Where(x => x.operationId == operation).ToList();
        }

        public int GoForBookingRequestApproval(ApprovalViewModel entity, int loanBookingRequestId)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    var request = context.TBL_LOAN_BOOKING_REQUEST.Find(entity.targetId);
                    var applicationDet = context.TBL_LOAN_APPLICATION_DETAIL.Find(request.LOANAPPLICATIONDETAILID);
                    var application = context.TBL_LOAN_APPLICATION.Find(applicationDet.LOANAPPLICATIONID);

                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                    workflow.TargetId = entity.targetId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = entity.operationId;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;
                    workflow.FinalLevel = application.TRANCHEAPPROVAL_LEVELID;

                    workflow.LogActivity();

                    context.SaveChanges();

                    if ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                        trans.Commit();
                        context.SaveChanges();
                        return 3;
                    }
                        
                    else if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        request.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                        var operationId = 0;
                        if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan)
                            operationId = (short)OperationsEnum.CommercialLoanBooking;
                        if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                            operationId = (short)OperationsEnum.ContigentLoanBooking;
                        if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan || request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating || request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SyndicatedTermLoan)
                            operationId = (short)OperationsEnum.TermLoanBooking;
                        if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving)
                            operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                        if (request.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                            operationId = (short)OperationsEnum.RevolvingLoanBooking;

                        var approvalModel = new ForwardViewModel
                        {
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            applicationId = request.LOAN_BOOKING_REQUESTID,
                            comment = "A request for booking needs your attention",
                            amount = request.AMOUNT_REQUESTED,
                        };

                        if (operationId > 0) LogApproval(approvalModel, operationId, true, (short)ApprovalStatusEnum.Pending);
                        application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestCompleted;
                        context.SaveChanges();
                        trans.Commit();
                        return 0;
                    }

                    else
                    {
                        application.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.BookingRequestInitiated;
                        context.SaveChanges();
                        trans.Commit();
                        return 1;
                    }
                }
                catch (ConditionNotMetException ce)
                {
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (BadLogicException be)
                {
                    throw new BadLogicException(be.Message);
                }
                catch (APIErrorException e)
                {
                    throw new APIErrorException(e.Message);
                }
                catch (Exception e)
                {
                    //trans.Rollback();
                    throw new ConditionNotMetException("Approval failed. Operation unsuccessful. " + e.Message);
                }
            }
        }
        /// <summary>
        /// Gets the term loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<CamProcessedLoanViewModel> GetBookingRequestAwaitingApproval(int staffId, int companyId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanBookingRequest).ToList();
            List<int> operationIds = new List<int>();
            operationIds.Add((int)OperationsEnum.LoanBookingRequest);

            try
            {
                var data = (from req in context.TBL_LOAN_BOOKING_REQUEST 
                            join d in context.TBL_LOAN_APPLICATION_DETAIL on req.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                            join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                            join coy in context.TBL_COMPANY on m.COMPANYID equals coy.COMPANYID
                            join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                            join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                            join br in context.TBL_BRANCH on m.BRANCHID equals br.BRANCHID
                            join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                            where ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing) || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending))
                                  && operationIds.Contains(atrail.OPERATIONID)
                                  && req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                                  && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CAMInProgress 
                                  && m.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                  && atrail.RESPONSESTAFFID == null
                            orderby d.LOANAPPLICATIONDETAILID descending

                            select new CamProcessedLoanViewModel
                            {
                                loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                                approvalStatusId = (short)m.APPROVALSTATUSID,
                                loanApplicationId = m.LOANAPPLICATIONID,
                                loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                                applicationStatusId = m.APPLICATIONSTATUSID,
                                operationId = (short)OperationsEnum.LoanBookingRequest,
                                requestedAmount = req.AMOUNT_REQUESTED,
                                customerId = m.CUSTOMERID ?? 0,
                                customerCode = cust.CUSTOMERCODE,

                                customerName = cust.FIRSTNAME + " " + cust.MIDDLENAME + " " + cust.LASTNAME,
                                customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                                customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                                isRelatedParty = m.ISRELATEDPARTY,
                                customerSensitivityLevelId = cust.CUSTOMERSENSITIVITYLEVELID,
                                customerOccupation = cust.OCCUPATION,
                                customerType = cust.TBL_CUSTOMER_TYPE.NAME,

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
                                productTypeId = p.PRODUCTTYPEID,
                                productTypeName = p.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                productName = p.PRODUCTNAME,
                                productClassProcessId = m.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,
                                misCode = m.MISCODE,
                                teamMisCode = m.TEAMMISCODE,

                                interestRate = d.APPROVEDINTERESTRATE,
                                approvedAmount = d.APPROVEDAMOUNT,
                                approvedDate = m.APPROVEDDATE,
                                groupApprovedAmount = m.APPROVEDAMOUNT,
                                approvedTenor = d.APPROVEDTENOR,
                                createdBy = m.CREATEDBY,
                                newApplicationDate = m.APPLICATIONDATE,
                                dateTimeCreated = d.DATETIMECREATED,
                                availmentDate = m.AVAILMENTDATE,
                                requestDate = req.DATETIMECREATED
                            }).ToList();

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
                return data.ToList();
            }
            catch (Exception ex)
            {
                throw new ConditionNotMetException("An error Occured reading loan application details");
            }

        }

       
        public IEnumerable<LoanViewModel> GetLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TermLoanBooking).ToList();
            List<int> operationIds = new List<int>();
            operationIds.Add((int)OperationsEnum.TermLoanBooking);
            operationIds.Add((int)OperationsEnum.CommercialLoanBooking);
            operationIds.Add((int)OperationsEnum.ForeignExchangeLoanBooking);

            try
            {
                var data = (from ln in context.TBL_LOAN
                            join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                            join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                            where (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                  && operationIds.Contains(atrail.OPERATIONID)
                                  && req.DELETED == false
                                  && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                  && ln.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)// == staffApprovalLevelId
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.TERMLOANID descending

                            select new LoanViewModel()
                            {
                                loanId = ln.TERMLOANID,
                                operationId = ln.OPERATIONID,
                                operationTypeId = atrail.OPERATIONID,
                                loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                casaAccountId = ln.CASAACCOUNTID,
                                casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                                loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,
                                loanApplicationId = ln.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                
                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                                staffId = staffId,

                                pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION ?? null,
                                interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE1.DESCRIPTION ?? null,

                                principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                misCode = ln.MISCODE,
                                teamMiscode = ln.TEAMMISCODE,
                                interestRate = ln.INTERESTRATE,
                                effectiveDate = ln.EFFECTIVEDATE,
                                maturityDate = ln.MATURITYDATE,
                                bookingDate = ln.BOOKINGDATE,
                                principalAmount = ln.PRINCIPALAMOUNT,
                                principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                approvalStatusId = ln.APPROVALSTATUSID,
                                approvedBy = ln.APPROVEDBY,
                                approverComment = ln.APPROVERCOMMENT,
                                dateApproved = ln.DATEAPPROVED,
                                scheduleTypeId = ln.SCHEDULETYPEID,
                                isDisbursed = ln.ISDISBURSED,
                                disbursedBy = ln.DISBURSEDBY,
                                disburserComment = ln.DISBURSERCOMMENT,
                                disburseDate = ln.DISBURSEDATE,
                                disbursableAmount = ln.PRINCIPALAMOUNT,
                                approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,

                                loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                equityContribution = ln.EQUITYCONTRIBUTION,
                                subSectorId = ln.SUBSECTORID,
                                subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,

                                firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                fixedPrincipal = ln.FIXEDPRINCIPAL,
                                profileLoan = ln.PROFILELOAN,
                                dischargeLetter = ln.DISCHARGELETTER,
                                suspendInterest = ln.SUSPENDINTEREST,

                                scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,

                                firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                                middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                                lastName = ln.TBL_CUSTOMER.LASTNAME,
                                customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                                productAccountName = ln.TBL_PRODUCT.PRODUCTNAME,
                                loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                                currencyId = ln.CURRENCYID,

                                branchName = ln.TBL_BRANCH.BRANCHNAME,
                                relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                                relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                                productName = ln.TBL_PRODUCT.PRODUCTNAME,
                                productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,

                                createdBy = ln.CREATEDBY,
                                creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                                dateTimeCreated = ln.DATETIMECREATED,
                                comment = "",
                                isBidbond = false,
                                isOverdraft = false,

                                requestStatusId = req.APPROVALSTATUSID,
                                requestDeleted = req.DELETED,
                                loanStatusId = ln.LOANSTATUSID,

                                loanCollateral = (from cm in context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANID == ln.TERMLOANID && x.LOANSYSTEMTYPEID == ln.LOANSYSTEMTYPEID)
                                                  select (new LoanCollateralMappingViewModel
                                                  {
                                                      collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                                               + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME + ")",
                                                      collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                                      collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                      hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                      valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                      currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                                      currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                      currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME
                                                  })).ToList(),
                                monitoringTriggers = (from i in context.TBL_LOAN_MONITORING_TRIGGER.Where(x => x.LOANID == ln.TERMLOANID && x.LOANSYSTEMTYPEID == ln.LOANSYSTEMTYPEID)
                                                      select (
                                                               new LoanMonitoringTriggerViewModel
                                                               {
                                                                   loanMonitoringTriggerId = i.LOAN_MONITORING_TRIGGERID,
                                                                   loanSystemTypeId = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                                                                   monitoringTriggerId = i.MONITORING_TRIGGERID,
                                                                   monitoringTrigger = i.MONITORING_TRIGGER,
                                                                   monitoringTriggerSetupName = i.TBL_LOAN_MONITORING_TRIG_SETUP.MONITORING_TRIGGER_NAME,
                                                               })).ToList(),


                            });

                return data;
            }
            catch (Exception ex)
            {
                throw new ConditionNotMetException("An error Occured reading term loan details");
            }

        }

        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<RevolvingLoanViewModel> GetRevolvingFacilityBookingAwaitingApproval(int staffId, int companyId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TermLoanBooking).ToList();
            try
            {
                var data = (from ln in context.TBL_LOAN_REVOLVING
                            join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                            join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                            where (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                  && atrail.OPERATIONID == (int)OperationsEnum.RevolvingLoanBooking
                                  && req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                  && ln.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.REVOLVINGLOANID descending
                            select new RevolvingLoanViewModel()
                            {
                                loanId = ln.REVOLVINGLOANID,
                                operationId = ln.OPERATIONID,
                                operationTypeId = (int)OperationsEnum.RevolvingLoanBooking,
                                loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                                customerId = ln.CUSTOMERID,
                                productId = ln.PRODUCTID,
                                casaAccountId = ln.CASAACCOUNTID,
                                casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                                loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,
                                loanApplicationId = ln.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,

                                branchId = ln.BRANCHID,
                                loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                                relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                misCode = ln.MISCODE,
                                teamMiscode = ln.TEAMMISCODE,
                                interestRate = ln.INTERESTRATE,
                                effectiveDate = ln.EFFECTIVEDATE,
                                maturityDate = ln.MATURITYDATE,
                                bookingDate = ln.BOOKINGDATE,

                                approvalStatusId = ln.APPROVALSTATUSID,
                                // approvedBy = (int)ln.APPROVEDBY,
                                approverComment = ln.APPROVERCOMMENT,
                                dateApproved = ln.DATEAPPROVED,
                                loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                isDisbursed = ln.ISDISBURSED,
                                disbursedBy = ln.DISBURSEDBY,
                                disburserComment = ln.DISBURSERCOMMENT,
                                disburseDate = ln.DISBURSEDATE,
                                loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,

                                overdraftLimit = ln.OVERDRAFTLIMIT,
                                approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                disbursableAmount = ln.OVERDRAFTLIMIT,
                                customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,

                                subSectorId = ln.SUBSECTORID,
                                subSectorName = ln.TBL_SUB_SECTOR.NAME,

                                dischargeLetter = ln.DISCHARGELETTER,
                                suspendInterest = ln.SUSPENDINTEREST,

                                customerSensitivityLevelId = ln.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                customerSensitivityLevelName = ln.TBL_CUSTOMER.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                                firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                                middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                                lastName = ln.TBL_CUSTOMER.LASTNAME,
                                customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                                productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                                loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                                currencyId = ln.CURRENCYID,

                                branchName = ln.TBL_BRANCH.BRANCHNAME,
                                relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                                relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                                productName = ln.TBL_PRODUCT.PRODUCTNAME,
                                productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,

                                createdBy = ln.CREATEDBY,
                                creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                                dateTimeCreated = ln.DATETIMECREATED,
                                comment = "",
                                isBidbond = false,
                                isOverdraft = false,
                                loanCollateral = (from cm in context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANID == ln.REVOLVINGLOANID && x.LOANSYSTEMTYPEID == ln.LOANSYSTEMTYPEID)
                                                  select (
                                                           new LoanCollateralMappingViewModel
                                                           {
                                                               loanCollateralMappingId = cm.LOANCOLLATERALMAPPINGID,
                                                               collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                                               + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME + ")",
                                                               collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                                               collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                               hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                               valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                               currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                                               currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                               currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME
                                                           })).ToList(),
                                monitoringTriggers = (from i in context.TBL_LOAN_MONITORING_TRIGGER.Where(x => x.LOANID == ln.REVOLVINGLOANID && x.LOANSYSTEMTYPEID == ln.TBL_PRODUCT.PRODUCTTYPEID)
                                                      select (
                                                               new LoanMonitoringTriggerViewModel
                                                               {
                                                                   loanMonitoringTriggerId = i.LOAN_MONITORING_TRIGGERID,
                                                                   loanSystemTypeId = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                                                                   monitoringTriggerId = i.MONITORING_TRIGGERID,
                                                                   monitoringTrigger = i.MONITORING_TRIGGER,
                                                                   monitoringTriggerSetupName = i.TBL_LOAN_MONITORING_TRIG_SETUP.MONITORING_TRIGGER_NAME,
                                                               })).ToList(),

                            });
                return data;
            }
            catch (Exception ex)
            {
                throw new BadLogicException("An error Occured reading reading revolving loan details");
            }

        }

        /// <summary>
        /// Gets the contingent loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<ContingentLoanViewModel> GetContingentFacilityBookingAwaitingApproval(int staffId, int companyId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ContigentLoanBooking).ToList();
            

            var data = (from ln in context.TBL_LOAN_CONTINGENT
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on req.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                        where (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                              && atrail.OPERATIONID == (int)OperationsEnum.ContigentLoanBooking
                              && req.DELETED == false && req.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                              && ln.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                              && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.CONTINGENTLOANID descending

                        select new ContingentLoanViewModel()
                        {
                            loanId = ln.CONTINGENTLOANID,
                            operationId = (int)OperationsEnum.ContigentLoanBooking,
                            operationTypeId = (int)OperationsEnum.ContigentLoanBooking,
                            loanBookingRequestId = req.LOAN_BOOKING_REQUESTID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                            loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,
                            loanApplicationId = ln.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,

                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                            relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                            relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                            misCode = ln.MISCODE,
                            teamMiscode = ln.TEAMMISCODE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,

                            approvalStatusId = ln.APPROVALSTATUSID,
                            //approvedBy = (int)ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            loanStatusId = ln.LOANSTATUSID,
                            loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,
                            loanSystemTypeId =ln.LOANSYSTEMTYPEID,
                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                            disbursableAmount = ln.CONTINGENTAMOUNT,

                            customerGroupId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                            loanTypeId = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            dischargeLetter = ln.DISCHARGELETTER,

                            customerSensitivityLevelId = ln.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                            customerSensitivityLevelName = ln.TBL_CUSTOMER.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                            middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                            lastName = ln.TBL_CUSTOMER.LASTNAME,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            createdBy = ln.CREATEDBY,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            comment = "",
                            isBidbond = ln.TBL_PRODUCT.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees ? true : false,
                            isOverdraft = ln.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan ? true : false,

                            loanCollateral = (from cm in context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANID == ln.CONTINGENTLOANID && x.LOANSYSTEMTYPEID == ln.LOANSYSTEMTYPEID)
                                              select (
                                                       new LoanCollateralMappingViewModel
                                                       {
                                                           loanCollateralMappingId = cm.LOANCOLLATERALMAPPINGID,
                                                           collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                                           + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME + ")",
                                                           collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                                           loanApplicationId = cm.LOANID,
                                                           collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                           hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                           valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                           currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                                           currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                           currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME
                                                       })).ToList(),
                            monitoringTriggers = (from i in context.TBL_LOAN_MONITORING_TRIGGER.Where(x => x.LOANID == ln.CONTINGENTLOANID && x.LOANSYSTEMTYPEID == ln.TBL_PRODUCT.PRODUCTTYPEID)
                                                  select (
                                                           new LoanMonitoringTriggerViewModel
                                                           {
                                                               loanMonitoringTriggerId = i.LOAN_MONITORING_TRIGGERID,
                                                               loanSystemTypeId = (short)LoanSystemTypeEnum.TermDisbursedFacility,
                                                               monitoringTriggerId = i.MONITORING_TRIGGERID,
                                                               monitoringTrigger = i.MONITORING_TRIGGER,
                                                               monitoringTriggerSetupName = i.TBL_LOAN_MONITORING_TRIG_SETUP.MONITORING_TRIGGER_NAME,
                                                           })).ToList(),
                        });

            return data;
        }


        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool GoForFeeOverrideApproval(ApprovalViewModel entity)
        {
            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.LogForApproval(entity);

                    var b = workflow.NextLevelId ?? 0;

                    if (b == 0 && workflow.NewState != (int)ApprovalState.Ended)
                    {
                        trans.Rollback();
                        throw new BadLogicException("Approval Failed");
                    }

                    try
                    {
                        if (workflow.NewState != (int)ApprovalState.Ended)
                        {
                            var feeRec = context.TBL_LOAN_FEE.Find(entity.targetId);
                            var allTargetLoanFees = context.TBL_LOAN_FEE.Where(x => x.LOANID == feeRec.LOANID);
                            foreach (var fee in allTargetLoanFees)
                            {
                                fee.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }

                            ApprovalViewModel approvalModel = new ApprovalViewModel();
                            if (entity.operationId == (int)OperationsEnum.TermLoanBooking)
                            {
                                var termLoanRecord = context.TBL_LOAN.Find(feeRec.LOANID);
                                approvalModel.targetId = termLoanRecord.TERMLOANID;
                                approvalModel.amount = termLoanRecord.PRINCIPALAMOUNT;
                            }
                            else if (entity.operationId == (int)OperationsEnum.RevolvingLoanBooking)
                            {
                                var revolvingLoanRecord = context.TBL_LOAN_REVOLVING.Find(feeRec.LOANID);
                                approvalModel.targetId = revolvingLoanRecord.REVOLVINGLOANID;
                                approvalModel.amount = revolvingLoanRecord.OVERDRAFTLIMIT;
                            }
                            else if (entity.operationId == (int)OperationsEnum.ContigentLoanBooking)
                            {
                                var contingentLoanRecord = context.TBL_LOAN_CONTINGENT.Find(feeRec.LOANID);
                                approvalModel.targetId = contingentLoanRecord.CONTINGENTLOANID;
                                approvalModel.amount = contingentLoanRecord.CONTINGENTAMOUNT;
                            }

                            approvalModel.companyId = entity.companyId;
                            approvalModel.createdBy = entity.createdBy;

                            approvalModel.approvalStatusId = (int)ApprovalStatusEnum.Pending;
                            approvalModel.externalInitialization = true;
                            approvalModel.operationId = (int)OperationsEnum.LoanBookingFeeDeferral;

                            workflow.LogForApproval(approvalModel);

                            return context.SaveChanges() > 0;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw new SecureException("Approval failed. " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }

        }

        public int getDaysInLoanPeriod(DateTime startDate, DateTime endDate)
        {
            int totalDays = 0;
            if (endDate.Year > startDate.Year)
            {
                for (int year = startDate.Year; year <= endDate.Year; year++)
                {
                    DateTime newDate = startDate;
                    bool isLastdate = false;

                    if (new DateTime(year, 12, 31) >= endDate)
                        isLastdate = true;

                    DateTime lastDate = !isLastdate ? new DateTime(year, 12, 31) : endDate;

                    var days = (int)(lastDate - newDate).TotalDays;
                    totalDays = +days;
                }
            }
            else
            {
                totalDays = (int)(endDate - startDate).TotalDays;
            }
            return totalDays;
        }

        public decimal getDailyInterest(decimal principal, double interestRate, int interestDaysPeriod )
        {
            return  decimal.Round(((decimal)(interestRate / 100) * principal * 1) / interestDaysPeriod, 4);
        }

        public decimal getTotalInterest(decimal principal, double interestRate, int interestDaysPeriod)
        {
            Decimal dailyInterest = decimal.Round(((decimal)(interestRate / 100) * principal * 1) / interestDaysPeriod, 4);
            Decimal totalInterest = (decimal)(dailyInterest * (decimal)interestDaysPeriod);

            return totalInterest;
        }

        /// <summary>
        /// Approves the loan booking.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="approvalStatusId">The approval status identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>

        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public int GoForApproval(ApprovalViewModel entity, int loanBookingRequestId)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = ((int)entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) ? (int)ApprovalStatusEnum.Processing : (int)entity.approvalStatusId;
                    workflow.TargetId = loanBookingRequestId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = entity.operationId;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;

                    workflow.LogActivity();

                    context.SaveChanges();

                    if (ApproveLoanBooking(entity.targetId, loanBookingRequestId, (short)workflow.StatusId, entity))
                    {
                        trans.Commit();
                        if (workflow.NewState != (int)ApprovalState.Ended)
                        {
                            if (entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) return 1;
                            else return 3;
                        }
                        else
                        {
                            if (entity.approvalStatusId == (int)ApprovalStatusEnum.Approved) return 2;
                            else return 3;
                        }
                    }
                    else
                    {
                        trans.Rollback();
                        return 0;
                    }
                }
                catch (ConditionNotMetException ce)
                {
                    throw new ConditionNotMetException(ce.Message);
                }
                catch (BadLogicException be)
                {
                    throw new BadLogicException(be.Message);
                }
                catch (APIErrorException e)
                {
                    throw new APIErrorException(e.Message);
                }
                catch (TwoFactorAuthenticationException e)
                {
                    throw new TwoFactorAuthenticationException(e.Message);
                }
                catch (Exception e)
                {
                    //trans.Rollback();
                    throw new ConditionNotMetException("Approval failed. Operation unsuccessful. " + e.Message);
                }
            }
        }

        /// <summary>
        /// Approves the loan booking.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="approvalStatusId">The approval status identifier.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private bool ApproveLoanBooking(int loanId, int loanBookingRequestId, short approvalStatusId, ApprovalViewModel user)
        {
            var loanRecord = context.TBL_LOAN.Find(loanId);
            var revolvingLoanRecord = context.TBL_LOAN_REVOLVING.Find(loanId);
            var contingentLoanRecord = context.TBL_LOAN_CONTINGENT.Find(loanId);
            var loanReferenceNumber = string.Empty;
            var systemDate = generalSetup.GetApplicationDate();

            var twoFactorAuthDetails = new TwoFactorAutheticationViewModel
            {
                username = user.userName,
                passcode = user.passCode
            };

            if (workflow.NewState != (int)ApprovalState.Ended)
            {
                if ((user.operationId == (int)OperationsEnum.TermLoanBooking || user.operationId == (int)OperationsEnum.CommercialLoanBooking
                        || user.operationId == (int)OperationsEnum.ForeignExchangeLoanBooking) && user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                {
                    loanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    loanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
                    loanRecord.APPROVEDBY = user.staffId;
                    loanRecord.APPROVERCOMMENT = user.comment;
                }
                else if ((user.operationId == (int)OperationsEnum.TermLoanBooking || user.operationId == (int)OperationsEnum.CommercialLoanBooking
                        || user.operationId == (int)OperationsEnum.ForeignExchangeLoanBooking) && user.approvalStatusId != (short)ApprovalStatusEnum.Disapproved)
                {
                    loanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    context.SaveChanges();
                    return true;
                }

                if (user.operationId == (int)OperationsEnum.RevolvingLoanBooking && user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                {
                    revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    revolvingLoanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
                    revolvingLoanRecord.APPROVEDBY = user.staffId;
                    revolvingLoanRecord.APPROVERCOMMENT = user.comment;
                }
                else if (user.operationId == (int)OperationsEnum.RevolvingLoanBooking && user.approvalStatusId != (short)ApprovalStatusEnum.Disapproved)
                {
                    revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    context.SaveChanges();
                    return true;
                }

                if (user.operationId == (int)OperationsEnum.ContigentLoanBooking && user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                {
                    contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    contingentLoanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
                    contingentLoanRecord.APPROVEDBY = user.staffId;
                    contingentLoanRecord.APPROVERCOMMENT = user.comment;
                }
                else if (user.operationId == (int)OperationsEnum.ContigentLoanBooking && user.approvalStatusId != (short)ApprovalStatusEnum.Disapproved)
                {
                    contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                    context.SaveChanges();
                    return true;
                }
            }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if(user.operationId == (int)OperationsEnum.RevolvingLoanBooking)
                {
                    ProcessRevolvingLoanFacilityApproval(loanId, twoFactorAuthDetails, revolvingLoanRecord, user);
                }

                if (user.operationId == (int)OperationsEnum.ContigentLoanBooking)
                {
                    ProcessContingentLiabilityApproval(loanId, twoFactorAuthDetails, contingentLoanRecord, user);
                }

                if (user.operationId == (int)OperationsEnum.TermLoanBooking || user.operationId == (int)OperationsEnum.CommercialLoanBooking
                    || user.operationId == (int)OperationsEnum.ForeignExchangeLoanBooking)
                {
                    ProcessLoanBookingApproval(loanId, twoFactorAuthDetails, loanRecord, user);
                }
                

                // Audit Section ---------------------------
                var action = user.approvalStatusId == (short)ApprovalStatusEnum.Approved ? "Approved" : "Disapproved";
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = user.approvalStatusId == (short)ApprovalStatusEnum.Approved ? (short)AuditTypeEnum.LoanBookingApproved : (short)AuditTypeEnum.LoanBookingDisapproved,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"{action} Facility Booking with code ({loanReferenceNumber})",
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------

                var request = context.TBL_LOAN_BOOKING_REQUEST.Find(loanBookingRequestId);
                if (request != null)
                {
                    request.ISUSED = true;
                }
                    
            }
            return this.context.SaveChanges() > 0;
        }

        private void ProcessLoanBookingApproval(int loanId, TwoFactorAutheticationViewModel twoFactorAuthDetails, TBL_LOAN loanRecord, ApprovalViewModel user)
        {
            var systemDate = generalSetup.GetApplicationDate();
            var loanReferenceNumber = loanRecord.LOANREFERENCENUMBER;
            decimal totalBookedAmount = 0;

            if (user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
            {
                loanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                loanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
            }
            else
            {
                loanRecord.DATEAPPROVED = DateTime.Now;
                loanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                totalBookedAmount = (from a in context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == loanRecord.LOANAPPLICATIONDETAILID) select a).Sum(s => s.PRINCIPALAMOUNT);
                if (totalBookedAmount >= loanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                {
                    var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(loanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                    loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                }

                var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanRecord.LOANAPPLICATIONDETAILID);
                var loanProductInfo = context.TBL_PRODUCT.Find(loanRecord.PRODUCTID);

                if (user.operationId == (int)OperationsEnum.TermLoanBooking)
                {

                    if (loanProductInfo.PRODUCTCLASSID == (short)ProductClassEnum.InvoiceDiscountingFacility)
                    {
                        var casa = context.TBL_CASA.Find(loanRecord.CASAACCOUNTID2);

                        var lienModel = new CasaLienViewModel
                        {
                            productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                            sourceReferenceNumber = loanRecord.LOANREFERENCENUMBER,
                            userBranchId = (short)user.BranchId,
                            branchId = (short)user.BranchId,
                            companyId = user.companyId,
                            lienAmount = loanRecord.PRINCIPALAMOUNT + loanRecord.OUTSTANDINGINTEREST,
                            description = "Lien for IDF Booking",
                            lienTypeId = (short)LienTypeEnum.IDFBooking,
                            createdBy = user.createdBy,
                            userIPAddress = user.userIPAddress,
                            applicationUrl = user.applicationUrl,
                        };
                        casaLien.PlaceLien(lienModel, twoFactorAuthDetails);
                    }

                    //var firstPrincipalPaymentDaysInterval = (loanRecord.FIRSTPRINCIPALPAYMENTDATE.Value - loanRecord.EFFECTIVEDATE).Days;
                    //var firstInterestPayementDaysInterval = (loanRecord.FIRSTINTERESTPAYMENTDATE.Value - loanRecord.EFFECTIVEDATE).Days;

                    //if (loanRecord.EFFECTIVEDATE != systemDate)
                    //{
                    //    loanRecord.EFFECTIVEDATE = systemDate;
                    //    loanRecord.FIRSTPRINCIPALPAYMENTDATE = loanRecord.EFFECTIVEDATE.AddDays(firstPrincipalPaymentDaysInterval);
                    //    loanRecord.FIRSTINTERESTPAYMENTDATE = loanRecord.EFFECTIVEDATE.AddDays(firstInterestPayementDaysInterval);
                    //}
                }

                if (user.operationId == (int)OperationsEnum.CommercialLoanBooking)
                {
                    int interestDaysPeriod = getDaysInLoanPeriod(loanRecord.EFFECTIVEDATE, loanRecord.MATURITYDATE) - 1;
                    var totalInterest = getTotalInterest(loanRecord.PRINCIPALAMOUNT, loanRecord.INTERESTRATE, interestDaysPeriod);

                    loanRecord.OUTSTANDINGINTEREST = totalInterest;

                    if (loanProductInfo.DEALTYPEID == (short)DealTypeEnum.Upfront)
                    {
                        loanRecord.OUTSTANDINGPRINCIPAL = loanRecord.PRINCIPALAMOUNT - totalInterest;
                    }
                    else
                    {
                        loanRecord.OUTSTANDINGPRINCIPAL = loanRecord.PRINCIPALAMOUNT;
                    }
                }

                if (user.operationId == (int)OperationsEnum.ForeignExchangeLoanBooking)
                {

                    int interestDaysPeriod = getDaysInLoanPeriod(loanRecord.EFFECTIVEDATE, loanRecord.MATURITYDATE) - 1;
                    var totalInterest = getTotalInterest(loanRecord.PRINCIPALAMOUNT, loanRecord.INTERESTRATE, interestDaysPeriod);

                    loanRecord.OUTSTANDINGINTEREST = totalInterest;

                    if (loanProductInfo.DEALTYPEID == (short)DealTypeEnum.Upfront)
                    {
                        loanRecord.OUTSTANDINGPRINCIPAL = loanRecord.PRINCIPALAMOUNT - totalInterest;
                    }
                    else
                    {
                        loanRecord.OUTSTANDINGPRINCIPAL = loanRecord.PRINCIPALAMOUNT;
                    }
                }

                if (loanRecord.EFFECTIVEDATE < systemDate)
                {
                    ProcessBackDatedTeamLoansInterestAccrual(loanRecord.EFFECTIVEDATE, loanRecord.TERMLOANID);
                }

                loanRecord.LOANSTATUSID = (short)LoanStatusEnum.Active;
                loanRecord.DISBURSEDATE = generalSetup.GetApplicationDate();
                loanRecord.ISDISBURSED = true;
                loanRecord.DISBURSEDBY = user.createdBy;
                loanRecord.APPROVEDBY = user.createdBy;
                loanRecord.APPROVERCOMMENT = user.comment;

                /*UPDATING STAFF MIS */
                this.updateloanStaffMIS(loanRecord);

                context.SaveChanges();
                /* BUILD SCHEDULE MODEL & CALL GENERATE SCHEDULE METHOD */
                var loanScheduleModel = BuildScheduleModel(loanId, user.createdBy);

                if (user.operationId == (int)OperationsEnum.TermLoanBooking)
                    this.loanSchedule.AddLoanSchedule(loanId, loanScheduleModel, user.createdBy);

                /* BUILD DISBURSEMENT MODEL & CALL LOAN DISBURSEMENT METHOD */
                var loanDisbursementModel = BuildDisbursementModel(loanId, loanScheduleModel, user.createdBy);

                DisburseLoan(loanDisbursementModel, twoFactorAuthDetails);
            }
        }

        private void ProcessRevolvingLoanFacilityApproval(int loanId, TwoFactorAutheticationViewModel twoFactorAuthDetails, TBL_LOAN_REVOLVING revolvingLoanRecord, ApprovalViewModel user)
        {
            decimal totalBookedAmount = 0;
            var loanReferenceNumber = revolvingLoanRecord.LOANREFERENCENUMBER;
            if (user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
            {
                revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                revolvingLoanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
            }
            else
            {
                totalBookedAmount = (from a in context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == revolvingLoanRecord.LOANAPPLICATIONDETAILID) select a).Sum(s => s.OVERDRAFTLIMIT);
                if (totalBookedAmount >= revolvingLoanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                {
                    var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(revolvingLoanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                    loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                }

                if (USE_THIRD_PARTY_INTEGRATION)
                {
                    var reviewDate = revolvingLoanRecord.BOOKINGDATE.AddMonths(1);
                    var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
                    var acctType = "DR";
                    if (revolvingLoanRecord.REVOLVINGTYPEID == (short)LoanRevolvingTypeEnum.NormalOverdraft)
                    {
                        var model = new OverDraftNormalViewModel
                        {
                            accountNumber = revolvingLoanRecord.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            applicationDate = revolvingLoanRecord.EFFECTIVEDATE.ToString("dd-MMM-yyyy", null),
                            documentDate = revolvingLoanRecord.EFFECTIVEDATE.ToString("dd-MMM-yyyy", null),
                            expiryDate = revolvingLoanRecord.MATURITYDATE.ToString("dd-MMM-yyyy", null),
                            reviewedDate = reviewDate.ToString("dd-MMM-yyyy", null),
                            sanctionDate = revolvingLoanRecord.EFFECTIVEDATE.ToString("dd-MMM-yyyy", null),
                            sanctionLimit = String.Format("{0:0.00}", revolvingLoanRecord.OVERDRAFTLIMIT),
                            sanctionReferenceNumber = batchCode,//revolvingLoanRecord.LOANREFERENCENUMBER
                            interestRateAmount = String.Format("{0:0.00}", revolvingLoanRecord.INTERESTRATE),
                        };
                        InterestRateInquiryViewModel accountOutput = finacle.GetInterestRateInquiry(model.accountNumber, acctType);
                        if (accountOutput.interestRateAmount == model.interestRateAmount)
                        {
                            ResponseMessageViewModel res = finacle.OverDraftNormal(model, twoFactorAuthDetails);
                            revolvingLoanRecord.SERIALNUMBER = res.serialNumber;
                        }
                        else
                        {
                            var data = new InterestRateInquiryViewModel
                            {
                                interestRateAmount = String.Format("{0:0.00}", revolvingLoanRecord.INTERESTRATE),
                                interestTableCode = accountOutput.interestTableCode,
                                accountNumber = revolvingLoanRecord.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                endDate = revolvingLoanRecord.MATURITYDATE.ToString("dd-MMM-yyyy", null),
                                accountType = acctType,
                                startDate = revolvingLoanRecord.EFFECTIVEDATE.ToString("dd-MMM-yyyy", null),
                            };

                            ResponseMessageViewModel interestRateResult = finacle.OverDraftInterestRate(data, data.accountType, twoFactorAuthDetails);
                            if (interestRateResult.message == "interest Rate Modified sucessfully")
                            {
                                ResponseMessageViewModel res = finacle.OverDraftNormal(model, twoFactorAuthDetails);
                                revolvingLoanRecord.SERIALNUMBER = res.serialNumber;
                            }
                            else
                            {
                                throw new SecureException(interestRateResult.message);
                            }
                        }
                    }
                    if (revolvingLoanRecord.REVOLVINGTYPEID == (short)LoanRevolvingTypeEnum.NormalTemporaryOverdraft)
                    {
                        var model = new TemporaryOverDraftViewModel
                        {
                            AccountNumber = revolvingLoanRecord.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            TemporaryOverDraftFlag = "true",
                            TemporaryOverDraftAmount = String.Format("{0:0.00}", revolvingLoanRecord.OVERDRAFTLIMIT),
                            TemporaryOverDraftDate = reviewDate.ToString("dd-MMM-yyyy", null),
                            TemporaryOverDraftNaration = "Normal Temporary Overdraft",
                            TemporaryOverDraftInterestRate = String.Format("{0:0.00}", revolvingLoanRecord.INTERESTRATE),
                        };
                        ResponseMessageViewModel res = finacle.TemporaryOverDraftNormal(model, twoFactorAuthDetails);
                        revolvingLoanRecord.SERIALNUMBER = res.serialNumber;
                    }
                    if (revolvingLoanRecord.REVOLVINGTYPEID == (short)LoanRevolvingTypeEnum.SingleLimitTemporaryOverdraft)
                    {
                        var model = new TemporaryOverDraftViewModel
                        {
                            AccountNumber = revolvingLoanRecord.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            TemporaryOverDraftFlag = "true",
                            TemporaryOverDraftAmount = String.Format("{0:0.00}", revolvingLoanRecord.OVERDRAFTLIMIT),
                            TemporaryOverDraftDate = reviewDate.ToString("dd-MMM-yyyy", null),
                            TemporaryOverDraftNaration = "Single Limit Temporary Overdraft",
                            TemporaryOverDraftInterestRate = String.Format("{0:0.00}", revolvingLoanRecord.INTERESTRATE),
                        };
                        ResponseMessageViewModel res = finacle.TemporaryOverDraftSingle(model, twoFactorAuthDetails);
                        revolvingLoanRecord.SERIALNUMBER = res.serialNumber;
                    }
                }

                /*UPDATING STAFF MIS */
                //this.updateLoanRevolvingStaffMIS(revolvingLoanRecord);

                /* BUILD FEE MODEL & HANDLE FEE POSTING */
                var loanScheduleModel = BuildLoanFeeDisbursementModel(loanId, (short)LoanSystemTypeEnum.OverdraftFacility);
                var feePostings = BuildLoanChargeFeesPosting(loanScheduleModel);

                if (feePostings.Count() > 0)
                    financeTransaction.PostTransaction(feePostings, false, twoFactorAuthDetails);

                revolvingLoanRecord.DATEAPPROVED = DateTime.Now;
                revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                revolvingLoanRecord.LOANSTATUSID = (short)LoanStatusEnum.Active;
                revolvingLoanRecord.ISDISBURSED = true;
                revolvingLoanRecord.APPROVEDBY = user.createdBy;
                revolvingLoanRecord.APPROVERCOMMENT = user.comment;
            }
        }

        private void ProcessContingentLiabilityApproval(int loanId, TwoFactorAutheticationViewModel twoFactorAuthDetails, TBL_LOAN_CONTINGENT contingentLoanRecord, ApprovalViewModel user)
        {
            decimal totalBookedAmount = 0;
            var loanReferenceNumber = contingentLoanRecord.LOANREFERENCENUMBER;
            if (user.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
            {
                contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                contingentLoanRecord.LOANSTATUSID = (int)LoanStatusEnum.Cancelled;
            }
            else
            {
                totalBookedAmount = (from a in context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == contingentLoanRecord.LOANAPPLICATIONDETAILID) select a).Sum(s => s.CONTINGENTAMOUNT);
                if (totalBookedAmount >= contingentLoanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                {
                    var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(contingentLoanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                    loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                }

                contingentLoanRecord.DATEAPPROVED = DateTime.Now;
                contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                var loanProductInfo = context.TBL_PRODUCT.Find(contingentLoanRecord.PRODUCTID);
                var proBehaviour = loanProductInfo.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == loanProductInfo.PRODUCTID);
                if (loanProductInfo.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees && proBehaviour.Any() && proBehaviour.FirstOrDefault().ALLOWFUNDUSAGE == true)
                {   /* LIENABLE BOND AND GAURANTEE SPECIFIC TRANSACTION ENTRIES WHERE PRODUCT ALLOW FUND USAGE */
                    var casa = context.TBL_CASA.Where(x => x.PRODUCTID == loanProductInfo.PRODUCTID && x.CUSTOMERID == contingentLoanRecord.CUSTOMERID).FirstOrDefault();

                    var lienModel = new CasaLienViewModel
                    {
                        productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                        sourceReferenceNumber = contingentLoanRecord.LOANREFERENCENUMBER,
                        userBranchId = (short)user.BranchId,
                        branchId = (short)user.BranchId,
                        companyId = user.companyId,
                        lienAmount = contingentLoanRecord.CONTINGENTAMOUNT,
                        description = "Lien for APG Fund",
                        lienTypeId = (short)LienTypeEnum.APGBooking,
                        createdBy = user.createdBy,
                        userIPAddress = user.userIPAddress,
                        applicationUrl = user.applicationUrl,
                    };
                    casaLien.PlaceLien(lienModel, twoFactorAuthDetails);
                }
                else if (loanProductInfo.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees && proBehaviour.Any() && proBehaviour.FirstOrDefault().ALLOWFUNDUSAGE == false)
                {   /* DEBIT B&G CUSTOMER WHERE PRODUCT DOES NOT ALLOW FUND USAGE */
                    var casa = context.TBL_CASA.Where(x => x.PRODUCTID == loanProductInfo.PRODUCTID).FirstOrDefault();
                    var basicPostInputs = new BasicTrasactionSourceInputModel
                    {
                        sourceApplicationId = (short)contingentLoanRecord.CONTINGENTLOANID,
                        applicationUrl = user.applicationUrl,
                        description = "APG Transaction Booking",
                        companyId = user.companyId,
                        createdBy = user.createdBy,
                        userBranchId = (short)user.BranchId,
                        userIPAddress = user.userIPAddress
                    };

                    DebitAccount((int)loanProductInfo.PRINCIPALBALANCEGL, (int)loanProductInfo.PRINCIPALBALANCEGL2, casa, contingentLoanRecord.CONTINGENTAMOUNT, null, basicPostInputs);
                }

                var loanScheduleModel = BuildLoanFeeDisbursementModel(loanId, (short)LoanSystemTypeEnum.ContingentLiability);
                var feePostings = BuildLoanChargeFeesPosting(loanScheduleModel);
                if (feePostings.Count() > 0) { financeTransaction.PostTransaction(feePostings, false, twoFactorAuthDetails); }

                /*UPDATING STAFF MIS */
                //this.updateLoanContingentStaffMIS(contingentLoanRecord);

                contingentLoanRecord.LOANSTATUSID = (short)LoanStatusEnum.Active;
                contingentLoanRecord.ISDISBURSED = true;
                contingentLoanRecord.APPROVEDBY = user.createdBy;
                contingentLoanRecord.APPROVERCOMMENT = user.comment;
            }
            context.SaveChanges();
        }

        private void updateLoanRevolvingStaffMIS(TBL_LOAN_REVOLVING revolvingLoanRecord)
        {
            FinTrakBankingStagingContext staggingContext = new FinTrakBankingStagingContext();
            StaffMIS staffMIS = new StaffMIS(context, staggingContext);

            var relationshipOfficerStaffInfo = staffMIS.StaffInformationSystem(revolvingLoanRecord.RELATIONSHIPOFFICERID);
            revolvingLoanRecord.FIELD1 = relationshipOfficerStaffInfo.field1;
            revolvingLoanRecord.FIELD2 = relationshipOfficerStaffInfo.field2;
            revolvingLoanRecord.FIELD3 = relationshipOfficerStaffInfo.field3;
            revolvingLoanRecord.FIELD4 = relationshipOfficerStaffInfo.field4;
            revolvingLoanRecord.FIELD5 = relationshipOfficerStaffInfo.field5;
            revolvingLoanRecord.FIELD6 = relationshipOfficerStaffInfo.field6;
            revolvingLoanRecord.FIELD7 = relationshipOfficerStaffInfo.field7;
            revolvingLoanRecord.FIELD8 = relationshipOfficerStaffInfo.field8;
            revolvingLoanRecord.FIELD9 = relationshipOfficerStaffInfo.field9;
            revolvingLoanRecord.FIELD10 = relationshipOfficerStaffInfo.field10;
        }

        private void updateloanStaffMIS(TBL_LOAN loanRecord)
        {
            FinTrakBankingStagingContext staggingContext = new FinTrakBankingStagingContext();
            StaffMIS staffMIS = new StaffMIS(context, staggingContext);

            var relationshipOfficerStaffInfo = staffMIS.StaffInformationSystem(loanRecord.RELATIONSHIPOFFICERID);
            loanRecord.FIELD1 = relationshipOfficerStaffInfo.field1;
            loanRecord.FIELD2 = relationshipOfficerStaffInfo.field2;
            loanRecord.FIELD3 = relationshipOfficerStaffInfo.field3;
            loanRecord.FIELD4 = relationshipOfficerStaffInfo.field4;
            loanRecord.FIELD5 = relationshipOfficerStaffInfo.field5;
            loanRecord.FIELD6 = relationshipOfficerStaffInfo.field6;
            loanRecord.FIELD7 = relationshipOfficerStaffInfo.field7;
            loanRecord.FIELD8 = relationshipOfficerStaffInfo.field8;
            loanRecord.FIELD9 = relationshipOfficerStaffInfo.field9;
            loanRecord.FIELD10 = relationshipOfficerStaffInfo.field10;
        }

        private void updateLoanContingentStaffMIS(TBL_LOAN_CONTINGENT contingentLoanRecord)
        {
            FinTrakBankingStagingContext staggingContext = new FinTrakBankingStagingContext();
            StaffMIS staffMIS = new StaffMIS(context, staggingContext);

            var relationshipOfficerStaffInfo = staffMIS.StaffInformationSystem(contingentLoanRecord.RELATIONSHIPOFFICERID);
            contingentLoanRecord.FIELD1 = relationshipOfficerStaffInfo.field1;
            contingentLoanRecord.FIELD2 = relationshipOfficerStaffInfo.field2;
            contingentLoanRecord.FIELD3 = relationshipOfficerStaffInfo.field3;
            contingentLoanRecord.FIELD4 = relationshipOfficerStaffInfo.field4;
            contingentLoanRecord.FIELD5 = relationshipOfficerStaffInfo.field5;
            contingentLoanRecord.FIELD6 = relationshipOfficerStaffInfo.field6;
            contingentLoanRecord.FIELD7 = relationshipOfficerStaffInfo.field7;
            contingentLoanRecord.FIELD8 = relationshipOfficerStaffInfo.field8;
            contingentLoanRecord.FIELD9 = relationshipOfficerStaffInfo.field9;
            contingentLoanRecord.FIELD10 = relationshipOfficerStaffInfo.field10;
        }
        /// <summary>
        /// Builds the schedule model.
        /// </summary>
        /// <param name="targetId">The target identifier.</param>
        /// <param name="createdBy">The created by.</param>
        /// <returns></returns>
        /// 

        private IEnumerable<DailyInterestAccrualViewModel> ProcessAccrualTeamLoansInterestAccrual(DateTime applicationDate, int loanId)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);
            var schedule = context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.LOANID == loanId && x.DATE <= DbFunctions.TruncateTime(applicationDate));
            int count = schedule.Count();

            var data = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join c in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals c.LOANID
                        join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                        where a.DATE <= DbFunctions.TruncateTime(applicationDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active && a.LOANID == loanId

                        select new DailyInterestAccrualViewModel()
                        {
                            referenceNumber = b.LOANREFERENCENUMBER,
                            productId = b.PRODUCTID,
                            branchId = b.BRANCHID,
                            companyId = b.COMPANYID,
                            currencyId = b.CURRENCYID,
                            exchangeRate = b.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            date = applicationDate,
                            dailyAccuralAmount = (double)a.DAILYINTERESTAMOUNT,
                            mainAmount = c.PERIODINTERESTAMOUNT,
                            categoryId = (short)DailyAccrualCategory.TermLoan,
                            transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                            baseReferenceNumber = null,
                            dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                        }).ToList();

            List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();


            foreach (var item in data)
            {
                TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                dailyAccrual.PRODUCTID = item.productId;
                dailyAccrual.BRANCHID = item.branchId;
                dailyAccrual.EXCHANGERATE = item.exchangeRate;
                dailyAccrual.CURRENCYID = item.currencyId;
                dailyAccrual.INTERESTRATE = item.interestRate;
                dailyAccrual.DATE = item.date;
                dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                dailyAccrual.MAINAMOUNT = item.mainAmount;
                dailyAccrual.CATEGORYID = item.categoryId;
                dailyAccrual.COMPANYID = item.companyId;
                dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;


                transAccrual.Add(dailyAccrual);

            }
            this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);
            context.SaveChanges();

            //var model = (from a in context.TBL_DAILY_ACCRUAL
            //             where a.DATE == DbFunctions.TruncateTime(applicationDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
            //             group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID, a.EXCHANGERATE } into groupedQ
            //             select new DailyInterestAccrualViewModel()
            //             {
            //                 productId = groupedQ.Key.PRODUCTID,
            //                 branchId = groupedQ.Key.BRANCHID,
            //                 companyId = groupedQ.Key.COMPANYID,
            //                 currencyId = groupedQ.Key.CURRENCYID,
            //                 exchangeRate = groupedQ.Key.EXCHANGERATE,
            //                 dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
            //             }).ToList();

            //foreach (var item in model)
            //{
            //    financeTransaction.PostDailyLoansInterestAccrual(item);
            //}
            //context.SaveChanges();
            return data;
        }

        public LoanPaymentScheduleInputViewModel BuildScheduleModel(int targetId, int createdBy)
        {
            List<IrregularLoanScheduleInputViewModel> irregularPaymentScheduleList = new List<IrregularLoanScheduleInputViewModel>();
            var loanIrregularRecord = context.TBL_LOAN_SCHEDULE_IREGUL_INPUT.Where(x => x.LOANID == targetId);
            foreach (var irregularLoan in loanIrregularRecord)
            {
                var irregularViewData = new IrregularLoanScheduleInputViewModel
                {
                    paymentAmount = (double)irregularLoan.PAYMENTAMOUNT,
                    paymentDate = irregularLoan.PAYMENTDATE
                };
                irregularPaymentScheduleList.Add(irregularViewData);
            };

            var loanScheduleData = context.TBL_LOAN.Find(targetId);
            var loanFeeData = context.TBL_LOAN_FEE.Where(x => x.LOANID == targetId && x.ISINTEGRALFEE == true);
            double integraFeeAmount = 0;

            var effectiveDate = loanScheduleData.EFFECTIVEDATE;
            var maturityDate = effectiveDate.AddDays(loanScheduleData.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

            foreach (var record in loanFeeData)
            {
                integraFeeAmount = integraFeeAmount + (double)record.FEEAMOUNT;
            }

            var scheduleModel = new LoanPaymentScheduleInputViewModel();
            if (loanScheduleData.OPERATIONID != (short)OperationsEnum.CommercialLoanBooking && loanScheduleData.OPERATIONID != (short)OperationsEnum.ForeignExchangeLoanBooking)
            {
                if (loanScheduleData.SCHEDULETYPEID == (short)LoanScheduleTypeEnum.BulletPayment)
                {
                    loanScheduleData.PRINCIPALFREQUENCYTYPEID = null;
                    loanScheduleData.INTERESTFREQUENCYTYPEID = null;

                }

                scheduleModel = new LoanPaymentScheduleInputViewModel
                {
                    scheduleMethodId = loanScheduleData.SCHEDULETYPEID,

                    principalAmount = (double)loanScheduleData.PRINCIPALAMOUNT,
                    effectiveDate = effectiveDate,
                    interestRate = loanScheduleData.INTERESTRATE,
                    principalFrequency = loanScheduleData.PRINCIPALFREQUENCYTYPEID,
                    interestFrequency = loanScheduleData.INTERESTFREQUENCYTYPEID,
                    tenor = (loanScheduleData.MATURITYDATE - loanScheduleData.EFFECTIVEDATE).Days,
                    principalFirstpaymentDate = (DateTime)loanScheduleData.FIRSTPRINCIPALPAYMENTDATE,
                    interestFirstpaymentDate = (DateTime)loanScheduleData.FIRSTINTERESTPAYMENTDATE,
                    maturityDate = maturityDate,
                    accrualBasis = loanScheduleData.SCHEDULEDAYCOUNTCONVENTIONID,
                    integralFeeAmount = integraFeeAmount,
                    shouldDisburse = loanScheduleData.SHOULD_DISBURSE,
                    firstDayType = loanScheduleData.SCHEDULEDAYINTERESTTYPEID,
                    irregularPaymentSchedule = irregularPaymentScheduleList,

                };
            }


            return scheduleModel;
        }

        private LoanPaymentScheduleInputViewModel BuildBaloonScheduleModel(int targetId, int createdBy)
        {
            var loanScheduleData = context.TBL_LOAN.Find(targetId);
            var loanFeeData = context.TBL_LOAN_FEE.Where(x => x.LOANID == targetId && x.ISINTEGRALFEE == true);
            double integraFeeAmount = 0;

            var effectiveDate = loanScheduleData.EFFECTIVEDATE;
            var maturityDate = effectiveDate.AddDays(loanScheduleData.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

            foreach (var record in loanFeeData)
            {
                integraFeeAmount = integraFeeAmount + (double)record.FEEAMOUNT;
            }
            if (loanScheduleData.SCHEDULETYPEID == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanScheduleData.PRINCIPALFREQUENCYTYPEID = null;
                loanScheduleData.INTERESTFREQUENCYTYPEID = null;

            }
            var scheduleModel = new LoanPaymentScheduleInputViewModel
            {
                scheduleMethodId = (short)LoanScheduleTypeEnum.BallonPayment,

                principalAmount = (double)loanScheduleData.PRINCIPALAMOUNT,
                effectiveDate = effectiveDate,
                interestRate = loanScheduleData.INTERESTRATE,
                principalFrequency = loanScheduleData.PRINCIPALFREQUENCYTYPEID,
                interestFrequency = loanScheduleData.INTERESTFREQUENCYTYPEID,
                tenor = (loanScheduleData.MATURITYDATE - loanScheduleData.EFFECTIVEDATE).Days,
                principalFirstpaymentDate = (DateTime)loanScheduleData.FIRSTPRINCIPALPAYMENTDATE,
                interestFirstpaymentDate = (DateTime)loanScheduleData.FIRSTINTERESTPAYMENTDATE,
                maturityDate = maturityDate,
                accrualBasis = (short)DayCountConventionEnum.Actual_360,
                integralFeeAmount = integraFeeAmount,
                shouldDisburse = loanScheduleData.SHOULD_DISBURSE,
                firstDayType = loanScheduleData.SCHEDULEDAYINTERESTTYPEID,
            };

            return scheduleModel;
        }

        private LoanViewModel BuildLoanFeeDisbursementModel(int loanId, short loanSystemTypeId)
        {
            //List<TBL_LOAN_FEE> feeRecords = new List<TBL_LOAN_FEE>();
            var feeRecords = context.TBL_LOAN_FEE.Where(x => x.LOANID == loanId && x.LOANSYSTEMTYPEID == loanSystemTypeId && x.ISPOSTED == false).ToList();
            LoanViewModel loanModel;

            List<LoanChargeFeeViewModel> loanChargeFeeList = new List<LoanChargeFeeViewModel>();
            foreach (var fee in feeRecords)
            {
                var loanfee = new LoanChargeFeeViewModel
                {
                    loanChargeFeeId = fee.LOANCHARGEFEEID,
                    chargeFeeId = fee.CHARGEFEEID,
                    loanId = fee.LOANID,
                    loanSystemTypeId = fee.LOANSYSTEMTYPEID,
                    feeRateValue = fee.FEERATEVALUE,
                    feeDependentAmount = fee.FEERATEVALUE,
                    feeAmount = fee.FEEAMOUNT,
                    isIntegralFee = fee.ISINTEGRALFEE,
                    recurring = fee.ISRECURRING,
                };
                loanChargeFeeList.Add(loanfee);
            };
            loanModel = new LoanViewModel
            {
                loanChargeFee = loanChargeFeeList,
            };

            return loanModel;
        }
        /// <summary>
        /// Builds the disbursement model.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="loanInputModel">The loan input model.</param>
        /// <param name="staffId">The staff identifier.</param>
        /// <returns></returns>
        public LoanViewModel BuildDisbursementModel(int loanId, LoanPaymentScheduleInputViewModel loanInputModel, int staffId)
        {
            var covenantRecord = context.TBL_LOAN_COVENANT_DETAIL.Where(x => x.LOANID == loanId).ToList();
            List<LoanCovenantDetailViewModel> loanCovenantList = new List<LoanCovenantDetailViewModel>();

            foreach (var covenant in covenantRecord)
            {
                var loanCovenant = new LoanCovenantDetailViewModel
                {
                    loanCovenantDetailId = covenant.LOANCOVENANTDETAILID,
                    covenantDetail = covenant.COVENANTDETAIL,
                    loanId = covenant.LOANID,
                    covenantTypeId = covenant.COVENANTTYPEID,
                    frequencyTypeId = covenant.FREQUENCYTYPEID,
                    covenantAmount = covenant.COVENANTAMOUNT ?? 0,
                    covenantDate = covenant.COVENANTDATE,

                };
                loanCovenantList.Add(loanCovenant);
            };

            var feeRecord = context.TBL_LOAN_FEE.Where(x => x.LOANID == loanId && x.ISPOSTED == false).ToList();
            List<LoanChargeFeeViewModel> loanChargeFeeList = new List<LoanChargeFeeViewModel>();
            foreach (var fee in feeRecord)
            {
                var loanfee = new LoanChargeFeeViewModel
                {
                    loanChargeFeeId = fee.LOANCHARGEFEEID,
                    chargeFeeId = fee.CHARGEFEEID,
                    loanId = fee.LOANID,
                    loanSystemTypeId = fee.LOANSYSTEMTYPEID,
                    feeRateValue = fee.FEERATEVALUE,
                    feeDependentAmount = fee.FEERATEVALUE,
                    feeAmount = fee.FEEAMOUNT,
                    isIntegralFee = fee.ISINTEGRALFEE,
                    recurring = fee.ISRECURRING,
                };
                loanChargeFeeList.Add(loanfee);
            };

            var loanRecord = context.TBL_LOAN.Find(loanId);
            if (loanRecord.SCHEDULETYPEID == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanRecord.PRINCIPALFREQUENCYTYPEID = null;
                loanRecord.INTERESTFREQUENCYTYPEID = null;
            }

            LoanViewModel loanModel;
            if (loanRecord.OPERATIONID == (short)OperationsEnum.TermLoanBooking)
            {
                loanModel = new LoanViewModel
                {
                    loanId = loanRecord.TERMLOANID,
                    customerId = loanRecord.CUSTOMERID,
                    productId = loanRecord.PRODUCTID,
                    casaAccountId = loanRecord.CASAACCOUNTID,
                    loanApplicationDetailId = (int)loanRecord.LOANAPPLICATIONDETAILID,
                    branchId = loanRecord.BRANCHID,
                    loanReferenceNumber = loanRecord.LOANREFERENCENUMBER,
                    applicationReferenceNumber = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    principalFrequencyTypeId = loanRecord.PRINCIPALFREQUENCYTYPEID,
                    interestFrequencyTypeId = loanRecord.INTERESTFREQUENCYTYPEID,
                    principalNumberOfInstallment = loanRecord.PRINCIPALNUMBEROFINSTALLMENT,
                    interestNumberOfInstallment = loanRecord.INTERESTNUMBEROFINSTALLMENT,
                    interestRate = loanRecord.INTERESTRATE,
                    effectiveDate = generalSetup.GetApplicationDate(),
                    maturityDate = generalSetup.GetApplicationDate().AddDays(loanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR),
                    principalAmount = loanRecord.PRINCIPALAMOUNT,
                    createdBy = staffId,
                    loanStatusId = loanRecord.LOANSTATUSID,
                    scheduleTypeId = loanRecord.SCHEDULETYPEID,
                    operationId = (int)OperationsEnum.TermLoanBooking,
                    customerGroupId = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                    loanTypeId = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                    firstPrincipalPaymentDate = loanRecord.FIRSTPRINCIPALPAYMENTDATE,
                    firstInterestPaymentDate = loanRecord.FIRSTINTERESTPAYMENTDATE,
                    companyId = loanRecord.COMPANYID,
                    currencyId = loanRecord.CURRENCYID,
                    accurialBasis = loanRecord.SCHEDULEDAYCOUNTCONVENTIONID,
                    integralFeeAmount = (decimal)loanInputModel.integralFeeAmount,
                    firstDayType = loanRecord.SCHEDULEDAYINTERESTTYPEID,
                    loanChargeFee = loanChargeFeeList,
                };
            }
            else
            {
                var operationId = 0;
                if (loanRecord.OPERATIONID != (short)OperationsEnum.CommercialLoanBooking)
                    operationId = (short)OperationsEnum.CommercialLoanBooking;
                if (loanRecord.OPERATIONID != (short)OperationsEnum.ForeignExchangeLoanBooking)
                    operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;

                loanModel = new LoanViewModel
                {
                    loanId = loanRecord.TERMLOANID,
                    customerId = loanRecord.CUSTOMERID,
                    productId = loanRecord.PRODUCTID,
                    casaAccountId = loanRecord.CASAACCOUNTID,
                    loanApplicationDetailId = (int)loanRecord.LOANAPPLICATIONDETAILID,
                    branchId = loanRecord.BRANCHID,
                    loanReferenceNumber = loanRecord.LOANREFERENCENUMBER,
                    applicationReferenceNumber = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    interestRate = loanRecord.INTERESTRATE,
                    effectiveDate = loanRecord.EFFECTIVEDATE,
                    maturityDate = loanRecord.MATURITYDATE,
                    principalAmount = loanRecord.PRINCIPALAMOUNT,
                    createdBy = staffId,
                    loanStatusId = loanRecord.LOANSTATUSID,
                    operationId = operationId,
                    customerGroupId = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                    loanTypeId = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                    companyId = loanRecord.COMPANYID,
                    currencyId = loanRecord.CURRENCYID,
                    integralFeeAmount = (decimal)loanInputModel.integralFeeAmount,
                    loanChargeFee = loanChargeFeeList,
                };
            }

            return loanModel;
        }

        /// <summary>
        /// Builds the loan disbursment posting.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public List<FinanceTransactionViewModel> BuildLoanDisbursmentPosting(LoanViewModel model)
        {
            List<FinanceTransactionViewModel> loanTransaction = new List<FinanceTransactionViewModel>();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);
            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId && x.COMPANYID == model.companyId);
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var description = string.Empty;
            var loan = context.TBL_LOAN.Find(model.loanId);
            string rateCode = "";
            TBL_CUSTOM_CHART_OF_ACCOUNT customNostro = new TBL_CUSTOM_CHART_OF_ACCOUNT();


            //loanTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
            //loanTransaction.description = "Loan Disbursment Amount";
            //loanTransaction.valueDate = generalSetup.GetApplicationDate();
            //loanTransaction.transactionDate = loanTransaction.valueDate;
            //loanTransaction.currencyId = casa.CURRENCYID;
            //loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.valueDate,loanTransaction.currencyId, model.companyId).sellingRate;
            //loanTransaction.isApproved = true;
            //loanTransaction.postedBy = model.createdBy;
            //loanTransaction.approvedBy = model.createdBy;
            //loanTransaction.approvedDate = loanTransaction.transactionDate;
            //loanTransaction.approvedDateTime = DateTime.Now;
            //loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //loanTransaction.companyId = model.companyId;

            //FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();


            debit.operationId = (int)model.operationId; 
            debit.description = $"Loan disbursement";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;

            if (model.productTypeId != (int)LoanProductTypeEnum.ForeignXRevolving)
            {
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            }
            else
            {
                debit.currencyId = loan.CURRENCYID;
                debit.currencyRate = (double) loan.NOSTRORATEAMOUNT; //financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;

                rateCode = this.context.TBL_CURRENCY_RATECODE.FirstOrDefault(x => x.RATECODEID == loan.NOSTRORATECODEID).RATECODE;
                customNostro = context.TBL_CUSTOM_CHART_OF_ACCOUNT.Where(x => x.ACCOUNTID == loan.NOSTROACCOUNTID && x.ISNOSTROACCOUNT == true).FirstOrDefault();

                debit.rateCode = rateCode;
                debit.rateUnit = String.Format("{0:0.00}", loan.NOSTRORATEAMOUNT); //loan.NOSTRORATEAMOUNT;
                debit.currencyCrossCode = customNostro.CURRENCYCODE;
            }

            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            if (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == product.PRODUCTID).PRINCIPALBALANCEGL == null)
                throw new BadLogicException("No GL is currently mapped to this product code '{product.PRODUCTCODE}'.");

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == product.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.batchCode = batchCode;
            debit.casaAccountId = null;
            debit.debitAmount = model.principalAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;

            List<FinanceTransactionViewModel> credits = new List<FinanceTransactionViewModel>();

            if (model.productTypeId != (int)LoanProductTypeEnum.ForeignXRevolving)
            {
                FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                credit.operationId = (int)model.operationId;
                credit.description = "Loan disbursement";
                credit.valueDate = debit.valueDate;
                credit.transactionDate = debit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = debit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;

                var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                credit.glAccountId = repaymentAccountGL;
                credit.sourceReferenceNumber = model.loanReferenceNumber;
                credit.batchCode = batchCode;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = 0;
                credit.creditAmount = model.principalAmount;
                credit.sourceBranchId = model.branchId;
                credit.destinationBranchId = model.branchId;

                credits.Add(credit);
            }
            else
            {
                var loanDisbursement = context.TBL_LOAN_DISBURSEMENT.Where(x => x.TERMLOANID == model.loanId);
                
                var nostroGL = context.TBL_CHART_OF_ACCOUNT.Where(x => x.ACCOUNTCODE == customNostro.ACCOUNTID).FirstOrDefault();

                foreach (var item in loanDisbursement)
                {
                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    credit.operationId = (int)model.operationId;
                    credit.description = $"{item.NARRATION} ";
                    credit.valueDate = debit.valueDate;
                    credit.transactionDate = debit.valueDate;
                    credit.currencyId = (short)loan.NOSTROCURRENCYID; //.CURRENCYID;
                    credit.currencyRate = (double) loan.NOSTRORATEAMOUNT; // financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                    credit.isApproved = true;
                    credit.postedBy = model.createdBy;
                    credit.approvedBy = model.createdBy;
                    credit.approvedDate = debit.transactionDate;
                    credit.approvedDateTime = DateTime.Now;
                    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    credit.companyId = model.companyId;

                   // var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                    credit.glAccountId = nostroGL.GLACCOUNTID;
                    credit.sourceReferenceNumber = model.loanReferenceNumber;
                    credit.batchCode = batchCode;
                    credit.casaAccountId = null; // casa.CASAACCOUNTID;
                    credit.debitAmount = 0;
                    credit.creditAmount = item.AMOUNTDISBURSED; // model.principalAmount;
                    credit.sourceBranchId = model.branchId;
                    credit.destinationBranchId = model.branchId;

                    credit.rateCode = rateCode;
                    credit.rateUnit = String.Format("{0:0.00}", loan.NOSTRORATEAMOUNT); //loan.NOSTRORATEAMOUNT;
                    credit.currencyCrossCode = customNostro.CURRENCYCODE;

                    credits.Add(credit);
                };
            }

            loanTransaction.Add(debit);
            loanTransaction.AddRange(credits);
            //loanTransaction.Add(credit);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return loanTransaction;

        }

        /// <summary>
        /// Builds the loan charge fees posting.
        /// </summary>
        /// <param name="loanDetails">The loan details.</param>
        /// <returns></returns>
        public List<FinanceTransactionViewModel> BuildLoanChargeFeesPosting(LoanViewModel loanDetails)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            foreach (var item in loanDetails.loanChargeFee)
            {
                if (item.isPosted == false && item.feeAmount != 0)
                {
                    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanDetails.casaAccountId);

                    var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == item.chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

                    foreach (var post in postingGroups)
                    {
                        var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == item.chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                        {
                            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                            decimal debitAmount = 0;
                            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                debitAmount = (decimal)item.feeAmount * (decimal)(debits.VALUE / 100.0);
                            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                debitAmount = (decimal)debits.VALUE;

                            debit.operationId = (int)loanDetails.operationId;
                            debit.description = $"Fee charge on {debits.DESCRIPTION}";
                            debit.valueDate = generalSetup.GetApplicationDate();
                            debit.transactionDate = debit.valueDate;
                            debit.currencyId = casa.CURRENCYID;
                            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, loanDetails.companyId).sellingRate;
                            debit.isApproved = true;
                            debit.postedBy = loanDetails.createdBy;
                            debit.approvedBy = loanDetails.createdBy;
                            debit.approvedDate = debit.transactionDate;
                            debit.approvedDateTime = DateTime.Now;
                            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            debit.companyId = loanDetails.companyId;


                            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                            debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                            debit.batchCode = batchCode;
                            debit.casaAccountId = casa.CASAACCOUNTID;
                            debit.debitAmount = debitAmount;
                            debit.creditAmount = 0;
                            debit.sourceBranchId = loanDetails.branchId;
                            debit.destinationBranchId = casa.BRANCHID;
                            debit.rateCode = loanDetails.nostroRateCode; //"TTB";
                            debit.rateUnit = string.Empty;
                            debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(debit);
                        }

                        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                        {
                            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                            decimal creditAmount = 0;
                            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                                creditAmount = (decimal)item.feeAmount * (decimal)(credits.VALUE / 100.0);
                            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                                creditAmount = (decimal)credits.VALUE;


                            credit.operationId = (int)loanDetails.operationId;
                            credit.description = $"Fee charge on {credits.DESCRIPTION}";
                            credit.valueDate = generalSetup.GetApplicationDate();
                            credit.transactionDate = credit.valueDate;
                            credit.currencyId = (short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, loanDetails.companyId); //casa.CURRENCYID;
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, loanDetails.companyId).sellingRate;
                            credit.isApproved = true;
                            credit.postedBy = loanDetails.createdBy;
                            credit.approvedBy = loanDetails.createdBy;
                            credit.approvedDate = credit.transactionDate;
                            credit.approvedDateTime = DateTime.Now;
                            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                            credit.companyId = loanDetails.companyId;
                            credit.glAccountId = (int)credits.GLACCOUNTID1;
                            credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                            credit.batchCode = batchCode;
                            credit.casaAccountId = null;
                            credit.debitAmount = 0;
                            credit.creditAmount = creditAmount;
                            credit.sourceBranchId = loanDetails.branchId;
                            credit.destinationBranchId = loanDetails.branchId;
                            credit.rateCode = loanDetails.nostroRateCode; // "TTB";
                            credit.rateUnit = string.Empty;
                            credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                            inputTransactions.Add(credit);
                        }
                    }
                }

            }
            return inputTransactions;
        }

        /// <summary>
        /// Adds the loan monitoring trigger.
        /// </summary>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private bool AddLoanMonitoringTrigger(List<LoanMonitoringTriggerViewModel> monitoringTriggersModel, int loanId, short loanSystemTypeId)
        {
            foreach (LoanMonitoringTriggerViewModel entity in monitoringTriggersModel)
            {
                if (entity.monitoringTrigger != null && entity.monitoringTrigger != string.Empty)
                {
                    var data = new TBL_LOAN_MONITORING_TRIGGER
                    {
                        MONITORING_TRIGGER = entity.monitoringTrigger,
                        MONITORING_TRIGGERID = entity.monitoringTriggerId,
                        LOANID = loanId,
                        LOANSYSTEMTYPEID = loanSystemTypeId,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false

                    };
                    context.TBL_LOAN_MONITORING_TRIGGER.Add(data);
                }
            }
            //var result = context.SaveChanges() > 0;
            return true;
        }

        /// <summary>
        /// Adds the loan covenant.
        /// </summary>
        /// <param name="covenantModel">The covenant model.</param>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private bool AddLoanCovenant(LoanViewModel model, int loanId, short loanSystemTypeId)
        {
            var covenantModel = context.TBL_LOAN_APPLICATION_COVENANT.Where(x => x.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).ToList();
            foreach (var entity in covenantModel)
            {
                var covenant = new TBL_LOAN_COVENANT_DETAIL
                {
                    COMPANYID = model.companyId,
                    COVENANTAMOUNT = entity.COVENANTAMOUNT,
                    COVENANTDATE = entity.COVENANTDATE,
                    COVENANTDETAIL = entity.COVENANTDETAIL,
                    COVENANTTYPEID = entity.COVENANTTYPEID,
                    CREATEDBY = entity.CREATEDBY,
                    DATETIMECREATED = DateTime.Now,
                    FREQUENCYTYPEID = entity.FREQUENCYTYPEID,
                    LOANID = loanId,
                    CASAACCOUNTID = entity.CASAACCOUNTID,
                    LOANSYSTEMTYPEID = loanSystemTypeId,
                };

                context.TBL_LOAN_COVENANT_DETAIL.Add(covenant);
            }
            var result = context.SaveChanges() > 0;
            return result;

        }

        /// <summary>
        /// Adds the loan guarantor.
        /// </summary>
        /// <param name="guarantorModel">The guarantor model.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <returns></returns>
        public bool AddLoanGuarantor(LoanGuarantorViewModel entity, short productTypeId, int loanApplicationId)
        {
            if (entity.customerType == "Corporate")
            {
                entity.customerTypeId = (short)CustomerTypeEnum.Corporate;
            }
            else entity.customerTypeId = (short)CustomerTypeEnum.Individual;

            var guarantor = new TBL_COLLATERAL_GAURANTEE
            {
                FIRSTNAME = entity.firstname != null ? entity.firstname : " ",
                LASTNAME = entity.lastname != null ? entity.lastname : " ",
                MIDDLENAME = entity.middlename != null ? entity.middlename : " ",
                PHONENUMBER1 = entity.phoneNumber1,
                PHONENUMBER2 = entity.phoneNumber2,
                RELATIONSHIP = entity.relationship,
                BVN = entity.bvn,
                EMAILADDRESS = entity.emailAddress,

            };
            //context.TBL_LOAN_GUARANTOR.Add(guarantor);
            //return context.SaveChanges() > 0;

            return true;
        }

        /// <summary>
        /// Adds the loan covenant.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private ICollection<TBL_LOAN_COVENANT_DETAIL> AddLoanCovenant(LoanCovenantDetailViewModel entity, short loanSystemTypeId)
        {
            ICollection<TBL_LOAN_COVENANT_DETAIL> covenant;

            covenant = new List<TBL_LOAN_COVENANT_DETAIL>();

            covenant.Add(new TBL_LOAN_COVENANT_DETAIL
            {
                COMPANYID = entity.companyId,
                COVENANTAMOUNT = entity.covenantAmount,
                COVENANTDETAIL = entity.covenantDetail,
                COVENANTDATE = entity.covenantDate,
                LOANID = entity.loanId,
                LOANSYSTEMTYPEID = loanSystemTypeId,
                COVENANTTYPEID = entity.covenantTypeId,
                FREQUENCYTYPEID = entity.frequencyTypeId,
                CASAACCOUNTID = entity.casaAccountId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now // generalSetup.GetApplicationDate(),
            });

            return covenant;
        }


        public bool AddLoanCollateralMapping( int loanApplicationId, int loanId, short loanSystemTypeId)
        {
            var collateralModel = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
            foreach (var entity in collateralModel)
            {
                var collateral = new TBL_LOAN_COLLATERAL_MAPPING
                {  
                    COLLATERALCUSTOMERID = entity.COLLATERALCUSTOMERID,
                    LOANID = loanId,
                    LOANSYSTEMTYPEID = loanSystemTypeId,
                    ISRELEASED = false,
                };
                context.TBL_LOAN_COLLATERAL_MAPPING.Add(collateral);
            }
            return true;
        }

        private void AddLoanFees(List<LoanChargeFeeViewModel> feeModel, int staffId, int loanId, short loanSystemTypeId, int companyId, bool feeOverride)
        {
            foreach (var ent in feeModel)
            {
                var fee = new TBL_LOAN_FEE
                {
                    CHARGEFEEID = ent.chargeFeeId,
                    FEEAMOUNT = ent.feeAmount,
                    FEEDEPENDENTAMOUNT = ent.feeDependentAmount,
                    FEERATEVALUE = ent.feeRateValue,
                    ISINTEGRALFEE = ent.isIntegralFee,
                    LOANID = loanId,
                    LOANSYSTEMTYPEID = loanSystemTypeId,
                    ISRECURRING = ent.recurring,
                    RECURRINGPAYMENTDAY = 28,
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now.Date,
                    ISPOSTED = ent.isPosted
                };
                if (feeOverride)
                {
                    fee.ISPOSTED = false;
                }
                else fee.ISPOSTED = true;

                context.TBL_LOAN_FEE.Add(fee);
            }
            //return context.SaveChanges() > 0;
        }

        public List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTrigger()
        {
            var data = (from c in context.TBL_LOAN_MONITORING_TRIG_SETUP
                        select new LoanMonitoringTriggerViewModel
                        {
                            monitoringTriggerId = c.MONITORING_TRIGGERID,
                            monitoringTriggerSetupName = c.MONITORING_TRIGGER_NAME
                        }).ToList();
            return data;
        }

        public List<LoanMonitoringTriggerViewModel> GetLoanMonitoringTriggerByLoanApplicationDetailId(int loanApplicationDetailId)
        {
            var loanMonitoringTrigger = (from tr in context.TBL_LOAN_APPLICATN_DETL_MTRIG.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId)
                                         select (
                                                  new LoanMonitoringTriggerViewModel
                                                  {
                                                      loanMonitoringTriggerId = tr.LOAN_MONITORING_TRIGGERID,
                                                      monitoringTrigger = tr.MONITORING_TRIGGER,
                                                      monitoringTriggerId = tr.MONITORING_TRIGGERID,
                                                  })).ToList();
            return loanMonitoringTrigger;
        }

        /// <summary>
        /// Gets all loans.
        /// </summary>
        /// <returns></returns>
        private IQueryable<LoanViewModel> GetAllLoans()
        {
            var data = (from l in context.TBL_LOAN
                        join cust in context.TBL_CUSTOMER on l.CUSTOMERID equals cust.CUSTOMERID
                        join loanDetail in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals loanDetail.LOANAPPLICATIONDETAILID
                        join loanApp in context.TBL_LOAN_APPLICATION on loanDetail.LOANAPPLICATIONID equals loanApp.LOANAPPLICATIONID
                        select new LoanViewModel
                        {
                            loanId = l.TERMLOANID,
                            customerId = l.CUSTOMERID,
                            customerName = cust.FIRSTNAME + " " + cust.LASTNAME,
                            productId = l.PRODUCTID,

                            companyId = l.COMPANYID,
                            casaAccountId = l.CASAACCOUNTID,
                            branchId = l.BRANCHID,
                            branchName = l.TBL_BRANCH.BRANCHNAME,
                            loanReferenceNumber = l.LOANREFERENCENUMBER,

                            principalFrequencyTypeId = (short)l.PRINCIPALFREQUENCYTYPEID,
                            pricipalFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            interestFrequencyTypeId = (short)l.INTERESTFREQUENCYTYPEID,
                            interestFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            principalNumberOfInstallment = l.PRINCIPALNUMBEROFINSTALLMENT,
                            interestNumberOfInstallment = l.INTERESTNUMBEROFINSTALLMENT,

                            relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                            //  relationshipOfficerName = l.TBL_STAFF.FIRSTNAME + " " + l.TBL_STAFF.MIDDLENAME + " " + l.TBL_STAFF.LASTNAME,
                            relationshipManagerId = l.RELATIONSHIPMANAGERID,
                            // relationshipManagerName = l.TBL_STAFF1.FIRSTNAME + " " + l.TBL_STAFF1.MIDDLENAME + " " + l.TBL_STAFF1.LASTNAME,
                            misCode = l.MISCODE,
                            teamMiscode = l.TEAMMISCODE,
                            interestRate = l.INTERESTRATE,
                            effectiveDate = l.EFFECTIVEDATE,
                            maturityDate = l.MATURITYDATE,
                            bookingDate = l.BOOKINGDATE,
                            principalAmount = l.PRINCIPALAMOUNT,
                            principalInstallmentLeft = l.PRINCIPALINSTALLMENTLEFT,
                            interestInstallmentLeft = l.INTERESTINSTALLMENTLEFT,
                            approvalStatusId = l.APPROVALSTATUSID,
                            approvedBy = l.APPROVEDBY,
                            approverComment = l.APPROVERCOMMENT,
                            dateApproved = l.DATEAPPROVED,
                            loanStatusId = l.LOANSTATUSID,
                            scheduleTypeId = l.SCHEDULETYPEID,
                            isDisbursed = l.ISDISBURSED,
                            disbursedBy = l.DISBURSEDBY,
                            disburserComment = l.DISBURSERCOMMENT,
                            disburseDate = l.DISBURSEDATE,

                            approvedAmount = loanDetail.APPROVEDAMOUNT,

                            //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                            operationId = l.OPERATIONID,
                            // operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == l.OPERATIONID).OPERATIONNAME,
                            casaAccountNumber = l.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            // productAccountName = l.TBL_PRODUCT.PRODUCTNAME,
                            subSectorName = l.TBL_SUB_SECTOR.NAME,
                            sectorName = l.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            customerGroupId = loanApp.CUSTOMERGROUPID,
                            loanTypeId = loanApp.LOANAPPLICATIONTYPEID,
                            // loanTypeName = loanApp.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            equityContribution = l.EQUITYCONTRIBUTION,
                            firstPrincipalPaymentDate = l.FIRSTPRINCIPALPAYMENTDATE ?? DateTime.Now,
                            firstInterestPaymentDate = l.FIRSTINTERESTPAYMENTDATE ?? DateTime.Now,
                            outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                            principalAdditionCount = l.PRINCIPALADDITIONCOUNT ?? 0,
                            principalReductionCount = l.PRINCIPALREDUCTIONCOUNT ?? 0,
                            fixedPrincipal = l.FIXEDPRINCIPAL,
                            profileLoan = l.PROFILELOAN,
                            dischargeLetter = l.DISCHARGELETTER,
                            suspendInterest = l.SUSPENDINTEREST,
                            //customerSensitivityLevelId = l.CUSTOMERSENSITIVITYLEVELID,
                            createdBy = l.CREATEDBY,
                            dateTimeCreated = l.DATETIMECREATED,
                            // isCamsol = context.TBL_LOAN_CAMSOL.Any(x => x.LOANID == l.TERMLOANID),
                            productName = l.TBL_PRODUCT.PRODUCTNAME
                        });
            return data;
        }

        //public bool ValidateCamsol(int loanId)
        //{
        //    var check = context.tbl_Loan_Camsol.Where(x => x.LoanId == loanId);

        //    if (check.Any())
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        /// <summary>
        /// Calculates the tenor value.
        /// </summary>
        /// <param name="maturityDate">The maturity date.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <returns></returns>
        public int CalculateTenorValue(DateTime maturityDate, DateTime effectiveDate)
        {
            var result = (maturityDate - effectiveDate).Days;

            return result;
        }

        //public bool ValidateCamsol(int loanId)
        //{
        //    var check = context.tbl_Loan_Camsol.Where(x => x.LoanId == loanId);

        //    if (check.Any())
        //    {
        //        return true;
        //    }

        //    return false;
        //}


        /// <summary>
        /// Gets the loan by customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId)
        {
            var data = GetAllLoans().Where(l => l.customerId == customerId && l.loanStatusId == (short)LoanStatusEnum.Active).GroupBy(x => x.loanId).Select(g => g.FirstOrDefault());
            return data;

        }

        public List<LoanViewModel> GetLoanApplicationExistingLoans(int applicationId)
        {
            var customerIds = context.TBL_LOAN_APPLICATION_DETAIL
                                .Where(x => x.LOANAPPLICATIONID == applicationId && x.DELETED == false)
                                .Select(x => x.CUSTOMERID)
                                .ToList();

            var data = GetAllLoans().Where(l => customerIds.Contains(l.customerId) && l.loanStatusId == (short)LoanStatusEnum.Active).GroupBy(x => x.loanId).Select(g => g.FirstOrDefault());
            return data.ToList();
        }

        /// <summary>
        /// Gets the loan by customer group.
        /// </summary>
        /// <param name="customerGroupId">The customer group identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId)
        {
            var data = GetAllLoans().Where(l => l.customerGroupId == customerGroupId).GroupBy(x => x.loanId)
                .Select(g => g.FirstOrDefault());
            return data;
        }

        /// <summary>
        /// Runnings the loans.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId)
        {
            var loans = GetLoansByCompanyId(companyId)
                .Where(c => c.approvalStatusId == (int)ApprovalStatusEnum.Approved && c.customerId == customerId)
                .Select(c => new LoanRepaymentScheduleViewModel
                {
                    loanReferenceNumber = c.loanReferenceNumber,
                    loanApplicationId = c.loanApplicationId,
                    principalRepayment = c.outstandingPrincipal,
                    interestAccrual = c.outstandingInterest,
                    customerId = c.customerId,
                    principalAmount = c.principalAmount,
                    effectiveDate = c.effectiveDate,
                    maturityDate = c.maturityDate,
                    interestRate = c.interestRate,
                    loanId = c.loanId,
                    productName = c.productName,
                    productTypeId = c.productTypeId,
                    terminationDate = c.maturityDate
                })
                .AsQueryable();

            return loans;
        }

        public IEnumerable<LoanViewModel> GetCommercialLoanByApplicationDetailId(int loanApplicationDetailId)
        {
            var data =  GetLoanByApplicationDetailId(loanApplicationDetailId);
            return data.Where(x => x.operationId == (short)OperationsEnum.CommercialLoanBooking);
        }

        public IEnumerable<LoanViewModel> GetLoanHistoryByLoanAccountNumber(string loanReferenceNumber)
        {
            return (from data in context.TBL_LOAN
                    where (data.LOANREFERENCENUMBER == loanReferenceNumber) || (data.RELATED_LOAN_REFERENCE_NUMBER == loanReferenceNumber)
                    select new LoanViewModel()
                    {
                        loanId = data.TERMLOANID,
                        customerId = data.CUSTOMERID,
                        productId = data.PRODUCTID,
                        companyId = data.COMPANYID,
                        casaAccountId = data.CASAACCOUNTID,
                        branchId = data.BRANCHID,
                        loanReferenceNumber = data.LOANREFERENCENUMBER,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        principalFrequencyTypeId = (short)data.PRINCIPALFREQUENCYTYPEID,
                        interestFrequencyTypeId = (short)data.INTERESTFREQUENCYTYPEID,

                        principalNumberOfInstallment = data.PRINCIPALNUMBEROFINSTALLMENT,
                        interestNumberOfInstallment = data.INTERESTNUMBEROFINSTALLMENT,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID,
                        misCode = data.MISCODE,
                        teamMiscode = data.TEAMMISCODE,
                        interestRate = data.INTERESTRATE,
                        effectiveDate = data.EFFECTIVEDATE,
                        maturityDate = data.MATURITYDATE,
                        bookingDate = (DateTime)data.BOOKINGDATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        principalInstallmentLeft = data.PRINCIPALINSTALLMENTLEFT,
                        interestInstallmentLeft = data.INTERESTINSTALLMENTLEFT,
                        approvalStatusId = data.APPROVALSTATUSID,
                        approvedBy = data.APPROVEDBY,
                        approverComment = data.APPROVERCOMMENT,
                        dateApproved = data.DATEAPPROVED,
                        loanStatusId = data.LOANSTATUSID,
                        loanStatusName = data.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                        scheduleTypeId = data.SCHEDULETYPEID,
                        isDisbursed = data.ISDISBURSED,
                        disbursedBy = data.DISBURSEDBY,
                        disburserComment = data.DISBURSERCOMMENT,
                        disburseDate = data.DISBURSEDATE,
                        currencyCode = data.TBL_CURRENCY.CURRENCYCODE,
                        approvedAmount = data.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                        operationId = data.OPERATIONID,
                        customerGroupId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                        loanTypeId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                        equityContribution = data.EQUITYCONTRIBUTION,
                        firstPrincipalPaymentDate = data.FIRSTPRINCIPALPAYMENTDATE,
                        firstInterestPaymentDate = data.FIRSTINTERESTPAYMENTDATE,
                        outstandingPrincipal = data.OUTSTANDINGPRINCIPAL,
                        outstandingInterest = data.OUTSTANDINGINTEREST,
                        principalAdditionCount = data.PRINCIPALADDITIONCOUNT,
                        principalReductionCount = data.PRINCIPALREDUCTIONCOUNT,
                        fixedPrincipal = data.FIXEDPRINCIPAL,
                        profileLoan = data.PROFILELOAN,
                        dischargeLetter = data.DISCHARGELETTER,
                        suspendInterest = data.SUSPENDINTEREST,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED
                    }).ToList();
        }

        public IEnumerable<LoanBookingRequestViewModel> GetLoanRequestsByApplicationDetailId(int loanApplicationDetailId)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            return (from data in context.TBL_LOAN_BOOKING_REQUEST
                    where data.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                    select new LoanBookingRequestViewModel()
                    {
                        loanApplicationDetailId = data.LOANAPPLICATIONDETAILID,
                        loanBookingRequestId = data.LOAN_BOOKING_REQUESTID,
                        amount_Requested = data.AMOUNT_REQUESTED,
                        currencyCode = detail != null ? detail.TBL_CURRENCY.CURRENCYCODE : null,
                        approvalStatusId = data.APPROVALSTATUSID,
                        isUsed = data.ISUSED,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED
                    }).ToList();
        }


        public IEnumerable<LoanViewModel> GetLoanByApplicationDetailId(int loanApplicationDetailId)
        {
            return (from data in context.TBL_LOAN
                    where data.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                    select new LoanViewModel()
                    {
                        loanId = data.TERMLOANID,
                        customerId = data.CUSTOMERID,
                        productId = data.PRODUCTID,
                        companyId = data.COMPANYID,
                        casaAccountId = data.CASAACCOUNTID,
                        branchId = data.BRANCHID,
                        loanReferenceNumber = data.LOANREFERENCENUMBER,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        principalFrequencyTypeId = (short)data.PRINCIPALFREQUENCYTYPEID,
                        interestFrequencyTypeId = (short)data.INTERESTFREQUENCYTYPEID,

                        principalNumberOfInstallment = data.PRINCIPALNUMBEROFINSTALLMENT,
                        interestNumberOfInstallment = data.INTERESTNUMBEROFINSTALLMENT,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID,
                        misCode = data.MISCODE,
                        teamMiscode = data.TEAMMISCODE,
                        interestRate = data.INTERESTRATE,
                        effectiveDate = data.EFFECTIVEDATE,
                        maturityDate = data.MATURITYDATE,
                        bookingDate = (DateTime)data.BOOKINGDATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        principalInstallmentLeft = data.PRINCIPALINSTALLMENTLEFT,
                        interestInstallmentLeft = data.INTERESTINSTALLMENTLEFT,
                        approvalStatusId = data.APPROVALSTATUSID,
                        approvedBy = data.APPROVEDBY,
                        approverComment = data.APPROVERCOMMENT,
                        dateApproved = data.DATEAPPROVED,
                        loanStatusId = data.LOANSTATUSID,
                        loanStatusName = data.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                        scheduleTypeId = data.SCHEDULETYPEID,
                        isDisbursed = data.ISDISBURSED,
                        disbursedBy = data.DISBURSEDBY,
                        disburserComment = data.DISBURSERCOMMENT,
                        disburseDate = data.DISBURSEDATE,
                        currencyCode = data.TBL_CURRENCY.CURRENCYCODE,
                        approvedAmount = data.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                        operationId = data.OPERATIONID,
                        customerGroupId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                        loanTypeId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                        equityContribution = data.EQUITYCONTRIBUTION,
                        firstPrincipalPaymentDate = data.FIRSTPRINCIPALPAYMENTDATE,
                        firstInterestPaymentDate = data.FIRSTINTERESTPAYMENTDATE,
                        outstandingPrincipal = data.OUTSTANDINGPRINCIPAL,
                        outstandingInterest = data.OUTSTANDINGINTEREST,
                        principalAdditionCount = data.PRINCIPALADDITIONCOUNT,
                        principalReductionCount = data.PRINCIPALREDUCTIONCOUNT,
                        fixedPrincipal = data.FIXEDPRINCIPAL,
                        profileLoan = data.PROFILELOAN,
                        dischargeLetter = data.DISCHARGELETTER,
                        suspendInterest = data.SUSPENDINTEREST,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED
                    }).ToList();
        }

        /// <summary>
        /// Gets the loan.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns></returns>
        public LoanViewModel GetLoan(int loanId)
        {
            return (from data in context.TBL_LOAN
                    where data.TERMLOANID == loanId
                    select new LoanViewModel()
                    {
                        loanId = data.TERMLOANID,
                        customerId = data.CUSTOMERID,
                        productId = data.PRODUCTID,
                        companyId = data.COMPANYID,
                        casaAccountId = data.CASAACCOUNTID,
                        branchId = data.BRANCHID,
                        loanReferenceNumber = data.LOANREFERENCENUMBER,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        principalFrequencyTypeId = (short)data.PRINCIPALFREQUENCYTYPEID,
                        interestFrequencyTypeId = (short)data.INTERESTFREQUENCYTYPEID,

                        principalNumberOfInstallment = data.PRINCIPALNUMBEROFINSTALLMENT,
                        interestNumberOfInstallment = data.INTERESTNUMBEROFINSTALLMENT,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID,
                        misCode = data.MISCODE,
                        teamMiscode = data.TEAMMISCODE,
                        interestRate = data.INTERESTRATE,
                        effectiveDate = data.EFFECTIVEDATE,
                        maturityDate = data.MATURITYDATE,
                        bookingDate = (DateTime)data.BOOKINGDATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        principalInstallmentLeft = data.PRINCIPALINSTALLMENTLEFT,
                        interestInstallmentLeft = data.INTERESTINSTALLMENTLEFT,
                        approvalStatusId = data.APPROVALSTATUSID,
                        approvedBy = data.APPROVEDBY,
                        approverComment = data.APPROVERCOMMENT,
                        dateApproved = data.DATEAPPROVED,
                        loanStatusId = data.LOANSTATUSID,
                        scheduleTypeId = data.SCHEDULETYPEID,
                        isDisbursed = data.ISDISBURSED,
                        disbursedBy = data.DISBURSEDBY,
                        disburserComment = data.DISBURSERCOMMENT,
                        disburseDate = data.DISBURSEDATE,

                        approvedAmount = data.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                        operationId = data.OPERATIONID,
                        customerGroupId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                        loanTypeId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                        equityContribution = data.EQUITYCONTRIBUTION,
                        firstPrincipalPaymentDate = data.FIRSTPRINCIPALPAYMENTDATE,
                        firstInterestPaymentDate = data.FIRSTINTERESTPAYMENTDATE,
                        outstandingPrincipal = data.OUTSTANDINGPRINCIPAL,
                        principalAdditionCount = data.PRINCIPALADDITIONCOUNT,
                        principalReductionCount = data.PRINCIPALREDUCTIONCOUNT,
                        fixedPrincipal = data.FIXEDPRINCIPAL,
                        profileLoan = data.PROFILELOAN,
                        dischargeLetter = data.DISCHARGELETTER,
                        suspendInterest = data.SUSPENDINTEREST,
                        //customerSensitivityLevelId = data.CUSTOMERSENSITIVITYLEVELID,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED
                    }).FirstOrDefault();
        }

        /// <summary>
        /// Finds the loan.
        /// </summary>
        /// <param name="referenceNumberOrName">Name of the reference number or.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId)
        {
            return (from data in context.TBL_LOAN
                    where data.COMPANYID == companyId && (data.LOANREFERENCENUMBER == referenceNumberOrName ||
                      $"{data.TBL_CUSTOMER.FIRSTNAME} {data.TBL_CUSTOMER.MIDDLENAME} {data.TBL_CUSTOMER.LASTNAME} {data.TBL_CUSTOMER.CUSTOMERCODE} {data.TBL_CASA.PRODUCTACCOUNTNUMBER}".Contains(referenceNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new LoanViewModel()
                    {
                        loanId = data.TERMLOANID,
                        customerId = data.CUSTOMERID,
                        productId = data.PRODUCTID,
                        companyId = data.COMPANYID,
                        casaAccountId = data.CASAACCOUNTID,
                        branchId = data.BRANCHID,
                        loanReferenceNumber = data.LOANREFERENCENUMBER,
                        //tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        //tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = (short)data.PRINCIPALFREQUENCYTYPEID,
                        interestFrequencyTypeId = (short)data.INTERESTFREQUENCYTYPEID,

                        principalNumberOfInstallment = data.PRINCIPALNUMBEROFINSTALLMENT,
                        interestNumberOfInstallment = data.INTERESTNUMBEROFINSTALLMENT,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID,
                        misCode = data.MISCODE,
                        teamMiscode = data.TEAMMISCODE,
                        interestRate = data.INTERESTRATE,
                        effectiveDate = data.EFFECTIVEDATE,
                        maturityDate = data.MATURITYDATE,
                        bookingDate = (DateTime)data.BOOKINGDATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        principalInstallmentLeft = data.PRINCIPALINSTALLMENTLEFT,
                        interestInstallmentLeft = data.INTERESTINSTALLMENTLEFT,
                        approvalStatusId = data.APPROVALSTATUSID,
                        approvedBy = data.APPROVEDBY,
                        approverComment = data.APPROVERCOMMENT,
                        dateApproved = data.DATEAPPROVED,
                        loanStatusId = data.LOANSTATUSID,
                        scheduleTypeId = data.SCHEDULETYPEID,
                        isDisbursed = data.ISDISBURSED,
                        disbursedBy = data.DISBURSEDBY,
                        disburserComment = data.DISBURSERCOMMENT,
                        disburseDate = data.DISBURSEDATE,

                        approvedAmount = data.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                        operationId = data.OPERATIONID,
                        customerGroupId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                        loanTypeId = data.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                        equityContribution = data.EQUITYCONTRIBUTION,
                        firstPrincipalPaymentDate = data.FIRSTPRINCIPALPAYMENTDATE,
                        outstandingPrincipal = data.OUTSTANDINGPRINCIPAL,
                        principalAdditionCount = data.PRINCIPALADDITIONCOUNT,
                        principalReductionCount = data.PRINCIPALREDUCTIONCOUNT,
                        fixedPrincipal = data.FIXEDPRINCIPAL,
                        profileLoan = data.PROFILELOAN,
                        dischargeLetter = data.DISCHARGELETTER,
                        suspendInterest = data.SUSPENDINTEREST,
                        //customerSensitivityLevelId = data.CUSTOMERSENSITIVITYLEVELID,
                        createdBy = data.CREATEDBY,
                        dateTimeCreated = data.DATETIMECREATED

                    });
        }

        /// <summary>
        /// Gets the loans by company identifier.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IQueryable<LoanViewModel> GetLoansByCompanyId(int companyId) // EXTEND FOR ORDER LOAN TYPES
        {
            var systemDate = generalSetup.GetApplicationDate();
            return (context.TBL_LOAN //.Include("tbl_Customer").Include("tbl_CASA_AccountStatus")
                .Where(x => x.COMPANYID == companyId)
                .Select(o => new LoanViewModel
                {
                    loanId = o.TERMLOANID,
                    loanApplicationId = o.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                    customerId = o.CUSTOMERID,
                    productId = o.PRODUCTID,
                    casaAccountId = o.CASAACCOUNTID,
                    branchId = o.BRANCHID,
                    loanReferenceNumber = o.LOANREFERENCENUMBER,
                    principalFrequencyTypeId = (short)o.PRINCIPALFREQUENCYTYPEID,
                    interestFrequencyTypeId = (short)o.INTERESTFREQUENCYTYPEID,

                    principalNumberOfInstallment = o.PRINCIPALNUMBEROFINSTALLMENT,
                    interestNumberOfInstallment = o.INTERESTNUMBEROFINSTALLMENT,
                    relationshipOfficerId = o.RELATIONSHIPOFFICERID,
                    relationshipManagerId = o.RELATIONSHIPMANAGERID,
                    misCode = o.MISCODE,
                    teamMiscode = o.TEAMMISCODE,
                    interestRate = o.INTERESTRATE,
                    effectiveDate = o.EFFECTIVEDATE,
                    maturityDate = o.MATURITYDATE,
                    bookingDate = (DateTime)o.BOOKINGDATE,
                    principalAmount = o.PRINCIPALAMOUNT,
                    principalInstallmentLeft = o.PRINCIPALINSTALLMENTLEFT,
                    interestInstallmentLeft = o.INTERESTINSTALLMENTLEFT,
                    approvalStatusId = o.APPROVALSTATUSID,
                    firstName = o.TBL_CUSTOMER.FIRSTNAME,
                    middleName = o.TBL_CUSTOMER.MIDDLENAME,
                    lastName = o.TBL_CUSTOMER.LASTNAME,
                    customerCode = o.TBL_CUSTOMER.CUSTOMERCODE,
                    //productAccountNumber = o.TBL_CASA.PRODUCTACCOUNTNUMBER,
                    //productAccountName = o.TBL_CASA.PRODUCTACCOUNTNAME,
                    outstandingPrincipal = o.OUTSTANDINGPRINCIPAL,
                    outstandingInterest = o.OUTSTANDINGINTEREST,
                    productName = o.TBL_PRODUCT.PRODUCTNAME,
                    productTypeId = o.TBL_PRODUCT.PRODUCTTYPEID,
                }));
        }

        /// <summary>
        /// Loans the search.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchModel">The search model.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> LoanSearch(int companyId, LoanSearchViewModel searchModel)
        {
            var loans = GetLoansByCompanyId(companyId);

            if (!String.IsNullOrEmpty(searchModel.loanReferenceNumber))
            {
                loans = loans.Where(x => x.loanReferenceNumber == searchModel.loanReferenceNumber);
            }

            if (!String.IsNullOrEmpty(searchModel.productAccountNumber))
            {
                loans = loans.Where(x => x.casaAccountNumber == searchModel.productAccountNumber);
            }

            if (!String.IsNullOrEmpty(searchModel.customerName))
            {
                loans = loans.Where(x =>
                x.firstName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.lastName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.middleName.ToLower().Contains(searchModel.customerName.ToLower())
                || x.customerCode.ToLower().Contains(searchModel.customerName.ToLower())
                );
            }

            if (!String.IsNullOrEmpty(searchModel.loanName))
            {
                loans = loans.Where(x => x.productAccountName.ToLower().Contains(searchModel.loanName.ToLower()));
            }

            loans.OrderBy(x => x.casaAccountNumber).ThenBy(x => x.productAccountName);

            return loans;
        }

        /// <summary>
        /// Gets the days in a year.
        /// </summary>
        /// <param name="dayCountId">The day count identifier.</param>
        /// <returns></returns>
        private int GetDaysInAYear(DayCountConventionEnum dayCountId)
        {
            if (dayCountId == DayCountConventionEnum.Actual_Actual)
            {
                var currentDate = DateTime.Now;
                var firstDate = new DateTime(currentDate.Year, 1, 1); //  DateTime.ParseExact(user, "MM-dd-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var lastdate = new DateTime(currentDate.Year, 12, 31);
                var difference = (lastdate - firstDate).TotalDays;

                return Convert.ToInt32(difference);
            }

            var value = context.TBL_DAY_COUNT_CONVENTION.FirstOrDefault(x => x.DAYCOUNTCONVENTIONID == (short)dayCountId).DAYSINAYEAR;

            return value;
        }

        /// <summary>
        /// Gets the loan covenant.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns></returns>
        public List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId)
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

        /// <summary>
        /// Gets the product fees.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId)
        {
            var data = (from c in context.TBL_CHARGE_FEE
                        join p in context.TBL_PRODUCT_CHARGE_FEE on c.CHARGEFEEID equals p.CHARGEFEEID
                        where p.PRODUCTID == productId && c.DELETED == false
                        select new LoanChargeFeeViewModel
                        {
                            chargeFeeId = c.CHARGEFEEID,
                            chargeFeeName = c.CHARGEFEENAME,
                            feeTypeId = c.FEETYPEID,
                            feeTypeName = c.TBL_FEE_TYPE.FEETYPENAME,
                            feeIntervalId = c.FEEINTERVALID,
                            productFeeId = p.PRODUCTFEEID,
                            feeRateValue = p.RATEVALUE,
                            feeDependentAmount = p.DEPENDENTAMOUNT ?? 0,
                            chargeAmount = c.AMOUNT ?? 0,
                            feeIntervalName = c.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                            required = c.TBL_FEE_TYPE.BYAMOUNTREQUIRED,
                            recurring = (bool)c.RECURRING,
                            feeTargetId = c.FEETARGETID,
                            feeTargetName = c.TBL_FEE_TARGET.FEETARGETNAME,
                            isIntegralFee = c.ISINTEGRALFEE,
                            chargeRange = (from r in context.TBL_CHARGE_RANGE
                                           where r.CHARGEFEEID == c.CHARGEFEEID
                                           select new ChargeRangeViewModel
                                           {
                                               maximum = r.MAXIMUM,
                                               minimum = r.MINIMUM,
                                               rate = r.RATE,
                                               amount = r.AMOUNT,
                                               maximumAndBelow = r.MAXIMUMANDBELOW,
                                               minimumAndAbove = r.MINIMUMANDABOVE,
                                           }).ToList(),

                        }).ToList();
            return data;
        }

        public List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId)
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
                        }).ToList();
            return data;
        }

        /// <summary>
        /// Gets the loan product charge fees by product identifier.
        /// </summary>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>
        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByProductId(int productId)
        {
            var data = (from c in context.TBL_CHARGE_FEE
                        join p in context.TBL_PRODUCT_CHARGE_FEE
                        on c.CHARGEFEEID equals p.CHARGEFEEID
                        where p.PRODUCTID == productId && p.DELETED == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.PRODUCTFEEID,
                            productId = p.PRODUCTID,
                            productName = p.TBL_PRODUCT.PRODUCTNAME,
                            feeId = c.CHARGEFEEID,
                            feeName = c.CHARGEFEENAME,
                            feeTargetName = c.TBL_FEE_TARGET.FEETARGETNAME,
                            feeIntervalName = c.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                            rateValue = (decimal)p.RATEVALUE,
                            dependentAmount = p.DEPENDENTAMOUNT

                        }).ToList();
            return data;
        }

        /// <summary>
        /// Gets the current customer exposure.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        /// 
        /// <summary>
        /// Gets the loan product charge fees by charge fee identifier.
        /// </summary>
        /// <param name="chargeFeeId">The charge fee identifier.</param>
        /// <returns></returns>
        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByChargeFeeId(int chargeFeeId)
        {
            var data = (from c in context.TBL_CHARGE_FEE
                        join p in context.TBL_PRODUCT_CHARGE_FEE
                        on c.CHARGEFEEID equals p.CHARGEFEEID
                        where p.CHARGEFEEID == chargeFeeId && p.DELETED == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.PRODUCTFEEID,
                            productId = p.PRODUCTID,
                            productName = p.TBL_PRODUCT.PRODUCTNAME,
                            feeId = c.CHARGEFEEID,
                            feeName = c.CHARGEFEENAME,
                            feeTargetName = c.TBL_FEE_TARGET.FEETARGETNAME,
                            feeIntervalName = c.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                            rateValue = (decimal)p.RATEVALUE,
                            dependentAmount = p.DEPENDENTAMOUNT

                        }).ToList();
            return data;
        }

        /// <summary>
        /// Gets the loan guarantors.
        /// </summary>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <returns></returns>


        /// <summary>
        /// Gets the loan product charge fee.
        /// </summary>
        /// <param name="chargeFeeId">The charge fee identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <returns></returns>

        public IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            var data = (from c in context.TBL_CHARGE_FEE
                        join p in context.TBL_PRODUCT_CHARGE_FEE on c.CHARGEFEEID equals p.CHARGEFEEID
                        where p.PRODUCTID == productId && c.DELETED == false
                        select new LoanChargeFeeViewModel
                        {
                            productFeeId = p.PRODUCTFEEID,
                            chargeFeeId = c.CHARGEFEEID,
                            chargeFeeName = c.CHARGEFEENAME,
                            feeRateValue = p.RATEVALUE,
                            feeDependentAmount = p.DEPENDENTAMOUNT ?? 0,
                            feeAmount = c.AMOUNT ?? 0,
                            feeIntervalId = c.FEEINTERVALID,
                            feeIntervalName = c.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                            feeTypeId = c.FEETYPEID,
                            required = c.TBL_FEE_TYPE.BYAMOUNTREQUIRED,
                            recurring = (bool)c.RECURRING,
                            //feeTypeName = c.FeeTypeName

                        }).ToList();
            return data;
        }



        /// <summary>
        /// Searches the customer collateral.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public IQueryable<CustomerSearchItemViewModels> SearchCustomerCollateral(int companyId, string searchQuery)
        {
            return this.customers.CustomerSearchRealTime(companyId, searchQuery);
        }

        /// <summary>
        /// Searches for customer collateral.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public IQueryable<CustomerViewModels> SearchForCustomerCollateral(int companyId, string searchQuery)
        {
            IQueryable<CustomerViewModels> allCustomers = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allCustomers = GetCustomers()
                    .Where(c => c.companyId == companyId)
                    .Where(x => x.firstName.Contains(searchQuery)
               || x.lastName.Contains(searchQuery)
               || x.middleName.Contains(searchQuery)
                );
            }

            return allCustomers;
        }

        /// <summary>
        /// Gets the customers.
        /// </summary>
        /// <returns></returns>
        IQueryable<CustomerViewModels> GetCustomers()
        {
            return from a in context.TBL_CUSTOMER
                   where a.DELETED == false
                   select

                   new CustomerViewModels
                   {
                       accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                       branchId = a.BRANCHID,
                       branchName = a.TBL_BRANCH.BRANCHNAME,
                       //childDateOfBirth = a.CHILDDATEOFBIRTH.Value,
                       companyMainId = a.COMPANYID,
                       createdBy = a.CREATEDBY,
                       creationMailSent = a.CREATIONMAILSENT,
                       customerCode = a.CUSTOMERCODE,
                       customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                       customerTypeId = a.CUSTOMERTYPEID.Value,
                       dateOfBirth = (DateTime)a.DATEOFBIRTH,
                       customerId = a.CUSTOMERID,
                       emailAddress = a.EMAILADDRESS,
                       //firstChildName = a.FIRSTCHILDNAME,
                       firstName = a.FIRSTNAME,
                       gender = a.GENDER,
                       lastName = a.LASTNAME,
                       maidenName = a.MAIDENNAME,
                       maritalStatus = a.MARITALSTATUS.Value,
                       title = a.TITLE,
                       middleName = a.MIDDLENAME,
                       customerTypeName = context.TBL_CUSTOMER_TYPE.FirstOrDefault(c => c.CUSTOMERTYPEID == a.CUSTOMERTYPEID).NAME,
                       misCode = a.MISCODE,
                       misStaff = a.MISSTAFF,
                       nationality = a.NATIONALITY,
                       occupation = a.OCCUPATION,
                       placeOfBirth = a.PLACEOFBIRTH,
                       isInvestmentGrade = a.ISINVESTMENTGRADE,
                       isRealatedParty = a.ISREALATEDPARTY,
                       isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                       relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                       spouse = a.SPOUSE,
                       sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                       subSectorId = (short)a.SUBSECTORID,
                       subSectorName = a.TBL_SUB_SECTOR.NAME,
                       taxNumber = a.TAXNUMBER
                       ,
                       CustomerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == a.CUSTOMERID).Select(x => new CustomerAddressViewModels()
                       {
                           address = x.ADDRESS,
                           addressTypeId = x.ADDRESSTYPEID,
                           cityId = x.CITYID,
                           customerId = x.CUSTOMERID,
                           homeTown = x.HOMETOWN,
                           nearestLandmark = x.NEARESTLANDMARK,
                           electricMeterNumber = x.ELECTRICMETERNUMBER,
                           pobox = x.POBOX,
                           stateId = x.STATEID,
                           addressId = x.ADDRESSID
                       }).ToList(),
                       CustomerPhoneContact = context.TBL_CUSTOMER_PHONECONTACT.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => new CustomerPhoneContactViewModels
                       {
                           active = c.ACTIVE,
                           customerId = c.CUSTOMERID,
                           phone = c.PHONE,
                           phoneContactId = c.PHONECONTACTID,
                           phoneNumber = c.PHONENUMBER
                       }).ToList(),
                       CustomerCompanyInfomation = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(d => d.CUSTOMERID == a.CUSTOMERID).Select(d => new CustomerCompanyInfomationViewModels()
                       {
                           annualTurnOver = d.ANNUALTURNOVER,
                           companyEmail = d.COMPANYEMAIL,
                           companyId = d.CUSTOMERID,
                           companyName = d.COMPANYNAME,
                           companyWebsite = d.COMPANYWEBSITE,
                           companyInfomationId = d.COMPANYINFOMATIONID,
                           corporateBusinessCategory = d.CORPORATEBUSINESSCATEGORY,
                           createdBy = a.CREATEDBY,
                           //creditRating = d.CREDITRATING,
                           registeredOffice = d.REGISTEREDOFFICE,
                           // previousCreditRating = d.PREVIOUSCREDITRATING,
                           registrationNumber = d.REGISTRATIONNUMBER,
                           paidUpCapital = d.PAIDUPCAPITAL,
                           authorizedCapital = d.AUTHORISEDCAPITAL

                       }).ToList(),
                       CustomerIdentification = context.TBL_CUSTOMER_IDENTIFICATION.Where(e => e.CUSTOMERID == a.CUSTOMERID).Select(e => new CustomerIdentificationViewModels()
                       {
                           identificationId = e.IDENTIFICATIONID,
                           identificationModeId = e.IDENTIFICATIONMODEID.Value,
                           identificationMode = context.TBL_CUSTOMER_IDENTI_MODE_TYPE.FirstOrDefault(r => r.IDENTIFICATIONMODEID == e.IDENTIFICATIONMODEID).IDENTIFICATIONMODE,
                           identificationNo = e.IDENTIFICATIONNO,
                           issueAuthority = e.ISSUEAUTHORITY,
                           issuePlace = e.ISSUEPLACE
                       }).ToList(),
                       CustomerEmploymentHistory = context.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(s => s.CUSTOMERID == a.CUSTOMERID).Select(s => new CustomerEmploymentHistoryViewModels()
                       {
                           active = s.ACTIVE,
                           previousEmployer = s.PREVIOUSEMPLOYER,
                           customerId = s.CUSTOMERID,
                           employDate = s.EMPLOYDATE,
                           placeOfWorkId = s.PLACEOFWORKID,
                           employerAddress = s.EMPLOYERADDRESS,
                           employerCountryId = s.EMPLOYERCOUNTRYID,
                           employerName = s.EMPLOYERNAME,
                           officePhone = s.OFFICEPHONE,
                           employerStateId = s.EMPLOYERSTATEID
                       }).ToList(),
                       CustomerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                       .Select(s => new CustomerCompanyDirectorsViewModels()
                       {
                           companyDirectorId = s.COMPANYDIRECTORID,
                           surname = s.SURNAME,
                           firstname = s.FIRSTNAME,
                           numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           customerId = s.CUSTOMERID,
                           customerName = s.FIRSTNAME + " " + s.SURNAME,
                           address = s.ADDRESS,
                           phoneNumber = s.PHONENUMBER,
                           email = s.EMAILADDRESS
                       }).ToList(),
                       CustomerCompanyShareholder = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(s => s.CUSTOMERID == a.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                       .Select(s => new CustomerCompanyShareholderViewModels()
                       {
                           companyDirectorId = s.COMPANYDIRECTORID,
                           surname = s.SURNAME,
                           firstname = s.FIRSTNAME,
                           numberOfShares = s.SHAREHOLDINGPERCENTAGE,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                           customerId = s.CUSTOMERID,
                           customerName = s.FIRSTNAME + " " + s.SURNAME,
                           address = s.ADDRESS,
                           phoneNumber = s.PHONENUMBER,
                           email = s.EMAILADDRESS
                       }).ToList(),
                       CustomerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID)
                       .Select(x => new CollateralViewModel()
                       {
                           collateralId = x.COLLATERALCUSTOMERID,
                           collateralTypeId = x.COLLATERALTYPEID,
                           collateralSubTypeId = x.COLLATERALSUBTYPEID,
                           customerId = x.CUSTOMERID,
                           currencyId = x.CURRENCYID,
                           currency = x.TBL_CURRENCY.CURRENCYNAME,
                           collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                           collateralCode = x.COLLATERALCODE,
                           camRefNumber = x.CAMREFNUMBER,
                           allowSharing = x.ALLOWSHARING,
                           isLocationBased = (bool)x.ISLOCATIONBASED,
                           valuationCycle = x.VALUATIONCYCLE,
                           haircut = x.HAIRCUT,
                           approvalStatus = x.APPROVALSTATUS,
                       }).ToList(),
                   };

        }


        /// <summary>
        /// Gets the appraisal memorandum loan updates.
        /// </summary>
        /// <param name="appraisalMemorandumId">The appraisal memorandum identifier.</param>
        /// <returns></returns>
        public AppraisalMemorandumLoanDetailViewModel GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId)
        {
            return (from data in context.TBL_CREDIT_APPRAISAL_MEMO_DETL
                    where data.APPRAISALMEMORANDUMID == appraisalMemorandumId
                    orderby data.MEMORANDUMLOANDETAILID descending
                    select new AppraisalMemorandumLoanDetailViewModel()
                    {
                        interestRate = data.INTERESTRATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        tenor = data.TENOR,
                    }).FirstOrDefault();
        }

        private IEnumerable<CamProcessedLoanViewModel> AvailedLoanApplicationsDetails(int companyId, int staffId, int branchId)
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
                            && m.BRANCHID == branchId
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
                                createdBy = m.CREATEDBY,
                                newApplicationDate = m.APPLICATIONDATE,
                                dateTimeCreated = d.DATETIMECREATED,
                                availmentDate = m.AVAILMENTDATE,
                                isTemporaryOverdraft = p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault() != null ? p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault().ISTEMPORARYOVERDRAFT : false,
                                loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0
                            });
                foreach (var item in data)
                {
                    var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                    var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                    var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
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
                        case (short)LoanProductTypeEnum.RevolvingLoan:
                            decimal overdraftBal = 0;
                            foreach (var overdraft in overdrafts)
                            {
                                if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                            break;
                        case (short)LoanProductTypeEnum.ContingentLiability:
                            decimal contingentBal = 0;
                            foreach (var contingent in contingents)
                            {
                                if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - contingentBal;
                            break;
                    }
                }
                return data;
            }
            catch (Exception ex) { throw; }
        }

        public IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsDueForInitiateBooking(int companyId, int staffId, int branchId)
        {
            try
            {
                var data = AvailedLoanApplicationsDetails(companyId, staffId, branchId).Where(x => x.productClassProcessId != (short)ProductClassProcessEnum.ProductBased
                           && x.productTypeId != (short)LoanProductTypeEnum.RevolvingLoan
                           && x.productTypeId != (short)LoanProductTypeEnum.ContingentLiability);

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
                    if(disbursedLoan.Any())
                    {
                        item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                    }

                }

                return data;
            }
            catch (Exception ex) { throw ex; }
        }

        public IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationDetailById(int staffId, int companyId, int applicationDetailId, int loanBookingRequestId)
        {
            var data = AvailedLoanApplicationsReadyForBookingByApplicationDetailId(staffId, companyId, applicationDetailId, loanBookingRequestId); //.Where(x => x.bookingRequestStatusId == (int)ApprovalStatusEnum.Approved);

            data = (from a in data where ((a.customerAvailableAmount >= 0) || (a.customerAvailableAmount == null)) select a).ToList();

            foreach (var item in data)
            {
                if (item.customerAvailableAmount != 0)
                {
                    if (!item.customerAvailableAmount.HasValue)
                        item.customerAvailableAmount = item.approvedAmount;
                }
            }
            return data;
        }

        public bool AddLoanBookingRequest(int applicationStatusId, LoanBookingRequestViewModel entity)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.loanApplicationDetailId);
                var operationId = 0;
                var productTypeId = loanApplicationDetails.TBL_PRODUCT.PRODUCTTYPEID;

                if (productTypeId == (short)LoanProductTypeEnum.TermLoan || productTypeId == (short)LoanProductTypeEnum.SelfLiquidating)
                    operationId = (short)OperationsEnum.TermLoanBooking;
                if (productTypeId == (short)LoanProductTypeEnum.CommercialLoan)
                    operationId = (short)OperationsEnum.CommercialLoanBooking;
                if (productTypeId == (short)LoanProductTypeEnum.ForeignXRevolving)
                    operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;

                try
                {
                    var request = new TBL_LOAN_BOOKING_REQUEST
                    {
                        AMOUNT_REQUESTED = entity.amount_Requested,
                        APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                        LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                        ISUSED = false,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = entity.createdBy,

                    };
                    context.TBL_LOAN_BOOKING_REQUEST.Add(request);
                    context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = request.LOAN_BOOKING_REQUESTID,
                        comment = "Please approve this request for loan booking",
                        amount = entity.amount_Requested,
                    };
                    
                    LogApproval(approvalModel,(short)OperationsEnum.LoanBookingRequest, true, (int)ApprovalStatusEnum.Pending);

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LoanBookingRequested,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = $"Request to book loan of amount '{ entity.amount_Requested }' for customer'{entity.customerName}'",
                        IPADDRESS = entity.userIPAddress,
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = generalSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------

                    var response = context.SaveChanges() > 0;
                    trans.Commit();
                    return response;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
                   
        }

        public IEnumerable<CamProcessedLoanViewModel> GetAvailedLoanApplicationsReadyForBooking(int companyId, int staffId)
        {
            var staff = context.TBL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();

            var cpldStaffLevels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == (int)OperationsEnum.TermLoanBooking
            || x.OPERATIONID == (int)OperationsEnum.CommercialLoanBooking
            || x.OPERATIONID == (int)OperationsEnum.ForeignExchangeLoanBooking
            || x.OPERATIONID == (int)OperationsEnum.RevolvingLoanBooking)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         staffRoleId = l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList();

            var staffLevelId = (from x in context.TBL_APPROVAL_GROUP_MAPPING
                                join y in context.TBL_APPROVAL_GROUP on x.GROUPID equals y.GROUPID
                                join z in context.TBL_APPROVAL_LEVEL on x.GROUPID equals z.GROUPID
                                join st in context.TBL_APPROVAL_LEVEL_STAFF on z.APPROVALLEVELID equals st.APPROVALLEVELID
                                where x.OPERATIONID == (int)OperationsEnum.TermLoanBooking && (z.STAFFROLEID == staff.STAFFROLEID || st.STAFFID == staff.STAFFID)
                                select z.APPROVALLEVELID).FirstOrDefault();

            var cpldStaffRoleLevels = cpldStaffLevels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var cpldStaffRoleLevelIds = cpldStaffRoleLevels.Select(x => x.levelId);
            //var staffRoleLevelId = cpldStaffRoleLevelIds.FirstOrDefault();

            List<int> operationIds = new List<int>();
            if (cpldStaffRoleLevelIds.Any())
            {
                operationIds.Add((int)OperationsEnum.TermLoanBooking);
                operationIds.Add((int)OperationsEnum.RevolvingLoanBooking);
                operationIds.Add((int)OperationsEnum.ForeignExchangeLoanBooking);
                operationIds.Add((int)OperationsEnum.CommercialLoanBooking);
            }

            var bAndGStaffLevels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == (int)OperationsEnum.ContigentLoanBooking)
                .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                    mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                    {
                        groupPosition = mg.m.POSITION,
                        levelPosition = l.POSITION,
                        levelId = l.APPROVALLEVELID,
                        levelName = l.LEVELNAME,
                        staffRoleId = l.STAFFROLEID,
                    })
                    .OrderBy(x => x.groupPosition)
                    .ThenBy(x => x.levelPosition)
                    .ToList();

            var bAndGStaffRoleLevels = bAndGStaffLevels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var bAndGStaffRoleLevelIds = bAndGStaffRoleLevels.Select(x => x.levelId);
            //var staffRoleLevelId = cpldStaffRoleLevelIds.FirstOrDefault();

            if (!cpldStaffRoleLevelIds.Any() && bAndGStaffRoleLevelIds.Any())
            {
                operationIds.Add((int)OperationsEnum.ContigentLoanBooking);
            }

            

            IEnumerable<CamProcessedLoanViewModel> data = null;
            //var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TermLoanBooking).ToList();
            //var idContigent = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ContigentLoanBooking).ToList();

            //List<int> operationIds = new List<int>();
            //if(ids.Contains(74) || ids.Contains(76))
            //{
            //    operationIds.Add((int)OperationsEnum.ContigentLoanBooking);
            //}
            //else
            //{
            //    operationIds.Add((int)OperationsEnum.TermLoanBooking);
            //    operationIds.Add((int)OperationsEnum.RevolvingLoanBooking);
            //    operationIds.Add((int)OperationsEnum.ForeignExchangeLoanBooking);
            //    operationIds.Add((int)OperationsEnum.CommercialLoanBooking);
            //}


            data = (from s in context.TBL_LOAN_BOOKING_REQUEST
                    join atrail in context.TBL_APPROVAL_TRAIL on s.LOAN_BOOKING_REQUESTID equals atrail.TARGETID
                    join d in context.TBL_LOAN_APPLICATION_DETAIL on s.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                    join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                    join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                    join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                    join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                    where m.COMPANYID == companyId
                    && ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                    || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                    || (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred))
                    && s.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && s.ISUSED == false && s.DELETED == false
                    && ((cpldStaffRoleLevelIds.Contains((int)atrail.TOAPPROVALLEVELID)) || (bAndGStaffRoleLevelIds.Contains((int)atrail.TOAPPROVALLEVELID)) || (atrail.REQUESTSTAFFID == staffId))
                    && operationIds.Contains(atrail.OPERATIONID)
                    && atrail.RESPONSESTAFFID == null
                    orderby s.LOAN_BOOKING_REQUESTID descending
                    select new CamProcessedLoanViewModel()
                    {
                        bookingAmountRequested = s.AMOUNT_REQUESTED,
                        loanBookingRequestId = s.LOAN_BOOKING_REQUESTID,
                        bookingRequestStatusId = s.APPROVALSTATUSID,
                        requestDate = s.DATETIMECREATED,
                        requestedBy = "",
                        requestedAmount = s.AMOUNT_REQUESTED,
                        requestOperationId = (short)OperationsEnum.LoanBookingRequest,
                        approvalStatusId = atrail.APPROVALSTATUSID,
                        approvalStatusName = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                        loanApplicationId = m.LOANAPPLICATIONID,
                        loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                        applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                        applicationStatusId = m.APPLICATIONSTATUSID,

                        customerId = d.CUSTOMERID,
                        customerCode = cust.CUSTOMERCODE,
                        customerName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                        customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                        customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                        customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,

                        applicationTenor = m.APPLICATIONTENOR,
                        effectiveDate = (DateTime)d.EFFECTIVEDATE,
                        expiryDate = (DateTime)d.EXPIRYDATE,

                        ////currencyId = d.CURRENCYID,
                        currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                        exchangeRate = d.EXCHANGERATE,
                        loanTypeId = m.LOANAPPLICATIONTYPEID,
                        loanTypeName = m.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                        productId = d.APPROVEDPRODUCTID,
                        productTypeId = d.TBL_PRODUCT.PRODUCTTYPEID,
                        productTypeName = d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        productName = d.TBL_PRODUCT.PRODUCTNAME,

                        interestRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                        groupApprovedAmount = m.APPROVEDAMOUNT,
                        availmentDate = m.AVAILMENTDATE,
                        approvedTenor = d.APPROVEDTENOR,
                        toStaffId = atrail.TOSTAFFID,
                        requestStaffId = atrail.REQUESTSTAFFID,
                    }).ToList();
         
            foreach (var item in data)
            {
                item.isBooked = context.TBL_LOAN.Where(x => x.LOAN_BOOKING_REQUESTID == item.loanBookingRequestId).Any();

                int operationId = 0;
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                    operationId = (short)OperationsEnum.ContigentLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.ForeignXRevolving)
                    operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.CommercialLoan)
                    operationId = (short)OperationsEnum.CommercialLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.TermLoan || item.productTypeId == (short)LoanProductTypeEnum.SelfLiquidating)
                    operationId = (short)OperationsEnum.TermLoanBooking;

                var trailInfo = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == item.loanBookingRequestId);
                if (trailInfo.Any())
                    item.isUnderApproval = true;

                var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
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
                    case (short)LoanProductTypeEnum.RevolvingLoan:
                        decimal overdraftBal = 0;
                        foreach (var overdraft in overdrafts)
                        {
                            if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                        break;
                    case (short)LoanProductTypeEnum.ContingentLiability:
                        decimal contingentBal = 0;
                        foreach (var contingent in contingents)
                        {
                            if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - contingentBal;
                        break;
                    case (short)LoanProductTypeEnum.ForeignXRevolving:
                        decimal customerAvailableAmount4 = 0;
                        foreach (var loan in loans)
                        {
                            if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount4 = customerAvailableAmount4 + loan.PRINCIPALAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount4;
                        break;
                }

                var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                if (disbursedLoan.Any())
                {
                    item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                }
            }
            data = (from a in data where ((a.customerAvailableAmount >= 0)) select a).ToList();
            return data;
        }

        /// <summary>
        /// Gets the appraisal memorandum processed loan applications.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private IEnumerable<CamProcessedLoanViewModel> AvailedLoanApplicationsReadyForBookingByApplicationDetailId(int staffId,int companyId, int applicationDetailId, int loanBookingRequestId)
        {
            var canReRouteBooking = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == staffId && x.ACTIVITYID == 177).Any();
            var newApplicationDate = generalSetup.GetApplicationDate();
            var data = (from s in context.TBL_LOAN_BOOKING_REQUEST
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on s.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                        join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                        join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                        where d.LOANAPPLICATIONDETAILID == applicationDetailId && d.DELETED == false && s.DELETED == false
                        && s.LOAN_BOOKING_REQUESTID == loanBookingRequestId
                        orderby s.LOAN_BOOKING_REQUESTID descending
                        select new CamProcessedLoanViewModel
                        {
                            bookingAmountRequested = s.AMOUNT_REQUESTED,
                            loanBookingRequestId = s.LOAN_BOOKING_REQUESTID,
                            bookingRequestStatusId = s.APPROVALSTATUSID,
                            requestDate = s.DATETIMECREATED,
                            requestedBy = "",

                            requestedAmount = s.AMOUNT_REQUESTED,
                            requestOperationId = (short)OperationsEnum.LoanBookingRequest,
                            approvalStatusId = (short)m.APPROVALSTATUSID,
                            loanApplicationId = m.LOANAPPLICATIONID,
                            loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                            applicationStatusId = m.APPLICATIONSTATUSID,

                            customerId = d.CUSTOMERID,
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
                            newApplicationDate = newApplicationDate,
                            loanInformation = m.LOANINFORMATION,
                            companyId = m.COMPANYID,
                            branchId = m.BRANCHID,
                            branchName = m.TBL_BRANCH.BRANCHNAME,
                            subSectorId = d.SUBSECTORID,
                            subSectorName = d.TBL_SUB_SECTOR.NAME,
                            sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            applicationTenor = m.APPLICATIONTENOR,
                            effectiveDate = (DateTime)d.EFFECTIVEDATE,
                            expiryDate = (DateTime)d.EXPIRYDATE,
                            canReRouteBooking = canReRouteBooking,
                            //isTemporaryOverdraft = p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault() != null ? p.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault().ISTEMPORARYOVERDRAFT : false,

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
                            productClassId = p.PRODUCTCLASSID,
                            productClassProcessId = p.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID,

                            misCode = m.MISCODE,
                            teamMisCode = m.TEAMMISCODE,

                            interestRate = d.APPROVEDINTERESTRATE,
                            submittedForAppraisal = m.SUBMITTEDFORAPPRAISAL,
                            approvedAmount = d.APPROVEDAMOUNT,
                            groupApprovedAmount = m.APPROVEDAMOUNT,
                            availmentDate = m.AVAILMENTDATE,
                            isBidbond = d.TBL_PRODUCT.PRODUCTCLASSID == (short)ProductClassEnum.BondAndGuarantees ? true : false,
                            isOverdraft = d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan ? true : false,
                            repaymentTerms = d.REPAYMENTTERMS,
                            repaymentSchedule = d.REPAYMENTSCHEDULE,
                            approvedTenor = d.APPROVEDTENOR,
                            createdBy = m.CREATEDBY,
                            applicationDate = m.APPLICATIONDATE,
                            dateTimeCreated = d.DATETIMECREATED,
                            loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0,
                        }).ToList();


            foreach (var item in data)
            {
                item.isBooked = context.TBL_LOAN.Where(x => x.LOAN_BOOKING_REQUESTID == item.loanBookingRequestId).Any();

                int operationId = 0;
                if (item.productTypeId == (short)LoanProductTypeEnum.ContingentLiability)
                    operationId = (short)OperationsEnum.ContigentLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.ForeignXRevolving)
                    operationId = (short)OperationsEnum.ForeignExchangeLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.CommercialLoan)
                    operationId = (short)OperationsEnum.CommercialLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.TermLoan || item.productTypeId == (short)LoanProductTypeEnum.SelfLiquidating)
                    operationId = (short)OperationsEnum.TermLoanBooking;
                if (item.productTypeId == (short)LoanProductTypeEnum.RevolvingLoan )
                    operationId = (short)OperationsEnum.RevolvingLoanBooking;

                item.operationId = operationId;

                var trailInfo = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == item.loanBookingRequestId);
                if (trailInfo.Any())
                {
                    item.isUnderApproval = true;
                    foreach (var trail in trailInfo)
                    {
                        if (trail.TOSTAFFID == staffId && trail.RESPONSESTAFFID == null)
                        {
                            item.isApprovalOwner = true;
                        }
                    }
                }

                var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                

                var productBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == item.productId).FirstOrDefault();
                if (productBehaviour != null)
                    item.isTemporaryOverdraft = productBehaviour.ISTEMPORARYOVERDRAFT;

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
                    case (short)LoanProductTypeEnum.RevolvingLoan:
                        decimal overdraftBal = 0;
                        foreach (var overdraft in overdrafts)
                        {
                            if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                        break;
                    case (short)LoanProductTypeEnum.ContingentLiability:
                        decimal contingentBal = 0;
                        foreach (var contingent in contingents)
                        {
                            if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                        }
                        item.customerAvailableAmount = item.approvedAmount - contingentBal;
                        break;
                }
                //var companyInformation = (from a in context.TBL_CUSTOMER_COMPANYINFOMATION
                //                          where a.CUSTOMERID == item.customerId
                //                          select new CustomerCompanyInfomationViewModels
                //                          {
                //                              annualTurnOver = a.ANNUALTURNOVER,
                //                              authorizedCapital = a.AUTHORISEDCAPITAL,
                //                              companyName = a.COMPANYNAME,
                //                              companyEmail = a.COMPANYEMAIL,
                //                              companyWebsite = a.COMPANYWEBSITE,
                //                              corporateBusinessCategory = a.CORPORATEBUSINESSCATEGORY,
                //                              paidUpCapital = a.PAIDUPCAPITAL,
                //                              creditRating = a.TBL_CUSTOMER.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                //                              previousCreditRating = "",
                //                          }).FirstOrDefault();
                //if (companyInformation != null)
                //{
                //    item.companyInformation = companyInformation;
                //    var companyDirectors = (from b in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                //                            where b.CUSTOMERID == item.customerId && ((b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember) || (b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember_Shareholder))
                //                            select new CustomerCompanyDirectorsViewModels
                //                            {
                //                                numberOfShares = b.SHAREHOLDINGPERCENTAGE,
                //                                companyDirectorTypeName = b.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                //                                fullname = b.FIRSTNAME + " " + b.SURNAME,
                //                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                //                            }).ToList();

                //    var companyShareholders = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                //                               where e.CUSTOMERID == item.customerId && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder
                //                               select new CustomerCompanyShareholdersViewModels
                //                               {
                //                                   numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                //                                   companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                //                                   fullname = e.FIRSTNAME + " " + e.SURNAME,
                //                                   isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                //                               }).ToList();

                //    var companyAccountSignatories = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                //                                     where e.CUSTOMERID == item.customerId && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory
                //                                     select new CustomerCompanyAccountSignatoryViewModels
                //                                     {
                //                                         numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                //                                         companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                //                                         fullname = e.FIRSTNAME + " " + e.SURNAME,
                //                                         isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                //                                     }).ToList();

                //    if (item.companyInformation != null)
                //    {
                //        item.companyInformation.companyDirectors = companyDirectors;
                //        item.companyInformation.companyShareholders = companyShareholders;
                //        item.companyInformation.companyAccountSignatories = companyAccountSignatories;
                //    }
                //}
            }
            return data.ToList();
        }

        public IEnumerable<CustomerCompanyInfomationViewModels> getLoanCustomerCompanyInformation(int customerId)
        {
            var companyInformation = (from a in context.TBL_CUSTOMER_COMPANYINFOMATION
                                      where a.CUSTOMERID == customerId
                                      select new CustomerCompanyInfomationViewModels
                                      {
                                          annualTurnOver = a.ANNUALTURNOVER,
                                          authorizedCapital = a.AUTHORISEDCAPITAL,
                                          companyName = a.COMPANYNAME,
                                          companyEmail = a.COMPANYEMAIL,
                                          companyWebsite = a.COMPANYWEBSITE,
                                          // corporateBusinessCategory = a.CORPORATEBUSINESSCATEGORY,
                                          paidUpCapital = a.PAIDUPCAPITAL,
                                          //creditRating = a.TBL_CUSTOMER.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                                          previousCreditRating = "",
                                          //companyDirectors = (from b in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                          //                    where b.CUSTOMERID == a.CUSTOMERID
                                          //                      && ((b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                                          //                      || (b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember_Shareholder))
                                          //                    select new CustomerCompanyDirectorsViewModels
                                          //                    {
                                          //                        numberOfShares = b.SHAREHOLDINGPERCENTAGE,
                                          //                        companyDirectorTypeName = b.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                          //                        fullname = b.FIRSTNAME + " " + b.SURNAME,
                                          //                        isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                          //                    }).ToList(),
                                          //companyShareholders = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                          //                       where e.CUSTOMERID == a.CUSTOMERID
                                          //                       && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder
                                          //                       select new CustomerCompanyShareholdersViewModels
                                          //                       {
                                          //                           numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                                          //                           companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                          //                           fullname = e.FIRSTNAME + " " + e.SURNAME,
                                          //                           isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                                          //                       }).ToList(),
                                          //companyAccountSignatories = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                          //                             where e.CUSTOMERID == a.CUSTOMERID
                                          //                             && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory
                                          //                             select new CustomerCompanyAccountSignatoryViewModels
                                          //                             {
                                          //                                 numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                                          //                                 companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                          //                                 fullname = e.FIRSTNAME + " " + e.SURNAME,
                                          //                                 isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                                          //                             }).ToList(),
                                      });

            return companyInformation;
        }

        public List<CasaViewModel> GetLoanCustomerAccounts(int customerId, int loanApplicationDetailId)
        {
            var customerAccounts = (from k in context.TBL_CASA
                                    where k.DELETED == false
                                    && k.CUSTOMERID == customerId
                                    select (
                     new CasaViewModel
                     {
                         productAccountNumber = k.PRODUCTACCOUNTNUMBER,
                         productAccountName = k.PRODUCTACCOUNTNAME,
                         casaAccountId = k.CASAACCOUNTID,
                         currencyId = k.CURRENCYID,
                         customerCode = k.TBL_CURRENCY.CURRENCYCODE,
                         customerName = k.TBL_CURRENCY.CURRENCYNAME,
                         productId = k.PRODUCTID,
                         customerId = k.CUSTOMERID,
                         isCurrentAccount = k.ISCURRENTACCOUNT,
                         tenor = (int)k.TENOR,
                     })).ToList();
            return customerAccounts;
        }

        public List<ProductViewModel> GetLoanCommercialLoans(int companyId)
        {
            var commercialLoans = (from k in context.TBL_PRODUCT
                                   where k.DELETED == false && k.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                   && k.COMPANYID == companyId
                                    select (
                                    new ProductViewModel
                                    {
                                        productId = k.PRODUCTID,
                                        productName = k.PRODUCTNAME
                                    })
                                    ).ToList();
            return commercialLoans;
        }

        public List<loanApplicationColateralViewModel> GetLoanApplicationCollateralsByApplicationId(int loanApplicationId)
        {
            var loanCollateral = (from cm in context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Where(x => x.DELETED == false)
                                  select (
                                           new loanApplicationColateralViewModel
                                           {
                                               legalFeeTaken = cm.LEGAL_FEE_TAKEN.HasValue ? (bool)cm.LEGAL_FEE_TAKEN : false,
                                               legalFeeDate = (DateTime)cm.LEGAL_FEE_DATE,
                                               collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                               + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME + ")",
                                               collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                               collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                               hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                               valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE.HasValue ? (decimal)cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE : 0,
                                               currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                               currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                               currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME,
                                               loanApplicationCollateralId = cm.LOANAPPCOLLATERALID,
                                               loanApplicationId = cm.LOANAPPLICATIONID,
                                               legalFeeAmount = cm.LEGAL_FEE_AMOUNT.HasValue ? (decimal)cm.LEGAL_FEE_AMOUNT : 0

                                           })).ToList();
            return loanCollateral;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetLoanApplicationDetails(int loanApplicationDetailId, int companyId)
        {
            var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                        join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                        join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                        where m.COMPANYID == companyId && d.DELETED == false && d.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                        select new CamProcessedLoanViewModel
                        {
                            approvalStatusId = (short)m.APPROVALSTATUSID,
                            loanApplicationId = m.LOANAPPLICATIONID,
                            loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                            applicationStatusId = m.APPLICATIONSTATUSID,
                            customerId = m.CUSTOMERID ?? 0,
                            customerCode = cust.CUSTOMERCODE,
                            customerName = m.CUSTOMERID.HasValue ? m.TBL_CUSTOMER.FIRSTNAME + " " + m.TBL_CUSTOMER.MIDDLENAME + " " + m.TBL_CUSTOMER.LASTNAME : "",
                            isRelatedParty = m.ISRELATEDPARTY,
                            customerGroupId = m.CUSTOMERGROUPID.HasValue ? m.CUSTOMERGROUPID : 0,
                            customerGroupName = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerGroupCode = m.CUSTOMERGROUPID.HasValue ? m.TBL_CUSTOMER_GROUP.GROUPCODE : "",
                            customerSensitivityLevelId = d.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                            customerOccupation = d.TBL_CUSTOMER.OCCUPATION,
                            customerType = d.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            isPoliticallyExposed = d.TBL_CUSTOMER.ISPOLITICALLYEXPOSED,
                            isInvestmentGrade = m.ISINVESTMENTGRADE,

                            customerAccounts = (from k in context.TBL_CASA
                                                where k.DELETED == false
                                                && k.CUSTOMERID == d.CUSTOMERID
                                                select (
                                 new CasaViewModel
                                 {
                                     productAccountNumber = k.PRODUCTACCOUNTNUMBER,
                                     productAccountName = k.PRODUCTACCOUNTNAME,
                                     casaAccountId = k.CASAACCOUNTID,
                                     currencyId = k.CURRENCYID,
                                     productId = k.PRODUCTID,
                                     customerId = k.CUSTOMERID,
                                     isCurrentAccount = k.ISCURRENTACCOUNT,
                                     tenor = (int)k.TENOR,
                                 })).ToList(),
                            loanInformation = m.LOANINFORMATION,
                            companyInformation = (from a in context.TBL_CUSTOMER_COMPANYINFOMATION
                                                  where a.CUSTOMERID == d.CUSTOMERID
                                                  select new CustomerCompanyInfomationViewModels
                                                  {
                                                      annualTurnOver = a.ANNUALTURNOVER,
                                                      authorizedCapital = a.AUTHORISEDCAPITAL,
                                                      companyName = a.COMPANYNAME,
                                                      companyEmail = a.COMPANYEMAIL,
                                                      companyWebsite = a.COMPANYWEBSITE,
                                                      corporateBusinessCategory = a.CORPORATEBUSINESSCATEGORY,
                                                      paidUpCapital = a.PAIDUPCAPITAL,
                                                      creditRating = a.TBL_CUSTOMER.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                                                      previousCreditRating = "",
                                                      companyDirectors = (from b in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                                                          where b.CUSTOMERID == a.CUSTOMERID
                          && ((b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                                    || (b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember_Shareholder))
                                                                          select new CustomerCompanyDirectorsViewModels
                                                                          {
                                                                              numberOfShares = b.SHAREHOLDINGPERCENTAGE,
                                                                              companyDirectorTypeName = b.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                                                              fullname = b.FIRSTNAME + " " + b.SURNAME,
                                                                              isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                                                          }).ToList(),
                                                      companyShareholders = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                                                             where e.CUSTOMERID == a.CUSTOMERID
                                                                             && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder
                                                                             select new CustomerCompanyShareholdersViewModels
                                                                             {
                                                                                 numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                                                                                 companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                                                                 fullname = e.FIRSTNAME + " " + e.SURNAME,
                                                                                 isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                                                                             }).ToList(),
                                                      companyAccountSignatories = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                                                                   where e.CUSTOMERID == a.CUSTOMERID
                                                                                   && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory
                                                                                   select new CustomerCompanyAccountSignatoryViewModels
                                                                                   {
                                                                                       numberOfShares = e.SHAREHOLDINGPERCENTAGE,
                                                                                       companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                                                                       fullname = e.FIRSTNAME + " " + e.SURNAME,
                                                                                       isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                                                                                   }).ToList(),
                                                  }).FirstOrDefault(),
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

                            misCode = m.MISCODE,
                            teamMisCode = m.TEAMMISCODE,

                            interestRate = d.APPROVEDINTERESTRATE,
                            submittedForAppraisal = m.SUBMITTEDFORAPPRAISAL,
                            approvedAmount = d.APPROVEDAMOUNT,
                            groupApprovedAmount = m.APPROVEDAMOUNT,

                            customerAvailableAmount = (d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.TermLoan
                                                      || d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                                      || d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating)
                             ? (d.APPROVEDAMOUNT - d.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.PRINCIPALAMOUNT)) :
                                ((d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                                        ? (d.APPROVEDAMOUNT - d.TBL_LOAN_REVOLVING
                                            .Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.OVERDRAFTLIMIT)) :
                                  (d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability
                                        ? (d.APPROVEDAMOUNT - d.TBL_LOAN_CONTINGENT
                                            .Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.CONTINGENTAMOUNT)) :
                                            0)
                                ),
                            approvedTenor = d.APPROVEDTENOR,
                            createdBy = m.CREATEDBY,
                            newApplicationDate = m.APPLICATIONDATE,
                            dateTimeCreated = d.DATETIMECREATED,
                            loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0,
                            loanCollateral = (from cm in context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == m.LOANAPPLICATIONID) // ------------------ REFACTOR TO LOANID!
                                              select (new LoanCollateralMappingViewModel
                                              {
                                                  collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME
                                                           + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME + ")",
                                                  collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                                  loanApplicationId = cm.LOANAPPLICATIONID,
                                                  collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                  hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                  valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                  currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                                  currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                  currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME
                                              })).ToList()

                        }).ToList();

            return data;
        }

        /// <summary>
        /// Bookeds the loan.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        private IEnumerable<LoanViewModel> BookedLoan(int companyId)
        {
            return GetAllLoans().Where(x => x.companyId == companyId).OrderByDescending(x => x.loanId);
        }

        /// <summary>
        /// Gets the booked loan details.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId)
        {
            var loans = BookedLoan(companyId);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                // loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                //loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        /// <summary>
        /// Gets the booked loan details by loan reference number.
        /// </summary>
        /// <param name="loanReferenceNumber">The loan reference number.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x => x.loanReferenceNumber == loanReferenceNumber);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                //loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                // loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        /// <summary>
        /// Gets the booked loan details by customer code.
        /// </summary>
        /// <param name="customerCode">The customer code.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x => x.customerCode == customerCode);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                // loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                //loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }
            return loans;
        }
        public List<LoanCAMSOLViewModel> GetCurrentCamsolByCustomer(List<CustomerExposure> customer, int companyId)
        {
            List<LoanCAMSOLViewModel> camsol = new List<LoanCAMSOLViewModel>();

            foreach (var item in customer)
            {
                var customercode = context.TBL_CUSTOMER.Find(item.customerId).CUSTOMERCODE;
                camsol = (from cam in context.TBL_LOAN_CAMSOL
                          join c in context.TBL_LOAN_CAMSOL_TYPE on cam.CAMSOLTYPEID equals c.CAMSOLTYPEID
                          where cam.CUSTOMERCODE == customercode
                          select new LoanCAMSOLViewModel
                          {
                              accountname = cam.ACCOUNTNAME,
                              accountnumber = cam.ACCOUNTNUMBER,
                              balance = cam.BALANCE,
                              camsoltypeid = cam.CAMSOLTYPEID,
                              cantakeloan = cam.CANTAKELOAN,
                              customercode = cam.CUSTOMERCODE,
                              customername = cam.CUSTOMERNAME,
                              date = cam.DATE,
                              camsolType = c.CAMSOLTYPENAME,
                              loansystemtype = context.TBL_LOAN_SYSTEM_TYPE.Where(x => x.LOANSYSTEMTYPEID == cam.LOANSYSTEMTYPEID).Select(x => x.LOANSYSTEMTYPENAME).FirstOrDefault(),
                              interestinsuspense = cam.INTERESTINSUSPENSE,
                              loancamsolid = cam.LOAN_CAMSOLID,
                              loanid = cam.LOANID,
                              principal = cam.PRINCIPAL,
                              remark = cam.REMARK,
                          }).ToList();
            }                
            return camsol;
        }
        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int companyId)
        {
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            

            foreach (var item in customer)
            {
                var customCode = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == item.customerId).Select(x => x.CUSTOMERCODE).FirstOrDefault();

                exposure = from a in context.TBL_LOAN
                           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           select new CurrentCustomerExposure
                           {
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = a.PRINCIPALAMOUNT,
                               proposedLimit = a.OUTSTANDINGPRINCIPAL,
                               recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                               PastDueObligationsInterest = a.PASTDUEINTEREST,
                               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                               loanStatus = "Running",
                               referenceNumber =a.LOANREFERENCENUMBER
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                exposure = from a in context.TBL_LOAN_REVOLVING
                           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           select new CurrentCustomerExposure
                           {
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = a.OVERDRAFTLIMIT,
                               proposedLimit = a.OVERDRAFTLIMIT,
                               recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                               PastDueObligationsInterest = a.PASTDUEINTEREST,
                               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                               loanStatus = "Running",
                               referenceNumber = a.LOANREFERENCENUMBER
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                           where a.CUSTOMERID == item.customerId && a.TBL_LOAN_APPLICATION.COMPANYID == companyId && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                           select new CurrentCustomerExposure
                           {
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = 0,
                               proposedLimit = a.PROPOSEDAMOUNT,
                               recommendedLimit = a.APPROVEDAMOUNT,
                               PastDueObligationsInterest = 0,
                               PastDueObligationsPrincipal = 0,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = "Processing",
                               loanStatus = "Processing",
                              // referenceNumber = a.
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);


                var staggingLoan = from a in stgCon.STG_LOAN_MART
                           where a.CUST_ID == customCode
                           select new CurrentCustomerExposure
                           {
                               facilityType = a.SCHM_TYPE,
                               existingLimit = a.FAC_GRANT_AMT,
                               proposedLimit = a.FINAL_BALANCE,
                               recommendedLimit = 0,
                               PastDueObligationsInterest = a.INT_DUE,
                               PastDueObligationsPrincipal = 0,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.USER_CLASSIFICATION == "1" ? "Performing" : "Non-Performing",
                               loanStatus = "Running"
                           };

                exposures.Union(staggingLoan);

            }

            exposures.Add(new CurrentCustomerExposure
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposure.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                PastDueObligationsPrincipal = exposures.Sum(t => t.PastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
            });

            return exposures;
        }

        public List<CurrentCustomerExposure> GetApplicationFacilitySummary(int applicationId)
        {
            var customers = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONID == applicationId && x.DELETED == false)
                .Select(x => new { CUSTOMERID = x.CUSTOMERID })
                .Distinct()
                .ToList();

            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            foreach (var item in customers)
            {
                exposure = from a in context.TBL_LOAN
                           where a.CUSTOMERID == item.CUSTOMERID && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           select new CurrentCustomerExposure
                           {
                               loanId = a.TERMLOANID,
                               productTypeId = 1,
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = a.PRINCIPALAMOUNT,
                               proposedLimit = a.OUTSTANDINGPRINCIPAL,
                               recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                               PastDueObligationsInterest = a.PASTDUEINTEREST,
                               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                               loanStatus = "Running"
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                exposure = from a in context.TBL_LOAN_REVOLVING
                           where a.CUSTOMERID == item.CUSTOMERID && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           select new CurrentCustomerExposure
                           {
                               loanId = a.REVOLVINGLOANID,
                               productTypeId = 2,
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = a.OVERDRAFTLIMIT,
                               proposedLimit = a.OVERDRAFTLIMIT,
                               recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                               PastDueObligationsInterest = a.PASTDUEINTEREST,
                               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                               loanStatus = "Running"
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                           where a.CUSTOMERID == item.CUSTOMERID  && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                           select new CurrentCustomerExposure
                           {
                               loanId = 0,
                               productTypeId = 0,
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = 0,
                               proposedLimit = a.PROPOSEDAMOUNT,
                               recommendedLimit = a.APPROVEDAMOUNT,
                               PastDueObligationsInterest = 0,
                               PastDueObligationsPrincipal = 0,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = "Processing",
                               loanStatus = "Processing"
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);
            }

            exposures.Add(new CurrentCustomerExposure
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposure.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                PastDueObligationsPrincipal = exposures.Sum(t => t.PastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
            });

            return exposures;
        }
        /// <summary>
        /// Searches for loan.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public IQueryable<LoanViewModel> SearchForLoan(string searchQuery)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            try
            {
                IQueryable<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }
                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    var loans = (from a in context.TBL_LOAN
                                 join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                 join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                 where a.ISDISBURSED == true && a.MATURITYDATE >= DbFunctions.TruncateTime(applicationDate) && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                                 select new LoanViewModel
                                 {
                                     loanId = a.TERMLOANID,
                                     customerId = a.CUSTOMERID,
                                     customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                     firstName = b.FIRSTNAME,
                                     lastName = b.LASTNAME,
                                     customerCode = b.CUSTOMERCODE,
                                     productAccountName = c.PRODUCTACCOUNTNAME,
                                     loanReferenceNumber = a.LOANREFERENCENUMBER,
                                     principalAmount = a.PRINCIPALAMOUNT,

                                     //applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                     //principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                     //pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                     //interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                     //interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                     //productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                     //productName = a.TBL_PRODUCT.PRODUCTNAME,
                                     //productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                     //principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                     //interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                     //relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                     //relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                     //relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                     //relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                                     //misCode = a.MISCODE,
                                     //teamMiscode = a.TEAMMISCODE,
                                     //interestRate = a.INTERESTRATE,
                                     //effectiveDate = a.EFFECTIVEDATE,
                                     //maturityDate = a.MATURITYDATE,
                                     //bookingDate = a.BOOKINGDATE,

                                     //principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                     //interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                     //approvalStatusId = a.APPROVALSTATUSID,
                                     //approvedBy = a.APPROVEDBY,
                                     //approverComment = a.APPROVERCOMMENT,
                                     //dateApproved = a.DATEAPPROVED,
                                     //loanStatusId = a.LOANSTATUSID,
                                     //scheduleTypeId = a.SCHEDULETYPEID,
                                     //scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                     //isDisbursed = a.ISDISBURSED,
                                     //disbursedBy = a.DISBURSEDBY,
                                     //disburserComment = a.DISBURSERCOMMENT,
                                     //disburseDate = a.DISBURSEDATE,
                                     //approvedAmount = a.ApprovedAmount,
                                     //operationId = a.OPERATIONID,
                                     //operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                     //subSectorName = a.TBL_SUB_SECTOR.NAME,
                                     //sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                     //casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                     //customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                     //loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                     //loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                     //equityContribution = a.EQUITYCONTRIBUTION,
                                     //firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                     //firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                     //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                     //outstandingInterest = a.OUTSTANDINGINTEREST,
                                     //principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                     //principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                     //fixedPrincipal = a.FIXEDPRINCIPAL,
                                     //profileLoan = a.PROFILELOAN,
                                     //dischargeLetter = a.DISCHARGELETTER,
                                     //suspendInterest = a.SUSPENDINTEREST,
                                     //customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                     //createdBy = a.CREATEDBY,
                                     //dateTimeCreated = a.DATETIMECREATED,
                                     //isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                     //exchangeRate = a.EXCHANGERATE,
                                     //currencyId = a.CURRENCYID,
                                     //currency = a.TBL_CURRENCY.CURRENCYNAME
                                 });
                    allFilteredLoan = loans.Where(x => x.loanReferenceNumber.Contains(searchQuery) || x.customerCode.ToLower().Contains(searchQuery) ||
                                       x.firstName.ToLower().Contains(searchQuery) ||
                                       x.lastName.ToLower().Contains(searchQuery) ||
                                       x.productAccountName.ToLower().Contains(searchQuery)).Take(10).AsQueryable();
                }
                return allFilteredLoan;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public IQueryable<LoanViewModel> SearchForFXRevolvingLoan(string searchQuery)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            try
            {
                IQueryable<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }
                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    var loans = (from a in context.TBL_LOAN
                                 join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                 join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                 where a.ISDISBURSED == true //&& a.MATURITYDATE >= DbFunctions.TruncateTime(applicationDate)
                                 && a.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving
                                 select new LoanViewModel
                                 {
                                     loanId = a.TERMLOANID,
                                     customerId = a.CUSTOMERID,
                                     customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                     firstName = b.FIRSTNAME,
                                     lastName = b.LASTNAME,
                                     customerCode = b.CUSTOMERCODE,
                                     productAccountName = c.PRODUCTACCOUNTNAME,
                                     loanReferenceNumber = a.LOANREFERENCENUMBER,
                                     principalAmount = a.PRINCIPALAMOUNT,
                                     nostroAccount = a.NOSTROACCOUNTID,
                                     currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                                     currency = a.TBL_CURRENCY.CURRENCYNAME,
                                     productName = a.TBL_PRODUCT.PRODUCTNAME,
                                     productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                     casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                     productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                     //applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                     //principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                     //pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                     //interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                     //interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                     //productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                     //productName = a.TBL_PRODUCT.PRODUCTNAME,
                                     //productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                     //principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                     //interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                     //relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                     //relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                     //relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                     //relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                                     //misCode = a.MISCODE,
                                     //teamMiscode = a.TEAMMISCODE,
                                     //interestRate = a.INTERESTRATE,
                                     //effectiveDate = a.EFFECTIVEDATE,
                                     //maturityDate = a.MATURITYDATE,
                                     //bookingDate = a.BOOKINGDATE,

                                     //principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                     //interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                     //approvalStatusId = a.APPROVALSTATUSID,
                                     //approvedBy = a.APPROVEDBY,
                                     //approverComment = a.APPROVERCOMMENT,
                                     //dateApproved = a.DATEAPPROVED,
                                     //loanStatusId = a.LOANSTATUSID,
                                     //scheduleTypeId = a.SCHEDULETYPEID,
                                     //scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                     //isDisbursed = a.ISDISBURSED,
                                     //disbursedBy = a.DISBURSEDBY,
                                     //disburserComment = a.DISBURSERCOMMENT,
                                     //disburseDate = a.DISBURSEDATE,
                                     //approvedAmount = a.ApprovedAmount,
                                     //operationId = a.OPERATIONID,
                                     //operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                     //subSectorName = a.TBL_SUB_SECTOR.NAME,
                                     //sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                     //casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                     //customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                     //loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                     //loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                     //equityContribution = a.EQUITYCONTRIBUTION,
                                     //firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                     //firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                     //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                     //outstandingInterest = a.OUTSTANDINGINTEREST,
                                     //principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                     //principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                     //fixedPrincipal = a.FIXEDPRINCIPAL,
                                     //profileLoan = a.PROFILELOAN,
                                     //dischargeLetter = a.DISCHARGELETTER,
                                     //suspendInterest = a.SUSPENDINTEREST,
                                     //customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                     //createdBy = a.CREATEDBY,
                                     //dateTimeCreated = a.DATETIMECREATED,
                                     //isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                     //exchangeRate = a.EXCHANGERATE,
                                     //currencyId = a.CURRENCYID,
                                     //currency = a.TBL_CURRENCY.CURRENCYNAME
                                 });
                    allFilteredLoan = loans.Where(x => x.loanReferenceNumber.Contains(searchQuery) || x.customerCode.ToLower().Contains(searchQuery) ||
                                       x.firstName.ToLower().Contains(searchQuery) ||
                                       x.lastName.ToLower().Contains(searchQuery) ||
                                       x.currencyCode.ToLower().Contains(searchQuery) ||
                                       x.casaAccountNumber.ToLower().Contains(searchQuery) ||
                                       x.nostroAccount.ToLower().Contains(searchQuery) ||
                                       x.productAccountName.ToLower().Contains(searchQuery)).Take(10).AsQueryable();
                }
                return allFilteredLoan;
            }
            catch (System.Exception ex)
            {
                return null;
            }
        }

        public IQueryable<LoanViewModel> SearchRunningCommercialAndFXLoans(string searchQuery)
        {
            var applicationDate = generalSetup.GetApplicationDate();
            try
            {
                IQueryable<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allFilteredLoan = (from a in context.TBL_LOAN
                                       join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                       join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                       where a.ISDISBURSED == true 
                                       && ((a.OPERATIONID == (short)OperationsEnum.CommercialLoanBooking) || (a.OPERATIONID == (short)OperationsEnum.ForeignExchangeLoanBooking))
                                       && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      // && a.MATURITYDATE >= DbFunctions.TruncateTime(applicationDate) 
                                       && ((a.LOANREFERENCENUMBER.Contains(searchQuery)) ||
                                            (b.CUSTOMERCODE.ToLower().Contains(searchQuery)) ||
                                            (b.FIRSTNAME.ToLower().Contains(searchQuery)) ||
                                            (b.LASTNAME.ToLower().Contains(searchQuery)) ||
                                            (c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery)))
                                       select new LoanViewModel
                                       {
                                           loanId = a.TERMLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                                           productId = a.PRODUCTID,
                                           companyId = a.COMPANYID,
                                           casaAccountId = a.CASAACCOUNTID,
                                           branchId = a.BRANCHID,
                                           branchName = a.TBL_BRANCH.BRANCHNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                           pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                           interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                           interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                           productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                           productName = a.TBL_PRODUCT.PRODUCTNAME,
                                           productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                           principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                           interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,

                                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                                           loanSystemTypeId = (short)a.LOANSYSTEMTYPEID,
                                           misCode = a.MISCODE,
                                           teamMiscode = a.TEAMMISCODE,
                                           interestRate = a.INTERESTRATE,
                                           effectiveDate = a.EFFECTIVEDATE,
                                           maturityDate = a.MATURITYDATE,
                                           systemCurrentDate = applicationDate,
                                           //tenorUsed = (applicationDate - a.EFFECTIVEDATE).Days,
                                           //tenorLeft = (a.MATURITYDATE - applicationDate).Days,
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
                                           disbursedBy = a.DISBURSEDBY,
                                           disburserComment = a.DISBURSERCOMMENT,
                                           disburseDate = a.DISBURSEDATE,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OPERATIONID,
                                           // operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                           customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                           loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                       }).Take(10).AsQueryable();
                }
               // return allFilteredLoan;
                return allFilteredLoan.Where(x => x.operationId == (short)OperationsEnum.CommercialLoanBooking);
            }
            catch (System.Exception)
            {
                return null;
            }
        }


        public IEnumerable<LoanViewModel> GetLoanReviewApplicationOverDraft()
        {
            try
            {
                var currentDate = generalSetup.GetApplicationDate();
                var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.REVOLVINGLOANID equals b.LOANID
                                       join e in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                       where a.ISDISBURSED == true && b.TBL_OPERATIONS.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft && //(int)LoanSystemTypeEnum.OverdraftFacility &&
                                       e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && b.OPERATIONPERFORMED == false
                                       //orderby b.DATECREATED descending
                                       select new LoanViewModel
                                       {
                                           loanId = a.REVOLVINGLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                           interestRate = a.INTERESTRATE,
                                           principalAmount = a.OVERDRAFTLIMIT,
                                           effectiveDate = a.EFFECTIVEDATE,
                                           maturityDate = a.MATURITYDATE,
                                           operationId = b.OPERATIONID,
                                           operationName = b.TBL_OPERATIONS.OPERATIONNAME,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                           systemCurrentDate = currentDate,
                                           lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,

                                       }).ToList();
                return allFilteredLoan;
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public LoanViewModel GetOverdraftDetailsByLoanId(int revolvingLoanId)
        {
            try
            {
                decimal overDraftLimit = 0;
                decimal availableBalance = 0;
                var odDetail = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == revolvingLoanId);
                if (odDetail != null)
                {
                    overDraftLimit = odDetail.OVERDRAFTLIMIT;
                    availableBalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == odDetail.CASAACCOUNTID).AVAILABLEBALANCE;
                }
                //var overDraftDetail = (from a in context.TBL_LOAN_REVOLVING
                //                       join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                //                       join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                //                       where a.REVOLVINGLOANID == revolvingLoanId
                //                       select new LoanViewModel
                //                       {
                //                           loanId = a.REVOLVINGLOANID,
                //                           customerId = a.CUSTOMERID,
                //                           customerName = b.FIRSTNAME + " " + b.LASTNAME,
                //                           customerCode = b.CUSTOMERCODE,
                //                           productId = a.PRODUCTID,
                //                           companyId = a.COMPANYID,
                //                           casaAccountId = a.CASAACCOUNTID,
                //                           branchId = a.BRANCHID,
                //                           branchName = a.TBL_BRANCH.BRANCHNAME,
                //                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                //                           //applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                //                           //loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                //                           //productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                //                           //productName = a.TBL_PRODUCT.PRODUCTNAME,
                //                           //productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                //                           //relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                //                           //relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                //                           //relationshipManagerId = a.RELATIONSHIPMANAGERID,
                //                           //relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                //                           //misCode = a.MISCODE,
                //                           //teamMiscode = a.TEAMMISCODE,
                //                           //interestRate = a.INTERESTRATE,
                //                           //effectiveDate = a.EFFECTIVEDATE,
                //                           //maturityDate = a.MATURITYDATE,
                //                           //bookingDate = a.BOOKINGDATE,
                //                           //principalAmount = a.OVERDRAFTLIMIT,
                //                           //approvalStatusId = a.APPROVALSTATUSID,
                //                           //approverComment = a.APPROVERCOMMENT,
                //                           //dateApproved = a.DATEAPPROVED,
                //                           //loanStatusId = a.LOANSTATUSID,
                //                           //isDisbursed = a.ISDISBURSED,
                //                           //disburserComment = a.DISBURSERCOMMENT,
                //                           //disburseDate = a.DISBURSEDATE,
                //                           //operationId = a.OPERATIONID,
                //                           //operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                //                           //subSectorName = a.TBL_SUB_SECTOR.NAME,
                //                           //sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                //                           //casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                //                           //productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                //                           //customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                //                           //loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                //                           //loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                //                           outstandingPrincipal = availableBalance,
                //                           dischargeLetter = a.DISCHARGELETTER,
                //                           suspendInterest = a.SUSPENDINTEREST,
                //                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                //                           createdBy = a.CREATEDBY,
                //                           dateTimeCreated = a.DATETIMECREATED,
                //                           exchangeRate = a.EXCHANGERATE,
                //                           currencyId = a.CURRENCYID,
                //                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                //                       }).FirstOrDefault();

                var overDraftDetail = (from a in context.TBL_LOAN_REVOLVING
                                       join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                                       join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                       join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                       join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                       join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                       join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                       join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                                       join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                                       join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                                       where a.REVOLVINGLOANID == revolvingLoanId
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
                                           outstandingPrincipal = availableBalance,
                                           dischargeLetter = a.DISCHARGELETTER,
                                           suspendInterest = a.SUSPENDINTEREST,
                                           customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                       }).FirstOrDefault();
                if (availableBalance > 0)
                {
                    overDraftDetail.overDraft = overDraftLimit;
                }
                else
                {
                    overDraftDetail.overDraft = overDraftLimit - Math.Abs(availableBalance);
                }
                return overDraftDetail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

       

        public IEnumerable<LoanViewModel> GetApprovedLoanReviewRemedial()
        {
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                var allFilteredLoan = (from a in context.TBL_LOAN
                                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals b.LOANID
                                       join e in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                       //join d in context.TBL_LOAN_SCHEDULE_DAILY on a.TERMLOANID equals d.LOANID
                                       //join f in context.TBL_LOAN_CAMSOL on a.TERMLOANID equals f.LOANID
                                       where a.ISDISBURSED == true && e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      && b.TBL_OPERATIONS.OPERATIONTYPEID == (int)OperationTypeEnum.Remedial
                                      && b.OPERATIONPERFORMED == false
                                      //&& d.DATE == DbFunctions.TruncateTime(applicationDate)
                                       //orderby b.DATECREATED descending
                                       select new LoanViewModel
                                       {

                                           loanId = a.TERMLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           productId = a.PRODUCTID,
                                           companyId = a.COMPANYID,
                                           casaAccountId = a.CASAACCOUNTID,
                                           branchId = a.BRANCHID,
                                           branchName = a.TBL_BRANCH.BRANCHNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                           pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                           interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                           interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                           productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                           productName = a.TBL_PRODUCT.PRODUCTNAME,
                                           productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                           principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                           interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,

                                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,

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
                                           disbursedBy = a.DISBURSEDBY,
                                           disburserComment = a.DISBURSERCOMMENT,
                                           disburseDate = a.DISBURSEDATE,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OPERATIONID,
                                           operationName = b.TBL_OPERATIONS.OPERATIONNAME, //context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                           customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                           loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                           loanReviewOperationTypeId = b.OPERATIONID,
                                           reviewDetails = b.REVIEWDETAILS,
                                           interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                           interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                           pastDueInterest = a.PASTDUEINTEREST,
                                           lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                           //accrualedAmount = context.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).ACCRUEDINTEREST
                                       }).ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the loan schedule by loan identifier.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanPaymentSchedulePeriodicViewModel> GetLoanScheduleByLoanId(int loanId)
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

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsWithParameters(int companyId, string param)
        {
            var loans = BookedLoan(companyId);
            // .Where(x => x.loanReferenceNumber == param || x.firstName.StartsWith(param) || x.lastName.StartsWith(param) || x.middleName.StartsWith(param ) || param==null || param=="undefined");
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                //loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                // loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public LoanViewModel GetDisbursedLoanByLoanId(int loanId)//GetDisbursedLoanByLoanId
        {
            LoanViewModel result;
            var data = this.context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
            if (data != null)
            {
                result = GetDisbursedODByODId(loanId);
            }
            else
            {
                result = GetDisbursedLoanByLoan(loanId);
            }
            return result;
        }


        public LoanViewModel GetDisbursedLoanByLoanId(int loanId, int loanType)//GetDisbursedLoanByLoanId
        {
            LoanViewModel result;

            if (loanType == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                result = GetDisbursedLoanByLoan(loanId);
                return result;
            }
            else if (loanType == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                result = GetDisbursedODByODId(loanId);
                return result;
            }
            else if (loanType == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                result = GetDisbursedContingentDId(loanId);
                return result;
            }

            return null;
        }

        public LoanViewModel GetDisbursedODByODId(int loanId)//GetDisbursedODByODId
        {

            //var loanDetails = (from a in context.TBL_LOAN_REVOLVING
            //                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
            //                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
            //                   where a.REVOLVINGLOANID == loanId && a.ISDISBURSED == true
            //                   select new LoanViewModel
            //                   {
            //                       loanId = a.REVOLVINGLOANID,
            //                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
            //                       customerId = a.CUSTOMERID,
            //                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
            //                       customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
            //                       productId = a.PRODUCTID,
            //                       companyId = a.COMPANYID,
            //                       casaAccountId = a.CASAACCOUNTID,
            //                       branchId = a.BRANCHID,
            //                       branchName = a.TBL_BRANCH.BRANCHNAME,
            //                       loanReferenceNumber = a.LOANREFERENCENUMBER,
            //                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
            //                       //principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
            //                       //pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
            //                       //interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
            //                       //interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
            //                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
            //                       productName = a.TBL_PRODUCT.PRODUCTNAME,
            //                       productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
            //                       //principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
            //                       //interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
            //                       relationshipOfficerId = a.RELATIONSHIPOFFICERID,
            //                       relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
            //                       relationshipManagerId = a.RELATIONSHIPMANAGERID,
            //                       relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
            //                       misCode = a.MISCODE,
            //                       teamMiscode = a.TEAMMISCODE,
            //                       interestRate = a.INTERESTRATE,
            //                       effectiveDate = a.EFFECTIVEDATE,
            //                       maturityDate = a.MATURITYDATE,
            //                       bookingDate = a.BOOKINGDATE,
            //                       principalAmount = a.OVERDRAFTLIMIT,
            //                       //principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
            //                       //interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
            //                       approvalStatusId = a.APPROVALSTATUSID,
            //                       //approvedBy = a.APPROVEDBY,
            //                       approverComment = a.APPROVERCOMMENT,
            //                       dateApproved = a.DATEAPPROVED,
            //                       loanStatusId = a.LOANSTATUSID,
            //                       //scheduleTypeId = a.SCHEDULETYPEID,
            //                       //scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
            //                       isDisbursed = a.ISDISBURSED,
            //                       //disbursedBy = a.DISBURSEDBY,
            //                       disburserComment = a.DISBURSERCOMMENT,
            //                       disburseDate = a.DISBURSEDATE,
            //                       operationId = a.OPERATIONID,
            //                       operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
            //                       subSectorName = a.TBL_SUB_SECTOR.NAME,
            //                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
            //                       casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
            //                       productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
            //                       customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
            //                       loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
            //                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            //                       //equityContribution = a.EQUITYCONTRIBUTION,
            //                       //firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
            //                       //firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
            //                       //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
            //                       //outstandingInterest = a.OUTSTANDINGINTEREST,
            //                       //principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
            //                       //principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
            //                       //fixedPrincipal = a.FIXEDPRINCIPAL,
            //                       //profileLoan = a.PROFILELOAN,
            //                       dischargeLetter = a.DISCHARGELETTER,
            //                       suspendInterest = a.SUSPENDINTEREST,
            //                       customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
            //                       createdBy = a.CREATEDBY,
            //                       dateTimeCreated = a.DATETIMECREATED,
            //                       isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.REVOLVINGLOANID).Any(),
            //                       exchangeRate = a.EXCHANGERATE,
            //                       currencyId = a.CURRENCYID,
            //                       currency = a.TBL_CURRENCY.CURRENCYNAME
            //                   }).FirstOrDefault();

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
                                       customerType = b.TBL_CUSTOMER_TYPE.NAME,
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
                                       //outstandingPrincipal = availableBalance,
                                       dischargeLetter = a.DISCHARGELETTER,
                                       suspendInterest = a.SUSPENDINTEREST,
                                       customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                       createdBy = a.CREATEDBY,
                                       dateTimeCreated = a.DATETIMECREATED,
                                       exchangeRate = a.EXCHANGERATE,
                                       currencyId = a.CURRENCYID,
                                       currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                                       currency = a.TBL_CURRENCY.CURRENCYNAME,
                                   }).FirstOrDefault();
            return loanDetails;
        }
        public LoanViewModel GetDisbursedLoanByLoan(int loanId)//GetDisbursedLoanByLoanId
        {
            var loanDetails = (from a in context.TBL_LOAN
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
                                   // scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
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
                                   // isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME,
                                   currencyCode = cur.CURRENCYCODE
                               }).FirstOrDefault();

            return loanDetails;

        }
        public LoanViewModel GetDisbursedContingentDId(int loanId)//GetDisbursedODByODId
        {

            //var loanDetails = (from a in context.TBL_LOAN_REVOLVING
            //                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
            //                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
            //                   where a.REVOLVINGLOANID == loanId && a.ISDISBURSED == true
            //                   select new LoanViewModel
            //                   {
            //                       loanId = a.REVOLVINGLOANID,
            //                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
            //                       customerId = a.CUSTOMERID,
            //                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
            //                       customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
            //                       productId = a.PRODUCTID,
            //                       companyId = a.COMPANYID,
            //                       casaAccountId = a.CASAACCOUNTID,
            //                       branchId = a.BRANCHID,
            //                       branchName = a.TBL_BRANCH.BRANCHNAME,
            //                       loanReferenceNumber = a.LOANREFERENCENUMBER,
            //                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
            //                       //principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
            //                       //pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
            //                       //interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
            //                       //interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
            //                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
            //                       productName = a.TBL_PRODUCT.PRODUCTNAME,
            //                       productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
            //                       //principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
            //                       //interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
            //                       relationshipOfficerId = a.RELATIONSHIPOFFICERID,
            //                       relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
            //                       relationshipManagerId = a.RELATIONSHIPMANAGERID,
            //                       relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
            //                       misCode = a.MISCODE,
            //                       teamMiscode = a.TEAMMISCODE,
            //                       interestRate = a.INTERESTRATE,
            //                       effectiveDate = a.EFFECTIVEDATE,
            //                       maturityDate = a.MATURITYDATE,
            //                       bookingDate = a.BOOKINGDATE,
            //                       principalAmount = a.OVERDRAFTLIMIT,
            //                       //principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
            //                       //interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
            //                       approvalStatusId = a.APPROVALSTATUSID,
            //                       //approvedBy = a.APPROVEDBY,
            //                       approverComment = a.APPROVERCOMMENT,
            //                       dateApproved = a.DATEAPPROVED,
            //                       loanStatusId = a.LOANSTATUSID,
            //                       //scheduleTypeId = a.SCHEDULETYPEID,
            //                       //scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
            //                       isDisbursed = a.ISDISBURSED,
            //                       //disbursedBy = a.DISBURSEDBY,
            //                       disburserComment = a.DISBURSERCOMMENT,
            //                       disburseDate = a.DISBURSEDATE,
            //                       operationId = a.OPERATIONID,
            //                       operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
            //                       subSectorName = a.TBL_SUB_SECTOR.NAME,
            //                       sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
            //                       casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
            //                       productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
            //                       customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
            //                       loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
            //                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            //                       //equityContribution = a.EQUITYCONTRIBUTION,
            //                       //firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
            //                       //firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
            //                       //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
            //                       //outstandingInterest = a.OUTSTANDINGINTEREST,
            //                       //principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
            //                       //principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
            //                       //fixedPrincipal = a.FIXEDPRINCIPAL,
            //                       //profileLoan = a.PROFILELOAN,
            //                       dischargeLetter = a.DISCHARGELETTER,
            //                       suspendInterest = a.SUSPENDINTEREST,
            //                       customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
            //                       createdBy = a.CREATEDBY,
            //                       dateTimeCreated = a.DATETIMECREATED,
            //                       isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.REVOLVINGLOANID).Any(),
            //                       exchangeRate = a.EXCHANGERATE,
            //                       currencyId = a.CURRENCYID,
            //                       currency = a.TBL_CURRENCY.CURRENCYNAME
            //                   }).FirstOrDefault();

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
                                   //interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.CONTINGENTAMOUNT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
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
                                   //outstandingPrincipal = availableBalance,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   //suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,
                                   currencyCode = a.TBL_CURRENCY.CURRENCYCODE
                               }).FirstOrDefault();
            return loanDetails;
        }

        public LoanViewModel GetContingentByLoanId(int contingentLoanId)
        {
            //decimal overDraftLimit = 0;
            decimal availableBalance = 0;
            //var odDetail = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == revolvingLoanId);
            var odDetail = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == contingentLoanId);
            if (odDetail != null)
            {
                //overDraftLimit = odDetail.OVERDRAFTLIMIT;
                availableBalance = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == odDetail.CASAACCOUNTID).AVAILABLEBALANCE;
            }
            //var overDraftDetail = (from a in context.TBL_LOAN_REVOLVING
            //                       join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
            //                       join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
            //                       where a.REVOLVINGLOANID == revolvingLoanId
            //                       select new LoanViewModel
            //                       {
            //                           loanId = a.REVOLVINGLOANID,
            //                           customerId = a.CUSTOMERID,
            //                           customerName = b.FIRSTNAME + " " + b.LASTNAME,
            //                           customerCode = b.CUSTOMERCODE,
            //                           productId = a.PRODUCTID,
            //                           companyId = a.COMPANYID,
            //                           casaAccountId = a.CASAACCOUNTID,
            //                           branchId = a.BRANCHID,
            //                           branchName = a.TBL_BRANCH.BRANCHNAME,
            //                           loanReferenceNumber = a.LOANREFERENCENUMBER,
            //                           //applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
            //                           //loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
            //                           //productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
            //                           //productName = a.TBL_PRODUCT.PRODUCTNAME,
            //                           //productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
            //                           //relationshipOfficerId = a.RELATIONSHIPOFFICERID,
            //                           //relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
            //                           //relationshipManagerId = a.RELATIONSHIPMANAGERID,
            //                           //relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
            //                           //misCode = a.MISCODE,
            //                           //teamMiscode = a.TEAMMISCODE,
            //                           //interestRate = a.INTERESTRATE,
            //                           //effectiveDate = a.EFFECTIVEDATE,
            //                           //maturityDate = a.MATURITYDATE,
            //                           //bookingDate = a.BOOKINGDATE,
            //                           //principalAmount = a.OVERDRAFTLIMIT,
            //                           //approvalStatusId = a.APPROVALSTATUSID,
            //                           //approverComment = a.APPROVERCOMMENT,
            //                           //dateApproved = a.DATEAPPROVED,
            //                           //loanStatusId = a.LOANSTATUSID,
            //                           //isDisbursed = a.ISDISBURSED,
            //                           //disburserComment = a.DISBURSERCOMMENT,
            //                           //disburseDate = a.DISBURSEDATE,
            //                           //operationId = a.OPERATIONID,
            //                           //operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
            //                           //subSectorName = a.TBL_SUB_SECTOR.NAME,
            //                           //sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
            //                           //casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
            //                           //productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
            //                           //customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
            //                           //loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
            //                           //loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            //                           outstandingPrincipal = availableBalance,
            //                           dischargeLetter = a.DISCHARGELETTER,
            //                           suspendInterest = a.SUSPENDINTEREST,
            //                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
            //                           createdBy = a.CREATEDBY,
            //                           dateTimeCreated = a.DATETIMECREATED,
            //                           exchangeRate = a.EXCHANGERATE,
            //                           currencyId = a.CURRENCYID,
            //                           currency = a.TBL_CURRENCY.CURRENCYNAME,
            //                       }).FirstOrDefault();

            var overDraftDetail = (from a in context.TBL_LOAN_CONTINGENT
                                   join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                                   join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                   join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                   join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                   join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                                   join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                                   join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                                   where a.CONTINGENTLOANID == contingentLoanId
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
                                       //interestRate = a.INTERESTRATE,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       bookingDate = a.BOOKINGDATE,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       approvalStatusId = a.APPROVALSTATUSID,
                                       approverComment = a.APPROVERCOMMENT,
                                       dateApproved = a.DATEAPPROVED,
                                       loanStatusId = a.LOANSTATUSID,
                                       isDisbursed = a.ISDISBURSED,
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
                                       outstandingPrincipal = availableBalance,
                                       dischargeLetter = a.DISCHARGELETTER,
                                       //suspendInterest = a.SUSPENDINTEREST,
                                       customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                       createdBy = a.CREATEDBY,
                                       dateTimeCreated = a.DATETIMECREATED,
                                       exchangeRate = a.EXCHANGERATE,
                                       currencyId = a.CURRENCYID,
                                       currency = a.TBL_CURRENCY.CURRENCYNAME,
                                       currencyCode = a.TBL_CURRENCY.CURRENCYCODE
                                   }).FirstOrDefault();
            //if (availableBalance > 0)
            //{
            //    contingentDetail.overDraft = overDraftLimit;
            //}
            //else
            //{
            //    overDraftDetail.overDraft = overDraftLimit - Math.Abs(availableBalance);
            //}
            return overDraftDetail;

        }

        public IEnumerable<LoanViewModel> GetContingentApprovedApplication()
        {

            var currentDate = generalSetup.GetApplicationDate();
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_LMSR_APPLICATION_DETAIL on a.CONTINGENTLOANID equals b.LOANID
                                   join e in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                   join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                   where a.ISDISBURSED == true && (b.OPERATIONID == (int)OperationsEnum.ContingentLiabilityTermination || b.OPERATIONID == (int)OperationsEnum.ContingentLiabilityRenewal) && //(int)LoanSystemTypeEnum.OverdraftFacility &&
                                   e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && b.OPERATIONPERFORMED == false
                                   orderby b.DATETIMECREATED descending
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       //interestRate = a.INTERESTRATE,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       operationId = b.OPERATIONID,
                                       operationName = b.TBL_OPERATIONS.OPERATIONNAME,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       systemCurrentDate = currentDate,
                                       lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                       loanSystemTypeId = b.LOANSYSTEMTYPEID

                                   }).ToList();
            return allFilteredLoan;

        }


        private IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int companyId, int staffId, int targetId, int operationId)
        {
            var loggedsStaff = context.TBL_STAFF.Find(staffId);
            var result = (from a in context.TBL_APPROVAL_TRAIL
                              // join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                              //  join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                              // join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                              // join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                              // join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                              //join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                              // from j in apprStaff.DefaultIfEmpty()
                              // join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                          where a.COMPANYID == companyId
                          where a.TARGETID == targetId && a.OPERATIONID == operationId
                          select new WorkflowTrackerViewModel
                          {
                              arrivalDate = a.ARRIVALDATE,
                              responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? a.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                              responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                              systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                              systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                              responseStaffName = !a.TOAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                              comment = a.COMMENT,
                              requestStaffName = a.TBL_STAFF.FIRSTNAME != null ? a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME : null,
                              requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                              TargetId = a.TARGETID,
                              // operationId = e.OPERATIONID,
                              // operationName = e.OPERATIONNAME,
                              //approvalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID == a.APPROVALSTATUSID).FirstOrDefault().APPROVALSTATUSNAME
                              approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME
                          }).Distinct();


            var response = result.ToList();
            return result;
        }

        public async Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId)
        {
            var result = await GetApprovalTrail(companyId, staffId, targetId, operationId).ToListAsync();
            return result.Distinct();
        }

        public LoanViewModel GetGroupLoanByLoanId(int loanId)//GetDisbursedLoanByLoanId
        {
            var loanDetails = (from a in context.TBL_LOAN
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
                               where a.TERMLOANID == loanId && a.ISDISBURSED == true && e.CUSTOMERGROUPID != null
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   groupCustomerName = e.TBL_CUSTOMER_GROUP.GROUPNAME,
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
                                   // scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
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
                                   //isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME
                               }).FirstOrDefault();

            return loanDetails;
        }


        private void DebitAccount(int debitGLId, int creditGLId, TBL_CASA casa, decimal chargeAmount, int? debitAccountId, BasicTrasactionSourceInputModel basicInput)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
            debit.description = basicInput.description;
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, basicInput.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = basicInput.createdBy;
            debit.approvedBy = basicInput.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = basicInput.sourceApplicationId;
            debit.companyId = basicInput.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = debitGLId; // (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = debitAccountId;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = basicInput.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
            credit.description = basicInput.description;
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, basicInput.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = basicInput.createdBy;
            credit.approvedBy = basicInput.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = basicInput.sourceApplicationId;
            credit.companyId = basicInput.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = creditGLId;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = basicInput.userBranchId;
            credit.destinationBranchId = basicInput.userBranchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }

        private void ReverseDebit(int debitGLId, int creditGLId, TBL_CASA casa, decimal chargeAmount, int? creditAccountId, BasicTrasactionSourceInputModel basicInput)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CreditBureauSearch;
            debit.description = basicInput.description;
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, basicInput.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = basicInput.createdBy;
            debit.approvedBy = basicInput.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = basicInput.sourceApplicationId;
            debit.companyId = basicInput.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = debitGLId;
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = null;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = basicInput.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CreditBureauSearch;
            credit.description = basicInput.description;
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, basicInput.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = basicInput.createdBy;
            credit.approvedBy = basicInput.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = basicInput.sourceApplicationId;
            credit.companyId = basicInput.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = creditAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = basicInput.userBranchId;
            credit.destinationBranchId = basicInput.userBranchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }

        private IQueryable<LoanViewModel> SearchTermLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && a.LOANSTATUSID != 7 &&
                                   (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       //loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       writtenOff = a.LOANSTATUSID == 7
                                   });

            return allFilteredLoan;
        }

        private IQueryable<LoanViewModel> SearchRevolvingLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && a.LOANSTATUSID != 7 &&
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
                                       writtenOff = a.LOANSTATUSID == 7
                                   });
            return allFilteredLoan;
        }

        private IQueryable<LoanViewModel> SearchContigentLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && a.LOANSTATUSID != 7 && 
                                   (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
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
                                       // isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1,
                                       writtenOff = a.LOANSTATUSID == 7
                                   });
            return allFilteredLoan;
        }




        private IQueryable<LoanViewModel> SearchLoanLine(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_APPLICATION
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.APPROVALSTATUSID == 2 && d.STATUSID == 2
                                   && (
                                       a.APPLICATIONREFERENCENUMBER.Contains(searchQuery) ||
                                       b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                       b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                       b.LASTNAME.ToLower().Contains(searchQuery) ||
                                       c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery)
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
                                       interestRate = 1,
                                       principalAmount = d.APPROVEDAMOUNT,
                                       //effectiveDate = a.EFFECTIVEDATE,
                                       //maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = d.TBL_PRODUCT1.PRODUCTTYPEID, // 1
                                       productName = d.TBL_PRODUCT1.PRODUCTNAME, // 1
                                       //writtenOff = a.LOANSTATUSID == 7
                                   });
            return allFilteredLoan;
        }

        public IEnumerable<LoanViewModel> SearchForLoanAndRevolvingLoan(int performanceTypeId, int loanSystemTypeId, string searchQuery)
        {
            //if (searchQuery == "test1") throw new Exception("Exception 1");
            //if (searchQuery == "test2") throw new SecureException("SecuredException 2");
            //if (searchQuery == "test3") throw new BadLogicException("BadLogicException 3");

            bool all = (performanceTypeId != 1) && (performanceTypeId != 2);
            bool performing = performanceTypeId == 1;
            var applicationDate = generalSetup.GetApplicationDate();

            IEnumerable<LoanViewModel> allFilteredLoan = null;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                if (loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                {
                    allFilteredLoan = SearchTermLoan(searchQuery).Where(x => x.isPerforming == performing || all);
                }
                else if (loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                {
                    allFilteredLoan = SearchRevolvingLoan(searchQuery).Where(x => x.isPerforming == performing || all);
                }
                else if (loanSystemTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                {
                    allFilteredLoan = SearchContigentLoan(searchQuery);
                }
                else if (loanSystemTypeId == (int)LoanSystemTypeEnum.LineFacility)
                {
                    allFilteredLoan = SearchLoanLine(searchQuery);
                }
                else
                {
                    throw new SecureException("Not Implemented!");
                }

            }

            if (performanceTypeId == 3) // writeoff
            {
                allFilteredLoan = allFilteredLoan.Where(x => x.writtenOff == true);
            }

            return allFilteredLoan;
        }
        
        public IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId, ReportSearchParamViewModel param)
        {
            var data = (from l in context.TBL_LOAN
                        where l.LOANREFERENCENUMBER == param.param.Trim() && param.branchId == 0
                        || l.TBL_CUSTOMER.FIRSTNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && param.branchId == 0
                        || l.TBL_CUSTOMER.MAIDENNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && param.branchId == 0
                        || l.TBL_CUSTOMER.LASTNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && param.branchId == 0
                        || l.TBL_CUSTOMER.FIRSTNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && l.BRANCHID == param.branchId
                        || l.TBL_CUSTOMER.MAIDENNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && l.BRANCHID == param.branchId
                        || l.TBL_CUSTOMER.LASTNAME.ToLower().StartsWith(param.param.Trim().ToLower()) && l.BRANCHID == param.branchId
                        || l.BRANCHID == param.branchId && l.LOANREFERENCENUMBER == param.param.Trim()
                        || l.BRANCHID == param.branchId && param.param == null
                        || param.param == null && param.branchId == 0

                        select new LoanViewModel
                        {
                            loanId = l.TERMLOANID,
                            customerId = l.CUSTOMERID,
                            customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.LASTNAME,
                            productId = l.PRODUCTID,
                            companyId = l.COMPANYID,
                            casaAccountId = l.CASAACCOUNTID,
                            branchId = l.BRANCHID,
                            branchName = l.TBL_BRANCH.BRANCHNAME,
                            loanReferenceNumber = l.LOANREFERENCENUMBER,
                            interestRate = l.INTERESTRATE,
                            principalAmount = l.PRINCIPALAMOUNT,
                            approvedAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                            casaAccountNumber = l.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            productAccountName = l.TBL_PRODUCT.PRODUCTNAME,
                            subSectorName = l.TBL_SUB_SECTOR.NAME,
                            sectorName = l.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                        });
            return data;
        }

        public IEnumerable<LoanViewModel> GetLoanStatus(int companyId)
        {
            return (from a in context.TBL_LOAN_STATUS
                    select new LoanViewModel
                    {
                        loanStatus = a.ACCOUNTSTATUS,
                        loanStatusId = a.LOANSTATUSID
                    });
        }

        public IEnumerable<DailyInterestAccrualViewModel> ProcessBackDatedTeamLoansInterestAccrual(DateTime effectiveDate, int loanId)
        {
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                bool result = false;
                var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

                var data1 = (from a in context.TBL_LOAN_SCHEDULE_DAILY
                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                             join c in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals c.LOANID
                             join d in context.TBL_DAY_COUNT_CONVENTION on b.SCHEDULEDAYCOUNTCONVENTIONID equals d.DAYCOUNTCONVENTIONID
                             join e in context.TBL_PRODUCT on b.PRODUCTID equals e.PRODUCTID
                             join f in context.TBL_PRODUCT_TYPE on e.PRODUCTTYPEID equals f.PRODUCTTYPEID
                             where a.DATE == DbFunctions.TruncateTime(effectiveDate) && b.LOANSTATUSID == (short)LoanStatusEnum.Active && b.TERMLOANID == loanId
                             && a.PAYMENTDATE == c.PAYMENTDATE &&
                             (f.PRODUCTTYPEID != (short)LoanProductTypeEnum.CommercialLoan || f.PRODUCTTYPEID != (short)LoanProductTypeEnum.ForeignXRevolving)

                             select new DailyInterestAccrualViewModel()
                             {
                                 referenceNumber = b.LOANREFERENCENUMBER,
                                 productId = b.PRODUCTID,
                                 branchId = b.BRANCHID,
                                 companyId = b.COMPANYID,
                                 currencyId = b.CURRENCYID,
                                 exchangeRate = b.EXCHANGERATE,
                                 interestRate = a.INTERESTRATE,
                                 date = effectiveDate,
                                 dailyAccuralAmount = (double)a.DAILYINTERESTAMOUNT * (DateDiff(effectiveDate, applicationDate) - 1),
                                 mainAmount = c.PERIODINTERESTAMOUNT,
                                 categoryId = (short)DailyAccrualCategory.TermLoan,
                                 transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                                 baseReferenceNumber = null,
                                 dayCountConventionId = d.DAYCOUNTCONVENTIONID,

                             }).ToList();

                var data2 = (from a in context.TBL_LOAN
                             join e in context.TBL_PRODUCT on a.PRODUCTID equals e.PRODUCTID
                             join f in context.TBL_PRODUCT_TYPE on e.PRODUCTTYPEID equals f.PRODUCTTYPEID
                             where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.TERMLOANID == loanId &&
                              (f.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan || f.PRODUCTTYPEID == (short)LoanProductTypeEnum.ForeignXRevolving)
                             && a.MATURITYDATE == DbFunctions.TruncateTime(effectiveDate)


                             select new DailyInterestAccrualViewModel()
                             {
                                 referenceNumber = a.LOANREFERENCENUMBER,
                                 productId = a.PRODUCTID,
                                 branchId = a.BRANCHID,
                                 companyId = a.COMPANYID,
                                 currencyId = a.CURRENCYID,
                                 exchangeRate = a.EXCHANGERATE,
                                 interestRate = a.INTERESTRATE,
                                 date = effectiveDate,
                                 dailyAccuralAmount = ((a.INTERESTRATE / 100) * (double)a.PRINCIPALAMOUNT * 1 / 365) * (DateDiff(effectiveDate, applicationDate) - 1),//((a.INTERESTRATE / 100) * (double)a.PRINCIPALAMOUNT * 1 / 365),
                                 mainAmount = a.PRINCIPALAMOUNT,
                                 categoryId = f.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan ? (short)DailyAccrualCategory.CommercialLoan : (short)DailyAccrualCategory.FXRevolvingLoan,/// change to commercial paper 
                                 availableBalance = a.PRINCIPALAMOUNT,
                                 transactionTypeId = (byte)LoanTransactionTypeEnum.Interest,
                                 baseReferenceNumber = null,
                                 dayCountConventionId = 0,
                             }).ToList();

                List<DailyInterestAccrualViewModel> data = data1.Union(data2).ToList();

                List<TBL_DAILY_ACCRUAL> transAccrual = new List<TBL_DAILY_ACCRUAL>();

                foreach (var item in data)
                {
                    TBL_DAILY_ACCRUAL dailyAccrual = new TBL_DAILY_ACCRUAL();

                    dailyAccrual.REFERENCENUMBER = item.referenceNumber;
                    dailyAccrual.PRODUCTID = item.productId;
                    dailyAccrual.BRANCHID = item.branchId;
                    dailyAccrual.EXCHANGERATE = item.exchangeRate;
                    dailyAccrual.CURRENCYID = item.currencyId;
                    dailyAccrual.INTERESTRATE = item.interestRate;
                    dailyAccrual.DATE = item.date;
                    dailyAccrual.DAILYACCURALAMOUNT = (decimal)Math.Abs(item.dailyAccuralAmount);
                    dailyAccrual.MAINAMOUNT = item.mainAmount;
                    dailyAccrual.CATEGORYID = item.categoryId;
                    dailyAccrual.COMPANYID = item.companyId;
                    dailyAccrual.DAYCOUNTCONVENTIONID = item.dayCountConventionId;
                    dailyAccrual.BASEREFERENCENUMBER = item.baseReferenceNumber;
                    dailyAccrual.TRANSACTIONTYPEID = item.transactionTypeId;
                    transAccrual.Add(dailyAccrual);

                }
                this.context.TBL_DAILY_ACCRUAL.AddRange(transAccrual);
                context.SaveChanges();


                var model = (from a in context.TBL_DAILY_ACCRUAL
                             where a.DATE == DbFunctions.TruncateTime(effectiveDate) && a.CATEGORYID == (short)DailyAccrualCategory.TermLoan
                             group a by new { a.PRODUCTID, a.BRANCHID, a.COMPANYID, a.CURRENCYID } into groupedQ
                             select new DailyInterestAccrualViewModel()
                             {
                                 productId = groupedQ.Key.PRODUCTID,
                                 branchId = groupedQ.Key.BRANCHID,
                                 companyId = groupedQ.Key.COMPANYID,
                                 currencyId = groupedQ.Key.CURRENCYID,
                                 dailyAccuralAmount = (double)groupedQ.Sum(i => i.DAILYACCURALAMOUNT),
                             }).ToList();

                var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (setup.USE_THIRD_PARTY_INTEGRATION)
                {
                    BulkTransactionPosting bulkPosting = new BulkTransactionPosting();

                    result = bulkPosting.WriteBulkDailyTermLoanInterestAccuralToStaging(model, context, stgCon, finacle, financeTransaction, applicationDate);

                }
                else
                {
                    foreach (var item in model)
                    {
                        item.date = effectiveDate;

                        result = financeTransaction.PostDailyLoansInterestAccrual(item);
                    }
                }

                if (result)
                {
                    return model;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
                throw new SecureException(ex.Message);

            }

        }


        #region Loan Disbursement 
        public IEnumerable<LoanDisbursementViewModel> GetAllLoanDisbursement(int loanId)
        {
            var loan = (from a in context.TBL_LOAN_DISBURSEMENT
                        join b in context.TBL_LOAN on a.TERMLOANID equals b.TERMLOANID
                        join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                        where a.TERMLOANID == loanId
                        select new LoanDisbursementViewModel
                        {
                            loanDisbursementId = a.LOANDISBURSEMENTID,
                            loanId = a.TERMLOANID,
                            //beneficiaryCurrencyCode = a.TBL_ CURRENCYID
                           // beneficiaryRateCode = a.TBL,
                            amountDisbursed = a.AMOUNTDISBURSED,
                            loanReferenceNo = b.LOANREFERENCENUMBER,
                            customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME
                        });

            return loan;
        }

        public IEnumerable<LookupViewModel> GetAllFrequencyType()
        {
            var frequencyTypes = generalSetup.GetAllFrequencyTypes();
            return frequencyTypes;
        }

        public bool SendBackToBookingModifier(LoanViewModel model)
        {
            int staffId = 0;


            var staff = context.TBL_STAFF.Where(x => x.STAFFID == staffId).FirstOrDefault();

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == model.operationId.Value)
                 .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                     {
                         groupPosition = mg.m.POSITION,
                         levelPosition = l.POSITION,
                         levelId = l.APPROVALLEVELID,
                         levelName = l.LEVELNAME,
                         staffRoleId = l.STAFFROLEID,
                     })
                     .OrderBy(x => x.groupPosition)
                     .ThenBy(x => x.levelPosition)
                     .ToList();

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId.Value;
            workflow.TargetId = model.loanId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = null;
            workflow.ProductId = null;
            workflow.NextLevelId = staffRoleLevelId;
            workflow.ToStaffId = staffId;
            workflow.StatusId = (int)ApprovalStatusEnum.Referred;
            workflow.Comment = model.comment;
            workflow.DeferredExecution = true;

            workflow.LogActivity();

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.facilityBookingReferedBack,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"facility booking with booking account number {model.loanReferenceNumber} refered back to modifier.",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            context.TBL_AUDIT.Add(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }
        #endregion

        #region Line Operations

        #endregion
        public IEnumerable<CamProcessedLoanViewModel> GetApprovedLineReview()
        {
            var systemDate = generalSetup.GetApplicationDate();
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                       join l in context.TBL_LMSR_APPLICATION_DETAIL on d.LOANAPPLICATIONDETAILID equals l.LOANID
                                       join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                                       join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                                       where l.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility
                                       && l.OPERATIONPERFORMED == false && l.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                       select new CamProcessedLoanViewModel
                                       {
                                           approvalStatusId = (short)m.APPROVALSTATUSID,
                                           loanApplicationId = m.LOANAPPLICATIONID,
                                           loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                           applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                                           applicationStatusId = m.APPLICATIONSTATUSID,
                                           customerId = m.CUSTOMERID ?? 0,
                                           customerCode = c.CUSTOMERCODE,

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
                                           operationId = l.OPERATIONID,
                                           operationName = m.TBL_OPERATIONS.OPERATIONNAME,

                                           companyId = m.COMPANYID,
                                           branchId = m.BRANCHID,
                                           branchName = m.TBL_BRANCH.BRANCHNAME,
                                           subSectorId = d.SUBSECTORID,
                                           subSectorName = d.TBL_SUB_SECTOR.NAME,
                                           sectorName = d.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           applicationTenor = m.APPLICATIONTENOR,
                                           effectiveDate = (DateTime)d.EFFECTIVEDATE,
                                           expiryDate = d.EXPIRYDATE,
                                           
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
                                           createdBy = m.CREATEDBY,
                                           newApplicationDate = m.APPLICATIONDATE,
                                           dateTimeCreated = d.DATETIMECREATED,
                                           systemCurrentDate = systemDate,
                                           loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0
                                       }).ToList();

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

                    item.isBooked = context.TBL_LOAN.Where(x => x.LOAN_BOOKING_REQUESTID == item.loanBookingRequestId).Any();

                    var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                    var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                    var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
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
                        case (short)LoanProductTypeEnum.RevolvingLoan:
                            decimal overdraftBal = 0;
                            foreach (var overdraft in overdrafts)
                            {
                                if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - overdraftBal;
                            break;
                        case (short)LoanProductTypeEnum.ContingentLiability:
                            decimal contingentBal = 0;
                            foreach (var contingent in contingents)
                            {
                                if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - contingentBal;
                            break;
                        case (short)LoanProductTypeEnum.ForeignXRevolving:
                            decimal customerAvailableAmount4 = 0;
                            foreach (var loan in loans)
                            {
                                if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount4 = customerAvailableAmount4 + loan.PRINCIPALAMOUNT;
                            }
                            item.customerAvailableAmount = item.approvedAmount - customerAvailableAmount4;
                            break;
                    }

                    var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                    if (disbursedLoan.Any())
                    {
                        item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                    }
                    if(item.effectiveDate != null)
                    {
                        item.tenorUsed = (systemDate - item.effectiveDate).Value.Days;
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<LoanViewModel> GetApprovedLoanReview()
        {
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                var allFilteredLoan = (from a in context.TBL_LOAN
                                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals b.LOANID
                                       join e in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                       join d in context.TBL_LOAN_SCHEDULE_DAILY on a.TERMLOANID equals d.LOANID
                                       where a.ISDISBURSED == true && e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      && b.TBL_OPERATIONS.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement
                                      && b.OPERATIONPERFORMED == false
                                      && d.DATE == DbFunctions.TruncateTime(applicationDate)
                                       //orderby b.DATECREATED descending
                                       select new LoanViewModel
                                       {
                                           loanId = a.TERMLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           productId = a.PRODUCTID,
                                           companyId = a.COMPANYID,
                                           casaAccountId = a.CASAACCOUNTID,
                                           branchId = a.BRANCHID,
                                           branchName = a.TBL_BRANCH.BRANCHNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                           pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                           interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                           interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                           productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                           productName = a.TBL_PRODUCT.PRODUCTNAME,
                                           productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                           principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                           interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
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
                                           disbursedBy = a.DISBURSEDBY,
                                           disburserComment = a.DISBURSERCOMMENT,
                                           disburseDate = a.DISBURSEDATE,
                                           loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OPERATIONID,
                                           operationName = b.TBL_OPERATIONS.OPERATIONNAME, //context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                           customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                           loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                           loanReviewOperationTypeId = b.OPERATIONID,
                                           reviewDetails = b.REVIEWDETAILS,
                                           interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                           interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                           pastDueInterest = a.PASTDUEINTEREST,
                                           accrualedAmount = d.ACCRUEDINTEREST, //context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).FirstOrDefault().ACCRUEDINTEREST,
                                           systemCurrentDate = applicationDate,
                                           lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                       }).ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region Commercial Loan Operations
        public IEnumerable<LoanViewModel> GetApprovedNonTermLoansForReview()
        {
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                var allFilteredLoan = (from a in context.TBL_LOAN
                                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals b.LOANID
                                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                       where a.ISDISBURSED == true && b.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                       && b.OPERATIONPERFORMED == false
                                       && a.OPERATIONID != (short)OperationsEnum.TermLoanBooking
                                       && b.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
                                       select new LoanViewModel
                                       {
                                           loanId = a.TERMLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           productId = a.PRODUCTID,
                                           companyId = a.COMPANYID,
                                           casaAccountId = a.CASAACCOUNTID,
                                           branchId = a.BRANCHID,
                                           branchName = a.TBL_BRANCH.BRANCHNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                           pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                           interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                           interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                           productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                           productName = a.TBL_PRODUCT.PRODUCTNAME,
                                           productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                           principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                           interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
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
                                           disbursedBy = a.DISBURSEDBY,
                                           disburserComment = a.DISBURSERCOMMENT,
                                           disburseDate = a.DISBURSEDATE,
                                           currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OPERATIONID,
                                           operationName = b.TBL_OPERATIONS.OPERATIONNAME, //context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                           customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                           loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                           loanReviewOperationTypeId = b.OPERATIONID,
                                           reviewDetails = b.REVIEWDETAILS,
                                           interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                           interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                           pastDueInterest = a.PASTDUEINTEREST,
                                           //accrualedAmount = d.ACCRUEDINTEREST, //context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).FirstOrDefault().ACCRUEDINTEREST,
                                           systemCurrentDate = applicationDate,
                                           lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                       }).ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<LoanViewModel> GetApprovedFXRevolvingLoanReview()
        {
            try
            {
                var applicationDate = generalSetup.GetApplicationDate();
                var allFilteredLoan = (from a in context.TBL_LOAN
                                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals b.LOANID
                                       join e in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                       // d in context.TBL_LOAN_SCHEDULE_DAILY on a.TERMLOANID equals d.LOANID
                                       where a.ISDISBURSED == true && e.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                      && b.TBL_OPERATIONS.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement
                                      && b.OPERATIONPERFORMED == false
                                      && a.OPERATIONID == (short)OperationsEnum.ForeignExchangeLoanBooking
                                       //&& d.DATE == DbFunctions.TruncateTime(applicationDate)
                                       //orderby b.DATECREATED descending
                                       select new LoanViewModel
                                       {
                                           loanId = a.TERMLOANID,
                                           customerId = a.CUSTOMERID,
                                           customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           productId = a.PRODUCTID,
                                           companyId = a.COMPANYID,
                                           casaAccountId = a.CASAACCOUNTID,
                                           branchId = a.BRANCHID,
                                           branchName = a.TBL_BRANCH.BRANCHNAME,
                                           loanReferenceNumber = a.LOANREFERENCENUMBER,
                                           applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                           principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                           pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                           interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                           interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                           productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                           productName = a.TBL_PRODUCT.PRODUCTNAME,
                                           productTypeName = a.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                           principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                           interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
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
                                           disbursedBy = a.DISBURSEDBY,
                                           disburserComment = a.DISBURSERCOMMENT,
                                           disburseDate = a.DISBURSEDATE,
                                           //approvedAmount = a.ApprovedAmount,
                                           operationId = a.OPERATIONID,
                                           operationName = b.TBL_OPERATIONS.OPERATIONNAME, //context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                           customerGroupId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                                           loanTypeId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                                           loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                                           customerSensitivityLevelId = a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME,
                                           loanReviewOperationTypeId = b.OPERATIONID,
                                           reviewDetails = b.REVIEWDETAILS,
                                           interesrtOnPastDueInterest = a.INTERESTONPASTDUEINTEREST,
                                           interestOnPastDuePrincipal = a.INTERESTONPASTDUEPRINCIPAL,
                                           pastDueInterest = a.PASTDUEINTEREST,
                                           //accrualedAmount = d.ACCRUEDINTEREST, //context.TBL_LOAN_SCHEDULE_DAILY.Where(x => x.TBL_LOAN.LOANREFERENCENUMBER == a.LOANREFERENCENUMBER && x.DATE == applicationDate).FirstOrDefault().ACCRUEDINTEREST,
                                           systemCurrentDate = applicationDate,
                                           lmsApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                                       }).ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion Commercial loan Operations
    }
}