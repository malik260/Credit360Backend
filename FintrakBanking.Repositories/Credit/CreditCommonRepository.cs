using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;


namespace FintrakBanking.Repositories.Credit
{
    public class CreditCommonRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IWorkflow workflow;
        private IIntegrationWithFinacle integration;

        public CreditCommonRepository(
            FinTrakBankingContext context, 
            IGeneralSetupRepository general, 
            IAuditTrailRepository audit, 
            IWorkflow workflow,
            IIntegrationWithFinacle integration
            )
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.workflow = workflow;
            this.integration = integration;
        }

        public void LoadCustomerTurnover(int applicationId, List<int> customerIds, int staffId, bool isLms = false) // OBIE (Page 4)
        {
            string duration = WebConfigurationManager.AppSettings["AccountStatisticsDurationInMonths"];
            int newDuration = 0;
            if (string.IsNullOrEmpty(duration))
            {
                duration = "6";
            }
            Int32.TryParse(duration, out newDuration);


            int turnoverDuration = newDuration;
            var apiTransactions = new List<ViewModels.ThridPartyIntegration.CustomerTurnoverViewModel>();
            var apiTransactionsOthers = new List<ViewModels.ThridPartyIntegration.CustomerTurnoverViewModel>();

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

                //Task.Run(async () => { apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();
                //Task.Run(async () => apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration)).GetAwaiter().GetResult();
                apiTransactions = integration.GetCustomerAccountTurnover(customer.CUSTOMERCODE, turnoverDuration);

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

            foreach (var customer in customers)
            {
                //Task.Run(async () => { itx = await _customerIntegration.GetCustomerInterestTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();

                apiTransactionsOthers = integration.GetCustomerAccountInterestTransactions(customer.CUSTOMERCODE, turnoverDuration);

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

            //if (context.SaveChanges() == 0) throw new SecureException("Customer turnover failed to load!");

            context.SaveChanges();

        }
    }
}
