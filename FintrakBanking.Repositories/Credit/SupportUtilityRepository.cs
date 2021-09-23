using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
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
        public SupportUtilityRepository(FinTrakBankingContext _context)
        {
            context = _context;
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

        public IEnumerable<BusinessRuleViewModel> GetBusinessRule( int approvalLevelId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                return from a in context.TBL_APPROVAL_LEVEL
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
                       };

               
            }
        }

        public List<WorkflowSupportUtilityViewModel> GetApprovalTrail(string searchString)
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

                var applications = (from x in context.TBL_LOAN_APPLICATION
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                    join p in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals p.LOANAPPLICATIONDETAILID
                                    join c in context.TBL_APPROVAL_TRAIL on p.LOAN_BOOKING_REQUESTID equals c.TARGETID
                                    join cu in context.TBL_CUSTOMER on a.CUSTOMERID equals cu.CUSTOMERID
                                    join pr in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals pr.PRODUCTID
                                    where
                                    approvalLevelIds.Contains(c.APPROVALTRAILID)
                                    select new WorkflowSupportUtilityViewModel
                                    {
                                        customerName = context.TBL_CUSTOMER.Where(z => z.CUSTOMERID == a.CUSTOMERID).Select(z => z.LASTNAME + " " + z.FIRSTNAME + " " + z.MIDDLENAME).FirstOrDefault(),
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        productName = pr.PRODUCTNAME,
                                        customerGroupId = x.CUSTOMERGROUPID,
                                        loanTypeId = x.LOANAPPLICATIONTYPEID,
                                        applicationDate = x.APPLICATIONDATE,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        productClassId = x.PRODUCTCLASSID,
                                        productId = (short)(x.PRODUCTID ?? 0),
                                        customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                        isRelatedParty = x.ISRELATEDPARTY,
                                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                        isInvestmentGrade = x.ISINVESTMENTGRADE,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                        createdBy = x.OWNEDBY,
                                        operationId = c.OPERATIONID,
                                        oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == c.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                    }).ToList();


                return applications;
            }
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
    }
}
