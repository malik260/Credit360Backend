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

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalGroupMappingRepository : IApprovalGroupMappingRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository generalSetup;
        private IWorkflow workflow;


        public ApprovalGroupMappingRepository(FinTrakBankingContext _context,
                                                IAuditTrailRepository _auditTrail,
                                                IGeneralSetupRepository genSetup,
                                                IWorkflow _workflow)
        {
            this.context = _context;
            this.generalSetup = genSetup;
            this.auditTrail = _auditTrail;
            this.workflow = _workflow;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddApprovalGroupMapping(ApprovalGroupMappingViewModel model)
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
            var data = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);

            if (data == null)
                return false;

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
                OPERATION="delete"
               
            };

            context.TBL_TEMP_APPROVAL_GRP_MAPPING.Add(entity);

            // Audit Section ---------------------------
            var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == entity.OPERATIONID).OPERATIONNAME;
            var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == entity.GROUPID).GROUPNAME;

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = data.GROUPOPERATIONMAPPINGID;
            workflow.Comment = $"Request to Delete Approval Group Mapping for Operation: {operationName} in Group: {groupName}";
            workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalGroupMappingDeleted,
                STAFFID = (int)model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Request to Delete Approval Group Mapping for Operation: {operationName} in Group: {groupName}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                TARGETID = entity.GROUPOPERATIONMAPPINGID,

            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

           // context.TBL_TEMP_APPROVAL_GROUP_MAPPING.Remove(entity);

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
            var data = this.context.TBL_APPROVAL_GROUP_MAPPING.Find(operationMappingId);

            if (data == null)
                return false;

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
                GROUPOPERATIONMAPPINGID = operationMappingId,
                OPERATION="update"
            };

            context.TBL_TEMP_APPROVAL_GRP_MAPPING.Add(entity);

            // Audit Section ---------------------------
            var operationName = this.context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == model.operationId).OPERATIONNAME;
            var groupName = this.context.TBL_APPROVAL_GROUP.FirstOrDefault(x => x.GROUPID == model.groupId).GROUPNAME;

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = operationMappingId;
            workflow.Comment = $"Update approval request for group operation mapping for Operation: {operationName} in Group: {groupName}";
            workflow.OperationId = (int)OperationsEnum.ApprovalWorkflowGroupModification;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

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
            var data = this.context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x=>x.GROUPOPERATIONMAPPINGID==ApprovalModel.tempGroupOperationMappingId).Select(x=>x).FirstOrDefault();
            if (data != null)
            {
                if (data.OPERATION=="create")
                {
                    CreateApprovalGroup(data);
                }
                else if (data.OPERATION == "update")
                {
                    UpdateApprovalGroup(data);
                }
                else if (data.OPERATION == "delete")
                {
                    DeleteApprovalGroup(data);
                }

                UpdateTempApprovalGroup(ApprovalModel, status);

            }
        }

        private void CreateApprovalGroup(TBL_TEMP_APPROVAL_GRP_MAPPING data)
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
        private void UpdateApprovalGroup(TBL_TEMP_APPROVAL_GRP_MAPPING data)
        {
          var updateData=  context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.GROUPOPERATIONMAPPINGID == data.GROUPOPERATIONMAPPINGID).Select(x => x).FirstOrDefault();

            if (updateData!=null)
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
                updateData.DELETED = true;
                updateData.DELETEDBY = data.CREATEDBY;
                updateData.DATETIMEDELETED = data.DATETIMECREATED;
            }
        }
        private void UpdateTempApprovalGroup(ApprovalGroupMappingViewModel data, short status)
        {
            var update = context.TBL_TEMP_APPROVAL_GRP_MAPPING.Where(x => x.TEMPGROUPOPERATIONMAPPINGID == data.tempGroupOperationMappingId).Select(x => x).FirstOrDefault();
            if (update!=null)
            {
                update.APPROVALSTATUSID = status;
            }
        }

        public List<ApprovalGroupMappingViewModel> GetTempApprovalGroupForApproval(int staffId)
        {
            var ids = generalSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ApprovalWorkflowGroupModification).ToList();

            var insurance = (from x in context.TBL_TEMP_APPROVAL_GRP_MAPPING
                             join s in context.TBL_OPERATIONS_TYPE on x.OPERATIONID equals s.OPERATIONTYPEID
                             join atrail in context.TBL_APPROVAL_TRAIL on x.GROUPOPERATIONMAPPINGID equals atrail.TARGETID
                             join a in context.TBL_APPROVAL_GROUP on x.GROUPID equals a.GROUPID
                             where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.ApprovalWorkflowGroupModification
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.RESPONSESTAFFID == null
                             select new ApprovalGroupMappingViewModel
                             {
                                 tempGroupOperationMappingId=x.TEMPGROUPOPERATIONMAPPINGID,
                                 groupName = a.GROUPNAME,
                                 operationName = s.OPERATIONTYPENAME

                             }).ToList();

            return insurance;
        }

    }

}
