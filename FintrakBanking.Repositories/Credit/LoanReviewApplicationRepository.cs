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
    public class LoanReviewApplicationRepository : ILoanReviewApplicationRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;

        public LoanReviewApplicationRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetApplications(UserInfo user, int operationId, int? classId)
        {
            // var declarations
            int staffId = user.staffId;
            int branchId = user.BranchId;
            int companyId = user.companyId;
            IQueryable<LoanReviewApplicationViewModel> applications = null;
            bool isHeadOffice = (branchId == 1) ? true : false;
            bool screenCanViewAll = operationId == (int)OperationsEnum.LoanReviewApprovalApplication;

            // get approval levels 
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId);

            // query
            applications = context.TBL_LOAN_REVIEW_APPLICATION.Where(x =>
                    (x.BRANCHID == branchId || isHeadOffice) // branch filter
             )
            .Join(context.TBL_LOAN, a => a.LOANID, l => l.TERMLOANID, (a, l) => new { a, l })
            .GroupJoin(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId),
                al => al.a.LOANREVIEWAPPLICATIONID,
                t => t.TARGETID,
                (al, t) => new { a = al.a, l = al.l, t })
            .SelectMany(
                xy => xy.t.DefaultIfEmpty(),
                (x, t) => new LoanReviewApplicationViewModel
                {
                    loanReviewApplicationId = x.a.LOANREVIEWAPPLICATIONID,
                    applicationDate = x.a.DATECREATED,
                    operationTypeId = x.a.OPERATIONID,
                    operationType = context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == x.a.OPERATIONID).OPERATIONNAME,
                    referenceNumber = x.l.LOANREFERENCENUMBER,
                    approvalState = x.l.APPROVALSTATUSID == 0 ? "" : context.TBL_APPROVAL_STATUS.FirstOrDefault(k => k.APPROVALSTATUSID == x.a.APPROVALSTATUSID).APPROVALSTATUSNAME,
                    //principalAmount = x.l.PRINCIPALAMOUNT,
                    //effectiveDate = x.l.EFFECTIVEDATE,
                    //maturityDate = x.l.MATURITYDATE,
                    //interestRate = x.l.INTERESTRATE,

                    loanId = x.a.LOANID,

                    lastComment = t.COMMENT,
                    currentStage = t == null ? "N/A" : context.TBL_OPERATIONS.FirstOrDefault(s => s.OPERATIONID == t.OPERATIONID).OPERATIONNAME,
                    currentApprovalStateId = t == null ? (short)0 : t.APPROVALSTATEID,
                    currentApprovalState = t.TBL_APPROVAL_STATE.APPROVALSTATE,
                    currentApprovalLevelId = t.TOAPPROVALLEVELID,
                    currentApprovalLevel = t.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                    approvalTrailId = t == null ? 0 : t.APPROVALTRAILID, // for inner sequence ordering
                    toStaffId = t.TOSTAFFID,

                    approvalStatusId = x.a.APPROVALSTATUSID,
                    approvalStatus = x.a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME, // --------------- open after rel added and scaaffold
                    customerId = x.l.CUSTOMERID,
                    customerName = x.l.TBL_CUSTOMER.FIRSTNAME + " " + x.l.TBL_CUSTOMER.MIDDLENAME + " " + x.l.TBL_CUSTOMER.LASTNAME,

                    branchId = x.a.BRANCHID,
                    branchName = x.a.TBL_BRANCH.BRANCHNAME, // -------------------- open after scaffold
                    createdBy = x.a.CREATEDBY,
                })
                .GroupBy(d => d.loanReviewApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
                .OrderByDescending(x => x.applicationDate)
                .ThenByDescending(x => x.loanReviewApplicationId)
                ;

            //var list = applications.ToList();
            //var count = applications.Count();
            //var levs = levelIds.ToList();

            if (screenCanViewAll) { return applications; };

            return applications.Where(x => levelIds.Contains((int)x.currentApprovalLevelId) && (x.toStaffId == null || x.toStaffId == staffId));
        }

        //private IQueryable<int> GetStaffApprovalLevelIds(int staffId, int operationId)
        //{
        //    int scope = (int)ProcessViewScopeEnum.Level; // default 1

        //    var allLevels = context.TBL_APPROVAL_GROUP_MAPPING
        //        .Where(x => x.OPERATIONID == operationId)
        //        .Select(g => g.TBL_APPROVAL_GROUP)
        //        .SelectMany(x => x.TBL_APPROVAL_LEVEL
        //        .Where(l => l.ISACTIVE == true));

        //    var staffWorkflow = allLevels.SelectMany(l => l.TBL_APPROVAL_LEVEL_STAFF).Where(x => x.STAFFID == staffId);

        //    if (staffWorkflow.Count() > 0) scope = staffWorkflow.Max(x => x.PROCESSVIEWSCOPEID);

        //    if (scope == 3) return allLevels.Select(x => x.APPROVALLEVELID).Distinct();

        //    var staffLevels = staffWorkflow.Select(x => x.APPROVALLEVELID).Distinct();

        //    if (scope == 2)
        //    {
        //        var groups = context.TBL_APPROVAL_LEVEL.Where(x => staffLevels.Contains(x.APPROVALLEVELID)).Select(x => x.GROUPID).Distinct();
        //        return context.TBL_APPROVAL_LEVEL.Where(x => groups.Contains(x.GROUPID)).Select(x => x.APPROVALLEVELID).Distinct();
        //    }

        //    return staffLevels;
        //}

        public SelectListViewModel GetAllSelectList()
        {
            var list = new SelectListViewModel();

            var frequency = context.TBL_FREQUENCY_TYPE.Select(x => new DropDownSelect { id = x.FREQUENCYTYPEID, name = x.MODE }).ToList();

            list.interestFrequencyTypes = frequency;
            list.principalFrequencyTypes = frequency;

            list.casaAccounts = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.productTypes = context.TBL_PRODUCT_TYPE.Select(x => new DropDownSelect { id = x.PRODUCTTYPEID, name = x.PRODUCTTYPENAME }).ToList();
            list.operationTypes = context.TBL_OPERATIONS.Where(x => x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagement || x.OPERATIONTYPEID == (int)OperationTypeEnum.LoanManagementOverdraft).Select(x => new DropDownSelect { id = x.OPERATIONID, name = x.OPERATIONNAME }).OrderBy(o=>o.name).ToList();

            return list;
        }

        public bool SubmitLoanReviewApplication(LoanReviewApplicationViewModel model)
        {
            var application = new TBL_LOAN_REVIEW_APPLICATION
            {
                LOANID = model.loanId,
                PRODUCTTYPEID = 1, // 1. termloan
                OPERATIONID = model.operationTypeId,
                REVIEWDETAILS = model.reviewDetails,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                CREATEDBY = model.createdBy,
                BRANCHID = model.branchId,
                DATECREATED = general.GetApplicationDate(),
            };

            context.TBL_LOAN_REVIEW_APPLICATION.Add(application);

            // ------------AUDIT CODE HERE! -------------

            if (context.SaveChanges() == 0) return false;

            PassApplicationToOperation(application.LOANREVIEWAPPLICATIONID, (int)OperationsEnum.LoanReviewApprovalAppraisal, model.createdBy, "New loan review application");

            return true;
        }

        private void PassApplicationToOperation(int applicationId, int operationId, int staffId, string comment)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            workflow.StaffId = staffId;
            workflow.CompanyId = staff.COMPANYID;
            workflow.OperationId = operationId;
            workflow.TargetId = applicationId;
            workflow.ProductClassId = null;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = comment;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = false;
            workflow.LogActivity();
        }

        public List<LoanViewModel> LoanSearch(int getCompanyId, SearchViewModel search)
        {
            IQueryable<LoanViewModel> loans = null;

            loans = context.TBL_LOAN.Where(x=> x.ISDISBURSED == true).Select(l => new LoanViewModel
                     {
                         loanId = l.TERMLOANID,
                         customerId = l.CUSTOMERID,
                         customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
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
                         casaAccountNumber = l.TBL_CASA.PRODUCTACCOUNTNAME,
                         productAccountName = l.TBL_PRODUCT.PRODUCTNAME,
                         subSectorName = l.TBL_SUB_SECTOR.NAME,
                         sectorName = l.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                         customerGroupId = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,
                         loanTypeId = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID,
                         loanTypeName = l.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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
                         isCamsol = context.TBL_LOAN_CAMSOL.Any(x => x.LOANID == l.TERMLOANID),
                         productName = l.TBL_PRODUCT.PRODUCTNAME
                     });

            if (!String.IsNullOrEmpty(search.searchString))
            {
                loans = loans.Where(x =>
                x.customerName.ToLower().Contains(search.searchString.ToLower())
                || x.loanReferenceNumber.ToLower().Contains(search.searchString.ToLower())
                || x.casaAccountNumber.ToLower().Contains(search.searchString.ToLower())
                );
            }

            return loans.ToList();
        }

        public int SaveCam(CamViewModel model)
        {
            string finalAction = "Updated";
            var cam = new TBL_LOAN_REVIEW_APPLICATN_CAM();

            // check of memo exist for level
            var memo = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Where(x => 
                x.LOANREVIEWAPPLICATIONID == model.applicationId
                && x.APPROVALLEVELID == model.approvalLevelId
            );

            // if force new 
            // if null, create new for level
            if (memo.Any() == false || model.createNew == true)
            {
                cam = new TBL_LOAN_REVIEW_APPLICATN_CAM
                {
                    DOCUMENTATION = model.createNew ? "<p></p>" : model.documentation,
                    LOANREVIEWAPPLICATIONID = model.applicationId,
                    APPROVALLEVELID = model.approvalLevelId,
                    CAMREF = model.referenceNumber,
                    COMPANYID = model.companyId, // NN
                    ISCOMPLETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now
                };

                context.TBL_LOAN_REVIEW_APPLICATN_CAM.Add(cam);
                finalAction = "Added";
            }
            else
            {
                // if exist update for level
                cam = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Find(model.documentationId);
                if (cam == null) cam = memo.OrderByDescending((x => x.LOANREVIEWCAMID)).FirstOrDefault();
                cam.DOCUMENTATION = model.documentation;
                cam.LASTUPDATEDBY = model.lastUpdatedBy;
                cam.DATETIMEUPDATED = general.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = finalAction == "Added" ? (short)AuditTypeEnum.AppraisalMemorandumAdded : (short)AuditTypeEnum.AppraisalMemorandumUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"'{ finalAction }' Appraisal Memorandum Document'{ model.referenceNumber }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0 ? cam.LOANREVIEWCAMID : 0;
        }

        public List<CamViewModel> GetCamDocuments(int applicationId)
        {
            return context.TBL_LOAN_REVIEW_APPLICATN_CAM
                .Where(x => x.LOANREVIEWAPPLICATIONID == applicationId)
                .Select(cam => new CamViewModel
                {
                    documentationId = cam.LOANREVIEWCAMID,
                    documentation = cam.DOCUMENTATION,
                    approvalLevelId = cam.APPROVALLEVELID,
                    applicationId = cam.LOANREVIEWAPPLICATIONID,
                    referenceNumber = cam.CAMREF,
                }).ToList();
        }

        public CamViewModel GetCamDocumentByApprovalLevel(int applicationId, int approvalLevelId)
        {
            var cams = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Where(x =>
                x.LOANREVIEWAPPLICATIONID == applicationId
                //&& x.APPROVALLEVELID == approvalLevelId
            );

            if (cams.Any() == false) return new CamViewModel();

            TBL_LOAN_REVIEW_APPLICATN_CAM cam;

            if (cams.Any(x => x.APPROVALLEVELID == approvalLevelId) == true)
                cam = cams.Where(x => x.APPROVALLEVELID == approvalLevelId).OrderByDescending(x => x.LOANREVIEWCAMID).FirstOrDefault();
            else
                cam = cams.OrderByDescending(x => x.LOANREVIEWCAMID).FirstOrDefault();

            return new CamViewModel
            {
                documentationId = cam.LOANREVIEWCAMID,
                documentation = cam.DOCUMENTATION,
                approvalLevelId = cam.APPROVALLEVELID,
                applicationId = cam.LOANREVIEWAPPLICATIONID,
                referenceNumber = cam.CAMREF,
            };
        }

        public CamViewModel GetCamDocument(int documentationId)
        {
            var cam = context.TBL_LOAN_REVIEW_APPLICATN_CAM.Find(documentationId);
            if (cam == null) return new CamViewModel();

            return new CamViewModel
            {
                documentationId = cam.LOANREVIEWCAMID,
                documentation = cam.DOCUMENTATION,
                approvalLevelId = cam.APPROVALLEVELID,
                applicationId = cam.LOANREVIEWAPPLICATIONID,
                referenceNumber = cam.CAMREF,
            };
        }

        public int ForwardApplication(ForwardReviewViewModel model)
        {
            workflow.StaffId = model.lastUpdatedBy;
            workflow.CompanyId = model.companyId;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.ProductClassId = null;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            context.SaveChanges();

            var appl = context.TBL_LOAN_REVIEW_APPLICATION.Find(model.applicationId);
            int lastOperationId = (int)OperationsEnum.LoanReviewApprovalAvailment;

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved && model.operationId != lastOperationId) // jump process OR end flag
                {
                    PassApplicationToOperation(model.applicationId, model.operationId + 1, model.lastUpdatedBy, "New application");
                }
                appl.APPROVALSTATUSID = (short)workflow.StatusId;
                context.SaveChanges();
                return workflow.StatusId;
            }

            if (model.operationId == (int)OperationsEnum.LoanReviewApprovalAppraisal)
            {
                appl.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                context.SaveChanges();
            }

            return (int)ApprovalStatusEnum.Processing; // default for now
        }
    }
}
