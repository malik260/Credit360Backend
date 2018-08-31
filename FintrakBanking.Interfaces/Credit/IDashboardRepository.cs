using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IDashboardRepository
    {
        List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate);
    }
}
