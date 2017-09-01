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

namespace FintrakBanking.Repositories.WorkFlow
{
    public class JobRequestRepository : IJobRequestRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public JobRequestRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddJobRequest(JobRequestViewModel model)
        {
            var date = DateTime.Now;
            var applicationDate = general.GetApplicationDate();
            var data = new tbl_Job_Request
            {
                JobRequestCode = this.RequestCode(),
                JobTypeId = model.jobTypeId,
                SenderStaffId = model.createdBy,
                ReceiverStaffId = model.receiverStaffId,
                //StaffApprovalGroupId=model.staffApprovalGroupId,
                //ReassignedTo = model.reassignedTo,
                //IsReassigned = model.isReassigned,
                //IsAcknowledged = model.isAcknowledged,
                //TargetId = model.targetId,
                OperationsId = 6, // cam enum
                RequestStatusId = 1, // status enum
                SenderComment = model.senderComment,
                //ResponseComment = model.responseComment,
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

        private string RequestCode()
        {
            Random random = new Random();
            return random.Next().ToString();
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

            data.ReassignedTo = (int) model.reassignedTo;
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
            var allstaff = this.context.tbl_Staff.Select( s => new //OperationStaffViewModel
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
                .SelectMany(g => g.tbl_Approval_Level)
                .SelectMany(l => l.tbl_Approval_Level_Staff)
                .Select(s => new OperationStaffViewModel
                {
                    id = s.StaffId,
                    name = s.tbl_Staff.FirstName,
                    groupId = s.tbl_Approval_Level.GroupOperationMappingId
                }).ToList();
        }

        public IEnumerable<JobRequestViewModel> GetJobRequestByGroupId(int staffId)
        {
            var operationId = (int)OperationsEnum.CAM;

            var approvalGroupIds = context.tbl_Approval_Group_Mapping
                .Join(context.tbl_Approval_Level,
                    a => a.GroupOperationMappingId, b => b.GroupOperationMappingId, (a, b) => new { a, b })
                .Join(context.tbl_Approval_Level_Staff,
                    c => c.b.ApprovalLevelId, d => d.ApprovalLevelId, (c, d) => new { c, d })
                .Where(x => x.c.a.OperationId == operationId && x.d.StaffId == staffId)
                    .Select(x => x.c.b.GroupOperationMappingId);

            return this.GetAllJobRequest().Where(x => approvalGroupIds.Contains(x.staffApprovalGroupId)).OrderByDescending(x => x.jobRequestId).ToList();
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

        #endregion
    }
}
