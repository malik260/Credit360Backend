using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.CustomException;
using System.Data.Entity;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalLevelRepository : IApprovalLevelRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workflow;
        private IAdminRepository admin;

        public ApprovalLevelRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail,
              IWorkflow _workflow,
              IAdminRepository _admin
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.workflow = _workflow;
            this.admin = _admin;
        }

        private IEnumerable<ApprovalLevelViewModel> GetApprovalLevel(int companyId)
        {
            var data = (from x in this.context.TBL_APPROVAL_LEVEL
                        where x.DELETED == false && x.TBL_APPROVAL_GROUP.COMPANYID == companyId
                        select new ApprovalLevelViewModel
                        {
                            approvalLevelId = x.APPROVALLEVELID,
                            levelName = x.LEVELNAME,
                            position = x.POSITION,
                            tenor = x.TENOR,
                            maximumAmount = x.MAXIMUMAMOUNT,
                            investmentGradeAmount = x.INVESTMENTGRADEAMOUNT,
                            standardGradeAmount = x.STANDARDGRADEAMOUNT,
                            renewalLimit = x.RENEWALLIMIT,
                            feeRate = x.FEERATE,
                            interestRate = x.INTERESTRATE,
                            numberOfUsers = x.NUMBEROFUSERS,
                            numberOfApprovals = x.NUMBEROFAPPROVALS,
                            slaInterval = x.SLAINTERVAL,
                            //canRouteBack = x.CANROUTEBACK,
                            isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                            canEscalate = x.CANESCALATE,
                            canApproveUntenored = x.CANAPPROVEUNTENORED,
                            canResolveDispute = x.CANRESOLVEDISPUTE,
                            isActive = x.ISACTIVE,
                            slaNotificationInterval = x.SLANOTIFICATIONINTERVAL,
                            canViewDocument = x.CANVIEWDOCUMENT,
                            canEdit = x.CANEDIT,
                            canViewUploadedFile = x.CANVIEWUPLOAD,
                            canUploadFile = x.CANUPLOAD,
                            canViewApproval = x.CANVIEWAPPROVAL,
                            canApprove = x.CANAPPROVE,
                            //canDoRiskAssessment = x.CANDORISKASSESSMENT,
                            //canRecieveAdjustment = x.CANRECIEVEADJUSTMENT,
                            canRecieveEmail = x.CANRECIEVEEMAIL,
                            canRecieveSms = x.CANRECIEVESMS,
                            //hasChecklist = x.HASCHECKLIST,
                            //canPerformFinancialAnalysis = x.CANPERFORMFINANCIALANALYSIS,
                            //requireAuthorisation = x.REQUIREAUTHORISATION,
                            //canOverideAuthorisation = x.CANOVERIDEAUTHORISATION,
                            routeViaStaffOrganogram = x.ROUTEVIASTAFFORGANOGRAM,
                            createdBy = x.CREATEDBY,
                            dateTimeCreated = x.DATETIMECREATED,
                            dateTimeUpdated = x.DATETIMEUPDATED,
                            deleted = x.DELETED,
                            deletedBy = x.DELETEDBY,
                            dateTimeDeleted = x.DATETIMEDELETED,
                            groupId = (int)x.GROUPID,
                            roleId = x.STAFFROLEID,
                            levelTypeId = x.LEVELTYPEID,
                            levelBusinessRuleId = x.APPROVALBUSINESSRULEID,
                            roleIdToRoute = x.ROLEIDTOROUTE,
                            isPostApprovalReviewer = x.ISPOSTAPPROVALREVIEWER,
                            ignoreIfApprovalLevelStaff = x.IGNOREIFAPPROVALLEVELSTAFF

                        }).OrderBy(x => x.position).ToList();

            return data;
        }

        private IEnumerable<ApprovalLevelViewModel> GetAllDetailedApprovalLevel(int companyId)
        {
            var data = (from a in context.TBL_APPROVAL_LEVEL
                        join c in context.TBL_APPROVAL_GROUP on a.GROUPID equals c.GROUPID
                        join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                        where a.DELETED == false && a.TBL_APPROVAL_GROUP.COMPANYID == companyId
                        select new ApprovalLevelViewModel
                        {
                            approvalLevelId = a.APPROVALLEVELID,
                            levelName = a.LEVELNAME,
                            position = a.POSITION,
                            tenor = a.TENOR,
                            maximumAmount = a.MAXIMUMAMOUNT,
                            investmentGradeAmount = a.INVESTMENTGRADEAMOUNT,
                            standardGradeAmount = a.STANDARDGRADEAMOUNT,
                            renewalLimit = a.RENEWALLIMIT,
                            feeRate = a.FEERATE,
                            interestRate = a.INTERESTRATE,
                            numberOfUsers = a.NUMBEROFUSERS,
                            numberOfApprovals = a.NUMBEROFAPPROVALS,
                            slaInterval = a.SLAINTERVAL,
                            //canRouteBack = a.CANROUTEBACK,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            canEscalate = a.CANESCALATE,
                            canApproveUntenored = a.CANAPPROVEUNTENORED,
                            canResolveDispute = a.CANRESOLVEDISPUTE,
                            isActive = a.ISACTIVE,
                            isPostApprovalReviewer = a.ISPOSTAPPROVALREVIEWER,
                            ignoreIfApprovalLevelStaff = a.IGNOREIFAPPROVALLEVELSTAFF,
                            canViewDocument = a.CANVIEWDOCUMENT,
                            canEdit = a.CANEDIT,
                            canViewUploadedFile = a.CANVIEWUPLOAD,
                            canUploadFile = a.CANUPLOAD,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            slaNotificationInterval = a.SLANOTIFICATIONINTERVAL,
                            //canDoRiskAssessment = a.CANDORISKASSESSMENT,
                            //canRecieveAdjustment = a.CANRECIEVEADJUSTMENT,
                            canRecieveEmail = a.CANRECIEVEEMAIL,
                            canRecieveSms = a.CANRECIEVESMS,
                            //hasChecklist = a.HASCHECKLIST,
                            //canPerformFinancialAnalysis = a.CANPERFORMFINANCIALANALYSIS,
                            //requireAuthorisation = a.REQUIREAUTHORISATION,
                            //canOverideAuthorisation = a.CANOVERIDEAUTHORISATION,
                            routeViaStaffOrganogram = a.ROUTEVIASTAFFORGANOGRAM,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATETIMECREATED,
                            dateTimeUpdated = a.DATETIMEUPDATED,
                            deleted = a.DELETED,
                            deletedBy = a.DELETEDBY,
                            dateTimeDeleted = a.DATETIMEDELETED,
                            groupId = (int)a.GROUPID,
                            operationId = d.OPERATIONID //c.TBL_APPROVAL_GROUP_MAPPING.Select(x=> x.OPERATIONID).FirstOrDefault()
                        }).GroupBy(x => x.approvalLevelId).Select(g => g.FirstOrDefault()).ToList();

            return data.OrderBy(o=>o.levelName);
        }

        public IEnumerable<ApprovalLevelViewModel> GetAllApprovalLevel(int companyId)
        {
            return GetApprovalLevel(companyId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetAllApprovalLevelDetails(int companyId)
        {
            return GetAllDetailedApprovalLevel(companyId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelById(int ApprovalLevelId, int companyId)
        {
            return GetApprovalLevel(companyId).Where(c => c.approvalLevelId == ApprovalLevelId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByGroupId(int groupId, int companyId)
        {
            return GetApprovalLevel(companyId).Where(c => c.groupId == groupId);
        }

        public IEnumerable<ApprovalLevelViewModel> GetApprovalLevelByOperationId(int operationId, int companyId)
        {
            var data = GetAllDetailedApprovalLevel(companyId).Where(c => c.operationId == operationId);
            return data.GroupBy(x => x.approvalLevelId).Select(g => g.FirstOrDefault()).ToList();
        }

        public List<FintrakDropDownSelectList> GetApprovalLevelsByOperationIdAndProductClassId(int operationId, int? classId)
        {
            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == classId)
                .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL, mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new FintrakDropDownSelectList
                {
                    id = l.APPROVALLEVELID,
                    name = l.LEVELNAME,
                })
                .ToList();

            return levels;
        }

        public List<FintrakDropDownSelectList> GetTranchDisbursmentApprovalLevels()
        {
            var operationId = (int)OperationsEnum.CorporateDrawdownRequest;
            List<FintrakDropDownSelectList> tranchLevels = new List<FintrakDropDownSelectList>();
            int nextGroupId = 20; // ------------------------HARDCODING!!!!!!
            var operation = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == operationId);
            // if (operation != null) nextGroupId = operation.NEXTAPPROVALGROUPID;

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == null)
                .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL, mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                {
                    levelId = l.APPROVALLEVELID,
                    levelName = l.LEVELNAME,
                    groupId = mg.g.GROUPID
                })
                .ToList();

            foreach (var level in levels)
            {
                if (nextGroupId == level.groupId) break;
                tranchLevels.Add(new FintrakDropDownSelectList
                {
                    id = level.levelId,
                    name = level.levelName
                });
            }

            return tranchLevels;
        }

        public bool AddApprovalLevel(ApprovalLevelViewModel model)
        {
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                var data = new TBL_APPROVAL_LEVEL
                {
                    APPROVALLEVELID = model.approvalLevelId,
                    LEVELNAME = model.levelName,
                    POSITION = model.position,
                    TENOR = model.tenor,
                    MAXIMUMAMOUNT = model.maximumAmount,
                    INVESTMENTGRADEAMOUNT = model.investmentGradeAmount,
                    STANDARDGRADEAMOUNT = model.standardGradeAmount,
                    RENEWALLIMIT = model.renewalLimit,
                    FEERATE = model.feeRate,
                    INTERESTRATE = model.interestRate,
                    NUMBEROFUSERS = model.numberOfUsers,
                    NUMBEROFAPPROVALS = model.numberOfApprovals,
                    SLAINTERVAL = model.slaInterval,
                    ISPOLITICALLYEXPOSED = model.isPoliticallyExposed,
                    CANESCALATE = model.canEscalate,
                    CANAPPROVEUNTENORED = model.canApproveUntenored,
                    CANRESOLVEDISPUTE = model.canResolveDispute,
                    ISACTIVE = model.isActive,
                    CANVIEWDOCUMENT = model.canViewDocument,
                    CANEDIT = model.canEdit,
                    CANVIEWUPLOAD = model.canViewUploadedFile,
                    CANUPLOAD = model.canUploadFile,
                    CANVIEWAPPROVAL = model.canViewApproval,
                    CANAPPROVE = model.canApprove,
                    CANRECIEVEEMAIL = model.canRecieveEmail,
                    CANRECIEVESMS = model.canRecieveSms,
                    ROUTEVIASTAFFORGANOGRAM = model.routeViaStaffOrganogram,
                    CREATEDBY = model.createdBy,
                    GROUPID = model.groupId,
                    STAFFROLEID = model.roleId,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    SLANOTIFICATIONINTERVAL = model.slaNotificationInterval,
                    LEVELTYPEID = model.levelTypeId,
                    APPROVALBUSINESSRULEID = model.levelBusinessRuleId,
                    ROLEIDTOROUTE = model.roleIdToRoute,
                    ISPOSTAPPROVALREVIEWER = model.isPostApprovalReviewer,
                    IGNOREIFAPPROVALLEVELSTAFF = model.ignoreIfApprovalLevelStaff
                };

                context.TBL_APPROVAL_LEVEL.Add(data);

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"New approval Level '{ model.levelName }' created by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                var data = new TBL_TEMP_APPROVAL_LEVEL
                {
                    APPROVALLEVELID = model.approvalLevelId,
                    LEVELNAME = model.levelName,
                    POSITION = model.position,
                    TENOR = model.tenor,
                    MAXIMUMAMOUNT = model.maximumAmount,
                    INVESTMENTGRADEAMOUNT = model.investmentGradeAmount,
                    STANDARDGRADEAMOUNT = model.standardGradeAmount,
                    RENEWALLIMIT = model.renewalLimit,
                    FEERATE = model.feeRate,
                    INTERESTRATE = model.interestRate,
                    NUMBEROFUSERS = model.numberOfUsers,
                    NUMBEROFAPPROVALS = model.numberOfApprovals,
                    SLAINTERVAL = model.slaInterval,
                    ISPOLITICALLYEXPOSED = model.isPoliticallyExposed,
                    CANESCALATE = model.canEscalate,
                    CANAPPROVEUNTENORED = model.canApproveUntenored,
                    CANRESOLVEDISPUTE = model.canResolveDispute,
                    ISACTIVE = model.isActive,
                    CANVIEWDOCUMENT = model.canViewDocument,
                    CANEDIT = model.canEdit,
                    CANVIEWUPLOAD = model.canViewUploadedFile,
                    CANUPLOAD = model.canUploadFile,
                    CANVIEWAPPROVAL = model.canViewApproval,
                    CANAPPROVE = model.canApprove,
                    CANRECIEVEEMAIL = model.canRecieveEmail,
                    CANRECIEVESMS = model.canRecieveSms,
                    ROUTEVIASTAFFORGANOGRAM = model.routeViaStaffOrganogram,
                    CREATEDBY = model.createdBy,
                    GROUPID = model.groupId,
                    STAFFROLEID = model.roleId,
                    LEVELTYPEID = model.levelTypeId,
                    APPROVALBUSINESSRULEID = model.levelBusinessRuleId,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    SLANOTIFICATIONINTERVAL = model.slaNotificationInterval,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    OPERATION = "create",
                    ROLEIDTOROUTE = model.roleIdToRoute,

                };

                context.TBL_TEMP_APPROVAL_LEVEL.Add(data);

                if (context.SaveChanges() > 0)
                {
                    model.tempApprovalLevelId = data.TEMPAPPROVALLEVELID;
                }

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = data.TEMPAPPROVALLEVELID;
                workflow.Comment = $"New approval Level '{ model.levelName }' request for approval initiated";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowLevelModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"New approval Level '{ model.levelName }' request for approval initiated ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
                // End of Audit Section ---------------------
            }
            return context.SaveChanges() != 0;
        }

        public bool AddMultipleApprovalLevel(List<ApprovalLevelViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (ApprovalLevelViewModel model in models)
            {
                AddApprovalLevel(model);
            }

            return true;
        }

        public bool UpdateApprovalLevel(int approvalLevelId, ApprovalLevelViewModel model)
        {

            var data = this.context.TBL_APPROVAL_LEVEL.Find(approvalLevelId);
            if (data == null) { return false; }
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                if (data == null) { return false; }

                data.LEVELNAME = model.levelName;
                data.POSITION = model.position;
                data.TENOR = model.tenor;
                //data.TenorModeId = 1; // model.tenorModeId;
                data.MAXIMUMAMOUNT = model.maximumAmount;
                data.INVESTMENTGRADEAMOUNT = model.investmentGradeAmount;
                data.STANDARDGRADEAMOUNT = model.standardGradeAmount;
                data.RENEWALLIMIT = model.renewalLimit;
                data.FEERATE = model.feeRate;
                data.INTERESTRATE = model.interestRate;
                data.NUMBEROFUSERS = model.numberOfUsers;
                data.NUMBEROFAPPROVALS = model.numberOfApprovals;
                data.SLAINTERVAL = model.slaInterval;
                //data.CANROUTEBACK = model.canRouteBack;
                data.ISPOLITICALLYEXPOSED = model.isPoliticallyExposed;
                data.CANESCALATE = model.canEscalate;
                data.CANAPPROVEUNTENORED = model.canApproveUntenored;
                data.CANRESOLVEDISPUTE = model.canResolveDispute;
                data.ISACTIVE = model.isActive;

                data.CANVIEWDOCUMENT = model.canViewDocument;
                data.CANEDIT = model.canEdit;
                data.CANVIEWUPLOAD = model.canViewUploadedFile;
                data.CANUPLOAD = model.canUploadFile;
                data.CANVIEWAPPROVAL = model.canViewApproval;
                data.CANAPPROVE = model.canApprove;
                //data.CANDORISKASSESSMENT = model.canDoRiskAssessment;
                //data.CANRECIEVEADJUSTMENT = model.canRecieveAdjustment;
                data.CANRECIEVEEMAIL = model.canRecieveEmail;
                data.CANRECIEVESMS = model.canRecieveSms;
                //data.HASCHECKLIST = model.hasChecklist;
                //data.CANPERFORMFINANCIALANALYSIS = model.canPerformFinancialAnalysis;
                //data.REQUIREAUTHORISATION = model.requireAuthorisation;
                //data.CANOVERIDEAUTHORISATION = model.canOverideAuthorisation;
                data.ROUTEVIASTAFFORGANOGRAM = model.routeViaStaffOrganogram;
                data.LASTUPDATEDBY = model.createdBy;
                data.DATETIMEUPDATED = DateTime.Now;
                data.GROUPID = model.groupId;
                data.STAFFROLEID = model.roleId;
                data.LEVELTYPEID = model.levelTypeId;
                data.APPROVALBUSINESSRULEID = model.levelBusinessRuleId;
                //data.LASTUPDATEDBY = model.lastUpdatedBy;
                data.ROLEIDTOROUTE = model.roleIdToRoute;
                data.ISPOSTAPPROVALREVIEWER = model.isPostApprovalReviewer;
                data.IGNOREIFAPPROVALLEVELSTAFF = model.ignoreIfApprovalLevelStaff;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Approval Level '{model.levelName}'. ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.approvalLevelId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);

            }
            else
            {
                var values = new TBL_TEMP_APPROVAL_LEVEL
                {
                    LEVELNAME = model.levelName,
                    POSITION = model.position,
                    TENOR = model.tenor,
                    MAXIMUMAMOUNT = model.maximumAmount,
                    INVESTMENTGRADEAMOUNT = model.investmentGradeAmount,
                    STANDARDGRADEAMOUNT = model.standardGradeAmount,
                    RENEWALLIMIT = model.renewalLimit,
                    FEERATE = model.feeRate,
                    INTERESTRATE = model.interestRate,
                    NUMBEROFUSERS = model.numberOfUsers,
                    NUMBEROFAPPROVALS = model.numberOfApprovals,
                    SLAINTERVAL = model.slaInterval,
                    ISPOLITICALLYEXPOSED = model.isPoliticallyExposed,
                    CANESCALATE = model.canEscalate,
                    CANAPPROVEUNTENORED = model.canApproveUntenored,
                    CANRESOLVEDISPUTE = model.canResolveDispute,
                    ISACTIVE = model.isActive,
                    CANVIEWDOCUMENT = model.canViewDocument,
                    CANEDIT = model.canEdit,
                    CANVIEWUPLOAD = model.canViewUploadedFile,
                    CANUPLOAD = model.canUploadFile,
                    CANVIEWAPPROVAL = model.canViewApproval,
                    CANAPPROVE = model.canApprove,
                    CANRECIEVEEMAIL = model.canRecieveEmail,
                    CANRECIEVESMS = model.canRecieveSms,
                    ROUTEVIASTAFFORGANOGRAM = model.routeViaStaffOrganogram,
                    LASTUPDATEDBY = model.lastUpdatedBy,
                    DATETIMEUPDATED = DateTime.Now,
                    GROUPID = model.groupId,
                    STAFFROLEID = model.roleId,
                    LEVELTYPEID = model.levelTypeId,
                    APPROVALBUSINESSRULEID = model.levelBusinessRuleId,
                    SLANOTIFICATIONINTERVAL = model.slaNotificationInterval,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    CREATEDBY = model.createdBy,
                    OPERATION = "update",
                    ROLEIDTOROUTE = model.roleIdToRoute
                    
                };
                context.TBL_TEMP_APPROVAL_LEVEL.Add(values);

                if (context.SaveChanges() > 0)
                {
                    model.tempApprovalLevelId = values.TEMPAPPROVALLEVELID;
                }

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.tempApprovalLevelId;
                workflow.Comment = $"Request to Update Approval Level '{model.levelName}' ";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowLevelModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Approval Level '{model.levelName}' to go for approval. ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.approvalLevelId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
            }


            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        // ----------------------------APPROVAL TRAIL REPOSITORY ---------------------------------\\

        public async Task<bool> DeleteApprovalLevel(int id, UserInfo user)
        {

            int tempApprovalLevelId = 0;
            var model = this.context.TBL_APPROVAL_LEVEL.Find(id);
            if (admin.IsSuperAdmin(user.createdBy) == true)
            {
                model.DELETED = true;
                model.DELETEDBY = user.createdBy;
                model.DATETIMEDELETED = genSetup.GetApplicationDate();

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Workflow approval level '{model.LEVELNAME}' was deleted by this super-admin {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.APPROVALLEVELID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                if (model != null)
                {
                    var values = new TBL_TEMP_APPROVAL_LEVEL
                    {
                        LEVELNAME = model.LEVELNAME,
                        POSITION = model.POSITION,
                        TENOR = model.TENOR,
                        MAXIMUMAMOUNT = model.MAXIMUMAMOUNT,
                        INVESTMENTGRADEAMOUNT = model.INVESTMENTGRADEAMOUNT,
                        STANDARDGRADEAMOUNT = model.STANDARDGRADEAMOUNT,
                        RENEWALLIMIT = model.RENEWALLIMIT,
                        FEERATE = model.FEERATE,
                        INTERESTRATE = model.INTERESTRATE,
                        NUMBEROFUSERS = model.NUMBEROFUSERS,
                        NUMBEROFAPPROVALS = model.NUMBEROFAPPROVALS,
                        SLAINTERVAL = model.SLAINTERVAL,
                        ISPOLITICALLYEXPOSED = model.ISPOLITICALLYEXPOSED,
                        CANESCALATE = model.CANESCALATE,
                        CANAPPROVEUNTENORED = model.CANAPPROVEUNTENORED,
                        CANRESOLVEDISPUTE = model.CANRESOLVEDISPUTE,
                        ISACTIVE = model.ISACTIVE,
                        CANVIEWDOCUMENT = model.CANVIEWDOCUMENT,
                        CANEDIT = model.CANEDIT,
                        CANVIEWUPLOAD = model.CANVIEWDOCUMENT,
                        CANUPLOAD = model.CANUPLOAD,
                        CANVIEWAPPROVAL = model.CANVIEWAPPROVAL,
                        CANAPPROVE = model.CANAPPROVE,
                        CANRECIEVEEMAIL = model.CANRECIEVEEMAIL,
                        CANRECIEVESMS = model.CANRECIEVESMS,
                        ROUTEVIASTAFFORGANOGRAM = model.ROUTEVIASTAFFORGANOGRAM,
                        LASTUPDATEDBY = model.LASTUPDATEDBY,
                        DATETIMEUPDATED = DateTime.Now,
                        GROUPID = model.GROUPID,
                        STAFFROLEID = model.STAFFROLEID,
                        LEVELTYPEID = model.LEVELTYPEID,
                        APPROVALBUSINESSRULEID = model.APPROVALBUSINESSRULEID,
                        SLANOTIFICATIONINTERVAL = model.SLANOTIFICATIONINTERVAL,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                        CREATEDBY = model.CREATEDBY,
                        OPERATION = "delete"
                    };
                    context.TBL_TEMP_APPROVAL_LEVEL.Add(values);

                    if (context.SaveChanges() > 0)
                    {
                        tempApprovalLevelId = values.TEMPAPPROVALLEVELID;
                    }
                }

                workflow.StaffId = user.createdBy;
                workflow.CompanyId = user.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = tempApprovalLevelId;
                workflow.Comment = $"Approval request to delete workflow approval level '{model.LEVELNAME}'";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowLevelModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                //Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Approval request to delete workflow approval level '{model.LEVELNAME}'",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.APPROVALLEVELID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
           
            //end of Audit section -------------------------------

            if (context.TBL_APPROVAL_TRAIL.Where(x => x.TOAPPROVALLEVELID == id || x.FROMAPPROVALLEVELID == id).Any())
            {
                throw new SecureException("Can not delete this level because it is being used. You can de activate it.");
            }
            else
            {
                return await context.SaveChangesAsync() != 0;
            }


        }

        public async Task<bool> AddApprovalTrail(TBL_APPROVAL_TRAIL model)
        {
            try
            {
                context.TBL_APPROVAL_TRAIL.Add(model);
                var saved = await context.SaveChangesAsync();
                return saved > 0;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public bool UpdateApprovalTrail(TBL_APPROVAL_TRAIL model)
        {
            bool result = false;
            var update = context.TBL_APPROVAL_TRAIL.SingleOrDefault(m => m.OPERATIONID == model.OPERATIONID
                                                                     && m.TOAPPROVALLEVELID == model.TOAPPROVALLEVELID
                                                                     && m.TARGETID == model.TARGETID
                                                                 && m.APPROVALSTATUSID == 0);

            if (update != null)
            {
                update.APPROVALSTATUSID = model.APPROVALSTATUSID;
                update.RESPONSEDATE = genSetup.GetApplicationDate();
                update.RESPONSESTAFFID = model.REQUESTSTAFFID;
                update.SYSTEMRESPONSEDATETIME = DateTime.Now;
                result = context.SaveChanges() != 0;
            }
            return result;
        }

        public IEnumerable<TBL_STAFF> GetStaffOrganogram(int companyId)
        {
            return context.TBL_STAFF.Where(c => c.COMPANYID == companyId);
        }

        public IQueryable<TBL_APPROVAL_TRAIL> GetApprovalTrail(int operationId, int targetId, int approvalLevelId, int numberOfApprovals)
        {
            return context.TBL_APPROVAL_TRAIL
                .Where(c => c.TARGETID == targetId &&
                c.OPERATIONID == operationId &&
                c.TOAPPROVALLEVELID == approvalLevelId)
                .Take(numberOfApprovals);
        }

        public IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int operationId, int companyId)
        {
            var result = (from a in context.TBL_APPROVAL_TRAIL
                          join b in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals b.APPROVALLEVELID
                          join d in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals d.APPROVALSTATUSID
                          join c in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals c.APPROVALLEVELID into another
                          from c in another.DefaultIfEmpty()
                          where a.OPERATIONID == operationId && a.COMPANYID == companyId
                          select new
                          {
                              // RequestStaffName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                              RequestApprovalLevel = c == null ? "Initiation" : c.LEVELNAME,
                              ArrivalDate = a.ARRIVALDATE,
                              //ArrivalDate = a.ArrivalDate + a.SystemArrivalDateTime.TimeOfDay  ,

                              ApprovalStatus = d.APPROVALSTATUSNAME,
                              //ResponseDate = a.ResponseDate + a.SystemResponseDateTime.Value.TimeOfDay,

                              ResponseDate = a.RESPONSEDATE.HasValue ? a.RESPONSEDATE : DateTime.Now,
                              //    ResponseStaffName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                              ResponseApprovalLevel = b.LEVELNAME,
                              TargetId = a.TARGETID
                          })
                        .ToList().AsQueryable();

            return (result.Select(c => new WorkflowTrackerViewModel
            {
                approvalStatus = c.ApprovalStatus,
                arrivalDate = c.ArrivalDate,
                requestApprovalLevel = c.RequestApprovalLevel,
                //  requestStaffName = c.RequestStaffName,
                responseApprovalLevel = c.RequestApprovalLevel,
                responseDate = (DateTime)c.ResponseDate,
                //  responseStaffName = c.ResponseStaffName
            }));
        }

        public IQueryable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            return GetApprovalTrail(operationId, companyId).Where(c => c.TargetId == targetId);
        }

        public int GoForApproval(ApprovalLevelViewModel model)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.tempApprovalLevelId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowLevelModification;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (model.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            UpdateMainApprovalLevel(model, (short)workflow.StatusId);
                        }
                    }

                    responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return model.approvalStatusId;
                    }
                    return 0;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }

        private void UpdateMainApprovalLevel(ApprovalLevelViewModel ApprovalModel, short status)
        {
            var data = this.context.TBL_TEMP_APPROVAL_LEVEL.Where(x => x.TEMPAPPROVALLEVELID == ApprovalModel.tempApprovalLevelId).Select(x => x).FirstOrDefault();

            if (data != null)
            {
                var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
                var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFROLEID));

                var audit = new TBL_AUDIT();


                if (data.OPERATION == "create")
                {
                    CreateApprovalLevel(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelAdded;
                    audit.DETAIL = $" Workflow approval level '{ApprovalModel.levelName}' has been added successfully";

                }
                else if (data.OPERATION == "update")
                {
                    UpdateApprovalLevel(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelAdded;
                    audit.DETAIL = $" Workflow approval level '{ApprovalModel.levelName}' has been updated successfully";

                }
                else if (data.OPERATION == "delete")
                {
                    DeleteApprovalLevel(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted;
                    audit.DETAIL = $" Workflow approval level '{ApprovalModel.levelName}' has been delete successfully";
                }

                UpdateTempApprovalLevel(ApprovalModel, status);

                audit.STAFFID = ApprovalModel.createdBy;
                audit.BRANCHID = (short)ApprovalModel.userBranchId;
                audit.IPADDRESS = ApprovalModel.userIPAddress;
                audit.URL = ApprovalModel.applicationUrl;
                audit.APPLICATIONDATE = genSetup.GetApplicationDate();
                audit.SYSTEMDATETIME = DateTime.Now;
                audit.TARGETID = ApprovalModel.approvalLevelId;

                context.TBL_AUDIT.Add(audit);
            }
        }

        private void CreateApprovalLevel(TBL_TEMP_APPROVAL_LEVEL data)
        {
            var entity = new TBL_APPROVAL_LEVEL
            {
                APPROVALLEVELID = data.APPROVALLEVELID,
                LEVELNAME = data.LEVELNAME,
                POSITION = data.POSITION,
                TENOR = data.TENOR,
                MAXIMUMAMOUNT = data.MAXIMUMAMOUNT,
                INVESTMENTGRADEAMOUNT = data.INVESTMENTGRADEAMOUNT,
                STANDARDGRADEAMOUNT = data.STANDARDGRADEAMOUNT,
                RENEWALLIMIT = data.RENEWALLIMIT,
                FEERATE = data.FEERATE,
                INTERESTRATE = data.INTERESTRATE,
                NUMBEROFUSERS = data.NUMBEROFUSERS,
                NUMBEROFAPPROVALS = data.NUMBEROFAPPROVALS,
                SLAINTERVAL = data.SLAINTERVAL,
                ISPOLITICALLYEXPOSED = data.ISPOLITICALLYEXPOSED,
                CANESCALATE = data.CANESCALATE,
                CANAPPROVEUNTENORED = data.CANAPPROVEUNTENORED,
                CANRESOLVEDISPUTE = data.CANRESOLVEDISPUTE,
                ISACTIVE = data.ISACTIVE,
                CANVIEWDOCUMENT = data.CANVIEWDOCUMENT,
                CANEDIT = data.CANEDIT,
                CANVIEWUPLOAD = data.CANVIEWUPLOAD,
                CANUPLOAD = data.CANUPLOAD,
                CANVIEWAPPROVAL = data.CANVIEWAPPROVAL,
                CANAPPROVE = data.CANAPPROVE,
                CANRECIEVEEMAIL = data.CANRECIEVEEMAIL,
                CANRECIEVESMS = data.CANRECIEVESMS,
                ROUTEVIASTAFFORGANOGRAM = data.ROUTEVIASTAFFORGANOGRAM,
                CREATEDBY = data.CREATEDBY,
                GROUPID = data.GROUPID,
                STAFFROLEID = data.STAFFROLEID,
                LEVELTYPEID = data.LEVELTYPEID,
                APPROVALBUSINESSRULEID = data.APPROVALBUSINESSRULEID,
                SLANOTIFICATIONINTERVAL = data.SLANOTIFICATIONINTERVAL,
                DELETED = data.DELETED
            };

            context.TBL_APPROVAL_LEVEL.Add(entity);
        }
        private void UpdateApprovalLevel(TBL_TEMP_APPROVAL_LEVEL data)
        {
            var updateData = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == data.APPROVALLEVELID).Select(x => x).FirstOrDefault();

            if (updateData != null)
            {
                updateData.APPROVALLEVELID = data.APPROVALLEVELID;
                updateData.LEVELNAME = data.LEVELNAME;
                updateData.POSITION = data.POSITION;
                updateData.TENOR = data.TENOR;
                updateData.MAXIMUMAMOUNT = data.MAXIMUMAMOUNT;
                updateData.INVESTMENTGRADEAMOUNT = data.INVESTMENTGRADEAMOUNT;
                updateData.STANDARDGRADEAMOUNT = data.STANDARDGRADEAMOUNT;
                updateData.RENEWALLIMIT = data.RENEWALLIMIT;
                updateData.FEERATE = data.FEERATE;
                updateData.INTERESTRATE = data.INTERESTRATE;
                updateData.NUMBEROFUSERS = data.NUMBEROFUSERS;
                updateData.NUMBEROFAPPROVALS = data.NUMBEROFAPPROVALS;
                updateData.SLAINTERVAL = data.SLAINTERVAL;
                updateData.ISPOLITICALLYEXPOSED = data.ISPOLITICALLYEXPOSED;
                updateData.CANESCALATE = data.CANESCALATE;
                updateData.CANAPPROVEUNTENORED = data.CANAPPROVEUNTENORED;
                updateData.CANRESOLVEDISPUTE = data.CANRESOLVEDISPUTE;
                updateData.ISACTIVE = data.ISACTIVE;
                updateData.CANVIEWDOCUMENT = data.CANVIEWDOCUMENT;
                updateData.CANEDIT = data.CANEDIT;
                updateData.CANVIEWUPLOAD = data.CANVIEWUPLOAD;
                updateData.CANUPLOAD = data.CANUPLOAD;
                updateData.CANVIEWAPPROVAL = data.CANVIEWAPPROVAL;
                updateData.CANAPPROVE = data.CANAPPROVE;
                updateData.CANRECIEVEEMAIL = data.CANRECIEVEEMAIL;
                updateData.CANRECIEVESMS = data.CANRECIEVESMS;
                updateData.ROUTEVIASTAFFORGANOGRAM = data.ROUTEVIASTAFFORGANOGRAM;
                updateData.CREATEDBY = data.CREATEDBY;
                updateData.GROUPID = data.GROUPID;
                updateData.STAFFROLEID = data.STAFFROLEID;
                updateData.LEVELTYPEID = data.LEVELTYPEID;
                updateData.APPROVALBUSINESSRULEID = data.APPROVALBUSINESSRULEID;
                updateData.SLANOTIFICATIONINTERVAL = data.SLANOTIFICATIONINTERVAL;
                updateData.DELETED = data.DELETED;
                updateData.LASTUPDATEDBY = data.CREATEDBY;
            };
        }
        private void DeleteApprovalLevel(TBL_TEMP_APPROVAL_LEVEL data)
        {
            var updateData = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == data.APPROVALLEVELID).Select(x => x).FirstOrDefault();

            if (updateData != null)
            {
                updateData.DELETED = true;
                updateData.DELETEDBY = data.CREATEDBY;
                updateData.DATETIMEDELETED = data.DATETIMECREATED;
            }
        }
        private void UpdateTempApprovalLevel(ApprovalLevelViewModel data, short status)
        {
            var update = context.TBL_TEMP_APPROVAL_LEVEL.Where(x => x.TEMPAPPROVALLEVELID == data.tempApprovalLevelId).Select(x => x).FirstOrDefault();
            if (update != null)
            {
                update.APPROVALSTATUSID = status;
            }
        }


        public List<ApprovalLevelViewModel> GetTempApprovalApprovalLevel(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ApprovalWorkflowLevelModification).ToList();

            var insurance = (from x in context.TBL_TEMP_APPROVAL_LEVEL
                             join atrail in context.TBL_APPROVAL_TRAIL on x.TEMPAPPROVALLEVELID equals atrail.TARGETID
                             join a in context.TBL_APPROVAL_GROUP on x.GROUPID equals a.GROUPID
                             where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.ApprovalWorkflowLevelModification
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
                             select new ApprovalLevelViewModel
                             {
                                 tempApprovalLevelId = x.TEMPAPPROVALLEVELID,
                                 approvalLevelId = x.APPROVALLEVELID,
                                 levelName = x.LEVELNAME,
                                 position = x.POSITION,
                                 tenor = x.TENOR,
                                 maximumAmount = x.MAXIMUMAMOUNT,
                                 investmentGradeAmount = x.INVESTMENTGRADEAMOUNT,
                                 standardGradeAmount = x.STANDARDGRADEAMOUNT,
                                 renewalLimit = x.RENEWALLIMIT,
                                 feeRate = x.FEERATE,
                                 interestRate = x.INTERESTRATE,
                                 numberOfUsers = x.NUMBEROFUSERS,
                                 numberOfApprovals = x.NUMBEROFAPPROVALS,
                                 slaInterval = x.SLAINTERVAL,
                                 isPoliticallyExposedValue = x.ISPOLITICALLYEXPOSED ? "Yes" : "No",
                                 canEscalateValue = x.CANESCALATE ? "Yes" : "No",
                                 canApproveUntenoredValue = x.CANAPPROVEUNTENORED ? "Yes" : "No",
                                 canResolveDisputeValue = x.CANRESOLVEDISPUTE ? "Yes" : "No",
                                 isActiveValue = x.ISACTIVE ? "Yes" : "No",
                                 slaNotificationInterval = x.SLANOTIFICATIONINTERVAL,
                                 canViewDocumentValue = x.CANVIEWDOCUMENT ? "Yes" : "No",
                                 canEditValue = x.CANEDIT ? "Yes" : "No",
                                 canViewUploadedFileValue = x.CANVIEWUPLOAD ? "Yes" : "No",
                                 canUploadFileValue = x.CANUPLOAD ? "Yes" : "No",
                                 canViewApprovalValue = x.CANVIEWAPPROVAL ? "Yes" : "No",
                                 canApproveValue = x.CANAPPROVE ? "Yes" : "No",
                                 canRecieveEmailValue = x.CANRECIEVEEMAIL ? "Yes" : "No",
                                 canRecieveSmsValue = x.CANRECIEVESMS ? "Yes" : "No",
                                 routeViaStaffOrganogram = x.ROUTEVIASTAFFORGANOGRAM,
                                 createdBy = x.CREATEDBY,
                                 dateTimeCreated = x.DATETIMECREATED,
                                 dateTimeUpdated = x.DATETIMEUPDATED,
                                 deleted = x.DELETED,
                                 deletedBy = x.DELETEDBY,
                                 dateTimeDeleted = x.DATETIMEDELETED,
                                 groupId = (int)x.GROUPID,
                                 roleId = x.STAFFROLEID,
                                 levelTypeId = x.LEVELTYPEID,
                                 levelBusinessRuleId = x.APPROVALBUSINESSRULEID,
                                 groupName = a.GROUPNAME,
                                 operation = x.OPERATION
                             }).ToList();

            return insurance;
        }

        #region preset route

        public bool PresetRoute(PresetRouteViewModel entity)
        {
            if (entity.moduleId == 1)
            {
                var appl = context.TBL_LOAN_APPLICATION.Find(entity.applicationId);
                appl.NEXTAPPLICATIONSTATUSID = (short)entity.nextApplicationStatusId;
                appl.FINALAPPROVAL_LEVELID = entity.finalApprovalLevelId;
            }
            if (entity.moduleId == 2)
            {
                var appl = context.TBL_LMSR_APPLICATION.Find(entity.applicationId);
                appl.NEXTAPPLICATIONSTATUSID = (short)entity.nextApplicationStatusId;
                appl.FINALAPPROVAL_LEVELID = entity.finalApprovalLevelId;
            }

            return context.SaveChanges() > 0;
        }

        public PresetRouteViewModel GetPresetRouteCollection(int operationId, int? classId)
        {
            var preset = new PresetRouteViewModel();

            var process = context.TBL_LOAN_APPLICATION_STATUS.Select(x => new FintrakDropDownSelectList
                {
                    id = x.APPLICATIONSTATUSID,
                    name = x.APPLICATIONSTATUSNAME,
                })
                .ToList();

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == classId)
                .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                .Join(context.TBL_APPROVAL_LEVEL, mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new FintrakDropDownSelectList
                {
                    id = l.APPROVALLEVELID,
                    name = l.LEVELNAME,
                })
                .ToList();

            preset.applicationStatus = process;
            preset.approvalLevels = levels;

            return preset;
        }


        #endregion preset note


        public List<FintrakDropDownSelectList> GetRoutableOperations(List<int> operationIds)
        {
            return context.TBL_OPERATIONS
                .Where(x => operationIds.Contains(x.OPERATIONID))
                .Select(x => new FintrakDropDownSelectList
                {
                    id = x.OPERATIONID,
                    name = x.OPERATIONNAME
                })
            .ToList();
        }

        public List<ApprovalLevelViewModel> GetRerouteApprovalLevels(int operationId)
        {
            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.DELETED == false)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true && x.DELETED == false),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new ApprovalLevelViewModel
                        {
                            levelPosition = l.POSITION,
                            groupPosition = mg.m.POSITION,
                            levelName = l.LEVELNAME,
                            approvalLevelId = l.APPROVALLEVELID,
                            roleId = l.STAFFROLEID,
                            roleName = l.STAFFROLEID == null ? " " : l.TBL_STAFF_ROLE.STAFFROLENAME
                        })
                        .Distinct()
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            return levels;
        }

        public bool RerouteOperation(ForwardViewModel model) 
        {
            //workflow.StaffId = model.createdBy;
            //workflow.OperationId = model.operationId; 
            //workflow.TargetId = model.targetId;
            //workflow.CompanyId = model.companyId;
            //workflow.ProductClassId = model.productClassId;
            //workflow.ProductId = model.productId;
            //workflow.StatusId = (int)ApprovalStatusEnum.Authorised;
            //workflow.Comment = model.comment;
            //workflow.DeferredExecution = true;
            //workflow.LogActivity();

            var currentTrail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x =>
                x.OPERATIONID == model.operationId
                && x.RESPONSESTAFFID == null
                && x.TARGETID == model.targetId
            );

            if (currentTrail != null)
            {
                currentTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                currentTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Authorised;
                currentTrail.COMMENT = model.comment;
                currentTrail.TOAPPROVALLEVELID = null;
                currentTrail.TOSTAFFID = null;
                currentTrail.RESPONSESTAFFID = model.createdBy;
            }

            workflow.NextLevelId = model.nextApprovalLevelId;
            workflow.NextProcess(model.companyId, model.createdBy, model.nextOperationId, null, model.targetId, null, "NIL", true, true, true, false, null);

            UpdateTarget(model.operationId,model.targetId, model.nextOperationId);

            return context.SaveChanges() > 0;
        }

        private void UpdateTarget(int operationId, int targetId, int nextOperationId)
        {
            if (operationId == 46 || operationId == 71 || operationId == 79)
            {
                var appl = context.TBL_LMSR_APPLICATION.Find(targetId);
                appl.OPERATIONID = nextOperationId;
                context.Entry(appl).State = System.Data.Entity.EntityState.Modified;
            }
        }
        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }
        public IEnumerable<ApprovalTrailViewModel> GenericApprovalTrail(ApprovalTrailRequestViewModel entity)
        {

      
            var allstaff = this.GetAllStaffNames();

            var operationId = entity.operationIds[0];

            //var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID == entity.operationId && x.TARGETID == entity.targetId);

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID == operationId && x.TARGETID == entity.targetId);


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
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).OrderByDescending(x => x.approvalTrailId).ToList();

            return data.OrderByDescending(p => p.approvalTrailId);
        }

        public IEnumerable<ApprovalTrailViewModel> GenericLMSApprovalTrail(int targetId, int operationId)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();
            var data = (from x in context.TBL_APPROVAL_TRAIL
                         where
                         x.OPERATIONID == operationId 
                         && x.TARGETID == targetId
                         //&& x.FROMAPPROVALLEVELID != null
              select new ApprovalTrailViewModel
              {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                vote = x.VOTE,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                commentStage = "Credit Appaisal",
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).OrderBy(x => x.approvalTrailId).ToList();

            var initiation = data.FirstOrDefault();
            if (initiation?.fromApprovalLevelId == null)
            {
                //data.Remove(initiation);
                data = data.OrderByDescending(d => d.approvalTrailId).ToList();
            }

            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => x).FirstOrDefault();
            var reviewDetail = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANREVIEWAPPLICATIONID == applicationDetail.LOANREVIEWAPPLICATIONID).Select(x => x).FirstOrDefault();
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewApprovalAvailment, "Availment"));
            if (reviewDetail != null)
            {
                data.AddRange(GetNonAppraisalTrail(reviewDetail.LOANREVIEWOPERATIONID, reviewDetail.OPERATIONTYPEID, "Credit Operations"));
            }
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewDrawdownForExtension, "Loan Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.ContingentReviewDrawdownForExtension, "Contingent Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.OverdraftReviewDrawdownForExtension, "Overdraft Review Drawdown"));

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

        private IEnumerable<ApprovalTrailViewModel> GetNonAppraisalTrail(int applicationId, int operationid, string commentStage)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();

            //var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId && x.FROMAPPROVALLEVELID != null).ToList();
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
            })?.OrderBy(x => x.systemArrivalDateTime).ToList();


            var initiation = data.FirstOrDefault();
            if (initiation?.fromApprovalLevelId == null)
            {
                //data.Remove(initiation);
                data = data.OrderByDescending(d => d.approvalTrailId).ToList();
            }
            data.OrderByDescending(d => d.systemArrivalDateTime);
            return data;
        }

        public IQueryable<WorkflowNotificationViewModel> GetWorkflowMappingNotifications(int MappingId)
        {
            var notifications = context.TBL_WORKFLOW_NOTIFICATION.Where(x => x.GROUPOPERATIONMAPPINGID == MappingId).Select(x => new WorkflowNotificationViewModel
            {
                workflowNotificationId = x.WORKFLOWNOTIFICATIONID,
                groupOperationMappingId = x.GROUPOPERATIONMAPPINGID,
                approvalLevelId = x.APPROVALLEVELID,
                proceedingActionsAlertTitleId = x.PROCEEDINGACTIONSALERTTITLEID,
                poolAlertTitleId = x.POOLALERTTITLEID,
                ownerAlertTitleId = x.OWNERALERTTITLEID,
                includePoolInNotification = x.INCLUDEPOOLINNOTIFICATION,
                notifyOfProceedingWorkflowActions = x.NOTIFYOFPROCEEDINGWORKFLOWACTIONS,
                notifyOnwer = x.NOTIFYONWER,
                notifyOfPendingApprovals = x.NOTIFYOFPENDINGAPPROVALS,
                pendingApprovalAlertTitleId = x.PENDINGAPPROVALALERTTITLEID
            });

            return notifications;
        }

        public async Task<bool> AddWorkflowMappingNotification(WorkflowNotificationViewModel model)
        {
            var data = new TBL_WORKFLOW_NOTIFICATION()
            {
                GROUPOPERATIONMAPPINGID = model.groupOperationMappingId,
                APPROVALLEVELID = model.approvalLevelId,
                PROCEEDINGACTIONSALERTTITLEID = model.proceedingActionsAlertTitleId,
                POOLALERTTITLEID = model.poolAlertTitleId,
                OWNERALERTTITLEID = model.ownerAlertTitleId,
                INCLUDEPOOLINNOTIFICATION = model.includePoolInNotification,
                NOTIFYOFPROCEEDINGWORKFLOWACTIONS = model.notifyOfProceedingWorkflowActions,
                NOTIFYONWER = model.notifyOnwer,
                NOTIFYOFPENDINGAPPROVALS = model.notifyOfPendingApprovals,
                PENDINGAPPROVALALERTTITLEID = model.pendingApprovalAlertTitleId,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                CREATEDBY = model.createdBy
            };

            context.TBL_WORKFLOW_NOTIFICATION.Add(data);
            var saved = await context.SaveChangesAsync() > 0;
            return saved;
        }

        public bool UpdateWorkflowMappingNotification(WorkflowNotificationViewModel model, int workflowNotificationId)
        {
            var notification = context.TBL_WORKFLOW_NOTIFICATION.FirstOrDefault(x => x.WORKFLOWNOTIFICATIONID == workflowNotificationId);
            if (notification != null)
            {
                notification.PROCEEDINGACTIONSALERTTITLEID = model.proceedingActionsAlertTitleId;
                notification.POOLALERTTITLEID = model.poolAlertTitleId;
                notification.OWNERALERTTITLEID = model.ownerAlertTitleId;
                notification.INCLUDEPOOLINNOTIFICATION = model.includePoolInNotification;
                notification.NOTIFYOFPROCEEDINGWORKFLOWACTIONS = model.notifyOfProceedingWorkflowActions;
                notification.NOTIFYONWER = model.notifyOnwer;
                notification.NOTIFYOFPENDINGAPPROVALS = model.notifyOfPendingApprovals;
                notification.PENDINGAPPROVALALERTTITLEID = model.pendingApprovalAlertTitleId;
                notification.DATETIMEUPDATED = genSetup.GetApplicationDate();
                notification.LASTUPDATEDBY = model.createdBy;
            }

            var saved = context.SaveChanges() > 0;
            return saved;
        }

        public async Task<bool> DeleteWorkflowMappingNotification(int MappingId, UserInfo user)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                var notifications = context.TBL_WORKFLOW_NOTIFICATION.Where(x => x.GROUPOPERATIONMAPPINGID == MappingId);
                context.TBL_WORKFLOW_NOTIFICATION.RemoveRange(notifications);

                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.WorkflowMappingNotificationsDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Workflow mapping notification for mappingId '{MappingId}' was deleted by {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = MappingId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);

                var saved = await context.SaveChangesAsync() > 0;
                trans.Commit();
                return saved;
            }
            
        }

        public IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowContext()
        {
            var data = (from a in context.TBL_WORKFLOW_CONTEXT

                        select new DynamicWorkflowViewModel
                        {
                            contextId = a.CONTEXTID,
                            contextName = a.CONTEXTNAME,

                        }).ToList();
            return data;
        }

        public IEnumerable<OperatorsViewModel> GetAllOperators()
        {
            var data = (from a in context.TBL_OPERATORS

                        select new OperatorsViewModel
                        {
                            operatorId = a.OPERATORID,
                            operators = a.OPERATOR,
                            description = a.DESCRIPTION

                        }).ToList();
            return data;
        }

        public IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowDataItemDefinition()
        {
            var data = (from a in context.TBL_WORKFLOW_DATA_ITEM_DEFINITION

                        select new DynamicWorkflowViewModel
                        {
                            dataItemId = a.DATAITEMID,
                            contextName = context.TBL_WORKFLOW_CONTEXT.Where(c => c.CONTEXTID == a.CONTEXTID).Select(c => c.CONTEXTNAME).FirstOrDefault(),
                            dataItemName = a.DATAITEMNAME,
                            contextId = a.CONTEXTID,
                            valueTypeId = a.VALUETYPEID

                        }).ToList();
            return data;
        }

        public IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowDataItemByContextId(int contextId)
        {
            var data = (from a in context.TBL_WORKFLOW_DATA_ITEM_DEFINITION
                        where contextId == a.CONTEXTID
                        select new DynamicWorkflowViewModel
                        {
                            dataItemId = a.DATAITEMID,
                            contextName = context.TBL_WORKFLOW_CONTEXT.Where(c => c.CONTEXTID == a.CONTEXTID).Select(c => c.CONTEXTNAME).FirstOrDefault(),
                            dataItemName = a.DATAITEMNAME,
                            contextId = a.CONTEXTID,
                            valueTypeId = a.VALUETYPEID

                        }).ToList();
            return data;
        }

        public DynamicWorkflowViewModel GetValueTypeByItemId(int dataItemId)
        {
            var data = (from a in context.TBL_WORKFLOW_DATA_ITEM_DEFINITION
                        join c in context.TBL_WORKFLOW_CONTEXT on a.CONTEXTID equals c.CONTEXTID
                        where a.DATAITEMID == dataItemId
                        select new DynamicWorkflowViewModel
                        {
                            dataItemId = a.DATAITEMID,
                            contextName = c.CONTEXTNAME, //context.TBL_WORKFLOW_CONTEXT.Where(c => c.CONTEXTID == a.CONTEXTID).Select(c => c.CONTEXTNAME).FirstOrDefault(),
                            dataItemName = a.DATAITEMNAME,
                            contextId = a.CONTEXTID,
                            valueTypeId = a.VALUETYPEID

                        }).FirstOrDefault();
            return data;
        }

        public List<DynamicContextListViewModel> GetDynamicBusinessRuleItemValueListByItemId(int dataItemId)
        {
            var dynamicCntxModel = new List<DynamicContextListViewModel>();
            var dataItem = context.TBL_WORKFLOW_DATA_ITEM_DEFINITION.Find(dataItemId);
            bool isBoolValueType = context.TBL_WFCONTEXT_VALUE_TYPE.Where(x => x.VALUETYPEID == dataItem.VALUETYPEID && x.VALUETYPENAME.ToUpper() == "BOOLEAN" && x.INUSE == true).Any();

            if (isBoolValueType)
            {
                var boolList_False = new DynamicContextListViewModel() { id = 0, value = "False" };
                var boolList_True = new DynamicContextListViewModel() { id = 1, value = "True" };

                dynamicCntxModel.Add(boolList_False);
                dynamicCntxModel.Add(boolList_True);

                return dynamicCntxModel;
            }

            if (dataItemId == 10)//BUSINESS UNIT LIST
            {
                var businessunits = context.TBL_PROFILE_BUSINESS_UNIT.ToList();
                foreach (var item in businessunits)
                {
                    var prod = new DynamicContextListViewModel()
                    {
                        id = item.BUSINESSUNITID,
                        value = item.BUSINESSUNITNAME
                    };
                    dynamicCntxModel.Add(prod);
                }

            }
            if (dataItemId == 15) //PRODUCT LIST
            {
                var prodList = context.TBL_PRODUCT.ToList();
                foreach (var item in prodList)
                {
                    var prod = new DynamicContextListViewModel()
                    {
                        id = item.PRODUCTID,
                        value = item.PRODUCTNAME
                    };
                    dynamicCntxModel.Add(prod);
                }
            }

            return dynamicCntxModel;
        }

        public IEnumerable<DynamicWorkflowViewModel> GetDynamicWorkflowItemExpression()
        {
            var data = (from a in context.TBL_WORKFLOW_ITEM_EXPRESSION
                        join d in context.TBL_WORKFLOW_DATA_ITEM_DEFINITION on a.DATAITEMID equals d.DATAITEMID
                        where a.DELETED == false
                        select new DynamicWorkflowViewModel
                        {
                            expressionId = a.EXPRESSIONID,
                            contextId = a.CONTEXTID,
                            valueTypeId = d.VALUETYPEID,
                            dataItemId = a.DATAITEMID,
                            contextName = context.TBL_WORKFLOW_CONTEXT.Where(c => c.CONTEXTID == a.CONTEXTID).Select(c => c.CONTEXTNAME).FirstOrDefault(),
                            dataItemName = context.TBL_WORKFLOW_DATA_ITEM_DEFINITION.Where(c => c.DATAITEMID == a.DATAITEMID).Select(c => c.DATAITEMNAME).FirstOrDefault(),
                            value = a.BOOLEANVALUE != null ? a.BOOLEANVALUE.ToString() : a.TEXTVALUE != string.Empty ? a.TEXTVALUE : a.IDVALUE != null ? a.IDVALUE.ToString() : "",
                            expression = a.EXPRESSION,
                            comparisonId = a.COMPARISONID,
                            workflowExpression = context.TBL_WORKFLOW_DATA_ITEM_DEFINITION.Where(c => c.DATAITEMID == a.DATAITEMID).Select(c => c.DATAITEMNAME).FirstOrDefault() + " " +
                                                context.TBL_OPERATORS.Where(c => c.OPERATORID == a.COMPARISONID).Select(c => c.OPERATOR).FirstOrDefault() + " " + a.EXPRESSION,
                            approvalBusinessRuleId = a.APPROVALBUSINESSRULEID,
                            approvalBusinessRule = context.TBL_APPROVAL_BUSINESS_RULE.Where(c => c.APPROVALBUSINESSRULEID == a.APPROVALBUSINESSRULEID).Select(c => c.DESCRIPTION).FirstOrDefault(),
                            idValue = a.IDVALUE,
                            textValue = a.TEXTVALUE,
                            booleanValue = a.BOOLEANVALUE

                        }).ToList();
            return data;
        }

        public bool CreateDynamicWorkflowItemExpression(DynamicWorkflowViewModel model)
        {
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                var expression = new TBL_WORKFLOW_ITEM_EXPRESSION
                {
                    CONTEXTID = model.contextId,
                    DATAITEMID = model.dataItemId,
                    COMPARISONID = model.comparisonId,
                    EXPRESSION = model.value,
                    IDVALUE = model.idValue,
                    TEXTVALUE = model.textValue,
                    BOOLEANVALUE = model.booleanValue,
                    WORKFLOWEXPRESSION = model.dataItemId + "" + model.comparisonId + "" + model.value,
                    APPROVALBUSINESSRULEID = model.approvalBusinessRuleId
                };

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"New Dynamic Workflow businessrul '{ model.workflowExpression }' created.",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
                context.TBL_WORKFLOW_ITEM_EXPRESSION.Add(expression);

            }
            return context.SaveChanges() != 0;
        }

        public bool UpdateDynamicWorkflowItemExpression(DynamicWorkflowViewModel model, int expressionId)
        {
            try
            {
                var expre = context.TBL_WORKFLOW_ITEM_EXPRESSION.FirstOrDefault(x => x.EXPRESSIONID == expressionId);
                if (expre != null)
                {
                    expre.CONTEXTID = model.contextId;
                    expre.DATAITEMID = model.dataItemId;
                    expre.IDVALUE = model.idValue;
                    expre.COMPARISONID = model.comparisonId;
                    expre.EXPRESSION = model.value;
                    expre.TEXTVALUE = model.textValue;
                    expre.BOOLEANVALUE = model.booleanValue;
                    expre.WORKFLOWEXPRESSION = model.dataItemName + " " + model.comparisonId + " " + model.value;
                    expre.APPROVALBUSINESSRULEID = model.approvalBusinessRuleId;
                }

                var saved = context.SaveChanges() > 0;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BusinessRuleUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"New Dynamic Workflow business rule '{ model.workflowExpression }' updated.",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
                return saved;
            }
            catch (Exception e)
            {
                throw e;
            }
            
            
        }
    }
}