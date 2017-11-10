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

namespace FintrakBanking.Repositories.Setups.General
{
    public class StaffRepository : IStaffRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;
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
            this.workFlow = _workFlow;
            level = _level;
            documentsContext = _documentsContext;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        /// <summary>
        /// Add staff information which a collection of staff.
        /// </summary>
        /// <param name="staffModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get all staff information.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<StaffInfoViewModel> GetAllStaff()
        {
            var staff = (from c in context.TBL_STAFF
                         join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                         join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                         join dept in context.TBL_DEPARTMENT on c.DEPARTMENTID equals dept.DEPARTMENTID
                         select new StaffInfoViewModel()
                         {
                             StaffId = c.STAFFID,
                             Address = c.ADDRESS,
                             companyId = coy.COMPANYID,
                             AddressOfNok = c.ADDRESSOFNOK,
                             BranchId = br.BRANCHID,
                             Comment = c.COMMENT,
                             //createdBy = c.CreatedBy.Value,
                             CustomerSensitivityLevel = c.CUSTOMERSENSITIVITYLEVEL,
                             DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                             //dateTimeCreated = c.DateTimeCreated,
                             DepartmentId = c.DEPARTMENTID,
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
                             StaffSignature = c.STAFFSIGNATURE,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             RankId = c.RANKID,
                             BranchName = br.BRANCHNAME,
                             DepartmentName = dept.DEPARTMENTNAME,
                             DepartmentUnitId = (short)c.DEPARTMENT_UNITID,
                             DepartmentUnitName = c.TBL_DEPARTMENT_UNIT.UNIT_NAME,
                             //MisInfoCode = c.MISC,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVEL).DESCRIPTION,
                             //State = c.State.StateName
                             CityId = c.CITYID
                         });
            return staff;
        }

        /// <summary>
        /// Get staff by the staff id which returens object of staff
        /// </summary>
        /// <param name="staffId">staff id </param>
        /// <returns></returns>
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
                             CustomerSensitivityLevel = c.CUSTOMERSENSITIVITYLEVEL,
                             DateOfBirth = c.DATEOFBIRTH,
                             dateTimeCreated = (DateTime)c.DATETIMECREATED,
                             DepartmentId = c.DEPARTMENTID,
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
                             StaffSignature = c.STAFFSIGNATURE,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             RankId = c.RANKID,
                             //BranchName = br.BranchName,

                             //DepartmentName = c.Department.DepartmentName,
                             //MisInfoCode = c.Misinfo.Misname,
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVEL).DESCRIPTION,
                             //State = c.State.StateName
                         }).SingleOrDefault();
            return staff;
        }

        /// <summary>
        /// Update staff information.
        /// </summary>
        /// <param name="StafffViewModel"></param>
        /// <returns></returns>
        public async Task<bool> UpdateStaff(int staffid, StaffInfoViewModel staffModel)
        {
            var existingTempStaff = context.TBL_TEMP_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower() && x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

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
                tempStaffToUpdate.RANKID = staffModel.RankId;
                tempStaffToUpdate.ADDRESS = staffModel.Address;
                tempStaffToUpdate.ADDRESSOFNOK = staffModel.AddressOfNok;
                tempStaffToUpdate.BRANCHID = staffModel.BranchId;
                tempStaffToUpdate.COMMENT = staffModel.Comment;
                tempStaffToUpdate.CREATEDBY = staffModel.createdBy;
                tempStaffToUpdate.CUSTOMERSENSITIVITYLEVEL = staffModel.CustomerSensitivityLevel;
                tempStaffToUpdate.DATEOFBIRTH = staffModel.DateOfBirth;
                tempStaffToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempStaffToUpdate.DEPARTMENTID = staffModel.DepartmentId;
                tempStaffToUpdate.DEPARTMENTUNITID = (short)staffModel.DepartmentUnitId;
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
                    RANKID = staffModel.RankId,
                    ADDRESS = staffModel.Address,
                    ADDRESSOFNOK = staffModel.AddressOfNok,
                    BRANCHID = staffModel.BranchId,
                    COMMENT = staffModel.Comment,
                    CREATEDBY = staffModel.createdBy,
                    CUSTOMERSENSITIVITYLEVEL = staffModel.CustomerSensitivityLevel,
                    DATEOFBIRTH = staffModel.DateOfBirth,
                    DATETIMECREATED = DateTime.Now,
                    DEPARTMENTID = staffModel.DepartmentId,
                    DEPARTMENTUNITID = (short)staffModel.DepartmentUnitId,
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

                    var output = await context.SaveChangesAsync() > 0;

                    var targetStaffId = existingTempStaff?.STAFFID ?? tempStaff.STAFFID;

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
                    var response = workFlow.LogForApproval(entity);

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

        /// <summary>
        /// Delete selected staff.
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get staff names
        /// </summary>
        /// <returns></returns>
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
            entity.operationId = (int)OperationsEnum.StaffCreation;

            entity.externalInitialization = false;

            using ( var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.LogForApproval(entity);
                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new Exception("Approval Failed");
                    }

                    if (workFlow.NewState == (int) ApprovalState.Ended)
                    {
                        var response = ApproveStaff(entity.targetId, (short) workFlow.StatusId, entity);

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
            var staffModel = context.TBL_TEMP_STAFF.Find(staffid);
            var staffToUpdate = context.TBL_STAFF.Where(x => x.STAFFCODE.ToLower() == staffModel.STAFFCODE.ToLower());

            if (staffToUpdate.Any()) //Update existing staff with tempStaff record
            {
                var existingStaff = staffToUpdate.First();

                if (staffModel != null)
                {
                    existingStaff.FIRSTNAME = staffModel.FIRSTNAME;
                    existingStaff.COMPANYID = staffModel.COMPANYID;
                    existingStaff.MIDDLENAME = staffModel.MIDDLENAME;
                    existingStaff.LASTNAME = staffModel.LASTNAME;
                    existingStaff.STAFFCODE = staffModel.STAFFCODE;
                    existingStaff.JOBTITLEID = staffModel.JOBTITLEID;
                    existingStaff.RANKID = staffModel.RANKID;
                    existingStaff.ADDRESS = staffModel.ADDRESS;
                    existingStaff.ADDRESSOFNOK = staffModel.ADDRESSOFNOK;
                    existingStaff.BRANCHID = staffModel.BRANCHID;
                    existingStaff.COMMENT = staffModel.COMMENT;
                    existingStaff.CREATEDBY = staffModel.CREATEDBY;
                    existingStaff.CUSTOMERSENSITIVITYLEVEL = staffModel.CUSTOMERSENSITIVITYLEVEL;
                    existingStaff.DATEOFBIRTH = staffModel.DATEOFBIRTH;
                    existingStaff.DATETIMEUPDATED = DateTime.Now;
                    existingStaff.DEPARTMENTID = staffModel.DEPARTMENTID;
                    existingStaff.EMAIL = staffModel.EMAIL;
                    existingStaff.EMAILOFNOK = staffModel.EMAILOFNOK;
                    existingStaff.GENDER = staffModel.GENDER;
                    existingStaff.GENDEROFNOK = staffModel.GENDEROFNOK;
                    existingStaff.MISINFOID = staffModel.MISINFOID;
                    existingStaff.NAMEOFNOK = staffModel.NAMEOFNOK;
                    existingStaff.NOKRELATIONSHIP = staffModel.NOKRELATIONSHIP;
                    existingStaff.PHONE = staffModel.PHONE;
                    existingStaff.PHONEOFNOK = staffModel.PHONEOFNOK;
                    existingStaff.STATEID = staffModel.STATEID;
                    existingStaff.CITYID = staffModel.CITYID;
                    existingStaff.STAFFSIGNATURE = staffModel.STAFFSIGNATURE;
                    existingStaff.DELETED = false;
                }
            }
            else //Insert a new staff record into the real staff table
            {
                if (staffModel != null)
                {
                    var staff = new TBL_STAFF()
                    {
                        FIRSTNAME = staffModel.FIRSTNAME,
                        MIDDLENAME = staffModel.MIDDLENAME,
                        COMPANYID = staffModel.COMPANYID,
                        LASTNAME = staffModel.LASTNAME,
                        STAFFCODE = staffModel.STAFFCODE,
                        JOBTITLEID = staffModel.JOBTITLEID,
                        RANKID = staffModel.RANKID,
                        ADDRESS = staffModel.ADDRESS,
                        ADDRESSOFNOK = staffModel.ADDRESSOFNOK,
                        BRANCHID = staffModel.BRANCHID,
                        COMMENT = staffModel.COMMENT,
                        CREATEDBY = staffModel.CREATEDBY,
                        CUSTOMERSENSITIVITYLEVEL = staffModel.CUSTOMERSENSITIVITYLEVEL,
                        DATEOFBIRTH = staffModel.DATEOFBIRTH,
                        DATETIMECREATED = DateTime.Now,
                        DEPARTMENTID = staffModel.DEPARTMENTID,
                        EMAIL = staffModel.EMAIL,
                        EMAILOFNOK = staffModel.EMAILOFNOK,
                        GENDER = staffModel.GENDER,
                        GENDEROFNOK = staffModel.GENDEROFNOK,
                        MISINFOID = staffModel.MISINFOID,
                        NAMEOFNOK = staffModel.NAMEOFNOK,
                        NOKRELATIONSHIP = staffModel.NOKRELATIONSHIP,
                        PHONE = staffModel.PHONE,
                        PHONEOFNOK = staffModel.PHONEOFNOK,
                        STATEID = staffModel.STATEID,
                        CITYID = staffModel.CITYID,
                        STAFFSIGNATURE = staffModel.STAFFSIGNATURE,
                    };
                    context.TBL_STAFF.Add(staff);
                }
            }

            staffModel.ISCURRENT = false;
            staffModel.APPROVALSTATUSID = approvalStatusId;
            staffModel.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff '{staffModel?.FIRSTNAME + " " + staffModel?.LASTNAME}' with staff code'{staffModel?.STAFFCODE}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            try
            {
                context.TBL_AUDIT.Add(audit);
                // Audit Section ---------------------------
                var output = context.SaveChanges() > 0;

                if (output)
                {
                    return output;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<bool> AddTempStaff(StaffInfoViewModel staffModel)
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

            var staff = new TBL_TEMP_STAFF()
            {
                FIRSTNAME = staffModel.FirstName,
                MIDDLENAME = staffModel.MiddleName,
                COMPANYID = staffModel.companyId,
                LASTNAME = staffModel.LastName,
                STAFFCODE = StaticHelpers.GetUniqueKey(6),
                JOBTITLEID = staffModel.JobTitleId,
                RANKID = staffModel.RankId,
                ADDRESS = staffModel.Address,
                ADDRESSOFNOK = staffModel.AddressOfNok,
                BRANCHID = staffModel.BranchId,
                COMMENT = staffModel.Comment,
                CREATEDBY = staffModel.createdBy,
                CUSTOMERSENSITIVITYLEVEL = staffModel.CustomerSensitivityLevel,
                DATEOFBIRTH = staffModel.DateOfBirth,
                DATETIMECREATED = DateTime.Now,
                DEPARTMENTID = staffModel.DepartmentId,
                DEPARTMENTUNITID = (short)staffModel.DepartmentUnitId,
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
                ISCURRENT = true
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

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    auditTrail.AddAuditTrail(audit);
                    context.TBL_TEMP_STAFF.Add(staff);
                    output = await context.SaveChangesAsync() > 0;

                    var entity = new ApprovalViewModel
                    {
                        staffId = staffModel.createdBy,
                        companyId = staffModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = staff.STAFFID,
                        operationId = (int)OperationsEnum.StaffCreation,
                        BranchId = staffModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();
                    }

                    return output;
                }
                catch (Exception)
                {
                    trans.Rollback();
                }
            }

            return output;
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
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.StaffCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from c in context.TBL_TEMP_STAFF
                    join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                    join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                    join dept in context.TBL_DEPARTMENT on c.DEPARTMENTID equals dept.DEPARTMENTID
                    join atrail in context.TBL_APPROVAL_TRAIL on c.STAFFID equals atrail.TARGETID
                    where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && c.ISCURRENT == true
                        && atrail.RESPONSESTAFFID == null
                          && atrail.OPERATIONID == (int)OperationsEnum.StaffCreation && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                    select new StaffInfoViewModel()
                    {
                        StaffId = c.STAFFID,
                        Address = c.ADDRESS,
                        companyId = coy.COMPANYID,
                        AddressOfNok = c.ADDRESSOFNOK,
                        BranchId = br.BRANCHID,
                        Comment = c.COMMENT,
                        CustomerSensitivityLevel = c.CUSTOMERSENSITIVITYLEVEL,
                        DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                        DepartmentId = c.DEPARTMENTID,
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
                        StateName = c.TBL_CITY.TBL_STATE.STATENAME,
                        CityId = (int)c.CITYID,
                        CityName = c.TBL_CITY.CITYNAME,
                        FirstName = c.FIRSTNAME,
                        MiddleName = c.MIDDLENAME,
                        LastName = c.LASTNAME,
                        StaffCode = c.STAFFCODE,
                        RankId = c.RANKID,
                        RankName = c.TBL_STAFF_RANK.RANKNAME,
                        BranchName = br.BRANCHNAME,
                        DepartmentName = dept.DEPARTMENTNAME,
                        DepartmentUnitId = c.DEPARTMENTUNITID,
                        DepartmentUnitName = c.TBL_DEPARTMENT_UNIT.UNIT_NAME,

                        OperationId = atrail.OPERATIONID,
                        SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVEL).DESCRIPTION
                    }).GroupBy(x => x.StaffId).Select(g => g.FirstOrDefault());
        }

        public StaffDetailsModel GetTempStaffDetail(int staffId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.TBL_TEMP_STAFF
                    join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                    join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                    join dept in context.TBL_DEPARTMENT on c.DEPARTMENTID equals dept.DEPARTMENTID
                    where c.STAFFID == staffId //c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved && c.IsCurrent == true
                    select new StaffDetailsModel()
                    {
                        StaffId = c.STAFFID,
                        Address = c.ADDRESS,
                        companyId = coy.COMPANYID,
                        AddressOfNok = c.ADDRESSOFNOK,
                        BranchId = br.BRANCHID,
                        BranchName = br.BRANCHNAME,
                        Comment = c.COMMENT,
                        CustomerSensitivityLevel = c.CUSTOMERSENSITIVITYLEVEL,
                        DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                        DepartmentId = c.DEPARTMENTID ?? 0,
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
                        RankId = c.RANKID,
                        Rank = c.TBL_STAFF_RANK.RANKNAME,
                        DepartmentName = dept.DEPARTMENTNAME,
                        DepartmentUnitId = c.DEPARTMENTUNITID,
                        DepartmentUnitName = c.TBL_DEPARTMENT_UNIT.UNIT_NAME,
                        ApprovalStatusId = c.APPROVALSTATUSID,
                        SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVEL).DESCRIPTION,
                    }).FirstOrDefault();
        }

        public StaffDetailsModel GetStaffDetail(string staffCode, int companyId)
        {
            return GetStaffDetails(companyId).FirstOrDefault(x => x.StaffCode == staffCode && x.companyId == companyId);
        }

        //public IEnumerable<StaffDetailsModel> GetTempStaffDetails()
        //{
        //    return (from c in context.TblTempStaff
        //            join br in context.TblBranch on c.BranchId equals br.BranchId
        //            join coy in context.TblCompany on br.CompanyId equals coy.CompanyId
        //            join dept in context.TblDepartment on c.DepartmentId equals dept.DepartmentId
        //            where c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved && c.IsCurrent == true
        //            select new StaffDetailsModel()
        //            {
        //                StaffId = c.StaffId,
        //                Address = c.Address,
        //                companyId = coy.CompanyId,
        //                AddressOfNok = c.AddressOfNok,
        //                BranchId = br.BranchId,
        //                BranchName =br.BranchName,
        //                Comment = c.Comment,
        //                CustomerSensitivityLevel = c.CustomerSensitivityLevel,
        //                DateOfBirth = c.DateOfBirth ?? DateTime.Now,
        //                DepartmentId = c.DepartmentId ?? 0,
        //                CityId =c.CityId ?? 0,
        //                City = c.City.CityName,
        //                company = coy.Name,
        //                JobTitle = c.JobTitle.JobTitleName,
        //                MisInfo = c.Misinfo.Misname,
        //                Staffsignature =c.Staffsignature,
        //                Email = c.Email,
        //                EmailOfNok = c.EmailOfNok,
        //                Gender = c.Gender,
        //                GenderOfNok = c.GenderOfNok,
        //                JobTitleId = c.JobTitleId,
        //                MisinfoId = c.MisinfoId ?? 0,
        //                NameOfNok = c.NameOfNok,
        //                NokrelationShip = c.NokrelationShip,
        //                Phone = c.Phone,
        //                PhoneOfNok = c.PhoneOfNok,
        //                StateId = c.StateId ?? 0,
        //                State =c.State.StateName,
        //                FirstName = c.FirstName,
        //                MiddleName = c.MiddleName,
        //                LastName = c.LastName,
        //                StaffCode = c.StaffCode,
        //                RankId = c.RankId,
        //                Rank =c.Rank.RankName,
        //                DepartmentName = dept.DepartmentName,
        //                SensitivityLevel = context.TblCustomerSensitivityLevel.SingleOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description,

        //            });
        //}

        public IEnumerable<StaffDetailsModel> GetStaffDetails(int companyId)
        {
            var data = (from c in context.TBL_STAFF
                        join br in context.TBL_BRANCH on c.BRANCHID equals br.BRANCHID
                        join coy in context.TBL_COMPANY on br.COMPANYID equals coy.COMPANYID
                        join dept in context.TBL_DEPARTMENT on c.DEPARTMENTID equals dept.DEPARTMENTID
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
                            CustomerSensitivityLevel = c.CUSTOMERSENSITIVITYLEVEL,
                            DateOfBirth = c.DATEOFBIRTH ?? DateTime.Now,
                            DepartmentId = c.DEPARTMENTID ?? 0,
                            CityId = c.CITYID ?? 0,
                            City = c.TBL_CITY.CITYNAME,
                            company = coy.NAME,
                            JobTitle = c.TBL_STAFF_JOBTITLE.JOBTITLENAME,
                            MisInfo = context.TBL_MIS_INFO.Find(c.MISINFOID).MISNAME,
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
                            State = context.TBL_STATE.Find(c.STATEID).STATENAME,
                            FirstName = c.FIRSTNAME,
                            MiddleName = c.MIDDLENAME,
                            LastName = c.LASTNAME,
                            StaffCode = c.STAFFCODE,
                            RankId = c.RANKID,
                            Rank = c.TBL_STAFF_RANK.RANKNAME,
                            DepartmentName = dept.DEPARTMENTNAME,
                            DepartmentUnitId = (short)c.DEPARTMENT_UNITID,
                            DepartmentUnitName = c.TBL_DEPARTMENT_UNIT.UNIT_NAME,
                            SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.SingleOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVEL).DESCRIPTION,
                        });

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffNames()
        {
            var data =  from st in context.TBL_STAFF
                   select new simpleStaffModel
                   {
                       staffId = st.STAFFID,
                       staffCode = st.STAFFCODE,
                       firstName = st.FIRSTNAME,
                       middleName = st.MIDDLENAME,
                       lastName = st.LASTNAME,
                       departmentId = (short)st.DEPARTMENTID,
                       departmentUnitId = (short)st.DEPARTMENT_UNITID
                   };

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffByUnitId(short departmentUnitId)
        {
            return this.GetStaffNames().Where(x => x.departmentUnitId == departmentUnitId);
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
                    && x.DEPARTMENTID == departmentId)
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

        #region Staff Signature 

        public bool AddStaffSignature(StaffDocumentViewModel model, byte[] file)
        {
            try
            {
                var document = new TBL_MEDIA_STAFF_SIGNATURE()
                {
                    DOCUMENT_TITLE = model.documentTitle,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    FILEDATA = file,
                    SYSTEMDATETIME = DateTime.Now,
                    COMPANYID = model.companyId,
                    STAFFCODE = model.StaffCode,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate()
                };

                documentsContext.TBL_MEDIA_STAFF_SIGNATURE.Add(document);

                var staffInfo = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == model.StaffCode.ToLower());

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.StaffSignatureUploaded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Signature for staff {staffInfo?.FIRSTNAME + ' ' + staffInfo?.LASTNAME} with code '{ model.StaffCode }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateStaffSignature(StaffDocumentViewModel model, int documentId)
        {
            var data = documentsContext.TBL_MEDIA_STAFF_SIGNATURE.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.STAFFCODE = model.StaffCode;
            data.DOCUMENT_TITLE = model.documentTitle;
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;
            data.DATETIMEUPDATED = genSetup.GetApplicationDate();

            var staffInfo = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == model.StaffCode.ToLower());

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffSignatureUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated Signature for staff {staffInfo?.FIRSTNAME + ' ' + staffInfo?.LASTNAME} with code '{ model.StaffCode }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<StaffDocumentViewModel> GetAllStaffSignatures()
        {
            var documents = (from doc in documentsContext.TBL_MEDIA_STAFF_SIGNATURE
                             join s in context.TBL_STAFF on doc.STAFFCODE.ToLower() equals s.STAFFCODE.ToLower()
                             select new StaffDocumentViewModel
                             {
                                 documentId = doc.DOCUMENTID,
                                 companyId = s.COMPANYID,
                                 companyName = s.TBL_COMPANY.NAME,
                                 branchId = s.BRANCHID,
                                 branchName = context.TBL_BRANCH.FirstOrDefault(x => x.BRANCHID == s.BRANCHID).BRANCHNAME,
                                 departmentId = context.TBL_DEPARTMENT.FirstOrDefault(x => x.DEPARTMENTID == s.DEPARTMENTID).DEPARTMENTNAME,
                                 departmentName = s.DEPARTMENTID,
                                 rankId = s.RANKID,
                                 rankName = s.TBL_STAFF_RANK.RANKNAME,
                                 StaffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                                 StaffCode = doc.STAFFCODE,
                                 documentTitle = doc.DOCUMENT_TITLE,
                                 fileData = doc.FILEDATA,
                                 fileName = doc.FILENAME,
                                 fileExtension = doc.FILEEXTENSION,
                                 SystemDateTime = doc.SYSTEMDATETIME,
                                 dateTimeCreated = doc.DATETIMECREATED
                             }).ToList();

            return documents;
        }

        public IEnumerable<StaffDocumentViewModel> GetStaffSignatureByStaffCode(string staffCode)
        {
            var data = this.GetAllStaffSignatures().Where(x =>
                string.Equals(x.StaffCode.ToLower(), staffCode.ToLower(), StringComparison.Ordinal)).ToList();
            return data;
        }

        #endregion Staff Signature

    }
}