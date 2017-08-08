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
            var accountCategory = new tbl_Account_Category()
            {
                AccountCategoryId = category.accountCategoryId,
                AccountCategoryName = category.accountCategoryName
            };
            this.context.tbl_Account_Category.Add(accountCategory);


            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.AccountCategoryAdded,
                StaffId = category.createdBy,
                BranchId = (short)category.userBranchId,
                Detail = $"Added Finance Account Category:  {accountCategory.AccountCategoryName}",
                IPAddress = category.userIPAddress,
                Url = category.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now


            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        /// <summary>
        /// Get All Account Category
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AccountCategoryViewModel> GetAllAccountCategory()
        {
            return context.tbl_Account_Category.Select(x => new AccountCategoryViewModel()
            {
                accountCategoryId = x.AccountCategoryId,
                accountCategoryName = x.AccountCategoryName
            });
        }

        /// <summary>
        /// Get Account Category By  Id
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public AccountCategoryViewModel GetAccountCategoryById(int categoryId)
        {
            return context.tbl_Account_Category
                .Select(x => new AccountCategoryViewModel()
                {
                    accountCategoryId = x.AccountCategoryId,
                    accountCategoryName = x.AccountCategoryName
                }).FirstOrDefault(u => u.accountCategoryId == categoryId);
        }
    }
}