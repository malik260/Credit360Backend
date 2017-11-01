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
            var group = new TBL_CUSTOMER_FS_CAPTION_GROUP
            {
                FSCAPTIONGROUPNAME = entity.fsCaptionGroupName,
                COMPANYID = entity.companyId,
                //GroupDescription = entity.groupDescription,
                CREATEDBY = (int)entity.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            context.TBL_CUSTOMER_FS_CAPTION_GROUP.Add(group);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionGroupAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer FS Caption Group: { entity.fsCaptionGroupName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerFSCaptionGroupViewModel> GetCustomerFSCaptionGroup(int companyId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_GROUP
                                where a.COMPANYID == companyId && a.DELETED == false
                                select new CustomerFSCaptionGroupViewModel
                                {
                                    fsCaptionGroupId = a.FSCAPTIONGROUPID,
                                    fsCaptionGroupName = a.FSCAPTIONGROUPNAME,
                                    companyId = a.COMPANYID,     
                                    companyName = a.TBL_COMPANY.NAME,
                                    dateTimeCreated = a.DATETIMECREATED,
                                    createdBy = a.CREATEDBY
                                }).ToList();
            return data;
        }

        public CustomerFSCaptionGroupViewModel GetCustomerFSCaptionGroupById(short fsCaptionGroupId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_GROUP
                        where a.FSCAPTIONGROUPID == fsCaptionGroupId
                        select new CustomerFSCaptionGroupViewModel
                        {
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            fsCaptionGroupName = a.FSCAPTIONGROUPNAME,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).FirstOrDefault();
            return data;
        }

        public bool UpdateCustomerFSCaptionGroup(short groupId, CustomerFSCaptionGroupViewModel entity)
        {
            var group = this.context.TBL_CUSTOMER_FS_CAPTION_GROUP.Find(groupId);
            if (group == null) return false;

            group.FSCAPTIONGROUPNAME = entity.fsCaptionGroupName;
            
            group.LASTUPDATEDBY = (int)entity.createdBy;
            group.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionGroupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer FS Caption Group: { entity.fsCaptionGroupName } ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
    }
}
