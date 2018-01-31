using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{

    
    public class AccountSensitivityRepository : IAccountSensitivityRepository
    {
        private FinTrakBankingContext context;

        public AccountSensitivityRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        public AccountSensitivityViewModel GetAccountSensitivityLevelsByLevelId(int sensitivityId)
        {
            var accountSensitivity = (from a in context.TBL_CUSTOMER_SENSITIVITY_LEVEL
                                      where a.CUSTOMERSENSITIVITYLEVELID == sensitivityId
                                      select new AccountSensitivityViewModel
                                      {
                                          SensitivityDescription = a.DESCRIPTION,
                                          SensitivityId = a.CUSTOMERSENSITIVITYLEVELID,
                                          SensitivityLevel = a.CUSTOMERSENSITIVITYLEVELID
                                      }).SingleOrDefault();
            return accountSensitivity;
        }

        public IEnumerable<AccountSensitivityViewModel> GetAllAccountSensitivityLevels()
        {
            var accountSensitivity = (from a in context.TBL_CUSTOMER_SENSITIVITY_LEVEL
                                      select new AccountSensitivityViewModel
                                      {
                                          SensitivityDescription = a.DESCRIPTION,
                                          SensitivityId = a.CUSTOMERSENSITIVITYLEVELID,
                                          SensitivityLevel = a.CUSTOMERSENSITIVITYLEVELID
                                      });
            return accountSensitivity;
        }
    }
}