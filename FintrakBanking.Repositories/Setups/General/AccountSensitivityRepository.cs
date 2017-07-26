using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{

    [Export(typeof(IAccountSensitivityRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountSensitivityRepository : IAccountSensitivityRepository
    {
        private FinTrakBankingContext context;

        public AccountSensitivityRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public AccountSensitivityViewModel GetAccountSensitivityLevelsByLevelId(int sensitivityId)
        {
            var accountSensitivity = (from a in context.tbl_Customer_Sensitivity_Level
                                      where a.CustomerSensitivityLevelId == sensitivityId
                                      select new AccountSensitivityViewModel
                                      {
                                          SensitivityDescription = a.Description,
                                          SensitivityId = a.CustomerSensitivityLevelId,
                                          SensitivityLevel = a.Level
                                      }).SingleOrDefault();
            return accountSensitivity;
        }

        public IEnumerable<AccountSensitivityViewModel> GetAllAccountSensitivityLevels()
        {
            var accountSensitivity = (from a in context.tbl_Customer_Sensitivity_Level
                                      select new AccountSensitivityViewModel
                                      {
                                          SensitivityDescription = a.Description,
                                          SensitivityId = a.CustomerSensitivityLevelId,
                                          SensitivityLevel = a.Level
                                      });
            return accountSensitivity;
        }
    }
}