using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels.Customer;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CashFlowLendingRepository :ICashFlowLendingRepository
    {
        private FinTrakBankingContext context;
        private ICustomerRepository customer;

        public CashFlowLendingRepository(FinTrakBankingContext _context, ICustomerRepository _customer)
        {
            this.context = _context;
            this.customer = _customer;
        }

        public bool AddCustomer(CustomerViewModels model)
        {
            customer.AddCustomer(model);
            return true;
        }


    }
}
