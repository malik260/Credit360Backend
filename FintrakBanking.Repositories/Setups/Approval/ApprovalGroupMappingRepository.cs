using FintrakBanking.Interfaces.Setups.Approval;
using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using System.Linq;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalGroupMappingRepository : IApprovalGroupMappingRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;

        public ApprovalGroupMappingRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup)
        {
            this.context = _context;
            this.generalSetup = genSetup;
            this.auditTrail = _auditTrail;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddApprovalGroupMapping(ApprovalGroupMappingViewModel model)
        {
            var entity = new tbl_Approval_Group_Mapping
            {
                OperationId = model.operationId,
                GroupId = model.groupId,
                ProductClassId = model.productClassId,
                ProductId = model.productId,
                Position = model.position,

                CreatedBy = model.createdBy,
                DateTimeCreated = generalSetup.GetApplicationDate()
            };

            this.context.tbl_Approval_Group_Mapping.Add(entity);

            // Audit Section ---------------------------
            var operationName = this.context.tbl_Operations.FirstOrDefault(x => x.OperationId == model.operationId).OperationName;
            var groupName = this.context.tbl_Approval_Group.FirstOrDefault(x => x.GroupId == model.groupId).GroupName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupMappingAdded,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = generalSetup.GetApplicationDate(),
                TargetId = entity.GroupOperationMappingId
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var status = this.SaveAll();

            if (status)
                return entity.GroupOperationMappingId;
            else
                return -1;
        }

        public bool DeleteApprovalGroupMapping(int operationMappingId, UserInfo model)
        {
            var entity = this.context.tbl_Approval_Group_Mapping.Find(operationMappingId);

            if (entity == null)
                return false;

            //entity.Deleted = true;
            //entity.DeletedBy = model.createdBy;
            //entity.DateTimeDeleted = generalSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var operationName = this.context.tbl_Operations.FirstOrDefault(x => x.OperationId == entity.OperationId).OperationName;
            var groupName = this.context.tbl_Approval_Group.FirstOrDefault(x => x.GroupId == entity.GroupId).GroupName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                StaffId = (int)model.staffId,
                BranchId = (short)model.BranchId,
                Detail = $"Deleted Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = generalSetup.GetApplicationDate(),
                TargetId = entity.GroupOperationMappingId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            context.tbl_Approval_Group_Mapping.Remove(entity);

            return this.context.SaveChanges() > 0;
        }

        public IQueryable<ApprovalGroupMappingViewModel> GetAllApprovalGroupMapping()
        {
            var groupdata = (from data in context.tbl_Approval_Group_Mapping
                             where data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                             select new ApprovalGroupMappingViewModel()
                             {
                                 groupOperationMappingId = data.GroupOperationMappingId,
                                 operationId = data.OperationId,
                                 operationName = data.tbl_Operations.OperationName,
                                 groupId = data.GroupId,
                                 groupName = data.tbl_Approval_Group.GroupName,
                                 productClassId = data.ProductClassId,
                                 productClassName = data.ProductClassId.HasValue == true ? data.tbl_Product_Class.ProductClassName : "",
                                 position = data.Position,
                                 createdBy = data.CreatedBy,
                             });
            return groupdata;
        }

        public ApprovalGroupMappingViewModel GetApprovalGroupMapping(int operationMappingId)
        {
            return (from data in GetAllApprovalGroupMapping()
                    where data.groupOperationMappingId == operationMappingId
                    select data).FirstOrDefault();
        }

        public IEnumerable<ApprovalGroupMappingViewModel> GetApprovalGroupMapping(
            int operationId,
            short? productClassId,
            short? productId
            )
        {
            var operationGroups = context.tbl_Approval_Group_Mapping
                                            .Where(x => x.Deleted == false
                                                && x.OperationId == operationId
                                                && x.ProductClassId == productClassId
                                                && x.ProductId == productId
                                            )
                                            .Select(data => new ApprovalGroupMappingViewModel
                                            {
                                                groupOperationMappingId = data.GroupOperationMappingId,
                                                operationId = data.OperationId,
                                                productClassId = data.ProductClassId,
                                                operationName = data.tbl_Operations.OperationName,
                                                groupId = data.GroupId,
                                                groupName = data.tbl_Approval_Group.GroupName,
                                                position = data.Position,
                                                createdBy = data.CreatedBy,
                                            })
                                            .OrderBy(x => x.position);

            return operationGroups;
        }

        public bool UpdateApprovalGroupMapping(int operationMappingId, ApprovalGroupMappingViewModel model)
        {
            var entity = this.context.tbl_Approval_Group_Mapping.Find(operationMappingId);

            if (entity == null)
                return false;

            entity.OperationId = model.operationId;
            entity.GroupId = model.groupId;
            entity.ProductClassId = model.productClassId;
            entity.ProductId = model.productId;
            entity.Position = model.position;

            entity.LastUpdatedBy = model.createdBy;
            entity.DateTimeUpdated = generalSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var operationName = this.context.tbl_Operations.FirstOrDefault(x => x.OperationId == model.operationId).OperationName;
            var groupName = this.context.tbl_Approval_Group.FirstOrDefault(x => x.GroupId == model.groupId).GroupName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupMappingUpdated,
                StaffId = (int)model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = generalSetup.GetApplicationDate(),
                TargetId = entity.GroupOperationMappingId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.context.SaveChanges() > 0;
        }
    }
}
