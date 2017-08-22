using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IStaffRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StaffRepository : IStaffRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;
        private IWorkFlowRepository workFlow;
        private IApprovalLevelStaffRepository level;

        public StaffRepository(FinTrakBankingContext _context,
                               IAuditTrailRepository _auditTrail,
                               IGeneralSetupRepository _genSetup,
                               IWorkFlowRepository _workFlow,
                               IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            this.workFlow = _workFlow;
            level = _level;
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
            var staff = (from c in context.tbl_Staff
                         join br in context.tbl_Branch
on c.BranchId equals br.BranchId
                         join coy in context.tbl_Company
on br.CompanyId equals coy.CompanyId
                         join dept in context.tbl_Department
on c.DepartmentId equals dept.DepartmentId
                         select new StaffInfoViewModel()
                         {
                             StaffId = c.StaffId,
                             Address = c.Address,
                             companyId = coy.CompanyId,
                             AddressOfNok = c.AddressOfNOK,
                             BranchId = br.BranchId,
                             Comment = c.Comment,
                             //createdBy = c.CreatedBy.Value,
                             CustomerSensitivityLevel = c.CustomerSensitivityLevel,
                             DateOfBirth = c.DateOfBirth ?? DateTime.Now,
                             //dateTimeCreated = c.DateTimeCreated,
                             DepartmentId = c.DepartmentId,
                             Email = c.Email,
                             EmailOfNok = c.EmailOfNOK,
                             Gender = c.Gender,
                             GenderOfNok = c.GenderOfNOK,
                             JobTitleId = c.JobTitleId,
                             MisinfoId = c.MISInfoId,
                             NameOfNok = c.NameOfNOK,
                             NokrelationShip = c.NOKRelationShip,
                             Phone = c.Phone,
                             PhoneOfNok = c.PhoneOfNOK,
                             StateId = c.StateId,
                             //Staffsignature = c.Staffsignature,
                             FirstName = c.FirstName,
                             MiddleName = c.MiddleName,
                             LastName = c.LastName,
                             StaffCode = c.StaffCode,
                             RankId = c.RankId,
                             BranchName = br.BranchName,
                             DepartmentName = dept.DepartmentName,
                             // MisInfoCode = c.Misinfo.Misname,
                             SensitivityLevel = context.tbl_Customer_Sensitivity_Level.FirstOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description,
                             // State = c.State.StateName
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
            var staff = (from c in context.tbl_Staff
                         where c.StaffId == staffId
                         select new StaffInfoViewModel()
                         {
                             Address = c.Address,
                             AddressOfNok = c.AddressOfNOK,
                             BranchId = c.BranchId,
                             Comment = c.Comment,
                             createdBy = c.CreatedBy.Value,
                             CustomerSensitivityLevel = c.CustomerSensitivityLevel,
                             DateOfBirth = c.DateOfBirth,
                             dateTimeCreated = (DateTime)c.DateTimeCreated,
                             DepartmentId = c.DepartmentId,
                             Email = c.Email,
                             EmailOfNok = c.EmailOfNOK,
                             Gender = c.Gender,
                             GenderOfNok = c.GenderOfNOK,
                             JobTitleId = c.JobTitleId,
                             MisinfoId = c.MISInfoId,
                             NameOfNok = c.NameOfNOK,
                             NokrelationShip = c.NOKRelationShip,
                             Phone = c.Phone,
                             PhoneOfNok = c.PhoneOfNOK,
                             StateId = c.StateId,
                             Staffsignature = c.Staffsignature,
                             FirstName = c.FirstName,
                             MiddleName = c.MiddleName,
                             LastName = c.LastName,
                             StaffCode = c.StaffCode,
                             RankId = c.RankId,
                             //BranchName = br.BranchName,

                             //DepartmentName = c.Department.DepartmentName,
                             //MisInfoCode = c.Misinfo.Misname,
                             SensitivityLevel = context.tbl_Customer_Sensitivity_Level.SingleOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description,
                             //State = c.State.StateName
                         }).SingleOrDefault();
            return staff;
        }

        /// <summary>
        /// Update staff information.
        /// </summary>
        /// <param name="StafffViewModel"></param>
        /// <returns></returns>
        public bool UpdateStaff(int staffid, StaffInfoViewModel staffModel)
        {
            var existStingTempStaff = context.tbl_Temp_Staff.Where(x => x.StaffCode.ToLower() == staffModel.StaffCode.ToLower() && x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);

            if (existStingTempStaff.Any())
            {
                foreach (var item in existStingTempStaff)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetStaff = context.tbl_Staff.Find(staffid);

            var unApprovedStaffEdit = context.tbl_Temp_Staff.Where(x => x.IsCurrent == true && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Staff tempStaff;

            if (unApprovedStaffEdit.Any())
            {
                throw new Exception("Staff is already undergoing approval");
            }
            else
            {
                tempStaff = new tbl_Temp_Staff()
                {
                    FirstName = staffModel.FirstName,
                    MiddleName = staffModel.MiddleName,
                    LastName = staffModel.LastName,
                    StaffCode = targetStaff.StaffCode,
                    JobTitleId = staffModel.JobTitleId,
                    CompanyId = staffModel.companyId,
                    RankId = staffModel.RankId,
                    Address = staffModel.Address,
                    AddressOfNOK = staffModel.AddressOfNok,
                    BranchId = staffModel.BranchId,
                    Comment = staffModel.Comment,
                    CreatedBy = staffModel.createdBy,
                    CustomerSensitivityLevel = staffModel.CustomerSensitivityLevel,
                    DateOfBirth = staffModel.DateOfBirth,
                    DateTimeCreated = DateTime.Now,
                    DepartmentId = staffModel.DepartmentId,
                    Email = staffModel.Email,
                    EmailOfNOK = staffModel.EmailOfNok,
                    Gender = staffModel.Gender,
                    GenderOfNOK = staffModel.GenderOfNok,
                    MISInfoId = staffModel.MisinfoId,
                    NameOfNOK = staffModel.NameOfNok,
                    NOKRelationShip = staffModel.NokrelationShip,
                    Phone = staffModel.Phone,
                    PhoneOfNOK = staffModel.PhoneOfNok,
                    StateId = staffModel.StateId,
                    CityId = staffModel.CityId,
                    Staffsignature = staffModel.Staffsignature,
                    ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                    IsCurrent = true
                };

                context.tbl_Temp_Staff.Add(tempStaff);

            }


            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.StaffUpdated,
                StaffId = staffModel.createdBy,
                BranchId = (short)staffModel.BranchId,
                Detail = $"Updated Staff '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
                IPAddress = staffModel.userIPAddress,
                Url = staffModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = staffid

            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------  

            var output = this.SaveAll();

            var entity = new ApprovalViewModel
            {
                staffId = staffModel.createdBy,
                companyId = staffModel.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempStaff.StaffId,
                operationId = (int)OperationsEnum.StaffCreation,
                BranchId = staffModel.userBranchId
            };
            var response = workFlow.LogForApproval(entity);

            return output;
        }

        /// <summary>
        /// Delete selected staff.
        /// </summary>
        /// <param name="staffId"></param>
        /// <returns></returns>
        public bool DeleteStaff(int staffId, UserInfo user)
        {
            var targetStaff = context.tbl_Staff.Find(staffId);

            targetStaff.Deleted = true;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.StaffDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Staff '{targetStaff.FirstName}' with code'{targetStaff.StaffCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
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
            var staff = (from c in context.tbl_Staff
                         select new StaffViewModel()
                         {
                             StaffName = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                             StaffId = c.StaffId
                         });
            return staff;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.StaffCreation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApproveStaff(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveStaff(int staffid, short approvalStatusId, UserInfo user)
        {

            var staffModel = context.tbl_Temp_Staff.Find(staffid);
            var staffToUpdate = context.tbl_Staff.Where(x => x.StaffCode == staffModel.StaffCode);


            if (staffToUpdate.Any()) //Update existing staff with tempStaff record
            {
                var existingStaff = staffToUpdate.First();
                existingStaff.FirstName = staffModel.FirstName;
                existingStaff.CompanyId = staffModel.CompanyId;
                existingStaff.MiddleName = staffModel.MiddleName;
                existingStaff.LastName = staffModel.LastName;
                existingStaff.StaffCode = staffModel.StaffCode;
                existingStaff.JobTitleId = staffModel.JobTitleId;
                existingStaff.RankId = staffModel.RankId;
                existingStaff.Address = staffModel.Address;
                existingStaff.AddressOfNOK = staffModel.AddressOfNOK;
                existingStaff.BranchId = staffModel.BranchId;
                existingStaff.Comment = staffModel.Comment;
                existingStaff.CreatedBy = staffModel.CreatedBy;
                existingStaff.CustomerSensitivityLevel = staffModel.CustomerSensitivityLevel;
                existingStaff.DateOfBirth = staffModel.DateOfBirth;
                existingStaff.DateTimeCreated = DateTime.Now;
                existingStaff.DepartmentId = staffModel.DepartmentId;
                existingStaff.Email = staffModel.Email;
                existingStaff.EmailOfNOK = staffModel.EmailOfNOK;
                existingStaff.Gender = staffModel.Gender;
                existingStaff.GenderOfNOK = staffModel.GenderOfNOK;
                existingStaff.MISInfoId = staffModel.MISInfoId;
                existingStaff.NameOfNOK = staffModel.NameOfNOK;
                existingStaff.NOKRelationShip = staffModel.NOKRelationShip;
                existingStaff.Phone = staffModel.Phone;
                existingStaff.PhoneOfNOK = staffModel.PhoneOfNOK;
                existingStaff.StateId = staffModel.StateId;
                existingStaff.CityId = staffModel.CityId;
                existingStaff.Staffsignature = staffModel.Staffsignature;
            }
            else //Insert a new staff record into the real staff table
            {
                var staff = new tbl_Staff()
                {
                    FirstName = staffModel.FirstName,
                    MiddleName = staffModel.MiddleName,
                    CompanyId = staffModel.CompanyId,
                    LastName = staffModel.LastName,
                    StaffCode = staffModel.StaffCode,
                    JobTitleId = staffModel.JobTitleId,
                    RankId = staffModel.RankId,
                    Address = staffModel.Address,
                    AddressOfNOK = staffModel.AddressOfNOK,
                    BranchId = staffModel.BranchId,
                    Comment = staffModel.Comment,
                    CreatedBy = staffModel.CreatedBy,
                    CustomerSensitivityLevel = staffModel.CustomerSensitivityLevel,
                    DateOfBirth = staffModel.DateOfBirth,
                    DateTimeCreated = DateTime.Now,
                    DepartmentId = staffModel.DepartmentId,
                    Email = staffModel.Email,
                    EmailOfNOK = staffModel.EmailOfNOK,
                    Gender = staffModel.Gender,
                    GenderOfNOK = staffModel.GenderOfNOK,
                    MISInfoId = staffModel.MISInfoId,
                    NameOfNOK = staffModel.NameOfNOK,
                    NOKRelationShip = staffModel.NOKRelationShip,
                    Phone = staffModel.Phone,
                    PhoneOfNOK = staffModel.PhoneOfNOK,
                    StateId = staffModel.StateId,
                    CityId = staffModel.CityId,
                    Staffsignature = staffModel.Staffsignature,
                };
                context.tbl_Staff.Add(staff);
            }

            staffModel.IsCurrent = false;
            staffModel.ApprovalStatusId = approvalStatusId;
            staffModel.DateTimeUpdated = DateTime.Now;


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.StaffApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Staff '{staffModel.FirstName + " " + staffModel.LastName}' with staff code'{staffModel.StaffCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public bool AddTempStaff(StaffInfoViewModel staffModel)
        {
            bool output = false;
            var existStingTempStaff = context.tbl_Temp_Staff.Where(x => x.StaffCode.ToLower() == staffModel.StaffCode.ToLower()
                                                                  && x.IsCurrent == true 
                                                                  && x.CompanyId == staffModel.companyId
                                                                  && x.ApprovalStatusId == (short)ApprovalStatusEnum.Pending);

            if (existStingTempStaff.Any())
            {
                throw new Exception("Staff Information already exist and is undergoing approval");
            }

            var staff = new tbl_Temp_Staff()
            {
                FirstName = staffModel.FirstName,
                MiddleName = staffModel.MiddleName,
                CompanyId = staffModel.companyId,
                LastName = staffModel.LastName,
                StaffCode = staffModel.StaffCode,
                JobTitleId = staffModel.JobTitleId,
                RankId = staffModel.RankId,
                Address = staffModel.Address,
                AddressOfNOK = staffModel.AddressOfNok,
                BranchId = staffModel.BranchId,
                Comment = staffModel.Comment,
                CreatedBy = staffModel.createdBy,
                CustomerSensitivityLevel = staffModel.CustomerSensitivityLevel,
                DateOfBirth = staffModel.DateOfBirth,
                DateTimeCreated = DateTime.Now,
                DepartmentId = staffModel.DepartmentId,
                Email = staffModel.Email,
                EmailOfNOK = staffModel.EmailOfNok,
                Gender = staffModel.Gender,
                GenderOfNOK = staffModel.GenderOfNok,
                MISInfoId = staffModel.MisinfoId,
                NameOfNOK = staffModel.NameOfNok,
                NOKRelationShip = staffModel.NokrelationShip,
                Phone = staffModel.Phone,
                PhoneOfNOK = staffModel.PhoneOfNok,
                StateId = staffModel.StateId,
                CityId = staffModel.CityId,
                Staffsignature = staffModel.Staffsignature,
                ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                IsCurrent = true

            };
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CreateStaffInitiated,
                StaffId = staffModel.createdBy,
                BranchId = (short)staffModel.BranchId,
                Detail = $"Initiated Staff Creation for '{staffModel.StaffFullName}' with code'{staffModel.StaffCode}'",
                IPAddress = staffModel.userIPAddress,
                Url = staffModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            if (workFlow.CheckRouteForOperation((int)OperationsEnum.StaffCreation, staffModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        this.context.tbl_Temp_Staff.Add(staff);
                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = staffModel.createdBy,
                            companyId = staffModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = staff.StaffId,
                            operationId = (int)OperationsEnum.StaffCreation,
                            BranchId = staffModel.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }
            return output;

        }

        public bool IsStaffCodeAlreadyExist(string staffCode)
        {
            return context.tbl_Staff.Any(x => x.StaffCode.ToLower() == staffCode.ToLower());
        }

        public bool IsStaffExist(string staffCode)
        {
            return context.tbl_Temp_Staff.Any(x => x.StaffCode.ToLower() == staffCode.ToLower() && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && x.IsCurrent == true);
        }

        public IEnumerable<StaffInfoViewModel> GetStaffAwaitingApprovals(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)OperationsEnum.StaffCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from c in context.tbl_Temp_Staff
                    join br in context.tbl_Branch on c.BranchId equals br.BranchId
                    join coy in context.tbl_Company on br.CompanyId equals coy.CompanyId
                    join dept in context.tbl_Department on c.DepartmentId equals dept.DepartmentId
                    join atrail in context.tbl_Approval_Trail on c.StaffId equals atrail.TargetId
                    where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && c.IsCurrent == true
                          && atrail.OperationId == (int)OperationsEnum.StaffCreation && atrail.ToApprovalLevelId == staffApprovalLevelId
                    select new StaffInfoViewModel()
                    {
                        StaffId = c.StaffId,
                        Address = c.Address,
                        companyId = coy.CompanyId,
                        AddressOfNok = c.AddressOfNOK,
                        BranchId = br.BranchId,
                        Comment = c.Comment,
                        CustomerSensitivityLevel = c.CustomerSensitivityLevel,
                        DateOfBirth = c.DateOfBirth ?? DateTime.Now,
                        DepartmentId = c.DepartmentId,
                        Email = c.Email,
                        EmailOfNok = c.EmailOfNOK,
                        Gender = c.Gender,
                        GenderOfNok = c.GenderOfNOK,
                        JobTitleId = c.JobTitleId,
                        MisinfoId = c.MISInfoId,
                        NameOfNok = c.NameOfNOK,
                        NokrelationShip = c.NOKRelationShip,
                        Phone = c.Phone,
                        PhoneOfNok = c.PhoneOfNOK,
                        StateId = c.StateId,
                        StateName = c.tbl_City.tbl_State.StateName,
                        CityId = (int)c.CityId,
                        CityName = c.tbl_City.CityName,
                        FirstName = c.FirstName,
                        MiddleName = c.MiddleName,
                        LastName = c.LastName,
                        StaffCode = c.StaffCode,
                        RankId = c.RankId,
                        RankName = c.tbl_Staff_Rank.RankName,
                        BranchName = br.BranchName,
                        DepartmentName = dept.DepartmentName,
                        OperationId = atrail.OperationId,
                        SensitivityLevel = context.tbl_Customer_Sensitivity_Level.FirstOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description

                    });
        }

        public StaffDetailsModel GetTempStaffDetail(int staffId)
        {
            //return GetTempStaffDetails().Where(x => x.StaffId == staffId).Single();

            return (from c in context.tbl_Temp_Staff
                    join br in context.tbl_Branch on c.BranchId equals br.BranchId
                    join coy in context.tbl_Company on br.CompanyId equals coy.CompanyId
                    join dept in context.tbl_Department on c.DepartmentId equals dept.DepartmentId
                    where c.StaffId == staffId //c.ApprovalStatusId == (int)ApprovalStatusEnum.Approved && c.IsCurrent == true
                    select new StaffDetailsModel()
                    {
                        StaffId = c.StaffId,
                        Address = c.Address,
                        companyId = coy.CompanyId,
                        AddressOfNok = c.AddressOfNOK,
                        BranchId = br.BranchId,
                        BranchName = br.BranchName,
                        Comment = c.Comment,
                        CustomerSensitivityLevel = c.CustomerSensitivityLevel,
                        DateOfBirth = c.DateOfBirth ?? DateTime.Now,
                        DepartmentId = c.DepartmentId ?? 0,
                        CityId = c.CityId ?? 0,
                        City = c.tbl_City.CityName,
                        company = coy.Name,
                        JobTitle = c.tbl_Staff_JobTitle.JobTitleName,
                        MisInfo = c.tbl_MIS_Info.MISName,
                        Staffsignature = c.Staffsignature,
                        Email = c.Email,
                        EmailOfNok = c.EmailOfNOK,
                        Gender = c.Gender,
                        GenderOfNok = c.GenderOfNOK,
                        JobTitleId = c.JobTitleId,
                        MisinfoId = c.MISInfoId ?? 0,
                        NameOfNok = c.NameOfNOK,
                        NokrelationShip = c.NOKRelationShip,
                        Phone = c.Phone,
                        PhoneOfNok = c.PhoneOfNOK,
                        StateId = c.StateId ?? 0,
                        State = c.tbl_State.StateName,
                        FirstName = c.FirstName,
                        MiddleName = c.MiddleName,
                        LastName = c.LastName,
                        StaffCode = c.StaffCode,
                        RankId = c.RankId,
                        Rank = c.tbl_Staff_Rank.RankName,
                        DepartmentName = dept.DepartmentName,
                        ApprovalStatusId = c.ApprovalStatusId,
                        SensitivityLevel = context.tbl_Customer_Sensitivity_Level.SingleOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description,

                    }).FirstOrDefault();

        }

        public StaffDetailsModel GetStaffDetail(string staffCode, int companyId)
        {
            return GetStaffDetails(companyId).Where(x => x.StaffCode == staffCode && x.companyId == companyId).Single();
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
            var data = (from c in context.tbl_Staff
                        join br in context.tbl_Branch on c.BranchId equals br.BranchId
                        join coy in context.tbl_Company on br.CompanyId equals coy.CompanyId
                        join dept in context.tbl_Department on c.DepartmentId equals dept.DepartmentId
                        where c.CompanyId == companyId
                        select new StaffDetailsModel()
                        {
                            StaffId = c.StaffId,
                            Address = c.Address,
                            companyId = coy.CompanyId,
                            AddressOfNok = c.AddressOfNOK,
                            BranchId = br.BranchId,
                            BranchName = br.BranchName,
                            Comment = c.Comment,
                            CustomerSensitivityLevel = c.CustomerSensitivityLevel,
                            DateOfBirth = c.DateOfBirth ?? DateTime.Now,
                            DepartmentId = c.DepartmentId ?? 0,
                            CityId = c.CityId ?? 0,
                            City = c.tbl_City.CityName,
                            company = coy.Name,
                            JobTitle = c.tbl_Staff_JobTitle.JobTitleName,
                            MisInfo = context.tbl_MIS_Info.Find(c.MISInfoId).MISName,
                            Staffsignature = c.Staffsignature,
                            Email = c.Email,
                            EmailOfNok = c.EmailOfNOK,
                            Gender = c.Gender,
                            GenderOfNok = c.GenderOfNOK,
                            JobTitleId = c.JobTitleId,
                            MisinfoId = c.MISInfoId ?? 0,
                            NameOfNok = c.NameOfNOK,
                            NokrelationShip = c.NOKRelationShip,
                            Phone = c.Phone,
                            PhoneOfNok = c.PhoneOfNOK,
                            StateId = c.StateId ?? 0,
                            State = context.tbl_State.Find(c.StateId).StateName,
                            FirstName = c.FirstName,
                            MiddleName = c.MiddleName,
                            LastName = c.LastName,
                            StaffCode = c.StaffCode,
                            RankId = c.RankId,
                            Rank = c.tbl_Staff_Rank.RankName,
                            DepartmentName = dept.DepartmentName,
                            SensitivityLevel = context.tbl_Customer_Sensitivity_Level.SingleOrDefault(x => x.CustomerSensitivityLevelId == c.CustomerSensitivityLevel).Description,
                        });

            return data;
        }

        public IEnumerable<simpleStaffModel> GetStaffNames()
        {
            return from st in context.tbl_Staff
                   select new simpleStaffModel
                   {
                       staffId = st.StaffId,
                       staffCode = st.StaffCode,
                       firstName = st.FirstName,
                       middleName = st.MiddleName,
                       lastName = st.LastName
                   };
        }

        public IEnumerable<ApprovalStatusViewModel> GetApprovalStatus()
        {
            return from ap in context.tbl_Approval_Status
                   select new ApprovalStatusViewModel
                   {
                       approvalStatusId = ap.ApprovalStatusId,
                       approvalStatusName = ap.ApprovalStatusName,
                       forDisplay = ap.ForDisplay,
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
                    context.tbl_Staff.Where(x => x.Deleted == false)// && x.c == companyId)
                    .Where(x => x.FirstName.ToLower().Contains(searchQuery)
                    || x.MiddleName.ToLower().Contains(searchQuery)
                    || x.LastName.ToLower().Contains(searchQuery)
                    || x.StaffCode.Contains(searchQuery))
                    .Select(o => new simpleStaffModel
                    {
                        staffId = o.StaffId,
                        firstName = o.FirstName,
                        middleName = o.MiddleName,
                        lastName = o.LastName,
                        staffCode = o.StaffCode,
                    })
                ;
            }

            return staff;
        }
    }


}