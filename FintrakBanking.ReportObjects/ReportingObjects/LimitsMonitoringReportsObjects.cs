using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public  class LimitsMonitoringReportsObjects
    {
        public IEnumerable<SectorLimitViewModel> GetSectorLoanAmountLimit(int companyId, int operationId)
        {
            
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.tbl_Company.Where(c => c.CompanyId == companyId).FirstOrDefault();

                var output = (

                    from B in context.tbl_Sub_Sector
                    select new SectorLimitViewModel()
                    {
                        companyLogo = company.LogoPath ,
                        companyName = company.Name,
                        sectorcode = B.Code,
                        sectorName = B.tbl_Sector.Name,
                        subsectorName = (B.Name ?? "NOT DEFINED"),
                        subsectorCode = (B.Code ?? "NOT DEFINED"),
                        limitMaximumValue = ((System.Decimal?)((Int64)((Int16?)B.SubSectorId ?? (Int16?)0) > 0 ? (System.Decimal?)
                        ((from D in context.tbl_Limit_Detail
                          where D.LimitTypeId == 2 && D.TargetId == (Int32)B.SubSectorId
                          select new { D.MaximumValue }).FirstOrDefault().MaximumValue) : (Int64)((Int16?)B.SubSectorId ?? (Int16?)0) == 0 ? (System.Decimal?)0 : null) ?? (System.Decimal?)0),
                        usage = ((System.Decimal?)((Int64)((Int16?)B.SubSectorId ?? (Int16?)0) > 0 ? (System.Decimal?)
                        (from C in context.tbl_Loan
                         where C.SubSectorId == B.SubSectorId
                         select new
                         { C.OutstandingPrincipal }).Sum(p => p.OutstandingPrincipal) : null) ?? (System.Decimal?)0)
                    }).ToList();


                //from Loan in context.tbl_Loan
                //           join LimitDetail in context.tbl_Limit_Detail
                //                 on new { SubSectorId = (int)Loan.SubSectorId, LimitTypeId =(int) LimitType.Sector }
                //             equals new { SubSectorId = LimitDetail.TargetId, LimitDetail.LimitTypeId } into LimitDetail_join
                //           from LimitDetail in LimitDetail_join.DefaultIfEmpty()

                //           group new { Loan.tbl_Sub_Sector.tbl_Sector, Loan.tbl_Sub_Sector, LimitDetail, Loan } by new
                //           {
                //               SectorId = Loan.tbl_Sub_Sector.tbl_Sector.SectorId,
                //               Sector = Loan.tbl_Sub_Sector.tbl_Sector.Name,
                //               Subsector = Loan.tbl_Sub_Sector.Name,
                //               MaximumValue = LimitDetail.MaximumValue,
                //               CompanyName = Loan.tbl_Company.Name,
                //               subsectorCode = Loan.tbl_Sub_Sector.Code,
                //               sectorCode = Loan.tbl_Sub_Sector.tbl_Sector.Code
                //           } into groupedQ
                //           select new SectorLimitViewModel()
                //           {
                //               companyName = company.Name,
                //               Id = groupedQ.Key.SectorId,
                //               Code = groupedQ.Key.sectorCode,
                //               Name = groupedQ.Key.Sector,
                //               Limit = (decimal?) groupedQ.Key.MaximumValue ?? 0,
                //               Usage = (decimal?) groupedQ.Sum(i => i.Loan.OutstandingPrincipal) ?? 0,
                //              // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.Loan.OutstandingPrincipal)
                //           }).ToList();

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
                                   limitMaximumValue = groupedQ.Key.MaximumValue,
                                   usage = groupedQ.Sum(p => p.c.OutstandingPrincipal),
                                   sectorName = groupedQ.Key.BranchName,
                                   subsectorCode = groupedQ.Key.BranchCode,
                                  // Id = groupedQ.Key.BranchId,
                                 // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.c.OutstandingPrincipal)
                                   
            }).ToList();

                return output;
            }
        }
    }
}
