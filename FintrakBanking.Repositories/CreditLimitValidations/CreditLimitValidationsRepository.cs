using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.CreditLimitValidations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Interfaces.CASA;
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

        public int ValidateCamsol(int customerId)
        {
            var camsol = (from a in context.TBL_LOAN_CAMSOL
                          join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                          where b.CUSTOMERID == customerId 
                          select a);
            int camsolresults = camsol.Count();
            if (camsol.Any())
            {
                string custCode = camsol.FirstOrDefault().CUSTOMERCODE;
                camsolresults = this.customOverride.EffectOverride(custCode, (int)OverrideEnum.CAMSOLOverride, custCode);
            }
            return camsolresults;
        }
        //public int ValidateBlackList(int customerId)
        //{
        //    var blacklist = (from a in context.TBL_CUSTOMER_BLACKLIST
        //                     where a.CUSTOMERID == customerId
        //                     select a);
        //    int blacklistresults = blacklist.Count();

        //    return blacklistresults;
        //}
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

        public IEnumerable<CustomerEligibilityViewModel> ValidateCustomerEligibility(string customerCode)
        {
            var blacklist = (from a in context.TBL_CUSTOMER_BLACKLIST
                             where a.CUSTOMERCODE == customerCode
                             select new CustomerEligibilityViewModel()
                             {
                                 customerCode = a.CUSTOMERCODE,
                                 dateBlackListed = a.DATEBLACKLISTED,
                                 reason = a.REASON
                             }).ToList();
            return blacklist;
        }

        public CreditLimitValidationsModel ValidateAmountByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = (from d in context.TBL_LOAN
                                  where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select d.OUTSTANDINGPRINCIPAL).Sum();

            var limitAmount = (from a in context.TBL_LIMIT_DETAIL
                               join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                               where a.LIMITTYPEID == (int)LimitType.Branch && a.TARGETID == branchId &&
                               b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
                               select a.MAXIMUMVALUE).Sum();

            model.outstandingBalance = (double)outstandingbal;
            model.limit = (double)limitAmount;
            model.difference = (double)(outstandingbal - limitAmount);
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var loanOutstandingBalance  = (from d in context.TBL_LOAN
                                 where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active || d.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                                 select new
                                 {
                                     d.OUTSTANDINGPRINCIPAL
                                 }).ToList();
            var sumLoanOutstandingBalance = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL).Sum();


            var ODOutstandingBalance  = (from d in context.TBL_LOAN_REVOLVING
                                          where d.BRANCHID == branchId && d.LOANSTATUSID == (short)LoanStatusEnum.Active || d.LOANSTATUSID == (short)LoanStatusEnum.Inactive
                                          select new
                                          {
                                              d.OVERDRAFTLIMIT
                                          }).ToList();
            var sumODOutstandingBalance = ODOutstandingBalance.Select(c => c.OVERDRAFTLIMIT).Sum();

            var limitAmount = from a in context.TBL_BRANCH where a.BRANCHID == branchId
                              let maximumLimit = a.NPL_LIMIT
                              select maximumLimit;


            model.outstandingBalance = (double)(sumLoanOutstandingBalance + sumODOutstandingBalance);
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = model.limit - model.outstandingBalance;

            return model;
        }

        public CreditLimitValidationsModel ValidateAmountBySegment(short segmentId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.PRODUCTID == segmentId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.PRODUCTID == segmentId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == segmentId &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
                              select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLBySegment(short segmentId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.PRODUCTID == segmentId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.PRODUCTID == segmentId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == segmentId &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                              select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
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
            //int sectorId = 1;
            int sectorId = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault().SECTORID.Value;
            //var sectorDetail = context.TBL_SUB_SECTOR.FirstOrDefault(a => a.SUBSECTORID == subSectorId);
            //int sectorId = sectorDetail.SECTORID.Value;
            var data = from a in context.TBL_SECTOR
                       where a.SECTORID == sectorId
                       let maximumLimit = a.LOAN_LIMIT
                       select maximumLimit;
         
            var sector = context.TBL_SECTOR.Where(a => a.SECTORID == sectorId).FirstOrDefault();

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var loanOutstandingBalance = (from d in context.TBL_LOAN 
                                          join f in context.TBL_SUB_SECTOR on d.SUBSECTORID equals f.TBL_SECTOR.SECTORID
                                          where d.TBL_SUB_SECTOR.SECTORID == sectorId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
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
            model.limit = (double)data.FirstOrDefault();
            model.difference = model.limit - model.outstandingBalance;

            return model;

        }

        public CreditLimitValidationsModel ValidateAmountBySectorOld(int subSectorId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();


            short sectorId = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault().SECTORID.Value;


            var outstandingbal = (from a in context.TBL_LOAN
                                  join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                  where a.TBL_SUB_SECTOR.SECTORID == sectorId && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select a.OUTSTANDINGPRINCIPAL).Sum();


            var limitAmount = (from a in context.TBL_LIMIT_DETAIL
                               join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                               where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == sectorId &&
                               b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
                               select a.MAXIMUMVALUE).Sum();

            model.outstandingBalance = (double)outstandingbal;
            model.limit = (double)limitAmount;
            model.difference = (double)outstandingbal - (double)limitAmount;
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLBySector(int subSectorId)
        {
            short sectorId = context.TBL_SUB_SECTOR.Where(a => a.SUBSECTORID == subSectorId).FirstOrDefault().SECTORID.Value;

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var outstandingbal = from a in context.TBL_LOAN
                                 join c in context.TBL_SUB_SECTOR on a.SUBSECTORID equals c.SUBSECTORID
                                 where a.SUBSECTORID == c.SUBSECTORID && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(x => x.TBL_SUB_SECTOR.SECTORID == sectorId).Sum(x => x.OUTSTANDINGPRINCIPAL)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Sector && a.TARGETID == sectorId &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
            return model;
        }

        public IEnumerable<SectorLimitViewModel> GetSectorLoanAmountLimit()
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var output = (from a in context.TBL_LIMIT_DETAIL
                          join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                          join c in context.TBL_SECTOR on a.TARGETID equals c.SECTORID
                          join d in context.TBL_LOAN on c.SECTORID equals d.TBL_SUB_SECTOR.SECTORID
                          where a.LIMITTYPEID == (int)LimitType.Sector && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                          && b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount
                          group d by new { c.SECTORID, c.CODE, c.NAME, a.MAXIMUMVALUE } into groupedQ
                          select new SectorLimitViewModel()
                          {
                              sectorId = groupedQ.Key.SECTORID,
                              sectorCode = groupedQ.Key.CODE,
                              sectorName = groupedQ.Key.NAME,
                              sectorLimit = groupedQ.Key.MAXIMUMVALUE,
                              sectorUsage = groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL),
                              sectorBalance = groupedQ.Key.MAXIMUMVALUE - groupedQ.Sum(i => i.OUTSTANDINGPRINCIPAL)
                          });

            return output;
        }

        //public CreditLimitValidationsModel ValidateAmountByCustomer(int customerId)
        //{
        //    CreditLimitValidationsModel model = new CreditLimitValidationsModel();
        //    var outstandingbal = from d in context.TBL_LOAN
        //                         where d.CUSTOMERID == customerId
        //                         let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.CUSTOMERID == customerId).Sum(a => a.PRINCIPALAMOUNT)
        //                         select sumPrincipalAmount;

        //    var limitAmount = from a in context.TBL_LIMIT_DETAIL
        //                      join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
        //                      where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customerId &&
        //                      b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount// &&
        //                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
        //                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
        //                                                                        //select maximumValue;
        //                      select a.MAXIMUMVALUE;

        //    model.outstandingBalance = outstandingbal.FirstOrDefault();
        //    model.limit = limitAmount.FirstOrDefault();
        //    model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
        //    return model;
        //}

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
                                          where d.CUSTOMERID == customerId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                          select new
                                          {
                                            d.OUTSTANDINGPRINCIPAL
                                          }).ToList();
            var sumLoanOutstandingBalance  = loanOutstandingBalance.Select(c => c.OUTSTANDINGPRINCIPAL).Sum();

            var OverdraftOutstandingBalance = (from d in context.TBL_LOAN_REVOLVING
                                              where d.CUSTOMERID == customerId && d.LOANSTATUSID == (short)LoanStatusEnum.Active
                                              select new
                                              {
                                                  d.OVERDRAFTLIMIT
                                              }).ToList();
            var sumOverdraftOutstandingBalance = OverdraftOutstandingBalance.Select(c => c.OVERDRAFTLIMIT).Sum();

            //var loanApplicationBalance = (from d in context.TBL_LOAN_APPLICATION
            //                             where d.APPLICATIONSTATUSID == (short)LoanStatusEnum.Active && d.CUSTOMERID == customerId
            //                             && !context.TBL_LOAN.Any(e => e.CUSTOMERID == d.CUSTOMERID)
            //                             select new
            //                             {
            //                                 d.APPLICATIONAMOUNT
            //                             }).ToList();
            //var sumloanApplicationBalance  = loanApplicationBalance.Select(c => c.APPLICATIONAMOUNT ).Sum();

            //var OverDraftApplicationBalance = (from d in context.TBL_LOAN_APPLICATION
            //                                  where d.APPLICATIONSTATUSID == (short)LoanStatusEnum.Active && d.CUSTOMERID == customerId
            //                                  && !context.TBL_LOAN_REVOLVING.Any(e => e.CUSTOMERID == d.CUSTOMERID)
            //                                  select new
            //                                  {
            //                                      d.APPLICATIONAMOUNT
            //                                  }).ToList();
            //var sumOverDraftApplicationBalance  = OverDraftApplicationBalance.Select(c => c.APPLICATIONAMOUNT).Sum();
            model.outstandingBalance = (double)(sumLoanOutstandingBalance + sumOverdraftOutstandingBalance);
            model.limit = data.FirstOrDefault();
            model.difference = model.limit - model.outstandingBalance;

            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByCustomer(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.CUSTOMERID == customerId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.CUSTOMERID == customerId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customerId &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateAmountByCustomerGroup(int customergroupId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var customer = this.context.TBL_LOAN.FirstOrDefault(x => x.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.CUSTOMERGROUPID == customergroupId).CUSTOMERID;
            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customer &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.LoanAmount //&&
                                                                                 //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                 //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                 //select maximumValue;
                              select a.MAXIMUMVALUE;
            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
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
            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.Obligor && a.TARGETID == customer &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MAXIMUMVALUE;

            var limitAmountresult = limitAmount.FirstOrDefault();
            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateCreditLimitNPLByRMBM(short relationshipofficerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.TBL_LOAN
                                 where d.RELATIONSHIPOFFICERID == relationshipofficerId || d.RELATIONSHIPMANAGERID == relationshipofficerId
                                 let sumPrincipalAmount = context.TBL_LOAN.Where(a => a.RELATIONSHIPOFFICERID == relationshipofficerId).Sum(a => a.PRINCIPALAMOUNT)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.TBL_LIMIT_DETAIL
                              join b in context.TBL_LIMIT on a.LIMITID equals b.LIMITID
                              where a.LIMITTYPEID == (int)LimitType.RelationshipManager && a.TARGETID == relationshipofficerId &&
                              b.LIMITMETRICID == (int)LimitMatricEnum.NonPerformingLoan
                              select a.MAXIMUMVALUE;

            model.outstandingBalance = (double)outstandingbal.FirstOrDefault();
            model.limit = (double)limitAmount.FirstOrDefault();
            model.difference = (double)outstandingbal.FirstOrDefault() - (double)limitAmount.FirstOrDefault();
            return model;
        }

    }
}
