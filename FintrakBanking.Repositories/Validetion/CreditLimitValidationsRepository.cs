using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.CreditLimitValidations;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//using System.Math;

namespace FintrakBanking.Repositories.CreditLimitValidations

{
    public class CreditLimitValidationsRepository : ICreditLimitValidationsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private ILoanRepository loanRepository;
        private ICustomerRepository customerRepository;
        private IOverRideRepository customOverride;

        public CreditLimitValidationsRepository(IGeneralSetupRepository _genSetup, ILoanRepository _loanRepository, ICustomerRepository _customerRepository,

        FinTrakBankingContext _context, IOverRideRepository customOverride)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            loanRepository = _loanRepository;
            customerRepository = _customerRepository;
            this.customOverride = customOverride;
        }

        public int ValidateWatchList(int customerId)
        {
            int watchlistresults = 0;
            var watchlist = (from a in context.TBL_LOAN
                             join b in context.TBL_LOAN_PRUDENTIALGUIDELINE on a.EXT_PRUDENT_GUIDELINE_STATUSID equals b.PRUDENTIALGUIDELINESTATUSID
                             where a.CUSTOMERID == customerId && b.PRUDENTIALGUIDELINESTATUSID == (int)LoanPrudentialStatusEnum.WatchList
                             select a);
            if (watchlist.Any())
            {
                string custCode = watchlist.FirstOrDefault().TBL_CUSTOMER.CUSTOMERCODE;
                watchlistresults = this.customOverride.EffectOverride(custCode, (int)LoanPrudentialStatusEnum.WatchList, custCode);
            }

            return watchlistresults;
        }

        public IEnumerable<CustomerEligibilityViewModel> ValidateCustomerEligibility(string customerCode)
        {
            var customerEligibility = (from a in context.TBL_LOAN_CAMSOL
                                       join b in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals b.CAMSOLTYPEID
                                       where a.CUSTOMERCODE == customerCode && a.CANTAKELOAN == false
                                       select new CustomerEligibilityViewModel()
                                       {
                                           camsolType = b.CAMSOLTYPENAME.ToUpper()
                                       }).ToList();
            return customerEligibility;
        }

        public CustomerEligibility GetCustomerEligibility(string customerCode)
        {
            var camsol = (from a in context.TBL_LOAN_CAMSOL
                          join b in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals b.CAMSOLTYPEID
                          where a.CUSTOMERCODE == customerCode && a.CANTAKELOAN == false
                          select new { b.CAMSOLTYPENAME }).FirstOrDefault();

            CustomerEligibility result = new CustomerEligibility();
            result.eligible = true;
            result.message = String.Empty;

            if (camsol != null)
            {
                result.eligible = false;
                result.message = "This customer is in " + camsol.CAMSOLTYPENAME;
            }

            return result;
        }

        public int ValidateBlackList(string customerCode)
        {
            int blacklistresults = 0;
            var blacklist = (from a in context.TBL_CUSTOMER_BLACKLIST
                             where a.CUSTOMERCODE == customerCode
                             select a);

            if (blacklist.Any())
            {
                string custCode = blacklist.FirstOrDefault().CUSTOMERCODE;
                blacklistresults = this.customOverride.EffectOverride(custCode, (int)OverrideEnum.BlackbookOverride, custCode);
            }

            return blacklistresults;
        }

        //public IEnumerable<CustomerEligibilityViewModel> ValidateCustomerEligibility(string customerCode)
        //{
        //    var blacklist = (from a in context.TBL_CUSTOMER_BLACKLIST
        //                     where a.CUSTOMERCODE == customerCode
        //                     select new CustomerEligibilityViewModel()
        //                     {
        //                         customerCode = a.CUSTOMERCODE,
        //                         dateBlackListed = a.DATEBLACKLISTED,
        //                         reason = a.REASON
        //                     }).ToList();
        //    return blacklist;
        //}

        public CreditLimitValidationsModel ValidateAmountByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = (from d in context.TBL_LOAN
                                  where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select d.OUTSTANDINGPRINCIPAL).Sum();

            var limitAmount = 0;
            //(from a in context.TBL_LIMIT_DETAIL
            //               join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //               where a.LIMITTYPEID == (int)LimitType.Branch && a.TARGETID == branchId &&
            //               b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
            //               select a.MAXIMUMVALUE).Sum();

            model.outstandingBalance = (double)outstandingbal;
            model.limit = (double)limitAmount;
            model.difference = (double)(outstandingbal - limitAmount);
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var loanTotalExposure = (from d in context.TBL_LOAN
                                     where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active || d.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                                     select new
                                     {
                                         d.OUTSTANDINGPRINCIPAL,
                                         d.EXCHANGERATE
                                     }).ToList();
            var sumLoanTotalExposure = loanTotalExposure.Select(c => c.OUTSTANDINGPRINCIPAL * (decimal)c.EXCHANGERATE).Sum();


            var ODTotalExposure = (from d in context.TBL_LOAN_REVOLVING
                                   where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                   select new
                                   {
                                       d.OVERDRAFTLIMIT,
                                       d.EXCHANGERATE
                                   }).ToList();
            var sumODTotalExposure = ODTotalExposure.Select(c => c.OVERDRAFTLIMIT * (decimal)c.EXCHANGERATE).Sum();

            var contingentTotalExposure = (from d in context.TBL_LOAN_CONTINGENT
                                           where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                           select new
                                           {
                                               d.CONTINGENTAMOUNT,
                                               d.EXCHANGERATE
                                           }).ToList();
            var sumContingentTotalExposure = contingentTotalExposure.Select(c => c.CONTINGENTAMOUNT * (decimal)c.EXCHANGERATE).Sum();

            var loanOutstandingBalance = (from d in context.TBL_LOAN
                                          where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                          && d.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                          select new
                                          {
                                              d.OUTSTANDINGPRINCIPAL,
                                              d.EXCHANGERATE
                                          }).ToList();
            var sumLoanOutstandingBalance = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL * (decimal)c.EXCHANGERATE).Sum();


            var ODOutstandingBalance = (from d in context.TBL_LOAN_REVOLVING
                                        where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                        && d.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                        select new
                                        {
                                            d.OVERDRAFTLIMIT,
                                            d.EXCHANGERATE
                                        }).ToList();
            var sumODOutstandingBalance = ODOutstandingBalance.Select(c => c.OVERDRAFTLIMIT * (decimal)c.EXCHANGERATE).Sum();

            var limitAmount = from a in context.TBL_BRANCH
                              where a.BRANCHID == branchId
                              let maximumLimit = a.NPL_LIMIT
                              select maximumLimit;

            var branchRatio = (((double)(sumLoanOutstandingBalance + sumODOutstandingBalance) / (double)(sumLoanTotalExposure + sumODTotalExposure)) * 100);
            if (!doubleHasRealValue(branchRatio)) { branchRatio = 0; }
            model.totalExposure = (double)(sumLoanTotalExposure + sumODTotalExposure + sumContingentTotalExposure);
            model.outstandingBalance = (double)(sumLoanOutstandingBalance + sumODOutstandingBalance);
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = model.limit - model.outstandingBalance;
            model.ratio = branchRatio;

            return model;
        }

        public bool doubleHasRealValue(double value)
        {
            return (!Double.IsNaN(value) && !Double.IsInfinity(value));
        }

        public CreditLimitValidationsModel ValidateAmountBySegment(short segmentId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.PRODUCTID == segmentId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.PRODUCTID == segmentId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = 0;
            //from a in context.TBL_LIMIT_DETAIL
            //                  join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                  where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == segmentId &&
            //                  b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
            //                  select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLBySegment(short segmentId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.PRODUCTID == segmentId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.PRODUCTID == segmentId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = 0;
            //from a in context.TBL_LIMIT_DETAIL
            //                  join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                  where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == segmentId &&
            //                  b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
            //                  select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = 0;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;
        }

        //public decimal ValidateAmountByBranch(short branchId)
        //{
        //    //var branch = this.context.tbl_Loan.FirstOrDefault(x => x.CustomerId == customerId).BranchId;
        //    //var outstandingbal = this.context.tbl_Loan.FirstOrDefault(x =>  x.BranchId== branchId).PrincipalAmount;
        //    var outstandingbal = from d in context.tbl_Loan
        //                         where d.BranchId == branchId
        //                         let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.OutstandingPrincipal)
        //                         select sumPrincipalAmount;
        //    var outstandingbalresult = outstandingbal.FirstOrDefault();

        //    var limitAmount = from a in context.tbl_Limit_Detail
        //                      join b in context.tbl_Limit on a.LimitId equals b.LimitId
        //                      where a.LimitTypeId == (int)LimitType.Branch && a.TargetId == branchId &&
        //                      b.LimitMetricId == (int)LimitMatricEnum.LoanAmount // &&
        //                                                                         // b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
        //                                                                         //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
        //                      select a.MaximumValue;  //).FirstOrDefault();

        //    var limitAmountresult = limitAmount.FirstOrDefault();

        //    decimal diff = (decimal)outstandingbalresult - limitAmountresult;


        //    return diff;
        //}

        //public decimal ValidateNPLByBranch(short branchId)
        //{
        //    var outstandingbal = from d in context.tbl_Loan
        //                         where d.BranchId == branchId
        //                         let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
        //                         select sumPrincipalAmount;
        //    var outstandingbalresult = outstandingbal.FirstOrDefault();

        //    var limitAmount = from a in context.tbl_Limit_Detail
        //                      join b in context.tbl_Limit on a.LimitId equals b.LimitId
        //                      where a.LimitTypeId == (int)LimitType.Branch && a.TargetId == branchId &&
        //                      b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan //&&
        //                                                                                //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
        //                                                                                //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
        //                                                                                //select maximumValue;
        //                      select a.MaximumValue;

        //    var limitAmountresult = limitAmount.FirstOrDefault();

        //    decimal diff = outstandingbalresult - limitAmountresult;





        //    return diff;
        //}

        public CreditLimitValidationsModel ValidateAmountBySector(int subSectorId)
        {
            var subSector = context.TBL_SUB_SECTOR.Find(subSectorId);
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var data = (from a in context.TBL_SECTOR
                        where a.SECTORID == subSector.SECTORID
                        let maximumLimit = a.LOAN_LIMIT
                        select maximumLimit).FirstOrDefault();

            var sector = context.TBL_SECTOR.Find(subSector.SECTORID);

            var totalExposure = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.TOTALEXPOSURELCY).FirstOrDefault();
            var sectorLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.SECTORLIMIT).FirstOrDefault();
            var exposureLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.EXPOSURES).FirstOrDefault();

            model.outstandingBalance = (double?)totalExposure ?? 0;
            model.sectorLimit = (double?)sectorLimit ?? 0;
            model.exposureLimit = (double?)exposureLimit ?? 0;

            if (data != null)
                model.limit = (double)data;
            model.difference = model.limit - model.outstandingBalance;

            return model;

        }


        public CreditLimitValidationsModel ValidateAmountFacilityBySector(int sectorId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var data = (from a in context.TBL_SECTOR
                        where a.SECTORID == sectorId
                        let maximumLimit = a.LOAN_LIMIT
                        select maximumLimit).FirstOrDefault();

            var sector = context.TBL_SECTOR.Find(sectorId);

            var totalExposure = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.TOTALEXPOSURELCY).FirstOrDefault();
            var sectorLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.SECTORLIMIT).FirstOrDefault();
            var exposureLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(g => g.CBNSECTORID == sector.CODE)?.Select(g => g.EXPOSURES).FirstOrDefault();

            model.outstandingBalance = (double?)totalExposure ?? 0;
            model.sectorLimit = (double?)sectorLimit ?? 0;
            model.exposureLimit = (double?)exposureLimit ?? 0;

            if (data != null)
                model.limit = (double)data;
            model.difference = model.limit - model.outstandingBalance;

            return model;

        }


        /*public CreditLimitValidationsModel ValidateAmountBySector(int sectorId)
        {

            var data = (from a in context.TBL_SECTOR
                        where a.SECTORID == sectorId
                        let maximumLimit = a.LOAN_LIMIT
                        select maximumLimit).FirstOrDefault();

            // var sector = context.TBL_SECTOR.Where(a => a.SECTORID == sectorId).FirstOrDefault();

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var loanOutstandingBalance = (from d in context.TBL_LOAN
                                          join f in context.TBL_SUB_SECTOR on d.SUBSECTORID equals f.SUBSECTORID
                                          where f.SECTORID == sectorId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                          select new
                                          {
                                              d.OUTSTANDINGPRINCIPAL
                                          }).ToList();
            var sumLoanOutstandingBalance = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL).Sum();

            var OverdraftOutstandingBalance = (from d in context.TBL_LOAN_REVOLVING
                                               join f in context.TBL_SUB_SECTOR on d.SUBSECTORID equals f.TBL_SECTOR.SECTORID
                                               where d.TBL_SUB_SECTOR.SECTORID == sectorId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                               select new
                                               {
                                                   d.OVERDRAFTLIMIT
                                               }).ToList();
            var sumOverdraftOutstandingBalance = OverdraftOutstandingBalance.Select(c => c.OVERDRAFTLIMIT).Sum();

            model.outstandingBalance = (double)(sumLoanOutstandingBalance + sumOverdraftOutstandingBalance);
            if (data != null)
                model.limit = (double)data;
            model.difference = model.limit - model.outstandingBalance;

            return model;

        }*/

        public CreditLimitValidationsModel ValidateAmountBySectorOld(int subSectorId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();


            short sectorId = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault().SECTORID.Value;


            var outstandingbal = (from a in context.TBL_LOAN
                                  join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                  where a.TBL_SUB_SECTOR.SECTORID == sectorId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select a.OUTSTANDINGPRINCIPAL).Sum();


            var limitAmount = 0;
            //(from a in context.TBL_LIMIT_DETAIL
            //                   join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                   where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == sectorId &&
            //                   b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
            //                   select a.MAXIMUMVALUE).Sum();

            model.outstandingBalance = (double)outstandingbal;
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal - (double)limitAmount;
            return model;
        }

        //public CreditLimitValidationsModel ValidateNPLBySector(int sectorId)
        //{
        //    CreditLimitValidationsModel model = new CreditLimitValidationsModel();

        //    var outstandingbal = from a in context.TBL_LOAN
        //                         join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
        //                         where a.SUBSECTORID == c.SUBSECTORID && a.LOANSTATUSID == (short)LoanStatusEnum.Active
        //                         let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.TBL_SUB_SECTOR.SECTORID == sectorId).Sum(x => x.OUTSTANDINGPRINCIPAL)
        //                         select sumPrincipalAmount;

        //    var limitAmount = 0;

        //    model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
        //    model.limit = (double)limitAmount;
        //    model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
        //    return model;
        //}

        /*public CreditLimitValidationsModel ValidateNPLBySector(int subSectorId)
        {
            var subSector = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault();//.SECTORID.Value;

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var principalAmountLoan = context.TBL_LOAN.Where(x => x.TBL_SUB_SECTOR.SECTORID == subSector.SECTORID && x.LOANSTATUSID == (short)LoanStatusEnum.Active).ToList();
            var sumPrincipalAmountLoan = principalAmountLoan.Sum(x => x.OUTSTANDINGPRINCIPAL * (decimal)x.EXCHANGERATE);
            var principalAmountRevolving = context.TBL_LOAN_REVOLVING.Where(x => x.TBL_SUB_SECTOR.SECTORID == subSector.SECTORID).ToList();
            var sumPrincipalAmountRevolving = principalAmountRevolving.Sum(x => x.OVERDRAFTLIMIT * (decimal)x.EXCHANGERATE);
            var principalAmountContingent = context.TBL_LOAN_CONTINGENT.Where(x => x.TBL_SUB_SECTOR.SECTORID == subSector.SECTORID).ToList();
            var sumPrincipalAmountContingent = principalAmountContingent.Sum(x => x.CONTINGENTAMOUNT * (decimal)x.EXCHANGERATE);
            var data = sumPrincipalAmountLoan + sumPrincipalAmountRevolving + sumPrincipalAmountContingent;

            var limitAmount = 0;
            var sector = context.TBL_SECTOR.FirstOrDefault(a => a.SECTORID == subSector.SECTORID);

            model.outstandingBalance = (double)data;

            model.limit = (double)limitAmount;
            model.difference = (double)data - (double)limitAmount;
            model.maximumAllowedLimit = (decimal?)sector.LOAN_LIMIT ?? 0;

            return model;
        }*/

        public CreditLimitValidationsModel ValidateNPLBySector(int subSectorId)
        {
            var subSector = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault();//.SECTORID.Value;
            if (subSector == null)
            {
                return null;
            }
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var sector = context.TBL_SECTOR.Find(subSector.SECTORID);
            var sectorsExposures = (from a in context.TBL_SECTOR_GLOBAL_LIMIT
                                    select a.TOTALEXPOSURELCY).Sum() ?? 0;

            var sectorExposure = context.TBL_SECTOR_GLOBAL_LIMIT.Where(a => a.CBNSECTORID.Trim() == sector.CODE.Trim())?.Select(a => a.TOTALEXPOSURELCY).FirstOrDefault() ?? 0;

            var CurrentSectorsExposures = sectorsExposures;
            var currentsectorExposure = sectorExposure;

            var totalExposure = currentsectorExposure / CurrentSectorsExposures;
            decimal percentageTotalExposure = decimal.Round((decimal)totalExposure, 5, MidpointRounding.AwayFromZero);
            var sectorLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(s => s.CBNSECTORID.Trim() == sector.CODE.Trim())?.Select(s => s.SECTORLIMIT).FirstOrDefault();

            model.outstandingBalance = (double)currentsectorExposure;
            model.outstandingSectorsBalance = (double)CurrentSectorsExposures;
            model.limit = (double)percentageTotalExposure;
            model.maximumAllowedLimit = (decimal?)sectorLimit ?? 0;

            return model;
        }

        public IEnumerable<SectorLimitViewModel> GetSectorLoanAmountLimit()
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var output = new List<SectorLimitViewModel>();

            //(from a in context.TBL_LIMIT_DETAIL
            //          join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //          join c in context.TBL_SECTOR on a.TARGETID equals c.SECTORID
            //          join d in context.TBL_LOAN on c.SECTORID equals d.TBL_SUB_SECTOR.SECTORID
            //          where a.LIMITTYPEID == (int)LimitType.Sector && d.LOANSTATUSID == (short)LoanStatusEnum.Active
            //          && b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
            //          group d by new { c.SECTORID, c.CODE, c.NAME, a.MAXIMUMVALUE } into groupedQ
            //          select new SectorLimitViewModel()
            //          {
            //              sectorId = groupedQ.Key.SECTORID,
            //              sectorCode = groupedQ.Key.CODE,
            //              sectorName = groupedQ.Key.NAME,
            //              sectorLimit = groupedQ.Key.MAXIMUMVALUE,
            //              sectorUsage = groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL),
            //              sectorBalance = groupedQ.Key.MAXIMUMVALUE - groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL)
            //          });

            return output;
        }

        public CreditLimitValidationsModel ValidateAmountByCustomer(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var data = from a in context.TBL_CUSTOMER
                       join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
                       join c in context.TBL_COMPANY on a.COMPANYID equals c.COMPANYID
                       where a.CUSTOMERID == customerId && a.DELETED == false
                       let maximumLimit = ((b.MAX_SHAREHOLDER_FUND_PERCENTAG / 100) * (double)c.SHAREHOLDERSFUND)
                       select maximumLimit;

            var loanOutstandingBalance = (from d in context.TBL_LOAN
                                          join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                                          join g in context.TBL_LOAN_APPLICATION_DETAIL on d.LOANAPPLICATIONDETAILID equals g.LOANAPPLICATIONDETAILID
                                          where d.LOANAPPLICATIONDETAILID == g.LOANAPPLICATIONDETAILID && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                          && g.STATUSID == (short)ApprovalStatusEnum.Approved
                                          select new
                                          {
                                              d.OUTSTANDINGPRINCIPAL,
                                              d.PRINCIPALAMOUNT,
                                          }).ToList();
            var sumLoanOutstandingBalance = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL).Sum();
            var sumLoanPrincipalAmount = loanOutstandingBalance.Select(c => c.PRINCIPALAMOUNT).Sum();

            var loanApprovedAmount = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                      join e in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                      where d.CUSTOMERID == customerId && d.STATUSID == (short)ApprovalStatusEnum.Approved
                                      && d.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID
                                      select new
                                      {
                                          d.APPROVEDAMOUNT
                                      }).ToList();
            var sumLoanApprovedAmount = loanApprovedAmount.Select(c => c.APPROVEDAMOUNT).Sum();

            var loanTotal = sumLoanApprovedAmount + sumLoanOutstandingBalance - sumLoanPrincipalAmount;


            var overDraftLimit = (from d in context.TBL_LOAN_REVOLVING
                                  join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                                  join g in context.TBL_LOAN_APPLICATION_DETAIL on d.LOANAPPLICATIONDETAILID equals g.LOANAPPLICATIONDETAILID
                                  where d.LOANAPPLICATIONDETAILID == g.LOANAPPLICATIONDETAILID && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  && g.STATUSID == (short)ApprovalStatusEnum.Approved
                                  select new
                                  {
                                      d.OVERDRAFTLIMIT,
                                  }).ToList();
            var sumOverDraftLimit = overDraftLimit.Select(c => c.OVERDRAFTLIMIT).Sum();

            var oDApprovedAmount = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                    join e in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                    where d.CUSTOMERID == customerId && d.STATUSID == (short)ApprovalStatusEnum.Approved
                                    && d.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID
                                    select new
                                    {
                                        d.APPROVEDAMOUNT
                                    }).ToList();
            var sumODApprovedAmount = oDApprovedAmount.Select(c => c.APPROVEDAMOUNT).Sum();

            var oDTotal = sumODApprovedAmount - sumOverDraftLimit;

            var customer = context.TBL_CUSTOMER.Find(customerId);
            model.outstandingBalance = (double)(loanTotal + oDTotal);
            model.limit = data.FirstOrDefault();
            model.difference = model.limit - model.outstandingBalance;
            model.riskRatingId = customer == null ? 0 : (short?)customer.RISKRATINGID;

            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByCustomer(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.CUSTOMERID == customerId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.CUSTOMERID == customerId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = 0;
            //from a in context.TBL_LIMIT_DETAIL
            //                  join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                  where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customerId &&
            //                  b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan //&&
            //                                                                            //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
            //                                                                            //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
            //                                                                            //select maximumValue;
            //                  select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByInsiderCustomer()
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var customerCodes = context.TBL_CUSTOMER.Where(c => c.ISREALATEDPARTY == true && c.DELETED == false).Select(c => c.CUSTOMERCODE).ToList();
            var limitAmount = 0;
            //var principalAmountLoan = context.TBL_LOAN.Where(x => x.TBL_CUSTOMER.ISREALATEDPARTY && x.LOANSTATUSID == (short)LoanStatusEnum.Active).ToList();
            //var sumPrincipalAmountLoan = principalAmountLoan.Sum(x => x.OUTSTANDINGPRINCIPAL * (decimal)x.EXCHANGERATE);
            //var principalAmountRevolving = context.TBL_LOAN_REVOLVING.Where(x => x.TBL_CUSTOMER.ISREALATEDPARTY && x.LOANSTATUSID == (short)LoanStatusEnum.Active).ToList();
            //var sumPrincipalAmountRevolving = principalAmountRevolving.Sum(x => x.OVERDRAFTLIMIT * (decimal)x.EXCHANGERATE);
            //var principalAmountContingent = context.TBL_LOAN_CONTINGENT.Where(x => x.TBL_CUSTOMER.ISREALATEDPARTY && x.LOANSTATUSID == (short)LoanStatusEnum.Active).ToList();
            //var sumPrincipalAmountContingent = principalAmountContingent.Sum(x => x.CONTINGENTAMOUNT * (decimal)x.EXCHANGERATE);
            //var data = sumPrincipalAmountLoan + sumPrincipalAmountRevolving + sumPrincipalAmountContingent;
            var exposures = GetGlobalCustomerExposure(customerCodes);
            var data = exposures.Sum(e => e.outstandings);
            var companyCapital = context.TBL_COMPANY.FirstOrDefault().SHAREHOLDERSFUND;
            double maxLimit = (float)companyCapital * 0.01;
            model.outstandingBalance = (double)data;
            model.limit = (double)limitAmount;
            model.difference = (double)data - (double)limitAmount;
            model.maximumAllowedLimit = (decimal?)maxLimit ?? 0;
            return model;
        }

        public bool ValidateIsInsiderCustomer(int customerId)
        {
            var isRelatedcustomer = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == customerId && c.ISREALATEDPARTY == true).Select(c => c.CUSTOMERCODE).FirstOrDefault();
            if (isRelatedcustomer != null)
                return true;
            return false;
        }

        public List<CurrentCustomerExposure> GetGlobalCustomerExposure(List<string> customerCodes)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            foreach (var customerCode in customerCodes)
            {
                customerCode.Trim();

                exposure = from a in context.TBL_GLOBAL_EXPOSURE
                           where a.CUSTOMERID.Contains(customerCode)
                           select new CurrentCustomerExposure
                           {
                               facilityType = a.ADJFACILITYTYPE,
                               existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                               proposedLimit = a.LOANAMOUNYLCY ?? 0,
                               outstandings = a.TOTALEXPOSURE ?? 0,
                               recommendedLimit = 0,
                               //PastDueObligationsInterest = a.PASTDUEINTEREST,
                               pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                               reviewDate = DateTime.Now,
                               loanStatus = a.CBNCLASSIFICATION,
                               referenceNumber = a.REFERENCENUMBER,
                           };

                if (exposure.Count() > 0) exposures.AddRange(exposure);
            }

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGlobalCustomerExposureByCurrency()
        {
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            exposures = (from a in context.TBL_GLOBAL_EXPOSURE
                         where a.ALPHACODE.ToUpper().Trim() != "NGN"
                         select new CurrentCustomerExposure
                         {
                             facilityType = a.ADJFACILITYTYPE,
                             existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                             proposedLimit = a.LOANAMOUNYLCY ?? 0,
                             outstandings = a.TOTALEXPOSURE ?? 0,
                             recommendedLimit = 0,
                             //PastDueObligationsInterest = a.PASTDUEINTEREST,
                             pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                             reviewDate = DateTime.Now,
                             loanStatus = a.CBNCLASSIFICATION,
                             referenceNumber = a.REFERENCENUMBER,
                         }).ToList();

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGlobalCustomerExposureByGroupFirstTwenty(List<string> customerCodes)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            foreach (var customerCode in customerCodes)
            {
                customerCode.Trim();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                facilityType = a.ADJFACILITYTYPE,
                                existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.TOTALEXPOSURE ?? 0,
                                recommendedLimit = 0,
                                //PastDueObligationsInterest = a.PASTDUEINTEREST,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).OrderByDescending(x => x.outstandings).ToList();

                if (exposure.Count() > 0) exposures.AddRange(exposure);
            }

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGlobalCustomerExposureByGroupFirstHundred(List<string> customerCodes)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            foreach (var customerCode in customerCodes)
            {
                customerCode.Trim();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                facilityType = a.ADJFACILITYTYPE,
                                existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.TOTALEXPOSURE ?? 0,
                                recommendedLimit = 0,
                                //PastDueObligationsInterest = a.PASTDUEINTEREST,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).OrderByDescending(x => x.outstandings).ToList();

                if (exposure.Count() > 0) exposures.AddRange(exposure);
            }

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupCustomerGlobalExposure(int customerGroupId)
        {
            var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                        where a.CUSTOMERGROUPID == customerGroupId && a.DELETED == false
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                            customerGroupId = a.CUSTOMERGROUPID,
                                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                                            relationshipTypeName = a.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                            customerId = a.CUSTOMERID,
                                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                                            customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME,
                                            customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                        }).ToList();
            var customerCodes = customerGroupMapping.Select(m => m.customerCode).ToList();
            var exposures = GetGlobalCustomerExposure(customerCodes);
            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupCustomerGlobalExposureByCurrency()
        {
            var exposures = GetGlobalCustomerExposureByCurrency();
            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupCustomerGlobalExposureByGroupFirstTwenty()
        {
            var customerGroupMapping = (from a in context.TBL_GLOBAL_EXPOSURE
                                        where a.CUSTOMERTYPE.Trim() == "C"
                                        orderby a.TOTALEXPOSURE descending
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerCode = a.CUSTOMERID,
                                        }).Distinct().ToList().Take(20);
            var customerCodes = customerGroupMapping.Select(m => m.customerCode).ToList();
            var exposures = GetGlobalCustomerExposureByGroupFirstTwenty(customerCodes);
            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupCustomerGlobalExposureByGroupFirstHundred()
        {
            var customerGroupMapping = (from a in context.TBL_GLOBAL_EXPOSURE
                                        where a.CUSTOMERTYPE.Trim() == "C"
                                        orderby a.TOTALEXPOSURE descending
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerCode = a.CUSTOMERID,
                                        }).Distinct().ToList().Take(100);
            var customerCodes = customerGroupMapping.Select(m => m.customerCode).ToList();
            var exposures = GetGlobalCustomerExposureByGroupFirstHundred(customerCodes);
            return exposures;
        }

        public CreditLimitValidationsModel ValidateSingleObligorLimit(LoanApplicationViewModel application)
        {
            List<CurrentCustomerExposure> exposures;
            CreditLimitValidationsModel models = new CreditLimitValidationsModel();
            var company = context.TBL_COMPANY.FirstOrDefault(c => c.COMPANYID == application.companyId);
            var globalLimit = company.SINGLEOBLIGORLIMIT;
            if (!(globalLimit > 0) || globalLimit == null)
            {
                throw new SecureException("Single Obligor Limit has not been setup for" + company.NAME + " !");
            }
            if (application.loanTypeId == (int)LoanTypeEnum.Single)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == application.customerId).CUSTOMERCODE;
                var customerCodes = new List<string>();
                customerCodes.Add(customerCode);
                exposures = GetGlobalCustomerExposure(customerCodes);
            }
            else
            {
                exposures = GetGroupCustomerGlobalExposure((int)application.customerGroupId);
            }

            models.maximumAllowedLimit = (decimal?)globalLimit ?? 0;
            models.outstandingBalance = exposures.Sum(e => (double)e.outstandings);
            return models;
        }


        public CreditLimitValidationsModel ValidateNPLByDirectors(LoanApplicationViewModel application)
        {
            List<CurrentCustomerExposure> exposures;
            if (application.loanTypeId == (int)LoanTypeEnum.Single)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == application.customerId).CUSTOMERCODE;
                var customerCodes = new List<string>();
                customerCodes.Add(customerCode);
                exposures = GetGlobalCustomerExposure(customerCodes);
            }
            else
            {
                exposures = GetGroupCustomerGlobalExposure((int)application.customerGroupId);
            }
            CreditLimitValidationsModel models = new CreditLimitValidationsModel();
            var companyCapital = context.TBL_COMPANY.FirstOrDefault().SHAREHOLDERSFUND;
            double maxLimit = (float)companyCapital * 0.01;
            models.maximumAllowedLimit = (decimal?)maxLimit ?? 0;
            models.outstandingBalance = exposures.Sum(e => (double)e.outstandings);
            return models;
        }

        public CreditLimitValidationsModel ValidateNPLByCurrency(LoanApplicationViewModel application)
        {
            CreditLimitValidationsModel models = new CreditLimitValidationsModel();
            List<CurrentCustomerExposure> exposures;
            exposures = GetGlobalCustomerExposureByCurrency();
            var currencyLimit = context.TBL_CURRENCY_LIMIT.Where(x => x.DELETED == false).FirstOrDefault();
            if (currencyLimit == null)
            {
                return null;
            }
            double maxLimit = (float?)currencyLimit?.CURRENCYLIMITVALUE ?? 0;
            models.maximumAllowedLimit = (decimal?)maxLimit ?? 0;
            models.outstandingBalance = exposures.Sum(e => (double)e.outstandings);

            return models;
        }

        public CreditLimitValidationsModel ValidateNPLByGroupFirstTwenty(LoanApplicationViewModel application)
        {
            List<CurrentCustomerExposure> exposures;
            CreditLimitValidationsModel models = new CreditLimitValidationsModel();
            if (application.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                exposures = GetGroupCustomerGlobalExposureByGroupFirstTwenty();
                var groupLimit = context.TBL_GROUP_LIMIT.Where(x => x.DELETED == false && x.LIMITNUMBER == 20).FirstOrDefault();
                double maxLimit = (float?)groupLimit?.GROUPLIMITVALUE ?? 0;
                models.maximumAllowedLimit = (decimal?)maxLimit ?? 0;
                models.outstandingBalance = exposures.Sum(e => (double)e.outstandings);
            }
            return models;
        }

        public CreditLimitValidationsModel ValidateNPLByGroupFirstHundred(LoanApplicationViewModel application)
        {
            List<CurrentCustomerExposure> exposures;
            CreditLimitValidationsModel models = new CreditLimitValidationsModel();
            if (application.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                exposures = GetGroupCustomerGlobalExposureByGroupFirstHundred();
                var groupLimit = context.TBL_GROUP_LIMIT.Where(x => x.DELETED == false && x.LIMITNUMBER == 100).FirstOrDefault();
                double maxLimit = (float?)groupLimit?.GROUPLIMITVALUE ?? 0;
                models.maximumAllowedLimit = (decimal?)maxLimit ?? 0;
                models.outstandingBalance = exposures.Sum(e => (double)e.outstandings);
            }
            return models;
        }

        public bool IsDirectorRelatedGroup(int? customerGroupId)
        {

            var customerIds = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == customerGroupId)?.Select(x => x.CUSTOMERID).ToList();
            if (customerIds.Count() <= 0) return false;
            foreach (var id in customerIds)
            {
                var bvn = context.TBL_CUSTOMER.Find(id)?.CUSTOMERBVN;

                var isDirector = context.TBL_COMPANY_DIRECTOR.Any(d => d.BVN.Trim() == bvn.Trim());

                if (isDirector) return isDirector;
            }
            return false;
        }

        public bool CustomerIsDirector(int? customerId)
        {
            if (customerId == null || customerId == 0)
            {
                return false;
            }
            var bvn = context.TBL_CUSTOMER.Find(customerId).CUSTOMERBVN;
            if (String.IsNullOrEmpty(bvn) || String.IsNullOrEmpty(bvn)) return false;
            var isDirector = context.TBL_COMPANY_DIRECTOR.Any(d => d.BVN.Trim() == bvn.Trim());
            if (isDirector) return isDirector;
            return false;
        }


        public CreditLimitValidationsModel ValidateAmountByCustomerGroup(int customergroupId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var customer = this.context.TBL_LOAN.FirstOrDefault(x => x.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).CUSTOMERID;
            var limitAmount = 0;
            //from a in context.TBL_LIMIT_DETAIL
            //                  join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                  where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customer &&
            //                  b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount //&&
            //                                                                     //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
            //                                                                     //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
            //                                                                     //select maximumValue;
            //                  select a.MAXIMUMVALUE;
            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByCustomerGroup(int customergroupId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var customer = this.context.TBL_LOAN.FirstOrDefault(x => x.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).CUSTOMERID;
            var limitAmount = 0;
            //from a in context.TBL_LIMIT_DETAIL
            //                  join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
            //                  where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customer &&
            //                  b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan //&&
            //                                                                            //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
            //                                                                            //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
            //                                                                            //select maximumValue;
            //                  select a.MAXIMUMVALUE;

            var limitAmountresult = limitAmount;
            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;
        }

        public CreditLimitValidationsModel ValidateCreditLimitNPLByRMBM(short relationshipofficerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.RELATIONSHIPOFFICERID == relationshipofficerId || d.RELATIONSHIPMANAGERID == relationshipofficerId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.RELATIONSHIPOFFICERID == relationshipofficerId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = this.context.TBL_STAFF.Where(a => a.STAFFID == relationshipofficerId).FirstOrDefault().LOAN_LIMIT;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount;
            return model;


        }

        public CreditLimitValidationsModel ValidateCreditLimitByRMBM(short staffId)
        {
            //CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            // limitAmount = this.context.TBL_STAFF.Where(a => a.STAFFID == relationshipofficerId).FirstOrDefault().LOAN_LIMIT;

            decimal? accountOfficerMaximumNPLExposure = 0;

            var outstandingLoan = (from d in context.TBL_LOAN
                                   where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                   (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                   select (decimal?)d.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

            var outstandingRevolving = (from d in context.TBL_LOAN_REVOLVING
                                        where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                        (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                        select (decimal?)d.OVERDRAFTLIMIT).Sum() ?? 0;

            var accountOfficerNPLExposure = outstandingLoan + outstandingRevolving;

            var officer = context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == staffId);
            if (officer != null) accountOfficerMaximumNPLExposure = officer.NPL_LIMIT ?? 0;

            var accountOfficerNPLLimit = accountOfficerMaximumNPLExposure - accountOfficerNPLExposure;


            var initiated = (from a in context.TBL_LOAN_APPLICATION
                             join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                             where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE == null
                             select (decimal?)d.APPROVEDAMOUNT).Sum() ?? 0;

            var approved = (from a in context.TBL_LOAN_APPLICATION
                            join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                            where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE != null &&
                            (a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestCompleted &&
                            a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestInitiated)
                            select (decimal?)d.APPROVEDAMOUNT).Sum() ?? 0;

            return new CreditLimitValidationsModel
            {
                initiated = initiated,
                approved = approved,
                limit = (double)accountOfficerNPLLimit < 0 ? 0 : (double)accountOfficerNPLLimit,
                limitString = (accountOfficerMaximumNPLExposure == 0) ? "No limit" : string.Format("{0:#,0.00}", accountOfficerNPLLimit),
                nplExposure = accountOfficerNPLExposure,
                maximumAllowedLimit = (decimal)accountOfficerMaximumNPLExposure
            };
        }

        public IEnumerable<ObligorLimitViewModel> GetAllObligorLimit()
        {
            var risk = (from a in context.TBL_CUSTOMER_RISK_RATING
                        select new ObligorLimitViewModel
                        {
                            riskRating = a.RISKRATING,
                            riskRatingId = a.RISKRATINGID,
                            companyId = a.COMPANYID,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            maxShareholderPercentage = a.MAX_SHAREHOLDER_FUND_PERCENTAG,
                            description = a.DESCRIPTION
                        }).ToList();

            return risk;
        }

        public bool AddUpdateRiskRating(ObligorLimitViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CUSTOMER_RISK_RATING riskRating;
                    if (entity.riskRatingId > 0)
                    {
                        riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(entity.riskRatingId);
                        if (entity != null)
                        {
                            riskRating.RISKRATING = entity.riskRating;
                            riskRating.DESCRIPTION = entity.description;
                            riskRating.MAX_SHAREHOLDER_FUND_PERCENTAG = entity.maxShareholderPercentage;
                            riskRating.COMPANYID = entity.companyId;
                            riskRating.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                        }

                    }
                    else
                    {
                        riskRating = new TBL_CUSTOMER_RISK_RATING
                        {
                            RISKRATING = entity.riskRating,
                            DESCRIPTION = entity.description,
                            MAX_SHAREHOLDER_FUND_PERCENTAG = entity.maxShareholderPercentage,
                            COMPANYID = entity.companyId,
                            ISINVESTMENTGRADE = entity.isInvestmentGrade
                        };
                    }
                    context.TBL_CUSTOMER_RISK_RATING.Add(riskRating);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }


        public IEnumerable<CurrencyLimitViewModel> GetAllCurrencyLimit()
        {
            var currencyLimits = (from a in context.TBL_CURRENCY_LIMIT
                                  where a.DELETED == false
                                  select new CurrencyLimitViewModel
                                  {
                                      currencyLimitId = a.CURRENCYLIMITID,
                                      currencyLimitName = a.CURRENCYLIMITNAME,
                                      currencyLimitValue = a.CURRENCYLIMITVALUE,
                                      description = a.DESCRIPTION
                                  })?.ToList();

            return currencyLimits;
        }

        public bool AddCurrencyLimits(CurrencyLimitViewModel entity)
        {
            if (entity != null)
            {
                //var limitExist = context.TBL_CURRENCY_LIMIT.FirstOrDefault(x => x.CURRENCYID == entity.currencyId);
                //if (limitExist != null)
                //{
                //    throw new SecureException("Currency Limit setup already exist");
                //}
                try
                {
                    TBL_CURRENCY_LIMIT currencyLimit;
                    currencyLimit = new TBL_CURRENCY_LIMIT
                    {
                        CURRENCYLIMITNAME = entity.currencyLimitName,
                        DESCRIPTION = entity.description,
                        CURRENCYLIMITVALUE = entity.currencyLimitValue,
                        DELETED = false,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now
                    };

                    context.TBL_CURRENCY_LIMIT.Add(currencyLimit);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateCurrencyLimits(CurrencyLimitViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CURRENCY_LIMIT currencyLimit;
                    if (entity.currencyLimitId > 0)
                    {
                        currencyLimit = context.TBL_CURRENCY_LIMIT.Find(entity.currencyLimitId);
                        if (currencyLimit != null)
                        {
                            currencyLimit.CURRENCYLIMITNAME = entity.currencyLimitName;
                            currencyLimit.DESCRIPTION = entity.description;
                            currencyLimit.CURRENCYLIMITVALUE = entity.currencyLimitValue;
                            currencyLimit.LASTUPDATEDBY = entity.createdBy;
                            currencyLimit.DATETIMEUPDATED = DateTime.Now;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool DeleteCurrencyLimit(int id, UserInfo user)
        {
            var response = 0;
            var currencyLimit = context.TBL_CURRENCY_LIMIT.Find(id);

            if (currencyLimit != null)
            {
                currencyLimit.DELETED = true;
                currencyLimit.DELETEDBY = user.staffId;
                currencyLimit.DATETIMEDELETED = DateTime.Now;
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public IEnumerable<GroupLimitViewModel> GetAllGroupLimit()
        {
            var groupLimits = (from a in context.TBL_GROUP_LIMIT where a.DELETED == false
                               select new GroupLimitViewModel
                               {
                                   groupLimitId = a.GROUPLIMITID,
                                   groupLimitValue = a.GROUPLIMITVALUE,
                                   groupName = a.GROUPNAME,
                                   description = a.DESCRIPTION,
                                   limitNumber = a.LIMITNUMBER
                               }).ToList();

            return groupLimits;
        }

        public bool AddGroupLimits(GroupLimitViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_GROUP_LIMIT groupLimit;
                    groupLimit = new TBL_GROUP_LIMIT
                    {
                        GROUPNAME = entity.groupName,
                        LIMITNUMBER = entity.limitNumber,
                        DESCRIPTION = entity.description,
                        GROUPLIMITVALUE = entity.groupLimitValue,
                        DELETED = false,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now
                    };
                    context.TBL_GROUP_LIMIT.Add(groupLimit);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateGroupLimits(GroupLimitViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_GROUP_LIMIT groupLimit;
                    if (entity.groupLimitId > 0)
                    {
                        groupLimit = context.TBL_GROUP_LIMIT.Find(entity.groupLimitId);
                        if (groupLimit != null)
                        {
                            groupLimit.GROUPLIMITVALUE = entity.groupLimitValue;
                            groupLimit.DESCRIPTION = entity.description;
                            groupLimit.GROUPNAME = entity.groupName;
                            groupLimit.LASTUPDATEDBY = entity.createdBy;
                            groupLimit.DATETIMEUPDATED = DateTime.Now;
                            groupLimit.LIMITNUMBER = entity.limitNumber;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool DeleteGroupLimit(int id, UserInfo user)
        {
            var response = 0;
            var groupLimit = context.TBL_GROUP_LIMIT.Find(id);

            if (groupLimit != null)
            {
                groupLimit.DELETED = true;
                groupLimit.DELETEDBY = user.staffId;
                groupLimit.DATETIMEDELETED = DateTime.Now;
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public bool DeleteRiskRating(int id, UserInfo user)
        {
            var response = 0;
            var rating = context.TBL_CUSTOMER_RISK_RATING.Find(id);

            if (rating != null)
            {
                context.TBL_CUSTOMER_RISK_RATING.Remove(rating);


                //// Audit Section ---------------------------
                //var audit = new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.ratin,
                //    STAFFID = user.staffId,
                //    BRANCHID = (short)user.BranchId,
                //    DETAIL = $"Deleted sector: '{rating.ToString()} ",
                //    IPADDRESS = user.userIPAddress,
                //    URL = user.applicationUrl,
                //    APPLICATIONDATE = GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now
                //};
                ////end of Audit section -------------------------------
                response = context.SaveChanges();
            }

            return response != 0;
        }

        public bool ValidateRiskRating(string riskRating)
        {
            var isExist = (from x in context.TBL_CUSTOMER_RISK_RATING where x.RISKRATING == riskRating select x).ToList();
            if (isExist.Any())
            {
                return true;
            }
            return false;
        }

        public bool UpdateCustomerRating(ObligorLimitViewModel entity)
        {
            var customer = context.TBL_CUSTOMER.Find(entity.customerId);
            customer.RISKRATINGID = (short?)entity.riskRatingId;
            return context.SaveChanges() > 0;
        }

        public bool UpdateApplicationCustomerRating(ObligorLimitViewModel entity)
        {
            if (entity.scenerio == 1)
            {
                var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == entity.applicationId);
                appl.RISKRATINGID = (short?)entity.riskRatingId;
            }

            if (entity.scenerio == 2)
            {
                var lmra = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == entity.applicationId);
                lmra.RISKRATINGID = (short?)entity.riskRatingId;
            }

            if (entity.customerId != null)
            {
                var singleCustomer = context.TBL_CUSTOMER.Find(entity.customerId);
                singleCustomer.RISKRATINGID = (short?)entity.riskRatingId;
            }

            if (entity.customerGroupId != null)
            {
                var groupCustomer = context.TBL_CUSTOMER_GROUP.Find(entity.customerGroupId);
                groupCustomer.RISKRATINGID = (short?)entity.riskRatingId;
            }

            //try
            //{
            //    var test = context.SaveChanges() > 0;
            //}
            //catch (Exception ex)
            //{
            //    var err = ex;
            //}
            return context.SaveChanges() > 0;
        }

        public CreditLimitValidationsModel ValidateApplicationCustomerRating(ObligorLimitViewModel entity)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            double approvedAmount = 0;

            if (entity.scenerio == 1)
            {
                var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == entity.applicationId);
                approvedAmount = appl?.APPROVEDAMOUNT != null ? (double)appl?.APPROVEDAMOUNT : (double)0;
            }

            if (entity.scenerio == 2)
            {
                var lmar = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == entity.applicationId && x.CUSTOMERID == entity.customerId);
                approvedAmount = (double)lmar.Sum(x => x.APPROVEDAMOUNT);
            }

            if (entity.customerId != null)
            {
                var data = from a in context.TBL_CUSTOMER
                           join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
                           join c in context.TBL_COMPANY on a.COMPANYID equals c.COMPANYID
                           where a.CUSTOMERID == entity.customerId && a.DELETED == false
                           let maximumLimit = ((b.MAX_SHAREHOLDER_FUND_PERCENTAG / 100) * (double)c.SHAREHOLDERSFUND)
                           select maximumLimit;

                var loanOutstandingBalance = (from d in context.TBL_LOAN
                                              join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                                              join g in context.TBL_LOAN_APPLICATION_DETAIL on d.LOANAPPLICATIONDETAILID equals g.LOANAPPLICATIONDETAILID
                                              where d.LOANAPPLICATIONDETAILID == g.LOANAPPLICATIONDETAILID && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                              && g.STATUSID == (short)ApprovalStatusEnum.Approved
                                              select new
                                              {
                                                  d.OUTSTANDINGPRINCIPAL,
                                                  d.PRINCIPALAMOUNT,
                                              }).ToList();
                var sumLoanOutstandingBalance = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL).Sum();
                var sumLoanPrincipalAmount = loanOutstandingBalance.Select(c => c.PRINCIPALAMOUNT).Sum();

                var loanApprovedAmount = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                          join e in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                          where d.CUSTOMERID == entity.customerId && d.STATUSID == (short)ApprovalStatusEnum.Approved
                                          && d.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID
                                          select new
                                          {
                                              d.APPROVEDAMOUNT
                                          }).ToList();
                var sumLoanApprovedAmount = loanApprovedAmount.Select(c => c.APPROVEDAMOUNT).Sum();

                var loanTotal = sumLoanApprovedAmount + sumLoanOutstandingBalance - sumLoanPrincipalAmount;


                var overDraftLimit = (from d in context.TBL_LOAN_REVOLVING
                                      join f in context.TBL_CUSTOMER on d.CUSTOMERID equals f.CUSTOMERID
                                      join g in context.TBL_LOAN_APPLICATION_DETAIL on d.LOANAPPLICATIONDETAILID equals g.LOANAPPLICATIONDETAILID
                                      where d.LOANAPPLICATIONDETAILID == g.LOANAPPLICATIONDETAILID && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      && g.STATUSID == (short)ApprovalStatusEnum.Approved
                                      select new
                                      {
                                          d.OVERDRAFTLIMIT,
                                      }).ToList();
                var sumOverDraftLimit = overDraftLimit.Select(c => c.OVERDRAFTLIMIT).Sum();

                var oDApprovedAmount = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                        join e in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                        where d.CUSTOMERID == entity.customerId && d.STATUSID == (short)ApprovalStatusEnum.Approved
                                        && d.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID
                                        select new
                                        {
                                            d.APPROVEDAMOUNT
                                        }).ToList();

                var sumODApprovedAmount = oDApprovedAmount.Select(c => c.APPROVEDAMOUNT).Sum();
                var oDTotal = sumODApprovedAmount - sumOverDraftLimit;
                var customer = context.TBL_CUSTOMER.Find(entity.customerId);

                model.riskRatingId = customer == null ? 0 : (short?)customer.RISKRATINGID;
                model.outstandingBalance = (double)(loanTotal + oDTotal);
                model.limit = data.FirstOrDefault();
            }

            model.difference = model.limit - model.outstandingBalance;
            model.validated = model.limit == 0 ? true : ((double)approvedAmount + model.outstandingBalance) <= model.outstandingBalance;
            model.obligorExposure = model.outstandingBalance;
            model.maximumAllowedLimit = (decimal)model.difference;

            return model;
        }

        public TotalExposureLimit GetTotalExposureLimit(ExposureLimitRequestModel model)
        {
            int? branchId = model.branchId;
            int? sectorId = model.sectorId;
            int? staffId = model.staffId;
            int? customerId = model.customerId;
            int? customerGroupId = model.customerGroupId;

            decimal? outstandingLoan = 0;
            decimal? outstandingRevolving = 0;

            double shareHoldersFund = (double)context.TBL_COMPANY.Find(model.companyId).SHAREHOLDERSFUND;

            TotalExposureLimit result = new TotalExposureLimit();

            if (model.applicationId != null)
            {
                var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);
                if (appl != null)
                {
                    if (branchId == null) branchId = appl.BRANCHID;
                    if (customerId == null) customerId = appl.CUSTOMERID;
                    if (customerGroupId == null) customerGroupId = appl.CUSTOMERGROUPID;
                    if (sectorId == null) sectorId = appl.TBL_CUSTOMER?.SUBSECTORID;
                    if (staffId == null) staffId = appl.RELATIONSHIPOFFICERID;
                }
            }

            // relationship officer (npl)

            if (staffId != null)
            {
                outstandingLoan = (from d in context.TBL_LOAN
                                   where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                   (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                   select (decimal?)d.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

                outstandingRevolving = (from d in context.TBL_LOAN_REVOLVING
                                        where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                        (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                        select (decimal?)d.OVERDRAFTLIMIT).Sum() ?? 0;

                result.AccountOfficerNPLExposure = outstandingLoan + outstandingRevolving;

                var officer = context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == staffId);
                if (officer != null) result.AccountOfficerMaximumNPLExposure = officer.NPL_LIMIT ?? 0;

                result.AccountOfficerNPLLimit = result.AccountOfficerMaximumNPLExposure - result.AccountOfficerNPLExposure;
            }

            // branch (npl)

            if (branchId != null)
            {
                outstandingLoan = (from a in context.TBL_LOAN
                                   where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.BRANCHID == branchId &&
                                   a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                   select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

                outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
                                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.BRANCHID == branchId &&
                                        a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                        select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

                result.BranchNPLExposure = outstandingLoan + outstandingRevolving;

                var branch = context.TBL_BRANCH.FirstOrDefault(a => a.BRANCHID == branchId);
                if (branch != null) result.BranchMaximumNPLExposure = branch.NPL_LIMIT;

                result.BranchNPLLimit = result.BranchMaximumNPLExposure - result.BranchNPLExposure;
            }

            var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.DELETED == false && x.STATUSID == (int)ApprovalStatusEnum.Approved && x.LOANAPPLICATIONID == model.applicationId).ToList();

            List<RequestExposureLimit> limits = new List<RequestExposureLimit>();

            foreach (var facility in facilities)
            {
                sectorId = facility.SUBSECTORID;
                customerId = facility.CUSTOMERID;

                var limit = new RequestExposureLimit();

                limit.productCustomerName = facility.TBL_PRODUCT.PRODUCTNAME + " -- " + facility.TBL_CUSTOMER.FIRSTNAME + " " + facility.TBL_CUSTOMER.MIDDLENAME + " " + facility.TBL_CUSTOMER.LASTNAME;


                // sector

                if (sectorId != null)
                {
                    outstandingLoan = (from a in context.TBL_LOAN
                                       join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                       where a.LOANSTATUSID == (short)LoanStatusEnum.Active && c.SUBSECTORID == sectorId
                                       select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

                    outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
                                            join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                            where a.LOANSTATUSID == (short)LoanStatusEnum.Active && c.SUBSECTORID == sectorId
                                            select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

                    limit.SectorExposure = outstandingLoan + outstandingRevolving;

                    var sector = context.TBL_SUB_SECTOR.FirstOrDefault(a => a.SECTORID == sectorId);
                    if (sector != null) limit.SectorMaximumExposure = sector.TBL_SECTOR.LOAN_LIMIT ?? 0;

                    limit.SectorLimit = limit.SectorMaximumExposure - limit.SectorExposure;
                }

                // customer

                if (customerId != null)
                {
                    outstandingLoan = (from a in context.TBL_LOAN
                                       where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.CUSTOMERID == customerId
                                       select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

                    outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
                                            where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.CUSTOMERID == customerId
                                            select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

                    limit.ObligorExposure = outstandingLoan + outstandingRevolving;

                    var bank = (from a in context.TBL_CUSTOMER
                                join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
                                where a.CUSTOMERID == customerId && a.DELETED == false
                                select b
                                                     ).FirstOrDefault();
                    if (bank != null) limit.ObligorMaximumExposure = (decimal)((bank.MAX_SHAREHOLDER_FUND_PERCENTAG / 100) * shareHoldersFund);

                    limit.ObligorLimit = limit.ObligorMaximumExposure - limit.ObligorExposure;
                }

                limits.Add(limit);

            }

            result.RequestExposureLimits = limits;

            // customer group

            //if (customerGroupId != null)
            //{
            //    outstandingLoan = (from a in context.TBL_LOAN
            //                       where a.LOANSTATUSID == (short)LoanStatusEnum.Active &&
            //                       a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customerGroupId
            //                       select a.OUTSTANDINGPRINCIPAL).Sum();

            //    outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
            //                            where a.LOANSTATUSID == (short)LoanStatusEnum.Active &&
            //                            a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customerGroupId
            //                            select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

            //    result.ObligorExposure = outstandingLoan + outstandingRevolving;

            //    var bank = (from a in context.TBL_CUSTOMER_GROUP
            //                join b in context.TBL_CUSTOMER_RISK_RATING on a.RISKRATINGID equals b.RISKRATINGID
            //                where a.CUSTOMERGROUPID == customerId && a.DELETED == false
            //                select b
            //                                     ).FirstOrDefault();
            //    if (bank != null) result.ObligorMaximumExposure = (decimal)((bank.MAX_SHAREHOLDER_FUND_PERCENTAG / 100) * shareHoldersFund);

            //    result.ObligorLimit = result.ObligorMaximumExposure - result.ObligorExposure;
            //}

            // pipeline

            var initiated = (from a in context.TBL_LOAN_APPLICATION
                             join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                             where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE == null
                             select (decimal?)d.APPROVEDAMOUNT).Sum() ?? 0;

            var approved = (from a in context.TBL_LOAN_APPLICATION
                            join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                            where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE != null &&
                            (a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestCompleted &&
                            a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestInitiated)
                            select (decimal?)d.APPROVEDAMOUNT).Sum() ?? 0;

            result.InitiatedLoansBalance = initiated;

            result.UndisbursedApprovedLoansBalance = approved;

            return result;
        }

        public bool BranchLimitExceeded(int branchId, decimal applicationAmount)
        {
            var outstandingLoan = (from a in context.TBL_LOAN
                                   where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.BRANCHID == branchId &&
                                   a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                   select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

            var outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
                                        where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.BRANCHID == branchId &&
                                        a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                        select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

            var branchNPLExposure = outstandingLoan + outstandingRevolving;

            var branch = context.TBL_BRANCH.FirstOrDefault(a => a.BRANCHID == branchId);
            var branchMaximumNPLExposure = branch.NPL_LIMIT;

            var branchNPLLimit = branchMaximumNPLExposure - branchNPLExposure;

            return branchMaximumNPLExposure > 0 && branchNPLLimit < applicationAmount;
        }

        //public bool SectorLimitExceeded(int sectorId, decimal applicationAmount)
        //{
        //    var sectorCode = context.TBL_SECTOR.Find(sectorId);
        //    var sectorsExposures = (from a in context.TBL_SECTOR_GLOBAL_LIMIT
        //                                   select (decimal?)a.TOTALEXPOSURELCY).Sum() ?? 0;
        //    var sectorExposure = context.TBL_SECTOR_GLOBAL_LIMIT.Where(a=>a.CBNSECTORID == sectorCode.CODE).Select(a=>a.TOTALEXPOSURELCY).FirstOrDefault() ?? 0;

        //    var CurrentSectorsExposures = sectorsExposures + applicationAmount;
        //    var currentsectorExposure = (decimal)sectorExposure + applicationAmount;

        //    var totalExposure = currentsectorExposure / CurrentSectorsExposures;
        //    decimal percentageTotalExposure =  decimal.Round(totalExposure, 2, MidpointRounding.AwayFromZero);

        //    var sectorLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(s=>s.CBNSECTORID == sectorCode.CODE).Select(s=>s.SECTORLIMIT).FirstOrDefault();

        //    return sectorLimit < percentageTotalExposure;
        //}

        public bool SectorLimitExceeded(int sectorId, decimal applicationAmount)
        {

            var subSector = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == sectorId).FirstOrDefault();
            var sectorCode = context.TBL_SECTOR.Find(subSector.SECTORID);
            var sectorsExposures = (from a in context.TBL_SECTOR_GLOBAL_LIMIT
                                    select a.TOTALEXPOSURELCY).Sum() ?? 0;

            var sectorExposure = context.TBL_SECTOR_GLOBAL_LIMIT.Where(a => a.CBNSECTORID == sectorCode.CODE).Select(a => a.TOTALEXPOSURELCY).FirstOrDefault() ?? 0;

            var CurrentSectorsExposures = sectorsExposures + applicationAmount;
            var currentsectorExposure = sectorExposure + applicationAmount;

            var totalExposure = currentsectorExposure / CurrentSectorsExposures;
            decimal percentageTotalExposure = decimal.Round((decimal)totalExposure, 5, MidpointRounding.AwayFromZero);
            var sectorLimit = context.TBL_SECTOR_GLOBAL_LIMIT.Where(s => s.CBNSECTORID == sectorCode.CODE).Select(s => s.SECTORLIMIT).FirstOrDefault();
            return percentageTotalExposure > 0 && sectorLimit < percentageTotalExposure;

            /* var outstandingLoan = (from a in context.TBL_LOAN
                               join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                               where a.LOANSTATUSID == (short)LoanStatusEnum.Active && c.SUBSECTORID == sectorId
                               select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;
            var outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
                                    join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                    where a.LOANSTATUSID == (short)LoanStatusEnum.Active && c.SUBSECTORID == sectorId
                                    select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;
            var sectorExposure = outstandingLoan + outstandingRevolving;
            decimal? sectorMaximumExposure = 0;
            var sector = context.TBL_SUB_SECTOR.FirstOrDefault(a => a.SECTORID == sectorId);
            if (sector != null) sectorMaximumExposure = sector.TBL_SECTOR.LOAN_LIMIT ?? 0;
            var sectorLimit = sectorMaximumExposure - sectorExposure;
            return sectorMaximumExposure > 0 && sectorLimit < applicationAmount;
            */
        }

        public bool ProductLimitExceeded(int productId, decimal applicationAmount)
        {
            //var outstandingLoan = (from a in context.TBL_LOAN
            //                       join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
            //                       where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == productId
            //                       select (decimal?)a.OUTSTANDINGPRINCIPAL).Sum() ?? 0;

            //var outstandingRevolving = (from a in context.TBL_LOAN_REVOLVING
            //                            join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
            //                            where a.LOANSTATUSID == (short)LoanStatusEnum.Active && a.PRODUCTID == productId
            //                            select (decimal?)a.OVERDRAFTLIMIT).Sum() ?? 0;

            //var productExposure = outstandingLoan + outstandingRevolving;

            //decimal? productMaximumExposure = 0;
            //var product =context.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault(a => a.PRODUCTID == productId);
            //if (product != null) productMaximumExposure = (decimal?)product.PRODUCT_LIMIT ?? 0;

            //var productLimit = productMaximumExposure - productExposure;

            //return productMaximumExposure > 0 && productLimit <= applicationAmount;
            if (productId == 0)
            {
                return false;
            }
            decimal productMaximumExposure = 0;
            var product = context.TBL_PRODUCT_BEHAVIOUR.FirstOrDefault(a => a.PRODUCTID == productId);
            if (product != null) productMaximumExposure = (decimal?)product.PRODUCT_LIMIT ?? 0;
            if (productMaximumExposure <= 0)
            {
                return false;
            }

            return applicationAmount > productMaximumExposure;
        }

        public TotalExposureLimit GetTotalExposureLimitReference(string reference, int companyId)
        {
            var appl = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.APPLICATIONREFERENCENUMBER == reference);

            if (appl == null) return new TotalExposureLimit();

            return GetTotalExposureLimit(new ExposureLimitRequestModel {
                companyId = companyId,
                applicationId = appl.LOANAPPLICATIONID,
            });
        }

        public IEnumerable<ProjectRiskRatingCategoryViewModel> getAllProjectRiskRatingCategories()
        {
            var categories = (from a in context.TBL_PROJECT_RISK_RATING_CATEGORY
                              select new ProjectRiskRatingCategoryViewModel
                              {
                                  categoryId = a.CATEGORYID,
                                  categoryName = a.CATEGORYNAME
                              }).ToList();

            return categories;
        }

        public IEnumerable<ContractorCriteriaViewModel> getAllCriteriaList()
        {
            var categories = (from a in context.TBL_CONTRACTOR_CRITERIA
                              select new ContractorCriteriaViewModel
                              {
                                  criteriaId = a.CRITERIAID,
                                  criteria = a.CRITERIA
                              }).ToList();

            return categories;
        }


        public bool AddContractorCriteria(ContractorCriteriaViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CONTRACTOR_CRITERIA cONTRACTOR_CRITERIA;
                    cONTRACTOR_CRITERIA = new TBL_CONTRACTOR_CRITERIA
                    {
                        CRITERIA = entity.criteria,
                        TIERONE = entity.tierOne,
                        TIERTWO = entity.tierTwo,
                        TIERTHREE = entity.tierThree,
                    };
                    context.TBL_CONTRACTOR_CRITERIA.Add(cONTRACTOR_CRITERIA);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool AddIBLChecklist(IBLChecklistViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_IBL_CHECKLIST checks;
                    checks = new TBL_IBL_CHECKLIST
                    {
                        CHECKLIST = entity.checklist,
                        
                    };
                    context.TBL_IBL_CHECKLIST.Add(checks);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool AddContractorCriteriaOption(ContractorCriteriaOptionViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CONTRACTOR_CRITERIA_OPTION cONTRACTOR_CRITERIA;
                    cONTRACTOR_CRITERIA = new TBL_CONTRACTOR_CRITERIA_OPTION
                    {
                        CRITERIAID = entity.criteriaId,
                        OPTIONNAME = entity.optionName,
                        OPTIONVALUE = entity.optionValue,
                    };
                    context.TBL_CONTRACTOR_CRITERIA_OPTION.Add(cONTRACTOR_CRITERIA);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }
        
        public bool AddIBLChecklistOption(IBLChecklistOptionViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_IBL_CHECKLIST_OPTION cHECKLIST_OPTION;
                    cHECKLIST_OPTION = new TBL_IBL_CHECKLIST_OPTION
                    {
                        IBLCHECKLISTID = entity.iblChecklistId,
                        OPTIONNAME = entity.optionName,
                        //OPTIONVALUE = entity.optionValue,
                    };
                    context.TBL_IBL_CHECKLIST_OPTION.Add(cHECKLIST_OPTION);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateContractorCriteria(ContractorCriteriaViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CONTRACTOR_CRITERIA _CRITERIA;
                    if (entity.criteriaId > 0)
                    {
                        _CRITERIA = context.TBL_CONTRACTOR_CRITERIA.Find(entity.criteriaId);
                        if (_CRITERIA != null)
                        {
                            _CRITERIA.CRITERIA = entity.criteria;
                            _CRITERIA.TIERONE = entity.tierOne;
                            _CRITERIA.TIERTWO = entity.tierTwo;
                            _CRITERIA.TIERTHREE = entity.tierThree;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }
        public bool UpdateIBLChecklist(IBLChecklistViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_IBL_CHECKLIST cHECKLIST;
                    if (entity.iblChecklistId > 0)
                    {
                        cHECKLIST = context.TBL_IBL_CHECKLIST.Find(entity.iblChecklistId);
                        if (cHECKLIST != null)
                        {
                            cHECKLIST.CHECKLIST = entity.checklist;
                            
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateContractorCriteriaOption(ContractorCriteriaOptionViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_CONTRACTOR_CRITERIA_OPTION _CRITERIA;
                    if (entity.criteriaId > 0)
                    {
                        _CRITERIA = context.TBL_CONTRACTOR_CRITERIA_OPTION.Find(entity.optionId);
                        if (_CRITERIA != null)
                        {
                            _CRITERIA.CRITERIAID = entity.criteriaId;
                            _CRITERIA.OPTIONVALUE = entity.optionValue;
                            _CRITERIA.OPTIONNAME = entity.optionName;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }
        public bool UpdateIBLChecklistOption(IBLChecklistOptionViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_IBL_CHECKLIST_OPTION cHECKLIST_OPTION;
                    if (entity.optionId > 0)
                    {
                        cHECKLIST_OPTION = context.TBL_IBL_CHECKLIST_OPTION.Find(entity.optionId);
                        if (cHECKLIST_OPTION != null)
                        {
                            cHECKLIST_OPTION.IBLCHECKLISTID = entity.iblChecklistId;
                            //cHECKLIST_OPTION.OPTIONVALUE = entity.optionValue;
                            cHECKLIST_OPTION.OPTIONNAME = entity.optionName;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }



        public bool AddProjectRiskCriteria(ProjectRiskRatingCriteriaViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_PROJECT_RISK_RATING_CRITERIA tBL_PROJECT_RISK;
                    tBL_PROJECT_RISK = new TBL_PROJECT_RISK_RATING_CRITERIA
                    {
                        CRITERIA = entity.criteria,
                        CRITERIAVALUE = entity.criteriaValue,
                        PROJECTRISKRATINGCATEGORYID = entity.projectRiskRatingCategoryId,
                    };
                    context.TBL_PROJECT_RISK_RATING_CRITERIA.Add(tBL_PROJECT_RISK);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateProjectRiskCriteria(ProjectRiskRatingCriteriaViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_PROJECT_RISK_RATING_CRITERIA tBL_PROJECT_RISK;
                    if (entity.projectRiskRatingCriteriaId > 0)
                    {
                        tBL_PROJECT_RISK = context.TBL_PROJECT_RISK_RATING_CRITERIA.Find(entity.projectRiskRatingCriteriaId);
                        if (tBL_PROJECT_RISK != null)
                        {
                            tBL_PROJECT_RISK.CRITERIA = entity.criteria;
                            tBL_PROJECT_RISK.CRITERIAVALUE = entity.criteriaValue;
                            tBL_PROJECT_RISK.PROJECTRISKRATINGCATEGORYID = entity.projectRiskRatingCategoryId;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool AddProjectRiskCategory(ProjectRiskRatingCategoryViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_PROJECT_RISK_RATING_CATEGORY tBL_PROJECT_RISK;
                    tBL_PROJECT_RISK = new TBL_PROJECT_RISK_RATING_CATEGORY
                    {
                        CATEGORYNAME = entity.categoryName
                    };
                    context.TBL_PROJECT_RISK_RATING_CATEGORY.Add(tBL_PROJECT_RISK);
                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public bool UpdateProjectRiskCategory(ProjectRiskRatingCategoryViewModel entity)
        {
            if (entity != null)
            {
                try
                {
                    TBL_PROJECT_RISK_RATING_CATEGORY tBL_PROJECT_RISK;
                    if (entity.categoryId > 0)
                    {
                        tBL_PROJECT_RISK = context.TBL_PROJECT_RISK_RATING_CATEGORY.Find(entity.categoryId);
                        if (tBL_PROJECT_RISK != null)
                        {
                            tBL_PROJECT_RISK.CATEGORYNAME = entity.categoryName;
                        }

                    }

                    var response = context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
            return false;
        }

        public IEnumerable<ContractorCriteriaViewModel> getAllContractorCriteria()
        {
            var contractorCriteria = (from a in context.TBL_CONTRACTOR_CRITERIA
                                      select new ContractorCriteriaViewModel
                                      {
                                          criteriaId = a.CRITERIAID,
                                          criteria = a.CRITERIA,
                                          tierOne = a.TIERONE,
                                          tierTwo = a.TIERTWO,
                                          tierThree = a.TIERTHREE,
                                          options = context.TBL_CONTRACTOR_CRITERIA_OPTION.Where(x => x.CRITERIAID == a.CRITERIAID).Select(x => new ContractorCriteriaOptionViewModel
                                          {
                                              optionName = x.OPTIONNAME,
                                              optionValue = x.OPTIONVALUE
                                          }).ToList(),
                                      }).ToList();

            return contractorCriteria;
        }

        public IEnumerable<IBLChecklistViewModel> getAllIBLChecklist()
        {
            var iblChecklist = (from a in context.TBL_IBL_CHECKLIST
                                select new IBLChecklistViewModel
                                      {
                                          iblChecklistId = a.IBLCHECKLISTID,
                                          checklist = a.CHECKLIST,
                                          options = context.TBL_IBL_CHECKLIST_OPTION.Where(x => x.IBLCHECKLISTID == a.IBLCHECKLISTID).Select(x => new IBLChecklistOptionViewModel
                                          {
                                              optionName = x.OPTIONNAME,
                                              optionId = x.OPTIONID
                                          }).ToList(),
                                      }).ToList();

            return iblChecklist;
        }

        public IEnumerable<ContractorCriteriaOptionViewModel> getAllContractorCriteriaOption()
        {
            var contractorCriteria = (from a in context.TBL_CONTRACTOR_CRITERIA_OPTION

                                      select new ContractorCriteriaOptionViewModel
                                      {
                                          criteriaId = a.CRITERIAID,
                                          optionId = a.OPTIONID,
                                          optionName = a.OPTIONNAME,
                                          optionValue = a.OPTIONVALUE,
                                          criteria = context.TBL_CONTRACTOR_CRITERIA.Where(c => c.CRITERIAID == a.CRITERIAID).Select(c => c.CRITERIA).FirstOrDefault(),
                                      }).ToList();

            return contractorCriteria;
        }
        public IEnumerable<IBLChecklistViewModel> getAllIBLCheclistOption()
        {
            var contractorCriteria = (from a in context.TBL_IBL_CHECKLIST_OPTION

                                      select new IBLChecklistViewModel
                                      {
                                          iblChecklistId = a.IBLCHECKLISTID,
                                          optionId = a.OPTIONID,
                                          optionName = a.OPTIONNAME,
                                          //optionValue = a.OPTIONVALUE,
                                          checklist = context.TBL_IBL_CHECKLIST.Where(c => c.IBLCHECKLISTID == a.IBLCHECKLISTID).Select(c => c.CHECKLIST).FirstOrDefault(),
                                      }).ToList();

            return contractorCriteria;
        }

        public IEnumerable<ContractorTieringViewModel> getContractorTieringByApplication(int loanApplicationId, int customerId)
        {
            var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                     where a.LOANAPPLICATIONID == loanApplicationId && a.CUSTOMERID == customerId
                                     select new ContractorTieringViewModel
                                     {
                                         contractorTierId = a.CONTRACTORTIERID,
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         customerId = a.CUSTOMERID,
                                         actualValue = a.ACTUALVALUE
                                     }).ToList();

            return contractorTiering;
        }

        public IEnumerable<IBLChecklistViewModel> getIBLChecklistDetailByApplication(int loanApplicationId, int customerId)
        {
            var iblCheclistDetail = (from a in context.TBL_IBL_CHECKLIST_DETAIL
                                     where a.LOANAPPLICATIONID == loanApplicationId && a.CUSTOMERID == customerId
                                     select new IBLChecklistViewModel
                                     {
                                         iblChecklistDetailId = a.IBLCHECKLISTDETAILID,
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         customerId = a.CUSTOMERID,
                                         checklist = context.TBL_IBL_CHECKLIST.Where(c=>c.IBLCHECKLISTID == a.IBLCHECKLISTID).FirstOrDefault().CHECKLIST,
                                         actualValue = context.TBL_IBL_CHECKLIST_OPTION.Where(o=>o.OPTIONID == a.OPTIONID).FirstOrDefault().OPTIONNAME
                                     }).ToList();

            return iblCheclistDetail;
        }

        public IEnumerable<ContractorTieringViewModel> getContractorTieringByApplicationAndCustomer(int loanApplicationId, int customerId)
        {
            var contractorTiering = (from a in context.TBL_CONTRACTOR_TIERING
                                     join c in context.TBL_CONTRACTOR_CRITERIA on a.CONTRACTORCRITERIAID equals c.CRITERIAID
                                     where a.LOANAPPLICATIONID == loanApplicationId && a.CUSTOMERID == customerId
                                     select new
                                     {
                                         contractorTierId = a.CONTRACTORTIERID,
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         customerId = a.CUSTOMERID,
                                         criteria = c.CRITERIA,
                                         actualValue = a.ACTUALVALUE
                                     }).AsEnumerable().Select(a => new ContractorTieringViewModel
                                     {
                                         contractorTierId = a.contractorTierId,
                                         loanApplicationId = a.loanApplicationId,
                                         customerId = a.customerId,
                                         criteria = a.criteria,
                                         actualValue = a.actualValue
                                     }).ToList();

            var result = contractorTiering.Select(a => new ContractorTieringViewModel
            {
                contractorTierId = a.contractorTierId,
                loanApplicationId = a.loanApplicationId,
                customerId = a.customerId,
                criteria = a.criteria,
                actualValue = a.actualValue,
                computation = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == a.loanApplicationId).Sum(d => d.ACTUALVALUE),
            }).ToList();

            return result;
        }

        public IEnumerable<IBLChecklistViewModel> getIBLChecklistDetailByApplicationAndCustomer(int loanApplicationId, int customerId)
        {
            var contractorTiering = (from a in context.TBL_IBL_CHECKLIST_DETAIL
                                     join c in context.TBL_IBL_CHECKLIST on a.IBLCHECKLISTID equals c.IBLCHECKLISTID
                                     where a.LOANAPPLICATIONID == loanApplicationId && a.CUSTOMERID == customerId
                                     select new
                                     {
                                         iblChecklistDetailId = a.IBLCHECKLISTDETAILID,
                                         loanApplicationId = a.LOANAPPLICATIONID,
                                         customerId = a.CUSTOMERID,
                                         checklist = c.CHECKLIST,
                                         //actualValue = a.ACTUALVALUE
                                     }).AsEnumerable().Select(a => new IBLChecklistViewModel
                                     {
                                         iblChecklistDetailId = a.iblChecklistDetailId,
                                         loanApplicationId = a.loanApplicationId,
                                         customerId = a.customerId,
                                         checklist = a.checklist,
                                         //actualValue = a.actualValue
                                     }).ToList();

            var result = contractorTiering.Select(a => new IBLChecklistViewModel
            {
                iblChecklistDetailId = a.iblChecklistDetailId,
                loanApplicationId = a.loanApplicationId,
                customerId = a.customerId,
                checklist = a.checklist,
                
            }).ToList();

            return result;
        }


        public IEnumerable<ContractorCriteriaViewModel> getContractorTieringForEdit(int contractorTieringId)
        {
            var contractorTiering = context.TBL_CONTRACTOR_TIERING.Find(contractorTieringId);
            var contractorCriteria = (from a in context.TBL_CONTRACTOR_CRITERIA
                                      where a.CRITERIAID == contractorTiering.CONTRACTORCRITERIAID
                                      select new ContractorCriteriaViewModel
                                      {
                                          contractorTieringId = contractorTieringId,
                                          criteriaId = a.CRITERIAID,
                                          criteria = a.CRITERIA,
                                          tierOne = a.TIERONE,
                                          tierTwo = a.TIERTWO,
                                          tierThree = a.TIERTHREE,
                                          options = context.TBL_CONTRACTOR_CRITERIA_OPTION.Where(x => x.CRITERIAID == a.CRITERIAID).Select(x => new ContractorCriteriaOptionViewModel
                                          {
                                              optionName = x.OPTIONNAME,
                                              optionValue = x.OPTIONVALUE
                                          }).ToList(),
                                      }).ToList();

            return contractorCriteria;
        }
        public IEnumerable<IBLChecklistViewModel> getIBLChecklistDetailForEdit(int iblChecklistDetailId)
        {
            var iblDetail = context.TBL_IBL_CHECKLIST_DETAIL.Find(iblChecklistDetailId);
            var checklisDetail = (from a in context.TBL_IBL_CHECKLIST
                                      where a.IBLCHECKLISTID == iblDetail.IBLCHECKLISTID
                                      select new IBLChecklistViewModel
                                      {
                                          iblChecklistDetailId = iblChecklistDetailId,
                                          iblChecklistId = a.IBLCHECKLISTID,
                                          checklist = a.CHECKLIST,
                                          
                                          options = context.TBL_IBL_CHECKLIST_OPTION.Where(x => x.IBLCHECKLISTID == a.IBLCHECKLISTID).Select(x => new IBLChecklistOptionViewModel
                                          {
                                              optionName = x.OPTIONNAME,
                                              optionId = x.OPTIONID
                                          }).ToList(),
                                      }).ToList();

            return checklisDetail;
        }

        public IEnumerable<ProjectRiskRatingCriteriaViewModel> getAllProjectRiskRatingCriteria()
        {
            
                return (from a in context.TBL_PROJECT_RISK_RATING_CRITERIA
                        where a.PROJECTRISKRATINGCRITERIAID > 0
                        select new ProjectRiskRatingCriteriaViewModel
                        {
                            projectRiskRatingCriteriaId = a.PROJECTRISKRATINGCRITERIAID,
                            projectRiskRatingCategoryId = a.PROJECTRISKRATINGCATEGORYID,
                            criteria = a.CRITERIA,
                            criteriaValue = a.CRITERIAVALUE,
                            category = context.TBL_PROJECT_RISK_RATING_CATEGORY.Where(p => p.CATEGORYID == a.PROJECTRISKRATINGCATEGORYID).Select(p => p.CATEGORYNAME).FirstOrDefault()
                        }).ToList();

        }

        public IEnumerable<ProjectRiskRatingCategoryViewModel> getAllProjectRiskRatingByCategories()
        {
            var projectRiskRatingCriteria = (from a in context.TBL_PROJECT_RISK_RATING_CATEGORY
                                      where a.CATEGORYNAME != null
                                      select new ProjectRiskRatingCategoryViewModel
                                      {
                                          categoryId = a.CATEGORYID,
                                          categoryName = a.CATEGORYNAME,
                                          criterias = context.TBL_PROJECT_RISK_RATING_CRITERIA.Where(x=>x.PROJECTRISKRATINGCATEGORYID == a.CATEGORYID).Select(x => new ProjectRiskRatingCriteriaViewModel
                                          {
                                              criteria = x.CRITERIA,
                                              criteriaValue = x.CRITERIAVALUE
                                          }).ToList(),
                                      }).ToList();

            return projectRiskRatingCriteria;
        }

        public IEnumerable<ProjectRiskRatingViewModel> getProjectRiskRatingByApplicationDetailId(int loanApplicationId, int loanApplicationDetailId, int loanBookingRequestId)
        {
            var projectRiskrating = (from a in context.TBL_PROJECT_RISK_RATING
                                     where a.LOANAPPLICATIONID == loanApplicationId && a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                     select new ProjectRiskRatingViewModel
                                     {
                                         categoryId = a.CATEGORYID,
                                         categoryValue = a.CATEGORYVALUE,
                                         projectLocation = a.PROJECTLOCATION,
                                         projectDetails = a.PROJECTDETAILS
                                     }).ToList();

            return projectRiskrating;
        }

        public IEnumerable<ProjectRiskRatingViewModel> getProjectRiskRatingByApplicationAndApplicationDetailId(int loanApplicationId, int loanApplicationDetailId)
        {
                var customerTier1 = context.TBL_CONTRACTOR_TIERING.Where(d => d.LOANAPPLICATIONID == loanApplicationId).ToList();
                var projectRiskRating = (from a in context.TBL_PROJECT_RISK_RATING
                                         join c in context.TBL_PROJECT_RISK_RATING_CATEGORY on a.CATEGORYID equals c.CATEGORYID
                                         where a.LOANAPPLICATIONID == loanApplicationId && a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                         select new
                                         {
                                             loanApplicationId = a.LOANAPPLICATIONID,
                                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                             loanBookingRequestId = a.LOANBOOKINGREQUESTID,
                                             categoryName = c.CATEGORYNAME,
                                             categoryValue = a.CATEGORYVALUE,
                                             projectLocation = a.PROJECTLOCATION,
                                             projectDetails = a.PROJECTDETAILS
                                         }).AsEnumerable().Select(a => new ProjectRiskRatingViewModel
                                         {
                                             loanApplicationId = a.loanApplicationId,
                                             loanApplicationDetailId = a.loanApplicationDetailId,
                                             loanBookingRequestId = a.loanBookingRequestId,
                                             categoryName = a.categoryName,
                                             categoryValue = a.categoryValue,
                                             projectLocation = a.projectLocation,
                                             projectDetails = a.projectDetails
                                         }).ToList();

                var result = projectRiskRating.Select(a => new ProjectRiskRatingViewModel
                {
                    loanApplicationId = a.loanApplicationId,
                    loanApplicationDetailId = a.loanApplicationDetailId,
                    loanBookingRequestId = a.loanBookingRequestId,
                    categoryName = a.categoryName,
                    categoryValue = a.categoryValue,
                    projectLocation = a.projectLocation,
                    projectDetails = a.projectDetails,
                    computation = context.TBL_PROJECT_RISK_RATING.Where(d => d.LOANAPPLICATIONDETAILID == a.loanApplicationDetailId).Sum(d => d.CATEGORYVALUE),
                }).ToList();

                foreach (var i in result)
                {
                    int rating = 0;
                    i.customerTier = 0;
                    if (customerTier1 != null)
                    {
                        decimal compute = customerTier1.Where(x => x.LOANAPPLICATIONID == i.loanApplicationId).Sum(x => x.ACTUALVALUE);
                        if (compute >= 80) { rating = 25; }
                        if (compute >= 60 && compute <= 79) { rating = 20; }
                        if (compute <= 59) { rating = 10; }
                        i.customerTier = compute;
                    }

                    var overRallTotal = i.computation + rating;
                    if (overRallTotal >= 81 && overRallTotal <= 100)
                    {
                        i.riskCategorisation = "LOW";
                    }
                    if (overRallTotal >= 66 && overRallTotal < 81)
                    {
                        i.riskCategorisation = "MODERATE";
                    }
                    if (overRallTotal >= 51 && overRallTotal < 66)
                    {
                        i.riskCategorisation = "ABOVE AVERAGE";
                    }
                    if (overRallTotal < 51)
                    {
                        i.riskCategorisation = "HIGH";
                    }
                }

                return result;
            
        }
    }
}
