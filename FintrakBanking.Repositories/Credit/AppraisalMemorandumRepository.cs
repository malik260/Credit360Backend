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
            //&& x.ProductClassId == appl.tbl_Product.ProductClassId // ---- REFACTOR!!!
            //&& x.ProductId == appl.ProductId // ---- REFACTOR!!!
            );

            if (groupMappings.Any() == false)
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == (int)OperationsEnum.CAM
                //&& x.ProductClassId == appl.tbl_Product.ProductClassId // ---- REFACTOR!!!
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

            var memos = context.TBL_CREDIT_APPRAISAL_MEMORANDUM.Where(x => x.LOANAPPLICATIONID == applicationId)
                .SelectMany(x => x.TBL_CREDIT_APPRAISAL_MEMO_DOCUM)
                .Select(x => new
                {
                    doc = x,
                    mem = x.TBL_CREDIT_APPRAISAL_MEMORANDUM
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

            int approvalLevelId = GetFirstApprovalLevelId(
                //appl.ProductId, 
                //appl.tbl_Product.ProductClassId, 
                model.createdBy);

            var memo = context.TBL_CREDIT_APPRAISAL_MEMORANDUM.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).SingleOrDefault();

            if (memo == null)
            {
                var newMemo = new TBL_CREDIT_APPRAISAL_MEMORANDUM
                {
                    COMPANYID = model.companyId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    CAMREF = appl.APPLICATIONREFERENCENUMBER,
                    ISCOMPLETED = false,
                    RISKRATED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                memo = context.TBL_CREDIT_APPRAISAL_MEMORANDUM.Add(newMemo);
            }

            var newDocument = new TBL_CREDIT_APPRAISAL_MEMO_DOCUM
            {
                CAMDOCUMENTATION = "Blank Document",
                APPRAISALMEMORANDUMID = memo.APPRAISALMEMORANDUMID,
                APPROVALLEVELID = approvalLevelId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            var document = context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM.Add(newDocument);


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
            this.LoadConditionPrecedent(model.loanApplicationId);

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

        private void LoadConditionPrecedent(int loanApplicationId)
        {
            var conditions = context.TBL_CONDITION_PRECEDENT.Where(x=>x.CORPORATE == true || x.RETAIL == true); // <----------- refactor 
            foreach (var c in conditions)
            {
                var data = new TBL_LOAN_CONDITION_PRECEDENT
                {
                    CONDITION = c.CONDITION,
                    ISEXTERNAL = c.ISEXTERNAL,
                    CREATEDBY = c.CREATEDBY,
                    LOANAPPLICATIONID = loanApplicationId,
                    DATETIMECREATED = general.GetApplicationDate(),
                };
                context.TBL_LOAN_CONDITION_PRECEDENT.Add(data);
            }
        }

        private int GetFirstApprovalLevelId(/*short productId, int productClassId, */int staffId = 0) // ---- REFACTOR!!!
        {
            var groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                x.OPERATIONID == (int)OperationsEnum.CAM // GIVEN ---- REFACTOR!!!
                                                         //&& x.ProductClassId == productClassId
                                                         //&& x.ProductId == productId
            );

            if (groupMappings.Any() == false)
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == (int)OperationsEnum.CAM// GIVEN ---- REFACTOR!!!
                                                            //&& x.ProductClassId == productClassId
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
            var data = this.context.TBL_CREDIT_APPRAISAL_MEMO_DOCUM.Find(documentId);

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

        public bool ForwardAppraisalMemorandum(ForwardViewModel model)
        {
            var operationId = (int)OperationsEnum.CAM;

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            workflow.ProductClassId = model.productClassId;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;

            workflow.Amount = model.amount;
            workflow.InvestmentGrade = model.investmentGrade;
            workflow.Tenor = model.applicationTenor;
            workflow.PoliticallyExposed = model.politicallyExposed;
            // log

            workflow.LogActivity();

            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            appl.APPROVALSTATUSID = workflow.StatusId;

            if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) // redundant block
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDUM.Find(model.appraisalMemorandumId);

            if (memo != null)
            {
                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                    memo.ISCOMPLETED = true;
                }

                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved || workflow.StatusId == (int)ApprovalStatusEnum.Authorised) // approving authority
                {
                    var items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == memo.LOANAPPLICATIONID);

                    var approvedAmount = items.Where(x => x.STATUSID != (int)ApprovalStatusEnum.Disapproved).Sum(x => x.APPROVEDAMOUNT);
                    appl.APPROVEDAMOUNT = approvedAmount;

                    foreach (var item in items)
                    {
                        var approved = model.lineItems.First(x => x.loanApplicationDetailId == item.LOANAPPLICATIONDETAILID);
                        if (approved != null)
                        {
                            item.APPROVEDPRODUCTID = (short)approved.approvedProductId;
                            item.APPROVEDAMOUNT = approved.approvedAmount;
                            item.APPROVEDINTERESTRATE = approved.approvedRate;
                            item.APPROVEDTENOR = approved.approvedTenor;
                            item.STATUSID = approved.statusId;
                            item.EXCHANGERATE = approved.exchangeRate;
                            item.LASTUPDATEDBY = model.createdBy;
                            item.DATETIMEUPDATED = DateTime.Now;
                        }
                    }

                }
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"CAM: '{ model.applicationId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId)
        {
            var allstaff = this.GetAllStaffNames();

            return this.context.TBL_APPROVAL_TRAIL
                .Where(x => x.OPERATIONID == (int)OperationsEnum.CAM && x.TARGETID == applicationId)
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
                    toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    comment = x.COMMENT,
                    staffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                }).OrderByDescending(x => x.approvalTrailId);
        }

        public PrivilegeViewModel GetUserPrivilege(int staffId, int applicationId, int operationId = (int)OperationsEnum.CAM)
        {
            operationId = (int)OperationsEnum.CAM; // <--------------------- overide incoming for now

            var privilege = new PrivilegeViewModel();

            var application = this.context.TBL_LOAN_APPLICATION.Find(applicationId);

            var grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                                //.Where(x => x.OperationId == (int)OperationsEnum.CAM && x.ProductClassId == application.tbl_Product.ProductClassId) // REFACTOR!!!!!!!!!!!!
                                .Select(x => x.TBL_APPROVAL_GROUP)
                                .SelectMany(x => x.TBL_APPROVAL_LEVEL)
                                .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

            var grant = grants.FirstOrDefault();
            //var staffApprovalLevelIds = grants.Select(x => x.ApprovalLevelId).ToList();

            if (grant != null)
            {
                return new PrivilegeViewModel
                {
                    viewCamDocument = grant.CANVIEWCAMDOCUMENT,
                    viewUploadedFiles = grant.CANVIEWUPLOADEDFILE,
                    viewApproval = grant.CANVIEWAPPROVAL,
                    canMakeChanges = grant.CANEDIT,
                    canAppendTemplate = grant.CANEDIT,
                    canApprove = grant.CANAPPROVE,
                    canUploadFile = grant.CANUPLOADFILE,
                    canSendRequest = grant.CANSENDJOBREQUEST,
                    approvalLimit = grant.MAXIMUMAMOUNT,
                    userApprovalLevelIds = grants.Select(x => x.APPROVALLEVELID).ToList()
                };
            }
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
        // tbl_Credit_Appraisal_Memorandum_Loan_Detail SHOULD LEAVE THE DB

        public IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId)
        {
            var documentation = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => x.TBL_CREDIT_APPRAISAL_MEMORANDUM).First()
                .SelectMany(x => x.TBL_CREDIT_APPRAISAL_MEMO_DOCUM)
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
                case 2:
                    application.NOTINNEGATIVECRMS = (application.NOTINNEGATIVECRMS == false) ? true : false;
                    result = application.NOTINNEGATIVECRMS;
                    break;
                case 3:
                    application.NOTINBLACKBOOK = (application.NOTINBLACKBOOK == false) ? true : false;
                    result = application.NOTINBLACKBOOK;
                    break;
                case 4:
                    application.NOTINCAMSOL = (application.NOTINCAMSOL == false) ? true : false;
                    result = application.NOTINCAMSOL;
                    break;
                default:
                    break;
            }

            context.SaveChanges();

            return result;
        }
    }
}
