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

            var data = new tbl_Job_Request
            {
                JobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode(),
                JobTypeId = model.jobTypeId,
                SenderStaffId = model.createdBy,
                ReceiverStaffId = model.receiverStaffId,
                DepartmentId = model.departmentId,
                ReassignedTo = model.reassignedTo,
                IsReassigned = model.isReassigned,
                IsAcknowledged = model.isAcknowledged,
                TargetId = model.targetId,
                OperationsId = model.operationsId, // cam enum
                RequestStatusId = model.requestStatusId, // status enum
                SenderComment = model.senderComment,
                ResponseComment = model.responseComment,
                ArrivalDate = applicationDate,
                SystemArrivalDate = date,
            };

            context.tbl_Job_Request.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobRequestAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added JobRequest '{ model.jobRequestCode }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public string AddGlobalJobRequest(JobRequestViewModel model)
        {
            var date = DateTime.Now;
            var applicationDate = general.GetApplicationDate();

            var data = new tbl_Job_Request
            {
                JobRequestCode = model.jobTypeId + "" + model.createdBy + "" + model.receiverStaffId + "" + this.RequestCode(),
                JobTypeId = model.jobTypeId,
                SenderStaffId = model.createdBy,
                ReceiverStaffId = model.receiverStaffId,
                DepartmentId = model.departmentId,
                ReassignedTo = model.reassignedTo,
                IsReassigned = model.isReassigned,
                IsAcknowledged = model.isAcknowledged,
                TargetId = model.targetId,
                OperationsId = model.operationsId, // cam enum
                RequestStatusId = model.requestStatusId, // status enum
                SenderComment = model.senderComment,
                ResponseComment = model.responseComment,
                ArrivalDate = applicationDate,
                SystemArrivalDate = date,
            };

            context.tbl_Job_Request.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobRequestAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added JobRequest '{ model.jobRequestCode }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return data.JobRequestCode;
        }

        private string RequestCode()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        public bool ReplyJobRequest(JobRequestViewModel model, int jobRequestId)
        {
            var data = this.context.tbl_Job_Request.Find(jobRequestId);
            if (data == null)
            {
                return false;
            }

            var applicationDate = general.GetApplicationDate();

            data.IsAcknowledged = true;
            data.RequestStatusId = 3;
            data.ResponseComment = model.responseComment;
            data.ResponseDate = applicationDate;
            data.SystemResponseDate = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobRequestUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Reply JobRequest '{ model.jobRequestCode }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool ReassignJobRequest(JobRequestViewModel model, int jobRequestId)
        {
            var data = this.context.tbl_Job_Request.Find(jobRequestId);
            if (data == null)
            {
                return false;
            }

            var applicationDate = general.GetApplicationDate();

            data.ReassignedTo = (int)model.reassignedTo;
            data.IsReassigned = true;
            data.IsAcknowledged = true;
            data.RequestStatusId = 2;
            data.ReassignedDate = applicationDate;
            data.SystemReassignedDate = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobRequestUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Reassigned JobRequest '{ model.jobRequestCode }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = applicationDate,
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<JobRequestViewModel> GetAllJobRequest()
        {
            var allstaff = this.context.tbl_Staff.Select(s => new //OperationStaffViewModel
            {
                id = s.StaffId,
                name = s.LastName + " " + s.FirstName
            });

            return this.context.tbl_Job_Request.Select(x => new JobRequestViewModel
            {
                jobRequestId = x.JobRequestId,
                jobRequestCode = x.JobRequestCode,
                jobTypeId = x.JobTypeId,
                senderStaffId = x.SenderStaffId,
                receiverStaffId = x.ReceiverStaffId,
                reassignedTo = x.ReassignedTo,
                isReassigned = x.IsReassigned,
                isAcknowledged = x.IsAcknowledged,
                operationsId = x.OperationsId,
                requestStatusId = x.RequestStatusId,
                senderComment = x.SenderComment,
                responseComment = x.ResponseComment,
                arrivalDate = x.ArrivalDate,
                systemArrivalDate = x.SystemArrivalDate,
                reassignedDate = x.ReassignedDate,
                systemReassignedDate = x.SystemReassignedDate,
                responseDate = x.ResponseDate,
                systemResponseDate = x.SystemResponseDate,
                acknowledgementDate = x.AcknowledgementDate,
                systemAcknowledgementDate = x.SystemAcknowledgementDate,
                from = allstaff.FirstOrDefault(s => s.id == x.SenderStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.SenderStaffId).name,
                to = allstaff.FirstOrDefault(s => s.id == x.ReceiverStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.ReceiverStaffId).name,
                assignee = allstaff.FirstOrDefault(s => s.id == x.ReassignedTo) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.ReassignedTo).name,
                //from = allstaff.GetStaffName(s => s.id == x.SenderStaffId),
                //to = allstaff.GetStaffName(s => s.id == x.ReceiverStaffId),
                //assignee = allstaff.GetStaffName(s => s.id == x.ReassignedTo),
            });
        }

        public JobRequestViewModel GetJobRequest(int jobRequestId)
        {
            var data = this.context.tbl_Job_Request.Find(jobRequestId);

            if (data == null)
            {
                return null;
            }

            return new JobRequestViewModel
            {
                jobRequestId = data.JobRequestId,
                jobRequestCode = data.JobRequestCode,
                jobTypeId = data.JobTypeId,
                senderStaffId = data.SenderStaffId,
                receiverStaffId = data.ReceiverStaffId,
                reassignedTo = data.ReassignedTo,
                isReassigned = data.IsReassigned,
                isAcknowledged = data.IsAcknowledged,
                operationsId = data.OperationsId,
                requestStatusId = data.RequestStatusId,
                senderComment = data.SenderComment,
                responseComment = data.ResponseComment,
                arrivalDate = data.ArrivalDate,
                systemArrivalDate = data.SystemArrivalDate,
                reassignedDate = data.ReassignedDate,
                systemReassignedDate = data.SystemReassignedDate,
                responseDate = data.ResponseDate,
                systemResponseDate = data.SystemResponseDate,
                acknowledgementDate = data.AcknowledgementDate,
                systemAcknowledgementDate = data.SystemAcknowledgementDate,
            };
        }

        public IEnumerable<OperationStaffViewModel> GetOperationStaff(int operationId)
        {
            return this.context.tbl_Approval_Group_Mapping.Where(x => x.OperationId == operationId)
                 .Select(g => g.tbl_Approval_Group)
                 .SelectMany(l => l.tbl_Approval_Level)
                 .SelectMany(s => s.tbl_Approval_Level_Staff)
                 .Select(s => new OperationStaffViewModel
                 {
                     id = s.StaffId,
                     name = s.tbl_Staff.FirstName,
                     groupId = (int)s.tbl_Approval_Level.GroupId
                 }).ToList();
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;

            var approvalGroupIds = context.tbl_Approval_Group_Mapping
                .Join(context.tbl_Approval_Level,
                    a => a.GroupId, b => b.GroupId, (a, b) => new { a, b })
                .Join(context.tbl_Approval_Level_Staff,
                    c => c.b.ApprovalLevelId, d => d.ApprovalLevelId, (c, d) => new { c, d })
                .Where(x => x.c.a.OperationId == operationId && x.d.StaffId == staffId)
                    .Select(x => x.c.b.GroupId);

            return this.GetAllJobRequest().Where(x => approvalGroupIds.Contains(x.departmentId)).OrderByDescending(x => x.jobRequestId).ToList();
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByDepartment(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;
            var departmentId = 0;
            var staff = context.tbl_Staff.Find(staffId);
            if (staff != null) { departmentId = (int)staff.DepartmentId; }

            var allstaff = this.context.tbl_Staff.Select(s => new
            {
                id = s.StaffId,
                name = s.LastName + " " + s.FirstName
            });

            return context.tbl_Department
                .Join(context.tbl_Job_Request.Where(x => x.OperationsId == operationId),
                a => a.DepartmentId, b => b.DepartmentId, (a, b) => new { a, b })
                .Where(x =>
                    x.b.SenderStaffId == staffId
                    || x.b.DepartmentId == departmentId
                    || x.b.ReassignedTo == staffId
                )
                .Select(x => new JobRequestViewModel
                {
                    jobRequestId = x.b.JobRequestId,
                    jobRequestCode = x.b.JobRequestCode,
                    jobTypeId = x.b.JobTypeId,
                    senderStaffId = x.b.SenderStaffId,
                    receiverStaffId = x.b.ReceiverStaffId,
                    reassignedTo = x.b.ReassignedTo,
                    isReassigned = x.b.IsReassigned,
                    isAcknowledged = x.b.IsAcknowledged,
                    operationsId = x.b.OperationsId,
                    requestStatusId = x.b.RequestStatusId,
                    senderComment = x.b.SenderComment,
                    responseComment = x.b.ResponseComment,
                    arrivalDate = x.b.ArrivalDate,
                    systemArrivalDate = x.b.SystemArrivalDate,
                    reassignedDate = x.b.ReassignedDate,
                    systemReassignedDate = x.b.SystemReassignedDate,
                    responseDate = x.b.ResponseDate,
                    systemResponseDate = x.b.SystemResponseDate,
                    acknowledgementDate = x.b.AcknowledgementDate,
                    systemAcknowledgementDate = x.b.SystemAcknowledgementDate,
                    from = allstaff.FirstOrDefault(s => s.id == x.b.SenderStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.SenderStaffId).name,
                    to = allstaff.FirstOrDefault(s => s.id == x.b.ReceiverStaffId) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.ReceiverStaffId).name,
                    assignee = allstaff.FirstOrDefault(s => s.id == x.b.ReassignedTo) == null ? "n/a" : allstaff.FirstOrDefault(s => s.id == x.b.ReassignedTo).name,
                })
                .OrderByDescending(x => x.jobRequestId)
                .Take(100);
        }

        #region job-type

        public bool AddJobType(JobTypeViewModel model)
        {
            var data = new tbl_Job_Type
            {
                JobTypeName = model.jobTypeName,
            };

            context.tbl_Job_Type.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobTypeAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added JobType '{ model.jobTypeName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateJobType(JobTypeViewModel model, short jobTypeId)
        {
            var data = this.context.tbl_Job_Type.Find(jobTypeId);
            if (data == null)
            {
                return false;
            }

            data.JobTypeName = model.jobTypeName;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.JobTypeUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated JobType '{ model.jobTypeName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<JobTypeViewModel> GetAllJobType()
        {
            return this.context.tbl_Job_Type.Select(x => new JobTypeViewModel
            {
                jobTypeId = x.JobTypeId,
                jobTypeName = x.JobTypeName,
            });
        }

        #endregion job-type

        #region Job-Request Document

        public bool AddJobDocument(RequestDocumentViewModel model, byte[] file)
        {
            var data = new tbl_Media_Job_Request_Documents
            {
                FileData = file,
                //LoanApplicationNumber = model.targetId,
                //LoanReferenceNumber = model.targetReferenceNumber,
                //operationId = model.operationId,
                JobRequestCode = model.jobRequestCode,
                DocumentTitle = model.documentTitle,
                DocumentTypeId = model.documentTypeId,
                FileName = model.fileName,
                FileExtension = model.fileExtension,
                SystemDateTime = DateTime.Now,
                PhysicalFileNumber = model.physicalFileNumber,
                PhysicalLocation = model.physicalLocation,
                CreatedBy = (int)model.createdBy,
            };

            docContext.tbl_Media_Job_Request_Documents.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Loan Document '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            var aud = context.SaveChanges() != 0;

            return docContext.SaveChanges() != 0;
        }

        public bool UpdateJobDocument(RequestDocumentViewModel model, int documentId)
        {
            var data = this.docContext.tbl_Media_Job_Request_Documents.Find(documentId);
            if (data == null)
            {
                return false;
            }

            //data.LoanApplicationNumber = model.loanApplicationNumber;
            //data.LoanReferenceNumber = model.loanReferenceNumber;
            data.JobRequestCode = model.jobRequestCode;
            data.DocumentTitle = model.documentTitle;
            data.DocumentTypeId = model.documentTypeId;
            data.FileName = model.fileName;
            data.FileExtension = model.fileExtension;
            data.SystemDateTime = DateTime.Now;
            data.PhysicalFileNumber = model.physicalFileNumber;
            data.PhysicalLocation = model.physicalLocation;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanDocumentUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated LoanDocument '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            var aud = context.SaveChanges() != 0;

            return docContext.SaveChanges() != 0;
        }

        public IEnumerable<RequestDocumentViewModel> GetAllJobDocument()
        {
            return this.docContext.tbl_Media_Job_Request_Documents.Select(x => new RequestDocumentViewModel
            {
                documentId = x.DocumentId,
                //loanApplicationNumber = x.LoanApplicationNumber,
                //loanReferenceNumber = x.LoanReferenceNumber,
                jobRequestCode = x.JobRequestCode,
                documentTitle = x.DocumentTitle,
                documentTypeId = x.DocumentTypeId,
                fileData = x.FileData,
                fileName = x.FileName,
                fileExtension = x.FileExtension,
                systemDateTime = x.SystemDateTime,
                physicalFileNumber = x.PhysicalFileNumber,
                physicalLocation = x.PhysicalLocation,
            });
        }

        public RequestDocumentViewModel GetJobDocument(int documentId)
        {
            var data = this.docContext.tbl_Media_Job_Request_Documents.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new RequestDocumentViewModel
            {
                documentId = data.DocumentId,
                //loanApplicationNumber = data.LoanApplicationNumber,
                //loanReferenceNumber = data.LoanReferenceNumber,
                jobRequestCode = data.JobRequestCode,
                documentTitle = data.DocumentTitle,
                documentTypeId = data.DocumentTypeId,
                fileData = data.FileData,
                fileName = data.FileName,
                fileExtension = data.FileExtension,
                systemDateTime = data.SystemDateTime,
                physicalFileNumber = data.PhysicalFileNumber,
                physicalLocation = data.PhysicalLocation,
            };
        }

        public IEnumerable<RequestDocumentViewModel> GetJobRequestDocument(string jobRequestCode)
        {
            return this.GetAllJobDocument().Where(x => x.jobRequestCode == jobRequestCode);
        }

        #endregion Job-Request Document
    }
}