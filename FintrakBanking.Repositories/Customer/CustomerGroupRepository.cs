
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
using FintrakBanking.Interfaces.Setups.Approval;

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
        private IApprovalLevelStaffRepository level;


        public CustomerGroupRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IWorkFlowRepository _workFlow,
                                        IApprovalLevelStaffRepository _level)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            workFlow = _workFlow;
            level = _level;
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
                DateTimeCreated = genSetup.GetApplicationDate()
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
                ApplicationDate = genSetup.GetApplicationDate(),
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
                DateTimeCreated = genSetup.GetApplicationDate(),
                ApprovalStatusId = (short)ApprovalStatusEnum.Pending,
                CompanyId = custGroupModel.companyId,
                IsCurrent = true
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
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            //end of Audit section -------------------------------


            if (workFlow.CheckRouteForOperation((int)Operations.CustomerGroupCreation, custGroupModel.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.tbl_Temp_Customer_Group.Add(tempGroup);
                        auditTrail.AddAuditTrail(audit);
                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = custGroupModel.createdBy,
                            companyId = custGroupModel.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = tempGroup.CustomerGroupId,
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
            group.DateTimeDeleted = genSetup.GetApplicationDate();
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
                ApplicationDate = genSetup.GetApplicationDate(),
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
            group.DateTimeUpdated = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
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
            && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && x.GroupCode.ToLower() == targetGroup.GroupCode.ToLower());

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
                    DateTimeCreated = genSetup.GetApplicationDate(),
                    CompanyId = entity.companyId,
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
                ApplicationDate = genSetup.GetApplicationDate(),
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
                    DateTimeCreated = genSetup.GetApplicationDate()
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
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public IEnumerable<CustomerGroupViewModel> GetCustomerGroupsAwaitingApprovals(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)Operations.CustomerGroupCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from c in context.tbl_Temp_Customer_Group
                    join coy in context.tbl_Company on c.CompanyId equals coy.CompanyId
                    join atrail in context.tbl_Approval_Trail on c.CustomerGroupId equals atrail.TargetId
                    where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && c.IsCurrent == true
                          && atrail.OperationId == (int)Operations.CustomerGroupCreation && atrail.ToApprovalLevelId == staffApprovalLevelId
                    select new CustomerGroupViewModel()
                    {
                        companyId = c.CompanyId,
                        companyName = c.tbl_Company.Name,
                        customerGroupId = c.CustomerGroupId,
                        groupName = c.GroupName,
                        groupCode = c.GroupCode,
                        groupDescription = c.GroupDescription,
                        operationId = atrail.OperationId,
                    });
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
                ////CreatedBy = entity.createdBy,
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
                //StaffId = entity.createdBy,
                //BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer Group Mapping to customer: { customer } with code: {entity.customerCode } to group  ( { groupName } ) ",
                //IPAddress = entity.userIPAddress,
                //Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddTempCustomerGroupMapping(CustomerGroupMappingViewModel model)
        {
            bool output = false;

            var groupMap = new tbl_Temp_Customer_Group_Mapping
            {
                CustomerId = model.customerId,
                CustomerGroupId = model.customerGroupId,
                RelationshipTypeId = model.relationshipTypeId,
                CreatedBy = model.createdBy,
                ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                CompanyId = model.companyId,
                IsCurrent = true,
                DateTimeCreated = DateTime.Now
            };

            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == groupMap.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();

            var groupName = this.context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == model.customerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Customer Group Mapping to customer: { customer } with code: {model.customerCode } to group  ( { groupName } ) ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };


            if (workFlow.CheckRouteForOperation((int)Operations.CustomerGroupCreation, model.companyId))
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        auditTrail.AddAuditTrail(audit);
                        context.tbl_Temp_Customer_Group_Mapping.Add(groupMap);
                        output = this.SaveAll();

                        var entity = new ApprovalViewModel
                        {
                            staffId = model.createdBy,
                            companyId = model.companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = groupMap.CustomerGroupMappingId,
                            operationId = (int)Operations.CustomerGroupCreation,
                            BranchId = model.userBranchId
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
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           //dateTimeCreated = a.DateTimeCreated
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
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           //dateTimeCreated = a.DateTimeCreated
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
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CustomerId,
                                           customerCode = a.tbl_Customer.CustomerCode,
                                           customerName = a.tbl_Customer.LastName + " " + a.tbl_Customer.FirstName,
                                           customerType = a.tbl_Customer.tbl_Customer_Type.Name,
                                           //dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping;
        }

        public bool DeleteCustomerGroupMaping(int groupMapId, UserInfo user)
        {
            var groupMap = context.tbl_Customer_Group_Mapping.Find(groupMapId);

            groupMap.Deleted = true;
            groupMap.DeletedBy = (int)user.createdBy;
            groupMap.DateTimeDeleted = genSetup.GetApplicationDate();

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
                ApplicationDate = genSetup.GetApplicationDate(),
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
            //groupMap.LastUpdatedBy = (int)entity.createdBy;
            groupMap.DateTimeUpdated = genSetup.GetApplicationDate();

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
                ////StaffId = entity.createdBy,
                //BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer Group Mapping for customer: { customer }  with code:  { groupMap.tbl_Customer.CustomerCode } to group ( {groupName } ) ",
                //IPAddress = entity.userIPAddress,
                //Url = entity./*applicationUrl*/,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupMappingForApproval(int groupMapId, CustomerGroupMappingViewModel model)
        {
            if (model == null)
                return false;

            var existStingTempGroupMapping = context.tbl_Temp_Customer_Group_Mapping.Where(x => x.CustomerGroupId ==
            model.customerGroupId && x.IsCurrent == true &&
            x.ApprovalStatusId == (int)ApprovalStatusEnum.Approved);

            if (existStingTempGroupMapping.Any())
            {
                foreach (var item in existStingTempGroupMapping)
                {
                    item.IsCurrent = false;
                    item.DateTimeUpdated = DateTime.Now;
                }
            }

            var targetGroupMapping = this.context.tbl_Customer_Group_Mapping.Find(groupMapId);

            var unApprovedCustomerGroupMapEdit = context.tbl_Temp_Customer_Group_Mapping.Where(x => x.IsCurrent == true
            && x.ApprovalStatusId == (int)ApprovalStatusEnum.Pending);

            tbl_Temp_Customer_Group_Mapping tempCustomerGroupMap;

            if (unApprovedCustomerGroupMapEdit.Any())
            {
                throw new Exception("Customer group map is already undergoing approval");
            }
            else
            {
                tempCustomerGroupMap = new tbl_Temp_Customer_Group_Mapping()
                {
                    CustomerId = model.customerId,
                    CustomerGroupId = targetGroupMapping.CustomerGroupId,
                    RelationshipTypeId = model.relationshipTypeId,
                    CreatedBy = model.createdBy,
                    DateTimeCreated = genSetup.GetApplicationDate(),
                    CompanyId = model.companyId,
                    ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                    IsCurrent = true,
                };

                context.tbl_Temp_Customer_Group_Mapping.Add(tempCustomerGroupMap);

            }

            // Audit Section ---------------------------
            var customer = this.context.tbl_Customer.Where(x => x.CustomerId == targetGroupMapping.CustomerId).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FirstName + " " + x.LastName
                                                    }).FirstOrDefault();
            var groupName = this.context.tbl_Customer_Group.FirstOrDefault(x => x.CustomerGroupId == targetGroupMapping.CustomerGroupId).GroupName;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupMappingUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Customer Group Mapping for customer: { customer }  with code:  { targetGroupMapping.tbl_Customer.CustomerCode } to group ( {groupName } ) ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var output = this.SaveAll();

            var approvalEntity = new ApprovalViewModel
            {
                staffId = model.createdBy,
                companyId = model.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempCustomerGroupMap.CustomerGroupMappingId,
                operationId = (int)Operations.CustomerGroupCreation,
                BranchId = model.userBranchId
            };
            var response = workFlow.LogForApproval(approvalEntity);

            return output;

        }

        public bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user)
        {
            var groupMap = context.tbl_Customer_Group_Mapping.Find(groupMapId);

            groupMap.Deleted = true;
            groupMap.DeletedBy = (int)user.createdBy;
            groupMap.DateTimeDeleted = genSetup.GetApplicationDate();

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
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        private bool ApproveCustomerGroupMapping(int customerGroupMapId, short approvalStatusId, UserInfo user)
        {
            var customerGroupMapModel = context.tbl_Temp_Customer_Group_Mapping.Find(customerGroupMapId);
            var customerGroupMapToUpdate = context.tbl_Customer_Group_Mapping.Where(x => x.CustomerGroupMappingId == customerGroupMapId);
            var existingCustomerGroupMap = customerGroupMapToUpdate.FirstOrDefault();

            //Update existing customer group map with tempCustomerGroupMap record
            if (customerGroupMapToUpdate.Any())
            {
                existingCustomerGroupMap.CustomerId = customerGroupMapModel.CustomerId;
                existingCustomerGroupMap.CustomerGroupId = customerGroupMapModel.CustomerGroupId;
                existingCustomerGroupMap.RelationshipTypeId = customerGroupMapModel.RelationshipTypeId;
                existingCustomerGroupMap.CreatedBy = customerGroupMapModel.CreatedBy;
                existingCustomerGroupMap.DateTimeUpdated = DateTime.Now;
            }
            else //Insert a new customer group map record into the real customer group map table
            {
                var customerGroupMap = new tbl_Customer_Group_Mapping()
                {
                    CustomerId = customerGroupMapModel.CustomerId,
                    CustomerGroupId = customerGroupMapModel.CustomerGroupId,
                    RelationshipTypeId = customerGroupMapModel.RelationshipTypeId,
                    CreatedBy = customerGroupMapModel.CreatedBy,
                    DateTimeCreated = DateTime.Now
                };
                context.tbl_Customer_Group_Mapping.Add(customerGroupMap);

            }

            customerGroupMapModel.IsCurrent = false;
            customerGroupMapModel.ApprovalStatusId = approvalStatusId;
            customerGroupMapModel.DateTimeUpdated = DateTime.Now;


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerGroupApproved,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Customer Group Mapping '{customerGroupMapModel.CustomerGroupMappingId}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapsAwaitingApprovals(int staffId, int companyId)
        {
            var levelResult = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, (int)Operations.CustomerGroupCreation);
            int staffApprovalLevelId = 0;

            if (levelResult != null) staffApprovalLevelId = levelResult.approvalLevelId;

            return (from c in context.tbl_Temp_Customer_Group_Mapping
                    join coy in context.tbl_Company on c.CompanyId equals coy.CompanyId
                    join atrail in context.tbl_Approval_Trail on c.CustomerGroupId equals atrail.TargetId
                    where atrail.ApprovalStatusId == (int)ApprovalStatusEnum.Pending && c.IsCurrent == true
                          && atrail.OperationId == (int)Operations.CustomerGroupCreation && atrail.ToApprovalLevelId == staffApprovalLevelId
                    select new CustomerGroupMappingViewModel()
                    {
                        companyId = c.CompanyId,
                        customerGroupId = c.CustomerGroupId,
                        customerGroupMappingId = c.CustomerGroupMappingId,
                        customerCode = c.tbl_Customer.CustomerCode,
                        customerId = c.CustomerId,
                        relationshipTypeId = c.RelationshipTypeId,
                        relationshipTypeName = c.tbl_Customer_Group_RelationshipType.RelationshipTypeName,
                        customerName = c.tbl_Customer.FirstName + " " + c.tbl_Customer.LastName,
                    });
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
