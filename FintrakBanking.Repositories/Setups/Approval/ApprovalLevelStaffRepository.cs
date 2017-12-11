using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalLevelStaffRepository : IApprovalLevelStaffRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public ApprovalLevelStaffRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaff(int companyId)
        {
            var data = (from a in context.TBL_APPROVAL_LEVEL_STAFF
                        join b in context.TBL_APPROVAL_LEVEL on a.APPROVALLEVELID equals b.APPROVALLEVELID
                        //join c in context.tbl_Approval_Group_Mapping on b.GroupId equals c.GroupId
                        where a.TBL_APPROVAL_LEVEL.TBL_APPROVAL_GROUP.COMPANYID == companyId
                        && a.DELETED == false
                        select new ApprovalLevelStaffViewModel
                        {
                            groupId = (int)a.TBL_APPROVAL_LEVEL.GROUPID,
                           // operationId = b.OperationId,
                            maximumAmount = a.MAXIMUMAMOUNT,
                            processViewScope = a.PROCESSVIEWSCOPEID,
                            canViewDocument = a.CANVIEWCAMDOCUMENT,
                            canViewUploadedFile = a.CANVIEWUPLOADEDFILE,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            canUploadFile = a.CANUPLOADFILE,
                            canSendRequest = a.CANSENDJOBREQUEST,
                            canEdit = a.CANEDIT,
                            vetoPower = a.VETOPOWER,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.TBL_APPROVAL_LEVEL.POSITION,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            staffId = a.STAFFID,
                            staffLevelId = a.STAFFLEVELID,// added
                            staffLevelName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        private IEnumerable<ApprovalLevelStaffViewModel> GetAllDetailedApprovalLevelStaff(int companyId)
        {
            var data = (from a in context.TBL_APPROVAL_LEVEL_STAFF
                        join e in context.TBL_STAFF on a.STAFFID equals e.STAFFID
                        join b in context.TBL_APPROVAL_LEVEL on a.APPROVALLEVELID equals b.APPROVALLEVELID
                        join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                        join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                        where c.COMPANYID == companyId
                        && a.DELETED == false
                        select new ApprovalLevelStaffViewModel
                        {
                            groupId = (int)a.TBL_APPROVAL_LEVEL.GROUPID,
                            operationId = d.OPERATIONID,
                            maximumAmount = a.MAXIMUMAMOUNT,
                            processViewScope = a.PROCESSVIEWSCOPEID,
                            canViewDocument = a.CANVIEWCAMDOCUMENT,
                            canViewUploadedFile = a.CANVIEWUPLOADEDFILE,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            canUploadFile = a.CANUPLOADFILE,
                            canSendRequest = a.CANSENDJOBREQUEST,
                            canEdit = a.CANEDIT,
                            vetoPower = a.VETOPOWER,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.TBL_APPROVAL_LEVEL.POSITION,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            staffId = a.STAFFID,
                            staffLevelId = a.STAFFLEVELID,// added
                            staffLevelName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaff(int companyId)
        {
            return GetApprovalLevelStaff(companyId);
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetAllAssignedApprovalLevelStaff(int companyId)
        {
            var data = GetAllDetailedApprovalLevelStaff(companyId);
            return data;
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetAllApprovalLevelStaffByOperationId(int operationId, int companyId)
        {
            var data =  GetApprovalLevelStaff(companyId).Where(c => c.operationId == operationId);
            return data;
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetApprovalLevelStaffById(int StaffLevelId, int companyId)
        {
            var data = GetApprovalLevelStaff(companyId).Where(c => c.approvalLevelId == StaffLevelId);

            return data;
        }

        public ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId, int operationId)
        {
            var levelStaff = GetAllDetailedApprovalLevelStaff(companyId);
            return levelStaff.FirstOrDefault(c => c.staffId == staffId && c.operationId == operationId);
        }

        public ApprovalLevelStaffViewModel GetAllApprovalLevelStaffByStaffId(int staffId, int companyId)
        {
            var levelStaff = GetAllDetailedApprovalLevelStaff(companyId);
            return levelStaff.FirstOrDefault(c => c.staffId == staffId);
        }

        public bool AddApprovalLevelStaff(ApprovalLevelStaffViewModel model)
        {
            var data = new TBL_APPROVAL_LEVEL_STAFF
            {
                MAXIMUMAMOUNT = model.maximumAmount,
                STAFFID = model.staffId,
                APPROVALLEVELID = model.approvalLevelId,
                PROCESSVIEWSCOPEID = (short)model.processViewScope,
                CANVIEWCAMDOCUMENT = model.canViewDocument,
                CANVIEWUPLOADEDFILE = model.canViewUploadedFile,
                CANVIEWAPPROVAL = model.canViewApproval,
                CANAPPROVE = model.canApprove,
                CANUPLOADFILE = model.canUploadFile,
                CANSENDJOBREQUEST = model.canSendRequest,
                CANEDIT = model.canEdit,
                VETOPOWER = model.vetoPower,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };

            // Audit Section ---------------------------
            var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
            var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' .",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.staffLevelId
            };

            context.TBL_APPROVAL_LEVEL_STAFF.Add(data);
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateApprovalLevelStaff(int StaffLevelId, ApprovalLevelStaffViewModel model)
        {
            var data = this.context.TBL_APPROVAL_LEVEL_STAFF.Find(StaffLevelId);
            if (data == null) return false;

            data.STAFFID = model.staffId;
            data.APPROVALLEVELID = model.approvalLevelId;
            data.MAXIMUMAMOUNT = model.maximumAmount;
            data.PROCESSVIEWSCOPEID = (short)model.processViewScope;
            data.CANVIEWCAMDOCUMENT = model.canViewDocument;
            data.CANVIEWUPLOADEDFILE = model.canViewUploadedFile;
            data.CANVIEWAPPROVAL = model.canViewApproval;
            data.CANAPPROVE = model.canApprove;
            data.CANUPLOADFILE = model.canUploadFile;
            data.CANSENDJOBREQUEST = model.canSendRequest;
            data.CANEDIT = model.canEdit;
            data.VETOPOWER = model.vetoPower;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = (int)model.createdBy;

            // Audit Section ---------------------------
            //var audit_staff_level = (context.TblApprovalLevel.FirstOrDefault(x => x.ApprovalLevelId == StaffLevelId));
            var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Approval Level for staff with code '{audit_staff.STAFFCODE}' to level {model.staffLevelName}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.staffLevelId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteApprovalLevelStaff(int StaffLevelId, UserInfo user)
        {
            var data = this.context.TBL_APPROVAL_LEVEL_STAFF.Find(StaffLevelId);

            //Audit Section ---------------------------
            var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
            var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Added Approval Level Staff {audit_staff_level.LEVELNAME}' for staff with code '{audit_staff.STAFFCODE}' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.STAFFLEVELID
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            this.context.TBL_APPROVAL_LEVEL_STAFF.Remove(data);

            return await context.SaveChangesAsync() != 0;
        }

        public bool AddApprovalTrail(TBL_APPROVAL_TRAIL model)
        {
            context.TBL_APPROVAL_TRAIL.Add(model);
            return context.SaveChanges() != 0;
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
                update.RESPONSEDATE = _genSetup.GetApplicationDate();
                update.SYSTEMRESPONSEDATETIME = model.SYSTEMRESPONSEDATETIME;
                update.RESPONSESTAFFID = model.RESPONSESTAFFID;

                result = context.SaveChanges() != 0;
            }
            return result;
        }

        public IEnumerable<TBL_STAFF_ORGANOGRAM> GetStaffOrganogram(int companyId)
        {
            return context.TBL_STAFF_ORGANOGRAM.Where(c => c.COMPANYID == companyId);
        }

        public IQueryable<TBL_APPROVAL_TRAIL> GetApprovalTrail(int operationId, int targetId, int approvalLevelId, int numberOfApprovals)
        {
            return context.TBL_APPROVAL_TRAIL
                .Where(c => c.TARGETID == targetId &&
                c.OPERATIONID == operationId &&
                c.TOAPPROVALLEVELID == approvalLevelId)
                .Take(numberOfApprovals);
        }

        private IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int companyId)
        {
            var result = (from a in context.TBL_APPROVAL_TRAIL
                          join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                          join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                          join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                          join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                          join f in context.TBL_APPROVAL_LEVEL on a.TOAPPROVALLEVELID equals f.APPROVALLEVELID
                          join g in context.TBL_APPROVAL_GROUP on f.GROUPID equals g.GROUPID
                          join h in context.TBL_APPROVAL_GROUP_MAPPING on g.GROUPID equals h.GROUPID
                          join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                          join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID
                          join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                          where a.COMPANYID == companyId
                          select new WorkflowTrackerViewModel

                          {
                              arrivalDate = a.ARRIVALDATE,
                              responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? f.LEVELNAME : "N/A",
                              responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                              systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                              systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                              responseStaffName = !a.RESPONSESTAFFID.HasValue ? "Awaiting Action" : j.FIRSTNAME + " " + j.LASTNAME,
                              comment = a.COMMENT,
                              requestStaffName = i.FIRSTNAME + " " + i.LASTNAME,
                              requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : b.LEVELNAME,
                              TargetId = a.TARGETID,
                              operationId = e.OPERATIONID,
                              operationName = e.OPERATIONNAME,
                              approvalStatus = k.APPROVALSTATUSNAME
                          });
            return result;
        }

        public async Task<IEnumerable<WorkflowTrackerViewModel>> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            var result = await GetApprovalTrail(companyId).Where(c => c.TargetId == targetId && c.operationId == operationId).OrderByDescending(c => c.systemArrivalDate).ToListAsync();
            return result;
        }

        public IQueryable<WorkflowTrackerViewModel> GetAllRecordsOnApprovalTrail(int companyId)
        {
            var result = GetApprovalTrail(companyId).OrderByDescending(c => c.systemArrivalDate);

            return result;
        }
    }
}