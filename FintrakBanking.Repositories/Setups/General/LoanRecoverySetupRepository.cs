using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using FintrakBanking.Common.CustomException;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity.Validation;
using System.Linq;
using FintrakBanking.ViewModels.Credit;
using System.Transactions;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Interfaces.Finance;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{
    public class LoanRecoverySetupRepository : ILoanRecoverySetupRepository
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private FinTrakBankingContext context;
        private IWorkflow workflow;
        private IAdminRepository admin;
        private ITwoFactorAuthIntegrationService twoFactoeAuth;
        private IFinanceTransactionRepository financeTransaction;

        public LoanRecoverySetupRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup, 
            FinTrakBankingContext _context,
            IWorkflow _workflow, IFinanceTransactionRepository _financeTransaction, IAdminRepository _admin, ITwoFactorAuthIntegrationService _twoFactoeAuth
            )
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this._genSetup = genSetup;
            this.workflow = _workflow;
            this.financeTransaction = _financeTransaction;
            this.twoFactoeAuth = _twoFactoeAuth;
        }
        
            


        private bool SaveAll()
        {
            try
            {
                return this.context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }

        public bool AddLoanRecoverySetup(LoanRecoverySetupViewModel entity)
        {
            int loanId = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == entity.loanId).TERMLOANID;
            try
            {
                var LoanRecoverySetup = new TBL_LOAN_RECOVERY_PLAN
                {
                
                    LOANID = loanId,
                    PRODUCTTYPEID = entity.productTypeId,
                    CASAACCOUNTID = entity.casaAccountId,
                    AGENTID = entity.agentId,
                    AMOUNTOWED = entity.amountOwed,
                    WRITEOFFAMOUNT = entity.writeOffAmount,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    LASTUPDATEDBY = entity.createdBy,
                    DELETED = false,
                };

                this.context.TBL_LOAN_RECOVERY_PLAN.Add(LoanRecoverySetup);
                context.SaveChanges();

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanRecoverySetupAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Added new TBL_LOAN_RECOVERY_PLAN ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                auditTrail.AddAuditTrail(audit);
                return SaveAll();
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }
           
        public IEnumerable<LoanRecoverySetupViewModel> GetAllLoanRecoverySetup()
        {
            var LoanRecoverySetup = (from d in context.TBL_LOAN_RECOVERY_PLAN
                                     select new LoanRecoverySetupViewModel()
                              {
                                  recoveryPlanId = d.RECOVERYPLANID,
                                  loanId = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  productTypeId = d.PRODUCTTYPEID,
                                  productTypeName = d.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                  casaAccountId = d.CASAACCOUNTID,
                                  casaAccountName = d.TBL_CASA.PRODUCTACCOUNTNAME,
                                  agentId = (int)d.AGENTID,
                                  agentName = context.TBL_ACCREDITEDCONSULTANT.FirstOrDefault(x => x.ACCREDITEDCONSULTANTID == (int)d.AGENTID).NAME,
                                  amountOwed = (decimal)d.AMOUNTOWED,
                                  writeOffAmount = d.WRITEOFFAMOUNT,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllCasa() 
        {
            var LoanRecoverySetup = (from d in context.TBL_CASA
                                   select new LoanRecoverySetupViewModel()
                                   {
                                       casaAccountId = d.CASAACCOUNTID,
                                       casaAccountName = d.PRODUCTACCOUNTNAME,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllAgent()
        {
            var LoanRecoverySetup = (from d in context.TBL_ACCREDITEDCONSULTANT
                                     select new LoanRecoverySetupViewModel()
                                     {
                                         agentId = d.ACCREDITEDCONSULTANTID,
                                         agentName = d.NAME + d.FIRMNAME,

                                     }).ToList();
            return LoanRecoverySetup;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllProductType()
        {
            var LoanRecoverySetup = (from d in context.TBL_PRODUCT_TYPE 
                                   select new LoanRecoverySetupViewModel()
                                   {
                                       productTypeId = d.PRODUCTTYPEID,
                                       productTypeName = d.PRODUCTTYPENAME,

                                   }).ToList();
            return LoanRecoverySetup;
        }

        public LoanRecoverySetupViewModel GetLoanRecoverySetup(int recoveryPlanId )
        {
            var LoanRecoverySetup = (from d in context.TBL_LOAN_RECOVERY_PLAN
                                     where d.RECOVERYPLANID == recoveryPlanId
                                     select new LoanRecoverySetupViewModel()
                              {
                                  recoveryPlanId = d.RECOVERYPLANID,
                                  loanId = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,
                                  productTypeId = d.PRODUCTTYPEID,
                                  casaAccountId = d.CASAACCOUNTID,
                                  agentId = (int)d.AGENTID,
                                  agentName = context.TBL_ACCREDITEDCONSULTANT.FirstOrDefault(x => x.ACCREDITEDCONSULTANTID == (int)d.AGENTID).NAME,
                                  amountOwed = (decimal)d.AMOUNTOWED,
                                  writeOffAmount = d.WRITEOFFAMOUNT,
                              }).SingleOrDefault();
            return LoanRecoverySetup;
        }

        public bool UpdateLoanRecoverySetup(int LoanRecoverySetupId, LoanRecoverySetupViewModel entity)
        {
            var LoanRecoverySetup = context.TBL_LOAN_RECOVERY_PLAN.Find(LoanRecoverySetupId);
            int loanId = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == entity.loanId).TERMLOANID;

            LoanRecoverySetup.LOANID = loanId;
            LoanRecoverySetup.PRODUCTTYPEID = entity.productTypeId;
            LoanRecoverySetup.CASAACCOUNTID = entity.casaAccountId;
            LoanRecoverySetup.AGENTID = entity.agentId;
            LoanRecoverySetup.AMOUNTOWED = entity.amountOwed;
            LoanRecoverySetup.WRITEOFFAMOUNT = entity.writeOffAmount;
            LoanRecoverySetup.CREATEDBY = entity.createdBy;
            LoanRecoverySetup.DATETIMECREATED = DateTime.Now;
            LoanRecoverySetup.LASTUPDATEDBY = entity.createdBy;
            LoanRecoverySetup.DELETED = false;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanRecoverySetupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_LoanRecoverySetup with Id: {entity.recoveryPlanId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        public bool AddLoanRecoveryPaymentPlan(LoanRecoverySetupViewModel entity)
        {
            //int loanId = context.TBL_LOAN.FirstOrDefault(x => x.LOANREFERENCENUMBER == entity.loanId).TERMLOANID;
            //int recoveryId = context.TBL_LOAN_RECOVERY_PLAN.FirstOrDefault(x => x.LOANID == loanId).RECOVERYPLANID;
            try
            {
                var LoanRecoveryPaymentPlan = new TBL_LOAN_RECOVERY_PLAN_PAYMNT
                {

                    RECOVERYPLANID = entity.recoveryPlanId,
                    PAYMENTDATE = entity.paymentDate,
                    PAYMENTAMOUNT = entity.paymentAmount,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    LASTUPDATEDBY = entity.createdBy,
                    DELETED = false,
                };

                this.context.TBL_LOAN_RECOVERY_PLAN_PAYMNT.Add(LoanRecoveryPaymentPlan);
                context.SaveChanges();

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanRecoveryPaymentPlanAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Added new TBL_LOAN_RECOVERY_PLAN_PAYMNT ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                auditTrail.AddAuditTrail(audit);
                return SaveAll();
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public LoanRecoverySetupViewModel GetLoanRecoveryPaymentPlan(int recoveryPaymentPlanId)
        {
            var LoanRecoveryPaymentPlan  = (from d in context.TBL_LOAN_RECOVERY_PLAN_PAYMNT
                                            where d.RECOVERYPLANPAYMENTID == recoveryPaymentPlanId
                                     select new LoanRecoverySetupViewModel()
                                     {
                                         recoveryPaymentPlanId = d.RECOVERYPLANPAYMENTID,
                                         recoveryPlanId = d.RECOVERYPLANID,//context.TBL_LOAN_RECOVERY_PLAN_PAYMNT.FirstOrDefault(x => x.RECOVERYPLANPAYMENTID == d.RECOVERYPLANID).RECOVERYPLANID,
                                         paymentloanId = context.TBL_LOAN_RECOVERY_PLAN.FirstOrDefault(x => x.RECOVERYPLANID == d.RECOVERYPLANID).LOANID,
                                         loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.TBL_LOAN_RECOVERY_PLAN.LOANID).LOANREFERENCENUMBER,
                                         paymentAmount = d.PAYMENTAMOUNT,
                                         paymentDate = d.PAYMENTDATE,
                                     }).SingleOrDefault();
            return LoanRecoveryPaymentPlan;
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetAllLoanRecoveryPaymentPlan ()
        {
            var LoanRecoveryPaymentPlan = (from d in context.TBL_LOAN_RECOVERY_PLAN_PAYMNT
                                           select new LoanRecoverySetupViewModel()
                                           {
                                               recoveryPaymentPlanId = d.RECOVERYPLANPAYMENTID,
                                               recoveryPlanId = d.RECOVERYPLANID,//context.TBL_LOAN_RECOVERY_PLAN_PAYMNT.FirstOrDefault(x => x.RECOVERYPLANPAYMENTID == d.RECOVERYPLANID).RECOVERYPLANID,
                                               paymentloanId = context.TBL_LOAN_RECOVERY_PLAN.FirstOrDefault(x => x.RECOVERYPLANID == d.RECOVERYPLANID).LOANID,
                                               loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.TBL_LOAN_RECOVERY_PLAN.LOANID).LOANREFERENCENUMBER,
                                               paymentAmount = d.PAYMENTAMOUNT,
                                               paymentDate = d.PAYMENTDATE,
                                           }).ToList();
            return LoanRecoveryPaymentPlan;
        }


        public bool AddLaonRecoveryPayment(LoanRecoveryPaymentViewModel model)
        {
            var data = new TBL_LOAN_RECOVERY_PAYMENT
            {
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
                DELETED = false,
                PAYMENTAMOUNT = model.paymentAmount,
                PAYMENTDATE = model.paymentDate,
                LOANREVIEWOPERATIONID = (short)model.loanReviewOperationId,

            };

            var recovery = context.TBL_LOAN_RECOVERY_PAYMENT.Add(data);

            if (context.SaveChanges() > 0)
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = recovery.LOANRECOVERYPAYMENTID;
                workflow.Comment = "Request for Loan Recovery Payment";
                workflow.OperationId = (int)OperationsEnum.LoanRecoveryPayment;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                return true;
            }

            return false;
        }


        public LoanRecoveryPaymentViewModel GetTotalRecoveryPayments(int loanReviewOperationId)
        {
            LoanRecoveryPaymentViewModel value = new LoanRecoveryPaymentViewModel();
            var data = (from r in context.TBL_LOAN_RECOVERY_PAYMENT
                        where r.LOANREVIEWOPERATIONID == loanReviewOperationId
                        && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                        select new LoanRecoveryPaymentViewModel
                        {
                            paymentAmount = r.PAYMENTAMOUNT
                        }).ToList();

            value.paymentAmount = data.Sum(o => o.paymentAmount);

            return value;
        }


        public IEnumerable<LoanRecoveryPaymentViewModel> GetRecoveryPaymentSchedule(int loanReviewOperationId)
        {
            return (from r in context.TBL_LOAN_REVIEW_OPRATN_IREG_SC
                    join o in context.TBL_LOAN_REVIEW_OPERATION on r.LOANREVIEWOPERATIONID equals o.LOANREVIEWOPERATIONID
                    join l in context.TBL_LOAN on o.LOANID equals l.TERMLOANID
                    where r.LOANREVIEWOPERATIONID == loanReviewOperationId
                    select new LoanRecoveryPaymentViewModel
                    {
                        customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.LASTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME,
                        loanReferenceNumber = l.LOANREFERENCENUMBER,
                        effectiveDate = l.EFFECTIVEDATE,
                        maturityDate = l.MATURITYDATE,
                        principalAmount = l.PRINCIPALAMOUNT,
                        loanReviewOperationId = o.LOANREVIEWOPERATIONID,
                        loanId = l.TERMLOANID,
                        firstName = l.TBL_CUSTOMER.FIRSTNAME,
                        lastName = l.TBL_CUSTOMER.LASTNAME,
                        dateTimeCreated = r.DATETIMECREATED,
                        paymentDate = r.PAYMENTDATE,
                        paymentAmount = r.PAYMENTAMOUNT
                    });
        }


        public IEnumerable<LoanRecoveryPaymentViewModel> GetLoanRecoveryPayment(string searchQuery)
        {


            var result = (from o in context.TBL_LOAN_REVIEW_OPERATION
                          join l in context.TBL_LOAN on o.LOANID equals l.TERMLOANID
                          join k in context.TBL_LOAN_REVIEW_OPRATN_IREG_SC on o.LOANREVIEWOPERATIONID equals k.LOANREVIEWOPERATIONID
                          where l.RECOVERYSTATUSID == (int)RecoveryStatusEnum.OnGoing && o.OPERATIONTYPEID == (int)OperationsEnum.LoanRecovery && o.MATURITYDATE != null
                          select new LoanRecoveryPaymentViewModel
                          {
                              customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.LASTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME,
                              loanReferenceNumber = l.LOANREFERENCENUMBER,
                              effectiveDate = o.EFFECTIVEDATE,
                              loanReviewOperationId = o.LOANREVIEWOPERATIONID,
                              maturityDateNew = o.MATURITYDATE,
                              principalAmountNew = o.PREPAYMENT,
                              loanId = l.TERMLOANID,
                              firstName = l.TBL_CUSTOMER.FIRSTNAME,
                              lastName = l.TBL_CUSTOMER.LASTNAME,

                          }).ToList().Select(x =>
                          {
                              x.maturityDate = (DateTime)x.maturityDateNew;
                              x.principalAmount = (decimal)x.principalAmountNew;
                              return x;
                          }).Distinct().ToList();



            return (from a in result
                    group a by new { a.customerName, a.loanReferenceNumber, a.effectiveDate, a.loanReviewOperationId, a.loanId, a.firstName, a.lastName, a.maturityDate, a.principalAmount } into groupedQ
                    select new LoanRecoveryPaymentViewModel()
                    {
                        customerName = groupedQ.Key.customerName,
                        loanReferenceNumber = groupedQ.Key.loanReferenceNumber,
                        effectiveDate = groupedQ.Key.effectiveDate,
                        loanReviewOperationId = groupedQ.Key.loanReviewOperationId,
                        maturityDate = groupedQ.Key.maturityDate,
                        principalAmount = groupedQ.Key.principalAmount,
                        loanId = groupedQ.Key.loanId,
                        firstName = groupedQ.Key.firstName,
                        lastName = groupedQ.Key.lastName,
                    }).ToList();


        }


        public bool RecoveryPaymentGoForApproval(LoanRecoveryPaymentViewModel entity)
        {

            bool resultant = false;

            using (TransactionScope transactionScope = new TransactionScope())
            {

                try
                {

                    var dataNew = (from x in context.TBL_LOAN_RECOVERY_PAYMENT
                                   where x.LOANRECOVERYPAYMENTID == entity.loanRecoveryPaymentId
                                   select x).FirstOrDefault();

                    var loanReviewOperationId = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANREVIEWOPERATIONID == dataNew.LOANREVIEWOPERATIONID).FirstOrDefault();

                    TBL_LOAN_RECOVERY_PAYMENT data = new TBL_LOAN_RECOVERY_PAYMENT();

                    workflow.StaffId = entity.staffId;
                    workflow.OperationId = (int)OperationsEnum.LoanRecoveryPayment;
                    workflow.TargetId = entity.loanRecoveryPaymentId;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = entity.approvalStatusId == (short)ApprovalStatusEnum.Approved ? (short)ApprovalStatusEnum.Processing : entity.approvalStatusId;
                    workflow.Comment = entity.comment;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;
                    workflow.LogActivity();

                    var twoFADetails = new TwoFactorAutheticationViewModel
                    {
                        username = entity.userName,
                        passcode = entity.passCode
                    };
                    if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
                    {
                        twoFADetails.username = context.TBL_STAFF.Find(entity.staffId).STAFFCODE;
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (twoFADetails != null && admin.TwoFactorAuthenticationEnabled())
                        {
                            var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                            if (authenticated.authenticated == false)
                                throw new TwoFactorAuthenticationException(authenticated.message);
                        }
                        twoFADetails.skipAuthentication = true;

                        bool result = LoanRecoveryPayment(loanReviewOperationId.LOANID, entity, twoFADetails, _genSetup.GetApplicationDate(), entity.staffId, entity.loanRecoveryPaymentId);

                        //data = (from x in context.TBL_LOAN_RECOVERY_PAYMENT
                        //        where x.LOANREVIEWOPERATIONID == entity.loanRecoveryPaymentId
                        //        select x).FirstOrDefault();

                        //data.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                    }

                    if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        data.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    }

                    context.SaveChanges();

                    resultant = true;

                    transactionScope.Complete();

                    transactionScope.Dispose();

                    return resultant;

                }
                catch (TransactionException ex)
                {
                    transactionScope.Dispose();
                    throw ex;
                }

            }


        }

        private bool LoanRecoveryPayment(int loanId, LoanRecoveryPaymentViewModel loanInput, TwoFactorAutheticationViewModel twoFactorAuth, DateTime applicationDate, int staffId, short loanRecoveryPaymentId)
        {
            bool output = false;

            decimal amt = 0;

            int loanReviewOperationId = 0;

            LoanPaymentRestructureScheduleInputViewModel recoInfo = new LoanPaymentRestructureScheduleInputViewModel();

            try
            {
                var loan = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId && x.RECOVERYSTATUSID == (int)RecoveryStatusEnum.OnGoing).FirstOrDefault();

                if (loan != null)
                {

                    var loanPayment = context.TBL_LOAN_RECOVERY_PAYMENT.Where(x => x.LOANRECOVERYPAYMENTID == loanInput.loanRecoveryPaymentId && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing).FirstOrDefault();

                    var _otherOperationAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (int)OtherOperationEnum.Recovery).FirstOrDefault();

                    var otherOperationAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (int)OtherOperationEnum.PrincipalOffBalansheetCompleteWriteOffAccount).FirstOrDefault();

                    var sllp = _otherOperationAccount.GLACCOUNTID;

                    amt = loanPayment.PAYMENTAMOUNT;


                    recoInfo.loanId = loanId;
                    recoInfo.date = _genSetup.GetApplicationDate();
                    recoInfo.createdBy = loanInput.staffId;
                    recoInfo.companyId = loanInput.companyId;


                    if (loanPayment.PAYMENTAMOUNT > 0)
                    {
                        financeTransaction.PostTerminateAndRebookDoubleEntries(loanId, recoInfo, loanPayment.PAYMENTAMOUNT, _otherOperationAccount.GLACCOUNTID, (int)_otherOperationAccount.GLACCOUNTID2, "Recovery Repayment", twoFactorAuth);

                        financeTransaction.PostTerminateAndRebookDoubleEntries(loanId, recoInfo, loanPayment.PAYMENTAMOUNT, (int)otherOperationAccount.GLACCOUNTID2, otherOperationAccount.GLACCOUNTID, "Recovery Repayment", twoFactorAuth);
                    }


                    loanPayment.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    loanReviewOperationId = loanPayment.LOANREVIEWOPERATIONID;
                    context.SaveChanges();

                    decimal loanPaymentSum = context.TBL_LOAN_RECOVERY_PAYMENT.Where(x => x.LOANREVIEWOPERATIONID == loanReviewOperationId && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Sum(x => x.PAYMENTAMOUNT);

                    int onGoing = (int)RecoveryStatusEnum.OnGoing;
                    short writeOff = (short)LoanStatusEnum.WriteOff;

                    var camsol = (from a in context.TBL_LOAN_CAMSOL
                                  join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                  where b.RECOVERYSTATUSID == onGoing && b.LOANSTATUSID == writeOff
                                  select new LoanCamsolViewModel
                                  {
                                      loanCamsolId = a.LOAN_CAMSOLID,
                                      balance = a.BALANCE,
                                  }).FirstOrDefault();



                    var camsolId = context.TBL_LOAN_CAMSOL.Where(x => x.LOAN_CAMSOLID == camsol.loanCamsolId).FirstOrDefault();

                    TBL_LOAN_CAMSOL_ARCHIVE loanCamsol = new TBL_LOAN_CAMSOL_ARCHIVE();

                    loanCamsol.LOAN_CAMSOLID = camsolId.LOAN_CAMSOLID;
                    loanCamsol.ARCHIVEDATE = System.DateTime.Now;
                    loanCamsol.COMPANYID = camsolId.COMPANYID;
                    loanCamsol.CUSTOMERCODE = camsolId.CUSTOMERCODE;
                    loanCamsol.LOANID = camsolId.LOANID;
                    loanCamsol.BALANCE = camsolId.BALANCE;
                    loanCamsol.DATE = camsolId.DATE;
                    loanCamsol.LOANSYSTEMTYPEID = camsolId.LOANSYSTEMTYPEID;
                    loanCamsol.CUSTOMERNAME = camsolId.CUSTOMERNAME;
                    loanCamsol.PRINCIPAL = camsolId.PRINCIPAL;
                    loanCamsol.INTERESTINSUSPENSE = camsolId.INTERESTINSUSPENSE;
                    loanCamsol.WRITTENOFFACCRUALAMOUNT = (decimal)camsolId.WRITTENOFFACCRUALAMOUNT;
                    loanCamsol.CAMSOLTYPEID = camsolId.CAMSOLTYPEID;
                    loanCamsol.ACCOUNTNUMBER = camsolId.ACCOUNTNUMBER;
                    loanCamsol.ACCOUNTNAME = camsolId.ACCOUNTNAME;
                    loanCamsol.REMARK = camsolId.REMARK;
                    loanCamsol.CANTAKELOAN = camsolId.CANTAKELOAN;
                    loanCamsol.CREATEDBY = camsolId.CREATEDBY;
                    loanCamsol.LASTUPDATEDBY = camsolId.LASTUPDATEDBY;
                    loanCamsol.DATETIMECREATED = camsolId.DATETIMECREATED;
                    loanCamsol.DATETIMEUPDATED = camsolId.DATETIMEUPDATED;
                    loanCamsol.DELETED = camsolId.DELETED;
                    loanCamsol.DELETEDBY = camsolId.DELETEDBY;
                    loanCamsol.DATETIMEDELETED = camsolId.DATETIMEDELETED;

                    context.TBL_LOAN_CAMSOL_ARCHIVE.Add(loanCamsol);
                    context.SaveChanges();


                    if (camsolId.BALANCE >= amt)
                    {
                        camsolId.BALANCE = camsolId.BALANCE - amt;
                        context.SaveChanges();
                    }
                    else if (camsolId.BALANCE < amt)
                    {
                        var negativeAmt = camsolId.BALANCE - amt;
                        camsolId.BALANCE = 0;
                        camsolId.WRITTENOFFACCRUALAMOUNT = camsolId.WRITTENOFFACCRUALAMOUNT - Math.Abs(negativeAmt);
                        context.SaveChanges();
                    }

                }


                output = true;

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return output;

        }


        public List<LoanRecoveryPaymentViewModel> LoanRecoveryPaymentWaitingForApproval(int staffId, int companyId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanRecoveryPayment).ToList();

            var data = (from r in context.TBL_LOAN_RECOVERY_PAYMENT
                        join o in context.TBL_LOAN_REVIEW_OPERATION on r.LOANREVIEWOPERATIONID equals o.LOANREVIEWOPERATIONID
                        join l in context.TBL_LOAN on o.LOANID equals l.TERMLOANID
                        //join d in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        join atrail in context.TBL_APPROVAL_TRAIL on r.LOANRECOVERYPAYMENTID equals atrail.TARGETID

                        let amountRecovered = (from rec in context.TBL_LOAN_RECOVERY_PAYMENT
                                               where rec.LOANREVIEWOPERATIONID == r.LOANREVIEWOPERATIONID
                                               && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                               select rec).ToList()

                        let totalAmountRecovered = amountRecovered.Sum(a => a.PAYMENTAMOUNT)

                        where atrail.OPERATIONID == (int)OperationsEnum.LoanRecoveryPayment && l.COMPANYID == companyId
                        && o.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility
                        && atrail.RESPONSESTAFFID == null
                        && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                    && atrail.APPROVALSTATEID != (int)ApprovalState.Ended && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                        orderby r.LOANRECOVERYPAYMENTID descending
                        select new LoanRecoveryPaymentViewModel
                        {
                            customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.LASTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME,
                            paymentAmount = r.PAYMENTAMOUNT,
                            paymentDate = r.PAYMENTDATE,
                            loanRecoveryPaymentId = r.LOANRECOVERYPAYMENTID,
                            loanReviewOperationId = r.LOANREVIEWOPERATIONID,
                            loanId = o.LOANID,
                            loanReferenceNumber = l.LOANREFERENCENUMBER,
                            totalAmountRecovered = totalAmountRecovered != null ? totalAmountRecovered : 0,
                            principalAmountNew = o.PREPAYMENT,
                            effectiveDate = o.EFFECTIVEDATE,
                            maturityDateNew = o.MATURITYDATE,
                            currencyCode = context.TBL_CURRENCY.Where(o => o.CURRENCYID == l.CURRENCYID).Select(o => o.CURRENCYCODE).FirstOrDefault(),
                            operationId = atrail.OPERATIONID,

                        }).ToList().Select(x =>
                        {
                            x.maturityDate = (DateTime)x.maturityDateNew;
                            x.principalAmount = (Decimal)x.principalAmountNew;
                            return x;
                        }).ToList();

            return data;
        }


        public bool UpdateLoanRecoveryPaymentPlan(int recoveryPaymentPlanId , LoanRecoverySetupViewModel entity)
        {
            var LoanRecoveryPaymentPlan  = context.TBL_LOAN_RECOVERY_PLAN_PAYMNT.Find(recoveryPaymentPlanId);

            LoanRecoveryPaymentPlan.RECOVERYPLANID = LoanRecoveryPaymentPlan.RECOVERYPLANID;
            LoanRecoveryPaymentPlan.PAYMENTDATE = entity.paymentDate;
            LoanRecoveryPaymentPlan.PAYMENTAMOUNT = entity.paymentAmount;
            LoanRecoveryPaymentPlan.CREATEDBY = entity.createdBy;
            LoanRecoveryPaymentPlan.DATETIMECREATED = DateTime.Now;
            LoanRecoveryPaymentPlan.LASTUPDATEDBY = entity.createdBy;
            LoanRecoveryPaymentPlan.DELETED = false;

            //this.context.TBL_LOAN_RECOVERY_PLAN_PAYMNT.Add(LoanRecoveryPaymentPlan);
            //context.SaveChanges();
            //Audit Section ----------------------------
           var audit = new TBL_AUDIT
           {
               AUDITTYPEID = (short)AuditTypeEnum.LoanRecoveryPaymentPlanUpdated,
               STAFFID = entity.createdBy,
               BRANCHID = (short)entity.userBranchId,
               DETAIL = $"Updated TBL_LOAN_RECOVERY_PLAN_PAYMNT with Id: {entity.recoveryPaymentPlanId} ",
               IPADDRESS = CommonHelpers.GetLocalIpAddress(),
               URL = entity.applicationUrl,
               APPLICATIONDATE = _genSetup.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now,
               DEVICENAME = CommonHelpers.GetDeviceName(),
               OSNAME = CommonHelpers.FriendlyName(),
           };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }

        public IEnumerable<LoanRecoverySetupViewModel> GetDistinctLoanRecoveryPaymentPlan()
        {
            var LoanRecoveryPaymentPlan = (from d in context.TBL_LOAN_RECOVERY_PLAN
                                     select new LoanRecoverySetupViewModel()
                                     {
                                         recoveryPlanId = d.RECOVERYPLANID,
                                         loanRefNo = context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == d.LOANID).LOANREFERENCENUMBER,

                                     }).ToList();
            return LoanRecoveryPaymentPlan;
        }
    }
}