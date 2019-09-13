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

            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID));//.Select(x => x.CUSTOMERCODE);
            
            foreach (var customer in customers)
            {
                if(customer.ISPROSPECT == false)
                {
                    var casa = context.TBL_CASA.Where(x => x.CUSTOMERID == customer.CUSTOMERID);
                    //Task.Run(async () => { apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();
                    //Task.Run(async () => apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration)).GetAwaiter().GetResult();
                    apiCustomerAccounts = integration.GetCustomerAccountsBalanceByCustomerCode(customer.CUSTOMERCODE);
                    //apiCustomerAccounts = integration.GetCustomerAccountsBalanceByCustomerCode("0689601167");
                    foreach (var account in apiCustomerAccounts) 
                    {
                        apiTransactions = integration.GetCustomerAccountTurnover(account.productAccountNumber, turnoverDuration);

                        foreach (var transaction in apiTransactions)
                        {

                            context.TBL_LOAN_APPLICATION_TRANS.Add(new TBL_LOAN_APPLICATION_TRANS
                            {
                                LOANAPPLICATIONID = applicationId,
                                CUSTOMERID = customer.CUSTOMERID,
                                CUSTOMERCODE = customer.CUSTOMERCODE,
                                ACCOUNTNUMBER = transaction.accountNumber,
                                PERIOD = transaction.period,
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
                    }
                    

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
            var apiTransactions = new List<ViewModels.ThridPartyIntegration.RatingAndRatioViewModel>();
            var apiCustomerRatio = new List<RatingAndRatioViewModel>();
            var apiTransactionsOthers = new List<ViewModels.ThridPartyIntegration.RatingAndRatioViewModel>();
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
                        context.TBL_CUSTOMER_RATIOS.Add(new TBL_CUSTOMER_RATIOS
                        {
                            DESCRIPTION = item.indicatorname,
                            VALUE = item.indicatorvalue,
                            CUSTOMERID = item.customerId,
                            LOANAPPLICATIONID = application.LOANAPPLICATIONID,
                            CUSTOMERGROUPID = application.CUSTOMERGROUPID,
                            DATETIMECREATED = DateTime.Now,
                            CREATEDBY = staffId,
                            DELETED = false,
                        });
                    }
                }
            }

            context.SaveChanges();
        }

        public void GetCorporateCustomerRating(List<int> customerIds, int staffId)
        {
            var apiTransactions = new List<ViewModels.ThridPartyIntegration.RatingAndRatioViewModel>();
            var apiCustomerRatio = new List<RatingAndRatioViewModel>();
            var apiTransactionsOthers = new List<ViewModels.ThridPartyIntegration.RatingAndRatioViewModel>();

            var customers = context.TBL_CUSTOMER.Where(x => customerIds.Contains(x.CUSTOMERID));

            foreach (var customer in customers)
            {
                if (customer.ISPROSPECT == false)
                {
                    apiCustomerRatio = integration.GetCustomerRatioByCustomerCode(customer.CUSTOMERCODE);
                    foreach (var item in apiCustomerRatio)
                    {
                        context.TBL_CUSTOMER_RATIOS.Add(new TBL_CUSTOMER_RATIOS
                        {
                            DESCRIPTION = item.indicatorname,
                            VALUE = item.indicatorvalue,
                            CUSTOMERID = item.customerId,
                            DATETIMECREATED = DateTime.Now,
                            CREATEDBY = staffId,
                            DELETED = false,
                        });
                    }
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
