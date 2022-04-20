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
using System.Net.Http;
using System.Net.Http.Headers;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System.Collections;
using FintrakBanking.ViewModels.Flexcube;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using System.Net;
using System.Web.Script.Serialization;
using System.Text;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.ThridPartyIntegration;

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
        private ILoanApplicationRepository loanApp;
        private IMemorandumRepository memo;
        private TransactionPosting transaction;
        FinTrakBankingStagingContext stgContext;
        IHeadOfficeToSubIntegration headOfficeToSub;

        public AppraisalMemorandumRepository(
            FinTrakBankingContext context, 
            IGeneralSetupRepository general, 
            IAuditTrailRepository audit, 
            IWorkflow workflow,
            ICreditLimitValidationsRepository limitValidation,
            IEmailAlertLogger _emailLogger,
            IOfferLetterAndAvailmentRepository _offerLetter,
            ILoanApplicationRepository _loanApp,
            IMemorandumRepository _memo,
            TransactionPosting _transaction,
            FinTrakBankingStagingContext _stgContext,
            IHeadOfficeToSubIntegration _headOfficeToSub
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.limitValidation = limitValidation;
            this.emailLogger = _emailLogger;
            this.offerLetter = _offerLetter;
            this.loanApp = _loanApp;
            this.memo = _memo;
            this.transaction = _transaction;
            this.stgContext = _stgContext;
            this.headOfficeToSub = _headOfficeToSub;
        }

        public AppraisalMemorandumViewModel GetAppraisalMemorandum(int applicationId, int staffId)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);

            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE where flow.FLOWCHANGEID == appl.FLOWCHANGEID
                                             select flow.OPERATIONID).ToList();
            if(ExclusiveOperations.Count == 0)
            {
                ExclusiveOperations.Add((int)OperationsEnum.CreditAppraisal);
            }
            
            var groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                ExclusiveOperations.Contains(x.OPERATIONID) // == (int)OperationsEnum.CreditAppraisal
                && x.PRODUCTCLASSID == appl.PRODUCTCLASSID
            // && x.ProductId == appl.ProductId // ---- REFACTOR when we have appl.PRODUCTID!!!
            );

            if (groupMappings.Any() == false) // -----  MAY BECOME REDUNDANT!
            {
                groupMappings = context.TBL_APPROVAL_GROUP_MAPPING.Where(x =>
                    ExclusiveOperations.Contains(x.OPERATIONID) // == (int)OperationsEnum.CreditAppraisal
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

            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                                             where flow.FLOWCHANGEID == appl.FLOWCHANGEID
                                             select flow.OPERATIONID).ToList();
            
            if(ExclusiveOperations.Count == 0)
            {
                ExclusiveOperations.Add((int)OperationsEnum.CreditAppraisal);
            }
            
            int approvalLevelId = GetFirstApprovalLevelId(model.createdBy, ExclusiveOperations.FirstOrDefault(), appl.PRODUCTCLASSID, null);

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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),  //model.userIPAddress,
                 URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                    x.OPERATIONID == (int)OperationsEnum.CreditAppraisal
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(), //model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()


            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public WorkflowResponse ForwardAppraisalMemorandum(ForwardViewModel model)
        {
                //Task.Run(() => CreateOutPutDocument(model.applicationId));

                if (model.isExternalSystemApprover)
                {
                    var response = headOfficeToSub.PostFacilityApprovalToSubnputs(model);
                    if(response != null)
                    {
                    var update =  context.TBL_SUB_BASICTRANSACTION.Where(x => x.LOANAPPLICATIONID == model.applicationId && x.APPROVALLEVELID == model.nextApprovalLevelId && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved).FirstOrDefault();
                    update.APPROVALSTATUSID = model.applicationStatusId;
                    stgContext.SaveChanges();
                    }
                }

                bool updateApprovedAmount = false;
                bool generateOutPutDocument = false;
                int operationId = (int)OperationsEnum.CreditAppraisal;
                var applicationDate = general.GetApplicationDate();
                List<TBL_LOAN_APPLICATION_DETAIL> items = null;
                var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
                // LoadConditionsAndDynamics(appl.LOANAPPLICATIONID);
                var staff = context.TBL_STAFF.Where(x => x.STAFFID == model.staffId).FirstOrDefault();

                // VALIDATION TODO if (model.recommendedChanges.Count() > 0)
                items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();
                var approvedList = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).ToList();

                decimal totalApprovedAmount = approvedList.Sum(x => x.APPROVEDAMOUNT * (decimal)x.EXCHANGERATE);
                //decimal totalApplicationAmount = appl.TBL_LOAN_APPLICATION_DETAIL.Sum(a => a.PROPOSEDAMOUNT * (decimal)a.EXCHANGERATE) + (loanApp.GetExposures(appl).Sum(e => e.outstandingsLcy));
                if (!(model.legalLendingLimit > 0))
                {
                    throw new SecureException("Please Kindly refresh your browser and try again, Thanks");
                }
                decimal totalApplicationAmount = model.legalLendingLimit;
                if (appl.TOTALEXPOSUREAMOUNT <= 0)
                {
                    appl.TOTALEXPOSUREAMOUNT = totalApplicationAmount;
                }
                //decimal totalApplicationAmount = items.Sum(x => x.APPROVEDAMOUNT * (decimal)x.EXCHANGERATE);
                using (var trans = context.Database.BeginTransaction())
                {
                    if (appl.RISKRATINGID != null && model.isBusiness == false)
                    {
                        ValidateCustomerExposure(1, appl.LOANAPPLICATIONID, totalApprovedAmount, appl.CUSTOMERID, appl.CUSTOMERGROUPID);
                    }


                    if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                    {
                        //model.isFlowTest = false;

                        var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                            x.OPERATIONID == (int)appl.OPERATIONID
                            && x.RESPONSESTAFFID == null
                            && x.DESTINATIONOPERATIONID > 0
                            && x.TARGETID == appl.LOANAPPLICATIONID
                        );
                        if (currentTrail != null)
                        {

                            currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                            currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
                            currentTrail.COMMENT = model.comment;
                            currentTrail.RESPONSESTAFFID = model.createdBy;
                            currentTrail.RESPONSEDATE = DateTime.Now;

                            appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
                            appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            //workflow.SetResponse = false;
                            //workflow.ExternalInitialization = true;
                            //workflow.StaffId = model.staffId;

                            //workflow.NextLevelId = currentTrail.FROMAPPROVALLEVELID;

                            workflow.StaffId = model.staffId;
                            workflow.OperationId = (short)currentTrail.DESTINATIONOPERATIONID;
                            workflow.TargetId = model.applicationId;
                            workflow.CompanyId = model.companyId;
                            workflow.Comment = model.comment;
                            workflow.ExternalInitialization = true;
                            workflow.ToStaffId = currentTrail.REQUESTSTAFFID;
                            workflow.IsFlowTest = model.isFlowTest;
                            workflow.StatusId = (short)ApprovalStatusEnum.Pending;
                            workflow.Amount = appl.TOTALEXPOSUREAMOUNT;   //model.legalLendingLimit;
                            workflow.FacilityAmount = appl.APPLICATIONAMOUNT;
                            workflow.BusinessUnitId = appl.TBL_CUSTOMER?.BUSINESSUNTID;
                            workflow.LogActivity();

                            //workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.OfferLetterApproval, null, model.applicationId, null, "New approved application", true, false, false, model.isFlowTest);
                            context.SaveChanges();
                            if (model.isFlowTest == false) { trans.Commit(); } else { trans.Rollback(); }
                            return workflow.Response;
                        }


                    }
                    // WORKFLOW
                    workflow.OperationId = appl.OPERATIONID;
                    workflow.ProductClassId = appl.PRODUCTCLASSID;
                    workflow.ProductId = appl.PRODUCTID;
                    workflow.StaffId = model.createdBy;
                    workflow.TargetId = model.applicationId;
                    workflow.CompanyId = model.companyId;
                    workflow.Vote = model.vote;
                    workflow.NextLevelId = model.receiverLevelId;
                    workflow.ToStaffId = model.receiverStaffId;
                    workflow.StatusId = model.forwardAction;
                    workflow.Comment = model.comment;
                    workflow.Amount = appl.TOTALEXPOSUREAMOUNT;
                    workflow.FacilityAmount = appl.APPLICATIONAMOUNT;
                    workflow.InvestmentGrade = model.investmentGrade;
                    workflow.PoliticallyExposed = model.politicallyExposed;
                    workflow.Untenored = model.untenored;
                    workflow.InterestRateConcession = model.interestRateConcession;
                    workflow.FeeRateConcession = model.feeRateConcession;
                    workflow.FinalLevel = appl.FINALAPPROVAL_LEVELID;
                    workflow.ExclusiveFlowChangeId = appl.FLOWCHANGEID;
                    workflow.BusinessUnitId = appl.TBL_CUSTOMER?.BUSINESSUNTID;
                    workflow.IsFromPc = model.isFromPc;
                    workflow.IsFlowTest = model.isFlowTest;
                    workflow.OwnerId = appl.OWNEDBY;
                    workflow.SkipLimitsCheck = appl.TBL_LOAN_APPLICATION_DETAIL.Any(a => a.TBL_CUSTOMER.ISREALATEDPARTY == true);
                    var details = appl.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.DELETED == false
                                                 && d.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID == (int)ProductClassProcessEnum.CAMBased
                                                 && d.TBL_LOAN_APPLICATION.FLOWCHANGEID != (int)FlowChangeEnum.CASHCOLLATERIZED
                                                 && d.TBL_LOAN_APPLICATION.ISADHOCAPPLICATION == false
                                                 && d.TBL_CUSTOMER.CUSTOMERTYPEID != (int)CustomerTypeEnum.Individual).ToList();
                    workflow.LevelBusinessRule = new LevelBusinessRule
                    {
                        Amount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                        PepAmount = appl.TOTALEXPOSUREAMOUNT, // totalApplicationAmount,
                                                              //Pep = model.politicallyExposed,
                        Pep = appl.TBL_LOAN_APPLICATION_DETAIL.Any(a => a.TBL_CUSTOMER.ISPOLITICALLYEXPOSED == true),
                        InsiderRelated = appl.TBL_LOAN_APPLICATION_DETAIL.Any(a => a.TBL_CUSTOMER.ISREALATEDPARTY == true),
                        ProjectRelated = appl.ISPROJECTRELATED,
                        OnLending = appl.ISONLENDING,
                        InterventionFunds = appl.ISINTERVENTIONFUNDS,
                        isAgricRelated = appl.ISAGRICRELATED,
                        isRenewal = appl.TBL_LOAN_APPLICATION_DETAIL.Any(d => d.LOANDETAILREVIEWTYPEID == (short)LoanDetailReviewTypeEnum.Renewal || d.LOANDETAILREVIEWTYPEID == (short)LoanDetailReviewTypeEnum.RenewalWithDecrease),
                        OrrBasedApproval = appl.ISORRBASEDAPPROVAL,
                        DomiciliationNotInPlace = appl.DOMICILIATIONNOTINPLACE,
                        //esrm = appl.TBL_LOAN_APPLICATION_DETAIL.Any(d => d.TBL_CUSTOMER.CUSTOMERTYPEID != (int)CustomerTypeEnum.Individual),
                        esrm = details.Any(),
                        isContingentFacility = appl.TBL_LOAN_APPLICATION_DETAIL.Any(d => d.TBL_PRODUCT.PRODUCTTYPEID == (short)LoanProductTypeEnum.ContingentLiability)
                    };

                    if (model.forwardAction == 8 || model.forwardAction == 9)
                    {
                        //workflow.StatusId = (int)ApprovalStatusEnum.Referred;
                        var dictionary = GetRepresentStepdownItems(model.applicationId, model.forwardAction, operationId);
                        workflow.NextLevelId = dictionary["levelId"];
                        //workflow.ToStaffId = dictionary["staffId"];
                        //if (model.forwardAction == 8) workflow.ToStaffId = null;
                    }

                    string facilityInformationMarkup = GetFacilityInformationMarkup(appl.LOANAPPLICATIONID);

                    var placeholders = new AlertPlaceholders();
                    if (appl.CUSTOMERGROUPID == null)
                    {
                        var c = appl.TBL_CUSTOMER;
                        placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c?.MIDDLENAME + " " + c.LASTNAME;
                    }
                    else
                    {
                        placeholders.customerName = "<br />CUSTOMER NAME: " + appl.TBL_CUSTOMER_GROUP.GROUPNAME;
                    }
                    placeholders.referenceNumber = "<br />APPLICATION REFERENCENUMBER: " + appl.APPLICATIONREFERENCENUMBER;
                    placeholders.facilityType = "<br />FACILITY INFORMATION: " + facilityInformationMarkup;
                    placeholders.operationName = "<br />OPERATION NAME: Loan Origination";
                    placeholders.branchName = "<br />BRANCH NAME: " + appl.TBL_BRANCH.BRANCHNAME;
                    workflow.Placeholders = placeholders;

                    //if (appl.PRODUCTID == 2)
                    //{
                    //    //workflow.ProductClassId = null;
                    //    workflow.ProductId = null;
                    //}
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

                                if (model.isBusiness && model.forwardAction != (int)ApprovalStatusEnum.Referred) // DELETE OR UPDATE PROPOSED
                                {
                                    if (detail.STATUSID == (int)ApprovalStatusEnum.Disapproved) { detail.DELETED = true; }
                                    else
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
                    appl.APPROVALSTATUSID = (short)workflow.StatusId;
                    if (model.vote == 1) { appl.DISPUTED = true; }
                    appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
                    if (appl.SUBMITTEDFORAPPRAISAL == false) { appl.SUBMITTEDFORAPPRAISAL = true; } // for product programs
                    if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

                    ///////////////////// Call Refer Back API /////////////////////
                    if (workflow.StatusId == (short)ApprovalStatusEnum.Referred)
                    {
                        if (model.isFlowTest == false) ReferBackThroughAPI(appl, model, staff.STAFFROLEID);
                    }
                    ///////////////////// Call Refer Back API /////////////////////


                    ////////////////////// Call Status Change API /////////////////
                    if (workflow.StatusId == (short)ApprovalStatusEnum.Processing || workflow.StatusId == (short)ApprovalStatusEnum.Approved || workflow.StatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var statusCode = ""; // Approved = "90", Rejected = "99"
                        statusCode = workflow.StatusId == (short)ApprovalStatusEnum.Disapproved ? "99" : "90";
                        if (model.isFlowTest == false) LoanStatusChangeThroughAPI(appl, model.comment, staff.STAFFID, statusCode);
                    }
                    ////////////////////// Call Status Change API /////////////////


                    if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                    {
                        appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            appl.APPROVEDDATE = applicationDate;
                            appl.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;

                            if (appl.PRODUCTCLASSID == (short)ProductClassEnum.Creditcards)
                            {
                                appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                                appl.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                            }

                            //Send Email to Customer
                            SendEmailToCustomerForLoanApproval(model.applicationId, model.companyId);
                            SaveApprovedDocumentation(model.createdBy, 6, model.applicationId);
                            //generate offer letter doc
                            //offerLetter.AddOfferLetterClauses(model.applicationId, model.staffId,false,false);

                            generateOutPutDocument = true;
                        }
                        else if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                        {
                            SendEmailToCustomerForLoanDisapproval(model.applicationId, model.companyId);
                            loanApp.ArchiveLoanApplication(model.applicationId, operationId, (short)LoanApplicationStatusEnum.ApplicationRejected, model.createdBy);

                        }

                        if (model.forwardAction == (int)ApprovalStatusEnum.Disapproved) { appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected; }
                        if (appl.NEXTAPPLICATIONSTATUSID != null && appl.FINALAPPROVAL_LEVELID != null) { appl.APPLICATIONSTATUSID = (short)appl.NEXTAPPLICATIONSTATUSID; } // may be redundant!!!
                                                                                                                                                                            // MEMORANDUM update
                        var memo = this.context.TBL_CREDIT_APPRAISAL_MEMORANDM.Find(model.appraisalMemorandumId);
                        if (memo != null) { memo.ISCOMPLETED = true; }
                        if (contextControl != null)
                        {
                            contextControl.SaveChanges();

                            if (model.isFlowTest == true) 
                            { 
                                trans.Rollback();
                            }
                        }
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

                        IPADDRESS = CommonHelpers.GetLocalIpAddress(), //model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = applicationDate,
                        SYSTEMDATETIME = DateTime.Now,
                        OSNAME = CommonHelpers.FriendlyName(),
                        DEVICENAME = CommonHelpers.GetDeviceName()

                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------

                    if (model.comment == "debug_test") throw new SecureException("debug_test => FFW:" + model.forwardAction + ", APR:" + workflow.StatusId + ", APL:" + appl.APPLICATIONSTATUSID + ", CHG:" + model.recommendedChanges.Count() + ", STE:" + workflow.NewState + ", AMO:" + appl.APPROVEDAMOUNT + ", upd:" + updateApprovedAmount + ", EXP:" + appl.TOTALEXPOSUREAMOUNT);

                    LogApplicationDetailChanges(appl.LOANAPPLICATIONID, model.createdBy, applicationDate, model.vote, (short)model.forwardAction); // LOG CHANGES

                    context.SaveChanges();

                    ///ResolveBusinessUnitForED(appl);


                    var lastStatus = workflow.StatusId; // prevents the next

                    if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId != (int)ApprovalStatusEnum.Disapproved && appl.PRODUCTCLASSID != (short)ProductClassEnum.Creditcards)
                    {
                        appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
                        workflow.SetResponse = false;
                    //workflow.ProductClassId = null;
                    //workflow.ProductId = null;
                    workflow.ExclusiveFlowChangeId = null;
                        var productId = appl.PRODUCTID != null ? appl.PRODUCTID : appl.TBL_LOAN_APPLICATION_DETAIL.First().APPROVEDPRODUCTID;
                    //The null passed in place of appl.FlowchangeId should be made generic 07/08/2021 after enum.offerletappr.
                        workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.OfferLetterApproval, null, 
                            model.applicationId, appl.PRODUCTCLASSID, "New approved application", true, false, false, 
                            model.isFlowTest, appl.TBL_CUSTOMER?.BUSINESSUNTID, null, 0, productId);
                    //worked on by ifeanyi and zino on 23/06/2021 for account officer offer letter (productId was added)
                    }

                if (model.isFlowTest == false) { trans.Commit(); }
                else { trans.Rollback(); }

                //workflow.Response.success = true;
                workflow.Response.isFinal = generateOutPutDocument;
                    return workflow.Response;
                }
                //decimal totalApprovedAmount = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).Sum(x => x.APPROVEDAMOUNT);
             
        }

        private void ResolveBusinessUnitForED(TBL_LOAN_APPLICATION appl)
        {
            var edRoles = context.TBL_STAFF_ROLE.Where(d => d.STAFFROLESHORTCODE == "ED").Select(x => x.STAFFROLEID).ToList();
            var edLevels = context.TBL_APPROVAL_LEVEL.Where(x => edRoles.Contains(x.STAFFROLEID.Value)).Select(c => c.APPROVALLEVELID).ToList();
            if (edLevels.Contains(workflow.NextLevelId.Value))
            {
                var trailLog = context.TBL_APPROVAL_TRAIL.Where(x => x.APPROVALTRAILID == workflow.ApprovalTrail.APPROVALTRAILID).FirstOrDefault();
                if (trailLog != null)
                {
                    var allEDRecord = context.TBL_STAFF.Where(x => x.BUSINESSUNITID == appl.TBL_CUSTOMER.BUSINESSUNTID && x.STAFFROLEID == edRoles.FirstOrDefault());
                    trailLog.TOSTAFFID = allEDRecord.FirstOrDefault()?.STAFFID;
                    context.SaveChanges();
                }

            }
        }
        public void LoanStatusChangeThroughAPI(TBL_LOAN_APPLICATION loanApplication, string comment, int staffId, string statusCode)
        {
            //string cflReport = builder.ToString();
            string WorkflowStageName = "";
            var staff = context.TBL_STAFF.Where(s => s.STAFFID == staffId).FirstOrDefault();
            var WorkflowStage = context.TBL_STAFF_ROLE.Where(s => s.STAFFROLEID == staff.STAFFROLEID).Select(s => s.STAFFROLECODE).FirstOrDefault();

            if (WorkflowStage == "RM")
            {
                WorkflowStageName = "11";
            }
            //if (WorkflowStage.Substring(0, 2) == "CR")
            if (WorkflowStage == "CA")
            {
                WorkflowStageName = "12";
            }
            if (WorkflowStage == "GH")
            {
                WorkflowStageName = "13";
            }

            var staffFullName = staff.FIRSTNAME + " " + staff.LASTNAME;

            OfferLetterResponse offerLetters = new OfferLetterResponse();
            offerLetters.StatusCode = statusCode;
            offerLetters.Comment = comment;
            offerLetters.RequestId = loanApplication.APIREQUESTID;
            offerLetters.WorkflowStage = WorkflowStageName;
            //offerLetters.Attachment.FileLink = cflReport;
            //offerLetters.Attachment.FileType = "pdf";
            //offerLetters.ReasonForRejection = ReasonForRejection;
            offerLetters.ActionByName = staffFullName;

            if (WorkflowStageName != "" && loanApplication.APIREQUESTID != null) {
               transaction.ApiOfferLetterPosting(offerLetters, loanApplication.APPLICATIONREFERENCENUMBER);
            }

        }

        private void ReferBackThroughAPI(TBL_LOAN_APPLICATION loanApplication, ForwardViewModel model, int staffRoleId)
        {
            if (loanApplication != null && loanApplication.APIREQUESTID != null)
            {
                string WorkflowStageName = "";
                var WorkflowStage = context.TBL_STAFF_ROLE.Where(s => s.STAFFROLEID == staffRoleId).Select(s => s.STAFFROLECODE).FirstOrDefault();

                if (WorkflowStage == "RM")
                {
                    WorkflowStageName = "11";
                }
                //if (WorkflowStage.Substring(0, 2) == "CR")
                if (WorkflowStage == "CA")
                {
                    WorkflowStageName = "12";
                }
                if (WorkflowStage == "GH")
                {
                    WorkflowStageName = "13";
                }

                var product = context.TBL_PRODUCT.Find(loanApplication.PRODUCTID);

                if (product?.PRODUCTCODE == "EBFC")
                {
                    OfferLetterResponse offerLetters = new OfferLetterResponse();
                    var staffDetail = context.TBL_STAFF.Where(s => s.STAFFID == model.createdBy).FirstOrDefault();
                    var staffFullName = staffDetail.FIRSTNAME + " " + staffDetail.LASTNAME;
                    offerLetters.Comment = model.comment;
                    offerLetters.RequestId = loanApplication.APIREQUESTID;
                    offerLetters.WorkflowStage = WorkflowStageName;
                    offerLetters.ActionByName = staffFullName;

                    if (WorkflowStageName != "" && loanApplication.APIREQUESTID != null) {
                        transaction.ReferBackThroughAPI(offerLetters, loanApplication.APPLICATIONREFERENCENUMBER);
                    }
                }
            }
        }

        public bool SaveApprovedDocumentation(int staffId, int operationId, int targetId)
        {
            // int staffId, is REDUNDANT! 
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();

            memo.Init(operationId, targetId); //content = memo.Replace(content);
            foreach (var raw in rawSections)
            {
                raw.templateDocument = memo.Replace(raw.templateDocument);
                replacedSections.Add(raw);
                printedDoc = raw.title;
            }

            var docsToSave = new List<TBL_DOC_TEMPLATE_SAVED>();
            foreach (var section in replacedSections)
            {
                TBL_DOC_TEMPLATE_SAVED docToSave = new TBL_DOC_TEMPLATE_SAVED();
                var sect = context.TBL_DOC_TEMPLATE_DETAIL.Find(section.sectionId);
                docToSave.OPERATIONID = sect.OPERATIONID;
                docToSave.DOCUMENTDETAILID = sect.DOCUMENTDETAILID;
                docToSave.TARGETID = sect.TARGETID;
                docToSave.TEMPLATESECTIONID = sect.TEMPLATESECTIONID;
                docToSave.TITLE = sect.TITLE;
                docToSave.DESCRIPTION = sect.DESCRIPTION;
                docToSave.TEMPLATEDOCUMENT = section.templateDocument;
                docToSave.POSITION = sect.POSITION;
                docToSave.CANEDIT = sect.CANEDIT;
                docToSave.CREATEDBY = sect.CREATEDBY;
                docToSave.DATETIMECREATED = DateTime.Now;
                docsToSave.Add(docToSave);
            }

            context.TBL_DOC_TEMPLATE_SAVED.AddRange(docsToSave);

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
            //    STAFFID = staffId,
            //    BRANCHID = 1, //(short)model.userBranchId,
            //    DETAIL = $"Printed Document Template '{ printedDoc }' ",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = "localhost",//model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName()
            //};
            //this.audit.AddAuditTrail(audit);
            return context.SaveChanges() > 0;
        }

        public IQueryable<LoanApplicationViewModel> GetPendingAdhocApplications(int operationId, int companyId, int branchId, int staffId, int? classId)
        {
            // var declarations
            IQueryable<LoanApplicationViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var querytest1 = (from a in context.TBL_LOAN_APPLICATION
                             where
                                a.DELETED == false && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && a.COMPANYID == companyId
                                && (classId == null) ? true : (a.PRODUCTCLASSID == (short?)classId)
                                && a.ISADHOCAPPLICATION == true select a
                            ).ToList();

            var querytest2 = (from b in context.TBL_APPROVAL_TRAIL where
                     
                                 b.OPERATIONID == (int)OperationsEnum.AdhocApproval 
                                 && b.APPROVALSTATEID != (int)ApprovalState.Ended
                                 && b.RESPONSESTAFFID == null
                                 && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                                 && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                                 select b
                             ).ToList();
            // query
            var query = (from a in context.TBL_LOAN_APPLICATION where
                            (
                                a.DELETED == false && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && a.COMPANYID == companyId
                                && (classId == null) ? true : (a.PRODUCTCLASSID == (short?)classId)
                                && a.ISADHOCAPPLICATION == true
                            )
                         orderby a.LOANAPPLICATIONID
                         join b in context.TBL_APPROVAL_TRAIL on a.LOANAPPLICATIONID equals b.TARGETID where
                     (
                         b.OPERATIONID == (int)OperationsEnum.AdhocApproval 
                         && b.APPROVALSTATEID != (int)ApprovalState.Ended
                         && b.RESPONSESTAFFID == null
                         && ((levelIds.Contains((int)b.TOAPPROVALLEVELID) && b.LOOPEDSTAFFID == null) || (!levelIds.Contains((int)b.TOAPPROVALLEVELID) && b.LOOPEDSTAFFID == staffId)) //|| (!levelIds.Contains((int)b.TOAPPROVALLEVELID) && b.REQUESTSTAFFID == b.TOSTAFFID))
                         && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                         //&& b.LOOPEDSTAFFID == null
                     )
                         //join c in context.TBL_LOAN_APPLICATION_DETAIL
                         //on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                         select new LoanApplicationViewModel()
                         {
                             //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                             loanApplicationId = a.LOANAPPLICATIONID,
                             //loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                             relatedReferenceNumber = a.RELATEDREFERENCENUMBER,
                             branchId = a.BRANCHID,
                             productClassId = a.PRODUCTCLASSID,
                             productClassName = a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                             //currencyCode = c.TBL_CURRENCY.CURRENCYCODE,
                             loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                             relationshipManagerId = a.RELATIONSHIPMANAGERID,
                             applicationDate = a.APPLICATIONDATE,
                             newApplicationDate = a.DATEACTEDON,
                             //newApplicationDate = x.a.APPLICATIONDATE,
                             applicationAmount = a.APPLICATIONAMOUNT,
                             approvedAmount = a.APPROVEDAMOUNT,
                             interestRate = a.INTERESTRATE,
                             applicationTenor = a.APPLICATIONTENOR,
                             //approvedTenor = c.APPROVEDTENOR,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             approvalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             timeIn = b.SYSTEMARRIVALDATETIME,
                             slaTime = b.SLADATETIME,
                             loanInformation = a.LOANINFORMATION,
                             submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                             isRelatedParty = a.ISRELATEDPARTY,
                             isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             approvalStatusName = b.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME.ToUpper(),
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             branchName = a.TBL_BRANCH.BRANCHNAME,
                             relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                             relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                             misCode = a.MISCODE,
                             loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                             createdBy = a.OWNEDBY,
                             loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                             customerGroupId = a.CUSTOMERGROUPID,
                             customerId = a.CUSTOMERID,
                             customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                             customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                             customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                             customerInfoValidated = a.CUSTOMERINFOVALIDATED,
                             customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                             operationId = a.OPERATIONID,
                             productClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                             tranchLevelId = a.TRANCHEAPPROVAL_LEVELID,
                             isEmployerRelated = a.ISEMPLOYERRELATED,
                             employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == a.RELATEDEMPLOYERID).EMPLOYER_NAME,
                             applicationDetails = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                                   where d.LOANAPPLICATIONID == a.LOANAPPLICATIONID
                                                   select new LoanApplicationDetailViewModel()
                                                   {
                                                       loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                                       proposedTenor = d.APPROVEDTENOR,
                                                       proposedProductId = d.PROPOSEDPRODUCTID,
                                                       proposedProductName = d.TBL_PRODUCT.PRODUCTNAME,
                                                       proposedAmount = d.PROPOSEDAMOUNT,
                                                       proposedInterestRate = (int)d.PROPOSEDINTERESTRATE,
                                                       customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                                                       exchangeRate = d.EXCHANGERATE,
                                                       currencyName = d.TBL_CURRENCY.CURRENCYNAME,
                                                   }).ToList(),
                                    //globalsla = context.TBL_LOAN_APPLICATION_DETAIL
                                    //                            .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID)
                                    //                            .Select(s => s.TBL_PRODUCT1.TBL_PRODUCT_CLASS.GLOBALSLA)
                                    //                            .FirstOrDefault(),
                                    //currentApprovalLevelSlaInterval = x.b.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
                            dateTimeCreated = a.DATETIMECREATED
                                }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications;
        }

        public WorkflowResponse AdhocAppraisalMemorandum(ForwardViewModel model)
        {
            //   Task.Run(() => CreateOutPutDocument(model.applicationId));

            bool updateApprovedAmount = false;
            bool generateOutPutDocument = false;
            int operationId = (int)OperationsEnum.AdhocApproval; // CHANGE
            var applicationDate = general.GetApplicationDate();
            List<TBL_LOAN_APPLICATION_DETAIL> items = null;
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
            // LoadConditionsAndDynamics(appl.LOANAPPLICATIONID);

            // VALIDATION TODO if (model.recommendedChanges.Count() > 0)
            items = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID && x.DELETED == false).ToList();

            var approvedList = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).ToList();

            decimal totalApprovedAmount = approvedList.Sum(x => x.APPROVEDAMOUNT);
            decimal totalApplicationAmount = items.Sum(x => x.APPROVEDAMOUNT);

            //decimal totalApprovedAmount = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).Sum(x => x.APPROVEDAMOUNT);
            if (appl.RISKRATINGID != null && model.isBusiness == false)
            {
                ValidateCustomerExposure(1, appl.LOANAPPLICATIONID, totalApprovedAmount, appl.CUSTOMERID, appl.CUSTOMERGROUPID);
            }

            // WORKFLOW
            //workflow.ResolveMultipleProductPath(operationId, items.Select(x => (short)x.APPROVEDPRODUCTID).ToList());
            workflow.OperationId = operationId;
            //workflow.ProductClassId = appl.PRODUCTCLASSID;
            //workflow.ProductId = model.productId;
            workflow.BusinessUnitId = appl.TBL_CUSTOMER?.BUSINESSUNTID;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            //workflow.Vote = model.vote;
            //var test4 = model.receiverLevelId;
            var nextLevel = loanApp.GetFirstReceiverLevel(model.createdBy, operationId, appl.PRODUCTCLASSID, null, null, true);
            var nextStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
            workflow.NextLevelId = null;
            //workflow.ToStaffId = nextStaff;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            string facilityInformationMarkup = GetFacilityInformationMarkup(appl.LOANAPPLICATIONID);


            var placeholders = new AlertPlaceholders();
                if (appl.CUSTOMERGROUPID == null)
                {
                    var c = appl.TBL_CUSTOMER;
                    placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                }
                else
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

                // UPDATE APPLICATION
                appl.APPROVALSTATUSID = (short)workflow.StatusId;
                //            if (model.vote == 1) { appl.DISPUTED = true; }
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
                if (appl.SUBMITTEDFORAPPRAISAL == false) { appl.SUBMITTEDFORAPPRAISAL = true; } // for product programs
                if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMCompleted;
                        appl.APPROVEDDATE = applicationDate;
                        appl.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
                        appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                        foreach (var item in items)
                        {
                            item.STATUSID = (short)ApprovalStatusEnum.Approved;
                        }

                        approvedList = items.Where(x => x.STATUSID == (short)ApprovalStatusEnum.Approved).ToList();
                        totalApprovedAmount = approvedList.Sum(x => x.APPROVEDAMOUNT);
                        totalApplicationAmount = items.Sum(x => x.APPROVEDAMOUNT);
                        appl.APPROVEDAMOUNT = totalApprovedAmount;
                        //Send Email to Customer
                        //SendEmailToCustomerForLoanApproval(model.applicationId, model.companyId);

                        //generate offer letter doc
                        //offerLetter.AddOfferLetterClauses(model.applicationId, model.staffId,false,false);

                        generateOutPutDocument = true;
                    }
                    else if (appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                    {
                        loanApp.ArchiveLoanApplication(model.applicationId, operationId, (short)LoanApplicationStatusEnum.ApplicationRejected, model.createdBy);
                        //SendEmailToCustomerForLoanDisapproval(model.applicationId, model.companyId);
                }

                if (model.forwardAction == (int)ApprovalStatusEnum.Disapproved) { appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.ApplicationRejected; }
                    if (appl.NEXTAPPLICATIONSTATUSID != null && appl.FINALAPPROVAL_LEVELID != null) { appl.APPLICATIONSTATUSID = (short)appl.NEXTAPPLICATIONSTATUSID; } // may be redundant!!!
                                                                                                                                                                        // MEMORANDUM update                                                                                                                                               //        if (memo != null) { memo.ISCOMPLETED = true; }
                    if (contextControl != null) contextControl.SaveChanges();
                }

                // UPDATE APPROVED AMOUNT
                if (updateApprovedAmount == true && items != null) appl.APPROVEDAMOUNT = totalApprovedAmount;

                // Audit Section ---------------------------
                /*  var audit = new TBL_AUDIT
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
      */

                //LogApplicationDetailChanges(appl.LOANAPPLICATIONID, model.createdBy, applicationDate,model.vote , (short)model.forwardAction); // LOG CHANGES

                //var lastStatus = workflow.StatusId; // prevents the nex

                if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.AvailmentCompleted;
                    appl.AVAILMENTDATE = DateTime.Now;
                    appl.APPROVEDDATE = DateTime.Now;
                    workflow.SetResponse = false;
                    //workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.IndividualDrawdownRequest,null, model.applicationId, null, "New approved application", true, false, false,model.isFlowTest, appl.TBL_CUSTOMER?.BUSINESSUNTID);
                }
                appl.DATEACTEDON = DateTime.Now;
                context.SaveChanges();
                //workflow.Response.success = true;
                workflow.Response.isFinal = generateOutPutDocument;
                return workflow.Response;
        }

        private bool PopulateLcCashBuildUp(int lcIssuanceId)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(lcIssuanceId);
            if (lc == null)
            {
                new SecureException("LC for cash buildup plan date population cannot be null!");
            }
            var cashBuildUps = context.TBL_LC_CASHBUILDUPPLAN.Where(p => p.LCISSUANCEID == lcIssuanceId && p.DELETED == false).ToList();
            var today = DateTime.Now.Date;
            foreach(var b in cashBuildUps)
            {
                b.PLANDATE = today.AddDays(b.DAYSINTERVAL);
            }
            return context.SaveChanges() > 0;
        }

        public WorkflowResponse LcAppraisalMemorandum(LcForwardViewModel model)
        {
            int operationId = (int)OperationsEnum.lcIssuance; // CHANGE
            var applicationDate = general.GetApplicationDate();
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            lc.OPERATIONID = operationId;
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }

            // WORKFLOW
            //workflow.ResolveMultipleProductPath(operationId, items.Select(x => (short)x.APPROVEDPRODUCTID).ToList()); tr
            using (var trans = context.Database.BeginTransaction())
            {
                workflow.OperationId = operationId;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.LcIssuanceId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = model.vote;
                var nextLevel = loanApp.GetFirstReceiverLevel(model.createdBy, operationId, null, null, null, true);
                //var test = loanApp.GetFirstAdhocReceiverLevel(model.createdBy, operationId, null, false);
                var nextStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
                workflow.NextLevelId = 0; //0
                workflow.ToStaffId = null; //NULL
                workflow.StatusId = model.forwardAction; //1
                workflow.Comment = model.comment;
                var c = context.TBL_CUSTOMER.Find(lc.CUSTOMERID);
                workflow.BusinessUnitId = c?.BUSINESSUNTID;

                var placeholders = new AlertPlaceholders();
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + lc.LCREFERENCENUMBER;
                placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT";
                placeholders.operationName = "<br />OPERATION NAME: LC ISSUANCE";
                placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
                workflow.Placeholders = placeholders;

                workflow.DeferredExecution = true;
                workflow.LogActivity();

                WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

                // UPDATE APPLICATION
                lc.APPROVALSTATUSID = (short)workflow.StatusId;
                //            if (model.vote == 1) { appl.DISPUTED = true; }
                lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceInProgress;
                if (lc.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { lc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {

                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        lc.LCUSSANCESTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceCompleted;
                        lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceCompleted;
                        lc.APPROVEDDATE = DateTime.Now;
                        workflow.SetResponse = false;
                        lc.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
                        lc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        PopulateLcCashBuildUp(lc.LCISSUANCEID);
                    }
                    else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        lc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                        //SendEmailToCustomerForLoanDisapproval(model.LcIssuanceId, model.companyId);
                    }

                    if (contextControl != null) contextControl.SaveChanges();
                }

                // UPDATE APPROVED AMOUNT
                //if (updateApprovedAmount == true && items != null) appl.APPROVEDAMOUNT = totalApprovedAmount;

                lc.DATEACTEDON = DateTime.Now;
                context.SaveChanges();
                //ValidateAllFromReceiverLevels(model.createdBy, operationId);
                trans.Commit();
                return workflow.Response;
            }
        }

        public WorkflowResponse LcReleaseMemorandum(LcForwardViewModel model)
        {
            LcReleaseAmountViewModel release = new LcReleaseAmountViewModel();
                release.lcReleaseAmountId = model.lcReleaseAmountId;
                release.lcIssuanceId = model.LcIssuanceId;
                release.releaseAmount = model.releaseAmount;
                ValidateReleaseAmount(release);
            int operationId = (int)OperationsEnum.lcReleaseOfShippingDocuments; // CHANGE
            //var applicationDate = general.GetApplicationDate();
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            var rl = context.TBL_LCRELEASE_AMOUNT.Find(model.lcReleaseAmountId);
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }

            // WORKFLOW
            //workflow.ResolveMultipleProductPath(operationId, items.Select(x => (short)x.APPROVEDPRODUCTID).ToList());
            workflow.OperationId = operationId;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.lcReleaseAmountId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            if (model.forwardAction == (int)ApprovalStatusEnum.Reroute)
            {
                var nextLevel = loanApp.GetFirstReceiverLevel(model.createdBy, operationId, null, null,null, true);
                var nextLvlStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
                var secondLvl = loanApp.GetFirstReceiverLevel((int)nextLvlStaff, operationId, null, null,null, true);
                workflow.NextLevelId = secondLvl;
                var testStaff = loanApp.GetFirstLevelStaffId((int)secondLvl, model.userBranchId);
                workflow.ToStaffId = testStaff;
                workflow.StatusId = 1;
            }
            else
            {
                var test4 = model.receiverLevelId;
                var test6 = loanApp.GetFirstReceiverLevel(model.createdBy, operationId, null, null,null);
                var test = loanApp.GetFirstAdhocReceiverLevel(model.createdBy, operationId, null, false);
                var test1 = loanApp.GetFirstAdhocReceiverLevel(model.createdBy, operationId, null, true);
                //var nextStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
                workflow.NextLevelId = 0;
                workflow.ToStaffId = null;
                workflow.StatusId = model.forwardAction;
            }
            workflow.Comment = model.comment;
            var c = context.TBL_CUSTOMER.Find(lc.CUSTOMERID);

            workflow.BusinessUnitId = c?.BUSINESSUNTID;

            var placeholders = new AlertPlaceholders();
            placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
            placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + lc.LCREFERENCENUMBER;
            placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT: SHIPPING DOCUMENTS RELEASE";
            placeholders.operationName = "<br />OPERATION NAME: LC SHIPPING DOCUMENTS RELEASE";
            placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
            workflow.Placeholders = placeholders;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // UPDATE APPLICATION
            rl.RELEASEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress;
            rl.RELEASEAPPROVALSTATUSID = (short)workflow.StatusId;
            if ((short)workflow.StatusId == (int)ApprovalStatusEnum.Pending)
            {
                rl.RELEASEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {

                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    rl.RELEASEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted;
                    rl.RELEASEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    workflow.SetResponse = false;
                }
                else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    rl.RELEASEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }
                if (contextControl != null) contextControl.SaveChanges();
            }

            // UPDATE APPROVED AMOUNT
            //if (updateApprovedAmount == true && items != null) appl.APPROVEDAMOUNT = totalApprovedAmount;

            // Audit Section ---------------------------
            /*  var audit = new TBL_AUDIT
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
  */

            context.SaveChanges();
            //ValidateAllFromReceiverLevels(model.createdBy, operationId);
            //workflow.Response.success = true;
            return workflow.Response;
        }

        public WorkflowResponse LcCancelationMemorandum(LcForwardViewModel model)
        {
            int operationId = (int)OperationsEnum.LCTerminationApproval; // CHANGE
            var applicationDate = general.GetApplicationDate();
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            var cancelationInProgress = context.TBL_APPROVAL_TRAIL.Any(t => t.TARGETID == lc.LCISSUANCEID && t.OPERATIONID == (int)OperationsEnum.LCTerminationApproval && t.RESPONSESTAFFID == null && t.APPROVALSTATEID != (int)ApprovalState.Ended);
            if ((lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress && model.isInitiation) || lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted)
            {
                throw new SecureException("LC Issuance Cancelation Approval Already Ongoing or Completed");
            }
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            // WORKFLOW
            using (var trans = context.Database.BeginTransaction())
            {
                workflow.OperationId = operationId;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.LcIssuanceId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = model.vote;
                workflow.ToStaffId = null;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                var c = context.TBL_CUSTOMER.Find(lc.CUSTOMERID);

                workflow.BusinessUnitId = c?.BUSINESSUNTID;
                var placeholders = new AlertPlaceholders();
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + lc.LCREFERENCENUMBER;
                placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT: CANCELATION";
                placeholders.operationName = "<br />OPERATION NAME: LC CANCELATION";
                placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
                workflow.Placeholders = placeholders;

                workflow.DeferredExecution = true;
                workflow.LogActivity();

                WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

                lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationInProgress;

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationCompleted;
                        lc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        workflow.SetResponse = false;
                    }
                    else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        lc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                        if (lc.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted)
                        {
                            lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceCompleted;
                        }
                        else
                        {
                            lc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceInProgress;
                        }
                    }
                }
                context.SaveChanges();
                trans.Commit();
                return workflow.Response;
            }
        }

        public bool AddLcArchive(int LcIssuanceId, int operationId)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(LcIssuanceId);
            if (lc == null)
            {
                throw new SecureException("LC NOT FOUND TO BE ARCHIVED!");
            }
            var newArch = new TBL_LC_ISSUANCE_ARCHIVE()
            {
                LCISSUANCEID = lc.LCISSUANCEID,
                LCREFERENCENUMBER = lc.LCREFERENCENUMBER,
                BENEFICIARYNAME = lc.BENEFICIARYNAME,
                TOTALAPPROVEDAMOUNT = lc.TOTALAPPROVEDAMOUNT,
                LETTEROFCREDITTYPEID = lc.LETTEROFCREDITTYPEID,
                ISDRAFTREQUIRED = lc.ISDRAFTREQUIRED,
                BENEFICIARYADDRESS = lc.BENEFICIARYADDRESS,
                BENEFICIARYEMAIL = lc.BENEFICIARYEMAIL,
                CUSTOMERID = lc.CUSTOMERID,
                FUNDSOURCEID = lc.FUNDSOURCEID,
                FUNDSOURCEDETAILS = lc.FUNDSOURCEDETAILS,
                FORMMNUMBER = lc.FORMMNUMBER,
                BENEFICIARYPHONENUMBER = lc.BENEFICIARYPHONENUMBER,
                BENEFICIARYBANK = lc.BENEFICIARYBANK,
                CURRENCYID = lc.CURRENCYID,
                PROFORMAINVOICEID = lc.PROFORMAINVOICEID,
                AVAILABLEAMOUNT = lc.AVAILABLEAMOUNT,
                LETTEROFCREDITAMOUNT = lc.LETTEROFCREDITAMOUNT,
                LETTEROFCREDITEXPIRYDATE = lc.LETTEROFCREDITEXPIRYDATE,
                INVOICEDATE = lc.INVOICEDATE,
                INVOICEDUEDATE = lc.INVOICEDUEDATE,
                TRANSACTIONCYCLE = lc.TRANSACTIONCYCLE,
                DATETIMECREATED = lc.DATETIMECREATED,
                DATETIMEUPDATED = lc.DATETIMEUPDATED,
                DELETED = lc.DELETED,
                DELETEDBY = lc.DELETEDBY,
                CREATEDBY = lc.CREATEDBY,
                LASTUPDATEDBY = lc.LASTUPDATEDBY,
                DATETIMEDELETED = lc.DATETIMEDELETED,
                APPROVEDBY = lc.APPROVEDBY,
                APPROVED = lc.APPROVED,
                APPROVALSTATUSID = lc.APPROVALSTATUSID,
                LCUSSANCESTATUSID = lc.LCUSSANCESTATUSID,
                LCUSSANCEAPPROVALSTATUSID = lc.LCUSSANCEAPPROVALSTATUSID,
                APPLICATIONSTATUSID = lc.APPLICATIONSTATUSID,
                FINALAPPROVAL_LEVELID = lc.FINALAPPROVAL_LEVELID,
                LCUSSANCEFINALAPPROVAL_LEVELID = lc.LCUSSANCEFINALAPPROVAL_LEVELID,
                LCUSSANCEAPPROVEDDATE = lc.LCUSSANCEAPPROVEDDATE,
                DATEACTEDON = lc.DATEACTEDON,
                ACTEDONBY = lc.ACTEDONBY,
                APPROVEDDATE = lc.APPROVEDDATE,
                TOTALAPPROVEDAMOUNTCURRENCYID = lc.TOTALAPPROVEDAMOUNTCURRENCYID,
                AVAILABLEAMOUNTCURRENCYID = lc.AVAILABLEAMOUNTCURRENCYID,
                CASHBUILDUPAVAILABLE = lc.CASHBUILDUPAVAILABLE,
                CASHBUILDUPREFERENCETYPE = lc.CASHBUILDUPREFERENCENUMBER,
                CASHBUILDUPREFERENCENUMBER = lc.CASHBUILDUPREFERENCENUMBER,
                PERCENTAGETOCOVER = lc.PERCENTAGETOCOVER,
                LCTOLERANCEPERCENTAGE = lc.LCTOLERANCEPERCENTAGE,
                LCTOLERANCEVALUE = lc.LCTOLERANCEVALUE,
                TOTALUSANCEAMOUNTLOCAL = lc.TOTALUSANCEAMOUNTLOCAL,
                RELEASEDAMOUNT = lc.RELEASEDAMOUNT,
                OPERATIONID = lc.OPERATIONID,
                ARCHIVINGOPERATIONID = operationId,
                DATETIMEARCHIVED = DateTime.Now
            };
            context.TBL_LC_ISSUANCE_ARCHIVE.Add(newArch);
            return context.SaveChanges() != 0;
        }

        private bool UpdateLcWithEnhancement(int tempLcIssuanceId)
        {
            var newLc = context.TBL_TEMP_LC_ISSUANCE.Find(tempLcIssuanceId);
            var oldLc = context.TBL_LC_ISSUANCE.Find(newLc.LCISSUANCEID);
            if (newLc == null)
            {
                throw new SecureException("LC Enhancement Data Not Found!");
            }
            if (newLc == null)
            {
                throw new SecureException("LC Issuance Data Not Found!");
            }

            oldLc.BENEFICIARYNAME = newLc.BENEFICIARYNAME;
            oldLc.TOTALAPPROVEDAMOUNT = newLc.TOTALAPPROVEDAMOUNT;
            oldLc.LETTEROFCREDITTYPEID = newLc.LETTEROFCREDITTYPEID;
            oldLc.ISDRAFTREQUIRED = newLc.ISDRAFTREQUIRED;
            oldLc.BENEFICIARYADDRESS = newLc.BENEFICIARYADDRESS;
            oldLc.BENEFICIARYEMAIL = newLc.BENEFICIARYEMAIL;
            oldLc.CUSTOMERID = newLc.CUSTOMERID;
            oldLc.FUNDSOURCEID = newLc.FUNDSOURCEID;
            oldLc.FUNDSOURCEDETAILS = newLc.FUNDSOURCEDETAILS;
            oldLc.FORMMNUMBER = newLc.FORMMNUMBER;
            oldLc.BENEFICIARYPHONENUMBER = newLc.BENEFICIARYPHONENUMBER;
            oldLc.BENEFICIARYBANK = newLc.BENEFICIARYBANK;
            oldLc.CURRENCYID = newLc.CURRENCYID;
            oldLc.PROFORMAINVOICEID = newLc.PROFORMAINVOICEID;
            oldLc.AVAILABLEAMOUNT = newLc.AVAILABLEAMOUNT;
            oldLc.LETTEROFCREDITAMOUNT = newLc.LETTEROFCREDITAMOUNT;
            oldLc.LETTEROFCREDITEXPIRYDATE = newLc.LETTEROFCREDITEXPIRYDATE;
            oldLc.INVOICEDATE = newLc.INVOICEDATE;
            oldLc.INVOICEDUEDATE = newLc.INVOICEDUEDATE;
            oldLc.DATETIMEUPDATED = DateTime.Now;
            oldLc.LASTUPDATEDBY = newLc.LASTUPDATEDBY;
            oldLc.TOTALAPPROVEDAMOUNTCURRENCYID = newLc.TOTALAPPROVEDAMOUNTCURRENCYID;
            oldLc.AVAILABLEAMOUNTCURRENCYID = newLc.AVAILABLEAMOUNTCURRENCYID;
            oldLc.CASHBUILDUPAVAILABLE = newLc.CASHBUILDUPAVAILABLE;
            oldLc.CASHBUILDUPREFERENCETYPE = newLc.CASHBUILDUPREFERENCENUMBER;
            oldLc.CASHBUILDUPREFERENCENUMBER = newLc.CASHBUILDUPREFERENCENUMBER;
            oldLc.PERCENTAGETOCOVER = newLc.PERCENTAGETOCOVER;
            oldLc.LCTOLERANCEPERCENTAGE = newLc.LCTOLERANCEPERCENTAGE;
            oldLc.LCTOLERANCEVALUE = newLc.LCTOLERANCEVALUE;
            oldLc.RELEASEDAMOUNT = newLc.RELEASEDAMOUNT;
            return context.SaveChanges() != 0;
        }


        public WorkflowResponse LcEnhancementMemorandum(LcForwardViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            if (lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress || lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress)
            {
                throw new SecureException("This LC is already undergoing Termination Or has been Terminated");
            }
            int operationId = (int)OperationsEnum.LCEnhancementApproval; // CHANGE
            var applicationDate = general.GetApplicationDate();
            var tempLc = context.TBL_TEMP_LC_ISSUANCE.Find(model.tempLcIssuanceId);
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            // WORKFLOW
            using (var trans = context.Database.BeginTransaction())
            {
                workflow.OperationId = operationId;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.tempLcIssuanceId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = model.vote;
                workflow.ToStaffId = null;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                var c = context.TBL_CUSTOMER.Find(tempLc.CUSTOMERID);

                workflow.BusinessUnitId = c?.BUSINESSUNTID;
                var placeholders = new AlertPlaceholders();
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + tempLc.LCREFERENCENUMBER;
                placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT";
                placeholders.operationName = "<br />OPERATION NAME: LC AMOUNT MODIFICATION";
                placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
                workflow.Placeholders = placeholders;

                workflow.DeferredExecution = true;
                workflow.LogActivity();

                WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

                tempLc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcEnhancementInProgress;
                tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        tempLc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcEnhancementCompleted;
                        tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        workflow.SetResponse = false;
                        var archived = AddLcArchive(tempLc.LCISSUANCEID, operationId);
                        if (archived)
                        {
                            UpdateLcWithEnhancement(tempLc.TEMPLCISSUANCEID);
                        }
                        else
                        {
                            throw new SecureException("There was an Error Archiving this LC!");
                        }
                    }
                    else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    }
                }
                context.SaveChanges();
                trans.Commit();
                return workflow.Response;
            }
        }

        public WorkflowResponse LcIssuanceExtensionMemorandum(LcForwardViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            if (lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress || lc.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress)
            {
                throw new SecureException("This LC is already undergoing Termination Or has been Terminated");
            }
            int operationId = (int)OperationsEnum.LCIssuanceExtensionApproval; // CHANGE
            var applicationDate = general.GetApplicationDate();
            var tempLc = context.TBL_TEMP_LC_ISSUANCE.Find(model.tempLcIssuanceId);
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            // WORKFLOW
            using (var trans = context.Database.BeginTransaction())
            {
                workflow.OperationId = operationId;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.tempLcIssuanceId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = model.vote;
                workflow.ToStaffId = null;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                var c = context.TBL_CUSTOMER.Find(tempLc.CUSTOMERID);

                workflow.BusinessUnitId = c?.BUSINESSUNTID;
                var placeholders = new AlertPlaceholders();
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + tempLc.LCREFERENCENUMBER;
                placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT";
                placeholders.operationName = "<br />OPERATION NAME: LC AMOUNT MODIFICATION";
                placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
                workflow.Placeholders = placeholders;

                workflow.DeferredExecution = true;
                workflow.LogActivity();

                WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

                tempLc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceExtensionInProgress;
                tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        tempLc.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcIssuanceExtensionCompleted;
                        tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        workflow.SetResponse = false;
                        var archived = AddLcArchive(tempLc.LCISSUANCEID, operationId);
                        if (archived)
                        {
                            UpdateLcWithEnhancement(tempLc.TEMPLCISSUANCEID);
                        }
                        else
                        {
                            trans.Rollback();
                            throw new SecureException("There was an Error Archiving this LC Issuance!");
                        }
                    }
                    else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        tempLc.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    }
                }
                context.SaveChanges();
                trans.Commit();
                return workflow.Response;
            }
        }

        private bool ArchiveLCUsance(int lcUssanceId, int operationId)
        {
            var lcUsance = context.TBL_LC_USSANCE.Find(lcUssanceId);
            if (lcUsance == null)
            {
                throw new ConditionNotMetException("Lc Usance record not found!");
            }
            var usanceForArchive = new TBL_LC_USSANCE_ARCHIVE();
            usanceForArchive.LCISSUANCEID = lcUsance.LCISSUANCEID;
            usanceForArchive.LCUSSANCEID = lcUsance.LCUSSANCEID;
            usanceForArchive.USSANCEAMOUNT = lcUsance.USSANCEAMOUNT;
            usanceForArchive.USSANCERATE = lcUsance.USSANCERATE;
            usanceForArchive.USSANCETENOR = lcUsance.USSANCETENOR;
            usanceForArchive.LCUSSANCEEFFECTIVEDATE = lcUsance.LCUSSANCEEFFECTIVEDATE;
            usanceForArchive.USANCEAPPLICATIONSTATUSID = lcUsance.USANCEAPPLICATIONSTATUSID;
            usanceForArchive.USANCEAPPROVALSTATUSID = lcUsance.USANCEAPPLICATIONSTATUSID;
            usanceForArchive.DELETED = lcUsance.DELETED;
            usanceForArchive.DELETEDBY = lcUsance.DELETEDBY;
            usanceForArchive.LASTUPDATEDBY = lcUsance.LASTUPDATEDBY;
            usanceForArchive.DATETIMEUPDATED = lcUsance.DATETIMEUPDATED;
            usanceForArchive.DATETIMEDELETED = lcUsance.DATETIMEDELETED;
            usanceForArchive.ARCHIVINGOPERATIONID = operationId;
            usanceForArchive.LCUSSANCEMATURITYDATE = lcUsance.LCUSSANCEMATURITYDATE;
            usanceForArchive.CREATEDBY = lcUsance.CREATEDBY;
            usanceForArchive.DATETIMECREATED = lcUsance.DATETIMECREATED;
            usanceForArchive.USANCEAMOUNTCURRENCYID = lcUsance.USANCEAMOUNTCURRENCYID;
            usanceForArchive.USSANCEAMOUNTLOCAL = lcUsance.USSANCEAMOUNTLOCAL;
            usanceForArchive.USANCEREF = lcUsance.USANCEREF;
            usanceForArchive.ARCHIVEDATE = DateTime.Now;

            context.TBL_LC_USSANCE_ARCHIVE.Add(usanceForArchive);

            var saved = context.SaveChanges() > 0;
            return saved;
        }

        private void UpdateLcUsance(int tempLcUsanceId)
        {
            if (tempLcUsanceId <= 0)
            {
                throw new ConditionNotMetException("Temp Usance Id must be greaterthan 1!");
            }

            var tempUs = context.TBL_TEMP_LC_USSANCE.Find(tempLcUsanceId);
            if (tempUs == null)
            {
                throw new ConditionNotMetException("Temp Lc Usance record not found!");
            }
            var us = context.TBL_LC_USSANCE.Find(tempUs.LCUSSANCEID);
            if (us == null)
            {
                throw new ConditionNotMetException("Lc Usance record not found!");
            }
            us.USSANCETENOR = tempUs.NEWUSSANCETENOR;
            us.LCUSSANCEMATURITYDATE = tempUs.NEWLCUSSANCEMATURITYDATE;
        }

        public WorkflowResponse LcUsanceExtensionMemorandum(LcForwardViewModel model)
        {
            int operationId = (int)OperationsEnum.LCUsanceExtensionApproval; // CHANGE
            var applicationDate = general.GetApplicationDate();
            if (model.LcIssuanceId <= 0)
            {
                throw new SecureException("LC Issuance Id must be greater than zero!");
            }
            if (model.tempLcUsanceId <= 0)
            {
                throw new SecureException("TempLcUsanceId must be greater than zero!");
            }
            if (model.lcUssanceId <= 0)
            {
                throw new SecureException("LcUssanceId must be greater than zero!");
            }
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            var us = context.TBL_TEMP_LC_USSANCE.Find(model.tempLcUsanceId);
            var u = context.TBL_LC_USSANCE.Find(model.lcUssanceId);
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            using (var trans = context.Database.BeginTransaction())
            {
                // WORKFLOW
                workflow.OperationId = operationId;
                workflow.StaffId = model.createdBy;
                workflow.TargetId = model.tempLcUsanceId;
                workflow.CompanyId = model.companyId;
                workflow.Vote = model.vote;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                var c = context.TBL_CUSTOMER.Find(lc.CUSTOMERID);

                workflow.BusinessUnitId = c?.BUSINESSUNTID;
                var placeholders = new AlertPlaceholders();
                placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
                placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + lc.LCREFERENCENUMBER;
                placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT";
                placeholders.operationName = "<br />OPERATION NAME: USANCE EXTENSION";
                placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
                workflow.Placeholders = placeholders;

                workflow.DeferredExecution = true;
                workflow.LogActivity();

                WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

                // UPDATE APPLICATION
                //            if (model.vote == 1) { appl.DISPUTED = true; }
                us.APPROVALSTATUSID = (short)workflow.StatusId;
                us.USANCEEXTENSIONAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress;
                u.USANCEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress;
                if ((short)workflow.StatusId == (int)ApprovalStatusEnum.Pending)
                {
                    us.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

                if (workflow.NewState == (int)ApprovalState.Ended) // cam status
                {
                    if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                    {
                        us.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        us.USANCEEXTENSIONAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LcUsanceExtensionCompleted;
                        u.USANCEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.lcUssanceCompleted;
                        var archived = ArchiveLCUsance(us.LCUSSANCEID, operationId);
                        if (archived)
                        {
                            UpdateLcUsance(model.tempLcUsanceId);
                        }
                        else
                        {
                            trans.Rollback();
                            throw new SecureException("There was an Error Archiving this LC Usance!");
                        }
                        workflow.SetResponse = false;
                    }
                    else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                    {
                        us.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                        //SendEmailToCustomerForLoanDisapproval(model.LcIssuanceId, model.companyId);
                    }
                }
                context.SaveChanges();
                trans.Commit();
                return workflow.Response;
            }
            
        }

        public WorkflowResponse LcUssanceMemorandum(LcForwardViewModel model)
        {
            int operationId = (int)OperationsEnum.lcUssance; // CHANGE
            var applicationDate = general.GetApplicationDate();
            var lc = context.TBL_LC_ISSUANCE.Find(model.LcIssuanceId);
            var us = context.TBL_LC_USSANCE.Find(model.lcUssanceId);
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            // WORKFLOW
            workflow.OperationId = operationId;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.lcUssanceId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            //var test1 = loanApp.GetFirstAdhocReceiverLevel(model.createdBy, operationId, null, true);
            //var nextStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            var c = context.TBL_CUSTOMER.Find(lc.CUSTOMERID);

            workflow.BusinessUnitId = c?.BUSINESSUNTID;
            var placeholders = new AlertPlaceholders();
            placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
            placeholders.referenceNumber = "<br />LC REFERENCENUMBER: " + lc.LCREFERENCENUMBER;
            placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER OF CREDIT: USANCE";
            placeholders.operationName = "<br />OPERATION NAME: LC USANCE";
            placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
            workflow.Placeholders = placeholders;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // UPDATE APPLICATION
            //            if (model.vote == 1) { appl.DISPUTED = true; }
            us.USANCEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.lcUssanceInProgress;
            us.USANCEAPPROVALSTATUSID = (short)workflow.StatusId;
            if ((short)workflow.StatusId == (int)ApprovalStatusEnum.Pending)
            {
                us.USANCEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    var usList = context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == model.LcIssuanceId && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).ToList();
                    var companyId = context.TBL_COMPANY.FirstOrDefault().COMPANYID;
                    var totalAmountUsedForUsance = usList.Sum(l => l.USSANCEAMOUNTLOCAL) + us.USSANCEAMOUNTLOCAL;
                    var lcCurrRate = loanApp.GetExchangeRate(DateTime.Now, (short)lc.CURRENCYID, companyId);
                    var lcTotalAmountAvailableForUsance = lc.LCTOLERANCEVALUE * (decimal)lcCurrRate.sellingRate;

                    lc.TOTALUSANCEAMOUNTLOCAL = (totalAmountUsedForUsance);
                    if ((totalAmountUsedForUsance) >= lcTotalAmountAvailableForUsance)
                    { // if the whole lcamount has been used
                        lc.LCUSSANCESTATUSID = (int)LoanApplicationStatusEnum.lcUssanceCompleted;
                        lc.LCUSSANCEAPPROVEDDATE = DateTime.Now;
                        lc.DATEACTEDON = DateTime.Now;
                        lc.LCUSSANCEFINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
                        lc.LCUSSANCEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        
                    }
                    us.USANCEAPPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.lcUssanceCompleted;
                    us.USANCEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    workflow.SetResponse = false;
                }
                else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    us.USANCEAPPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    //SendEmailToCustomerForLoanDisapproval(model.LcIssuanceId, model.companyId);
                }

                if (contextControl != null) contextControl.SaveChanges();
            }

            // Audit Section ---------------------------
            /*  var audit = new TBL_AUDIT
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
  */
            context.SaveChanges();
            //workflow.Response.success = true;
            return workflow.Response;
        }

        public String ResponseMessage(WorkflowResponse response, string itemHeading)
        {
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    return "The " + itemHeading + " request has been APPROVED successfully";
                }
                else
                {
                    return "The " + itemHeading + " request has been DISAPPROVED successfully";
                }
            }

        }

        public WorkflowResponse LetterGenerationRequestMemorandum(LetterGenerationRequestViewModel model)
        {
            int operationId = (int)OperationsEnum.LetterGenerationRequest; // CHANGE
            var lgr = context.TBL_LETTER_GENERATION_REQUEST.Find(model.requestId);
            lgr.OPERATIONID = operationId;
            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
            // WORKFLOW
            //workflow.ResolveMultipleProductPath(operationId, items.Select(x => (short)x.APPROVEDPRODUCTID).ToList());
            workflow.OperationId = operationId;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.requestId;
            workflow.CompanyId = model.companyId;
            workflow.Vote = model.vote;
            workflow.NextLevelId = 0;
            workflow.ToStaffId = null;
            workflow.StatusId = (int)model.forwardAction;
            workflow.Comment = model.comment;
            var c = context.TBL_CUSTOMER.Find(lgr.CUSTOMERID);

            workflow.BusinessUnitId = c?.BUSINESSUNTID;
            var placeholders = new AlertPlaceholders();
            placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
            placeholders.referenceNumber = "<br />LETTER GEN REQ REFERENCENUMBER: " + lgr.REQUESTREF;
            placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER LETTER GEN REQ";
            placeholders.operationName = "<br />OPERATION NAME: LETTER GEN REQ";
            placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
            workflow.Placeholders = placeholders;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // UPDATE APPLICATION
            lgr.APPROVALSTATUSID = (short)workflow.StatusId;
            //            if (model.vote == 1) { appl.DISPUTED = true; }
            lgr.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LetterGenerationRequestInProgress;
            if (lgr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { lgr.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    lgr.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.LetterGenerationRequestCompleted;
                    lgr.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
                    lgr.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    lgr.APPROVEDDATE = DateTime.Now;
                    workflow.SetResponse = false;
                }
                else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    lgr.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }

                if (contextControl != null) contextControl.SaveChanges();
            }

            lgr.DATEACTEDON = DateTime.Now;
            //ValidateAllFromReceiverLevels(model.createdBy, operationId);
            context.SaveChanges();
            //workflow.Response.success = true;
            return workflow.Response;
        }

        //public WorkflowResponse CollateralSwapMemorandum(CollateralSwapViewModel model)
        //{
        //    int operationId = (int)OperationsEnum.CollateralSwap; // CHANGE
        //    var cs = context.TBL_COLLATERAL_SWAP_REQUEST.Find(model.collateralSwapId);
        //    if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved) { model.forwardAction = (int)ApprovalStatusEnum.Processing; }
        //    // WORKFLOW
        //    workflow.OperationId = operationId;
        //    workflow.StaffId = model.createdBy;
        //    workflow.TargetId = model.collateralSwapId;
        //    workflow.CompanyId = model.companyId;
        //    workflow.Vote = model.vote;
        //    //var test1 = loanApp.GetFirstAdhocReceiverLevel(model.createdBy, operationId, null, true);
        //    //var nextStaff = loanApp.GetFirstLevelStaffId((int)nextLevel, model.userBranchId);
        //    workflow.NextLevelId = 0;
        //    workflow.ToStaffId = null;
        //    workflow.StatusId = (int)model.forwardAction;
        //    workflow.Comment = model.comment;
        //    var c = context.TBL_CUSTOMER.Find(cs.TBL_CUSTOMER.CUSTOMERID);

        //    workflow.BusinessUnitId = c?.BUSINESSUNTID;
        //    var placeholders = new AlertPlaceholders();
        //    placeholders.customerName = "<br />CUSTOMER NAME: " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME;
        //    placeholders.referenceNumber = "<br />LETTER GEN REQ REFERENCENUMBER: " + cs.COLLATERALSWAPID;
        //    placeholders.facilityType = "<br />FACILITY INFORMATION: LETTER LETTER GEN REQ";
        //    placeholders.operationName = "<br />OPERATION NAME: LETTER GEN REQ";
        //    placeholders.branchName = "<br />BRANCH NAME: " + c.TBL_BRANCH.BRANCHNAME;
        //    workflow.Placeholders = placeholders;

        //    workflow.DeferredExecution = true;
        //    workflow.LogActivity();

        //    WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

        //    // UPDATE APPLICATION
        //    //cs.APPROVALSTATUSID = (short)workflow.StatusId;
        //    cs.COLLATERALSWAPSTATUSID = (int)LoanApplicationStatusEnum.collateralSwapInProgress;
        //    //if (cs.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { cs.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

        //    if (workflow.NewState == (int)ApprovalState.Ended) // cam status
        //    {
        //        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
        //        {
        //            cs.COLLATERALSWAPSTATUSID = (int)LoanApplicationStatusEnum.collateralSwapCompleted;
        //            //cs.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
        //            //cs.APPROVEDDATE = DateTime.Now;
        //            workflow.SetResponse = true;
        //        }
        //        else if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
        //        {
        //            cs.COLLATERALSWAPSTATUSID = (int)ApprovalStatusEnum.Disapproved;
        //            //SendEmailToCustomerForLoanDisapproval(model.LcIssuanceId, model.companyId);
        //        }

        //        if (contextControl != null) contextControl.SaveChanges();
        //    }

        //    //cs.DATEACTEDON = DateTime.Now;
        //    context.SaveChanges();
        //    return workflow.Response;
        //}


        private bool ValidateReleaseAmount(LcReleaseAmountViewModel model)
        {
           var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var currCode = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == lc.CURRENCYID).CURRENCYCODE;
            var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => (r.RELEASEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseCompleted
                                                                         && r.RELEASEAPPROVALSTATUSID == (int)ApprovalStatusEnum.Approved) && r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT) ?? 0;
            var availableAmount = lc.LCTOLERANCEVALUE - totalReleasedAmount;
            if (model.releaseAmount > availableAmount)
            {
                throw new SecureException("Sorry, available Amount for release is now " + currCode + " " + availableAmount);
            }
            lc.RELEASEDAMOUNT = totalReleasedAmount;
            context.SaveChanges();
            return true;
        }

        public bool ValidateAllFromReceiverLevels(int staffId, int operationId)
        {
            var trail = context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == operationId && t.FROMAPPROVALLEVELID == null).ToList();
            if (trail.Count() > 0)
            {
                foreach(var t in trail)
                {
                    var staffRoleId = context.TBL_STAFF.Where(s => s.DELETED != true && s.STAFFID == t.REQUESTSTAFFID).FirstOrDefault().STAFFROLEID;
                    var level = context.TBL_APPROVAL_LEVEL.Where(l => l.DELETED != true && l.STAFFROLEID == staffRoleId).FirstOrDefault();
                    if (level == null && t.REQUESTSTAFFID == staffId) { throw new SecureException("Please make sure you are assigned an approval Level for the current operation"); }
                    t.FROMAPPROVALLEVELID = level.APPROVALLEVELID;
                }
                    context.SaveChanges();
                return true;
            }
            return false;
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
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s; 
            
            var allstaff = this.GetAllStaffNames();

            var application = context.TBL_LOAN_APPLICATION.Find(applicationId);

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId).ToList();

            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == application.OPERATIONID && x.TARGETID == applicationId ).ToList();
            }

            var data =  trail.Select(x => new ApprovalTrailViewModel
                {
                    approvalTrailId = x.APPROVALTRAILID,
                    comment = x.COMMENT,
                    targetId = x.TARGETID,
                    operationId = x.OPERATIONID,
                    arrivalDate = x.ARRIVALDATE,
                    systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                    responseDate = x.RESPONSEDATE,
                    systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                    responseStaffId = x.RESPONSESTAFFID,
                    requestStaffId = x.REQUESTSTAFFID,
                    toStaffId = x.TOSTAFFID,
                    loopedStaffId = x.LOOPEDSTAFFID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID ,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a=>a.APPROVALLEVELID ==x.FROMAPPROVALLEVELID).Select(a=>a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a=>a.APPROVALLEVELID ==x.TOAPPROVALLEVELID).Select(a=>a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    //applicationId = application.LOANAPPLICATIONID,
                    commentStage = "Credit Appaisal",
                    toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                    fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();

            //data.AddRange(GetOfferLetterTrail(applicationId));
            if (getAll)
            {
                data.AddRange(GetNonAppraisalTrail(applicationId, (short)OperationsEnum.OfferLetterApproval, "Offer Letter"));
                //data.AddRange(GetNonAppraisalTrail(applicationId, (short)OperationsEnum.LoanAvailment, "Availment"));

                //foreach (var t in data.ToList())
                if (application != null)
                {
                    var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId).ToList();
                    foreach (var f in facilities)
                    {
                        var request = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID);
                        foreach (var r in request)
                        {
                            if (r?.OPERATIONID != null)
                            {
                                data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, r.OPERATIONID ?? 0, "Drawdown"));
                            }
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.TermLoanBooking, "Booking"));
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.RevolvingLoanBooking, "Booking"));
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.ContigentLoanBooking, "Booking"));
                        }
                    }
                };
                
                
            }
            

            data.OrderByDescending(d => d.approvalTrailId);
            foreach (var d in data)
            {
                if (d.fromApprovalLevelId == d.toApprovalLevelId)
                {
                    if (d.loopedStaffId > 0)
                    {
                        d.toApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.loopedStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                    else
                    {
                        d.fromApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.requestStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                }
            }

            return data;
        }

        public IEnumerable<ApprovalTrailViewModel> GetCallmemoApprovalTrail(int applicationId, int operationId)
        {
            var staffs = from s in context.TBL_STAFF select s;
            var allstaff = this.GetAllStaffNames();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId).ToList();
            
            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                operationId = x.OPERATIONID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();
            return data;
        }

        private IEnumerable<ApprovalTrailViewModel> GetNonAppraisalTrail(int applicationId, int operationid, string commentStage)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
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
                toStaffId = x.TOSTAFFID,
                loopedStaffId = x.LOOPEDSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                commentStage = commentStage,

                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();
          
           
            data.OrderByDescending(d => d.systemArrivalDateTime);
            return data;
        }

        public IEnumerable<ApprovalTrailViewModel> GetGlobalInterestRateChangeTrail(int applicationId, int operationid)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
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
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();


            data.OrderByDescending(d => d.systemArrivalDateTime);
            return data;
        }

        public IEnumerable<ApprovalTrailViewModel> GetTrailForReferBack(int applicationId, int operationId, int currentLevelId = 0, bool getAll = false, bool isClassified = false, bool isLMSCrossWorkflow = false)
        {
            if (isLMSCrossWorkflow)
            {
                //operationId = context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID == operationId)?.SYNCHOPERATIONID ?? operationId;
                return GetClassifiedLMSTrailForReferBack(applicationId, operationId, currentLevelId, getAll = false, isClassified);
            }

            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;
            var creditOperationIds = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Select(f => f.OPERATIONID).ToList();
            var allstaff = this.GetAllStaffNames();

            
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId && x.FROMAPPROVALLEVELID != null).ToList();
            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId && x.TARGETID == applicationId).ToList();
            }
            if (isClassified)
            {
                var operationRecord = context.TBL_OPERATIONS.Find(operationId);
                var classOperations = context.TBL_OPERATIONS.Where(x => x.CLASS == operationRecord.CLASS && operationRecord.CLASS != null).Select(c=>c.OPERATIONID).ToList() ;
                trail.AddRange(context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == applicationId && classOperations.Contains(x.OPERATIONID)).ToList());

            }
            trail = trail.Where(t => !(t.FROMAPPROVALLEVELID == t.TOAPPROVALLEVELID && t.LOOPEDSTAFFID > 0)).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
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
                operationId = x.OPERATIONID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderBy(x => x.systemArrivalDateTime).ToList();

            if(currentLevelId == 0)
            {
                currentLevelId = data.LastOrDefault()?.toApprovalLevelId ?? 0;
            }

            while (data.Exists(d => d.approvalStateId == (int)ApprovalState.Ended && (d.approvalStatusId != (int)ApprovalStatusEnum.Approved && d.approvalStatusId != (int)ApprovalStatusEnum.Closed)) && !isClassified)//get only un-ended trail incase of workflow ending&/change
            {
                var firstTrail = data.FirstOrDefault(t => t.approvalStateId == (int)ApprovalState.Ended && (t.approvalStatusId != (int)ApprovalStatusEnum.Approved && t.approvalStatusId != (int)ApprovalStatusEnum.Closed));
                data = data.Where(t => t.approvalTrailId > firstTrail.approvalTrailId).ToList();
            }

            var data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            if (data.Count > 0 && currentLevelId > 0)//get only from the current level downwards
            {
                var firstTrail = data.FirstOrDefault(t => t.toApprovalLevelId == currentLevelId);
                if (firstTrail != null)
                {
                    data = data.Where(t => t.approvalTrailId <= firstTrail?.approvalTrailId).ToList();
                    //data = data.Where(t => t.approvalTrailId <= firstTrail?.approvalTrailId && t.fromApprovalLevelId > 0).ToList();
                }
            }

            if (data.Count == 0)
            {
                data = trail.Where(x => x.FROMAPPROVALLEVELID > 0).Select(x => new ApprovalTrailViewModel
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
                    operationId = x.OPERATIONID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                    fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                })?.OrderBy(x => x.systemArrivalDateTime).ToList();

                data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            }


            var data2 = data.ToList();
            var testData = data.ToList();
            foreach (var t in testData)//filter repeated levels as result of refer backs
            {
                var firstTrailForLevel = testData.OrderBy(x => x.approvalTrailId).FirstOrDefault(x => x.fromApprovalLevelId == t.fromApprovalLevelId);
                var multipleTrails = testData.Where(d => d.fromApprovalLevelId == firstTrailForLevel.fromApprovalLevelId && d.approvalTrailId != firstTrailForLevel.approvalTrailId).ToList();
                foreach(var tr in multipleTrails)
                {
                    data2.RemoveAll(d => d.approvalTrailId == tr.approvalTrailId);
                }
            }

            foreach (var d in data2)
            {
                var lastOccurrence = data3.FirstOrDefault(d3 => d3.fromApprovalLevelId == d.fromApprovalLevelId);
                if (lastOccurrence != null)
                {
                    d.requestStaffId = lastOccurrence.requestStaffId;
                }
            }

            data = data2;
            data.OrderByDescending(d => d.systemArrivalDateTime).ToList();
             return data;
        }//Ify


        public IEnumerable<ApprovalTrailViewModel> GetClassifiedLMSTrailForReferBack(int applicationId, int operationId, int currentLevelId = 0, bool getAll = false, bool isClassified = false)
        {
            //var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;
            //var creditOperationIds = context.TBL_LMSR_FLOW_ORDER.Select(f => f.OPERATIONID).ToList();
            var allstaff = this.GetAllStaffNames();

            List<int> lmsOperationIds = new List<int>();
            List<int> lmsDrawdownOperationIds = new List<int>()
            { (short)OperationsEnum.LoanReviewDrawdownForExtension, (short)OperationsEnum.OverdraftReviewDrawdownForExtension,(short)OperationsEnum.ContingentReviewDrawdownForExtension  };

            var lmsAppraisalOperations = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (short)OperationTypeEnum.LoanReviewApplication).Select(c=>c.OPERATIONID).ToList();

            var lmsAppraisalOperation = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == applicationId && lmsAppraisalOperations.Contains(x.OPERATIONID)).Select(b => b.OPERATIONID).ToList();

            var isFromOperations = context.TBL_LMSR_APPLICATION.FirstOrDefault(l => l.LOANAPPLICATIONID == applicationId)?.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved;
            var operationTypeId = context.TBL_OPERATIONS.Where(x => x.OPERATIONID == operationId).Select(b => b.OPERATIONTYPEID).FirstOrDefault();

            var applicationWentForDrawDown = context.TBL_APPROVAL_TRAIL.Any(d => d.TARGETID == applicationId && lmsDrawdownOperationIds.Contains(d.OPERATIONID));
            if (!isFromOperations)
            {
                if (applicationWentForDrawDown)
                {
                    lmsOperationIds.AddRange(lmsDrawdownOperationIds);
                }
                else
                {
                    lmsOperationIds.AddRange(lmsAppraisalOperation);
                }
            }
            if (operationId != (int)OperationsEnum.LoanReviewApprovalAvailment)
            {
                lmsOperationIds.Add((short)OperationsEnum.LoanReviewApprovalAvailment);
            }
            //if (operationId == (short)OperationsEnum.LoanReviewApprovalAvailment)
            //{
            //    lmsOperationIds.AddRange(lmsDrawdownOperationIds);
            //}
            //if (operationTypeId == (short)OperationTypeEnum.LoanReviewApplication)
            //{
            //    lmsOperationIds.AddRange(lmsAppraisalOperation);
            //}

            //if (operationTypeId == (short)OperationTypeEnum.LoanManagement || operationTypeId == (short)OperationTypeEnum.LoanManagementOverdraft)
            //{
            //    lmsOperationIds.Add((short)OperationsEnum.LoanReviewApprovalAvailment);
            //}
            //if (operationId != (short)OperationsEnum.LoanReviewApprovalAvailment)
            //{// for lms inputter
            //    lmsOperationIds.Add((short)OperationsEnum.LoanReviewApprovalAvailment);
            //}
            //lmsOperationIds.Add((short)OperationsEnum.LoanReviewApprovalAvailment);

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => lmsOperationIds.Contains(x.OPERATIONID) && x.TARGETID == applicationId && x.FROMAPPROVALLEVELID != null).ToList();
            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => lmsOperationIds.Contains(x.OPERATIONID) && x.TARGETID == applicationId).ToList();
            }
            if (isClassified)
            {
                var operationRecord = context.TBL_OPERATIONS.Find(operationId);
                var classOperations = context.TBL_OPERATIONS.Where(x => x.CLASS == operationRecord.CLASS && operationRecord.CLASS != null).Select(c => c.OPERATIONID).ToList();
                trail.AddRange(context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == applicationId && classOperations.Contains(x.OPERATIONID)).ToList());

            }
            trail = trail.Where(t => !(t.FROMAPPROVALLEVELID == t.TOAPPROVALLEVELID && t.LOOPEDSTAFFID > 0)).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
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
                operationId = x.OPERATIONID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderBy(x => x.systemArrivalDateTime).ToList();

            if (currentLevelId == 0)
            {
                currentLevelId = data.LastOrDefault()?.toApprovalLevelId ?? 0;
            }

            while (data.Exists(d => d.approvalStateId == (int)ApprovalState.Ended && (d.approvalStatusId != (int)ApprovalStatusEnum.Approved && d.approvalStatusId != (int)ApprovalStatusEnum.Closed)) && !isClassified)//get only un-ended trail incase of workflow ending&/change
            {
                var firstTrail = data.FirstOrDefault(t => t.approvalStateId == (int)ApprovalState.Ended && (t.approvalStatusId != (int)ApprovalStatusEnum.Approved && t.approvalStatusId != (int)ApprovalStatusEnum.Closed));
                data = data.Where(t => t.approvalTrailId > firstTrail.approvalTrailId).ToList();
            }

            var data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            if (data.Count > 0 && currentLevelId > 0)//get only from the current level downwards
            {
                var firstTrail = data.FirstOrDefault(t => t.toApprovalLevelId == currentLevelId);
                if (firstTrail != null)
                {
                    data = data.Where(t => t.approvalTrailId <= firstTrail?.approvalTrailId).ToList();
                    //data = data.Where(t => t.approvalTrailId <= firstTrail?.approvalTrailId && t.fromApprovalLevelId > 0).ToList();
                }
            }

            if (data.Count == 0)
            {
                data = trail.Where(x => x.FROMAPPROVALLEVELID > 0).Select(x => new ApprovalTrailViewModel
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
                    operationId = x.OPERATIONID,
                    fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                    fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = x.TOAPPROVALLEVELID,
                    approvalStateId = x.APPROVALSTATEID,
                    approvalStatusId = x.APPROVALSTATUSID,
                    approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                    approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                    fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                })?.OrderBy(x => x.systemArrivalDateTime).ToList();

                data3 = data.OrderByDescending(d => d.systemArrivalDateTime);
            }


            var data2 = data.ToList();
            var testData = data.ToList();
            foreach (var t in testData)//filter repeated levels as result of refer backs
            {
                var firstTrailForLevel = testData.OrderBy(x => x.approvalTrailId).FirstOrDefault(x => x.fromApprovalLevelId == t.fromApprovalLevelId);
                var multipleTrails = testData.Where(d => d.fromApprovalLevelId == firstTrailForLevel.fromApprovalLevelId && d.approvalTrailId != firstTrailForLevel.approvalTrailId).ToList();
                foreach (var tr in multipleTrails)
                {
                    data2.RemoveAll(d => d.approvalTrailId == tr.approvalTrailId);
                }
            }

            foreach (var d in data2)
            {
                var lastOccurrence = data3.FirstOrDefault(d3 => d3.fromApprovalLevelId == d.fromApprovalLevelId);
                if (lastOccurrence != null)
                {
                    d.requestStaffId = lastOccurrence.requestStaffId;
                    d.sourceOperationId = (short)operationId;
                    d.sourceTargetId = applicationId;
                }
            }

            data = data2;
            data.OrderByDescending(d => d.systemArrivalDateTime).ToList();
            return data;
        }//Ify



        public PrivilegeViewModel GetUserPrivilege(AuthoritySignatureViewModel entity)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(entity.targetId);
            List<int> ExclusiveOperations = new List<int>(); // (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();
            List<int> levelIds = new List<int>();

            //ExclusiveOperations.Add(entity.operationId);
            if (appl != null)
            {
                ExclusiveOperations.Add(appl.OPERATIONID);
            }

            //var operationId = entity.operationId;
            var staffId = entity.createdBy;
            var staff = context.TBL_STAFF.Find(staffId);
            IQueryable<PrivilegeViewModel> grants;
            PrivilegeViewModel grant;

            // check default role
            var rank = context.TBL_STAFF_ROLE.Find(staff.STAFFROLEID);

            grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID)) ) )
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
                        staffId = staff.STAFFID,
                        roleId = staff.STAFFROLEID,
                        userBranchId = (short)staff.BRANCHID
                    });

            if (grants.Any(x => x.approvalLevelId == entity.levelId) == false) // if no specifics
            {
                grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID))))
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
                            staffId = staff.STAFFID,
                            roleId = staff.STAFFROLEID,
                            userBranchId = (short)staff.BRANCHID
                        });
            }

            grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant == null) { return GetRelieverPrivilege(entity); }
            grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
            grant.owner = grant.userApprovalLevelIds.Contains((int)entity.levelId);

            return grant;
        }

        public PrivilegeViewModel GetUserPrivilegeByCode(AuthoritySignatureViewModel entity)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(entity.targetId);
            List<int> ExclusiveOperations = new List<int>(); // (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();
            List<int> levelIds = new List<int>();

            //ExclusiveOperations.Add(entity.operationId);
            if (appl != null)
            {
                ExclusiveOperations.Add(appl.OPERATIONID);
            }

            //var operationId = entity.operationId;
            var staffRoleCode = entity.staffRoleCode;
            var rank = context.TBL_STAFF_ROLE.Where(c => c.STAFFROLECODE == staffRoleCode).FirstOrDefault();
            if (rank == null)
            {
                throw new SecureException("Staff role " + staffRoleCode + " cannot be found");
            }
            IQueryable<PrivilegeViewModel> grants;
            PrivilegeViewModel grant;

            // check default role
            // var rank = context.TBL_STAFF_ROLE.Find(staffRole);

            //grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID))))
            //    .Join(context.TBL_APPROVAL_GROUP.Where(x => x.DELETED == false),
            //        m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
            //    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true),
            //        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })
            //    .Join(context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.DELETED == false ),
            //        gl => gl.l.APPROVALLEVELID, s => s.APPROVALLEVELID, (gl, s) => new PrivilegeViewModel
            //        {
            //            viewCamDocument = s.CANVIEWDOCUMENT,
            //            canMakeChanges = s.CANEDIT,
            //            canAppendTemplate = s.CANEDIT,
            //            viewUploadedFiles = s.CANVIEWUPLOAD,
            //            canUploadFile = s.CANUPLOAD,
            //            viewApproval = s.CANVIEWAPPROVAL,
            //            canApprove = s.CANAPPROVE,
            //            approvalLimit = s.MAXIMUMAMOUNT,
            //            approvalLevelId = s.APPROVALLEVELID,
            //            groupRoleId = gl.mg.g.ROLEID,
            //            canEscalate = gl.l.CANESCALATE,
            //            levelTypeId = gl.l.LEVELTYPEID,
            //            staffId = entity.createdBy,
            //            roleId = rank.STAFFROLEID,
            //            //userBranchId = (short)entity.BRANCHID
            //        });

            //if (grants.Any(x => x.approvalLevelId == entity.levelId) == false) // if no specifics
            //{
            grants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID))))
                .Join(context.TBL_APPROVAL_GROUP.Where(x => x.DELETED == false),
                    m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.DELETED == false && x.ISACTIVE == true && x.STAFFROLEID == rank.STAFFROLEID),
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
                        staffId = entity.createdBy,
                        roleId = rank.STAFFROLEID,
                            //userBranchId = (short)staff.BRANCHID
                        });
            //}

            grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant == null) { return GetRelieverPrivilege(entity); }
            grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId).ToList();
            grant.owner = grant.userApprovalLevelIds.Contains((int)entity.levelId);

            return grant;
        }

        private PrivilegeViewModel GetRelieverPrivilege(AuthoritySignatureViewModel entity)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(entity.targetId);
            List<int> ExclusiveOperations = new List<int>(); // (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();
            List<int> levelIds = new List<int>();
            List<PrivilegeViewModel> grants = new List<PrivilegeViewModel>();
            PrivilegeViewModel grant;

            //ExclusiveOperations.Add(entity.operationId);
            if (appl != null)
            {
                ExclusiveOperations.Add(appl.OPERATIONID);
            }
            var now = DateTime.Now;
            var relieverStaffs = context.TBL_STAFF_RELIEF
                    .Where(x => x.DELETED == false
                        && x.RELIEFSTAFFID == entity.createdBy
                        && x.STARTDATE <= now
                        && x.ENDDATE >= now
                        && x.ISACTIVE == true
                    ).ToList();

            if (relieverStaffs == null) { return new PrivilegeViewModel(); }
            foreach(var relieverStaff in relieverStaffs)
            {
                // mirror above
                var operationId = entity.operationId;
                var staffId = relieverStaff.STAFFID; // changed
                var staff = context.TBL_STAFF.Find(staffId);

                // check default role
                var rank = context.TBL_STAFF_ROLE.Find(staff.STAFFROLEID);

                var reliefgrants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID))))
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
                        }).ToList();

                var test1 = reliefgrants.ToList();

                if (reliefgrants.Any() == false) // check specific
                {
                    reliefgrants = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && ((x.OPERATIONID == entity.operationId && x.PRODUCTCLASSID == entity.productClassId) || (ExclusiveOperations.Contains(x.OPERATIONID))))
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
                         }).ToList();
                }

                grants.AddRange(reliefgrants);
            }

            var test = grants.ToList();

            grant = grants.FirstOrDefault(x => x.approvalLevelId == entity.levelId);
            if (grant == null) { grant = new PrivilegeViewModel(); } // changed
            grant.userApprovalLevelIds = grants.Select(x => x.approvalLevelId)?.ToList();

            grant.owner = grant.userApprovalLevelIds.Contains(entity.levelId ?? 0);

            return grant;
        }

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.LASTNAME
                //name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
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

                        approvedProductName = (x.a.FLOWCHANGEID == null || x.a.FLOWCHANGEID <= 0 || x.a.FLOWCHANGEID == (short)FlowChangeEnum.FAM) ? x.d.TBL_PRODUCT.PRODUCTNAME : x.d.TBL_PRODUCT.PRODUCTNAME + "(" + context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(c => c.FLOWCHANGEID == x.a.FLOWCHANGEID).PLACEHOLDER + ")", //x.d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                        approvedTenor = x.d.APPROVEDTENOR,
                        approvedRate = x.d.APPROVEDINTERESTRATE,
                        approvedAmount = x.d.APPROVEDAMOUNT,
                        approvedProductId = x.d.APPROVEDPRODUCTID,

                        statusId = x.d.STATUSID,
                        exchangeRate = x.d.EXCHANGERATE,
                        terms = x.d.REPAYMENTTERMS,
                        repaymentScheduleId = x.d.REPAYMENTSCHEDULEID,
                        schedule = x.d.TBL_REPAYMENT_TERM.REPAYMENTTERMDETAIL,
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

                        //schedule = x.d.REPAYMENTSCHEDULEID != null ? context.TBL_REPAYMENT_TERM.Where(O => O.REPAYMENTSCHEDULEID == x.d.REPAYMENTSCHEDULEID).FirstOrDefault().REPAYMENTTERMDETAIL : null,
                        interestRepayment = x.d.INTERESTREPAYMENTID != null ? context.TBL_REPAYMENT_TERM.Where(O => O.REPAYMENTSCHEDULEID == x.d.INTERESTREPAYMENTID).FirstOrDefault().REPAYMENTTERMDETAIL : null,
                        interestRepaymentId = x.d.INTERESTREPAYMENTID,
                        moratorium = x.d.MORATORIUM,
                        //approvedTradeCycleDays = context.TBL_APPROVED_TRADE_CYCLE.Where(T => T.APPROVEDTRADECYCLEID == x.d.APPROVEDTRADECYCLEID).FirstOrDefault().APPROVEDTRADECYCLEDAYS : null,
                        //approvedTradeCycleId = x.d.APPROVEDTRADECYCLEID
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

        public LoanApplicationDetailsViewModel GetApprovedTrancheDetail(int bookingRequestId)
        {
            var details = new LoanApplicationDetailsViewModel();
            var tranche = new LoanApplicationDetailsViewModel();

            var facilities = (from b in context.TBL_LOAN_BOOKING_REQUEST
                              join d in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                              join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                              where b.LOAN_BOOKING_REQUESTID == bookingRequestId
                              select new ApprovedLoanDetailViewModel
                              {
                                  bookingRequestId = b.LOAN_BOOKING_REQUESTID,
                                  trancheAmount = b.AMOUNT_REQUESTED,
                                  loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                  applicationId = d.LOANAPPLICATIONID,
                                  customerId = d.TBL_CUSTOMER.CUSTOMERID,
                                  obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                                  currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                  loanPurpose = d.LOANPURPOSE,
                                  proposedProductName = d.TBL_PRODUCT.PRODUCTNAME,
                                  proposedTenor = d.PROPOSEDTENOR,
                                  proposedRate = d.PROPOSEDINTERESTRATE,
                                  proposedAmount = d.PROPOSEDAMOUNT,
                                  proposedProductId = d.PROPOSEDPRODUCTID,
                                  proposedProductClassId = d.TBL_PRODUCT.PRODUCTCLASSID,

                                  approvedProductName = (a.FLOWCHANGEID == null || a.FLOWCHANGEID <= 0 || a.FLOWCHANGEID == (short)FlowChangeEnum.FAM) ? d.TBL_PRODUCT.PRODUCTNAME : d.TBL_PRODUCT.PRODUCTNAME + "(" + context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(c => c.FLOWCHANGEID == a.FLOWCHANGEID).PLACEHOLDER + ")", //d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                                  approvedTenor = d.APPROVEDTENOR,
                                  approvedRate = d.APPROVEDINTERESTRATE,
                                  approvedAmount = d.APPROVEDAMOUNT,
                                  approvedProductId = d.APPROVEDPRODUCTID,

                                  statusId = d.STATUSID,
                                  exchangeRate = d.EXCHANGERATE,
                                  terms = d.REPAYMENTTERMS,
                                  repaymentScheduleId = d.REPAYMENTSCHEDULEID,
                                  //schedule = d.TBL_REPAYMENT_TERM.REPAYMENTTERMDETAIL,
                                  securedByCollateral = d.SECUREDBYCOLLATERAL,
                                  crmsCollateralTypeId = d.CRMSCOLLATERALTYPEID,
                                  crmsRepaymentTypeId = d.CRMSREPAYMENTAGREEMENTID,
                                  isSpecialised = (bool)d.ISSPECIALISED,

                                  priceIndexId = d.PRODUCTPRICEINDEXID,
                                  priceIndexName = d.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                  productRiskRating = d.TBL_PRODUCT.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                                  syndicationName = d.FIELD2,
                                  syndicationRefNo = d.FIELD1,
                                  syndicationAmount = d.FIELD3,
                                  conditionPrecedent = d.CONDITIONPRECIDENT,
                                  conditionSubsequent = d.CONDITIONSUBSEQUENT,
                                  transactionDynamics = d.TRANSACTIONDYNAMICS,
                              }).ToList();
            //var facilities1 = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == bookingRequestId)
            //    .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => dELETED == false),
            //    a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
            //    .Select(x => new ApprovedLoanDetailViewModel
            //    {
                    

            //    })
            //    .ToList();

            
            details.facilities = facilities;
            details.application = GetLoanApplicationInformation(facilities.FirstOrDefault().applicationId);

            return details;
        }

        public LoanApplicationDetailsViewModel GetLoanApplicationDetailByRefNo(string applicationReferenceNumber)
        {
            var details = new LoanApplicationDetailsViewModel();
            var facilities = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == applicationReferenceNumber)
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

                    approvedProductName = (x.a.FLOWCHANGEID == null || x.a.FLOWCHANGEID <= 0 || x.a.FLOWCHANGEID == (short)FlowChangeEnum.FAM) ? x.d.TBL_PRODUCT.PRODUCTNAME : x.d.TBL_PRODUCT.PRODUCTNAME + "(" + context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(c => c.FLOWCHANGEID == x.a.FLOWCHANGEID).PLACEHOLDER + ")", //x.d.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                        approvedTenor = x.d.APPROVEDTENOR,
                    approvedRate = x.d.APPROVEDINTERESTRATE,
                    approvedAmount = x.d.APPROVEDAMOUNT,
                    approvedProductId = x.d.APPROVEDPRODUCTID,

                    statusId = x.d.STATUSID,
                    exchangeRate = x.d.EXCHANGERATE,
                    terms = x.d.REPAYMENTTERMS,
                    schedule = x.d.TBL_REPAYMENT_TERM.REPAYMENTTERMDETAIL,
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
            var duplications = context.TBL_LOAN_APPLICATION.Where(x => customerIds.Contains((int)x.CUSTOMERID)
                && x.DELETED == false
                && x.APPLICATIONREFERENCENUMBER != applicationReferenceNumber
                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
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
                //productName = x.d.TBL_PRODUCT.PRODUCTNAME,
            })
            .ToList();

            details.duplications = duplications;
            details.facilities = facilities;
           // details.application = GetLoanApplicationInformation(applicationId);

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
                    schedule = x.d.TBL_REPAYMENT_TERM.REPAYMENTTERMDETAIL,
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
                applicationStatusId = a.APPLICATIONSTATUSID,
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
                //isRelatedParty = a.ISRELATEDPARTY,
                isRelatedParty = a.TBL_LOAN_APPLICATION_DETAIL.Any(d => d.TBL_CUSTOMER.ISREALATEDPARTY == true),
                isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                customerGroupId = a.CUSTOMERGROUPID ?? 0,
                customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                createdBy = a.OWNEDBY,
                applicationDate = a.APPLICATIONDATE,
                applicationTenor = a.APPLICATIONTENOR,
                applicationAmount = a.APPLICATIONAMOUNT,
                dateTimeCreated = a.DATETIMECREATED,
                collateralDetail = a.COLLATERALDETAIL,
                isEmployerRelated = a.ISEMPLOYERRELATED,
                employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == a.RELATEDEMPLOYERID).EMPLOYER_NAME,
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
                }).ToList();

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

        #region FAM Pending Applications

        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications(int operationId, int companyId, int branchId, int staffId, int? classId, bool isSpecific)
        {
            // var declarations
            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();
            List<int> levelIds = new List<int>();
            //List<int> levelIds2 = new List<int>();
           
            ExclusiveOperations.Add(operationId);
            var levIds  = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            foreach (var i in ExclusiveOperations)
            {
                levelIds.AddRange(levIds);
            }
            
            var staffs = general.GetStaffRlieved(staffId);
            //var currentStaff = context.TBL_STAFF.Find(staffs[0]);

             IQueryable<LoanApplicationViewModel> applications = null;

            var query = new List<LoanApplicationViewModel>();

            query = context.TBL_LOAN_APPLICATION.Where(x =>
                x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.COMPANYID == companyId
                && (classId == null) ? true : (x.PRODUCTCLASSID == (short?)classId)
                && x.ISADHOCAPPLICATION != true
            )
        .OrderByDescending(x => x.LOANAPPLICATIONID)
        .Join(
            context.TBL_APPROVAL_TRAIL.Where(x => (ExclusiveOperations.Contains(x.OPERATIONID) || ExclusiveOperations.Contains(x.DESTINATIONOPERATIONID ?? 0))
                && x.APPROVALSTATEID != (int)ApprovalState.Ended
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.RESPONSESTAFFID == null
                && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                && (x.TOSTAFFID == null || staffs.Contains((int)x.TOSTAFFID))
                //&& (staffs.Contains((int)x.TOSTAFFID))
            ),
            a => a.LOANAPPLICATIONID,
            b => b.TARGETID,
            (a, b) => new { a, b })
        .Select(x => new LoanApplicationViewModel
        {
            //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
            loanApplicationId = x.a.LOANAPPLICATIONID,
            termSheetCode = x.a.TERMSHEETID,
            //loanApplicationDetailId = x.a.LOANAPPLICATIONID,
            applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
            relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
            customerId = x.a.CUSTOMERID,
            branchId = x.a.BRANCHID,
            currencyId = context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.CURRENCYID)
                                        .FirstOrDefault(),
            productClassId = x.a.PRODUCTCLASSID,
            productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
            proposedProductName = context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
            customerGroupId = x.a.CUSTOMERGROUPID,
            loanTypeId = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
            relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
            relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
            applicationDate = x.a.APPLICATIONDATE,
            systemDateTime = x.a.SYSTEMDATETIME,
            applicationAmount = x.a.APPLICATIONAMOUNT,
            facility = x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
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
            divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
            divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
            customerBusinessUnitId = context.TBL_CUSTOMER.Where(s => s.CUSTOMERID == x.a.CUSTOMERID).Select(c => c.BUSINESSUNTID).FirstOrDefault(),
            timeIn = x.b.SYSTEMARRIVALDATETIME,
            slaTime = x.b.SLADATETIME,

            loanInformation = x.a.LOANINFORMATION,
            submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
            customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
            isRelatedParty = x.a.ISRELATEDPARTY,
            isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
            approvalStatusId = x.b.APPROVALSTATUSID,
            applicationStatusId = x.a.APPLICATIONSTATUSID,
            branchName = x.a.TBL_BRANCH.BRANCHNAME,
            relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
            relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
            misCode = x.a.MISCODE,
            loanTypeName = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            createdBy = x.a.OWNEDBY,
            loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
            customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
            customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
            customerTypeId = x.a.LOANAPPLICATIONTYPEID,
            isInvestmentGrade = x.a.ISINVESTMENTGRADE,
            loantermSheetId = x.a.LOANTERMSHEETID,
            loantermSheetCode = x.a.TERMSHEETID,
            loansWithOthers = x.a.LOANSWITHOTHERS,
            ownershipStructure = x.a.OWNERSHIPSTRUCTURE,
            requireCollateral = x.a.REQUIRECOLLATERAL,
            regionId = x.a.CAPREGIONID,
            collateralDetail = x.a.COLLATERALDETAIL,
            isadhocapplication = x.a.ISADHOCAPPLICATION,
            requireCollateralTypeId = x.a.REQUIRECOLLATERALTYPEID,
            operationId = x.a.OPERATIONID,
            productClassProcessId = x.a.PRODUCT_CLASS_PROCESSID,
            tranchLevelId = x.a.TRANCHEAPPROVAL_LEVELID,
            countryId = context.TBL_COUNTRY.FirstOrDefault().COUNTRYID,
            globalsla = context.TBL_LOAN_APPLICATION_DETAIL
                                            .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                            .Select(s => s.TBL_PRODUCT1.TBL_PRODUCT_CLASS.GLOBALSLA).Max(),
            currentApprovalLevelSlaInterval = x.b.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
            dateTimeCreated = x.a.DATETIMECREATED,
            apiRequestId = x.a.APIREQUESTID
        }).ToList();

            if (isSpecific)
            {
                query = query.Where(q => q.toStaffId == null).ToList();
            }

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());
            //var test = applications.ToList();
            //applications = test.AsQueryable();
            return applications;
            //.Where(x=>x.originatorBusinessUnitId == loggedOnStaff.BUSINESSUNITID);//.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        public IEnumerable<SubsidiaryViewModel> GetSubsidiaryPendingLoanApplications(int applicationId, int countryId, int branchId, int staffId, int? classId, string staffRoleCode, bool isSpecific = false)
        {
            try
            {
                var data = (from a in context.TBL_SUB_BASICTRANSACTION
                            where
                            (a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            || a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                            || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                            && a.STAFFROLECODE == staffRoleCode && a.ACTEDON == false

                            select new SubsidiaryViewModel
                            {
                                subBasicId = a.ID,
                                loanApplicationId = a.LOANAPPLICATIONID,
                                loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                relatedReferenceNumber = a.RELATEDREFERENCENUMBER,
                                customerId = a.CUSTOMERID,
                                customerGlobalId = a.CUSTOMERGLOBALID,
                                countryCode = a.COUNTRYCODE,
                                productClassName = a.PRODUCTCLASSNAME,
                                productClassProcess = a.PRODUCT_CLASS_PROCESS,
                                subsidiaryId = a.SUBSIDIARYID,
                                applicationDate = a.APPLICATIONDATE,
                                systemDateTime = (DateTime)a.SYSTEMDATETIME,
                                applicationAmount = a.APPLICATIONAMOUNT,
                                totalExposureAmount = a.TOTALEXPOSUREAMOUNT,
                                interestRate = a.INTERESTRATE,
                                applicationTenor = a.APPLICATIONTENOR,
                                currentApprovalLevelId = a.APPROVALLEVELID,
                                currentApprovalLevelTypeId = a.APPROVALLEVELGLOBALCODE,
                                toStaffId = a.TOSTAFFID,
                                divisionCode = a.BUSINESSUNITSHORTCODE,
                                timeIn = a.SYSTEMARRIVALDATETIME,
                                approvalStatusId = (short)a.APPROVALSTATUSID,
                                applicationStatusId = (short)a.APPLICATIONSTATUSID,
                                operationName = a.OPERATIONNAME,
                                customerName = a.CUSTOMERID.HasValue ? a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME : "",
                                dateTimeCreated = a.DATETIMECREATED,
                                createdBy = a.CREATEDBY,
                                createdByName = a.CREATEDBYNAME,
                                targetId = a.TARGETID,
                                operationId = a.OPERATIONID,
                                actedOn = a.ACTEDON,
                                loanTypeName = a.LOANAPPLICATIONTYPENAME,
                                facility = a.PRODUCTNAME,
                                divisionShortCode = a.BUSINESSUNITSHORTCODE

                            }).ToList();

                return data;
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        public async Task<IEnumerable<SubsidiaryViewModel>> GetSubsidiaries()
        {
            var data = await (from a in stgContext.STG_SUBSIDIARIES
                              select new SubsidiaryViewModel
                              {
                                  subsidiaryId = a.SUBSIDIARYID,
                                  subsidiaryName = a.SUBSIDIARYNAME,
                                  countryId = a.COUNTRYID
                              }).ToListAsync();
            return data;
        }
        public List<LoanApplicationViewModel> CalculateSLA(List<LoanApplicationViewModel> apps)
        {
            foreach(var app in apps)
            {
                app.slaGlobalStatus = GetSlaGlobalStatus(app);
                app.slaInduvidualStatus = GetSlaInduvidualStatus(app);
                //if(app.slaGlobalStatus.ToLower() == "danger" || app.slaInduvidualStatus.ToLower() == "danger")
                //{
                //    SlaNotification(app);
                //}
                
            }
            return apps;
        }


        private void SlaNotification(LoanApplicationViewModel app)
        {
            //+ " and product name " + app.proposedProductName.ToUpper()
            AlertsViewModel alert = new AlertsViewModel();
            if (app.toStaffId != null)
            {
                var ownerRecord = context.TBL_STAFF.Where(s => s.STAFFID == app.toStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME).FirstOrDefault();
                var alertTitle = "SLA/TRT BREACH ON LOAN APPLICATION NUMBER " + app.applicationReferenceNumber;
                var alertTemplate = "The transaction with reference number " + app.applicationReferenceNumber  + " which is currently with " + app.currentApprovalLevel + "(" + ownerRecord + ") SLA/TRT has been breach";
                string emailList = GetBusinessUsersEmailsToGroupHead(app.createdBy);

                var message = new TBL_MESSAGE_LOG()
                {
                    MESSAGESUBJECT = alertTitle,
                    MESSAGEBODY = alertTemplate,
                    MESSAGESTATUSID = 1,
                    MESSAGETYPEID = 1,
                    FROMADDRESS = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    TOADDRESS = emailList,
                    DATETIMERECEIVED = DateTime.Now,
                    SENDONDATETIME = DateTime.Now,
                    OPERATIONMETHOD = "SLABREACH"
                };

                context.TBL_MESSAGE_LOG.Add(message);
                context.SaveChanges();
            }
            else
            {
                if (app.currentApprovalLevelId != null)
                {
                    //+ " and product name " + app.proposedProductName.ToUpper()
                    var staffRole = context.TBL_APPROVAL_LEVEL.Where(r => r.APPROVALLEVELID == app.currentApprovalLevelId).Select(r => r.STAFFROLEID).FirstOrDefault();
                    var roleName = context.TBL_STAFF_ROLE.Where(n => n.STAFFROLEID == staffRole).Select(n => n.STAFFROLENAME).FirstOrDefault();
                    var alertTitle = "SLA/TRT BREACH ON LOAN APPLICATION NUMBER " + app.applicationReferenceNumber;
                    var alertTemplate = "The transaction with reference number " + app.applicationReferenceNumber  + " which is currently with " + app.currentApprovalLevel + "(" + roleName + ") SLA/TRT has been breach";
                    
                    string emailList = "";
                    var mailList = context.TBL_STAFF.Where(s => s.STAFFROLEID == staffRole).Select(s => s).ToList();
                    foreach (var t in mailList)
                    {
                        emailList = emailList + ";" + t.EMAIL;
                    }

                    var message = new TBL_MESSAGE_LOG()
                    {
                        MESSAGESUBJECT = alertTitle,
                        MESSAGEBODY = alertTemplate,
                        MESSAGESTATUSID = 1,
                        MESSAGETYPEID = 1,
                        FROMADDRESS = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        TOADDRESS = emailList,
                        DATETIMERECEIVED = DateTime.Now,
                        SENDONDATETIME = DateTime.Now,
                        OPERATIONMETHOD = "SLABREACH"
                    };

                    context.TBL_MESSAGE_LOG.Add(message);
                    context.SaveChanges();
                }
            }
        }

        private string GetBusinessUsersEmailsToGroupHead(int accountOfficerId)
        {
            string emailList = "";

            var accountOfficer = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficerId && x.DELETED == false).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }

        private string GetSlaInduvidualStatus(LoanApplicationViewModel app)
        {
                float sla = app.currentApprovalLevelSlaInterval;
                //int? elapse = (DateTime.Now - timeIn)?.Hours;
                int? elapse = (int)GetTimeIntervalHours(app.timeIn.Value, DateTime.Now);
                return SlaStatus(sla, elapse);
        }

        public string GetSlaGlobalStatus(LoanApplicationViewModel app)
        {
                float sla = app.globalsla;
                //int? elapse = (DateTime.Now - dateTimeCreated).Hours;
                int? elapse = (int)GetTimeIntervalHours(app.systemDateTime, DateTime.Now);
                return SlaStatus(sla, elapse);
        }

        private string SlaStatus(float sla, int? elapse)
        {
            if (sla == 0) return "success";
            if (elapse == 0 || elapse == null) return "success";
            float factor = (float)(elapse / sla) * 100;
            if (factor <= 30) return "success";
            if (factor <= 70) return "warning";
            if (factor <= 100) return "danger";
            return "danger";
        }

        public IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate.Date < startDate.Date)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
            yield return startDate;

            while (startDate.Date < endDate.Date && startDate.AddDays(1).Date < endDate.Date)
            {
                yield return new DateTime(startDate.AddDays(1).Year, startDate.AddDays(1).Month, startDate.AddDays(1).Day, 23, 59, 59);
                startDate = startDate.AddDays(1);
            }
            yield return endDate;
        }

        public bool IsInHolidays(DateTime date, int countryId)
        {
            List<TBL_PUBLIC_HOLIDAY> holidays;
            holidays = context.TBL_PUBLIC_HOLIDAY.ToList();
            var output = holidays.Any(x => x.DATE == date.Date);
            return output;
        }

        public IEnumerable<DateTime> FilterHolidaysFromDateIntervals(IEnumerable<DateTime> dateTimes)
        {
            var list = dateTimes.ToList();
            var countryId = context.TBL_COUNTRY.FirstOrDefault().COUNTRYID;
            list = list.FindAll(l => !IsInHolidays(l, countryId));
            return list;
        }

        public double GetTimeIntervalHours(DateTime startDate, DateTime endDate)
        {
            double hours = 0;
            var second = new TimeSpan(0, 0, 1);
            var range = GetDateRange(startDate, endDate);
            var test = range.ToList();
            range = FilterHolidaysFromDateIntervals(range);
            var intervals = range.Select(r => new DateTimeAndTimeOfDayViewModel
            {
                dateTime = r
            });
            var list = intervals.ToList();
            //dateTimeAndTimeOfDay = list;
            if (list.Count == 1)
            {
                var startHour = 8;
                var elapsed = list[0].dateTime;
                var elapsedHour = elapsed.Hour;
                if (elapsedHour > startHour)
                {
                    var elapsedWorkingHour = elapsedHour - startHour;
                    hours += elapsedWorkingHour;
                }
            }
            else
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    var elapsed = list[i + 1].dateTime.Subtract(list[i].dateTime);
                    if (elapsed.Days <= 1)
                    {
                        list[i + 1].timeOfDay = elapsed;
                        hours += list[i + 1].timeOfDay.TotalHours;
                    }
                    else
                    {
                        var elapsedDays = elapsed.Days * 24;
                        list[i + 1].timeOfDay = elapsed;
                        hours += (list[i + 1].timeOfDay.TotalHours - elapsedDays);
                    }
                }
            }
            return hours;
        }

        public IQueryable<LoanApplicationViewModel> GetPoolApplications(int operationId, int companyId, int branchId, int staffId, int? classId)
        {
            // var declarations
            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();
            List<int> levelIds = new List<int>();
            //List<int> levelIds2 = new List<int>();

            ExclusiveOperations.Add(operationId);
            foreach (var i in ExclusiveOperations)
            {
                levelIds.AddRange(general.GetStaffApprovalLevelIds(staffId, operationId).ToList());
            }

            var staffs = general.GetStaffRlieved(staffId);
            IQueryable<LoanApplicationViewModel> applications = null;

            var query = new List<LoanApplicationViewModel>();

            query = context.TBL_LOAN_APPLICATION.Where(x =>
                x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.COMPANYID == companyId
                && (classId == null) ? true : (x.PRODUCTCLASSID == (short?)classId)
                && x.ISADHOCAPPLICATION != true
            )
        .OrderByDescending(x => x.LOANAPPLICATIONID)
        .Join(
            context.TBL_APPROVAL_TRAIL.Where(x => (ExclusiveOperations.Contains(x.OPERATIONID) || ExclusiveOperations.Contains(x.DESTINATIONOPERATIONID ?? 0))
                && x.APPROVALSTATEID != (int)ApprovalState.Ended
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.RESPONSESTAFFID == null
                && levelIds.Contains((int)x.TOAPPROVALLEVELID)
                //&& (x.TOSTAFFID == null || x.TOSTAFFID == staffId)
                && (staffs.Contains((int)x.TOSTAFFID))
            ),
            a => a.LOANAPPLICATIONID,
            b => b.TARGETID,
            (a, b) => new { a, b })
        .Select(x => new LoanApplicationViewModel
        {
            //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
            loanApplicationId = x.a.LOANAPPLICATIONID,
            //loanApplicationDetailId = x.a.LOANAPPLICATIONID,
            applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
            relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
            customerId = x.a.CUSTOMERID,
            branchId = x.a.BRANCHID,
            currencyId = context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.CURRENCYID)
                                        .FirstOrDefault(),
            productClassId = x.a.PRODUCTCLASSID,
            productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

            customerGroupId = x.a.CUSTOMERGROUPID,
            loanTypeId = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
            relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
            relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
            applicationDate = x.a.APPLICATIONDATE,
            systemDateTime = x.a.SYSTEMDATETIME,
            applicationAmount = x.a.APPLICATIONAMOUNT,
            facility = x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
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
            responsiblePerson = (x.b.TOSTAFFID > 0) ? x.b.TBL_STAFF1.FIRSTNAME + " " + x.b.TBL_STAFF1.MIDDLENAME + " " + x.b.TBL_STAFF1.LASTNAME : "N/A",
            divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
            divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
            customerBusinessUnitId = context.TBL_CUSTOMER.Where(s => s.CUSTOMERID == x.a.CUSTOMERID).Select(c => c.BUSINESSUNTID).FirstOrDefault(),
            timeIn = x.b.SYSTEMARRIVALDATETIME,
            slaTime = x.b.SLADATETIME,

            loanInformation = x.a.LOANINFORMATION,
            submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
            customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
            isRelatedParty = x.a.ISRELATEDPARTY,
            isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
            approvalStatusId = x.b.APPROVALSTATUSID,
            applicationStatusId = x.a.APPLICATIONSTATUSID,
            branchName = x.a.TBL_BRANCH.BRANCHNAME,
            relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
            relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
            misCode = x.a.MISCODE,
            loanTypeName = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            createdBy = x.a.OWNEDBY,
            loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
            customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
            customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
            customerTypeId = x.a.LOANAPPLICATIONTYPEID,
            isInvestmentGrade = x.a.ISINVESTMENTGRADE,
            loantermSheetId = x.a.LOANTERMSHEETID,
            loantermSheetCode = x.a.TERMSHEETID,
            loansWithOthers = x.a.LOANSWITHOTHERS,
            ownershipStructure = x.a.OWNERSHIPSTRUCTURE,
            requireCollateral = x.a.REQUIRECOLLATERAL,
            regionId = x.a.CAPREGIONID,
            collateralDetail = x.a.COLLATERALDETAIL,
            isadhocapplication = x.a.ISADHOCAPPLICATION,
            requireCollateralTypeId = x.a.REQUIRECOLLATERALTYPEID,
            operationId = x.a.OPERATIONID,
            productClassProcessId = x.a.PRODUCT_CLASS_PROCESSID,
            tranchLevelId = x.a.TRANCHEAPPROVAL_LEVELID,


            globalsla = context.TBL_LOAN_APPLICATION_DETAIL
                                            .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                            .Select(s => s.TBL_PRODUCT1.TBL_PRODUCT_CLASS.GLOBALSLA)
                                            .FirstOrDefault(),
            currentApprovalLevelSlaInterval = x.b.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
            dateTimeCreated = x.a.DATETIMECREATED,
            apiRequestId = x.a.APIREQUESTID
        }).ToList();

            
            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications;

            //.Where(x=>x.originatorBusinessUnitId == loggedOnStaff.BUSINESSUNITID);//.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        public bool ChangeApplicationOwner(int loanApplicationId, int staffId, GeneralEntity model)
        {
            bool saved = false;
            using (var trans = context.Database.BeginTransaction())
            {
                if (loanApplicationId > 0)
                {
                    var systemDateNow = DateTime.Now;
                    var appl = context.TBL_LOAN_APPLICATION.Find(loanApplicationId);
                    var applForAudit = context.TBL_LOAN_APPLICATION.Find(loanApplicationId);
                    if (appl != null)
                    {
                        appl.OWNEDBY = staffId;
                        appl.RELATIONSHIPOFFICERID = staffId;
                        appl.LASTUPDATEDBY = model.createdBy;
                        appl.DATETIMEUPDATED = systemDateNow;
                    }

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApplicationReassigned,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Reassigning of ownership to staff with staffId: '{ staffId }'. Loan Application before reassigning '{applForAudit.ToString()}'",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(), // model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = systemDateNow,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.audit.AddAuditTrail(audit);
                    trans.Commit();
                }
            }

            return context.SaveChanges() > 0;
        }

        public bool SelfAssignMultpleApplication(List<ForwardViewModel> models, GeneralEntity userEntity)
        {
            bool response = false;
            foreach(var model in models)
            {
                if (model.trailId != null) { response = AssignApplication(model.trailId.Value, userEntity.createdBy, userEntity); }
            }
            return response;
        }

        public bool ReassignMultipleRequests(List<int> models, GeneralEntity userEntity, int staffId)
        {
            bool response = false;
            foreach (var model in models)
            {
                if (model > 0) { response = AssignApplication(model, staffId, userEntity); }
            }
            return response;
        }

        public bool AssignApplication(int approvalTrailId, int staffId, GeneralEntity model)
        {
            //bool saved = false;
            using (var trans = context.Database.BeginTransaction())
            {
                if (approvalTrailId > 0)
                {
                    var systemDateNow = DateTime.Now;
                    var trail = context.TBL_APPROVAL_TRAIL.Find(approvalTrailId);
                    var trailForAudit = context.TBL_APPROVAL_TRAIL.Find(approvalTrailId);
                    var trails = new List<TBL_APPROVAL_TRAIL>();
                    var trailsForAudit = new List<TBL_APPROVAL_TRAIL>();
                    var level = context.TBL_APPROVAL_LEVEL.Find(trail.TOAPPROVALLEVELID);
                    if (trail != null)
                    {
                        if (trail.FROMAPPROVALLEVELID == trail.TOAPPROVALLEVELID && trail.LOOPEDSTAFFID > 0)
                        {
                            trail.LOOPEDSTAFFID = staffId;
                            //trail.SYSTEMARRIVALDATETIME = systemDateNow;
                        }
                        else
                        {
                            trail.TOSTAFFID = staffId;
                            //trail.SYSTEMARRIVALDATETIME = systemDateNow;
                        }
                    }

                    trailsForAudit.Add(trailForAudit);
                    trailsForAudit.OrderByDescending(t => t.APPROVALTRAILID);

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApplicationReassigned,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Reassigning of Request to staff with staffId: '{ staffId }'. Trails before reassigning '{trailsForAudit.ToString()}'",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(), // model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = systemDateNow,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.audit.AddAuditTrail(audit);

                    trans.Commit();
                }
            }

            return context.SaveChanges() > 0;
        }

        public bool ReturnAssignApplicationToPool(int approvalTrailId, GeneralEntity model)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                if (approvalTrailId > 0)
                {
                    var systemDateNow = DateTime.Now;
                    var trail = context.TBL_APPROVAL_TRAIL.Find(approvalTrailId);
                    var trailForAudit = context.TBL_APPROVAL_TRAIL.Find(approvalTrailId);
                    var trails = new List<TBL_APPROVAL_TRAIL>();
                    TBL_STAFF staff = new TBL_STAFF();

                    var level = context.TBL_APPROVAL_LEVEL.Find(trail.TOAPPROVALLEVELID);
                    if (trail != null)
                    {
                        staff = context.TBL_STAFF.Find(trail.TOSTAFFID);
                        trail.TOSTAFFID = null;
                    }

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApplicationReassigned,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Transaction that was previously assigned to {staff?.FIRSTNAME} {staff?.LASTNAME} {staff.STAFFCODE} was returned to general pool {level.LEVELNAME}.",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(), 
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = systemDateNow,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    this.audit.AddAuditTrail(audit);

                    trans.Commit();
                }
            }

            return context.SaveChanges() > 0;
        }
        public List<PendingProductProgramViewModel> GetPendingProductProgram(UserInfo user)
        {
            int staffId = user.staffId;
            int operationId = (int)OperationsEnum.CreditAppraisal;
            var levelIds = general.GetStaffApprovalLevelIds(user.staffId, operationId);// new int[] {3,1,5};
            int productBasedId = (int)ProductClassProcessEnum.ProductBased;
            //int[] productBasedIds = { (int)ProductClassProcessEnum.ProductBased, (int)ProductClassProcessEnum.CAMBased };

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

            //var productClasses = context.TBL_PRODUCT_CLASS.Where(x => productBasedIds.Contains(x.PRODUCT_CLASS_PROCESSID))
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
            int operationId = (int)OperationsEnum.CreditAppraisal;

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
            var operationId = (int)OperationsEnum.CreditAppraisal;

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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(), // model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            bool response = (context.SaveChanges() > 0) == result;

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.OfferLetterGenerationInProgress;
                workflow.NextProcess(appl.COMPANYID, model.createdBy, (int)OperationsEnum.OfferLetterApproval, appl.FLOWCHANGEID, model.applicationId, null, "New pproved application", true, false, false, model.isFlowTest,appl.TBL_CUSTOMER.BUSINESSUNTID);
            }

            return response;
        }

        public IQueryable<RegionLoanApplicationViewModel> GetRegionalLoanApplications(int staffId)
        { 

            var operationIds = context.TBL_LOAN_APPLICATION.Select(x => x.OPERATIONID).Distinct().ToList();
            //var operationId = (int)OperationsEnum.CreditAppraisal;

            //List<int> levels = general.GetRouteLevels(operationId, 1);//.ToList().Distinct();

            //var branches = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
            //                .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
            //                .Join(context.TBL_BRANCH, sr => sr.r.REGIONID, b => b.REGIONID, (sr, b) => new { sr, b })
            //                .Select(x => new {
            //                    BRANCHID = x.b.BRANCHID
            //                })
            //                .Select(x => x.BRANCHID)
            //                .ToList();

            //var regions = context.TBL_BRANCH_REGION_STAFF.Where(x => x.STAFFID == staffId)
            //                .Join(context.TBL_BRANCH_REGION, s => s.REGIONID, r => r.REGIONID, (s, r) => new { s, r })
            //                .Select(x => new
            //                {
            //                    REGIONID = x.r.REGIONID
            //                })
            //                .Select(x => x.REGIONID)
            //                .ToList();

            var applications = context.TBL_LOAN_APPLICATION
                .Where(x => x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress 
                            && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                    //&& regions.Contains((int)x.CAPREGIONID)
                    // && branches.Contains(x.BRANCHID)
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                    && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                    && x.SUBMITTEDFORAPPRAISAL == true
                )
                .OrderByDescending(x => x.LOANAPPLICATIONID)
                .Join(context.TBL_APPROVAL_TRAIL.Where(x => operationIds.Contains(x.OPERATIONID)
                        && (x.RESPONSESTAFFID == null || (x.RESPONSESTAFFID == staffId && x.TOSTAFFID != null))
                        && x.TOAPPROVALLEVELID != null 
                        //&& levels.Contains((int)x.TOAPPROVALLEVELID)
                        ),
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


        public IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggersByOperationId(int operationId,int applicationDetailId)
        {
            return context.TBL_LOAN_MONITORING_TRIG_SETUP
                .Where(x => x.OPERATIONID==operationId)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = applicationDetailId,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER_NAME,
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
            workflow.OperationId = (int)OperationsEnum.CreditAppraisal;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "flow_test";
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return true;
        }

        public ApprovalTrailViewModel GetapprovalTrailByTrailId(int approvalTrailId)
        {
            if (approvalTrailId > 0)
            {
                var trail = context.TBL_APPROVAL_TRAIL.Find(approvalTrailId);
                var data = new ApprovalTrailViewModel
                {
                    approvalTrailId = trail.APPROVALTRAILID,
                    //comment = trail.COMMENT,
                    targetId = trail.TARGETID,
                    operationId = trail.OPERATIONID,
                    //arrivalDate = trail.ARRIVALDATE,
                    //systemArrivalDateTime = trail.SYSTEMARRIVALDATETIME,
                    //responseDate = trail.RESPONSEDATE,
                    //systemResponseDateTime = trail.SYSTEMRESPONSEDATETIME,
                    //responseStaffId = trail.RESPONSESTAFFID,
                    requestStaffId = trail.REQUESTSTAFFID,
                    fromApprovalLevelId = trail.FROMAPPROVALLEVELID,
                    //fromApprovalLevelName = trail.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == trail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    //toApprovalLevelName = trail.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                    toApprovalLevelId = trail.TOAPPROVALLEVELID,
                    loopedStaffId = trail.LOOPEDSTAFFID,
                    //approvalStateId = trail.APPROVALSTATEID,
                    //approvalStatusId = trail.APPROVALSTATUSID,
                    //approvalState = trail.TBL_APPROVAL_STATE.APPROVALSTATE,
                    //approvalStatus = trail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                    ////applicationId = application.LOANAPPLICATIONID,
                    //commentStage = "Credit Appaisal",
                    //toStaffName = allstaff.FirstOrDefault(s => s.id == trail.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == trail.RESPONSESTAFFID).name,
                    //fromStaffName = allstaff.FirstOrDefault(s => s.id == trail.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == trail.REQUESTSTAFFID).name,
                };

                if (data.fromApprovalLevelId == data.toApprovalLevelId && data.loopedStaffId > 0)
                {
                    var loopedStaff = context.TBL_STAFF.Find(data.loopedStaffId);
                    if(loopedStaff == null)
                    {
                        throw new SecureException("Looped Staff Can't be null!");
                    }
                    var defaultLoopedStaffLevelId = context.TBL_APPROVAL_LEVEL.FirstOrDefault(l => l.STAFFROLEID == loopedStaff.STAFFROLEID).APPROVALLEVELID;
                    data.toApprovalLevelId = defaultLoopedStaffLevelId;
                }
            return data;
            }
            return null;
        }


        public IEnumerable<RepaymentScheduleTermsViewModel> SaveRepaymentScheduleAndTerms(RepaymentScheduleTermsViewModel model)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.applicationDetailId);
            if (detail != null)
            {
                detail.REPAYMENTTERMS = model.terms;
                detail.REPAYMENTSCHEDULEID = model.repaymentScheduleId;
                context.SaveChanges();
            }
            return context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == detail.LOANAPPLICATIONID && x.DELETED == false)
                .Select(x => new RepaymentScheduleTermsViewModel
                {
                    applicationDetailId = x.LOANAPPLICATIONDETAILID,
                    terms = x.REPAYMENTTERMS,
                    repaymentScheduleId = (int)x.REPAYMENTSCHEDULEID,
                    schedule = context.TBL_REPAYMENT_TERM.FirstOrDefault(r => r.REPAYMENTSCHEDULEID == x.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                    productCustomerName = x.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME
                }).ToList();

            //return new List<RepaymentScheduleTermsViewModel>();
        }

        public IEnumerable<RepaymentScheduleTermSetupViewModel> GetAllSetupRepaymentTerms()
        {
            var terms = context.TBL_REPAYMENT_TERM.Select(t =>
                new RepaymentScheduleTermSetupViewModel
                {
                    repaymentScheduleId = t.REPAYMENTSCHEDULEID,
                    repaymentScheduleDetail = t.REPAYMENTTERMDETAIL,
                }).ToList();
            // todo code
            return terms;
        }

        public string GetAllOldApplicationReference(string data)
        {
            var terms = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == data).Select(x => x.APPLICATIONREFERENCENUMBER).FirstOrDefault();
            // todo code
            return terms;
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

        public List<RecommendedCollateralViewModel> GetRecommendedCollateral(int applicationId, int staffId)
        {
            return context.TBL_LOAN_APPLICATION_COLLATRL2.Where(x => x.LOANAPPLICATIONID == applicationId)
                .Select(x => new RecommendedCollateralViewModel
                {
                    owner = x.CREATEDBY == staffId,
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
            return GetRecommendedCollateral(entity.applicationId,entity.createdBy);
        }


        public bool AddContractorTiering(ContractorTieringViewModel contractorCriteria)
        {
            List<TBL_CONTRACTOR_CRITERIA> definitions = new List<TBL_CONTRACTOR_CRITERIA>();
            var msg = new ContractorTieringViewModel();
            if (contractorCriteria.form == null || contractorCriteria.form.Count == 0) return false;
            var ids = contractorCriteria.form.Select(x => x.criteriaId);
            definitions = context.TBL_CONTRACTOR_CRITERIA.Where(x => ids.Contains(x.CRITERIAID)
              ).ToList();
            var submission = new ContractorCriteriaFormControlValue();

            List<TBL_CONTRACTOR_TIERING> details = new List<TBL_CONTRACTOR_TIERING>();

            var validateExisting = context.TBL_CONTRACTOR_TIERING.Where(c => c.LOANAPPLICATIONID == contractorCriteria.loanApplicationId && c.CUSTOMERID == contractorCriteria.customerId).ToList();
            if (validateExisting != null && validateExisting.Count() > 0)
            {
                for (int i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    submission = contractorCriteria.form.FirstOrDefault(x => x.criteriaId == definition.CRITERIAID);
                    if (submission == null) continue;
                    var updateRecord = context.TBL_CONTRACTOR_TIERING.Where(c => c.CONTRACTORCRITERIAID == definition.CRITERIAID && c.LOANAPPLICATIONID == contractorCriteria.loanApplicationId).FirstOrDefault();
                    updateRecord.ACTUALVALUE = submission.value;
                    updateRecord.CONTRACTORCRITERIAID = submission.criteriaId;
                    updateRecord.DATETIMEUPDATED = DateTime.Now;
                    updateRecord.LASTUPDATEDBY = contractorCriteria.createdBy;
                }
                if (context.SaveChanges() > 0) return true;

                return false;
            }
            else
            {
                try
                {
                    for (int i = 0; i < definitions.Count; i++)
                    {
                        var definition = definitions[i];
                        submission = contractorCriteria.form.FirstOrDefault(x => x.criteriaId == definition.CRITERIAID);
                        if (submission == null) continue;
                        details.Add(new TBL_CONTRACTOR_TIERING
                        {
                            LOANAPPLICATIONID = contractorCriteria.loanApplicationId,
                            CUSTOMERID = contractorCriteria.customerId,
                            CONTRACTORCRITERIAID = submission.criteriaId,
                            ACTUALVALUE = submission.value,
                            CREATEDBY = contractorCriteria.createdBy,
                            DATETIMECREATED = DateTime.Now,
                        });

                    }

                    context.TBL_CONTRACTOR_TIERING.AddRange(details);
                    if(context.SaveChanges()>0) return true;

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
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
            return GetRecommendedCollateral(entity.applicationId,entity.createdBy);
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
            detail.REPAYMENTSCHEDULEID = entity.repaymentScheduleId;
            context.SaveChanges();
            return context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == detail.LOANAPPLICATIONID)
                .Select(x => new RepaymentScheduleTermsViewModel
                {
                    applicationDetailId = x.LOANREVIEWAPPLICATIONID,
                    terms = x.REPAYMENTTERMS,
                    repaymentScheduleId = x.REPAYMENTSCHEDULEID,
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
                    repaymentScheduleId = x.d.REPAYMENTSCHEDULEID,
                    schedule = context.TBL_REPAYMENT_TERM.Where(t => t.REPAYMENTSCHEDULEID == x.d.REPAYMENTSCHEDULEID).FirstOrDefault().REPAYMENTTERMDETAIL,
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

            
            //if (((result.limit == 0) || ((double)amount + result.outstandingBalance) <= result.outstandingBalance) == false)
            //    throw new SecureException("Customer limit validation failed!");
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
                            email = c.EMAILADDRESS.Trim(),
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
                            email = c.EMAILADDRESS.Trim(),
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            approvalStatusId = a.APPROVALSTATUSID
                        }).Distinct().ToList();

            foreach (var customer in data)
            {
                string referenceNo = customer.applicationReferenceNumber;

                var failedEmailBody = "Dear Valuable Customer, <br /><br /> Your facility application with Reference Number : " + referenceNo + " has been disapproved,<br /> Kindly contact your Account Officer and collect your Offer Letter.";
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
                        <td>{string.Format("{0:#,##.00}", Convert.ToDecimal(f.APPROVEDAMOUNT))}</td>
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
            int operationId = (int)OperationsEnum.CreditAppraisal;
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

        public WorkflowResponse GetWorkflowNextStatusLms(ForwardReviewViewModel model)
        {
            var applicationDate = general.GetApplicationDate();
            var appl = context.TBL_LMSR_APPLICATION.Find(model.applicationId);

            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = appl.COMPANYID;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.Amount = model.totalExposureAmount; //model.amount;
            workflow.Tenor = model.applicationTenor;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return workflow.Response;
        }

        public ProjectRiskRatingViewModel AddProjectRiskRating(ProjectRiskRatingViewModel projectRiskRating)
        {
            var validateExisting = context.TBL_PROJECT_RISK_RATING.Where(c => c.LOANAPPLICATIONID == projectRiskRating.loanApplicationId && c.LOANAPPLICATIONDETAILID == projectRiskRating.loanApplicationDetailId).ToList();
            if (validateExisting != null && validateExisting.Count() > 0)
            {
                throw new SecureException("Sorry project risk rating already captured");
            }
            try
            {
                List<TBL_PROJECT_RISK_RATING_CATEGORY> definitions = new List<TBL_PROJECT_RISK_RATING_CATEGORY>();
                var msg = new ProjectRiskRatingViewModel();
                if (projectRiskRating.form == null || projectRiskRating.form.Count == 0) return null;
                var ids = projectRiskRating.form.Select(x => x.categoryId);

                definitions = context.TBL_PROJECT_RISK_RATING_CATEGORY.Where(x => ids.Contains(x.CATEGORYID)
                ).ToList();

                var submission = new ProjectRistratingFormControlValue();

                List<TBL_PROJECT_RISK_RATING> details = new List<TBL_PROJECT_RISK_RATING>();

                for (int i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    submission = projectRiskRating.form.FirstOrDefault(x => x.categoryId == definition.CATEGORYID);
                    if (submission == null) continue;
                    details.Add(new TBL_PROJECT_RISK_RATING
                    {
                        LOANAPPLICATIONID = projectRiskRating.loanApplicationId,
                        LOANAPPLICATIONDETAILID = projectRiskRating.loanApplicationDetailId,
                        LOANBOOKINGREQUESTID = projectRiskRating.loanBookingRequestId,
                        CATEGORYID = submission.categoryId,
                        CATEGORYVALUE = submission.value,
                        CREATEDBY = projectRiskRating.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        PROJECTLOCATION = projectRiskRating.projectLocation,
                        PROJECTDETAILS = projectRiskRating.projectDetails,
                    });

                }

                context.TBL_PROJECT_RISK_RATING.AddRange(details);
                context.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }


}