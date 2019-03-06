using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.CreditLimitValidations;
using System.Data.Entity;
using FintrakBanking.ViewModels.Setups.General;
using System.Configuration;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.AlertMonitoring;

namespace FintrakBanking.Repositories.Credit
{
    public class AppraisalMemorandumRepository : IAppraisalMemorandumRepository
    {

        private FinTrakBankingContext contextControl = null;

        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private ICreditLimitValidationsRepository limitValidation;
        private IEmailAlertLogger emailLogger;
        private IOfferLetterAndAvailmentRepository offerLetter;

        public AppraisalMemorandumRepository(
            FinTrakBankingContext context, 
            IGeneralSetupRepository general, 
            IAuditTrailRepository audit, 
            IWorkflow workflow,
            ICreditLimitValidationsRepository limitValidation,
            IEmailAlertLogger _emailLogger,
            IOfferLetterAndAvailmentRepository _offerLetter
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.limitValidation = limitValidation;
            emailLogger = _emailLogger;
            offerLetter = _offerLetter;
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

            int approvalLevelId = GetFirstApprovalLevelId(model.createdBy, (int)OperationsEnum.CAM, appl.PRODUCTCLASSID, null);

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

        private int GetFirstApprovalLevelId(int staffId, int operationId, int? productClassId, int? productId)
        {
            IQueryable<TBL_APPROVAL_GROUP_MAPPING> groupMappings;
            if (productId != null)
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == operationId
                    && x.PRODUCTCLASSID == productClassId
                    && x.PRODUCTID == productId
                );
            }
            else
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    x.OPERATIONID == (int)OperationsEnum.CAM
                    && x.PRODUCTCLASSID == productClassId
                );
            }

            var staff = context.TBL_STAFF.Find(staffId);

            var staffLevels = groupMappings
            .Select(x => x.TBL_APPROVAL_GROUP)
            .SelectMany(x => x.TBL_APPROVAL_LEVEL.Where(l => l.STAFFROLEID == staff.STAFFROLEID))
            .Select(x => new
            {
                staffId = staffId,
                levelId = x.APPROVALLEVELID
            });

            if (staffLevels.Any() == false)
            {
                staffLevels = groupMappings
                .Select(x => x.TBL_APPROVAL_GROUP)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL)
                .SelectMany(x => x.TBL_APPROVAL_LEVEL_STAFF)
                .Select(x => new
                {
                    staffId = x.STAFFID,
                    levelId = x.TBL_APPROVAL_LEVEL.APPROVALLEVELID
                })
                .Where(x => x.staffId == staffId);
            }

            if (staffLevels.FirstOrDefault() == null) { throw new SecureException("No workflow setup for this product"); }
            return staffLevels.Select(x => x.levelId).First();
        }

        private IQueryable<int> GetAllCamProductIds()
        {
            return context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCT_CLASS_PROCESSID == 1)
                .SelectMany(x => x.TBL_PRODUCT)
                .Select(x => (int)x.PRODUCTID);
        }

        private IQueryable<int?> GetLoanApplicationProductIds(int applicationId)
        {
            return context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                .Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => (int?)x.PROPOSEDPRODUCTID)
                .Distinct();
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

            if (data.LASTUPDATEDBY != model.lastUpdatedBy) // archive old
            {
                context.TBL_CREDIT_APPRAISAL_MEMO_LOG.Add(new TBL_CREDIT_APPRAISAL_MEMO_LOG
                {
                    CAMDOCUMENTATION = data.CAMDOCUMENTATION,
                    APPRAISALMEMORANDUMID = data.APPRAISALMEMORANDUMID,
                    CREATEDBY = model.lastUpdatedBy,
                    DATETIMECREATED = DateTime.Now
                });
            }

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

        public WorkflowResponse ForwardAppraisalMemorandum(ForwardViewModel model)
        {
            bool updateApprovedAmount = false;
            int operationId = (int)OperationsEnum.CAM;
            var applicationDate = general.GetApplicationDate();
            List<TBL_LOAN_APPLICATION_DETAIL> items = null;
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            // LoadConditionsAndDynamics(appl.LOANAPPLICATIONID);

            // VALIDATION TODO if (model.recommendedChanges.Count() > 0)
            items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();

            var test = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).ToList();

            decimal totalApprovedAmount = test.Sum(x => x.APPROVEDAMOUNT);

            //decimal totalApprovedAmount = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).Sum(x => x.APPROVEDAMOUNT);
            if (appl.RISKRATINGID != null && model.isBusiness == false)
            {
                ValidateCustomerExposure(1, appl.LOANAPPLICATIONID, totalApprovedAmount, appl.CUSTOMERID, appl.CUSTOMERGROUPID);
            }

            // WORKFLOW
            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
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
            workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;
            // workflow.Disputed = appl.DISPUTED; // buggy

            if (model.forwardAction == 8 || model.forwardAction == 9)
            {
                //workflow.StatusId = (int)ApprovalStatusEnum.Referred;
                var dictionary = GetRepresentStepdownItems(model.applicationId, model.forwardAction,operationId);
                workflow.NextLevelId = dictionary["levelId"];
                workflow.ToStaffId = dictionary["staffId"];
                if (model.forwardAction == 8) workflow.ToStaffId = null;
            }

            string facilityInformationMarkup = GetFacilityInformationMarkup(appl.LOANAPPLICATIONID);

            var placeholders = new AlertPlaceholders();
            if (appl.CUSTOMERGROUPID == null)
            {
                var c = appl.TBL_CUSTOMER;
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
            } else
            {
                placeholders.customerName = "<br />CUSTOMER NAME: " + appl.TBL_CUSTOMER_GROUP.GROUPNAME;
            }
            placeholders.referenceNumber = "<br />APPLICATION REFERENCENUMBER: " + appl.APPLICATIONREFERENCENUMBER;
            placeholders.facilityType = "<br />FACILITY INFORMATION: " + facilityInformationMarkup;
            placeholders.operationName = "<br />OPERATION NAME: Loan Origination";
            placeholders.branchName = "<br />BRANCH NAME: " + appl.TBL_BRANCH.BRANCHNAME;
            workflow.Placeholders = placeholders;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // DETAIL CHANGES
            if (model.recommendedChanges.Count() > 0) // only approving authority
            {
                updateApprovedAmount = true;
                // items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();
                foreach (var changed in model.recommendedChanges)
                {
                    var detail = items.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == changed.detailId);
                    if (detail != null)
                    {
                        if (changed.amount == 0) throw new SecureException("ZERO! => FFW:" + model.forwardAction + ", APR:" + workflow.StatusId + ", APL:" + appl.APPLICATIONSTATUSID + ", CHG:" + model.recommendedChanges.Count() + ", STE:" + workflow.NewState + ", AMO:" + appl.APPROVEDAMOUNT + ", upd:" + updateApprovedAmount + ", EXP:" + appl.TOTALEXPOSUREAMOUNT);

                        detail.APPROVEDPRODUCTID = (short)changed.productId;
                        detail.APPROVEDAMOUNT = changed.amount;
                        detail.APPROVEDINTERESTRATE = changed.interestRate;
                        detail.APPROVEDTENOR = changed.tenor;
                        detail.STATUSID = (short)changed.statusId;
                        detail.EXCHANGERATE = changed.exchangeRate;
                        detail.LASTUPDATEDBY = model.createdBy;
                        detail.DATETIMEUPDATED = DateTime.Now;

                        if (model.isBusiness) // DELETE OR UPDATE PROPOSED
                        {
                            if (detail.STATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; } else
                            {
                                detail.PROPOSEDPRODUCTID = (short)changed.productId;
                                detail.PROPOSEDAMOUNT = changed.amount;
                                detail.PROPOSEDINTERESTRATE = changed.interestRate;
                                detail.PROPOSEDTENOR = changed.tenor;
                            }
                        }

                        /*context.TBL_LOAN_APPLICATION_DETL_LOG.Add(new TBL_LOAN_APPLICATION_DETL_LOG // LOG CHANGES
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
                        });*/
                    }
                }
            }

            // UPDATE APPLICATION
            appl.APPROVALSTATUSID = (short) workflow.StatusId;
            if (model.vote == 1) { appl.DISPUTED = true; }
            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
            if (appl.SUBMITTEDFORAPPRAISAL == false) { appl.SUBMITTEDFORAPPRAISAL = true; } // for product programs
            if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    appl.APPROVEDDATE = applicationDate;
                    appl.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;

                    //Send Email to Customer
                    SendEmailToCustomerForLoanApproval(model.applicationId, model.companyId);

                    //generate offer letter doc
                    offerLetter.AddOfferLetterClauses(model.applicationId, model.staffId,false,false);
                }
                else if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                {
                    SendEmailToCustomerForLoanDisapproval(model.applicationId, model.companyId);
                }

                //applid, 

                if (model.forwardAction == (int)ApprovalStatusEnum.Disapproved) { appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected; }
                if (appl.NEXTAPPLICATIONSTATUSID != null && appl.FINALAPPROVAL_LEVELID != null) { appl.APPLICATIONSTATUSID = (short)appl.NEXTAPPLICATIONSTATUSID; } // may be redundant!!!
                // MEMORANDUM update
                var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDM.Find(model.appraisalMemorandumId);
                if (memo != null) { memo.ISCOMPLETED = true; }
                if (contextControl != null) contextControl.SaveChanges();
            }

            // UPDATE APPROVED AMOUNT
            if (updateApprovedAmount == true && items != null) appl.APPROVEDAMOUNT = totalApprovedAmount;

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

            if (model.comment == "debug_test") throw new SecureException("debug_test => FFW:" + model.forwardAction + ", APR:" + workflow.StatusId + ", APL:" + appl.APPLICATIONSTATUSID + ", CHG:" + model.recommendedChanges.Count() + ", STE:" + workflow.NewState + ", AMO:" + appl.APPROVEDAMOUNT + ", upd:" + updateApprovedAmount + ", EXP:" + appl.TOTALEXPOSUREAMOUNT);

            LogApplicationDetailChanges(appl.LOANAPPLICATIONID, model.createdBy, applicationDate,model.vote , (short)model.forwardAction); // LOG CHANGES
            context.SaveChanges();

            var lastStatus = workflow.StatusId; // prevents the nex

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
                workflow.SetResponse = false;
                workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.OfferLetterApproval, model.applicationId, null, "New pproved application", true, false);
            }

            //workflow.Response.success = true;
            return workflow.Response;
        }

        private Dictionary<string, int> GetRepresentStepdownItems(int applicationId, int action, int operationId)
        {
            int levelId;
            int staffId;

            var trails = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && x.TARGETID == applicationId
                    && x.FROMAPPROVALLEVELID != null
                    && x.TOAPPROVALLEVELID != null
                ).OrderBy(x => x.APPROVALTRAILID);

            if (action == 8)
            {
                var traill = trails.Join(context.TBL_APPROVAL_LEVEL.Where(x => x.LEVELTYPEID == 2)
                        , t => t.FROMAPPROVALLEVELID, l => l.APPROVALLEVELID, (t, l) => new { t, l })
                        .Select(x => new { x.t }).First();
                staffId = traill.t.REQUESTSTAFFID;
                levelId = (int)traill.t.FROMAPPROVALLEVELID;
            }
            else
            {
                var trail = trails.FirstOrDefault();
                staffId = trail.REQUESTSTAFFID;
                levelId = (int)trail.FROMAPPROVALLEVELID;
            }

            if (levelId < 1 || staffId < 1) throw new SecureException("Error while resolving receiving staff.");

            var data = new Dictionary<string, int>();
            data.Add("levelId", levelId);
            data.Add("staffId", staffId);

            return data;
        }

        private void LogApplicationDetailChanges(int applicationId, int staffId, DateTime date, short? decision, short status)
        {
            var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId && x.DELETED == false);
            foreach (var detail in details)
            {
                context.TBL_LOAN_APPLICATION_DETL_LOG.Add(new TBL_LOAN_APPLICATION_DETL_LOG // LOG CHANGES
                {
                    LOANAPPLICATIONDETAILID = detail.LOANAPPLICATIONDETAILID,
                    APPROVEDPRODUCTID = (short)detail.APPROVEDPRODUCTID,
                    APPROVEDTENOR = detail.APPROVEDTENOR,
                    APPROVEDINTERESTRATE = detail.APPROVEDINTERESTRATE,
                    APPROVEDAMOUNT = detail.APPROVEDAMOUNT,
                    EXCHANGERATE = detail.EXCHANGERATE,
                    //STATUSID = detail.STATUSID,
                    STATUSID= status,
                    CREATEDBY = staffId,
                    DATETIMECREATED = date,
                    SYSTEMDATETIME = DateTime.Now,
                    DECISION = decision
                });
            }
            //context.SaveChanges();
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

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId, bool getAll = false)
        {
           
             int[] operations = { (int)OperationsEnum.TermLoanBooking, (int)OperationsEnum.CAM, (int)OperationsEnum.InterestPastDueLoanRepayment,
                    (int)OperationsEnum.RevolvingLoanBooking, (int)OperationsEnum.ContigentLoanBooking,(int)OperationsEnum.OfferLetterApproval,
                (int)OperationsEnum.LoanAvailment,(int)OperationsEnum.LoanTrancheBookingRequest,(int)OperationsEnum.BondsAndGuarantees,
                    (int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.ForeignExchangeLoanBooking,(int)OperationsEnum.LoanAndOverdraftRequestBooking
                ,(int)OperationsEnum.ContigentLoanBooking,(int)OperationsEnum.CustomerInformationApproval};
            
            var allstaff = this.GetAllStaffNames();

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID == operationId && x.TARGETID == applicationId);

            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && operations.Contains(x.OPERATIONID) && x.TARGETID == applicationId);
            }

            var data =  trail.Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.APPROVALTRAILID,
                    comment = x.COMMENT,
                    targetId = x.TARGETID,
                    arrivalDate = x.ARRIVALDATE,
                    systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                    responseDate = x.RESPONSEDATE,
                    systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                    responseStaffId = x.RESPONSESTAFFID,
                    requestStaffId = x.REQUESTSTAFFID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a=>a.APPROVALLEVELID ==x.FROMAPPROVALLEVELID).Select(a=>a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a=>a.APPROVALLEVELID ==x.TOAPPROVALLEVELID).Select(a=>a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                    fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                }).OrderByDescending(x => x.approvalTrailId);

            return data;
        }

        public PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity)
        {
            var operationId = entity.operationId;
            var staffId = entity.createdBy;
            var staff = context.TBL_STAFF.Find(staffId);
            IQueryable<PrivilegeViewModel> grants;
            PrivilegeViewModel grant;

            // check default role
            var rank = context.TBL_STAFF_ROLE.Find(staff.STAFFROLEID);

            grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.PRODUCTCLASSID == entity.productClassId)
                .Join(context.TBL_APPROVAL_GROUP.Where(x => x.DELETED == false),
                    m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true),
                    mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.STAFFID == staffId),
                    gl => gl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (gl, s) => new PrivilegeViewModel
                    {
                        viewCamDocument = s.CANVIEWDOCUMENT,
                        canMakeChanges = s.CANEDIT,
                        canAppendTemplate = s.CANEDIT,
                        viewUploadedFiles = s.CANVIEWUPLOAD,
                        canUploadFile = s.CANUPLOAD,
                        viewApproval = s.CANVIEWAPPROVAL,
                        canApprove = s.CANAPPROVE,
                        approvalLimit = s.MAXIMUMAMOUNT,
                        approvalLevelId = s.APPROVALLEVELID,
                        groupRoleId = gl.mg.g.ROLEID,
                        canEscalate = gl.l.CANESCALATE,
                        levelTypeId = gl.l.LEVELTYPEID,
                    });

            if (grants.Any(x => x.approvalLevelId == entity.levelId) == false) // if no specifics
            {
                grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.PRODUCTCLASSID == entity.productClassId)
                    .Join(context.TBL_APPROVAL_GROUP.Where(x => x.DELETED == false),
                        m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true && x.STAFFROLEID == staff.STAFFROLEID),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new PrivilegeViewModel
                        {
                            viewCamDocument = l.CANVIEWDOCUMENT,
                            canMakeChanges = l.CANEDIT,
                            canAppendTemplate = l.CANEDIT,
                            viewUploadedFiles = l.CANVIEWUPLOAD,
                            canUploadFile = l.CANUPLOAD,
                            viewApproval = l.CANVIEWAPPROVAL,
                            canApprove = l.CANAPPROVE,
                            approvalLimit = l.MAXIMUMAMOUNT,
                            approvalLevelId = l.APPROVALLEVELID,
                            groupRoleId = l.TBL_APPROVAL_GROUP.ROLEID,
                            canEscalate = l.CANESCALATE,
                            levelTypeId = l.LEVELTYPEID,
                        });
            }

            grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant == null) { return GetRelieverPrivilege(entity); }
            grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
            grant.owner = grant.userApprovalLevelIds.Contains((int)entity.levelId);

            return grant;
        }

        private PrivilegeViewModel GetRelieverPrivilege(AuthoritySignatureViewModel entity)
        {
            var now = DateTime.Now;
            var relieverStaff = context.TBL_STAFF_RELIEF
                    .FirstOrDefault(x => x.DELETED == false
                        && x.RELIEFSTAFFID == entity.createdBy
                        && x.STARTDATE <= now
                        && x.ENDDATE >= now
                        && x.ISACTIVE == true
                    );

            if (relieverStaff == null) { return new PrivilegeViewModel(); }

            // mirror above
            var operationId = entity.operationId;
            var staffId = relieverStaff.STAFFID; // changed
            var staff = context.TBL_STAFF.Find(staffId);
            IQueryable<PrivilegeViewModel> grants;
            PrivilegeViewModel grant;

            // check default role
            var rank = context.TBL_STAFF_ROLE.Find(staff.STAFFROLEID);

            grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == entity.productClassId)
                .Join(context.TBL_APPROVAL_GROUP,
                    m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true && x.STAFFROLEID == staff.STAFFROLEID),
                    mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new PrivilegeViewModel
                    {
                        viewCamDocument = l.CANVIEWDOCUMENT,
                        canMakeChanges = l.CANEDIT,
                        canAppendTemplate = l.CANEDIT,
                        viewUploadedFiles = l.CANVIEWUPLOAD,
                        canUploadFile = l.CANUPLOAD,
                        viewApproval = l.CANVIEWAPPROVAL,
                        canApprove = l.CANAPPROVE,
                        approvalLimit = l.MAXIMUMAMOUNT,
                        approvalLevelId = l.APPROVALLEVELID,
                        groupRoleId = l.TBL_APPROVAL_GROUP.ROLEID,
                        canEscalate = l.CANESCALATE,
                        levelTypeId = l.LEVELTYPEID,
                    });

            if (grants.Any() == false) // check specific
            {
                grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.PRODUCTCLASSID == entity.productClassId)
                 .Join(context.TBL_APPROVAL_GROUP.Where(x => x.DELETED == false),
                     m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                 .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true),
                     mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })
                 .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false && x.STAFFID == staffId),
                     gl => gl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (gl, s) => new PrivilegeViewModel
                     {
                         viewCamDocument = s.CANVIEWDOCUMENT,
                         canMakeChanges = s.CANEDIT,
                         canAppendTemplate = s.CANEDIT,
                         viewUploadedFiles = s.CANVIEWUPLOAD,
                         canUploadFile = s.CANUPLOAD,
                         viewApproval = s.CANVIEWAPPROVAL,
                         canApprove = s.CANAPPROVE,
                         approvalLimit = s.MAXIMUMAMOUNT,
                         approvalLevelId = s.APPROVALLEVELID,
                         groupRoleId = gl.mg.g.ROLEID,
                         canEscalate = gl.l.CANESCALATE,
                         levelTypeId = gl.l.LEVELTYPEID,
                     });
            }

            var test = grants.ToList();

            grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant == null) { grant = new PrivilegeViewModel(); } // changed
            grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
            grant.owner = grant.userApprovalLevelIds.Contains((int)entity.levelId);

            return grant;
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

        //public IEnumerable<ApprovedLoanDetailViewModel> GetApprovedLoanDetail(int applicationId)
        //{
        //    var details = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
        //        .Join(context.TBL_LOAN_APPLICATION_DETAIL,
        //        a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
        //        .Select(x => new ApprovedLoanDetailViewModel
        //        {
        //            loanApplicationDetailId = x.d.LOANAPPLICATIONDETAILID,
        //            applicationId = x.d.LOANAPPLICATIONID,
        //            customerId = x.d.TBL_CUSTOMER.CUSTOMERID,
        //            obligorName = x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME,
        //            currencyCode = x.d.TBL_CURRENCY.CURRENCYCODE,

        //            proposedProductName = x.d.TBL_PRODUCT.PRODUCTNAME,
        //            proposedTenor = x.d.PROPOSEDTENOR,
        //            proposedRate = x.d.PROPOSEDINTERESTRATE,
        //            proposedAmount = x.d.PROPOSEDAMOUNT,
        //            proposedProductId = x.d.PROPOSEDPRODUCTID,

        //            approvedProductName = x.d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
        //            approvedTenor = x.d.APPROVEDTENOR,
        //            approvedRate = x.d.APPROVEDINTERESTRATE,
        //            approvedAmount = x.d.APPROVEDAMOUNT,
        //            approvedProductId = x.d.APPROVEDPRODUCTID,

        //            statusId = x.d.STATUSID,
        //            exchangeRate = x.d.EXCHANGERATE,
        //            terms = x.d.REPAYMENTTERMS,
        //            schedule = x.d.REPAYMENTSCHEDULE
        //        });

        //    var test = details.ToList();

        //    return details.ToList();
        //}
        public IEnumerable<LookupViewModel> GetAllCRMSAllCollateralType(int companyid)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.SecuredCollateralType || x.CRMSTYPEID == (int)RegulatoryTypeEnum.UnsecuredCollateralType && x.COMPANYID == companyid).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.DESCRIPTION

            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllCRMSSecuredCollateralType(int companyid)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.SecuredCollateralType && x.COMPANYID==companyid).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllCRMSUnsecuredCollateralType(int companyid)
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.UnsecuredCollateralType && x.COMPANYID == companyid).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public LoanApplicationDetailsViewModel GetLoanApplicationDetail(int applicationId)
        {
            var details = new LoanApplicationDetailsViewModel();
                var facilities = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.DELETED == false),
                    a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Select(x => new ApprovedLoanDetailViewModel
                    {
                        loanApplicationDetailId = x.d.LOANAPPLICATIONDETAILID,
                        applicationId = x.d.LOANAPPLICATIONID,
                        customerId = x.d.TBL_CUSTOMER.CUSTOMERID,
                        obligorName = x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME,
                        currencyCode = x.d.TBL_CURRENCY.CURRENCYCODE,
                        loanPurpose = x.d.LOANPURPOSE,
                        proposedProductName = x.d.TBL_PRODUCT.PRODUCTNAME,
                        proposedTenor = x.d.PROPOSEDTENOR,
                        proposedRate = x.d.PROPOSEDINTERESTRATE,
                        proposedAmount = x.d.PROPOSEDAMOUNT,
                        proposedProductId = x.d.PROPOSEDPRODUCTID,
                        proposedProductClassId = x.d.TBL_PRODUCT.PRODUCTCLASSID,

                        approvedProductName = x.d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                        approvedTenor = x.d.APPROVEDTENOR,
                        approvedRate = x.d.APPROVEDINTERESTRATE,
                        approvedAmount = x.d.APPROVEDAMOUNT,
                        approvedProductId = x.d.APPROVEDPRODUCTID,

                        statusId = x.d.STATUSID,
                        exchangeRate = x.d.EXCHANGERATE,
                        terms = x.d.REPAYMENTTERMS,
                        schedule = x.d.REPAYMENTSCHEDULE,
                        securedByCollateral = x.d.SECUREDBYCOLLATERAL,
                        crmsCollateralTypeId = x.d.CRMSCOLLATERALTYPEID,
                        crmsRepaymentTypeId = x.d.CRMSREPAYMENTAGREEMENTID,
                        isSpecialised = (bool)x.d.ISSPECIALISED,

                        priceIndexId = x.d.PRODUCTPRICEINDEXID,
                        priceIndexName = x.d.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                        productRiskRating = x.d.TBL_PRODUCT.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                        syndicationName = x.d.FIELD2,
                        syndicationRefNo = x.d.FIELD1,
                        syndicationAmount = x.d.FIELD3,
                        conditionPrecedent = x.d.CONDITIONPRECIDENT,
                        conditionSubsequent = x.d.CONDITIONSUBSEQUENT,
                        transactionDynamics = x.d.TRANSACTIONDYNAMICS,

                    })
                    .ToList();

                var customerIds = facilities.Select(x => x.customerId).ToList();
                var duplications = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => customerIds.Contains(x.CUSTOMERID)
                    && x.DELETED == false
                    && x.LOANAPPLICATIONID != applicationId
                    && x.STATUSID == (int)ApprovalStatusEnum.Approved
                )
                .Join(context.TBL_LOAN_APPLICATION.Where(x => x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved), 
                        d => d.LOANAPPLICATIONID, a => a.LOANAPPLICATIONID, (d, a) => new { d, a })
                .Select(x => new DedupeApplicationViewModel
                {
                    applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                    applicationDate = x.a.APPLICATIONDATE,
                    applicationAmount = x.a.APPLICATIONAMOUNT,
                    interestRate = x.a.INTERESTRATE,
                    applicationTenor = x.a.APPLICATIONTENOR,
                    branchName = x.a.TBL_BRANCH.BRANCHNAME,
                    productName = x.d.TBL_PRODUCT.PRODUCTNAME,
                })
                .ToList();

            details.duplications = duplications;
            details.facilities = facilities;
            details.application = GetLoanApplicationInformation(applicationId);

            return details;
        }

        public LoanApplicationDetailsViewModel GetSingleLoanApplicationDetail(int detailId)
        {
            var details = new LoanApplicationDetailsViewModel();
            var facilities = context.TBL_LOAN_APPLICATION//.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == detailId),
                a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                .Select(x => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = x.d.LOANAPPLICATIONDETAILID,
                    applicationId = x.d.LOANAPPLICATIONID,
                    customerId = x.d.TBL_CUSTOMER.CUSTOMERID,
                    obligorName = x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME,
                    currencyCode = x.d.TBL_CURRENCY.CURRENCYCODE,
                    loanPurpose = x.d.LOANPURPOSE,
                    proposedProductName = x.d.TBL_PRODUCT.PRODUCTNAME,
                    proposedTenor = x.d.PROPOSEDTENOR,
                    proposedRate = x.d.PROPOSEDINTERESTRATE,
                    proposedAmount = x.d.PROPOSEDAMOUNT,
                    proposedProductId = x.d.PROPOSEDPRODUCTID,

                    approvedProductName = x.d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                    approvedTenor = x.d.APPROVEDTENOR,
                    approvedRate = x.d.APPROVEDINTERESTRATE,
                    approvedAmount = x.d.APPROVEDAMOUNT,
                    approvedProductId = x.d.APPROVEDPRODUCTID,

                    statusId = x.d.STATUSID,
                    exchangeRate = x.d.EXCHANGERATE,
                    terms = x.d.REPAYMENTTERMS,
                    schedule = x.d.REPAYMENTSCHEDULE,
                    securedByCollateral = x.d.SECUREDBYCOLLATERAL,
                    crmsCollateralTypeId = x.d.CRMSCOLLATERALTYPEID,
                    isSpecialised = (bool)x.d.ISSPECIALISED,

                    priceIndexId = x.d.PRODUCTPRICEINDEXID,
                    priceIndexName = x.d.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                    productRiskRating = x.d.TBL_PRODUCT.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                    syndicationName = x.d.FIELD2,
                    syndicationRefNo = x.d.FIELD1,
                    syndicationAmount = x.d.FIELD3,
                    conditionPrecedent = x.d.CONDITIONPRECIDENT,
                    conditionSubsequent = x.d.CONDITIONSUBSEQUENT,

                }).ToList();
            //var syndicated = context.TBL_LOAN_APPLICATION_DETAIL//.Where(x => x.LOANAPPLICATIONID == applicationId)
            //    .Join(context.TBL_LOAN_APPLICATION_DETL_SYN.Where(x => x.LOANAPPLICATIONDETAILID == detailId),
            //    a => a.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (a, d) => new { a, d })
            //    .Select(x => new SyndicatedLoanDetailViewModel
            //    {
            //        syndicationId = x.d.SYNDICATIONID,
            //        loanApplicationDetailId = x.a.LOANAPPLICATIONDETAILID,
            //        bankCode = x.d.BANKCODE,
            //        bankName = x.d.BANKNAME,
            //        amountContributed = x.d.AMOUNTCONTRIBUTED,
            //        typeId = (short)x.d.PARTY_TYPEID,
            //        typeName = context.TBL_LOAN_SYNDICATION_PARTY_TYP.Where(f => f.PARTY_TYPEID == x.d.PARTY_TYPEID).FirstOrDefault().PARTY_TYPENAME,
            //    }).ToList();
            details.facilities = facilities;
            details.application = GetLoanApplicationInformation(detailId,true);
           // details.syndicated = syndicated;
            return details;
        }

        private LoanApplicationViewModel GetLoanApplicationInformation(int id, bool fromdetail = false)
        {
            LoanApplicationViewModel application = new LoanApplicationViewModel();
            var entity = context.TBL_LOAN_APPLICATION.Where(a => a.LOANAPPLICATIONID == id);
            if (fromdetail)
            {
                var detail = context.TBL_LOAN_APPLICATION_DETAIL.Find(id);
                entity = context.TBL_LOAN_APPLICATION.Where(a => a.LOANAPPLICATIONID == detail.LOANAPPLICATIONID);
            }

            application = entity.Select(a => new LoanApplicationViewModel
            {
                requireCollateral = a.REQUIRECOLLATERAL,
                approvalStatusId = (short)a.APPROVALSTATUSID,
                loanApplicationId = a.LOANAPPLICATIONID,
                applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                customerId = a.CUSTOMERID ?? 0,
                customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                loanInformation = a.LOANINFORMATION,
                companyId = a.COMPANYID,
                branchId = (short)a.BRANCHID,
                branchName = a.TBL_BRANCH.BRANCHNAME,
                relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                relationshipManagerId = a.RELATIONSHIPMANAGERID,
                relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                misCode = a.MISCODE,
                teamMisCode = a.TEAMMISCODE,
                interestRate = a.INTERESTRATE,
                isRelatedParty = a.ISRELATEDPARTY,
                isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                customerGroupId = a.CUSTOMERGROUPID ?? 0,
                customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                createdBy = a.CREATEDBY,
                applicationDate = a.APPLICATIONDATE,
                applicationTenor = a.APPLICATIONTENOR,
                applicationAmount = a.APPLICATIONAMOUNT,
                dateTimeCreated = a.DATETIMECREATED,
                collateralDetail = a.COLLATERALDETAIL,
                loanPurpose = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID && c.DELETED == false).Select(l => l.LOANPURPOSE).FirstOrDefault(),
                LoanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID && c.DELETED == false)
                                            .Select(c => new LoanApplicationDetailViewModel
                                            {
                                                equityAmount = c.EQUITYAMOUNT,
                                                equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                                approvedAmount = c.APPROVEDAMOUNT,
                                                approvedInterestRate = c.APPROVEDINTERESTRATE,
                                                approvedProductId = c.APPROVEDPRODUCTID,
                                                approvedTenor = c.APPROVEDTENOR,
                                                currencyId = c.CURRENCYID,
                                                currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                                customerId = c.CUSTOMERID,
                                                loanPurpose = c.LOANPURPOSE,
                                                exchangeRate = c.EXCHANGERATE,
                                                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                                subSectorId = c.SUBSECTORID,
                                                loanApplicationId = c.LOANAPPLICATIONID,
                                                proposedAmount = c.PROPOSEDAMOUNT,
                                                proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                                proposedProductId = c.PROPOSEDPRODUCTID,
                                                proposedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                                statusId = c.STATUSID,
                                                priceIndexId = c.PRODUCTPRICEINDEXID,
                                                priceIndexName = c.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                                fieldOne = c.FIELD1,
                                                fieldTwo = c.FIELD2,
                                                fieldThree = c.FIELD3,
                                                conditionPrecedent = c.CONDITIONPRECIDENT,
                                                conditionSubsequent = c.CONDITIONSUBSEQUENT,
                                            }).ToList()
            }).FirstOrDefault();
            return application;
        }

        public IEnumerable<LoanDetailsFeeViewModel> GetLoanDetailsFee(int applicationId)
        {
            var fees = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .SelectMany(x => x.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.DELETED == false))
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
                    productName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME
                });

            return fees;
        }

        public IEnumerable<LoanApplicationDetailLogViewModel> GetLoanDetailChangeLog(int applicationId)
        {
            var details = context.TBL_LOAN_APPLICATION_DETL_LOG.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
                .Select(x => new LoanApplicationDetailLogViewModel
                {
                    loanApplicationlogId = x.a.LOAN_APPLICATION_DETAIL_LOGID,
                    loanApplicationDetailId = x.a.LOANAPPLICATIONDETAILID,
                    applicationId = x.a.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                    customerId = x.a.TBL_LOAN_APPLICATION_DETAIL.CUSTOMERID,
                    approvedTenor = x.a.APPROVEDTENOR,
                    approvedRate = x.a.APPROVEDINTERESTRATE,
                    approvedAmount = x.a.APPROVEDAMOUNT,
                    approvedProductId = x.a.APPROVEDPRODUCTID,
                    statusId = x.a.STATUSID,
                    exchangeRate = x.a.EXCHANGERATE,
                    decision = x.a.DECISION,
                    customerName = x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME,
                    approvedProductName = x.a.TBL_PRODUCT.PRODUCTNAME,
                    staffName = x.b.FIRSTNAME + " " + x.b.MIDDLENAME + " " + x.b.LASTNAME,
                }).OrderByDescending(p=>p.loanApplicationlogId);

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

        #region CAM Pending Applications

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int operationId, int companyId, int branchId, int staffId, int? classId)
        {
            // var declarations
            IQueryable<LoanApplicationViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            // query
            var query = context.TBL_LOAN_APPLICATION.Where(x =>
                    x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                    && x.COMPANYID == companyId
                    && (classId == null) ? true : (x.PRODUCTCLASSID == (short?)classId)
                )
            .OrderByDescending(x => x.LOANAPPLICATIONID)
            .Join(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && x.RESPONSESTAFFID == null
                    && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                    && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)
                ),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (a, b) => new { a, b })
            .Select(x => new LoanApplicationViewModel
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
                loanTypeId = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                applicationDate = x.a.APPLICATIONDATE,
                //newApplicationDate = x.a.APPLICATIONDATE,
                applicationAmount = x.a.APPLICATIONAMOUNT,
                approvedAmount = x.a.APPROVEDAMOUNT,
                interestRate = x.a.INTERESTRATE,
                applicationTenor = x.a.APPLICATIONTENOR,
                lastComment = x.b.COMMENT,
                currentApprovalStateId = x.b.APPROVALSTATEID,
                currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                currentApprovalLevelTypeId = x.b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                approvalTrailId = x.b == null ? 0 : x.b.APPROVALTRAILID, // for inner sequence ordering
                toStaffId = x.b.TOSTAFFID,
                timeIn = x.b.SYSTEMARRIVALDATETIME,
                slaTime = x.b.SLADATETIME,
                loanInformation = x.a.LOANINFORMATION,
                submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                isRelatedParty = x.a.ISRELATEDPARTY,
                isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                approvalStatusId = (short)x.a.APPROVALSTATUSID,
                applicationStatusId = x.a.APPLICATIONSTATUSID,
                branchName = x.a.TBL_BRANCH.BRANCHNAME,
                relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                misCode = x.a.MISCODE,
                loanTypeName = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                createdBy = x.a.CREATEDBY,
                loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
                operationId = x.a.OPERATIONID,
                productClassProcessId = x.a.PRODUCT_CLASS_PROCESSID,
                tranchLevelId = x.a.TRANCHEAPPROVAL_LEVELID
            })
            .Where(x => x.currentApprovalLevelTypeId != 2) // hou
            .ToList()
            ;

            applications = query.AsQueryable()
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications;//.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        public List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user)
        {
            int staffId = user.staffId;
            int operationId = (int)OperationsEnum.CAM;
            var levelIds = general.GetStaffApprovalLevelIds(user.staffId, operationId);// new int[] {3,1,5};
            int productBasedId = (int)ProductClassProcessEnum.ProductBased;

            var applications = context.TBL_LOAN_APPLICATION.Where(x =>
                x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                && x.COMPANYID == user.companyId
                && x.PRODUCTCLASSID != null
            )
            .Join(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                    && x.APPROVALSTATEID != (int)ApprovalState.Ended
                    && x.RESPONSESTAFFID == null
                    && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                    && (x.TOSTAFFID == null || x.TOSTAFFID == staffId)
                    ),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (a, b) => new { a, b })
            .Select(
                x => new
                {
                    loanApplicationId = x.a.LOANAPPLICATIONID,
                    approvalTrailId = x == null ? 0 : x.b.APPROVALTRAILID, // for inner sequence ordering
                    currentApprovalLevelTypeId = x.b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                    productClassId = x.a.PRODUCTCLASSID,
                    currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                    toStaffId = x.b.TOSTAFFID,
                })
            .Where(x => x.currentApprovalLevelTypeId != 2) // hou
            .GroupBy(d => d.loanApplicationId)
            .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
            .ToList()
            ;

            var productClasses = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCT_CLASS_PROCESSID == productBasedId)
                .Select(item => new PendingProductProgramViewModel
                {
                    productClassId = item.PRODUCTCLASSID,
                    productClassName = item.PRODUCTCLASSNAME,
                    pendingNumber = 0,
                })
                .ToList();

            var result = new List<PendingProductProgramViewModel>();
            foreach (var pc in productClasses)
            {
                result.Add(new PendingProductProgramViewModel
                {
                    productClassId = pc.productClassId,
                    productClassName = pc.productClassName,
                    pendingNumber = applications.Count(x => x.productClassId == pc.productClassId)
                });
            }

            return result;
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

            var xcxc = result;

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

            appl.APPROVALSTATUSID = (short)workflow.StatusId;

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
                var items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false);
                var approvedAmount = items.Where(x => x.STATUSID != (short)ApprovalStatusEnum.Disapproved).Sum(x => x.APPROVEDAMOUNT);
                appl.APPROVEDAMOUNT = approvedAmount;

                //Send Email to Customer
                if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                {
                    SendEmailToCustomerForLoanApproval(model.applicationId,model.companyId);

                }else if(appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                {
                    SendEmailToCustomerForLoanDisapproval(model.applicationId, model.companyId);
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

            bool response = (context.SaveChanges() > 0) == result;

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
                workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.OfferLetterApproval, model.applicationId, null, "New pproved application", true, false);
            }

            return response;
        }

        public IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;

            List<int> levels = general.GetRouteLevels(operationId, 1);//.ToList().Distinct();

            //var branches = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
            //                .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
            //                .Join(context.TBL_BRANCH, sr => sr.r.REGIONID, b => b.REGIONID, (sr, b) => new { sr, b })
            //                .Select(x => new {
            //                    BRANCHID = x.b.BRANCHID
            //                })
            //                .Select(x => x.BRANCHID)
            //                .ToList();

            var regions = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
                            .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
                            .Select(x => new
                            {
                                REGIONID = x.r.REGIONID
                            })
                            .Select(x => x.REGIONID)
                            .ToList();

            var applications = context.TBL_LOAN_APPLICATION
                .Where(x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                    && regions.Contains((int)x.CAPREGIONID)
                    // && branches.Contains(x.BRANCHID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                    && x.SUBMITTEDFORAPPRAISAL == true
                )
                .OrderByDescending(x => x.LOANAPPLICATIONID)
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                        && (x.RESPONSESTAFFID == null || (x.RESPONSESTAFFID == staffId && x.TOSTAFFID != null))
                        && x.TOAPPROVALLEVELID != null && levels.Contains((int)x.TOAPPROVALLEVELID)),
                    a => a.LOANAPPLICATIONID, b => b.TARGETID, (a, b) => new { a, b })
                .Select(x => new RegionLoanApplicationViewModel
                    {
                        loanApplicationId = x.a.LOANAPPLICATIONID,
                        applicationDate = x.a.APPLICATIONDATE,
                        applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                        branchId = x.a.PRODUCTCLASSID,
                        productClassId = x.a.PRODUCTCLASSID,
                        finalApprovalLevelId = x.a.FINALAPPROVAL_LEVELID,
                        nextApplicationStatusId = x.a.NEXTAPPLICATIONSTATUSID,
                        customerId = x.a.CUSTOMERID,
                        applicationAmount = x.a.APPLICATIONAMOUNT,
                        interestRate = x.a.INTERESTRATE,
                        applicationTenor = x.a.APPLICATIONTENOR,
                        submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                        approvalStatusId = x.a.APPROVALSTATUSID,
                        operationId = x.a.OPERATIONID,
                        customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                        customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
                        facilityType = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED != true && s.STATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                        responsiblePerson = context.TBL_STAFF
                                                    .Where(s => s.STAFFID == x.b.TOSTAFFID)
                                                    .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME })
                                                    .FirstOrDefault().name ?? "",
                        timeIn = x.b.SYSTEMARRIVALDATETIME,
                        timeOut = x.b.SYSTEMRESPONSEDATETIME,
                        currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                        currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME,
                        currentApprovalLevelTypeId = x.b.TBL_APPROVAL_LEVEL1.LEVELTYPEID,
                        requestStaffId = x.b.REQUESTSTAFFID,
                        toApprovalLevelId = x.b.TOAPPROVALLEVELID,
                        toStaffId = x.b.TOSTAFFID,
                        approvalTrailId = x.b.APPROVALTRAILID,
                })
                .GroupBy(d => d.approvalTrailId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                ;

            return applications;
        }

        public bool GetUntenoredStatus(int applicationId)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETL_BG
                .FirstOrDefault(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId && x.DELETED == false);
            return detail == null ? false : !detail.ISTENORED;
        }

        public IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId)
        {
            return context.TBL_LOAN_APPLICATN_DETL_MTRIG
                .Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId && x.DELETED == false)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = x.LOANAPPLICATIONDETAILID,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER,
                    productCustomerName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
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

        public List<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTerms(RepaymentScheduleTermsViewModel model)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.applicationDetailId);
            detail.REPAYMENTTERMS = model.terms;
            detail.REPAYMENTSCHEDULE = model.schedule;
            context.SaveChanges();
            return context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == detail.LOANAPPLICATIONID && x.DELETED == false)
                .Select(x => new RepaymentScheduleTermsViewModel
                {
                    applicationDetailId = x.LOANAPPLICATIONDETAILID,
                    terms = x.REPAYMENTTERMS,
                    schedule = x.REPAYMENTSCHEDULE,
                    productCustomerName = x.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME
                }).ToList();

            //return new List<RepaymentScheduleTermsViewModel>();
        }

        public List<ProductLimitValidationViewModel> SaveProductLimitValidation(ProductLimitValidationViewModel entity)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Find(entity.applicationDetailId);
            detail.APPROVEDAMOUNT = entity.recommendedAmount;

            if (entity.productClassId == 7)
            {
                var control = context.TBL_LOAN_APPLICATION_DETL_EDU.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == entity.applicationDetailId);
                control.TOTAL_PREVIOUS_TERM_SCHOL_FEES = entity.controlAmount;
            }

            context.SaveChanges();
            return GetProductLimitValidation(detail.LOANAPPLICATIONID, entity.productClassId);
        }

        public List<ProductLimitValidationViewModel> GetProductLimitValidation(int applicationId, int classId)
        {
            List<ProductLimitValidationViewModel> limits = new List<ProductLimitValidationViewModel>();
            //List<ProductLimitValidationViewModel> limits = null;
            //List<ProductLimitValidationViewModel> limits;

            if (classId == 7) // first edu
            {
                limits = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId && x.DELETED == false)
                       .Join(context.TBL_LOAN_APPLICATION_DETL_EDU, a => a.LOANAPPLICATIONDETAILID, b => b.LOANAPPLICATIONDETAILID, (a, b) => new { a, b })
                       .Join(context.TBL_PRODUCT_BEHAVIOUR, ab => ab.a.APPROVEDPRODUCTID, c => c.PRODUCTID, (ab, c) => new { ab, c })
                       .Select(x => new ProductLimitValidationViewModel
                       {
                           applicationDetailId = x.ab.a.LOANAPPLICATIONDETAILID,
                           productCustomerName = x.ab.a.TBL_PRODUCT.PRODUCTNAME + " -- " + x.ab.a.TBL_CUSTOMER.FIRSTNAME + " " + x.ab.a.TBL_CUSTOMER.MIDDLENAME + " " + x.ab.a.TBL_CUSTOMER.LASTNAME,
                           recommendedAmount = x.ab.a.APPROVEDAMOUNT,
                           controlAmount = x.ab.b.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                           percentageLimit = x.c.PRODUCT_LIMIT,
                           productClassId = classId
                       }).ToList();
            }

            return limits;
        }

        public List<RecommendedCollateralViewModel> GetRecommendedCollateral(int applicationId)
        {
            return context.TBL_LOAN_APPLICATION_COLLATRL2.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => new RecommendedCollateralViewModel
                {
                    id = x.COLLATERALBASICDETAILID,
                    collateralDetail = x.COLLATERALDETAIL,
                    collateralValue = x.COLLATERALVALUE,
                    stampedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                    applicationDetailId = (int)x.LOANAPPLICATIONDETAILID,
                    productCustomerName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
                })
                .ToList();
        }

        public List<RecommendedCollateralViewModel> AddRecommendedCollateral(RecommendedCollateralViewModel entity)
        {
            var recommendation = context.TBL_LOAN_APPLICATION_COLLATRL2.Add(new TBL_LOAN_APPLICATION_COLLATRL2
            {
                LOANAPPLICATIONID = entity.applicationId,
                LOANAPPLICATIONDETAILID = entity.applicationDetailId,
                COLLATERALDETAIL = entity.collateralDetail,
                COLLATERALVALUE = entity.collateralValue,
                STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount,
                DATETIMECREATED = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });

            context.TBL_LOAN_APPLICATION_COLT2_LOG.Add(new TBL_LOAN_APPLICATION_COLT2_LOG
            {
                COLLATERALBASICDETAILID = recommendation.COLLATERALBASICDETAILID,
                LOANAPPLICATIONID = entity.applicationId,
                LOANAPPLICATIONDETAILID = entity.applicationDetailId,
                COLLATERALDETAIL = entity.collateralDetail,
                COLLATERALVALUE = entity.collateralValue,
                STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount,
                DATETIMECREATED = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                CREATEDBY = entity.createdBy
            });

            context.SaveChanges();
            return GetRecommendedCollateral(entity.applicationId);
        }

        public List<RecommendedCollateralViewModel> UpdateRecommendedCollateral(RecommendedCollateralViewModel entity)
        {
            var recommendation = context.TBL_LOAN_APPLICATION_COLLATRL2.Find(entity.id);
            recommendation.LOANAPPLICATIONDETAILID = entity.applicationDetailId;
            recommendation.COLLATERALDETAIL = entity.collateralDetail;
            recommendation.COLLATERALVALUE = entity.collateralValue;
            recommendation.STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount;

            context.TBL_LOAN_APPLICATION_COLT2_LOG.Add(new TBL_LOAN_APPLICATION_COLT2_LOG
            {
                COLLATERALBASICDETAILID = recommendation.COLLATERALBASICDETAILID,
                LOANAPPLICATIONID = recommendation.LOANAPPLICATIONID,
                LOANAPPLICATIONDETAILID = entity.applicationDetailId,
                COLLATERALDETAIL = entity.collateralDetail,
                COLLATERALVALUE = entity.collateralValue,
                STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount,
                DATETIMECREATED = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                CREATEDBY = entity.createdBy
            });

            context.SaveChanges();
            return GetRecommendedCollateral(entity.applicationId);
        }

        public List<RecommendedCollateralViewModel> GetRecommendedCollateralHistory(int applicationId)
        {
            return context.TBL_LOAN_APPLICATION_COLT2_LOG.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
                .Join(context.TBL_LOAN_APPLICATION_DETAIL, ab => ab.a.LOANAPPLICATIONDETAILID, c => c.LOANAPPLICATIONDETAILID, (ab, c) => new { ab, c })
               .Select(x => new RecommendedCollateralViewModel
               {
                   id = x.ab.a.COLLATERALBASICDETAILID,
                   collateralDetail = x.ab.a.COLLATERALDETAIL,
                   collateralValue = x.ab.a.COLLATERALVALUE,
                   stampedToCoverAmount = x.ab.a.STAMPEDTOCOVERAMOUNT, 
                   applicationDetailId = (int)x.ab.a.LOANAPPLICATIONDETAILID,
                   productCustomerName = x.c.TBL_PRODUCT.PRODUCTNAME + " -- " + x.c.TBL_CUSTOMER.FIRSTNAME + " " + x.c.TBL_CUSTOMER.MIDDLENAME + " " + x.c.TBL_CUSTOMER.LASTNAME,
                   staffName = x.ab.b.FIRSTNAME + " " + x.ab.b.MIDDLENAME + " " + x.ab.b.LASTNAME,
               })
               .ToList();
        }

        #region LMS APPROVAL

        public IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggersLms(int applicationId)
        {
            return context.TBL_LMSR_APPLICATN_DETL_MTRIG
                .Where(x => x.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = x.LOANREVIEWAPPLICATIONID,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER,
                    productCustomerName = x.TBL_LMSR_APPLICATION_DETAIL.TBL_OPERATIONS.OPERATIONNAME
                })
                .ToList();
        }

        public IEnumerable<MonitoringTriggersViewModel> SaveApplicationMonitoringTriggersLms(int applicationId, List<MonitoringTriggersViewModel> items, int staffId)
        {
            context.TBL_LMSR_APPLICATN_DETL_MTRIG
                .RemoveRange(
                    context.TBL_LMSR_APPLICATN_DETL_MTRIG.Where(x => x.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                );
            context.SaveChanges();

            foreach (var o in items)
            {
                context.TBL_LMSR_APPLICATN_DETL_MTRIG.Add(new TBL_LMSR_APPLICATN_DETL_MTRIG
                {
                    LOANREVIEWAPPLICATIONID = o.applicationDetailId,
                    MONITORING_TRIGGERID = o.monitoringTriggerId,
                    MONITORING_TRIGGER = o.monitoringTrigger,
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now
                });
            }
            context.SaveChanges();

            return GetApplicationMonitoringTriggers(applicationId);
        }

        public List<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTermsLms(RepaymentScheduleTermsViewModel entity)
        {
            var detail = context.TBL_LMSR_APPLICATION_DETAIL.Find(entity.applicationDetailId);
            detail.REPAYMENTTERMS = entity.terms;
            detail.REPAYMENTSCHEDULE = entity.schedule;
            context.SaveChanges();
            return context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == detail.LOANAPPLICATIONID)
                .Select(x => new RepaymentScheduleTermsViewModel
                {
                    applicationDetailId = x.LOANREVIEWAPPLICATIONID,
                    terms = x.REPAYMENTTERMS,
                    schedule = x.REPAYMENTSCHEDULE,
                    productCustomerName = x.TBL_OPERATIONS.OPERATIONNAME
                }).ToList();
        }

        public List<RecommendedCollateralViewModel> UpdateRecommendedCollateralLms(RecommendedCollateralViewModel entity)
        {
            var recommendation = context.TBL_LMSR_APPLICATION_COLLATRL2.Find(entity.id);
            recommendation.LOANREVIEWAPPLICATIONID = entity.applicationDetailId;
            recommendation.COLLATERALDETAIL = entity.collateralDetail;
            recommendation.COLLATERALVALUE = entity.collateralValue;
            recommendation.STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount;
            context.SaveChanges();
            return GetRecommendedCollateralLms(entity.applicationId);
        }

        public List<RecommendedCollateralViewModel> AddRecommendedCollateralLms(RecommendedCollateralViewModel entity)
        {
            context.TBL_LMSR_APPLICATION_COLLATRL2.Add(new TBL_LMSR_APPLICATION_COLLATRL2
            {
                LOANAPPLICATIONID = entity.applicationId,
                LOANREVIEWAPPLICATIONID = entity.applicationDetailId,
                COLLATERALDETAIL = entity.collateralDetail,
                COLLATERALVALUE = entity.collateralValue,
                STAMPEDTOCOVERAMOUNT = entity.stampedToCoverAmount,
                DATETIMECREATED = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            context.SaveChanges();
            return GetRecommendedCollateralLms(entity.applicationId);
        }

        public List<RecommendedCollateralViewModel> GetRecommendedCollateralLms(int applicationId)
        {
            return context.TBL_LMSR_APPLICATION_COLLATRL2.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => new RecommendedCollateralViewModel
                {
                    id = x.COLLATERALBASICDETAILID,
                    collateralDetail = x.COLLATERALDETAIL,
                    collateralValue = x.COLLATERALVALUE,
                    stampedToCoverAmount = x.STAMPEDTOCOVERAMOUNT,
                    applicationDetailId = x.LOANREVIEWAPPLICATIONID,
                    productCustomerName = x.TBL_LMSR_APPLICATION_DETAIL.TBL_OPERATIONS.OPERATIONNAME
                })
                .ToList();
        }

        public bool saveTranchDisbursmentApprovalLevel(TranchDisbursmentViewModel entity)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(entity.loanApplicationId);
            appl.TRANCHEAPPROVAL_LEVELID = entity.approvalLevelId;
            context.Entry(appl).State = System.Data.Entity.EntityState.Modified;
            return context.SaveChanges() > 0;
        }

        public List<RecommendedCollateralViewModel> GetRecommendedCollateralHistoryLms(int applicationId)
        {
            return context.TBL_LOAN_APPLICATION_COLT2_LOG.Where(x => x.LOANAPPLICATIONID == applicationId) // TBL_LOAN_APPLICATION_COLT2_LOG for LMS
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
                .Join(context.TBL_LOAN_APPLICATION_DETAIL, ab => ab.a.LOANAPPLICATIONDETAILID, c => c.LOANAPPLICATIONDETAILID, (ab, c) => new { ab, c })
               .Select(x => new RecommendedCollateralViewModel
               {
                   id = x.ab.a.COLLATERALBASICDETAILID,
                   collateralDetail = x.ab.a.COLLATERALDETAIL,
                   collateralValue = x.ab.a.COLLATERALVALUE,
                   stampedToCoverAmount = x.ab.a.STAMPEDTOCOVERAMOUNT,
                   applicationDetailId = (int)x.ab.a.LOANAPPLICATIONDETAILID,
                   productCustomerName = x.c.TBL_PRODUCT.PRODUCTNAME + " -- " + x.c.TBL_CUSTOMER.FIRSTNAME + " " + x.c.TBL_CUSTOMER.MIDDLENAME + " " + x.c.TBL_CUSTOMER.LASTNAME,
                   staffName = x.ab.b.FIRSTNAME + " " + x.ab.b.MIDDLENAME + " " + x.ab.b.LASTNAME,
               })
               .ToList();
        }

        #endregion LMS APPROVAL

        public LoanApplicationDetailsViewModel GetLMSLoanApplicationDetail(int applicationId)
        {
            var details = new LoanApplicationDetailsViewModel();
            var facilities = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Join(context.TBL_LMSR_APPLICATION_DETAIL,
                a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                .Select(x => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = x.d.LOANREVIEWAPPLICATIONID,
                    applicationId = x.d.LOANAPPLICATIONID,
                    customerId = x.d.TBL_CUSTOMER.CUSTOMERID,
                    obligorName = x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME,
                  //  currencyCode = x.d.TBL_CURRENCY.CURRENCYCODE,

                    proposedProductName = x.d.TBL_PRODUCT.PRODUCTNAME,
                    proposedTenor = x.d.PROPOSEDTENOR,
                    proposedRate = x.d.PROPOSEDINTERESTRATE,
                    proposedAmount = x.d.PROPOSEDAMOUNT,
                    proposedProductId = x.d.PRODUCTID,

                    approvedProductName = context.TBL_PRODUCT.Where(s=>s.PRODUCTID==x.d.PRODUCTID).Select(s=>s.PRODUCTNAME).FirstOrDefault(), // <----------take note of 1
                    approvedTenor = x.d.APPROVEDTENOR,
                    approvedRate = x.d.APPROVEDINTERESTRATE,
                    approvedAmount = x.d.APPROVEDAMOUNT,
                    approvedProductId = x.d.PRODUCTID,

                  //  statusId = x.d.STATUSID,
                   // exchangeRate = x.d.EXCHANGERATE,
                    terms = x.d.REPAYMENTTERMS,
                    schedule = x.d.REPAYMENTSCHEDULE,
                   // securedByCollateral = x.d.SECUREDBYCOLLATERAL,
                  //  crmsCollateralTypeId = x.d.CRMSCOLLATERALTYPEID,
                 //   isSpecialised = x.d.ISSPECIALISED
                }).ToList();

            var customerIds = facilities.Select(x => x.customerId).ToList();

            var duplications = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => customerIds.Contains(x.CUSTOMERID)
               // && x.DELETED == false
                && x.LOANAPPLICATIONID != applicationId
                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
            )
            .Join(
                context.TBL_LMSR_APPLICATION.Where(x =>
                    x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                    , d => d.LOANAPPLICATIONID, a => a.LOANAPPLICATIONID, (d, a) => new { d, a })
            .Select(x => new DedupeApplicationViewModel
            {
                applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                applicationDate = x.a.APPLICATIONDATE,
                applicationAmount = x.d.APPROVEDAMOUNT,
                interestRate = x.d.PROPOSEDINTERESTRATE,
                applicationTenor = x.d.PROPOSEDTENOR,
                branchName = x.a.TBL_BRANCH.BRANCHNAME,
                productName = x.d.TBL_PRODUCT.PRODUCTNAME,
            })
            .ToList();

            details.duplications = duplications;
            details.facilities = facilities;

            return details;
        }

        private void ValidateCustomerExposure(int scenerio, int applicationId, decimal amount, int? customerId, int? customerGroupId)
        {
            var overrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.CustomerExposureLimitOverride && x.ISUSED == false),
                    c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                .FirstOrDefault();

            if (overrideRequest != null)
            {
                if (contextControl == null) contextControl = new FinTrakBankingContext();
                var request = contextControl.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                request.ISUSED = true;
                contextControl.Entry(request).State = EntityState.Modified;
                // contextControl.SaveChanges();
                return;
            }

            var result = limitValidation.ValidateApplicationCustomerRating(new ObligorLimitViewModel
            {
                scenerio = scenerio,
                applicationId = applicationId,
                customerId = customerId,
                customerGroupId = customerGroupId
            });

            if (((result.limit == 0) || ((double)amount + result.outstandingBalance) <= result.outstandingBalance) == false)
                throw new SecureException("Customer limit validation failed!");
        }
        private void SendEmailToCustomerForLoanApproval(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationId)
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                        where a.COMPANYID == companyId && a.DELETED == false 
                        //&& a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                        && a.LOANAPPLICATIONID == loanApplicationId
                        select new LoanApplicationDetailViewModel
                        {
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            email = c.EMAILADDRESS,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            approvalStatusId = a.APPROVALSTATUSID
                        }).Distinct().ToList();


            

            foreach(var customer in data)
            {
               
                string referenceNo = customer.applicationReferenceNumber;
                var successEmailBody = "Dear Valuable Customer, <br /><br /> Your facility application with Reference Number : " + referenceNo + " has been approved,<br /> Kindly contact your Relationship Manager and collect your Offer Letter.";
                string messageSubject = "APPROVAL FOR LOAN APPLICATION";

                emailLogger.ComposeEmail(referenceNo,successEmailBody, messageSubject,customer.email,false);

            }
                
        }
        private void SendEmailToCustomerForLoanDisapproval(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationId)
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                        where a.COMPANYID == companyId && a.DELETED == false
                        && a.LOANAPPLICATIONID == loanApplicationId
                        select new LoanApplicationDetailViewModel
                        {
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            email = c.EMAILADDRESS,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            approvalStatusId = a.APPROVALSTATUSID
                        }).Distinct().ToList();

            foreach (var customer in data)
            {
                string referenceNo = customer.applicationReferenceNumber;

                var failedEmailBody = "Dear Valuable Customer, <br /><br /> Your facility application with Reference Number : " + referenceNo + " has been disapproved,<br /> Kindly contact your Relationship Manager and collect your Offer Letter.";
                string messageSubject = "DISAPPROVAL FOR LOAN APPLICATION";

                emailLogger.ComposeEmail(referenceNo, failedEmailBody, messageSubject, customer.email,false);

            }             
        }

        private string GetFacilityInformationMarkup(int applicationId)
        {
            var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x
                    => x.LOANAPPLICATIONID == applicationId
                    && x.DELETED == false 
                    && x.STATUSID != (int)ApprovalStatusEnum.Disapproved
                ).ToList();

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Facility Type</th>
                        <th>Amount</th>
                        <th>Rate</th>
                        <th>Tenor</th>
                    </tr>
                 ";
            foreach (var f in facilities)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{f.TBL_PRODUCT1.PRODUCTNAME}</td>
                        <td>{f.APPROVEDAMOUNT}</td>
                        <td>{f.APPROVEDINTERESTRATE}</td>
                        <td>{f.APPROVEDTENOR}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;
        }


        public WorkflowResponse GetWorkflowNextStatus(ForwardViewModel model)
        {
            int operationId = (int)OperationsEnum.CAM;
            var applicationDate = general.GetApplicationDate();
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);

            workflow.StaffId = model.createdBy;
            workflow.OperationId = operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
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
            workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return workflow.Response;
        }


    }


}