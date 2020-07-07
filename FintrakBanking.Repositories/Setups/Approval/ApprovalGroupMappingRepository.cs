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
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalGroupMappingRepository : IApprovalGroupMappingRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;
        private IWorkflow workflow;
        private IAdminRepository admin;


        public ApprovalGroupMappingRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup,
                                                IWorkflow _workflow,
                                                IAdminRepository _admin)
        {
            this.context = _context;
            this.generalSetup = genSetup;
            this.auditTrail = _auditTrail;
            this.workflow = _workflow;
            this.admin = _admin;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddApprovalGroupMapping(ApprovalGroupMappingViewModel model)
        {
            //if (context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
            //    && x.OPERATIONID == model.operationId && x.PRODUCTCLASSID == model.productClassId
            //    && x.PRODUCTID == model.productId).Any()) throw new SecureException("Duplicate mapping not allowed!");

            var recordExist = context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x => x.OPERATIONID == model.operationId && x.GROUPID == model.groupId && x.POSITION == model.position).Any();
            if (recordExist)
                throw new ConditionNotMetException("This operation has already been initiated and is approval pending");

            if (admin.IsSuperAdmin(model.staffId) == true)
            {
                var entity = new TBL_APPROVAL_GROUP_MAPPING
                {
                    OPERATIONID = model.operationId,
                    GROUPID = model.groupId,
                    PRODUCTCLASSID = model.productClassId,
                    PRODUCTID = model.productId,
                    POSITION = model.position,
                    ALLOWMULTIPLEINITIATOR = model.allowMultipleInitiator,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = generalSetup.GetApplicationDate(),
                    DELETED=false
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = entity.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);

                var status = this.context.SaveChanges() > 0;

                if (status)
                    return entity.GROUPOPERATIONMAPPINGID;
                else
                    return -1;
            }
            else
            {

                var entity = new TBL_TEMP_APPROVAL_GRP_MAPPING
                {
                    OPERATIONID = model.operationId,
                    GROUPID = model.groupId,
                    PRODUCTCLASSID = model.productClassId,
                    PRODUCTID = model.productId,
                    POSITION = model.position,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = generalSetup.GetApplicationDate(),
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    OPERATION = "create"
                };

                this.context.TBL_TEMP_APPROVAL_GRP_MAPPING.Add(entity);
                if (this.context.SaveChanges() > 0)
                {
                    model.tempGroupOperationMappingId = entity.TEMPGROUPOPERATIONMAPPINGID;
                }


                // Audit Section ---------------------------
                var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.tempGroupOperationMappingId;
                workflow.Comment = $"New approval request for group operation mapping for Operation: {operationName} in Group: {groupName}";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingAdded,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Approval Group Mapping for Operation: {operationName} in Group: {groupName} was added and is going for approval",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = entity.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);

                var status = this.SaveAll();

                if (status)
                    return entity.GROUPOPERATIONMAPPINGID;
                else
                    return -1;
            }

            //end of Audit section -------------------------------

        }

        public bool DeleteApprovalGroupMapping(int operationMappingId, UserInfo model)
        {
            int tempGroupOperationMappingId = 0;
            var data = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);
            var dataExist = context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.GROUPID == data.GROUPID && x.OPERATIONID == data.OPERATIONID).Any();

            if (dataExist)
                throw new ConditionNotMetException("This operation has already been initiated and is apprival pending");

            if (data == null)
                return false;
            if (admin.IsSuperAdmin(model.staffId) == true)
            {
                //data.DELETEDBY = model.createdBy;
                //data.DATETIMEDELETED = generalSetup.GetApplicationDate();
                //data.DELETED = true;

                context.TBL_APPROVAL_GROUP_MAPPING.Remove(data);

                 var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == data.OPERATIONID).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == data.GROUPID).GROUPNAME;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.BranchId,
                    DETAIL = $"Delete Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = data.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()

                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                var entity = new TBL_TEMP_APPROVAL_GRP_MAPPING
                {
                    OPERATIONID = data.OPERATIONID,
                    GROUPID = data.GROUPID,
                    PRODUCTCLASSID = data.PRODUCTCLASSID,
                    PRODUCTID = data.PRODUCTID,
                    POSITION = data.POSITION,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = generalSetup.GetApplicationDate(),
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    GROUPOPERATIONMAPPINGID = data.GROUPOPERATIONMAPPINGID,
                    OPERATION = "delete"

                };

                context.TBL_TEMP_APPROVAL_GRP_MAPPING.Add(entity);
                if (this.context.SaveChanges() > 0)
                {
                    tempGroupOperationMappingId = entity.TEMPGROUPOPERATIONMAPPINGID;
                }

                // Audit Section ---------------------------
                var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == entity.OPERATIONID).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == entity.GROUPID).GROUPNAME;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = tempGroupOperationMappingId;
                workflow.Comment = $"Request to Delete Approval Group Mapping for Operation: {operationName} in Group: {groupName}";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.BranchId,
                    DETAIL = $"Request to Delete Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = entity.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()

                };

                this.auditTrail.AddAuditTrail(audit);
            }

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
                                                allowMultipleInitiator = data.ALLOWMULTIPLEINITIATOR
                                            })
                                            .OrderBy(x => x.position).ToList();

            return operationGroups;
        }

        public bool UpdateApprovalGroupMapping(int operationMappingId, ApprovalGroupMappingViewModel model)
        {
            var data = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);

            if (data == null)
                return false;

            if (admin.IsSuperAdmin(model.staffId) == true)
            {
                data.OPERATIONID = model.operationId;
                data.GROUPID = model.groupId;
                data.PRODUCTCLASSID = model.productClassId;
                data.PRODUCTID = model.productId;
                data.POSITION = model.position;
                data.CREATEDBY = model.createdBy;
                data.DATETIMECREATED = generalSetup.GetApplicationDate();
                data.GROUPOPERATIONMAPPINGID = operationMappingId;
                data.ALLOWMULTIPLEINITIATOR = model.allowMultipleInitiator;

                var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingUpdated,
                    STAFFID = (int)model.lastUpdatedBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $" Approval Group Mapping for Operation: {operationName} in Group: {groupName} was updated by a super-admin",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = data.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
            }
            else
            {
                var entity = new TBL_TEMP_APPROVAL_GRP_MAPPING
                {
                    OPERATIONID = model.operationId,
                    GROUPID = model.groupId,
                    PRODUCTCLASSID = model.productClassId,
                    PRODUCTID = model.productId,
                    POSITION = model.position,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = generalSetup.GetApplicationDate(),
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    GROUPOPERATIONMAPPINGID = operationMappingId,
                    ALLOWMULTIPLEINITIATOR = model.allowMultipleInitiator,
                    OPERATION = "update"
                };

                context.TBL_TEMP_APPROVAL_GRP_MAPPING.Add(entity);
                if (this.context.SaveChanges() > 0)
                {
                    model.tempGroupOperationMappingId = entity.TEMPGROUPOPERATIONMAPPINGID;
                }

                // Audit Section ---------------------------
                var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.tempGroupOperationMappingId;
                workflow.Comment = $"Update approval request for group operation mapping for Operation: {operationName} in Group: {groupName}";
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingUpdated,
                    STAFFID = (int)model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    SYSTEMDATETIME = DateTime.Now,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    TARGETID = entity.GROUPOPERATIONMAPPINGID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
            }

            try
            {
                return this.context.SaveChanges() > 0;
            }catch(Exception ex)
            {
                return false;
            }
        }

        public int GoForApproval(ApprovalGroupMappingViewModel model)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.tempGroupOperationMappingId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (model.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            UpdateMainApprovalGroupMapping(model, (short)workflow.StatusId);
                        }
                    }

                    responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return model.approvalStatusId;
                    }
                    return 0;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }

        private void UpdateMainApprovalGroupMapping(ApprovalGroupMappingViewModel ApprovalModel, short status)
        {
            var data = this.context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x => x.TEMPGROUPOPERATIONMAPPINGID == ApprovalModel.tempGroupOperationMappingId).Select(x => x).FirstOrDefault();
            if (data != null)
            {
                var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == data.OPERATIONID).OPERATIONNAME;
                var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == data.GROUPID).GROUPNAME;

                var audit = new TBL_AUDIT();


                if (data.OPERATION == "create")
                {
                    CreateApprovalGroup(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingAdded;
                    audit.DETAIL = $"Approval Group Mapping for Operation: {operationName} in Group: {groupName} is added successfully";

                }
                else if (data.OPERATION == "update")
                {
                    UpdateApprovalGroup(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingUpdated;
                    audit.DETAIL = $"Approval Group Mapping for Operation: {operationName} in Group: {groupName} is updated successfully";

                }
                else if (data.OPERATION == "delete")
                {
                    DeleteApprovalGroup(data);

                    audit.AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingDeleted;
                    audit.DETAIL = $"Approval Group Mapping for Operation: {operationName} in Group: {groupName} is delete successfully";

                }

                UpdateTempApprovalGroup(ApprovalModel, status);

                audit.STAFFID = ApprovalModel.createdBy;
                audit.BRANCHID = (short)ApprovalModel.userBranchId;
                audit.IPADDRESS = ApprovalModel.userIPAddress;
                audit.URL = ApprovalModel.applicationUrl;
                audit.APPLICATIONDATE = generalSetup.GetApplicationDate();
                audit.SYSTEMDATETIME = DateTime.Now;
                audit.TARGETID = ApprovalModel.groupOperationMappingId;

                context.TBL_AUDIT.Add(audit);
            }
        }

        private void CreateApprovalGroup(TBL_TEMP_APPROVAL_GRP_MAPPING data)
        {
            var updateData = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == data.OPERATIONID && x.GROUPID == data.GROUPID).Select(x => x).FirstOrDefault();
            if (updateData != null)
            {
                updateData.DELETED = false;
                updateData.DELETEDBY = null;
                updateData.DATETIMEDELETED = null;
            }
            else
            {
                var entity = new TBL_APPROVAL_GROUP_MAPPING
                {
                    OPERATIONID = data.OPERATIONID,
                    GROUPID = data.GROUPID,
                    PRODUCTCLASSID = data.PRODUCTCLASSID,
                    PRODUCTID = data.PRODUCTID,
                    POSITION = data.POSITION,
                    CREATEDBY = data.CREATEDBY,
                    DATETIMECREATED = generalSetup.GetApplicationDate(),
                    DELETED = false,
                };

                context.TBL_APPROVAL_GROUP_MAPPING.Add(entity);
            }
        }
        private void UpdateApprovalGroup(TBL_TEMP_APPROVAL_GRP_MAPPING data)
        {
            var updateData = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.GROUPOPERATIONMAPPINGID == data.GROUPOPERATIONMAPPINGID).Select(x => x).FirstOrDefault();

            if (updateData != null)
            {
                updateData.OPERATIONID = data.OPERATIONID;
                updateData.GROUPID = data.GROUPID;
                updateData.PRODUCTCLASSID = data.PRODUCTCLASSID;
                updateData.PRODUCTID = data.PRODUCTID;
                updateData.POSITION = data.POSITION;
                updateData.LASTUPDATEDBY = data.CREATEDBY;
                updateData.DATETIMEUPDATED = data.DATETIMECREATED;
                updateData.DELETED = false;
            }
        }

        private void DeleteApprovalGroup(TBL_TEMP_APPROVAL_GRP_MAPPING data)
        {
            var updateData = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.GROUPOPERATIONMAPPINGID == data.GROUPOPERATIONMAPPINGID).Select(x => x).FirstOrDefault();

            if (updateData != null)
            {
                //updateData.DELETED = true;
                //updateData.DELETEDBY = data.CREATEDBY;
                //updateData.DATETIMEDELETED = data.DATETIMECREATED;
                context.TBL_APPROVAL_GROUP_MAPPING.Remove(updateData);
            }
        }
        private void UpdateTempApprovalGroup(ApprovalGroupMappingViewModel data, short status)
        {
            var update = context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x => x.TEMPGROUPOPERATIONMAPPINGID == data.tempGroupOperationMappingId).Select(x => x).FirstOrDefault();
            if (update != null)
            {
                update.APPROVALSTATUSID = status;
            }
        }

        public List<ApprovalGroupMappingViewModel> GetTempApprovalGroupForApproval(int staffId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ApprovalWorkflowGroupModification).ToList();

            var insurance = (from x in context.TBL_TEMP_APPROVAL_GRP_MAPPING
                             join atrail in context.TBL_APPROVAL_TRAIL on x.TEMPGROUPOPERATIONMAPPINGID equals atrail.TARGETID
                             where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.ApprovalWorkflowGroupModification
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
                             select new ApprovalGroupMappingViewModel
                             {
                                 tempGroupOperationMappingId = x.TEMPGROUPOPERATIONMAPPINGID,
                                 groupName = context.TBL_APPROVAL_GROUP.Where(a => a.GROUPID == x.GROUPID).Select(a => a.GROUPNAME).FirstOrDefault(),
                                 operationName = context.TBL_OPERATIONS.Where(a => a.OPERATIONID == x.OPERATIONID).Select(a => a.OPERATIONNAME).FirstOrDefault(),
                                 operation = x.OPERATION
                             }).ToList();

            return insurance;
        }

    }

}
