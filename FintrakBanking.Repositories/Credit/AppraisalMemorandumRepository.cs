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
                CAMDOCUMENTATION = "New",
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
            if (context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Any() == false)
            {
                var conditions = context.TBL_CONDITION_PRECEDENT.ToList(); // TEMPLATE
                var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).ToList();
                foreach (var f in facilities)
                {
                    foreach (var c in conditions)
                    {
                        var condition = new TBL_LOAN_CONDITION_PRECEDENT
                        {
                            CONDITION = c.CONDITION,
                            ISEXTERNAL = c.ISEXTERNAL,
                            CREATEDBY = c.CREATEDBY,
                            LOANAPPLICATIONID = loanApplicationId,
                            LOANAPPLICATIONDETAILID = f.LOANAPPLICATIONDETAILID,
                            DATETIMECREATED = DateTime.Now
                        };
                        context.TBL_LOAN_CONDITION_PRECEDENT.Add(condition);
                    }
                }
                context.SaveChanges();
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
            var applicationDate = general.GetApplicationDate();

            // init
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            workflow.ProductClassId = model.productClassId;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId;
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
                    foreach (var item in items)
                    {
                        var changed = model.recommendedChanges.FirstOrDefault(x => x.detailId == item.LOANAPPLICATIONDETAILID);
                        if (changed != null)
                        {
                            item.APPROVEDPRODUCTID = (short)changed.productId;
                            item.APPROVEDAMOUNT = changed.amount;
                            item.APPROVEDINTERESTRATE = changed.interestRate;
                            item.APPROVEDTENOR = changed.tenor;
                            item.STATUSID = (short)changed.statusId;
                            item.EXCHANGERATE = changed.exchangeRate;
                            item.LASTUPDATEDBY = model.createdBy;
                            item.DATETIMEUPDATED = DateTime.Now;

                            // log changes
                            context.TBL_LOAN_APPLICATION_DETL_LOG.Add(new TBL_LOAN_APPLICATION_DETL_LOG
                            {
                                LOANAPPLICATIONID = item.LOANAPPLICATIONID,
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
                                CUSTOMERID = item.CUSTOMERID,
                            });
                        }
                    }
                    var approvedAmount = items.Where(x => x.STATUSID != (short)ApprovalStatusEnum.Disapproved).Sum(x => x.APPROVEDAMOUNT);
                    appl.APPROVEDAMOUNT = approvedAmount;
                }
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

            return context.SaveChanges() > 0;
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
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : x.TBL_APPROVAL_LEVEL.LEVELNAME,
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
            /*
            var grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                                //.Where(x => x.OperationId == (int)OperationsEnum.CAM && x.ProductClassId == application.tbl_Product.ProductClassId) // REFACTOR!!!!!!!!!!!!
                                .Select(x => x.TBL_APPROVAL_GROUP)
                                .SelectMany(x => x.TBL_APPROVAL_LEVEL)
                                .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);*/

            var grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                .Join(context.TBL_APPROVAL_GROUP,
                    m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL,
                    mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })//, u=l.TBL_APPROVAL_LEVEL_STAFF })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == staffId),
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
                    });

            var grant = grants.FirstOrDefault();
            //var staffApprovalLevelIds = grants.Select(x => x.ApprovalLevelId).ToList();

            if (grant != null)
            {
                //return new PrivilegeViewModel
                //{
                //userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList()
                //userApprovalLevelIds = grants.Select(x => x.APPROVALLEVELID).ToList()
                //};

                grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
                return grant;
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

        public IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId)
        {
            var details = context.TBL_LOAN_APPLICATION_DETL_LOG.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a,b })
                .Select(x => new LoanApplicationDetailLogViewModel
                {
                    loanApplicationDetailId = x.a.LOANAPPLICATIONDETAILID,
                    applicationId = x.a.LOANAPPLICATIONID,
                    customerId = x.a.CUSTOMERID,
                    approvedTenor = x.a.APPROVEDTENOR,
                    approvedRate = x.a.APPROVEDINTERESTRATE,
                    approvedAmount = x.a.APPROVEDAMOUNT,
                    approvedProductId = x.a.APPROVEDPRODUCTID,
                    statusId = x.a.STATUSID,
                    exchangeRate = x.a.EXCHANGERATE,
                    customerName = x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME,
                   // approvedProductName = x.a.TBL_PRODUCT.PRODUCTNAME,
                    staffName = x.b.FIRSTNAME + " " + x.b.MIDDLENAME + " " + x.b.LASTNAME,
                });

            return details;
        }

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
                case 5:
                    application.NOTINXDS = (application.NOTINXDS == false) ? true : false;
                    result = application.NOTINXDS;
                    break;
                case 6:
                    application.NOTINCRC = (application.NOTINCRC == false) ? true : false;
                    result = application.NOTINCRC;
                    break;
                default:
                    break;
            }

            context.SaveChanges();

            return result;
        }

        #region CAM Pending Applications

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int companyId, int branchId, int staffId)
        {
            int operationId = (int)OperationsEnum.CAM;
            bool isHeadOffice = (branchId == 1) ? true : false;

            int scope = this.GetStaffWorkflowViewScope(operationId, staffId);

            int[] camStages = new int[] {
                (int)LoanApplicationStatusEnum.CAMInProgress,
                (int)LoanApplicationStatusEnum.CAMCompleted,
                (int)LoanApplicationStatusEnum.ChecklistCompleted
            };

            if (scope == (int)ProcessViewScopeEnum.Process) // 3
            {
                return context.TBL_LOAN_APPLICATION.Where(x =>
                    x.COMPANYID == companyId
                    && (x.BRANCHID == branchId || isHeadOffice) // branch filter
                    && x.DELETED == false
                    && camStages.Contains(x.APPLICATIONSTATUSID)
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
                            customerId = x.a.CUSTOMERID,
                            branchId = x.a.BRANCHID,
                            //productClassId = x.a.tbl_Product.ProductClassId,
                            //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                            customerGroupId = x.a.CUSTOMERGROUPID,
                            loanTypeId = x.a.LOANTYPEID,
                            relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                            applicationDate = x.a.APPLICATIONDATE,
                            applicationAmount = x.a.APPLICATIONAMOUNT,
                            approvedAmount = x.a.APPROVEDAMOUNT,
                            interestRate = x.a.INTERESTRATE,
                            applicationTenor = x.a.APPLICATIONTENOR,
                            lastComment = y.COMMENT,
                            currentApprovalStateId = y.APPROVALSTATEID,
                            currentApprovalLevelId = y.TOAPPROVALLEVELID,
                            currentApprovalLevel = y.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                            approvalTrailId = y == null ? 0 : y.APPROVALTRAILID, // for inner sequence ordering
                            loanInformation = x.a.LOANINFORMATION,
                            submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                            customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                            notInNegativeCrms = x.a.NOTINNEGATIVECRMS,
                            notInBlackbook = x.a.NOTINBLACKBOOK,
                            notInCamsol = x.a.NOTINCAMSOL,
                            notInXds = x.a.NOTINXDS,
                            notInCrc = x.a.NOTINCRC,
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
                            //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                            operationId = x.a.OPERATIONID,
                        })
                        .GroupBy(d => d.loanApplicationId)
                        .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                        .OrderByDescending(x => x.applicationDate)
                        .ThenByDescending(x => x.loanApplicationId)
                        ;
            }

            var staffApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF
                .Where(x => x.DELETED == false && x.STAFFID == staffId).Select(x => x.APPROVALLEVELID);

                   
            var pendingApplications = context.TBL_LOAN_APPLICATION.Where(x => camStages.Contains(x.APPLICATIONSTATUSID) && (x.BRANCHID == branchId || isHeadOffice)) // --- branch filter
                .Join(context.TBL_APPROVAL_TRAIL,
                    a => a.LOANAPPLICATIONID, b => b.TARGETID, (a, b) => new { a, b })
                .Where(x => x.b.OPERATIONID == operationId// && (x.b.TOBRANCHID == branchId || x.b.TOBRANCHID == null) // --- branch filter
                );

            int count = pendingApplications.Count(); // for testing

            if (scope == (int)ProcessViewScopeEnum.Group) // 2
            {
                var groupApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF
                    .Where(x => x.DELETED == false && x.STAFFID == staffId)
                    .Select(x => x.TBL_APPROVAL_LEVEL)
                    .Select(x => x.TBL_APPROVAL_GROUP)
                    .SelectMany(x => x.TBL_APPROVAL_GROUP_MAPPING)
                    .Where(x => x.DELETED == false && x.OPERATIONID == operationId)
                    .Select(x => x.TBL_APPROVAL_GROUP)
                    .SelectMany(x => x.TBL_APPROVAL_LEVEL)
                    .Select(x => x.APPROVALLEVELID);

                pendingApplications = pendingApplications.Where(x => groupApprovalLevelIds.Contains((int)x.b.TOAPPROVALLEVELID) && x.b.RESPONSESTAFFID == null);
            }

            if (scope == (int)ProcessViewScopeEnum.Level) // 1
            {
                pendingApplications = pendingApplications.Where(x => staffApprovalLevelIds.Contains((int)x.b.TOAPPROVALLEVELID) && x.b.RESPONSESTAFFID == null);
            }

            return pendingApplications.Select(x => new LoanApplicationViewModel
            {
                //groupRoleId = x.b.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                loanApplicationId = x.a.LOANAPPLICATIONID,
                applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                customerId = x.a.CUSTOMERID,
                branchId = x.a.BRANCHID,
                //productClassId = x.a.tbl_Product.ProductClassId,
                //productClassName = x.a.tbl_Product.tbl_Product_Class.ProductClassName,
                customerGroupId = x.a.CUSTOMERGROUPID,
                loanTypeId = x.a.LOANTYPEID,
                relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                applicationDate = x.a.APPLICATIONDATE,
                applicationAmount = x.a.APPLICATIONAMOUNT,
                approvedAmount = x.a.APPROVEDAMOUNT,
                interestRate = x.a.INTERESTRATE,
                applicationTenor = x.a.APPLICATIONTENOR,
                lastComment = x.b.COMMENT,
                currentApprovalStateId = x.b.APPROVALSTATEID,
                currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                loanInformation = x.a.LOANINFORMATION,
                submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                notInNegativeCrms = x.a.NOTINNEGATIVECRMS,
                notInBlackbook = x.a.NOTINBLACKBOOK,
                notInCamsol = x.a.NOTINCAMSOL,
                notInXds = x.a.NOTINXDS,
                notInCrc = x.a.NOTINCRC,
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
                //customerName = x.a.CustomerId.HasValue ? x.a.tbl_Customer.FirstName + " " + x.a.tbl_Customer.MiddleName + " " + x.a.tbl_Customer.LastName : "",
                operationId = x.a.OPERATIONID,
            })
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId)
            .Distinct();
        }

        public int GetStaffWorkflowViewScope(int operationId, int staffId)
        {
            int scope = (int)ProcessViewScopeEnum.Level; // default @Level
            var staffWorkflow = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                .Select(g => g.TBL_APPROVAL_GROUP)
                .SelectMany(g => g.TBL_APPROVAL_LEVEL)
                .SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

            if (staffWorkflow.Count() > 0)
            {
                scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);
            }

            return scope;
        }

        #endregion CAM Pending Applications

        public IEnumerable<CurrentCommitteeViewModel> GetCurrentCommittee(int loanApplicationId)
        {
            //int operationId = (int)OperationsEnum.CAM;

            var result = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationId && x.SUBMITTEDFORAPPRAISAL == true && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.RESPONSESTAFFID == null), a => a.LOANAPPLICATIONID, t => t.TARGETID, (a, t) => new { a, t })
                .Join(context.TBL_APPROVAL_LEVEL, at => at.t.TOAPPROVALLEVELID, l => l.APPROVALLEVELID, (at, l) => new { at, l })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF, atl => atl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (atl, s) => new { atl, s })
                .Join(context.TBL_APPROVAL_GROUP, atls => atls.atl.l.GROUPID, g => g.GROUPID, (atls, g) => new { atls, g })
                 .Select(x => new CurrentCommitteeViewModel
                 {
                     approvalLevelId = x.atls.atl.l.APPROVALLEVELID,
                     approvalLevelName = x.atls.atl.l.LEVELNAME,
                     approvalGroupName = x.g.GROUPNAME,
                     numberOfApprovals = x.atls.atl.l.NUMBEROFAPPROVALS,
                     groupRoleId = x.g.ROLEID,
                     staffId = x.atls.s.STAFFID,
                     staffName = x.atls.s.TBL_STAFF.FIRSTNAME + " " + x.atls.s.TBL_STAFF.MIDDLENAME + " " + x.atls.s.TBL_STAFF.LASTNAME,
                     vote = 0,
                     comment = string.Empty,
                 });

            return result;
        }

        public bool SecretariatForwardAppraisalMemorandum(ForwardCommitteeCamViewModel model)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            var operationId = (int)OperationsEnum.CAM;

            // init
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = 1;//appl.companyId;

            //workflow.ProductClassId = model.productClassId;
            //workflow.ProductId = model.productId;

            workflow.Amount = appl.APPROVEDAMOUNT;
            workflow.InvestmentGrade = appl.ISINVESTMENTGRADE;
            workflow.Tenor = appl.APPLICATIONTENOR;
            workflow.PoliticallyExposed = appl.ISPOLITICALLYEXPOSED;

            bool result = true;
            foreach (var member in model.votes)
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

            var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDUM.FirstOrDefault(x=>x.LOANAPPLICATIONID == model.applicationId);

            if (memo != null)
            {
                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                    memo.ISCOMPLETED = true;
                }
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
    }
}
