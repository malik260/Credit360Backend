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
using System.ComponentModel.Composition;
using System.Data.Entity;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalLevelRepository : IApprovalLevelRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalLevelRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
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

                            canViewDocument = a.CANVIEWDOCUMENT,
                            canEdit = a.CANEDIT,
                            canViewUploadedFile = a.CANVIEWUPLOAD,
                            canUploadFile = a.CANUPLOAD,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,

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

            return data;
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

        public bool AddApprovalLevel(ApprovalLevelViewModel model)
        {
            var data = new TBL_APPROVAL_LEVEL
            {
                APPROVALLEVELID = model.approvalLevelId,
                LEVELNAME = model.levelName,
                POSITION = model.position,
                TENOR = model.tenor,
                MAXIMUMAMOUNT = model.maximumAmount,
                INVESTMENTGRADEAMOUNT = model.investmentGradeAmount,
                FEERATE = model.feeRate,
                INTERESTRATE = model.interestRate,
                NUMBEROFUSERS = model.numberOfUsers,
                NUMBEROFAPPROVALS = model.numberOfApprovals,
                SLAINTERVAL = model.slaInterval,
                //CANROUTEBACK = model.canRouteBack,
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

                //CANDORISKASSESSMENT = model.canDoRiskAssessment,
                //CANRECIEVEADJUSTMENT = model.canRecieveAdjustment,
                CANRECIEVEEMAIL = model.canRecieveEmail,
                CANRECIEVESMS = model.canRecieveSms,
                //HASCHECKLIST = model.hasChecklist,
                //CANPERFORMFINANCIALANALYSIS = model.canPerformFinancialAnalysis,
                //REQUIREAUTHORISATION = model.requireAuthorisation,
                //CANOVERIDEAUTHORISATION = model.canOverideAuthorisation,
                ROUTEVIASTAFFORGANOGRAM = model.routeViaStaffOrganogram,
                CREATEDBY = model.createdBy,
                GROUPID = model.groupId,
                STAFFROLEID = model.roleId,
                DATETIMECREATED = genSetup.GetApplicationDate()
            };

            context.TBL_APPROVAL_LEVEL.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added ApprovalLevel '{ model.levelName }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

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

            data.LEVELNAME = model.levelName;
            data.POSITION = model.position;
            data.TENOR = model.tenor;
            //data.TenorModeId = 1; // model.tenorModeId;
            data.MAXIMUMAMOUNT = model.maximumAmount;
            data.INVESTMENTGRADEAMOUNT = model.investmentGradeAmount;
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
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.GROUPID = model.groupId;
            data.STAFFROLEID = model.roleId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Approval Level '{model.levelName}'. ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.approvalLevelId
            };
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        // ----------------------------APPROVAL TRAIL REPOSITORY ---------------------------------\\

        public async Task<bool> DeleteApprovalLevel(int id, UserInfo user)
        {
            var data = this.context.TBL_APPROVAL_LEVEL.Find(id);

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Approval Level '{data.LEVELNAME}'. ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.APPROVALLEVELID
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            if (context.TBL_APPROVAL_TRAIL.Where(x => x.TOAPPROVALLEVELID == id || x.FROMAPPROVALLEVELID == id).Any())
            {
                throw new Exception("Can not delete this level because it is being used. You can de activate it.");
            }
            else
            {
                this.context.TBL_APPROVAL_LEVEL.Remove(data);
            }

            return await context.SaveChangesAsync() != 0;
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
                throw new Exception(ex.Message);
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

        #region preset route

        public bool PresetRoute(PresetRouteViewModel entity)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(entity.applicationId);
            appl.NEXTAPPLICATIONSTATUSID = (short)entity.nextApplicationStatusId;
            appl.FINALAPPROVAL_LEVELID = entity.finalApprovalLevelId;
            return context.SaveChanges() > 0;
        }

        public PresetRouteViewModel GetPresetRouteCollection(int operationId, int? classId)
        {
            var preset = new PresetRouteViewModel();

            var process = context.TBL_LOAN_APPLICATION_STATUS.Select(x=> new FintrakDropDownSelectList
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
    }
}