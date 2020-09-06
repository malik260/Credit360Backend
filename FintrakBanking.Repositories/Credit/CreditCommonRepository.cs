using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.CASA;
using System.Globalization;

namespace FintrakBanking.Repositories.Credit
{
    public class CreditCommonRepository
    {
        private FinTrakBankingContext context;

        private IAdminRepository admin;
        private IIntegrationWithFinacle integration;
        private ICreditLimitValidationsRepository limitValidation;


        public CreditCommonRepository(
            IAdminRepository admin,
            FinTrakBankingContext context,
            IIntegrationWithFinacle integration,
            ICreditLimitValidationsRepository limitValidation
            )
        {
            this.admin = admin;
            this.context = context;
            this.integration = integration;
            this.limitValidation = limitValidation;
        }

        public void LoadCustomerTurnover(int applicationId, List<int> customerIds, int staffId, bool isLms = false) 
        {
            string duration = WebConfigurationManager.AppSettings["AccountStatisticsDurationInMonths"];
            int newDuration = 0;
            if (string.IsNullOrEmpty(duration))
            {
                duration = "6";
            }
            Int32.TryParse(duration, out newDuration);


            int turnoverDuration = newDuration;
            var apiTransactions = new List<CustomerTurnoverViewModel>();
            var apiCustomerAccounts = new List<CasaViewModel>();
            var apiTransactionsOthers = new List<CustomerTurnoverViewModel>();

            //var customers = (from a in context.TBL_LOAN_APPLICATION_DETAIL 
            //            join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
            //            where a.LOANAPPLICATIONID == applicationId 
            //            select new CustomerViewModels
            //            {
            //                customerId = a.CUSTOMERID,
            //                customerCode = b.CUSTOMERCODE

            //            }).Distinct().ToList();

            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID)).ToList();//.Select(x => x.CUSTOMERCODE);
            
            foreach (var customer in customers)
            {
                if(customer.ISPROSPECT == false)
                {
                    var casa = context.TBL_CASA.Where(x => x.CUSTOMERID == customer.CUSTOMERID);
                    //Task.Run(async () => { apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();
                    //Task.Run(async () => apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration)).GetAwaiter().GetResult();
                    apiCustomerAccounts = integration.GetCustomerAccountsBalanceByCustomerCode(customer.CUSTOMERCODE);
                    //apiCustomerAccounts = integration.GetCustomerAccountsBalanceByCustomerCode("0689601167");
                    apiTransactions = integration.GetCustomerAccountTurnover(customer.CUSTOMERCODE, turnoverDuration);

                    if (apiTransactions == null) {
                        throw new APIErrorException("Core Banking API Error, No Record Found!");
                    }

                    if (apiTransactions.Count() == 0)
                    {
                        throw new APIErrorException("Core Banking API Info, No Record Found for the Customer!");
                    }

                    foreach (var transaction in apiTransactions)
                    {
                        context.TBL_LOAN_APPLICATION_TRANS.Add(new TBL_LOAN_APPLICATION_TRANS
                        {
                            LOANAPPLICATIONID = applicationId,
                            CUSTOMERID = customer.CUSTOMERID,
                            CUSTOMERCODE = customer.CUSTOMERCODE,
                            ACCOUNTNUMBER = transaction.accountNumber,
                            //PERIOD = transaction.period,
                            PERIOD = transaction.month == null ? "" : new DateTime(2019, (int)transaction.month, 1).ToString("MMM", CultureInfo.InvariantCulture).ToUpper(),
                            PRODUCTNAME = transaction.productName,
                            MINIMUMDEBITBALANCE = transaction.min_Debit_Balance,
                            MAXIMUMDEBITBALANCE = transaction.max_Debit_Balance,
                            MINIMUMCREDITBALANCE = transaction.min_Credit_Balance,
                            MAXIMUMCREDITBALANCE = transaction.max_Credit_Balance,
                            DEBITTURNOVER = transaction.debit_Turnover,
                            CREDITTURNOVER = transaction.credit_Turnover,
                            SMSALERT = transaction.sms_Alert,
                            AMC = transaction.amc,
                            VAT = transaction.vat,
                            MANAGEMENTFEE = transaction.management_Fee,
                            COMMITMENTFEE = transaction.commitment_Fees,
                            CONTINGENTLIABILITYCOMM = transaction.com_Contigent_Liab,
                            LC_COMMISSION = transaction.lc_Commission,
                            CREATEDBY = staffId,
                            DATETIMECREATED = DateTime.Now,
                            MONTH = transaction.month,
                            YEAR = transaction.year,
                            ISLMS = isLms

                        });
                    }
                    //foreach (var account in apiCustomerAccounts) 
                    //{

                    //}


                }
            }
            context.SaveChanges();

            foreach (var customer in customers)
            {
                if(customer.ISPROSPECT == false)
                {
                    //Task.Run(async () => { itx = await _customerIntegration.GetCustomerInterestTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();
                    apiTransactionsOthers = integration.GetCustomerAccountInterestTransactions(customer.CUSTOMERCODE, turnoverDuration);
                    //apiTransactionsOthers = integration.GetCustomerAccountInterestTransactions("003068763", turnoverDuration);

                    foreach (var item in apiTransactionsOthers)
                    {
                        context.TBL_LOAN_APPLICATION_TRANS2.Add(new TBL_LOAN_APPLICATION_TRANS2
                        {
                            LOANAPPLICATIONID = applicationId,
                            CUSTOMERID = customer.CUSTOMERID,
                            CUSTOMERCODE = customer.CUSTOMERCODE,
                            ACCOUNTNUMBER = item.accountNumber,
                            PERIOD = item.period,
                            PRODUCTNAME = "n/a",
                            FLOATCHARGE = item.float_Charge,
                            INTEREST = item.interest,
                            CREATEDBY = staffId,
                            DATETIMECREATED = DateTime.Now,
                            MONTH = item.month,
                            YEAR = item.year,
                            ISLMS = isLms

                        });
                    }
                }
            }

            //if (context.SaveChanges() == 0) throw new SecureException("Customer turnover failed to load!");

            context.SaveChanges();

        }

        public void LoadCustomerRatios(int applicationId, List<int> customerIds, int staffId) 
        {
            bool isGroup = false;
            var apiCustomerRatio = new List<SubGroupRatingAndRatioViewModel>();
            var apiTransactionsOthers = new List<RatingAndRatioViewModel>();
            var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            if(application.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup) { isGroup = true; }
            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID));

            if (isGroup)
            {
                var ids = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == application.CUSTOMERGROUPID && x.DELETED == false).Select(x => x.CUSTOMERID).ToList();
                customers = context.TBL_CUSTOMER.Where(x => ids.Contains(x.CUSTOMERID));
            }

            foreach (var customer in customers)
            {
                if (customer.ISPROSPECT == false)
                {
                    apiCustomerRatio = integration.GetCustomerRatioByCustomerCode(customer.CUSTOMERCODE);

                    foreach (var item in apiCustomerRatio)
                    {
                        if (item.ratio_List != null)
                        {
                            for (int i = 0; i < item.ratio_List.Count; i++)
                            {
                                var lastTwo = item.ratio_List.Count - 2;

                                if (i < lastTwo) {
                                    context.TBL_CUSTOMER_RATIOS.Add(new TBL_CUSTOMER_RATIOS
                                    {
                                        FINANCIALPERIOD = item.financial_Period,
                                        DESCRIPTION = item.ratio_List[i].indicatorname,
                                        VALUE = item.ratio_List[i].indicatorvalue,
                                        CUSTOMERID = customer.CUSTOMERID,
                                        LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                                        CUSTOMERGROUPID = application.CUSTOMERGROUPID,
                                        DATETIMECREATED = DateTime.Now,
                                        CREATEDBY = staffId,
                                        DELETED = false,
                                        COMPILATIONDATE = DateTime.Parse(item.ratio_List[lastTwo].indicatorvalue),
                                        AUDITORNAME = item.ratio_List[lastTwo + 1].indicatorvalue
                                    });
                                }
                                //else if (i == lastTwo) { }
                                //else { }
                            }
                        }
                        
                    }
                }
            }

            context.SaveChanges();
        }

        public void LoadCustomerGroupRatios(int applicationId, List<int> customerIds, int staffId)
        {
            bool isGroup = false;
            var apiCustomerRatio = new List<MainGroupRatingAndRatioViewModel>();
            var apiTransactionsOthers = new List<GroupRatingAndRatioViewModel>();
            var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            if (application.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup) { isGroup = true; }
            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID));

            if (isGroup)
            {
                var ids = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == application.CUSTOMERGROUPID && x.DELETED == false).Select(x => x.CUSTOMERID).ToList();
                customers = context.TBL_CUSTOMER.Where(x => ids.Contains(x.CUSTOMERID));
            }

            foreach (var customer in customers)
            {
                if (customer.ISPROSPECT == false)
                {
                    apiCustomerRatio = integration.GetCustomerGroupRatioByCustomerCode(customer.CUSTOMERCODE);

                    foreach (var item in apiCustomerRatio)
                    {
                        if (item.ratio_List != null) {
                            for (int i = 0; i < item.ratio_List.Count; i++) {
                                for (int j = 0; j < item.ratio_List[i].ratio.Count; j++) {
                                    var customerRatio = new TBL_CUSTOMER_RATIOS
                                    {
                                        FINANCIALPERIOD = item.financial_Period,
                                        DESCRIPTION = item.ratio_List[i].ratio[j].indicatorname,
                                        VALUE = item.ratio_List[i].ratio[j].indicatorvalue,
                                        CUSTOMERID = customer.CUSTOMERID,
                                        LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                                        CUSTOMERGROUPID = application.CUSTOMERGROUPID,
                                        DATETIMECREATED = DateTime.Now,
                                        CREATEDBY = staffId,
                                        DELETED = false,
                                        // CATEGORYID = context.TBL_CUSTOMER_RATIO_CATEGORY.Where(O => item.ratioHeader.Contains(O.CATEGORYNAME)).FirstOrDefault()?.CATEGORYID,
                                    };

                                    if (item.ratio_List.Count == 4) {
                                        customerRatio.CATEGORYID = int.Parse(item.ratio_List[i].ratioHeaderId);
                                        customerRatio.CATEGORYIDFI = null;
                                    }
                                    else {
                                        customerRatio.CATEGORYID = null;
                                        customerRatio.CATEGORYIDFI = int.Parse(item.ratio_List[i].ratioHeaderId);
                                    }

                                    context.TBL_CUSTOMER_RATIOS.Add(customerRatio);
                                }
                            }
                        }
                        
                    }

                }
            }

            context.SaveChanges();
        }

        public void GetCorporateCustomerRating(int applicationId, List<int> customerIds, int staffId)
        {
            var apiCustomerRating = new CutomerRatingViewModel();
            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID));

            foreach (var customer in customers)
            {
                if (customer.ISPROSPECT == false)
                {
                    apiCustomerRating = integration.GetCorporateCustomerRatingByCustomerCode(customer.CUSTOMERCODE);
                    //apiCustomerRating = integration.GetCorporateCustomerRatingByCustomerCode("000107220");
                    //customer.CUSTOMERRATING = apiCustomerRating.companYRating;
                    customer.CUSTOMERRATING = apiCustomerRating.companY_RATING;
                }
            }

            context.SaveChanges();
        }

        public void GetAutoLoansRetail(int loanApplicationDetailId, int customerId, int staffId)
        {
            //bool isGroup = false;
            var autoLoan = new FacilityRatingViewModel();
            //var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            //if (application.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup) { isGroup = true; }
            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId).FirstOrDefault();

            if (customer.ISPROSPECT == false)
            {
                autoLoan = integration.GetAutoLoanRetailByCustomerCode(customer.CUSTOMERCODE);
                //autoLoans = integration.GetAutoLoanRetailByCustomerCode("007484680");

                if (autoLoan.probability_of_Default != null)
                {
                    context.TBL_FACILITY_RATING.Add(new TBL_FACILITY_RATING
                    {
                        LOANAPPLICATIONDETAILID = loanApplicationDetailId,
                        PROBABILITYOFDEFAULT = autoLoan.probability_of_Default,
                        REMARK = autoLoan.remark,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffId,
                        DELETED = false,
                    });
                }

            }

            context.SaveChanges();
        }

        public void GetPersonalLoansRetail(int loanApplicationDetailId, int customerId, int staffId)
        {
            //bool isGroup = false;
            var personalLoan = new FacilityRatingViewModel();
            //var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            //if (application.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup) { isGroup = true; }
            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId).FirstOrDefault();

            if (customer.ISPROSPECT == false)
            {
                //personalLoan = integration.GetPersonalLoanRetailByCustomerCode("000428107");
                personalLoan = integration.GetPersonalLoanRetailByCustomerCode(customer.CUSTOMERCODE);

                if (personalLoan.probability_of_Default != null)
                {
                    context.TBL_FACILITY_RATING.Add(new TBL_FACILITY_RATING
                    {
                        LOANAPPLICATIONDETAILID = loanApplicationDetailId,
                        PROBABILITYOFDEFAULT = personalLoan.probability_of_Default,
                        REMARK = personalLoan.remark,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffId,
                        DELETED = false,
                    });
                }
            }

            context.SaveChanges();
        }

        public void GetCreditCardsRetail(int loanApplicationDetailId, int customerId, int staffId)
        {
            //bool isGroup = false;
            var creditCard = new FacilityRatingViewModel();
            //var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            //if (application.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup) { isGroup = true; }
            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId).FirstOrDefault();

            if (customer.ISPROSPECT == false)
            {
                //creditCard = integration.GetCreditCardRetailByCustomerCode("006179612");
                creditCard = integration.GetCreditCardRetailByCustomerCode(customer.CUSTOMERCODE);

                if (creditCard.probability_of_Default != null)
                {
                    context.TBL_FACILITY_RATING.Add(new TBL_FACILITY_RATING
                    {
                        LOANAPPLICATIONDETAILID = loanApplicationDetailId,
                        PROBABILITYOFDEFAULT = creditCard.probability_of_Default,
                        REMARK = creditCard.remark,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        DATETIMECREATED = DateTime.Now,
                        CREATEDBY = staffId,
                        DELETED = false,
                    });
                }
            }
          
            context.SaveChanges();
        }

        public void ValidateLoanApplicationLimits(
                List<int> customerIds, 
                List<short> sectorIds, 
                int branchId,
                decimal applicationAmount, 
                int operationId
            )
        {
            int? branchOverrideRequestId = null;
            int? sectorOverrideRequestId = null;
            bool islastGate = operationId == (int)OperationsEnum.LoanAvailment;

            //var details = context.TBL_LOAN_APPLICATION_DETAIL
            //    .Where(x => x.LOANAPPLICATIONID == applicationId && x.DELETED == false && (x.STATUSID == (int)ApprovalStatusEnum.Approved || x.STATUSID == (int)ApprovalStatusEnum.Processing)
            //).ToList();

            foreach (var customerId in customerIds)
            {
                var branchOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                    .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride && x.ISUSED == false),
                        c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                    .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                    .FirstOrDefault();

                if (branchOverrideRequest != null) branchOverrideRequestId = branchOverrideRequest.id;

                var sectorOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                    .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride && x.ISUSED == false),
                        c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                    .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                    .FirstOrDefault();

                if (sectorOverrideRequest != null) sectorOverrideRequestId = sectorOverrideRequest.id;

                if (branchOverrideRequestId != null)
                {
                    if (islastGate)
                    {
                        var request = context.TBL_OVERRIDE_DETAIL.Find(branchOverrideRequestId);
                        request.ISUSED = true;
                        context.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                else
                {
                    if (limitValidation.BranchLimitExceeded(branchId,applicationAmount)) throw new SecureException("Branch NPL Limit exceeded!");
                }

                if (sectorOverrideRequestId != null)
                {
                    if (islastGate)
                    {
                        var request = context.TBL_OVERRIDE_DETAIL.Find(sectorOverrideRequestId);
                        request.ISUSED = true;
                        context.Entry(request).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                else
                {
                    // sector limits
                    // sectorId here is actually the subsectorId
                    // List<short> sectorIds = details.Select(x => x.SUBSECTORID).ToList();
                    foreach (var sectorId in sectorIds)
                    {
                        if (limitValidation.SectorLimitExceeded(sectorId, applicationAmount)) throw new SecureException("Sector Limit exceeded!");
                    }
                }
            }
        }

        public UserCurrencyViewFilter GetUserCurrencyViewFilter(int companyId, int userId)
        {
            UserCurrencyViewFilter result = new UserCurrencyViewFilter();
            var defaultCurrencyId = context.TBL_COMPANY.Where(x => x.CURRENCYID == companyId).Select(x => x).FirstOrDefault().CURRENCYID;
            var activities = admin.GetUserActivitiesByUser(userId);
            result.CanSeeLocalCurrency = activities.Contains("lcy-user");
            result.CanSeeForeignCurrency = activities.Contains("fcy-user");
            return result;
        }
    }
}
