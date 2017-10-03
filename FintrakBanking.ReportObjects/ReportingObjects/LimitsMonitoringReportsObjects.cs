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

                var output = (from Loan in context.tbl_Loan
                               join LimitDetail in context.tbl_Limit_Detail
                                     on new { SubSectorId = (int)Loan.SubSectorId, LimitTypeId = 2 }
                                 equals new { SubSectorId = LimitDetail.TargetId, LimitDetail.LimitTypeId } into LimitDetail_join
                               from LimitDetail in LimitDetail_join.DefaultIfEmpty()

                               group new { Loan.tbl_Sub_Sector.tbl_Sector, Loan.tbl_Sub_Sector, LimitDetail, Loan } by new
                               {
                                   SectorId = Loan.tbl_Sub_Sector.tbl_Sector.SectorId,
                                   Sector = Loan.tbl_Sub_Sector.tbl_Sector.Name,
                                   Subsector = Loan.tbl_Sub_Sector.Name,
                                   MaximumValue = LimitDetail.MaximumValue,
                                   CompanyName = Loan.tbl_Company.Name,
                                   subsectorCode = Loan.tbl_Sub_Sector.Code,
                                   sectorCode = Loan.tbl_Sub_Sector.tbl_Sector.Code
                               } into groupedQ
                               select new SectorLimitViewModel()
                               {
                                   companyName = company.Name,
                                   Id = groupedQ.Key.SectorId,
                                   Code = groupedQ.Key.sectorCode,
                                   Name = groupedQ.Key.Sector,
                                   Limit = (decimal?) groupedQ.Key.MaximumValue ?? 0,
                                   Usage = (decimal?) groupedQ.Sum(i => i.Loan.OutstandingPrincipal) ?? 0,
                                  // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.Loan.OutstandingPrincipal)
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
                                 // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.c.OutstandingPrincipal)
                                   
            }).ToList();

                return output;
            }
        }
    }
}
