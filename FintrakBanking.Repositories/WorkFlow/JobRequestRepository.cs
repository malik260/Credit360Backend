using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Entities.DocumentModels;
using System.Data.Entity;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Repositories.Credit;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Finance;
using System.Configuration;

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
                DEPARTMENTID = model.departmentId,
                DEPARTMENTUNITID = model.departmentUnitId,
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
            };
            var job = context.TBL_JOB_REQUEST.Add(data); 

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added JobRequest '{ model.jobRequestCode }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------
            if (context.SaveChanges() > 0)
            {
                return job.JOBREQUESTCODE;
            }
            else return string.Empty;
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
            data.SYSTEMRESPONSEDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Reply JobRequest '{ data.JOBREQUESTCODE }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
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
                toStaffCode = toStaffData.STAFFCODE;

            if (fromStaffData != null){
                if (toStaffCode == fromStaffData.STAFFCODE)
                    throw new ConditionNotMetException("This job is currently assigned to the selected staff. Choose another staff staff to reassign.");

                info = $"Reassigned JobRequest with code '{ model.jobRequestCode }' from staff with code '{fromStaffData.STAFFCODE}' to staff with code '{toStaffCode}'";
            }
            else { info = $"Assigned JobRequest with code '{ model.jobRequestCode }' to staff with code '{toStaffCode}'"; }
                

            var applicationDate = general.GetApplicationDate();

            data.REASSIGNEDTO = (int)model.reassignedTo;
            data.ISREASSIGNED = true;
            data.ISACKNOWLEDGED = true;
            data.REQUESTSTATUSID = (short)JobRequestStatusEnum.processing;
            data.REASSIGNEDDATE = applicationDate;
            data.SYSTEMREASSIGNEDDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = info,
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
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
                var allJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId).Count();

                var allPendingJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId
                               && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending).Count();

                var allApprovedJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId
                              && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved).Count();

                var allDisapproveJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId
                              && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Count();

                var allProcessingJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId
                               && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing).Count();

                var allCancelledJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == i.loanApplicationDetailId
                               && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.cancel).Count();

                i.invoiceDiscountDetail = invoiceDiscountDetail;
                i.firstEducationtDetail = firstEducationtDetail;
                //i.loanCollateral = loanCollateral;
                i.allJobsCount = allJobsCount;
                i.allPendingJobsCount = allPendingJobsCount;
                i.allApprovedJobsCount = allApprovedJobsCount;
                i.allDisapproveJobsCount = allDisapproveJobsCount;
                i.allProcessingJobsCount = allProcessingJobsCount;
                i.allCancelledJobsCount = allCancelledJobsCount;
            }
            return data;
        }

        private IEnumerable<JobRequestViewModel> GetAllGlobalJobRequest(int staffId, bool isFilter = false)
        {
            var thisStaff = this.context.TBL_STAFF.Find(staffId);
            var staffAdmin = this.context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);
            var staffHub = this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
            var middleOfficeUnit = from x in context.TBL_JOB_TYPE_UNIT join t in context.TBL_JOB_TYPE_HUB_STAFF on x.JOBTYPEUNITID equals t.JOBTYPEUNITID
                                   where x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && t.STAFFID ==staffId select x;

            bool isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault();

            var data = (from x in context.TBL_JOB_REQUEST
                        join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                        join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                        //where x.ISREASSIGNED == false
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
                            jobTypeName = t.JOBTYPENAME,
                            jobSubTypeName = s.JOB_SUB_TYPE_NAME,
                            jobStatusFeedBackId = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACKID,
                            jobStatusFeedBack = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACK_NAME,
                            senderStaffId = x.SENDERSTAFFID,
                            senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                            senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE,
                            departmentUnitId = x.DEPARTMENTUNITID,

                            receiverStaffId = (int)x.RECEIVERSTAFFID,
                            reassignedTo = x.REASSIGNEDTO,
                            isReassigned = x.ISREASSIGNED,
                            isAcknowledged = x.ISACKNOWLEDGED,
                            operationsId = x.OPERATIONSID,
                            operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                            requestStatusId = x.REQUESTSTATUSID,
                            requestStatusname = x.TBL_JOB_REQUEST_STATUS.STATUSNAME,
                            
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
                            branchId = x.BRANCHID,
                            isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault(),

                            refNo = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == x.TARGETID) != null
                                                     ? context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.TARGETID).FirstOrDefault().TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : "n/a",

                            fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                            fromBranchName = x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.FirstOrDefault().TBL_BRANCH.FirstOrDefault().BRANCHNAME : "n/a" : "n/a",
                            to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                            assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

                        })).ToList().OrderByDescending(x => x.arrivalDate); ;


            foreach (var item in data)
            {
                var detail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == item.jobRequestId);
                if (detail.Any())
                {
                    item.hasLegalRecommendedSearch = true;
                    if (detail.FirstOrDefault().ACCREDITEDCONSULTANTPAID)
                        item.customerCharged = true;
                };

                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.MiddleOfficeVerification) item.jobSubTypeName = "MO Verification";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfTreasuryBills) item.jobSubTypeName = "Treasury Bill confirm..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfFBNQUEST) item.jobSubTypeName = "FBN Quest confirm..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfDealSlip) item.jobSubTypeName = "Deal Slip confirm..";
                if (item.jobSubTypeId != null && item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfStock) item.jobSubTypeName = "Stock confirmation";
                if (item.jobSubTypeId == null || item.jobSubTypeId < 1) item.jobSubTypeName = "n/a";
            }

            if (staffAdmin.Any() && !isFilter)
            {
                return data.Where(x=>x.jobTypeId == staffAdmin.FirstOrDefault().JOBTYPEID && x.isReassigned == false);
            }

            if (staffAdmin.Any() && isFilter)
            {
                return data.Where(x => x.jobTypeId == staffAdmin.FirstOrDefault().JOBTYPEID);
            }

            else
            {
                if (staffHub.Any() && middleOfficeUnit.Any() && isTeamLead)
                {
                    return data.Where(x => x.senderStaffId == staffId || x.receiverStaffId == staffId || x.reassignedTo == staffId || x.jobTypeUnitId == staffHub.FirstOrDefault().JOBTYPEUNITID);
                }
                if (staffHub.Any() && middleOfficeUnit.Any() && !isTeamLead)
                {
                    return data.Where(x => x.senderStaffId == staffId || x.receiverStaffId == staffId || x.reassignedTo == staffId );
                }
                else if(staffHub.Any())
                {
                    return data.Where(x => x.senderStaffId == staffId || x.receiverStaffId == staffId || x.reassignedTo == staffId || x.jobTypeUnitId == staffHub.FirstOrDefault().JOBTYPEUNITID);
                }
                else { return data.Where(x => x.senderStaffId == staffId || x.receiverStaffId == staffId || x.reassignedTo == staffId); }
            }
        }



        //private IEnumerable<JobRequestViewModel> GetAllGlobalJobRequest(int staffId, int branchId)
        //{
        //    var thisStaff = this.context.TBL_STAFF.Find(staffId);
        //    var staffHub = this.context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
        //    var staffAdmin = this.context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);

        //    var data = new List<JobRequestViewModel>();
        //    if (!staffHub.Any() && !staffAdmin.Any())
        //    {
        //       //STAFF HAS NO JOB REQUEST/SENT NO JOB REQUEST/NOT ADMIN
        //        return data;
        //    }

        //    if (staffAdmin.Any())
        //    {
        //        //STAFF IS ADMIN
        //         data = (from x in context.TBL_JOB_REQUEST
        //                    join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
        //                    join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
        //                    where x.ISREASSIGNED == false
        //                    orderby x.ARRIVALDATE descending
        //                    select (
        //                    new JobRequestViewModel
        //                    {
        //                        jobRequestId = x.JOBREQUESTID,
        //                        requestTitle = x.JOB_TITLE,
        //                        jobRequestCode = x.JOBREQUESTCODE,
        //                        targetId = x.TARGETID,
        //                        jobTypeId = t.JOBTYPEID,
        //                        jobSubTypeId = s.JOB_SUB_TYPEID,
        //                        jobTypeName = t.JOBTYPENAME,
        //                        jobSubTypeName = s.JOB_SUB_TYPE_NAME,
        //                        jobStatusFeedBackId = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACKID,
        //                        jobStatusFeedBack = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACK_NAME,
        //                        senderStaffId = x.SENDERSTAFFID,
        //                        senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
        //                        senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE,
        //                        departmentUnitId = x.DEPARTMENTUNITID,
        //                        departmentId = x.DEPARTMENTID,

        //                        receiverStaffId = (int)x.RECEIVERSTAFFID,
        //                        reassignedTo = x.REASSIGNEDTO,
        //                        isReassigned = x.ISREASSIGNED,
        //                        isAcknowledged = x.ISACKNOWLEDGED,
        //                        operationsId = x.OPERATIONSID,
        //                        operationName = x.TBL_OPERATIONS.OPERATIONNAME,
        //                        requestStatusId = x.REQUESTSTATUSID,
        //                        requestStatusname = x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

        //                        senderComment = x.SENDERCOMMENT,
        //                        responseComment = x.RESPONSECOMMENT,
        //                        arrivalDate = x.ARRIVALDATE,
        //                        systemArrivalDate = x.SYSTEMARRIVALDATE,
        //                        reassignedDate = x.REASSIGNEDDATE,
        //                        systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
        //                        responseDate = x.RESPONSEDATE,
        //                        systemResponseDate = x.SYSTEMRESPONSEDATE,
        //                        acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
        //                        systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
        //                        loggedInStaffId = staffId,
        //                        jobTypeUnitId = x.JOBTYPEUNITID,
        //                        jobTypeHubId = x.JOBTYPEHUBID,
        //                        branchId = x.BRANCHID,

        //                        refNo = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == x.TARGETID) != null
        //                             ? context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.TARGETID).FirstOrDefault().TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : "n/a",

        //                        fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
        //                        fromBranchName = x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.FirstOrDefault().TBL_BRANCH.FirstOrDefault().BRANCHNAME : "n/a" : "n/a",
        //                        to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
        //                        assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

        //                    })).ToList();
        //    }

        //    if (!staffAdmin.Any() && staffHub.Any())
        //    {
        //        data = (from x in context.TBL_JOB_REQUEST
        //                    join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
        //                    join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
        //                    where (x.JOBTYPEHUBID == staffHub.FirstOrDefault().JOBTYPEHUBID || x.SENDERSTAFFID == staffId || x.REASSIGNEDTO == staffId)
        //                    orderby x.ARRIVALDATE descending
        //                    select (
        //                    new JobRequestViewModel
        //                    {
        //                        jobRequestId = x.JOBREQUESTID,
        //                        requestTitle = x.JOB_TITLE,
        //                        jobRequestCode = x.JOBREQUESTCODE,
        //                        targetId = x.TARGETID,
        //                        jobTypeId = t.JOBTYPEID,
        //                        jobSubTypeId = s.JOB_SUB_TYPEID,
        //                        jobTypeName = t.JOBTYPENAME,
        //                        jobSubTypeName = s.JOB_SUB_TYPE_NAME,
        //                        jobStatusFeedBackId = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACKID,
        //                        jobStatusFeedBack = x.TBL_JOB_REQUEST_STATUS_FEEDBAK.JOB_STATUS_FEEDBACK_NAME,
        //                        senderStaffId = x.SENDERSTAFFID,
        //                        senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
        //                        senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE,
        //                        departmentUnitId = x.DEPARTMENTUNITID,
        //                        departmentId = x.DEPARTMENTID,

        //                        receiverStaffId = (int)x.RECEIVERSTAFFID,
        //                        reassignedTo = x.REASSIGNEDTO,
        //                        isReassigned = x.ISREASSIGNED,
        //                        isAcknowledged = x.ISACKNOWLEDGED,
        //                        operationsId = x.OPERATIONSID,
        //                        operationName = x.TBL_OPERATIONS.OPERATIONNAME,
        //                        requestStatusId = x.REQUESTSTATUSID,
        //                        requestStatusname = x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

        //                        senderComment = x.SENDERCOMMENT,
        //                        responseComment = x.RESPONSECOMMENT,
        //                        arrivalDate = x.ARRIVALDATE,
        //                        systemArrivalDate = x.SYSTEMARRIVALDATE,
        //                        reassignedDate = x.REASSIGNEDDATE,
        //                        systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
        //                        responseDate = x.RESPONSEDATE,
        //                        systemResponseDate = x.SYSTEMRESPONSEDATE,
        //                        acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
        //                        systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
        //                        loggedInStaffId = staffId,
        //                        jobTypeUnitId = x.JOBTYPEUNITID,
        //                        jobTypeHubId = x.JOBTYPEHUBID,
        //                        branchId = x.BRANCHID,

        //                        refNo = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == x.TARGETID) != null
        //                             ? context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.TARGETID).FirstOrDefault().TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : "n/a",

        //                        fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
        //                        fromBranchName = x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.Any() ? x.TBL_STAFF.TBL_BRANCH_REGION.FirstOrDefault().TBL_BRANCH.FirstOrDefault().BRANCHNAME : "n/a" : "n/a",
        //                        to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
        //                        assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

        //                    })).ToList();
        //    }

        //    //var unitId = 0;
        //    //unitId = thisStaff.TBL_DEPARTMENT_UNIT != null ? thisStaff.TBL_DEPARTMENT_UNIT.DEPARTMENTUNITID : 0;

        //    foreach (var item in data)
        //    {
        //        var detail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == item.jobRequestId);
        //        if (detail.Any())
        //        {
        //            item.hasLegalRecommendedSearch = true;
        //            if (detail.FirstOrDefault().ACCREDITEDCONSULTANTPAID)
        //                item.customerCharged = true;
        //        };

        //        if(item.jobSubTypeId == (int)JobSubTypeEnum.MiddleOfficeVerification) item.jobSubTypeName = "MO Verification";
        //        if(item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfTreasuryBills) item.jobSubTypeName = "Treasury Bill confirm..";
        //        if (item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfFBNQUEST) item.jobSubTypeName = "FBN Quest confirm..";
        //        if (item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfDealSlip) item.jobSubTypeName = "Deal Slip confirm..";
        //        if (item.jobSubTypeId == (int)JobSubTypeEnum.ConfirmationOfStock) item.jobSubTypeName = "Stock confirmation";
        //        if (item.jobSubTypeId == null || item.jobSubTypeId < 1) item.jobSubTypeName = "n/a";

        //    }

        //   // var c = data.ToList();
        //    return data;
        //}

        public IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId, int branchId)
        {
            return GetAllGlobalJobRequest(staffId).OrderByDescending(x => x.jobRequestId);
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByFilter(int staffId, int branchId, string filter)
        {
            var filtered = filter.ToLower();
            switch (filtered)
            {
                case "completed":
                    return GetAllGlobalJobRequest(staffId, true).Where(x=>x.responseComment != null || x.requestStatusId == (short)RequestStatusEnum.Approved).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "approved":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Approved).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "pending":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Pending).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "in-progress":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Processing).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "cancelled":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Cancel).OrderByDescending(x => x.jobRequestId);
                   // break;

                case "disapproved":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.requestStatusId == (short)RequestStatusEnum.Disapproved).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "assigned":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.reassignedTo != null).OrderByDescending(x => x.jobRequestId);
                    //break;

                case "unassigned":
                    return GetAllGlobalJobRequest(staffId,true).Where(x => x.reassignedTo == null).OrderByDescending(x => x.jobRequestId);
                    //break;


                default:
                     return GetAllGlobalJobRequest(staffId).OrderByDescending(x => x.jobRequestId);
                    //break;

            }
        }

        public List<JobRequestDetailViewModel> GetJobRequestDetailsById(int jobRequestId)
        {
            return GetJobRequestDetails().Where(x => x.jobRequestId == jobRequestId).ToList();
        }
        private List<JobRequestDetailViewModel> GetJobRequestDetails()
        {
            var details = (from x in this.context.TBL_JOB_REQUEST_DETAIL
                           join b in context.TBL_JOB_REQUEST on x.JOBREQUESTID equals b.JOBREQUESTID
                           where x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated
                           && x.DELETED == false
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
            var jobRequest = context.TBL_JOB_REQUEST.Where(x => x.REQUESTSTATUSID != (short)ApprovalStatusEnum.Approved && x.JOBTYPEID == (short)JobTypeEnum.legal);
           
            List<JobRequestDetailViewModel> jobDetailList = new List<JobRequestDetailViewModel>();
            foreach(var i in jobRequest)
            {
                var jobDetail = context.TBL_JOB_REQUEST_DETAIL.Where(x => x.JOBREQUESTID == i.JOBREQUESTID && (x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated) && x.ACCREDITEDCONSULTANTPAID == false).ToList();
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
                    jobSubTypeName = item.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME != null ? jobSubTypeName + item.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME.ToString() + ", " : string.Empty;
                    //jobTypeName = item.TBL_JOB_REQUEST.TBL_JOB_TYPE.TBL_JOB_REQUEST != null ? jobTypeName + item.TBL_JOB_REQUEST.TBL_JOB_TYPE.TBL_JOB_REQUEST.ToString() + ", " : string.Empty;

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
            //var jobDetails = (from d in this.context.TBL_JOB_REQUEST_DETAIL
            //                 join r in context.TBL_JOB_REQUEST on d.JOBREQUESTID equals r.JOBREQUESTID
            //                 where d.JOB_SUB_TYPEID == (short)JobSubTypeEnum.LegalCharting || d.JOB_SUB_TYPEID == (short)JobSubTypeEnum.LegalSearch || d.JOB_SUB_TYPEID == (short)JobSubTypeEnum.LegalVerification || d.JOB_SUB_TYPEID == (short)JobSubTypeEnum.OtherLegalJobs
            //                 select new JobRequestDetailViewModel
            //                 {
            //                     jobRequestId = d.JOBREQUESTID,
            //                     jobRequestDetailId = d.JOBREQUEST_DETAILID,
            //                     accreditedConsultantId = (int)d.ACCREDITEDCONSULTANTID,
            //                     accreditedConsultantName = d.TBL_ACCREDITEDCONSULTANT.FIRMNAME,
            //                     jobSubTypeId = d.JOB_SUB_TYPEID,
            //                     jobRequestCode = r.JOBREQUESTCODE,
            //                     jobSubTypeName = d.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME,
            //                     jobTypeId = d.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPEID,
            //                     jobTypeName = d.TBL_JOB_REQUEST.TBL_JOB_TYPE.JOBTYPENAME,
            //                     description = d.DESCRIPTION,
            //                     operationsId = r.OPERATIONSID,
            //                     operationsName = r.TBL_OPERATIONS.OPERATIONNAME,
            //                     targetId = r.TARGETID,
            //                     amount = d.AMOUNT,
            //                     accountNumber = d.ACCOUNTNUMBER,
            //                     dateTimeCreated = d.DATETIMECREATED
            //                 }).ToList();
            //foreach (var item in jobDetail)
            //{
            //    var a = context.TBL_LOAN_APPLICATION_DETAIL.Where(z => z.LOANAPPLICATIONDETAILID == item.targetId);

            //    if (a.Any())
            //    {
            //        var t = a.FirstOrDefault();
            //        item.customerName = (from v in context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == t.CUSTOMERID) select v.FIRSTNAME + " " + v.MIDDLENAME + " " + v.LASTNAME).FirstOrDefault();
            //        item.applicationReferenceNumber = t.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER;
            //        item.customerId = t.CUSTOMERID;
            //    }
            //};
            //return jobDetail;
        }

        public List<JobRequestViewModel> GetJobRequestLegalJobDetail()
        {
            //var job = context.TBL_JOB_REQUEST.Where(x => x. == (short)JobSubTypeEnum.LegalCharting || x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.LegalSearch || x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.LegalVerification || x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.OtherLegalJobs);
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
        public List<JobRequestViewModel> GetApplicationJobRequest(int applicationDetailId)
        {
            var requests = this.context.TBL_JOB_REQUEST.Find(applicationDetailId);

            if (requests == null) return null;

            var jobDocumentsList = GetJobRequestDocuments(requests.JOBREQUESTCODE).AsEnumerable();
            TBL_JOB_REQUEST_STATUS_FEEDBAK feedback;
            var requestsList = new List<JobRequestViewModel>();

            feedback = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Where(c => c.JOB_STATUS_FEEDBACKID == requests.JOB_STATUS_FEEDBACKID).FirstOrDefault();
            var requestModel = new JobRequestViewModel
            {
                jobRequestId = requests.JOBREQUESTID,
                requestTitle = requests.JOB_TITLE,
                jobRequestCode = requests.JOBREQUESTCODE,
                targetId = requests.TARGETID,
                jobTypeId = requests.JOBTYPEID,
                senderStaffId = requests.SENDERSTAFFID,
                receiverStaffId = requests.RECEIVERSTAFFID ?? 0,
                reassignedTo = requests.REASSIGNEDTO,
                isReassigned = requests.ISREASSIGNED,
                isAcknowledged = requests.ISACKNOWLEDGED,
                operationsId = requests.OPERATIONSID,
                operationName = requests.TBL_OPERATIONS.OPERATIONNAME,
                requestStatusId = requests.REQUESTSTATUSID,
                senderComment = requests.SENDERCOMMENT,
                responseComment = requests.RESPONSECOMMENT,

                arrivalDate = requests.ARRIVALDATE,
                systemArrivalDate = requests.SYSTEMARRIVALDATE,
                reassignedDate = requests.REASSIGNEDDATE,
                systemReassignedDate = requests.SYSTEMREASSIGNEDDATE,
                responseDate = requests.RESPONSEDATE,
                systemResponseDate = requests.SYSTEMRESPONSEDATE,
                acknowledgementDate = requests.ACKNOWLEDGEMENTDATE,
                systemAcknowledgementDate = requests.SYSTEMACKNOWLEDGEMENTDATE,
                jobStatusFeedBackId = requests.JOB_STATUS_FEEDBACKID ?? 0,
                jobStatusFeedback = (feedback != null) ? feedback.JOB_STATUS_FEEDBACK_NAME : string.Empty,
                msgExchangeTrail = (from y in context.TBL_JOB_REQUEST_MESSAGE
                                    where y.JOBREQUESTID == requests.JOBREQUESTID
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

            var fromData = context.TBL_STAFF.Where(b => b.STAFFID == requests.SENDERSTAFFID).FirstOrDefault();
            requestModel.fromSender = fromData != null ? fromData.FIRSTNAME + " " + fromData.MIDDLENAME + " " + fromData.LASTNAME : "n/a";

            var toData = context.TBL_STAFF.Where(b => b.STAFFID == requests.RECEIVERSTAFFID).FirstOrDefault();
            requestModel.to = toData != null ? toData.FIRSTNAME + " " + toData.MIDDLENAME + " " + toData.LASTNAME : "n/a";

            var asigneeData = context.TBL_STAFF.Where(b => b.STAFFID == requests.REASSIGNEDTO).FirstOrDefault();
            requestModel.assignee = asigneeData != null ? asigneeData.FIRSTNAME + " " + asigneeData.MIDDLENAME + " " + asigneeData.LASTNAME : "n/a";

            requestsList.Add(requestModel);

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

        public IEnumerable<JobRequestViewModel> GetJobRequestByDepartment(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;
            var departmentId = 0;
            var staff = context.TBL_STAFF.Find(staffId);
            if (staff != null) { departmentId = (int)staff.TBL_DEPARTMENT_UNIT.DEPARTMENTID; }

            var allstaff = this.context.TBL_STAFF.Select(s => new
            {
                id = s.STAFFID,
                name = s.LASTNAME + " " + s.FIRSTNAME
            });

            return context.TBL_DEPARTMENT
                .Join(context.TBL_JOB_REQUEST.Where(x => x.OPERATIONSID == operationId),
                a => a.DEPARTMENTID, b => b.DEPARTMENTID, (a, b) => new { a, b })
                .Where(x =>
                    x.b.SENDERSTAFFID == staffId
                    || x.b.DEPARTMENTID == departmentId
                    || x.b.REASSIGNEDTO == staffId
                )
                .Select(x => new JobRequestViewModel
                {
                    jobRequestId = x.b.JOBREQUESTID,
                    jobRequestCode = x.b.JOBREQUESTCODE,
                    jobTypeId = x.b.JOBTYPEID,
                    senderStaffId = x.b.SENDERSTAFFID,
                    receiverStaffId = (int)x.b.RECEIVERSTAFFID,
                    reassignedTo = x.b.REASSIGNEDTO,
                    isReassigned = x.b.ISREASSIGNED,
                    isAcknowledged = x.b.ISACKNOWLEDGED,
                    operationsId = x.b.OPERATIONSID,
                    requestStatusId = x.b.REQUESTSTATUSID,
                    senderComment = x.b.SENDERCOMMENT,
                    responseComment = x.b.RESPONSECOMMENT,
                    arrivalDate = x.b.ARRIVALDATE,
                    systemArrivalDate = x.b.SYSTEMARRIVALDATE,
                    reassignedDate = x.b.REASSIGNEDDATE,
                    systemReassignedDate = x.b.SYSTEMREASSIGNEDDATE,
                    responseDate = x.b.RESPONSEDATE,
                    systemResponseDate = x.b.SYSTEMRESPONSEDATE,
                    acknowledgementDate = x.b.ACKNOWLEDGEMENTDATE,
                    systemAcknowledgementDate = x.b.SYSTEMACKNOWLEDGEMENTDATE,
                    fromSender = allstaff.FirstOrDefault(s => s.id == x.b.SENDERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.SENDERSTAFFID).name,
                    to = allstaff.FirstOrDefault(s => s.id == x.b.RECEIVERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.RECEIVERSTAFFID).name,
                    assignee = allstaff.FirstOrDefault(s => s.id == x.b.REASSIGNEDTO) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.REASSIGNEDTO).name,
                })
                .OrderByDescending(x => x.jobRequestId)
                .Take(100);
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

        #region
        public bool PlaceChargeOnCustomerForCollateralSearch(JobRequestCollateralSearchViewModel model)
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
                if (casa == null)
                    throw new ConditionNotMetException("Customer account number is not supplied");

                if (casa != null) accountBalance = financeTransaction.GetCASABalance(casa.CASAACCOUNTID).availableBalance;

                model.casaAccountId = casa.CASAACCOUNTID;
                model.operationId = (short)OperationsEnum.CollateralSearchInitiation;
                accountNumber = casa.PRODUCTACCOUNTNUMBER;
                auditDetail = $"Customer account number '{casa.PRODUCTACCOUNTNUMBER}' debited with collateral search fees";
            }
            else
            {
                var b = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;
                if ( b == null || b == string.Empty || b == " ")
                    throw new ConditionNotMetException("The solicitor's account number is not found. No account number has been mapped to this solicitor.");

                    accountNumber = consultantRecord.FirstOrDefault().ACCOUNTNUMBER;
                //var casa = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == accountNumber);

                //var casaUnique = casa.FirstOrDefault();
                //model.casaAccountId = casaUnique.CASAACCOUNTID;

                var witholdingAmount = (double)model.totalChargeAmount / 0.9;
                //model.totalChargeAmount = model.totalChargeAmount - (decimal)witholdingAmount;
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

            if (model.isInitiation && model.totalChargeAmount > accountBalance)
                throw new ConditionNotMetException("The customer's Account is not funded.");

            if (model.totalChargeAmount > 0)
            {
                var twoFADetails = new TwoFactorAutheticationViewModel
                {
                    username = model.username,
                    passcode = model.passCode
                };
                List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                //When RM apply fee on customer's account
                if (model.isInitiation)
                {
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
                        IPADDRESS = model.userIPAddress,
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };
                    this.audit.AddAuditTrail(audit);
                    // End of Audit Section ---------------------


                    if (model.isInitiation) //When RM apply fee on customer's account
                    {
                        inputTransactions.AddRange(BuildCollateralSearchChargeFeesPosting(model));
                        //Sending mail to solicitor
                        if (consultantRecord.Any())
                        {
                            List<string> jobs = new List<string>();
                            foreach (var i in jobRequestDetail)
                            {
                                jobs.Add(i.TBL_JOB_TYPE_SUB.JOB_SUB_TYPE_NAME);
                            }
                            var solicitor = consultantRecord.FirstOrDefault();
                            string messageBoby = $"Dear {solicitor.FIRMNAME}, <br /><br />Your attention is needed to attend to our customer's collateral on the following:<br /><br /> '{jobs}'. <br /><br /> Kindly kindly contact FBN legal department for more info. <br /><br />";
                            string alertSubject = $"FBN - Loan Collateral Search";
                            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, solicitor.EMAILADDRESS);
                        }
                    }

                    context.SaveChanges();
                    return true;
                }

                else return false;
            }
            else return false;
                
        }

        public bool EffectLegaCollateralJobs(JobRequestCollateralSearchViewModel model)
        {
            var jobRequest = context.TBL_JOB_REQUEST.Find(model.jobRequestId);
            var baseApplication = context.TBL_LOAN_APPLICATION_DETAIL.Find(jobRequest.TARGETID);
            var state = context.TBL_STATE.Find(model.collateralStateId);
            var company = context.TBL_COMPANY.Find(model.companyId);
            //var casa = context.TBL_CASA.Find(model.casaAccountId);

            //if (casa == null)
            //    throw new SecureException("Customer account number is not supplied");

            // Decimal chargeAmount = 0;

            var collateralStateDetails = context.TBL_STATE.Find(model.collateralStateId);
            if (model.requireCharting)
            {
                //chargeAmount = chargeAmount + (collateralStateDetails.CHARTINGAMOUNT ?? 0);
                var detail = new JobRequestDetailViewModel
                {
                    jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralCharting,
                    jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
                    jobTypeId = (short)JobTypeEnum.legal,
                    amount = model.chartChargeAmount, 
                    jobRequestId = model.jobRequestId,
                    createdBy = model.createdBy,
                    accreditedConsultantId = model.solicitorId,
                    accountNumber = model.accountNumber,
                    currencyId = company.CURRENCYID
                };
                saveJobRequestDetail(detail);
            }

            if (model.requireSearch)
            {
                //chargeAmount = chargeAmount + (collateralStateDetails.COLLATERALSEARCHCHARGEAMOUNT);
                var detail = new JobRequestDetailViewModel
                {
                    jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralSearch,
                    jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
                    jobTypeId = (short)JobTypeEnum.legal,
                    amount = model.searchChargeAmount, 
                    jobRequestId = model.jobRequestId,
                    createdBy = model.createdBy,
                    accreditedConsultantId = model.solicitorId,
                    accountNumber = model.accountNumber,
                    currencyId = company.CURRENCYID
                };
                saveJobRequestDetail(detail);
            }

            if (model.requireVerification)
            {
                //chargeAmount = chargeAmount + (collateralStateDetails.VERIFICATIONAMOUNT ?? 0);
                var detail = new JobRequestDetailViewModel
                {
                    jobSubTypeclassId = (short)JobSubTypeClassEnum.CollateralVerification,
                    jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
                    jobTypeId = (short)JobTypeEnum.legal,
                    amount = model.verificationChargeAmount,
                    jobRequestId = model.jobRequestId,
                    createdBy = model.createdBy,
                    accreditedConsultantId = model.solicitorId,
                    accountNumber = model.accountNumber,
                    currencyId = company.CURRENCYID
                };
                saveJobRequestDetail(detail);
            }

            if (model.additionalCharge > 0)
            {
                //chargeAmount = chargeAmount + (model.additionalCharge ?? 0);
                var detail = new JobRequestDetailViewModel
                {
                    jobSubTypeclassId = (short)JobSubTypeClassEnum.AdditionalCharges,
                    jobSubTypeId = (short)JobSubTypeEnum.CollateralRelated,
                    jobTypeId = (short)JobTypeEnum.legal,
                    amount = model.additionalCharge,
                    description = model.additionalChargeJustification,
                    jobRequestId = model.jobRequestId,
                    createdBy = model.createdBy,
                    accreditedConsultantId = model.solicitorId,
                    accountNumber = model.accountNumber,
                    currencyId = company.CURRENCYID
                };
                saveJobRequestDetail(detail);
            }

            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
            string messageBoby = $"Dear RM, <br /><br />This is to bring your attention that legal has specified that charges for collaral on job request with code '{jobRequest.JOBREQUESTCODE}'. <br /><br /> You attention is required to effect the charges. <br /><br />";
            string alertSubject = $"Collateral Search Request";
            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetStaffEmailRecipients(jobRequest.SENDERSTAFFID));
            //if (baseApplication != null)
            //{
            //    BasicTrasactionSourceInputModel input = new BasicTrasactionSourceInputModel();
            //    input.createdBy = model.createdBy;
            //    input.description = "Collateral Search";
            //    input.sourceApplicationId = (short)baseApplication.LOANAPPLICATIONDETAILID;
            //    input.companyId = model.companyId;
            //    input.userBranchId = model.userBranchId;
            //}

            return context.SaveChanges() > 0;
        }

        private void saveJobRequestDetail(JobRequestDetailViewModel model)
        {
            var jobDetail = new TBL_JOB_REQUEST_DETAIL();
            jobDetail.AMOUNT = model.amount;
            jobDetail.DESCRIPTION = model.description;
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

        private void LogEmailAlertForLoanApplicationCancellation(string messageBody, string alertSubject, string recipients)
        {
            try
            {
                string recipient = recipients.Trim();

                string messageSubject = alertSubject;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which are approaching their due date. <br /><br />";
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
                    SendOnDateTime = DateTime.Now
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
                SENDONDATETIME = model.SendOnDateTime
            };

            context.TBL_MESSAGE_LOG.Add(message);

        }
        #endregion

        #region job-type

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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                hubStaffName = (from s in context.TBL_STAFF where s.STAFFID == x.STAFFID select s.FIRSTNAME+" "+s.LASTNAME +"("+ s.STAFFCODE+")" ).FirstOrDefault()
            }).Where(c => c.jobTypeHubId == jobTypeHubId && c.deleted == false);
        }

        public IEnumerable<HubStaffViewModel> GetHubStaffByHubTypeUnitId(short jobTypeUnitId)
        {
            return this.context.TBL_JOB_TYPE_HUB_STAFF.Select(x => new HubStaffViewModel
            {
                hubStaffId = x.STAFFID,
                jobTypeHubId = x.JOBTYPEHUBID,
                jobTypeUnitId = x.JOBTYPEUNITID,
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

        public List<jobReasignment> GetJobReasignmentStaffById(int staffId, int companyId)
        {
            var details = (from x in context.TBL_JOB_TYPE_REASSIGNMENT
                           where x.STAFFID == staffId && x.COMPANYID == companyId && x.DELETED == false
                           select new jobReasignment
                           {
                               staffId = x.STAFFID,
                               jobTypeId = x.JOBTYPEID,
                               dateTimeCreated = x.DATETIMECREATED

                           }).ToList();

            return details;
        }

        public IEnumerable<JobTypeViewModel> GetJobSubType(short jobId)
        {
            return this.context.TBL_JOB_TYPE_SUB.Select(x => new JobSubTypeViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobSubTypeName = x.JOB_SUB_TYPE_NAME,
                jobSubTypeId = x.JOB_SUB_TYPEID
            }).Where(x => x.jobTypeId == jobId );
        }

        #endregion job-type

        #region Middle Office Updates
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

        public bool chargeCustomerForCollateralJobs(int jobRequestDetailId, JobRequestDetailViewModel model)
        {
            var jobDetail = context.TBL_JOB_REQUEST_DETAIL.Find(jobRequestDetailId);
            var consultant = context.TBL_ACCREDITEDCONSULTANT.Find(jobDetail.ACCREDITEDCONSULTANTID);
            var casa = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == consultant.ACCOUNTNUMBER).FirstOrDefault();

            if (casa == null)
                throw new ConditionNotMetException("Accredited consultant account number not found in the system");



            return false;
        }
        #endregion End Middle Office Updates

        #region Job-Request Document

        public bool AddJobReplyAndDocument(RequestDocumentViewModel model, byte[] file)
        {
            //var data = new Entities.DocumentModels.TBL_MEDIA_JOB_REQUEST_DOCUMENT
            //{
            //    FILEDATA = file,
            //    JOBREQUESTCODE = model.jobRequestCode,
            //    DOCUMENTTITLE = model.documentTitle,
            //    DOCUMENTTYPEID = model.documentTypeId,
            //    FILENAME = model.fileName,
            //    FILEEXTENSION = model.fileExtension,
            //    SYSTEMDATETIME = DateTime.Now,
            //    PHYSICALFILENUMBER = model.physicalFileNumber,
            //    PHYSICALLOCATION = model.physicalLocation,
            //    CREATEDBY = (int)model.createdBy,
            //};

            //docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Add(data);

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

                return ReplyJobRequest(jb, jb.jobRequestId);
            }
            else return false;
        }

        public bool AddJobDocumentOnly(RequestDocumentViewModel model, byte[] file)
        {
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            context.SaveChanges();

            return docContext.SaveChanges() != 0;
        }

        public string AddJobDocument(RequestDocumentViewModel model, JobRequestViewModel requestModel, byte[] file)
        {
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

        #endregion Job-Request Document

        #region Account Posting (Debit & Credit)
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
        #endregion



        //public bool AddJobRequest(JobRequestViewModel model)
        //{

        //    var date = DateTime.Now;
        //    var applicationDate = general.GetApplicationDate();

        //    var data = new TBL_JOB_REQUEST
        //    {
        //        JOBREQUESTCODE = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode(),
        //        JOBTYPEID = model.jobTypeId,
        //        JOB_TITLE = model.requestTitle,
        //        SENDERSTAFFID = model.createdBy,
        //        RECEIVERSTAFFID = model.receiverStaffId,
        //        DEPARTMENTID = model.departmentId,
        //        REASSIGNEDTO = model.reassignedTo,
        //        ISREASSIGNED = model.isReassigned,
        //        ISACKNOWLEDGED = model.isAcknowledged,
        //        TARGETID = model.targetId,
        //        OPERATIONSID = model.operationsId, // cam enum
        //        REQUESTSTATUSID = model.requestStatusId, // status enum
        //        SENDERCOMMENT = model.senderComment,
        //        RESPONSECOMMENT = model.responseComment,
        //        ARRIVALDATE = applicationDate,
        //        SYSTEMARRIVALDATE = date,
        //    };

        //    context.TBL_JOB_REQUEST.Add(data);

        //    Audit Section ---------------------------
        //   var audit = new TBL_AUDIT
        //   {
        //       AUDITTYPEID = (short)AuditTypeEnum.JobRequestAdded,
        //       STAFFID = model.createdBy,
        //       BRANCHID = (short)model.userBranchId,
        //       DETAIL = $"Added JobRequest '{ model.jobRequestCode }' ",
        //       IPADDRESS = model.userIPAddress,
        //       URL = model.applicationUrl,
        //       APPLICATIONDATE = applicationDate,
        //       SYSTEMDATETIME = DateTime.Now
        //   };
        //    this.audit.AddAuditTrail(audit);
        //    End of Audit Section ---------------------

        //    return context.SaveChanges() != 0;
        //}

        //public IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId)
        //{
        //    return this.context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
        //         .Select(g => g.TBL_APPROVAL_GROUP)
        //         .SelectMany(l => l.TBL_APPROVAL_LEVEL)
        //         .SelectMany(s => s.TBL_APPROVAL_LEVEL_STAFF)
        //         .Select(s => new OperationStaffViewModel
        //         {
        //             id = s.STAFFID,
        //             name = s.TBL_STAFF.FIRSTNAME,
        //             groupId = (int)s.TBL_APPROVAL_LEVEL.GROUPID
        //         }).ToList();
        //}

        //public IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId)
        //{
        //    var operationId = (int)OperationsEnum.CAM;

        //    var approvalGroupIds = context.TBL_APPROVAL_GROUP_MAPPING
        //        .Join(context.TBL_APPROVAL_LEVEL,
        //            a => a.GROUPID, b => b.GROUPID, (a, b) => new { a, b })
        //        .Join(context.TBL_APPROVAL_LEVEL_STAFF,
        //            c => c.b.APPROVALLEVELID, d => d.APPROVALLEVELID, (c, d) => new { c, d })
        //        .Where(x => x.c.a.OPERATIONID == operationId && x.d.STAFFID == staffId)
        //            .Select(x => x.c.b.GROUPID);

        //    return this.GetAllJobRequest().Where(x => approvalGroupIds.Contains(x.departmentId)).OrderByDescending(x => x.jobRequestId).ToList();
        //}


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
                        debit.description = model.feeNarration; // $"Fee charge on {debits.DESCRIPTION}";
                        debit.valueDate = general.GetApplicationDate();
                        debit.transactionDate = debit.valueDate;
                        debit.currencyId = casa.CURRENCYID;
                        debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                        debit.isApproved = true;
                        debit.postedBy = model.createdBy;
                        debit.approvedBy = model.createdBy;
                        debit.approvedDate = debit.transactionDate;
                        debit.approvedDateTime = DateTime.Now;
                        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        debit.companyId = model.companyId;



                        if (context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL == null)
                            throw new BadLogicException($"No GL is currently mapped to this product code '{casa.TBL_PRODUCT.PRODUCTCODE}'.");

                        debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        debit.sourceReferenceNumber = model.requestCode;
                        debit.batchCode = batchCode;
                        debit.casaAccountId = casa.CASAACCOUNTID;
                        debit.debitAmount = debitAmount;
                        debit.creditAmount = 0;
                        debit.sourceBranchId = model.userBranchId;
                        debit.destinationBranchId = casa.BRANCHID;
                        debit.rateCode = "TTB";
                        debit.rateUnit = string.Empty;
                        debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

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
                        credit.debitAmount = 0;
                        credit.creditAmount = creditAmount;
                        credit.sourceBranchId = model.userBranchId;
                        credit.destinationBranchId = model.userBranchId;
                        credit.rateCode = "TTB";
                        credit.rateUnit = string.Empty;
                        credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

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
                        debit.currencyId = (short) model.currencyId.Value;
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
                    jobFeedback = new TBL_JOB_REQUEST_STATUS_FEEDBAK()
                    {
                        JOBTYPEID = feedback.jobTypeId,
                        JOB_STATUS_FEEDBACKID = feedback.jobStatusFeedbackId,
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
                    IPADDRESS = feedback.userIPAddress,
                    URL = feedback.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
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