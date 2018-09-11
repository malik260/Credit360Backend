using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
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
        private IWorkflow workflow;
        private IAdminRepository admin;

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
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId
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
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId
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
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.staffLevelId
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
                        IPADDRESS = model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.staffLevelId
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
                        IPADDRESS = user.userIPAddress,
                        URL = user.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.STAFFLEVELID
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
                        IPADDRESS = user.userIPAddress,
                        URL = user.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = data.STAFFLEVELID
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

        #endregion ALIEN CODE BLOCKS
    }
}