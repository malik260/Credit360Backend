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
    [Export(typeof(IApprovalGroupMappingRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
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
                Position = model.position,                

                CreatedBy = model.createdBy,
                DateTimeCreated = generalSetup.GetApplicaionDate()                
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
                ApplicationDate = generalSetup.GetApplicaionDate(),
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

            entity.Deleted = true;
            entity.DeletedBy = model.createdBy;
            entity.DateTimeDeleted = generalSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var operationName = this.context.tbl_Operations.FirstOrDefault(x => x.OperationId == entity.OperationId).OperationName;
            var groupName = this.context.tbl_Approval_Group.FirstOrDefault(x => x.GroupId == entity.GroupId).GroupName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.BranchId,
                Detail = $"Deleted Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = generalSetup.GetApplicaionDate(),
                TargetId = entity.GroupOperationMappingId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.SaveAll();
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
                        productClassName = data.ProductClassId.HasValue == true ?  data.tbl_Product_Class.ProductClassName : "",
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

        public IEnumerable<ApprovalGroupMappingViewModel> GetApprovalGroupMapping(int operationId, short? productClassId)
        {
            var operationGroups = context.tbl_Approval_Group_Mapping
                .Where(x => x.Deleted != true 
                    && x.OperationId == operationId 
                    && x.ProductClassId == productClassId)
                //.Include(x => x.ProductClass)
                .Select(data => new ApprovalGroupMappingViewModel
                {
                    groupOperationMappingId = data.GroupOperationMappingId,
                    operationId = data.OperationId,
                    operationName = data.tbl_Operations.OperationName,
                    groupId = data.GroupId,
                    groupName = data.tbl_Approval_Group.GroupName,
                    productClassId = data.ProductClassId,
                    //productClassName = data.ProductClass.ProductClassName ?? "n/a", // BUGGY
                    //productClassName = data.ProductClassId == null ? "n/a" : data.ProductClass.ProductClassName, // BUGGY
                    position = data.Position,
                    createdBy = data.CreatedBy,
                });

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
            entity.Position = model.position;

            entity.LastUpdatedBy = model.createdBy;
            entity.DateTimeUpdated = generalSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var operationName = this.context.tbl_Operations.FirstOrDefault(x => x.OperationId == model.operationId).OperationName;
            var groupName = this.context.tbl_Approval_Group.FirstOrDefault(x => x.GroupId == model.groupId).GroupName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ApprovalGroupMappingUpdated,
                StaffId = (int)model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                SystemDateTime = DateTime.Now,
                ApplicationDate = generalSetup.GetApplicaionDate(),
                TargetId = entity.GroupOperationMappingId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.SaveAll();
        }
    }
}
