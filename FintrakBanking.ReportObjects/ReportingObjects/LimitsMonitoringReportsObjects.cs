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
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId ).FirstOrDefault();

                var output = (

                    from B in context.TBL_SUB_SECTOR
                    select new SectorLimitViewModel()
                    {
                        companyLogo = company.LOGOPATH ,
                        companyName = company.NAME,
                        sectorcode = B.CODE,
                        sectorName = B.TBL_SECTOR.NAME,
                        subsectorName = (B.NAME ?? "NOT DEFINED"),
                        subsectorCode = (B.CODE ?? "NOT DEFINED"),
                        limitMaximumValue = ((System.Decimal?)((Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) > 0 ? (System.Decimal?)
                        ((from D in context.TBL_LIMIT_DETAIL
                          where D.LIMITTYPEID == 2 && D.TARGETID == (Int32)B.SUBSECTORID
                          select new { D.MAXIMUMVALUE }).FirstOrDefault().MAXIMUMVALUE) : (Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) == 0 ? (System.Decimal?)0 : null) ?? (System.Decimal?)0),
                        usage = ((System.Decimal?)((Int64)((Int16?)B.SUBSECTORID ?? (Int16?)0) > 0 ? (System.Decimal?)
                        (from C in context.TBL_LOAN
                         where C.SUBSECTORID == B.SUBSECTORID
                         select new
                         { C.OUTSTANDINGPRINCIPAL }).Sum(p => p.OUTSTANDINGPRINCIPAL) : null) ?? (System.Decimal?)0)
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
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();

                var output = (from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_BRANCH on a.TARGETID equals b.BRANCHID
                              join c in context.TBL_LOAN on a.TARGETID equals c.BRANCHID
                              where a.LIMITTYPEID == (int)LimitType.Sector && c.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  && a.TBL_LIMIT.TBL_LIMIT_METRIC.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount && c.COMPANYID == companyId && c.BRANCHID == branchId
                              group new { a, b, c } by new
                              {
                                  a.MAXIMUMVALUE,
                                  b.BRANCHNAME,
                                  b.BRANCHCODE,
                                  b.BRANCHID
                              } into groupedQ
                              select new  SectorLimitViewModel

                              {
                                  companyName = company.NAME,
                                   limitMaximumValue = groupedQ.Key.MAXIMUMVALUE,
                                   usage = groupedQ.Sum(p => p.c.OUTSTANDINGPRINCIPAL),
                                   sectorName = groupedQ.Key.BRANCHNAME,
                                   subsectorCode = groupedQ.Key.BRANCHCODE,
                                  // Id = groupedQ.Key.BranchId,
                                 // Balance = groupedQ.Key.MaximumValue - groupedQ.Sum(i => i.c.OutstandingPrincipal)
                                   
            }).ToList();

                return output;
            }
        }
    }
}
