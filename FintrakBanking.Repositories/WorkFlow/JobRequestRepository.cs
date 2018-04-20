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

        public JobRequestRepository(FinTrakBankingDocumentsContext docContext, FinTrakBankingContext _context, IGeneralSetupRepository _general, 
            IAuditTrailRepository _audit, DepartmentRepository _department,  IFinanceTransactionRepository _financeTransaction)
        {
            this.context = _context;
            this.docContext = docContext;
            this.general = _general;
            this.audit = _audit;
            this.department = _department;
            this.financeTransaction = _financeTransaction;
    }

        public bool AddJobRequest(JobRequestViewModel model)
        {
             
            var date = DateTime.Now;
            var applicationDate = general.GetApplicationDate();
           
           var data = new TBL_JOB_REQUEST
            {
                JOBREQUESTCODE = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode(),
                JOBTYPEID = model.jobTypeId,
                JOB_TITLE = model.requestTitle,
                SENDERSTAFFID = model.createdBy,
                RECEIVERSTAFFID = model.receiverStaffId,
                DEPARTMENTID = model.departmentId,
                REASSIGNEDTO = model.reassignedTo,
                ISREASSIGNED = model.isReassigned,
                ISACKNOWLEDGED = model.isAcknowledged,
                TARGETID = model.targetId,
                OPERATIONSID = model.operationsId, // cam enum
                REQUESTSTATUSID = model.requestStatusId, // status enum
                SENDERCOMMENT = model.senderComment,
                RESPONSECOMMENT = model.responseComment,
                ARRIVALDATE = applicationDate,
                SYSTEMARRIVALDATE = date,
            };

            context.TBL_JOB_REQUEST.Add(data);

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

            return context.SaveChanges() != 0;
        }

        public bool ChargeCustomerJob(CollateralViewModel model, string actionName, string actionType, int loanApplicationDetailId)
        {
            TBL_LOAN_APPLICATION loanApplication;
            TBL_LOAN_APPLICATION_DETAIL loanApplicationDetail;
            TBL_STATE collateralLocationState;

            loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            loanApplication = context.TBL_LOAN_APPLICATION.Find(loanApplicationDetail.LOANAPPLICATIONID);

            if (loanApplication != null)
            {
                var collateralData = context.TBL_COLLATERAL_CUSTOMER.Find(model.collateralId);
                var propertyDetails = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(x => x.COLLATERALCUSTOMERID == 34).FirstOrDefault();
                if (propertyDetails != null)
                {
                    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

                    var city = context.TBL_CITY.Find(propertyDetails.CITYID);
                    if (city != null)
                    {
                        var genit = new GeneralEntity
                        {
                            createdBy = 1,
                            companyId = 1,
                            dateTimeCreated = DateTime.Now,
                            staffId = 1,
                            userBranchId = 1

                        };
                        TBL_CASA casaAccount = context.TBL_CASA.Find(loanApplication.CASAACCOUNTID);
                        collateralLocationState = context.TBL_STATE.Find(city.TBL_LOCALGOVERNMENT.STATEID);
                        var crdGL = context.TBL_SETUP_COMPANY.FirstOrDefault().LEGAL_CHARGE_GLACCOUNTID;

                        switch (actionName)
                        {
                            case "Search":
                                //if(actionType.ToLower() == "debit")inputTransactions.Add(financeTransaction.BuildCustomerApplicationChargeOrChargeReversalPosting("Post",loanApplication.LOANAPPLICATIONID, genit, collateralLocationState.COLLATERALSEARCHCHARGEAMOUNT, crdGL, "Collateral Search Charge"));
                                //else if(actionType.ToLower() == "reverse") inputTransactions.Add(financeTransaction.BuildCustomerApplicationChargeOrChargeReversalPosting("Reversal",loanApplication.LOANAPPLICATIONID, genit, collateralLocationState.COLLATERALSEARCHCHARGEAMOUNT, crdGL, "Collateral Search Charge Reversal"));

                                break;
                            case "Chart":
                                //if (actionType.ToLower() == "debit") inputTransactions.Add(financeTransaction.BuildCustomerApplicationChargeOrChargeReversalPosting("Post", loanApplication.LOANAPPLICATIONID, genit, (decimal)collateralLocationState.CHARTINGAMOUNT, crdGL, "Collateral Charting Charge"));
                                //else if (actionType.ToLower() == "reverse") inputTransactions.Add(financeTransaction.BuildCustomerApplicationChargeOrChargeReversalPosting("Reversal", loanApplication.LOANAPPLICATIONID, genit, (decimal)collateralLocationState.CHARTINGAMOUNT, crdGL, "Collateral Charting Charge Reversal"));

                                break;
                            default:
                                throw new Exception("Debit charge type is not specified.");
                                
                        }
                        financeTransaction.PostTransaction(inputTransactions);
                    }
                }
                else throw new Exception("The collateral details information is incomplete");
            }
            else throw new Exception(" Collateral cannot be traced to an active application in the system");
            //Audit Section ---------------------------
           var audit = new TBL_AUDIT
           {
               AUDITTYPEID = actionName.ToLower() == "chart" ? (short)AuditTypeEnum.CollateralChartJob : (short)AuditTypeEnum.CollateralSearchJob,
               STAFFID = model.createdBy,
               BRANCHID = (short)model.userBranchId,
               DETAIL = $"{actionType} Collateral {actionName} Charge for loan application with ref. '{ loanApplication.APPLICATIONREFERENCENUMBER }' ",
               IPADDRESS = model.userIPAddress,
               URL = model.applicationUrl, 
               APPLICATIONDATE = general.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now
           };

            this.audit.AddAuditTrail(audit);
           // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public string AddGlobalJobRequest(JobRequestViewModel model)
        {
            if (model.receiverStaffId == model.createdBy)
                throw new Exception("You cannot assign a job to yourself");

            var date = DateTime.Now;    
            var applicationDate = general.GetApplicationDate();
            model.jobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode();
            model.requestStatusId = 1;
            //if(model.operationsId == 0)  model.operationsId = 1;

           var data = new TBL_JOB_REQUEST
            {
                JOBREQUESTCODE = model.jobRequestCode,
                JOBTYPEID = model.jobTypeId,
                JOB_TITLE = model.requestTitle,
                SENDERSTAFFID = model.createdBy,
                RECEIVERSTAFFID = model.receiverStaffId == 0 ? null : model.receiverStaffId,
                DEPARTMENTID = model.departmentId,
                DEPARTMENTUNITID = model.departmentUnitId,
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
            var result = context.SaveChanges();
            return job.JOBREQUESTCODE;
        }

        public bool AddJobComment(JobRequestMessageViewModel model)
        {
            var data = new TBL_JOB_REQUEST_MESSAGE
            {
                JOBREQUESTID = model.jobRequestId ,
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
            data.REQUESTSTATUSID = 3;
            data.RESPONSECOMMENT = model.responseComment;
            data.RESPONSEDATE = applicationDate;
            data.SYSTEMRESPONSEDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Reply JobRequest '{ model.jobRequestCode }' ",
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
            var data = this.context.TBL_JOB_REQUEST.Find(jobRequestId);
            if (data == null)
            {
                return false;
            }

            var applicationDate = general.GetApplicationDate();

            data.REASSIGNEDTO = (int)model.reassignedTo;
            data.ISREASSIGNED = true;
            data.ISACKNOWLEDGED = true;
            data.REQUESTSTATUSID = 2;
            data.REASSIGNEDDATE = applicationDate;
            data.SYSTEMREASSIGNEDDATE = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.JobRequestUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Reassigned JobRequest '{ model.jobRequestCode }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = applicationDate,
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

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
                            
                            invoiceDiscountDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_INV.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new LoanApplicationDetailInvoiceViewModel
                                                     {
                                                         approvalComment = i.APPROVAL_COMMENT,
                                                         invoiceAmount = i.INVOICE_AMOUNT,
                                                         invoiceNo = i.INVOICENO,
                                                         approvaStatusId = i.APPROVALSTATUSID,
                                                         contractEndDate = i.CONTRACT_ENDDATE,
                                                         contractStartDate = i.CONTRACT_STARTDATE,
                                                         invoiceDate = i.INVOICE_DATE,
                                                         invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                         principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                         principalAccount = i.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                         principalRegNo = i.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                         principalId = i.PRINCIPALID,
                                                     }).ToList(),
                            firstEducationtDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_EDU.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new EducationLoanViewModel
                                                     {
                                                         educationId = i.EDUCATIONID,
                                                         loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                         numberOfStudent = i.NUMBER_OF_STUDENTS,
                                                         averageSchoolFees = i.AVERAGE_SCHOOL_FEES,
                                                         totalPreviousTermSchoolFees = i.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                                         productClassId = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                                         productClassName = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                                     }).ToList(),
                            firstTradderDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_TRA.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new TraderLoanViewModel
                                                  {
                                                      tradderId = i.TRADDERID,
                                                      marketId = i.MARKETID,
                                                      marketName = i.TBL_LOAN_MARKET.MARKETNAME,
                                                      averageMonthlyTurnover = i.AVERAGE_MONTHLY_TURNOVER,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      //productClassId = i.
                                                  }).ToList(),
                            loanCollateral = (from i in context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                              select new CollateralViewModel
                                              {
                                                  allowSharing = i.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                                                  collateralCode = i.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                                                  collateralValue = i.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                                                  collateralTypeName = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                                  collateralTypeId = i.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                                                  //collateralSubTypeId = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.
                                                  currencyCode = i.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                  valuationCycle = i.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                  haircut = i.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                  customerName = i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.FIRSTNAME + " " + i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.MIDDLENAME
                                                 + " " + i.TBL_COLLATERAL_CUSTOMER.TBL_CUSTOMER.LASTNAME,

                                              }).ToList(),
                            allJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID 
                                            && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       || (x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                            ).Count(),

                            allPendingJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID
                                           && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       ||( x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                           && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending).Count(),

                            allApprovedJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID
                                          && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       || (x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                          && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved).Count(),

                            allDisapproveJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID
                                            && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       || (x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                            && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Count(),

                            allProcessingJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID
                                           && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       || (x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                           && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing).Count(),

                            allCancelledJobsCount = context.TBL_JOB_REQUEST.Where(x => x.TARGETID == a.LOANAPPLICATIONDETAILID
                                           && ((x.OPERATIONSID == (short)OperationsEnum.LoanApplication) || (x.OPERATIONSID == (short)OperationsEnum.CAM)
                                                                                                       || (x.OPERATIONSID == (short)OperationsEnum.OfferLetterApproval))
                                           && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.cancel).Count(),


                        }).ToList();
            foreach (var i in data)
            {
                var relationshipOfficer = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipOfficerId).FirstOrDefault();
                var relationshipManager = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipManagerId).FirstOrDefault();
                i.relationshipOfficerName = relationshipOfficer.FIRSTNAME + " " + relationshipOfficer.MIDDLENAME + " " + relationshipOfficer.LASTNAME;
                i.relationshipManagerName = relationshipManager.FIRSTNAME + " " + relationshipManager.MIDDLENAME + " " + relationshipManager.LASTNAME;
            }
            return data;
        }


        public IEnumerable<JobRequestViewModel> GetAllJobRequest()
        {
            var allstaff = this.context.TBL_STAFF.Select(s => new //OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.LASTNAME + " " + s.FIRSTNAME
            });

            return this.context.TBL_JOB_REQUEST.Select(x => new JobRequestViewModel
            {
                jobRequestId = x.JOBREQUESTID,
                jobRequestCode = x.JOBREQUESTCODE,
                jobTypeId = x.JOBTYPEID,
                senderStaffId = x.SENDERSTAFFID,
                receiverStaffId = (int)x.RECEIVERSTAFFID,
                reassignedTo = x.REASSIGNEDTO,
                isReassigned = x.ISREASSIGNED,
                isAcknowledged = x.ISACKNOWLEDGED,
                operationsId = x.OPERATIONSID,
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
                from = allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID).name,
                to = allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID).name,
                assignee = allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO).name,
                //from = allstaff.GetStaffName(s => s.id == x.SenderStaffId),
                //to = allstaff.GetStaffName(s => s.id == x.ReceiverStaffId),
                //assignee = allstaff.GetStaffName(s => s.id == x.ReassignedTo),
            });
        }

        
        private IEnumerable<JobRequestViewModel> GetAllGlobalJobRequest(int staffId, int branchId)
        {
            var allstaff = this.context.TBL_STAFF.Select(s => new //OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.LASTNAME + " " + s.FIRSTNAME
            });

            var thisStaff = this.context.TBL_STAFF.Find(staffId);


              return  context.TBL_JOB_REQUEST
               .Where(t => ((t.DEPARTMENTUNITID == context.TBL_STAFF.Where(l=>l.STAFFID == staffId).FirstOrDefault().TBL_DEPARTMENT_UNIT.DEPARTMENTUNITID) || (t.SENDERSTAFFID == staffId) || (t.REASSIGNEDTO == staffId)) && t.TBL_STAFF.BRANCHID == branchId)
               .Select(
                  x =>
                     new JobRequestViewModel
                     {
                    jobRequestId = x.JOBREQUESTID,
                    requestTitle = x.JOB_TITLE,
                    jobRequestCode = x.JOBREQUESTCODE,
                    targetId = x.TARGETID,
                    jobTypeId = x.JOBTYPEID,
                    jobTypeName = x.TBL_JOB_TYPE.JOBTYPENAME,
                    senderStaffId = x.SENDERSTAFFID,
                    senderRole = context.TBL_STAFF.Where(c => c.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_STAFF_ROLE.STAFFROLENAME, //x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                    senderUnit = context.TBL_DEPARTMENT_UNIT.Where(c=>c.DEPARTMENTUNITID == context.TBL_STAFF.Where(z=>z.STAFFID == x.SENDERSTAFFID).FirstOrDefault().DEPARTMENTUNITID).FirstOrDefault().DEPARTMENTUNITNAME, // +"(" + x.TBL_DEPARTMENT.DEPARTMENTNAME +")",
                   // senderDepartment =  x.TBL_DEPARTMENT.DEPARTMENTNAME,
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
                    loggedInStaffId = staffId,
                    from = allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID) == null ? "n/al" : allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID).name,
                    fromBranchName = context.TBL_BRANCH.Where(c=>c.STATEID == x.SENDERSTAFFID).FirstOrDefault().BRANCHNAME,
                    to = allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID).name,
                    assignee = allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO).name,
                    toBranchName = context.TBL_BRANCH.Where(c => c.STATEID == x.RECEIVERSTAFFID).FirstOrDefault().BRANCHNAME,
                         //from = allstaff.GetStaffName(s => s.id == x.SenderStaffId),
                         //to = allstaff.GetStaffName(s => s.id == x.ReceiverStaffId),
                         //assignee = allstaff.GetStaffName(s => s.id == x.ReassignedTo),
                     }).OrderByDescending(x=>x.arrivalDate).Take(500);
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId, int branchId)
        {
            return GetAllGlobalJobRequest(staffId, branchId).OrderByDescending(x => x.jobRequestId); 
        }


        public List<JobRequestViewModel> GetApplicationJobRequest(int applicationDetailId)
        {
            var requests = this.context.TBL_JOB_REQUEST.Where(d=>d.TARGETID == applicationDetailId
            && ((d.OPERATIONSID == (short)OperationsEnum.LoanApplication) 
                || (d.OPERATIONSID == (short)OperationsEnum.CAM)
                || (d.OPERATIONSID == (short)OperationsEnum.LoanAvailment))).ToList();

            if (requests.Count == 0)
            {
                return null;
            }
            TBL_JOB_REQUEST_STATUS_FEEDBAK feedback;
            var requestsList = new List<JobRequestViewModel>();
            foreach(var x in requests)
            {
                feedback = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Where(c => c.JOB_STATUS_FEEDBACKID == x.JOB_STATUS_FEEDBACKID).FirstOrDefault();
                var request = new JobRequestViewModel
                {
                    jobRequestId = x.JOBREQUESTID,
                    requestTitle = x.JOB_TITLE,
                    jobRequestCode = x.JOBREQUESTCODE,
                    targetId = x.TARGETID,
                    jobTypeId = x.JOBTYPEID,
                    senderStaffId = x.SENDERSTAFFID,
                    receiverStaffId = x.RECEIVERSTAFFID ?? 0,
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
                    jobStatusFeedBackId = x.JOB_STATUS_FEEDBACKID ?? 0,
                    jobStatusFeedback = (feedback != null) ? feedback.JOB_STATUS_FEEDBACK_NAME : string.Empty,
                    msgExchangeTrail = (from y in context.TBL_JOB_REQUEST_MESSAGE
                                        where y.JOBREQUESTID == x.JOBREQUESTID
                                        select new JobRequestMessageViewModel
                                        {
                                            jobRequestMessageId = y.JOBREQUEST_MESSAGEID,
                                            jobRequestId = y.JOBREQUESTID,
                                            message = y.MESSAGE,
                                            staffId = y.STAFFID,
                                            staffName = y.TBL_STAFF.FIRSTNAME + " " + y.TBL_STAFF.MIDDLENAME + " " + y.TBL_STAFF.LASTNAME,
                                            datetimeSent = y.DATE_TIME_SENT
                                        }).ToList(),
                    //fromBranchName = context.TBL_BRANCH.Where(c => c.STATEID == x.SENDERSTAFFID).FirstOrDefault().BRANCHNAME,
                    //toBranchName = context.TBL_BRANCH.Where(c => c.STATEID == x.RECEIVERSTAFFID).FirstOrDefault().BRANCHNAME,
                };

                var fromData = context.TBL_STAFF.Where(b => b.STAFFID == request.senderStaffId).FirstOrDefault();
                request.from = fromData != null ? fromData.FIRSTNAME + " " + fromData.MIDDLENAME + " " + fromData.LASTNAME : "n/a";

                var toData = context.TBL_STAFF.Where(b => b.STAFFID == request.receiverStaffId).FirstOrDefault();
                request.to = toData !=null ? toData.FIRSTNAME + " " + toData.MIDDLENAME + " " + toData.LASTNAME : "n/a";

                var asigneeData = context.TBL_STAFF.Where(b => b.STAFFID == request.reassignedTo).FirstOrDefault();
                request.assignee = asigneeData != null ? asigneeData.FIRSTNAME + " " + asigneeData.MIDDLENAME + " " + asigneeData.LASTNAME : "n/a";

                requestsList.Add(request);
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
            request.from = fromData != null ? fromData.FIRSTNAME + " " + fromData.MIDDLENAME + " " + fromData.LASTNAME : "n/a";

            var toData = context.TBL_STAFF.Where(b => b.STAFFID == request.receiverStaffId).FirstOrDefault();
            request.to = toData != null ? toData.FIRSTNAME + " " + toData.MIDDLENAME + " " + toData.LASTNAME : "n/a";

            var asigneeData = context.TBL_STAFF.Where(b => b.STAFFID == request.reassignedTo).FirstOrDefault();
            request.assignee = asigneeData != null ? asigneeData.FIRSTNAME + " " + asigneeData.MIDDLENAME + " " + asigneeData.LASTNAME : "n/a";

            return request;

        }

        public IEnumerable<JobRequestMessageViewModel> GetJobComments(int jobRequestId)
        {
            var data =  (from x in context.TBL_JOB_REQUEST_MESSAGE
             where  x.JOBREQUESTID == jobRequestId
             orderby x.DATE_TIME_SENT ascending
             select new JobRequestMessageViewModel
                   {
                       jobRequestId = x.JOBREQUESTID,
                       message = x.MESSAGE,
                       staffId = x.STAFFID,
                       staffName = x.TBL_STAFF.FIRSTNAME
                     }).Take(200);

            return data;
        }


        public IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId)
        {
            return this.context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                 .Select(g => g.TBL_APPROVAL_GROUP)
                 .SelectMany(l => l.TBL_APPROVAL_LEVEL)
                 .SelectMany(s => s.TBL_APPROVAL_LEVEL_STAFF)
                 .Select(s => new OperationStaffViewModel
                 {
                     id = s.STAFFID,
                     name = s.TBL_STAFF.FIRSTNAME,
                     groupId = (int)s.TBL_APPROVAL_LEVEL.GROUPID
                 }).ToList();
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;

            var approvalGroupIds = context.TBL_APPROVAL_GROUP_MAPPING
                .Join(context.TBL_APPROVAL_LEVEL,
                    a => a.GROUPID, b => b.GROUPID, (a, b) => new { a, b })
                .Join(context.TBL_APPROVAL_LEVEL_STAFF,
                    c => c.b.APPROVALLEVELID, d => d.APPROVALLEVELID, (c, d) => new { c, d })
                .Where(x => x.c.a.OPERATIONID == operationId && x.d.STAFFID == staffId)
                    .Select(x => x.c.b.GROUPID);

            return this.GetAllJobRequest().Where(x => approvalGroupIds.Contains(x.departmentId)).OrderByDescending(x => x.jobRequestId).ToList();
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
                    from = allstaff.FirstOrDefault(s => s.id == x.b.SENDERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.SENDERSTAFFID).name,
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
            });
        }

        public IEnumerable<JobTypeViewModel> GetJobSubType(short jobId)
        {
            return this.context.TBL_JOB_TYPE_SUB.Select(x => new JobSubTypeViewModel
            {
                jobTypeId = x.JOBTYPEID,
                jobSubTypeName = x.JOB_SUB_TYPE_NAME,
                jobSubTypeId = x.JOB_SUB_TYPEID
            }).Where(x=>x.jobTypeId == jobId);
        }

        #endregion job-type

        #region Job-Request Document

        public bool AddJobReplyAndDocument(RequestDocumentViewModel model, byte[] file)
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

            JobRequestViewModel jb = new JobRequestViewModel();
            jb.jobRequestId = context.TBL_JOB_REQUEST.Where(x => x.JOBREQUESTCODE == model.jobRequestCode).FirstOrDefault().JOBREQUESTID;
            jb.responseComment = model.comment;
            jb.createdBy = model.createdBy;
            jb.companyId = model.companyId;
            jb.userBranchId = model.userBranchId;
            ReplyJobRequest(jb, jb.jobRequestId);

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

            return docContext.SaveChanges() != 0;
        }


        public bool AddJobDocument(RequestDocumentViewModel model, byte[] file)
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

            var aud = context.SaveChanges() != 0;

            return docContext.SaveChanges() != 0;
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
            return this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Select(x => new RequestDocumentViewModel
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
                fileData = data.FILEDATA,
                fileName = data.FILENAME,
                fileExtension = data.FILEEXTENSION,
                systemDateTime = data.SYSTEMDATETIME,
                physicalFileNumber = data.PHYSICALFILENUMBER,
                physicalLocation = data.PHYSICALLOCATION,
            };
        }

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocuments(string jobRequestCode)
        {
            return this.GetAllJobDocument().Where(x => x.jobRequestCode == jobRequestCode);
        }

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocumentById(int documentId)
        {
            return this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(x=>x.DOCUMENTID == documentId).Select(x => new RequestDocumentViewModel
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
        }

        #endregion Job-Request Document
    }
}