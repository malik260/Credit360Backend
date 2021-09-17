using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.SupportUtility;
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

        public List<WorkflowSupportUtilityViewModel> GetApprovalTrail(string searchString, int staffId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //var now = DateTime.Now;
                //TBL_STAFF_RELIEF relieverStaff = new TBL_STAFF_RELIEF();
                //var relifestaff = 0;
                //if (staffId != 0)
                //{
                //    relieverStaff = context.TBL_STAFF_RELIEF
                //           .FirstOrDefault(x => x.DELETED == false
                //               && x.STAFFID == staffId
                //               && x.STARTDATE <= now
                //               && x.ENDDATE >= now
                //               && x.ISACTIVE == true
                //           );
                //    if (relieverStaff != null)
                //    {
                //        relifestaff = relieverStaff.RELIEFSTAFFID;
                //    }
                //}
                //else
                //{
                //    relifestaff = 0;
                //}


                int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CreditAppraisal,(int)OperationsEnum.CreditCardsCashBacked,
                     (int)OperationsEnum.CreditCardsCleanCards, (int)OperationsEnum.AdhocApproval,
                    (int)OperationsEnum.CreditCardsSalaryBacked, (int)OperationsEnum.TemporaryOverdraftRequest, (int)OperationsEnum.CashCollaterizedRequest };

                //int[] operationsLms = {
                //    (int)OperationsEnum.ContigentLoanBooking ,
                //   (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
                //    (int)OperationsEnum.CommercialLoanBooking};


                searchString = searchString.Trim().ToLower();


                var applications = (from x in context.TBL_LOAN_APPLICATION
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                    join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                    let bizUnit = context.TBL_PROFILE_BUSINESS_UNIT.FirstOrDefault(u => u.BUSINESSUNITID == c.BUSINESSUNTID)
                                    let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.OWNEDBY)
                                    let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == x.FLOWCHANGEID)
                                    //join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
                                    where
                               //y.RESPONSESTAFFID == null
                               // && operations.Contains(y.OPERATIONID)
                               //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
                               // && 
                               (x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                            || c.FIRSTNAME.ToLower().Contains(searchString)
                            || c.LASTNAME.ToLower().Contains(searchString)
                            || c.MIDDLENAME.ToLower().Contains(searchString)
                            || bizUnit.BUSINESSUNITSHORTCODE.ToLower().Contains(searchString)
                            || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                    select new WorkflowSupportUtilityViewModel
                                    {
                                        firstName = c.FIRSTNAME,
                                        middleName = c.MIDDLENAME,
                                        lastName = c.LASTNAME,
                                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                        customerCode = c.CUSTOMERCODE,
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        productName = p.PRODUCTNAME,
                                        customerId = c.CUSTOMERID,
                                        customerGroupId = x.CUSTOMERGROUPID,
                                        loanTypeId = x.LOANAPPLICATIONTYPEID,
                                        applicationDate = x.APPLICATIONDATE,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        //bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                        //bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                        //applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        productClassId = x.PRODUCTCLASSID,
                                        productId = (short)(x.PRODUCTID ?? 0),
                                        //loanInformation = x.LOANINFORMATION,
                                        //productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                        customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                        isRelatedParty = x.ISRELATEDPARTY,
                                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                        isInvestmentGrade = x.ISINVESTMENTGRADE,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        //currentApprovalLevel = context.TBL_APPROVAL_TRAIL.Where(o=>o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e=>e.FROMAPPROVALLEVELID != null ? e.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a").OrderByDescending(r => r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                        //approvalTrailId = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e => e.APPROVALTRAILID).OrderByDescending(r => r).FirstOrDefault(),//y.APPROVALTRAILID,
                                        //responsiblePerson = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(y => y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME).OrderByDescending(r=>r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

                                        //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                        createdBy = x.OWNEDBY,
                                        operationId = x.OPERATIONID,
                                        oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),
                                        // accountNumber = ca.PRODUCTACCOUNTNUMBER,
                                        //apiRequestId = x.APIREQUESTID,
                                    }).ToList();

                var groupApplications = (from x in context.TBL_LOAN_APPLICATION
                                         join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                         join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                                         join c in context.TBL_CUSTOMER_GROUP on x.CUSTOMERGROUPID equals c.CUSTOMERGROUPID
                                         let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.OWNEDBY)
                                         let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == x.FLOWCHANGEID)
                                         //join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
                                         where
                                    //y.RESPONSESTAFFID == null
                                    // && operations.Contains(y.OPERATIONID)
                                    //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
                                    // && 
                                    (x.APPLICATIONREFERENCENUMBER == searchString
                                 || c.GROUPNAME.ToLower().Contains(searchString)
                                 || c.GROUPCODE.ToLower().Contains(searchString)
                                 || c.GROUPDESCRIPTION.ToLower().Contains(searchString)
                                 || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                         select new WorkflowSupportUtilityViewModel
                                         {
                                             //firstName = c.FIRSTNAME,
                                             //middleName = c.MIDDLENAME,
                                             //lastName = c.LASTNAME,
                                             customerName = c.GROUPNAME,
                                             customerCode = c.GROUPCODE,
                                             applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                             loanApplicationId = x.LOANAPPLICATIONID,
                                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                             productName = p.PRODUCTNAME,
                                             //customerId = null,
                                             //bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                             //bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                             customerGroupId = x.CUSTOMERGROUPID,
                                             loanTypeId = x.LOANAPPLICATIONTYPEID,
                                             
                                             applicationDate = x.APPLICATIONDATE,
                                             applicationAmount = x.APPLICATIONAMOUNT,
                                             approvedAmount = x.APPROVEDAMOUNT,
                                             productClassId = x.PRODUCTCLASSID,
                                             productId = (short)(x.PRODUCTID ?? 0),
                                             

                                             //productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                             //submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                             customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                             isRelatedParty = x.ISRELATEDPARTY,
                                             isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                             isInvestmentGrade = x.ISINVESTMENTGRADE,
                                             
                                             approvalStatusId = (short)x.APPROVALSTATUSID,
                                             approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                             

                                             //currentApprovalLevel = context.TBL_APPROVAL_TRAIL.Where(o=>o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e=>e.FROMAPPROVALLEVELID != null ? e.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a").OrderByDescending(r => r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                             //approvalTrailId = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e => e.APPROVALTRAILID).OrderByDescending(r => r).FirstOrDefault(),//y.APPROVALTRAILID,
                                             //responsiblePerson = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(y => y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME).OrderByDescending(r=>r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

                                             //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                             applicationStatusId = x.APPLICATIONSTATUSID,
                                             applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                            
                                             customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                             loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                             createdBy = x.OWNEDBY,
                                            
                                             operationId = x.OPERATIONID,
                                             oprationName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),

                // accountNumber = ca.PRODUCTACCOUNTNUMBER, 

            }).ToList();
             
                var allRecord = applications.Union(groupApplications).GroupBy(a => a.loanApplicationId).Select(a => a.FirstOrDefault()).ToList();
                //int[] bookedLoanOperations = { (int) OperationsEnum.RevolvingLoanBooking, (int) OperationsEnum.TermLoanBooking, (int) OperationsEnum.ContigentLoanBooking };

            //    foreach (var x in allRecord)
            //    {
            //        x.operationName = GetWorkFlowName(x.operationId.Value, x.productClassId, x.productId);
            //        var appRecord = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.loanApplicationId && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
            //        if (appRecord != null)
            //        {
            //            var singleRec = appRecord;
            //            var status = context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == x.applicationStatusId).APPLICATIONSTATUSNAME;
            //            if (singleRec.FROMAPPROVALLEVELID == singleRec.TOAPPROVALLEVELID)
            //            {
            //                if (singleRec.LOOPEDSTAFFID > 0)
            //                {
            //                    var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == singleRec.LOOPEDSTAFFID);
            //                    x.currentApprovalLevel = context.TBL_STAFF_ROLE.FirstOrDefault(r => r.STAFFROLEID == staff.STAFFROLEID).STAFFROLENAME;
            //                    x.responsiblePerson = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;

            //                }
            //                else
            //                {
            //                    x.currentApprovalLevel = singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME;
            //                    x.responsiblePerson = singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;
            //                }
            //            }
            //            else
            //            {
            //                x.currentApprovalLevel = singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "CLICK VIEW FOR DRAWDOWN DETAILS" : x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationRejected ? "REJECTED APPLICATION" : status; // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
            //                x.responsiblePerson = singleRec.TOSTAFFID == null ? (singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : (x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : (x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "CLICK VIEW FOR DRAWDOWN DETAILS" : x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationRejected ? "REJECTED APPLICATION" : status))) : singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
            //            }
            //            x.currentApprovalLevelId = singleRec.TOAPPROVALLEVELID > 0 ? singleRec.TOAPPROVALLEVELID : 0;
            //            x.approvalTrailId = singleRec.APPROVALTRAILID;//y.APPROVALTRAILID,
            //            //x.responsiblePerson = singleRec.TOSTAFFID == null ? singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a" : singleRec.TBL_STAFF1.STAFFCODE + " - " + singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

            //            x.toStaffId = singleRec.TOSTAFFID;
            //            x.currentOperationId = singleRec.OPERATIONID;
            //        }
            //        else
            //        {
            //            if (x.bookingRequestId > 0 || x.isSkipAppraisalEnabled)
            //            {
            //                x.currentApprovalLevel = "CLICK VIEW FOR DRAWDOWN DETAILS";
            //                x.responsiblePerson = "CLICK VIEW FOR DRAWDOWN DETAILS";
            //            }
            //            else
            //            {
            //                x.currentApprovalLevel = x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : "NOT YET IN APPRAISAL";
            //                x.responsiblePerson = x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : "WITH ACCOUNT OFFICER";
            //            }
            //        }
            //        VerifyDrawDownStatus(x);
            //    }

             return allRecord;
            }
        }

    }
}
