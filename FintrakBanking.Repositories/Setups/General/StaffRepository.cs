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
using System.Data.Entity;

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

        public string FileUploadControl { get; private set; }

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

        public staffBulkFeedbackViewModel UploadBulkPrepaymentData(StaffDocumentViewModel model, byte[] file)
        {
            var applicationDate = genSetup.GetApplicationDate();

            var staffInfo = new List<StaffInfoViewModel>();

            var failedStaffInfo = new List<StaffInfoViewModel>();

            var staffBulkFeedbackViewModel = new staffBulkFeedbackViewModel();
            var setupGlobal = context.TBL_SETUP_GLOBAL.FirstOrDefault();

            // Loads a spreadsheet from a file with the specified path
            //Limited unlicenced key : SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY"); 
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5");

            MemoryStream ms = new MemoryStream(file);

            ExcelFile ef = new ExcelFile();

            try
            {
                ef = ExcelFile.Load(ms, LoadOptions.XlsxDefault);
            }
            catch (Exception ex)
            {

                throw ex;
            }


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
                            staffRowData.loanReferenceNumber = cell.Value.ToString();
                            var exist = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber).FirstOrDefault();

                            if (exist == null)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Loan Reference Number Does Not Exist. ";
                            }

                            staffRowData.customerId = exist?.CUSTOMERID;

                            var existence = (from a in context.TBL_LOAN
                                             where a.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                                             select (a)).ToList();

                            if (existence.Count == 0)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Transaction Reference Number Does Not Exist Today. ";
                            }

                            var pastDueExistence = (from a in context.TBL_LOAN
                                                    where a.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber && a.LOANSTATUSID == (int)LoanStatusEnum.Active && (a.PASTDUEPRINCIPAL > 0 || a.PASTDUEINTEREST > 0)
                                                    select (a)).ToList();

                            if (pastDueExistence.Count > 0)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Transaction Reference Number Does Has Past Due Abligation. ";
                            }
                            
                            var irregularScheduleTypeExistence = (from a in context.TBL_LOAN
                                                                  where a.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.SCHEDULETYPEID == (int)LoanScheduleTypeEnum.IrregularSchedule
                                                                  select (a)).ToList();

                            if (pastDueExistence.Count > 0)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Irregular Schedule Cannot Form Part Of Bulk Irregular Schedule. ";
                            }
                            break;

                        case "B":
                            staffRowData.amount = Convert.ToDecimal(cell.Value.ToString());

                            var existAmount = (from a in context.TBL_LOAN
                                               join b in context.TBL_LOAN_REVIEW_OPERATION on a.TERMLOANID equals b.LOANID
                                               where a.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber && b.OPERATIONTYPEID == (int)OperationsEnum.Prepayment && b.OPERATIONDATE == DbFunctions.TruncateTime(applicationDate) && b.PREPAYMENT == staffRowData.amount && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                                               select (b)).ToList();

                            if (existAmount.Count > 0)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Prepayment Amount Does Exist Against This Transaction Reference Number Today. ";
                            }
                            
                            var reversal = context.TBL_BULK_PREPAYMENT.Where(x => x.LOANREFERENCENUMBER == staffRowData.loanReferenceNumber && x.AMOUNT == staffRowData.amount && x.PROCESSDATE == DbFunctions.TruncateTime(applicationDate)).ToList();

                            if (reversal.Count != 0)
                            {
                                rowSuccess = false;
                                staffRowData.message = staffRowData.message + "Prepayment Amount Transaction Reference Number Already Exist. ";
                            }

                            break;
                    }
                }

                if (rowSuccess && excelRowPosition > 1)
                {
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
                //staffBulkFeedbackViewModel.commitedRows = staffInfo;
                //staffBulkFeedbackViewModel.discardedRows = failedStaffInfo;

                var batchCode = CommonHelpers.GenerateRandomDigitCodeNew(10);

                long batchCodeInt = Convert.ToInt64(batchCode);

                batchCodeInt = Math.Abs(batchCodeInt);

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

                    //var response = AddBulkPrepaymentData(staffInfoRow, batchCode, applicationDate);

                    var response = AddBulkPrepaymentData(staffInfoRow, (int)batchCodeInt, applicationDate);


                    if (!response)
                    {
                        staffBulkFeedbackViewModel.discardedRows.Add(staffInfoRow);
                        staffBulkFeedbackViewModel.commitedRows.Remove(staffInfoRow);
                        staffBulkFeedbackViewModel.failureCount = staffBulkFeedbackViewModel.failureCount + 1;
                        staffBulkFeedbackViewModel.successCount = staffBulkFeedbackViewModel.successCount - 1;
                    }

                };


                //var rec = context.TBL_BULK_PREPAYMENT.Where(x => x.BATCHID == batchCode).FirstOrDefault();

                var rec = context.TBL_BULK_PREPAYMENT.Where(x => x.BATCHID == (int)batchCodeInt).FirstOrDefault();


                if (rec != null)
                {

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CreateBulkPrepayment,
                        STAFFID = model.createdBy,
                        BRANCHID = model.userBranchId,
                        DETAIL = $"Initiated Bulk Prepayment with code '{batchCodeInt}'",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };

                    auditTrail.AddAuditTrail(audit);

                    var output = context.SaveChanges() > 0;

                    //workflow.StaffId = model.createdBy;
                    //workflow.CompanyId = model.companyId;
                    //workflow.StatusId = (int)ApprovalStatusEnum.Pending;
                    ////workflow.TargetId = batchCode;
                    //workflow.TargetId = (int)batchCodeInt;
                    //workflow.Comment = "Bulk Prepayment Initiated";
                    //workflow.OperationId = (int)OperationsEnum.BulkLiquidation;
                    //workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    //workflow.ExternalInitialization = true;
                    //workflow.LogActivity();

                }

                context.SaveChanges();

            }

            return staffBulkFeedbackViewModel;
        }


        public bool AddBulkPrepaymentData(StaffInfoViewModel staffModel, int batchCode, DateTime applicationDate)
        {
            if (batchCode < 0) { batchCode = batchCode * -1; }

            var bulkPrepaymentInfo = new TBL_BULK_PREPAYMENT()
            {
                BATCHID = batchCode,
                LOANREFERENCENUMBER = staffModel.loanReferenceNumber,
                AMOUNT = staffModel.amount,
                CREATEDBY = staffModel.createdBy,
                LASTUPDATEDBY = staffModel.createdBy,
                PROCESSDATE = applicationDate,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                CUSTOMERID = staffModel.customerId
            };

            context.TBL_BULK_PREPAYMENT.Add(bulkPrepaymentInfo);

            var output = context.SaveChanges() > 0;

            return output;
        }

        public bool UpdatePrepayment(int staffid, StaffInfoViewModel staffModel)
        {
            bool isUpdate = false;

            var data = context.TBL_BULK_PREPAYMENT.Where(x => x.BULK_PREPAYMENTID == staffModel.prepaymentId).FirstOrDefault();

            if (data != null)
            {
                data.AMOUNT = staffModel.amount;
            }

            var output = context.SaveChanges();

            if (output > 0)
            {
                isUpdate = true;
            }

            return isUpdate;
        }


        public bool DeleteBulkPrepayment(int bulkPrepaymentId, UserInfo user)
        {

            bool result = false;

            var targetRecord = context.TBL_BULK_PREPAYMENT.Where(x => x.BULK_PREPAYMENTID == bulkPrepaymentId).FirstOrDefault();

            context.TBL_BULK_PREPAYMENT.Remove(targetRecord);

            result = context.SaveChanges() > 0;

            return result;

        }


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
                             Comment = c.COMMENT_,
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
                             businessUnitId =c.BUSINESSUNITID,
                             misCode = c.MISCODE,
                             businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
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
                             Comment = c.COMMENT_,
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
                             businessUnitId = c.BUSINESSUNITID,
                             misCode = c.MISCODE,
                             businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                         }).SingleOrDefault();
            return staff;
        }

        public bool UpdateStaff(int staffid, StaffInfoViewModel staffModel)
        {

            bool isUpdate = false;
            TBL_TEMP_PROFILE_USER user = null;
            var tempStaffForPassword = context.TBL_PROFILE_USER.FirstOrDefault(u => u.USERNAME == staffModel.user.username);
            


            List<TBL_TEMP_PROFILE_USERGROUP> userGroups = new List<TBL_TEMP_PROFILE_USERGROUP>();
            List<TBL_TEMP_PROFILE_ADTN_ACTIVITY> userActivities = new List<TBL_TEMP_PROFILE_ADTN_ACTIVITY>();

            foreach (var item in staffModel.user.activities)
                {
                    userActivities.Add(new TBL_TEMP_PROFILE_ADTN_ACTIVITY()
                    {
                        ACTIVITYID = item.activityId,
                        CANADD = false,
                        CANEDIT = false,
                        CANAPPROVE = false,
                        CANDELETE = false,
                        CANVIEW = false,
                        CREATEDBY = staffModel.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        EXPIREON = item.expireOn
                    });
                }

                foreach (var item in staffModel.user.group)
                {
                    userGroups.Add(new TBL_TEMP_PROFILE_USERGROUP()
                    {
                        GROUPID = item.groupId,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffModel.createdBy,
                        ISCURRENT = true,
                        APPROVALSTATUS = false,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
                    });
                }


            var existingTempStaff = context.TBL_TEMP_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower() && x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            if (existingTempStaff != null)
            {
                isUpdate = true;

                var unApprovedStaffEdit = context.TBL_TEMP_STAFF.Where(x => x.ISCURRENT == true && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower());
                if (unApprovedStaffEdit.Any()) throw new SecureException("Staff is already undergoing approval");

                var existingTempUser = context.TBL_TEMP_PROFILE_USER.FirstOrDefault(u => u.TEMPSTAFFID == existingTempStaff.TEMPSTAFFID);
                if (existingTempUser != null)
                {

                    existingTempUser.USERNAME = staffModel.user.username.ToLower().Trim();
                    existingTempUser.ISFIRSTLOGINATTEMPT = false;
                    existingTempUser.ISACTIVE = false;
                    existingTempUser.ISLOCKED = true;
                    existingTempUser.FAILEDLOGONATTEMPT = 0;

                    if (staffModel.user.changeSecutirtyQuestion)
                    {
                        existingTempUser.SECURITYQUESTION = staffModel.user.securityQuestion;
                        existingTempUser.SECURITYANSWER = staffModel.user.securityAnswer;
                    }
                    else
                    {
                        existingTempUser.SECURITYQUESTION = context.TBL_PROFILE_USER.Where(a => a.USERNAME.ToLower() == existingTempUser.USERNAME.ToLower()).FirstOrDefault().SECURITYQUESTION;
                        existingTempUser.SECURITYANSWER = context.TBL_PROFILE_USER.Where(a => a.USERNAME.ToLower() == existingTempUser.USERNAME.ToLower()).FirstOrDefault().SECURITYANSWER;
                    }
                    if (staffModel.user.changePassword)
                    {
                        existingTempUser.PASSWORD  = StaticHelpers.EncryptSha512(staffModel.user.password , StaticHelpers.EncryptionKey);// staffModel.user.password;
                        existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER);
                    }
                    else
                    {
                        existingTempUser.PASSWORD = context.TBL_PROFILE_USER.Where(a => a.USERNAME.ToLower() == existingTempUser.USERNAME.ToLower()).FirstOrDefault().PASSWORD;
                        existingTempUser.NEXTPASSWORDCHANGEDATE = context.TBL_PROFILE_USER.Where(a => a.USERNAME.ToLower() == existingTempUser.USERNAME.ToLower()).FirstOrDefault().NEXTPASSWORDCHANGEDATE;
                    }
                    //existingTempUser.PASSWORD = context.TBL_PROFILE_USER.Where(a=>a.USERNAME.ToLower() == existingTempStaff.STAFFCODE.ToLower()).FirstOrDefault().PASSWORD;
                    //existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(profile_Setting.EXPIREPASSWORDAFTER);
                    //existingTempUser.NEXTPASSWORDCHANGEDATE = DateTime.Now.AddDays(CommonHelpers.PasswordExpirationDays);
                    existingTempUser.LASTUPDATEDBY = staffModel.createdBy;
                    existingTempUser.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                    existingTempUser.APPROVALSTATUS = false;
                    existingTempUser.ISCURRENT = true;
                    existingTempUser.TBL_TEMP_PROFILE_ADTN_ACTIVITY = userActivities;
                    existingTempUser.TBL_TEMP_PROFILE_USERGROUP = userGroups;
                    context.Entry(existingTempUser).State = EntityState.Modified;
                }
            }
            else
            {
                user = new TBL_TEMP_PROFILE_USER()
                    {
                        TEMPSTAFFID = staffModel.staffId,
                        USERNAME = staffModel.user.username.ToLower().Trim(),
                        PASSWORD = tempStaffForPassword != null ? tempStaffForPassword.PASSWORD : StaticHelpers.EncryptSha512(staffModel.user.password, StaticHelpers.EncryptionKey),
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


            TBL_TEMP_STAFF tempStaff = new TBL_TEMP_STAFF();
          
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
                tempStaffToUpdate.STAFFCODE = staffModel.StaffCode.ToLower().Trim();
                //tempStaffToUpdate.JOBTITLEID = staffModel.JobTitleId;
                tempStaffToUpdate.COMPANYID = staffModel.companyId;
                tempStaffToUpdate.STAFFROLEID = staffModel.staffRoleId;
                tempStaffToUpdate.SUPERVISOR_STAFFID = staffModel.supervisorStaffId;
                tempStaffToUpdate.ADDRESS = staffModel.Address;
                tempStaffToUpdate.ADDRESSOFNOK = staffModel.AddressOfNok;
                tempStaffToUpdate.BRANCHID = staffModel.BranchId;
                tempStaffToUpdate.COMMENT_ = staffModel.Comment;
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
                tempStaffToUpdate.BUSINESSUNITID = staffModel.businessUnitId;
                tempStaffToUpdate.MISCODE = staffModel.misCode;
                context.Entry(tempStaffToUpdate).State = EntityState.Modified;
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
                        STAFFCODE = staffModel.StaffCode.ToLower().Trim(),
                        //JOBTITLEID = staffModel.JobTitleId,
                        COMPANYID = staffModel.companyId,
                        STAFFROLEID = staffModel.staffRoleId,
                        SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                        ADDRESS = staffModel.Address,
                        ADDRESSOFNOK = staffModel.AddressOfNok,
                        BRANCHID = staffModel.BranchId,
                        COMMENT_ = staffModel.Comment,
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
                        //MISINFOID = staffModel.MisinfoId,
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
                        WORKENDDURATION = staffModel.workEndDuration,
                        BUSINESSUNITID = staffModel.businessUnitId,
                        MISCODE = staffModel.misCode,
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = staffid,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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


        public IEnumerable<BatchPrepaymentViewModel> GetAllUnprocessedBulkPrepayment()
        {
            var data = (from a in context.TBL_BULK_PREPAYMENT
                        where a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing
                        select new BatchPrepaymentViewModel()
                        {
                            prepaymentId = a.BULK_PREPAYMENTID,
                            batchCode = Math.Abs(a.BATCHID),
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            processedDate = a.PROCESSDATE,
                            amount = a.AMOUNT,
                            dateCreated = a.DATETIMECREATED
                        }).OrderByDescending(x => x.dateCreated).ToList();

            return data;
        }

        public IEnumerable<BatchPrepaymentViewModel> GetAllUnprocessedBulkPrepaymentBatch(int staffId)
        {
            var staffIds = genSetup.GetStaffRlieved(staffId);
            var staff = from s in context.TBL_STAFF select s;

            var data = (from a in context.TBL_BULK_PREPAYMENT
                        where a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                        && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing
                        select new BatchPrepaymentViewModel()
                        {
                            prepaymentId = a.BULK_PREPAYMENTID,
                            batchCode = Math.Abs(a.BATCHID),
                            //loanReferenceNumber = a.LOANREFERENCENUMBER,
                            processedDate = a.PROCESSDATE,
                            amount = a.AMOUNT,
                            customerId = a.CUSTOMERID,
                            dateCreated = a.DATETIMECREATED
                        }).ToList();

            var result = data.GroupBy(b => b.batchCode).Select(b => new BatchPrepaymentViewModel()
                            {
                                batchCode = b.First().batchCode,
                                numberOfLoans = b.Count(),
                                totalAmount = b.Sum(a => a.amount),
                                processedDate = b.First().processedDate,
                                customerId = b.First().customerId,
                                dateCreated = b.First().dateCreated
            }).OrderByDescending(x => x.dateCreated).ToList();


            var processedBatch = (from a in context.TBL_BULK_PREPAYMENT
                                  join atrail in context.TBL_APPROVAL_TRAIL on a.BATCHID equals atrail.TARGETID
                                  where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing && ((atrail.OPERATIONID == (int)OperationsEnum.BulkLiquidation))
                                  && staffIds.Contains((int)atrail.LOOPEDSTAFFID) && atrail.RESPONSESTAFFID == null
                                    select new BatchPrepaymentViewModel()
                                    {
                                        prepaymentId = a.BULK_PREPAYMENTID,
                                        batchCode = Math.Abs(a.BATCHID),
                                        processedDate = a.PROCESSDATE,
                                        amount = a.AMOUNT,
                                        customerId = a.CUSTOMERID,
                                        dateCreated = a.DATETIMECREATED,
                                        numberOfLoans = 0,
                                        totalAmount = 0,

                                        approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                        //dateCreated = atrail.SYSTEMARRIVALDATETIME,
                                        comment = atrail.COMMENT,
                                        //operationId = atrail.OPERATIONID,
                                        //approvalStatusId = atrail.APPROVALSTATUSID,
                                        //toApprovalLevelName = atrail.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                        fromApprovalLevelName = atrail.FROMAPPROVALLEVELID == null ? staff.FirstOrDefault(r => r.STAFFID == atrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                    }).GroupBy(b => b.batchCode).Select(s => 
                                        new BatchPrepaymentViewModel()
                                        {
                                            prepaymentId = s.FirstOrDefault().prepaymentId,
                                            batchCode = s.FirstOrDefault().batchCode,
                                            processedDate = s.FirstOrDefault().processedDate,
                                            amount = 0,
                                            customerId = s.FirstOrDefault().customerId,
                                            dateCreated = s.FirstOrDefault().dateCreated,
                                            numberOfLoans = s.Count(),
                                            totalAmount = s.Sum(t => t.amount),

                                            approvalStatus = s.FirstOrDefault().approvalStatus.ToUpper(),
                                            //dateCreated = atrail.SYSTEMARRIVALDATETIME,
                                            comment = s.FirstOrDefault().comment,
                                            //operationId = atrail.OPERATIONID,
                                            //approvalStatusId = atrail.APPROVALSTATUSID,
                                            //toApprovalLevelName = atrail.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                            fromApprovalLevelName = s.FirstOrDefault().fromApprovalLevelName,
                                        }).OrderByDescending(x => x.dateCreated).ToList();

            return result.Union(processedBatch);
      }

        private IEnumerable<TBL_BULK_PREPAYMENT> GetPendingBulkPrepaymentByBatch(int batchId)
        {
            var data = (from a in context.TBL_BULK_PREPAYMENT
                        where a.BATCHID == batchId && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending 
                        select a).ToList();

            return data;
        }

        public IEnumerable<BatchPrepaymentViewModel> GetProcessingBulkPrepaymentByBatchId(int batchId)
        {
            var result = GetProcessingBulkPrepaymentByBatch(batchId).ToList();

            return result.Select(b => new BatchPrepaymentViewModel()
            {
                prepaymentId = b.BULK_PREPAYMENTID,
                batchCode = Math.Abs(b.BATCHID),
                loanReferenceNumber = b.LOANREFERENCENUMBER,
                processedDate = b.PROCESSDATE,
                amount = b.AMOUNT,
                dateCreated = b.DATETIMECREATED
            }).ToList();
        }

        private IEnumerable<TBL_BULK_PREPAYMENT> GetProcessingBulkPrepaymentByBatch(int batchId)
        {
            var data = (from a in context.TBL_BULK_PREPAYMENT
                        where a.BATCHID == batchId && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                        select a).ToList();

            return data;
        }

        public bool SubmitPrepaymentBatchForApproval(ApprovalViewModel model)
        {
            bool response = false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.targetId;
                workflow.Comment = model.comment; // "Bulk Prepayment Initiated";
                workflow.OperationId = (int)OperationsEnum.BulkLiquidation;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                var batchLoans = GetPendingBulkPrepaymentByBatch(model.targetId);

                foreach (var batchLoan in batchLoans)
                {
                    batchLoan.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                }

                try
                {
                    response = context.SaveChanges() > 0;
                    transaction.Commit();
                    return response;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public IEnumerable<BatchPrepaymentViewModel> GetBulkPrepaymentsAwaitingApprovalBatch(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.BulkLiquidation).ToList();
            var staff = from s in context.TBL_STAFF select s;

            var processedBatch = (from a in context.TBL_BULK_PREPAYMENT
                                  join atrail in context.TBL_APPROVAL_TRAIL on a.BATCHID equals atrail.TARGETID
                                  where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing && ((atrail.OPERATIONID == (int)OperationsEnum.BulkLiquidation))
                                  && ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.RESPONSESTAFFID == null && atrail.LOOPEDSTAFFID == null
                                  select new BatchPrepaymentViewModel()
                                  {
                                      prepaymentId = a.BULK_PREPAYMENTID,
                                      batchCode = Math.Abs(a.BATCHID),
                                      numberOfLoans = 0,
                                      totalAmount = 0,
                                      processedDate = a.PROCESSDATE,
                                      amount = a.AMOUNT,
                                      customerId = a.CUSTOMERID,
                                      dateCreated = a.DATETIMECREATED,

                                      approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME.ToUpper(),
                                      comment = atrail.COMMENT,
                                      operationId = atrail.OPERATIONID,
                                      approvalStatusId = atrail.APPROVALSTATUSID,
                                      toApprovalLevelName = atrail.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                      fromApprovalLevelName = atrail.FROMAPPROVALLEVELID == null ? staff.FirstOrDefault(r => r.STAFFID == atrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                  }).GroupBy(b => b.batchCode).Select(s =>
                                      new BatchPrepaymentViewModel()
                                      {
                                          prepaymentId = s.FirstOrDefault().prepaymentId,
                                          batchCode = s.FirstOrDefault().batchCode,
                                          numberOfLoans = s.Count(),
                                          totalAmount = s.Sum(t => t.amount),
                                          processedDate = s.FirstOrDefault().processedDate,
                                          amount = s.FirstOrDefault().amount,
                                          customerId = s.FirstOrDefault().customerId,
                                          dateCreated = s.FirstOrDefault().dateCreated,

                                          approvalStatus = s.FirstOrDefault().approvalStatus.ToUpper(),
                                          comment = s.FirstOrDefault().comment,
                                          operationId = s.FirstOrDefault().operationId,
                                          approvalStatusId = s.FirstOrDefault().approvalStatusId,
                                          toApprovalLevelName = s.FirstOrDefault().toApprovalLevelName,
                                          fromApprovalLevelName = s.FirstOrDefault().fromApprovalLevelName,
                                      }).OrderByDescending(x => x.dateCreated).ToList();

            return processedBatch;
        }

        public bool SubmitPrepaymentBatchForWorkflowApproval(ApprovalViewModel model)
        {
            bool response = false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.targetId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.BulkLiquidation;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var batchLoans = GetProcessingBulkPrepaymentByBatch(model.targetId);

                        if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            foreach (var batchLoan in batchLoans)
                            {
                                batchLoan.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            }
                        }

                        if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            foreach (var batchLoan in batchLoans)
                            {
                                batchLoan.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                            }
                        }
                    }

                    response = context.SaveChanges() > 0;
                    transaction.Commit();
                    return response;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
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
                newTempStaff.COMMENT_ = targetStaff.COMMENT_;
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
                newTempStaff.BUSINESSUNITID = targetStaff.BUSINESSUNITID;
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                             Comment = c.COMMENT_,
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
                             businessUnitId = c.BUSINESSUNITID,
                             businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
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

            TBL_STAFF targetStaff = null;
            TBL_PROFILE_USER targetUser = null;

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

            
            var tempStaff = context.TBL_TEMP_STAFF.Find(staffid);
            if (tempStaff != null)
            {
                targetStaff = context.TBL_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == tempStaff.STAFFCODE.ToLower());
                // handling groups and activities
                if (targetStaff != null)
                {

                    targetUser = (from a in context.TBL_PROFILE_USER
                                  where a.STAFFID == targetStaff.STAFFID
                                  select a).FirstOrDefault();
                    if (targetUser != null) //to take care of the existing users groups and activities
                    {
                        var targetGroups = context.TBL_PROFILE_USERGROUP.Where(x => x.USERID == targetUser.USERID).ToList();
                        var targetActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == targetUser.USERID).ToList();
                        if (targetGroups.Any()) //if user already has existing groups
                        {
                            foreach (var item in tempGroup) //check for selected groups not already existing
                            {
                                if (!targetGroups.Exists(a => a.GROUPID == item.GROUPID)) //if selected group isn't existing
                                {
                                    var grpItem = new TBL_PROFILE_USERGROUP()
                                    {
                                        GROUPID = item.GROUPID,
                                        APPROVALSTATUS = false,
                                        DATETIMECREATED = DateTime.Now,
                                        CREATEDBY = item.CREATEDBY,
                                        USERID = targetUser.USERID
                                    };
                                    context.TBL_PROFILE_USERGROUP.Add(grpItem);
                                }
                            }
                            foreach (var item in targetGroups)// handles already exisiting groups
                            {
                                if (tempGroup.Exists(a => a.GROUPID == item.GROUPID)) // if already existing group was selected to remain
                                {
                                    var group = context.TBL_PROFILE_USERGROUP.Find(item.USERGROUPID);
                                    group.GROUPID = item.GROUPID;
                                    group.APPROVALSTATUS = false;
                                    group.DATETIMEUPDATED = DateTime.Now;
                                    group.LASTUPDATEDBY = item.LASTUPDATEDBY;
                                    context.Entry(group).State = EntityState.Modified;
                                }
                                else // if already existing group was deselected
                                {
                                    context.TBL_PROFILE_USERGROUP.Remove(item);
                                }
                            }
                        }
                        else // if user has no existing groups AT ALL for the user
                        {
                            foreach (var item in tempGroup)
                            {
                                var grpItem = new TBL_PROFILE_USERGROUP()
                                {
                                    GROUPID = item.GROUPID,
                                    APPROVALSTATUS = false,
                                    DATETIMECREATED = DateTime.Now,
                                    CREATEDBY = item.CREATEDBY,
                                    USERID = targetUser.USERID
                                };
                                context.TBL_PROFILE_USERGROUP.Add(grpItem);
                            }
                        }
                        
                        if (targetActivities.Any()) //if user already has existing additionalactivities
                        {
                            foreach (var item in tempActivities) //check for selected additionalactivities not already existing
                            {
                                if (!targetActivities.Exists(a => a.ACTIVITYID == item.ACTIVITYID)) //if selected additionalactivity isn't existing
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
                                        EXPIREON = item.EXPIREON,
                                        USERID = targetUser.USERID
                                    };
                                    context.TBL_PROFILE_ADDITIONALACTIVITY.Add(userActivity);
                                }
                            }
                            foreach (var item in targetActivities)// handles already exisiting additionalactivities
                            {
                                if (tempActivities.Exists(a => a.ACTIVITYID == item.ACTIVITYID)) //if already existing additionalactivity was selected to remain
                                {
                                    var activity = context.TBL_PROFILE_ADDITIONALACTIVITY.Find(item.ADDITIONALACTIVITYID);
                                    activity.ACTIVITYID = item.ACTIVITYID;
                                    activity.CANADD = false;
                                    activity.CANEDIT = false;
                                    activity.CANAPPROVE = false;
                                    activity.CANDELETE = false;
                                    activity.CANVIEW = false;
                                    activity.CREATEDBY = item.CREATEDBY;
                                    activity.DATETIMECREATED = DateTime.Now;
                                    activity.EXPIREON = item.EXPIREON;
                                    context.Entry(activity).State = EntityState.Modified;
                                }
                                else // if already existing additionalactivity was deselected
                                {
                                    context.TBL_PROFILE_ADDITIONALACTIVITY.Remove(item);
                                }
                            }
                        }
                        else // if user has no existing additionalactivities AT ALL
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
                                    EXPIREON = item.EXPIREON,
                                    USERID = targetUser.USERID
                                };
                                context.TBL_PROFILE_ADDITIONALACTIVITY.Add(userActivity);
                            }
                        }
                        context.SaveChanges();
                    }
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
                    targetUser.NEXTPASSWORDCHANGEDATE = tempUser.NEXTPASSWORDCHANGEDATE;
                    targetUser.ISFIRSTLOGINATTEMPT = false;
                    targetUser.ISACTIVE = true;
                    targetUser.ISLOCKED = false;
                    targetUser.FAILEDLOGONATTEMPT = 0;
                    targetUser.SECURITYQUESTION = tempUser.SECURITYQUESTION;
                    targetUser.SECURITYANSWER = tempUser.SECURITYANSWER;
                    //targetUser.TBL_PROFILE_USERGROUP = userGroups;
                    //targetUser.TBL_PROFILE_ADDITIONALACTIVITY = userActivities;
                }
                else //creates an entirely new user with all new groups, activities and additional activities
                {
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
                                EXPIREON = item.EXPIREON
                            };
                            userActivities.Add(userActivity);
                        }
                    }
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


            if (targetStaff != null) //Update existing staff with tempStaff record
            {

                targetStaff.FIRSTNAME = tempStaff.FIRSTNAME;
                targetStaff.COMPANYID = tempStaff.COMPANYID;
                targetStaff.MIDDLENAME = tempStaff.MIDDLENAME;
                targetStaff.LASTNAME = tempStaff.LASTNAME;
                targetStaff.STAFFCODE = tempStaff.STAFFCODE;
                targetStaff.JOBTITLEID = tempStaff.JOBTITLEID;
                targetStaff.STAFFROLEID = tempStaff.STAFFROLEID;
                targetStaff.SUPERVISOR_STAFFID = tempStaff.SUPERVISOR_STAFFID;
                targetStaff.ADDRESS = tempStaff.ADDRESS;
                targetStaff.ADDRESSOFNOK = tempStaff.ADDRESSOFNOK;
                targetStaff.BRANCHID = tempStaff.BRANCHID;
                targetStaff.COMMENT_ = tempStaff.COMMENT_;
                targetStaff.CREATEDBY = tempStaff.CREATEDBY;
                if (tempStaff.CUSTOMERSENSITIVITYLEVELID >= 1) targetStaff.CUSTOMERSENSITIVITYLEVELID = tempStaff.CUSTOMERSENSITIVITYLEVELID;
                targetStaff.DATEOFBIRTH = tempStaff.DATEOFBIRTH;
                targetStaff.DATETIMEUPDATED = DateTime.Now;
                //entity.DEPARTMENTID = temp.DEPARTMENTID;
                targetStaff.DEPARTMENTUNITID = tempStaff.DEPARTMENTUNITID;
                targetStaff.EMAIL = tempStaff.EMAIL;
                targetStaff.EMAILOFNOK = tempStaff.EMAILOFNOK;
                targetStaff.GENDER = tempStaff.GENDER;
                targetStaff.GENDEROFNOK = tempStaff.GENDEROFNOK;
                targetStaff.MISINFOID = tempStaff.MISINFOID;
                targetStaff.NAMEOFNOK = tempStaff.NAMEOFNOK;
                targetStaff.NOKRELATIONSHIP = tempStaff.NOKRELATIONSHIP;
                targetStaff.PHONE = tempStaff.PHONE;
                targetStaff.PHONEOFNOK = tempStaff.PHONEOFNOK;
                targetStaff.STATEID = tempStaff.STATEID;
                targetStaff.CITYID = tempStaff.CITYID;
                targetStaff.LOAN_LIMIT = tempStaff.LOAN_LIMIT;
                targetStaff.DELETED = false;
                targetStaff.WORKSTARTDURATION = tempStaff.WORKSTARTDURATION;
                targetStaff.WORKENDDURATION = tempStaff.WORKENDDURATION;
                targetStaff.MISCODE = tempStaff.MISCODE;
            }
            else //Insert a new staff record into the real staff table
            {
                targetStaff = new TBL_STAFF()
                {
                    //STAFFID = 3000,
                    FIRSTNAME = tempStaff.FIRSTNAME,
                    MIDDLENAME = tempStaff.MIDDLENAME,
                    COMPANYID = tempStaff.COMPANYID,
                    LASTNAME = tempStaff.LASTNAME,
                    STAFFCODE = tempStaff.STAFFCODE,
                    JOBTITLEID = tempStaff.JOBTITLEID,
                    STAFFROLEID = tempStaff.STAFFROLEID,
                    SUPERVISOR_STAFFID = tempStaff.SUPERVISOR_STAFFID,
                    DEPARTMENTUNITID = tempStaff.DEPARTMENTUNITID,
                    ADDRESS = tempStaff.ADDRESS,
                    ADDRESSOFNOK = tempStaff.ADDRESSOFNOK,
                    BRANCHID = tempStaff.BRANCHID,
                    COMMENT_ = tempStaff.COMMENT_,
                    CREATEDBY = tempStaff.CREATEDBY,
                    DATEOFBIRTH = tempStaff.DATEOFBIRTH,
                    DATETIMECREATED = DateTime.Now,
                    //DEPARTMENTID = temp.DEPARTMENTID,
                    EMAIL = tempStaff.EMAIL,
                    EMAILOFNOK = tempStaff.EMAILOFNOK,
                    GENDER = tempStaff.GENDER,
                    GENDEROFNOK = tempStaff.GENDEROFNOK,
                    MISINFOID = tempStaff.MISINFOID,
                    NAMEOFNOK = tempStaff.NAMEOFNOK,
                    NOKRELATIONSHIP = tempStaff.NOKRELATIONSHIP,
                    PHONE = tempStaff.PHONE,
                    PHONEOFNOK = tempStaff.PHONEOFNOK,
                    STATEID = tempStaff.STATEID,
                    CITYID = tempStaff.CITYID,
                    LOAN_LIMIT = tempStaff.LOAN_LIMIT,
                    WORKSTARTDURATION = tempStaff.WORKSTARTDURATION,
                    WORKENDDURATION = tempStaff.WORKENDDURATION,
                    BUSINESSUNITID = tempStaff.BUSINESSUNITID,
                    MISCODE = tempStaff.MISCODE,
            };
                if (tempStaff.CUSTOMERSENSITIVITYLEVELID >= 1) targetStaff.CUSTOMERSENSITIVITYLEVELID = tempStaff.CUSTOMERSENSITIVITYLEVELID;
                context.TBL_STAFF.Add(targetStaff);
                var test = context.SaveChanges() > 0;

            }

            tempStaff.ISCURRENT = false;
            tempStaff.APPROVALSTATUSID = approvalStatusId;
            tempStaff.DATETIMEUPDATED = DateTime.Now;
            tempStaff.LASTUPDATEDBY = user.createdBy;
            //if (temp.TEMPSTAFFID != entity.RELIEF_STAFFID) { UpdateDelegateStaff(entity.STAFFID, temp.TEMPSTAFFID); }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Staff '{tempStaff?.FIRSTNAME + " " + tempStaff?.LASTNAME}' with staff code'{tempStaff?.STAFFCODE}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            try
            {
                context.TBL_AUDIT.Add(audit);
                var output = context.SaveChanges() > 0;

                if (isUpdate == false && targetUser != null)
                {
                    targetUser.STAFFID = targetStaff.STAFFID;
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
                            EXPIREON = item.EXPIREON,
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
                    entity.COMMENT_ = temp.COMMENT_;
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
                    entity.BUSINESSUNITID = temp.BUSINESSUNITID;
                    entity.MISCODE = temp.MISCODE;
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
                        COMMENT_ = temp.COMMENT_,
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
                        BUSINESSUNITID = temp.BUSINESSUNITID,
                        WORKENDDURATION = temp.WORKENDDURATION,
                        MISCODE = temp.MISCODE,
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                        EXPIREON = item.expireOn
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
                    USERNAME = staffModel.user.username.ToLower().Trim(),
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
                STAFFCODE = staffModel.StaffCode.ToLower().Trim(),
                //JOBTITLEID = staffModel.JobTitleId,
                STAFFROLEID = staffModel.staffRoleId,
                SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                ADDRESS = staffModel.Address,
                ADDRESSOFNOK = staffModel.AddressOfNok,
                BRANCHID = staffModel.BranchId,
                COMMENT_ = staffModel.Comment,
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
                WORKENDDURATION = staffModel.workEndDuration,
                BUSINESSUNITID = staffModel.businessUnitId,
                MISCODE = staffModel.misCode
            };
            // Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
            //    STAFFID = staffModel.createdBy,
            //    BRANCHID = (short)staffModel.BranchId,
            //    DETAIL = $"Updated Staff Creation for '{staffModel?.StaffFullName}' with code'{staffModel?.StaffCode}'",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = staffModel.applicationUrl,
            //    APPLICATIONDATE = genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName(),
            //};

            //auditTrail.AddAuditTrail(audit);
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
                             Comment = c.COMMENT_,
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
                             businessUnitId = c.BUSINESSUNITID,
                             OperationId = t.OPERATIONID,
                             businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
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
                        Comment = c.COMMENT_,
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
                        businessUnitId = c.BUSINESSUNITID,
                        businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
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
                            Comment = c.COMMENT_,
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
                            businessUnitName = c.BUSINESSUNITID != null ? c.TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITNAME : null,
                            businessUnitId = c.BUSINESSUNITID,
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
                           companyId = st.COMPANYID
                           //departmentId = st.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                           //departmentUnitId = (short)st.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITID,

                       };

            return data.ToList();
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
                        staffRoleName = o.TBL_STAFF_ROLE.STAFFROLENAME
                    })
                    .Take(12);
            }

            return staff;
        }

        public IEnumerable<simpleStaffModel> SearchApprovers(int levelId, string searchQuery ="", int companyId=0)
        {
            var level = context.TBL_APPROVAL_LEVEL.Find(levelId);
            //var allLevelStaffs = level.TBL_APPROVAL_LEVEL_STAFF.Select(l => l.TBL_STAFF).ToList();
            

            //var nextApprovalLvlRoleId = GetNextApprovalLvlRoleId(roleId, level.GROUPID);
            var approvalLvlRoleId = level.STAFFROLEID;
            IQueryable<simpleStaffModel> staff = null;
            IQueryable<simpleStaffModel> levelStaffs = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                levelStaffs = (from l in context.TBL_APPROVAL_LEVEL
                                      join ls in context.TBL_APPROVAL_LEVEL_STAFF on l.APPROVALLEVELID equals ls.APPROVALLEVELID
                                      join s in context.TBL_STAFF on ls.STAFFID equals s.STAFFID
                                      where l.DELETED == false && ls.DELETED == false && s.DELETED == false
                                      && l.APPROVALLEVELID == level.APPROVALLEVELID
                                      && (s.FIRSTNAME.ToLower().Contains(searchQuery)
                                        || s.MIDDLENAME.ToLower().Contains(searchQuery)
                                        || s.LASTNAME.ToLower().Contains(searchQuery)
                                        || s.STAFFCODE.ToLower().Contains(searchQuery))
                                      select new simpleStaffModel
                                      {
                                          staffId = s.STAFFID,
                                          firstName = s.FIRSTNAME,
                                          middleName = s.MIDDLENAME,
                                          lastName = s.LASTNAME,
                                          staffCode = s.STAFFCODE,
                                          staffRoleName = s.TBL_STAFF_ROLE.STAFFROLENAME,
                                          staffRoleId = s.STAFFROLEID,
                                      }).Distinct();

                //staff =
                //    context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false && x.OPERATIONID == operationId)
                //    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                //    .Join(context.TBL_APPROVAL_LEVEL, mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new { mg, l })
                //    .Join(context.TBL_STAFF.Where(x => x.DELETED == false
                //         && x.STAFFROLEID == nextApprovalLvlRoleId)
                //        .Where(x => x.FIRSTNAME.ToLower().Contains(searchQuery)
                //        || x.MIDDLENAME.ToLower().Contains(searchQuery)
                //        || x.LASTNAME.ToLower().Contains(searchQuery)
                //        || x.STAFFCODE.ToLower().Contains(searchQuery))
                //    , mgl => mgl.l.STAFFROLEID, s => s.STAFFROLEID, (mgl, s) => new { mgl, s })
                //    .Select(o => new simpleStaffModel
                //    {
                //        staffId = o.s.STAFFID,
                //        firstName = o.s.FIRSTNAME,
                //        middleName = o.s.MIDDLENAME,
                //        lastName = o.s.LASTNAME,
                //        staffCode = o.s.STAFFCODE,
                //        staffRoleName = o.s.TBL_STAFF_ROLE.STAFFROLENAME,
                //        staffRoleId = o.s.STAFFROLEID,
                //    }).Distinct();

                staff = (from l in context.TBL_APPROVAL_LEVEL
                         join s in context.TBL_STAFF on l.STAFFROLEID equals s.STAFFROLEID
                         where l.DELETED == false && s.DELETED == false
                         && l.APPROVALLEVELID == level.APPROVALLEVELID
                         && (s.FIRSTNAME.ToLower().Contains(searchQuery)
                           || s.MIDDLENAME.ToLower().Contains(searchQuery)
                           || s.LASTNAME.ToLower().Contains(searchQuery)
                           || s.STAFFCODE.ToLower().Contains(searchQuery))
                         select new simpleStaffModel
                         {
                             staffId = s.STAFFID,
                             firstName = s.FIRSTNAME,
                             middleName = s.MIDDLENAME,
                             lastName = s.LASTNAME,
                             staffCode = s.STAFFCODE,
                             staffRoleName = s.TBL_STAFF_ROLE.STAFFROLENAME,
                             staffRoleId = s.STAFFROLEID,
                         }).Distinct();
            }
            if (levelStaffs == null)
            {
                return staff;
            }
            if (staff == null)
            {
                return levelStaffs;
            }
            staff = staff.Union(levelStaffs);
            var staffs = staff.AsEnumerable().ToList();
            return staffs;
        }

        public int GetNextApprovalLvlRoleId(int roleId, int groupId)
        {
            //if (groupId == 0) groupId = 261; //ApprovalGroupEnum.Business
            var levels = context.TBL_APPROVAL_LEVEL.Where(l => l.GROUPID == groupId).ToList();

            //if (levels.Count == 0) {
            //    groupId = 844;
            //    levels = context.TBL_APPROVAL_LEVEL.Where(l => l.GROUPID == groupId).ToList();
            //}

            var currentPosition = levels.Find(l => l.STAFFROLEID == roleId)?.POSITION ?? 0;
            var nextRoleId = levels.Find(l => l.POSITION == currentPosition + 1)?.STAFFROLEID ?? 0;
            return (int)nextRoleId;
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
                            //if (setupGlobal.USE_ACTIVE_DIRECTORY)
                            //{
                            //    getADDetails = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                            //    if (getADDetails == null)
                            //    {
                            //        rowSuccess = false;
                            //        staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                            //    }
                            //    else
                            //    {
                            //        if (context.TBL_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any() || context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffRowData.StaffCode).Any())
                            //        {
                            //            rowSuccess = false;
                            //            staffRowData.message = staffRowData.message + "Staff Code Already Exist. ";
                            //        }
                            //    }
                            //}

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
                            //if(setupGlobal.USE_ACTIVE_DIRECTORY)
                            //{
                            //    var firstName = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                            //    if (firstName == null)
                            //    {
                            //        rowSuccess = false;
                            //        staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                            //    }
                            //    else
                            //    {
                            //        staffRowData.FirstName = firstName.firstName;
                            //    }
                            //}
                            //else
                            //{
                                staffRowData.FirstName = cell.Value.ToString();
                                if (staffRowData.FirstName == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + $"Firstname cannot be null. ";
                                }
                            //}                           
                            break;
                        case "F":
                            //if (setupGlobal.USE_ACTIVE_DIRECTORY == true)
                            //{
                            //    var record = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                            //    if (record == null)
                            //    {
                            //        rowSuccess = false;
                            //        staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                            //    }
                            //    else
                            //    {
                            //        staffRowData.LastName = record.lastName;
                            //    }
                            //}
                            //else
                            //{
                                staffRowData.LastName = cell.Value.ToString();
                                if (staffRowData.LastName == null)
                                {
                                    rowSuccess = false;
                                    staffRowData.message = staffRowData.message + $"LastName cannot be null. ";
                                }
                            //}                           
                            break;
                        case "G":
                            //if (setupGlobal.USE_ACTIVE_DIRECTORY == true)
                            //{
                            //    var record = adminRepo.GetStaffActiveDirectoryDetails(staffRowData.StaffCode, model.loginStaffCode, model.loginStaffPassword);
                            //    if (record != null)
                            //    {
                            //        staffRowData.MiddleName = record.middleName;
                            //    }
                            //    else
                            //    {
                            //        rowSuccess = false;
                            //        staffRowData.message = staffRowData.message + "Staff Code Doesnt Exist in Active Directory. ";
                            //    }
                            //}
                            //else
                            //{
                                staffRowData.MiddleName = cell.Value.ToString();
                            //}
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
                                //rowSuccess = false;
                                //staffRowData.message = staffRowData.message + $"Supervisor Code @ cell '{cellColumn}' of row '{cellRow}' does not exist. ";
                            }
                            break;
                        case "J":
                            ////var unit = context.TBL_DEPARTMENT_UNIT.Where(x => x.DEPARTMENTUNITNAME.ToLower() == cell.Value.ToString().ToLower()).FirstOrDefault();

                            //if (unit != null) staffRowData.departmentUnitId = unit.DEPARTMENTUNITID;
                            ////else
                            //{
                                staffRowData.misCode = cell.Value.ToString();
                            ////}
                            break;
                        case "K":
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
                    //staffRowData.JobTitleId = jobTitle.JOBTITLEID;
                    //staffRowData.JobTitleName = jobTitle.JOBTITLENAME;
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
                //USERNAME = staffCode,
                USERNAME = staffModel.StaffCode.ToLower().Trim(),
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
                //STAFFCODE = staffCode,
                STAFFCODE = staffModel.StaffCode.ToLower().Trim(),
                JOBTITLEID = staffModel.JobTitleId,
                STAFFROLEID = staffModel.staffRoleId,
                SUPERVISOR_STAFFID = staffModel.supervisorStaffId,
                ADDRESS = staffModel.Address,
                ADDRESSOFNOK = staffModel.AddressOfNok,
                BRANCHID = staffModel.BranchId,
                COMMENT_ = staffModel.Comment,
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
                BUSINESSUNITID = staffModel.businessUnitId,
                TBL_TEMP_PROFILE_USER = userInfo,
                MISCODE = staffModel.misCode
            };
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreateStaffInitiated,
                STAFFID = staffModel.createdBy,
                BRANCHID = (short)staffModel.BranchId,
                DETAIL = $"Initiated Staff and User Creation for '{staffModel?.StaffFullName}' with code'{staffModel?.StaffCode}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = staffModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = appdate,
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = appdate,
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                COMMENT_ = comment,
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
                BUSINESSUNITID = staff.BUSINESSUNITID
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
            if (misRecord != null) {
                model.username = misRecord.field1;
                model.teamUnit = misRecord.field2;
                model.costCent = misRecord.field3;
                model.dept = misRecord.field4;
                model.region = misRecord.field5;
                model.group = misRecord.field6;
                model.directorate = misRecord.field7;
            }
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

        public IEnumerable<simpleStaffModel> GetStaffRoles(int companyId)
        {
            // context.TBL_STAFF_ROLE.Where(o => o.COMPANYID == companyId).Select(o => o).ToList();
            var role = from x in context.TBL_STAFF_ROLE
                       where x.COMPANYID == companyId
                       select new simpleStaffModel
                       {
                           staffRoleId = x.STAFFROLEID,
                           staffRoleName = x.STAFFROLENAME
                       };

            return role;
        }
    }
}