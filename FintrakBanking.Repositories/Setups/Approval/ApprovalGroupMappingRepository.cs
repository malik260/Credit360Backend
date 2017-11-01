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
            var entity = new TBL_APPROVAL_GROUP_MAPPING
            {
                OPERATIONID = model.operationId,
                GROUPID = model.groupId,
                PRODUCTCLASSID = model.productClassId,
                PRODUCTID = model.productId,
                POSITION = model.position,

                CREATEDBY = model.createdBy,
                DATETIMECREATED = generalSetup.GetApplicationDate()
            };

            this.context.TBL_APPROVAL_GROUP_MAPPING.Add(entity);

            // Audit Section ---------------------------
            var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
            var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingAdded,
                STAFFID = (int)model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                TARGETID = entity.GROUPOPERATIONMAPPINGID
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            var status = this.SaveAll();

            if (status)
                return entity.GROUPOPERATIONMAPPINGID;
            else
                return -1;
        }

        public bool DeleteApprovalGroupMapping(int operationMappingId, UserInfo model)
        {
            var entity = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);

            if (entity == null)
                return false;

            //entity.Deleted = true;
            //entity.DeletedBy = model.createdBy;
            //entity.DateTimeDeleted = generalSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == entity.OPERATIONID).OPERATIONNAME;
            var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == entity.GROUPID).GROUPNAME;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                STAFFID = (int)model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Deleted Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                TARGETID = entity.GROUPOPERATIONMAPPINGID
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            context.TBL_APPROVAL_GROUP_MAPPING.Remove(entity);

            return this.context.SaveChanges() > 0;
        }

        public IQueryable<ApprovalGroupMappingViewModel> GetAllApprovalGroupMapping()
        {
            var groupdata = (from data in context.TBL_APPROVAL_GROUP_MAPPING
                             where data.DELETED == false //orderby account.AccountCode ascending, account.AccountName ascending
                             select new ApprovalGroupMappingViewModel()
                             {
                                 groupOperationMappingId = data.GROUPOPERATIONMAPPINGID,
                                 operationId = data.OPERATIONID,
                                 operationName = data.TBL_OPERATIONS.OPERATIONNAME,
                                 groupId = data.GROUPID,
                                 groupName = data.TBL_APPROVAL_GROUP.GROUPNAME,
                                 productClassId = data.PRODUCTCLASSID,
                                 productClassName = data.PRODUCTCLASSID.HasValue == true ? data.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME : "",
                                 position = data.POSITION,
                                 createdBy = data.CREATEDBY,
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
            var operationGroups = context.TBL_APPROVAL_GROUP_MAPPING
                                            .Where(x => x.DELETED == false
                                                && x.OPERATIONID == operationId
                                                && x.PRODUCTCLASSID == productClassId
                                                && x.PRODUCTID == productId
                                            )
                                            .Select(data => new ApprovalGroupMappingViewModel
                                            {
                                                groupOperationMappingId = data.GROUPOPERATIONMAPPINGID,
                                                operationId = data.OPERATIONID,
                                                productClassId = data.PRODUCTCLASSID,
                                                operationName = data.TBL_OPERATIONS.OPERATIONNAME,
                                                groupId = data.GROUPID,
                                                groupName = data.TBL_APPROVAL_GROUP.GROUPNAME,
                                                position = data.POSITION,
                                                createdBy = data.CREATEDBY,
                                            })
                                            .OrderBy(x => x.position);

            return operationGroups;
        }

        public bool UpdateApprovalGroupMapping(int operationMappingId, ApprovalGroupMappingViewModel model)
        {
            var entity = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);

            if (entity == null)
                return false;

            entity.OPERATIONID = model.operationId;
            entity.GROUPID = model.groupId;
            entity.PRODUCTCLASSID = model.productClassId;
            entity.PRODUCTID = model.productId;
            entity.POSITION = model.position;

            entity.LASTUPDATEDBY = model.createdBy;
            entity.DATETIMEUPDATED = generalSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
            var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingUpdated,
                STAFFID = (int)model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                TARGETID = entity.GROUPOPERATIONMAPPINGID
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return this.context.SaveChanges() > 0;
        }
    }
}
