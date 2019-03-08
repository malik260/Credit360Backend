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
using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.StagingModels;
using System.Data.Entity.Validation;

namespace FintrakBanking.Repositories.Setups.General
{
    public class StaffRepository : IStaffRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IAdminRepository adminRepo;
        private IWorkflow workflow;
        private IApprovalLevelStaffRepository level;
        private FinTrakBankingDocumentsContext documentsContext;
        private FinTrakBankingStagingContext stagingContext;
        private IStaffMIS staffMIS;
        TBL_PROFILE_SETTING profile_Setting;

        public object FileUploadControl { get; private set; }

        public StaffRepository(FinTrakBankingContext _context,
                               IAuditTrailRepository _auditTrail,
                               IGeneralSetupRepository _genSetup,
                               IWorkflow _workFlow,
                               IApprovalLevelStaffRepository _level,
                               FinTrakBankingDocumentsContext _documentsContext,
                               IStaffMIS _staffMIS,
                               IAdminRepository _adminRepo,
        FinTrakBankingStagingContext _stagingContext)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            this.workflow = _workFlow;
            level = _level;
            documentsContext = _documentsContext;
            stagingContext = _stagingContext;
            staffMIS = _staffMIS;
            adminRepo = _adminRepo;
            profile_Setting = _context.TBL_PROFILE_SETTING.FirstOrDefault();
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
                         where c.DELETED == false
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
                             loanLimit = c.LOAN_LIMIT,
                             workStartDuration = c.WORKSTARTDURATION,
                             workEndDuration = c.WORKENDDURATION,
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
        public List<StaffSensitivityLevelViewModel> GetStaffSensitivityLevel()
        {
            var staff = (from c in context.TBL_CUSTOMER_SENSITIVITY_LEVEL
                         select new StaffSensitivityLevelViewModel()
                         {
                             level = c.CUSTOMERSENSITIVITYLEVELID,
                             description = c.DESCRIPTION,
                         }).ToList();
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
                             loanLimit = c.LOAN_LIMIT,
                             workStartDuration = c.WORKSTARTDURATION,
                             workEndDuration = c.WORKENDDURATION,
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
                    if (staffModel.user.changeSecutirtyQuestion)
                    {
                        existingTempUser.SECURITYQUESTION = staffModel.user.securityQuestion;
                        existingTempUser.SECURITYANSWER = staffModel.user.securityAnswer;
                    }

                    existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER);
                    //existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays);
                    existingTempUser.LASTUPDATEDBY = staffModel.createdBy;
                    existingTempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    existingTempUser.APPROVALSTATUS = false;
                    existingTempUser.ISCURRENT = true;
                    existingTempUser.TBL_TEMP_PROFILE_ADTN_ACTIVITY = userActivities;
                    existingTempUser.TBL_TEMP_PROFILE_USERGROUP = userGroups;
                }
            }
            else
            {
                    user = new TBL_TEMP_PROFILE_USER()
                    {
                        TEMPSTAFFID = staffModel.staffId,
                        USERNAME = staffModel.user.username,
                        PASSWORD = StaticHelpers.EncryptSha512(staffModel.user.password != null ? staffModel.user.password : context.TBL_PROFILE_USER.Where(x=>x.USERNAME == staffModel.user.username).Select(m=>m.PASSWORD).FirstOrDefault(), StaticHelpers.EncryptionKey),
                        ISFIRSTLOGINATTEMPT = false,
                        ISACTIVE = false,
                        ISLOCKED = true,
                        FAILEDLOGONATTEMPT = 0,
                        SECURITYQUESTION = staffModel.user.securityQuestion,
                        SECURITYANSWER = staffModel.user.securityAnswer,
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER),
                        //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
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
            var unApprovedStaffEdit = context.TBL_TEMP_STAFF.Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending &&
                                                                        x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower());
            TBL_TEMP_STAFF tempStaff = new TBL_TEMP_STAFF();

            if (unApprovedStaffEdit.Any())
            {
                throw new SecureException("Staff is already undergoing approval");
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
                //tempStaffToUpdate.JOBTITLEID = staffModel.JobTitleId;
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
                //tempStaffToUpdate.DEPARTMENTUNITID = (short)staffModel.departmentUnitId;
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
                tempStaffToUpdate.LOAN_LIMIT = staffModel.loanLimit;
                tempStaffToUpdate.WORKSTARTDURATION = staffModel.workStartDuration;
                tempStaffToUpdate.WORKENDDURATION = staffModel.workEndDuration;
            }
            else
            {
                var targetStaff = context.TBL_STAFF.Find(staffid);
                try
                {
                    tempStaff = new TBL_TEMP_STAFF()
                    {
                        FIRSTNAME = staffModel.FirstName,
                        MIDDLENAME = staffModel.MiddleName,
                        LASTNAME = staffModel.LastName,
                        STAFFCODE = staffModel.StaffCode,
                        //JOBTITLEID = staffModel.JobTitleId,
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
                        //DEPARTMENTUNITID = (short)staffModel.departmentUnitId,
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
                        LOAN_LIMIT = staffModel.loanLimit,
                        ISCURRENT = true,
                        WORKSTARTDURATION = staffModel.workStartDuration,
                        WORKENDDURATION = staffModel.workEndDuration
                    };
                }
                catch (Exception ex)
                {
                    var ext = ex; 
                }
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

                    workflow.StaffId = staffModel.createdBy;
                    workflow.CompanyId = staffModel.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                    workflow.TargetId = targetStaffId;
                    workflow.Comment = "Update Staff Creation";
                    workflow.OperationId = (int)OperationsEnum.StaffCreation;
                    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                    var response = context.SaveChanges() > 0;

                    //var entity = new ApprovalViewModel
                    //{
                    //    staffId = staffModel.createdBy,
                    //    companyId = staffModel.companyId,
                    //    approvalStatusId = (int)ApprovalStatusEnum.Pending,
                    //    targetId = targetStaffId,
                    //    operationId = (int)OperationsEnum.StaffCreation,
                    //    BranchId = staffModel.userBranchId,
                    //    externalInitialization = true
                    //};
                    //var response = workflow.LogForApproval(entity);

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
                    throw new SecureException(ex.Message);
                }
            }
        }

        public bool LogDeleteRequestStaff(int staffId, UserInfo user)
        {
            var targetStaff = context.TBL_STAFF.Find(staffId);

            var existingApprovalEntry = context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == targetStaff.STAFFCODE && x.ISCURRENT == true && x.OPERATION.ToLower() == "delete").ToList();

            if(existingApprovalEntry.Count() <= 0)
            {
                var newTempStaff = new TBL_TEMP_STAFF();

                newTempStaff.ISCURRENT = true;
                newTempStaff.JOBTITLEID = targetStaff.JOBTITLEID;
                newTempStaff.LASTNAME = targetStaff.LASTNAME;
                newTempStaff.FIRSTNAME = targetStaff.FIRSTNAME;
                newTempStaff.MIDDLENAME = targetStaff.MIDDLENAME;
                newTempStaff.LOAN_LIMIT = targetStaff.LOAN_LIMIT;
                newTempStaff.LASTUPDATEDBY = targetStaff.LASTUPDATEDBY;
                newTempStaff.ADDRESS = targetStaff.ADDRESS;
                newTempStaff.ADDRESSOFNOK = targetStaff.ADDRESSOFNOK;
                newTempStaff.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                newTempStaff.BRANCHID = targetStaff.BRANCHID;
                newTempStaff.CITYID = targetStaff.CITYID;
                newTempStaff.COMMENT = targetStaff.COMMENT;
                newTempStaff.COMPANYID = targetStaff.COMPANYID;
                newTempStaff.CREATEDBY = targetStaff.CREATEDBY;
                newTempStaff.STAFFCODE = targetStaff.STAFFCODE;
                newTempStaff.STAFFROLEID = targetStaff.STAFFROLEID;
                newTempStaff.MISINFOID = targetStaff.MISINFOID;
                newTempStaff.NOKRELATIONSHIP = targetStaff.NOKRELATIONSHIP;
                newTempStaff.PHONE = targetStaff.PHONE;
                newTempStaff.PHONEOFNOK = targetStaff.PHONEOFNOK;
                newTempStaff.STATEID = targetStaff.STATEID;
                newTempStaff.SUPERVISOR_STAFFID = targetStaff.SUPERVISOR_STAFFID;
                newTempStaff.EMAILOFNOK = targetStaff.EMAILOFNOK;
                newTempStaff.EMAIL = targetStaff.EMAIL;
                newTempStaff.DEPARTMENTUNITID = targetStaff.DEPARTMENTUNITID;
                newTempStaff.DATETIMEUPDATED = targetStaff.DATETIMEUPDATED;
                newTempStaff.DATETIMECREATED = targetStaff.DATETIMECREATED;
                newTempStaff.DATEOFBIRTH = targetStaff.DATEOFBIRTH;
                newTempStaff.CUSTOMERSENSITIVITYLEVELID = targetStaff.CUSTOMERSENSITIVITYLEVELID;
                newTempStaff.OPERATION = "Delete";

                context.TBL_TEMP_STAFF.Add(newTempStaff);

                using (var trans = context.Database.BeginTransaction())
                {
                    if (context.SaveChanges() > 0 == false)
                    {
                        trans.Rollback();
                        throw new ConditionNotMetException("User delete failed. Contact Administrator");
                    }


                    workflow.StaffId = user.createdBy;
                    workflow.CompanyId = user.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                    workflow.TargetId = newTempStaff.TEMPSTAFFID;
                    workflow.Comment = "Delete Staff";
                    workflow.OperationId = (int)OperationsEnum.DeleteStaff;
                    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                    if(context.SaveChanges() > 0 == false)
                    {
                        trans.Rollback();
                        throw new ConditionNotMetException("User delete failed. Contact Administrator");
                    }
                    trans.Commit();
                    return true;

                }
            }
            else
            {
                throw new ConditionNotMetException("This staff record is currently undergoing approval");
            }
        }

        public int GoForStaffDeleteApproval(ApprovalViewModel entity)
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
                    workflow.OperationId = (int)OperationsEnum.DeleteStaff;
                    workflow.ExternalInitialization = false;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();

                    context.SaveChanges();

                    if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var staff = context.TBL_TEMP_STAFF.Find(entity.targetId);
                        staff.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                        context.SaveChanges();
                        trans.Commit();
                        return 2;
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = DeleteStaff(entity.targetId, (short)workflow.StatusId, entity);

                        if (response)
                        {
                            context.SaveChanges();
                            trans.Commit();
                            return 1;
                        }
                        else return 3;
                    }
                    else
                    {
                        context.SaveChanges();
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool DeleteStaff(int staffId, int approvalStatusId, ApprovalViewModel entity)
        {
            var tempStaff = context.TBL_TEMP_STAFF.Find(staffId);
            var targetStaff = context.TBL_STAFF.FirstOrDefault(x=>x.STAFFCODE == tempStaff.STAFFCODE);

            targetStaff.DELETED = true;
            targetStaff.DELETEDBY = entity.createdBy;
            targetStaff.DATETIMEDELETED = DateTime.Now;

            var userAccount = context.TBL_PROFILE_USER.Where(x => x.STAFFID == targetStaff.STAFFID).FirstOrDefault();
            userAccount.ISACTIVE = false;
            userAccount.ISLOCKED = true;

            tempStaff.ISCURRENT = false;


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffDeleted,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.BranchId,
                DETAIL = $"Approved Deleted Staff '{targetStaff?.FIRSTNAME}' with code'{targetStaff?.STAFFCODE}'",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return context.SaveChanges() > 0;
        }


        public IEnumerable<StaffInfoViewModel> GetStaffDeleteRequestAwaitingApprovals(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.DeleteStaff).ToList();

            var staff = (from c in context.TBL_TEMP_STAFF
                         join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                         join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                         join t in context.TBL_APPROVAL_TRAIL on c.TEMPSTAFFID equals t.TARGETID
                         where
                             (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                             && c.ISCURRENT == true
                             && t.RESPONSESTAFFID == null
                             && t.OPERATIONID == (int)OperationsEnum.DeleteStaff
                         && ids.Contains((int)t.TOAPPROVALLEVELID) 
                         && c.OPERATION.ToLower() == "delete"
                         && c.COMPANYID ==companyId
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

                             OperationId = (short)OperationsEnum.DeleteStaff,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION
                         }).ToList();

            return staff;
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

        public int GoForApproval(ApprovalViewModel entity)
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

                    if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                    {
                        var staff = context.TBL_TEMP_STAFF.Find(entity.targetId);
                        staff.APPROVALSTATUSID = (short)ApprovalStatusEnum.Disapproved;
                        context.SaveChanges();
                        trans.Commit();
                        return 2;
                    }

                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveStaff(entity.targetId, (short)workflow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return 1;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
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
                            // var comparedGroup = targetGroups.Where(t1 => !tempGroup.Any(t2 => t1.GROUPID == t2.GROUPID));
                            // foreach (var item in comparedGroup)
                            foreach (var item in targetGroups)
                            {
                                context.TBL_PROFILE_USERGROUP.Remove(item);
                            }
                        }
                        if (targetActivities.Any())
                        {
                            // var comparedactivities = targetActivities.Where(t1 => !tempActivities.Any(t2 => t1.ACTIVITYID == t2.ACTIVITYID));
                            // foreach (var item in comparedactivities)
                            foreach (var item in targetActivities)
                            {
                                context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
                            }
                        }
                      context.SaveChanges();
                    }
                }
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
            foreach (var item in tempGroup)
            {
                item.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                item.ISCURRENT = false;
                item.DATEAPPROVED = DateTime.Now;
            }
            foreach (var item in tempGroup)
            {
                context.TBL_TEMP_PROFILE_USERGROUP.Remove(item);
            }
            foreach (var item in tempActivities)
            {
                context.TBL_TEMP_PROFILE_ADTN_ACTIVITY.Remove(item);
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
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER),
                    //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
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
                entity.LOAN_LIMIT = temp.LOAN_LIMIT;
                entity.DELETED = false;
                entity.WORKSTARTDURATION = temp.WORKSTARTDURATION;
                entity.WORKENDDURATION = temp.WORKENDDURATION;
            }
            else //Insert a new staff record into the real staff table
            {
                entity = new TBL_STAFF()
                {
                    //STAFFID = 3000,
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
                    LOAN_LIMIT = temp.LOAN_LIMIT,
                    WORKSTARTDURATION = temp.WORKSTARTDURATION,
                    WORKENDDURATION = temp.WORKENDDURATION,
                };
                if (temp.CUSTOMERSENSITIVITYLEVELID >= 1) entity.CUSTOMERSENSITIVITYLEVELID = temp.CUSTOMERSENSITIVITYLEVELID;
                context.TBL_STAFF.Add(entity);
                var test = context.SaveChanges() > 0;

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
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ",
                    ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
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
                            output = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
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
                        NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER),
                        //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
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
                    entity.LOAN_LIMIT = temp.LOAN_LIMIT;
                    entity.WORKSTARTDURATION = temp.WORKSTARTDURATION;
                    entity.WORKENDDURATION = temp.WORKENDDURATION;
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
                        LOAN_LIMIT = temp.LOAN_LIMIT,
                        WORKSTARTDURATION = temp.WORKSTARTDURATION,
                    WORKENDDURATION = temp.WORKENDDURATION
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
                throw new SecureException(ex.Message);
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
                throw new SecureException("Staff Information already exist and is undergoing approval");
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
                    NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER),
                   // NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
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
                STAFFCODE = staffModel.StaffCode,
                //JOBTITLEID = staffModel.JobTitleId,
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
                //DEPARTMENTUNITID = staffModel.departmentUnitId,
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
                LOAN_LIMIT = staffModel.loanLimit,
                STAFFSIGNATURE = staffModel.StaffSignature,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = true,
                WORKSTARTDURATION = staffModel.workStartDuration,
                WORKENDDURATION = staffModel.workEndDuration
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = staffModel.createdBy,
                BRANCHID = (short)staffModel.BranchId,
                DETAIL = $"Updated Staff Creation for '{staffModel?.StaffFullName}' with code'{staffModel?.StaffCode}'",
                IPADDRESS = staffModel.userIPAddress,
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
            context.TBL_TEMP_STAFF.Add(staff);
            try
            {
                output = context.SaveChanges() > 0;
            }
            catch(Exception ex)
            {
                var cd = ex;
            }
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
                            loanLimit = c.LOAN_LIMIT,
                        });

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffNames(int companyId)
        {
            var data = from st in context.TBL_STAFF
                       where st.COMPANYID == companyId
                       orderby st.FIRSTNAME, st.MIDDLENAME, st.LASTNAME ascending
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

        public IQueryable<simpleStaffModel> SearchStaff(string searchQuery = "", int companyId=0)
        {
            IQueryable<simpleStaffModel> staff = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            { 
                searchQuery = searchQuery.Trim().ToLower();
            
                staff = context.TBL_STAFF.Where(x => x.DELETED == false)// && x.c == companyId)
                    .Where(x => x.FIRSTNAME.ToLower().Contains(searchQuery)
                    || x.MIDDLENAME.ToLower().Contains(searchQuery)
                    || x.LASTNAME.ToLower().Contains(searchQuery)
                    || x.STAFFCODE.ToLower().Contains(searchQuery))
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

        public staffBulkFeedbackViewModel UploadStaffData(StaffDocumentViewModel model, byte[] file)
        {
            var staffInfo = new List<StaffInfoViewModel>();

            var failedStaffInfo = new List<StaffInfoViewModel>();

            var staffBulkFeedbackViewModel = new staffBulkFeedbackViewModel();
            var setupGlobal = context.TBL_SETUP_GLOBAL.FirstOrDefault();

            // Loads a spreadsheet from a file with the specified path
            //Limited unlicenced key : SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY"); 
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5");

            MemoryStream ms = new MemoryStream(file);

            ExcelFile ef = ExcelFile.Load(ms, LoadOptions.XlsxDefault);

            //ExcelWorksheet ws = ef.Worksheets.ActiveWorksheet;
            ExcelWorksheet ws = ef.Worksheets[0]; //.ActiveWorksheet;

            CellRange range = ef.Worksheets.ActiveWorksheet.GetUsedCellRange(true);
            var jobTitle = context.TBL_STAFF_JOBTITLE.FirstOrDefault();
            for (int j = range.FirstRowIndex; j <= range.LastRowIndex; j++)
            {
                var rowSuccess = true;
                int excelRowPosition = 1;
                StaffInfoViewModel staffRowData = new StaffInfoViewModel();
                Users getADDetails = new Users();
                
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
                            if (context.TBL_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any() || context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any())
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Staff Code Already Exist. ";
                            }
                            if (setupGlobal.USE_ACTIVE_DIRECTORY)
                            {
                                getADDetails = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                                if (getADDetails == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                                }
                                else
                                {
                                    if (context.TBL_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any() || context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any())
                                    {
                                        rowSuccess = false;
                                        staffRowData.message = staffRowData.message + "Staff Code Already Exist. ";
                                    }
                                }
                            }
                            // staffRowData.user.username = cell.Value.ToString();
                            // staffRowData.user.password = cell.Value.ToString();
                           
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
                                staffRowData.message = staffRowData.message + $"The ROLECODE @  cell '{cellColumn}' of row '{cellRow}' does not exist in the role log. ";
                            }
                            break;
                        case "D":
                            string cellValue = cell.Value.ToString();
                            var branchInfoSub = context.TBL_BRANCH.Where(x => x.BRANCHCODE == cellValue);

                            var branchInfo = branchInfoSub.FirstOrDefault();

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
                                staffRowData.message = staffRowData.message + $"the 'BRANCHCODE' @  cell '{cellColumn}' of row '{cellRow}' does not exist in the branch log";
                            }
                            break;
                        case "E":
                            if(setupGlobal.USE_ACTIVE_DIRECTORY)
                            {
                                var firstName = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                                if (firstName == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                                }
                                else
                                {
                                    staffRowData.FirstName = firstName.firstName;
                                }
                            }
                            else
                            {
                            staffRowData.FirstName = cell.Value.ToString();
                            if (staffRowData.FirstName == null)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + $"Firstname cannot be null. ";
                            }
                            }                           
                            break;
                        case "F":
                            if (setupGlobal.USE_ACTIVE_DIRECTORY == true)
                            {
                                var record = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                                if (record == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                                }
                                else
                                {
                                    staffRowData.LastName = record.lastName;
                                }
                            }
                            else
                            {
                                staffRowData.LastName = cell.Value.ToString();
                                if (staffRowData.LastName == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + $"LastName cannot be null. ";
                                }
                            }                           
                            break;
                        case "G":
                            if (setupGlobal.USE_ACTIVE_DIRECTORY == true)
                            {
                                var record = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                                if (record != null)
                                {
                                    staffRowData.MiddleName = record.middleName;
                                }
                                else
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                                }
                            }
                            else
                            {
                                staffRowData.MiddleName = cell.Value.ToString();
                            }
                            break;
                        case "H":
                            staffRowData.Email = cell.Value.ToString();
                            break;
                        case "I":
                            string iCellValue = cell.Value.ToString();
                            var supervisorInfoSub = context.TBL_STAFF.Where(x => x.STAFFCODE == iCellValue);

                            var supervisor = supervisorInfoSub.FirstOrDefault();

                            if (supervisor != null)
                            {
                                staffRowData.staffId = supervisor.STAFFID;
                                staffRowData.supervisorStaffName = supervisor.FIRSTNAME + " " + supervisor.MIDDLENAME + " " + supervisor.LASTNAME;
                            }
                            else
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + $"Supervisor Code @ cell '{cellColumn}' of row '{cellRow}' does not exist. ";
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
                            //        //throw new SecureException($"the 'State Code' @" + cellColumn + " does not exist.");
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
                else if (!rowSuccess && excelRowPosition > 1) failedStaffInfo.Add(staffRowData);

            };
            if (staffInfo.Count() < 1)
            {
                staffBulkFeedbackViewModel.commitedRows = staffInfo;
                staffBulkFeedbackViewModel.discardedRows = failedStaffInfo;
            }
            else
            {
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
            }

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
                NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER),
                //NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays),
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

            if (pending.Any()) throw new SecureException("Staff is already undergoing approval");

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

        public List<simpleStaffModel> StaffReportingLine(int staffId, string staffCode, int companyId)
        {
            List<simpleStaffModel> list=new List<simpleStaffModel>();
            if (staffCode != "undefined")
            {
                staffId = context.TBL_STAFF.Where(a => a.STAFFCODE.ToLower() == staffCode.Trim().ToLower() && a.COMPANYID == companyId).Select(a => a.STAFFID).FirstOrDefault();
            }
            list = context.TBL_STAFF.Where(x => x.SUPERVISOR_STAFFID == staffId && x.COMPANYID == companyId)
                        .Select(x => new simpleStaffModel
                        {
                            staffCode = x.STAFFCODE,
                            firstName = x.FIRSTNAME + " " + x.LASTNAME + " " + x.MIDDLENAME,
                            branchCode = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHCODE).FirstOrDefault(),
                            branchName = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHNAME).FirstOrDefault(),
                            email = x.EMAIL
                        }).ToList();
            
            return list;


        }

        public simpleStaffModel StaffReportingTo(int staffId, string staffCode, int companyId)
        {
            if (staffCode != "null")
            {
                staffId = context.TBL_STAFF.Where(a => a.STAFFCODE.ToLower() == staffCode.Trim().ToLower() && a.COMPANYID == companyId).Select(a => a.STAFFID).FirstOrDefault();
            }
            var supervisorStaffId =  context.TBL_STAFF.Where(a => a.STAFFID == staffId && a.COMPANYID == companyId).Select(a => a.SUPERVISOR_STAFFID).FirstOrDefault();
            return context.TBL_STAFF.Where(x => x.STAFFID == supervisorStaffId && x.COMPANYID == companyId)
                  .Select(x => new simpleStaffModel
                  {
                      staffCode = x.STAFFCODE,
                      firstName = x.FIRSTNAME + " " + x.LASTNAME + " " + x.MIDDLENAME,
                      branchCode = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHCODE).FirstOrDefault(),
                      branchName = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHNAME).FirstOrDefault(),
                      email = x.EMAIL
                  }).FirstOrDefault();
        }
        public simpleStaffModel StaffInformation(int staffId, string staffCode, int companyId)
        {
            if (staffCode != "null")
            {
                staffId = context.TBL_STAFF.Where(a => a.STAFFCODE.ToLower() == staffCode.Trim().ToLower() && a.COMPANYID == companyId).Select(a => a.STAFFID).FirstOrDefault();
            }

            return context.TBL_STAFF.Where(x => x.STAFFID == staffId && x.COMPANYID == companyId)
                  .Select(x => new simpleStaffModel
                  {
                      staffCode = x.STAFFCODE,
                      firstName = x.FIRSTNAME + " " + x.LASTNAME + " " + x.MIDDLENAME,
                      branchCode = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHCODE).FirstOrDefault(),
                      branchName = context.TBL_BRANCH.Where(a => a.BRANCHID == x.BRANCHID).Select(a => a.BRANCHNAME).FirstOrDefault(),
                      email = x.EMAIL
                  }).FirstOrDefault();
        }

        public StaffMISDetailsModel StaffMIS(int staffId, string staffCode)
        {
            if (staffCode != "null")
            {
                staffId = context.TBL_STAFF.Where(a => a.STAFFCODE.ToLower() == staffCode.Trim().ToLower()).Select(a => a.STAFFID).FirstOrDefault();
            }
            StaffMISDetailsModel model = new StaffMISDetailsModel();
            var misRecord = staffMIS.StaffInformationSystem(staffId);
            model.username = misRecord.field1;
            model.teamUnit = misRecord.field2;
            model.costCent = misRecord.field3;
            model.dept = misRecord.field4;
            model.region = misRecord.field5;
            model.group = misRecord.field6;
            model.directorate = misRecord.field7;

            return model;
        }

        public IEnumerable<simpleStaffModel> GetSearchedStaff(string search)
        {

            var branches = from x in context.TBL_STAFF
                           where x.DELETED == false
                           && x.FIRSTNAME.Contains(search.ToUpper())
                           || x.LASTNAME.Contains(search.ToUpper())
                           || x.STAFFCODE.Contains(search.ToUpper())
                           select new simpleStaffModel
                           {
                               staffId = x.STAFFID,
                               firstName = x.FIRSTNAME + " " + x.LASTNAME,
                               staffCode = x.STAFFCODE,
                               branchName =  context.TBL_BRANCH.Where(a=>a.BRANCHID==x.BRANCHID).Select(a=>a.BRANCHNAME + " - " + a.BRANCHCODE).FirstOrDefault()
                            
                           };

            return branches.ToList();
        }
    }
}