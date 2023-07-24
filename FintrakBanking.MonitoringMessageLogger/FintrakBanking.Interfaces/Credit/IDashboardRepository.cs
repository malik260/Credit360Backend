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
        List<DashboardViewModel> LoanApplicationsBySector(DateTime startDate, DateTime endDate,int companyId, int staffId);
        List<DashboardReportItem>     LoanPerformance(DateTime startDate, DateTime endDate,int companyId, int staffId);
        List<DashboardViewModel> LoanOnThePipeline(DateTime startDate, DateTime endDate, int companyId, int staffId);
        List<DashboardViewModel> ExpotureByRiskRating(DateTime startDate, DateTime endDate, int companyId, int staffId);
        List<DashboardViewModel> CollateralCoverage(DateTime startDate, DateTime endDate, int companyId, int staffId);
        List<DashboardViewModel> ApprovedLoan(DateTime startDate, DateTime endDate, int companyId, int staffId);
        List<DashboardViewModel> TotalRiskExposure(DateTime startDate, DateTime endDate, int companyId, int staffId);
        List<LoanDisburseByType> LoanDisbursedByType(DateTime startDate, DateTime endDate, int companyId, int staffId);
        DashboardViewModel GetLoanInThePipelineLms(int operationId, int staffId, int companyId, int branchId, int? classId);
        DashboardViewModel GetApprovedLoansLms(int companyId, int staffId);
    }
}
