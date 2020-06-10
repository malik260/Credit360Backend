using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using OfficeOpenXml;
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
        private IWorkflow workflow;
        private IAdminRepository admin;
        private object divisionName;

        public ApprovalLevelStaffRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail,
                                                       IWorkflow _workflow,
                                                         IAdminRepository _admin)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
            workflow = _workflow;
            admin = _admin;
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
                            canViewDocument = a.CANVIEWDOCUMENT,
                            canViewUploadedFile = a.CANVIEWUPLOAD,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            canUploadFile = a.CANUPLOAD,
                            //canSendRequest = a.CANSENDJOBREQUEST,
                            canEdit = a.CANEDIT,
                            vetoPower = a.VETOPOWER,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.POSITION,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            staffId = a.STAFFID,
                            staffLevelId = a.STAFFLEVELID,// added
                            staffLevelName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        })
                        .OrderBy(x => x.position)
                        .ToList()
                        ;
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
                            canViewDocument = a.CANVIEWDOCUMENT,
                            canViewUploadedFile = a.CANVIEWUPLOAD,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            canUploadFile = a.CANUPLOAD,
                            //canSendRequest = a.CANSENDJOBREQUEST,
                            canEdit = a.CANEDIT,
                            vetoPower = a.VETOPOWER,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.POSITION,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                            staffId = a.STAFFID,
                            staffLevelId = a.STAFFLEVELID,// added
                            staffLevelName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY
                        }).ToList();

            var data2 = (from a in context.TBL_STAFF_ROLE
                         join e in context.TBL_STAFF on a.STAFFROLEID equals e.STAFFROLEID
                         join b in context.TBL_APPROVAL_LEVEL on e.STAFFROLEID equals b.STAFFROLEID
                         join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                         join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                         where c.COMPANYID == companyId
                         && e.DELETED == false
                         select new ApprovalLevelStaffViewModel
                         {
                             groupId = (int)b.GROUPID,
                             operationId = d.OPERATIONID,
                             maximumAmount = b.MAXIMUMAMOUNT,
                             //processViewScope = b.PROCESSVIEWSCOPEID,
                             //canViewDocument = b.CANVIEWDOCUMENT,
                             //canViewUploadedFile = b.CANVIEWUPLOAD,
                             //canViewApproval = b.CANVIEWAPPROVAL,
                             //canApprove = b.CANAPPROVE,
                             //canUploadFile = b.CANUPLOAD,
                             //canSendRequest = a.CANSENDJOBREQUEST,
                             //canEdit = b.CANEDIT,
                             //vetoPower = b.VETOPOWER,
                             //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                             position = b.POSITION,
                             approvalLevelId = b.APPROVALLEVELID,
                             approvalLevelName = b.LEVELNAME,
                             staffId = e.STAFFID,
                             staffLevelId = b.APPROVALLEVELID,
                             staffRoleId = a.STAFFROLEID,// added
                             staffLevelName = e.FIRSTNAME + " " + e.MIDDLENAME + " " + e.LASTNAME,
                             //dateTimeCreated = e.DATETIMECREATED,
                             //createdBy = (int)e.CREATEDBY
                         }).ToList();

            return data.Union(data2);
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
            var data = GetApprovalLevelStaff(companyId).Where(c => c.operationId == operationId);
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
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                var data = new TBL_APPROVAL_LEVEL_STAFF
                {
                    MAXIMUMAMOUNT = model.maximumAmount,
                    STAFFID = model.staffId,
                    APPROVALLEVELID = model.approvalLevelId,
                    POSITION = model.position,
                    PROCESSVIEWSCOPEID = (short)model.processViewScope,
                    CANVIEWDOCUMENT = model.canViewDocument,
                    CANVIEWUPLOAD = model.canViewUploadedFile,
                    CANVIEWAPPROVAL = model.canViewApproval,
                    CANAPPROVE = model.canApprove,
                    CANUPLOAD = model.canUploadFile,
                    //CANSENDJOBREQUEST = model.canSendRequest,
                    CANEDIT = model.canEdit,
                    VETOPOWER = model.vetoPower,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    CREATEDBY = (int)model.createdBy,
                    DELETED = false

                };
                context.TBL_APPROVAL_LEVEL_STAFF.Add(data);

                var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
                var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));
                var admin = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' is was created by the super-admin with id {admin}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };


                this.auditTrail.AddAuditTrail(audit);

            }
            else
            {
                var data = new TBL_TEMP_APPROVAL_LEVEL_STAFF
                {
                    MAXIMUMAMOUNT = model.maximumAmount,
                    STAFFID = model.staffId,
                    APPROVALLEVELID = model.approvalLevelId,
                    POSITION = model.position,
                    PROCESSVIEWSCOPEID = (short)model.processViewScope,
                    CANVIEWDOCUMENT = model.canViewDocument,
                    CANVIEWUPLOAD = model.canViewUploadedFile,
                    CANVIEWAPPROVAL = model.canViewApproval,
                    CANAPPROVE = model.canApprove,
                    CANUPLOAD = model.canUploadFile,
                    //CANSENDJOBREQUEST = model.canSendRequest,
                    CANEDIT = model.canEdit,
                    VETOPOWER = model.vetoPower,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    CREATEDBY = (int)model.createdBy,
                    OPERATION = "create",
                    DELETED = false

                };
                context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Add(data);

                if (context.SaveChanges() > 0)
                {
                    model.tempStaffLevelId = data.TEMPSTAFFLEVELID;
                }

                // Audit Section ---------------------------
                var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
                var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.tempStaffLevelId;
                workflow.Comment = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' has been created and is going for approvals .";
                workflow.OperationId = (int)OperationsEnum.ApprovalLevelStaffModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' has been created and is going for approvals",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };


                this.auditTrail.AddAuditTrail(audit);
            }

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ApprovalLevelStaffViewModel> GetTempApprovalLevelStaff(int staffId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ApprovalLevelStaffModification).ToList();


            var data = (from a in context.TBL_TEMP_APPROVAL_LEVEL_STAFF
                        join atrail in context.TBL_APPROVAL_TRAIL on a.TEMPSTAFFLEVELID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.ApprovalLevelStaffModification
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
                        select new ApprovalLevelStaffViewModel
                        {
                            maximumAmount = a.MAXIMUMAMOUNT,
                            processViewScope = a.PROCESSVIEWSCOPEID,
                            canViewDocument = a.CANVIEWDOCUMENT,
                            canViewUploadedFile = a.CANVIEWUPLOAD,
                            canViewApproval = a.CANVIEWAPPROVAL,
                            canApprove = a.CANAPPROVE,
                            canUploadFile = a.CANUPLOAD,
                            //canSendRequest = a.CANSENDJOBREQUEST,
                            canEdit = a.CANEDIT,
                            vetoPower = a.VETOPOWER,
                            //minimumAmount = a.tbl_Approval_Level.MaximumAmount,
                            position = a.POSITION,
                            approvalLevelId = a.APPROVALLEVELID,
                            approvalLevelName = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == a.APPROVALLEVELID).Select(x => x.LEVELNAME).FirstOrDefault(),
                            staffId = a.STAFFID,
                            staffLevelId = a.STAFFLEVELID,// added
                            staffLevelName = context.TBL_STAFF.Where(x => x.STAFFID == a.STAFFID).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault(),
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = (int)a.CREATEDBY,
                            tempStaffLevelId = a.TEMPSTAFFLEVELID
                        })
                        .OrderBy(x => x.position)
                        .ToList()
                        ;
            return data;
        }
        public bool UpdateApprovalLevelStaff(int StaffLevelId, ApprovalLevelStaffViewModel model)
        {
            if (admin.IsSuperAdmin(model.createdBy) == true)
            {
                var data = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFLEVELID == StaffLevelId).Select(x => x).FirstOrDefault();
                if (data == null) return false;

                data.MAXIMUMAMOUNT = model.maximumAmount;
                data.STAFFID = model.staffId;
                data.APPROVALLEVELID = model.approvalLevelId;
                data.POSITION = model.position;
                data.PROCESSVIEWSCOPEID = (short)model.processViewScope;
                data.CANVIEWDOCUMENT = model.canViewDocument;
                data.CANVIEWUPLOAD = model.canViewUploadedFile;
                data.CANVIEWAPPROVAL = model.canViewApproval;
                data.CANAPPROVE = model.canApprove;
                data.CANUPLOAD = model.canUploadFile;
                data.CANEDIT = model.canEdit;
                data.VETOPOWER = model.vetoPower;
                data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
                data.LASTUPDATEDBY = (int)model.createdBy;

                var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == StaffLevelId));
                var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == data.STAFFID).Select(x => x.STAFFCODE));
                var admin = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));


                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Approval Level for staff with code '{audit_staff}' to level {model.staffLevelName}' was updated by this super-admin {admin}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);

            }
            else
            {
                if (model != null)
                {
                    var data = new TBL_TEMP_APPROVAL_LEVEL_STAFF
                    {
                        MAXIMUMAMOUNT = model.maximumAmount,
                        STAFFID = model.staffId,
                        APPROVALLEVELID = model.approvalLevelId,
                        POSITION = model.position,
                        PROCESSVIEWSCOPEID = (short)model.processViewScope,
                        CANVIEWDOCUMENT = model.canViewDocument,
                        CANVIEWUPLOAD = model.canViewUploadedFile,
                        CANVIEWAPPROVAL = model.canViewApproval,
                        CANAPPROVE = model.canApprove,
                        CANUPLOAD = model.canUploadFile,
                        CANEDIT = model.canEdit,
                        VETOPOWER = model.vetoPower,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        CREATEDBY = (int)model.createdBy,
                        OPERATION = "update",
                        DELETED = false
                    };
                    context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Add(data);

                    if (context.SaveChanges() > 0)
                    {
                        model.tempStaffLevelId = data.TEMPSTAFFLEVELID;
                    }

                    var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == StaffLevelId));
                    var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = model.tempStaffLevelId;
                    workflow.Comment = $"Approval Level for staff with code '{audit_staff.STAFFCODE}' to level {model.staffLevelName}' is updated and is going for approval";
                    workflow.OperationId = (int)OperationsEnum.ApprovalLevelStaffModification;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffUpdated,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Approval Level for staff with code '{audit_staff.STAFFCODE}' to level {model.staffLevelName}' is updated and is going for approval",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.staffLevelId,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);

                    //end of Audit section -------------------------------
                }
            }

            return context.SaveChanges() != 0;
        }

        public async Task<bool> DeleteApprovalLevelStaff(int StaffLevelId, UserInfo user)
        {
            int tempStaffLevelId = 0;

            var model = this.context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFLEVELID == StaffLevelId).Select(x => x).FirstOrDefault();
            //var dataExist = context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.MAXIMUMAMOUNT == user. && x.OPERATIONID == data.OPERATIONID).Any();

            //if (dataExist)
            //    throw new ConditionNotMetException("This operation has already been initiated and is apprival pending");

           

            if (model != null)
            {
                if (admin.IsSuperAdmin(user.createdBy) == true)
                {
                    model.DATETIMEDELETED = _genSetup.GetApplicationDate();
                    model.DELETEDBY = (int)model.CREATEDBY;
                    model.DELETED = true;

                    var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == model.APPROVALLEVELID));
                    var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == user.createdBy));
                    var admin = (context.TBL_STAFF.Where(x => x.STAFFID == model.STAFFID).Select(x => x.STAFFCODE));

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                        STAFFID = user.createdBy,
                        BRANCHID = (short)user.BranchId,
                        DETAIL = $"Added Approval Level Staff {audit_staff_level.LEVELNAME}' for staff with code '{audit_staff.STAFFCODE}' ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = user.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.STAFFLEVELID,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);
                }
                else
                {

                    var data = new TBL_TEMP_APPROVAL_LEVEL_STAFF
                    {
                        MAXIMUMAMOUNT = model.MAXIMUMAMOUNT,
                        STAFFID = model.STAFFID,
                        APPROVALLEVELID = model.APPROVALLEVELID,
                        POSITION = model.POSITION,
                        PROCESSVIEWSCOPEID = (short)model.PROCESSVIEWSCOPEID,
                        CANVIEWDOCUMENT = model.CANVIEWDOCUMENT,
                        CANVIEWUPLOAD = model.CANVIEWUPLOAD,
                        CANVIEWAPPROVAL = model.CANVIEWAPPROVAL,
                        CANAPPROVE = model.CANAPPROVE,
                        CANUPLOAD = model.CANUPLOAD,
                        //CANSENDJOBREQUEST = model.canSendRequest,
                        CANEDIT = model.CANEDIT,
                        VETOPOWER = model.VETOPOWER,
                        DATETIMECREATED = _genSetup.GetApplicationDate(),
                        CREATEDBY = (int)model.CREATEDBY,
                        OPERATION = "delete",
                        DELETED = false
                    };
                    context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Add(data);

                    if (context.SaveChanges() > 0)
                    {
                        tempStaffLevelId = data.TEMPSTAFFLEVELID;
                    }

                    //Audit Section ---------------------------
                    var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
                    var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

                    workflow.StaffId = data.CREATEDBY;
                    workflow.CompanyId = 1;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = tempStaffLevelId;
                    workflow.Comment = $"Approval Level for staff with code '{audit_staff.STAFFCODE}' to level {model.STAFFLEVELID}' is delete and the action is going for approval";
                    workflow.OperationId = (int)OperationsEnum.ApprovalLevelStaffModification;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();


                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                        STAFFID = user.createdBy,
                        BRANCHID = (short)user.BranchId,
                        DETAIL = $"Approval Level for staff with code '{audit_staff.STAFFCODE}' to level {model.STAFFLEVELID}' is delete and the action is going for approval ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = user.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = data.STAFFLEVELID,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.auditTrail.AddAuditTrail(audit);
                }

                //end of Audit section -------------------------------

            }
            return await context.SaveChangesAsync() != 0;
        }

        private void UpdateMainApprovalLevelStaff(ApprovalLevelStaffViewModel ApprovalModel, short status)
        {
            var data = this.context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Where(x => x.TEMPSTAFFLEVELID == ApprovalModel.tempStaffLevelId).Select(x => x).FirstOrDefault();
            if (data != null)
            {
                var audit_staff_level = (context.TBL_APPROVAL_LEVEL.FirstOrDefault(x => x.APPROVALLEVELID == data.APPROVALLEVELID));
                var audit_staff = (context.TBL_STAFF.FirstOrDefault(x => x.STAFFID == data.STAFFID));

                var audit = new TBL_AUDIT();

                if (data.OPERATION == "create")
                {
                    ApprovalLevelStaffCreation(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffAdded;
                    audit.DETAIL = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' has been added successfully";

                }
                else if (data.OPERATION == "update")
                {
                    ApprovalLevelStaffUpdate(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffUpdated;
                    audit.DETAIL = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' has been update successfully";
                }
                else if (data.OPERATION == "delete")
                {
                    ApprovalLevelStaffDelete(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelStaffDeleted;
                    audit.DETAIL = $"The Approval-Level '{audit_staff_level?.LEVELNAME}' for user code '{audit_staff?.STAFFCODE}' has been delete successfully";
                }

                TempApprovalLevelStaffUpdate(ApprovalModel, status);

                audit.STAFFID = ApprovalModel.createdBy;
                audit.BRANCHID = (short)ApprovalModel.userBranchId;
                audit.IPADDRESS = ApprovalModel.userIPAddress;
                audit.URL = ApprovalModel.applicationUrl;
                audit.APPLICATIONDATE = _genSetup.GetApplicationDate();
                audit.SYSTEMDATETIME = DateTime.Now;
                audit.TARGETID = ApprovalModel.staffLevelId;

                context.TBL_AUDIT.Add(audit);
            }
        }

        private void TempApprovalLevelStaffUpdate(ApprovalLevelStaffViewModel approvalModel, short status)
        {
            var update = context.TBL_TEMP_APPROVAL_LEVEL_STAFF.Where(x => x.TEMPSTAFFLEVELID == approvalModel.tempStaffLevelId).Select(x => x).FirstOrDefault();
            if (update != null)
            {
                update.APPROVALSTATUSID = status;
            }
        }

        private void ApprovalLevelStaffDelete(TBL_TEMP_APPROVAL_LEVEL_STAFF model)
        {
            var data = new TBL_APPROVAL_LEVEL_STAFF
            {
                DATETIMEDELETED = model.DATETIMECREATED,
                DELETEDBY = (int)model.CREATEDBY,
                DELETED = true
            };
        }

        private void ApprovalLevelStaffUpdate(TBL_TEMP_APPROVAL_LEVEL_STAFF model)
        {
            var data = new TBL_APPROVAL_LEVEL_STAFF
            {
                MAXIMUMAMOUNT = model.MAXIMUMAMOUNT,
                STAFFID = model.STAFFID,
                APPROVALLEVELID = model.APPROVALLEVELID,
                POSITION = model.POSITION,
                PROCESSVIEWSCOPEID = (short)model.PROCESSVIEWSCOPEID,
                CANVIEWDOCUMENT = model.CANVIEWDOCUMENT,
                CANVIEWUPLOAD = model.CANVIEWUPLOAD,
                CANVIEWAPPROVAL = model.CANVIEWAPPROVAL,
                CANAPPROVE = model.CANAPPROVE,
                CANUPLOAD = model.CANUPLOAD,
                CANEDIT = model.CANEDIT,
                VETOPOWER = model.VETOPOWER,
                DATETIMEUPDATED = model.DATETIMECREATED,
                LASTUPDATEDBY = (int)model.CREATEDBY,
                DELETED = false
            };
        }

        private void ApprovalLevelStaffCreation(TBL_TEMP_APPROVAL_LEVEL_STAFF model)
        {
            var data = new TBL_APPROVAL_LEVEL_STAFF
            {
                MAXIMUMAMOUNT = model.MAXIMUMAMOUNT,
                STAFFID = model.STAFFID,
                APPROVALLEVELID = model.APPROVALLEVELID,
                POSITION = model.POSITION,
                PROCESSVIEWSCOPEID = (short)model.PROCESSVIEWSCOPEID,
                CANVIEWDOCUMENT = model.CANVIEWDOCUMENT,
                CANVIEWUPLOAD = model.CANVIEWUPLOAD,
                CANVIEWAPPROVAL = model.CANVIEWAPPROVAL,
                CANAPPROVE = model.CANAPPROVE,
                CANUPLOAD = model.CANUPLOAD,
                CANEDIT = model.CANEDIT,
                VETOPOWER = model.VETOPOWER,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                CREATEDBY = (int)model.CREATEDBY,
                DELETED = false
            };

            context.TBL_APPROVAL_LEVEL_STAFF.Add(data);
        }

        public int GoForApproval(ApprovalLevelStaffViewModel model)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.tempStaffLevelId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.ApprovalLevelStaffModification;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (model.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            UpdateMainApprovalLevelStaff(model, (short)workflow.StatusId);
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
        #region ALIEN CODE BLOCKS

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
                          join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                          from j in apprStaff.DefaultIfEmpty()
                          join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                          where a.COMPANYID == companyId
                          select new WorkflowTrackerViewModel

                          {
                              arrivalDate = a.ARRIVALDATE,
                              responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? f.LEVELNAME : "N/A",
                              responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                              systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                              systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                              //responseStaffName = !a.RESPONSESTAFFID.HasValue ? "Awaiting Action" : j.FIRSTNAME + " " + j.LASTNAME,
                              comment = a.COMMENT,
                              // = i.FIRSTNAME + " " + i.LASTNAME,
                              requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : b.LEVELNAME,
                              TargetId = a.TARGETID,
                              operationId = e.OPERATIONID,
                              operationName = e.OPERATIONNAME,
                              approvalStatus = k.APPROVALSTATUSNAME
                          });
            var vr = result.Distinct();
            var vr2 = vr.ToList();
            return result;
        }

        public IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId)
        {
            var result = GetApprovalTrail(companyId).Where(c=>c.TargetId==targetId && c.operationId==operationId).OrderByDescending(c => c.systemArrivalDate).ToList();
            return result;
        }



        public IQueryable<WorkflowTrackerViewModel> GetAllRecordsOnApprovalTrail(int companyId)
        {
            var result = GetApprovalTrail(companyId).OrderByDescending(c => c.systemArrivalDate);

            return result;
        }

        public List<WorkflowTrackerViewModel> GetAllApprovalStatus()
        {
            var result = from x in context.TBL_APPROVAL_STATUS
                         select (new WorkflowTrackerViewModel
                         {
                             approvalStatusId = x.APPROVALSTATUSID,
                             approvalStatus = x.APPROVALSTATUSNAME
                         });

            return result.ToList();
        }

        public List<WorkflowTrackerViewModel> GetAllApprovalOperations()
        {
            var result = from x in context.TBL_OPERATIONS
                         select (new WorkflowTrackerViewModel
                         {
                             operationId = x.OPERATIONID,
                             operationName = x.OPERATIONNAME
                         });

            return result.ToList();
        }

        public List<WorkflowTrackerViewModel> GetApprovalMointoring(DateRange param)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var disbursedLoans = (from a in context.TBL_LOAN
                                      join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                      select b.LOANAPPLICATIONID).ToList();
                List<int> operations = new List<int> { (int)OperationsEnum.CreditAppraisal, (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.LoanAvailment };
                operations.AddRange(context.TBL_LOAN_APPLICATION.Select(x => x.OPERATIONID).Distinct());

                int[] approvals = new int[] { (int)ApprovalStatusEnum.Approved, (int)ApprovalStatusEnum.Disapproved,  }; //(int)ApprovalStatusEnum.Authorised
                List<WorkflowTrackerViewModel> approvalRecord = new List<WorkflowTrackerViewModel>();

                //var recordApproved = (from a in context.TBL_APPROVAL_TRAIL
                //                  join b in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals b.APPROVALSTATUSID
                //                  join c in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals c.APPROVALSTATEID
                //              join d in context.TBL_LOAN_APPLICATION on a.TARGETID equals d.LOANAPPLICATIONID
                //              where (DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) >= DbFunctions.TruncateTime(param.startDate)
                //                 && DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) <= DbFunctions.TruncateTime(param.endDate)) &&
                //                 a.OPERATIONID == (int)OperationsEnum.LoanAvailment && a.RESPONSESTAFFID == null && approvals.Contains(a.APPROVALSTATUSID)
                //                 && !disbursedLoans.Contains(d.LOANAPPLICATIONID)
                //              select (new WorkflowTrackerViewModel
                //              { 
                //                  approvalStatusId = a.APPROVALSTATUSID,
                //                  operationName = a.TBL_OPERATIONS.OPERATIONNAME,
                //                  currentLevel = context.TBL_APPROVAL_LEVEL.Where(cl => cl.APPROVALLEVELID == a.TOAPPROVALLEVELID).Select(rec => rec.LEVELNAME).FirstOrDefault(),
                //                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                //                  approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                //                  applicationReferenceNumber = d.APPLICATIONREFERENCENUMBER,
                //                  requestStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                //                  responseStaffName = a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                //                  comment = a.COMMENT.ToString(),
                //                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                //                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                //                  requestApprovalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                //                  responseApprovalLevel = a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                //                  customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                //                  //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID == d.CUSTOMERID).Select(cu=>cu.LASTNAME + " " + cu.MIDDLENAME + cu.LASTNAME).FirstOrDefault(),
                //                  responseDate = a.RESPONSEDATE.HasValue ? (DateTime)a.RESPONSEDATE : (DateTime)(DateTime.Now),
                //                  arrivalDate = a.ARRIVALDATE,
                //                  applicationDate = d.SYSTEMDATETIME,
                //                 // amount = d.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == d.LOANAPPLICATIONID).Select(x => x.APPROVEDAMOUNT).FirstOrDefault(),
                //                //  amount = context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONID == d.LOANAPPLICATIONID).Sum(x=>x.APPROVEDAMOUNT * Convert.ToDecimal( x.EXCHANGERATE)),
                //                  TargetId = a.TARGETID,
                //                  branchName = d.TBL_BRANCH.BRANCHNAME + " (" + d.TBL_BRANCH.BRANCHCODE + ") ",
                //                  loanApplicationId = d.LOANAPPLICATIONID,
                //                  approvalTrailId = a.APPROVALTRAILID,

                //              })
                // ).ToList().Select(x =>
                // {
                //     // x.amount = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONID == x.loanApplicationId).Sum(m => m.APPROVEDAMOUNT *(decimal) m.EXCHANGERATE);
                //     x.amount = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONID == x.loanApplicationId).Sum(m => m.APPROVEDAMOUNT);

                //     return x;

                // }).OrderBy(o => o.approvalTrailId);
                var currentDate = DateTime.Now;

                var record = (from a in context.TBL_APPROVAL_TRAIL
                              join b in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals b.APPROVALSTATUSID
                              join c in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals c.APPROVALSTATEID
                              join d in context.TBL_LOAN_APPLICATION on a.TARGETID equals d.LOANAPPLICATIONID
                              join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                              where (DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) >= DbFunctions.TruncateTime(param.startDate)
                                 && DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) <= DbFunctions.TruncateTime(param.endDate))
                                 && operations.Contains(a.OPERATIONID)
                                 && a.RESPONSESTAFFID == null
                                 && d.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
                                && d.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                                && !approvals.Contains(a.APPROVALSTATUSID)
                                && !disbursedLoans.Contains(d.LOANAPPLICATIONID)
                                && a.APPROVALSTATEID != (int)ApprovalState.Ended 
                               select (new WorkflowTrackerViewModel
                              {
                                  approvalStatusId = a.APPROVALSTATUSID,
                                  operationName = a.TBL_OPERATIONS.OPERATIONNAME,
                                  currentLevel = context.TBL_APPROVAL_LEVEL.Where(cl => cl.APPROVALLEVELID == a.TOAPPROVALLEVELID).Select(rec => rec.LEVELNAME).FirstOrDefault(),
                                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                                  applicationReferenceNumber = d.APPLICATIONREFERENCENUMBER,
                                  requestStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                                  responseStaffName = a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                  comment = a.COMMENT,
                                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                  requestApprovalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  responseApprovalLevel = a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                  divisionCode =  (from p in context.TBL_PROFILE_BUSINESS_UNIT join  t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(), 
                                  divisionName = (from p in context.TBL_PROFILE_BUSINESS_UNIT join t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault(), 
                                  responseDate = a.RESPONSEDATE.HasValue ? (DateTime)a.RESPONSEDATE : (DateTime)(DateTime.Now),
                                  arrivalDate = a.ARRIVALDATE,
                                  applicationDate = d.SYSTEMDATETIME,
                                  customerDivisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                  TargetId = a.TARGETID,
                                  branchName = d.TBL_BRANCH.BRANCHNAME + " (" + d.TBL_BRANCH.BRANCHCODE + ") ",
                                  loanApplicationId = d.LOANAPPLICATIONID,
                                  approvalTrailId = a.APPROVALTRAILID,

                              })
                              ).ToList()
                              
                              .OrderBy(o => o.approvalTrailId);

                var records = record.OrderByDescending(o => o.approvalTrailId);

                int serial = 1;
                foreach (var item in records.ToList())
                {
                    var ProductName = "";
                    int count = serial;
                    var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == item.loanApplicationId).ToList();
                    decimal amount = 0;
                    foreach (var rec in loanDetails.ToList())
                    {
                        ProductName += rec.TBL_PRODUCT.PRODUCTNAME + ",";
                        amount += (decimal)rec.APPROVEDAMOUNT * (decimal)rec.EXCHANGERATE;
                    }
                    serial += 1;
                    item.productNames = ProductName;
                    item.amount = amount;
                    item.serial = count;
                    approvalRecord.Add(item);
                }

                return approvalRecord.OrderBy(o => o.serial).ToList();
            }
        }

        public List<WorkflowTrackerViewModel> GetBookingMointoring(DateRange param)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                int[] operations = new int[] {
                    (int)OperationsEnum.TermLoanBooking,
                    (int)OperationsEnum.IndividualDrawdownRequest,
                    (int)OperationsEnum.CorporateDrawdownRequest,
                    (int)OperationsEnum.CreditCardDrawdownRequest,
                    (int)OperationsEnum.CRMSApproval,
                    (int)OperationsEnum.CommercialLoanBooking,
                    (int)OperationsEnum.ForeignExchangeLoanBooking,
                    (int)OperationsEnum.RevolvingLoanBooking,
                    (int)OperationsEnum.ContigentLoanBooking};
                int[] approvals = new int[] { (int)ApprovalStatusEnum.Approved, (int)ApprovalStatusEnum.Disapproved }; //(int)ApprovalStatusEnum.Authorised
                List<WorkflowTrackerViewModel> approvalRecord = new List<WorkflowTrackerViewModel>();



                var preBookingrecord = (from a in context.TBL_APPROVAL_TRAIL
                                        join b in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals b.APPROVALSTATUSID
                                        join c in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals c.APPROVALSTATEID
                                        join e in context.TBL_LOAN_APPLICATION on a.TARGETID equals e.LOANAPPLICATIONID
                                        join d in context.TBL_LOAN_APPLICATION_DETAIL on e.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                        join r in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                        join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                                        where a.OPERATIONID == (int)OperationsEnum.LoanAvailment
                                           && a.RESPONSESTAFFID == null
                                           && a.APPROVALSTATEID == (int)ApprovalState.Ended
                                           && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                           && ((e.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.AvailmentCompleted)
                                            || (e.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestInitiated)
                                            || (e.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BookingRequestCompleted)
                                            || (e.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingInProgress)
                                            || (e.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LoanBookingCompleted))
                                            && e.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
                                            && e.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted

                                        //&& a.OPERATIONID == (param.operationId == -1 ? a.OPERATIONID : param.operationId) 
                                       // && a.APPROVALSTATUSID == (param.approvalStatus == -1 ? a.APPROVALSTATUSID : param.approvalStatus )
                                        select (new WorkflowTrackerViewModel
                                        {
                                            approvalStatusId = a.APPROVALSTATUSID,
                                            loanBookingRequestId = r.LOAN_BOOKING_REQUESTID,
                                            operationName = a.TBL_OPERATIONS.OPERATIONNAME,
                                            currentLevel = "Booking Initiation",//context.TBL_APPROVAL_LEVEL.Where(cl => cl.APPROVALLEVELID == a.TOAPPROVALLEVELID).Select(rec => rec.LEVELNAME).FirstOrDefault(),
                                            approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                            approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                                            applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                            requestStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                                            responseStaffName = a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                            comment = a.COMMENT,
                                            systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                            systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                            requestApprovalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                            responseApprovalLevel = a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                            customerName = d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                            //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                            divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                                            divisionName = (from p in context.TBL_PROFILE_BUSINESS_UNIT join t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault(),
                                            //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID == d.CUSTOMERID).Select(cu=>cu.LASTNAME + " " + cu.MIDDLENAME + cu.LASTNAME).FirstOrDefault(),
                                            customerDivisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                            responseDate = a.RESPONSEDATE.HasValue ? (DateTime)a.RESPONSEDATE : (DateTime)(DateTime.Now),
                                            arrivalDate = a.ARRIVALDATE,
                                            applicationDate = e.SYSTEMDATETIME,
                                            amount = d.APPROVEDAMOUNT,
                                            TargetId = a.TARGETID,
                                            branchName = e.TBL_BRANCH.BRANCHNAME + " (" + e.TBL_BRANCH.BRANCHCODE + ") ",
                                            loanApplicationId = d.LOANAPPLICATIONID,
                                            loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                            //dueDate = (DateTime.Now.Date - a.SYSTEMARRIVALDATETIME.Date).Days,
                                            approvalTrailId = a.APPROVALTRAILID,
                                            productNames = context.TBL_PRODUCT.Where(u => u.PRODUCTID == d.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                        })
                           ).OrderByDescending(o => o.approvalTrailId);


                var record = (from a in context.TBL_APPROVAL_TRAIL
                              join b in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals b.APPROVALSTATUSID
                              join c in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals c.APPROVALSTATEID
                              join bo in context.TBL_LOAN_BOOKING_REQUEST on a.TARGETID equals bo.LOAN_BOOKING_REQUESTID
                              join d in context.TBL_LOAN_APPLICATION_DETAIL on bo.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                              join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                              join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                              where (DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) >= DbFunctions.TruncateTime(param.startDate)
                                 && DbFunctions.TruncateTime(a.SYSTEMARRIVALDATETIME) <= DbFunctions.TruncateTime(param.endDate)) &&
                                 operations.Contains(a.OPERATIONID) && a.RESPONSESTAFFID == null && !approvals.Contains(a.APPROVALSTATUSID)
                                 && a.APPROVALSTATEID != (int)ApprovalState.Ended
                                 && e.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationCompleted
                                 && e.APPLICATIONSTATUSID != (short)LoanApplicationStatusEnum.CancellationInProgress
                              // a.OPERATIONID == (param.operationId == -1 ? a.OPERATIONID : param.operationId) 
                              //&& a.APPROVALSTATUSID == (param.approvalStatus == -1 ? a.APPROVALSTATUSID : param.approvalStatus )
                              select (new WorkflowTrackerViewModel
                              {
                                  approvalStatusId = a.APPROVALSTATUSID,
                                  operationName = a.TBL_OPERATIONS.OPERATIONNAME,
                                  currentLevel = "Booking Initiation",//context.TBL_APPROVAL_LEVEL.Where(cl => cl.APPROVALLEVELID == a.TOAPPROVALLEVELID).Select(rec => rec.LEVELNAME).FirstOrDefault(),
                                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                                  applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                  requestStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                                  responseStaffName = a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                  comment = a.COMMENT,
                                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                  requestApprovalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  responseApprovalLevel = a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  customerName = d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                  //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                  divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                                  divisionName = (from p in context.TBL_PROFILE_BUSINESS_UNIT join t in context.TBL_CUSTOMER on p.BUSINESSUNITID equals t.BUSINESSUNTID where t.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault(),
                                  //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID == d.CUSTOMERID).Select(cu=>cu.LASTNAME + " " + cu.MIDDLENAME + cu.LASTNAME).FirstOrDefault(),
                                  customerDivisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                  responseDate = a.RESPONSEDATE.HasValue ? (DateTime)a.RESPONSEDATE : (DateTime)(DateTime.Now),
                                  arrivalDate = a.ARRIVALDATE,
                                  applicationDate = e.SYSTEMDATETIME,
                                  amount = d.APPROVEDAMOUNT,
                                  TargetId = a.TARGETID,
                                  branchName = e.TBL_BRANCH.BRANCHNAME + " (" + e.TBL_BRANCH.BRANCHCODE + ") ",
                                  loanApplicationId = d.LOANAPPLICATIONID,
                                  loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,

                                  approvalTrailId = a.APPROVALTRAILID,
                                  productNames = context.TBL_PRODUCT.Where(u => u.PRODUCTID == d.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                              })
                           ).OrderByDescending(o => o.approvalTrailId);

                var test = preBookingrecord.ToList();

                var records = record; //.Union(preBookingrecord).OrderByDescending(a => a.approvalTrailId).ToList();

                int serial = 1;
                foreach (var item in records.ToList())
                {
                    int count = serial;

                    serial += 1;
                    item.serial = count;
                    approvalRecord.Add(item);
                }
                //var test = approvalRecord.ToList();

                return approvalRecord.OrderBy(o => o.serial).ToList();
            }
        }

        public List<WorkflowTrackerViewModel> GetApprovalTrailByTargetId(int targetId, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                // int[] operations = new int[] { 6, 37, 38 };
                var operations = context.TBL_LOAN_APPLICATION.Select(x => x.OPERATIONID).Distinct().ToList();
                

                var result = (from a in context.TBL_APPROVAL_TRAIL
                                  // join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                                  //  join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                                  // join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                                  // join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                                  // join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                                  //join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                                  // from j in apprStaff.DefaultIfEmpty()
                                  // join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                              where a.COMPANYID == companyId
                              where a.TARGETID == targetId && operations.Contains(a.OPERATIONID)
                              select new WorkflowTrackerViewModel
                              {
                                  arrivalDate = a.ARRIVALDATE,
                                  responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? a.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                                  responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                  responseStaffName = !a.TOAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  comment = a.COMMENT,
                                  requestStaffName = a.TBL_STAFF.FIRSTNAME != null ? a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME : null,
                                  requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  TargetId = a.TARGETID,
                                  // operationId = e.OPERATIONID,
                                  // operationName = e.OPERATIONNAME,
                                  //approvalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID == a.APPROVALSTATUSID).FirstOrDefault().APPROVALSTATUSNAME
                                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalTrailId = a.APPROVALTRAILID,
                              }).Distinct().OrderByDescending(i => i.approvalTrailId);


                var response = result.ToList();


                // var test = GetApprovalTrail(companyId).ToList();
                // var result = GetApprovalTrail(companyId).Where(c => c.TargetId == targetId && operations.Contains(c.operationId)).OrderByDescending(c => c.systemArrivalDate).ToList();
                return result.ToList();
            }
        }
        public List<WorkflowTrackerViewModel> GetBookingApprovalTrailByTargetId(int targetId, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //int[] operations = new int[] { 1, 39 };


                int[] operations = new int[] {
                    (int)OperationsEnum.TermLoanBooking,
                    (int)OperationsEnum.IndividualDrawdownRequest,
                    (int)OperationsEnum.CorporateDrawdownRequest,
                    (int)OperationsEnum.CreditCardDrawdownRequest,
                    (int)OperationsEnum.CRMSApproval,
                    (int)OperationsEnum.CommercialLoanBooking,
                    (int)OperationsEnum.ForeignExchangeLoanBooking,
                    (int)OperationsEnum.RevolvingLoanBooking,
                    (int)OperationsEnum.ContigentLoanBooking};


                var result = (from a in context.TBL_APPROVAL_TRAIL
                                  // join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                                  //  join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                                  // join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                                  // join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                                  // join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                                  //join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                                  // from j in apprStaff.DefaultIfEmpty()
                                  // join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                              where a.COMPANYID == companyId
                              where a.TARGETID == targetId && operations.Contains(a.OPERATIONID)
                              select new WorkflowTrackerViewModel
                              {
                                  arrivalDate = a.ARRIVALDATE,
                                  responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? a.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                                  responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                  responseStaffName = !a.TOAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  comment = a.COMMENT,
                                  requestStaffName = a.TBL_STAFF.FIRSTNAME != null ? a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME : null,
                                  requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  TargetId = a.TARGETID,
                                  // operationId = e.OPERATIONID,
                                  // operationName = e.OPERATIONNAME,
                                  //approvalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID == a.APPROVALSTATUSID).FirstOrDefault().APPROVALSTATUSNAME
                                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalTrailId = a.APPROVALTRAILID,
                              }).Distinct().OrderByDescending(i => i.approvalTrailId);


                var response = result.ToList();


                // var test = GetApprovalTrail(companyId).ToList();
                // var result = GetApprovalTrail(companyId).Where(c => c.TargetId == targetId && operations.Contains(c.operationId)).OrderByDescending(c => c.systemArrivalDate).ToList();
                return result.ToList();
            }
        }

        public WorkflowTrackerViewModel GenerateApprovalMonitoringReport(DateRange param)
        {

            return GenerateWorkflowRecord(param);

        }
        private WorkflowTrackerViewModel GenerateWorkflowRecord(DateRange param)
        {
            var record = GetApprovalMointoring(param).ToList();
            if (record == null)
                throw new ConditionNotMetException("Record Not Found For Download");

            return GenerateApprovalMonitoring(record.ToList(), param);
        }
        private WorkflowTrackerViewModel GenerateApprovalMonitoring(List<WorkflowTrackerViewModel> loanInput, DateRange param)
        {

            Byte[] fileBytes = null;
            WorkflowTrackerViewModel data = new WorkflowTrackerViewModel();

            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Pending Approvals");

                    ws.Cells[1, 1].Value = "S/N";
                    ws.Cells[1, 2].Value = "APPLICATION REFERENCE NUMBER";
                    ws.Cells[1, 3].Value = "APPLICATION DATE";
                    ws.Cells[1, 4].Value = "ARRIVAL DATE";
                    ws.Cells[1, 5].Value = "CUSTOMER NAME";
                    ws.Cells[1, 6].Value = "PRODUCT";
                    //ws.Cells[1, 7].Value = "AMOUNT";
                    ws.Cells[1, 7].Value = "BRANCH";
                    ws.Cells[1, 8].Value = "CURRENT LEVEL";
                    ws.Cells[1, 9].Value = "STATE";
                    ws.Cells[1, 10].Value = "FROM STAFF";
                    ws.Cells[1, 11].Value = "TO STAFF";
                    ws.Cells[1, 12].Value = "COMMENT_";
                    ws.Cells[1, 13].Value = "AMOUNT";


                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];
                        ws.Cells[i, 1].Value = record.serial;
                        ws.Cells[i, 2].Value = record.applicationReferenceNumber;
                        ws.Cells[i, 3].Value = record.applicationDate.ToString("dd/MM/yyyy");
                        ws.Cells[i, 4].Value = record.systemArrivalDate.Value.ToString("dd/MM/yyyy");
                        ws.Cells[i, 5].Value = record.customerName;
                        ws.Cells[i, 6].Value = record.productNames;
                        //ws.Cells[i, 7].Value = record.amount;
                        ws.Cells[i, 7].Value = record.branchName;
                        ws.Cells[i, 8].Value = record.currentLevel;
                        ws.Cells[i, 9].Value = record.approvalState;
                        ws.Cells[i, 10].Value = record.requestStaffName;
                        ws.Cells[i, 11].Value = record.responseStaffName;
                        ws.Cells[i, 12].Value = record.comment;
                        ws.Cells[i, 13].Value = record.amount;


                    }
                    //if (forSingle == 1)
                    //{
                    //    fee = GetFee100(param).Where(a => a.LOANAPPLICATIONDETAILID == param.loanId &&
                    //a.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Government).ToList();
                    //}
                    //else
                    //{
                    //    fee = GetFee100(param).Where(x => x.CRMSDATE >= param.startDate
                    // && x.CRMSDATE <= param.endDate &&
                    //x.CRMSLEGALSTATUSID == (int)CRMSRegulatory.Government).ToList();
                    //}

                    //ExcelWorksheet ws3 = pck.Workbook.Worksheets.Add("FEE");
                    //ws3.Cells[1, 1].Value = "ACCOUNT";
                    //ws3.Cells[1, 2].Value = "FEE_TYPE";
                    //ws3.Cells[1, 3].Value = "FEE_AMOUNT";


                    //for (int i = 2; i <= fee.Count + 1; i++)
                    //{
                    //    var feeRecord = fee[i - 2];

                    //    ws3.Cells[i, 1].Value = feeRecord.ACCOUNT;
                    //    ws3.Cells[i, 2].Value = feeRecord.FEE_TYPE_NAME;
                    //    ws3.Cells[i, 3].Value = feeRecord.FEE_AMOUNT;

                    //}

                    var booking = GetBookingMointoring(param).ToList();

                    ExcelWorksheet ws3 = pck.Workbook.Worksheets.Add("Booking Approvals");
                    ws3.Cells[1, 1].Value = "S/N";
                    ws3.Cells[1, 2].Value = "APPLICATION REFERENCE NUMBER";
                    ws3.Cells[1, 3].Value = "APPLICATION DATE";
                    ws3.Cells[1, 4].Value = "ARRIVAL DATE";
                    ws3.Cells[1, 5].Value = "CUSTOMER NAME";
                    ws3.Cells[1, 6].Value = "PRODUCT";
                    //ws.Cells[1, 7].Value = "AMOUNT";
                    ws3.Cells[1, 7].Value = "BRANCH";
                    ws3.Cells[1, 8].Value = "CURRENT LEVEL";
                    ws3.Cells[1, 9].Value = "STATE";
                    ws3.Cells[1, 10].Value = "FROM STAFF";
                    ws3.Cells[1, 11].Value = "TO STAFF";
                    ws3.Cells[1, 12].Value = "COMMENT_";
                    ws3.Cells[1, 13].Value = "AMOUNT";


                    for (int i = 2; i <= booking.Count + 1; i++)
                    {
                        var bookingRecord = booking[i - 2];

                        ws3.Cells[i, 1].Value = bookingRecord.serial;
                        ws3.Cells[i, 2].Value = bookingRecord.applicationReferenceNumber;
                        ws3.Cells[i, 3].Value = bookingRecord.applicationDate.ToString("dd/MM/yyyy");
                        ws3.Cells[i, 4].Value = bookingRecord.systemArrivalDate.Value.ToString("dd/MM/yyyy");
                        ws3.Cells[i, 5].Value = bookingRecord.customerName;
                        ws3.Cells[i, 6].Value = bookingRecord.productNames;
                        //3ws.Cells[i, 7].Value = record.amount;
                        ws3.Cells[i, 7].Value = bookingRecord.branchName;
                        ws3.Cells[i, 8].Value = bookingRecord.currentLevel;
                        ws3.Cells[i, 9].Value = bookingRecord.approvalState;
                        ws3.Cells[i, 10].Value = bookingRecord.requestStaffName;
                        ws3.Cells[i, 11].Value = bookingRecord.responseStaffName;
                        ws3.Cells[i, 12].Value = bookingRecord.comment;
                        ws3.Cells[i, 13].Value = bookingRecord.amount;

                    }


                    fileBytes = pck.GetAsByteArray();
                    data.reportData = fileBytes;
                    data.templateTypeName = "Pending Approvals";
                }


            }

            return data;
        }

        #endregion ALIEN CODE BLOCKS
    }
}