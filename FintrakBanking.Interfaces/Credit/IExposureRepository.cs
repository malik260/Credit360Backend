using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IExposureRepository
    {
        List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId);
    }
}
