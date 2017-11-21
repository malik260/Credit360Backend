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
using System.Linq;
//using XLeratorDLL_financial;

namespace FintrakBanking.Repositories.Credit
{
    //using FinancialTypes = XLeratorDLL_financial.FinancialTypes;
    //using wct = XLeratorDLL_financial.XLeratorDLL_financial;

    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private IApprovalLevelStaffRepository level;
        private ICustomerRepository customers;
        private IWorkflow workflow;




        public LoanRepository(FinTrakBankingContext _context, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        ILoanCovenantRepository _loanCovenant,
                                        IFinanceTransactionRepository _financeTransaction, IApprovalLevelStaffRepository _level,
                                        ICustomerRepository _customers, IWorkflow _workflow)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.loanCovenant = _loanCovenant;
            this.financeTransaction = _financeTransaction;
            this.level = _level;
            this.customers = _customers;
            this.workflow = _workflow;
        }

        /// <summary>
        /// Gets all loan types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LookupViewModel> GetAllLoanTypes()
        {
            return (from data in context.TBL_LOAN_TYPE
                    select new LookupViewModel()
                    {
                        lookupId = data.LOANTYPEID,
                        lookupName = data.LOANTYPENAME
                    });
        }

        /// <summary>
        /// Generates the loan reference number.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <param name="productId">The product identifier.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Loan Product Type not defined for Loan Booking</exception>
        private string GenerateLoanReferenceNumber(int customerId, int productId, int productTypeId)
        {
            var customerCode = this.context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == customerId).CUSTOMERCODE;
            var productCode = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == productId).PRODUCTCODE;
            if (productTypeId == (int)LoanProductTypeEnum.TermLoan || productTypeId == (int)LoanProductTypeEnum.SelfLiquidating)
            {
                var data = ((this.context.TBL_LOAN.Count(x => x.CUSTOMERID == customerId && x.PRODUCTID == productId)) + 1);
                return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
            }
            else if (productTypeId == (int)LoanProductTypeEnum.RevolvingLoan)
            {
                var data = ((this.context.TBL_LOAN_REVOLVING.Count(x => x.CUSTOMERID == customerId && x.PRODUCTID == productId)) + 1);
                return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
            }
            else if (productTypeId == (int)LoanProductTypeEnum.ContingentLiability)
            {
                var data = ((this.context.TBL_LOAN_CONTINGENT.Count(x => x.CUSTOMERID == customerId && x.PRODUCTID == productId)) + 1);
                return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
            }
            else throw new Exception("Loan Product Type not defined for Loan Booking");

        }
     
        /// <summary>
        /// Adds the loan booking.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">The Product type is Invalid</exception>
        public string AddLoanBooking(LoanViewModel entity)
        {
            //...................CHECK IF THE LOAN RECORD IS A SCHEDULED LOAN..................//
            if (entity.productTypeId == (int)LoanProductTypeEnum.TermLoan || entity.productTypeId == (int)LoanProductTypeEnum.SelfLiquidating)
            {
                return this.AddTermLoan(entity);
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
                throw new Exception("The Product type is Invalid");
            }
        }
     
        /// <summary>
        /// Adds the revolving loan.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string addRevolvingLoan(LoanViewModel model)
        {
            var revolvingLoanInput = model.revolvingLoanInput;

            var overdraftLimit = from a in context.TBL_LOAN_REVOLVING
                                   where a.LOANAPPLICATIONDETAILID == revolvingLoanInput.loanApplicationDetailId
                                   let sumLimit = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == revolvingLoanInput.loanApplicationDetailId).Sum(x => x.OVERDRAFTLIMIT)
                                   select sumLimit;

            var totalPreviouslyBookedAmount = overdraftLimit.FirstOrDefault();

            var totaloverdraftLimit = totalPreviouslyBookedAmount + revolvingLoanInput.overdraftLimit;

            if (totaloverdraftLimit > model.customerAvailableAmount)
                throw new Exception("The loan amount cannot greater than the availiable amount");

            var CurrRatings = context.TBL_CURRENCY_RATE.Where(x => x.CURRENCYID == model.currencyId).FirstOrDefault();
            var currentExchangeRate = 1.0;

            if (CurrRatings != null) currentExchangeRate = CurrRatings.SELLINGRATE;

            var loanReferenceNumber = GenerateLoanReferenceNumber(model.customerId, model.productId, model.productTypeId);
            var data = new TBL_LOAN_REVOLVING
            {
                CUSTOMERID = model.customerId,
                PRODUCTID = model.productId,
                CASAACCOUNTID = model.casaAccountId,
                BRANCHID = model.branchId,
                CURRENCYID = (short)model.exchangeRate,
                EXCHANGERATE = currentExchangeRate,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                SUBSECTORID = model.subSectorId,
                RELATIONSHIPOFFICERID = model.relationshipOfficerId,
                RELATIONSHIPMANAGERID = model.relationshipManagerId,
                MISCODE = model.misCode,
                TEAMMISCODE = model.teamMiscode,
                INTERESTRATE = model.interestRate,
                EFFECTIVEDATE = model.effectiveDate,
                MATURITYDATE = model.maturityDate,
                BOOKINGDATE = model.bookingDate,
                OVERDRAFTLIMIT = revolvingLoanInput.overdraftLimit,
                DAYCOUNTCONVENTIONID = model.scheduleDayCountConventionId,

                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                LOANSTATUSID = (int)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                OPERATIONID = (int)OperationsEnum.RevolvingLoanBooking,
                CUSTOMERGROUPID = model.customerGroupId,
                LOANTYPEID = model.loanTypeId,
                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,
                CUSTOMERSENSITIVITYLEVELID = model.customerSensitivityLevelId,
                COMPANYID = model.companyId,
                CREATEDBY=model.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate(),

            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.branchId,
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

                    //...................Adding Contingent Loan Record.........................
                    var loan = context.TBL_LOAN_REVOLVING.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                   // AddLoanCollateralMapping(model.loanCollateral, model.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    //AddLoanGuarantor(model.loanGuarantor, (short)model.productTypeId, model.loanApplicationId);

                    //...................Adding Audit...............................
                    context.TBL_AUDIT.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = model.createdBy,
                        companyId = model.companyId,
                        applicationId = loan.REVOLVINGLOANID,
                        comment = "Please approve this Loan",
                        amount = revolvingLoanInput.approvedAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (model.feeOverride)
                    {
                        trans.Commit();
                        //............save Loan Covenant..........
                        AddLoanCovenant(model.loanCovenant, model.loanApplicationDetailId, loan.REVOLVINGLOANID, (short)model.productTypeId);
                        //............save Loan Fees..........
                        AddLoanFees(model.loanChargeFee, loan.REVOLVINGLOANID, (short)model.productTypeId, model.companyId, model.feeOverride);

                        context.SaveChanges();
                    }
                    else
                    {
                        if (LogApproval(approvalModel, (int)OperationsEnum.RevolvingLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                        {
                            trans.Commit();
                            //............save Loan Covenant..........
                            AddLoanCovenant(model.loanCovenant, model.loanApplicationDetailId, loan.REVOLVINGLOANID, (short)model.productTypeId);
                            //............save Loan Fees..........
                            AddLoanFees(model.loanChargeFee, loan.REVOLVINGLOANID, (short)model.productTypeId, model.companyId, model.feeOverride);
                            PostLoanFees(model);

                            context.SaveChanges();
                        }
                    }
                 
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }

        }
      
        /// <summary>
        /// Adds the contingent liability.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string addContingentLiability(LoanViewModel entity)
        {
            var contingentLoanInput = entity.contingentLoanInput;

            var contingentAmount = from a in context.TBL_LOAN_CONTINGENT
                                  where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                  let sumAmount = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.CONTINGENTAMOUNT)
                                  select sumAmount;

            var totalPreviouslyBookedAmount = contingentAmount.FirstOrDefault();

            var totalContingentAmount = totalPreviouslyBookedAmount + contingentLoanInput.contingentAmount;

            if (totalContingentAmount > entity.customerAvailableAmount)
                throw new Exception("The loan amount cannot greater than the availiable amount");

            var CurrRatings = context.TBL_CURRENCY_RATE.Where(x => x.CURRENCYID == contingentLoanInput.currencyId).FirstOrDefault();
            var currentExchangeRate = 1.0;

            if (CurrRatings != null) currentExchangeRate = CurrRatings.SELLINGRATE;


            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId, entity.productTypeId);
            var data = new TBL_LOAN_CONTINGENT
            {
                CUSTOMERID = entity.customerId,
                PRODUCTID = entity.productId,
                CASAACCOUNTID = entity.casaAccountId,
                BRANCHID = entity.branchId,
                CURRENCYID = contingentLoanInput.currencyId,
                EXCHANGERATE = currentExchangeRate,
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                SUBSECTORID = entity.subSectorId,
                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                EFFECTIVEDATE = entity.effectiveDate,
                MATURITYDATE = entity.maturityDate,
                
                BOOKINGDATE = entity.bookingDate,

                CONTINGENTAMOUNT = contingentLoanInput.contingentAmount,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                LOANSTATUSID = (int)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                OPERATIONID = (int)OperationsEnum.ContigentLoanBooking,
                CUSTOMERGROUPID = entity.customerGroupId,
                LOANTYPEID = entity.loanTypeId,
                DISCHARGELETTER = false,
                CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate(),


            };


            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanBookingAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.branchId,
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
                    //...................Adding Contingent Loan Record.........................
                    var loan = context.TBL_LOAN_CONTINGENT.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                   // AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    //AddLoanGuarantor(entity.loanGuarantor,  (short)entity.productTypeId, entity.loanApplicationId);

                    //...................Adding Audit...............................
                    context.TBL_AUDIT.Add(audit);

                    //........Save Changes...............
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = loan.CONTINGENTLOANID,
                        comment = "Please approve this Loan",
                        amount = contingentLoanInput.approvedAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (entity.feeOverride)
                    {
                        //.....Commit transaction ............
                        trans.Commit();
                        //............save Loan Covenant..........
                        AddLoanCovenant(entity.loanCovenant, entity.loanApplicationDetailId, loan.CONTINGENTLOANID, (short)entity.productTypeId);
                        //............save Loan Fees..........
                        AddLoanFees(entity.loanChargeFee, loan.CONTINGENTLOANID, (short)entity.productTypeId, entity.companyId, entity.feeOverride);

                        context.SaveChanges();
                    }
                    else
                    {
                        if (LogApproval(approvalModel, (int)OperationsEnum.ContigentLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                        {
                            //.....Commit transaction ............
                            trans.Commit();
                            //............save Loan Covenant..........
                            AddLoanCovenant(entity.loanCovenant, entity.loanApplicationDetailId, loan.CONTINGENTLOANID, (short)entity.productTypeId);
                            //............save Loan Fees..........
                            AddLoanFees(entity.loanChargeFee, loan.CONTINGENTLOANID, (short)entity.productTypeId, entity.companyId, entity.feeOverride);

                            context.SaveChanges();
                        }
                    }
                    
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }
    
        /// <summary>
        /// Adds the term loan.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Loan terminal date should be more than effective date
        /// or
        /// The loan amount cannot be greater than the availiable amount
        /// or
        /// </exception>
        private string AddTermLoan(LoanViewModel entity)
        {
            if (entity.loanScheduleInput.maturityDate <= entity.loanScheduleInput.effectiveDate)
                throw new Exception("Loan terminal date should be more than effective date");

            var principalAmount  = from a in context.TBL_LOAN where a.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId
                                   let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).Sum(x => x.PRINCIPALAMOUNT)
                                   select sumPrincipalAmount;

            var approvedAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId).FirstOrDefault().APPROVEDAMOUNT;
            
            var totalPreviouslyBookedAmount = principalAmount.FirstOrDefault();

            var totalPrincipalAmount = (decimal)(totalPreviouslyBookedAmount + (decimal)entity.loanScheduleInput.principalAmount);

            if (totalPrincipalAmount > (decimal)approvedAmount)
                throw new Exception("The loan amount cannot be greater than the availiable amount");


            var CurrRatings = context.TBL_CURRENCY_RATE.Where(x => x.CURRENCYID == entity.currencyId).FirstOrDefault();
            var currentExchangeRate  = 1.0;
            
            if(CurrRatings  != null) currentExchangeRate = CurrRatings.SELLINGRATE;

            double? priceIndex = (from a in context.TBL_PRODUCT
                                  where a.PRODUCTID == entity.productId
                                  select a.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXRATE).FirstOrDefault();

            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId, entity.productTypeId);

            if (entity.loanScheduleInput.scheduleMethodId == (short) LoanScheduleTypeEnum.BulletPayment)
            {
                entity.loanScheduleInput.principalFrequency = null;
                entity.loanScheduleInput.interestFrequency = null;
            }

            var data = new TBL_LOAN
            {
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                LOANREFERENCENUMBER = loanReferenceNumber,
                LOANSTATUSID = (short)LoanStatusEnum.Inactive,
                ISDISBURSED = false,
                PRINCIPALNUMBEROFINSTALLMENT = 0,
                INTERESTNUMBEROFINSTALLMENT = 0,
                //IsScheduledPrepayment = null,
                SCHEDULEDPREPAYMENTAMOUNT = entity.scheduledPrepaymentAmount,
                SCH_PREPAYMENT_FREQUENCY_TYPEID = null,
                PRODUCTPRICEINDEXRATE = (double)priceIndex,

                CUSTOMERGROUPID = (entity.customerGroupId != 0 ? entity.customerGroupId : null),

                LOANTYPEID = entity.loanTypeId,
                SUBSECTORID = entity.subSectorId,
                CURRENCYID = (short)entity.currencyId,
                EXCHANGERATE = currentExchangeRate,

                DISCHARGELETTER = false,
                SUSPENDINTEREST = false,
                
                CUSTOMERID = entity.customerId,
                PRODUCTID = (short)entity.productId,
                COMPANYID = entity.companyId,
                CASAACCOUNTID = entity.casaAccountId,
                BRANCHID = entity.branchId,
                
                PRINCIPALFREQUENCYTYPEID = entity.loanScheduleInput.principalFrequency,
                INTERESTFREQUENCYTYPEID = entity.loanScheduleInput.interestFrequency,

                RELATIONSHIPOFFICERID = entity.relationshipOfficerId,
                RELATIONSHIPMANAGERID = entity.relationshipManagerId,
                MISCODE = entity.misCode,
                TEAMMISCODE = entity.teamMiscode,
                INTERESTRATE = Convert.ToInt32(entity.interestRate),

                PRINCIPALINSTALLMENTLEFT = 0,
                INTERESTINSTALLMENTLEFT = 0,

                SCHEDULETYPEID = entity.loanScheduleInput.scheduleMethodId,

                PRINCIPALAMOUNT = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),

                OPERATIONID = (int)OperationsEnum.TermLoanBooking,

                EQUITYCONTRIBUTION = 0,
                OUTSTANDINGPRINCIPAL = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),
                PRINCIPALADDITIONCOUNT = 0,
                PRINCIPALREDUCTIONCOUNT = 0,
                FIXEDPRINCIPAL = false,
                PROFILELOAN = false,
                CUSTOMERSENSITIVITYLEVELID = entity.customerSensitivityLevelId,

                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,

                BOOKINGDATE = entity.bookingDate,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate(),
                EFFECTIVEDATE = entity.loanScheduleInput.effectiveDate,
                MATURITYDATE = entity.loanScheduleInput.maturityDate,
                FIRSTPRINCIPALPAYMENTDATE = entity.loanScheduleInput.principalFirstpaymentDate,
                FIRSTINTERESTPAYMENTDATE = entity.loanScheduleInput.interestFirstpaymentDate,
                ALLOWFORCEDEBITREPAYMENT = false,

            };

            if (entity.customerGroupId > 0)
            {
                data.CUSTOMERGROUPID = entity.customerGroupId;
            }

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

                    //...................Adding Contingent Loan Record.........................
                    var loan = context.TBL_LOAN.Add(data);

                    //...................Saving Loan Collaterals Mapping.......................
                    //AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId);

                    //...................Saving Loan Gaurantors................................
                    //AddLoanGuarantor(entity.loanGuarantor, (short)entity.productTypeId, entity.loanApplicationId);

                    //...................Adding Audit...............................
                    var dataCount = context.SaveChanges();

                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        applicationId = loan.TERMLOANID,
                        comment = "Please approve this Loan",
                        amount = entity.principalAmount,
                    };

                    //.....................LOG LOAN BOOKING TRANSACTION FOR APPROVAL......................................
                    if (entity.feeOverride)
                    {
                        //.....Commit transaction ............
                        trans.Commit();

                        if (entity.loanScheduleInput.scheduleMethodId == (short)LoanScheduleTypeEnum.IrregularSchedule)
                        {
                            foreach (var irregular in entity.loanScheduleInput.irregularPaymentSchedule)
                            {
                                var irregularRecordData = new TBL_LOAN_SCHEDULE_IRREGUL_INPUT
                                {
                                    LOANID = loan.TERMLOANID,
                                    PAYMENTAMOUNT = (decimal)irregular.paymentAmount,
                                    PAYMENTDATE = irregular.paymentDate,
                                    CREATEDBY = entity.createdBy,
                                    DATETIMECREATED = generalSetup.GetApplicationDate()
                                };
                                context.TBL_LOAN_SCHEDULE_IRREGUL_INPUT.Add(irregularRecordData);
                            }

                        }
                        AddLoanCovenant(entity.loanCovenant, entity.loanApplicationId, loan.TERMLOANID, (short)entity.productTypeId);
                        AddLoanFees(entity.loanChargeFee, loan.TERMLOANID, (short)entity.productTypeId,entity.companyId, entity.feeOverride);

                        context.SaveChanges();
                    }
                    else
                    {
                        if (LogApproval(approvalModel, (int)OperationsEnum.TermLoanBooking, true, (int)ApprovalStatusEnum.Pending))
                        {
                            //.....Commit transaction ............
                            trans.Commit();

                            if (entity.loanScheduleInput.scheduleMethodId == (short)LoanScheduleTypeEnum.IrregularSchedule)
                            {
                                foreach (var irregular in entity.loanScheduleInput.irregularPaymentSchedule)
                                {
                                    var irregularRecordData = new TBL_LOAN_SCHEDULE_IRREGUL_INPUT
                                    {
                                        LOANID = loan.TERMLOANID,
                                        PAYMENTAMOUNT = (decimal)irregular.paymentAmount,
                                        PAYMENTDATE = irregular.paymentDate,
                                        CREATEDBY = entity.createdBy,
                                        DATETIMECREATED = generalSetup.GetApplicationDate()
                                    };
                                    context.TBL_LOAN_SCHEDULE_IRREGUL_INPUT.Add(irregularRecordData);
                                }

                            }
                            AddLoanCovenant(entity.loanCovenant, entity.loanApplicationId, loan.TERMLOANID, (short)entity.productTypeId);
                            AddLoanFees(entity.loanChargeFee, loan.TERMLOANID, (short)entity.productTypeId, entity.companyId, entity.feeOverride);

                            entity.loanReferenceNumber = loan.LOANREFERENCENUMBER;
                            PostLoanFees(entity);

                            context.SaveChanges();
                        }
                    }
                    //.......................END OF APPROVAL LOG......................................................

                    if (dataCount > 0)
                        return loanReferenceNumber;
                    else
                        return "";
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }
    
        /// <summary>
        /// Logs the approval.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="operationId">The operation identifier.</param>
        /// <param name="externalInitialization">if set to <c>true</c> [external initialization].</param>
        /// <param name="ApprovalStatusId">The approval status identifier.</param>
        /// <returns></returns>
        public bool LogApproval(ForwardViewModel model, int operationId, bool externalInitialization, int ApprovalStatusId)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Comment = model.comment;
            workflow.ExternalInitialization = externalInitialization;
            workflow.StatusId = ApprovalStatusId;
            workflow.Amount = model.amount;

            workflow.LogActivity();

            return workflow.Saved;
        }
    
        /// <summary>
        /// Disburses the loan.
        /// </summary>
        /// <param name="entity">The entity.</param>
        private void DisburseLoan(LoanViewModel entity)
        {
            //PostLoanDisbursment(entity);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(BuildLoanDisbursmentPosting(entity));

            inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

            financeTransaction.PostTransaction(inputTransactions);
        }

        public void PostLoanFees(LoanViewModel entity)
        {
          
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

            financeTransaction.PostTransaction(inputTransactions);
        }


        //[OperationBehavior(TransactionScopeRequired = true)]
        //public LoanViewModel PostLoanDisbursment(LoanViewModel model)
        //{      
        //    FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


        //    var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

        //    loanTransaction.operationId = (int)OperationsEnum.LoanBooking;
        //    loanTransaction.description = "Loan Disbursment Amount";
        //    loanTransaction.valueDate = generalSetup.GetApplicationDate();
        //    loanTransaction.transactionDate = loanTransaction.valueDate;
        //    loanTransaction.currencyId = casa.CurrencyId;
        //    loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.currencyId, loanTransaction.valueDate, model.companyId);
        //    loanTransaction.isApproved = true;
        //    loanTransaction.postedBy = model.createdBy;
        //    loanTransaction.approvedBy = model.createdBy;
        //    loanTransaction.approvedDate = loanTransaction.transactionDate;
        //    loanTransaction.approvedDateTime = DateTime.Now;
        //    loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    loanTransaction.companyId = model.companyId;

        //    FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
        //    debit.glAccountId = model.casaAccountId;  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    debit.sourceReferenceNumber = model.loanReferenceNumber;
        //    debit.casaAccountId = casa.CasaAccountId;
        //    debit.debitAmount = model.principalAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = model.branchId;
        //    debit.destinationBranchId = casa.BranchId;

        //    var repaymentGL = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
        //    credit.glAccountId = repaymentGL;

        //    credit.sourceReferenceNumber = model.loanReferenceNumber;
        //    credit.casaAccountId = casa.CasaAccountId;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = model.principalAmount;
        //    credit.sourceBranchId = model.branchId;
        //    credit.destinationBranchId = model.branchId;


        //    loanTransaction.transactionDetails.Add(debit);
        //    loanTransaction.transactionDetails.Add(credit);

        //    financeTransaction.PostTransaction(loanTransaction);

        //    // Audit Section ---------------------------            

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.LoanApplication,
        //        StaffId = model.createdBy,
        //        BranchId = model.branchId,
        //        Detail = $"Applied for Loan Disbursment with reference number: {model.loanReferenceNumber}",
        //        IPAddress = model.userIPAddress,
        //        Url = model.applicationUrl,
        //        ApplicationDate = generalSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return model;

        //}

        //public LoanViewModel PostLoanChargeFee(LoanViewModel data)
        //{
        //    //foreach ( var model in data.loanChargeFee)
        //    //{
        //    //    //model.ledgerAccountId = data.
        //    //}
        //    //data.loanChargeFee
        //     LoanChargeFeeViewModel model = new LoanChargeFeeViewModel();
        //     FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();


        //    var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.ledgerAccountId && x.CompanyId == model.companyId);

        //    feeTransaction.operationId = (int)OperationsEnum.LoanBooking;
        //    feeTransaction.description = "Fee charge";
        //    feeTransaction.valueDate = generalSetup.GetApplicationDate();
        //    feeTransaction.transactionDate = feeTransaction.valueDate;
        //    feeTransaction.currencyId = casa.CurrencyId;
        //    feeTransaction.currencyRate = financeTransaction.GetExchangeRate(feeTransaction.currencyId, feeTransaction.valueDate, model.companyId);
        //    feeTransaction.isApproved = true;
        //    feeTransaction.postedBy = model.createdBy;
        //    feeTransaction.approvedBy = model.createdBy;
        //    feeTransaction.approvedDate = feeTransaction.transactionDate;
        //    feeTransaction.approvedDateTime = DateTime.Now;
        //    feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    feeTransaction.companyId = model.companyId;

        //    FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
        //    debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    debit.sourceReferenceNumber = data.loanReferenceNumber;
        //    debit.casaAccountId = casa.CasaAccountId;
        //    debit.debitAmount = (decimal)model.amount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = data.branchId;
        //    debit.destinationBranchId = casa.BranchId;

        //    var feeGL = model.ledgerAccountId; //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
        //    FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
        //    credit.glAccountId = feeGL;

        //    credit.sourceReferenceNumber = data.loanReferenceNumber;
        //    credit.casaAccountId = casa.CasaAccountId;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = (decimal)model.amount;
        //    credit.sourceBranchId = data.branchId;
        //    credit.destinationBranchId = data.branchId;


        //    feeTransaction.transactionDetails.Add(debit);
        //    feeTransaction.transactionDetails.Add(credit);

        //    financeTransaction.PostTransaction(feeTransaction);

        //    // Audit Section ---------------------------            

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.LoanApplication,
        //        StaffId = model.createdBy,
        //        BranchId = data.branchId,
        //        Detail = $"Applied for fee charge with reference number: {data.loanReferenceNumber}",
        //        IPAddress = model.userIPAddress,
        //        Url = model.applicationUrl,
        //        ApplicationDate = generalSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return data;

        //}

        /// <summary>
        /// Gets the term loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetTermLoanBookingAwaitingApproval(int staffId, int companyId)
        { 

            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.TermLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on ln.TERMLOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.TermLoanBooking
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.TERMLOANID descending

                        select new LoanViewModel()
                        {
                            loanId = ln.TERMLOANID,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,

                            branchId = ln.BRANCHID,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                            //tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            //principalFrequencyTypeId = ln.PrincipalFrequencyTypeId ?? 0,
                            pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.DESCRIPTION ?? null,
                           // interestFrequencyTypeId = ln.PrincipalFrequencyTypeId ?? 0,
                            interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE1.DESCRIPTION ?? null,

                            principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                            interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                            //relationshipOfficerId = ln.RelationshipOfficerId,
                            //relationshipManagerId = ln.RelationshipManagerId,
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
                            //loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.SCHEDULETYPEID,
                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                            customerGroupId = ln.CUSTOMERGROUPID,
                           // operationId = ln.OperationId,
                            loanTypeId = ln.LOANTYPEID,
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
                            //scheduledPrepaymentFrequencyTypeId = ln.ScheduledPrepaymentFrequencyTypeId,

                            customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                            middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                            lastName = ln.TBL_CUSTOMER.LASTNAME,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_TYPE.LOANTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                            productName = ln.TBL_PRODUCT.PRODUCTNAME,

                            createdBy = ln.CREATEDBY,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            comment = "",
                            

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
                    
            return data;
        }
     
        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<RevolvingLoanViewModel> GetRevolvingLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.RevolvingLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN_REVOLVING
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on ln.REVOLVINGLOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.RevolvingLoanBooking
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.REVOLVINGLOANID descending

                        select new RevolvingLoanViewModel()
                        {
                            loanId = ln.REVOLVINGLOANID,
                            operationId = ln.OPERATIONID,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,

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
                            approvedBy = ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            //loanStatusId = ln.LoanStatusId,
                           
                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            overdraftLimit = ln.OVERDRAFTLIMIT,
                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                            customerGroupId = ln.CUSTOMERGROUPID,
                            // operationId = ln.OperationId,
                            loanTypeId = ln.LOANTYPEID,
                            
                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            //SectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            dischargeLetter = ln.DISCHARGELETTER,
                            suspendInterest = ln.SUSPENDINTEREST,

                            customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                            middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                            lastName = ln.TBL_CUSTOMER.LASTNAME,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_TYPE.LOANTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                            productName = ln.TBL_PRODUCT.PRODUCTNAME,

                            createdBy = ln.CREATEDBY,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            comment = "",

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
            return data.ToList();
        }
     
        /// <summary>
        /// Gets the contingent loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<ContingentLoanViewModel> GetContingentLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.ContigentLoanBooking);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN_CONTINGENT
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on ln.CONTINGENTLOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.ContigentLoanBooking
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.CONTINGENTLOANID descending

                        select new ContingentLoanViewModel()
                        {
                            loanId = ln.CONTINGENTLOANID,
                            operationId = (int)OperationsEnum.ContigentLoanBooking,
                            customerId = ln.CUSTOMERID,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            loanApplicationDetailId = (int)ln.LOANAPPLICATIONDETAILID,

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
                            approvedBy = ln.APPROVEDBY,
                            approverComment = ln.APPROVERCOMMENT,
                            dateApproved = ln.DATEAPPROVED,
                            //loanStatusId = ln.LoanStatusId,

                            isDisbursed = ln.ISDISBURSED,
                            disbursedBy = ln.DISBURSEDBY,
                            disburserComment = ln.DISBURSERCOMMENT,
                            disburseDate = ln.DISBURSEDATE,

                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                            customerGroupId = ln.CUSTOMERGROUPID,
                            // operationId = ln.OperationId,
                            loanTypeId = ln.LOANTYPEID,

                            subSectorId = ln.SUBSECTORID,
                            subSectorName = ln.TBL_SUB_SECTOR.NAME,
                            //SectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,
                            dischargeLetter = ln.DISCHARGELETTER,

                            customerSensitivityLevelId = ln.CUSTOMERSENSITIVITYLEVELID,
                            customerSensitivityLevelName = ln.TBL_CUSTOMER_SENSITIVITY_LEVEL.DESCRIPTION,
                            firstName = ln.TBL_CUSTOMER.FIRSTNAME,
                            middleName = ln.TBL_CUSTOMER.MIDDLENAME,
                            lastName = ln.TBL_CUSTOMER.LASTNAME,
                            customerCode = ln.TBL_CUSTOMER.CUSTOMERCODE,
                            productAccountNumber = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = ln.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            loanTypeName = ln.TBL_LOAN_TYPE.LOANTYPENAME,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            currencyId = ln.CURRENCYID,

                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,

                            productName = ln.TBL_PRODUCT.PRODUCTNAME,

                            createdBy = ln.CREATEDBY,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            comment = "",

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
            return data;
        }

        /// <summary>
        /// Goes for approval.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Approval Failed
        /// or
        /// Approval failed. " + e.Message
        /// or
        /// </exception>

        /// <summary>
        /// Gets the term loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanChargeFeeViewModel> GetDeferredTermLoanFeeAwaitingApproval(int staffId, int companyId)
        {

            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanBookingFeeDeferral);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join fee in context.TBL_LOAN_FEE on ln.TERMLOANID equals fee.LOANID
                        join atrail in context.TBL_APPROVAL_TRAIL on fee.LOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.LoanBookingFeeDeferral
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.TERMLOANID descending

                        select new LoanChargeFeeViewModel()
                        {
                            loanId = ln.TERMLOANID,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            feeAmount = (decimal)(from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == ln.TERMLOANID) select tot).Sum(x => x.FEEAMOUNT),
                            loanAmount = (from m in context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == ln.LOANAPPLICATIONDETAILID) select m).Sum(x => x.APPROVEDAMOUNT),
                            //loanDeferredFeeList = new List<LoanChargeFeeViewModel>()

                        });
            foreach(var a in data)
            {
                a.loanDeferredFeeList = (from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == a.loanId)
                   select
                    new LoanChargeFeeViewModel
                    {
                        feeAmount = tot.FEEAMOUNT,
                        feeRateValue = tot.FEERATEVALUE,
                        isIntegralFee = tot.ISINTEGRALFEE,
                        recurring = tot.ISRECURRING,
                        productTypeId = tot.PRODUCTTYPEID,
                        isPosted = tot.ISPOSTED,
                        chargeFeeId = tot.CHARGEFEEID,
                        feeDependentAmount = tot.FEEDEPENDENTAMOUNT
                    }).ToList();
            }

            return data;
             
        }

                /// <summary>
        /// Gets the term loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanChargeFeeViewModel> GetDeferredRevolvingLoanFeeAwaitingApproval(int staffId, int companyId)
        {

            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanBookingFeeDeferral);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN_REVOLVING
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join fee in context.TBL_LOAN_FEE on ln.REVOLVINGLOANID equals fee.LOANID
                        join atrail in context.TBL_APPROVAL_TRAIL on fee.LOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.LoanBookingFeeDeferral
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.REVOLVINGLOANID descending

                        select new LoanChargeFeeViewModel()
                        {
                            loanId = ln.REVOLVINGLOANID,
                            operationId = (int)OperationsEnum.RevolvingLoanBooking,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            feeAmount = (decimal)(from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == ln.REVOLVINGLOANID) select tot).Sum(x => x.FEEAMOUNT),
                            loanAmount = (from m in context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONDETAILID == ln.LOANAPPLICATIONDETAILID) select m).Sum(x => x.APPROVEDAMOUNT),

                            

                        });

            foreach(var a in data)
            {
                a.loanDeferredFeeList = (from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == a.loanId)
                                         select
                                              new LoanChargeFeeViewModel
                                              {
                                                  feeAmount = tot.FEEAMOUNT,
                                                  feeRateValue = tot.FEERATEVALUE,
                                                  isIntegralFee = tot.ISINTEGRALFEE,
                                                  recurring = tot.ISRECURRING,
                                                  productTypeId = tot.PRODUCTTYPEID,
                                                  isPosted = tot.ISPOSTED,
                                                  chargeFeeId = tot.CHARGEFEEID,
                                                  feeDependentAmount = tot.FEEDEPENDENTAMOUNT
                                              }).ToList();
            }

            return data;
        }


        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>


                /// <summary>
        /// Gets the term loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanChargeFeeViewModel> GetDeferredContingentLoanFeeAwaitingApproval(int staffId, int companyId)
        {

            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.LoanBookingFeeDeferral);
            //var levelResult = level.GetAllAssignedApprovalLevelStaff(companyId);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.TBL_LOAN_CONTINGENT
                        join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                        join fee in context.TBL_LOAN_FEE on ln.CONTINGENTLOANID equals fee.LOANID
                        join atrail in context.TBL_APPROVAL_TRAIL on fee.LOANID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                              && atrail.OPERATIONID == (int)OperationsEnum.LoanBookingFeeDeferral
                              && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                              && atrail.RESPONSESTAFFID == null
                        orderby ln.CONTINGENTLOANID descending

                        select new LoanChargeFeeViewModel()
                        {
                            loanId = ln.CONTINGENTLOANID,
                            operationId = (int)OperationsEnum.ContigentLoanBooking,
                            productId = ln.PRODUCTID,
                            casaAccountId = ln.CASAACCOUNTID,
                            feeAmount = (decimal)(from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == ln.CONTINGENTLOANID) select tot).Sum(x => x.FEEAMOUNT),
                            loanAmount = (from m in context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONDETAILID == ln.LOANAPPLICATIONDETAILID) select m).Sum(x => x.APPROVEDAMOUNT),
                            
                        });
            foreach(var a in data)
            {
                a.loanDeferredFeeList = (from tot in context.TBL_LOAN_FEE.Where(x => x.LOANID == a.loanId)
                                         select new LoanChargeFeeViewModel
                                              {
                                                  feeAmount = tot.FEEAMOUNT,
                                                  feeRateValue = tot.FEERATEVALUE,
                                                  isIntegralFee = tot.ISINTEGRALFEE,
                                                  recurring = tot.ISRECURRING,
                                                  productTypeId = tot.PRODUCTTYPEID,
                                                  isPosted = tot.ISPOSTED,
                                                  chargeFeeId = tot.CHARGEFEEID,
                                                  feeDependentAmount = tot.FEEDEPENDENTAMOUNT
                                              }).ToList();
            }
            return data;
        }


        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>


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
                        throw new Exception("Approval Failed");
                    }

                    try
                    {
                        
                        if (workflow.NewState != (int)ApprovalState.Ended)
                        {
                            var feeRec = context.TBL_LOAN_FEE.Find(entity.targetId);
                            var allTargetLoanFees = context.TBL_LOAN_FEE.Where(x => x.LOANID == feeRec.LOANID);
                            foreach(var fee in allTargetLoanFees)
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
                        throw new Exception("Approval failed. " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
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

        /// <summary>
        /// Gets the revolving loan booking awaiting approval.
        /// </summary>
        /// <param name="staffId">The staff identifier.</param>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public bool GoForApproval(ApprovalViewModel entity)
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
                        throw new Exception("Approval Failed");
                    }

                    try
                    {
                        ApproveLoanBooking(entity.targetId, (short)workflow.StatusId, entity);
                        trans.Commit();
                        if (workflow.NewState != (int)ApprovalState.Ended) return false;
                        else return true;
                    }
                    catch (Exception e)
                    {
                        trans.Rollback();
                        throw new Exception("Approval failed. " + e.Message);
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
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
        private bool ApproveLoanBooking(int loanId, short approvalStatusId, ApprovalViewModel user)
        {
            var loanRecord = context.TBL_LOAN.Find(loanId);
            var revolvingLoanRecord = context.TBL_LOAN_REVOLVING.Find(loanId);
            var contingentLoanRecord = context.TBL_LOAN_CONTINGENT.Find(loanId);


            if (workflow.NewState != (int)ApprovalState.Ended)
            {
                if (user.operationId == (int)OperationsEnum.TermLoanBooking)
                {
                    if (loanRecord.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing)
                        loanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)OperationsEnum.ContigentLoanBooking)
                {
                    if (contingentLoanRecord.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing)
                        contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)OperationsEnum.RevolvingLoanBooking)
                {
                    if (revolvingLoanRecord.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing)
                        revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

            }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                //...................Updating Loan Application Status....................
                decimal totalBookedAmount = 0;
                
                //=======================================================================

                //...................Updating Loan Tables With Approved State Properties Values....................
                if (user.operationId == (int)OperationsEnum.RevolvingLoanBooking)
                {
                    totalBookedAmount = (from a in context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == revolvingLoanRecord.LOANAPPLICATIONDETAILID)
                                         select a).Sum(s => s.OVERDRAFTLIMIT);

                    if(totalBookedAmount >= revolvingLoanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                    {
                        var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(revolvingLoanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                        loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    revolvingLoanRecord.DATEAPPROVED = DateTime.Now;
                    revolvingLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

                if (user.operationId == (int)OperationsEnum.ContigentLoanBooking)
                {
                    totalBookedAmount = (from a in context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == contingentLoanRecord.LOANAPPLICATIONDETAILID)
                                         select a).Sum(s => s.CONTINGENTAMOUNT);

                    if (totalBookedAmount >= contingentLoanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                    {
                        var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(contingentLoanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                        loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    contingentLoanRecord.DATEAPPROVED = DateTime.Now;
                    contingentLoanRecord.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                }
                //================================================================== 

                if (user.operationId == (int)OperationsEnum.TermLoanBooking )
                {
                    loanRecord.DATEAPPROVED = DateTime.Now;
                    loanRecord.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;

                    totalBookedAmount = (from a in context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == loanRecord.LOANAPPLICATIONDETAILID)
                                         select a).Sum(s => s.PRINCIPALAMOUNT);

                    if (totalBookedAmount >= loanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT)
                    {
                        var loanApplicationRecord = context.TBL_LOAN_APPLICATION.Find(loanRecord.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID);
                        loanApplicationRecord.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LoanBookingCompleted;
                    }

                    //...................Build Schedule Model & Call Schedule Repo Add Function....................
                    var loanScheduleModel = BuildScheduleModel(loanId, user.createdBy);
                    this.loanSchedule.AddLoanSchedule(loanId, loanScheduleModel, user.createdBy);
                    //=================================================================================================

                    //...................Build Disbursement Model & Invoke Loan Disbursement....................
                    var loanDisbursementModel = BuildDisbursementModel(loanId, loanScheduleModel, user.createdBy);
                    DisburseLoan(loanDisbursementModel);

                    loanRecord.LOANSTATUSID = 1;
                    loanRecord.ISDISBURSED = true;
                    loanRecord.DISBURSEDATE = generalSetup.GetApplicationDate();
                    loanRecord.DISBURSEDBY = user.createdBy;
                    loanRecord.APPROVEDBY = user.createdBy;
                    //==========================================================================================


                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LoanBookingApproved,
                        STAFFID = user.staffId,
                        BRANCHID = (short)user.BranchId,
                        DETAIL = $"Approved Loan Booking with code ({loanRecord.LOANREFERENCENUMBER})",
                        IPADDRESS = user.userIPAddress,
                        URL = user.applicationUrl,
                        APPLICATIONDATE = generalSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    this.auditTrail.AddAuditTrail(audit);
                    // Audit Section ---------------------------
                }


            }

            return this.context.SaveChanges() > 0;
        }

        /// <summary>
        /// Builds the schedule model.
        /// </summary>
        /// <param name="targetId">The target identifier.</param>
        /// <param name="createdBy">The created by.</param>
        /// <returns></returns>
        private LoanPaymentScheduleInputViewModel BuildScheduleModel(int targetId, int createdBy)
        {
            List<IrregularLoanScheduleInputViewModel> irregularPaymentScheduleList = new List<IrregularLoanScheduleInputViewModel>();
            var loanIrregularRecord = context.TBL_LOAN_SCHEDULE_IRREGUL_INPUT.Where(x => x.LOANID == targetId);
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

            var applicationDate = generalSetup.GetApplicationDate();
            var maturityDate = applicationDate.AddDays(loanScheduleData.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

            foreach (var record in loanFeeData)
            {
                integraFeeAmount = integraFeeAmount + (double)record.FEEAMOUNT;
            }
            if(loanScheduleData.SCHEDULETYPEID == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanScheduleData.PRINCIPALFREQUENCYTYPEID = null;
                loanScheduleData.INTERESTFREQUENCYTYPEID = null;
                
            }
            var scheduleModel = new LoanPaymentScheduleInputViewModel
            {
                scheduleMethodId = loanScheduleData.SCHEDULETYPEID,

                principalAmount = (double)loanScheduleData.PRINCIPALAMOUNT,
                effectiveDate = applicationDate,
                interestRate = loanScheduleData.INTERESTRATE,
                principalFrequency = loanScheduleData.PRINCIPALFREQUENCYTYPEID,
                interestFrequency = loanScheduleData.INTERESTFREQUENCYTYPEID,
                tenor = (loanScheduleData.MATURITYDATE - loanScheduleData.EFFECTIVEDATE).Days,
                principalFirstpaymentDate = (DateTime)loanScheduleData.FIRSTPRINCIPALPAYMENTDATE,
                interestFirstpaymentDate = (DateTime)loanScheduleData.FIRSTINTERESTPAYMENTDATE,
                maturityDate = maturityDate,
                accurialBasis = loanScheduleData.SCHEDULEDAYCOUNTCONVENTIONID,
                integralFeeAmount = integraFeeAmount,
                firstDayType = loanScheduleData.SCHEDULEDAYINTERESTTYPEID,
                irregularPaymentSchedule = irregularPaymentScheduleList
            };

            return scheduleModel;
        }
     
        /// <summary>
        /// Builds the disbursement model.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="loanInputModel">The loan input model.</param>
        /// <param name="staffId">The staff identifier.</param>
        /// <returns></returns>
        private LoanViewModel BuildDisbursementModel(int loanId, LoanPaymentScheduleInputViewModel loanInputModel, int staffId)
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
                    productTypeId = fee.PRODUCTTYPEID,
                    feeRateValue = fee.FEERATEVALUE,
                    feeDependentAmount = fee.FEERATEVALUE,
                    feeAmount = fee.FEEAMOUNT,
                    isIntegralFee = fee.ISINTEGRALFEE,
                    recurring = fee.ISRECURRING,
                };
                loanChargeFeeList.Add(loanfee);
            };


            var loanRecord = context.TBL_LOAN.Find(loanId);
            if(loanRecord.SCHEDULETYPEID == (short)LoanScheduleTypeEnum.BulletPayment)
            {
                loanRecord.PRINCIPALFREQUENCYTYPEID = null;
                loanRecord.INTERESTFREQUENCYTYPEID = null;
            }
        
            var loanModel = new LoanViewModel
            {
                loanId = loanRecord.TERMLOANID,
                customerId = loanRecord.CUSTOMERID,
                productId = loanRecord.PRODUCTID,
                //productPriceIndexRate = (decimal)loanRecord.ProductPriceIndexRate,
                casaAccountId = loanRecord.CASAACCOUNTID,
                loanApplicationDetailId = (int)loanRecord.LOANAPPLICATIONDETAILID,

                branchId = loanRecord.BRANCHID,
                loanReferenceNumber = loanRecord.LOANREFERENCENUMBER,
                applicationReferenceNumber = loanRecord.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,

                principalFrequencyTypeId = loanRecord.PRINCIPALFREQUENCYTYPEID,
                interestFrequencyTypeId = loanRecord.INTERESTFREQUENCYTYPEID,
                principalNumberOfInstallment = loanRecord.PRINCIPALNUMBEROFINSTALLMENT,
                interestNumberOfInstallment = loanRecord.INTERESTNUMBEROFINSTALLMENT,
                
               // relationshipOfficerId = loanRecord.RelationshipOfficerId,
               // relationshipManagerId = loanRecord.RelationshipManagerId,
               // misCode = loanRecord.MISCode,
               // teamMiscode = loanRecord.TeamMISCode,
                interestRate = loanRecord.INTERESTRATE,
                effectiveDate = generalSetup.GetApplicationDate(),
                maturityDate = generalSetup.GetApplicationDate().AddDays(loanRecord.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR),
                //bookingDate = loanRecord.BookingDate,
                principalAmount = loanRecord.PRINCIPALAMOUNT,
                createdBy = staffId,
                //principalInstallmentLeft = loanRecord.PrincipalInstallmentLeft,
                //interestInstallmentLeft = loanRecord.InterestInstallmentLeft,
                loanStatusId = loanRecord.LOANSTATUSID,
                scheduleTypeId = loanRecord.SCHEDULETYPEID,
                operationId = (int)OperationsEnum.TermLoanBooking,
                customerGroupId = loanRecord.CUSTOMERGROUPID,
                loanTypeId = loanRecord.LOANTYPEID,
                //equityContribution = loanRecord.EquityContribution,
                //subSectorId = loanRecord.SubSectorId,
                firstPrincipalPaymentDate = loanRecord.FIRSTPRINCIPALPAYMENTDATE,
                firstInterestPaymentDate = loanRecord.FIRSTINTERESTPAYMENTDATE,
                //outstandingPrincipal = loanRecord.OutstandingPrincipal,
                //principalAdditionCount = loanRecord.PrincipalAdditionCount,
                //principalReductionCount = loanRecord.PrincipalReductionCount,
                //fixedPrincipal = loanRecord.FixedPrincipal,
                //profileLoan = loanRecord.ProfileLoan,
                //dischargeLetter = loanRecord.DischargeLetter,
                //suspendInterest = loanRecord.SuspendInterest,
                //scheduled = true,
                //isScheduledPrepayment = (bool)loanRecord.IsScheduledPrepayment,
                //scheduledPrepaymentAmount = (decimal)loanRecord.ScheduledPrepaymentAmount,
                //scheduledPrepaymentDate = (DateTime)loanRecord.ScheduledPrepaymentDate,
                //scheduledPrepaymentFrequencyTypeId = (short)loanRecord.ScheduledPrepaymentFrequencyTypeId,
                //customerSensitivityLevelId = loanRecord.CustomerSensitivityLevelId,
                //customerCode = loanRecord.tbl_Customer.CustomerCode,
                companyId = loanRecord.COMPANYID,
                currencyId = loanRecord.CURRENCYID,
                accurialBasis = loanRecord.SCHEDULEDAYCOUNTCONVENTIONID,
                integralFeeAmount = (decimal)loanInputModel.integralFeeAmount,
                firstDayType = loanRecord.SCHEDULEDAYINTERESTTYPEID,

                //loanScheduleInput = loanInputModel,

                //loanCovenant = loanCovenantList,
                loanChargeFee = loanChargeFeeList,
            };

            return loanModel;
        }
      
        /// <summary>
        /// Builds the loan disbursment posting.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public FinanceTransactionViewModel BuildLoanDisbursmentPosting(LoanViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);
            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId && x.COMPANYID == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
            loanTransaction.description = "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CURRENCYID;
            loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.valueDate,loanTransaction.currencyId, model.companyId).sellingRate;
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == product.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = model.principalAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;

            var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = repaymentAccountGL;

            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = model.principalAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            loanTransaction.transactionDetails.Add(debit);
            loanTransaction.transactionDetails.Add(credit);

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
            //foreach ( var model in data.loanChargeFee)
            //{
            //    //model.ledgerAccountId = data.
            //}
            //data.loanChargeFee

            // LoanChargeFeeViewModel model = new LoanChargeFeeViewModel();

            List<FinanceTransactionViewModel> output = new List<FinanceTransactionViewModel>();

            foreach (var item in loanDetails.loanChargeFee)
            {
                FinanceTransactionViewModel feeTransaction = new FinanceTransactionViewModel();


                var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanDetails.casaAccountId);

                feeTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
                feeTransaction.description = "Fee charge";
                feeTransaction.valueDate = generalSetup.GetApplicationDate();
                feeTransaction.transactionDate = feeTransaction.valueDate;
                feeTransaction.currencyId = casa.CURRENCYID;
                feeTransaction.currencyRate = financeTransaction.GetExchangeRate(feeTransaction.valueDate, feeTransaction.currencyId, loanDetails.companyId).sellingRate;
                feeTransaction.isApproved = true;
                feeTransaction.postedBy = loanDetails.createdBy;
                feeTransaction.approvedBy = loanDetails.createdBy;
                feeTransaction.approvedDate = feeTransaction.transactionDate;
                feeTransaction.approvedDateTime = DateTime.Now;
                feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                feeTransaction.companyId = loanDetails.companyId;

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = (decimal)item.feeAmount;
                debit.creditAmount = 0;
                debit.sourceBranchId = loanDetails.branchId;
                debit.destinationBranchId = casa.BRANCHID;

                var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == item.chargeFeeId).Select(x => x.GLACCOUNTID).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;
                credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)item.feeAmount;
                credit.sourceBranchId = loanDetails.branchId;
                credit.destinationBranchId = loanDetails.branchId;


                if (item.feeAmount != 0)
                {
                    feeTransaction.transactionDetails.Add(debit);
                    feeTransaction.transactionDetails.Add(credit);
                    output.Add(feeTransaction);
                }


            }

            // Audit Section ---------------------------            

            return output;

        }
      
        /// <summary>
        /// Adds the loan covenant.
        /// </summary>
        /// <param name="covenantModel">The covenant model.</param>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private bool AddLoanCovenant(List<LoanCovenantDetailViewModel> covenantModel, int loanApplicationId, int loanId, short productTypeId)
        {
            foreach (LoanCovenantDetailViewModel entity in covenantModel)
            {
                var covenant = new TBL_LOAN_COVENANT_DETAIL
                {
                    COMPANYID = entity.companyId,
                    COVENANTAMOUNT = entity.covenantAmount,
                    COVENANTDATE = entity.covenantDate,
                    COVENANTDETAIL = entity.covenantDetail,
                    COVENANTTYPEID = entity.covenantTypeId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = this.generalSetup.GetApplicationDate().Date,
                    FREQUENCYTYPEID = entity.frequencyTypeId,
                    LOANID = loanId,
                    PRODUCTTYPEID = productTypeId
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
            var guarantor = new TBL_LOAN_GUARANTOR
            {
                PRODUCTTYPEID = productTypeId,
                LOANAPPLICATIONID = loanApplicationId,
                FIRSTNAME = entity.firstname,
                LASTNAME = entity.lastname,
                MIDDLENAME = entity.middlename,
                ADDRESS = entity.address,
                PHONENUMBER1 = entity.phoneNumber1,
                PHONENUMBER2 = entity.phoneNumber2,
                RELATIONSHIP = entity.relationship,
                RELATIONSHIPDURATION = (short)entity.relationshipDuration,
                BVN = entity.bvn,
                REGISTRATION_NUMBER = entity.rcNumber,
                TAX_NUMBER = entity.rcNumber,
                CUSTOMERTYPEID = entity.customerTypeId,
                EMAILADDRESS = entity.emailAddress,
                CREATEDBY = 1,
                DATETIMECREATED = generalSetup.GetApplicationDate()
            };
            context.TBL_LOAN_GUARANTOR.Add(guarantor);
            //return context.SaveChanges() > 0
            return true;
        }
     
        /// <summary>
        /// Adds the loan covenant.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private ICollection<TBL_LOAN_COVENANT_DETAIL> AddLoanCovenant(LoanCovenantDetailViewModel entity, short productTypeId)
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
                PRODUCTTYPEID = productTypeId,
                COVENANTTYPEID = entity.covenantTypeId,
                FREQUENCYTYPEID = entity.frequencyTypeId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate(),
            });

            return covenant;
        }
      
        /// <summary>
        /// Adds the loan collateral mapping.
        /// </summary>
        /// <param name="collateralModel">The collateral model.</param>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <returns></returns>
        public bool AddLoanCollateralMapping(List<LoanCollateralMappingViewModel> collateralModel, int loanApplicationId)
        {

            foreach (LoanCollateralMappingViewModel entity in collateralModel)
            {
                var collateral = new TBL_LOAN_COLLATERAL_MAPPING
                {
                    COLLATERALCUSTOMERID = entity.collateralId,
                    LOANAPPLICATIONID = loanApplicationId,
                    ISRELEASED = false,
                };
                context.TBL_LOAN_COLLATERAL_MAPPING.Add(collateral);
            }

            //return context.SaveChanges() > 0;
            return true;

        }
    
        /// <summary>
        /// Adds the loan fees.
        /// </summary>
        /// <param name="feeModel">The fee model.</param>
        /// <param name="loanId">The loan identifier.</param>
        /// <param name="productTypeId">The product type identifier.</param>
        /// <returns></returns>
        private void AddLoanFees(List<LoanChargeFeeViewModel> feeModel, int loanId, short productTypeId, int companyId, bool feeOverride)
        {
            var feeAmount = 0;
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
                    PRODUCTTYPEID = productTypeId,
                    ISRECURRING = ent.recurring, 
                    RECURRINGPAYMENTDAY = 28,
                    CREATEDBY = ent.createdBy,
                    DATETIMECREATED = DateTime.Now.Date,
                    ISPOSTED = ent.isPosted
                };
                if (feeOverride && ent.isPosted)
                    throw new Exception("Fee posted must be must be disabled for fee override until after approval");

                context.TBL_LOAN_FEE.Add(fee);
                if (feeOverride)
                {
                    context.SaveChanges();
                    var approvalModel = new ForwardViewModel
                    {
                        createdBy = fee.CREATEDBY,
                        companyId = companyId,
                        applicationId = fee.LOANCHARGEFEEID,
                        comment = "Please approve this fee ",
                        amount = fee.FEEAMOUNT,
                    };
                    LogApproval(approvalModel, (int)OperationsEnum.LoanBookingFeeDeferral, false, (int)ApprovalStatusEnum.Pending);
                } else
                {
                    context.SaveChanges();
                }
                
            }

            //return context.SaveChanges() > 0;
        }
    
        /// <summary>
        /// Gets all loans.
        /// </summary>
        /// <returns></returns>
        private IQueryable<LoanViewModel> GetAllLoans()
        {
            var data = (from l in context.TBL_LOAN
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
                            //tenor = (l.MaturityDate - l.EffectiveDate).Days, // returning error

                            principalFrequencyTypeId = (short)l.PRINCIPALFREQUENCYTYPEID,
                            pricipalFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,
                            interestFrequencyTypeId = (short)l.INTERESTFREQUENCYTYPEID,
                            interestFrequencyTypeName = l.TBL_FREQUENCY_TYPE.DESCRIPTION,

                            principalNumberOfInstallment = l.PRINCIPALNUMBEROFINSTALLMENT,
                            interestNumberOfInstallment = l.INTERESTNUMBEROFINSTALLMENT,

                            relationshipOfficerId = l.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = l.TBL_STAFF.FIRSTNAME + " " + l.TBL_STAFF.MIDDLENAME + " " + l.TBL_STAFF.LASTNAME,
                            relationshipManagerId = l.RELATIONSHIPMANAGERID,
                            relationshipManagerName = l.TBL_STAFF1.FIRSTNAME + " " + l.TBL_STAFF1.MIDDLENAME + " " + l.TBL_STAFF1.LASTNAME,
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

                            approvedAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,

                            //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                            operationId = l.OPERATIONID,
                            operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == l.OPERATIONID).OPERATIONNAME,
                            productAccountNumber = l.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                            productAccountName = l.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                            subSectorName = l.TBL_SUB_SECTOR.NAME,
                            sectorName = l.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            customerGroupId = l.CUSTOMERGROUPID,
                            loanTypeId = l.LOANTYPEID,
                            loanTypeName = l.TBL_LOAN_TYPE.LOANTYPENAME,
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
                            customerSensitivityLevelId = l.CUSTOMERSENSITIVITYLEVELID,
                            createdBy = l.CREATEDBY,
                            dateTimeCreated = l.DATETIMECREATED,
                            isCamsol = context.TBL_LOAN_CAMSOL.Any(x => x.LOANID == l.TERMLOANID),
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
  
        /// <summary>
        /// Gets the loan by customer.
        /// </summary>
        /// <param name="customerId">The customer identifier.</param>
        /// <returns></returns>
        public IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId)
        {
            var data = GetAllLoans().Where(l => l.customerId == customerId).GroupBy(x => x.loanId).Select(g => g.FirstOrDefault());
            return data;
               
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
                    productName = c.productAccountName,
                    terminationDate = c.maturityDate
                })
                .AsQueryable();

            return loans;
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
                        customerGroupId = data.CUSTOMERGROUPID,
                        loanTypeId = data.LOANTYPEID,
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
                        customerSensitivityLevelId = data.CUSTOMERSENSITIVITYLEVELID,
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
                        customerGroupId = data.CUSTOMERGROUPID,
                        loanTypeId = data.LOANTYPEID,
                        equityContribution = data.EQUITYCONTRIBUTION,
                        firstPrincipalPaymentDate = data.FIRSTPRINCIPALPAYMENTDATE,
                        outstandingPrincipal = data.OUTSTANDINGPRINCIPAL,
                        principalAdditionCount = data.PRINCIPALADDITIONCOUNT,
                        principalReductionCount = data.PRINCIPALREDUCTIONCOUNT,
                        fixedPrincipal = data.FIXEDPRINCIPAL,
                        profileLoan = data.PROFILELOAN,
                        dischargeLetter = data.DISCHARGELETTER,
                        suspendInterest = data.SUSPENDINTEREST,
                        customerSensitivityLevelId = data.CUSTOMERSENSITIVITYLEVELID,
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
                    productAccountNumber = o.TBL_CASA.PRODUCTACCOUNTNUMBER,
                    productAccountName = o.TBL_CASA.PRODUCTACCOUNTNAME,
                    outstandingPrincipal = o.OUTSTANDINGPRINCIPAL,
                    outstandingInterest = o.OUTSTANDINGINTEREST,
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
                loans = loans.Where(x => x.productAccountNumber == searchModel.productAccountNumber);
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

            loans.OrderBy(x => x.productAccountNumber).ThenBy(x => x.productAccountName);

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
                            covenantTypeId = a.COVENANTTYPEID,
                            frequencyTypeId = a.FREQUENCYTYPEID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE

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
                            feeIntervalName = c.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME

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
        /// Gets the loan charge fee.
        /// </summary>
        /// <param name="loanId">The loan identifier.</param>
        /// <returns></returns>
        public List<LoanGuarantorViewModel> GetLoanGuarantors(int loanApplicationId )
        {
            var data = (from c in context.TBL_LOAN_GUARANTOR
                        where c.LOANAPPLICATIONID == loanApplicationId
                        select new LoanGuarantorViewModel
                        {
                            loanGuarantorId = c.LOANGUARANTORID,
                            loanApplicationId = (int)c.LOANAPPLICATIONID,
                            firstname = c.FIRSTNAME,
                            lastname = c.LASTNAME,
                            middlename = c.MIDDLENAME,
                            address = c.ADDRESS,
                            phoneNumber1 = c.PHONENUMBER1,
                            phoneNumber2 = c.PHONENUMBER2,
                            relationship = c.RELATIONSHIP,
                            relationshipDuration = (short)c.RELATIONSHIPDURATION,
                            bvn = c.BVN,
                            taxNumber = c.TAX_NUMBER,
                            rcNumber = c.REGISTRATION_NUMBER,
                            customerTypeId = c.CUSTOMERTYPEID,
                            customerTypeName = c.TBL_CUSTOMER_TYPE.NAME,
                            emailAddress = c.EMAILADDRESS,
                            fullName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME

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
                       isInvestmentGrade  = a.ISINVESTMENTGRADE ,
                        isRealatedParty = a.ISREALATEDPARTY,
                       isPoliticallyExposed = a.ISPOLITICALLYEXPOSED ,
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
                           numberOfShares = s.NUMBEROFSHARES,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                           numberOfShares = s.NUMBEROFSHARES,
                           isPoliticallyExposed = s.ISPOLITICALLYEXPOSED,
                           bankVerificationNumber = s.CUSTOMERBVN,
                           companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                           companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                           isLocationBased = x.ISLOCATIONBASED,
                           valuationCycle = x.VALUATIONCYCLE,
                           haircut = x.HAIRCUT,
                           approvalStatus = x.APPROVALSTATUS,
                       }).ToList(),
                   };

        }

        /// <summary>
        /// Gets the appraisal memorandum collateral changes.
        /// </summary>
        /// <param name="loanApplicationId">The loan application identifier.</param>
        /// <returns></returns>
        //public IEnumerable<LoanApplicationCollateralViewModel> GetAppraisalMemorandumCollateralChanges(int loanApplicationId)
        //{
        //    var data = (from lac in context.TBL_LOAN_APPLICATION_COLLATERAL
        //                where lac.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loanApplicationId && lac.DELETED == false
        //                select new LoanApplicationCollateralViewModel()
        //                {
        //                    customerCollateralId = lac.CUSTOMERCOLLATERALID,
        //                    latitude = lac.LATITUDE,
        //                    longitude = lac.LONGITUDE,
        //                    nearestBusStop = lac.NEARESTBUSSTOP,
        //                    nearestLandmark = lac.NEARESTLANDMARK,
        //                    locationAddress = lac.LOCATIONADDRESS,
        //                    documentTitle = lac.DOCUMENTTITLE,
        //                    otherInformations = lac.OTHERINFORMATIONS
        //                });
        //    return data;
        //}
      
        /// <summary>
        /// Gets the appraisal memorandum loan updates.
        /// </summary>
        /// <param name="appraisalMemorandumId">The appraisal memorandum identifier.</param>
        /// <returns></returns>
        public AppraisalMemorandumLoanDetailViewModel GetAppraisalMemorandumLoanUpdates(int appraisalMemorandumId)
        {
            return (from data in context.TBL_CREDIT_APPRAISAL_MEMO_DETL
                    where data.APPRAISALMEMORANDUMID == appraisalMemorandumId
                    orderby data.APPRAISALMEMORANDUMLOANDETAILID descending
                    select new AppraisalMemorandumLoanDetailViewModel()
                    {
                        interestRate = data.INTERESTRATE,
                        principalAmount = data.PRINCIPALAMOUNT,
                        tenor = data.TENOR,
                    }).FirstOrDefault();
        }


     
        /// <summary>
        /// Gets the appraisal memorandum processed loan applications.
        /// </summary>
        /// <param name="companyId">The company identifier.</param>
        /// <returns></returns>
        public IEnumerable<CamProcessedLoanViewModel> GetAppraisalMemorandumProcessedLoanApplications(int companyId)
        {
            var data = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                        join m in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals m.LOANAPPLICATIONID
                        join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                        where m.COMPANYID == companyId && d.DELETED == false && m.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.AvailmentCompleted
                        select new CamProcessedLoanViewModel
                        {
                            approvalStatusId = m.APPROVALSTATUSID,
                            loanApplicationId = m.LOANAPPLICATIONID,
                            loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                            applicationReferenceNumber = m.APPLICATIONREFERENCENUMBER,
                            //// casaAccountId = m.CasaAccountId,
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
                            companyInformation = (from a in context.TBL_CUSTOMER_COMPANYINFOMATION where a.CUSTOMERID == d.CUSTOMERID
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
                                                       companyDiretcors =(from b in context.TBL_CUSTOMER_COMPANY_DIRECTOR where b.CUSTOMERID == a.CUSTOMERID 
                                                                          && ((b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember) 
                                                                                    || (b.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember_Shareholder) )
                                                                          select new CustomerCompanyDirectorsViewModels
                                                                          {
                                                                             numberOfShares = b.NUMBEROFSHARES,
                                                                             companyDirectorTypeName = b.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
                                                                             fullname = b.FIRSTNAME +" "+ b.SURNAME,
                                                                             isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                                                          }).ToList(),
                                                      companyShareholders = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                                                             where e.CUSTOMERID == a.CUSTOMERID 
                                                                             && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder
                                                                             select new CustomerCompanyShareholdersViewModels
                                                                          {
                                                                              numberOfShares = e.NUMBEROFSHARES,
                                                                              companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
                                                                              fullname = e.FIRSTNAME + " " + e.SURNAME,
                                                                              isPoliticallyExposed = e.ISPOLITICALLYEXPOSED,
                                                                          }).ToList(),
                                                      companyAccountSignatories = (from e in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                                                             where e.CUSTOMERID == a.CUSTOMERID
                                                                             && e.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Account_Signatory
                                                                             select new CustomerCompanyAccountSignatoryViewModels
                                                                             {
                                                                                 numberOfShares = e.NUMBEROFSHARES,
                                                                                 companyDirectorTypeName = e.TBL_CUSTOMER_COMPANY_DIREC_TYPE.COMPANYDIRECTORYTYPENAME,
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
                            effectiveDate = (DateTime)m.EFFECTIVEDATE,
                            expiryDate = (DateTime)m.EXPIRYDATE,
                            relationshipOfficerId = m.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = m.TBL_STAFF.FIRSTNAME + " " + m.TBL_STAFF.MIDDLENAME + " " + m.TBL_STAFF.LASTNAME,
                            relationshipManagerId = m.RELATIONSHIPMANAGERID,
                            relationshipManagerName = m.TBL_STAFF1.FIRSTNAME + " " + m.TBL_STAFF1.MIDDLENAME + " " + m.TBL_STAFF1.LASTNAME,

                            currencyId = d.CURRENCYID,
                            currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                            exchangeRate = d.EXCHANGERATE,
                            loanTypeId = m.LOANTYPEID,
                            loanTypeName = m.TBL_LOAN_TYPE.LOANTYPENAME,
                            camReference = m.TBL_CREDIT_APPRAISAL_MEMORANDUM.FirstOrDefault().CAMREF,
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
                                                      || d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.SelfLiquidating)
                             ? (d.APPROVEDAMOUNT - d.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.PRINCIPALAMOUNT)) :
                            (
                                (d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.RevolvingLoan)
                                        ? (d.APPROVEDAMOUNT - d.TBL_LOAN_REVOLVING
                                            .Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.OVERDRAFTLIMIT)) :
                                (d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability
                                        ? (d.APPROVEDAMOUNT - d.TBL_LOAN_CONTINGENT
                                            .Where(tl => tl.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID).Sum(s => s.CONTINGENTAMOUNT)) :
                                            0)
                            ),
                            approvedTenor = d.APPROVEDTENOR,
                            createdBy = m.CREATEDBY,
                            applicationDate =m.APPLICATIONDATE,
                            dateTimeCreated = d.DATETIMECREATED,

                            loanPreliminaryEvaluationId = m.LOANPRELIMINARYEVALUATIONID ?? 0,
                            loanGuarantor = (from g in context.TBL_LOAN_GUARANTOR.Where(x => x.LOANAPPLICATIONID == m.LOANAPPLICATIONID)
                                             select (
                                                      new LoanGuarantorViewModel
                                                      {
                                                          loanGuarantorId = g.LOANGUARANTORID,
                                                          firstname = g.FIRSTNAME,
                                                          lastname = g.LASTNAME,
                                                          middlename = g.MIDDLENAME,
                                                          fullName = g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME,
                                                          emailAddress = g.EMAILADDRESS,
                                                          phoneNumber1 = g.PHONENUMBER1,
                                                          phoneNumber2 = g.PHONENUMBER2,
                                                          address = g.ADDRESS,
                                                          bvn = g.BVN,
                                                          relationship = g.RELATIONSHIP,
                                                          rcNumber = g.REGISTRATION_NUMBER,
                                                          taxNumber = g.TAX_NUMBER,
                                                          customerTypeId = g.CUSTOMERTYPEID,
                                                          customerTypeName = g.TBL_CUSTOMER_TYPE.NAME,
                                                          relationshipDuration = g.RELATIONSHIPDURATION
                                                      })).ToList(),

                            loanCollateral = (from cm in context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANAPPLICATIONID == m.LOANAPPLICATIONID)
                                              select (
                                                       new LoanCollateralMappingViewModel
                                                       {
                                                           loanCollateralMappingId = cm.LOANCOLLATERALMAPPINGID,
                                                           collateralTypeName = cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME 
                                                           + "(" + cm.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.FirstOrDefault().COLLATERALSUBTYPENAME +")" ,
                                                           collateralCustomerId = cm.COLLATERALCUSTOMERID,
                                                           loanApplicationId = cm.LOANAPPLICATIONID,
                                                           collateralValue = cm.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                           hairCut = cm.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                           valuationCycle = cm.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                           currencyId = cm.TBL_COLLATERAL_CUSTOMER.CURRENCYID,
                                                           currencyCode = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                           currency = cm.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYNAME
                                                       })).ToList(),
                        }).ToList();

             data = (from a in data where ((a.customerAvailableAmount >= 0) || (a.customerAvailableAmount == null)) select a).ToList();
           
            foreach (var item in data)
            {
                if(item.customerAvailableAmount != 0)
                {
                    if (!item.customerAvailableAmount.HasValue)
                        item.customerAvailableAmount = item.approvedAmount;
                }
                
            }


            return data.ToList();
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
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
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
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
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
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                //loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }
     

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int companyId)
        {

            try
            {
                List<CurrentCustomerExposure> datalst = new List<CurrentCustomerExposure>();

                foreach (var item in  customer)
                {
                    
                          var data = (from a in context.TBL_LOAN
                                      where
                                        a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.APPROVALSTATUSID == (int)LoanStatusEnum.Active
                                      select new CurrentCustomerExposure
                                      {
                                          facilityType = a.TBL_PRODUCT.PRODUCTNAME,

                                          existingLimit = a.PRINCIPALAMOUNT,

                                          proposedLimit = a.OUTSTANDINGINTEREST,
                                          PastDueObligationsInterest = ((System.Decimal?)(
                                        a.ALLOWFORCEDEBITREPAYMENT == false ? (System.Decimal?)
                                          (from c in context.TBL_LOAN_FORCE_DEBIT
                                           where c.LOANID == a.TERMLOANID && c.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                           select new
                                           {
                                               DebitRepayment = (c.DEBITAMOUNT - c.CREDITAMOUNT)
                                           }).Sum(p => p.DebitRepayment) :
                                        a.ALLOWFORCEDEBITREPAYMENT == false ? (System.Decimal?)
                                          (from c in context.TBL_LOAN_FORCE_DEBIT
                                           where c.LOANID == a.TERMLOANID &&
                                              c.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                           select new
                                           {
                                               DebitRepayment = (c.DEBITAMOUNT - c.CREDITAMOUNT)
                                           }).Sum(p => p.DebitRepayment) : null) ?? (System.Decimal?)0 ?? 0),
                                          PastDueObligationsPrincipal = ((System.Decimal?)(
                                        a.ALLOWFORCEDEBITREPAYMENT == false ?
                                          (from c in context.TBL_LOAN_FORCE_DEBIT
                                           where c.LOANID == a.TERMLOANID && c.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Principal
                                           select new
                                           {
                                               DebitRepayment = (c.DEBITAMOUNT - c.CREDITAMOUNT)
                                           }).Sum(p => p.DebitRepayment) :
                                        a.ALLOWFORCEDEBITREPAYMENT == false ? (System.Decimal?)
                                          (from c in context.TBL_LOAN_FORCE_DEBIT
                                           where c.LOANID == a.TERMLOANID && c.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Principal
                                           select new
                                           {
                                               DebitRepayment = (c.DEBITAMOUNT - c.CREDITAMOUNT)
                                           }).Sum(p => p.DebitRepayment) : null) ?? (System.Decimal?)0 ?? 0),
                                          reviewDate = DateTime.Now
                                      });
                    if (data.Count() > 0)
                        datalst.AddRange(data.ToList());
                }

                return datalst;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
   
        /// <summary>
        /// Searches for loan.
        /// </summary>
        /// <param name="searchQuery">The search query.</param>
        /// <returns></returns>
        public IQueryable<LoanViewModel> SearchForLoan(string searchQuery)
        {
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
                                       where (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                       b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                       b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                       b.LASTNAME.ToLower().Contains(searchQuery) ||
                                       c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
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
                                           operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                           productAccountNumber = a.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                           productAccountName = a.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME,
                                           customerGroupId = a.CUSTOMERGROUPID,
                                           loanTypeId = a.LOANTYPEID,
                                           loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
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
                                           customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                                           createdBy = a.CREATEDBY,
                                           dateTimeCreated = a.DATETIMECREATED,
                                           isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                           exchangeRate = a.EXCHANGERATE,
                                           currencyId = a.CURRENCYID,
                                           currency = a.TBL_CURRENCY.CURRENCYNAME
                                       }).Take(10).AsQueryable();
                }
                return allFilteredLoan;
            }
            catch (System.Exception ex)
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
    }


}

