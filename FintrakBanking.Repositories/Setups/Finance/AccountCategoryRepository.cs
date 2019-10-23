using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.Finance;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.Finance
{
    [Export(typeof(IAccountCategoryRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountCategoryRepository : IAccountCategoryRepository
    {
        private FinTrakBankingContext context;
        IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public AccountCategoryRepository(FinTrakBankingContext _context,
                                          IGeneralSetupRepository genSetup,
                                          IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            this.auditTrail = _auditTrail;

        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        /// <summary>
        /// Add New Account Category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public bool AddFinanceAccountCategorySetup(AccountCategoryViewModel category)
        {
            var accountCategory = new TBL_ACCOUNT_CATEGORY()
            {
                ACCOUNTCATEGORYID = category.accountCategoryId,
                ACCOUNTCATEGORYNAME = category.accountCategoryName
            };
            this.context.TBL_ACCOUNT_CATEGORY.Add(accountCategory);


            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AccountCategoryAdded,
                STAFFID = category.createdBy,
                BRANCHID = (short)category.userBranchId,
                DETAIL = $"Added Finance Account Category:  {accountCategory.ACCOUNTCATEGORYNAME}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = category.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()


            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        /// <summary>
        /// Get All Account Category
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountCategoryViewModel> GetAllAccountCategory()
        {
            return context.TBL_ACCOUNT_CATEGORY.Select(x => new AccountCategoryViewModel()
            {
                accountCategoryId = x.ACCOUNTCATEGORYID,
                accountCategoryName = x.ACCOUNTCATEGORYNAME
            });
        }

        /// <summary>
        /// Get Account Category By  Id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public AccountCategoryViewModel GetAccountCategoryById(int categoryId)
        {
            return context.TBL_ACCOUNT_CATEGORY
                .Select(x => new AccountCategoryViewModel()
                {
                    accountCategoryId = x.ACCOUNTCATEGORYID,
                    accountCategoryName = x.ACCOUNTCATEGORYNAME
                }).FirstOrDefault(u => u.accountCategoryId == categoryId);
        }
    }
}