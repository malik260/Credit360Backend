using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.SupportUtility;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class SupportUtilityRepository : ISupportUtilityRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        public SupportUtilityRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup)
        {
            context = _context;
        }


        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.FIRSTNAME + " " + s.LASTNAME
                //name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }
        public IEnumerable<CustomerViewModels> GetCustomersIssuesByParams(string searchParam, short? IssueTypeId)
        {
           if(IssueTypeId == (short)IssueTypeEnum.CustomerBusinessUnitNotSet) { return GetCustomersByParams(searchParam, IssueTypeId).Where(x => x.businessUnitId == null); }

           if (IssueTypeId == (short)IssueTypeEnum.DuplicateCustomerRecord) {

                IEnumerable<CustomerViewModels> resultset = GetCustomersByParams(searchParam, IssueTypeId);
                return resultset.Where(x => x.customerCode.Count() > 1 || x.emailAddress.Count() > 1) ;
            }

            else  
            {
                IEnumerable<CustomerViewModels>  MissingBusinesUnitResult = GetCustomersByParams(searchParam, IssueTypeId).Where(x => x.businessUnitId == null);
                IEnumerable<CustomerViewModels> resultset = GetCustomersByParams(searchParam, IssueTypeId);
                var duplicateRecord =  resultset.Where(x => x.customerCode.Count() > 1 || x.emailAddress.Count() > 1);

                return MissingBusinesUnitResult.Union(duplicateRecord);
            }
        }

        IQueryable<CustomerViewModels> GetCustomersByParams(string searchParam, short? IssueTypeId)
        {
            return from a in context.TBL_CUSTOMER
                   join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                   where a.DELETED == false 
                   && (a.CUSTOMERCODE.Contains(searchParam) || a.FIRSTNAME.Contains(searchParam) || a.MIDDLENAME.Contains(searchParam) || a.LASTNAME.Contains(searchParam))

                   select new CustomerViewModels
                       {
                           crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                           crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                           crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                           accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                           branchId = a.BRANCHID,
                           branchName = a.TBL_BRANCH.BRANCHNAME,
                           companyMainId = a.COMPANYID,
                           createdBy = a.CREATEDBY,
                           creationMailSent = a.CREATIONMAILSENT,
                           customerCode = a.CUSTOMERCODE,
                           customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                           customerTypeId = (short)a.CUSTOMERTYPEID,
                           dateOfBirth = (DateTime)a.DATEOFBIRTH,
                           customerId = a.CUSTOMERID,
                           emailAddress = a.EMAILADDRESS,
                           firstName = a.FIRSTNAME,
                           gender = a.GENDER,
                           lastName = a.LASTNAME,
                           maidenName = a.MAIDENNAME,
                           maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : "F",
                           title = a.TITLE,
                           middleName = a.MIDDLENAME,
                           misCode = a.MISCODE,
                           misStaff = a.MISSTAFF,
                           nationalityId = a.NATIONALITYID,
                           occupation = a.OCCUPATION,
                           placeOfBirth = a.PLACEOFBIRTH,
                           isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                           relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                           relationshipOfficerName = st.FIRSTNAME + " " + st.LASTNAME,
                           spouse = a.SPOUSE,
                           sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                           sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                           subSectorId = (short)a.SUBSECTORID,
                           subSectorName = a.TBL_SUB_SECTOR.NAME,
                           taxNumber = a.TAXNUMBER,
                           riskRatingId = a.RISKRATINGID,
                           ownership = a.OWNERSHIP,
                           customerBVN = a.CUSTOMERBVN,
                           customerIssueTypeId = IssueTypeId 
                   };
        }

       public SupportUtilityViewModel GetSupportIssueType(int supportIssueTypeId)
        {
            var entity = context.TBL_SUPPORTISSUETYPE.FirstOrDefault(x => x.SUPPORTISSUETYPEID == supportIssueTypeId);
            return new SupportUtilityViewModel
            {
                supportIssueTypeId = entity.SUPPORTISSUETYPEID,
                description = entity.DESCRIPTION,
                tag = entity.TAG,
            };
        }

        public IEnumerable<SupportUtilityViewModel>GetAllSupportIssueType()
        {
            return context.TBL_SUPPORTISSUETYPE.Where(x => x.TAG == 1)
               .Select(x => new SupportUtilityViewModel
               {
                   supportIssueTypeId = x.SUPPORTISSUETYPEID,
                   description = x.DESCRIPTION,
                   tag = x.TAG,
               })
               .ToList();
        }

        public BusinessRuleViewModel GetBusinessRule( int approvalLevelId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var businessRule = (from a in context.TBL_APPROVAL_LEVEL
                                       join x in context.TBL_APPROVAL_BUSINESS_RULE on a.APPROVALBUSINESSRULEID equals x.APPROVALBUSINESSRULEID
                                       where approvalLevelId == a.APPROVALLEVELID

                                   select new BusinessRuleViewModel
                                   {
                                       levelBusinessRuleId = x.APPROVALBUSINESSRULEID,
                                       description = x.DESCRIPTION,
                                       minimumAmount = x.MINIMUMAMOUNT,
                                       maximumAmount = x.MAXIMUMAMOUNT,
                                       pepAmount = x.PEPAMOUNT,
                                       pep = x.PEP,
                                       projectRelated = x.PROJECTRELATED,
                                       insiderRelated = x.INSIDERRELATED,
                                       onLending = x.ONLENDING,
                                       interventionFunds = x.INTERVENTIONFUNDS,
                                       orrBasedApproval = x.ORRBASEDAPPROVAL,
                                       esrm = x.ESRM,
                                       isForContingentFacility = x.ISFORCONTINGENTFACILITY,
                                       isForRevolvingFacility = x.ISFORREVOLVINGFACILITY,
                                       isForRenewal = x.ISFORRENEWAL,
                                       exemptContingentFacility = x.EXEMPTCONTINGENTFACILITY,
                                       exemptRevolvingFacility = x.EXEMPTREVOLVINGFACILITY,
                                       exemptRenewal = x.EXEMPTRENEWAL,
                                       tenor = x.TENOR,
                                       withoutInstruction = x.WITHINSTRUCTION,
                                       domiciliationNotInPlace = x.DOMICILIATIONNOTINPLACE,
                                       excludeLevel = x.EXCLUDELEVEL,
                                       isAgricRelated = x.ISAGRICRELATED,
                                   }).FirstOrDefault();
               return businessRule;               
            }
        }

        public IEnumerable<WorkflowSupportUtilityViewModel> GetApprovalTrail(string searchString)
        {
            IEnumerable<WorkflowSupportUtilityViewModel> data = null;

          
                var allstaff = this.GetAllStaffNames();
                var staffs = from s in context.TBL_STAFF select s;

                searchString = searchString.Trim().ToLower();

                var loanApplicationId = (from x in context.TBL_LOAN_APPLICATION
                                        
                                        where
                                        x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                                        select x.LOANAPPLICATIONID).ToList();

                var bookingId = (from x in context.TBL_LOAN_APPLICATION
                                        join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                        join p in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals p.LOANAPPLICATIONDETAILID
                                        
                                        where
                                        x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                                        select p.LOAN_BOOKING_REQUESTID).ToList();

                

                var applications = (from x in context.TBL_LOAN_APPLICATION
                                    join c in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals c.TARGETID
                                    where
                                    loanApplicationId.Contains(c.TARGETID)
                                   
                                    select new WorkflowSupportUtilityViewModel
                                    {
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        
                                        customerGroupId = x.CUSTOMERGROUPID,
                                        loanTypeId = x.LOANAPPLICATIONTYPEID,
                                        transactionDate = c.SYSTEMARRIVALDATETIME,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        productClassId = x.PRODUCTCLASSID,
                                        productId = (short)(x.PRODUCTID ?? 0),
                                        customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                        isRelatedParty = x.ISRELATEDPARTY,
                                        responsestaffName = c.RESPONSESTAFFID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == c.RESPONSESTAFFID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                        toStaffName = c.TOSTAFFID == null ? "N/A" : context.TBL_STAFF.Where(a => a.STAFFID == c.TOSTAFFID).Select(a => a.FIRSTNAME + " " + a.LASTNAME).FirstOrDefault(),
                                        fromStaffName = c.REQUESTSTAFFID == 0 ? "N/A" : context.TBL_STAFF.Where(a => a.STAFFID == c.REQUESTSTAFFID).Select(a => a.FIRSTNAME + " " + a.LASTNAME).FirstOrDefault(),
                                        toApprovalLevelName = c.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == c.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                        isInvestmentGrade = x.ISINVESTMENTGRADE,
                                        approvalTrailId = c.APPROVALTRAILID,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == c.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                        createdBy = x.OWNEDBY,
                                        operationId = c.OPERATIONID,
                                        oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == c.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                    }).OrderBy(x => x.oprationName).ThenBy(x => x.toApprovalLevelName).ToList();

            foreach(var i in applications)
            {
                    var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == i.loanApplicationId).FirstOrDefault();
                    var product = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanDetail.APPROVEDPRODUCTID).FirstOrDefault();

                i.loanApplicationDetailId = loanDetail.LOANAPPLICATIONDETAILID;
                i.productName = product.PRODUCTNAME;
                i.customerName = context.TBL_CUSTOMER.Where(z => z.CUSTOMERID == loanDetail.CUSTOMERID).Select(z => z.LASTNAME + " " + z.FIRSTNAME + " " + z.MIDDLENAME).FirstOrDefault();

            }

            var applications2 = (from x in context.TBL_LOAN_APPLICATION
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                    join p in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals p.LOANAPPLICATIONDETAILID
                                    join c in context.TBL_APPROVAL_TRAIL on p.LOAN_BOOKING_REQUESTID equals c.TARGETID
                                    join cu in context.TBL_CUSTOMER on a.CUSTOMERID equals cu.CUSTOMERID
                                    join pr in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals pr.PRODUCTID 

                                    where
                                    bookingId.Contains(c.TARGETID)
                                    select new WorkflowSupportUtilityViewModel
                                    {
                                        customerName = context.TBL_CUSTOMER.Where(z => z.CUSTOMERID == a.CUSTOMERID).Select(z => z.LASTNAME + " " + z.FIRSTNAME + " " + z.MIDDLENAME).FirstOrDefault(),
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        productName = pr.PRODUCTNAME,
                                        customerGroupId = x.CUSTOMERGROUPID,
                                        loanTypeId = x.LOANAPPLICATIONTYPEID,
                                        transactionDate = c.SYSTEMARRIVALDATETIME,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        productClassId = x.PRODUCTCLASSID,
                                        productId = (short)(x.PRODUCTID ?? 0),
                                        customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                        isRelatedParty = x.ISRELATEDPARTY,
                                        responsestaffName = c.RESPONSESTAFFID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == c.RESPONSESTAFFID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                        toStaffName = c.TOSTAFFID == null ? "N/A" : context.TBL_STAFF.Where(a => a.STAFFID == c.TOSTAFFID).Select(a => a.FIRSTNAME + " " + a.LASTNAME).FirstOrDefault(),
                                        fromStaffName = c.REQUESTSTAFFID == 0 ? "N/A" : context.TBL_STAFF.Where(a => a.STAFFID == c.REQUESTSTAFFID).Select(a => a.FIRSTNAME + " " + a.LASTNAME).FirstOrDefault(),
                                        toApprovalLevelName = c.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == c.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                        isInvestmentGrade = x.ISINVESTMENTGRADE,
                                        approvalTrailId = c.APPROVALTRAILID,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == c.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                        createdBy = x.OWNEDBY,
                                        operationId = c.OPERATIONID,
                                        oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == c.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                    }).OrderBy(x => x.oprationName).ThenBy(x => x.toApprovalLevelName).ToList();

                         data =  applications.Union(applications2);
                        var totalRecords = data.ToList();

                        return totalRecords;
        }


        public ApprovalTrailViewModel GetSingleTrail(int approvalTrailId)
        {
            var allstaff = this.GetAllStaffNames();
            var staffs = from s in context.TBL_STAFF select s;

            var singleTrail = (from x in context.TBL_APPROVAL_TRAIL
                                   where x.APPROVALTRAILID == approvalTrailId


                                   select new ApprovalTrailViewModel
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
                                       loopedStaff = allstaff.FirstOrDefault(s => s.id == x.LOOPEDSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.LOOPEDSTAFFID).name,
                                       fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                                       fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                       toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                       toApprovalLevelId = x.TOAPPROVALLEVELID,
                                       approvalStateId = x.APPROVALSTATEID,
                                       approvalStatusId = x.APPROVALSTATUSID,
                                       reliefStaffId = x.RELIEVEDSTAFFID,
                                       reliefStaff = allstaff.FirstOrDefault(s => s.id == x.RELIEVEDSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RELIEVEDSTAFFID).name,
                                       vote = x.VOTE,
                                       oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                       approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                                       approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                       responsestaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                                       fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                                       toStaffName = allstaff.FirstOrDefault(s => s.id == x.TOSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.TOSTAFFID).name,
                                       referBackState = x.REFEREBACKSTATEID,
                                   }).FirstOrDefault();
            return singleTrail;
        }

        public List<ExpectedWorkflowViewModel> GetExpectedWorkFlow(int searchString)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var applications = (from x in context.TBL_APPROVAL_GROUP_MAPPING
                                    join a in context.TBL_APPROVAL_GROUP on x.GROUPID equals a.GROUPID
                                    join p in context.TBL_APPROVAL_LEVEL on a.GROUPID equals p.GROUPID
                                    where p.ISACTIVE == true && p.DELETED == false &&
                                    x.OPERATIONID == searchString
                                    orderby p.POSITION ascending
                                    select new ExpectedWorkflowViewModel
                                    {
                                      operationName = context.TBL_OPERATIONS.Where(op => op.OPERATIONID == x.OPERATIONID).Select(op => op.OPERATIONNAME).FirstOrDefault(),
                                      groupName = a.GROUPNAME,
                                      productClassId = x.PRODUCTCLASSID,
                                      productClassName = context.TBL_PRODUCT_CLASS.Where(pc => pc.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(pc => pc.PRODUCTCLASSNAME).FirstOrDefault(),
                                      productId = x.PRODUCTID,
                                      productName = context.TBL_PRODUCT.Where(pr => pr.PRODUCTID == x.PRODUCTID).Select(pr => pr.PRODUCTNAME + " " + pr.PRODUCTCODE).FirstOrDefault(),
                                      levelName = p.LEVELNAME,
                                      approvalLevelId = p.APPROVALLEVELID, 
                                      canApprove = p.CANAPPROVE,
                                      position = p.POSITION,
                                      approvalBusinessRuleId = p.APPROVALBUSINESSRULEID,
                                      maximumAmount  = p.MAXIMUMAMOUNT,
                                      businessRule = context.TBL_APPROVAL_BUSINESS_RULE.Where(o => o.APPROVALBUSINESSRULEID == p.APPROVALBUSINESSRULEID).Select(o => o.DESCRIPTION).FirstOrDefault(),
                                    }).ToList();


                return applications;
            }
        }

        public List<WorkflowSupportUtilityViewModel> GetDistinctOperations(string searchString)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                searchString = searchString.Trim().ToLower();

                var approvalLevelIds = (from x in context.TBL_LOAN_APPLICATION
                                        join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                        join p in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals p.LOANAPPLICATIONDETAILID
                                        join c in context.TBL_APPROVAL_TRAIL on p.LOAN_BOOKING_REQUESTID equals c.TARGETID
                                        where
                                        x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                                        select c.APPROVALTRAILID).ToList();

                var operations = (from x in context.TBL_LOAN_APPLICATION
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                    join p in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals p.LOANAPPLICATIONDETAILID
                                    join c in context.TBL_APPROVAL_TRAIL on p.LOAN_BOOKING_REQUESTID equals c.TARGETID
                                    join cu in context.TBL_CUSTOMER on a.CUSTOMERID equals cu.CUSTOMERID
                                    join pr in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals pr.PRODUCTID
                                    where
                                    approvalLevelIds.Contains(c.APPROVALTRAILID)
                                    select new WorkflowSupportUtilityViewModel
                                    {
                                        operationId = c.OPERATIONID,
                                        oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == c.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                    }).GroupBy(p => p.operationId).Select(l => l.OrderByDescending(t => t.oprationName).FirstOrDefault())
                                     .ToList();


                return operations;
            }

        }

        public List<StaffSupportUtilityViewModel> GetStaff(string searchString)
        {
            searchString = searchString.Trim().ToLower();

            var staff = (from c in context.TBL_STAFF
                        where c.STAFFCODE.ToLower().Contains(searchString)
                        || c.FIRSTNAME.ToLower().Contains(searchString)
                        || c.MIDDLENAME.ToLower().Contains(searchString)
                        || c.LASTNAME.ToLower().Contains(searchString)
                        
                         select new StaffSupportUtilityViewModel
                         {
                             staffId = c.STAFFID,
                             BranchId = c.BRANCHID,
                             BranchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                             //customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                            // DepartmentId = c.TBL_DEPARTMENT_UNIT.DEPARTMENTID,
                             email = c.EMAIL,
                             gender = c.GENDER,
                            //s jobTitleId = c.JOBTITLEID,
                             MisinfoId = c.MISINFOID,
                             Phone = c.PHONE,
                             StateId = c.STATEID,
                             FirstName = c.FIRSTNAME,
                             MiddleName = c.MIDDLENAME,
                             deleted = c.DELETED,
                             deletedby = c.DELETEDBY == null? "N/A" : context.TBL_STAFF.Where(o => o.DELETEDBY == c.STAFFID).Select(o => o.FIRSTNAME).FirstOrDefault(),
                             updatedById = c.LASTUPDATEDBY,
                             updatedBy = c.LASTUPDATEDBY == null? "N/A" : context.TBL_STAFF.Where(o => o.LASTUPDATEDBY == c.STAFFID).Select(o => o.FIRSTNAME).FirstOrDefault(),
                             LastName = c.LASTNAME,
                             StaffCode = c.STAFFCODE,
                             staffRoleId = c.STAFFROLEID,
                             staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                             supervisorStaffId = c.SUPERVISOR_STAFFID,
                             supervisorStaffName = context.TBL_STAFF.Where(o => o.STAFFID == c.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                             SensitivityLevel = context.TBL_CUSTOMER_SENSITIVITY_LEVEL.FirstOrDefault(x => x.CUSTOMERSENSITIVITYLEVELID == c.CUSTOMERSENSITIVITYLEVELID).DESCRIPTION,
                             businessUnitId = c.BUSINESSUNITID,
                             misCode = c.MISCODE,
                             timeUpdated = c.DATETIMEUPDATED,
                             businessUnitName = c.BUSINESSUNITID == null ? "N/A" : context.TBL_PROFILE_BUSINESS_UNIT.Where(o => o.BUSINESSUNITID == c.BUSINESSUNITID).Select(o => o.BUSINESSUNITNAME).FirstOrDefault(),
                             timeDeleted = c.DATETIMEDELETED
                         }).ToList();
    
            return staff;
        }

        public StaffInfoViewModel GetStaffCompairRecord(string searchString)
        {
            searchString = searchString.Trim().ToLower();

            var tempStaffRecord = (from c in context.TBL_TEMP_STAFF
                               where c.STAFFCODE.Trim() == searchString
                               

                               select new StaffInfoViewModel
                               {
                                 FirstName = c.FIRSTNAME,
                                 MiddleName = c.MIDDLENAME,
                                 LastName = c.LASTNAME,
                                 supervisorStaffName = context.TBL_STAFF.Where(o => o.STAFFID == c.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                 StaffCode = c.STAFFCODE, 
                                 staffRoleId = c.STAFFROLEID,
                                 staffRoleName = c.TBL_STAFF_ROLE.STAFFROLENAME,
                                 supervisorStaffId = c.SUPERVISOR_STAFFID,
                                 BranchId = c.BRANCHID,
                                 BranchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                 customerSensitivityLevelId = c.CUSTOMERSENSITIVITYLEVELID,
                                 Email = c.EMAIL,
                                 Gender = c.GENDER,
                                 Phone = c.PHONE,
                                 StateId = c.STATEID,
                                 CityId = c.CITYID,
                                 createdByName = context.TBL_STAFF.Where(o => o.STAFFID == c.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                 ApprovalStatusId = c.APPROVALSTATUSID,
                                 loanLimit = c.LOAN_LIMIT,
                                 workStartDuration = c.WORKSTARTDURATION,
                                 workEndDuration = c.WORKENDDURATION,
                                 businessUnitId = c.BUSINESSUNITID,
                                 approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == c.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                                 businessUnitName = context.TBL_PROFILE_BUSINESS_UNIT.Where(o => o.BUSINESSUNITID == c.BUSINESSUNITID).Select(o => o.BUSINESSUNITNAME).FirstOrDefault(),
                                 misCode  = c.MISCODE,
                               }).FirstOrDefault();
        return tempStaffRecord;
        }

        public List<CustomerViewModels> GetSingleCustomerGeneralInfo(string searchString)
        {
            searchString = searchString.Trim().ToLower();
            var data = (from a in context.TBL_CUSTOMER
                        where a.CUSTOMERCODE == searchString
                                   ||   a.FIRSTNAME.ToLower().Contains(searchString.ToLower())
                                   || a.LASTNAME.ToLower().Contains(searchString.ToLower())
                                   || a.MAIDENNAME.ToLower().Contains(searchString.ToLower())
                                   || a.CUSTOMERCODE.Contains(searchString)
                        select new CustomerViewModels
                        {
                            crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                            crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                            crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                            accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                            branchId = a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            companyMainId = a.COMPANYID,
                            createdBy = a.CREATEDBY,
                            lastUpdatedByName = a.LASTUPDATEDBY == null ? "N/A" : context.TBL_STAFF.Where(o => o.STAFFID == a.LASTUPDATEDBY).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                            creationMailSent = a.CREATIONMAILSENT,
                            customerCode = a.CUSTOMERCODE,
                            customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                            customerTypeId = (short)a.CUSTOMERTYPEID,
                            dateOfBirth = (DateTime)a.DATEOFBIRTH,
                            customerId = a.CUSTOMERID,
                            emailAddress = a.EMAILADDRESS,
                            firstName = a.FIRSTNAME,
                            gender = a.GENDER,
                            dateTimeCreated = a.DATETIMECREATED,
                            lastName = a.LASTNAME,
                            maidenName = a.MAIDENNAME,
                            maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : a.MARITALSTATUS.Value == 2 ? "F" : null,
                            title = a.TITLE,
                            middleName = a.MIDDLENAME,
                            customerTypeName = a.TBL_CUSTOMER_TYPE.NAME,
                            misCode = a.MISCODE,
                            misStaff = a.MISSTAFF,
                            nationalityId = a.NATIONALITYID,
                            occupation = a.OCCUPATION,
                            placeOfBirth = a.PLACEOFBIRTH,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            isRealatedParty = a.ISREALATEDPARTY,
                            isProspect = a.ISPROSPECT,
                             relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                            spouse = a.SPOUSE,
                            sectorId = a.TBL_SUB_SECTOR.TBL_SECTOR.SECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            subSectorId = (short)a.SUBSECTORID,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            taxNumber = a.TAXNUMBER,
                            prospectCustomerCode = a.PROSPECTCUSTOMERCODE,
                            customerRating = a.CUSTOMERRATING,
                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                            businessUnitId = a.BUSINESSUNTID,
                            businessUnitName = context.TBL_PROFILE_BUSINESS_UNIT.Where(o => o.BUSINESSUNITID == a.BUSINESSUNTID).Select(o => o.BUSINESSUNITNAME).FirstOrDefault(),
                            ownership = a.OWNERSHIP,
                            relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID).Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                            riskRatingName = a.TBL_CUSTOMER_RISK_RATING.RISKRATING,
                            customerBVN = a.CUSTOMERBVN,
                            nameofSignatories = a.NAMEOFSIGNATORY,
                            addressofSignatories = a.ADDRESSOFSIGNATORY,
                            phoneNumberofSignatories = a.PHONENUMBEROFSIGNATORY,
                            emailofSignatories = a.EMAILOFSIGNATORY,
                            bvnNumberofSignatories = a.BVNNUMBEROFSIGNATORY,
                            deleted = a.DELETED,
                            dateTimeUpdated = a.DATETIMEUPDATED,
                        }).ToList();
            
            return data;
        }

        public CustomerViewModels GetTempCustomerRecord(int customerId)
        {
            var tempCustomer = (from a in context.TBL_TEMP_CUSTOMER
                        where a.CUSTOMERID == customerId
                        select new CustomerViewModels
                        {
                            crmsRelationshipTypeId = a.CRMSRELATIONSHIPTYPEID,
                            crmsLegalStatusId = a.CRMSLEGALSTATUSID,
                            crmsCompanySizeId = a.CRMSCOMPANYSIZEID,
                            accountCreationComplete = a.ACCOUNTCREATIONCOMPLETE,
                            branchId = a.BRANCHID,
                            companyMainId = a.COMPANYID,
                            createdBy = a.CREATEDBY,
                            creationMailSent = a.CREATIONMAILSENT,
                            customerCode = a.CUSTOMERCODE,
                            customerSensitivityLevelId = a.CUSTOMERSENSITIVITYLEVELID,
                            customerTypeId = (short)a.CUSTOMERTYPEID,
                            dateOfBirth = (DateTime)a.DATEOFBIRTH,
                            customerId = a.CUSTOMERID,
                            emailAddress = a.EMAILADDRESS,
                            firstName = a.FIRSTNAME,
                            gender = a.GENDER,
                            customerTypeName = context.TBL_CUSTOMER_TYPE.Where(o => o.CUSTOMERTYPEID == a.CUSTOMERTYPEID).Select(o => o.NAME).FirstOrDefault(),
                            lastName = a.LASTNAME,
                            maidenName = a.MAIDENNAME,
                            maritalStatus = a.MARITALSTATUS.Value == 1 ? "M" : a.MARITALSTATUS.Value == 2 ? "F" : null,
                            title = a.TITLE,
                            middleName = a.MIDDLENAME,
                            misCode = a.MISCODE,
                            misStaff = a.MISSTAFF,
                            nationalityId = a.NATIONALITYID,
                            occupation = a.OCCUPATION,
                            placeOfBirth = a.PLACEOFBIRTH,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            isRealatedParty = a.ISREALATEDPARTY,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID.Value,
                            isCurrent = a.ISCURRENT,
                            spouse = a.SPOUSE,
                            subSectorId = (short)a.SUBSECTORID,
                            taxNumber = a.TAXNUMBER,
                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                            businessUnitId = a.BUSINESSUNTID,
                            businessUnitName = a.BUSINESSUNTID == null ? "N/A" : context.TBL_PROFILE_BUSINESS_UNIT.Where(o => o.BUSINESSUNITID == a.BUSINESSUNTID).Select(o => o.BUSINESSUNITNAME).FirstOrDefault(),
                            ownership = a.OWNERSHIP,
                            relationshipOfficerName = context.TBL_STAFF.Where(f => f.STAFFID == a.RELATIONSHIPOFFICERID)
                                .Select(f => f.FIRSTNAME + " " + f.FIRSTNAME).FirstOrDefault(),
                            approvalStatus= a.APPROVALSTATUSID,
                           approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        }).FirstOrDefault();

            return tempCustomer;
        }

        public List <CasaViewModel> GetCasaCustomerRecord(int customerId)
        {
            var casaCustomer = (from a in context.TBL_CASA
                                join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                where a.CUSTOMERID == customerId
                                select new CasaViewModel
                                {
                                    productAccountNumber = a.PRODUCTACCOUNTNUMBER,
                                    productAccountName = a.PRODUCTACCOUNTNAME,
                                    customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                                    customerId = a.CUSTOMERID,
                                    productId = a.PRODUCTID,
                                    productName = context.TBL_PRODUCT.Where(p => p.PRODUCTID == a.PRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault(),


                                }).ToList();

            return casaCustomer;
        }




        public bool UpdateCustomerRecord(int customerId, CustomerViewModels entity)
        {
            try
            {
                var customerTemp = context.TBL_TEMP_CUSTOMER.Where(x=> x.CUSTOMERID == customerId).FirstOrDefault();
                var customerMain = context.TBL_CUSTOMER.Find(customerId);
                
                if (customerMain != null && customerTemp !=null)
                {
                    TBL_TEMP_CUSTOMER customer = new TBL_TEMP_CUSTOMER();
                    customer.CUSTOMERCODE = customerTemp.CUSTOMERCODE;
                    customer.CUSTOMERTYPEID = customerTemp.CUSTOMERTYPEID;
                    customer.FIRSTNAME = customerTemp.FIRSTNAME;
                    customer.MIDDLENAME = customerTemp.MIDDLENAME;
                    customer.LASTNAME = customerTemp.LASTNAME;
                    customer.CUSTOMERID = customerTemp.CUSTOMERID;
                    customer.BRANCHID = customerTemp.BRANCHID;
                    customer.COMPANYID = customerTemp.COMPANYID;
                    customer.CUSTOMERSENSITIVITYLEVELID = customerTemp.CUSTOMERSENSITIVITYLEVELID;
                    customer.DATEOFBIRTH = customerTemp.DATEOFBIRTH;
                    customer.EMAILADDRESS = customerTemp.EMAILADDRESS;
                    customer.GENDER = customerTemp.GENDER;
                    customer.MAIDENNAME = customerTemp.MAIDENNAME;
                    customer.MARITALSTATUS = customerTemp.MARITALSTATUS;
                    customer.TITLE = customerTemp.TITLE;
                    customer.MISCODE = customerTemp.MISCODE;
                    customer.MISSTAFF = customerTemp.MISSTAFF;
                    customer.NATIONALITYID = customerTemp.NATIONALITYID;
                    customer.OCCUPATION = customerTemp.OCCUPATION;
                    customer.PLACEOFBIRTH = customerTemp.PLACEOFBIRTH;
                    customer.ISPOLITICALLYEXPOSED = customerTemp.ISPOLITICALLYEXPOSED;
                    customer.ISINVESTMENTGRADE = customerTemp.ISINVESTMENTGRADE;
                    customer.ISREALATEDPARTY = customerTemp.ISREALATEDPARTY;
                    customer.RELATIONSHIPOFFICERID = customerTemp.RELATIONSHIPOFFICERID;
                    customer.SPOUSE = customerTemp.SPOUSE;
                    customer.SUBSECTORID = customerTemp.SUBSECTORID;
                    customer.TAXNUMBER = customerTemp.TAXNUMBER;
                    customer.RISKRATINGID = customerTemp.RISKRATINGID;
                    customer.CUSTOMERBVN = customerTemp.CUSTOMERBVN;
                    customer.CREATEDBY = customerTemp.CREATEDBY;
                    customer.DATETIMECREATED = customerTemp.DATETIMECREATED;
                    customer.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    customer.ISCURRENT = entity.isCurrent;
                    customer.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                    customer.COUNTRYOFRESIDENTID = customerTemp.COUNTRYOFRESIDENTID;
                    customer.NUMBEROFDEPENDENTS = customerTemp.NUMBEROFDEPENDENTS;
                    customer.NUMBEROFLOANSTAKEN = customerTemp.NUMBEROFLOANSTAKEN;
                    customer.MONTHLYLOANREPAYMENT = customerTemp.MONTHLYLOANREPAYMENT;
                    customer.DATEOFRELATIONSHIPWITHBANK = customerTemp.DATEOFRELATIONSHIPWITHBANK;
                    customer.RELATIONSHIPTYPEID = customerTemp.RELATIONSHIPTYPEID;
                    customer.TEAMLDR = customerTemp.TEAMLDR;
                    customer.TEAMNPL = customerTemp.TEAMNPL;
                    customer.CORR = customerTemp.CORR;
                    customer.BUSINESSUNTID = customerTemp.BUSINESSUNTID;
                    customer.PASTDUEOBLIGATIONS = customerTemp.PASTDUEOBLIGATIONS;
                    context.TBL_TEMP_CUSTOMER.Add(customer);
                }

             
                //// Audit Section ----------------------------
                //var audit = new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                //    STAFFID = entity.createdBy,
                //    BRANCHID = (short)entity.userBranchId,
                //    DETAIL = "Updated TBL_CUSTOMER from support Utility: " + customerMain.FIRSTNAME + " with code: " + customerMain.CUSTOMERCODE +
                //             " on" + " (" + entity.customerId + ") ",
                //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                //    URL = entity.applicationUrl,
                //    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now,
                //    DEVICENAME = CommonHelpers.GetDeviceName(),
                //    OSNAME = CommonHelpers.FriendlyName()
                //};
                
                {
                   
                    customerMain.ISPROSPECT = entity.isProspect;
                    customerMain.ACCOUNTCREATIONCOMPLETE = entity.accountCreationComplete;
                   // this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    var output = context.SaveChanges() > 0;

                    return output;
                }
            }catch(Exception e)
            {
                throw e;
            }
        }


        public bool UpdateStaffRecord( int staffId, string staffCode, StaffInfoViewModel staffModel)
        {

           
            var staffMain = context.TBL_STAFF.Find(staffId);
            var staffTemp = context.TBL_TEMP_STAFF.Where(x => x.STAFFCODE == staffMain.STAFFCODE).FirstOrDefault();
            //var existingTempStaff = context.TBL_TEMP_STAFF.FirstOrDefault(x => x.STAFFCODE.ToLower() == staffModel.StaffCode.ToLower() && x.ISCURRENT == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            if (staffTemp != null && staffMain !=null)
            {
                TBL_TEMP_STAFF tempStaff = new TBL_TEMP_STAFF();
                tempStaff.FIRSTNAME = staffTemp.FIRSTNAME;
                tempStaff.MIDDLENAME = staffTemp.MIDDLENAME;
                tempStaff.LASTNAME = staffTemp.LASTNAME;
                tempStaff.STAFFCODE = staffTemp.STAFFCODE.ToLower().Trim();
                tempStaff.COMPANYID = staffTemp.COMPANYID;
                tempStaff.STAFFROLEID = staffTemp.STAFFROLEID;
                tempStaff.SUPERVISOR_STAFFID = staffTemp.SUPERVISOR_STAFFID;
                tempStaff.ADDRESS = staffTemp.ADDRESS;
                tempStaff.ADDRESSOFNOK = staffTemp.ADDRESSOFNOK;
                tempStaff.BRANCHID = staffTemp.BRANCHID;
                tempStaff.COMMENT_ = staffTemp.COMMENT_;
                tempStaff.CREATEDBY = staffTemp.CREATEDBY;
                tempStaff.CUSTOMERSENSITIVITYLEVELID = staffTemp.CUSTOMERSENSITIVITYLEVELID;
                tempStaff.DATEOFBIRTH = staffTemp.DATEOFBIRTH;
                tempStaff.DATETIMEUPDATED = DateTime.Now;
                tempStaff.EMAIL = staffTemp.EMAIL;
                tempStaff.EMAILOFNOK = staffTemp.EMAILOFNOK;
                tempStaff.GENDER = staffTemp.GENDER;
                tempStaff.GENDEROFNOK = staffTemp.GENDEROFNOK;
                tempStaff.MISINFOID = staffTemp.MISINFOID;
                tempStaff.NAMEOFNOK = staffTemp.NAMEOFNOK;
                tempStaff.NOKRELATIONSHIP = staffTemp.NOKRELATIONSHIP;
                tempStaff.PHONE = staffTemp.PHONE;
                tempStaff.PHONEOFNOK = staffTemp.PHONEOFNOK;
                tempStaff.STATEID = staffTemp.STATEID;
                tempStaff.CITYID = staffTemp.CITYID;
                tempStaff.STAFFSIGNATURE = staffTemp.STAFFSIGNATURE;
                tempStaff.APPROVALSTATUSID = staffModel.ApprovalStatusId;
                tempStaff.ISCURRENT = staffModel.isCurrent;
                tempStaff.LOAN_LIMIT = staffTemp.LOAN_LIMIT;
                tempStaff.WORKSTARTDURATION = staffTemp.WORKSTARTDURATION;
                tempStaff.WORKENDDURATION = staffTemp.WORKENDDURATION;
                tempStaff.BUSINESSUNITID = staffTemp.BUSINESSUNITID;
                tempStaff.MISCODE = staffTemp.MISCODE;
                context.TBL_TEMP_STAFF.Add(tempStaff);
            }

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.StaffUpdated,
            //    STAFFID = staffModel.createdBy,
            //    BRANCHID = (short)staffModel.BranchId,
            //    DETAIL = $"Updated Staff '{staffModel.StaffFullName}' with code '{staffModel.StaffCode}' from support utility",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = staffModel.applicationUrl,
            //    APPLICATIONDATE = _genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName(),
            //};


                    var output = context.SaveChanges() > 0;
                       
                        context.SaveChanges();

                {
                    return output;
                }
            }
          
        }
    }


