using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Customer
{
 
    public class CustomerFSCaptionGroupRepository : ICustomerFSCaptionGroupRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSCaptionGroupRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        public bool AddCustomerFSCaptionGroup(CustomerFSCaptionGroupViewModel entity)
        {
            var group = new tbl_Customer_FS_Caption_Group
            {
                FSCaptionGroupName = entity.fsCaptionGroupName,
                CompanyId = entity.companyId,
                //GroupDescription = entity.groupDescription,
                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate()
            };

            context.tbl_Customer_FS_Caption_Group.Add(group);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionGroupAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer FS Caption Group: { entity.fsCaptionGroupName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerFSCaptionGroupViewModel> GetCustomerFSCaptionGroup(int companyId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption_Group
                                where a.CompanyId == companyId && a.Deleted == false
                                select new CustomerFSCaptionGroupViewModel
                                {
                                    fsCaptionGroupId = a.FSCaptionGroupId,
                                    fsCaptionGroupName = a.FSCaptionGroupName,
                                    companyId = a.CompanyId,     
                                    companyName = a.tbl_Company.Name,
                                    dateTimeCreated = a.DateTimeCreated,
                                    createdBy = a.CreatedBy
                                }).ToList();
            return data;
        }

        public CustomerFSCaptionGroupViewModel GetCustomerFSCaptionGroupById(short fsCaptionGroupId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption_Group
                        where a.FSCaptionGroupId == fsCaptionGroupId
                        select new CustomerFSCaptionGroupViewModel
                        {
                            fsCaptionGroupId = a.FSCaptionGroupId,
                            fsCaptionGroupName = a.FSCaptionGroupName,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).FirstOrDefault();
            return data;
        }

        public bool UpdateCustomerFSCaptionGroup(short groupId, CustomerFSCaptionGroupViewModel entity)
        {
            var group = this.context.tbl_Customer_FS_Caption_Group.Find(groupId);
            if (group == null) return false;

            group.FSCaptionGroupName = entity.fsCaptionGroupName;
            
            group.LastUpdatedBy = (int)entity.createdBy;
            group.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionGroupUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer FS Caption Group: { entity.fsCaptionGroupName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
    }
}
