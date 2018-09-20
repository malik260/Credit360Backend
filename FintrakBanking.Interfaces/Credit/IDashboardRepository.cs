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
        List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate,int companyId);
        List<DashboardReportItem>     LoanPerformance(DateTime startDate, DateTime endDate,int companyId);
        List<DashboardViewModel> LoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId);
        List<DashboardViewModel> ExpotureByRiskRating(DateTime startDate, DateTime endDate, int companyId);
        List<DashboardViewModel> CollateralCoverage(DateTime startDate, DateTime endDate, int companyId);
        List<DashboardViewModel> ApprovedLoan(DateTime startDate, DateTime endDate, int companyId);
        List<DashboardViewModel> TotalRiskExposure(DateTime startDate, DateTime endDate, int companyId);
        List<LoanDisburseByType> LoanDisbursedByType(DateTime startDate, DateTime endDate, int companyId);
    }
}
