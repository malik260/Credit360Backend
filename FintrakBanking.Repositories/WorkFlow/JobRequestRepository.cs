using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Entities.DocumentModels;

namespace FintrakBanking.Repositories.WorkFlow
{
    public class JobRequestRepository : IJobRequestRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingDocumentsContext docContext;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public JobRequestRepository(FinTrakBankingDocumentsContext docContext, FinTrakBankingContext _context, IGeneralSetupRepository _general, IAuditTrailRepository _audit)
        {
            this.context = _context;
            this.docContext = docContext;
            this.general = _general;
            this.audit = _audit;
        }

        public bool AddJobRequest(JobRequestViewModel model)
        {
             
            var date = DateTime.Now;
            var applicationDate = general.GetApplicationDate();
           
           var data = new TBL_JOB_REQUEST
            {
                JOBREQUESTCODE = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode(),
                JOBTYPEID = model.jobTypeId,
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

        public string AddGlobalJobRequest(JobRequestViewModel model)
        {
            if (model.receiverStaffId == model.createdBy)
                throw new Exception("You cannot assign a job to yourself");

            var date = DateTime.Now;
            var applicationDate = general.GetApplicationDate();
            model.jobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode();
            model.requestStatusId = 1;
            if(model.operationsId == 0)  model.operationsId = 1;

           var data = new TBL_JOB_REQUEST
            {
                JOBREQUESTCODE = model.jobRequestCode,
                JOBTYPEID = model.jobTypeId,
                SENDERSTAFFID = model.createdBy,
                RECEIVERSTAFFID = model.receiverStaffId,
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
            context.SaveChanges();
            return job.JOBREQUESTCODE;
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
                STAFFID = model.lastUpdatedBy,
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
                receiverStaffId = x.RECEIVERSTAFFID,
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

        private IEnumerable<JobRequestViewModel> GetAllGlobalJobRequest(int staffId)
        {
            var allstaff = this.context.TBL_STAFF.Select(s => new //OperationStaffViewModel
            {
                id = s.STAFFID,
                name = s.LASTNAME + " " + s.FIRSTNAME
            });

              return  context.TBL_JOB_REQUEST
               .Where(t => ((t.DEPARTMENTUNITID == context.TBL_STAFF.Where(l=>l.STAFFID == staffId).FirstOrDefault().DEPARTMENT_UNITID) || (t.SENDERSTAFFID == staffId) || (t.REASSIGNEDTO == staffId) ))
               .Select(
                  x =>
                     new JobRequestViewModel
                     {
                    jobRequestId = x.JOBREQUESTID,
                    jobRequestCode = x.JOBREQUESTCODE,
                    jobTypeId = x.JOBTYPEID,
                    senderStaffId = x.SENDERSTAFFID,
                    receiverStaffId = x.RECEIVERSTAFFID,
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
                    from = allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID) == null ? "n/al" : allstaff.FirstOrDefault(s => s.id == x.SENDERSTAFFID).name,
                    fromBranchName = context.TBL_BRANCH.Where(c=>c.STATEID == x.SENDERSTAFFID).FirstOrDefault().BRANCHNAME,
                    to = allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.RECEIVERSTAFFID).name,
                    assignee = allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.REASSIGNEDTO).name,
                    //from = allstaff.GetStaffName(s => s.id == x.SenderStaffId),
                    //to = allstaff.GetStaffName(s => s.id == x.ReceiverStaffId),
                    //assignee = allstaff.GetStaffName(s => s.id == x.ReassignedTo),
                }).OrderByDescending(x=>x.arrivalDate).Take(500);
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByStaffId(int staffId)
        {
            return GetAllGlobalJobRequest(staffId).OrderByDescending(x => x.jobRequestId); 
        }

        public JobRequestViewModel GetJobRequest(int jobRequestId)
        {
            var data = this.context.TBL_JOB_REQUEST.Find(jobRequestId);

            if (data == null)
            {
                return null;
            }

            return new JobRequestViewModel
            {
                jobRequestId = data.JOBREQUESTID,
                jobRequestCode = data.JOBREQUESTCODE,
                jobTypeId = data.JOBTYPEID,
                senderStaffId = data.SENDERSTAFFID,
                receiverStaffId = data.RECEIVERSTAFFID,
                reassignedTo = data.REASSIGNEDTO,
                isReassigned = data.ISREASSIGNED,
                isAcknowledged = data.ISACKNOWLEDGED,
                operationsId = data.OPERATIONSID,
                requestStatusId = data.REQUESTSTATUSID,
                senderComment = data.SENDERCOMMENT,
                responseComment = data.RESPONSECOMMENT,
                arrivalDate = data.ARRIVALDATE,
                systemArrivalDate = data.SYSTEMARRIVALDATE,
                reassignedDate = data.REASSIGNEDDATE,
                systemReassignedDate = data.SYSTEMREASSIGNEDDATE,
                responseDate = data.RESPONSEDATE,
                systemResponseDate = data.SYSTEMRESPONSEDATE,
                acknowledgementDate = data.ACKNOWLEDGEMENTDATE,
                systemAcknowledgementDate = data.SYSTEMACKNOWLEDGEMENTDATE,
            };
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
            if (staff != null) { departmentId = (int)staff.DEPARTMENTID; }

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
                    receiverStaffId = x.b.RECEIVERSTAFFID,
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

        #endregion job-type

        #region Job-Request Document

        public bool AddJobDocument(RequestDocumentViewModel model, byte[] file)
        {
            var data = new TBL_MEDIA_JOB_REQUEST_DOCUMENTS
            {
                FILEDATA = file,
                //LoanApplicationNumber = model.targetId,
                //LoanReferenceNumber = model.targetReferenceNumber,
                //operationId = model.operationId,
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

            docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENTS.Add(data);

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
            var data = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENTS.Find(documentId);
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
            return this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENTS.Select(x => new RequestDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                //loanApplicationNumber = x.LoanApplicationNumber,
                //loanReferenceNumber = x.LoanReferenceNumber,
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

        public RequestDocumentViewModel GetJobDocument(int documentId)
        {
            var data = this.docContext.TBL_MEDIA_JOB_REQUEST_DOCUMENTS.Find(documentId);

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

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocument(string jobRequestCode)
        {
            return this.GetAllJobDocument().Where(x => x.jobRequestCode == jobRequestCode);
        }

        #endregion Job-Request Document
    }
}