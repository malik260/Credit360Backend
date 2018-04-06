using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Credit;

namespace FintrakBanking.Repositories.Customer
{
    public class CustomerGroupRepository : ICustomerGroupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;
        private ICreditLimitValidationsRepository creditLimitRepo;
        private ICustomerCreditBureauRepository creditBureau;
       

        public CustomerGroupRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail,
                                        IWorkflow _workFlow,
                                        IApprovalLevelStaffRepository _level,
            ICreditLimitValidationsRepository _creditLimitRepo,
            ICustomerCreditBureauRepository _creditBureau)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
            workFlow = _workFlow;
            level = _level;
            creditLimitRepo = _creditLimitRepo;
            creditBureau = _creditBureau;
        }

        private bool SaveAll()
        {
            return context.SaveChanges() > 0;
        }

        #region customer KYC

        public IEnumerable<KYCItemViewModel> GetKYCItems(int companyId)
        {
           var kycItem = (from d in context.TBL_KYC_ITEM where d.TBL_PRODUCT.COMPANYID == companyId select new KYCItemViewModel
            {
                createdBy = (int)d.CREATEDBY,
                productId = (short)d.PRODUCTID,
                kYCItemId = d.KYCITEMID,
                item = d.ITEM,
                isMandatory = d.ISMANDATORY,
                dateTimeCreated = (DateTime)d.DATETIMECREATED,
                displayOrder = d.DISPLAYORDER,
                productName = d.TBL_PRODUCT.PRODUCTNAME
            }).ToList();
            return kycItem;
        }

        public bool AddKycItem(KYCItemViewModel entity)
        {
            var data = new TBL_KYC_ITEM
            {
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                DISPLAYORDER = entity.displayOrder,
                ITEM = entity.item,
                ISMANDATORY = entity.isMandatory,
                KYCITEMID = entity.kYCItemId,
                PRODUCTID = entity.productId
            };
            context.TBL_KYC_ITEM.Add(data);

           // Audit Section ---------------------------
           var audit = new TBL_AUDIT
           {
               AUDITTYPEID = (short)AuditTypeEnum.KYCItemAdded,
               STAFFID = entity.createdBy,
               BRANCHID = (short)entity.userBranchId,
               DETAIL = $"Added KYC Item: { entity.item  } ",
               IPADDRESS = entity.userIPAddress,
               URL = entity.applicationUrl,
               APPLICATIONDATE = genSetup.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now
           };
            this.auditTrail.AddAuditTrail(audit);

           // end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        public bool UpdatedKycItem(int kYCItemId, KYCItemViewModel entity)
        {
            var data = context.TBL_KYC_ITEM.Where(c => c.KYCITEMID == kYCItemId).SingleOrDefault();
           
            data.DATETIMEUPDATED = DateTime.Now;
            data.DISPLAYORDER = entity.displayOrder;
            data.ITEM = entity.item;
            data.LASTUPDATEDBY = entity.createdBy;
            data.PRODUCTID = entity.productId;
            data.ISMANDATORY = entity.isMandatory;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.KYCItemUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated KYC Item: { entity.item  } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        #endregion customer KYC

        #region TBL_CUSTOMER - Group

        public bool AddCustomerGroup(CustomerGroupViewModel entity)
        {
            var group = new TBL_CUSTOMER_GROUP
            {
                GROUPCODE = entity.groupCode,
                GROUPNAME = entity.groupName,
                GROUPDESCRIPTION = entity.groupDescription,
                CREATEDBY = (int)entity.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate()
            };
            context.TBL_CUSTOMER_GROUP.Add(group);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName } )",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool DoesGroupNameExist(string groupName, string groupCode)
        {
            var exist = (from a in context.TBL_CUSTOMER_GROUP where
                       a.GROUPNAME == groupName || a.GROUPCODE == groupCode select a).Any();
            return exist;
        }
            public bool AddTempCustomerGroup(CustomerGroupViewModel custGroupModel)
        {
            bool output = false;

            var tempGroup = new TBL_TEMP_CUSTOMER_GROUP
            {
                GROUPCODE = custGroupModel.groupCode,
                GROUPNAME = custGroupModel.groupName,
                GROUPDESCRIPTION = custGroupModel.groupDescription,
                CREATEDBY = (int)custGroupModel.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                COMPANYID = custGroupModel.companyId,
                ISCURRENT = true
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                STAFFID = custGroupModel.createdBy,
                BRANCHID = (short)custGroupModel.userBranchId,
                DETAIL = $"Added Customer Group: { custGroupModel.groupName } with Code: { custGroupModel.groupCode } ( { custGroupModel.groupName } )",
                IPADDRESS = custGroupModel.userIPAddress,
                URL = custGroupModel.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            //end of Audit section -------------------------------

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_TEMP_CUSTOMER_GROUP.Add(tempGroup);
                    auditTrail.AddAuditTrail(audit);
                    output = this.SaveAll();

                    var entity = new ApprovalViewModel
                    {
                        staffId = custGroupModel.createdBy,
                        companyId = custGroupModel.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = tempGroup.CUSTOMERGROUPID,
                        operationId = (int)OperationsEnum.CustomerGroupCreation,
                        BranchId = custGroupModel.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        public bool DeleteCustomerGroup(int groupId, UserInfo user)
        {
            var group = context.TBL_CUSTOMER_GROUP.Find(groupId);
            group.DELETED = true;
            group.DELETEDBY = (int)user.createdBy;
            group.DATETIMEDELETED = genSetup.GetApplicationDate();
            // Audit Section ---------------------------
            var entity = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == groupId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Customer Group: { entity.GROUPNAME } with Code: { entity.GROUPCODE } ( { entity.GROUPNAME })",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        private IQueryable<CustomerGroupViewModel> GetAllCustomerGroups()
        {
            var data = (from a in context.TBL_CUSTOMER_GROUP
                        where a.DELETED == false
                        select new CustomerGroupViewModel
                        {
                            groupCode = a.GROUPCODE,
                            groupName = a.GROUPNAME,
                            groupDescription = a.GROUPDESCRIPTION,
                            customerGroupId = a.CUSTOMERGROUPID,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        });
            return data;
        }

        public IEnumerable<CustomerGroupViewModel> GetCustomerGroup()
        {
            var customerGroup = GetAllCustomerGroups();

            return customerGroup;
        }

        public CustomerGroupViewModel GetCustomerGroupByCustomerId(int customerGroupId)
        {
            var customerGroup = GetCustomerGroup().Where(x => x.customerGroupId == customerGroupId);

            return customerGroup.FirstOrDefault();
        }

        public bool UpdateCustomerGroup(int customerGroupId, CustomerGroupViewModel entity)
        {
            var group = this.context.TBL_CUSTOMER_GROUP.Find(customerGroupId);
            if (group == null) return false;
            group.GROUPCODE = entity.groupCode;
            group.GROUPNAME = entity.groupName;
            group.GROUPDESCRIPTION = entity.groupDescription;
            group.LASTUPDATEDBY = (int)entity.createdBy;
            group.DATETIMEUPDATED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupForApproval(int customerGroupId, CustomerGroupViewModel entity)
        {
            if (entity == null)
                return false;

            var existingTempGroup = context.TBL_TEMP_CUSTOMER_GROUP.FirstOrDefault(x => x.GROUPCODE.ToLower() ==
            entity.groupCode.ToLower() && x.ISCURRENT == true &&
            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            var unApprovedCustomerGroupEdit = context.TBL_TEMP_CUSTOMER_GROUP.Where(x => x.ISCURRENT == true
                                                                                         && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && x.GROUPCODE.ToLower() == entity.groupCode.ToLower());
            if (unApprovedCustomerGroupEdit.Any())
            {
                throw new Exception("Customer group is already undergoing approval");
            }

            TBL_TEMP_CUSTOMER_GROUP tempCustomerGroup = new TBL_TEMP_CUSTOMER_GROUP();

            if (existingTempGroup != null)
            {
                //foreach (var item in existStingTempGroup)
                //{
                //    item.IsCurrent = false;
                //    item.DateTimeUpdated = DateTime.Now;
                //}
                var tempGroupToUpdate = existingTempGroup;

                tempGroupToUpdate.GROUPCODE = entity.groupCode;
                tempGroupToUpdate.GROUPNAME = entity.groupName;
                tempGroupToUpdate.GROUPDESCRIPTION = entity.groupDescription;
                tempGroupToUpdate.CREATEDBY = entity.createdBy;
                tempGroupToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempGroupToUpdate.COMPANYID = entity.companyId;
                tempGroupToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempGroupToUpdate.ISCURRENT = true;
            }
            else
            {
                var targetGroup = this.context.TBL_CUSTOMER_GROUP.Find(customerGroupId);

                tempCustomerGroup = new TBL_TEMP_CUSTOMER_GROUP()
                {
                    GROUPCODE = targetGroup?.GROUPCODE,
                    GROUPNAME = entity.groupName,
                    GROUPDESCRIPTION = entity.groupDescription,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    COMPANYID = entity.companyId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                };

                context.TBL_TEMP_CUSTOMER_GROUP.Add(tempCustomerGroup);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = customerGroupId
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            var output = this.SaveAll();

            var targetGroupId = existingTempGroup?.CUSTOMERGROUPID ?? tempCustomerGroup.CUSTOMERGROUPID;

            var approvalEntity = new ApprovalViewModel
            {
                staffId = entity.createdBy,
                companyId = entity.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = targetGroupId,
                operationId = (int)OperationsEnum.CustomerGroupCreation,
                BranchId = entity.userBranchId,
                externalInitialization = true
            };

            var response = workFlow.LogForApproval(approvalEntity);

            if (response)
            {
                return output;
            }

            return false;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.CustomerGroupCreation;

            entity.externalInitialization = false;

            workFlow.LogForApproval(entity);

            if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                return ApproveCustomerGroup(entity.targetId, (short)workFlow.StatusId, entity);
            }

            return false;
        }

        private bool ApproveCustomerGroup(int customerGroupId, short approvalStatusId, UserInfo user)
        {
            var customerGroupModel = context.TBL_TEMP_CUSTOMER_GROUP.Find(customerGroupId);
            var customerGroupToUpdate = context.TBL_CUSTOMER_GROUP.Where(x => x.GROUPCODE == customerGroupModel.GROUPCODE);
            var existingCustomerGroup = customerGroupToUpdate.FirstOrDefault();

            //Update existing customer group with tempCustomerGroup record
            if (customerGroupToUpdate.Any())
            {
                existingCustomerGroup.GROUPCODE = customerGroupModel?.GROUPCODE;
                existingCustomerGroup.GROUPNAME = customerGroupModel?.GROUPNAME;
                existingCustomerGroup.GROUPDESCRIPTION = customerGroupModel?.GROUPDESCRIPTION;
                existingCustomerGroup.CREATEDBY = customerGroupModel.CREATEDBY;
                existingCustomerGroup.DATETIMEUPDATED = DateTime.Now;
            }
            else //Insert a new customer group record into the real customer group table
            {
                var customerGroup = new TBL_CUSTOMER_GROUP()
                {
                    GROUPCODE = customerGroupModel.GROUPCODE,
                    GROUPNAME = customerGroupModel.GROUPNAME,
                    GROUPDESCRIPTION = customerGroupModel.GROUPDESCRIPTION,
                    CREATEDBY = customerGroupModel.CREATEDBY,
                    DATETIMECREATED = genSetup.GetApplicationDate()
                };
                context.TBL_CUSTOMER_GROUP.Add(customerGroup);
            }

            customerGroupModel.ISCURRENT = false;
            customerGroupModel.APPROVALSTATUSID = approvalStatusId;
            customerGroupModel.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Customer Group '{customerGroupModel.GROUPNAME}' with group code'{customerGroupModel.GROUPCODE}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public IEnumerable<CustomerGroupViewModel> GetCustomerGroupsAwaitingApprovals(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CustomerGroupCreation).ToList();

            return (from c in context.TBL_TEMP_CUSTOMER_GROUP
                    join coy in context.TBL_COMPANY on c.COMPANYID equals coy.COMPANYID
                    join atrail in context.TBL_APPROVAL_TRAIL on c.CUSTOMERGROUPID equals atrail.TARGETID
                    where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && c.ISCURRENT == true
                          && atrail.OPERATIONID == (int)OperationsEnum.CustomerGroupCreation && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                    select new CustomerGroupViewModel()
                    {
                        companyId = c.COMPANYID,
                        companyName = c.TBL_COMPANY.NAME,
                        customerGroupId = c.CUSTOMERGROUPID,
                        groupName = c.GROUPNAME,
                        groupCode = c.GROUPCODE,
                        groupDescription = c.GROUPDESCRIPTION,
                        operationId = atrail.OPERATIONID,
                    });
        }

        #endregion TBL_CUSTOMER - Group

        #region TBL_CUSTOMER Group Mapping

        public bool AddCustomerGroupMapping(CustomerGroupMappingViewModel entity)
        {
            var groupMap = new TBL_CUSTOMER_GROUP_MAPPING
            {
                CUSTOMERID = entity.customerId,
                CUSTOMERGROUPID = entity.customerGroupId,
                RELATIONSHIPTYPEID = entity.relationshipTypeId,
                ////CreatedBy = entity.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            context.TBL_CUSTOMER_GROUP_MAPPING.Add(groupMap);

            // Audit Section ---------------------------
            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();

            var groupName = this.context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == entity.customerGroupId).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                //StaffId = entity.createdBy,
                //BranchId = (short)entity.userBranchId,
                DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {entity.customerCode } to group  ( { groupName } ) ",
                //IPAddress = entity.userIPAddress,
                //Url = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddTempCustomerGroupMapping(CustomerGroupMappingViewModel model)
        {
            bool output = false;

            var groupMap = new TBL_TEMP_CUSTOMER_GROUP_MAPPNG
            {
                CUSTOMERID = model.customerId,
                CUSTOMERGROUPID = model.customerGroupId,
                RELATIONSHIPTYPEID = model.relationshipTypeId,
                CREATEDBY = model.createdBy,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                COMPANYID = model.companyId,
                ISCURRENT = true,
                DATETIMECREATED = DateTime.Now
            };

            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();

            var groupName = this.context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == model.customerGroupId).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {model.customerCode } to group  ( { groupName } ) ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    auditTrail.AddAuditTrail(audit);
                    context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Add(groupMap);
                    output = this.SaveAll();

                    var entity = new ApprovalViewModel
                    {
                        staffId = model.createdBy,
                        companyId = model.companyId,
                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
                        targetId = groupMap.CUSTOMERGROUPMAPPINGID,
                        operationId = (int)OperationsEnum.CustomerGroupCreation,
                        BranchId = model.userBranchId,
                        externalInitialization = true
                    };
                    var response = workFlow.LogForApproval(entity);

                    if (response)
                    {
                        trans.Commit();
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new Exception(ex.Message);
                }
            }
        }

        ///TODO: Implement a more efficient method
        public bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups, int createdBy, short userBranchId)
        {
            if (customerGroups.Count <= 0)
                return false;
            List<TBL_CUSTOMER_GROUP_MAPPING> listOfMappedGroup = new List<TBL_CUSTOMER_GROUP_MAPPING>();
            foreach (CustomerGroupMappingViewModel item in customerGroups)
            {
                var group = this.context.TBL_CUSTOMER_GROUP_MAPPING.FirstOrDefault(x => x.CUSTOMERID == item.customerId && x.CUSTOMERGROUPID == item.customerGroupId);
                if (group == null)
                {
                    var groupMap = new TBL_CUSTOMER_GROUP_MAPPING
                    {
                        CUSTOMERID = item.customerId,
                        CUSTOMERGROUPID = item.customerGroupId,
                        RELATIONSHIPTYPEID = item.relationshipTypeId,
                        CREATEDBY = createdBy,
                        DELETED = false,
                        DATETIMECREATED = DateTime.Now
                    };
                    listOfMappedGroup.Add(groupMap);
                  
                    // Audit Section ---------------------------
                    var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                            .Select(x => new
                                                            {
                                                                customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                            }).FirstOrDefault();
                    var groupName = (from gr in this.context.TBL_CUSTOMER_GROUP where gr.CUSTOMERGROUPID == item.customerGroupId select gr.GROUPNAME).FirstOrDefault();

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                        STAFFID = createdBy,
                        BRANCHID = userBranchId,
                        DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {item.customerCode } to group  ( { groupName } ) ",
                        //IPAddress = entity.userIPAddress,
                        //Url = entity.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -----------------------
                  
                }
            }
            context.TBL_CUSTOMER_GROUP_MAPPING.AddRange(listOfMappedGroup);
            return context.SaveChanges() != 0;
          
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                       where a.DELETED == false
                                       select new CustomerGroupMappingViewModel
                                       {
                                           customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                           customerGroupId = a.CUSTOMERGROUPID,
                                           relationshipTypeId = a.RELATIONSHIPTYPEID,
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CUSTOMERID,
                                           //dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping;
        }

        public CustomerGroupMappingViewModel GetCustomerGroupMappingByGroupMapId(int groupMapId)
        {
            var customerGroupMapping = from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                       where a.CUSTOMERGROUPMAPPINGID == groupMapId && a.DELETED == false
                                       select new CustomerGroupMappingViewModel
                                       {
                                           customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                           customerGroupId = a.CUSTOMERGROUPID,
                                           relationshipTypeId = a.RELATIONSHIPTYPEID,
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CUSTOMERID,
                                           //dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping.FirstOrDefault();
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMappingByGroupId(int customerGroupId)
        {
            var customerGroupMapping = from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                       where a.CUSTOMERGROUPID == customerGroupId && a.DELETED == false
                                       select new CustomerGroupMappingViewModel
                                       {
                                           customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                           customerGroupId = a.CUSTOMERGROUPID,
                                           relationshipTypeId = a.RELATIONSHIPTYPEID,
                                           relationshipTypeName = a.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                           //createdBy = a.CreatedBy,
                                           customerId = a.CUSTOMERID,
                                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                           customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME,
                                           customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                           //dateTimeCreated = a.DateTimeCreated
                                       };

            return customerGroupMapping;
        }


        public IEnumerable<GroupCustomerMembersViewModel> GetGroupMembersByGroupId(int customerGroupId, int companyId)
        {
            try
            {
                List<GroupCustomerMembersViewModel> lstCustomer = new List<GroupCustomerMembersViewModel>();

                var data = from b in context.TBL_CUSTOMER_GROUP_MAPPING
                                           
                                           where b.CUSTOMERGROUPID == customerGroupId && b.DELETED  == false 
                                           && b.TBL_CUSTOMER.COMPANYID == companyId 
                                           && b.TBL_CUSTOMER.VALIDATED == true
                                           select new GroupCustomerMembersViewModel
                                           {
                                               customerId = b.CUSTOMERID,
                                               customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                               lastName = b.TBL_CUSTOMER.LASTNAME,
                                               firstName = b.TBL_CUSTOMER.FIRSTNAME
                                           };

                foreach (var item in data)
                {
                    if (creditBureau.VerifyCustomerValidCreditBureau(item.customerId))
                    {
                        lstCustomer.Add(item);
                    }
                }

                return lstCustomer;

              

            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        public bool DeleteCustomerGroupMaping(int groupMapId, UserInfo user)
        {
            var groupMap = context.TBL_CUSTOMER_GROUP_MAPPING.Find(groupMapId);

            groupMap.DELETED = true;
            groupMap.DELETEDBY = (int)user.createdBy;
            groupMap.DATETIMEDELETED = genSetup.GetApplicationDate();

            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();

            var customerGroupName = context.TBL_CUSTOMER_GROUP.Find(groupMap.CUSTOMERGROUPID).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Customer Group Mapping for customer: { customer} with Code: {groupMap.TBL_CUSTOMER.CUSTOMERCODE} to group({customerGroupName })",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_AUDIT.Add(audit);

            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupMapping(int groupMapId, CustomerGroupMappingViewModel entity)
        {
            var groupMap = context.TBL_CUSTOMER_GROUP_MAPPING.Find(groupMapId);
            if (groupMap == null) return false;

            groupMap.CUSTOMERGROUPMAPPINGID = groupMapId;
            //groupMap.LastUpdatedBy = (int)entity.createdBy;
            groupMap.DATETIMEUPDATED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();
            var groupName = this.context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == groupMap.CUSTOMERGROUPID).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingUpdated,
                ////StaffId = entity.createdBy,
                //BranchId = (short)entity.userBranchId,
                DETAIL = $"Updated Customer Group Mapping for customer: { customer }  with code:  { groupMap.TBL_CUSTOMER.CUSTOMERCODE } to group ( {groupName } ) ",
                //IPAddress = entity.userIPAddress,
                //Url = entity./*applicationUrl*/,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        public bool UpdateCustomerGroupMappingForApproval(int groupMapId, CustomerGroupMappingViewModel model)
        {
            if (model == null)
                return false;

            var existStingTempGroupMapping = context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Where(x => x.CUSTOMERGROUPID ==
            model.customerGroupId && x.ISCURRENT == true &&
            x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);

            if (existStingTempGroupMapping.Any())
            {
                foreach (var item in existStingTempGroupMapping)
                {
                    item.ISCURRENT = false;
                    item.DATETIMEUPDATED = DateTime.Now;
                }
            }

            var targetGroupMapping = this.context.TBL_CUSTOMER_GROUP_MAPPING.Find(groupMapId);

            var unApprovedCustomerGroupMapEdit = context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Where(x => x.ISCURRENT == true
            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending);

            TBL_TEMP_CUSTOMER_GROUP_MAPPNG tempCustomerGroupMap;

            if (unApprovedCustomerGroupMapEdit.Any())
            {
                throw new Exception("Customer group map is already undergoing approval");
            }
            else
            {
                tempCustomerGroupMap = new TBL_TEMP_CUSTOMER_GROUP_MAPPNG()
                {
                    CUSTOMERID = model.customerId,
                    CUSTOMERGROUPID = targetGroupMapping.CUSTOMERGROUPID,
                    RELATIONSHIPTYPEID = model.relationshipTypeId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    COMPANYID = model.companyId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                };

                context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Add(tempCustomerGroupMap);
            }

            // Audit Section ---------------------------
            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == targetGroupMapping.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();
            var groupName = this.context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == targetGroupMapping.CUSTOMERGROUPID).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Customer Group Mapping for customer: { customer }  with code:  { targetGroupMapping.TBL_CUSTOMER.CUSTOMERCODE } to group ( {groupName } ) ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            var output = this.SaveAll();

            var approvalEntity = new ApprovalViewModel
            {
                staffId = model.createdBy,
                companyId = model.companyId,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                targetId = tempCustomerGroupMap.CUSTOMERGROUPMAPPINGID,
                operationId = (int)OperationsEnum.CustomerGroupCreation,
                BranchId = model.userBranchId
            };
            var response = workFlow.LogForApproval(approvalEntity);

            return output;
        }

        public bool DeleteCustomerGroupMapping(int groupMapId, UserInfo user)
        {
            var groupMap = context.TBL_CUSTOMER_GROUP_MAPPING.Find(groupMapId);

            groupMap.DELETED = true;
            groupMap.DELETEDBY = (int)user.createdBy;
            groupMap.DATETIMEDELETED = genSetup.GetApplicationDate();

            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
                                                    .Select(x => new
                                                    {
                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                    }).FirstOrDefault();

            var customerGroupName = context.TBL_CUSTOMER_GROUP.Find(groupMap.CUSTOMERGROUPID).GROUPNAME;
            var groupName = this.context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == groupMap.CUSTOMERGROUPID).GROUPNAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Customer Group Mapping for customer: { customer} with Code: {groupMap.TBL_CUSTOMER.CUSTOMERCODE} to group({customerGroupName })",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() != 0;
        }

        private bool ApproveCustomerGroupMapping(int customerGroupMapId, short approvalStatusId, UserInfo user)
        {
            var customerGroupMapModel = context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Find(customerGroupMapId);
            var customerGroupMapToUpdate = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPMAPPINGID == customerGroupMapId);
            var existingCustomerGroupMap = customerGroupMapToUpdate.FirstOrDefault();

            //Update existing customer group map with tempCustomerGroupMap record
            if (customerGroupMapToUpdate.Any())
            {
                existingCustomerGroupMap.CUSTOMERID = customerGroupMapModel.CUSTOMERID;
                existingCustomerGroupMap.CUSTOMERGROUPID = customerGroupMapModel.CUSTOMERGROUPID;
                existingCustomerGroupMap.RELATIONSHIPTYPEID = customerGroupMapModel.RELATIONSHIPTYPEID;
                existingCustomerGroupMap.CREATEDBY = customerGroupMapModel.CREATEDBY;
                existingCustomerGroupMap.DATETIMEUPDATED = DateTime.Now;
            }
            else //Insert a new customer group map record into the real customer group map table
            {
                var customerGroupMap = new TBL_CUSTOMER_GROUP_MAPPING()
                {
                    CUSTOMERID = customerGroupMapModel.CUSTOMERID,
                    CUSTOMERGROUPID = customerGroupMapModel.CUSTOMERGROUPID,
                    RELATIONSHIPTYPEID = customerGroupMapModel.RELATIONSHIPTYPEID,
                    CREATEDBY = customerGroupMapModel.CREATEDBY,
                    DATETIMECREATED = DateTime.Now
                };
                context.TBL_CUSTOMER_GROUP_MAPPING.Add(customerGroupMap);
            }

            customerGroupMapModel.ISCURRENT = false;
            customerGroupMapModel.APPROVALSTATUSID = approvalStatusId;
            customerGroupMapModel.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Customer Group Mapping '{customerGroupMapModel.CUSTOMERGROUPMAPPINGID}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapsAwaitingApprovals(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CustomerGroupCreation).ToList();

            return (from c in context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG
                    join coy in context.TBL_COMPANY on c.COMPANYID equals coy.COMPANYID
                    join atrail in context.TBL_APPROVAL_TRAIL on c.CUSTOMERGROUPID equals atrail.TARGETID
                    where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && c.ISCURRENT == true
                          && atrail.OPERATIONID == (int)OperationsEnum.CustomerGroupCreation && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                    select new CustomerGroupMappingViewModel()
                    {
                        companyId = c.COMPANYID,
                        customerGroupId = c.CUSTOMERGROUPID,
                        customerGroupMappingId = c.CUSTOMERGROUPMAPPINGID,
                        customerCode = c.TBL_CUSTOMER.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        relationshipTypeId = c.RELATIONSHIPTYPEID,
                        relationshipTypeName = c.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                        customerName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.LASTNAME,
                    });
        }

        public IEnumerable<LookupViewModel> GetCustomerGroupRelationshipTypes()
        {
            return from a in context.TBL_CUSTOMER_GROUP_RELATN_TYPE
                   select new LookupViewModel
                   {
                       lookupId = a.RELATIONSHIPTYPEID,
                       lookupName = a.RELATIONSHIPTYPENAME
                   };
        }
        public bool AddCustomerGroupRelationshipTypes(LookupViewModel model )
        {
            if (model.lookupId > 0)
            {
                var type = context.TBL_CUSTOMER_GROUP_RELATN_TYPE.FirstOrDefault(x=> x.RELATIONSHIPTYPEID == model.lookupId);
                type.RELATIONSHIPTYPENAME = model.lookupName;
              
            }
            else
            {
                var type = new TBL_CUSTOMER_GROUP_RELATN_TYPE
                {
                    RELATIONSHIPTYPENAME = model.lookupName
                };
                context.TBL_CUSTOMER_GROUP_RELATN_TYPE.Add(type);
            }
            return this.SaveAll();
        }

        private IQueryable<CustomerGroupViewModel> GellAllCustomerGroupMappings()
        {
            var data = (from a in context.TBL_CUSTOMER_GROUP
                        where a.DELETED == false
                        select new CustomerGroupViewModel
                        {
                            customerGroupId = a.CUSTOMERGROUPID,
                            customerGroupName = a.GROUPNAME,
                            customerGroupCode = a.GROUPCODE,
                            customerGroupMappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(s => new CustomerGroupMappingViewModel
                            {
                                customerGroupMappingId = s.CUSTOMERGROUPMAPPINGID,
                                customerGroupId = s.CUSTOMERGROUPID,
                                customerId = s.CUSTOMERID,
                                customerCode = s.TBL_CUSTOMER.CUSTOMERCODE,
                                customerName = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                                customerType = s.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                relationshipTypeId = s.RELATIONSHIPTYPEID,
                                relationshipTypeName = s.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                accountHolder = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                                companyId = s.TBL_CUSTOMER.COMPANYID,
                                branchId = s.TBL_CUSTOMER.BRANCHID,
                                isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == s.TBL_CUSTOMER.CUSTOMERCODE),
                                isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == s.CUSTOMERID) && x.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList),
                                isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == s.CUSTOMERID)),
                                taxIdentificationNumber = s.TBL_CUSTOMER.TAXNUMBER,
                                registrationNumber = s.TBL_CUSTOMER.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).REGISTRATIONNUMBER,
                                completedInformation = s.TBL_CUSTOMER.ACCOUNTCREATIONCOMPLETE,
                               // crdeitBureauCompleted = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(w=>w.CUSTOMERID==s.CUSTOMERID).ISREPORTOKAY,
                                customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == s.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                                {
                                    bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                    customerBvnid = b.CUSTOMERBVNID,
                                    firstname = b.FIRSTNAME,
                                    isValidBvn = b.ISVALIDBVN,
                                    isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                    surname = b.SURNAME
                                }).ToList(),
                                customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                                x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember).Select(x => new CustomerCompanyDirectorsViewModels()
                                {
                                    bankVerificationNumber = x.CUSTOMERBVN,
                                    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                    customerId = x.CUSTOMERID,
                                    firstname = x.FIRSTNAME,
                                    surname = x.SURNAME
                                }).ToList(),
                                customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                                x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder).Select(x => new CustomerCompanyShareholdersViewModels()
                                {
                                    bankVerificationNumber = x.CUSTOMERBVN,
                                    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                    customerId = x.CUSTOMERID,
                                    firstname = x.FIRSTNAME,
                                    surname = x.SURNAME
                                }).ToList(),
                                customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                                    .Select(cs => new CustomerClientOrSupplierViewModels()
                                    {
                                        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                        firstName = cs.FIRSTNAME,
                                        middleName = cs.MIDDLENAME,
                                        lastName = cs.LASTNAME,
                                        client_SupplierAddress = cs.ADDRESS,
                                        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                        client_SupplierEmail = cs.EMAILADDRESS,
                                        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                    }).ToList(),
                                customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                                    .Select(cs => new CustomerSupplierViewModels()
                                    {
                                        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                        firstName = cs.FIRSTNAME,
                                        middleName = cs.MIDDLENAME,
                                        lastName = cs.LASTNAME,
                                        client_SupplierAddress = cs.ADDRESS,
                                        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                        client_SupplierEmail = cs.EMAILADDRESS,
                                        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                    }).ToList(),
                                //relationshipOfficerId = context.tbl_Staff.FirstOrDefault(),
                                //relationshipManagerId = ,
                            }).ToList(),
                        });

            return data;
        }

        public IQueryable<CustomerGroupViewModel> SearchForCustomerGroupRealtime(int companyId, string searchQuery)
        {
            IQueryable<CustomerGroupViewModel> allGroups = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allGroups = GellAllCustomerGroupMappings()
                    //.Where(c => c.companyId == companyId)
                    .Where(x => x.customerGroupName.Contains(searchQuery)
                                || x.customerGroupCode.Contains(searchQuery)
                    ).Take(10);
            }

            return allGroups;
        }

        public IEnumerable<CustomerGroupViewModel> CustomerGroupSearch(string search)
        {
            var customerGroups = GetAllCustomerGroups().ToList();
            if (!string.IsNullOrWhiteSpace(search))
            {
                customerGroups = customerGroups.Where(x =>
               x.groupName.ToLower().Contains(search.ToLower())
               || x.groupCode.ToLower().Contains(search.ToLower())
               ).ToList();
                
            }
            return customerGroups;
        }

        public CustomerGroupViewModel GetCustomerGroupDetailsByGroupId(int customerGroupId)
        {
            var data = GellAllCustomerGroupMappings().FirstOrDefault(x => x.customerGroupId == customerGroupId);

            if (data != null)
            {
                return data;
            }

            return new CustomerGroupViewModel { };
        }
        #endregion TBL_CUSTOMER Group Mapping
    }
}