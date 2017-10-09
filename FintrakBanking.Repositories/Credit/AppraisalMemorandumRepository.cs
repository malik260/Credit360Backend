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
            var appl = context.tbl_Loan_Application.Find(applicationId);

            var groupMappings = context.tbl_Approval_Group_Mapping.Where(x => 
                x.OperationId == (int)OperationsEnum.CAM
            //&& x.ProductClassId == appl.tbl_Product.ProductClassId // ---- REFACTOR!!!
            //&& x.ProductId == appl.ProductId // ---- REFACTOR!!!
            );

            if (groupMappings.Any() == false)
            {
                groupMappings = context.tbl_Approval_Group_Mapping.Where(x => 
                    x.OperationId == (int)OperationsEnum.CAM
                //&& x.ProductClassId == appl.tbl_Product.ProductClassId // ---- REFACTOR!!!
                );
            }

            var staffLevels = groupMappings
            .Select(x => x.tbl_Approval_Group)
            .SelectMany(x => x.tbl_Approval_Level)
            .SelectMany(x => x.tbl_Approval_Level_Staff)
            .Select(x => new
            {
                staffId = x.StaffId,
                levelId = x.tbl_Approval_Level.ApprovalLevelId
            })
            .Where(x => x.staffId == staffId);

            var memos = context.tbl_Credit_Appraisal_Memorandum.Where(x => x.LoanApplicationId == applicationId)
                .SelectMany(x => x.tbl_Credit_Appraisal_Memorandum_Document)
                .Select(x => new
                {
                    doc = x,
                    mem = x.tbl_Credit_Appraisal_Memorandum
                })
                .Select(x => new AppraisalMemorandumViewModel
                {
                    documentationId = x.doc.CAMDocumentationId,
                    appraisalMemorandumId = x.mem.AppraisalMemorandumId,
                    loanApplicationId = x.mem.LoanApplicationId,
                    camRef = x.mem.CAMRef,
                    isCompleted = x.mem.IsCompleted,
                    riskRated = x.mem.RiskRated,
                    camDocumentation = x.doc.CAMDocumentation,
                    approvalLevelId = x.doc.ApprovalLevelId
                })
                .OrderByDescending(x => x.documentationId);

            var memo = memos.FirstOrDefault(x => staffLevels.Select(o => o.levelId).Contains(x.approvalLevelId));

            if (memo == null) { return memos.FirstOrDefault(); }

            return memo;
        }


        public AppraisalMemorandumViewModel AddAppraisalMemorandum(AppraisalMemorandumViewModel model)
        {
            var appl = context.tbl_Loan_Application.Find(model.loanApplicationId);

            int approvalLevelId = GetFirstApprovalLevelId(
                //appl.ProductId, 
                //appl.tbl_Product.ProductClassId, 
                model.createdBy);

            var memo = context.tbl_Credit_Appraisal_Memorandum.Where(x => x.LoanApplicationId == model.loanApplicationId).SingleOrDefault();

            if (memo == null)
            {
                var newMemo = new tbl_Credit_Appraisal_Memorandum
                {
                    CompanyId = model.companyId,
                    LoanApplicationId = model.loanApplicationId,
                    CAMRef = appl.ApplicationReferenceNumber,
                    IsCompleted = false,
                    RiskRated = false,
                    CreatedBy = model.createdBy,
                    DateTimeCreated = DateTime.Now
                };

                memo = context.tbl_Credit_Appraisal_Memorandum.Add(newMemo);
            }

            var newDocument = new tbl_Credit_Appraisal_Memorandum_Document
            {
                CAMDocumentation = "Blank Document",
                AppraisalMemorandumId = memo.AppraisalMemorandumId,
                ApprovalLevelId = approvalLevelId,
                CreatedBy = model.createdBy,
                DateTimeCreated = DateTime.Now
            };

            var document = context.tbl_Credit_Appraisal_Memorandum_Document.Add(newDocument);


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added AppraisalMemorandum '{ model.camRef }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            this.FlagSubmittedForAppraisal(model.loanApplicationId);

            context.SaveChanges();

            return new AppraisalMemorandumViewModel
            {
                appraisalMemorandumId = memo.AppraisalMemorandumId,
                loanApplicationId = memo.LoanApplicationId,
                camRef = memo.CAMRef,
                isCompleted = memo.IsCompleted,
                riskRated = memo.RiskRated,
                camDocumentation = document.CAMDocumentation,
                documentationId = document.CAMDocumentationId,
                approvalLevelId = 0
            };
        }

        private int GetFirstApprovalLevelId(/*short productId, int productClassId, */int staffId = 0) // ---- REFACTOR!!!
        {
            var groupMappings = context.tbl_Approval_Group_Mapping.Where(x =>
                x.OperationId == (int)OperationsEnum.CAM // GIVEN ---- REFACTOR!!!
                //&& x.ProductClassId == productClassId
                //&& x.ProductId == productId
            );

            if (groupMappings.Any() == false)
            {
                groupMappings = context.tbl_Approval_Group_Mapping.Where(x =>
                    x.OperationId == (int)OperationsEnum.CAM// GIVEN ---- REFACTOR!!!
                                                            //&& x.ProductClassId == productClassId
                );
            }

            var staffLevels = groupMappings
            .Select(x => x.tbl_Approval_Group)
            .SelectMany(x => x.tbl_Approval_Level)
            .SelectMany(x => x.tbl_Approval_Level_Staff)
            .Select(x => new
            {
                staffId = x.StaffId,
                levelId = x.tbl_Approval_Level.ApprovalLevelId
            })
            .Where(x => x.staffId == staffId);

            if (staffLevels.FirstOrDefault() == null) { throw new Exception("No workflow setup for this product"); }

            return staffLevels.Select(x => x.levelId).First();
        }

        private bool FlagSubmittedForAppraisal(int id)
        {
            var application = context.tbl_Loan_Application.Find(id);
            if (application != null)
            {
                application.SubmittedForAppraisal = true;
                application.ApplicationStatusId = (int)LoanApplicationStatusEnum.CAMInProgress;
                return true;
            }
            return false;
        }

        public bool UpdateAppraisalMemorandum(AppraisalMemorandumViewModel model, int documentId)
        {
            var data = this.context.tbl_Credit_Appraisal_Memorandum_Document.Find(documentId);

            if (data == null) { return false; }

            data.CAMDocumentation = model.camDocumentation;
            data.LastUpdatedBy = model.lastUpdatedBy;
            data.DateTimeUpdated = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Appraisal Memorandum Document'{ model.camRef }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
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
            workflow.Tenor = model.tenor;
            workflow.PoliticallyExposed = model.politicallyExposed;
            // log

             workflow.LogActivity();


            if (workflow.Saved)
            {
                // Audit Section ---------------------------
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.AppraisalMemorandumAdded,
                    StaffId = model.createdBy,
                    BranchId = (short)model.userBranchId,
                    Detail = $"Forward AppraisalMemorandum '{ model.applicationId }' ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = general.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                var appl = context.tbl_Loan_Application.Find(model.applicationId);
                appl.ApprovalStatusId = workflow.StatusId;
                if (appl.ApprovalStatusId == (int)ApprovalStatusEnum.Pending) // redundant block
                {
                    appl.ApprovalStatusId = (int)ApprovalStatusEnum.Processing;
                }

                var memo = this.context.tbl_Credit_Appraisal_Memorandum.Find(model.appraisalMemorandumId);

                if (memo != null)
                {
                    if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                    {
                        appl.ApplicationStatusId = (int)LoanApplicationStatusEnum.CAMCompleted;
                        memo.IsCompleted = true;
                    }

                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved || workflow.StatusId == (int)ApprovalStatusEnum.Authorised) // loan details
                    {
                        var items = context.tbl_Loan_Application_Detail.Where(x => x.LoanApplicationId == memo.LoanApplicationId);

                        foreach (var item in items)
                        {
                            var approved = model.lineItems.First(x => x.loanApplicationDetailId == item.LoanApplicationDetailId);
                            if (approved != null)
                            {
                                item.ApprovedProductId = (short)approved.approvedProductId;
                                item.ApprovedAmount = approved.approvedAmount;
                                item.ApprovedInterestRate = approved.approvedRate;
                                item.ApprovedTenor = approved.approvedTenor;
                                item.StatusId = approved.statusId;
                                item.ExchangeRate = approved.exchangeRate;
                                item.LastUpdatedBy = model.createdBy;
                                item.DateTimeUpdated = DateTime.Now;
                            }
                        }

                    }
                }

                context.SaveChanges();
            }

            return workflow.Saved;
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId)
        {
            var allstaff = this.GetAllStaffNames();

            return this.context.tbl_Approval_Trail
                .Where(x => x.OperationId == (int)OperationsEnum.CAM && x.TargetId == applicationId)
                .Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.ApprovalTrailId,
                    targetId = x.TargetId,
                    arrivalDate = x.ArrivalDate,
                    systemArrivalDateTime = x.SystemArrivalDateTime,
                    responseDate = x.ResponseDate,
                    systemResponseDateTime = x.SystemResponseDateTime,
                    responseStaffId = x.ResponseStaffId,
                    requestStaffId = x.RequestStaffId,
                    fromApprovalLevelId = x.FromApprovalLevelId,
                    toApprovalLevelId = (int)x.ToApprovalLevelId,
                    approvalStateId = x.ApprovalStateId,
                    approvalStatusId = x.ApprovalStatusId,
                    comment = x.Comment,
                    staffName = allstaff.FirstOrDefault(s => s.id == x.RequestStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.RequestStaffId).name,
                }).OrderByDescending(x => x.approvalTrailId);
        }

        public PrivilegeViewModel GetUserPrivilege(int staffId, int applicationId, int operationId = (int)OperationsEnum.CAM)
        {
            var privilege = new PrivilegeViewModel();

            var application = this.context.tbl_Loan_Application.Find(applicationId);
            var grants = context.tbl_Approval_Group_Mapping
                                    .Where(x => x.OperationId == operationId) 
                                    //.Where(x => x.OperationId == (int)OperationsEnum.CAM && x.ProductClassId == application.tbl_Product.ProductClassId) // REFACTOR!!!!!!!!!!!!
                                .Select(x => x.tbl_Approval_Group)
                                .SelectMany(x => x.tbl_Approval_Level)
                                .SelectMany(x => x.tbl_Approval_Level_Staff)
                                    .Where(x => x.StaffId == staffId);

            var grant = grants.FirstOrDefault();
            //var staffApprovalLevelIds = grants.Select(x => x.ApprovalLevelId).ToList();

            if (grant != null)
            {
                return new PrivilegeViewModel
                {
                    viewCamDocument = grant.CanViewCAMDocument,
                    viewUploadedFiles = grant.CanViewUploadedFile,
                    viewApproval = grant.CanViewApproval,
                    canMakeChanges = grant.CanEdit,
                    canAppendTemplate = grant.CanEdit,
                    canApprove = grant.CanApprove,
                    canUploadFile = grant.CanUploadFile,
                    canSendRequest = grant.CanSendJobRequest,
                    approvalLimit = grant.MaximumAmount,
                    userApprovalLevelIds = grants.Select(x => x.ApprovalLevelId).ToList()
                };
            }

            return privilege;
        }

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.tbl_Staff.Select(s => new OperationStaffViewModel
            {
                id = s.StaffId,
                name = s.LastName + " " + s.FirstName
            });
        }

        private bool RunningProcess(int operationId, int targetId)
        {
            var trail = context.tbl_Approval_Trail.FirstOrDefault(x => x.OperationId == operationId && x.TargetId == targetId);
            if (trail == null) { return false; }
            return true;
        }

        public IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId)
        {
            var details = context.tbl_Loan_Application.Where(x => x.LoanApplicationId == applicationId)
                .SelectMany(x => x.tbl_Loan_Application_Detail)
                .Select(x => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = x.LoanApplicationDetailId,
                    applicationId = x.LoanApplicationId,
                    obligorName = x.tbl_Customer.FirstName + " " + x.tbl_Customer.MiddleName + " " + x.tbl_Customer.LastName,
                    currencyCode = x.tbl_Currency.CurrencyCode,

                    proposedProductName = x.tbl_Product.ProductName,
                    proposedTenor = x.ProposedTenor,
                    proposedRate = x.ProposedInterestRate,
                    proposedAmount = x.ProposedAmount,
                    proposedProductId = x.ProposedProductId,

                    approvedProductName = x.tbl_Product1.ProductName, // <----------take note of 1
                    approvedTenor = x.ApprovedTenor,
                    approvedRate = x.ApprovedInterestRate,
                    approvedAmount = x.ApprovedAmount,
                    approvedProductId = x.ApprovedProductId,

                    statusId = x.StatusId,
                    exchangeRate = x.ExchangeRate,
                });

                return details;
        }
        // tbl_Credit_Appraisal_Memorandum_Loan_Detail SHOULD LEAVE THE DB

        public IEnumerable<DocumentationViewModel> GetAllDocumentation(int applicationId)
        {
            var documentation = context.tbl_Loan_Application.Where(x => x.LoanApplicationId == applicationId)
                .Select(x => x.tbl_Credit_Appraisal_Memorandum).First()
                .SelectMany(x => x.tbl_Credit_Appraisal_Memorandum_Document)
                .Select(x => new DocumentationViewModel
                {
                    documentationId = x.CAMDocumentationId,
                    documentation = x.CAMDocumentation,
                    appraisalMemorandumId = x.AppraisalMemorandumId,
                    approvalLevelId = x.ApprovalLevelId,
                });

            return documentation;
        }


    }
}
