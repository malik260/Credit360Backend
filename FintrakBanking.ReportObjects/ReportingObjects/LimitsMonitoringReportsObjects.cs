using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ReportObjects.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
   public  class LimitsMonitoringReportsObjects
    {
        public IEnumerable<SectorLimitViewModel> GetSectorLoanAmountLimit(int companyId)
        {
            
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();

                var output = (from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              join c in context.tbl_Sector on a.TargetId equals c.SectorId
                              join d in context.tbl_Loan on c.SectorId equals d.tbl_Sub_Sector.SectorId

                              where a.LimitTypeId == (int)LimitType.Sector && d.LoanStatusId == (short)LoanStatusEnum.Active
                              && b.LimitMetricId == (int)LimitMatricEnum.LoanAmount && d.CompanyId == companyId
                              group d by new { c.SectorId, c.Code, c.Name, a.MaximumValue } into groupedQ
                              select new SectorLimitViewModel()
                              {
                                  companyName = company.Name ,
                                  Id = groupedQ.Key.SectorId,
                                  Code = groupedQ.Key.Code,
                                  Name = groupedQ.Key.Name,
                                  Limit = groupedQ.Key.MaximumValue,
                                  Usage = groupedQ.Sum(i => i.OutstandingPrincipal),
                                  Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.OutstandingPrincipal)
                              }).ToList();

                return output;
            }
        }

        public IEnumerable<SectorLimitViewModel> GetBranchLoanAmountLimit(int branchId,int companyId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();

                var output = (from a in context.tbl_Limit_Detail
                              join b in context.tbl_Branch on a.TargetId equals b.BranchId
                              join c in context.tbl_Loan on a.TargetId equals c.BranchId
                              where a.LimitTypeId == (int)LimitType.Sector && c.LoanStatusId == (short)LoanStatusEnum.Active
                                  && a.tbl_Limit.tbl_Limit_Metric.LimitMetricId == (int)LimitMatricEnum.LoanAmount && c.CompanyId == companyId && c.BranchId == branchId
                              group new { a, b, c } by new
                              {
                                  a.MaximumValue,
                                  b.BranchName,
                                  b.BranchCode,
                                  b.BranchId
                              } into groupedQ
                              select new  SectorLimitViewModel

                              {
                                  companyName = company.Name,
                                   Limit = groupedQ.Key.MaximumValue,
                                   Usage = groupedQ.Sum(p => p.c.OutstandingPrincipal),
                                   Name = groupedQ.Key.BranchName,
                                   Code = groupedQ.Key.BranchCode,
                                   Id = groupedQ.Key.BranchId,
                                  Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.c.OutstandingPrincipal)
                                   
            }).ToList();

                return output;
            }
        }
    }
}
