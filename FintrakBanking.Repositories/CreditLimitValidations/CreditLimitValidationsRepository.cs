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
using System.Threading.Tasks;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum; 
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using System.ComponentModel.Composition;
//using System.Math;

namespace FintrakBanking.Repositories.CreditLimitValidations

{
    public class CreditLimitValidationsRepository : ICreditLimitValidationsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private ILoanRepository loanRepository;
        private ICustomerRepository customerRepository;

        public CreditLimitValidationsRepository(IGeneralSetupRepository _genSetup, ILoanRepository _loanRepository, ICustomerRepository _customerRepository,

        FinTrakBankingContext _context)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            loanRepository = _loanRepository;
            customerRepository = _customerRepository;
        }

        public int ValidateWatchList(int customerId)
        {
            var watchlist = (from a in context.tbl_Loan
                             join b in context.tbl_Loan_PrudentialGuideline on a.ExternalPrudentialGuidelineStatusId equals b.PrudentialGuidelineStatusId
                             where a.CustomerId == customerId  //loan.customerId
                             select a);
            int watchlistresults = watchlist.Count();

            return watchlistresults;
        }

        public int ValidateCamsol(int customerId)
        {
            var camsol = (from a in context.tbl_Loan_Camsol
                          where a.tbl_Loan.CustomerId == customerId
                          select a);
            int camsolresults = camsol.Count();

            return camsolresults;
        }

        public int ValidateBlackList(int customerId)
        {
            var blacklist = (from a in context.tbl_Customer_Blacklist
                             where a.CustomerId == customerId
                             select a);
            int blacklistresults = blacklist.Count();

            return blacklistresults;
        }

        public CreditLimitValidationsModel ValidateAmountByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.BranchId == branchId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Branch && a.TargetId == branchId &&
                              b.LimitMetricId == (int)LimitMatricEnum.LoanAmount
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByBranch(short branchId)
        {

            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.BranchId == branchId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Branch && a.TargetId == branchId &&
                              b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
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

        public CreditLimitValidationsModel ValidateAmountBySector(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var subsector = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId).SubSectorId;
            var sector = this.context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == subsector).SectorId;
            var outstandingbal = from a in context.tbl_Loan
                                 join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                 join c in context.tbl_Sub_Sector on b.SubSectorId equals c.SubSectorId
                                 where a.CustomerId==b.CustomerId && b.SubSectorId==c.SubSectorId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Sector && a.TargetId == customerId &&
                              b.LimitMetricId == (int)LimitMatricEnum.LoanAmount //&&
                                                                                 //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                 //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                 //select maximumValue;
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLBySector(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();

            var subsector = this.context.tbl_Customer.FirstOrDefault(x => x.CustomerId == customerId).SubSectorId;
            var sector = this.context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == subsector).SectorId;
            var outstandingbal = from a in context.tbl_Loan
                                 join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                                 join c in context.tbl_Sub_Sector on b.SubSectorId equals c.SubSectorId
                                 where a.CustomerId == b.CustomerId && b.SubSectorId == c.SubSectorId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Sector && a.TargetId == customerId &&
                              b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateAmountByCustomer(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.CustomerId == customerId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Customer && a.TargetId == customerId &&
                              b.LimitMetricId == (int)LimitMatricEnum.LoanAmount// &&
                                                                                //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                //select maximumValue;
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByCustomer(int customerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.CustomerId == customerId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Customer && a.TargetId == customerId &&
                              b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateAmountByCustomerGroup(int customergroupId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.CustomerGroupId == customergroupId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var customer = this.context.tbl_Loan.FirstOrDefault(x => x.CustomerGroupId == customergroupId).CustomerId;
            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Customer && a.TargetId == customer &&
                              b.LimitMetricId == (int)LimitMatricEnum.LoanAmount //&&
                                                                                 //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                 //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                 //select maximumValue;
                              select a.MaximumValue;
            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

        public CreditLimitValidationsModel ValidateNPLByCustomerGroup(int customergroupId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.CustomerGroupId == customergroupId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var customer = this.context.tbl_Loan.FirstOrDefault(x => x.CustomerGroupId == customergroupId).CustomerId;
            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.Customer && a.TargetId == customer &&
                              b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan //&&
                                                                                        //b.LimitValueTypeId == (int)LimitValueTypeEnum.Amount
                                                                                        //let maximumValue = context.tbl_Limit_Detail.Sum(a => a.MaximumValue)
                                                                                        //select maximumValue;
                              select a.MaximumValue;

            var limitAmountresult = limitAmount.FirstOrDefault();
            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model; 
        }

        public CreditLimitValidationsModel ValidateCreditLimitNPLByRMBM(short relationshipofficerId)
        {
            CreditLimitValidationsModel model = new CreditLimitValidationsModel();
            var outstandingbal = from d in context.tbl_Loan
                                 where d.RelationshipOfficerId == relationshipofficerId || d.RelationshipManagerId== relationshipofficerId
                                 let sumPrincipalAmount = context.tbl_Loan.Sum(a => a.PrincipalAmount)
                                 select sumPrincipalAmount;

            var limitAmount = from a in context.tbl_Limit_Detail
                              join b in context.tbl_Limit on a.LimitId equals b.LimitId
                              where a.LimitTypeId == (int)LimitType.AccountOfficer && a.TargetId == relationshipofficerId &&
                              b.LimitMetricId == (int)LimitMatricEnum.NonPerformingLoan 
                              select a.MaximumValue;

            model.outstandingBalance = outstandingbal.FirstOrDefault();
            model.limit = limitAmount.FirstOrDefault();
            model.difference = outstandingbal.FirstOrDefault() - limitAmount.FirstOrDefault();
            return model;
        }

    }
}
