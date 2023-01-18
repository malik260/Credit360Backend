using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel;

namespace FintrakBanking.Repositories.WorkFlow
{
    public class JobRequestRepository : IJobRequestRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IFinanceTransactionRepository financeTransaction;
        private IGeneralSetupRepository general;
        private DepartmentRepository department;
        private IAuditTrailRepository audit;
        private IChartOfAccountRepository chartOfAccount;

        public JobRequestRepository(FinTrakBankingDocumentsContext docContext, FinTrakBankingContext _context, IGeneralSetupRepository _general,
            IAuditTrailRepository _audit, DepartmentRepository _department, IFinanceTransactionRepository _financeTransaction,
            IChartOfAccountRepository _chartOfAccount)
        {
            this.context = _context;
            this.docContext = docContext;
            this.general = _general;
            this.audit = _audit;
            this.department = _department;
            this.financeTransaction = _financeTransaction;
            this.chartOfAccount = _chartOfAccount;
        }

       
        private string saveJobRequest(JobRequestViewModel model)
        {
            if(model.jobSourceId == null || model.jobSourceId <= 0) { throw new ConditionNotMetException("Could not resolve job source. Please contact admin."); }
                if (model.operationsId == 0)
            {
                throw new ConditionNotMetException("Job request could not resolve the operation.");
            }
            var applicationDate = general.GetApplicationDate();
            var date = DateTime.Now;
            var data = new TBL_JOB_REQUEST
            {
                JOBREQUESTCODE = model.jobRequestCode,
                JOBTYPEID = model.jobTypeId,
                JOB_TITLE = model.requestTitle,
                SENDERSTAFFID = model.createdBy,
                JOB_SUB_TYPEID = model.jobSubTypeId,
                RECEIVERSTAFFID = model.receiverStaffId == 0 ? null : model.receiverStaffId,
                JOBTYPEUNITID = model.departmentUnitId,
                REASSIGNEDTO = model.reassignedTo,
                ISREASSIGNED = model.isReassigned,
                ISACKNOWLEDGED = model.isAcknowledged,
                TARGETID = model.targetId,
                OPERATIONSID = model.operationsId,
                REQUESTSTATUSID = model.requestStatusId,
                SENDERCOMMENT = model.senderComment,
                RESPONSECOMMENT = model.responseComment,
                ARRIVALDATE = applicationDate,
                SYSTEMARRIVALDATE = date,
                BRANCHID = model.branchId,
                JOBSOURCEID = model.jobSourceId 
            };
            try
            {
                var job = context.TBL_JOB_REQUEST.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.JobRequestAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added JobRequest '{ model.jobRequestCode }' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = applicationDate,
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------
                if (context.SaveChanges() > 0)
                {
                    return job.JOBREQUESTCODE;
                }
                else return string.Empty;
            }
            catch(Exception ex)
            {
                throw new Exception("An error occured while saving records.");
            }
        }

        public string AddGlobalJobRequest(JobRequestViewModel model)
        {
            if (model.receiverStaffId == model.createdBy)
                throw new SecureException("You cannot assign a job to yourself");

            model.requestStatusId = (short)RequestStatusEnum.Pending;

            if (model.isApplicationLevel)
            {
                var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == model.targetId);
                if (!applicationDetail.Any())
                    throw new ConditionNotMetException("Targeted for this record could not be resolved");

                var code = string.Empty;
                int ctr = 0;
                foreach (var item in applicationDetail)
                {
                    model.jobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode();
                    code = saveJobRequest(model);
                    ctr = ctr + 1;
                }
                if (ctr > 1) { code = code + "..."; }
                return code;
            }
            else
            {
                model.jobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode();
                return saveJobRequest(model);
            }
        }

        public bool AddJobComment(JobRequestMessageViewModel model)
        {
            var data = new TBL_JOB_REQUEST_MESSAGE
            {
                JOBREQUESTID = model.jobRequestId,
                MESSAGE = model.message,
                DATE_TIME_SENT = DateTime.Now,
                STAFFID = model.createdBy
            };

            var job = context.TBL_JOB_REQUEST_MESSAGE.Add(data);
            return context.SaveChanges() > 0;
        }

        private string RequestCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        public bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId)
        {
            var data = this.context.TBL_JOB_REQUEST.Find(jobRequestId);
            if (data == null)
            {
                return false;
            }

            var applicationDate = general.GetApplicationDate();

            data.ISACKNOWLEDGED = true;
            data.REQUESTSTATUSID = (short)model?.statusId;
            if(model.rejectionReasonId != null)data.JOB_STATUS_FEEDBACKID = (short)model.rejectionReasonId;
            data.RESPONSECOMMENT = model.responseComment;
            data.RESPONSEDATE = applicationDate;
            data.RESPONSESTAFFID = model.createdBy;
            data.SYSTEMRESPONSEDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Reply JobRequest '{ data.JOBREQUESTCODE }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool ReRouteJobRequest(JobRequestViewModel model)
        {
            var applicationDate = general.GetApplicationDate();
            var data = this.context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            if (data == null)
            {
                return false;
            }
            var existingUnit = context.TBL_JOB_TYPE_UNIT.Find(data.JOBTYPEUNITID);
            var newUnit = context.TBL_JOB_TYPE_UNIT.Find(model.jobTypeUnitId);

            if (existingUnit == null || newUnit == null) throw new ConditionNotMetException("Missing unit. Contact admin.");

            data.JOBTYPEUNITID = model.jobTypeUnitId;
            //data.REASSIGNEDTO = null;
            //data.RECEIVERSTAFFID = null;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Job request with code '{ data.JOBREQUESTCODE }' was re-routed from '{existingUnit.UNITNAME}' to '{newUnit.UNITNAME}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;
        }

        public bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId)
        {
            var data = context.TBL_JOB_REQUEST.Find(jobRequestId);
            if (data == null)
            {
                return false;
            }

            if (data.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved)
                throw new BadLogicException("You cannot assign/reassign this job. Job has already been responded and/or closed.");

            var toStaffData = context.TBL_STAFF.Find(model.reassignedTo);
            var toStaffCode = string.Empty;
            var toStaffName = string.Empty;

            TBL_STAFF fromStaffData = new TBL_STAFF();
            if (data.REASSIGNEDTO != null)
            {
                fromStaffData = context.TBL_STAFF.Find(data.REASSIGNEDTO);
            }
            else if(data.RECEIVERSTAFFID != null)
            {
                fromStaffData = context.TBL_STAFF.Find(data.RECEIVERSTAFFID);
            }

            var fromStaffName = string.Empty;
            var info = string.Empty;

            if (toStaffData != null)
            {
                toStaffCode = toStaffData.STAFFCODE;
                toStaffName = toStaffData.FIRSTNAME + " " + toStaffData.MIDDLENAME + " " + toStaffData.LASTNAME;
            }
                

            if (fromStaffData != null){
                if (toStaffCode == fromStaffData.STAFFCODE)
                    throw new ConditionNotMetException("This job is currently assigned to the selected staff. Choose another staff staff to reassign.");

                info = $"Reassigned JobRequest with code '{ model.jobRequestCode }' from staff with code '{fromStaffData.STAFFCODE}' to staff with code '{toStaffCode}'";
            }
            else { info = $"Assigned JobRequest with code '{ model.jobRequestCode }' to staff '<strong>{toStaffName}</strong>' with code '<strong>{toStaffCode}</strong>'"; }
                

            var applicationDate = general.GetApplicationDate();

            var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Find(data.TARGETID);


            if (data.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification)
            {
                var hubCordinatorStaffId = context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification).FirstOrDefault();
                var hubCordinatorFullStaffData = hubCordinatorStaffId != null ? context.TBL_STAFF.Find(hubCordinatorStaffId.STAFFID) : null;

                var from = fromStaffData != null ? fromStaffData.FIRSTNAME + " " + fromStaffData.LASTNAME + " (" + fromStaffData.STAFFCODE + ")" : "None";
                var to = toStaffData != null ? toStaffData.FIRSTNAME + " " + toStaffData.LASTNAME + " (" + toStaffData.STAFFCODE + ")" : "None";

                string messageBoby = $"Attention!, <br /><br />Please note that a job request assignmnet/reassignment occured with the following details:<br /><br /> 'Job Request Code:' " +
                    $" {data.JOBREQUESTCODE} <br /><br /> Previously Assigned Staff: {from} " +
                    $"<br /><br />  Current Staff Assigned: {to} <br /><br />  Regards. ";
                string alertSubject = $"REQUEST FOR DOCUMENT VERIFICATION";


                if (loanDetails != null)
                {
                    var customerInfo = context.TBL_CUSTOMER.Find(loanDetails.CUSTOMERID);
                    var facilityInfo = context.TBL_PRODUCT.Find(loanDetails.APPROVEDPRODUCTID);
                    var currencyInfo = context.TBL_CURRENCY.Find(loanDetails.CURRENCYID);
                    var invoiceInfo = context.TBL_LOAN_APPLICATION_DETL_INV.Where(x=>x.LOANAPPLICATIONDETAILID == loanDetails.LOANAPPLICATIONDETAILID);
                    var casaInfo = context.TBL_CASA.Find(loanDetails.CASAACCOUNTID);

                    var accountLine = casaInfo != null ? $"<br /><br /> <strong>Account Number:</strong>  {casaInfo.PRODUCTACCOUNTNUMBER} " : null;
                    var customerNameLine = $"<br /><br /> <strong>Customer Name:</strong>  {customerInfo.FIRSTNAME}  {customerInfo.LASTNAME}  ";
                    var loantTypeLine = facilityInfo != null ? $"<br /><br /> <strong>Facility Type:</strong>  {facilityInfo.PRODUCTNAME} " : null;
                    var applicationRef = loanDetails.TBL_LOAN_APPLICATION != null ? $"<br /><br /> <strong>Application Reference:</strong>  {loanDetails.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER} " : null;
                    var accountNumberLine = casaInfo != null ? $"<br /><br /> <strong>Account Number:</strong>  {casaInfo.PRODUCTACCOUNTNUMBER} " : null;
                    var currencyTypeLine = currencyInfo != null ? $"<br /><br /> <strong>Currency:</strong>  {currencyInfo.CURRENCYCODE} " : null;

                    var principalNameLine = invoiceInfo.FirstOrDefault() != null  ? $"<br /><br /> <strong>Principal Name:</strong>  {invoiceInfo.FirstOrDefault().TBL_LOAN_PRINCIPAL.NAME} " : null;
                    var rmCommentLine = $"<br /><br /><br /> kindly proceed with verification and provide timely feedback ";
                    var NumberLine = string.Empty;

                    var numbers = string.Empty;
                    decimal amount = 0;
                    if (invoiceInfo != null)
                    {   
                        foreach (var item in invoiceInfo)
                        {
                            numbers = numbers + item.PURCHASEORDERNUMBER + "/" + item.CONTRACTNO + "/" + item.INVOICENO ;
                            amount = amount + item.INVOICE_AMOUNT;
                        }
                    }
                    var poAmountLine = invoiceInfo.FirstOrDefault() != null ? $"<br /><br /> <strong>Total amount on PO/Contract/Invoice:</strong>  {string.Format("{0:#,0.00}", amount) }  " : null;
                    var poNumber = invoiceInfo.FirstOrDefault() != null ? $"<br /><br />  <strong>PO/Contract/Invoice No:</strong>  {numbers }  " : null;
                    messageBoby = $"Attention! <br /><br />Please note that a job has been assigned with the following details:" +
                           $"{ principalNameLine} " +
                           $"{ customerNameLine} " +
                           $"{ accountLine} " +
                           $"{ loantTypeLine} " +
                           $"{applicationRef}" +
                           $"{ currencyTypeLine} " +
                           $"{ NumberLine} " +
                           $"{ poNumber} " +
                           $"{ poAmountLine} " +
                           $"{ rmCommentLine} .";
                    messageBoby = messageBoby + "<br/><br/>";
                }
                
                if (hubCordinatorFullStaffData != null)
                    LogEmailAlert(messageBoby, alertSubject, hubCordinatorFullStaffData.EMAIL, data.JOBREQUESTCODE, data.JOBREQUESTID);

                var hubTeamLeadEntry = context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.JOBTYPEUNITID == data.JOBTYPEUNITID && x.ISTEAMLEAD == true).FirstOrDefault();
                if(hubTeamLeadEntry != null && fromStaffData != null)
                {
                    var teamLeadStaff = context.TBL_STAFF.Find(hubTeamLeadEntry.STAFFID);
                    LogEmailAlert(messageBoby, alertSubject, teamLeadStaff.EMAIL, data.JOBREQUESTCODE, data.JOBREQUESTID);
                }
                if (toStaffData != null)
                {
                    var reciverStaff = context.TBL_STAFF.Find(toStaffData.STAFFID);
                    LogEmailAlert(messageBoby, alertSubject, reciverStaff.EMAIL, data.JOBREQUESTCODE, data.JOBREQUESTID);
                }
                var verificationOfficer = context.TBL_STAFF.Find(model.createdBy); 
                if(verificationOfficer != null)
                {
                    LogEmailAlert(messageBoby, alertSubject, verificationOfficer.EMAIL, data.JOBREQUESTCODE, data.JOBREQUESTID);
                }
            }

            data.REASSIGNEDTO = (int)model.reassignedTo;
            data.ISREASSIGNED = true;
            data.ISACKNOWLEDGED = true;
            data.REASSIGNEDDATE = applicationDate;
            data.SYSTEMREASSIGNEDDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = info,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool AcknowledgeJob(JobRequestViewModel entity, int jobRequestId)
        {
            var data = context.TBL_JOB_REQUEST.Find(jobRequestId);
            if (data == null)
                return false;

            if (data.ISACKNOWLEDGED )
                throw new BadLogicException("Job already ackowledged.");

            var applicationDate = general.GetApplicationDate();

            data.ISACKNOWLEDGED = true;
            data.ACKNOWLEDGEMENTDATE = DateTime.Now;

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ApplicationJobRequest> GetLoanApplicationJobsById(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        where a.TBL_LOAN_APPLICATION.COMPANYID == companyId && a.DELETED == false
                        && a.LOANAPPLICATIONID == loanApplicationId
                        select new ApplicationJobRequest
                        {
                            approvedAmount = a.APPROVEDAMOUNT,
                            currencyId = a.CURRENCYID,
                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerId = a.CUSTOMERID,
                            exchangeRate = a.EXCHANGERATE,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            subSectorId = a.SUBSECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "/" + a.TBL_SUB_SECTOR.NAME,
                            branchName = a.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHNAME,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            proposedAmount = a.PROPOSEDAMOUNT,
                            proposedTenor = a.PROPOSEDTENOR,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = a.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = a.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            relationshipOfficerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPMANAGERID,
                            dateTimeCreated = a.TBL_LOAN_APPLICATION.APPLICATIONDATE
                        }).ToList();
            foreach (var i in data)
            {
                var relationshipOfficer = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipOfficerId).FirstOrDefault();
                var relationshipManager = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipManagerId).FirstOrDefault();
                i.relationshipOfficerName = relationshipOfficer.FIRSTNAME + " " + relationshipOfficer.MIDDLENAME + " " + relationshipOfficer.LASTNAME;
                i.relationshipManagerName = relationshipManager.FIRSTNAME + " " + relationshipManager.MIDDLENAME + " " + relationshipManager.LASTNAME;
                var invoiceDiscountDetail = (from ap in context.TBL_LOAN_APPLICATION_DETL_INV.Where(x => x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)
                                             select new LoanApplicationDetailInvoiceViewModel
                                             {
                                                 approvalComment = ap.APPROVAL_COMMENT,
                                                 invoiceAmount = ap.INVOICE_AMOUNT,
                                                 invoiceNo = ap.INVOICENO,
                                                 approvaStatusId = ap.APPROVALSTATUSID,
                                                 contractEndDate = ap.CONTRACT_ENDDATE,
                                                 contractStartDate = ap.CONTRACT_STARTDATE,
                                                 invoiceDate = ap.INVOICE_DATE,
                                                 invoiceCurrencyCode = ap.TBL_CURRENCY.CURRENCYCODE,
                                                 principalName = ap.TBL_LOAN_PRINCIPAL.NAME,
                                                 principalAccount = ap.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                 principalRegNo = ap.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                 principalId = ap.PRINCIPALID,
                                             }).ToList();
                var firstEducationtDetail = (from ed in context.TBL_LOAN_APPLICATION_DETL_EDU.Where(x => x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)
                                             select new EducationLoanViewModel
                                             {
                                                 educationId = ed.EDUCATIONID,
                                                 loanApplicationDetailId = ed.LOANAPPLICATIONDETAILID,
                                                 numberOfStudent = ed.NUMBER_OF_STUDENTS,
                                                 averageSchoolFees = ed.AVERAGE_SCHOOL_FEES,
                                                 totalPreviousTermSchoolFees = ed.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                                 productClassId = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == ed.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                                 productClassName = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == ed.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                             }).ToList();
                var firstTradderDetail = (from tr in context.TBL_LOAN_APPLICATION_DETL_TRA.Where(x => x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)
                                          select new TraderLoanViewModel
                                          {
                                              tradderId = tr.TRADDERID,
                                              marketId = tr.MARKETID,
                                              marketName = tr.TBL_LOAN_MARKET.MARKETNAME,
                                              averageMonthlyTurnover = tr.AVERAGE_MONTHLY_TURNOVER,
                                              loanApplicationDetailId = tr.LOANAPPLICATIONDETAILID,
                                          }).ToList();

                i.invoiceDiscountDetail = invoiceDiscountDetail;
                i.firstEducationtDetail = firstEducationtDetail;

            }
            return data;
        }

        private IEnumerable<JobRequestViewModel> GetAllGlobalJobRequest(int staffId, bool isFilter = false)
        {
            var thisStaff = this.context.TBL_STAFF.Find(staffId);
            var staffAdmin = this.context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);
            var staffHub = this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
            var middleOfficeUnit = from x in context.TBL_JOB_TYPE_UNIT
                                   join t in context.TBL_JOB_TYPE_HUB_STAFF on x.JOBTYPEUNITID equals t.JOBTYPEUNITID
                                   where x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && t.STAFFID == staffId
                                   select x;

            bool isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault();
            List<JobRequestViewModel> allData = new List<JobRequestViewModel>();

            List<int> adminJobTypeIds = new List<int>();
            if (staffAdmin.Any())
            {
                foreach (var i in staffAdmin)
                {
                    adminJobTypeIds.Add(i.JOBTYPEID);
                }

            }
            List<int> unitIds = new List<int>();
            foreach (var i in staffHub)
            {
                unitIds.Add(i.JOBTYPEUNITID);
            }
                allData = (from x in context.TBL_JOB_REQUEST
                       join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                       join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                       where ((x.SENDERSTAFFID == staffId) || (x.RECEIVERSTAFFID == staffId) || (x.REASSIGNEDTO == staffId))
                       || ((unitIds.Contains((int)x.JOBTYPEUNITID)) && !middleOfficeUnit.Any())
                       || adminJobTypeIds.Contains(x.JOBTYPEID)
                       orderby x.JOBREQUESTID descending
                       select (
                      new JobRequestViewModel
                      {
                          jobRequestId = x.JOBREQUESTID,
                          requestTitle = x.JOB_TITLE,
                          jobRequestCode = x.JOBREQUESTCODE,
                          targetId = x.TARGETID,
                          jobTypeId = t.JOBTYPEID,
                          jobSubTypeId = s.JOB_SUB_TYPEID,
                          jobTypeName = t.JOBTYPENAME,
                          requireCharge = s.REQUIRECHARGE ?? false,
                          chargeFeeId = s.CHARGEFEEID ?? 0,
                          jobSubTypeName = s.JOB_SUB_TYPE_NAME,
                          
                          senderStaffId = x.SENDERSTAFFID,
                          senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                          senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE != null ? x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE : string.Empty,


                          receiverStaffId = (int)x.RECEIVERSTAFFID,
                          reassignedTo = x.REASSIGNEDTO,
                          isReassigned = x.ISREASSIGNED,
                          isAcknowledged = x.ISACKNOWLEDGED,
                          operationsId = x.OPERATIONSID,
                          operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                          requestStatusId = x.REQUESTSTATUSID,
                          requestStatusname = x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved ? "Completed" : x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                          senderComment = x.SENDERCOMMENT,
                          responseComment = x.RESPONSECOMMENT,
                          arrivalDate = x.ARRIVALDATE,
                          systemArrivalDate = x.SYSTEMARRIVALDATE,
                          reassignedDate = x.REASSIGNEDDATE,
                          systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                          responseDate = x.RESPONSEDATE,
                          systemResponseDate = x.SYSTEMRESPONSEDATE,
                          acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                          systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                          loggedInStaffId = staffId,
                          jobTypeUnitId = x.JOBTYPEUNITID,
                          jobTypeHubId = x.JOBTYPEHUBID,
                          jobSourceId = x.JOBSOURCEID,
                          branchId = x.BRANCHID,
                          sourceRegionName = context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).Any()
                                                   ? context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_BRANCH_REGION.REGION_NAME : "n/a",
                          sourceBranchCode = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                   ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHCODE : "n/a",
                          sourceBranchName = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                   ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",
                          isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault(),

                          //refNo = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == x.TARGETID) != null
                                                  // ? context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.TARGETID).FirstOrDefault().TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : "n/a",

                          fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                          fromBranchName = (from y in context.TBL_BRANCH.Where(i => i.BRANCHID == x.BRANCHID) select y.BRANCHNAME).FirstOrDefault(),
                          to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                          assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

                      })).Take(100).ToList();



            allData = setCustomerName(allData);

            allData = setJobRequestOtherDetails(allData);

            allData = PadModelWithLMSReference(allData);


            return allData;
        }

        private List<JobRequestViewModel> setJobRequestOtherDetails(List<JobRequestViewModel> models)
        {
            foreach (var item in models)
            {
                 
                var detail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == item.jobRequestId);
                if (detail.Any())
                {
                    item.hasLegalRecommendedSearch = true;
                    if (detail.FirstOrDefault().CUSTOMERORBUSINESSCHARGED == true)
                        item.customerCharged = true;

                    if (detail.FirstOrDefault().ACCREDITEDCONSULTANTPAID)
                        item.consultantPaid = true;
                };

                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.MiddleOfficeVerification) item.jobSubTypeName = "MO Verification";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfTreasuryBills) item.jobSubTypeName = "Treasury Bill confirm..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.CreditRisk) item.jobSubTypeName = "Credit Risk..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfDealSlip) item.jobSubTypeName = "Deal Slip confirm..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfStock) item.jobSubTypeName = "Stock confirmation";
                if (item.jobSubTypeId == null || item.jobSubTypeId < 1) item.jobSubTypeName = "n/a";

                var jobStatusFeedBackRecord = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Where(x => x.JOB_STATUS_FEEDBACKID == item.jobStatusFeedBackId).FirstOrDefault();
                if (jobStatusFeedBackRecord != null) item.jobStatusFeedBack = jobStatusFeedBackRecord.JOB_STATUS_FEEDBACK_NAME;
            }
            return models;
        }

        private List<JobRequestViewModel> setCustomerName(List<JobRequestViewModel> models)
        {
            foreach(var model in models)  //(applicationDet != null && model.jobSourceId == (short)JobSourcesEnum.LoanApplicationDetail)
            {
                var applicationDet = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.targetId);
                if (applicationDet != null )
                {
                    var customer = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == applicationDet.CUSTOMERID);
                    if (customer != null) model.customerName = customer.FIRSTNAME;
                }
            }
            return models;
        }

        private IEnumerable<JobRequestViewModel> GetGlobalJobRequestByStatus(int staffId, short statusId, int? startNumber = null)
        {
            var thisStaff = this.context.TBL_STAFF.Find(staffId);
            var staffAdmin = this.context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);
            var staffHub = this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
            var middleOfficeUnit = from x in context.TBL_JOB_TYPE_UNIT
                                   join t in context.TBL_JOB_TYPE_HUB_STAFF on x.JOBTYPEUNITID equals t.JOBTYPEUNITID
                                   where x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && t.STAFFID == staffId
                                   select x;

            bool isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault();
            List<JobRequestViewModel> allData = new List<JobRequestViewModel>();

            List<int> adminJobTypeIds = new List<int>();
            if (staffAdmin.Any())
            {
                foreach (var i in staffAdmin)
                {
                    adminJobTypeIds.Add(i.JOBTYPEID);
                }
                
            }
            List<int> unitIds = new List<int>();
            foreach (var i in staffHub)
            {
                unitIds.Add(i.JOBTYPEUNITID);
            }
            allData = (from x in context.TBL_JOB_REQUEST
                             join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                             join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                             where ((x.SENDERSTAFFID == staffId) || (x.RECEIVERSTAFFID == staffId) || (x.REASSIGNEDTO == staffId)) 
                             && x.REQUESTSTATUSID == (short)statusId
                             || ((unitIds.Contains((int)x.JOBTYPEUNITID)) && !middleOfficeUnit.Any())
                             || adminJobTypeIds.Contains(x.JOBTYPEID)
                             && (startNumber != null && x.JOBREQUESTID > startNumber)
                           orderby x.JOBREQUESTID descending
                             select (
                            new JobRequestViewModel
                            {
                                jobRequestId = x.JOBREQUESTID,
                                requestTitle = x.JOB_TITLE,
                                jobRequestCode = x.JOBREQUESTCODE,
                                targetId = x.TARGETID,
                                jobTypeId = t.JOBTYPEID,
                                jobSubTypeId = s.JOB_SUB_TYPEID,
                                jobTypeName = t.JOBTYPENAME,
                                requireCharge = s.REQUIRECHARGE ?? false,
                                chargeFeeId = s.CHARGEFEEID ?? 0,
                                jobSubTypeName = s.JOB_SUB_TYPE_NAME,
                                
                                senderStaffId = x.SENDERSTAFFID,
                                senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                                senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE != null ? x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE : string.Empty,

                                receiverStaffId = (int)x.RECEIVERSTAFFID,
                                reassignedTo = x.REASSIGNEDTO,
                                isReassigned = x.ISREASSIGNED,
                                isAcknowledged = x.ISACKNOWLEDGED,
                                operationsId = x.OPERATIONSID,
                                operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                                requestStatusId = x.REQUESTSTATUSID,
                                requestStatusname = x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved
                                                                      ? "Completed"
                                                                      : (from y in context.TBL_JOB_REQUEST_STATUS.Where(i => i.REQUESTSTATUSID == x.REQUESTSTATUSID) select y.STATUSNAME).FirstOrDefault(), //x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved ? "Completed" : x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                                senderComment = x.SENDERCOMMENT,
                                responseComment = x.RESPONSECOMMENT,
                                arrivalDate = x.ARRIVALDATE,
                                systemArrivalDate = x.SYSTEMARRIVALDATE,
                                reassignedDate = x.REASSIGNEDDATE,
                                systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                                responseDate = x.RESPONSEDATE,
                                systemResponseDate = x.SYSTEMRESPONSEDATE,
                                acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                                systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                                loggedInStaffId = staffId,
                                jobTypeUnitId = x.JOBTYPEUNITID,
                                jobTypeHubId = x.JOBTYPEHUBID,
                                jobSourceId = x.JOBSOURCEID,
                                branchId = x.BRANCHID,
                                sourceRegionName = context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).Any()
                                                         ? context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_BRANCH_REGION.REGION_NAME : "n/a",
                                sourceBranchCode = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                         ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHCODE : "n/a",
                                sourceBranchName = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                         ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",
                                isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault(),

                                fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                                fromBranchName = (from y in context.TBL_BRANCH.Where(i => i.BRANCHID == x.BRANCHID) select y.BRANCHNAME).FirstOrDefault(),
                                to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                                assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

                            })).Take(100).ToList();



            allData = setCustomerName(allData);

            allData = setJobRequestOtherDetails(allData);

            allData = PadModelWithLMSReference(allData);


            return allData;
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestBySearchString(int staffId, string searchString)
        {
            List<int> applicationDetailsIds = SearchFacilityApplicationEntries(searchString.ToLower());
            List<int> loanIds = SearchBookedFacilityEntries(searchString.ToLower());

            loanIds.AddRange(SearchCustomerLoanFacilities(searchString.ToLower()));
            applicationDetailsIds.AddRange(SearchCustomerFacilityApplication(searchString.ToLower()));

            var thisStaff = this.context.TBL_STAFF.Find(staffId);
            var staffAdmin = this.context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);
            var staffHub = this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
            var middleOfficeUnit = from x in context.TBL_JOB_TYPE_UNIT
                                   join t in context.TBL_JOB_TYPE_HUB_STAFF on x.JOBTYPEUNITID equals t.JOBTYPEUNITID
                                   where x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && t.STAFFID == staffId
                                   select x;

            bool isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault();
            List<JobRequestViewModel> allData = new List<JobRequestViewModel>();

            List<int> adminJobTypeIds = new List<int>();
            if (staffAdmin.Any())
            {
                foreach (var i in staffAdmin)
                {
                    adminJobTypeIds.Add(i.JOBTYPEID);
                }
            }
            List<int> unitIds = new List<int>();
            foreach (var i in staffHub)
            {
                unitIds.Add(i.JOBTYPEUNITID);
            }
            allData = (from x in context.TBL_JOB_REQUEST
                       join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                       join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                       where ((x.SENDERSTAFFID == staffId) 
                                || (x.RECEIVERSTAFFID == staffId) || (x.REASSIGNEDTO == staffId)
                                || ((unitIds.Contains((int)x.JOBTYPEUNITID)) && !middleOfficeUnit.Any())
                                || adminJobTypeIds.Contains(x.JOBTYPEID))
                       && ((x.JOBREQUESTCODE.ToLower() == searchString.ToLower()) 
                                || ((applicationDetailsIds.Contains(x.TARGETID)) && (x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail))
                                || ((loanIds.Contains(x.TARGETID)) && (x.JOBSOURCEID != ((short)JobSourcesEnum.LoanApplicationDetail))) )
                       orderby x.JOBREQUESTID descending
                       select (
                      new JobRequestViewModel
                      {
                          jobRequestId = x.JOBREQUESTID,
                          requestTitle = x.JOB_TITLE,
                          jobRequestCode = x.JOBREQUESTCODE,
                          targetId = x.TARGETID,
                          jobTypeId = t.JOBTYPEID,
                          jobSubTypeId = s.JOB_SUB_TYPEID,
                          jobTypeName = t.JOBTYPENAME,
                          requireCharge = s.REQUIRECHARGE ?? false,
                          chargeFeeId = s.CHARGEFEEID ?? 0,
                          jobSubTypeName = s.JOB_SUB_TYPE_NAME,

                          senderStaffId = x.SENDERSTAFFID,
                          senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                          senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE != null ? x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE : string.Empty,

                          receiverStaffId = (int)x.RECEIVERSTAFFID,
                          reassignedTo = x.REASSIGNEDTO,
                          isReassigned = x.ISREASSIGNED,
                          isAcknowledged = x.ISACKNOWLEDGED,
                          operationsId = x.OPERATIONSID,
                          operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                          requestStatusId = x.REQUESTSTATUSID,
                          requestStatusname = x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved ? "Completed" : x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                          senderComment = x.SENDERCOMMENT,
                          responseComment = x.RESPONSECOMMENT,
                          arrivalDate = x.ARRIVALDATE,
                          systemArrivalDate = x.SYSTEMARRIVALDATE,
                          reassignedDate = x.REASSIGNEDDATE,
                          systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                          responseDate = x.RESPONSEDATE,
                          systemResponseDate = x.SYSTEMRESPONSEDATE,
                          acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                          systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                          loggedInStaffId = staffId,
                          jobTypeUnitId = x.JOBTYPEUNITID,
                          jobTypeHubId = x.JOBTYPEHUBID,
                          jobSourceId = x.JOBSOURCEID,
                          branchId = x.BRANCHID,
                          sourceRegionName = context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).Any()
                                                   ? context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_BRANCH_REGION.REGION_NAME : "n/a",
                          sourceBranchCode = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                   ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHCODE : "n/a",
                          sourceBranchName = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                   ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",
                          isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault(),

                          fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                          fromBranchName = (from y in context.TBL_BRANCH.Where(i => i.BRANCHID == x.BRANCHID) select y.BRANCHNAME).FirstOrDefault(),
                          to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                          assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

                      })).Take(100).ToList();

            allData = setCustomerName(allData);

            allData = setJobRequestOtherDetails(allData);

            allData = PadModelWithLMSReference(allData);

            return allData;
         }

        public IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId, int branchId)
        {
            return GetAllGlobalJobRequest(staffId).OrderByDescending(x => x.jobRequestId);
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByFilter(int staffId, int branchId, string filter, int? startNumber)
        {
            var filtered = filter.ToLower();
            switch (filtered)
            {
                case "completed":
                    return GetGlobalJobRequestByStatus(staffId, (short)RequestStatusEnum.Approved).OrderByDescending(x => x.jobRequestId);

                case "approved":
                    return GetGlobalJobRequestByStatus(staffId, (short)RequestStatusEnum.Approved).OrderByDescending(x => x.jobRequestId);

                case "pending":
                    //return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Pending).OrderByDescending(x => x.jobRequestId);
                    return GetGlobalJobRequestByStatus( staffId, (short)RequestStatusEnum.Pending);
                case "in-progress":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Processing).OrderByDescending(x => x.jobRequestId);

                case "cancelled":
                    return GetGlobalJobRequestByStatus(staffId, (short)RequestStatusEnum.Cancel).OrderByDescending(x => x.jobRequestId);

                case "disapproved":
                    return GetGlobalJobRequestByStatus(staffId, (short)RequestStatusEnum.Disapproved).OrderByDescending(x => x.jobRequestId);

                //case "assigned":
                //    return GetAllGlobalJobRequest(staffId,true).Where(x => x.reassignedTo != null).OrderByDescending(x => x.jobRequestId);

                //case "unassigned":
                //    return GetAllGlobalJobRequest(staffId,true).Where(x => x.reassignedTo == null).OrderByDescending(x => x.jobRequestId);

                default:
                     return GetAllGlobalJobRequest(staffId).OrderByDescending(x => x.jobRequestId);
            }
        }

        public IEnumerable<JobRequestViewModel> GetAllGlobalJobRequestByFacilityRef(string facilityRef)
        {
            var application = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER == facilityRef);

            var targetId = application != null ? application.LOANAPPLICATIONDETAILID : 0;
            var data = (from x in context.TBL_JOB_REQUEST
                        join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                        join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                        where x.TARGETID == targetId
                        orderby x.ARRIVALDATE descending
                        select (
                        new JobRequestViewModel
                        {
                            jobRequestId = x.JOBREQUESTID,
                            requestTitle = x.JOB_TITLE,
                            jobRequestCode = x.JOBREQUESTCODE,
                            targetId = x.TARGETID,
                            jobTypeId = t.JOBTYPEID,
                            jobSubTypeId = s.JOB_SUB_TYPEID,
                            //requireCharge = s.REQUIRECHARGE ?? false,
                            //chargeFeeId = s.CHARGEFEEID ?? 0,
                            jobTypeName = t.JOBTYPENAME,
                            jobSubTypeName = s.JOB_SUB_TYPE_NAME,
                            senderStaffId = x.SENDERSTAFFID,
                            senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                            senderRoleCode = (from y in context.TBL_STAFF_ROLE.Where(i => i.STAFFROLEID == x.TBL_STAFF.STAFFROLEID) select y.STAFFROLECODE).FirstOrDefault(), // x.TBL_STAFF.TBL_STAFF_ROLE != null ? x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE : string.Empty,


                            receiverStaffId = (int)x.RECEIVERSTAFFID,
                            reassignedTo = x.REASSIGNEDTO,
                            isReassigned = x.ISREASSIGNED,
                            isAcknowledged = x.ISACKNOWLEDGED,
                            operationsId = x.OPERATIONSID,
                            operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                            requestStatusId = x.REQUESTSTATUSID,
                            requestStatusname = x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved 
                                                                      ? "Completed" 
                                                                      : (from y in context.TBL_JOB_REQUEST_STATUS.Where(i => i.REQUESTSTATUSID == x.REQUESTSTATUSID) select y.STATUSNAME).FirstOrDefault(), // x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                            senderComment = x.SENDERCOMMENT,
                            responseComment = x.RESPONSECOMMENT,
                            arrivalDate = x.ARRIVALDATE,
                            systemArrivalDate = x.SYSTEMARRIVALDATE,
                            reassignedDate = x.REASSIGNEDDATE,
                            systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                            responseDate = x.RESPONSEDATE,
                            systemResponseDate = x.SYSTEMRESPONSEDATE,
                            acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                            systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                            jobTypeUnitId = x.JOBTYPEUNITID,
                            jobTypeHubId = x.JOBTYPEHUBID,
                            branchId = x.BRANCHID,
                            sourceRegionName = context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).Any()
                                                         ? context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_BRANCH_REGION.REGION_NAME : "n/a",
                            sourceBranchCode = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                         ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHCODE : "n/a",
                            sourceBranchName = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                         ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",
                            fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                            fromBranchName = (from y in context.TBL_BRANCH.Where(i => i.BRANCHID == x.BRANCHID) select y.BRANCHNAME).FirstOrDefault(),//  x.TBL_BRANCH.BRANCHNAME,

                            to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                            assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,
                            responseStaffId = x.RESPONSESTAFFID,
                           
                        })).ToList();

            List<JobRequestViewModel> requests = new List<JobRequestViewModel>();

            //requests.Add(data);
            data = setCustomerName(data);

            data = setJobRequestOtherDetails(data);

            data = PadModelWithLMSReference(data);

            return data;
        }

        private List<int> SearchFacilityApplicationEntries(string searchString)
        {
            List<int> applicationDetailsIds = new List<int>();
            var applicationSearchResult = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER.Contains(searchString));
            foreach (var i in applicationSearchResult)
            {
                var applicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == i.LOANAPPLICATIONID);

                applicationDetailsIds.AddRange(i.TBL_LOAN_APPLICATION_DETAIL.Select(x => x.LOANAPPLICATIONDETAILID));
            };
            return applicationDetailsIds;
        }

        private List<int> SearchBookedFacilityEntries(string searchString)
        {
            List<int> loanIds = new List<int>();
            var loanSearchResult = context.TBL_LOAN.Where(x => x.LOANREFERENCENUMBER.Contains(searchString));
            loanIds.AddRange(loanSearchResult.Select(x=>x.TERMLOANID));

            var revolvingSearchResult = context.TBL_LOAN_REVOLVING.Where(x => x.LOANREFERENCENUMBER.Contains(searchString));
            loanIds.AddRange(revolvingSearchResult.Select(x => x.REVOLVINGLOANID));

            var contingentSearchResult = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANREFERENCENUMBER.Contains(searchString));
            loanIds.AddRange(contingentSearchResult.Select(x => x.CONTINGENTLOANID));

            return loanIds;
        }

        private List<int> SearchCustomerFacilityApplication(string searchString)
        {
            List<int> customerIds = new List<int>();
            var customerSearchResult = context.TBL_CUSTOMER.Where(x => x.FIRSTNAME.ToLower().Contains(searchString) 
                                                                    || x.MIDDLENAME.ToLower().Contains(searchString) 
                                                                    || x.LASTNAME.ToLower().Contains(searchString));

            List<int> loanIds = new List<int>();
            var loanSearchResult = context.TBL_LOAN.Where(x => customerSearchResult.Select(c => c.CUSTOMERID).Contains(x.CUSTOMERID));
            loanIds.AddRange(loanSearchResult.Select(x => x.TERMLOANID));

            var revolvingSearchResult = context.TBL_LOAN_REVOLVING.Where(x => customerSearchResult.Select(c => c.CUSTOMERID).Contains(x.CUSTOMERID));
            loanIds.AddRange(revolvingSearchResult.Select(x => x.REVOLVINGLOANID));

            var contingentSearchResult = context.TBL_LOAN_CONTINGENT.Where(x => customerSearchResult.Select(c => c.CUSTOMERID).Contains(x.CUSTOMERID));
            loanIds.AddRange(contingentSearchResult.Select(x => x.CONTINGENTLOANID));


            return loanIds;
        }

        private List<int> SearchCustomerLoanFacilities(string searchString)
        {
            List<int> customerIds = new List<int>();
            List<int> customerFacilityIds = new List<int>();
            var customerSearchResult = context.TBL_CUSTOMER.Where(x => x.FIRSTNAME.ToLower().Contains(searchString) 
                                                                    || x.MIDDLENAME.ToLower().Contains(searchString)  
                                                                    || x.LASTNAME.ToLower().Contains(searchString));
            

            var applicationSearchResult = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => customerSearchResult.Select(c => c.CUSTOMERID).Contains(x.CUSTOMERID));
            customerFacilityIds.AddRange(applicationSearchResult.Select(x => x.LOANAPPLICATIONDETAILID));

            return customerFacilityIds;
        }

        private List<JobRequestViewModel> PadModelWithLMSReference(List<JobRequestViewModel> models)
        {
            foreach(var model in models)
            {
                if(model.applicationReferenceNumber == null)
                {
                    model.refNo = "n/a";
                    if (model.jobSourceId == (short)JobSourcesEnum.LoanApplicationDetail)
                    {
                        var losApplication = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x=>x.LOANAPPLICATIONDETAILID == model.targetId);

                        if(losApplication != null)
                        {
                            var application = losApplication.TBL_LOAN_APPLICATION;
                            model.refNo = application.APPLICATIONREFERENCENUMBER;

                            var customer = context.TBL_CUSTOMER.Find(losApplication.CUSTOMERID);
                            if (customer != null)
                            {
                                model.customerName = customer.FIRSTNAME;
                                model.customerId = customer.CUSTOMERID;
                            }
                        }
                    }

                    if (model.jobSourceId == (short)JobSourcesEnum.LMSOperationAndApproval)
                    {
                        var lmsoperation = context.TBL_LOAN_REVIEW_OPERATION.Find(model.targetId);
                       
                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility)
                        {
                            var loan = context.TBL_LOAN.Find(lmsoperation.LOANID);
                            if (loan != null)
                            {
                                model.refNo = loan.LOANREFERENCENUMBER;
                                var appDetail = loan.TBL_LOAN_APPLICATION_DETAIL;
                                model.loanApplicationId = appDetail.LOANAPPLICATIONID;

                                var customer = context.TBL_CUSTOMER.Find(loan.CUSTOMERID);
                                if (customer != null)
                                {
                                    model.customerName = customer.FIRSTNAME;
                                    model.customerId = customer.CUSTOMERID;
                                }
                            }
                        }


                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility)
                        {
                            var loan = context.TBL_LOAN_REVOLVING.Find(lmsoperation.LOANID);
                            if (loan != null)
                            {
                                model.refNo = loan.LOANREFERENCENUMBER;
                                var appDetail = loan.TBL_LOAN_APPLICATION_DETAIL;
                                model.loanApplicationId = appDetail.LOANAPPLICATIONID;

                                var customer = context.TBL_CUSTOMER.Find(loan.CUSTOMERID);
                                if (customer != null)
                                {
                                    model.customerName = customer.FIRSTNAME;
                                    model.customerId = customer.CUSTOMERID;
                                }
                            }
                        }


                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability)
                        {
                            var loan = context.TBL_LOAN_CONTINGENT.Find(lmsoperation.LOANID);
                            if (loan != null)
                            {
                                model.refNo = loan.LOANREFERENCENUMBER;
                                var appDetail = loan.TBL_LOAN_APPLICATION_DETAIL;
                                model.loanApplicationId = appDetail.LOANAPPLICATIONID;

                                var customer = context.TBL_CUSTOMER.Find(loan.CUSTOMERID);
                                if (customer != null)
                                {
                                    model.customerName = customer.FIRSTNAME;
                                    model.customerId = customer.CUSTOMERID;
                                }
                            }
                        }


                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility)
                        {
                            var applicationRequests = this.context.TBL_LOAN_APPLICATION_DETAIL.Find(lmsoperation.LOANID);
                            if (applicationRequests != null)
                            {
                                var appdetail = applicationRequests.TBL_LOAN_APPLICATION;
                                model.refNo = appdetail.APPLICATIONREFERENCENUMBER;
                                model.loanApplicationId = appdetail.LOANAPPLICATIONID;

                                var customer = context.TBL_CUSTOMER.Find(applicationRequests.CUSTOMERID);
                                if (customer != null)
                                {
                                    model.customerName = customer.FIRSTNAME;
                                    model.customerId = customer.CUSTOMERID;
                                }
                            }
                        }
                    }


                    if (model.jobSourceId == (short)JobSourcesEnum.LMSApplication)
                    {
                        var lmsoperation = context.TBL_LMSR_APPLICATION_DETAIL.Find(model.targetId);
                        var customer = context.TBL_CUSTOMER.Find(lmsoperation.CUSTOMERID);
                        if (customer != null) model.customerName = customer.FIRSTNAME;

                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility)
                        {
                            var loan = context.TBL_LOAN.Find(lmsoperation.LOANID);
                            if (loan != null) model.refNo = loan.LOANREFERENCENUMBER;
                        }

                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility)
                        {
                            var loan = context.TBL_LOAN_REVOLVING.Find(lmsoperation.LOANID);
                            if (loan != null) model.refNo = loan.LOANREFERENCENUMBER;
                        }

                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability)
                        {
                            var loan = context.TBL_LOAN_CONTINGENT.Find(lmsoperation.LOANID);
                            if (loan != null)
                            {
                                model.refNo = loan.LOANREFERENCENUMBER;
                            }
                        }

                        if (lmsoperation != null && lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility)
                        {
                            var applicationRequests = this.context.TBL_LOAN_APPLICATION_DETAIL.Find(lmsoperation.LOANID);
                            if (applicationRequests != null)
                            {
                                var appRequest = applicationRequests.TBL_LOAN_APPLICATION;
                                model.refNo = appRequest.APPLICATIONREFERENCENUMBER;
                            }
                        }

                    }
                }
            }
            return models;
        }

        public jobRequestCountViewModel GetJobRequestStatusCount(int staffId, int branchId)
        {
            var jobRequests = GetAllGlobalJobRequest(staffId, false);
            return new jobRequestCountViewModel()
            {
                pendingCount = jobRequests.Where(x => x.requestStatusId == (short)JobRequestStatusEnum.pending).Count(),
                finishedCount = jobRequests.Where(x => x.responseComment != null).Count(),
                inProgresCount = jobRequests.Where(x => x.requestStatusId == (short)JobRequestStatusEnum.processing).Count(),
                cancelledCount = jobRequests.Where(x => x.requestStatusId == (short)JobRequestStatusEnum.cancel).Count(),
                assignedCount = jobRequests.Where(x => x.receiverStaffId != null || x.reassignedTo != null).Count(),
                unAssignedCount = jobRequests.Where(x =>  x.reassignedTo == null && x.receiverStaffId == null).Count(),
                allCount = jobRequests.Count(),
            };
        }

        public List<JobRequestDetailViewModel> GetJobRequestDetailsById(int jobRequestId)
        {
            return GetJobRequestDetails().Where(x => x.jobRequestId == jobRequestId).ToList();
        }

        private List<JobRequestDetailViewModel> GetJobRequestDetails()
        {
            var details = (from x in this.context.TBL_JOB_REQUEST_DETAIL
                           join b in context.TBL_JOB_REQUEST on x.JOBREQUESTID equals b.JOBREQUESTID
                           where x.DELETED == false //x.TBL_JOB_TYPE_SUB.JOBTYPEID == (short)JobTypeEnum.legal
                           //&& x.DELETED == false
                           select new JobRequestDetailViewModel
                           {
                               jobRequestId = x.JOBREQUESTID,
                               jobRequestDetailId = x.JOBREQUEST_DETAILID,
                               accreditedConsultantId = (int)x.ACCREDITEDCONSULTANTID,
                               accreditedConsultantName = x.TBL_ACCREDITEDCONSULTANT.FIRMNAME,
                               jobSubTypeId = x.JOB_SUB_TYPEID,
                               jobSubTypeclassId = x.JOB_SUB_TYPE_CLASSID,
                               jobSubTypeClassName = x.TBL_JOB_TYPE_SUB_CLASS.JOB_SUB_TYPE_CLASS_NAME,
                               jobRequestCode = b.JOBREQUESTCODE,
                               jobSubTypeName = x.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME,
                               jobTypeId = x.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPEID,
                               jobTypeName = x.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPENAME,
                               description = x.DESCRIPTION,
                               targetId = x.TBL_JOB_REQUEST.TARGETID,
                               operationsId = x.TBL_JOB_REQUEST.OPERATIONSID,
                               operationsName = x.TBL_JOB_REQUEST.TBL_OPERATIONS.OPERATIONNAME,
                               amount = x.AMOUNT,
                               accountNumber = x.ACCOUNTNUMBER,
                               dateTimeCreated = x.DATETIMECREATED
                           }).ToList();

            foreach (var item in details)
            {
                var a = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONDETAILID == item.targetId);

                if (a.Any())
                {
                    var t = a.FirstOrDefault();
                    item.customerName = (from v in context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == t.CUSTOMERID) select v.FIRSTNAME + " " + v.MIDDLENAME + " " + v.LASTNAME).FirstOrDefault();
                    item.applicationReferenceNumber = t.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
                    item.customerId = t.CUSTOMERID;
                }
            };

            return details.ToList();
        }

        public List<JobRequestDetailViewModel> GetLegalJobRequestDetails()
        {
            var jobRequest = context.TBL_JOB_REQUEST.Where(x => x.REQUESTSTATUSID > (short)ApprovalStatusEnum.Pending && x.JOBTYPEID == (short)JobTypeEnum.legal);
           
            List<JobRequestDetailViewModel> jobDetailList = new List<JobRequestDetailViewModel>();
            foreach(var i in jobRequest)
            {
                var jobDetail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == i.JOBREQUESTID && x.ACCREDITEDCONSULTANTPAID == false && x.TRANSACTIONREVERSED != true && x.JOB_SUB_TYPEID == x.TBL_JOB_TYPE_SUB_CLASS.JOB_SUB_TYPEID).ToList();
                decimal chargeAmount = 0;
                var jobSubTypeName = string.Empty; 
                var jobTypeName = string.Empty;
                var description = string.Empty;
                var customerName = string.Empty;
                var applicationReferenceNumber = string.Empty;
                var customerId = 0;

                var singleJobDetail = jobDetail.FirstOrDefault();
                foreach (var item in jobDetail)
                {
                    if (item.AMOUNT == null) item.AMOUNT = 0;

                     chargeAmount = chargeAmount + item.AMOUNT.Value;
                    description = item.DESCRIPTION != null ? description + item.DESCRIPTION.ToString() + ", " : string.Empty;
                    jobSubTypeName = item.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME != null  ?  item.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME.ToString()  : string.Empty;

                    var a = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONDETAILID == i.TARGETID);

                    if (a.Any())
                    {
                        var t = a.FirstOrDefault();
                        customerName = (from v in context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == t.CUSTOMERID) select v.FIRSTNAME + " " + v.MIDDLENAME + " " + v.LASTNAME).FirstOrDefault();
                        applicationReferenceNumber = t.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
                        customerId = t.CUSTOMERID;
                    }
                }

                if(jobDetail.Count() > 0)
                {
                    JobRequestDetailViewModel detail = new JobRequestDetailViewModel();
                    detail.jobRequestId = singleJobDetail.JOBREQUESTID;
                    detail.jobRequestDetailId = singleJobDetail.JOBREQUEST_DETAILID;
                    detail.accreditedConsultantId = (int)singleJobDetail.ACCREDITEDCONSULTANTID;
                    detail.accreditedConsultantName = singleJobDetail.TBL_ACCREDITEDCONSULTANT.FIRMNAME;
                    detail.jobRequestCode = singleJobDetail.TBL_JOB_REQUEST.JOBREQUESTCODE;
                    detail.jobSubTypeName = jobSubTypeName;
                    detail.jobTypeName = jobTypeName;
                    detail.jobSubTypeId = singleJobDetail.JOB_SUB_TYPEID;
                    detail.description = description;
                    detail.operationsId = singleJobDetail.TBL_JOB_REQUEST.OPERATIONSID;
                    detail.targetId = singleJobDetail.TBL_JOB_REQUEST.TARGETID;
                    detail.amount = chargeAmount;
                    detail.accountNumber = singleJobDetail.ACCOUNTNUMBER;
                    detail.dateTimeCreated = singleJobDetail.DATETIMECREATED;
                    detail.customerName = customerName;
                    detail.applicationReferenceNumber = applicationReferenceNumber;
                    detail.customerId = customerId;

                    
                    jobDetailList.Add(detail);
                }
            }

            return jobDetailList;
        }

        public List<JobRequestViewModel> GetJobRequestLegalJobDetail()
        {
            var details = (from x in this.context.TBL_JOB_REQUEST
                           join b in context.TBL_JOB_REQUEST_DETAIL on x.JOBREQUESTID equals b.JOBREQUESTID
                           where b.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated
                           && b.DELETED == false
                           select new JobRequestViewModel
                           {
                               jobRequestId = x.JOBREQUESTID,
                               jobRequestCode = x.JOBREQUESTCODE,
                               jobTypeId = x.TBL_JOB_TYPE.JOBTYPEID,
                               jobTypeName = x.TBL_JOB_TYPE.JOBTYPENAME,
                               targetId = x.TARGETID,
                               operationsId = x.OPERATIONSID,
                               operationsName = x.TBL_OPERATIONS.OPERATIONNAME,
                               dateTimeCreated = x.ARRIVALDATE,
                               jobDetail = (from d in this.context.TBL_JOB_REQUEST_DETAIL
                                           select new JobRequestDetailViewModel
                                            {
                                                jobRequestId = d.JOBREQUESTID,
                                                jobRequestDetailId = d.JOBREQUEST_DETAILID,
                                                accreditedConsultantId = (int)d.ACCREDITEDCONSULTANTID,
                                                accreditedConsultantName = d.TBL_ACCREDITEDCONSULTANT.FIRMNAME,
                                                jobSubTypeId = d.JOB_SUB_TYPEID,
                                                jobRequestCode = x.JOBREQUESTCODE,
                                                jobSubTypeName = d.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME,
                                                jobTypeId = d.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPEID,
                                                jobTypeName = d.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPENAME,
                                                description = d.DESCRIPTION,
                                                amount = d.AMOUNT,
                                                accountNumber = d.ACCOUNTNUMBER,
                                                dateTimeCreated = d.DATETIMECREATED
                                            }).ToList(),
                                }).ToList();
            

            foreach (var item in details)
            {
                var a = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONDETAILID == item.targetId);

                if (a.Any())
                {
                    var t = a.FirstOrDefault();
                    item.customerName = (from v in context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == t.CUSTOMERID) select v.FIRSTNAME + " " + v.MIDDLENAME + " " + v.LASTNAME).FirstOrDefault();
                    item.applicationReferenceNumber = t.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
                    item.customerId = t.CUSTOMERID;
                }
            };

            return details.ToList();
        }

        public List<JobRequestViewModel> GetApplicationJobRequest(int targetId, int operationId, short jobSourceId)
        {
            //var requests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == applicationDetailId && x.OPERATIONSID == operationId).ToList();
            List<TBL_JOB_REQUEST> requests = new List<TBL_JOB_REQUEST>(); // this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == applicationDetailId).ToList();
            //var operationRecord = context.TBL_OPERATIONS.Find(operationId);
            if (jobSourceId == (short)JobSourcesEnum.LoanApplicationDetail)
            {
                var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                requests.AddRange(applicationRequests);
            }

            if(jobSourceId == (short)JobSourcesEnum.LoanBookingAndApproval)
            {
                var loan = context.TBL_LOAN.Find(targetId);
                var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();

                requests.AddRange(disbursementRequests);
                requests.AddRange(applicationRequests);

            }

            if (jobSourceId == (short)JobSourcesEnum.ContingentLiabilityBookingAndApproval)
            {
                var loan = context.TBL_LOAN_CONTINGENT.Find(targetId);
                var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.ContingentLiabilityBookingAndApproval).ToList();
                var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();

                requests.AddRange(disbursementRequests);
                requests.AddRange(applicationRequests);

            }

            if (jobSourceId == (short)JobSourcesEnum.OverdraftBookingAndApproval)
            {
                var loan = context.TBL_LOAN_REVOLVING.Find(targetId);
                var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.OverdraftBookingAndApproval).ToList();
                var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();

                requests.AddRange(disbursementRequests);
                requests.AddRange(applicationRequests);

            }

            if (jobSourceId == (short)JobSourcesEnum.LMSOperationAndApproval)
            {
                if (operationId == (short)OperationsEnum.TermLoanBooking)
                {
                    var lmsoperation = context.TBL_LOAN_REVIEW_OPERATION.Find(targetId);
                    var lmsoperationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId &&  x.JOBSOURCEID == (short)JobSourcesEnum.LMSOperationAndApproval).ToList();

                    if(lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        var loan = context.TBL_LOAN.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.TERMLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        var loan = context.TBL_LOAN_REVOLVING.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.REVOLVINGLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability)
                    {
                        var loan = context.TBL_LOAN_CONTINGENT.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.CONTINGENTLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility)
                    {
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == lmsoperation.LOANID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(applicationRequests);
                    }


                    requests.AddRange(lmsoperationRequests);
                }

            }

            if (jobSourceId == (short)JobSourcesEnum.LMSApplication)
            {
                if (operationId == (short)OperationsEnum.TermLoanBooking)
                {
                    var lmsoperation = context.TBL_LMSR_APPLICATION_DETAIL.Find(targetId);
                    var lmsoperationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.JOBSOURCEID == (short)JobSourcesEnum.LMSApplication).ToList();

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        var loan = context.TBL_LOAN.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.TERMLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        var loan = context.TBL_LOAN_REVOLVING.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.REVOLVINGLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability)
                    {
                        var loan = context.TBL_LOAN_CONTINGENT.Find(lmsoperation.LOANID);
                        var disbursementRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.CONTINGENTLOANID && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.LoanBookingAndApproval).ToList();
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == loan.LOANAPPLICATIONDETAILID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(disbursementRequests);
                        requests.AddRange(applicationRequests);
                    }

                    if (lmsoperation.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.LineFacility)
                    {
                        var applicationRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == lmsoperation.LOANID && x.JOBSOURCEID == (short)JobSourcesEnum.LoanApplicationDetail).ToList();
                        requests.AddRange(applicationRequests);
                    }


                    requests.AddRange(lmsoperationRequests);
                }

            }

            if (jobSourceId == (short)JobSourcesEnum.CollateralReleaseApproval)
            {
                var collateralReleaseJobRequests = this.context.TBL_JOB_REQUEST.Where(x => x.TARGETID == targetId && x.OPERATIONSID == operationId && x.JOBSOURCEID == (short)JobSourcesEnum.CollateralReleaseApproval).ToList();

                requests.AddRange(collateralReleaseJobRequests);

            }

            if (requests.Count() <= 0) return null;

            var requestsList = new List<JobRequestViewModel>();
            foreach ( var request in requests)
            {
                
                var jobDocumentsList = GetJobRequestDocuments(request.JOBREQUESTCODE).AsEnumerable();
                TBL_JOB_REQUEST_STATUS_FEEDBAK feedback;
                
                feedback = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Where(c => c.JOB_STATUS_FEEDBACKID == request.JOB_STATUS_FEEDBACKID).FirstOrDefault();
                var requestModel = new JobRequestViewModel
                {
                    jobRequestId = request.JOBREQUESTID,
                    requestTitle = request.JOB_TITLE,
                    jobRequestCode = request.JOBREQUESTCODE,
                    targetId = request.TARGETID,
                    jobTypeId = request.JOBTYPEID,
                    jobTypeName = request.TBL_JOB_TYPE.JOBTYPENAME,
                    senderStaffId = request.SENDERSTAFFID,
                    receiverStaffId = request.RECEIVERSTAFFID ?? 0,
                    reassignedTo = request.REASSIGNEDTO,
                    isReassigned = request.ISREASSIGNED,
                    isAcknowledged = request.ISACKNOWLEDGED,
                    operationsId = request.OPERATIONSID,
                    operationName = request.TBL_OPERATIONS.OPERATIONNAME,
                    requestStatusId = request.REQUESTSTATUSID,
                    senderComment = request.SENDERCOMMENT,
                    responseComment = request.RESPONSECOMMENT,
                    requestStatusname = request.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                    arrivalDate = request.ARRIVALDATE,
                    systemArrivalDate = request.SYSTEMARRIVALDATE,
                    reassignedDate = request.REASSIGNEDDATE,
                    systemReassignedDate = request.SYSTEMREASSIGNEDDATE,
                    responseDate = request.RESPONSEDATE,
                    systemResponseDate = request.SYSTEMRESPONSEDATE,
                    acknowledgementDate = request.ACKNOWLEDGEMENTDATE,
                    systemAcknowledgementDate = request.SYSTEMACKNOWLEDGEMENTDATE,
                    jobStatusFeedBackId = request.JOB_STATUS_FEEDBACKID ?? 0,
                    jobStatusFeedback = (feedback != null) ? feedback.JOB_STATUS_FEEDBACK_NAME : string.Empty,
                    msgExchangeTrail = (from y in context.TBL_JOB_REQUEST_MESSAGE
                                        where y.JOBREQUESTID == request.JOBREQUESTID
                                        select new JobRequestMessageViewModel
                                        {
                                            jobRequestMessageId = y.JOBREQUEST_MESSAGEID,
                                            jobRequestId = y.JOBREQUESTID,
                                            message = y.MESSAGE,
                                            staffId = y.STAFFID,
                                            staffName = y.TBL_STAFF.FIRSTNAME + " " + y.TBL_STAFF.MIDDLENAME + " " + y.TBL_STAFF.LASTNAME,
                                            datetimeSent = y.DATE_TIME_SENT
                                        }).ToList(),
                    jobDocuments = jobDocumentsList,
                };
                requestModel.recievingUnitName = request.JOBTYPEUNITID != null && request.JOBTYPEUNITID >0 ? context.TBL_JOB_TYPE_UNIT.FirstOrDefault(x => x.JOBTYPEUNITID == request.JOBTYPEUNITID).UNITNAME : "";
                requestModel.recievingHub = request.JOBTYPEHUBID != null && request.JOBTYPEHUBID  >0 ? context.TBL_JOB_TYPE_HUB.FirstOrDefault(x => x.JOBTYPEHUBID == request.JOBTYPEHUBID).HUBNAME : "";
                requestModel.jobSourceName = request.JOBSOURCEID != null && request.JOBSOURCEID  >0 ? context.TBL_JOB_SOURCE.FirstOrDefault(x => x.JOBSOURCEID == request.JOBSOURCEID).JOBSOURCENAME : "";

                var fromData = context.TBL_STAFF.Where(b => b.STAFFID == request.SENDERSTAFFID).FirstOrDefault();
                requestModel.fromSender = fromData != null ? fromData.FIRSTNAME + " " + fromData.MIDDLENAME + " " + fromData.LASTNAME : "n/a";

                var toData = context.TBL_STAFF.Where(b => b.STAFFID == request.RECEIVERSTAFFID).FirstOrDefault();
                requestModel.to = toData != null ? toData.FIRSTNAME + " " + toData.MIDDLENAME + " " + toData.LASTNAME : "n/a";

                var asigneeData = context.TBL_STAFF.Where(b => b.STAFFID == request.REASSIGNEDTO).FirstOrDefault();
                requestModel.assignee = asigneeData != null ? asigneeData.FIRSTNAME + " " + asigneeData.MIDDLENAME + " " + asigneeData.LASTNAME : "n/a";

                requestsList.Add(requestModel);
            }
           

            return requestsList;

        }

        public JobRequestViewModel GetJobRequest(int jobRequestId)
        {
            var x = this.context.TBL_JOB_REQUEST.Find(jobRequestId);

            if (x == null)
            {
                return null;
            }

            var request = new JobRequestViewModel
            {
                jobRequestId = x.JOBREQUESTID,
                requestTitle = x.JOB_TITLE,
                jobRequestCode = x.JOBREQUESTCODE,
                targetId = x.TARGETID,
                jobTypeId = x.JOBTYPEID,
                senderStaffId = x.SENDERSTAFFID,
                receiverStaffId = (int)x.RECEIVERSTAFFID,
                reassignedTo = x.REASSIGNEDTO,
                isReassigned = x.ISREASSIGNED,
                isAcknowledged = x.ISACKNOWLEDGED,
                operationsId = x.OPERATIONSID,
                operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                requestStatusId = x.REQUESTSTATUSID,
                senderComment = x.SENDERCOMMENT,
                responseComment = x.RESPONSECOMMENT,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDate = x.SYSTEMARRIVALDATE,
                reassignedDate = x.REASSIGNEDDATE,
                systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                responseDate = x.RESPONSEDATE,
                systemResponseDate = x.SYSTEMRESPONSEDATE,
                acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                fromBranchName = context.TBL_BRANCH.Where(c => c.STATEID == x.SENDERSTAFFID).FirstOrDefault().BRANCHNAME,
                toBranchName = context.TBL_BRANCH.Where(c => c.STATEID == x.RECEIVERSTAFFID).FirstOrDefault().BRANCHNAME,
            };

            var fromData = context.TBL_STAFF.Where(b => b.STAFFID == request.senderStaffId).FirstOrDefault();
            request.fromSender = fromData != null ? fromData.FIRSTNAME + " " + fromData.MIDDLENAME + " " + fromData.LASTNAME : "n/a";

            var toData = context.TBL_STAFF.Where(b => b.STAFFID == request.receiverStaffId).FirstOrDefault();
            request.to = toData != null ? toData.FIRSTNAME + " " + toData.MIDDLENAME + " " + toData.LASTNAME : "n/a";

            var asigneeData = context.TBL_STAFF.Where(b => b.STAFFID == request.reassignedTo).FirstOrDefault();
            request.assignee = asigneeData != null ? asigneeData.FIRSTNAME + " " + asigneeData.MIDDLENAME + " " + asigneeData.LASTNAME : "n/a";

            List<JobRequestViewModel> requests = new List<JobRequestViewModel>();

            requests.Add(request);
            request = setCustomerName(requests).FirstOrDefault();

            request = setJobRequestOtherDetails(requests).FirstOrDefault();

            request = PadModelWithLMSReference(requests).FirstOrDefault();

            return request;

        }

        public IEnumerable<JobRequestMessageViewModel> GetJobComments(int jobRequestId)
        {
            var data = (from x in context.TBL_JOB_REQUEST_MESSAGE
                        where x.JOBREQUESTID == jobRequestId
                        orderby x.DATE_TIME_SENT ascending
                        select new JobRequestMessageViewModel
                        {
                            jobRequestId = x.JOBREQUESTID,
                            message = x.MESSAGE,
                            staffId = x.STAFFID,
                            staffName = x.TBL_STAFF.FIRSTNAME,
                            datetimeSent = x.DATE_TIME_SENT
                        }).Take(200);

            return data;
        }

        public IEnumerable<ApprovalStatusViewModel> GetJobRequestApprovaStatus()
        {
            var data = (from x in context.TBL_JOB_REQUEST_STATUS
                        select new ApprovalStatusViewModel
                        {
                            approvalStatusId = x.REQUESTSTATUSID,
                            approvalStatusName = x.STATUSNAME
                        });

            return data;
        }

      
        public IEnumerable<JobRequestStatusFeedbackViewModel> GetJobRequestStatusFeedback(short statusId, short jobTypeId)
        {
            return this.context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Select(x => new JobRequestStatusFeedbackViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobStatusFeedbackId = x.JOB_STATUS_FEEDBACKID,
                requestStatusId = x.REQUESTSTATUSID,
                jobStatusFeedbackName = x.JOB_STATUS_FEEDBACK_NAME
            });
        }

        private void LogEmailAlert(string messageBody, string alertSubject, string recipients, string jobReQuestCode, int targetId)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = recipients.Trim();
                string messageSubject = title;
                string messageContent = messageBody;
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = jobReQuestCode,
                    targetId = targetId,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId
            };

            context.TBL_MESSAGE_LOG.Add(message);

        }


        #region ...Collateral Search Job Charges...
        [OperationBehavior(TransactionScopeRequired = true)]
        private bool PlaceChargeOnCustomerForCollateralSearch2(JobRequestCollateralSearchViewModel model)
        {
            // NOTE: THIS METHOD IS USED BY RM & LEGAL IN TWO WAYS
            // 1. THE INITIATION STAGE: RM INITIATES PAYMENT, DEBITS CUSTOMER'S ACCOUNT WITH RECOMMENDED FEE FROM EGAL
            // 2.  THE NONE INITIATION: LEGAL COMPLETE'S THE PROCESS. CONFIRMING SOLICITOR'S JOB DONE TO CREDIT SOLICITOR AND DEBIT SUSPENSE ACCOUNT, WITHOLDING %.
            var jobRequestDetail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == model.jobRequestId && x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated).ToList();

            var auditDetail = string.Empty;
            var accountNumber = string.Empty;
            var consultantId = jobRequestDetail.FirstOrDefault().ACCREDITEDCONSULTANTID;
            var consultantRecord = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == consultantId);
            decimal accountBalance = 0;
            if (model.isInitiation)
            {
                var casa = context.TBL_CASA.Find(model.casaAccountId);
                if (casa == null && !model.debitBusiness)
                    throw new ConditionNotMetException("Customer account number is not supplied");

                if (casa != null) accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;

                if (!model.debitBusiness)
                {
                    model.casaAccountId = casa.CASAACCOUNTID;
                    accountNumber = casa.PRODUCTACCOUNTNUMBER;
                    auditDetail = $"Customer account number '{casa.PRODUCTACCOUNTNUMBER}' debited with collateral search fees";
                }
                else { auditDetail = $"Bank account debited with collateral search fees"; }

                model.operationId = (short)OperationsEnum.CollateralSearchInitiation;
            }
            else
            {
                var b = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;
                if ( b == null || b == string.Empty || b == " ")
                    throw new ConditionNotMetException("The solicitor's account number is not found. No account number has been mapped to this solicitor.");

                    accountNumber = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;


                var witholdingAmount = (double)model.totalChargeAmount / 0.9;
                var id = (short)jobRequestDetail.FirstOrDefault().CURRENCYID.Value;
                model.currencyId = (short)jobRequestDetail.FirstOrDefault().CURRENCYID;
                var currency = context.TBL_CURRENCY.Find(model.currencyId);
                model.operationId = (short)OperationsEnum.CollateralSearchCompletion;
                model.feeNarration = $"Payment to solicitor";
                auditDetail = $"Solicitor account number '{accountNumber}' credited for collateral search job with '{currency.CURRENCYCODE}{witholdingAmount}'";
            }

            var jobRequestData = context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            model.requestCode = jobRequestData.JOBREQUESTCODE;

            foreach (var item in jobRequestDetail)
            {
                model.totalChargeAmount = model.totalChargeAmount  + item.AMOUNT.Value;
                item.ACCREDITEDCONSULTANTPAID = !model.isInitiation ? true : false;
                item.ACCOUNTNUMBER = model.isInitiation ? accountNumber : null;
            }

            if (model.isInitiation && model.totalChargeAmount > accountBalance && !model.debitBusiness )
                throw new ConditionNotMetException("The customer's Account is not funded.");

            if (model.totalChargeAmount > 0)
            {
                var twoFADetails = new TwoFactorAutheticationViewModel
                {
                    username = model.username,
                    passcode = model.passCode
                };
                if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
                {
                    twoFADetails.username = context.TBL_STAFF.Find(model.createdBy).STAFFCODE;
                }
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                //When RM apply fee on customer's account
                if(model.isInitiation)
                {
                    if (model.debitBusiness)
                    {
                        var bizAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (short)OtherOperationEnum.ChargeOnBank).FirstOrDefault();
                        if (bizAccount == null) throw new ConditionNotMetException("No Account has been mapped for charges on business");

                        model.glAccountId = bizAccount.GLACCOUNTID;
                        model.casaAccountId = null;
                        model.currencyId = (short)jobRequestDetail.FirstOrDefault().CURRENCYID;
                        model.currencyCode = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId).CURRENCYCODE;
                    }
                    inputTransactions.AddRange(BuildCollateralSearchChargeFeesPosting(model));
                }

                //When Legal confirms colletral job search
                if (!model.isInitiation)
                {
                    model.accountNumber = accountNumber;
                    inputTransactions.AddRange(BuildSolicitorFeePaymentPosting(model));
                }
                    
                if (inputTransactions.Count > 0)
                {
                    financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CollateralSearchJob,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------


                    //if (model.isInitiation) //When RM apply fee on customer's account
                    //{
                    //    if (consultantRecord.Any())
                    //    {
                    //        var solicitor = consultantRecord.FirstOrDefault();
                    //        string messageBoby = $"Dear {solicitor.FIRMNAME}, <br /><br />Your attention is needed to attend to our customer's collateral on the following:<br /> <ul>";
                    //        foreach (var i in jobRequestDetail)
                    //        {
                    //            if(i.JOB_SUB_TYPE_CLASSID != (short) (JobSubTypeClassEnum.AdditionalCharges)) messageBoby = messageBoby + $@"<li>{i.TBL_JOB_TYPE_SUB_CLASS.JOB_SUB_TYPE_CLASS_NAME}</li>";

                    //            i.CUSTOMERORBUSINESSCHARGED = true;
                    //            if (model.debitBusiness) i.DEBITBUSINESS = true;
                    //        }

                    //        messageBoby = messageBoby + $@"</ul> <br /> Kindly contact FBN legal department for more information.";
                    //        string alertSubject = $"FBN - Loan Collateral Search";
                    //        LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, solicitor.EMAILADDRESS, jobRequestData.JOBREQUESTCODE);
                    //    }
                    //}

                    if (model.isInitiation) //When RM apply fee on customer's account
                    {
                        if (consultantRecord.Any())
                        {
                            var solicitor = consultantRecord.FirstOrDefault();
                            var stateId = solicitor.TBL_ACCREDITEDCONSULTANT_STATE.FirstOrDefault(x=>x.STATEID != 0)?.STATEID;
                            var state = context.TBL_STATE.Find(stateId);

                            string messageBoby = $"Dear {solicitor.FIRMNAME}, " +
                                $"<br /><br />Kindly conduct a comprehensive search on the title document" +
                                $" – DEED OF SUB-LEASE REGD AS 22/22/594 IN {state.STATENAME}  and dated July 15, 1994 " + //to work on this dynamic line
                                $"with a view to determining who is the current holder/owner of the legal estate interest in the property and whether or " +
                                $"not the title of the said current holder is free from encumbrance(s) and encroachment(s) of any nature and/or " +
                                $"Government acquisition (please note that for this purpose it is sufficient to state that there is no annotation to " +
                                $"that effect on the title document/survey plan/land registry file)." +
                                $"<br /> <br />" +
                                $"We will be much obliged to receive your report of search and certificate of good title (if applicable) for the attention of the undersigned, within 24 hours." +
                                $"<br /> <br />" +
                                $"Where the search report is favourable, kindly proceed to carry out a charting  exercise in respect of the survey plan attached to the title document  at the Surveyor General Office to confirm the following:-" +
                                $"<ol type= i>" +
                                $"<li>That the property is free from acquisition or revocation of any kind</li>" +
                                $"<li>That  the survey plan is within the right coordinates,</li>" +
                                $"<li>That  the survey plan is within the right coordinates,</li>" +
                                $"<li>That the survey plan attached to the title document corresponds/matches with the Surveyor General’s copy and the Assignor’s  Survey (where applicable)</li>" +
                                $"<li>That the address of the Property stated on the Survey Plan is the same with the address stated on the title document.  (here we may state the actual addresses for clarity)</li>" +
                                $"<li> That the survey plan is without any other defect that may lead to queries which may stall or delay subsequent perfection exercises</li>" +
                                $"<br /> <br />" +
                                $"Thank you." +
                                $"<br /> <br /> <br />" +
                                $"Please acknowledge receipt of this mail.";

                            string alertSubject = $"Loan Collateral Search";
                            LogEmailAlert(messageBoby, alertSubject, solicitor.EMAILADDRESS, jobRequestData.JOBREQUESTCODE, jobRequestData.JOBREQUESTID);
                        }
                    }

                    context.SaveChanges();
                    return true;
                }

                else return false;
            }
            else return false;
                
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ReverseChargeOnCustomerForCollateralSearch(JobRequestCollateralSearchViewModel model)
        {
            var jobRequestDetail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == model.jobRequestId && x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated).ToList();
            var consultantId = jobRequestDetail.FirstOrDefault().ACCREDITEDCONSULTANTID;
            var consultantRecord = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == consultantId);
            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                username = model.username,
                passcode = model.passCode
            };

            if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
            {
                twoFADetails.username = context.TBL_STAFF.Find(model.createdBy).STAFFCODE;
            }

            return ReverseChargeOnCustomerForCollatteralSearch(model, jobRequestDetail, twoFADetails, consultantRecord);

        }

        private bool ReverseChargeOnCustomerForCollatteralSearch(JobRequestCollateralSearchViewModel model,
           List<TBL_JOB_REQUEST_DETAIL> jobRequestDetail, TwoFactorAutheticationViewModel twoFADetails, IEnumerable<TBL_ACCREDITEDCONSULTANT> consultantRecord)
        {
            model.debitBusiness = jobRequestDetail.FirstOrDefault().DEBITBUSINESS;
            var casa = context.TBL_CASA.Find(jobRequestDetail.FirstOrDefault().CUSTOMERCASAACCOUNTID);
            if (casa == null && !model.debitBusiness) { throw new ConditionNotMetException("Customer account number missing"); }
            if (!model.debitBusiness) { model.casaAccountId = casa.CASAACCOUNTID; }

            var jobRequestData = context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            model.operationId = (short)OperationsEnum.CollateralSearchInitiation;
            model.requestCode = jobRequestData.JOBREQUESTCODE;

            foreach (var item in jobRequestDetail)
            {
                model.totalChargeAmount = model.totalChargeAmount + item.AMOUNT.Value;
                item.ACCREDITEDCONSULTANTPAID =  false;
                item.TRANSACTIONREVERSED = true;
            }

            if (model.totalChargeAmount > 0)
            {
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                if (model.debitBusiness)
                {
                    var bizAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (short)OtherOperationEnum.ChargeOnBank).FirstOrDefault();
                    if (bizAccount == null) throw new ConditionNotMetException("No Account has been mapped for charges on business");

                    model.glAccountId = bizAccount.GLACCOUNTID;
                    model.casaAccountId = null;
                    model.currencyId = (short)jobRequestDetail.FirstOrDefault().CURRENCYID;
                    model.currencyCode = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId).CURRENCYCODE;
                }
                inputTransactions.AddRange(BuildCollateralSearchChargeReversalPosting(model));

                if (inputTransactions.Count > 0)
                {
                    financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);
                    jobRequestData.REQUESTSTATUSID = (short)JobRequestStatusEnum.cancel;
                    if (jobRequestData.RESPONSECOMMENT == null) jobRequestData.RESPONSECOMMENT = "This request has been cancelled. The collateral search by solicitor is not confirmed";
                     // Audit Section ---------------------------
                     var auditDetail = !model.debitBusiness ? $"Customer account number '{casa.PRODUCTACCOUNTNUMBER}' credited with collateral search fees" : $"Bank account collateral search fees debit reversal";
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CollateralSearchJob,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------

                    context.SaveChanges();
                    return true;
                }

                else return false;
            }
            else return false;
        }



        [OperationBehavior(TransactionScopeRequired = true)]
        public bool ChargeCustomerForOnSearchJobs(JobRequestCollateralSearchViewModel model)
        {
            var jobRequestDetail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == model.jobRequestId && x.JOB_SUB_TYPEID == model.jobSubTypeId).ToList();
            var subJobRecord = context.TBL_JOB_TYPE_SUB.Find(model.jobSubTypeId);
            var consultantId = jobRequestDetail.FirstOrDefault().ACCREDITEDCONSULTANTID;
            var consultantRecord = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == consultantId);

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                username = model.username,
                passcode = model.passCode
            };
            if (context.TBL_SETUP_GLOBAL.FirstOrDefault().USERSPECIFIC2FA == true)
            {
                twoFADetails.username = context.TBL_STAFF.Find(model.createdBy).STAFFCODE;
            }

            if (model.isInitiation )
            {
                return initiateChargeOnCustomerForCollatteralSearch(model, jobRequestDetail, twoFADetails, consultantRecord);
            }
            else
            {
                return creditSolicitorForCollatteralSearch(model, jobRequestDetail, twoFADetails, consultantRecord);
            }
        }

        private bool initiateChargeOnCustomerForCollatteralSearch(JobRequestCollateralSearchViewModel model, 
            List<TBL_JOB_REQUEST_DETAIL> jobRequestDetail, TwoFactorAutheticationViewModel twoFADetails, IEnumerable<TBL_ACCREDITEDCONSULTANT> consultantRecord)
        {

            var auditDetail = string.Empty;
            var accountNumber = string.Empty;
            decimal accountBalance = 0;

            var casa = context.TBL_CASA.Find(model.casaAccountId);
            if (casa == null && !model.debitBusiness)
                throw new ConditionNotMetException("Customer account number is not supplied");

            if (casa != null) accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;
            var jobSubTypeId = jobRequestDetail.FirstOrDefault().JOB_SUB_TYPEID;
            var subTypeClass = jobRequestDetail.FirstOrDefault().JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated 
                ? "collateral search"
                : context.TBL_JOB_TYPE_SUB.FirstOrDefault(x=>x.JOB_SUB_TYPEID == jobSubTypeId).JOB_SUB_TYPE_NAME;

            if (!model.debitBusiness)
            {
                model.casaAccountId = casa.CASAACCOUNTID;
                accountNumber = casa.PRODUCTACCOUNTNUMBER;
                auditDetail =  $"Customer account number '{casa.PRODUCTACCOUNTNUMBER}' debited with {subTypeClass.ToLower()} fees";
            }
            else { auditDetail = $"Bank account debited with {subTypeClass} fees"; }

            var jobRequestData = context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            model.operationId = (short)OperationsEnum.CollateralSearchInitiation;
            model.requestCode = jobRequestData.JOBREQUESTCODE;

            foreach (var item in jobRequestDetail)
            {
                model.totalChargeAmount = model.totalChargeAmount + item.AMOUNT.Value;
                item.ACCREDITEDCONSULTANTPAID = !model.isInitiation ? true : false;
                item.ACCOUNTNUMBER = model.isInitiation ? accountNumber : null;
                item.CUSTOMERCASAACCOUNTID = model.casaAccountId;
            }

            if(model.totalChargeAmount > accountBalance && !model.debitBusiness)
                throw new ConditionNotMetException("The customer's Account is not funded.");

            if(model.totalChargeAmount > 0)
            {
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
                if (model.debitBusiness)
                {
                    var bizAccount = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (short)OtherOperationEnum.ChargeOnBank).FirstOrDefault();
                    if (bizAccount == null) throw new ConditionNotMetException("No Account has been mapped for charges on business");

                    model.glAccountId = bizAccount.GLACCOUNTID;
                    model.casaAccountId = null;
                    model.currencyId = (short)jobRequestDetail.FirstOrDefault().CURRENCYID;
                    model.currencyCode = context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId).CURRENCYCODE;
                }
                inputTransactions.AddRange(BuildCollateralSearchChargeFeesPosting(model));

                if (inputTransactions.Count > 0)
                {
                    financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);

                    if (consultantRecord.Any())
                    {
                        var solicitor = consultantRecord.FirstOrDefault();
                        var stateId = solicitor.TBL_ACCREDITEDCONSULTANT_STATE.FirstOrDefault(x => x.STATEID != 0)?.STATEID;
                        var state = context.TBL_STATE.Find(stateId);
                        string messageBoby = jobRequestDetail.FirstOrDefault().DESCRIPTION2;

                        string alertSubject = $"Loan {subTypeClass}";
                        LogEmailAlert(messageBoby, alertSubject, solicitor.EMAILADDRESS, jobRequestData.JOBREQUESTCODE, jobRequestData.JOBREQUESTID);
                    }

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CollateralSearchJob,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------
                    
                    context.SaveChanges();
                    return true;
                }

                else return false;
            }
            else return false;
        }

        private bool creditSolicitorForCollatteralSearch(JobRequestCollateralSearchViewModel model,
            List<TBL_JOB_REQUEST_DETAIL> jobRequestDetail, TwoFactorAutheticationViewModel twoFADetails, IEnumerable<TBL_ACCREDITEDCONSULTANT> consultantRecord)
        {
            
            var auditDetail = string.Empty;
            var accountNumber = string.Empty;

            var b = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;
            if (b == null || b == string.Empty || b == " ")
                throw new ConditionNotMetException("The solicitor's account number is not found. No account number has been mapped to this solicitor.");

            accountNumber = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;


            var witholdingAmount = (double)model.totalChargeAmount / 0.9;
            var id = (short)jobRequestDetail.FirstOrDefault().CURRENCYID.Value;
            var jobRequestData = context.TBL_JOB_REQUEST.Find(model.jobRequestId);

            var jobSubTypeId = jobRequestDetail.FirstOrDefault().JOB_SUB_TYPEID;
            var subTypeClass = jobRequestDetail.FirstOrDefault().JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated
                ? "collateral search"
                : context.TBL_JOB_TYPE_SUB.FirstOrDefault(x => x.JOB_SUB_TYPEID == jobSubTypeId).JOB_SUB_TYPE_NAME;

            model.currencyId = (short)jobRequestDetail.FirstOrDefault().CURRENCYID;
            var currency = context.TBL_CURRENCY.Find(model.currencyId);

            model.operationId = (short)OperationsEnum.CollateralSearchCompletion;
            model.requestCode = jobRequestData.JOBREQUESTCODE;
            model.feeNarration = $"Payment to solicitor";
            auditDetail = $"Solicitor account number '{accountNumber}' credited for {subTypeClass.ToLower()} job with '{currency.CURRENCYCODE}{witholdingAmount}'";

            foreach (var item in jobRequestDetail)
            {
                model.totalChargeAmount = model.totalChargeAmount + item.AMOUNT.Value;
                item.ACCREDITEDCONSULTANTPAID = !model.isInitiation ? true : false;
                item.ACCOUNTNUMBER = model.isInitiation ? accountNumber : null;
            }

            if (model.totalChargeAmount > 0)
            {
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                model.accountNumber = accountNumber;
                inputTransactions.AddRange(BuildSolicitorFeePaymentPosting(model));

                if (inputTransactions.Count > 0)
                {
                    financeTransaction.PostTransaction(inputTransactions, false, twoFADetails);

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CollateralSearchJob,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = auditDetail,
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------

                    context.SaveChanges();
                    return true;
                }
                else return false;
            }
            else return false;
        }
        //private string GetProposedConditionsMarkup()
        //{
        //    var conditions = GetProposedConditions(); // new

        //    var result = String.Empty;
        //    var n = 0;
        //    result = result + $@"
        //        <table border=1>
        //            <tr>
        //                <th>S/N</th>
        //                <th>Facility Type</th>
        //            </tr>
        //         ";
        //    foreach (var e in conditions)
        //    {
        //        n++;
        //        result = result + $@"
        //            <tr>
        //                <td>{n}</td>
        //                <td>{e.name}</td>
        //            </tr>
        //        ";
        //    }
        //    result = result + $"</table>";
        //    return result;

        //}

        public bool saveCollateralJobsChargesSpecifiedByLegal(JobRequestCollateralSearchViewModel model)
        {
            var jobRequest = context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            var baseApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(jobRequest.TARGETID);
            var state = context.TBL_STATE.Find(model.collateralStateId);
            var company = context.TBL_COMPANY.Find(model.companyId);
            //var casa = context.TBL_CASA.Find(model.casaAccountId);

            //if (casa == null)
            //    throw new SecureException("Customer account number is not supplied");

            // Decimal chargeAmount = 0;
            if(model.searchDetails.Count() <= 0)
            {
                throw new ConditionNotMetException("You must add at least one charge.");
            }
            foreach(var searchModel in model.searchDetails)
            {
                var detail = new JobRequestDetailViewModel
                {
                    jobSubTypeclassId = (short)searchModel.jobSubTypeclassId,
                    jobSubTypeId = (short)jobRequest.JOB_SUB_TYPEID,
                    jobTypeId = (short)jobRequest.JOBTYPEID,
                    amount = searchModel.amount,
                    jobRequestId = model.jobRequestId,
                    createdBy = model.createdBy,
                    accreditedConsultantId = searchModel.accreditedConsultantId,
                    accountNumber = model.accountNumber,
                    currencyId = company.CURRENCYID,
                    description2 = model.description2,
                    description = model.additionalChargeJustification,
                };
                saveJobRequestDetail(detail);
            }

            //var collateralStateDetails = context.TBL_STATE.Find(model.collateralStateId);
            //if (model.requireCharting)
            //{
            //    //chargeAmount = chargeAmount + (collateralStateDetails.CHARTINGAMOUNT ?? 0);
            //    var detail = new JobRequestDetailViewModel
            //    {
            //        jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralCharting,
            //        jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
            //        jobTypeId = (short)JobTypeEnum.legal,
            //        amount = model.chartChargeAmount, 
            //        jobRequestId = model.jobRequestId,
            //        createdBy = model.createdBy,
            //        accreditedConsultantId = model.solicitorId,
            //        accountNumber = model.accountNumber,
            //        currencyId = company.CURRENCYID,
            //        description2 = model.description2
            //    };
            //    saveJobRequestDetail(detail);
            //}

            //if (model.requireSearch)
            //{
            //    //chargeAmount = chargeAmount + (collateralStateDetails.COLLATERALSEARCHCHARGEAMOUNT);
            //    var detail = new JobRequestDetailViewModel
            //    {
            //        jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralSearch,
            //        jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
            //        jobTypeId = (short)JobTypeEnum.legal,
            //        amount = model.searchChargeAmount, 
            //        jobRequestId = model.jobRequestId,
            //        createdBy = model.createdBy,
            //        accreditedConsultantId = model.solicitorId,
            //        accountNumber = model.accountNumber,
            //        currencyId = company.CURRENCYID,
            //        description2 = model.description2
            //    };
            //    saveJobRequestDetail(detail);
            //}

            //if (model.requireVerification)
            //{
            //    //chargeAmount = chargeAmount + (collateralStateDetails.VERIFICATIONAMOUNT ?? 0);
            //    var detail = new JobRequestDetailViewModel
            //    {
            //        jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralVerification,
            //        jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
            //        jobTypeId = (short)JobTypeEnum.legal,
            //        amount = model.verificationChargeAmount,
            //        jobRequestId = model.jobRequestId,
            //        createdBy = model.createdBy,
            //        accreditedConsultantId = model.solicitorId,
            //        accountNumber = model.accountNumber,
            //        currencyId = company.CURRENCYID,
            //        description2 = model.description2
            //    };
            //    saveJobRequestDetail(detail);
            //}

            //if (model.additionalCharge > 0)
            //{
            //    //chargeAmount = chargeAmount + (model.additionalCharge ?? 0);
            //    var detail = new JobRequestDetailViewModel
            //    {
            //        jobSubTypeclassId = (short)JobSubTypeClassEnum.AdditionalCharges,
            //        jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
            //        jobTypeId = (short)JobTypeEnum.legal,
            //        amount = model.additionalCharge,
            //        description = model.additionalChargeJustification,
            //        jobRequestId = model.jobRequestId,
            //        createdBy = model.createdBy,
            //        accreditedConsultantId = model.solicitorId,
            //        accountNumber = model.accountNumber,
            //        currencyId = company.CURRENCYID,
            //        description2 = model.description2
            //    };
            //    saveJobRequestDetail(detail);
            //}

            jobRequest.REQUESTSTATUSID = (short)JobRequestStatusEnum.processing;

            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
            string messageBoby = $"Dear RM, <br /><br />This is to bring to your attention that legal has specified charges for collateral on job request with code '{jobRequest.JOBREQUESTCODE}'. <br /><br /> You attention is required to effect the charges. <br /><br />";
            string alertSubject = $"Collateral Search Request";
            LogEmailAlert(messageBoby, alertSubject, GetStaffEmailRecipients(jobRequest.SENDERSTAFFID), jobRequest.JOBREQUESTCODE, jobRequest.JOBREQUESTID);


            return context.SaveChanges() > 0;
        }

        private void saveJobRequestDetail(JobRequestDetailViewModel model)
        {
            var jobDetail = new TBL_JOB_REQUEST_DETAIL();
            jobDetail.AMOUNT = model.amount;
            jobDetail.DESCRIPTION = model.description;
            jobDetail.DESCRIPTION2 = model.description2;
            jobDetail.JOBREQUESTID = model.jobRequestId;
            jobDetail.JOB_SUB_TYPE_CLASSID = model.jobSubTypeclassId;
            jobDetail.JOB_SUB_TYPEID = model.jobSubTypeId;
            jobDetail.DESCRIPTION = model.description;
            jobDetail.ACCREDITEDCONSULTANTID = model.accreditedConsultantId;
            jobDetail.ACCOUNTNUMBER = model.accountNumber;
            jobDetail.CURRENCYID = model.currencyId;
            jobDetail.CREATEDBY = model.createdBy;
            jobDetail.DATETIMECREATED = DateTime.Now;
            context.TBL_JOB_REQUEST_DETAIL.Add(jobDetail);
        }

        private string GetStaffEmailRecipients(int staffId)
        {
            if (staffId != 0)
            {
                return context.TBL_STAFF.Where(x => x.STAFFID == staffId).Select(x => x.EMAIL).FirstOrDefault();
            }
            return "";
        }

        private string GetLoanApplicationEmailRecipients(int targetId)
        {
            string recipientEmailAddresses = string.Empty;
            int? staffId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == targetId).Select(x => x.REQUESTSTAFFID).FirstOrDefault();
            if (staffId != null)
            {
                return context.TBL_STAFF.Where(x => x.STAFFID == staffId).Select(x => x.EMAIL).FirstOrDefault();

            }
            else
            {
                int? approvalLevelId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == targetId).OrderByDescending(x => x.SYSTEMARRIVALDATETIME).Select(x => x.FROMAPPROVALLEVELID).FirstOrDefault();
                var staffIds = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == approvalLevelId).Select(x =>
                new StaffInfoViewModel
                {
                    staffId = (int)x.STAFFROLEID
                }).ToList();
                foreach (var a in staffIds)
                    recipientEmailAddresses = context.TBL_STAFF.Where(x => x.STAFFID == a.staffId).Select(x => x.EMAIL).FirstOrDefault() + ";";
                var emails = recipientEmailAddresses.TrimEnd(';');
                return emails;
            }

        }

        #endregion ...End of Collateral Search Job Charges...


        #region job-type
        public bool ConfirmLegalCollateralJobSearch(int jobRequestDetailId, bool payStatus, UserInfo model)
        {
            var data = this.context.TBL_JOB_REQUEST_DETAIL.Find(jobRequestDetailId);
            if (data == null)
            {
                return false;
            }

            data.ACCREDITEDCONSULTANTPAID = payStatus;

            //CreditSolicitor()

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralSearchJob,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Updated Job request details table with accredited consultant payment status as'{ payStatus }' on job request with code '{data.TBL_JOB_REQUEST.JOBREQUESTCODE}' for '{data.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<JobTypeViewModel> GetAllJobType()
        {
           return this.context.TBL_JOB_TYPE.Select(x => new JobTypeViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobTypeName = x.JOBTYPENAME,
                inUse = x.INUSE,
                canBeReasigned = x.CANBEREASSIGNED
            }).Where(c=>c.inUse == true);
        }

        public IEnumerable<JobTypeHubViewModel> GetAllJobTypeHub(short jobTypeId)
        {
            return this.context.TBL_JOB_TYPE_HUB.Select(x => new JobTypeHubViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobTypeHubId = x.JOBTYPEHUBID,
                jobTypeHubName = x.HUBNAME,
                deleted = x.DELETED
            }).Where(c => c.jobTypeId == jobTypeId && c.deleted == false).ToList();
        }

        public IEnumerable<HubStaffViewModel> GetHubStaffByHubId(short jobTypeHubId)
        {
            return this.context.TBL_JOB_TYPE_HUB_STAFF.Select(x => new HubStaffViewModel
            {
                hubStaffId = x.STAFFID,
                jobTypeHubId = x.JOBTYPEHUBID,
                jobTypeUnitId = x.JOBTYPEUNITID,
                isTeamLead = x.ISTEAMLEAD,
                deleted = x.DELETED,
                jobTypeUnitName = (from s in context.TBL_JOB_TYPE_UNIT where s.JOBTYPEUNITID == x.JOBTYPEUNITID select s.UNITNAME).FirstOrDefault(),
                hubStaffName = (from s in context.TBL_STAFF where s.STAFFID == x.STAFFID select s.FIRSTNAME+" "+s.LASTNAME +"("+ s.STAFFCODE+")" ).FirstOrDefault()
            }).Where(c => c.jobTypeHubId == jobTypeHubId && c.deleted == false);
        }

        public IEnumerable<HubStaffViewModel> GetHubStaffByHubTypeUnitId(short jobTypeUnitId)
        {
            return this.context.TBL_JOB_TYPE_HUB_STAFF.Select(x => new HubStaffViewModel
            {
                hubStaffId = x.STAFFID,
                jobTypeHubId = x.JOBTYPEHUBID,
                hubStaffName = (from s in context.TBL_STAFF where s.STAFFID == x.STAFFID select s.FIRSTNAME + " " + s.LASTNAME + "(" + s.STAFFCODE + ")").FirstOrDefault(),
                jobTypeUnitId = x.JOBTYPEUNITID,
                jobTypeUnitName = (from s in context.TBL_JOB_TYPE_UNIT where s.JOBTYPEUNITID == x.JOBTYPEUNITID select s.UNITNAME).FirstOrDefault(),
                isTeamLead = x.ISTEAMLEAD,
                deleted = x.DELETED,
            }).Where(c => c.jobTypeHubId == jobTypeUnitId && c.deleted == false);
        }

        public IEnumerable<JobTypeUnitViewModel> GetAllJobTypeUnit(short jobTypeId)
        {
            return this.context.TBL_JOB_TYPE_UNIT.Select(x => new JobTypeUnitViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobTypeUnitId = x.JOBTYPEUNITID,
                unitName = x.UNITNAME
            }).Where(c => c.jobTypeId == jobTypeId);
        }

        public List<jobReasignment> GetJobTypeReasignmentAdmin(int companyId)
        {
            var details = (from x in context.TBL_JOB_TYPE_REASSIGNMENT
                           where  x.COMPANYID == companyId && x.DELETED == false
                           select new jobReasignment
                           {
                               reasignmentId = x.REASSIGNMENTID,
                               staffId = x.STAFFID,
                               dateTimeDeleted = x.DATETIMECREATED,
                               jobTypeId = x.JOBTYPEID,
                               dateTimeCreated = x.DATETIMECREATED,
                               staffName = context.TBL_STAFF.Where(o => o.STAFFID == x.STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME + " - (" + o.STAFFCODE + ")").FirstOrDefault(),
                               jobTypeName = context.TBL_JOB_TYPE.Where(o => o.JOBTYPEID == x.JOBTYPEID).Select(o => o.JOBTYPENAME).FirstOrDefault(),

                           }).ToList();

            return details;
        }

        public bool mapJobTypeHubStaff(JobTypeHubViewModel model)
        {
            var applicationDate = general.GetApplicationDate();

            var newJobTypeHubStaff = new TBL_JOB_TYPE_HUB_STAFF
            {
                STAFFID = model.staffId,
                JOBTYPEUNITID = model.jobTypeUnitId,
                JOBTYPEHUBID = model.jobTypeHubId,
                ISTEAMLEAD = model.isTeamLead,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,

            };
            context.TBL_JOB_TYPE_HUB_STAFF.Add(newJobTypeHubStaff);

            var staff = context.TBL_STAFF.Find(model.staffId);
            var hub = context.TBL_JOB_TYPE_HUB.Find(model.jobTypeHubId);
            var unit = context.TBL_JOB_TYPE_UNIT.Find(model.jobTypeUnitId);
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestHubStaffAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Staff with staff code '{ staff.STAFFCODE }' of '{unit.UNITNAME}' unit was added to '{hub.HUBNAME}' hub  ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;

        }

        public bool DeleteMappedJobTypeHubStaff(int hubStaffId, int staffId)
        {
            var hubStaffRecord = context.TBL_JOB_TYPE_HUB_STAFF.Find(hubStaffId);
            hubStaffRecord.DELETED = true;
            hubStaffRecord.DELETEDBY = staffId;
            hubStaffRecord.DATETIMEDELETED = DateTime.Now;

            return context.SaveChanges() > 0;

        }

        public bool UpdatemappedJobTypeHubStaff(JobTypeHubViewModel model)
        {
            var data = context.TBL_JOB_TYPE_HUB_STAFF.Find(model.hubStaffId);
            if (data == null) throw new ConditionNotMetException("Could not find record to update");

            data.STAFFID = model.staffId;
            data.ISTEAMLEAD = model.isTeamLead;
            data.JOBTYPEHUBID = model.jobTypeHubId;
            data.JOBTYPEUNITID = model.jobTypeUnitId;
            

            var staff = context.TBL_STAFF.Find(model.staffId);
            var hub = context.TBL_JOB_TYPE_HUB.Find(model.jobTypeHubId);
            var unit = context.TBL_JOB_TYPE_UNIT.Find(model.jobTypeUnitId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestHubStaffUpdate,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"New update made to 'job type hub staff'  thus: staff code - '{ staff.STAFFCODE }', hub - '{hub.HUBNAME}', unit - '{unit.UNITNAME}'.",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);

            return context.SaveChanges() > 0;

        }


        public bool AssignJobTypeToStaff(jobReasignment model)
        {
            var applicationDate = general.GetApplicationDate();
            if (context.TBL_JOB_TYPE_REASSIGNMENT.Any(x => x.JOBTYPEID == model.jobTypeId && x.STAFFID == model.staffId))
                throw new ConditionNotMetException("This job type exist for this staff");

            var newType = new TBL_JOB_TYPE_REASSIGNMENT
            {
                STAFFID = model.staffId,
                JOBTYPEID = (short)model.jobTypeId,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                
            };
            context.TBL_JOB_TYPE_REASSIGNMENT.Add(newType);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffJobTypeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Joy Type has been assigned to a staff with ID '{ model.staffId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;

        }

        public bool DeleteJobTypeForAStaff(jobReasignment model)
        {
            var data = context.TBL_JOB_TYPE_REASSIGNMENT.Find(model.reasignmentId);
            if(data==null) throw new ConditionNotMetException("No record selected.");

            data.DATETIMEDELETED = model.dateTimeDeleted;
            data.DELETED = true;

            var jobType = context.TBL_JOB_TYPE.Find(data.JOBTYPEID);
            var staff = context.TBL_STAFF.Find(data.STAFFID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffJobTypeDeleted,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Deleted job Type admin with detail: JobType - '{jobType.JOBTYPENAME}' staff code '{ staff.STAFFCODE }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);

            if (context.SaveChanges() > 0) return true;

            return false;
        }

        public bool UpdateAsignedJobTypeToStaff(jobReasignment model)
        {
          var data = context.TBL_JOB_TYPE_REASSIGNMENT.Find(model.reasignmentId);
            if (data != null)
            {
                data.STAFFID = model.staffId;
                data.JOBTYPEID = (short)model.jobTypeId;

            }

            var jobType = context.TBL_JOB_TYPE.Find(model.jobTypeId);
            var staff = context.TBL_STAFF.Find(model.staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffJobTypeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"'{jobType?.JOBTYPENAME}' staff admin has been modified. New admin staff code : '{ staff.STAFFCODE }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);

            if (context.SaveChanges() > 0) return true;

            return false;
        }

        public List<jobReasignment> GetJobReasignmentStaffById(int staffId, int companyId)
        {
            var jobTypeAdminStaff = GetJobTypeReasignmentAdmin(companyId).Where(x => x.staffId == staffId).ToList();
            return jobTypeAdminStaff;
        }

        public IEnumerable<JobTypeHubViewModel> GetJobTypeHubStaff()
        {
            return this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x=>x.DELETED == false ).Select(x => new JobTypeHubViewModel
            {
                hubStaffId = x.HUBSTAFFID,
                jobTypeHubId = x.JOBTYPEHUBID,
                staffId = x.STAFFID,
                jobTypeHubName = context.TBL_JOB_TYPE_HUB.Where(o => o.JOBTYPEHUBID == x.JOBTYPEHUBID).Select(o => o.HUBNAME).FirstOrDefault(),
                staffName = context.TBL_STAFF.Where(o => o.STAFFID == x.STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME +" ("+o.STAFFCODE+")").FirstOrDefault(),
                jobTypeUnitId = x.JOBTYPEUNITID,
                jobTypeUnitName = context.TBL_JOB_TYPE_UNIT.Where(o => o.JOBTYPEUNITID == x.JOBTYPEUNITID).Select(o => o.UNITNAME).FirstOrDefault(),
                isTeamLead = x.ISTEAMLEAD
            });
        }

        public IEnumerable<JobTypeViewModel> GetJobSubType(short jobId)
        {
            return context.TBL_JOB_TYPE_SUB.Select(x => new JobSubTypeViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobSubTypeName = x.JOB_SUB_TYPE_NAME,
                jobSubTypeId = x.JOB_SUB_TYPEID,
                requireCharge = x.REQUIRECHARGE ?? false,
                chargeFeeId = x.CHARGEFEEID
            }).Where(x => x.jobTypeId == jobId );
        }

        public IEnumerable<JobSubTypeClassViewModel> GetJobSubTypeClass(short jobSubTypeId)
        {
            return context.TBL_JOB_TYPE_SUB_CLASS.Select(x => new JobSubTypeClassViewModel
            {
                jobSubTypeclassId = x.JOB_SUB_TYPE_CLASSID,
                jobSubTypeclassName = x.JOB_SUB_TYPE_CLASS_NAME,
                jobSubTypeId = x.JOB_SUB_TYPEID,
                defaultChargeAmount = x.DEFAULTCHARGEAMOUNT,
            }).Where(x => x.jobSubTypeId == jobSubTypeId);
        }


        #endregion job-type


        #region ...Middle Office Updates...
        public bool UpdateInvoiceStatus(JobRequestInvoiceViewModel model)
        {
            var job = this.context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            var invoice = this.context.TBL_LOAN_APPLICATION_DETL_INV.Where(x=> x.INVOICEID == model.invoiceId).FirstOrDefault();
            if (invoice != null)
            {
                if (!model.status)
                {
                    invoice.APPROVALSTATUSID = (short)RequestStatusEnum.Disapproved;
                    job.REQUESTSTATUSID = (short)RequestStatusEnum.Disapproved;
                    job.JOB_STATUS_FEEDBACKID = model.rejectionId ?? null;
                }
                else
                {
                    invoice.APPROVALSTATUSID = (short)RequestStatusEnum.Approved;
                }
                return context.SaveChanges() > 0;

            }
            else return false;
        }

        #endregion ...End of Middle Office Updates...


        #region ...Job-Request Document...

        public bool AddJobReplyAndDocument(RequestDocumentViewModel model, byte[] file)
        {
            if (AddJobDocumentOnly(model, file))
            {
                JobRequestViewModel jb = new JobRequestViewModel();
                jb.jobRequestId = context.TBL_JOB_REQUEST.Where(x => x.JOBREQUESTCODE == model.jobRequestCode).FirstOrDefault().JOBREQUESTID;
                jb.responseComment = model.comment;
                jb.createdBy = model.createdBy;
                jb.companyId = model.companyId;
                jb.userBranchId = model.userBranchId;
                jb.statusId = (short)model?.statusId;
                jb.rejectionReasonId =  model?.rejectionReasonId;
                jb.responseStaffId = model.createdBy;

                return ReplyJobRequest(jb, jb.jobRequestId);
            }
            else return false;
        }

        public bool AddJobDocumentOnly(RequestDocumentViewModel model, byte[] file)
        {
            string[] chanelArray = new string[] { "docx", "pdf", "jpg", "jpeg", "png", "txt", "xlsx", "xls", "doc", "xml" };
            if (!chanelArray.Contains(model.fileExtension))
            {
                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + chanelArray);
            }
            var data = new Entities.DocumentModels.TBL_MEDIA_JOB_REQUEST_DOCUMENT
            {
                FILEDATA = file,
                JOBREQUESTCODE = model.jobRequestCode,
                DOCUMENTTITLE = model.documentTitle,
                DOCUMENTTYPEID = model.documentTypeId,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                SYSTEMDATETIME = DateTime.Now,
                PHYSICALFILENUMBER = model.physicalFileNumber,
                PHYSICALLOCATION = model.physicalLocation,
                CREATEDBY = (int)model.createdBy,
            };

            docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Document '{ model.documentTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            context.SaveChanges();

            return docContext.SaveChanges() != 0;
        }

        public string AddJobDocument(RequestDocumentViewModel model, JobRequestViewModel requestModel, byte[] file)
        {
            string[] chanelArray = new string[] { "docx", "pdf", "jpg", "jpeg", "png", "txt", "xlsx", "xls", "doc", "xml" };
            if (!chanelArray.Contains(model.fileExtension))
            {
                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + chanelArray);
            }
            var code = AddGlobalJobRequest(requestModel);
            model.jobRequestCode = code;
            var data = new Entities.DocumentModels.TBL_MEDIA_JOB_REQUEST_DOCUMENT
            {
                FILEDATA = file,
                JOBREQUESTCODE = model.jobRequestCode,
                DOCUMENTTITLE = model.documentTitle,
                DOCUMENTTYPEID = model.documentTypeId,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                SYSTEMDATETIME = DateTime.Now,
                PHYSICALFILENUMBER = model.physicalFileNumber,
                PHYSICALLOCATION = model.physicalLocation,
                CREATEDBY = (int)model.createdBy,
            };

            docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Loan Document '{ model.documentTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            var aud = context.SaveChanges() != 0;

            if (docContext.SaveChanges() != 0)
            {
                return code;
            }
            else return string.Empty;
        }

        public bool UpdateJobDocument(RequestDocumentViewModel model, int documentId)
        {
            string[] chanelArray = new string[] { "docx", "pdf", "jpg", "jpeg", "png", "txt", "xlsx", "xls", "doc", "xml" };
            if (!chanelArray.Contains(model.fileExtension))
            {
                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + chanelArray);
            }
            var data = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Find(documentId);
            if (data == null)
            {
                return false;
            }

            //data.LoanApplicationNumber = model.loanApplicationNumber;
            //data.LoanReferenceNumber = model.loanReferenceNumber;
            data.JOBREQUESTCODE = model.jobRequestCode;
            data.DOCUMENTTITLE = model.documentTitle;
            data.DOCUMENTTYPEID = model.documentTypeId;
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;
            data.PHYSICALFILENUMBER = model.physicalFileNumber;
            data.PHYSICALLOCATION = model.physicalLocation;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated LoanDocument '{ model.documentTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            var aud = context.SaveChanges() != 0;

            return docContext.SaveChanges() != 0;
        }

        public IEnumerable<RequestDocumentViewModel> GetAllJobDocument()
        {
            var c = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Select(x => new RequestDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                jobRequestCode = x.JOBREQUESTCODE,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                //fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            });
            var n = c.ToList();
            return c;
        }

        public RequestDocumentViewModel GetJobDocument(int documentId)
        {
            var data = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new RequestDocumentViewModel
            {
                documentId = data.DOCUMENTID,
                //loanApplicationNumber = data.LoanApplicationNumber,
                //loanReferenceNumber = data.LoanReferenceNumber,
                jobRequestCode = data.JOBREQUESTCODE,
                documentTitle = data.DOCUMENTTITLE,
                documentTypeId = data.DOCUMENTTYPEID,
                //fileData = data.FILEDATA,
                fileName = data.FILENAME,
                fileExtension = data.FILEEXTENSION,
                systemDateTime = data.SYSTEMDATETIME,
                physicalFileNumber = data.PHYSICALFILENUMBER,
                physicalLocation = data.PHYSICALLOCATION,
            };
        }

        public IEnumerable<LMSOperationListViewModel> getLMSRApplicationDetail(int targetId)
        {
            var data =  (from x in this.context.TBL_LMSR_APPLICATION_DETAIL
                    where x.LOANREVIEWAPPLICATIONID == targetId
                    select new LMSOperationListViewModel
                    {
                        loanSystemTypeId = x.LOANSYSTEMTYPEID,
                        operationId = (short)x.OPERATIONID,
                        loanId = x.LOANID,
                        customerId = x.CUSTOMERID
                    });
            return data;
        }

        public IEnumerable<LMSOperationListViewModel> getLMSROperation(int targetId)
        { 
            var data = (from x in this.context.TBL_LOAN_REVIEW_OPERATION
                        where x.LOANREVIEWOPERATIONID == targetId
                        select new LMSOperationListViewModel
                        {
                            loanSystemTypeId = (short) x.LOANSYSTEMTYPEID,
                            operationId = (short)x.OPERATIONTYPEID,
                            loanId = x.LOANID,
                        });
            return data;
        }

        public IEnumerable<LMSOperationListViewModel> getLOSOperationLoanData(int loanId, int operationId)
        {
            List<LMSOperationListViewModel> data = new List<LMSOperationListViewModel>();
            if (operationId == (short)OperationsEnum.TermLoanBooking 
                || operationId == (short)OperationsEnum.CommercialLoanBooking 
                || operationId == (short)OperationsEnum.ForeignExchangeLoanBooking)
            {
                 data = (from x in this.context.TBL_LOAN
                            where x.TERMLOANID == loanId
                            select new LMSOperationListViewModel
                            {
                                loanSystemTypeId = (short)x.LOANSYSTEMTYPEID,
                                operationId = (short)operationId,
                                loanId = x.TERMLOANID,
                            }).ToList();
               
            }

            if (operationId == (short)OperationsEnum.RevolvingLoanBooking)
            {
                data = (from x in this.context.TBL_LOAN_REVOLVING
                        where x.REVOLVINGLOANID == loanId
                        select new LMSOperationListViewModel
                        {
                            loanSystemTypeId = (short)x.LOANSYSTEMTYPEID,
                            operationId = (short)OperationsEnum.RevolvingLoanBooking,
                            loanId = x.REVOLVINGLOANID,
                        }).ToList();

            }

            if (operationId == (short)OperationsEnum.ContigentLoanBooking)
            {
                data = (from x in this.context.TBL_LOAN_CONTINGENT
                        where x.CONTINGENTLOANID == loanId
                        select new LMSOperationListViewModel
                        {
                            loanSystemTypeId = (short)x.LOANSYSTEMTYPEID,
                            operationId = (short)OperationsEnum.ContigentLoanBooking,
                            loanId = x.CONTINGENTLOANID,
                        }).ToList();

            }
            return data;

        }

        public bool deleteJobDocument(int documentId, int staffId)
        {
            var data = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Find(documentId);
            if (data.CREATEDBY != staffId)
                throw new ConditionNotMetException("You did not upload the document you are trying to delete.");

                this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Remove(data);
                return docContext.SaveChanges() > 0;
        }

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocuments(string jobRequestCode)
        {

            var c = from x in this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT
                    where x.JOBREQUESTCODE == jobRequestCode
                    select new RequestDocumentViewModel
                    {
                        documentId = x.DOCUMENTID,
                        jobRequestCode = x.JOBREQUESTCODE,
                        documentTitle = x.DOCUMENTTITLE,
                        documentTypeId = x.DOCUMENTTYPEID,
                        //fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        systemDateTime = x.SYSTEMDATETIME,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                    };
            var n = c.ToList();
            return c;
        }

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocumentById(int documentId)
        {
            var data =  this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(x => x.DOCUMENTID == documentId).Select(x => new RequestDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                jobRequestCode = x.JOBREQUESTCODE,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            });
            return data.ToList();
        }

        #endregion ...Job-Request Document...


        #region ...Account Posting (Debit & Credit)...
        public List<FinanceTransactionViewModel> BuildCollateralSearchChargeFeesPosting(JobRequestCollateralSearchViewModel model)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            var searchCharges = context.TBL_CHARGE_FEE.Where(x => x.OPERATIONID == model.operationId);
            if (searchCharges.Any() && model.totalChargeAmount != 0)
            {
                var chargeFeeId = searchCharges.FirstOrDefault().CHARGEFEEID;
                var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();
                foreach (var post in postingGroups)
                {
                    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                    {
                        FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                        decimal debitAmount = 0;
                        if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            debitAmount = (decimal)model.totalChargeAmount * (decimal)(debits.VALUE / 100.0);
                        else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            debitAmount = (decimal)model.totalChargeAmount;

                        debit.operationId = (int)model.operationId;
                        debit.description = "Collateral related charge on facility"; // model.feeNarration; // $"Fee charge on {debits.DESCRIPTION}";
                        debit.valueDate = general.GetApplicationDate();
                        debit.transactionDate = debit.valueDate;
                        debit.currencyId = model.debitBusiness ? (short)model.currencyId : casa.CURRENCYID;
                        debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                        debit.isApproved = true;
                        debit.postedBy = model.createdBy;
                        debit.approvedBy = model.createdBy;
                        debit.approvedDate = debit.transactionDate;
                        debit.approvedDateTime = DateTime.Now;
                        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        debit.companyId = model.companyId;

                        if (!model.debitBusiness && context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL == null)
                            throw new BadLogicException($"No GL is currently mapped to this product code '{casa.TBL_PRODUCT.PRODUCTCODE}'.");

                        debit.glAccountId = model.debitBusiness ? model.glAccountId : context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        debit.sourceReferenceNumber = model.requestCode;
                        debit.batchCode = batchCode;
                        if (!model.debitBusiness) debit.casaAccountId = casa.CASAACCOUNTID;
                        debit.debitAmount = debitAmount;
                        debit.creditAmount = 0;
                        debit.sourceBranchId = model.userBranchId;
                        debit.destinationBranchId = model.debitBusiness ? model.userBranchId : casa.BRANCHID;
                        debit.rateCode = "TTB";
                        debit.rateUnit = string.Empty;
                        debit.currencyCrossCode = model.debitBusiness ? model.currencyCode : casa.TBL_CURRENCY.CURRENCYCODE;

                        inputTransactions.Add(debit);
                    }

                    foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                    {
                        FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                        decimal creditAmount = 0;
                        if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            creditAmount = (decimal)model.totalChargeAmount * (decimal)(credits.VALUE / 100.0);
                        else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            creditAmount = (decimal)model.totalChargeAmount;


                        credit.operationId = (int)model.operationId;
                        credit.description = "Collateral related charge on facility"; // model.feeNarration;  //$"Fee charge on {credits.DESCRIPTION}";
                        credit.valueDate = general.GetApplicationDate();
                        credit.transactionDate = credit.valueDate;
                        credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID; // (short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId); //casa.CURRENCYID;
                        credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                        credit.isApproved = true;
                        credit.postedBy = model.createdBy;
                        credit.approvedBy = model.createdBy;
                        credit.approvedDate = credit.transactionDate;
                        credit.approvedDateTime = DateTime.Now;
                        credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        credit.companyId = model.companyId;
                        if(credits.GLACCOUNTID1 == null || credits.GLACCOUNTID1 == 0)
                        {
                            throw new ConditionNotMetException("There is no GL defined for "+ credits.DESCRIPTION + " record("+ credits.CHARGEFEEDETAILID+").");
                        }
                        credit.glAccountId = (int)credits.GLACCOUNTID1;
                        credit.sourceReferenceNumber = model.requestCode;
                        credit.batchCode = batchCode;
                        credit.casaAccountId = null;
                        credit.debitAmount = 0;
                        credit.creditAmount = creditAmount;
                        credit.sourceBranchId = model.userBranchId;
                        credit.destinationBranchId = model.userBranchId;
                        credit.rateCode = "TTB";
                        credit.rateUnit = string.Empty;
                        credit.currencyCrossCode = model.debitBusiness ? model.currencyCode : casa.TBL_CURRENCY.CURRENCYCODE;

                        inputTransactions.Add(credit);
                    }
                }
            }

            return inputTransactions;
        }

        public List<FinanceTransactionViewModel> BuildCollateralSearchChargeReversalPosting(JobRequestCollateralSearchViewModel model)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            var searchCharges = context.TBL_CHARGE_FEE.Where(x => x.OPERATIONID == model.operationId);
            if (searchCharges.Any() && model.totalChargeAmount != 0)
            {
                var chargeFeeId = searchCharges.FirstOrDefault().CHARGEFEEID;
                var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();
                foreach (var post in postingGroups)
                {
                    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                    {
                        FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                        decimal debitAmount = 0;
                        if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            debitAmount = (decimal)model.totalChargeAmount * (decimal)(debits.VALUE / 100.0);
                        else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            debitAmount = (decimal)model.totalChargeAmount;

                        debit.operationId = (int)model.operationId;
                        debit.description = "Collateral related charge reversal"; // model.feeNarration; // $"Fee charge on {debits.DESCRIPTION}";
                        debit.valueDate = general.GetApplicationDate();
                        debit.transactionDate = debit.valueDate;
                        debit.currencyId = model.debitBusiness ? (short)model.currencyId : casa.CURRENCYID;
                        debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                        debit.isApproved = true;
                        debit.postedBy = model.createdBy;
                        debit.approvedBy = model.createdBy;
                        debit.approvedDate = debit.transactionDate;
                        debit.approvedDateTime = DateTime.Now;
                        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        debit.companyId = model.companyId;

                        if (!model.debitBusiness && context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL == null)
                            throw new BadLogicException($"No GL is currently mapped to this product code '{casa.TBL_PRODUCT.PRODUCTCODE}'.");

                        debit.glAccountId = model.debitBusiness ? model.glAccountId : context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        debit.sourceReferenceNumber = model.requestCode;
                        debit.batchCode = batchCode;
                        if (!model.debitBusiness) debit.casaAccountId = casa.CASAACCOUNTID;
                        debit.debitAmount = 0;
                        debit.creditAmount = debitAmount;
                        debit.sourceBranchId = model.userBranchId;
                        debit.destinationBranchId = model.debitBusiness ? model.userBranchId : casa.BRANCHID;
                        debit.rateCode = "TTB";
                        debit.rateUnit = string.Empty;
                        debit.currencyCrossCode = model.debitBusiness ? model.currencyCode : casa.TBL_CURRENCY.CURRENCYCODE;

                        inputTransactions.Add(debit);
                    }

                    foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                    {
                        FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                        decimal creditAmount = 0;
                        if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            creditAmount = (decimal)model.totalChargeAmount * (decimal)(credits.VALUE / 100.0);
                        else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            creditAmount = (decimal)model.totalChargeAmount;


                        credit.operationId = (int)model.operationId;
                        credit.description = "Collateral related charge reversal"; // model.feeNarration;  //$"Fee charge on {credits.DESCRIPTION}";
                        credit.valueDate = general.GetApplicationDate();
                        credit.transactionDate = credit.valueDate;
                        credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID; // (short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId); //casa.CURRENCYID;
                        credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                        credit.isApproved = true;
                        credit.postedBy = model.createdBy;
                        credit.approvedBy = model.createdBy;
                        credit.approvedDate = credit.transactionDate;
                        credit.approvedDateTime = DateTime.Now;
                        credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        credit.companyId = model.companyId;
                        credit.glAccountId = (int)credits.GLACCOUNTID1;
                        credit.sourceReferenceNumber = model.requestCode;
                        credit.batchCode = batchCode;
                        credit.casaAccountId = null;
                        credit.debitAmount = creditAmount;
                        credit.creditAmount = 0;
                        credit.sourceBranchId = model.userBranchId;
                        credit.destinationBranchId = model.userBranchId;
                        credit.rateCode = "TTB";
                        credit.rateUnit = string.Empty;
                        credit.currencyCrossCode = model.debitBusiness ? model.currencyCode : casa.TBL_CURRENCY.CURRENCYCODE;

                        inputTransactions.Add(credit);
                    }
                }
            }

            return inputTransactions;
        }


        public List<FinanceTransactionViewModel> BuildSolicitorFeePaymentPosting(JobRequestCollateralSearchViewModel model)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CURRENCYID == model.currencyId);
            var currency = this.context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == model.currencyId);

            var searchCharges = context.TBL_CHARGE_FEE.Where(x => x.OPERATIONID == model.operationId);
            if (searchCharges.Any() && model.totalChargeAmount != 0)
            {
                var chargeFeeId = searchCharges.FirstOrDefault().CHARGEFEEID;
                var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();
                foreach (var post in postingGroups)
                {
                    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                    {
                        FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                        decimal debitAmount = 0;
                        if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            debitAmount = (decimal)model.totalChargeAmount * (decimal)(debits.VALUE / 100.0);
                        else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            debitAmount = (decimal)model.totalChargeAmount;

                        debit.operationId = (int)model.operationId;
                        debit.description = model.feeNarration;
                        debit.valueDate = general.GetApplicationDate();
                        debit.transactionDate = debit.valueDate;
                        debit.currencyId = (short)model.currencyId.Value;
                        debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                        debit.isApproved = true;
                        debit.postedBy = model.createdBy;
                        debit.approvedBy = model.createdBy;
                        debit.approvedDate = debit.transactionDate;
                        debit.approvedDateTime = DateTime.Now;
                        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        debit.companyId = model.companyId;


                        debit.glAccountId = debits.GLACCOUNTID1.Value;
                        debit.sourceReferenceNumber = model.requestCode;
                        debit.batchCode = batchCode;
                        debit.casaAccountId = null;
                        debit.debitAmount = debitAmount;
                        debit.creditAmount = 0;
                        debit.sourceBranchId = model.userBranchId;
                        debit.destinationBranchId = model.userBranchId;
                        debit.rateCode = "TTB";
                        debit.rateUnit = string.Empty;
                        debit.currencyCrossCode = currency.CURRENCYCODE;

                        inputTransactions.Add(debit);
                    }

                    foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                    {
                        FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                        decimal creditAmount = 0;
                        if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                            creditAmount = (decimal)model.totalChargeAmount * (decimal)(credits.VALUE / 100.0);
                        else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                            creditAmount = (decimal)model.totalChargeAmount;


                        credit.operationId = (int)model.operationId;
                        credit.description = model.feeNarration;  //$"Fee charge on {credits.DESCRIPTION}";
                        credit.valueDate = general.GetApplicationDate();
                        credit.transactionDate = credit.valueDate;

                        if (credits.DETAILTYPEID != (short)ChargeFeeDetailTypeEnum.Customer)
                        {
                            credit.glAccountId = (int)credits.GLACCOUNTID1;
                            credit.casaAccountId = null;
                            //credit.currencyId = (short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId);
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, model.currencyId.Value, model.companyId).sellingRate;
                        }
                        else
                        {
                            credit.accountNumber = model.accountNumber;
                            credit.useDirectAccount = true;
                            var casa = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == 1); // .TBL_PRODUCT.Find((short)DefaultProductEnum.CASA);
                            //credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == (short)DefaultProductEnum.CASA).PRINCIPALBALANCEGL.Value;
                            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID);
                            credit.glAccountId = product.PRINCIPALBALANCEGL.Value;
                            credit.casaAccountId = casa.CASAACCOUNTID;
                            //credit.currencyId = (short)chartOfAccount.GetAccountDefaultCurrency((int)credit.glAccountId, model.companyId);
                            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, model.currencyId.Value, model.companyId).sellingRate;
                        }

                        //casa.CURRENCYID;
                        //credit.glAccountId = (int)credits.GLACCOUNTID1;
                        credit.currencyId = model.currencyId.Value; //  (short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, model.companyId);
                        credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                        credit.isApproved = true;
                        credit.postedBy = model.createdBy;
                        credit.approvedBy = model.createdBy;
                        credit.approvedDate = credit.transactionDate;
                        credit.approvedDateTime = DateTime.Now;
                        credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        credit.companyId = model.companyId;

                        credit.sourceReferenceNumber = model.requestCode;
                        credit.batchCode = batchCode;

                        credit.debitAmount = 0;
                        credit.creditAmount = creditAmount;
                        credit.sourceBranchId = model.userBranchId;
                        credit.destinationBranchId = model.userBranchId;
                        credit.rateCode = "TTB";
                        credit.rateUnit = string.Empty;
                        credit.currencyCrossCode = currency.CURRENCYCODE;

                        inputTransactions.Add(credit);
                    }
                }
            }

            return inputTransactions;
        }

        private void DebitCustomer(int glAccountIdCR, TBL_CASA casa, decimal chargeAmount, BasicTrasactionSourceInputModel inputs)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CollateralSearchInitiation;
            debit.description = inputs.description;
            debit.valueDate = general.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, inputs.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = inputs.createdBy;
            debit.approvedBy = inputs.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = inputs.sourceApplicationId;
            debit.companyId = inputs.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = inputs.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CollateralSearchInitiation;
            credit.description = inputs.description;
            credit.valueDate = general.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, inputs.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = inputs.createdBy;
            credit.approvedBy = inputs.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = inputs.sourceApplicationId;
            credit.companyId = inputs.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = glAccountIdCR;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = inputs.userBranchId;
            credit.destinationBranchId = inputs.userBranchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }

        private void ReverseDebit(TBL_CREDIT_BUREAU creditBureau, TBL_CASA casa, decimal chargeAmount, BasicTrasactionSourceInputModel inputs)
        {
            var transactionCode = CommonHelpers.GenerateRandomDigitCode(10);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.CollateralSearchInitiation;
            debit.description = inputs.description;
            debit.valueDate = general.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, inputs.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = inputs.createdBy;
            debit.approvedBy = inputs.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = inputs.sourceApplicationId;
            debit.companyId = inputs.companyId;
            debit.batchCode = transactionCode;
            debit.glAccountId = creditBureau.GLACCOUNTID;
            debit.sourceReferenceNumber = transactionCode;
            debit.casaAccountId = null;
            debit.debitAmount = chargeAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = inputs.userBranchId;
            debit.destinationBranchId = casa.BRANCHID;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.CollateralSearchInitiation;
            credit.description = inputs.description;
            credit.valueDate = general.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = financeTransaction.GetExchangeRate(credit.valueDate, credit.currencyId, inputs.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = inputs.createdBy;
            credit.approvedBy = inputs.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = inputs.sourceApplicationId;
            credit.companyId = inputs.companyId;
            credit.batchCode = transactionCode;
            credit.glAccountId = (int)casa.TBL_PRODUCT.PRINCIPALBALANCEGL;
            credit.sourceReferenceNumber = transactionCode;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = chargeAmount;
            credit.sourceBranchId = inputs.userBranchId;
            credit.destinationBranchId = inputs.userBranchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            financeTransaction.PostTransaction(inputTransactions);
        }
        #endregion ...End of Account Posting (Debit & Credit)...


        #region ...JOB REQUEST SETUPS...
        public bool UpdateJobType(JobTypeViewModel model, short jobTypeId)
        {
            var data = this.context.TBL_JOB_TYPE.Find(jobTypeId);
            if (data == null)
            {
                return false;
            }

            data.JOBTYPENAME = model.jobTypeName;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobTypeUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated JobType '{ model.jobTypeName }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool AddJobType(JobTypeViewModel model)
        {
            var data = new TBL_JOB_TYPE
            {
                JOBTYPENAME = model.jobTypeName,
            };

            context.TBL_JOB_TYPE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobTypeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added JobType '{ model.jobTypeName }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        #endregion ...END OF JOB REQUEST SETUPS...


        #region Job Request Feedback
        public IEnumerable<LookupViewModel> GetJobRequestStatus()
        {
            var status = (from rs in context.TBL_JOB_REQUEST_STATUS
                          select new LookupViewModel
                          {
                              lookupId = rs.REQUESTSTATUSID,
                              lookupName = rs.STATUSNAME
                          }).ToList();
            return status;
        }

        public IEnumerable<JobRequestStatusFeedbackViewModel> GetAllJobRequestStatusFeedback()
        {
            var feedback = (from x in context.TBL_JOB_REQUEST_STATUS_FEEDBAK
                            join jt in context.TBL_JOB_TYPE on x.JOBTYPEID equals jt.JOBTYPEID
                            join rs in context.TBL_JOB_REQUEST_STATUS on x.REQUESTSTATUSID equals rs.REQUESTSTATUSID
                            select new JobRequestStatusFeedbackViewModel
                            {
                                jobTypeId = x.JOBTYPEID,
                                jobStatusFeedbackId = x.JOB_STATUS_FEEDBACKID,
                                requestStatusId = x.REQUESTSTATUSID,
                                jobStatusFeedbackName = x.JOB_STATUS_FEEDBACK_NAME,
                                jobTypeName = jt.JOBTYPENAME,
                                requestStatusName = rs.STATUSNAME
                            }).ToList();

            return feedback;
        }

        public bool AddUpdateJobRequestFeedBack(JobRequestStatusFeedbackViewModel feedback)
        {
            if (feedback == null) return false;
            try
            {
                TBL_JOB_REQUEST_STATUS_FEEDBAK jobFeedback = null;

                if (feedback.jobStatusFeedbackId > 0)
                {
                    jobFeedback = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Find(feedback.jobStatusFeedbackId);
                    if (jobFeedback != null)
                    {
                        jobFeedback.JOBTYPEID = feedback.jobTypeId;
                        jobFeedback.JOB_STATUS_FEEDBACK_NAME = feedback.jobStatusFeedbackName;
                        jobFeedback.REQUESTSTATUSID = feedback.requestStatusId;
                    }
                }
                else
                {
                    int position = (short)context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Count();
                    jobFeedback = new TBL_JOB_REQUEST_STATUS_FEEDBAK()
                    {
                        JOBTYPEID = feedback.jobTypeId,
                        JOB_STATUS_FEEDBACKID = (short)position++, //feedback.jobStatusFeedbackId,
                        JOB_STATUS_FEEDBACK_NAME = feedback.jobStatusFeedbackName,
                        REQUESTSTATUSID = feedback.requestStatusId,
                    };
                    context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Add(jobFeedback);
                }
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CompanyDirectorAddedUpdated,
                    STAFFID = feedback.createdBy,
                    BRANCHID = (short)feedback.userBranchId,
                    DETAIL = $"Added/Updated new job request feedback: {feedback.jobStatusFeedbackName}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = feedback.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };
                this.audit.AddAuditTrail(audit);
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool ValidateJobRequestFeedBack(string feedback)
        {
            var isExist = (from a in context.TBL_JOB_REQUEST_STATUS_FEEDBAK
                           where a.JOB_STATUS_FEEDBACK_NAME.ToLower().Trim() == feedback.ToLower().Trim()
                           select a).ToList();
            if (isExist.Any())
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}


