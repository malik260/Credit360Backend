using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using System.Threading.Tasks;
using FintrakBanking.ViewModels;

namespace FintrakBanking.Repositories.Credit
{
    public class AppraisalMemorandumRepository : IAppraisalMemorandumRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

        public AppraisalMemorandumRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
        }

        public AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);

            var groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                x.OPERATIONID == (int)OperationsEnum.CAM
                && x.PRODUCTCLASSID == appl.PRODUCTCLASSID
            // && x.ProductId == appl.ProductId // ---- REFACTOR when we have appl.PRODUCTID!!!
            );

            if (groupMappings.Any() == false) // -----  MAY BECOME REDUNDANT!
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == (int)OperationsEnum.CAM
                    && x.PRODUCTCLASSID == appl.PRODUCTCLASSID
                );
            }

            var staffLevels = groupMappings
            .Select(x => x.TBL_APPROVAL_GROUP)
            .SelectMany(x => x.TBL_APPROVAL_LEVEL)
            .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF)
            .Select(x => new
            {
                staffId = x.STAFFID,
                levelId = x.TBL_APPROVAL_LEVEL.APPROVALLEVELID
            })
            .Where(x => x.staffId == staffId);

            var memos = context.TBL_CREDIT_APPRAISAL_MEMORANDM.Where(x => x.LOANAPPLICATIONID == applicationId)
                .SelectMany(x => x.TBL_CREDIT_APPRAISAL_MEMO_DOCU)
                .Select(x => new
                {
                    doc = x,
                    mem = x.TBL_CREDIT_APPRAISAL_MEMORANDM
                })
                .Select(x => new AppraisalMemorandumViewModel
                {
                    documentationId = x.doc.CAMDOCUMENTATIONID,
                    appraisalMemorandumId = x.mem.APPRAISALMEMORANDUMID,
                    loanApplicationId = x.mem.LOANAPPLICATIONID,
                    camRef = x.mem.CAMREF,
                    isCompleted = x.mem.ISCOMPLETED,
                    riskRated = x.mem.RISKRATED,
                    camDocumentation = x.doc.CAMDOCUMENTATION,
                    approvalLevelId = x.doc.APPROVALLEVELID
                })
                .OrderByDescending(x => x.documentationId);

            var memo = memos.FirstOrDefault(x => staffLevels.Select(o => o.levelId).Contains(x.approvalLevelId));

            if (memo == null) { return memos.FirstOrDefault(); }

            return memo;
        }

        public AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(model.loanApplicationId);

            int approvalLevelId = GetFirstApprovalLevelId(/*appl.ProductId,*/ appl.PRODUCTCLASSID, model.createdBy);

            var memo = context.TBL_CREDIT_APPRAISAL_MEMORANDM.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).SingleOrDefault();

            if (memo == null)
            {
                var newMemo = new TBL_CREDIT_APPRAISAL_MEMORANDM
                {
                    COMPANYID = model.companyId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    CAMREF = appl.APPLICATIONREFERENCENUMBER,
                    ISCOMPLETED = false,
                    RISKRATED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                memo = context.TBL_CREDIT_APPRAISAL_MEMORANDM.Add(newMemo);
            }

            var newDocument = new TBL_CREDIT_APPRAISAL_MEMO_DOCU
            {
                CAMDOCUMENTATION = "New",
                APPRAISALMEMORANDUMID = memo.APPRAISALMEMORANDUMID,
                APPROVALLEVELID = approvalLevelId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            var document = context.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Add(newDocument);


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added AppraisalMemorandum '{ model.camRef }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            this.FlagSubmittedForAppraisal(model.loanApplicationId);

            context.SaveChanges();

            return new AppraisalMemorandumViewModel
            {
                appraisalMemorandumId = memo.APPRAISALMEMORANDUMID,
                loanApplicationId = memo.LOANAPPLICATIONID,
                camRef = memo.CAMREF,
                isCompleted = memo.ISCOMPLETED,
                riskRated = memo.RISKRATED,
                camDocumentation = document.CAMDOCUMENTATION,
                documentationId = document.CAMDOCUMENTATIONID,
                approvalLevelId = 0
            };
        }

        private void LoadConditionPrecedent(int loanApplicationId) // AND TRANSACTION DYNAMICS
        {
            if (context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId).Any() == false)
            {
                var conditions = context.TBL_CONDITION_PRECEDENT.ToList(); // TEMPLATE
                var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
                foreach (var f in facilities)
                {
                    foreach (var c in conditions)
                    {
                        var row = new TBL_LOAN_CONDITION_PRECEDENT
                        {
                            CONDITION = c.CONDITION,
                            ISEXTERNAL = c.ISEXTERNAL,
                            CREATEDBY = c.CREATEDBY,
                            //LOANAPPLICATIONID = loanApplicationId,
                            TIMELINEID = c.TIMELINEID,
                            LOANAPPLICATIONDETAILID = f.LOANAPPLICATIONDETAILID,
                            DATETIMECREATED = DateTime.Now
                        };
                        context.TBL_LOAN_CONDITION_PRECEDENT.Add(row);
                    }
                }
                context.SaveChanges();
            }

            if (context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId).Any() == false)
            {
                var dynamics = context.TBL_TRANSACTION_DYNAMICS.ToList(); // TEMPLATE
                var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
                foreach (var f in facilities)
                {
                    foreach (var c in dynamics)
                    {
                        var row = new TBL_LOAN_TRANSACTION_DYNAMICS
                        {
                            DYNAMICS = c.DYNAMICS,
                            DYNAMICSID = c.DYNAMICSID,
                            CREATEDBY = c.CREATEDBY,
                            //LOANAPPLICATIONID = loanApplicationId,
                            LOANAPPLICATIONDETAILID = f.LOANAPPLICATIONDETAILID,
                            DATETIMECREATED = DateTime.Now
                        };
                        context.TBL_LOAN_TRANSACTION_DYNAMICS.Add(row);
                    }
                }
                context.SaveChanges();
            }
        }

        private int GetFirstApprovalLevelId(/*short productId,*/ short? productClassId, int staffId = 0) // ---- REFACTOR when we have productId!!!
        {
            var groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                x.OPERATIONID == (int)OperationsEnum.CAM
                && x.PRODUCTCLASSID == productClassId
            //&& x.ProductId == productId
            );

            if (groupMappings.Any() == false) // MAY BECOME REDUNDANT!
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == (int)OperationsEnum.CAM
                    && x.PRODUCTCLASSID == productClassId
                );
            }

            var staffLevels = groupMappings
            .Select(x => x.TBL_APPROVAL_GROUP)
            .SelectMany(x => x.TBL_APPROVAL_LEVEL)
            .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF)
            .Select(x => new
            {
                staffId = x.STAFFID,
                levelId = x.TBL_APPROVAL_LEVEL.APPROVALLEVELID
            })
            .Where(x => x.staffId == staffId);

            if (staffLevels.FirstOrDefault() == null) { throw new Exception("No workflow setup for this product"); }

            return staffLevels.Select(x => x.levelId).First();
        }

        private bool FlagSubmittedForAppraisal(int id)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(id);
            if (application != null)
            {
                application.SUBMITTEDFORAPPRAISAL = true;
                application.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
                return true;
            }
            return false;
        }

        public bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int documentId)
        {
            var data = this.context.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Find(documentId);

            if (data == null) { return false; }

            data.CAMDOCUMENTATION = model.camDocumentation;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AppraisalMemorandumUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Appraisal Memorandum Document'{ model.camRef }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public int ForwardAppraisalMemorandum(ForwardViewModel model)
        {
            bool updateApprovedAmount = false;
            int operationId = (int)OperationsEnum.CAM;
            var applicationDate = general.GetApplicationDate();
            List<TBL_LOAN_APPLICATION_DETAIL> items = null;
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            this.LoadConditionPrecedent(model.applicationId);

            // WORKFLOW
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            workflow.Disputed = appl.DISPUTED;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.Amount = appl.TOTALEXPOSUREAMOUNT; //model.amount;
            workflow.InvestmentGrade = model.investmentGrade;
            workflow.Tenor = model.applicationTenor;
            workflow.PoliticallyExposed = model.politicallyExposed;
            workflow.Untenored = model.untenored;
            workflow.InterestRateConcession = model.interestRateConcession;
            workflow.FeeRateConcession = model.feeRateConcession;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            // DETAIL CHANGES
            if (model.recommendedChanges.Count() > 0) // only approving authority
            {
                updateApprovedAmount = true;
                items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID).ToList();
                foreach (var changed in model.recommendedChanges)
                {
                    var detail = items.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == changed.detailId);
                    if (detail != null)
                    {
                        detail.APPROVEDPRODUCTID = (short)changed.productId;
                        detail.APPROVEDAMOUNT = changed.amount;
                        detail.APPROVEDINTERESTRATE = changed.interestRate;
                        detail.APPROVEDTENOR = changed.tenor;
                        detail.STATUSID = (short)changed.statusId;
                        detail.EXCHANGERATE = changed.exchangeRate;
                        detail.LASTUPDATEDBY = model.createdBy;
                        detail.DATETIMEUPDATED = DateTime.Now;

                        if (model.isBusiness) // UPDATE PROPOSED
                        {
                            detail.PROPOSEDPRODUCTID = (short)changed.productId;
                            detail.PROPOSEDAMOUNT = changed.amount;
                            detail.PROPOSEDINTERESTRATE = changed.interestRate;
                            detail.PROPOSEDTENOR = changed.tenor;
                        }

                        context.TBL_LOAN_APPLICATION_DETL_LOG.Add(new TBL_LOAN_APPLICATION_DETL_LOG // LOG CHANGES
                        {
                            LOANAPPLICATIONDETAILID = changed.detailId,
                            APPROVEDPRODUCTID = (short)changed.productId,
                            APPROVEDTENOR = changed.tenor,
                            APPROVEDINTERESTRATE = changed.interestRate,
                            APPROVEDAMOUNT = changed.amount,
                            EXCHANGERATE = changed.exchangeRate,
                            STATUSID = (short)changed.statusId,
                            CREATEDBY = model.createdBy,
                            DATETIMECREATED = applicationDate,
                            SYSTEMDATETIME = DateTime.Now,
                        });
                    }
                }
            }

            // UPDATE APPLICATION
            appl.APPROVALSTATUSID = workflow.StatusId;
            if (model.vote == 1) { appl.DISPUTED = true; }
            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
            if (appl.SUBMITTEDFORAPPRAISAL == false) { appl.SUBMITTEDFORAPPRAISAL = true; } // for product programs
            if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                if (model.forwardAction == (int)ApprovalStatusEnum.Approved) { appl.APPROVEDDATE = applicationDate; }
                if (model.forwardAction == (int)ApprovalStatusEnum.Disapproved) { appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected; }
                // MEMORANDUM update
                var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDM.Find(model.appraisalMemorandumId);
                if (memo != null) { memo.ISCOMPLETED = true; }
            }

            // UPDATE APPROVED AMOUNT
            if (updateApprovedAmount == true && items != null)
            {
                appl.APPROVEDAMOUNT = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).Sum(x => x.APPROVEDAMOUNT);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ForwardAppraisalMemorandum,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Loan Application Reference Number: '{ appl.APPLICATIONREFERENCENUMBER }', " +
                            $"StaffId: '{ model.createdBy }', " +
                            $"TargetId: '{ model.applicationId }', " +
                            $"Vote: '{ model.vote }', " +
                            $"NextLevelId: '{ model.receiverLevelId }', " +
                            $"ToStaffId: '{ model.receiverStaffId }', " +
                            $"StatusId: '{ model.forwardAction }', " +
                            $"Comment: '{ model.comment }', " +
                            $"LINE CHANGES:" +
                            $"'{ LineItemChanges(model.recommendedChanges) }'",

                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            if (model.comment == "debug_test") throw new Exception("debug_test => FFW:" + model.forwardAction + ", APR:" + workflow.StatusId + ", APL:" + appl.APPLICATIONSTATUSID + ", CHG:" + model.recommendedChanges.Count() + ", STE:" + workflow.NewState + ", AMO:" + appl.APPROVEDAMOUNT + ", upd:" + updateApprovedAmount + ", EXP:" + appl.TOTALEXPOSUREAMOUNT);

            context.SaveChanges();

            if (workflow.NewState == (int)ApprovalState.Ended) { return workflow.StatusId; }

            return (int)ApprovalStatusEnum.Processing; // default for now
        }

        private string LineItemChanges(List<RecommendedChangesViewModel> recommendedChanges)
        {
            string changes = string.Empty;
            foreach (var x in recommendedChanges)
            {
                changes += "DetailId: " + x.detailId + ", ProductId: " + x.productId + ", ApprovalStatusId: " + x.statusId + ", Amount: " + x.amount + ", Ex Rate: " + x.exchangeRate + ", Int Rate: " + x.interestRate + ", Tenor: " + x.tenor + ", Product Name: " + x.productName + ", Converted: " + x.convertedAmount;
            }
            return changes;
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId)
        {
            var allstaff = this.GetAllStaffNames();

            return this.context.TBL_APPROVAL_TRAIL
                .Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId)
                .Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.APPROVALTRAILID,
                    targetId = x.TARGETID,
                    arrivalDate = x.ARRIVALDATE,
                    systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                    responseDate = x.RESPONSEDATE,
                    systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                    responseStaffId = x.RESPONSESTAFFID,
                    requestStaffId = x.REQUESTSTAFFID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : x.TBL_APPROVAL_LEVEL.LEVELNAME,
                    toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    comment = x.COMMENT,
                    staffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                }).OrderByDescending(x => x.approvalTrailId);
        }

        public PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity)
        {
            var operationId = entity.operationId; // (int)OperationsEnum.CAM; // <--------------------- overide incoming for now
            var privilege = new PrivilegeViewModel();
            var application = this.context.TBL_LOAN_APPLICATION.Find(entity.targetId);

            var grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == entity.productClassId)
                .Join(context.TBL_APPROVAL_GROUP,
                    m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL,//.Where(x => x.APPROVALLEVELID == entity.levelId),
                    mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })//, u=l.TBL_APPROVAL_LEVEL_STAFF })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == entity.createdBy),
                    gl => gl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (gl, s) => new PrivilegeViewModel
                    {
                        viewCamDocument = s.CANVIEWCAMDOCUMENT,
                        viewUploadedFiles = s.CANVIEWUPLOADEDFILE,
                        viewApproval = s.CANVIEWAPPROVAL,
                        canMakeChanges = s.CANEDIT,
                        canAppendTemplate = s.CANEDIT,
                        canApprove = s.CANAPPROVE,
                        canUploadFile = s.CANUPLOADFILE,
                        canSendRequest = s.CANSENDJOBREQUEST,
                        approvalLimit = s.MAXIMUMAMOUNT,
                        approvalLevelId = s.APPROVALLEVELID,
                        groupRoleId = gl.mg.g.ROLEID,
                        canEscalate = gl.l.CANESCALATE,
                    });

            var grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant != null) privilege = grant;
            privilege.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
            return privilege;
        }

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }

        private bool RunningProcess(int operationId, int targetId)
        {
            var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.TARGETID == targetId);
            if (trail == null) { return false; }
            return true;
        }

        public IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId)
        {
            var details = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .SelectMany(x => x.TBL_LOAN_APPLICATION_DETAIL)
                .Select(x => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                    applicationId = x.LOANAPPLICATIONID,
                    customerId = x.TBL_CUSTOMER.CUSTOMERID,
                    obligorName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                    currencyCode = x.TBL_CURRENCY.CURRENCYCODE,

                    proposedProductName = x.TBL_PRODUCT.PRODUCTNAME,
                    proposedTenor = x.PROPOSEDTENOR,
                    proposedRate = x.PROPOSEDINTERESTRATE,
                    proposedAmount = x.PROPOSEDAMOUNT,
                    proposedProductId = x.PROPOSEDPRODUCTID,

                    approvedProductName = x.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                    approvedTenor = x.APPROVEDTENOR,
                    approvedRate = x.APPROVEDINTERESTRATE,
                    approvedAmount = x.APPROVEDAMOUNT,
                    //convertedApprovedAmount = x.ApprovedAmount * Convert.ToDecimal(x.ExchangeRate),
                    approvedProductId = x.APPROVEDPRODUCTID,

                    statusId = x.STATUSID,
                    exchangeRate = x.EXCHANGERATE,
                });

            return details;
        }

        public IEnumerable<LoanDetailsFeeViewModel> GetLoanDetailsFee(int applicationId)
        {
            var fees = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .SelectMany(x => x.TBL_LOAN_APPLICATION_DETAIL)
                .SelectMany(x => x.TBL_LOAN_APPLICATION_DETL_FEE)
                .Select(x => new LoanDetailsFeeViewModel
                {
                    loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                    loanChargeFeeId = x.LOANCHARGEFEEID,
                    chargeFeeId = x.CHARGEFEEID,
                    hasConcession = x.HASCONSESSION,
                    concessionReason = x.CONSESSIONREASON,
                    defaultFeeRate = x.DEFAULT_FEERATEVALUE,
                    recommendedFeeRate = x.RECOMMENDED_FEERATEVALUE,
                    statusId = x.APPROVALSTATUSID,
                    approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    feeName = x.TBL_CHARGE_FEE.CHARGEFEENAME,
                });

            return fees;
        }

        public IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId)
        {
            var details = context.TBL_LOAN_APPLICATION_DETL_LOG.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
                .Select(x => new LoanApplicationDetailLogViewModel
                {
                    loanApplicationDetailId = x.a.LOANAPPLICATIONDETAILID,
                    applicationId = x.a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                    customerId = x.a.TBL_LOAN_APPLICATION_DETAIL.CUSTOMERID,
                    approvedTenor = x.a.APPROVEDTENOR,
                    approvedRate = x.a.APPROVEDINTERESTRATE,
                    approvedAmount = x.a.APPROVEDAMOUNT,
                    approvedProductId = x.a.APPROVEDPRODUCTID,
                    statusId = x.a.STATUSID,
                    exchangeRate = x.a.EXCHANGERATE,
                    customerName = x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME,
                    approvedProductName = x.a.TBL_PRODUCT.PRODUCTNAME,
                    staffName = x.b.FIRSTNAME + " " + x.b.MIDDLENAME + " " + x.b.LASTNAME,
                });

            return details;
        }

        public IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId)
        {
            var documentation = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => x.TBL_CREDIT_APPRAISAL_MEMORANDM).First()
                .SelectMany(x => x.TBL_CREDIT_APPRAISAL_MEMO_DOCU)
                .Select(x => new DocumentationViewModel
                {
                    documentationId = x.CAMDOCUMENTATIONID,
                    documentation = x.CAMDOCUMENTATION,
                    appraisalMemorandumId = x.APPRAISALMEMORANDUMID,
                    approvalLevelId = x.APPROVALLEVELID,
                });

            return documentation;
        }

        public bool Confirmation(int type, int applicationId)
        {
            var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            bool result = false;

            switch (type)
            {
                case 1:
                    application.CUSTOMERINFOVALIDATED = (application.CUSTOMERINFOVALIDATED == false) ? true : false;
                    result = application.CUSTOMERINFOVALIDATED;
                    break;
                //case 2:
                //    application.NOTINNEGATIVECRMS = (application.NOTINNEGATIVECRMS == false) ? true : false;
                //    result = application.NOTINNEGATIVECRMS;
                //    break;
                //case 3:
                //    application.NOTINBLACKBOOK = (application.NOTINBLACKBOOK == false) ? true : false;
                //    result = application.NOTINBLACKBOOK;
                //    break;
                //case 4:
                //    application.NOTINCAMSOL = (application.NOTINCAMSOL == false) ? true : false;
                //    result = application.NOTINCAMSOL;
                //    break;
                //case 5:
                //    application.NOTINXDS = (application.NOTINXDS == false) ? true : false;
                //    result = application.NOTINXDS;
                //    break;
                //case 6:
                //    application.NOTINCRC = (application.NOTINCRC == false) ? true : false;
                //    result = application.NOTINCRC;
                //    break;
                default:
                    break;
            }

            context.SaveChanges();

            return result;
        }

        #region CAM Pending Applications

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int companyId, int branchId, int staffId, int? classId)
        {
            // var declarations
            IQueryable<LoanApplicationViewModel> applications = null;
            int operationId = (int)OperationsEnum.CAM;
            bool isHeadOffice = (branchId == 1) ? true : false;

            // get approval levels 
            var levelIds = GetStaffApprovalLevelIds(staffId, operationId);

            // query
            applications = context.TBL_LOAN_APPLICATION.Where(x =>
                    x.DELETED == false
                    && x.COMPANYID == companyId
                    && (x.BRANCHID == branchId || isHeadOffice) // branch filter
                    && (classId == null) ? true : (x.PRODUCTCLASSID == (short?)classId)
                )
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (x, y) => new { a = x, bs = y })
            .SelectMany(
                xy => xy.bs.DefaultIfEmpty(),
                (x, y) => new LoanApplicationViewModel
                {
                    //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                    loanApplicationId = x.a.LOANAPPLICATIONID,
                    applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                    relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
                    customerId = x.a.CUSTOMERID,
                    branchId = x.a.BRANCHID,
                    productClassId = x.a.PRODUCTCLASSID,
                    productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                    customerGroupId = x.a.CUSTOMERGROUPID,
                    loanTypeId = x.a.LOANTYPEID,
                    relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                    relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                    newApplicationDate = x.a.APPLICATIONDATE,
                    applicationAmount = x.a.APPLICATIONAMOUNT,
                    approvedAmount = x.a.APPROVEDAMOUNT,
                    interestRate = x.a.INTERESTRATE,
                    applicationTenor = x.a.APPLICATIONTENOR,
                    lastComment = y.COMMENT,
                    currentApprovalStateId = y.APPROVALSTATEID,
                    currentApprovalLevelId = y.TOAPPROVALLEVELID,
                    currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                    toStaffId = y.TOSTAFFID,
                    loanInformation = x.a.LOANINFORMATION,
                    submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                    customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                    isRelatedParty = x.a.ISRELATEDPARTY,
                    isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                    approvalStatusId = x.a.APPROVALSTATUSID,
                    applicationStatusId = x.a.APPLICATIONSTATUSID,
                    branchName = x.a.TBL_BRANCH.BRANCHNAME,
                    relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                    relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                    misCode = x.a.MISCODE,
                    customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                    loanTypeName = x.a.TBL_LOAN_TYPE.LOANTYPENAME,
                    createdBy = x.a.CREATEDBY,
                    loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                    customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
                    operationId = x.a.OPERATIONID,
                })
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.newApplicationDate)
                .ThenByDescending(x => x.loanApplicationId)
                ;

            //var list = applications.ToList();
            //var count = applications.Count();
            //var levs = levelIds.ToList();

            return applications.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        #endregion CAM Pending Applications

        public IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId)
        {
            int operationId = (int)OperationsEnum.CAM;

            var result = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationId && x.SUBMITTEDFORAPPRAISAL == true && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.RESPONSESTAFFID == null && x.OPERATIONID == operationId), a => a.LOANAPPLICATIONID, t => t.TARGETID, (a, t) => new { a, t })
                .Join(context.TBL_APPROVAL_LEVEL, at => at.t.TOAPPROVALLEVELID, l => l.APPROVALLEVELID, (at, l) => new { at, l })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF, atl => atl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (atl, s) => new { atl, s })
                .Join(context.TBL_APPROVAL_GROUP, atls => atls.atl.l.GROUPID, g => g.GROUPID, (atls, g) => new { atls, g })
                 .Select(x => new CurrentCommitteeViewModel
                 {
                     position = x.atls.s.POSITION,
                     approvalLevelId = x.atls.atl.l.APPROVALLEVELID,
                     approvalLevelName = x.atls.atl.l.LEVELNAME,
                     approvalGroupName = x.g.GROUPNAME,
                     numberOfApprovals = x.atls.atl.l.NUMBEROFAPPROVALS,
                     groupRoleId = x.g.ROLEID,
                     staffId = x.atls.s.STAFFID,
                     staffName = x.atls.s.TBL_STAFF.FIRSTNAME + " " + x.atls.s.TBL_STAFF.MIDDLENAME + " " + x.atls.s.TBL_STAFF.LASTNAME,
                     vote = 0,
                     comment = string.Empty,
                 })
                 .OrderBy(x => x.position)
                 .ToList();

            return result;
        }

        public bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel model)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            var operationId = (int)OperationsEnum.CAM;

            // init
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = appl.COMPANYID;

            workflow.ProductClassId = appl.PRODUCTCLASSID;
            //workflow.ProductId = appl.PRODUCTID; 

            workflow.Amount = appl.APPROVEDAMOUNT;
            workflow.InvestmentGrade = appl.ISINVESTMENTGRADE;
            workflow.Tenor = appl.APPLICATIONTENOR;
            workflow.PoliticallyExposed = appl.ISPOLITICALLYEXPOSED;

            bool result = true;
            foreach (var member in model.votes.OrderBy(x => x.position))
            {
                workflow.StaffId = member.staffId;
                workflow.Vote = (short)member.vote;
                workflow.Comment = member.comment;
                workflow.StatusId = ((int)member.vote > 1) ? (int)ApprovalStatusEnum.Approved : (int)ApprovalStatusEnum.Disapproved;

                workflow.NextLevelId = null;
                result = workflow.LogActivity();
            }

            // LIFTED FROM ABOVE

            appl.APPROVALSTATUSID = workflow.StatusId;

            if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) // redundant block
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault(x => x.LOANAPPLICATIONID == model.applicationId);

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                if (workflow.GroupStatusId == (int)ApprovalStatusEnum.Disapproved) { appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected; }

                if (memo != null) memo.ISCOMPLETED = true;
                var items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                var approvedAmount = items.Where(x => x.STATUSID != (short)ApprovalStatusEnum.Disapproved).Sum(x => x.APPROVEDAMOUNT);
                appl.APPROVEDAMOUNT = approvedAmount;
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ForwardAppraisalMemorandum,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Committee Vote on Loan Application Reference Number: '{ appl.APPLICATIONREFERENCENUMBER }', ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return (context.SaveChanges() > 0) == result;
        }

        public IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;
            var levels = GetStaffApprovalLevelIds(staffId, operationId);
            var region = context.TBL_BRANCH_REGION.FirstOrDefault(x => x.CAM_HOU_STAFFID == staffId);
            if (region == null) { throw new Exception("This user does not have a region mapped to him."); }
            var branches = context.TBL_BRANCH.Where(x => x.REGIONID == region.REGIONID).Select(x => x.BRANCHID);

            var applications = context.TBL_LOAN_APPLICATION.Where(x => branches.Contains(x.BRANCHID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                    && x.SUBMITTEDFORAPPRAISAL == true
                ).GroupJoin(
                    context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),// && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)),
                    a => a.LOANAPPLICATIONID,
                    b => b.TARGETID,
                    (x, y) => new { a = x, bs = y })
                .SelectMany(
                    xy => xy.bs.DefaultIfEmpty(),
                    (x, y) => new RegionLoanApplicationViewModel
                    {
                        loanApplicationId = x.a.LOANAPPLICATIONID,
                        applicationDate = x.a.APPLICATIONDATE,
                        applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                        branchId = x.a.BRANCHID,
                        customerId = x.a.CUSTOMERID,
                        applicationAmount = x.a.APPLICATIONAMOUNT,
                        interestRate = x.a.INTERESTRATE,
                        applicationTenor = x.a.APPLICATIONTENOR,
                        submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                        approvalStatusId = x.a.APPROVALSTATUSID,
                        operationId = x.a.OPERATIONID,
                        timeIn = y.SYSTEMARRIVALDATETIME,
                        timeOut = y.SYSTEMRESPONSEDATETIME,
                        responsiblePerson = context.TBL_STAFF
                                                .Where(s => s.STAFFID == y.TOSTAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                                .FirstOrDefault().name ?? "",
                        requestStaffId = y.REQUESTSTAFFID,
                        toApprovalLevelId = y.TOAPPROVALLEVELID,
                        toStaffId = y.TOSTAFFID,
                        //sla time, timein timeout, timespent, responsible person
                        currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                        approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                    })
                .Where(x => levels.Contains((int)x.toApprovalLevelId) || (x.requestStaffId == staffId && x.toStaffId != null))
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.applicationDate)
                .ThenByDescending(x => x.loanApplicationId)
                ;

            return applications;
        }

        public List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user)
        {
            int staffId = user.staffId;
            bool isHeadOffice = (user.BranchId == 1) ? true : false;
            int operationId = (int)OperationsEnum.CAM;
            var levelIds = GetStaffApprovalLevelIds(user.staffId, operationId);// new int[] {3,1,5};
            int productBasedId = (int)ProductClassProcessEnum.ProductBased;

            var applications = context.TBL_LOAN_APPLICATION.Where(x =>
                x.DELETED == false
                && x.COMPANYID == user.companyId
                && (x.BRANCHID == user.BranchId || isHeadOffice) // branch filter
                && x.TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID == productBasedId
                && x.PRODUCTCLASSID != null
                //&& x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                //&& x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            )
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (x, y) => new { a = x, bs = y })
            .SelectMany(
                xy => xy.bs.DefaultIfEmpty(),
                (x, y) => new
                {
                    loanApplicationId = x.a.LOANAPPLICATIONID,
                    approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                    productClassId = x.a.PRODUCTCLASSID,
                    currentApprovalLevelId = y.TOAPPROVALLEVELID,
                    toStaffId = y.TOSTAFFID,
                })
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            //var levs = levelIds.ToList();

            //var test = applications.ToList();

            applications = applications.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));

            //var test2 = applications.ToList();

            var productClasses = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCT_CLASS_PROCESSID == productBasedId)
                .Select(item => new PendingProductProgramViewModel
                {
                    productClassId = item.PRODUCTCLASSID,
                    productClassName = item.PRODUCTCLASSNAME,
                    pendingNumber = applications.Where(x => x.productClassId == item.PRODUCTCLASSID).Count(),
                })
                .ToList();

            return productClasses;
        }

        public bool GetUntenoredStatus(int applicationId)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETL_BG
                .FirstOrDefault(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId);
            return detail == null ? false : !detail.ISTENORED;
        }

        private IQueryable<int> GetStaffApprovalLevelIds(int staffId, int operationId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default 1

            var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
                .Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL
                .Where(l => l.ISACTIVE == true));

            var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

            if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

            if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct();

            var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

            if (scope == 2)
            {
                var groups = context.TBL_APPROVAL_LEVEL.Where(x => staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
                return context.TBL_APPROVAL_LEVEL.Where(x => groups.Contains(x.GROUPID)).Select(x => x.APPROVALLEVELID).Distinct();
            }

            return staffLevels;
        }

        public IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId)
        {
            return context.TBL_LOAN_APPLICATN_DETL_MTRIG
                .Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = x.LOANAPPLICATIONDETAILID,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER,
                    productCustomerName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
                })
                .ToList();
        }

        public IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggers(int applicationId, List<MonitoringTriggersViewModel> items, int staffId)
        {
            context.TBL_LOAN_APPLICATN_DETL_MTRIG
                .RemoveRange(
                    context.TBL_LOAN_APPLICATN_DETL_MTRIG.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId) 
                );
            context.SaveChanges();

            foreach (var o in items)
            {
                context.TBL_LOAN_APPLICATN_DETL_MTRIG.Add(new TBL_LOAN_APPLICATN_DETL_MTRIG
                {
                    LOANAPPLICATIONDETAILID = o.applicationDetailId,
                    MONITORING_TRIGGERID = o.monitoringTriggerId,
                    MONITORING_TRIGGER = o.monitoringTrigger,
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now
                });
            }
            context.SaveChanges();

            return GetApplicationMonitoringTriggers(applicationId);
        }

        public bool WorkflowTest()
        {
            workflow.StaffId = 1558; // RM-1558
            workflow.TargetId = 2472;
            workflow.CompanyId = 1;
            workflow.ProductClassId = 5;
            workflow.OperationId = (int)OperationsEnum.CAM;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "flow_test";
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return true;
        }
    }
}
