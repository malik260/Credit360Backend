using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.Finance
{
    [Export(typeof(IAccountTypeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountTypeRepository : IAccountTypeRepository
    {
        private FinTrakBankingContext context;
        IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public AccountTypeRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
        }

        private bool SaveAll()
        {
            var result = this.context.SaveChanges();

            return this.context.SaveChanges() >= 0;
        }

        /// <summary>
        /// Add New Account Type
        /// </summary>
        /// <param name="accounttype"></param>
        /// <returns></returns>
        public bool AddAccountType(AddAccountTypeViewModel accounttype)
        {
            var type = new TBL_ACCOUNT_TYPE()
            {
                ACCOUNTTYPECODE = accounttype.accountTypeCode,
                ACCOUNTTYPENAME = accounttype.accountTypeName,
                ACCOUNTCATEGORYID = accounttype.accountCategoryId,
                CREATEDBY = accounttype.createdBy,
                DATETIMECREATED = DateTime.Now
            };

            this.context.TBL_ACCOUNT_TYPE.Add(type);
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AccountTypeAdded,
                STAFFID = (int)accounttype.createdBy,
                BRANCHID = (short)accounttype.userBranchId,
                DETAIL = $"Added accounttype: '{accounttype.accountTypeName}' with code: '{accounttype.accountTypeCode}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = accounttype.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        /// <summary>
        /// Get All Account Type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountTypeViewModel> GetAllAccountType()
        {
            return this.context.TBL_ACCOUNT_TYPE.Select(p => new AccountTypeViewModel()
            {
                accountTypeId = p.ACCOUNTTYPEID,
                accountCategoryId = p.ACCOUNTCATEGORYID,
                accountCategoryName = p.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                accountTypeCode = p.ACCOUNTTYPECODE,
                accountTypeName = p.ACCOUNTTYPENAME,
                // dateTimeCreated =(DateTime) p.DateTimeCreated,
                // createdBy =(int) p.CreatedBy
            });
        }

        /// <summary>
        /// Get Account Type By ID
        /// </summary>
        /// <param name="accountTypeId"></param>
        /// <returns></returns>
        public AccountTypeViewModel GetAllAccountTypeById(int accountTypeId)
        {
            return this.context.TBL_ACCOUNT_TYPE.Select(p => new AccountTypeViewModel()
            {
                accountTypeId = p.ACCOUNTTYPEID,
                accountCategoryId = p.ACCOUNTCATEGORYID,
                accountTypeCode = p.ACCOUNTTYPECODE,
                accountTypeName = p.ACCOUNTTYPENAME,
                dateTimeCreated = p.DATETIMECREATED.Value ,
                createdBy = p.CREATEDBY.Value 
            }).FirstOrDefault(u => u.accountTypeId == accountTypeId);
        }

        /// <summary>
        /// Update Account Type
        /// </summary>
        /// <param name="accounttype"></param>
        /// <returns></returns>
        public bool UpdateAccountType(int accountTypeId, AccountTypeViewModel accounttype)
        {
            var type = this.context.TBL_ACCOUNT_TYPE.Where(p => p.ACCOUNTTYPEID == accountTypeId).FirstOrDefault();
            if (type != null)
            {
                type.ACCOUNTTYPECODE = accounttype.accountTypeCode;
                type.ACCOUNTTYPENAME = accounttype.accountTypeName;
                type.ACCOUNTCATEGORYID = accounttype.accountCategoryId;
                type.LASTUPDATEDBY = accounttype.lastUpdatedBy;
                type.DATETIMEUPDATED = accounttype.dateTimeUpdated;
            }
           
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AccountTypeUpdated,
                STAFFID = (int)accounttype.createdBy,
                BRANCHID = (short)accounttype.userBranchId,
                DETAIL = $"Updated accounttype: '{accounttype.accountTypeName}' with code: '{accounttype.accountTypeCode}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = accounttype.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteAcountType(int accountTypeId, UserInfo user)
        {
            var type = this.context.TBL_ACCOUNT_TYPE.Where(p => p.ACCOUNTTYPEID == accountTypeId).FirstOrDefault();
            if (type != null)
            {
                type.DELETED = true;
                //type.DateTimeDeleted;
            }
             
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AccountTypeAdded,
                STAFFID = (int)user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted accounttype: '{type.ACCOUNTTYPENAME}' with code: '{type.ACCOUNTTYPECODE}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }
    }
}