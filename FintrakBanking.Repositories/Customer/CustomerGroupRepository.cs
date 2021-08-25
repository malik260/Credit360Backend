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
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;

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
            var kycItem = (from d in context.TBL_KYC_ITEM
                           where d.TBL_PRODUCT.COMPANYID == companyId
                           select new KYCItemViewModel
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                DATETIMECREATED = genSetup.GetApplicationDate(),
                GROUPADDRESS = entity.groupAddress,
                GROUPCONTACTPERSON = entity.groupContactPerson
            };
            context.TBL_CUSTOMER_GROUP.Add(group);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName } )",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        public bool DoesGroupNameExist(string groupName, string groupCode)
        {
            var exist = (from a in context.TBL_CUSTOMER_GROUP
                         where a.GROUPNAME == groupName || a.GROUPCODE == groupCode
                         select a).Any();
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
                RISKRATINGID = custGroupModel.riskRatingId,
                CREATEDBY = (int)custGroupModel.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                COMPANYID = custGroupModel.companyId,
                ISCURRENT = true,
                GROUPADDRESS = custGroupModel.groupAddress,
                GROUPCONTACTPERSON = custGroupModel.groupContactPerson,
            };

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupAdded,
                STAFFID = custGroupModel.createdBy,
                BRANCHID = (short)custGroupModel.userBranchId,
                DETAIL = $"Added Customer Group: { custGroupModel.groupName } with Code: { custGroupModel.groupCode } ( { custGroupModel.groupName } )",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = custGroupModel.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                    throw new SecureException(ex.Message);
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                        join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
                        into bb from b in bb.DefaultIfEmpty()
                        where a.DELETED == false
                        select new CustomerGroupViewModel
                        {
                            groupCode = a.GROUPCODE,
                            groupName = a.GROUPNAME,
                            groupDescription = a.GROUPDESCRIPTION,
                            riskRating = b.RISKRATING,
                            riskRatingId = a.RISKRATINGID,
                            customerGroupId = a.CUSTOMERGROUPID,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY,
                            groupAddress = a.GROUPADDRESS,
                            groupContactPerson = a.GROUPCONTACTPERSON
                        });
            return data;
        }

        public IEnumerable<CustomerGroupViewModel> GetCustomerGroup()
        {
            var customerGroup = GetAllCustomerGroups().ToList();

            return customerGroup;
        }

        public IEnumerable<CustomerGroupViewModel> GetAllTempCustomerGroups()
        {
            var data = (from a in context.TBL_TEMP_CUSTOMER_GROUP
                        //join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
                        //into bb from b in bb.DefaultIfEmpty()
                        where a.DELETED == false
                        select new CustomerGroupViewModel
                        {
                            groupCode = a.GROUPCODE,
                            groupName = a.GROUPNAME,
                            groupDescription = a.GROUPDESCRIPTION,
                            //riskRating = b.RISKRATING,
                            riskRatingId = a.RISKRATINGID,
                            customerGroupId = a.CUSTOMERGROUPID,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY,
                            groupAddress = a.GROUPADDRESS,
                            groupContactPerson = a.GROUPCONTACTPERSON
                        }).ToList();
            return data;
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
            group.RISKRATINGID = entity.riskRatingId;
            group.LASTUPDATEDBY = (int)entity.createdBy;
            group.GROUPADDRESS = entity.groupAddress;
            group.GROUPCONTACTPERSON = entity.groupContactPerson;
            group.DATETIMEUPDATED = genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer Group: { entity.groupName } with Code: { entity.groupCode } ( { entity.groupName })",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                throw new SecureException("Customer group is already undergoing approval");
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
                tempGroupToUpdate.RISKRATINGID = entity.riskRatingId;
                tempGroupToUpdate.CREATEDBY = entity.createdBy;
                tempGroupToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempGroupToUpdate.COMPANYID = entity.companyId;
                tempGroupToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempGroupToUpdate.ISCURRENT = true;
                tempGroupToUpdate.GROUPADDRESS = entity.groupAddress;
                tempGroupToUpdate.GROUPCONTACTPERSON = entity.groupContactPerson;
            }
            else
            {
                //      var targetGroup = this.context.TBL_CUSTOMER_GROUP.Find(customerGroupId);

                tempCustomerGroup = new TBL_TEMP_CUSTOMER_GROUP()
                {
                    GROUPCODE = entity.groupCode, // targetGroup?.GROUPCODE,
                    GROUPNAME = entity.groupName,
                    GROUPDESCRIPTION = entity.groupDescription,
                    RISKRATINGID = entity.riskRatingId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    COMPANYID = entity.companyId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    GROUPADDRESS = entity.groupAddress,
                    GROUPCONTACTPERSON = entity.groupContactPerson
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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

        public bool GoForGroupMappingApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.CustomerGroupMapping;
            workFlow.StatusId = entity.approvalStatusIdUI == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;

            entity.externalInitialization = false;

            workFlow.LogForApproval(entity);

            if (workFlow.NewState == (int)ApprovalState.Ended)
            {
                return ApproveCustomerGroupMapping(entity.targetId, (short)workFlow.StatusId, entity);
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
                existingCustomerGroup.RISKRATINGID = customerGroupModel?.RISKRATINGID;
                existingCustomerGroup.CREATEDBY = customerGroupModel.CREATEDBY;
                existingCustomerGroup.DATETIMEUPDATED = DateTime.Now;
                existingCustomerGroup.GROUPADDRESS = customerGroupModel.GROUPADDRESS;
                existingCustomerGroup.GROUPCONTACTPERSON = customerGroupModel.GROUPCONTACTPERSON;
            }
            else //Insert a new customer group record into the real customer group table
            {
                var customerGroup = new TBL_CUSTOMER_GROUP()
                {
                    GROUPCODE = customerGroupModel.GROUPCODE,
                    GROUPNAME = customerGroupModel.GROUPNAME,
                    GROUPDESCRIPTION = customerGroupModel.GROUPDESCRIPTION,
                    RISKRATINGID = customerGroupModel.RISKRATINGID,
                    CREATEDBY = customerGroupModel.CREATEDBY,
                    GROUPADDRESS = customerGroupModel.GROUPADDRESS,
                    GROUPCONTACTPERSON = customerGroupModel.GROUPCONTACTPERSON,
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                          && atrail.RESPONSESTAFFID == null
                    select new CustomerGroupViewModel()
                    {
                        companyId = c.COMPANYID,
                        companyName = c.TBL_COMPANY.NAME,
                        customerGroupId = c.CUSTOMERGROUPID,
                        groupName = c.GROUPNAME,
                        groupCode = c.GROUPCODE,
                        riskRating = context.TBL_RISK_RATING.Where(x => x.RISKRATINGID == c.RISKRATINGID).FirstOrDefault().RATESDESCRIPTION,
                        riskRatingId = c.RISKRATINGID,
                        groupDescription = c.GROUPDESCRIPTION,
                        operationId = atrail.OPERATIONID,
                        groupAddress = c.GROUPADDRESS,
                        groupContactPerson = c.GROUPCONTACTPERSON
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
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingAdded,
                //StaffId = entity.createdBy,
                //BranchId = (short)entity.userBranchId,
                DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {entity.customerCode } to group  ( { groupName } ) ",
                //IPAddress = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {model.customerCode } to group  ( { groupName } ) ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                        operationId = (int)OperationsEnum.CustomerGroupMapping,
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
                    throw new SecureException(ex.Message);
                }
            }
        }

        ///TODO: Implement a more efficient method
        public bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> groupCustomers, int createdBy, short userBranchId, int companyId)
        {
            if (groupCustomers.Count <= 0)
                return false;

            bool output = false;
            //short relationshipTypeId = 0;
            var groupCustomer = groupCustomers[0];
            TBL_TEMP_CUSTOMER_GROUP_MAPPNG newGroupCustomer;
            List<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> listOfMappedGroup = new List<TBL_TEMP_CUSTOMER_GROUP_MAPPNG>();

            var oldGroupCustomers = this.context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Where(x => x.CUSTOMERGROUPID == groupCustomer.customerGroupId
                                                                                && x.DELETED == false).ToList();
            // Change status of the oldGroupCustomers 
            foreach (var oldGroupCustomer in oldGroupCustomers) {
                oldGroupCustomer.DELETED = true;
                oldGroupCustomer.DELETEDBY = createdBy;
                oldGroupCustomer.DATETIMEDELETED = DateTime.Now;
            }

            foreach (CustomerGroupMappingViewModel item in groupCustomers)
            {
                var oldGroupCustomer = oldGroupCustomers.FirstOrDefault(O => O.CUSTOMERID == item.customerId &&
                                                            O.CUSTOMERGROUPID == item.customerGroupId);

                newGroupCustomer = new TBL_TEMP_CUSTOMER_GROUP_MAPPNG();
                newGroupCustomer.CUSTOMERID = item.customerId;
                newGroupCustomer.CUSTOMERGROUPID = item.customerGroupId;
                newGroupCustomer.COMPANYID = companyId;
                newGroupCustomer.CREATEDBY = createdBy;
                newGroupCustomer.DELETED = false;
                newGroupCustomer.ISCURRENT = true;
                newGroupCustomer.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                newGroupCustomer.DATETIMECREATED = DateTime.Now;

                if (oldGroupCustomer == null && oldGroupCustomers.Count == 0) {
                    if (item.relationshipTypeId <= 0) {
                        throw new SecureException("Kindly select the 'Relationship Type'");
                    }
                    newGroupCustomer.RELATIONSHIPTYPEID = item.relationshipTypeId;
                }
                else {
                    newGroupCustomer.RELATIONSHIPTYPEID = oldGroupCustomers[0].RELATIONSHIPTYPEID;
                }
                
                context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.Add(newGroupCustomer);

                // Audit Section ---------------------------
                var customerName = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == newGroupCustomer.CUSTOMERID).ToList()
                                                        .Select(x => new
                                                        {
                                                            customerName = x.FIRSTNAME + " " + x.LASTNAME
                                                        }).FirstOrDefault();

                var groupName = (from gr in this.context.TBL_CUSTOMER_GROUP where gr.CUSTOMERGROUPID == groupCustomer.customerGroupId select gr.GROUPNAME).FirstOrDefault();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingAdded,
                    STAFFID = createdBy,
                    BRANCHID = userBranchId,
                    DETAIL = $"Added Customer Group Mapping to customer: { customerName } with code: { newGroupCustomer.TBL_CUSTOMER.CUSTOMERCODE } to group  ( { groupName } ) ",
                    //IPAddress =CommonHelpers.GetLocalIpAddress() ,
                    //Url = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -----------------------

                
                if (oldGroupCustomer != null) {
                    // Delete oldGroupCustomer from TBL_CUSTOMER_GROUP_MAPPING
                    var cust = context.TBL_CUSTOMER_GROUP_MAPPING.Where(O => O.CUSTOMERID == oldGroupCustomer.CUSTOMERID && O.CUSTOMERGROUPID == oldGroupCustomer.CUSTOMERGROUPID).FirstOrDefault();

                    if (cust != null) {
                        context.TBL_CUSTOMER_GROUP_MAPPING.Remove(cust);

                        // Insert this customer in TBL_CUSTOMER_GROUP_MAPPING_ARC
                        var deletedGroupCustomer = new TBL_CUSTOMER_GROUP_MAPPING_ARC()
                        {
                            CUSTOMERGROUPMAPPINGID = cust.CUSTOMERGROUPMAPPINGID,
                            CUSTOMERID = cust.CUSTOMERID,
                            CUSTOMERGROUPID = cust.CUSTOMERGROUPID,
                            CREATEDBY = cust.CREATEDBY,
                            RELATIONSHIPTYPEID = cust.RELATIONSHIPTYPEID,
                            DELETED = true,
                            DATETIMECREATED = DateTime.Now
                        };

                        context.TBL_CUSTOMER_GROUP_MAPPING_ARC.Add(deletedGroupCustomer);
                    }
                }
                

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        output = context.SaveChanges() > 0;

                        var entity = new ApprovalViewModel
                        {
                            staffId = createdBy,
                            companyId = companyId,
                            approvalStatusId = (int)ApprovalStatusEnum.Pending,
                            targetId = newGroupCustomer.CUSTOMERGROUPMAPPINGID,
                            operationId = (int)OperationsEnum.CustomerGroupMapping,
                            BranchId = userBranchId,
                            externalInitialization = true
                        };

                        var response = workFlow.LogForApproval(entity);
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }

            }

            return output;
        }

        //public bool AddMultipleCustomerGroupMapping(List<CustomerGroupMappingViewModel> customerGroups, int createdBy, short userBranchId, int companyId)
        //{
        //    if (customerGroups.Count <= 0)
        //        return false;
        //    List<TBL_TEMP_CUSTOMER_GROUP_MAPPNG> listOfMappedGroup = new List<TBL_TEMP_CUSTOMER_GROUP_MAPPNG>();
        //    bool output = false;
        //    foreach (CustomerGroupMappingViewModel item in customerGroups)
        //    {
        //        var group = this.context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.FirstOrDefault(x => x.CUSTOMERID == item.customerId && x.CUSTOMERGROUPID == item.customerGroupId);
        //        if (group == null)
        //        {
        //            var groupMap = new TBL_TEMP_CUSTOMER_GROUP_MAPPNG
        //            {
        //                CUSTOMERID = item.customerId,
        //                CUSTOMERGROUPID = item.customerGroupId,
        //                RELATIONSHIPTYPEID = item.relationshipTypeId,
        //                COMPANYID = companyId,
        //                CREATEDBY = createdBy,
        //                DELETED = false,
        //                ISCURRENT = true,
        //                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
        //                DATETIMECREATED = DateTime.Now
        //            };
        //            listOfMappedGroup.Add(groupMap);

        //            // Audit Section ---------------------------
        //            var customer = this.context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == groupMap.CUSTOMERID).ToList()
        //                                                    .Select(x => new
        //                                                    {
        //                                                        customerName = x.FIRSTNAME + " " + x.LASTNAME
        //                                                    }).FirstOrDefault();
        //            var groupName = (from gr in this.context.TBL_CUSTOMER_GROUP where gr.CUSTOMERGROUPID == item.customerGroupId select gr.GROUPNAME).FirstOrDefault();

        //            var audit = new TBL_AUDIT
        //            {
        //                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingAdded,
        //                STAFFID = createdBy,
        //                BRANCHID = userBranchId,
        //                DETAIL = $"Added Customer Group Mapping to customer: { customer } with code: {item.customerCode } to group  ( { groupName } ) ",
        //                //IPAddress = entity.userIPAddress,
        //                //Url = entity.applicationUrl,
        //                APPLICATIONDATE = genSetup.GetApplicationDate(),
        //                SYSTEMDATETIME = DateTime.Now
        //            };
        //            //this.auditTrail.AddAuditTrail(audit);
        //            //end of Audit section -----------------------

        //            using (var trans = context.Database.BeginTransaction())
        //            {
        //                try
        //                {
        //                    auditTrail.AddAuditTrail(audit);
        //                    context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG.AddRange(listOfMappedGroup);
        //                    output = context.SaveChanges() > 0;
        //                    if (!output)
        //                    {
        //                        trans.Rollback(); throw new Exception("Customer Group Mapping failed.");
        //                    }
        //                    var entity = new ApprovalViewModel
        //                    {
        //                        staffId = createdBy,
        //                        companyId = companyId,
        //                        approvalStatusId = (int)ApprovalStatusEnum.Pending,
        //                        targetId = groupMap.CUSTOMERGROUPMAPPINGID,
        //                        operationId = (int)OperationsEnum.CustomerGroupMapping,
        //                        BranchId = userBranchId,
        //                        externalInitialization = true
        //                    };
        //                    var response = workFlow.LogForApproval(entity);

        //                    if (response)
        //                    {
        //                        trans.Commit();
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    trans.Rollback();
        //                    throw new SecureException(ex.Message);
        //                }
        //            }
        //        }

        //    }
        //    return output;
        //}

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

        public IEnumerable<CustomerGroupMappingViewModel> GetTempCustomerGroupMappingByGroupId(int customerGroupId)
        {
            var customerGroupMapping = from a in context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG
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

                var data = (from b in context.TBL_CUSTOMER_GROUP_MAPPING
                            join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                            where b.CUSTOMERGROUPID == customerGroupId && b.DELETED != true
                            && c.COMPANYID == companyId
                           && c.ACCOUNTCREATIONCOMPLETE == true
                            select new GroupCustomerMembersViewModel
                            {
                                customerId = b.CUSTOMERID,
                                customerCode = c.CUSTOMERCODE,
                                lastName = c.LASTNAME,
                                firstName = c.FIRSTNAME
                            }).ToList();

                foreach (var item in data)
                {
                    if (creditBureau.VerifyCustomerValidCreditBureau(item.customerId))
                    {
                        lstCustomer.Add(item);
                    }
                }

                return lstCustomer.OrderBy(x => x.customerName);



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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                throw new SecureException("Customer group map is already undergoing approval");
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupMappingAdded,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Customer Group Mapping '{customerGroupMapModel.CUSTOMERGROUPMAPPINGID}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.SaveAll();
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapsAwaitingApprovals(int staffId, int companyId)
        {
            /*var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CustomerGroupCreation).ToList();*/ 

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CustomerGroupMapping).ToList();

            var data = (from c in context.TBL_TEMP_CUSTOMER_GROUP_MAPPNG
                    join g in context.TBL_CUSTOMER_GROUP on c.CUSTOMERGROUPID equals g.CUSTOMERGROUPID
                    join coy in context.TBL_COMPANY on c.COMPANYID equals coy.COMPANYID 
                    join atrail in context.TBL_APPROVAL_TRAIL on c.CUSTOMERGROUPMAPPINGID equals atrail.TARGETID
                    where atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending 
                    && c.ISCURRENT == true
                    && c.DELETED == false
                    && atrail.OPERATIONID == (int) OperationsEnum.CustomerGroupMapping
                    && atrail.RESPONSESTAFFID == null 
                    && ids.Contains((int) atrail.TOAPPROVALLEVELID)
                    orderby c.CUSTOMERGROUPMAPPINGID descending
                    select new CustomerGroupMappingViewModel()
                    {
                        approvalStatusId = c.APPROVALSTATUSID,
                        companyId = c.COMPANYID,
                        companyName = coy.NAME,
                        customerGroupId = c.CUSTOMERGROUPID,
                        customerGroupName = g.GROUPNAME,
                        customerGroupCode = g.GROUPCODE,
                        groupDescription = g.GROUPDESCRIPTION,
                        customerGroupMappingId = c.CUSTOMERGROUPMAPPINGID,
                        customerCode = c.TBL_CUSTOMER.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        relationshipTypeId = c.RELATIONSHIPTYPEID,
                        relationshipTypeName = c.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                        customerName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.LASTNAME,
                        operationId = (int) OperationsEnum.CustomerGroupMapping
                    }).ToList();
            return data;
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
        public bool AddCustomerGroupRelationshipTypes(LookupViewModel model)
        {
            if (model.lookupId > 0)
            {
                var type = context.TBL_CUSTOMER_GROUP_RELATN_TYPE.FirstOrDefault(x => x.RELATIONSHIPTYPEID == model.lookupId);
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
                            // customerGroupMappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == a.CUSTOMERGROUPID).Select(s => new CustomerGroupMappingViewModel

                            //{
                            //    customerGroupMappingId = s.CUSTOMERGROUPMAPPINGID,
                            //    customerGroupId = s.CUSTOMERGROUPID,
                            //    customerId = s.CUSTOMERID,
                            //    customerCode = s.TBL_CUSTOMER.CUSTOMERCODE,
                            //    customerName = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                            //    customerType = s.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            //    relationshipTypeId = s.RELATIONSHIPTYPEID,
                            //    relationshipTypeName = s.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                            //    productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                            //    accountHolder = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                            //    companyId = s.TBL_CUSTOMER.COMPANYID,
                            //    branchId = s.TBL_CUSTOMER.BRANCHID,
                            //    isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == s.TBL_CUSTOMER.CUSTOMERCODE),
                            //    isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == s.CUSTOMERID) && x.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList),
                            //    isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == s.CUSTOMERID)),
                            //    taxIdentificationNumber = s.TBL_CUSTOMER.TAXNUMBER,
                            //    registrationNumber = s.TBL_CUSTOMER.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).REGISTRATIONNUMBER,
                            //    completedInformation = s.TBL_CUSTOMER.ACCOUNTCREATIONCOMPLETE,
                            //   // crdeitBureauCompleted = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(w=>w.CUSTOMERID==s.CUSTOMERID).ISREPORTOKAY,
                            //    //customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == s.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            //    //{
                            //    //    bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                            //    //    customerBvnid = b.CUSTOMERBVNID,
                            //    //    firstname = b.FIRSTNAME,
                            //    //    isValidBvn = b.ISVALIDBVN,
                            //    //    isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                            //    //    surname = b.SURNAME
                            //    //}).ToList(),
                            //    //customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                            //    //x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember).Select(x => new CustomerCompanyDirectorsViewModels()
                            //    //{
                            //    //    bankVerificationNumber = x.CUSTOMERBVN,
                            //    //    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                            //    //    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                            //    //    customerId = x.CUSTOMERID,
                            //    //    firstname = x.FIRSTNAME,
                            //    //    surname = x.SURNAME
                            //    //}).ToList(),
                            //    //customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                            //    //x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder).Select(x => new CustomerCompanyShareholdersViewModels()
                            //    //{
                            //    //    bankVerificationNumber = x.CUSTOMERBVN,
                            //    //    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                            //    //    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                            //    //    customerId = x.CUSTOMERID,
                            //    //    firstname = x.FIRSTNAME,
                            //    //    surname = x.SURNAME
                            //    //}).ToList(),
                            //    //customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            //    //    .Select(cs => new CustomerClientOrSupplierViewModels()
                            //    //    {
                            //    //        client_SupplierId = cs.CLIENT_SUPPLIERID,
                            //    //        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                            //    //        firstName = cs.FIRSTNAME,
                            //    //        middleName = cs.MIDDLENAME,
                            //    //        lastName = cs.LASTNAME,
                            //    //        client_SupplierAddress = cs.ADDRESS,
                            //    //        client_SupplierPhoneNumber = cs.PHONENUMBER,
                            //    //        client_SupplierEmail = cs.EMAILADDRESS,
                            //    //        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                            //    //        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            //    //    }).ToList(),
                            //    //customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                            //    //    .Select(cs => new CustomerSupplierViewModels()
                            //    //    {
                            //    //        client_SupplierId = cs.CLIENT_SUPPLIERID,
                            //    //        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                            //    //        firstName = cs.FIRSTNAME,
                            //    //        middleName = cs.MIDDLENAME,
                            //    //        lastName = cs.LASTNAME,
                            //    //        client_SupplierAddress = cs.ADDRESS,
                            //    //        client_SupplierPhoneNumber = cs.PHONENUMBER,
                            //    //        client_SupplierEmail = cs.EMAILADDRESS,
                            //    //        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                            //    //        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            //    //    }).ToList(),
                            //    //relationshipOfficerId = context.tbl_Staff.FirstOrDefault(),
                            //    //relationshipManagerId = ,
                            //}).ToList(),
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

        public List<CurrentCustomerExposure> GetGroupExposureByGroupId(int customerGroupId, int companyId)
        { 
            var exposures = new List<CurrentCustomerExposure>();
            var customers = GetCustomerGroupMappingByGroupId(customerGroupId).Select(c => new { c.customerGroupId, c.customerName, c.customerId }).ToList();
            exposures = GetAllGroupCustomersExposureWithoutTotal(customers.Select(c => new CustomerExposure { customerId = c.customerId }).ToList(), companyId);
            return exposures;
        }

        public List<CurrentCustomerExposure> GetAllGroupCustomersExposureWithoutTotal(List<CustomerExposure> customer, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();


            foreach (var item in customer)
            {
                var customerCode = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == item.customerId).Select(x => x.CUSTOMERCODE).FirstOrDefault();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                customerName = a.CUSTOMERNAME,
                                customerCode = a.CUSTOMERID.Trim(),
                                facilityType = a.ADJFACILITYTYPE,
                                approvedAmount = a.LOANAMOUNYTCY ?? 0,
                                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
                                currency = a.CURRENCYNAME,
                                exposureTypeCodeString = a.EXPOSURETYPECODE,
                                adjFacilityTypeString = a.ADJFACILITYTYPE,
                                adjFacilityTypeCode = a.ADJFACILITYTYPEid,
                                productIdString = a.PRODUCTID,
                                productCode = a.PRODUCTCODE,
                                productName = a.PRODUCTNAME,
                                //existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                //proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
                                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                bookingDate = a.BOOKINGDATE,
                                tenorString = a.TENOR,
                                //maturityDateString = a.MATURITYDATE,
                                maturityDate = Convert.ToDateTime(a.MATURITYDATE),
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).ToList();

                if (exposure.Count() > 0)
                {
                    foreach (var e in exposure)
                    {
                        e.exposureTypeId = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
                        //e.productId = int.Parse(e.productIdString);
                        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);
                        //e.bookingDate = e.bookingDateString;
                        //e.maturityDate = DateTime.Parse(e.maturityDateString);
                        //e.productId = int.Parse(e.productIdString);
                    }
                    exposures.AddRange(exposure);
                }


                //exposure = from a in context.TBL_LOAN
                //           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                //           join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //           select new CurrentCustomerExposure
                //           {
                //               customerName = c.FIRSTNAME + c.LASTNAME,
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               existingLimit = a.PRINCIPALAMOUNT,
                //               //proposedLimit = a.OUTSTANDINGPRINCIPAL,
                //               proposedLimit = 0,
                //               //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //               approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //               outstandings = a.OUTSTANDINGPRINCIPAL + a.OUTSTANDINGINTEREST,
                //               recommendedLimit = 0,
                //               PastDueObligationsInterest = a.PASTDUEINTEREST,
                //               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //               reviewDate = DateTime.Now,
                //               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //               loanStatus = "Running",
                //               referenceNumber = a.LOANREFERENCENUMBER,
                //               applicationStatusId = b.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                //               currency = a.TBL_CURRENCY.CURRENCYNAME,
                //               maturityDate = a.MATURITYDATE
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = (from a in context.TBL_LOAN_REVOLVING
                //            join b in context.TBL_LOAN_APPLICATION on a.LOANREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //            join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //            where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //            select new CurrentCustomerExposure
                //            {
                //                customerName = c.FIRSTNAME + c.LASTNAME,
                //                facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //                existingLimit = a.OVERDRAFTLIMIT,
                //                //proposedLimit = a.OVERDRAFTLIMIT,
                //                proposedLimit = 0,
                //                //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,   
                //                approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //                outstandings = a.OVERDRAFTLIMIT,
                //                recommendedLimit = 0,
                //                casaAccountId = a.CASAACCOUNTID,
                //                PastDueObligationsInterest = a.PASTDUEINTEREST,
                //                PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //                reviewDate = DateTime.Now,
                //                prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //                loanStatus = "Running",
                //                referenceNumber = a.LOANREFERENCENUMBER,
                //                applicationStatusId = b.APPLICATIONSTATUSID,
                //                currency = a.TBL_CURRENCY.CURRENCYNAME,
                //                maturityDate = a.MATURITYDATE
                //            }).ToList();

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = (from a in context.TBL_LOAN_CONTINGENT
                //            join b in context.TBL_LOAN_APPLICATION on a.LOANREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //            join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //            where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //            select new CurrentCustomerExposure
                //            {
                //                customerName = c.FIRSTNAME + c.LASTNAME,
                //                facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //                existingLimit = a.CONTINGENTAMOUNT,
                //                //proposedLimit = a.OVERDRAFTLIMIT,
                //                proposedLimit = 0,
                //                //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,   
                //                approvedAmount = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //                outstandings = a.CONTINGENTAMOUNT,
                //                recommendedLimit = 0,
                //                casaAccountId = a.CASAACCOUNTID,
                //                PastDueObligationsInterest = a.CONTINGENTAMOUNT,
                //                PastDueObligationsPrincipal = a.CONTINGENTAMOUNT,
                //                reviewDate = DateTime.Now,
                //                //prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //                loanStatus = "Running",
                //                referenceNumber = a.LOANREFERENCENUMBER,
                //                applicationStatusId = b.APPLICATIONSTATUSID,
                //                currency = a.TBL_CURRENCY.CURRENCYNAME,
                //                maturityDate = a.MATURITYDATE
                //            }).ToList();

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                //           join b in context.TBL_LOAN_APPLICATION on a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //           join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //           where a.CUSTOMERID == item.customerId && a.TBL_LOAN_APPLICATION.COMPANYID == companyId && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                //           select new CurrentCustomerExposure
                //           {

                //               applicationStatusId = b.APPLICATIONSTATUSID,
                //               customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                //               customerCode = c.CUSTOMERCODE.Trim(),
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               approvedAmount = a.APPROVEDAMOUNT,
                //               currency = a.TBL_CURRENCY.CURRENCYNAME,
                //               //exposureTypeId = int.Parse(a.EXPOSURETYPECODE),
                //               //adjFacilityType = a.ADJFACILITYTYPE,
                //               productId = a.TBL_PRODUCT.PRODUCTID,
                //               productName = a.TBL_PRODUCT.PRODUCTNAME,
                //               outstandings = 0,
                //               pastDueObligationsPrincipal = 0,
                //               reviewDate = DateTime.Now,
                //               loanStatus = "Processing",
                //               referenceNumber = b.APPLICATIONREFERENCENUMBER
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);


                //var staggingLoan = from a in stgCon.STG_LOAN_MART
                //                   where a.CUST_ID == customCode
                //                   select new CurrentCustomerExposure
                //                   {
                //                       facilityType = a.SCHM_TYPE,
                //                       existingLimit = a.FAC_GRANT_AMT,
                //                       //proposedLimit = a.FINAL_BALANCE,
                //                       proposedLimit = 0,
                //                       recommendedLimit = 0,
                //                       outstandings = a.FINAL_BALANCE,
                //                       PastDueObligationsInterest = a.INT_DUE,
                //                       PastDueObligationsPrincipal = a.DAYS_PAST_DUE,// 0,
                //                       reviewDate = DateTime.Now,
                //                       prudentialGuideline = a.USER_CLASSIFICATION == "1" ? "Performing" : "Non-Performing",
                //                       loanStatus = "Running"
                //                   };

                //exposures.Union(staggingLoan);

            }
            
            return exposures;
        }

        public List<CurrentCustomerExposure> GetAllGroupCustomersExposure(List<CustomerExposure> customer, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();


            foreach (var item in customer)
            {
                var customCode = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == item.customerId).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                exposure = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           select new CurrentCustomerExposure
                           {
                               customerName = c.FIRSTNAME + c.LASTNAME,
                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                               existingLimit = a.PRINCIPALAMOUNT,
                               //proposedLimit = a.OUTSTANDINGPRINCIPAL,
                               proposedLimit = 0,
                               //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                               outstandings = a.OUTSTANDINGPRINCIPAL + a.OUTSTANDINGINTEREST,
                               recommendedLimit = 0,
                               PastDueObligationsInterest = a.PASTDUEINTEREST,
                               pastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                               reviewDate = DateTime.Now,
                               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                               loanStatus = "Running",
                               referenceNumber = a.LOANREFERENCENUMBER,
                               applicationStatusId = b.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               currency = a.TBL_CURRENCY.CURRENCYNAME,
                               maturityDate = a.MATURITYDATE
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                exposure = (from a in context.TBL_LOAN_REVOLVING
                            join b in context.TBL_LOAN_APPLICATION on a.LOANREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                            join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                            where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                            select new CurrentCustomerExposure
                            {
                                customerName = c.FIRSTNAME + c.LASTNAME,
                                facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                                existingLimit = a.OVERDRAFTLIMIT,
                                //proposedLimit = a.OVERDRAFTLIMIT,
                                proposedLimit = 0,
                                //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,   
                                outstandings = a.OVERDRAFTLIMIT,
                                recommendedLimit = 0,
                                casaAccountId = a.CASAACCOUNTID,
                                PastDueObligationsInterest = a.PASTDUEINTEREST,
                                pastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                                reviewDate = DateTime.Now,
                                prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                                loanStatus = "Running",
                                referenceNumber = a.LOANREFERENCENUMBER,
                                applicationStatusId = b.APPLICATIONSTATUSID,
                                currency = a.TBL_CURRENCY.CURRENCYNAME,
                                maturityDate = a.MATURITYDATE
                            }).ToList();

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                //           join b in context.TBL_LOAN_APPLICATION on a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //           where a.CUSTOMERID == item.customerId && a.TBL_LOAN_APPLICATION.COMPANYID == companyId && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                //           select new CurrentCustomerExposure
                //           {
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               existingLimit = 0,
                //               proposedLimit = a.PROPOSEDAMOUNT,
                //               recommendedLimit = a.APPROVEDAMOUNT,
                //               outstandings = 0,
                //               PastDueObligationsInterest = 0,
                //               PastDueObligationsPrincipal = 0,
                //               reviewDate = DateTime.Now,
                //               prudentialGuideline = "Processing",
                //               loanStatus = "Processing",
                //               referenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                //               applicationStatusId = b.APPLICATIONSTATUSID
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);


                //var staggingLoan = from a in stgCon.STG_LOAN_MART
                //                   where a.CUST_ID == customCode
                //                   select new CurrentCustomerExposure
                //                   {
                //                       facilityType = a.SCHM_TYPE,
                //                       existingLimit = a.FAC_GRANT_AMT,
                //                       //proposedLimit = a.FINAL_BALANCE,
                //                       proposedLimit = 0,
                //                       recommendedLimit = 0,
                //                       outstandings = a.FINAL_BALANCE,
                //                       PastDueObligationsInterest = a.INT_DUE,
                //                       PastDueObligationsPrincipal = a.DAYS_PAST_DUE,// 0,
                //                       reviewDate = DateTime.Now,
                //                       prudentialGuideline = a.USER_CLASSIFICATION == "1" ? "Performing" : "Non-Performing",
                //                       loanStatus = "Running"
                //                   };

                //exposures.Union(staggingLoan);

            }

            exposures.Add(new CurrentCustomerExposure
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                outstandings = exposures.Sum(t => t.outstandings),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
            });

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupExposureByCustomerId(int customerId, int companyId)
        {
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            var customerGroups = GetCustomerGroupMapping().Where(m => m.customerGroupId == customerId).ToList();
            //var customers = GetGroupMembersByGroupId(customerGroup.customerGroupId, companyId).Select(c => new { c.customerName, c.customerId });
            foreach (var group in customerGroups)
            {
                exposures.AddRange(GetGroupExposureByGroupId(group.customerGroupId, companyId));
            }
            return exposures;
        }

        public IEnumerable<CustomerGroupViewModel> SearchForCustomerGroup(int companyId, string searchQuery)
        {
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var data = (from a in context.TBL_CUSTOMER_GROUP
                            where a.DELETED == false && a.GROUPNAME.ToLower().Contains(searchQuery)
                            || a.GROUPCODE.ToLower().Contains(searchQuery)
                            select new CustomerGroupViewModel
                            {
                                customerGroupId = a.CUSTOMERGROUPID,
                                customerGroupName = a.GROUPNAME,
                                customerGroupCode = a.GROUPCODE,
                                customerGroupMappings = (from b in context.TBL_CUSTOMER_GROUP_MAPPING
                                                         join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                                                         where b.CUSTOMERGROUPID == a.CUSTOMERGROUPID
                                                         select new CustomerGroupMappingViewModel
                                                         {
                                                             customerGroupMappingId = b.CUSTOMERGROUPMAPPINGID,
                                                             customerGroupId = b.CUSTOMERGROUPID,
                                                             customerId = b.CUSTOMERID,
                                                             customerCode = c.CUSTOMERCODE,
                                                             customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                                             customerType = c.TBL_CUSTOMER_TYPE.NAME,
                                                             relationshipTypeId = b.RELATIONSHIPTYPEID,
                                                             relationshipManagerId = c.RELATIONSHIPOFFICERID,
                                                             relationshipOfficerId = c.RELATIONSHIPOFFICERID,
                                                             relationshipTypeName = b.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                                             productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                                             accountHolder = c.FIRSTNAME + " " + c.LASTNAME,
                                                             companyId = c.COMPANYID,
                                                             branchId = c.BRANCHID,
                                                             isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == c.CUSTOMERCODE),
                                                             isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == c.CUSTOMERID) && x.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList),
                                                             isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == c.CUSTOMERID)),
                                                             taxIdentificationNumber = c.TAXNUMBER,
                                                             registrationNumber = c.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).REGISTRATIONNUMBER,
                                                             completedInformation = c.ACCOUNTCREATIONCOMPLETE,
                                                             //  crdeitBureauCompleted = creditBureau.VerifyCustomerValidCreditBureau(c.customerId)
                                                         }).ToList()
                            }).Take(10).ToList();

                //foreach (var item in data)
                //{
                //    foreach (var ss in item.customerGroupMappings)
                //    {
                //        item.crdeitBureauCompleted = creditBureau.VerifyCustomerValidCreditBureau(item.customerId);
                //    }
                //}


                return data;
            }

            return null;
        }
        public IEnumerable<CustomerGroupMappingViewModel> GetAllCustomerGroupMappingByGroupId(int customerGroupId)
        {
            var GroupMappings = (from b in context.TBL_CUSTOMER_GROUP_MAPPING
                                 join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                                 where b.CUSTOMERGROUPID == customerGroupId
                                 select new CustomerGroupMappingViewModel
                                 {
                                     customerGroupMappingId = b.CUSTOMERGROUPMAPPINGID,
                                     customerGroupId = b.CUSTOMERGROUPID,
                                     customerId = b.CUSTOMERID,
                                     customerCode = c.CUSTOMERCODE,
                                     customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                     customerType = c.TBL_CUSTOMER_TYPE.NAME,
                                     relationshipTypeId = b.RELATIONSHIPTYPEID,
                                     relationshipTypeName = b.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                     productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                     accountHolder = c.FIRSTNAME + " " + c.LASTNAME,
                                     companyId = c.COMPANYID,
                                     branchId = c.BRANCHID,
                                     isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == c.CUSTOMERCODE),
                                     isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == c.CUSTOMERID) && x.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList),
                                     isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == c.CUSTOMERID)),
                                     taxIdentificationNumber = c.TAXNUMBER,
                                     registrationNumber = c.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).REGISTRATIONNUMBER,
                                     completedInformation = c.ACCOUNTCREATIONCOMPLETE,
                                     // crdeitBureauCompleted = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(w=>w.CUSTOMERID==s.CUSTOMERID).ISREPORTOKAY,
                                     //customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == s.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                                     //{
                                     //    bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                     //    customerBvnid = b.CUSTOMERBVNID,
                                     //    firstname = b.FIRSTNAME,
                                     //    isValidBvn = b.ISVALIDBVN,
                                     //    isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                     //    surname = b.SURNAME
                                     //}).ToList(),
                                     //customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                                     //x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember).Select(x => new CustomerCompanyDirectorsViewModels()
                                     //{
                                     //    bankVerificationNumber = x.CUSTOMERBVN,
                                     //    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                     //    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                     //    customerId = x.CUSTOMERID,
                                     //    firstname = x.FIRSTNAME,
                                     //    surname = x.SURNAME
                                     //}).ToList(),
                                     //customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(x => x.CUSTOMERID == s.CUSTOMERID &&
                                     //x.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder).Select(x => new CustomerCompanyShareholdersViewModels()
                                     //{
                                     //    bankVerificationNumber = x.CUSTOMERBVN,
                                     //    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                     //    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                     //    customerId = x.CUSTOMERID,
                                     //    firstname = x.FIRSTNAME,
                                     //    surname = x.SURNAME
                                     //}).ToList(),
                                     //customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                                     //    .Select(cs => new CustomerClientOrSupplierViewModels()
                                     //    {
                                     //        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                     //        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                     //        firstName = cs.FIRSTNAME,
                                     //        middleName = cs.MIDDLENAME,
                                     //        lastName = cs.LASTNAME,
                                     //        client_SupplierAddress = cs.ADDRESS,
                                     //        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                     //        client_SupplierEmail = cs.EMAILADDRESS,
                                     //        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                     //        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                     //    }).ToList(),
                                     //customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                                     //    .Select(cs => new CustomerSupplierViewModels()
                                     //    {
                                     //        client_SupplierId = cs.CLIENT_SUPPLIERID,
                                     //        clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                     //        firstName = cs.FIRSTNAME,
                                     //        middleName = cs.MIDDLENAME,
                                     //        lastName = cs.LASTNAME,
                                     //        client_SupplierAddress = cs.ADDRESS,
                                     //        client_SupplierPhoneNumber = cs.PHONENUMBER,
                                     //        client_SupplierEmail = cs.EMAILADDRESS,
                                     //        client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                     //        client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                     //    }).ToList(),
                                     //relationshipOfficerId = context.tbl_Staff.FirstOrDefault(),
                                     //relationshipManagerId = ,
                                 }).ToList();

            return GroupMappings;
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
            var data = (from a in context.TBL_CUSTOMER_GROUP
                        where a.DELETED == false && a.CUSTOMERGROUPID == customerGroupId
                        select new CustomerGroupViewModel
                        {
                            customerGroupId = a.CUSTOMERGROUPID,
                            customerGroupName = a.GROUPNAME,
                            customerGroupCode = a.GROUPCODE,
                            customerGroupMappings = (from b in context.TBL_CUSTOMER_GROUP_MAPPING
                                                     join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                                                     where b.CUSTOMERGROUPID == a.CUSTOMERGROUPID
                                                     select new CustomerGroupMappingViewModel
                                                     {
                                                         customerGroupMappingId = b.CUSTOMERGROUPMAPPINGID,
                                                         customerGroupId = b.CUSTOMERGROUPID,
                                                         customerId = b.CUSTOMERID,
                                                         customerCode = c.CUSTOMERCODE,
                                                         customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                                         customerType = c.TBL_CUSTOMER_TYPE.NAME,
                                                         customerTypeId = c.TBL_CUSTOMER_TYPE.CUSTOMERTYPEID,
                                                         relationshipTypeId = b.RELATIONSHIPTYPEID,
                                                         relationshipTypeName = b.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                                         productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                                         accountHolder = c.FIRSTNAME + " " + c.LASTNAME,
                                                         companyId = c.COMPANYID,
                                                         branchId = c.BRANCHID,
                                                         isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == c.CUSTOMERCODE),
                                                         isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == c.CUSTOMERID) && x.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList),
                                                         isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == c.CUSTOMERID)),
                                                         taxIdentificationNumber = c.TAXNUMBER,
                                                         registrationNumber = c.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == c.CUSTOMERID).REGISTRATIONNUMBER,
                                                         completedInformation = c.ACCOUNTCREATIONCOMPLETE,
                                                     }).ToList()
                        }).FirstOrDefault();

            foreach (var item in data.customerGroupMappings)
            {
                item.crdeitBureauCompleted = creditBureau.VerifyCustomerValidCreditBureau(item.customerId);
            }


            return data;
        }
        #endregion TBL_CUSTOMER Group Mapping
    }
}