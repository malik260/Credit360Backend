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
            var type = new tbl_Account_Type()
            {
                AccountTypeCode = accounttype.accountTypeCode,
                AccountTypeName = accounttype.accountTypeName,
                AccountCategoryId = accounttype.accountCategoryId,
                CreatedBy = accounttype.createdBy,
                DateTimeCreated = DateTime.Now
            };

            this.context.tbl_Account_Type.Add(type);
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AccountTypeAdded,
                StaffId = (int)accounttype.createdBy,
                BranchId = (short)accounttype.userBranchId,
                Detail = $"Added accounttype: '{accounttype.accountTypeName}' with code: '{accounttype.accountTypeCode}' ",
                IPAddress = accounttype.userIPAddress,
                Url = accounttype.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
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
            return this.context.tbl_Account_Type.Select(p => new AccountTypeViewModel()
            {
                accountTypeId = p.AccountTypeId,
                accountCategoryId = p.AccountCategoryId,
                accountCategoryName = p.tbl_Account_Category.AccountCategoryName,
                accountTypeCode = p.AccountTypeCode,
                accountTypeName = p.AccountTypeName,
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
            return this.context.tbl_Account_Type.Select(p => new AccountTypeViewModel()
            {
                accountTypeId = p.AccountTypeId,
                accountCategoryId = p.AccountCategoryId,
                accountTypeCode = p.AccountTypeCode,
                accountTypeName = p.AccountTypeName,
                dateTimeCreated = p.DateTimeCreated.Value ,
                createdBy = p.CreatedBy.Value 
            }).FirstOrDefault(u => u.accountTypeId == accountTypeId);
        }

        /// <summary>
        /// Update Account Type
        /// </summary>
        /// <param name="accounttype"></param>
        /// <returns></returns>
        public bool UpdateAccountType(int accountTypeId, AccountTypeViewModel accounttype)
        {
            var type = this.context.tbl_Account_Type.Where(p => p.AccountTypeId == accountTypeId).FirstOrDefault();
            if (type != null)
            {
                type.AccountTypeCode = accounttype.accountTypeCode;
                type.AccountTypeName = accounttype.accountTypeName;
                type.AccountCategoryId = accounttype.accountCategoryId;
                type.LastUpdatedBy = accounttype.lastUpdatedBy;
                type.DateTimeUpdated = accounttype.dateTimeUpdated;
            }
           
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AccountTypeUpdated,
                StaffId = (int)accounttype.createdBy,
                BranchId = (short)accounttype.userBranchId,
                Detail = $"Updated accounttype: '{accounttype.accountTypeName}' with code: '{accounttype.accountTypeCode}' ",
                IPAddress = accounttype.userIPAddress,
                Url = accounttype.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public bool DeleteAcountType(int accountTypeId, UserInfo user)
        {
            var type = this.context.tbl_Account_Type.Where(p => p.AccountTypeId == accountTypeId).FirstOrDefault();
            if (type != null)
            {
                type.Deleted = true;
                //type.DateTimeDeleted;
            }
             
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AccountTypeAdded,
                StaffId = (int)user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted accounttype: '{type.AccountTypeName}' with code: '{type.AccountTypeCode}' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------
            return this.SaveAll();
        }
    }
}