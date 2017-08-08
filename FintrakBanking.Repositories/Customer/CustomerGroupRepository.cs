
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Admin;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Customer;
using System.ComponentModel.Composition;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Business;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ICustomerGroupRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomerGroupRepository : ICustomerGroupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkFlowRepository workFlow;


        public CustomerGroupRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IWorkFlowRepository _workFlow)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            workFlow = _workFlow;
        }

        private bool SaveAll()
        {
            return context.SaveChanges() > 0;
        }

        #region tbl_Customer - Group
        public bool AddCustomerGroup(CustomerGroupViewModel entity)
        {
            var group = new tbl_Customer_Group
            {
                GroupCode = entity.groupCode,
                GroupName = entity.groupName,
                GroupDescription = entity.groupDescription,
                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = genSetup.GetApplicaionDate()
            };
            context.tbl_Customer_Group.Add(group);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName } )",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------


            return context.SaveChanges() != 0;

        }

        public bool AddTempCustomerGroup(CustomerGroupViewModel custGroupModel)
        {
            bool output = false;

            var tempGroup = new tbl_Temp_Customer_Group
            {
                GroupCode = custGroupModel.groupCode,
                GroupName = custGroupModel.groupName,
                GroupDescription = custGroupModel.groupDescription,
                CreatedBy = (int)custGroupModel.createdBy,
                DateTimeCreated = genSetup.GetApplicaionDate()
            };

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = custGroupModel.createdBy,
                BranchId = (short)custGroupModel.userBranchId,
                Detail = $"Added Customer Group: { custGroupModel.groupName } with Code: { custGroupModel.groupCode } ( { custGroupModel.groupName } )",
                IPAddress = custGroupModel.userIPAddress,
                Url = custGroupModel.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------


            if (workFlow.CheckRouteForOperation((int)Operations.CustomerGroupCreation, custGroupModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        context.tbl_Temp_Customer_Group.Add(tempGroup);
                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = custGroupModel.createdBy,
                            companyId = custGroupModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = custGroupModel.customerGroupId,
                            operationId = (int)Operations.CustomerGroupCreation,
                            BranchId = custGroupModel.userBranchId
                        };
                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new Exception(ex.Message);
                    }
                }
            }
            else
            {
                throw new Exception("Approval route have not been defined for this operation");
            }

            return output;
        }


        public bool DeleteCustomerGroup(int groupId, UserInfo user)
        {
            var group = context.tbl_Customer_Group.Find(groupId);
            group.Deleted = true;
            group.DeletedBy = (int)user.createdBy;
            group.DateTimeDeleted = genSetup.GetApplicaionDate();
            // Audit Section ---------------------------
            var entity = context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == groupId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Customer Group: { entity.GroupName } with Code: { entity.GroupCode } ( { entity.GroupName })",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerGroupViewModel> GetCustomerGroup()
        {
            var customerGroup = from a in context.tbl_Customer_Group
                                where a.Deleted == false
                                select new CustomerGroupViewModel
                                {
                                    groupCode = a.GroupCode,
                                    groupName = a.GroupName,
                                    groupDescription = a.GroupDescription,
                                    customerGroupId = a.CustomerGroupId,
                                    dateTimeCreated = a.DateTimeCreated,
                                    createdBy = a.CreatedBy
                                };
            return customerGroup;
        }

        public CustomerGroupViewModel GetCustomerGroupByCustomerId(int customerGroupId)
        {
            var customerGroup = from a in context.tbl_Customer_Group
                                where a.CustomerGroupId == customerGroupId && a.Deleted == false
                                select new CustomerGroupViewModel
                                {
                                    groupCode = a.GroupCode,
                                    groupName = a.GroupName,
                                    groupDescription = a.GroupDescription,
                                    customerGroupId = a.CustomerGroupId,
                                    dateTimeCreated = a.DateTimeCreated,
                                    createdBy = a.CreatedBy
                                };
            return customerGroup.FirstOrDefault();
        }

        public bool UpdateCustomerGroup(int customerGroupId, CustomerGroupViewModel entity)
        {
            var group = this.context.tbl_Customer_Group.Find(customerGroupId);
            if (group == null) return false;
            group.GroupCode = entity.groupCode;
            group.GroupName = entity.groupName;
            group.GroupDescription = entity.groupDescription;
            group.LastUpdatedBy = (int)entity.createdBy;
            group.DateTimeUpdated = genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupForApproval(int customerGroupId, CustomerGroupViewModel entity)
        {
            if (entity == null)
                return false;

            var existStingTempGroup = context.tbl_Temp_Customer_Group.Where(x => x.GroupCode.ToLower() ==
            entity.groupCode.ToLower() && x.IsCurrent == true &&
            x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);

            if (existStingTempGroup.Any())
            {
                foreach (var item in existStingTempGroup)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetGroup = this.context.tbl_Customer_Group.Find(customerGroupId);

            var unApprovedCustomerGroupEdit = context.tbl_Temp_Customer_Group.Where(x => x.IsCurrent == true
            && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Customer_Group tempCustomerGroup;

            if (unApprovedCustomerGroupEdit.Any())
            {
                throw new Exception("Customer group is already undergoing approval");
            }
            else
            {
                tempCustomerGroup = new tbl_Temp_Customer_Group()
                {
                    GroupCode = targetGroup.GroupCode,
                    GroupName = entity.groupName,
                    GroupDescription = entity.groupDescription,
                    CreatedBy = entity.createdBy,
                    DateTimeCreated = genSetup.GetApplicaionDate(),
                    ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                    IsCurrent = true,
                };

                context.tbl_Temp_Customer_Group.Add(tempCustomerGroup);

            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now,
                TargetId = customerGroupId
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section ------------------------------- 

            var output = this.SaveAll();

            var approvalEntity = new ApprovalViewModel
            {
                staffId = entity.createdBy,
                companyId = entity.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempCustomerGroup.CustomerGroupId,
                operationId = (int)Operations.CustomerGroupCreation,
                BranchId = entity.userBranchId
            };
            var response = workFlow.LogForApproval(approvalEntity);

            return output;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)Operations.CustomerGroupCreation;

            var response = workFlow.GoForApproval(entity);

            if (response.Result.Item1)
            {
                return ApproveCustomerGroup(entity.targetId, response.Result.Item2.approvalStatusId, entity);
            }
            else
            {
                return false;
            }

        }

        private bool ApproveCustomerGroup(int customerGroupId, short approvalStatusId, UserInfo user)
        {
            var customerGroupModel = context.tbl_Temp_Customer_Group.Find(customerGroupId);
            var customerGroupToUpdate = context.tbl_Customer_Group.Where(x => x.GroupCode == customerGroupModel.GroupCode);
            var existingCustomerGroup = customerGroupToUpdate.FirstOrDefault();

            //Update existing customer group with tempCustomerGroup record
            if (customerGroupToUpdate.Any())
            {
                existingCustomerGroup.GroupCode = customerGroupModel.GroupCode;
                existingCustomerGroup.GroupName = customerGroupModel.GroupName;
                existingCustomerGroup.GroupDescription = customerGroupModel.GroupDescription;
                existingCustomerGroup.CreatedBy = customerGroupModel.CreatedBy;
                existingCustomerGroup.DateTimeUpdated = DateTime.Now;
            }
            else //Insert a new customer group record into the real customer group table
            {
                var customerGroup = new tbl_Customer_Group()
                {
                    GroupCode = customerGroupModel.GroupCode,
                    GroupName = customerGroupModel.GroupName,
                    GroupDescription = customerGroupModel.GroupDescription,
                    CreatedBy = customerGroupModel.CreatedBy,
                    DateTimeCreated = DateTime.Now
                };
                context.tbl_Customer_Group.Add(customerGroup);

            }

            customerGroupModel.IsCurrent = false;
            customerGroupModel.ApprovalStatusId = approvalStatusId;
            customerGroupModel.DateTimeUpdated = DateTime.Now;


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Customer Group '{customerGroupModel.GroupName}' with group code'{customerGroupModel.GroupCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        #endregion

        #region tbl_Customer Group Mapping
        public bool AddCustomerGroupMapping(CustomerGroupMappingViewModel entity)
        {
            var groupMap = new tbl_Customer_Group_Mapping
            {
                CustomerId = entity.customerId,
                CustomerGroupId = entity.customerGroupId,
                RelationshipTypeId = entity.relationshipTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = DateTime.Now
            };

            context.tbl_Customer_Group_Mapping.Add(groupMap);

            // Audit Section ---------------------------
            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == groupMap.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();

            var groupName = this.context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == entity.customerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer Group Mapping to customer: { customer } with code: {entity.customerCode } to group  ( { groupName } ) ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddTempCustomerGroupMapping(CustomerGroupMappingViewModel model)
        {
            throw new Exception("On the way");
        }


        ///TODO: Implement a more efficient method 
        public bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups)
        {
            if (customerGroups.Count <= 0)
                return false;

            foreach (CustomerGroupMappingViewModel item in customerGroups)
            {
                AddCustomerGroupMapping(item);
            }

            return true;
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = from a in context.tbl_Customer_Group_Mapping
                                       where a.Deleted == false
                                       select new CustomerGroupMappingViewModel
                                       {

                                           customerGroupMappingId = a.CustomerGroupMappingId,
                                           customerGroupId = a.CustomerGroupId,
                                           relationshipTypeId = a.RelationshipTypeId,
                                           createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping;
        }

        public CustomerGroupMappingViewModel GetCustomerGroupMappingByGroupMapId(int groupMapId)
        {
            var customerGroupMapping = from a in context.tbl_Customer_Group_Mapping
                                       where a.CustomerGroupMappingId == groupMapId && a.Deleted == false
                                       select new CustomerGroupMappingViewModel
                                       {

                                           customerGroupMappingId = a.CustomerGroupMappingId,
                                           customerGroupId = a.CustomerGroupId,
                                           relationshipTypeId = a.RelationshipTypeId,
                                           createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping.SingleOrDefault();

        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMappingByGroupId(int customerGroupId)
        {
            var customerGroupMapping = from a in context.tbl_Customer_Group_Mapping
                                       where a.CustomerGroupId == customerGroupId && a.Deleted == false
                                       select new CustomerGroupMappingViewModel
                                       {
                                           customerGroupMappingId = a.CustomerGroupMappingId,
                                           customerGroupId = a.CustomerGroupId,
                                           relationshipTypeId = a.RelationshipTypeId,
                                           relationshipTypeName = a.tbl_Customer_Group_RelationshipType.RelationshipTypeName,
                                           createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           customerCode = a.tbl_Customer.CustomerCode,
                                           customerName = a.tbl_Customer.LastName + " " + a.tbl_Customer.FirstName,
                                           customerType = a.tbl_Customer.tbl_Customer_Type.Name,
                                           dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping;
        }

        public bool DeleteCustomerGroupMaping(int groupMapId, UserInfo user)
        {
            var groupMap = context.tbl_Customer_Group_Mapping.Find(groupMapId);

            groupMap.Deleted = true;
            groupMap.DeletedBy = (int)user.createdBy;
            groupMap.DateTimeDeleted = genSetup.GetApplicaionDate();

            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == groupMap.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();

            var customerGroupName = context.tbl_Customer_Group.Find(groupMap.CustomerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupMappingDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Customer Group Mapping for customer: { customer} with Code: {groupMap.tbl_Customer.CustomerCode} to group({customerGroupName })",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            context.tbl_Audit.Add(audit);

            return context.SaveChanges() != 0;

        }

        public bool UpdateCustomerGroupMapping(int groupMapId, CustomerGroupMappingViewModel entity)
        {
            var groupMap = context.tbl_Customer_Group_Mapping.Find(groupMapId);
            if (groupMap == null) return false;

            groupMap.CustomerGroupMappingId = groupMapId;
            groupMap.LastUpdatedBy = (int)entity.createdBy;
            groupMap.DateTimeUpdated = genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == groupMap.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();
            var groupName = this.context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == groupMap.CustomerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupMappingUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer Group Mapping for customer: { customer }  with code:  { groupMap.tbl_Customer.CustomerCode } to group ( {groupName } ) ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupMappingForApproval(int groupMapId, CustomerGroupMappingViewModel model)
        {
            throw new Exception("Not implemented"); 
        }

        public bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user)
        {
            var groupMap = context.tbl_Customer_Group_Mapping.Find(groupMapId);

            groupMap.Deleted = true;
            groupMap.DeletedBy = (int)user.createdBy;
            groupMap.DateTimeDeleted = genSetup.GetApplicaionDate();

            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == groupMap.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();

            var customerGroupName = context.tbl_Customer_Group.Find(groupMap.CustomerGroupId).GroupName;
            var groupName = this.context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == groupMap.CustomerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupMappingDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Customer Group Mapping for customer: { customer} with Code: {groupMap.tbl_Customer.CustomerCode} to group({customerGroupName })",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public IEnumerable<LookupViewModel> GetCustomerGroupRelationshipTypes()
        {
            return from a in context.tbl_Customer_Group_RelationshipType
                   select new LookupViewModel
                   {
                       lookupId = a.RelationshipTypeId,
                       lookupName = a.RelationshipTypeName
                   };
        }
        #endregion
    }

}
