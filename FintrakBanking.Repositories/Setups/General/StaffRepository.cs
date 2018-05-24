using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common;
using FintrakBanking.Entities.DocumentModels;
using System.Drawing;
using System.Text;
using GemBox.Spreadsheet;
using System.IO;
using FintrakBanking.ViewModels.Admin;
using System.Web;

namespace FintrakBanking.Repositories.Setups.General
{
    public class StaffRepository : IStaffRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workflow;
        private IApprovalLevelStaffRepository level;
        private FinTrakBankingDocumentsContext documentsContext;

        public StaffRepository(FinTrakBankingContext _context,
                               IAuditTrailRepository _auditTrail,
                               IGeneralSetupRepository _genSetup,
                               IWorkflow _workFlow,
                               IApprovalLevelStaffRepository _level,
                               FinTrakBankingDocumentsContext _documentsContext)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            this.workflow = _workFlow;
            level = _level;
            documentsContext = _documentsContext;
        }

        public StaffRepository(FinTrakBankingContext context)
        {
            this.context = context;

        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        //public bool AddStaff(StaffInfoViewModel staffModel)
        //{
        //    var staff = new tbl_Staff()
        //    {
        //        FirstName = staffModel.FirstName,
        //        MiddleName = staffModel.MiddleName,
        //        LastName = staffModel.LastName,
        //        StaffCode = staffModel.StaffCode,
        //        JobTitleId = staffModel.JobTitleId,
        //        RankId = staffModel.RankId,
        //        Address = staffModel.Address,
        //        AddressOfNok = staffModel.AddressOfNok,
        //        BranchId = staffModel.BranchId,
        //        Comment = staffModel.Comment,
        //        CreatedBy = staffModel.createdBy,
        //        CustomerSensitivityLevel = staffModel.CustomerSensitivityLevel,
        //        DateOfBirth = staffModel.DateOfBirth,
        //        DateTimeCreated = DateTime.Now,
        //        DepartmentId = staffModel.DepartmentId,
        //        Email = staffModel.Email,
        //        EmailOfNok = staffModel.EmailOfNok,
        //        Gender = staffModel.Gender,
        //        GenderOfNok = staffModel.GenderOfNok,
        //        MisinfoId = staffModel.MisinfoId,
        //        NameOfNok = staffModel.NameOfNok,
        //        NokrelationShip = staffModel.NokrelationShip,
        //        Phone = staffModel.Phone,
        //        PhoneOfNok = staffModel.PhoneOfNok,
        //        StateId = staffModel.StateId,
        //        CityId = staffModel.CityId,
        //        Staffsignature = staffModel.Staffsignature,
        //    };
        //    this.context.tbl_Staff.Add(staff);
        //    return this.SaveAll();
        //}

        public IEnumerable<StaffInfoViewModel> GetAllStaff()
        {
            var staff = (from c in context.TBL_STAFF
                         join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                         join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                         //join dept in context.TBL_DEPARTMENT on c.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                         select new StaffInfoViewModel()
                         {
                             StaffId = c.STAFFID,
                             Address = c.ADDRESS,
                             companyId = coy.COMPANYID,
                             AddressOfNok = c.ADDRESSOFNOK,
                             BranchId = br.BRANCHID,
                             Comment = c.COMMENT,
                             //createdBy = c.CreatedBy.Value,
                             customerSensitivityLevelId = c.TBL_CUSTOMER_SENSITIVITY_LEVEL.CUSTOMERSENSITIVITYLEVELID,
                             DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                             //dateTimeCreated = c.DateTimeCreated,
                             DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                             Email = c.EMAIL,
                             EmailOfNok = c.EMAILOFNOK,
                             Gender = c.GENDER,
                             GenderOfNok = c.GENDEROFNOK,
                             JobTitleId = c.JOBTITLEID,
                             MisinfoId = c.MISINFOID,
                             NameOfNok = c.NAMEOFNOK,
                             NokrelationShip = c.NOKRELATIONSHIP,
                             Phone = c.PHONE,
                             PhoneOfNok = c.PHONEOFNOK,
                             StateId = c.STATEID,
                             //  StaffSignature = c.STAFFSIGNATURE,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             staffRoleId = c.STAFFROLEID,
                             staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                             supervisorStaffId = c.SUPERVISOR_STAFFID,
                             supervisorStaffName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                             BranchName = br.BRANCHNAME,
                             departmentName = c.TBL_DEPARTMENT_UNIT.TBL_DEPARTMENT.DEPARTMENTNAME,
                             departmentUnitId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITID,
                             departmentUnitName = c.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITNAME,

                             //MisInfoCode = c.MISC,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION,
                             //State = c.State.StateName
                             CityId = c.CITYID,
                         }).ToList();

            var department = (from k in context.TBL_DEPARTMENT_UNIT
                              select new DepartmentViewModel()
                              {
                                  departmentId = (short)k.DEPARTMENTID,
                                  unitId = k.DEPARTMENTUNITID,
                                  unitName = k.DEPARTMENTUNITNAME
                              }).ToList();

            foreach (var s in staff)
            {

                s.departmentUnits = department.Where(x => x.departmentId == s.DepartmentId);

            }
            return staff;
        }

        public StaffInfoViewModel GetStaffById(int staffId)
        {
            var staff = (from c in context.TBL_STAFF
                         where c.STAFFID == staffId
                         select new StaffInfoViewModel()
                         {
                             Address = c.ADDRESS,
                             AddressOfNok = c.ADDRESSOFNOK,
                             BranchId = c.BRANCHID,
                             Comment = c.COMMENT,
                             createdBy = c.CREATEDBY.Value,
                             customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                             DateOfBirth = c.DATEOFBIRTH,
                             dateTimeCreated = (DateTime)c.DATETIMECREATED,
                             DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                             Email = c.EMAIL,
                             EmailOfNok = c.EMAILOFNOK,
                             Gender = c.GENDER,
                             GenderOfNok = c.GENDEROFNOK,
                             JobTitleId = c.JOBTITLEID,
                             MisinfoId = c.MISINFOID,
                             NameOfNok = c.NAMEOFNOK,
                             NokrelationShip = c.NOKRELATIONSHIP,
                             Phone = c.PHONE,
                             PhoneOfNok = c.PHONEOFNOK,
                             StateId = c.STATEID,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             staffRoleId = c.STAFFROLEID,
                             staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                             supervisorStaffId = c.SUPERVISOR_STAFFID,
                             supervisorStaffName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                             //BranchName = br.BranchName,

                             //DepartmentName = c.Department.DepartmentName,
                             //MisInfoCode = c.Misinfo.Misname,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION,
                             //State = c.State.StateName
                         }).SingleOrDefault();
            return staff;
        }

        public bool UpdateStaff(int staffid, StaffInfoViewModel staffModel)
        {

            bool isUpdate = false;
            List<TBL_TEMP_PROFILE_USERGROUP> userGroups = new List<TBL_TEMP_PROFILE_USERGROUP>();
            List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> userActivities = new List<TBL_TEMP_PROFILE_ADTN_ACTIVITY>();
            TBL_TEMP_PROFILE_USER user = null;

            if (staffModel.user.activities.Any())
            {
                foreach (var item in staffModel.user.activities)
                {
                    var userActivity = new TBL_TEMP_PROFILE_ADTN_ACTIVITY()
                    {
                        ACTIVITYID = item.activityId,
                        CANADD = false,
                        CANEDIT = false,
                        CANAPPROVE = false,
                        CANDELETE = false,
                        CANVIEW = false,
                        CREATEDBY = staffModel.createdBy,
                        DATETIMECREATED = DateTime.Now,
                    };

                    userActivities.Add(userActivity);
                }
            }

            if (staffModel.user.group.Count > 0)
            {
                foreach (var item in staffModel.user.group)
                {
                    var grpItem = new TBL_TEMP_PROFILE_USERGROUP()
                    {
                        GROUPID = item.groupId,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffModel.createdBy,
                        ISCURRENT = true,
                        APPROVALSTATUS = false,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                    };
                    userGroups.Add(grpItem);
                }
            }
            if (staffModel.user != null)
            {



            }


            var existingTempStaff = context.TBL_TEMP_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower() && x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
            if (existingTempStaff != null)
            {
                var existingTempUser = context.TBL_TEMP_PROFILE_USER.FirstOrDefault(u => u.TEMPSTAFFID == existingTempStaff.TEMPSTAFFID);
                if (existingTempUser != null)
                {
                    isUpdate = true;

                    existingTempUser.USERNAME = staffModel.user.username;
                    existingTempUser.ISFIRSTLOGINATTEMPT = false;
                    existingTempUser.ISACTIVE = false;
                    existingTempUser.ISLOCKED = true;
                    existingTempUser.FAILEDLOGONATTEMPT = 0;
                    existingTempUser.SECURITYQUESTION = staffModel.user.securityQuestion;
                    existingTempUser.SECURITYANSWER = staffModel.user.securityAnswer;
                    existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays);
                    existingTempUser.LASTUPDATEDBY = staffModel.createdBy;
                    existingTempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    existingTempUser.APPROVALSTATUS = false;
                    existingTempUser.ISCURRENT = true;
                    existingTempUser.TBL_TEMP_PROFILE_ADTN_ACTIVITY = userActivities;
                    existingTempUser.TBL_TEMP_PROFILE_USERGROUP = userGroups;
                }
                else
                {
                    user = new TBL_TEMP_PROFILE_USER()
                    {
                        TEMPSTAFFID = staffModel.staffId,
                        USERNAME = staffModel.user.username,
                        PASSWORD = StaticHelpers.EncryptSha512(staffModel.user.password, StaticHelpers.EncryptionKey),
                        ISFIRSTLOGINATTEMPT = false,
                        ISACTIVE = false,
                        ISLOCKED = true,
                        FAILEDLOGONATTEMPT = 0,
                        SECURITYQUESTION = staffModel.user.securityQuestion,
                        SECURITYANSWER = staffModel.user.securityAnswer,
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                        CREATEDBY = staffModel.createdBy,
                        LASTUPDATEDBY = staffModel.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                        APPROVALSTATUS = false,
                        ISCURRENT = true,
                        TBL_TEMP_PROFILE_ADTN_ACTIVITY = userActivities,
                        TBL_TEMP_PROFILE_USERGROUP = userGroups
                    };
                }
            }
            var unApprovedStaffEdit = context.TBL_TEMP_STAFF.Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                                                                        x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower());
            TBL_TEMP_STAFF tempStaff = new TBL_TEMP_STAFF();

            if (unApprovedStaffEdit.Any())
            {
                throw new Exception("Staff is already undergoing approval");
            }

            if (existingTempStaff != null)
            {
                //foreach (var item in existStingTempStaff)
                //{
                //    item.IsCurrent = false;
                //    item.DateTimeUpdated = DateTime.Now;
                //}

                var tempStaffToUpdate = existingTempStaff;

                tempStaffToUpdate.FIRSTNAME = staffModel.FirstName;
                tempStaffToUpdate.MIDDLENAME = staffModel.MiddleName;
                tempStaffToUpdate.LASTNAME = staffModel.LastName;
                tempStaffToUpdate.STAFFCODE = staffModel.StaffCode;
                tempStaffToUpdate.JOBTITLEID = staffModel.JobTitleId;
                tempStaffToUpdate.COMPANYID = staffModel.companyId;
                tempStaffToUpdate.STAFFROLEID = staffModel.staffRoleId;
                tempStaffToUpdate.SUPERVISOR_STAFFID = staffModel.supervisorStaffId;
                tempStaffToUpdate.ADDRESS = staffModel.Address;
                tempStaffToUpdate.ADDRESSOFNOK = staffModel.AddressOfNok;
                tempStaffToUpdate.BRANCHID = staffModel.BranchId;
                tempStaffToUpdate.COMMENT = staffModel.Comment;
                tempStaffToUpdate.CREATEDBY = staffModel.createdBy;
                tempStaffToUpdate.CUSTOMERSENSITIVITYLEVELID = staffModel.customerSensitivityLevelId;
                tempStaffToUpdate.DATEOFBIRTH = staffModel.DateOfBirth;
                tempStaffToUpdate.DATETIMEUPDATED = DateTime.Now;
                //tempStaffToUpdate.DEPARTMENTID = staffModel.DepartmentId;
                tempStaffToUpdate.DEPARTMENTUNITID = (short)staffModel.departmentUnitId;
                tempStaffToUpdate.EMAIL = staffModel.Email;
                tempStaffToUpdate.EMAILOFNOK = staffModel.EmailOfNok;
                tempStaffToUpdate.GENDER = staffModel.Gender;
                tempStaffToUpdate.GENDEROFNOK = staffModel.GenderOfNok;
                tempStaffToUpdate.MISINFOID = staffModel.MisinfoId;
                tempStaffToUpdate.NAMEOFNOK = staffModel.NameOfNok;
                tempStaffToUpdate.NOKRELATIONSHIP = staffModel.NokrelationShip;
                tempStaffToUpdate.PHONE = staffModel.Phone;
                tempStaffToUpdate.PHONEOFNOK = staffModel.PhoneOfNok;
                tempStaffToUpdate.STATEID = staffModel.StateId;
                tempStaffToUpdate.CITYID = staffModel.CityId;
                tempStaffToUpdate.STAFFSIGNATURE = staffModel.StaffSignature;
                tempStaffToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempStaffToUpdate.ISCURRENT = true;

            }
            else
            {
                var targetStaff = context.TBL_STAFF.Find(staffid);

                tempStaff = new TBL_TEMP_STAFF()
                {
                    FIRSTNAME = staffModel.FirstName,
                    MIDDLENAME = staffModel.MiddleName,
                    LASTNAME = staffModel.LastName,
                    STAFFCODE = targetStaff?.STAFFCODE,
                    JOBTITLEID = staffModel.JobTitleId,
                    COMPANYID = staffModel.companyId,
                    STAFFROLEID = staffModel.staffRoleId,
                    SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                    ADDRESS = staffModel.Address,
                    ADDRESSOFNOK = staffModel.AddressOfNok,
                    BRANCHID = staffModel.BranchId,
                    COMMENT = staffModel.Comment,
                    CREATEDBY = staffModel.createdBy,
                    CUSTOMERSENSITIVITYLEVELID = staffModel.customerSensitivityLevelId,
                    DATEOFBIRTH = staffModel.DateOfBirth,
                    DATETIMECREATED = DateTime.Now,
                    //DEPARTMENTID = staffModel.DepartmentId,
                    DEPARTMENTUNITID = (short)staffModel.departmentUnitId,
                    EMAIL = staffModel.Email,
                    EMAILOFNOK = staffModel.EmailOfNok,
                    GENDER = staffModel.Gender,
                    GENDEROFNOK = staffModel.GenderOfNok,
                    MISINFOID = staffModel.MisinfoId,
                    NAMEOFNOK = staffModel.NameOfNok,
                    NOKRELATIONSHIP = staffModel.NokrelationShip,
                    PHONE = staffModel.Phone,
                    PHONEOFNOK = staffModel.PhoneOfNok,
                    STATEID = staffModel.StateId,
                    CITYID = staffModel.CityId,
                    STAFFSIGNATURE = staffModel.StaffSignature,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    ISCURRENT = true
                };

                context.TBL_TEMP_STAFF.Add(tempStaff);
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffUpdated,
                STAFFID = staffModel.createdBy,
                BRANCHID = (short)staffModel.BranchId,
                DETAIL = $"Updated Staff '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
                IPADDRESS = staffModel.userIPAddress,
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = staffid
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    var output = context.SaveChanges() > 0;

                    var targetStaffId = existingTempStaff?.TEMPSTAFFID ?? tempStaff.TEMPSTAFFID;
                    if (isUpdate != true)
                    {
                        user.TEMPSTAFFID = targetStaffId;
                        context.TBL_TEMP_PROFILE_USER.Add(user);
                        context.SaveChanges();
                    }
                    var entity = new ApprovalViewModel
                    {
                        staffId = staffModel.createdBy,
                        companyId = staffModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = targetStaffId,
                        operationId = (int)OperationsEnum.StaffCreation,
                        BranchId = staffModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workflow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool DeleteStaff(int staffId, UserInfo user)
        {
            var targetStaff = context.TBL_STAFF.Find(staffId);

            targetStaff.DELETED = true;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Staff '{targetStaff?.FIRSTNAME}' with code'{targetStaff?.STAFFCODE}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public IEnumerable<StaffViewModel> GetStaffName()
        {
            var staff = (from c in context.TBL_STAFF
                         select new StaffViewModel()
                         {
                             StaffName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                             StaffId = c.STAFFID
                         });
            return staff;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workflow.StaffId = entity.staffId;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                    workflow.TargetId = entity.targetId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = (int)OperationsEnum.StaffCreation;
                    workflow.ExternalInitialization = false;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();

                    context.SaveChanges();

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveStaff(entity.targetId, (short)workflow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }


        private bool ApproveStaff(int staffid, short approvalStatusId, UserInfo user)
        {
            bool isUpdate = false;
            TBL_TEMP_PROFILE_USER tempUser = null;
            List<TBL_TEMP_PROFILE_USERGROUP> tempGroup = null;
            List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> tempActivities = null;

            List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
            List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();

            tempUser = (from a in context.TBL_TEMP_PROFILE_USER
                        where a.TEMPSTAFFID == staffid && a.ISCURRENT == true &&
                        a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        select a).FirstOrDefault();
            if (tempUser != null)
            {
                tempGroup = (from a in context.TBL_TEMP_PROFILE_USERGROUP where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();
                tempActivities = (from a in context.TBL_TEMP_PROFILE_ADTN_ACTIVITY where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();

                tempUser.ISCURRENT = false;
                tempUser.APPROVALSTATUS = true;
                tempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                foreach (var item in tempGroup)
                {
                    item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    item.ISCURRENT = false;
                    item.DATEAPPROVED = DateTime.Now;
                }

                if (tempActivities.Count > 0)
                {
                    foreach (var item in tempActivities)
                    {
                        var userActivity = new TBL_PROFILE_ADDITIONALACTIVITY()
                        {
                            ACTIVITYID = item.ACTIVITYID,
                            CANADD = false,
                            CANEDIT = false,
                            CANAPPROVE = false,
                            CANDELETE = false,
                            CANVIEW = false,
                            CREATEDBY = item.CREATEDBY,
                            DATETIMECREATED = DateTime.Now,
                        };
                        userActivities.Add(userActivity);
                    }
                }

                if (tempGroup.Count > 0)
                {
                    foreach (var item in tempGroup)
                    {
                        var grpItem = new TBL_PROFILE_USERGROUP()
                        {
                            GROUPID = item.GROUPID,
                            APPROVALSTATUS = false,
                            DATETIMECREATED = DateTime.Now,
                            CREATEDBY = item.CREATEDBY,
                        };
                        userGroups.Add(grpItem);
                    }
                }
            }

            TBL_STAFF entity = null;
            TBL_PROFILE_USER targetUser = null;
            var temp = context.TBL_TEMP_STAFF.Find(staffid);
            if (temp != null)
            {
                entity = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == temp.STAFFCODE.ToLower());
                // Removing existing groups and activities
                if (entity != null)
                {

                    targetUser = (from a in context.TBL_PROFILE_USER
                                  where a.STAFFID == entity.STAFFID
                                  select a).FirstOrDefault();
                    if (targetUser != null)
                    {
                        var targetGroups = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == targetUser.USERID).ToList();
                        var targetActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == targetUser.USERID).ToList();
                        if (targetGroups.Any())
                        {
                            foreach (var item in targetGroups)
                            {
                                context.TBL_PROFILE_USERGROUP.Remove(item);
                            }
                        }
                        if (targetActivities.Any())
                        {
                            foreach (var item in targetActivities)
                            {
                                context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
                            }
                        }
                    }
                }
            }


            if (tempUser != null)
            {
                if (targetUser != null)
                {
                    isUpdate = true;
                    targetUser.USERNAME = tempUser.USERNAME;
                    targetUser.PASSWORD = tempUser.PASSWORD;
                    targetUser.ISFIRSTLOGINATTEMPT = false;
                    targetUser.ISACTIVE = true;
                    targetUser.ISLOCKED = false;
                    targetUser.FAILEDLOGONATTEMPT = 0;
                    targetUser.SECURITYQUESTION = tempUser.SECURITYQUESTION;
                    targetUser.SECURITYANSWER = tempUser.SECURITYANSWER;
                    targetUser.TBL_PROFILE_USERGROUP = userGroups;
                    targetUser.TBL_PROFILE_ADDITIONALACTIVITY = userActivities;
                }
                else
                {
                    targetUser = new TBL_PROFILE_USER()
                    {
                        USERNAME = tempUser.USERNAME,
                        PASSWORD = tempUser.PASSWORD,
                        ISFIRSTLOGINATTEMPT = false,
                        ISACTIVE = true,
                        ISLOCKED = false,
                        FAILEDLOGONATTEMPT = 0,
                        SECURITYQUESTION = tempUser.SECURITYQUESTION,
                        SECURITYANSWER = tempUser.SECURITYANSWER,
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                        CREATEDBY = tempUser.CREATEDBY,
                        LASTUPDATEDBY = tempUser.CREATEDBY,
                        DATETIMECREATED = tempUser.DATETIMECREATED,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved,
                        APPROVALSTATUS = false,
                        TBL_PROFILE_USERGROUP = userGroups,
                        TBL_PROFILE_ADDITIONALACTIVITY = userActivities,
                    };

                }
            }


            if (entity != null) //Update existing staff with tempStaff record
            {

                entity.FIRSTNAME = temp.FIRSTNAME;
                entity.COMPANYID = temp.COMPANYID;
                entity.MIDDLENAME = temp.MIDDLENAME;
                entity.LASTNAME = temp.LASTNAME;
                entity.STAFFCODE = temp.STAFFCODE;
                entity.JOBTITLEID = temp.JOBTITLEID;
                entity.STAFFROLEID = temp.STAFFROLEID;
                entity.SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID;
                entity.ADDRESS = temp.ADDRESS;
                entity.ADDRESSOFNOK = temp.ADDRESSOFNOK;
                entity.BRANCHID = temp.BRANCHID;
                entity.COMMENT = temp.COMMENT;
                entity.CREATEDBY = temp.CREATEDBY;
                if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                entity.DATETIMEUPDATED = DateTime.Now;
                //entity.DEPARTMENTID = temp.DEPARTMENTID;
                entity.DEPARTMENTUNITID = temp.DEPARTMENTUNITID;
                entity.EMAIL = temp.EMAIL;
                entity.EMAILOFNOK = temp.EMAILOFNOK;
                entity.GENDER = temp.GENDER;
                entity.GENDEROFNOK = temp.GENDEROFNOK;
                entity.MISINFOID = temp.MISINFOID;
                entity.NAMEOFNOK = temp.NAMEOFNOK;
                entity.NOKRELATIONSHIP = temp.NOKRELATIONSHIP;
                entity.PHONE = temp.PHONE;
                entity.PHONEOFNOK = temp.PHONEOFNOK;
                entity.STATEID = temp.STATEID;
                entity.CITYID = temp.CITYID;
                entity.DELETED = false;
            }
            else //Insert a new staff record into the real staff table
            {
                entity = new TBL_STAFF()
                {
                    FIRSTNAME = temp.FIRSTNAME,
                    MIDDLENAME = temp.MIDDLENAME,
                    COMPANYID = temp.COMPANYID,
                    LASTNAME = temp.LASTNAME,
                    STAFFCODE = temp.STAFFCODE,
                    JOBTITLEID = temp.JOBTITLEID,
                    STAFFROLEID = temp.STAFFROLEID,
                    SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID,
                    DEPARTMENTUNITID = temp.DEPARTMENTUNITID,
                    ADDRESS = temp.ADDRESS,
                    ADDRESSOFNOK = temp.ADDRESSOFNOK,
                    BRANCHID = temp.BRANCHID,
                    COMMENT = temp.COMMENT,
                    CREATEDBY = temp.CREATEDBY,
                    DATEOFBIRTH = temp.DATEOFBIRTH,
                    DATETIMECREATED = DateTime.Now,
                    //DEPARTMENTID = temp.DEPARTMENTID,
                    EMAIL = temp.EMAIL,
                    EMAILOFNOK = temp.EMAILOFNOK,
                    GENDER = temp.GENDER,
                    GENDEROFNOK = temp.GENDEROFNOK,
                    MISINFOID = temp.MISINFOID,
                    NAMEOFNOK = temp.NAMEOFNOK,
                    NOKRELATIONSHIP = temp.NOKRELATIONSHIP,
                    PHONE = temp.PHONE,
                    PHONEOFNOK = temp.PHONEOFNOK,
                    STATEID = temp.STATEID,
                    CITYID = temp.CITYID,

                };
                if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                context.TBL_STAFF.Add(entity);
            }

            temp.ISCURRENT = false;
            temp.APPROVALSTATUSID = approvalStatusId;
            temp.DATETIMEUPDATED = DateTime.Now;
            temp.LASTUPDATEDBY = user.createdBy;

            //if (temp.TEMPSTAFFID != entity.RELIEF_STAFFID) { UpdateDelegateStaff(entity.STAFFID, temp.TEMPSTAFFID); }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff '{temp?.FIRSTNAME + " " + temp?.LASTNAME}' with staff code'{temp?.STAFFCODE}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            try
            {
                context.TBL_AUDIT.Add(audit);
                var output = context.SaveChanges() > 0;

                if (isUpdate == false && targetUser != null)
                {
                    targetUser.STAFFID = entity.STAFFID;
                    context.TBL_PROFILE_USER.Add(targetUser);
                    return context.SaveChanges() > 0;
                }
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public bool GoForBulkApproval(List<ApprovalViewModel> model, UserInfo userInfo)
        {
            if (model.Count == 0) return false;
            bool output = false;
            foreach (var entity in model)
            {

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        workflow.StaffId = userInfo.staffId;
                        workflow.CompanyId = userInfo.companyId;
                        workflow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                        workflow.TargetId = entity.targetId;
                        workflow.Comment = entity.comment;
                        workflow.OperationId = (int)OperationsEnum.StaffCreation;
                        workflow.ExternalInitialization = false;
                        workflow.DeferredExecution = true;
                        workflow.LogActivity();

                        context.SaveChanges();

                        if (workflow.NewState == (int)ApprovalState.Ended)
                        {
                            var response = ApproveBulkStaff(entity.targetId, (short)workflow.StatusId, userInfo);

                            if (response)
                            {
                                trans.Commit();
                            }
                            output = true;
                        }
                        else
                        {
                            trans.Commit();
                        }

                        output = false;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
            return output;
        }

        private bool ApproveBulkStaff(int staffid, short approvalStatusId, UserInfo user)
        {
            bool isUpdate = false;
            TBL_TEMP_PROFILE_USER tempUser = null;
            List<TBL_TEMP_PROFILE_USERGROUP> tempGroup = null;
            List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> tempActivities = null;

            List<TBL_PROFILE_USERGROUP> userGroups = new List<TBL_PROFILE_USERGROUP>();
            List<TBL_PROFILE_ADDITIONALACTIVITY> userActivities = new List<TBL_PROFILE_ADDITIONALACTIVITY>();

            tempUser = (from a in context.TBL_TEMP_PROFILE_USER
                        where a.TEMPSTAFFID == staffid && a.ISCURRENT == true &&
                        a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        select a).FirstOrDefault();
            if (tempUser != null)
            {
                tempGroup = (from a in context.TBL_TEMP_PROFILE_USERGROUP where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();
                tempActivities = (from a in context.TBL_TEMP_PROFILE_ADTN_ACTIVITY where a.TEMPUSERID == tempUser.TEMPUSERID select a).ToList();

                tempUser.ISCURRENT = false;
                tempUser.APPROVALSTATUS = true;
                tempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                if (tempActivities.Count > 0)
                {
                    foreach (var item in tempActivities)
                    {
                        var userActivity = new TBL_PROFILE_ADDITIONALACTIVITY()
                        {
                            ACTIVITYID = item.ACTIVITYID,
                            CANADD = false,
                            CANEDIT = false,
                            CANAPPROVE = false,
                            CANDELETE = false,
                            CANVIEW = false,
                            CREATEDBY = item.CREATEDBY,
                            DATETIMECREATED = DateTime.Now,
                        };
                        userActivities.Add(userActivity);
                    }
                }

                if (tempGroup.Count > 0)
                {
                    foreach (var item in tempGroup)
                    {
                        var grpItem = new TBL_PROFILE_USERGROUP()
                        {
                            GROUPID = item.GROUPID,
                            APPROVALSTATUS = false,
                            DATETIMECREATED = DateTime.Now,
                            CREATEDBY = item.CREATEDBY,
                        };
                        userGroups.Add(grpItem);
                    }
                    foreach (var item in tempGroup)
                    {
                        item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        item.ISCURRENT = false;
                        item.DATEAPPROVED = DateTime.Now;
                    }
                }
            }

            TBL_STAFF entity = null;
            TBL_PROFILE_USER targetUser = null;
            var temp = context.TBL_TEMP_STAFF.Find(staffid);
            if (temp != null)
            {
                entity = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == temp.STAFFCODE.ToLower());
                // Removing existing groups and activities
                if (entity != null)
                {

                    targetUser = (from a in context.TBL_PROFILE_USER
                                  where a.STAFFID == entity.STAFFID
                                  select a).FirstOrDefault();
                    if (targetUser != null)
                    {
                        var targetGroups = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == targetUser.USERID).ToList();
                        var targetActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == targetUser.USERID).ToList();
                        if (targetGroups.Any())
                        {
                            foreach (var item in targetGroups)
                            {
                                context.TBL_PROFILE_USERGROUP.Remove(item);
                            }
                        }
                        if (targetActivities.Any())
                        {
                            foreach (var item in targetActivities)
                            {
                                context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
                            }
                        }
                    }
                }
            }


            if (tempUser != null)
            {
                if (targetUser != null)
                {
                    isUpdate = true;
                    targetUser.USERNAME = tempUser.USERNAME;
                    targetUser.PASSWORD = tempUser.PASSWORD;
                    targetUser.ISFIRSTLOGINATTEMPT = false;
                    targetUser.ISACTIVE = true;
                    targetUser.ISLOCKED = false;
                    targetUser.FAILEDLOGONATTEMPT = 0;
                    targetUser.SECURITYQUESTION = tempUser.SECURITYQUESTION;
                    targetUser.SECURITYANSWER = tempUser.SECURITYANSWER;
                    targetUser.TBL_PROFILE_USERGROUP = userGroups;
                    targetUser.TBL_PROFILE_ADDITIONALACTIVITY = userActivities;
                }
                else
                {
                    targetUser = new TBL_PROFILE_USER()
                    {
                        USERNAME = tempUser.USERNAME,
                        PASSWORD = tempUser.PASSWORD,
                        ISFIRSTLOGINATTEMPT = false,
                        ISACTIVE = true,
                        ISLOCKED = false,
                        FAILEDLOGONATTEMPT = 0,
                        SECURITYQUESTION = tempUser.SECURITYQUESTION,
                        SECURITYANSWER = tempUser.SECURITYANSWER,
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                        CREATEDBY = tempUser.CREATEDBY,
                        LASTUPDATEDBY = tempUser.CREATEDBY,
                        DATETIMECREATED = tempUser.DATETIMECREATED,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved,
                        APPROVALSTATUS = false,
                        TBL_PROFILE_USERGROUP = userGroups,
                        TBL_PROFILE_ADDITIONALACTIVITY = userActivities,
                    };

                }
            }

            if (temp != null)
            {
                if (entity != null)
                {
                    entity.FIRSTNAME = temp.FIRSTNAME;
                    entity.COMPANYID = temp.COMPANYID;
                    entity.MIDDLENAME = temp.MIDDLENAME;
                    entity.LASTNAME = temp.LASTNAME;
                    entity.STAFFCODE = temp.STAFFCODE;
                    entity.JOBTITLEID = temp.JOBTITLEID;
                    entity.STAFFROLEID = temp.STAFFROLEID;
                    entity.SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID;
                    entity.ADDRESS = temp.ADDRESS;
                    entity.ADDRESSOFNOK = temp.ADDRESSOFNOK;
                    entity.BRANCHID = temp.BRANCHID;
                    entity.COMMENT = temp.COMMENT;
                    entity.CREATEDBY = temp.CREATEDBY;
                    if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                    entity.DATEOFBIRTH = temp.DATEOFBIRTH;
                    entity.DATETIMEUPDATED = DateTime.Now;
                    //entity.DEPARTMENTID = temp.DEPARTMENTID;
                    entity.DEPARTMENTUNITID = temp.DEPARTMENTUNITID;
                    entity.EMAIL = temp.EMAIL;
                    entity.EMAILOFNOK = temp.EMAILOFNOK;
                    entity.GENDER = temp.GENDER;
                    entity.GENDEROFNOK = temp.GENDEROFNOK;
                    entity.MISINFOID = temp.MISINFOID;
                    entity.NAMEOFNOK = temp.NAMEOFNOK;
                    entity.NOKRELATIONSHIP = temp.NOKRELATIONSHIP;
                    entity.PHONE = temp.PHONE;
                    entity.PHONEOFNOK = temp.PHONEOFNOK;
                    entity.STATEID = temp.STATEID;
                    entity.CITYID = temp.CITYID;
                    entity.DELETED = false;
                }
                else
                {
                    entity = new TBL_STAFF()
                    {
                        FIRSTNAME = temp.FIRSTNAME,
                        MIDDLENAME = temp.MIDDLENAME,
                        COMPANYID = temp.COMPANYID,
                        LASTNAME = temp.LASTNAME,
                        STAFFCODE = temp.STAFFCODE,
                        JOBTITLEID = temp.JOBTITLEID,
                        STAFFROLEID = temp.STAFFROLEID,
                        SUPERVISOR_STAFFID = temp.SUPERVISOR_STAFFID,
                        DEPARTMENTUNITID = temp.DEPARTMENTUNITID,
                        ADDRESS = temp.ADDRESS,
                        ADDRESSOFNOK = temp.ADDRESSOFNOK,
                        BRANCHID = temp.BRANCHID,
                        COMMENT = temp.COMMENT,
                        CREATEDBY = temp.CREATEDBY,
                        DATEOFBIRTH = temp.DATEOFBIRTH,
                        DATETIMECREATED = DateTime.Now,
                        //DEPARTMENTID = temp.DEPARTMENTID,
                        EMAIL = temp.EMAIL,
                        EMAILOFNOK = temp.EMAILOFNOK,
                        GENDER = temp.GENDER,
                        GENDEROFNOK = temp.GENDEROFNOK,
                        MISINFOID = temp.MISINFOID,
                        NAMEOFNOK = temp.NAMEOFNOK,
                        NOKRELATIONSHIP = temp.NOKRELATIONSHIP,
                        PHONE = temp.PHONE,
                        PHONEOFNOK = temp.PHONEOFNOK,
                        STATEID = temp.STATEID,
                        CITYID = temp.CITYID,
                    };
                    if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                    context.TBL_STAFF.Add(entity);
                }
                temp.ISCURRENT = false;
                temp.APPROVALSTATUSID = approvalStatusId;
                temp.DATETIMEUPDATED = DateTime.Now;
                temp.LASTUPDATEDBY = user.createdBy;
            }


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff '{temp?.FIRSTNAME + " " + temp?.LASTNAME}' with staff code'{temp?.STAFFCODE}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            try
            {
                context.TBL_AUDIT.Add(audit);
                var output = context.SaveChanges() > 0;

                if (tempUser != null && isUpdate == false)
                {
                    targetUser.STAFFID = entity.STAFFID;
                    context.TBL_PROFILE_USER.Add(targetUser);
                    return context.SaveChanges() > 0;
                }
                return output;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        private void UpdateDelegateStaff(int staffId, int? reliefId)
        {
            throw new NotImplementedException(); // <----------------------------- UPDATE APP_LEVEL_STAFF
        }

        public bool AddTempStaff(StaffInfoViewModel staffModel)
        {
            bool output = false;
            var existStingTempStaff = context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower()
                                                                  && x.ISCURRENT == true
                                                                  && x.COMPANYID == staffModel.companyId
                                                                  && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending);

            if (existStingTempStaff.Any())
            {
                throw new Exception("Staff Information already exist and is undergoing approval");
            }



            List<TBL_TEMP_PROFILE_USERGROUP> userGroups = new List<TBL_TEMP_PROFILE_USERGROUP>();
            List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> userActivities = new List<TBL_TEMP_PROFILE_ADTN_ACTIVITY>();
            TBL_TEMP_PROFILE_USER user = null;

            if (staffModel.user.activities.Any())
            {
                foreach (var item in staffModel.user.activities)
                {
                    var userActivity = new TBL_TEMP_PROFILE_ADTN_ACTIVITY()
                    {
                        ACTIVITYID = item.activityId,
                        CANADD = false,
                        CANEDIT = false,
                        CANAPPROVE = false,
                        CANDELETE = false,
                        CANVIEW = false,
                        CREATEDBY = staffModel.createdBy,
                        DATETIMECREATED = DateTime.Now,
                    };

                    userActivities.Add(userActivity);
                }
            }

            if (staffModel.user.group.Count > 0)
            {
                foreach (var item in staffModel.user.group)
                {
                    var grpItem = new TBL_TEMP_PROFILE_USERGROUP()
                    {
                        GROUPID = item.groupId,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffModel.createdBy,
                        ISCURRENT = true,
                        APPROVALSTATUS = false,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                    };
                    userGroups.Add(grpItem);
                }
            }
            if (staffModel.user != null)
            {
                user = new TBL_TEMP_PROFILE_USER()
                {
                    TEMPSTAFFID = staffModel.staffId,
                    USERNAME = staffModel.user.username,
                    PASSWORD = StaticHelpers.EncryptSha512(staffModel.user.password, StaticHelpers.EncryptionKey),
                    ISFIRSTLOGINATTEMPT = false,
                    ISACTIVE = false,
                    ISLOCKED = true,
                    FAILEDLOGONATTEMPT = 0,
                    SECURITYQUESTION = staffModel.user.securityQuestion,
                    SECURITYANSWER = staffModel.user.securityAnswer,
                    NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                    CREATEDBY = staffModel.createdBy,
                    LASTUPDATEDBY = staffModel.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    APPROVALSTATUS = false,
                    ISCURRENT = true,
                    TBL_TEMP_PROFILE_ADTN_ACTIVITY = userActivities,
                    TBL_TEMP_PROFILE_USERGROUP = userGroups
                };

            }

            var staff = new TBL_TEMP_STAFF()
            {
                FIRSTNAME = staffModel.FirstName,
                MIDDLENAME = staffModel.MiddleName,
                COMPANYID = staffModel.companyId,
                LASTNAME = staffModel.LastName,
                STAFFCODE = StaticHelpers.GetUniqueKey(6),
                JOBTITLEID = staffModel.JobTitleId,
                STAFFROLEID = staffModel.staffRoleId,
                SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                ADDRESS = staffModel.Address,
                ADDRESSOFNOK = staffModel.AddressOfNok,
                BRANCHID = staffModel.BranchId,
                COMMENT = staffModel.Comment,
                CREATEDBY = staffModel.createdBy,
                CUSTOMERSENSITIVITYLEVELID = staffModel.customerSensitivityLevelId,
                DATEOFBIRTH = staffModel.DateOfBirth,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTUNITID = staffModel.departmentUnitId,
                EMAIL = staffModel.Email,
                EMAILOFNOK = staffModel.EmailOfNok,
                GENDER = staffModel.Gender,
                GENDEROFNOK = staffModel.GenderOfNok,
                MISINFOID = staffModel.MisinfoId,
                NAMEOFNOK = staffModel.NameOfNok,
                NOKRELATIONSHIP = staffModel.NokrelationShip,
                PHONE = staffModel.Phone,
                PHONEOFNOK = staffModel.PhoneOfNok,
                STATEID = staffModel.StateId,
                CITYID = staffModel.CityId,
                STAFFSIGNATURE = staffModel.StaffSignature,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,

            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = staffModel.createdBy,
                BRANCHID = (short)staffModel.BranchId,
                DETAIL = $"Initiated Staff Creation for '{staffModel?.StaffFullName}' with code'{staffModel?.StaffCode}'",
                IPADDRESS = staffModel.userIPAddress,
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
            context.TBL_TEMP_STAFF.Add(staff);
            output = context.SaveChanges() > 0;

            user.TEMPSTAFFID = staff.TEMPSTAFFID;
            context.TBL_TEMP_PROFILE_USER.Add(user);


            workflow.StaffId = staffModel.createdBy;
            workflow.CompanyId = staffModel.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.TargetId = staff.TEMPSTAFFID;
            workflow.Comment = "New Staff Creation";
            workflow.OperationId = (int)OperationsEnum.StaffCreation;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            context.SaveChanges();

            return output;

            //using (var trans = context.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        auditTrail.AddAuditTrail(audit);
            //        context.TBL_TEMP_STAFF.Add(staff);
            //        output = await context.SaveChangesAsync() > 0;

            //        workflow.StaffId = staffModel.createdBy;
            //        workflow.CompanyId = staffModel.companyId;
            //        workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            //        workflow.TargetId = staff.TEMPSTAFFID;
            //        workflow.Comment = "New Staff Creation";
            //        workflow.OperationId = (int)OperationsEnum.StaffCreation;
            //        workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            //        workflow.ExternalInitialization = true;
            //        workflow.LogActivity();

            //        context.SaveChanges();

            //        if (workflow.Saved)
            //        {
            //            trans.Commit();
            //        }

            //        return output;
            //    }
            //    catch (Exception)
            //    {
            //        trans.Rollback();
            //    }
            //}

        }

        public bool IsStaffCodeAlreadyExist(string staffCode)
        {
            return context.TBL_STAFF.Any(x => x.STAFFCODE.ToLower() == staffCode.ToLower());
        }

        public bool IsTempStaffExist(string staffCode)
        {
            return context.TBL_TEMP_STAFF.Any(x => x.STAFFCODE.ToLower() == staffCode.ToLower() && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.ISCURRENT == true);
        }

        public IEnumerable<StaffInfoViewModel> GetStaffAwaitingApprovals(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.StaffCreation).ToList();

            var staff = (from c in context.TBL_TEMP_STAFF
                         join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                         join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                         //join dept in context.TBL_DEPARTMENT on c.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                         join t in context.TBL_APPROVAL_TRAIL on c.TEMPSTAFFID equals t.TARGETID
                         where
                             (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                             && c.ISCURRENT == true
                             && t.RESPONSESTAFFID == null
                             && t.OPERATIONID == (int)OperationsEnum.StaffCreation
                         && ids.Contains((int)t.TOAPPROVALLEVELID)
                         select new StaffInfoViewModel
                         {
                             DelegateName = context.TBL_STAFF
                                                     .Where(x => x.STAFFID == c.TEMPSTAFFID)
                                                     .Select(x => new { name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME })
                                                     .FirstOrDefault().name ?? "",
                             StaffId = c.TEMPSTAFFID,
                             Address = c.ADDRESS,
                             companyId = coy.COMPANYID,
                             AddressOfNok = c.ADDRESSOFNOK,
                             BranchId = br.BRANCHID,
                             Comment = c.COMMENT,
                             customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                             DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                             DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                             Email = c.EMAIL,
                             EmailOfNok = c.EMAILOFNOK,
                             Gender = c.GENDER,
                             GenderOfNok = c.GENDEROFNOK,
                             JobTitleId = c.JOBTITLEID,
                             JobTitleName = c.TBL_STAFF_JOBTITLE.JOBTITLENAME,
                             MisinfoId = c.MISINFOID,
                             MisInfoCode = c.TBL_MIS_INFO.MISCODE,
                             NameOfNok = c.NAMEOFNOK,
                             NokrelationShip = c.NOKRELATIONSHIP,
                             Phone = c.PHONE,
                             PhoneOfNok = c.PHONEOFNOK,
                             StateId = c.STATEID,
                             StateName = c.TBL_CITY.TBL_LOCALGOVERNMENT.TBL_STATE.STATENAME,
                             CityId = (int)c.CITYID,
                             CityName = c.TBL_CITY.CITYNAME,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             staffRoleId = c.STAFFROLEID,
                             staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                             BranchName = br.BRANCHNAME,
                             departmentName = c.TBL_DEPARTMENT_UNIT.TBL_DEPARTMENT.DEPARTMENTNAME,
                             departmentUnitId = c.DEPARTMENTUNITID,
                             departmentUnitName = c.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITNAME,

                             OperationId = t.OPERATIONID,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION
                         }).ToList();

            return staff;
        }

        public StaffDetailsModel GetTempStaffDetail(int staffId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.TBL_TEMP_STAFF
                    join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                    join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                    //join dept in context.TBL_DEPARTMENT on c.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                    where c.TEMPSTAFFID == staffId //c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved && c.IsCurrent == true
                    select new StaffDetailsModel()
                    {
                        StaffId = c.TEMPSTAFFID,
                        Address = c.ADDRESS,
                        companyId = coy.COMPANYID,
                        AddressOfNok = c.ADDRESSOFNOK,
                        BranchId = br.BRANCHID,
                        BranchName = br.BRANCHNAME,
                        Comment = c.COMMENT,
                        customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                        DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                        DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                        CityId = c.CITYID ?? 0,
                        City = c.TBL_CITY.CITYNAME,
                        company = coy.NAME,
                        JobTitle = c.TBL_STAFF_JOBTITLE.JOBTITLENAME,
                        MisInfo = c.TBL_MIS_INFO.MISNAME,
                        StaffSignature = c.STAFFSIGNATURE,
                        Email = c.EMAIL,
                        EmailOfNok = c.EMAILOFNOK,
                        Gender = c.GENDER,
                        GenderOfNok = c.GENDEROFNOK,
                        JobTitleId = c.JOBTITLEID,
                        MisinfoId = c.MISINFOID ?? 0,
                        NameOfNok = c.NAMEOFNOK,
                        NokrelationShip = c.NOKRELATIONSHIP,
                        Phone = c.PHONE,
                        PhoneOfNok = c.PHONEOFNOK,
                        StateId = c.STATEID ?? 0,
                        State = c.TBL_STATE.STATENAME,
                        FirstName = c.FIRSTNAME,
                        MiddleName = c.MIDDLENAME,
                        LastName = c.LASTNAME,
                        StaffCode = c.STAFFCODE,
                        staffRoleId = c.STAFFROLEID,
                        staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                        supervisorStaff = c.SUPERVISOR_STAFFID,
                        supervisorStaffName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                        departmentName = c.TBL_DEPARTMENT_UNIT.TBL_DEPARTMENT.DEPARTMENTNAME,
                        departmentUnitId = c.DEPARTMENTUNITID,
                        departmentUnitName = c.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITNAME,
                        ApprovalStatusId = c.APPROVALSTATUSID,
                        SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION,
                    }).FirstOrDefault();
        }

        public StaffDetailsModel GetStaffDetail(string staffCode, int companyId)
        {
            return GetStaffDetails(companyId).FirstOrDefault(x => x.StaffCode == staffCode && x.companyId == companyId);
        }

        public IEnumerable<StaffDetailsModel> GetStaffDetails(int companyId)
        {
            var data = (from c in context.TBL_STAFF
                        join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                        join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                        //join dept in context.TBL_DEPARTMENT on c.TBL_DEPARTMENT_UNIT.DEPARTMENTID equals dept.DEPARTMENTID
                        where c.COMPANYID == companyId
                        select new StaffDetailsModel()
                        {
                            StaffId = c.STAFFID,
                            Address = c.ADDRESS,
                            companyId = coy.COMPANYID,
                            AddressOfNok = c.ADDRESSOFNOK,
                            BranchId = br.BRANCHID,
                            BranchName = br.BRANCHNAME,
                            Comment = c.COMMENT,
                            customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                            DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                            DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                            CityId = c.CITYID ?? 0,
                            City = c.TBL_CITY.CITYNAME,
                            company = coy.NAME,
                            JobTitle = c.TBL_STAFF_JOBTITLE.JOBTITLENAME,
                            MisInfo = context.TBL_MIS_INFO.Find(c.MISINFOID).MISNAME,
                            Email = c.EMAIL,
                            EmailOfNok = c.EMAILOFNOK,
                            Gender = c.GENDER,
                            GenderOfNok = c.GENDEROFNOK,
                            JobTitleId = c.JOBTITLEID,
                            MisinfoId = c.MISINFOID ?? 0,
                            NameOfNok = c.NAMEOFNOK,
                            NokrelationShip = c.NOKRELATIONSHIP,
                            Phone = c.PHONE,
                            PhoneOfNok = c.PHONEOFNOK,
                            StateId = c.STATEID ?? 0,
                            State = context.TBL_STATE.Find(c.STATEID).STATENAME,
                            FirstName = c.FIRSTNAME,
                            MiddleName = c.MIDDLENAME,
                            LastName = c.LASTNAME,
                            StaffCode = c.STAFFCODE,
                            staffRoleId = c.STAFFROLEID,
                            staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                            supervisorStaff = c.SUPERVISOR_STAFFID,
                            supervisorStaffName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                            departmentName = c.TBL_DEPARTMENT_UNIT.TBL_DEPARTMENT.DEPARTMENTNAME,
                            departmentUnitId = (short)c.DEPARTMENTUNITID,
                            departmentUnitName = c.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITNAME,
                            SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION,
                        });

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffNames(int companyId)
        {
            var data = from st in context.TBL_STAFF
                       where st.COMPANYID == companyId
                       select new simpleStaffModel
                       {
                           staffId = st.STAFFID,
                           staffCode = st.STAFFCODE,
                           firstName = st.FIRSTNAME,
                           middleName = st.MIDDLENAME,
                           lastName = st.LASTNAME,
                           //departmentId = st.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                           //departmentUnitId = (short)st.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITID,

                       };

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffRelationshipManagerByStaffId(int staffId)
        {
            var data = from st in context.TBL_STAFF
                       where st.STAFFID == staffId
                       select new simpleStaffModel
                       {
                           staffId = st.STAFFID,
                           staffCode = st.STAFFCODE,
                           firstName = st.FIRSTNAME,
                           middleName = st.MIDDLENAME,
                           lastName = st.LASTNAME,
                       };
            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffBusinessManagerByStaffId(int staffId)
        {
            var data = from st in context.TBL_STAFF
                       where st.STAFFID == staffId
                       select new simpleStaffModel
                       {
                           staffId = st.STAFFID,
                           staffCode = st.STAFFCODE,
                           firstName = st.FIRSTNAME,
                           middleName = st.MIDDLENAME,
                           lastName = st.LASTNAME,
                       };
            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffByUnitId(int companyId, short departmentUnitId)
        {
            return this.GetStaffNames(companyId).Where(x => x.departmentUnitId == departmentUnitId);
        }

        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.TBL_APPROVAL_STATUS
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.APPROVALSTATUSID,
                       approvalStatusName = ap.APPROVALSTATUSNAME,
                       forDisplay = ap.FORDISPLAY,
                   };
        }

        public IQueryable<simpleStaffModel> SearchStaff(string searchQuery, int companyId)
        {
            IQueryable<simpleStaffModel> staff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                staff =
                    context.TBL_STAFF.Where(x => x.DELETED == false)// && x.c == companyId)
                    .Where(x => x.FIRSTNAME.ToLower().Contains(searchQuery)
                    || x.MIDDLENAME.ToLower().Contains(searchQuery)
                    || x.LASTNAME.ToLower().Contains(searchQuery)
                    || x.STAFFCODE.Contains(searchQuery))
                    .Select(o => new simpleStaffModel
                    {
                        staffId = o.STAFFID,
                        firstName = o.FIRSTNAME,
                        middleName = o.MIDDLENAME,
                        lastName = o.LASTNAME,
                        staffCode = o.STAFFCODE,
                    })
                    .Take(12);

            }

            return staff;
        }

        public IQueryable<simpleStaffModel> SearchStaffbyDepartmentId(string searchQuery, int companyId, int departmentId)
        {
            IQueryable<simpleStaffModel> staff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                staff =
                    context.TBL_STAFF.Where(x => x.DELETED == false)// && x.c == companyId)
                    .Where(x => x.FIRSTNAME.ToLower().Contains(searchQuery)
                    || x.MIDDLENAME.ToLower().Contains(searchQuery)
                    || x.LASTNAME.ToLower().Contains(searchQuery)
                    || x.STAFFCODE.Contains(searchQuery)
                    && x.TBL_DEPARTMENT_UNIT.DEPARTMENTID == departmentId)
                    .Select(o => new simpleStaffModel
                    {
                        staffId = o.STAFFID,
                        firstName = o.FIRSTNAME,
                        middleName = o.MIDDLENAME,
                        lastName = o.LASTNAME,
                        staffCode = o.STAFFCODE,
                    })
                    .Take(12)
                ;
            }

            return staff;
        }


        private string StoredFilePath(string filename)
        {
            CreditBureauHelp helper = new CreditBureauHelp();

            string folderName = string.Empty;
            folderName = helper.FilePath();
            folderName = Path.Combine(folderName, "Excel_Uploads");
            string pathString = Path.Combine(folderName, filename);

            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }

            if (!File.Exists(pathString))
            {
                using (StreamWriter sw = new StreamWriter(pathString))
                {
                    // sw.Write(pathString);
                }
                return pathString;
            }
            return pathString;
            //else
            //{
            //    DisposeTicket(pathString);
            //    StoredTicket(userName, ticket);
            //}
        }

        public staffBulkFeedbackViewModel UploadStaffData(StaffDocumentViewModel model, byte[] file)
        {
            var staffInfo = new List<StaffInfoViewModel>();

            var failedStaffInfo = new List<StaffInfoViewModel>();

            var staffBulkFeedbackViewModel = new staffBulkFeedbackViewModel();

            // Loads a spreadsheet from a file with the specified path
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5"); //SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY"); 

            string path = "_" + model.createdBy + "." + model.fileExtension;
            path = StoredFilePath(path);
            File.WriteAllBytes(path, file.ToArray());

            ExcelFile ef = ExcelFile.Load(path);

            ExcelWorksheet ws = ef.Worksheets.ActiveWorksheet;

            CellRange range = ef.Worksheets.ActiveWorksheet.GetUsedCellRange(true);
            var jobTitle = context.TBL_STAFF_JOBTITLE.FirstOrDefault();
            for (int j = range.FirstRowIndex; j <= range.LastRowIndex; j++)
            {
                var rowSuccess = true;
                int excelRowPosition = 1;
                StaffInfoViewModel staffRowData = new StaffInfoViewModel();
                for (int i = range.FirstColumnIndex; i <= range.LastColumnIndex; i++)
                {
                    ExcelCell cell = range[j - range.FirstRowIndex, i - range.FirstColumnIndex];

                    string cellName = CellRange.RowColumnToPosition(j, i);
                    string cellRow = ExcelRowCollection.RowIndexToName(j);
                    string cellColumn = ExcelColumnCollection.ColumnIndexToName(i);
                    excelRowPosition = Convert.ToInt32(cellRow);
                    if (Convert.ToInt32(cellRow) == 1) continue;

                    switch (cellColumn)
                    {
                        case "A":
                        // staffRowData.user.username = cell.Value.ToString();
                        // staffRowData.user.password = cell.Value.ToString();
                        //if (context.TBL_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any() || context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any())
                        //{
                        //    rowSuccess = false;
                        //    staffRowData.errorMessage = staffRowData.errorMessage + "Staff Code Already Exist. ";
                        //}
                        //break;
                        case "B":
                            staffRowData.StaffCode = cell.Value.ToString();
                            // staffRowData.user.username = cell.Value.ToString();
                            // staffRowData.user.password = cell.Value.ToString();
                            if (context.TBL_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any() || context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any())
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Staff Code Already Exist. ";
                            }
                            break;
                        case "C":
                            var roleInfo = context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE == cell.Value.ToString()).FirstOrDefault();
                            if (roleInfo != null)
                            {
                                staffRowData.staffRoleId = roleInfo.STAFFROLEID;
                                staffRowData.staffRoleName = roleInfo.STAFFROLENAME;
                                staffRowData.staffRoleCode = cell.Value.ToString();
                            }
                            else
                            {
                                rowSuccess = false;
                                staffRowData.staffRoleCode = cell.Value.ToString();
                                staffRowData.message = staffRowData.message + $"The ROLECODE @  '{cellColumn}' does not exist in the role log. ";
                                //throw new Exception("The ROLECODE @" + cellColumn + " does not exist in the role log");
                            }
                            break;
                        case "D":
                            var branchInfo = context.TBL_BRANCH.Where(x => x.BRANCHCODE == cell.Value.ToString()).FirstOrDefault();
                            if (branchInfo != null)
                            {
                                staffRowData.BranchId = branchInfo.BRANCHID;
                                staffRowData.BranchName = branchInfo.BRANCHNAME;
                                staffRowData.branchCode = cell.Value.ToString();
                            }
                            else
                            {
                                rowSuccess = false;
                                staffRowData.branchCode = cell.Value.ToString();
                                staffRowData.message = staffRowData.message + $"the 'BRANCHCODE' @  '{cellColumn}' does not exist in the branch log";
                                //throw new Exception($"the 'BRANCHCODE' @" + cellColumn + " does not exist in the branch log");
                            }
                            break;
                        case "E":
                            staffRowData.FirstName = cell.Value.ToString();
                            if (staffRowData.FirstName == null)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + $"Firstname cannot be null. ";
                            }
                            break;
                        case "F":
                            staffRowData.LastName = cell.Value.ToString();
                            if (staffRowData.LastName == null)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + $"LastName cannot be null. ";
                            }
                            break;
                        case "G":
                            staffRowData.MiddleName = cell.Value.ToString();
                            break;
                        case "H":
                            staffRowData.Email = cell.Value.ToString();
                            break;
                        case "I":
                            var supervisor = context.TBL_STAFF.Where(x => x.STAFFCODE.ToLower() == cell.Value.ToString().ToLower()).FirstOrDefault();

                            if (supervisor != null)
                            {
                                staffRowData.staffId = supervisor.STAFFID;
                                staffRowData.supervisorStaffName = supervisor.FIRSTNAME + " " + supervisor.MIDDLENAME + " " + supervisor.LASTNAME;
                            }
                            else
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + $"Supervisor Code @ '{cellColumn}' does not exist. ";
                                // throw new Exception($"Supervisor @" + cellColumn + " does not exist.");
                            }
                            break;
                        case "J":
                            var unit = context.TBL_DEPARTMENT_UNIT.Where(x => x.DEPARTMENTUNITNAME.ToLower() == cell.Value.ToString().ToLower()).FirstOrDefault();

                            if (unit != null) staffRowData.departmentUnitId = unit.DEPARTMENTUNITID;
                            else
                            {
                                staffRowData.departmentUnitId = 10;
                            }
                            break;
                            //case "M":
                            //    var state = context.TBL_STATE.Where(x => x.STATECODE.ToLower() == cell.Value.ToString().ToLower()).FirstOrDefault();

                            //    if (state != null) staffRowData.StateId = state.STATEID;
                            //    else
                            //    {
                            //        rowSuccess = false;
                            //        staffRowData.errorMessage = staffRowData.errorMessage + $"State Code @ '{cellColumn}' does not exist. ";
                            //        //throw new Exception($"the 'State Code' @" + cellColumn + " does not exist.");
                            //    }
                            //    break;

                    }
                }

                if (rowSuccess && excelRowPosition > 1)
                {
                    staffRowData.customerSensitivityLevelId = (short)CustomerSensitivityLevelENum.Negligible;
                    staffRowData.JobTitleId = jobTitle.JOBTITLEID;
                    staffRowData.JobTitleName = jobTitle.JOBTITLENAME;
                    staffRowData.message = "Success";
                    staffInfo.Add(staffRowData);
                }
                else if (!rowSuccess) failedStaffInfo.Add(staffRowData);

            };

            foreach (var staffInfoRow in staffInfo)
            {
                staffInfoRow.createdBy = model.createdBy;
                staffInfoRow.companyId = model.companyId;
                staffInfoRow.BranchId = model.userBranchId;
                staffInfoRow.applicationUrl = model.applicationUrl;
                staffInfoRow.userIPAddress = model.userIPAddress;
                staffInfoRow.applicationUrl = model.applicationUrl;

                staffBulkFeedbackViewModel.commitedRows = staffInfo;
                staffBulkFeedbackViewModel.discardedRows = failedStaffInfo;

                var response = AddSimpleTempStaff(staffInfoRow);
                if (!response)
                {
                    staffBulkFeedbackViewModel.discardedRows.Add(staffInfoRow);
                    staffBulkFeedbackViewModel.commitedRows.Remove(staffInfoRow);
                    staffBulkFeedbackViewModel.failureCount = staffBulkFeedbackViewModel.failureCount + 1;
                    staffBulkFeedbackViewModel.successCount = staffBulkFeedbackViewModel.successCount - 1;
                }

            };
            return staffBulkFeedbackViewModel;
        }

        public bool AddSimpleTempStaff(StaffInfoViewModel staffModel)
        {
            var staffCode = StaticHelpers.GetUniqueKey(6);
            List<TBL_TEMP_PROFILE_USER> userInfo = new List<TBL_TEMP_PROFILE_USER>();
            var user = new TBL_TEMP_PROFILE_USER()
            {
                TEMPSTAFFID = staffModel.staffId,
                USERNAME = staffCode,
                PASSWORD = StaticHelpers.EncryptSha512("password", StaticHelpers.EncryptionKey),
                ISFIRSTLOGINATTEMPT = false,
                ISACTIVE = false,
                ISLOCKED = true,
                FAILEDLOGONATTEMPT = 0,
                SECURITYQUESTION = "What is my firstname",
                SECURITYANSWER = staffModel.FirstName,
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
                CREATEDBY = staffModel.createdBy,
                LASTUPDATEDBY = staffModel.createdBy,
                DATETIMECREATED = DateTime.Now,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                APPROVALSTATUS = false,
                ISCURRENT = true,
            };
            userInfo.Add(user);
            var staff = new TBL_TEMP_STAFF()
            {
                FIRSTNAME = staffModel.FirstName,
                MIDDLENAME = staffModel.MiddleName,
                COMPANYID = staffModel.companyId,
                LASTNAME = staffModel.LastName,
                STAFFCODE = staffCode,
                JOBTITLEID = staffModel.JobTitleId,
                STAFFROLEID = staffModel.staffRoleId,
                SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                ADDRESS = staffModel.Address,
                ADDRESSOFNOK = staffModel.AddressOfNok,
                BRANCHID = staffModel.BranchId,
                COMMENT = staffModel.Comment,
                CREATEDBY = staffModel.createdBy,
                CUSTOMERSENSITIVITYLEVELID = staffModel.customerSensitivityLevelId,
                DATEOFBIRTH = staffModel.DateOfBirth,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTUNITID = staffModel.departmentUnitId,
                EMAIL = staffModel.Email,
                EMAILOFNOK = staffModel.EmailOfNok,
                GENDER = staffModel.Gender,
                GENDEROFNOK = staffModel.GenderOfNok,
                MISINFOID = staffModel.MisinfoId,
                NAMEOFNOK = staffModel.NameOfNok,
                NOKRELATIONSHIP = staffModel.NokrelationShip,
                PHONE = staffModel.Phone,
                PHONEOFNOK = staffModel.PhoneOfNok,
                STATEID = staffModel.StateId,
                CITYID = staffModel.CityId,
                STAFFSIGNATURE = staffModel.StaffSignature,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
                TBL_TEMP_PROFILE_USER = userInfo
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = staffModel.createdBy,
                BRANCHID = (short)staffModel.BranchId,
                DETAIL = $"Initiated Staff and User Creation for '{staffModel?.StaffFullName}' with code'{staffModel?.StaffCode}'",
                IPADDRESS = staffModel.userIPAddress,
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
            context.TBL_TEMP_STAFF.Add(staff);

            var output = context.SaveChanges() > 0;

            workflow.StaffId = staffModel.createdBy;
            workflow.CompanyId = staffModel.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.TargetId = staff.TEMPSTAFFID;
            workflow.Comment = "New Staff Creation";
            workflow.OperationId = (int)OperationsEnum.StaffCreation;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            context.SaveChanges();

            return output;
        }

        #region Staff Signature 

        public bool AddStaffSignature(StaffDocumentViewModel model, byte[] file)
        {
            var audit = new TBL_AUDIT();
            var appdate = genSetup.GetApplicationDate();
            var staffInfo = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == model.staffCode.ToLower());
            var signature = documentsContext.TBL_MEDIA_STAFF_SIGNATURE.FirstOrDefault(x => x.STAFFCODE.ToLower() == model.staffCode.ToLower());

            if (signature == null)
            {
                var document = new Entities.DocumentModels.TBL_MEDIA_STAFF_SIGNATURE()
                {
                    FILENAME = $"{model.staffCode}-{model.fileName}",
                    FILEEXTENSION = model.fileExtension,
                    FILEDATA = file,
                    SYSTEMDATETIME = DateTime.Now,
                    COMPANYID = model.companyId,
                    STAFFCODE = model.staffCode,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = appdate
                };
                documentsContext.TBL_MEDIA_STAFF_SIGNATURE.Add(document);

                // Audit Section ---------------------------
                audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.StaffSignatureUploaded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Signature for staff {staffInfo?.FIRSTNAME + ' ' + staffInfo?.LASTNAME} with code '{ model.staffCode }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = appdate,
                    SYSTEMDATETIME = DateTime.Now
                };
                // End of Audit Section ---------------------
            }
            else
            {
                signature.FILEDATA = file;
                signature.STAFFCODE = model.staffCode;
                signature.FILEEXTENSION = model.fileExtension;
                signature.SYSTEMDATETIME = DateTime.Now;
                signature.DATETIMEUPDATED = appdate;

                // Audit Section ---------------------------
                audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.StaffSignatureUpdated,
                    STAFFID = model.lastUpdatedBy,
                    BRANCHID = model.userBranchId,
                    DETAIL = $"Updated Signature for staff {staffInfo?.FIRSTNAME + ' ' + staffInfo?.LASTNAME} with code '{ model.staffCode }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = appdate,
                    SYSTEMDATETIME = DateTime.Now
                };
            }

            auditTrail.AddAuditTrail(audit);

            return documentsContext.SaveChanges() != 0;
        }

        public bool UpdateStaffSignature(StaffDocumentViewModel model, int documentId)
        {
            var data = documentsContext.TBL_MEDIA_STAFF_SIGNATURE.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.STAFFCODE = model.staffCode;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;
            data.DATETIMEUPDATED = genSetup.GetApplicationDate();

            var staffInfo = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == model.staffCode.ToLower());

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffSignatureUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated Signature for staff {staffInfo?.FIRSTNAME + ' ' + staffInfo?.LASTNAME} with code '{ model.staffCode }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<StaffDocumentViewModel> GetAllStaffSignatures(int companyId)
        {
            var documents = (from doc in documentsContext.TBL_MEDIA_STAFF_SIGNATURE
                             where doc.COMPANYID == companyId
                             select new StaffDocumentViewModel
                             {
                                 documentId = doc.DOCUMENTID,
                                 companyId = doc.COMPANYID,
                                 staffCode = doc.STAFFCODE,
                                 //documentTitle = doc.DOCUMENT_TITLE,
                                 fileData = doc.FILEDATA,
                                 fileName = doc.FILENAME,
                                 fileExtension = doc.FILEEXTENSION,
                                 SystemDateTime = doc.SYSTEMDATETIME,
                                 dateTimeCreated = doc.DATETIMECREATED
                             }).ToList();

            return documents;
        }

        public StaffDocumentViewModel GetStaffSignatureByStaffCode(string staffCode, int companyId)
        {
            var data = GetAllStaffSignatures(companyId).FirstOrDefault(x =>
                string.Equals(x.staffCode.ToLower(), staffCode.ToLower(), StringComparison.Ordinal));
            return data;
        }


        #endregion Staff Signature

        public bool UpdateSupervisor(SupervisorViewModel model)
        {
            var pending = context.TBL_TEMP_STAFF.Where(x =>
                x.ISCURRENT == true
                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                && x.STAFFCODE.ToLower() == model.staffCode.ToLower()
              );

            if (pending.Any()) throw new Exception("Staff is already undergoing approval");

            string comment = model.status + " delegate";
            int staffid = model.supervisorStaffId;
            var staff = context.TBL_STAFF.Find(model.supervisorStaffId);


            var temp = context.TBL_TEMP_STAFF.Add(new TBL_TEMP_STAFF()
            {
                TEMPSTAFFID = (int)staffid, // <-------- real item changing here
                FIRSTNAME = staff.FIRSTNAME,
                MIDDLENAME = staff.MIDDLENAME,
                LASTNAME = staff.LASTNAME,
                STAFFCODE = staff.STAFFCODE,
                JOBTITLEID = staff.JOBTITLEID,
                COMPANYID = staff.COMPANYID,
                STAFFROLEID = staff.STAFFROLEID,
                SUPERVISOR_STAFFID = model.supervisorId,
                ADDRESS = staff.ADDRESS,
                ADDRESSOFNOK = staff.ADDRESSOFNOK,
                BRANCHID = staff.BRANCHID,
                COMMENT = comment,
                CREATEDBY = model.createdBy,
                CUSTOMERSENSITIVITYLEVELID = staff.CUSTOMERSENSITIVITYLEVELID,
                DATEOFBIRTH = staff.DATEOFBIRTH,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTUNITID = staff.DEPARTMENTUNITID,
                EMAIL = staff.EMAIL,
                EMAILOFNOK = staff.EMAILOFNOK,
                GENDER = staff.GENDER,
                GENDEROFNOK = staff.GENDEROFNOK,
                MISINFOID = staff.MISINFOID,
                NAMEOFNOK = staff.NAMEOFNOK,
                NOKRELATIONSHIP = staff.NOKRELATIONSHIP,
                PHONE = staff.PHONE,
                PHONEOFNOK = staff.PHONEOFNOK,
                STATEID = staff.STATEID,
                CITYID = staff.CITYID,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
            });

            context.SaveChanges();

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.TargetId = temp.TEMPSTAFFID;
            workflow.Comment = comment;
            workflow.OperationId = (int)OperationsEnum.StaffCreation;
            workflow.ExternalInitialization = true;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }
        public byte[] GetStaffSampleDocument()
        {
            // HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data")
            var pathString = HttpContext.Current.ApplicationInstance.Server.MapPath("~/App_Data/StaffSampleDocument.xlsx");
            byte[] readBuffer = System.IO.File.ReadAllBytes(pathString);

            return readBuffer;

            //string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //string filePath = Path.Combine(appDataFolder, "test.txt");
            //var reader = new StreamReader(filePath);

            //string path = Server.MapPath(String.Format("~/App_Data/uploads/{0}", fileName));
            //if (File.Exists(path))
            //{
            //    return File(path, "application/pdf");
            //}
        }
    }
}