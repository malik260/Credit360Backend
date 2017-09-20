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
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using XLeratorDLL_financial;

namespace FintrakBanking.Repositories.Credit
{
    using FinancialTypes = XLeratorDLL_financial.FinancialTypes;
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;

    public class LoanRepository : ILoanRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkFlowRepository workFlow;
        private ILoanScheduleRepository loanSchedule; 
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private IApprovalLevelStaffRepository level;


        public LoanRepository(FinTrakBankingContext _context,IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail, ILoanScheduleRepository _loanSchedule,
                                        IWorkFlowRepository _workFlow, ILoanCovenantRepository _loanCovenant, 
                                        IFinanceTransactionRepository _financeTransaction, IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.loanSchedule = _loanSchedule;
            this.workFlow = _workFlow;
            this.loanCovenant = _loanCovenant;
            this.financeTransaction = _financeTransaction;
            this.level = _level;
        }

  
        public IEnumerable<LookupViewModel> GetAllLoanTypes()
        {
            return (from data in context.tbl_Loan_Type
                    select new LookupViewModel()
                    {
                        lookupId = data.LoanTypeId,
                        lookupName = data.LoanTypeName
                    });
        }

 

        private string GenerateLoanReferenceNumber(int customerId, int productId)
        {
            var customerCode = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId).CustomerCode;
            var productCode = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productId).ProductCode;
            var data = ((this.context.tbl_Loan.Count(x => x.CustomerId == customerId && x.ProductId == productId)) + 1);
            return $"{customerCode}-{productCode}-{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }



        //private int GetLoanTypeBatchId(LoanTypeEnum loanTypeId, int customerId, int customerGroupId, decimal groupAmount)
        //{
        //    if (loanTypeId == LoanTypeEnum.Single)
        //    { return 1; }
        //    else if (loanTypeId == LoanTypeEnum.Batch)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerId == customerId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerId = customerId,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }
        //    else if (loanTypeId == LoanTypeEnum.CustomerGroup)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerGroupId == customerGroupId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerGroupId = customerGroupId,
        //                GroupAmount = groupAmount,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }

        //    return -1;

        //}





        //private int GetLoanTypeBatchId(LoanTypeEnum loanTypeId, int customerId, int customerGroupId, decimal groupAmount)
        //{
        //    if (loanTypeId == LoanTypeEnum.Single)
        //    { return 1; }
        //    else if (loanTypeId == LoanTypeEnum.Batch)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerId == customerId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerId = customerId,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }
        //    else if (loanTypeId == LoanTypeEnum.CustomerGroup)
        //    {
        //        int? loanInfo = (from data in context.tbl_Loan
        //                         where data.CustomerGroupId == customerGroupId && data.LoanTypeId == (short)loanTypeId && data.LoanStatusId == (short)LoanStatusEnum.Inactive
        //                         select data.LoanTypeBatchId).FirstOrDefault();

        //        if (loanInfo.HasValue)
        //            return loanInfo.Value;
        //        else
        //        {
        //            var data = new tbl_Loan_Type_Batch()
        //            {
        //                LoanTypeBatchCode = GenerateLoanTypeBatchCode(),
        //                CustomerGroupId = customerGroupId,
        //                GroupAmount = groupAmount,
        //                LoanTypeId = (short)loanTypeId,
        //                DateCreated = generalSetup.GetApplicationDate()
        //            };

        //            this.context.tbl_Loan_Type_Batch.Add(data);

        //            context.SaveChanges();

        //            return data.LoanTypeBatchId;
        //        }
        //    }

        //    return -1;

        //}


        public async Task<string> AddLoanBooking(LoanViewModel entity)
        {
            //...................CHECK IF THE LOAN RECORD IS A SCHEDULED LOAN..................//
            if (entity.productTypeId == (int)ScheduleTypeEnum.TermLoan || entity.productTypeId == (int)ScheduleTypeEnum.SelfLiquidating)
            {
                return this.AddTermLoan(entity).Result;
            }
            // ...............CHECK IF THE LOAN RECORD IS ANON SCHEDULED LOAN....................//
            else if (entity.productTypeId != (int)ScheduleTypeEnum.RevolvingLoan)
            {
                var revolvingLoanInput = entity.revolvingLoanInput;

                revolvingLoanInput.companyId = entity.companyId;
                revolvingLoanInput.createdBy = entity.createdBy;
                revolvingLoanInput.branchId = entity.branchId;

                return await addRevolvingLoan(revolvingLoanInput);
            }
            else if (entity.productTypeId != (int)ScheduleTypeEnum.ContingentLiability)
            {
                var contingentLoanInput = entity.contingentLoanInput;

                contingentLoanInput.companyId = entity.companyId;
                contingentLoanInput.createdBy = entity.createdBy;
                contingentLoanInput.branchId = entity.branchId;

                return await addContingentLiability(contingentLoanInput);
            }
            {
                return "The Product type is Invalid";
            }
        }

        private async Task<string> addRevolvingLoan(RevolvingLoanViewModel entity)
        {
            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);
            var data = new tbl_Loan_Revolving
            {

                RevolvingLoanId = entity.loanId,
                CustomerId = entity.customerId,
                ProductId = entity.productId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,
                CurrencyId = entity.currencyId,
                ExchangeRate = entity.exchangeRate,
                LoanApplicationId = entity.loanApplicationId,
                LoanReferenceNumber = entity.loanReferenceNumber,
                SubSectorId = entity.subSectorId,
                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMisCode,
                InterestRate = entity.interestRate,
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,
                BookingDate = entity.bookingDate,
                OverdraftLimit = entity.overdraftLimit,
                ApprovedAmount = entity.approvedAmount,
                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                LoanStatusId = entity.loanStatusId,
                IsDisbursed = false,
                OperationId = (int)OperationsEnum.TermLoanBooking,
                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                TrancheBatchCode = entity.trancheBatchCode,
                DischargeLetter = false,
                SuspendInterest = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
                InternalPrudentialGuidelineStatusId = entity.internalPrudentialGuidelineStatusId,
                ExternalPrudentialGuidelineStatusId = entity.externalPrudentialGuidelineStatusId,
                NPLDate = entity.nplDate,
                CompanyId = entity.companyId

            };

            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.TermLoanBooking, entity.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {

                        var loan = context.tbl_Loan_Revolving.Add(data);
                        AddLoanCovenantDetail(entity.loanCovenant, loan.RevolvingLoanId, (short)entity.productTypeId);
                        AddLoanGuarantor(entity.loanGuarantor, loan.RevolvingLoanId, (short)entity.productTypeId);
                        AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId, loan.RevolvingLoanId, (short)entity.productTypeId);
                        AddLoanFees(entity.loanChargeFee, loan.RevolvingLoanId, (short)entity.productTypeId);
                        context.tbl_Audit.Add(audit);

                        var dataCount = context.SaveChanges();

                        var approvalModel = new ApprovalViewModel
                        {
                            staffId = entity.createdBy,
                            companyId = entity.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = loan.RevolvingLoanId,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            BranchId = entity.userBranchId
                        };
                        var response = await workFlow.LogForApproval(approvalModel);
                        trans.Commit();


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
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

        }

        private async Task<string> addContingentLiability(ContingentLoanViewModel entity)
        {
            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);
            var data = new tbl_Loan_Contingent
            {
                ContingentLoanId = entity.loanId,
                CustomerId = entity.customerId,
                ProductId = entity.productId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,
                CurrencyId = entity.currencyId,
                ExchangeRate = entity.exchangeRate,
                LoanApplicationId = entity.loanApplicationId,
                LoanReferenceNumber = entity.loanReferenceNumber,
                SubSectorId = entity.subSectorId,
                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMisCode,
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,
                BookingDate = entity.bookingDate,
                ApprovedAmount = entity.approvedAmount,
                ContingentAmount = entity.contingentAmount,
                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                LoanStatusId = entity.loanStatusId,
                IsDisbursed = false,
                OperationId = (int)OperationsEnum.TermLoanBooking,
                CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                TrancheBatchCode = entity.trancheBatchCode,
                DischargeLetter = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,
 

            };


            //Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.TermLoanBooking, entity.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {

                        var loan = context.tbl_Loan_Contingent.Add(data);
                        AddLoanCovenantDetail(entity.loanCovenant, loan.ContingentLoanId, (short)entity.productTypeId);
                        AddLoanGuarantor(entity.loanGuarantor, loan.ContingentLoanId, (short)entity.productTypeId);
                        AddLoanCollateralMapping(entity.loanCollateral, entity.loanApplicationId, loan.ContingentLoanId, (short)entity.productTypeId);
                        AddLoanFees(entity.loanChargeFee, loan.ContingentLoanId, (short)entity.productTypeId);
                        context.tbl_Audit.Add(audit);

                        var dataCount = context.SaveChanges();

                        var approvalModel = new ApprovalViewModel
                        {
                            staffId = entity.createdBy,
                            companyId = entity.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = loan.ContingentLoanId,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            BranchId = entity.userBranchId
                        };
                        var response = await workFlow.LogForApproval(approvalModel);
                        trans.Commit();


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
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }
        }

        private async Task<string> AddTermLoan(LoanViewModel entity)
        {
            if (entity.loanScheduleInput.maturityDate <= entity.loanScheduleInput.effectiveDate)
                throw new Exception("Loan terminal date should be more than effective date");

            var loanReferenceNumber = GenerateLoanReferenceNumber(entity.customerId, entity.productId);

            var data = new tbl_Loan
            {
                LoanApplicationId = entity.loanApplicationId,
                LoanReferenceNumber = loanReferenceNumber,
                LoanStatusId = (short)LoanStatusEnum.Inactive,
                IsDisbursed = false,
                PrincipalNumberOfInstallment = 0, // loanSchedule.CalculateNumberOfInstallments((TenorModeEnum)entity.tenorModeId, entity.principalFrequencyTypeId, entity.tenor),
                InterestNumberOfInstallment = 0, // loanSchedule.CalculateNumberOfInstallments((TenorModeEnum)entity.tenorModeId, entity.interestFrequencyTypeId, entity.tenor),
                //IsScheduledPrepayment = null,
                ScheduledPrepaymentAmount = entity.scheduledPrepaymentAmount,
                ScheduledPrepaymentFrequencyTypeId = null,

                // CustomerGroupId = entity.customerGroupId,
                LoanTypeId = entity.loanTypeId,
                SubSectorId = entity.subSectorId,
                CurrencyId = (short)entity.currencyId,

                DischargeLetter = false,
                SuspendInterest = false,

                CustomerId = entity.customerId,
                ProductId = (short)entity.productId,
                CompanyId = entity.companyId,
                CasaAccountId = entity.casaAccountId,
                BranchId = entity.branchId,

                PrincipalFrequencyTypeId = entity.loanScheduleInput.principalFrequency,
                InterestFrequencyTypeId = entity.loanScheduleInput.interestFrequency,

                RelationshipOfficerId = entity.relationshipOfficerId,
                RelationshipManagerId = entity.relationshipManagerId,
                MISCode = entity.misCode,
                TeamMISCode = entity.teamMiscode,
                InterestRate = entity.interestRate,
                PrincipalAmount = entity.principalAmount,

                PrincipalInstallmentLeft = 0,
                InterestInstallmentLeft = 0,

                ScheduleTypeId = entity.loanScheduleInput.scheduleMethodId,
                ApprovedAmount = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),
                OperationId = (int)OperationsEnum.TermLoanBooking,
                TrancheBatchCode = entity.trancheBatchCode,

                EquityContribution = 0,
                OutstandingPrincipal = Convert.ToDecimal(entity.loanScheduleInput.principalAmount),
                PrincipalAdditionCount = 0,
                PrincipalReductionCount = 0,
                FixedPrincipal = false,
                ProfileLoan = false,
                CustomerSensitivityLevelId = entity.customerSensitivityLevelId,

                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,

                BookingDate = entity.bookingDate,
                CreatedBy = entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate(),
                EffectiveDate = entity.effectiveDate,
                MaturityDate = entity.maturityDate,
                FirstPrincipalPaymentDate = entity.loanScheduleInput.principalFirstpaymentDate,
                FirstInterestPaymentDate = entity.loanScheduleInput.interestFirstpaymentDate,
                AllowForceDebitRepayment = false,

            };

            if (entity.customerGroupId > 0)
            {
                data.CustomerGroupId = entity.customerGroupId;
            }


            //try
            //{
            //    LoanPaymentScheduleInputViewModel loanScheduleInput = new LoanPaymentScheduleInputViewModel()
            //    {
            //        accurialBasis = entity.accurialBasis,
            //        effectiveDate = entity.effectiveDate,
            //        firstDayType = entity.firstDayType,
            //        integralFeeAmount = entity.integralFeeAmount,
            //        interestFirstpaymentDate = (DateTime)entity.firstInterestPaymentDate,
            //        interestFrequency = entity.interestFrequencyTypeId,
            //        interestRate = entity.interestRate,
            //        maturityDate = entity.maturityDate,
            //        principalAmount = (double)entity.principalAmount,
            //        principalFirstpaymentDate = (DateTime)entity.firstPrincipalPaymentDate,
            //        principalFrequency = entity.principalFrequencyTypeId,
            //        scheduleMethodId = entity.loanScheduleInput.scheduleMethodId,
            //        tenor = entity.tenor,
            //        irregularPaymentSchedule = entity.loanScheduleInput.irregularPaymentSchedule
            //    };
            //}
            //catch (NoNullAllowedException) {
            //    throw new NullReferenceException("An error occured processing Loan Schedule Inputs");
            //}


            ////Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Applied for loan with reference number: {loanReferenceNumber}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            ////end of Audit section -------------------------------


            if (workFlow.CheckRouteForOperation((int)OperationsEnum.TermLoanBooking, entity.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {

                        var loan = context.tbl_Loan.Add(data);
                        context.tbl_Audit.Add(audit);

                        var dataCount = context.SaveChanges();
                        this.loanSchedule.AddLoanSchedule(loan.TermLoanId, entity.loanScheduleInput, entity.createdBy);

                        var approvalModel = new ApprovalViewModel
                        {
                            staffId = entity.createdBy,
                            companyId = entity.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = loan.TermLoanId,
                            operationId = (int)OperationsEnum.TermLoanBooking,
                            BranchId = entity.userBranchId,
                        };
                        trans.Commit();
                        //var response = await workFlow.LogForApproval(approvalModel);

                        //if (response.Item1)
                        //{
                        //    this.loanSchedule.AddLoanSchedule(loan.LoanId, entity.loanScheduleInput, entity.createdBy);
                        //    trans.Commit();
                        //}
                        //else
                        //{
                        //    trans.Rollback();
                        //    throw new Exception("This transaction failed to log for approval");
                        //}

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
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }
            //PostLoanDisbursment(entity);
            //List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //inputTransactions.Add(BuildLoanDisbursmentPosting(entity));

            //inputTransactions.AddRange(BuildLoanChargeFeesPosting(entity));

            //financeTransaction.PostTransaction(inputTransactions);
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

        public IEnumerable<LoanViewModel> GetLoanBookingAwaitingApproval(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.TermLoanBooking);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            var data = (from ln in context.tbl_Loan
                        join coy in context.tbl_Company on ln.CompanyId equals coy.CompanyId
                        join br in context.tbl_Branch on ln.BranchId equals br.BranchId
                        join atrail in context.tbl_Approval_Trail on ln.TermLoanId equals atrail.TargetId
                        where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending  
                              && atrail.OperationId == (int)OperationsEnum.TermLoanBooking && atrail.ToApprovalLevelId == staffApprovalLevelId
                        select new LoanViewModel()
                        {
                            loanId = ln.TermLoanId,
                            customerId  = ln.CustomerId,
                            productId = ln.ProductId,
                            casaAccountId =ln.CasaAccountId,
                            loanApplicationId = ln.LoanApplicationId,

                            branchId = ln.BranchId,
                            loanReferenceNumber =ln.LoanReferenceNumber,
                            applicationReferenceNumber =ln.tbl_Loan_Application.ApplicationReferenceNumber,

                            tenor = (ln.MaturityDate - ln.EffectiveDate).Days,
                            //principalFrequencyTypeId = ln.PrincipalFrequencyTypeId.Value,
                            pricipalFrequencyTypeName = ln.tbl_Frequency_Type.Description,
                            //interestFrequencyTypeId = ln.PrincipalFrequencyTypeId.Value,
                            interestFrequencyTypeName = ln.tbl_Frequency_Type.Description,

                            principalNumberOfInstallment = ln.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = ln.InterestNumberOfInstallment,
                            relationshipOfficerId = ln.RelationshipOfficerId,
                            relationshipManagerId =ln.RelationshipManagerId,
                            misCode =ln.MISCode,
                            teamMiscode =ln.TeamMISCode,
                            interestRate  = ln.InterestRate,
                            effectiveDate = ln.EffectiveDate,
                            maturityDate = ln.MaturityDate,
                            bookingDate = ln.BookingDate,
                            principalAmount = ln.PrincipalAmount,
                            principalInstallmentLeft = ln.PrincipalInstallmentLeft,
                            interestInstallmentLeft = ln.InterestInstallmentLeft,
                            approvalStatusId = ln.ApprovalStatusId,
                            approvedBy = ln.ApprovedBy,
                            approverComment = ln.ApproverComment,
                            dateApproved = ln.DateApproved,
                            loanStatusId = ln.LoanStatusId,
                            scheduleTypeId = ln.ScheduleTypeId,
                            isDisbursed  = ln.IsDisbursed,
                            disbursedBy = ln.DisbursedBy,
                            disburserComment = ln.DisburserComment,
                            disburseDate = ln.DisburseDate,
                            approvedAmount = ln.ApprovedAmount,
                            customerGroupId = ln.CustomerGroupId,
                            operationId = ln.OperationId,
                            loanTypeId = ln.LoanTypeId,
                            trancheBatchCode = ln.TrancheBatchCode,
                            equityContribution = ln.EquityContribution,
                            subSectorId = ln.SubSectorId,
                            subSectorName = ln.tbl_Sub_Sector.Name,
                            SectorName = ln.tbl_Sub_Sector.tbl_Sector.Name,

                            firstPrincipalPaymentDate = ln.FirstInterestPaymentDate,
                            firstInterestPaymentDate = ln.FirstInterestPaymentDate,
                            outstandingPrincipal = ln.OutstandingPrincipal,
                            principalAdditionCount = ln.PrincipalAdditionCount,
                            principalReductionCount = ln.PrincipalReductionCount,
                            fixedPrincipal = ln.FixedPrincipal,
                            profileLoan = ln.ProfileLoan,
                            dischargeLetter = ln.DischargeLetter,
                            suspendInterest = ln.SuspendInterest,
                           // scheduled = (bool)ln.IsScheduledPrepayment,
                            //isScheduledPrepayment = (bool)ln.IsScheduledPrepayment,
                            scheduledPrepaymentAmount = (decimal) ln.ScheduledPrepaymentAmount,
                           // scheduledPrepaymentDate = (DateTime)( ln.ScheduledPrepaymentDate),
                            //scheduledPrepaymentFrequencyTypeId  = ln.ScheduledPrepaymentFrequencyTypeId.Value,
                            customerSensitivityLevelId = ln.CustomerSensitivityLevelId,
                            customerSensitivityLevelName = ln.tbl_Customer_Sensitivity_Level.Description,
                            firstName = ln.tbl_Customer.FirstName,
                            middleName = ln.tbl_Customer.MiddleName,
                            lastName = ln.tbl_Customer.LastName,
                            customerCode = ln.tbl_Customer.CustomerCode,
                            productAccountNumber = ln.tbl_Product.tbl_Chart_Of_Account.AccountCode,
                            productAccountName = ln.tbl_Product.tbl_Chart_Of_Account.AccountName,
                            loanTypeName = ln.tbl_Loan_Type.LoanTypeName,
                            customerName = ln.tbl_Customer.LastName+" "+ln.tbl_Customer.FirstName +" "+ ln.tbl_Customer.MiddleName,
                            currencyId = ln.CurrencyId,

                            branchName = ln.tbl_Branch.BranchName,
                            relationshipOfficerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,
                            relationshipManagerName = ln.tbl_Staff.FirstName + " " + ln.tbl_Staff.MiddleName + " " + ln.tbl_Staff.LastName,

                            productName = ln.tbl_Product.ProductName,

                            createdBy = ln.CreatedBy,
                            creatorName = ln.tbl_Staff.LastName +" "+ ln.tbl_Staff.FirstName + " ("+ ln.tbl_Staff.StaffCode+")",
                            dateTimeCreated = ln.DateTimeCreated,

                            //isCamsol 
                            //loanCovenant 
                            //loanChargeFee 
                            //loanGuarantor 
                            //loanCollateral 
                        });
            return data;
        }

        public async Task<bool> GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.TermLoanBooking;

            var response = await workFlow.GoForApproval(entity);

            if (response.Item1)
            {
                return ApproveLoanBooking(entity.targetId, response.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }
        }

        private bool ApproveLoanBooking(int loanId, short approvalStatusId, UserInfo user)
        {
            var loanRecord = context.tbl_Loan.Find(loanId);

            //LoanRecord.IsCurrent = false;
            loanRecord.ApprovalStatusId = approvalStatusId;
            loanRecord.DateApproved = DateTime.Now;
            //LoanRecord.DateTimeUpdated = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanBookingAdded,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Loan Booking with code ({loanRecord.LoanReferenceNumber})",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = generalSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.context.SaveChanges() > 0;

        }


        public FinanceTransactionViewModel BuildLoanDisbursmentPosting(LoanViewModel model)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();


            var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == model.casaAccountId && x.CompanyId == model.companyId);

            loanTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
            loanTransaction.description = "Loan Disbursment Amount";
            loanTransaction.valueDate = generalSetup.GetApplicationDate();
            loanTransaction.transactionDate = loanTransaction.valueDate;
            loanTransaction.currencyId = casa.CurrencyId;
            loanTransaction.currencyRate = financeTransaction.GetExchangeRate(loanTransaction.currencyId, model.companyId);
            loanTransaction.isApproved = true;
            loanTransaction.postedBy = model.createdBy;
            loanTransaction.approvedBy = model.createdBy;
            loanTransaction.approvedDate = loanTransaction.transactionDate;
            loanTransaction.approvedDateTime = DateTime.Now;
            loanTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            loanTransaction.companyId = model.companyId;

            FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
            debit.glAccountId = model.casaAccountId;  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = casa.CasaAccountId;
            debit.debitAmount = model.principalAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BranchId;

            var repaymentGL = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
            FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
            credit.glAccountId = repaymentGL;

            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = casa.CasaAccountId;
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


                var casa = this.context.tbl_CASA.FirstOrDefault(x => x.CasaAccountId == loanDetails.casaAccountId);

                feeTransaction.operationId = (int)OperationsEnum.TermLoanBooking;
                feeTransaction.description = "Fee charge";
                feeTransaction.valueDate = generalSetup.GetApplicationDate();
                feeTransaction.transactionDate = feeTransaction.valueDate;
                feeTransaction.currencyId = casa.CurrencyId;
                feeTransaction.currencyRate = financeTransaction.GetExchangeRate(feeTransaction.currencyId, loanDetails.companyId);
                feeTransaction.isApproved = true;
                feeTransaction.postedBy = loanDetails.createdBy;
                feeTransaction.approvedBy = loanDetails.createdBy;
                feeTransaction.approvedDate = feeTransaction.transactionDate;
                feeTransaction.approvedDateTime = DateTime.Now;
                feeTransaction.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                feeTransaction.companyId = loanDetails.companyId;

                FinanceTransactionDetailViewModel debit = new FinanceTransactionDetailViewModel();
                debit.glAccountId = context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                debit.casaAccountId = casa.CasaAccountId;
                debit.debitAmount = (decimal)item.amount;
                debit.creditAmount = 0;
                debit.sourceBranchId = loanDetails.branchId;
                debit.destinationBranchId = casa.BranchId;

                var feeGL = this.context.tbl_Charge_Fee.Where(x => x.ChargeFeeId == item.chargeFeeId).Select(x => x.GLAccountId).FirstOrDefault();  //context.tbl_Product.FirstOrDefault(x => x.ProductId == casa.ProductId).PrincipalBalanceGL.Value;
                FinanceTransactionDetailViewModel credit = new FinanceTransactionDetailViewModel();
                credit.glAccountId = feeGL;
                credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                credit.casaAccountId = casa.CasaAccountId;
                credit.debitAmount = 0;
                credit.creditAmount = (decimal)item.amount;
                credit.sourceBranchId = loanDetails.branchId;
                credit.destinationBranchId = loanDetails.branchId;


                feeTransaction.transactionDetails.Add(debit);
                feeTransaction.transactionDetails.Add(credit);

                output.Add(feeTransaction);
            }

            // Audit Section ---------------------------            

            return output;

        }

        public ICollection<tbl_Loan_Covenant_Detail> AddLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel, int loanId, short productTypeId)
        {
            ICollection<tbl_Loan_Covenant_Detail> covenant;

            if (covenantModel.Count  < 1)
                return null;

            covenant = new List<tbl_Loan_Covenant_Detail>();
            foreach(LoanCovenantDetailViewModel entity in covenantModel)
            covenant.Add(new tbl_Loan_Covenant_Detail
            {
                CompanyId = entity.companyId,
                CovenantAmount = entity.covenantAmount,
                CovenantDate = entity.covenantDate,
                CovenantDetail = entity.covenantDetail,
                CovenantTypeId = entity.covenantTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = this.generalSetup.GetApplicationDate().Date,
                FrequencyTypeId = entity.frequencyTypeId,
                LoanId = entity.loanId,
                ProductTypeId = productTypeId

            });

            return covenant;
        }

        private ICollection<tbl_Loan_Covenant_Detail> AddLoanCovenant(LoanCovenantDetailViewModel entity, short productTypeId)
        {
            ICollection<tbl_Loan_Covenant_Detail> covenant;

            covenant = new List<tbl_Loan_Covenant_Detail>();

            covenant.Add(new tbl_Loan_Covenant_Detail
            {
                CompanyId = entity.companyId,
                CovenantAmount = entity.covenantAmount,
                CovenantDetail = entity.covenantDetail,
                CovenantDate = entity.covenantDate,
                LoanId = entity.loanId,
                ProductTypeId = productTypeId,
                CovenantTypeId = entity.covenantTypeId,
                FrequencyTypeId = entity.frequencyTypeId,
                CreatedBy =  entity.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate(),
            });

            return covenant;
        }

        public ICollection<tbl_Loan_Guarantor> AddLoanGuarantor(List<LoanGuarantorViewModel> guarantorModel, int loanId, short productTypeId)
        {
            ICollection<tbl_Loan_Guarantor> guarantor;

            if (guarantorModel.Count < 1)
                return null;

            guarantor = new List<tbl_Loan_Guarantor>();
            foreach (LoanGuarantorViewModel entity in guarantorModel)
                guarantor.Add(new tbl_Loan_Guarantor
                {
                    ProductTypeId = productTypeId,
                    LoanId = entity.loanId,
                    Firstname = entity.firstname,
                    Lastname = entity.lastname,
                    Middlename = entity.middlename,
                    Address = entity.address,
                    PhoneNumber1 = entity.phoneNumber1,
                    PhoneNumber2 = entity.phoneNumber2,
                    Relationship = entity.relationship,
                    RelationshipDuration = (short)entity.relationshipDuration,
                    BVN = entity.bvn,
                    EmailAddress = entity.emailAddress, CreatedBy = 1, DateTimeCreated = generalSetup.GetApplicationDate()
                });

            return guarantor;
        }

        public ICollection<tbl_Loan_Collateral_Mapping> AddLoanCollateralMapping(List<LoanCollateralMappingViewModel> collateralModel, int loanApplicationId, int loanId, short productTypeId)
        {
            ICollection<tbl_Loan_Collateral_Mapping> collateral;

            if (collateralModel.Count < 1)
                return null;

            collateral = new List<tbl_Loan_Collateral_Mapping>();
            foreach (LoanCollateralMappingViewModel entity in collateralModel)
                collateral.Add(new tbl_Loan_Collateral_Mapping
                {
                    LoanId = loanId,
                    ProductTypeId = productTypeId,
                    CollateralCustomerId =entity.collateralCustomerId,
                    LoanApplicationId = loanApplicationId,
                });

            return collateral;
        }

        private ICollection<tbl_Loan_Fee> AddLoanFees(List<LoanChargeFeeViewModel> feeModel, int loanId, short productTypeId)
        {
            ICollection<tbl_Loan_Fee> fee;
            var feeAmount = 0;
            
            fee = new List<tbl_Loan_Fee>();

            foreach (LoanChargeFeeViewModel ent in feeModel)
            {
                if (ent.feeTypeId == 1)
                {
                    feeAmount = 0;
                }
                fee.Add(new tbl_Loan_Fee
                {
                    ChargeFeeId =  ent.chargeFeeId,
                    FeeAmount = feeAmount,
                    FeeDependentAmount = ent.feeDependentAmount,
                    FeeRateValue = ent.feeRateValue,
                    IsIntegralFee = ent.isIntegralFee,
                    LoanId = loanId,
                    ProductTypeId = productTypeId,
                    CreatedBy = ent.createdBy,
                    DateTimeCreated = DateTime.Now.Date

                });
            }
                
            return fee;
        }

        private IQueryable<LoanViewModel> GetAllLoans()
        {
            var data = (from l in context.tbl_Loan
                        select new LoanViewModel
                        {
                            loanId = l.TermLoanId,
                            customerId = l.CustomerId,
                            customerName = l.tbl_Customer.FirstName + " " + l.tbl_Customer.LastName,
                            productId = l.ProductId,
                            companyId = l.CompanyId,
                            casaAccountId = l.CasaAccountId,
                            branchId = l.BranchId,
                            branchName = l.tbl_Branch.BranchName,
                            loanReferenceNumber = l.LoanReferenceNumber,
                            //tenor = (l.MaturityDate - l.EffectiveDate).Days, // returning error
                            principalFrequencyTypeId = (short)l.PrincipalFrequencyTypeId,
                            interestFrequencyTypeId = (short)l.InterestFrequencyTypeId,

                            principalNumberOfInstallment = l.PrincipalNumberOfInstallment,
                            interestNumberOfInstallment = l.InterestNumberOfInstallment,
                            relationshipOfficerId = l.RelationshipOfficerId,
                            relationshipManagerId = l.RelationshipManagerId,
                            misCode = l.MISCode,
                            teamMiscode = l.TeamMISCode,
                            interestRate = l.InterestRate,
                            effectiveDate = l.EffectiveDate,
                            maturityDate = l.MaturityDate,
                            bookingDate = l.BookingDate,
                            principalAmount = l.PrincipalAmount,
                            principalInstallmentLeft = l.PrincipalInstallmentLeft,
                            interestInstallmentLeft = l.InterestInstallmentLeft,
                            approvalStatusId = l.ApprovalStatusId,
                            approvedBy = l.ApprovedBy,
                            approverComment = l.ApproverComment,
                            dateApproved = l.DateApproved,
                            loanStatusId = l.LoanStatusId,
                            scheduleTypeId = l.ScheduleTypeId,
                            isDisbursed = l.IsDisbursed,
                            disbursedBy = l.DisbursedBy,
                            disburserComment = l.DisburserComment,
                            disburseDate = l.DisburseDate,
                            approvedAmount = l.ApprovedAmount,
                            //creditAppraisalCompleted = l.CreditAppraisalCompleted,
                            operationId = l.OperationId,
                            //hasLien = l.HasLien,
                            //hasOfferLetter = l.HasOfferLetter,
                            customerGroupId = l.CustomerGroupId,
                            loanTypeId = l.LoanTypeId,
                            loanTypeName = l.tbl_Loan_Type.LoanTypeName,
                            trancheBatchCode = l.TrancheBatchCode ?? "Empty",
                            equityContribution = l.EquityContribution,
                            firstPrincipalPaymentDate = l.FirstPrincipalPaymentDate ?? DateTime.Now,
                            firstInterestPaymentDate = l.FirstInterestPaymentDate ?? DateTime.Now,
                            outstandingPrincipal = l.OutstandingPrincipal ,
                            principalAdditionCount = l.PrincipalAdditionCount ?? 0,
                            principalReductionCount = l.PrincipalReductionCount ?? 0,
                            fixedPrincipal = l.FixedPrincipal,
                            profileLoan = l.ProfileLoan,
                            dischargeLetter = l.DischargeLetter,
                            suspendInterest = l.SuspendInterest,
                            customerSensitivityLevelId = l.CustomerSensitivityLevelId,
                            createdBy = l.CreatedBy,
                            dateTimeCreated = l.DateTimeCreated,
                            isCamsol = context.tbl_Loan_Camsol.Where(x => x.LoanId == l.TermLoanId).Any()
                        });
            return data;
        }

        public bool ValidateCamsol(int loanId)
        {
            var check = context.tbl_Loan_Camsol.Where(x => x.LoanId == loanId);

            if (check.Any())
            {
                return true;
            }

            return false;
        }

        public int CalculateTenorValue(DateTime maturityDate, DateTime effectiveDate)
        {
            var result = (maturityDate - effectiveDate).Days;

            return result;
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomer(int customerId)
        {
            return GetAllLoans().Where(l => l.customerId == customerId);
        }

        public IEnumerable<LoanViewModel> GetLoanByCustomerGroup(int customerGroupId)
        {
            return GetAllLoans().Where(l => l.customerGroupId == customerGroupId);
        }

        public IQueryable<LoanRepaymentScheduleViewModel> RunningLoans(int customerId, int companyId)
        {

            var loans = GetLoansByCompanyId(companyId).Where(c => c.approvalStatusId == (int)ApprovalStatusEnum.Approved && c.companyId == customerId)
                .Select(c => new LoanRepaymentScheduleViewModel()
                {
                    principalRepayment = (decimal)c.outstandingPrincipal,
                    interestAccrual = (decimal)c.outstandingPrincipal,
                    customerId = c.customerId,
                    principalAmount = c.principalAmount,
                    effectiveDate = c.effectiveDate,
                    interestRate = c.interestRate,
                    loanId = c.loanId,
                    productName = c.productAccountName,
                    tenor = c.tenor,
                    terminationDate = c.maturityDate
                }).AsQueryable();
            return loans;
        }

        public LoanViewModel GetLoan(int loanId)
        {
            return (from data in context.tbl_Loan
                    where data.TermLoanId == loanId
                    select new LoanViewModel()
                    {
                        loanId = data.TermLoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,

                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        bookingDate = (DateTime)data.BookingDate,
                        principalAmount = data.PrincipalAmount,
                        principalInstallmentLeft = data.PrincipalInstallmentLeft,
                        interestInstallmentLeft = data.InterestInstallmentLeft,
                        approvalStatusId = data.ApprovalStatusId,
                        approvedBy = data.ApprovedBy,
                        approverComment = data.ApproverComment,
                        dateApproved = data.DateApproved,
                        loanStatusId = data.LoanStatusId,
                        scheduleTypeId = data.ScheduleTypeId,
                        isDisbursed = data.IsDisbursed,
                        disbursedBy = data.DisbursedBy,
                        disburserComment = data.DisburserComment,
                        disburseDate = data.DisburseDate,
                        approvedAmount = data.ApprovedAmount,
                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        trancheBatchCode = data.TrancheBatchCode,
                        equityContribution = data.EquityContribution,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        firstInterestPaymentDate = data.FirstInterestPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        customerSensitivityLevelId = data.CustomerSensitivityLevelId,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated
                    }).FirstOrDefault();
        }

        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<LoanViewModel> FindLoan(string referenceNumberOrName, int companyId)
        {
            return (from data in context.tbl_Loan
                    where data.CompanyId == companyId && (data.LoanReferenceNumber == referenceNumberOrName ||
                      $"{data.tbl_Customer.FirstName} {data.tbl_Customer.MiddleName} {data.tbl_Customer.LastName} {data.tbl_Customer.CustomerCode} {data.tbl_CASA.ProductAccountNumber}".Contains(referenceNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new LoanViewModel()
                    {
                        loanId = data.TermLoanId,
                        customerId = data.CustomerId,
                        productId = data.ProductId,
                        companyId = data.CompanyId,
                        casaAccountId = data.CasaAccountId,
                        branchId = data.BranchId,
                        loanReferenceNumber = data.LoanReferenceNumber,
                        tenor = (data.MaturityDate - data.EffectiveDate).Days,
                        //tenorModeId = data.TenorModeId,
                        principalFrequencyTypeId = (short)data.PrincipalFrequencyTypeId,
                        interestFrequencyTypeId = (short)data.InterestFrequencyTypeId,

                        principalNumberOfInstallment = data.PrincipalNumberOfInstallment,
                        interestNumberOfInstallment = data.InterestNumberOfInstallment,
                        relationshipOfficerId = data.RelationshipOfficerId,
                        relationshipManagerId = data.RelationshipManagerId,
                        misCode = data.MISCode,
                        teamMiscode = data.TeamMISCode,
                        interestRate = data.InterestRate,
                        effectiveDate = data.EffectiveDate,
                        maturityDate = data.MaturityDate,
                        bookingDate = (DateTime)data.BookingDate,
                        principalAmount = data.PrincipalAmount,
                        principalInstallmentLeft = data.PrincipalInstallmentLeft,
                        interestInstallmentLeft = data.InterestInstallmentLeft,
                        approvalStatusId = data.ApprovalStatusId,
                        approvedBy = data.ApprovedBy,
                        approverComment = data.ApproverComment,
                        dateApproved = data.DateApproved,
                        loanStatusId = data.LoanStatusId,
                        scheduleTypeId = data.ScheduleTypeId,
                        isDisbursed = data.IsDisbursed,
                        disbursedBy = data.DisbursedBy,
                        disburserComment = data.DisburserComment,
                        disburseDate = data.DisburseDate,
                        approvedAmount = data.ApprovedAmount,
                        operationId = data.OperationId,
                        customerGroupId = data.CustomerGroupId,
                        loanTypeId = data.LoanTypeId,
                        trancheBatchCode = data.TrancheBatchCode,
                        equityContribution = data.EquityContribution,
                        firstPrincipalPaymentDate = data.FirstPrincipalPaymentDate,
                        outstandingPrincipal = data.OutstandingPrincipal,
                        principalAdditionCount = data.PrincipalAdditionCount,
                        principalReductionCount = data.PrincipalReductionCount,
                        fixedPrincipal = data.FixedPrincipal,
                        profileLoan = data.ProfileLoan,
                        dischargeLetter = data.DischargeLetter,
                        suspendInterest = data.SuspendInterest,
                        customerSensitivityLevelId = data.CustomerSensitivityLevelId,
                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated

                    });
        }

        public IQueryable<LoanViewModel> GetLoansByCompanyId(int companyId)
        {
            return (context.tbl_Loan //.Include("tbl_Customer").Include("tbl_CASA_AccountStatus")
                .Where(x => x.CompanyId == companyId)
                .Select(o => new LoanViewModel
                {
                    loanId = o.TermLoanId,
                    customerId = o.CustomerId,
                    productId = o.ProductId,
                    casaAccountId = o.CasaAccountId,
                    branchId = o.BranchId,
                    loanReferenceNumber = o.LoanReferenceNumber,
                    tenor = (o.MaturityDate - o.EffectiveDate).Days,
                    principalFrequencyTypeId = (short)o.PrincipalFrequencyTypeId,
                    interestFrequencyTypeId = (short)o.InterestFrequencyTypeId,

                    principalNumberOfInstallment = o.PrincipalNumberOfInstallment,
                    interestNumberOfInstallment = o.InterestNumberOfInstallment,
                    relationshipOfficerId = o.RelationshipOfficerId,
                    relationshipManagerId = o.RelationshipManagerId,
                    misCode = o.MISCode,
                    teamMiscode = o.TeamMISCode,
                    interestRate = o.InterestRate,
                    effectiveDate = o.EffectiveDate,
                    maturityDate = o.MaturityDate,
                    bookingDate = (DateTime)o.BookingDate,
                    principalAmount = o.PrincipalAmount,
                    principalInstallmentLeft = o.PrincipalInstallmentLeft,
                    interestInstallmentLeft = o.InterestInstallmentLeft,
                    approvalStatusId = o.ApprovalStatusId,
                    firstName = o.tbl_Customer.FirstName,
                    middleName = o.tbl_Customer.MiddleName,
                    lastName = o.tbl_Customer.LastName,
                    customerCode = o.tbl_Customer.CustomerCode,
                    productAccountNumber = o.tbl_CASA.ProductAccountNumber,
                    productAccountName = o.tbl_CASA.ProductAccountName,
                }));
        }

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

            var value = context.tbl_Day_Count_Convention.FirstOrDefault(x => x.DayCountConventionId == (short) dayCountId).DaysInAYear;

            return value;
        }


        private List<LoanCovenantDetailViewModel> GetLoanCovenant(int loanId)
        {
            var data = (from a in context.tbl_Loan_Covenant_Detail
                        where a.LoanId == loanId && a.Deleted == false 
                        select new LoanCovenantDetailViewModel
                        {
                            loanCovenantDetailId = a.LoanCovenantDetailId,
                            covenantDetail = a.CovenantDetail,
                            loanId = a.LoanId,
                            covenantTypeId = a.CovenantTypeId,
                            frequencyTypeId = a.FrequencyTypeId,
                            covenantAmount = a.CovenantAmount,
                            covenantDate = a.CovenantDate

                        }).ToList();
            return data;
        }

        public IEnumerable<LoanChargeFeeViewModel> GetProductFees(int productId)
        {
            var data = (from a in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee on a.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && a.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            chargeFeeId = a.ChargeFeeId,
                            chargeFeeName = a.ChargeFeeName,
                            feeTypeId = a.FeeTypeId,
                            feeTypeName = a.tbl_Fee_Type.FeeTypeName,
                            feeIntervalId = a.FeeIntervalId
                        }).ToList();
            return data;
        }

        public IEnumerable<LoanChargeFeeViewModel> GetLoanProductChargeFee(int chargeFeeId, int productId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            chargeFeeId = c.ChargeFeeId,
                            chargeFeeName = c.ChargeFeeName,
                            feeRateValue =  p.RateValue ,
                            feeDependentAmount =  p.DependentAmount ?? 0,
                            feeAmount = c.Amount ?? 0,
                            feeIntervalId = c.FeeIntervalId,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            feeTypeId = c.FeeTypeId,
                            //feeTypeName = c.FeeTypeName

                        }).ToList();
            return data;
        }

        public List<LoanChargeFeeViewModel> GetLoanChargeFee(int loanId)
        {
            var data = (from c in context.tbl_Loan_Fee
                        where c.LoanId == loanId //&& c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            loanChargeFeeId = c.LoanChargeFeeId,
                            loanId = c.LoanId,
                            chargeFeeId = c.ChargeFeeId,
                            chargeFeeName = c.tbl_Charge_Fee.ChargeFeeName,
                            feeRateValue = c.FeeRateValue,
                            feeDependentAmount = c.FeeDependentAmount,
                            feeAmount = c.FeeAmount,
                            feeIntervalId = c.tbl_Charge_Fee.FeeIntervalId,
                            feeIntervalName = c.tbl_Charge_Fee.tbl_Fee_Interval.FeeIntervalName

                        }).ToList();
            return null;
        }


        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByChargeFeeId(int chargeFeeId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee
                        on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ChargeFeeId == chargeFeeId && p.Deleted == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            productId = p.ProductId,
                            productName = p.tbl_Product.ProductName,
                            feeId = c.ChargeFeeId,
                            feeName = c.ChargeFeeName,
                            feeTargetName = c.tbl_Fee_Target.FeeTargetName,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            rateValue = (decimal)p.RateValue,
                            dependentAmount = p.DependentAmount

                        }).ToList();
            return data;
        }

        public List<LoanGuarantorViewModel> GetLoanGuarantors(int loanId)
        {
            var data = (from c in context.tbl_Loan_Guarantor
                        where c.LoanId == loanId 
                        select new LoanGuarantorViewModel
                        {
                            loanGuarantorId = c.LoanGuarantorId,
                            //loanId = (int)c.LoanId,
                            firstname = c.Firstname,
                            lastname = c.Lastname,
                            middlename = c.Middlename,
                            address = c.Address,
                            phoneNumber1 = c.PhoneNumber1,
                            phoneNumber2 = c.PhoneNumber2,
                            relationship = c.Relationship,
                            relationshipDuration = (short)c.RelationshipDuration,
                            bvn = c.BVN,
                            emailAddress = c.EmailAddress,
                            fullName = c.Lastname + " "+ c.Firstname + " "+c.Middlename

                        }).ToList();
            return data;
        }

        public List<LoanCollateralMappingViewModel> GetLoanCollaterals(int loanId)
        {
            var data = (from c in context.tbl_Loan_Collateral_Mapping
                        where c.LoanId == loanId
                        select new LoanCollateralMappingViewModel
                        {
                            loanCollateralMappingId = c.LoanCollateralMappingId,
                            loanId = c.LoanId,
                            collateralCustomerId = c.CollateralCustomerId,
                            loanApplicationId = c.LoanApplicationId

                        }).ToList();
            return data;
        }

        public IEnumerable<CamProcessedLoanViewModel> GetCamProcessedLoanApplications(int companyId)
        {
            var data = (from a in context.tbl_Loan_Application
                        join c in context.tbl_Credit_Appraisal_Memorandum
                        on a.LoanApplicationId equals c.LoanApplicationId
                        join cust in context.tbl_Customer on a.CustomerId equals cust.CustomerId
                        //join custgrp in context.tbl_Customer_Group on a.CustomerGroupId equals custgrp.CustomerGroupId
                        where a.CompanyId == companyId && a.Deleted == false && c.IsCompleted == true
                        select new CamProcessedLoanViewModel
                        {
                            approvalStatusId = a.ApprovalStatusId,
                            loanApplicationId = a.LoanApplicationId,
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            casaAccountId = a.CasaAccountId,
                            customerId = a.CustomerId ?? 0,
                            customerCode = cust.CustomerCode,
                            customerName = a.CustomerId.HasValue ? a.tbl_Customer.FirstName + " " + a.tbl_Customer.MiddleName + " " + a.tbl_Customer.LastName : "",

                            customerGroupId = a.CustomerGroupId,
                            customerGroupName = a.CustomerGroupId.HasValue ? a.tbl_Customer_Group.GroupName : "",
                            customerGroupCode = a.tbl_Customer_Group.GroupCode,
                            customerSensitivityLevelId = a.tbl_Customer.CustomerSensitivityLevelId,

                            loanInformation = a.LoanInformation,
                            companyId = a.CompanyId,
                            branchId = a.BranchId,
                            branchName = a.tbl_Branch.BranchName,
                            subSectorId = a.SubSectorId,

                            tenor = a.Tenor,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,
                            relationshipManagerId = a.RelationshipManagerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.MiddleName + " " + a.tbl_Staff.LastName,

                            currencyId = a.CurrencyId,
                            currencyCode = a.tbl_Currency.CurrencyCode,
                            loanTypeId = a.LoanTypeId,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            //loanStatusId = a.LoanStatusId,
                            //loanStatusName = a.tbl_Loan_Type.AccountStatus,
                            camReference = c.CAMRef,
                            loanDetails = c.LoanDetails,
                            productId = (short)a.ProductId,
                            productTypeId = a.tbl_Product.ProductTypeId,
                            productTypeName = a.tbl_Product.tbl_Product_Type.ProductTypeName,
                            productName = a.tbl_Product.ProductName,

                            misCode = a.MISCode,
                            teamMisCode = a.TeamMISCode,

                            interestRate = a.InterestRate,
                            isRealatedParty = a.IsRealatedParty,
                            isPoliticallyExposed = a.IsPoliticallyExposed,
                            submittedForAppraisal = a.SubmittedForAppraisal,
                            principalAmount = a.PrincipalAmount,

                            createdBy = a.CreatedBy,
                            applicationDate = a.ApplicationDate,
                            dateTimeCreated = a.DateTimeCreated,

                            loanPreliminaryEvaluationId = a.LoanPreliminaryEvaluationId,
                            exchangeRate = a.ExchangeRate,

                        }).ToList().Where(r => !context.tbl_Loan.AsEnumerable()

                        .Any(c => r.loanApplicationId == c.LoanApplicationId && r.loanTypeId == (int)LoanTypeEnum.Single)
                        || (context.tbl_Customer_Group_Mapping.Where(x => x.CustomerGroupId == r.customerGroupId).Count() >= context.tbl_Loan.Where(g => g.CustomerGroupId == r.customerGroupId).Count() && r.loanTypeId == (int)LoanTypeEnum.CustomerGroup));


            return data;
        }


        private IEnumerable<LoanViewModel> BookedLoan(int companyId)
        {
            return GetAllLoans().Where(x => x.companyId == companyId);
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetails(int companyId)
        {
            var loans = BookedLoan(companyId);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByLoanReferenceNumber(string loanReferenceNumber, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x=>x.loanReferenceNumber == loanReferenceNumber);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<LoanViewModel> GetBookedLoanDetailsByCustomerCode(string customerCode, int companyId)
        {
            var loans = BookedLoan(companyId).Where(x => x.customerCode == customerCode);
            foreach (var loan in loans)
            {
                loan.loanCovenant = GetLoanCovenant(loan.loanId);
                loan.loanChargeFee = GetLoanChargeFee(loan.loanId);
                loan.loanGuarantor = GetLoanGuarantors(loan.loanId);
                loan.loanCollateral = GetLoanCollaterals(loan.loanId);
            }

            return loans;
        }

        public IEnumerable<ProductFeeViewModel> GetLoanProductChargeFeesByProductId(int productId)
        {
            var data = (from c in context.tbl_Charge_Fee
                        join p in context.tbl_Product_Charge_Fee
                        on c.ChargeFeeId equals p.ChargeFeeId
                        where p.ProductId == productId && p.Deleted == false
                        select new ProductFeeViewModel
                        {
                            productFeeId = p.ProductFeeId,
                            productId = p.ProductId,
                            productName = p.tbl_Product.ProductName,
                            feeId = c.ChargeFeeId,
                            feeName = c.ChargeFeeName,
                            feeTargetName = c.tbl_Fee_Target.FeeTargetName,
                            feeIntervalName = c.tbl_Fee_Interval.FeeIntervalName,
                            rateValue = (decimal) p.RateValue,
                            dependentAmount = p.DependentAmount

                        }).ToList();
            return data;
        }

        



    }

}

