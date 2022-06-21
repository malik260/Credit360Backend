using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Common.Enum;
using System.Linq;
using System;
using System.ServiceModel;
using System.Collections.Generic;
using FintrakBanking.ViewModels.Credit;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Credit;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using FintrakBanking.Common.CustomException;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using OfficeOpenXml;
using System.Data.Entity;

namespace FintrakBanking.Repositories.Finance

{
    public class FinanceTransactionRepository : IFinanceTransactionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        //private ILoanOperationsRepository creditOperations;
        private CustomerDetails customerInfo;
        private IIntegrationWithFinacle integration;
        private ITwoFactorAuthIntegrationService twoFactoeAuth;
        bool USE_THIRD_PARTY_INTEGRATION;
        bool USE_TWO_FACTOR_AUTHENTICATION;
        TBL_INTEGRATION_CONTROL globalIntegrationSetting = new TBL_INTEGRATION_CONTROL();
        public FinanceTransactionRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail, IIntegrationWithFinacle _integration,
                                            //ILoanOperationsRepository _creditOperations, 
                                            ITwoFactorAuthIntegrationService _twoFactoeAuth,
                                            FinTrakBankingContext _context, CustomerDetails customerInfo)
        {
            this.context = _context;
            this.customerInfo = customerInfo;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            this.integration = _integration;
            this.twoFactoeAuth = _twoFactoeAuth;
            //this.creditOperations = _creditOperations;
            globalIntegrationSetting = context.TBL_INTEGRATION_CONTROL.FirstOrDefault();
            var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (global != null)
            {
                USE_THIRD_PARTY_INTEGRATION = global.USE_THIRD_PARTY_INTEGRATION;
                USE_TWO_FACTOR_AUTHENTICATION = global.USE_TWO_FACTOR_AUTHENTICATION;
            }
        }

        private void UpdateCASABalances(int casaAccountId, decimal debitAmount, decimal creditAmount)
        {
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            if (debitAmount > 0)
            {
                account.LEDGERBALANCE = account.LEDGERBALANCE - debitAmount;
                account.AVAILABLEBALANCE = account.AVAILABLEBALANCE - debitAmount;
            }
            else
            {
                account.LEDGERBALANCE = account.LEDGERBALANCE + creditAmount;
                account.AVAILABLEBALANCE = account.AVAILABLEBALANCE + creditAmount;
            }
        }

        public CasaBalanceViewModel GetCASABalance(int casaAccountId)
        {
            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);
            var data = new CasaBalanceViewModel();
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                data = integration.GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER);
                //Task.Run(async () => { data = await customerInfo .GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER); }).GetAwaiter().GetResult();
                return data; //  new CasaBalanceViewModel { availableBalance = data.availableBalance, ledgerBalance = 0,accountStatusId = data.accountStatusId);
            }
            else
            {
                return new CasaBalanceViewModel { availableBalance = account.AVAILABLEBALANCE, ledgerBalance = account.LEDGERBALANCE, accountStatusId = (CASAAccountStatusEnum)account.ACCOUNTSTATUSID, currencyId = account.CURRENCYID, accountNo = account.PRODUCTACCOUNTNUMBER, accountName = account.PRODUCTACCOUNTNAME, hasBalance = true };
            }

        }

        public CasaBalanceViewModel GetCASABalance(string accountNumber, int companyId)
        {            
            var data = new CasaBalanceViewModel();
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                data = integration.GetCustomerAccountBalance(accountNumber);
                //Task.Run(async () => { data = await customerInfo .GetCustomerAccountBalance(account.PRODUCTACCOUNTNUMBER); }).GetAwaiter().GetResult();
                return data; //  new CasaBalanceViewModel { availableBalance = data.availableBalance, ledgerBalance = 0,accountStatusId = data.accountStatusId);
            }
            else
            {
                var account = context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == accountNumber && x.COMPANYID == companyId);
                if (account != null)
                    return new CasaBalanceViewModel {
                        availableBalance = account.AVAILABLEBALANCE,
                        ledgerBalance = account.LEDGERBALANCE,
                        accountStatusId = (CASAAccountStatusEnum)account.ACCOUNTSTATUSID,
                        currencyId = account.CURRENCYID,
                        accountNo = account.PRODUCTACCOUNTNUMBER,
                        accountName = account.PRODUCTACCOUNTNAME,
                        hasBalance = true,
                        isCasaAccountDetailAvailable = account.AVAILABLEBALANCE > 0 ? true : false };
                else
                    return data;
            }

        }

        public string GetCustomerAccountType(string accountNumber)
        {
            if (USE_THIRD_PARTY_INTEGRATION)
            {
                var type = integration.GetCustomerAccountBalance(accountNumber);
                return type.productType;
            }
            return null;
        }

        public CasaBalanceViewModel GetCASABalanceFromTransactions(int casaAccountId)
        {
            CasaBalanceViewModel balance = new CasaBalanceViewModel();

            var trans = (from data in context.TBL_FINANCE_TRANSACTION
                         where data.CASAACCOUNTID == casaAccountId
                         select data.CREDITAMOUNT - data.DEBITAMOUNT).Sum();

            var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            var lienBalance = GetLienBalance(account.PRODUCTACCOUNTNUMBER);

            balance.ledgerBalance = trans;
            balance.availableBalance = trans - lienBalance;

            return balance;
        }

        public decimal GetLienBalance(string productAccountNumber)
        {
            var balance = (from data in context.TBL_CASA_LIEN
                           where data.PRODUCTACCOUNTNUMBER == productAccountNumber
                           select data.LIENAMOUNT).Sum();

            return balance;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransactionBulk(List<FinanceTransactionViewModel> inputTransactions)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            //transaction.batchCode = batchCode;

            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);

            //transaction.transactionDetails.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            //transaction.transactionDetails.Sum(x => x.creditAmount);
            //var sumDebit = debitSum.FirstOrDefault();

            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");



            foreach (var item in inputTransactions)
            {
                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                }
                else
                { item.casaAccountId = null; }

                TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                trans.BATCHCODE = batchCode;
                //trans.OPERATIONID = mainItem.operationId;
                trans.OPERATIONID = (int)item.operationId;
                trans.DESCRIPTION = item.description;
                trans.VALUEDATE = item.valueDate;
                trans.POSTEDDATE = item.transactionDate;
                trans.CURRENCYID = item.currencyId;
                trans.CURRENCYRATE = item.currencyRate;
                trans.POSTEDDATETIME = DateTime.Now;
                trans.ISAPPROVED = item.isApproved;
                trans.POSTEDBY = item.postedBy;
                trans.APPROVEDBY = item.approvedBy;
                trans.APPROVEDDATE = item.approvedDate;
                trans.APPROVEDDATETIME = item.approvedDateTime;
                trans.SOURCEAPPLICATIONID = item.sourceApplicationId;
                trans.COMPANYID = item.companyId;
                trans.GLACCOUNTID = item.glAccountId;
                trans.SOURCEREFERENCENUMBER = item.sourceReferenceNumber;
                trans.CASAACCOUNTID = item.casaAccountId;
                trans.DEBITAMOUNT = item.debitAmount;
                trans.CREDITAMOUNT = item.creditAmount;
                trans.SOURCEBRANCHID = item.sourceBranchId;
                trans.DESTINATIONBRANCHID = item.destinationBranchId;

                transactions.Add(trans);
            }

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        public void UpdateCustomTransactions(string batchCode)
        {
            //var result = (from p in context.TBL_CUSTOM_FIANCE_TRANSACTION
            //              where p.BATCHCODE == batchCode && p.CONSUMED == false
            //                select new CustomFinanceTransactionViewModel()
            //                  {

            //                  });
            var result = context.TBL_CUSTOM_FIANCE_TRANSACTION.Where(x => x.CONSUMED == false && x.DATETIMECONSUMED == null && x.BATCHCODE.Contains(batchCode)).ToList();
            result.ForEach(a => a.CONSUMED = true);
            result.ForEach(a => a.DATETIMECONSUMED = DateTime.Now);

            context.SaveChanges();
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string PostTransaction(List<FinanceTransactionViewModel> inputTransactions, bool isBulkPosting = false, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) 
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");

            foreach (var item in inputTransactions)
            {
                item.batchCode = batchCode;

                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    //UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);


                    var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.casaAccountId).FirstOrDefault();

                    if (casa == null)
                        throw new ConditionNotMetException($"CASA account number {item.sourceReferenceNumber} does not exist in the CASA table for this transaction for GL Code {glInfo.ACCOUNTCODE}");

                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                }
                //else
                //{
                //    item.casaAccountId = null;
                //}

            }

            //api call
            if (USE_TWO_FACTOR_AUTHENTICATION && isBulkPosting == false)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if (twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactoeAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException(authenticated.message);
                }
            }

            string referenceCode = batchCode;
            if (USE_THIRD_PARTY_INTEGRATION && isBulkPosting == false && globalIntegrationSetting.USE_THIRPARTY_POSTING)
            {
                PostingResult response;
                response = integration.PostTransactions(inputTransactions);


                if (response.posted == true) // successful
                {
                    referenceCode = response.responseCode;
                    PostTransactionSub(batchCode, inputTransactions, transactions);
                    UpdateCustomTransactions(batchCode);
                }
                else
                {
                    throw new SecureException($"Transaction Failed.");
                }
            }
            else
            {
                PostTransactionSub(batchCode, inputTransactions, transactions);
            }

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            
             var result = context.SaveChanges() > 0;
            
            return referenceCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void PostTransactionSub(string batchCode, List<FinanceTransactionViewModel> inputTransactions, List<TBL_FINANCE_TRANSACTION> transactions)
        {
            foreach (var item in inputTransactions)
            {

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {

                    UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);
                }
                else
                { item.casaAccountId = null; }

                TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                trans.BATCHCODE = batchCode;
                //trans.OPERATIONID = mainItem.operationId;
                trans.OPERATIONID = (int)item.operationId;
                trans.DESCRIPTION = item.description;
                trans.VALUEDATE = item.valueDate;
                trans.POSTEDDATE = DateTime.Now;
                trans.CURRENCYID = item.currencyId;
                trans.CURRENCYRATE = item.currencyRate;
                trans.POSTEDDATETIME = DateTime.Now;
                trans.ISAPPROVED = item.isApproved;
                trans.POSTEDBY = item.postedBy;
                trans.APPROVEDBY = item.approvedBy;
                trans.APPROVEDDATE = item.approvedDateTime;
                trans.APPROVEDDATETIME = item.approvedDateTime;
                trans.SOURCEAPPLICATIONID = item.sourceApplicationId;
                trans.COMPANYID = item.companyId;
                trans.GLACCOUNTID = item.glAccountId;
                trans.SOURCEREFERENCENUMBER = item.sourceReferenceNumber;
                trans.CASAACCOUNTID = item.casaAccountId;
                trans.DEBITAMOUNT = item.debitAmount;
                trans.CREDITAMOUNT = item.creditAmount;
                trans.SOURCEBRANCHID = item.sourceBranchId;
                trans.DESTINATIONBRANCHID = item.destinationBranchId;
                trans.BATCHCODE2 = item.batchId;
                transactions.Add(trans);

            }
        }


        //[OperationBehavior(TransactionScopeRequired = true)]
        //public List<FinanceTransactionViewModel> PostCollateralSearch(CasaLienViewModel model)
        //{
        //    var lienSearchAmount = this.context.TBL_CASA_LIEN.FirstOrDefault(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).LIENAMOUNT;


        //    //var data = new TBL_CASA_LIEN
        //    //{
        //    //    PRODUCTACCOUNTNUMBER = model.productAccountNumber,
        //    //    LIENREFERENCENUMBER = model.lienReferenceNumber, //CommonHelpers.GenerateRandomDigitCode(10),
        //    //    BRANCHID = model.branchId,
        //    //    COMPANYID = model.companyId,
        //    //    LIENCREDITAMOUNT = 0,
        //    //    LIENDEBITAMOUNT = lienSearchAmount,//creditOperations.GetCollateralSearchChargeAmount(model.stateId),
        //    //    LIENTYPEID = (short)LienTypeEnum.CollateralSearch,
        //    //    CREATEDBY = model.createdBy,
        //    //    DESCRIPTION = "",
        //    //    DATECREATED = generalSetup.GetApplicationDate()
        //    //};

        //    //context.TBL_CASA_LIEN.Add(data);                       

        //    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.PRODUCTACCOUNTNUMBER == model.productAccountNumber && x.COMPANYID == model.companyId);

        //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
        //    debit.operationId = (int)OperationsEnum.CollateralSearch;
        //    debit.description = "Customer Collateral Search Charge";
        //    debit.valueDate = generalSetup.GetApplicationDate();
        //    debit.transactionDate = debit.valueDate;
        //    debit.currencyId = casa.CURRENCYID;
        //    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //    debit.isApproved = true;
        //    debit.postedBy = model.createdBy;
        //    debit.approvedBy = model.createdBy;
        //    debit.approvedDate = debit.transactionDate;
        //    debit.approvedDateTime = DateTime.Now;
        //    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    debit.companyId = model.companyId;

        //    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
        //    debit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
        //    debit.casaAccountId = casa.CASAACCOUNTID;
        //    debit.debitAmount = lienSearchAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = model.branchId;
        //    debit.destinationBranchId = casa.BRANCHID;


        //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //    credit.operationId = (int)OperationsEnum.CollateralSearch;
        //    credit.description = "Customer Collateral Search Charge";
        //    credit.valueDate = debit.valueDate;
        //    credit.transactionDate = debit.valueDate;
        //    credit.currencyId = casa.CURRENCYID;
        //    credit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //    credit.isApproved = true;
        //    credit.postedBy = model.createdBy;
        //    credit.approvedBy = model.createdBy;
        //    credit.approvedDate = debit.transactionDate;
        //    credit.approvedDateTime = DateTime.Now;
        //    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    credit.companyId = model.companyId;

        //    var chargeGL = this.context.TBL_COLLATERAL_TYPE.FirstOrDefault(x => x.COLLATERALTYPEID == (int)CollateralTypeEnum.Property).CHARGEGLACCOUNTID.Value;
        //    credit.glAccountId = chargeGL;

        //    credit.sourceReferenceNumber = casa.PRODUCTACCOUNTNUMBER;
        //    credit.casaAccountId = null;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = lienSearchAmount;
        //    credit.sourceBranchId = model.branchId;
        //    credit.destinationBranchId = model.branchId;


        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
        //    inputTransactions.Add(debit);
        //    inputTransactions.Add(credit);
        //    PostTransaction(inputTransactions);

        //    // Audit Section ---------------------------            

        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.LienAdded,
        //        STAFFID = model.createdBy,
        //        BRANCHID = model.branchId,
        //        DETAIL = $"Applied for lien with reference number: {model.lienReferenceNumber}",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = generalSetup.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);

        //    //end of Audit section -------------------------------
        //    context.SaveChanges();
        //    return null;

        //}


        public List<FinanceTransactionViewModel> BuildLoanContingentFeesReversal(int loanApplicationDetailId, int staffId)
        {
            var record = context.TBL_LOAN_FEE.Where(a => a.LOANID == loanApplicationDetailId && a.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.LineFacility);// 4);
            //var loanAppDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONDETAILID == loanApplicationDetailId ).FirstOrDefault();// 4);

            var loanApplication = (from a in context.TBL_LOAN_APPLICATION
                                   join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                   where b.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                   select new
                                   {
                                       b.CURRENCYID,
                                       b.TBL_CURRENCY.CURRENCYCODE,
                                       a.BRANCHID,
                                       a.APPLICATIONREFERENCENUMBER,
                                       a.COMPANYID,

                                   }).FirstOrDefault();

            //LoanViewModel loanDetails = new LoanViewModel();



            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            //TBL_LOAN loanTable = new TBL_LOAN();
            //var bookingRequestDetails = context.TBL_LOAN_BOOKING_REQUEST.FirstOrDefault(x => x.LOAN_BOOKING_REQUESTID == loanDetails.loanBookingRequestId);

            //var company = context.TBL_COMPANY.Find(loanDetails.companyId);
            //  foreach (var item in loanDetails.loanChargeFee)
            // {

            foreach (var loanFee in record.ToList())
            {
                if (loanFee.ISPOSTED == true && loanFee.FEEAMOUNT > 0)
                {

                    //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanFee.CASAACCOUNTID);

                    var feeDetailInfo = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == loanFee.CHARGEFEEID && details.DETAILTYPEID == (int)ChargeFeeDetailTypeEnum.Primary select details).FirstOrDefault();


                    //var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == loanFee.CHARGEFEEID && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    //foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                    //{
                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                    //decimal debitAmount = 0;
                    //if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                    //    debitAmount = (decimal)loanFee.FEEAMOUNT * (decimal)(debits.VALUE / 100.0);
                    //else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                    //    debitAmount = (decimal)debits.VALUE;

                    debit.operationId = (int)OperationsEnum.CancelContingentLiability;
                    debit.description = $"Contingent Liability Fee Recorgnition";
                    debit.valueDate = generalSetup.GetApplicationDate();
                    debit.transactionDate = debit.valueDate;
                    debit.currencyId = loanApplication.CURRENCYID;
                    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, loanApplication.COMPANYID).sellingRate;
                    debit.isApproved = true;
                    debit.postedBy = staffId;
                    debit.approvedBy = staffId;
                    debit.approvedDate = debit.transactionDate;
                    debit.approvedDateTime = DateTime.Now;
                    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    debit.companyId = loanApplication.COMPANYID;


                    debit.glAccountId = (int)feeDetailInfo.GLACCOUNTID3;
                    debit.sourceReferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                    debit.batchCode = batchCode;
                    debit.casaAccountId = null;
                    debit.debitAmount = loanFee.FEEAMOUNT;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = loanApplication.BRANCHID;
                    debit.destinationBranchId = loanApplication.BRANCHID;

                    debit.rateCode = "TTB"; //loanDetails.nostroRateCode;
                    debit.rateUnit = string.Empty;
                    debit.currencyCrossCode = loanApplication.CURRENCYCODE;// casa.TBL_CURRENCY.CURRENCYCODE;

                    inputTransactions.Add(debit);
                    //}

                    //foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                    //{
                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    //decimal creditAmount = 0;
                    //if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                    //    creditAmount = (decimal)loanFee.FEEAMOUNT * (decimal)(credits.VALUE / 100.0);
                    //else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                    //    creditAmount = (decimal)credits.VALUE;


                    credit.operationId = (int)OperationsEnum.CancelContingentLiability;
                    credit.description = $"Contingent Liability Fee Recorgnition";
                    credit.valueDate = generalSetup.GetApplicationDate();
                    credit.transactionDate = credit.valueDate;
                    credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == loanApplication.COMPANYID).CURRENCYID; //(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, loanDetails.companyId); //casa.CURRENCYID;
                    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, loanApplication.COMPANYID).sellingRate;
                    credit.isApproved = true;
                    credit.postedBy = staffId;
                    credit.approvedBy = staffId;
                    credit.approvedDate = credit.transactionDate;
                    credit.approvedDateTime = DateTime.Now;
                    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    credit.companyId = loanApplication.COMPANYID;
                    credit.glAccountId = feeDetailInfo.GLACCOUNTID1.Value;
                    credit.sourceReferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                    credit.batchCode = batchCode;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = loanFee.FEEAMOUNT;
                    credit.sourceBranchId = loanApplication.BRANCHID;
                    credit.destinationBranchId = loanApplication.BRANCHID;
                    credit.rateCode = "TTB"; //loanDetails.nostroRateCode;
                    credit.rateUnit = string.Empty;
                    credit.currencyCrossCode = loanApplication.CURRENCYCODE;

                    inputTransactions.Add(credit);
                    //}

                }
            }




            //}
            return inputTransactions;
        }


        public CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId)
        {
            if (currencyId == 0)
            {
                return new CurrencyExchangeRateViewModel();
            }
            var systemDate = generalSetup.GetApplicationDate();
            var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;


            if (USE_THIRD_PARTY_INTEGRATION)
            {
                
                if (currencyId == baseCurrency)
                {
                    return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true };
                }
                else
                {
                    var toCurrencyCode = this.context.TBL_CURRENCY.FirstOrDefault(x => x.CURRENCYID == baseCurrency).CURRENCYCODE;
                    var fromCurrencyCode = this.context.TBL_CURRENCY.Where(x => x.CURRENCYID == currencyId).Select(f => f.CURRENCYCODE).FirstOrDefault();
                    var rateCode = "TT";
                    var today = DateTime.Now.Date;
                    var exRate = GetExchangeRateStaging(date, currencyId, baseCurrency, rateCode);
                    if (today > exRate.date.Date)
                    {
                        var rate = integration.GetExchangeRate(fromCurrencyCode, toCurrencyCode, rateCode);
                        if (rate.sellingRate <= 0)
                        {
                            return exRate;
                        }
                        UpdateExchangeRate(rate, currencyId, rateCode, baseCurrency);
                        return rate;
                    }
                    else
                    {
                        return exRate;
                    }


                }

                //return data;
            }
            else
            {
                var rateCode = "TT";
                //var baseCurrency = this.context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;

                //CurrencyExchangeRateViewModel rateInfo = new CurrencyExchangeRateViewModel();
                return GetExchangeRateStaging(date, currencyId, baseCurrency, rateCode);
                //if (currencyId == baseCurrency)
                //{
                //    return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true };
                //}
                //else
                //{
                //    //DateTime systemDate = generalSetup.GetApplicationDate();
                //    //DateTime date = generalSetup.GetApplicationDate().Date;
                //    var rateInfo = (from x in this.context.TBL_CURRENCY_EXCHANGERATE
                //                    where x.CURRENCYID == currencyId && x.DATE == systemDate && x.RATECODEID == 1
                //                    select x).FirstOrDefault();

                //    if (rateInfo == null)
                //        throw new ConditionNotMetException($"Exchange rate for {generalSetup.GetApplicationDate()} is not defined. Define the exchange rate and try again");

                //    return new CurrencyExchangeRateViewModel
                //    {
                //        baseCurrencyId = rateInfo.BASECURRENCYID,
                //        currencyId = rateInfo.CURRENCYID,
                //        buyingRate = rateInfo.EXCHANGERATE,
                //        sellingRate = rateInfo.EXCHANGERATE,
                //        date = rateInfo.DATE,
                //        isBaseCurrency = false
                //    };
                //}
            }
        }

        private CurrencyExchangeRateViewModel GetExchangeRateStaging(DateTime date, short currencyId, short baseCurrency, string rateCode)
        {
            var currency = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == currencyId);
            var exchangeRateCode = context.TBL_CURRENCY_RATECODE.FirstOrDefault(r => r.RATECODE.Trim() == rateCode);
            //var systemDate = generalSetup.GetApplicationDate();
            if (currencyId == baseCurrency)
            {
                return new CurrencyExchangeRateViewModel { baseCurrencyId = baseCurrency, currencyId = currencyId, buyingRate = 1, sellingRate = 1, date = date, isBaseCurrency = true };
            }
            else
            {
                //DateTime systemDate = generalSetup.GetApplicationDate();
                //DateTime date = generalSetup.GetApplicationDate().Date;
                var rateCodeId = exchangeRateCode?.RATECODEID ?? 0;
                var rateInfo = (from x in this.context.TBL_CURRENCY_EXCHANGERATE
                                where x.CURRENCYID == currencyId && x.RATECODEID == rateCodeId 
                                select x).OrderByDescending(x => x.CURRENCYRATEID).FirstOrDefault();

                if (rateInfo == null)
                {
                    if (currency == null)
                    {
                        throw new ConditionNotMetException($"Currency is not defined. Define the Currency First and try again");
                    }
                    throw new ConditionNotMetException($"Exchange rate for {currency.CURRENCYNAME} is not defined. Define the exchange rate and try again");
                    //throw new ConditionNotMetException($"Exchange rate for {generalSetup.GetApplicationDate()} is not defined. Define the exchange rate and try again");
                }

                return new CurrencyExchangeRateViewModel
                {
                    baseCurrencyId = rateInfo.BASECURRENCYID,
                    currencyId = rateInfo.CURRENCYID,
                    buyingRate = rateInfo.EXCHANGERATE,
                    sellingRate = rateInfo.EXCHANGERATE,
                    date = rateInfo.DATE,
                    isBaseCurrency = false
                };
            }
        }

        private bool UpdateExchangeRate(CurrencyExchangeRateViewModel rate, short currencyId, string rateCode, short baseCurrency)
        {
            var exchangeRateCode = context.TBL_CURRENCY_RATECODE.FirstOrDefault(r => r.RATECODE.Trim() == rateCode);
            var currencyRate = context.TBL_CURRENCY_EXCHANGERATE.FirstOrDefault(r => r.CURRENCYID == currencyId && r.RATECODEID == exchangeRateCode.RATECODEID && r.DELETED == false);
            if (currencyRate != null)
            {
                currencyRate.DATE = DateTime.Now;
                currencyRate.CURRENCYID = currencyId;
                currencyRate.EXCHANGERATE = rate.sellingRate;
                currencyRate.BASECURRENCYID = baseCurrency;
                currencyRate.DATETIMEUPDATED = DateTime.Now;
                currencyRate.RATECODEID = exchangeRateCode.RATECODEID;
            }else
            {
                var newRate = new TBL_CURRENCY_EXCHANGERATE()
                {
                    DATE = DateTime.Now,
                    CURRENCYID = currencyId,
                    EXCHANGERATE = rate.exchangeRate,
                    BASECURRENCYID = baseCurrency,
                    DATETIMECREATED = DateTime.Now,
                    RATECODEID = exchangeRateCode.RATECODEID,
                    CREATEDBY = 0,
                    DELETED = false

                };
                context.TBL_CURRENCY_EXCHANGERATE.Add(newRate);
            }
            return context.SaveChanges() > 0;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PostDailyWriteoffLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {

            //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            var accounts = context.TBL_OTHER_OPERATION_ACCOUNT.Where(x => x.OTHEROPERATIONID == (int)OtherOperationEnum.InterestOffBalansheetCompleteWriteOffAccount).FirstOrDefault();


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyWriteoffInterestAccural;
            debit.description = "Loan Daily Write-off Interest Accrual Posting";
            debit.valueDate = model.date; //generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = accounts.GLACCOUNTID;
            debit.sourceReferenceNumber = model.referenceNumber; //product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyWriteoffInterestAccural;
            credit.description = "Loan Daily Write-off Interest Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = accounts.GLACCOUNTID2.Value;

            credit.sourceReferenceNumber = model.referenceNumber;  //product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            var batchPost = PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = (int)SystemStaff.System,//model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Loan Daily Write-off Interest Accrual Posting: {model.referenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            if (batchPost != null)
            {
                var result = context.SaveChanges() > 0;
                return result;
            }
            return false;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public bool PostDailyLoansInterestAccrual(DailyInterestAccrualViewModel model)

        {

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Loan Daily Interest Accrual Posting";
            debit.valueDate = model.date; //generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = model.referenceNumber; //product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;
            debit.createdBy = model.createdBy;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Loan Daily Interest Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;

            credit.sourceReferenceNumber = model.referenceNumber;  //product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;
            credit.createdBy = model.createdBy;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            var batchPost = PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = (int)SystemStaff.System,//model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Loan Daily Interest Accrual Posting: {model.referenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            if (batchPost != null)
            {
                var result = context.SaveChanges() > 0;
                return result;
            }
            return false;

        }

        public FinanceTransactionViewModel PostDailyFeeAccrual(DailyInterestAccrualViewModel model)

        {
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Daily Amortized Fee Posting";
            debit.valueDate = model.date; //generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Daily Amortized Fee Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;

            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Daily Amortized Fee Posting: {model.referenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyAuthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)
        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Authorised Overdraft Daily Interest Accrual Posting";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Authorised Overdraft Daily Interest Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Authorised Overdraft Daily Interest Accrual Posting: {model.referenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyUnauthorisedOverdraftInterestAccrual(DailyInterestAccrualViewModel model)
        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Unauthorised Overdraft Daily Interest Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Unauthorised Overdraft Daily Interest Accrual Posting: {product.PRODUCTCODE}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyPastDueInterestAccrual(DailyInterestAccrualViewModel model)
        {

            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Past Due Daily Interest Accrual Posting";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Past Due Daily Interest Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
            //    STAFFID = model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"Past Due Daily Interest Accrual Posting: {model.referenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostDailyPastDuePrincipalAccrual(DailyInterestAccrualViewModel model)
        {
            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.DailyInterestAccural;
            debit.description = "Past Due Daily Principal Accrual Posting";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = product.PRODUCTCODE;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.DailyInterestAccural;
            credit.description = "Past Due Daily Principal Accrual Posting";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.INTERESTINCOMEEXPENSEGL.Value;
            credit.sourceReferenceNumber = product.PRODUCTCODE;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);
            //context.SaveChanges();
            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LoanDailyPrincipalAccrual,
            //    STAFFID = model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"Past Due Daily Principal Accrual Posting: {model.referenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        public FinanceTransactionViewModel PostBuildLoanRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            // FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);

            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            return null;

        }

        public FinanceTransactionViewModel PostBuildAuthorisedOverdraftRepaymentPosting(LoanRepaymentViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId && x.COMPANYID == model.companyId);
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanRefNo;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.paymentDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = model.loanRefNo;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;



            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            


            return null;

        }

        public bool PostBuildLoanChargeFeesPosting(LoanViewModel model)

        {
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            //var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId orderby details.POSTINGGROUP select details ).ToList();

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)model.totalAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    debit.operationId = (int)OperationsEnum.TermLoanBooking;//// to be change 
                    debit.description = $"Fee charge on {debits.DESCRIPTION}";
                    debit.valueDate = generalSetup.GetApplicationDate();
                    debit.transactionDate = debit.valueDate;
                    debit.currencyId = casa.CURRENCYID;
                    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                    debit.isApproved = true;
                    debit.postedBy = model.createdBy;
                    debit.approvedBy = model.createdBy;
                    debit.approvedDate = debit.transactionDate;
                    debit.approvedDateTime = DateTime.Now;
                    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    debit.companyId = model.companyId;

                    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                    debit.sourceReferenceNumber = model.loanReferenceNumber;
                    debit.casaAccountId = casa.CASAACCOUNTID;
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = model.branchId;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)model.totalAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    credit.operationId = (int)OperationsEnum.TermLoanBooking;
                    credit.description = $"Fee charge on {credits.DESCRIPTION}";
                    credit.valueDate = generalSetup.GetApplicationDate();
                    credit.transactionDate = credit.valueDate;
                    credit.currencyId = casa.CURRENCYID;
                    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                    credit.isApproved = true;
                    credit.postedBy = model.createdBy;
                    credit.approvedBy = model.createdBy;
                    credit.approvedDate = credit.transactionDate;
                    credit.approvedDateTime = DateTime.Now;
                    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    credit.companyId = model.companyId;
                    credit.glAccountId = (int)credits.GLACCOUNTID1;
                    credit.sourceReferenceNumber = model.loanReferenceNumber;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = model.branchId;
                    credit.destinationBranchId = model.branchId;

                    inputTransactions.Add(credit);

                }
            }


            var batchPost = PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Interval Fees and Commission charge Posting : {model.loanReferenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            //context.SaveChanges();
            //return null;

            if (batchPost != null)
            {
                var result = context.SaveChanges() > 0;
                return result;
            }
            return false;

        }

        public List<FinanceTransactionViewModel> BuildLoanPrepaymentPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            //PostTransaction(inputTransactions, false, twoFactorAuth);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return inputTransactions;

        }

        public FinanceTransactionViewModel PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

                    debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    debit.description = description; // "Loan Disbursment Amount";
                    debit.valueDate = generalSetup.GetApplicationDate();
                    debit.transactionDate = debit.valueDate;
                    debit.currencyId = casa.CURRENCYID;
                    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                    debit.isApproved = true;
                    debit.postedBy = model.createdBy;
                    debit.approvedBy = model.createdBy;
                    debit.approvedDate = debit.transactionDate;
                    debit.approvedDateTime = DateTime.Now;
                    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    debit.companyId = model.companyId;
                    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                    debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                    debit.casaAccountId = casa.CASAACCOUNTID;
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = loan.BRANCHID;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                    credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    credit.description = description; // "Loan Disbursment Amount";
                    credit.valueDate = generalSetup.GetApplicationDate();
                    credit.transactionDate = credit.valueDate;
                    credit.currencyId = casa.CURRENCYID;
                    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                    credit.isApproved = true;
                    credit.postedBy = model.createdBy;
                    credit.approvedBy = model.createdBy;
                    credit.approvedDate = credit.transactionDate;
                    credit.approvedDateTime = DateTime.Now;
                    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    credit.companyId = model.companyId;
                    credit.glAccountId = credits.GLACCOUNTID1.Value;
                    credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = loan.BRANCHID;
                    credit.destinationBranchId = loan.BRANCHID;

                    inputTransactions.Add(credit);
                }
            }
            PostTransaction(inputTransactions);

            return null;


        }

        //public List<FinanceTransactionViewModel> PostBuildLoanPrepaymentFeePosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int chargeFeeId, string description, int operationId)
        //{
        //    FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

        //    //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

        //    var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

        //    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

        //    var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

        //    foreach (var item in postingGroups)
        //    {
        //        var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

        //        foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
        //        {
        //            decimal debitAmount = 0;
        //            if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
        //                debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
        //            else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
        //                debitAmount = (decimal)debits.VALUE;

        //            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

        //            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
        //            debit.description = description; // "Loan Disbursment Amount";
        //            debit.valueDate = generalSetup.GetApplicationDate();
        //            debit.transactionDate = debit.valueDate;
        //            debit.currencyId = casa.CURRENCYID;
        //            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //            debit.isApproved = true;
        //            debit.postedBy = model.createdBy;
        //            debit.approvedBy = model.createdBy;
        //            debit.approvedDate = debit.transactionDate;
        //            debit.approvedDateTime = DateTime.Now;
        //            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //            debit.companyId = model.companyId;
        //            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
        //            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
        //            debit.casaAccountId = casa.CASAACCOUNTID;
        //            debit.debitAmount = debitAmount;
        //            debit.creditAmount = 0;
        //            debit.sourceBranchId = loan.BRANCHID;
        //            debit.destinationBranchId = casa.BRANCHID;

        //            inputTransactions.Add(debit);
        //        }

        //        foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
        //        {
        //            decimal creditAmount = 0;
        //            if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
        //                creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
        //            else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
        //                creditAmount = (decimal)credits.VALUE;

        //            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
        //            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
        //            credit.description = description; // "Loan Disbursment Amount";
        //            credit.valueDate = generalSetup.GetApplicationDate();
        //            credit.transactionDate = credit.valueDate;
        //            credit.currencyId = casa.CURRENCYID;
        //            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
        //            credit.isApproved = true;
        //            credit.postedBy = model.createdBy;
        //            credit.approvedBy = model.createdBy;
        //            credit.approvedDate = credit.transactionDate;
        //            credit.approvedDateTime = DateTime.Now;
        //            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //            credit.companyId = model.companyId;
        //            credit.glAccountId = credits.GLACCOUNTID1.Value;
        //            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
        //            credit.casaAccountId = null;
        //            credit.debitAmount = 0;
        //            credit.creditAmount = creditAmount;
        //            credit.sourceBranchId = loan.BRANCHID;
        //            credit.destinationBranchId = loan.BRANCHID;

        //            inputTransactions.Add(credit);
        //        }
        //    }

        //    //PostTransaction(inputTransactions);

        //    return inputTransactions;


        //}


        public List<FinanceTransactionViewModel> BuildContingentChargeFeePosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {
            FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId && x.COMPANYID == model.companyId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();

            foreach (var item in postingGroups)
            {
                var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId && details.POSTINGGROUP == item orderby details.POSTINGTYPEID select details).ToList();

                foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                {
                    decimal debitAmount = 0;
                    if (debits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        debitAmount = (decimal)postedAmount * (decimal)(debits.VALUE / 100.0);
                    else if (debits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        debitAmount = (decimal)debits.VALUE;

                    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

                    //debit.batchCode = batchCode;
                    debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    debit.description = description; // "Loan Disbursment Amount";
                    debit.valueDate = applicationDate;
                    debit.transactionDate = debit.valueDate;
                    debit.currencyId = casa.CURRENCYID;
                    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                    debit.isApproved = true;
                    debit.postedBy = model.createdBy;
                    debit.approvedBy = model.createdBy;
                    debit.approvedDate = debit.transactionDate;
                    debit.approvedDateTime = DateTime.Now;
                    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    debit.companyId = model.companyId;
                    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                    debit.sourceReferenceNumber = sourceReferenceNumber;
                    debit.casaAccountId = casa.CASAACCOUNTID;
                    debit.debitAmount = debitAmount;
                    debit.creditAmount = 0;
                    debit.sourceBranchId = loan.BRANCHID;
                    debit.destinationBranchId = casa.BRANCHID;

                    inputTransactions.Add(debit);
                }

                foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                {
                    decimal creditAmount = 0;
                    if (credits.FEETYPEID == (int)FeeTypeEnum.Rate)
                        creditAmount = (decimal)postedAmount * (decimal)(credits.VALUE / 100.0);
                    else if (credits.FEETYPEID == (int)FeeTypeEnum.Amount)
                        creditAmount = (decimal)credits.VALUE;

                    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

                    //credit.batchCode = batchCode;
                    credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                    credit.description = description; // "Loan Disbursment Amount";
                    credit.valueDate = applicationDate;
                    credit.transactionDate = credit.valueDate;
                    credit.currencyId = casa.CURRENCYID;
                    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                    credit.isApproved = true;
                    credit.postedBy = model.createdBy;
                    credit.approvedBy = model.createdBy;
                    credit.approvedDate = credit.transactionDate;
                    credit.approvedDateTime = DateTime.Now;
                    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                    credit.companyId = model.companyId;
                    credit.glAccountId = credits.GLACCOUNTID1.Value;
                    credit.sourceReferenceNumber = sourceReferenceNumber;
                    credit.casaAccountId = null;
                    credit.debitAmount = 0;
                    credit.creditAmount = creditAmount;
                    credit.sourceBranchId = loan.BRANCHID;
                    credit.destinationBranchId = loan.BRANCHID;

                    inputTransactions.Add(credit);
                }
            }

            //PostTransaction(inputTransactions);

            return inputTransactions;

        }
        public List<FinanceTransactionViewModel> BuildContingentPrincipalPostingReduction(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID);

            //var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.PRINCIPALBALANCEGL2.Value;
            debit.sourceReferenceNumber = sourceReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loan.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = sourceReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }
        public List<FinanceTransactionViewModel> BuildContingentPrincipalPosting(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            //var feeDetails = context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId);            

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID);

            //var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = sourceReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loan.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.PRINCIPALBALANCEGL2.Value;
            credit.sourceReferenceNumber = sourceReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildContingentPrincipalPostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, string description, int operationId)
        {
            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var product = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = product.PRINCIPALBALANCEGL2.Value;
            debit.sourceReferenceNumber = sourceReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = loan.CONTINGENTAMOUNT;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loan.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = product.PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = sourceReferenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = loan.CONTINGENTAMOUNT;
            credit.sourceBranchId = loan.BRANCHID;
            credit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildContingentUnEarnedFeePostingReversal(LoanPaymentRestructureScheduleInputViewModel model, string sourceReferenceNumber, decimal postedAmount, int chargeFeeId, string description, int operationId)
        {

            var loan = this.context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CONTINGENTLOANID == model.loanId);

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loan.CASAACCOUNTID && x.COMPANYID == model.companyId);

            var casaProduct = this.context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID);

            var feeGL = this.context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(x => x.CHARGEFEEID == chargeFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Primary);

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            //var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var applicationDate = generalSetup.GetApplicationDate();



            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //debit.batchCode = batchCode;
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = applicationDate;
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loan.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = feeGL.GLACCOUNTID1.Value;
            debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loan.BRANCHID;
            debit.destinationBranchId = loan.BRANCHID;

            inputTransactions.Add(debit);




            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //credit.batchCode = batchCode;
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = applicationDate;
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = casaProduct.PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;

            inputTransactions.Add(credit);


            //PostTransaction(inputTransactions);

            return inputTransactions;

        }

        public List<FinanceTransactionViewModel> BuildLoanChargeFeesPostingReversal(LoanViewModel loanDetails, LoanChargeFeeViewModel feeModel)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var feeDescription = "FTK" + loanDetails.loanReferenceNumber + " fees reversal";
            if (feeDescription.Length > 40)
            {
                feeDescription = feeDescription.Substring(0, 40);
            }

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            var company = context.TBL_COMPANY.Find(loanDetails.companyId);
            if (feeModel.isPosted == false && feeModel.feeAmount != 0)
            {

                var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanDetails.casaAccountId);

                if (company.CURRENCYID != loanDetails.currencyId && loanDetails.casaAccountId2 != null)
                {
                    //FOREIGN LOANS FEES ARE TAKEN FROM CASAACCOUNT SELECTED @BOOKING REQUEST
                    casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanDetails.casaAccountId);
                }

                var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == feeModel.chargeFeeId select details.POSTINGGROUP).Distinct().ToList();

                foreach (var post in postingGroups)
                {
                    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == feeModel.chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

                    foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
                    {
                        decimal debitAmount = (decimal)feeModel.feeAmount;
                        if (debits.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax)
                        {
                            feeDescription = "FTK" + loanDetails.loanReferenceNumber + " VAT reversal";
                            debitAmount = (decimal)debitAmount * (decimal)(debits.VALUE / 100.0);
                            if (feeDescription.Length > 40)
                            {
                                feeDescription = feeDescription.Substring(0, 40);
                            }
                        }

                        FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
                        debit.operationId = (int)loanDetails.operationId;
                        debit.description = feeDescription; // $"Fee charge on {debits.DESCRIPTION}";
                        debit.valueDate = generalSetup.GetApplicationDate();
                        debit.transactionDate = debit.valueDate;
                        debit.currencyId = casa.CURRENCYID;
                        debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, loanDetails.companyId).sellingRate;
                        debit.isApproved = true;
                        debit.postedBy = loanDetails.createdBy;
                        debit.approvedBy = loanDetails.createdBy;
                        debit.approvedDate = debit.transactionDate;
                        debit.approvedDateTime = DateTime.Now;
                        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        debit.companyId = loanDetails.companyId;

                        var creditedChargeFeeRecord = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == feeModel.chargeFeeId && x.POSTINGTYPEID == (short)GLPostingTypeEnum.Credit).FirstOrDefault();


                        debit.glAccountId = (int)creditedChargeFeeRecord.GLACCOUNTID1;

                        //debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                        debit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                        debit.batchCode = batchCode;
                        debit.casaAccountId = null;
                        debit.debitAmount = debitAmount;
                        debit.creditAmount = 0;
                        debit.sourceBranchId = loanDetails.branchId;
                        debit.destinationBranchId = casa.BRANCHID;

                        debit.rateCode = "TTB";
                        debit.rateUnit = string.Empty;
                        debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

                        inputTransactions.Add(debit);
                    }

                    foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
                    {
                        decimal creditAmount = (decimal)feeModel.feeAmount;

                        if (credits.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax)
                        {
                            feeDescription = "FTK" + loanDetails.loanReferenceNumber + " VAT reversal";
                            creditAmount = (decimal)creditAmount * (decimal)(credits.VALUE / 100.0);
                            if (feeDescription.Length > 40)
                            {
                                feeDescription = feeDescription.Substring(0, 40);
                            }
                        }

                        FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
                        credit.operationId = (int)loanDetails.operationId;
                        credit.description = feeDescription;
                        credit.valueDate = generalSetup.GetApplicationDate();
                        credit.transactionDate = credit.valueDate;
                        credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == loanDetails.companyId).CURRENCYID;
                        credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, loanDetails.companyId).sellingRate;
                        credit.isApproved = true;
                        credit.postedBy = loanDetails.createdBy;
                        credit.approvedBy = loanDetails.createdBy;
                        credit.approvedDate = credit.transactionDate;
                        credit.approvedDateTime = DateTime.Now;
                        credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                        credit.companyId = loanDetails.companyId;
                        credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;  //(int)credits.GLACCOUNTID1;
                        credit.sourceReferenceNumber = loanDetails.loanReferenceNumber;
                        credit.batchCode = batchCode;
                        credit.casaAccountId = casa.CASAACCOUNTID;
                        credit.debitAmount = 0;
                        credit.creditAmount = creditAmount;
                        credit.sourceBranchId = loanDetails.branchId;
                        credit.destinationBranchId = loanDetails.branchId;
                        credit.rateCode = "TTB";
                        credit.rateUnit = string.Empty;
                        credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;


                        inputTransactions.Add(credit);
                    }
                }
            }
            return inputTransactions;
        }


        public List<FinanceTransactionViewModel> BuildChargeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, TwoFactorAutheticationViewModel twoFactorAuth, string postType)
        {

            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == model.casaAccountId);
            var postingGroups = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId 
                                 && details.DETAILTYPEID != (short)ChargeFeeDealTypeEnum.Tax
                                 && details.DETAILTYPEID != (short)ChargeFeeDealTypeEnum.Others
                                 select details.POSTINGGROUP).Distinct().ToList();
            var feeDescription = "Charge Reversal";
            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            decimal debitAmount = (decimal)model.payAmount;

            //if (debits.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax)
            //{
            //    feeDescription = "VAT Reversal";
            //    debitAmount = (decimal)model.feeAmountDiff * (decimal)(debits.VALUE / 100.0);
            //}

            var feeVATRecord = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == model.chargeFeeId && x.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax && x.DELETED == false).FirstOrDefault();

            if (postType == "VAT")
            {
                feeDescription = "Charge Reversal (" + feeVATRecord.DESCRIPTION + ")";
                debitAmount = (decimal)model.payAmount * (decimal)(feeVATRecord.VALUE / 100.0);
            }
            debit.operationId = (int)model.operationId;
            debit.description = feeDescription; //feeDescription; // $"Fee charge on {debits.DESCRIPTION}";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            var creditedChargeFeeRecord = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == model.chargeFeeId && x.POSTINGTYPEID == (short)GLPostingTypeEnum.Credit).FirstOrDefault();


            debit.glAccountId = (int)creditedChargeFeeRecord.GLACCOUNTID1;
            debit.sourceReferenceNumber = model.sourceReferenceNumber;
            debit.batchCode = batchCode;
            debit.casaAccountId = null;
            debit.debitAmount = debitAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.sourceBranchId;
            debit.destinationBranchId = model.sourceBranchId;

            debit.rateCode = "TTB"; //loanDetails.nostroRateCode;
            debit.rateUnit = string.Empty;
            debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

            inputTransactions.Add(debit);

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            decimal creditAmount = (decimal)model.payAmount;

            if (postType == "VAT")
            {
                feeDescription = "Charge Reversal (" + feeVATRecord.DESCRIPTION + ")";
                creditAmount = (decimal)model.payAmount * (decimal)(feeVATRecord.VALUE / 100.0);
            }
            //if (credits.TBL_CHARGE_FEE_DETAIL_TYPE.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax) { feeDescription = "VAT Reversal"; }
            credit.operationId = (int)model.operationId;
            credit.description = feeDescription;
            credit.valueDate = generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID; //(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, loanDetails.companyId); //casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = model.sourceReferenceNumber;
            credit.batchCode = batchCode;
            credit.casaAccountId = casa.CASAACCOUNTID;
            credit.debitAmount = 0;
            credit.creditAmount = creditAmount;
            credit.sourceBranchId = model.sourceBranchId;
            credit.destinationBranchId = casa.BRANCHID;
            credit.rateCode = "TTB";
            credit.rateUnit = string.Empty;
            credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

            inputTransactions.Add(credit);




            //foreach (var post in postingGroups)
            //{
            //    var feeDetails = (from details in this.context.TBL_CHARGE_FEE_DETAIL where details.CHARGEFEEID == model.chargeFeeId && details.POSTINGGROUP == post orderby details.POSTINGTYPEID select details).ToList();

            //    foreach (var debits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Debit))
            //    {
            //        FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            //        decimal debitAmount = (decimal)model.feeAmountDiff;

            //        if(debits.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax ) {
            //            feeDescription = "VAT Reversal";
            //            debitAmount = (decimal)model.feeAmountDiff * (decimal)(debits.VALUE / 100.0);
            //        }

            //        debit.operationId = (int)model.operationId;
            //        debit.description = feeDescription; //feeDescription; // $"Fee charge on {debits.DESCRIPTION}";
            //        debit.valueDate = generalSetup.GetApplicationDate();
            //        debit.transactionDate = debit.valueDate;
            //        debit.currencyId = casa.CURRENCYID;
            //        debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            //        debit.isApproved = true;
            //        debit.postedBy = model.createdBy;
            //        debit.approvedBy = model.createdBy;
            //        debit.approvedDate = debit.transactionDate;
            //        debit.approvedDateTime = DateTime.Now;
            //        debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //        debit.companyId = model.companyId;

            //        var creditedChargeFeeRecord = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == model.chargeFeeId && x.POSTINGTYPEID == (short)GLPostingTypeEnum.Credit).FirstOrDefault();


            //        debit.glAccountId = (int)creditedChargeFeeRecord.GLACCOUNTID1;
            //        debit.sourceReferenceNumber = model.sourceReferenceNumber;
            //        debit.batchCode = batchCode;
            //        debit.casaAccountId = null;
            //        debit.debitAmount = debitAmount;
            //        debit.creditAmount = 0;
            //        debit.sourceBranchId = model.userBranchId;
            //        debit.destinationBranchId = casa.BRANCHID;

            //        debit.rateCode = "TTB"; //loanDetails.nostroRateCode;
            //        debit.rateUnit = string.Empty;
            //        debit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

            //        inputTransactions.Add(debit);
            //    }

            //    foreach (var credits in feeDetails.Where(a => a.POSTINGTYPEID == (int)GLPostingTypeEnum.Credit))
            //    {
            //        FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            //        decimal creditAmount = (decimal)model.feeAmountDiff;
            //        if (credits.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax)
            //        {
            //            feeDescription = "VAT Reversal";
            //            creditAmount = (decimal)model.feeAmountDiff * (decimal)(credits.VALUE / 100.0);
            //        }

            //        if (credits.TBL_CHARGE_FEE_DETAIL_TYPE.DETAILTYPEID == (short)ChargeFeeDealTypeEnum.Tax) { feeDescription = "VAT Reversal"; }
            //        credit.operationId = (int)model.operationId;
            //        credit.description =  feeDescription;
            //        credit.valueDate = generalSetup.GetApplicationDate();
            //        credit.transactionDate = credit.valueDate;
            //        credit.currencyId = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == model.companyId).CURRENCYID; //(short)chartOfAccount.GetAccountDefaultCurrency((int)credits.GLACCOUNTID1, loanDetails.companyId); //casa.CURRENCYID;
            //        credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            //        credit.isApproved = true;
            //        credit.postedBy = model.createdBy;
            //        credit.approvedBy = model.createdBy;
            //        credit.approvedDate = credit.transactionDate;
            //        credit.approvedDateTime = DateTime.Now;
            //        credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //        credit.companyId = model.companyId;
            //        credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value; 
            //        credit.sourceReferenceNumber = model.sourceReferenceNumber;
            //        credit.batchCode = batchCode;
            //        credit.casaAccountId = casa.CASAACCOUNTID;
            //        credit.debitAmount = 0;
            //        credit.creditAmount = creditAmount;
            //        credit.sourceBranchId = model.userBranchId;
            //        credit.destinationBranchId = model.userBranchId;
            //        credit.rateCode = "TTB";
            //        credit.rateUnit = string.Empty;
            //        credit.currencyCrossCode = casa.TBL_CURRENCY.CURRENCYCODE;

            //        inputTransactions.Add(credit);

            //    }

            //}
           // PostTransaction(inputTransactions, false, twoFactorAuth);

            //var b = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId);

            //if (model.feeAmountDiff > 0)
            //{
            //    var v = details.FirstOrDefault(x => x.POSTINGTYPEID == (short)GLPostingTypeEnum.Credit );
            //   // if(v.TBL_CHARGE_FEE_DETAIL_TYPE.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax) { }

            //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            //    debit.operationId = (int)OperationsEnum.Fee_chargeChange;
            //    debit.description = "Charge Reversal";
            //    debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            //    debit.transactionDate = debit.valueDate;
            //    debit.currencyId = casa.CURRENCYID;
            //    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            //    debit.isApproved = true;
            //    debit.postedBy = model.createdBy;
            //    debit.approvedBy = model.createdBy;
            //    debit.approvedDate = debit.transactionDate;
            //    debit.approvedDateTime = DateTime.Now;
            //    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //    debit.companyId = model.companyId;
            //    debit.glAccountId = v.GLACCOUNTID1.Value;
            //    debit.sourceReferenceNumber = model.sourceReferenceNumber; //loanData.LOANREFERENCENUMBER;
            //    debit.casaAccountId = null;
            //    debit.debitAmount = Math.Abs(model.feeAmountDiff);
            //    debit.creditAmount = 0;
            //    debit.sourceBranchId = model.sourceBranchId;
            //    debit.destinationBranchId = model.sourceBranchId;


            //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            //    credit.operationId = (int)OperationsEnum.Fee_chargeChange;
            //    credit.description = "Charge Reversal";
            //    credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            //    credit.transactionDate = credit.valueDate;
            //    credit.currencyId = casa.CURRENCYID;
            //    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            //    credit.isApproved = true;
            //    credit.postedBy = model.createdBy;
            //    credit.approvedBy = model.createdBy;
            //    credit.approvedDate = credit.transactionDate;
            //    credit.approvedDateTime = DateTime.Now;
            //    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //    credit.companyId = model.companyId;
            //    credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value; 
            //    credit.sourceReferenceNumber = model.sourceReferenceNumber;
            //    credit.casaAccountId = casa.CASAACCOUNTID;
            //    credit.debitAmount = 0;
            //    credit.creditAmount = Math.Abs(model.feeAmountDiff);
            //    credit.sourceBranchId = model.sourceBranchId;
            //    credit.destinationBranchId = casa.BRANCHID;


            //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            //    inputTransactions.Add(debit);
            //    inputTransactions.Add(credit);
            //    PostTransaction(inputTransactions, false, twoFactorAuth);
            //}
            //else
            //{

            //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            //    debit.operationId = (int)OperationsEnum.Fee_chargeChange;
            //    debit.description = "Charge Addition";
            //    debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            //    debit.transactionDate = debit.valueDate;
            //    debit.currencyId = casa.CURRENCYID;
            //    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            //    debit.isApproved = true;
            //    debit.postedBy = model.createdBy;
            //    debit.approvedBy = model.createdBy;
            //    debit.approvedDate = debit.transactionDate;
            //    debit.approvedDateTime = DateTime.Now;
            //    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //    debit.companyId = model.companyId;
            //    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            //    debit.sourceReferenceNumber = model.sourceReferenceNumber;
            //    debit.casaAccountId = casa.CASAACCOUNTID;
            //    debit.debitAmount = Math.Abs(model.feeAmountDiff);
            //    debit.creditAmount = 0;
            //    debit.sourceBranchId = model.userBranchId;
            //    debit.destinationBranchId = casa.BRANCHID;

            //    var feeGL = this.context.TBL_CHARGE_FEE.Where(x => x.CHARGEFEEID == model.chargeFeeId).Select( x=> x.TBL_CHARGE_FEE_DETAIL.FirstOrDefault().GLACCOUNTID1.Value).FirstOrDefault();
            //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            //    credit.operationId = (int)OperationsEnum.Fee_chargeChange;
            //    credit.description = "Charge Addition";
            //    credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            //    credit.transactionDate = credit.valueDate;
            //    credit.currencyId = casa.CURRENCYID;
            //    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            //    credit.isApproved = true;
            //    credit.postedBy = model.createdBy;
            //    credit.approvedBy = model.createdBy;
            //    credit.approvedDate = credit.transactionDate;
            //    credit.approvedDateTime = DateTime.Now;
            //    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            //    credit.companyId = model.companyId;
            //    credit.glAccountId = feeGL;

            //    credit.sourceReferenceNumber = model.sourceReferenceNumber;
            //    credit.casaAccountId = null;
            //    credit.debitAmount = 0;
            //    credit.creditAmount = Math.Abs(model.feeAmountDiff);
            //    credit.sourceBranchId = model.userBranchId;
            //    credit.destinationBranchId = model.userBranchId;

            //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            //    inputTransactions.Add(debit);
            //    inputTransactions.Add(credit);
            //    PostTransaction(inputTransactions, false, twoFactorAuth);
            //}
            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return inputTransactions;

        }

        public FinanceTransactionViewModel PostBuildLoanNegativeReversalPosting(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);
            debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            debit.description = description; // "Loan Disbursment Amount";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
            credit.description = description; // "Loan Disbursment Amount";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }


        public FinanceTransactionViewModel PostLoanPositiveReversalEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true
            };


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            var casa = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            if (postedAmount > 0)
            {
                
                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                debit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
                debit.casaAccountId = casa.CASAACCOUNTID;
                debit.debitAmount = Math.Abs(postedAmount);
                debit.creditAmount = 0;
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;



                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = creditGL;
                credit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(postedAmount);
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;


            }
            else
            {

                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = credit.valueDate;
                debit.currencyId = casa.CURRENCYID;
                debit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = credit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = creditGL;
                debit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = 0;
                debit.creditAmount = Math.Abs(postedAmount);
                debit.sourceBranchId = casa.BRANCHID;
                debit.destinationBranchId = casa.BRANCHID;

                

                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = debit.valueDate;
                credit.currencyId = casa.CURRENCYID;
                credit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = debit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
                credit.sourceReferenceNumber = casa.LOANREFERENCENUMBER;
                credit.casaAccountId = casa.CASAACCOUNTID;
                credit.debitAmount = Math.Abs(postedAmount);
                credit.creditAmount = 0;
                credit.sourceBranchId = casa.BRANCHID;
                credit.destinationBranchId = casa.BRANCHID;
                

            }




            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions,false, twoFADetails);
            

            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }

        public FinanceTransactionViewModel PostLoanGLEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();
            
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == model.loanId).FirstOrDefault();

            model.date = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;


            credit.glAccountId = creditGL; // context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = credit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = credit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = debitGL;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            //debit.sourceBranchId = casa.BRANCHID;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true
            };

            PostTransaction(inputTransactions, false, twoFADetails);

            return null;

        }


        public FinanceTransactionViewModel PostLoanPositiveReversalCasaEntries(LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description, int operationId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true
            };


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            if (postedAmount > 0)
            {

                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = context.TBL_COMPANY.Where(x => x.COMPANYID == loan.COMPANYID).Select(x => x.CURRENCYID).FirstOrDefault();
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID).INTERESTINCOMEEXPENSEGL.Value;
                debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(postedAmount);
                debit.creditAmount = 0;
                debit.sourceBranchId = loan.BRANCHID;
                debit.destinationBranchId = loan.BRANCHID;
                debit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                debit.rateUnit = "";
                debit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";




                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = loan.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = creditGL;
                credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                //credit.casaAccountId = loan.CASAACCOUNTID;
                credit.casaAccountId = loan.CASAACCOUNTID2;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(postedAmount);
                credit.sourceBranchId = loan.BRANCHID;
                credit.destinationBranchId = loan.BRANCHID;
                credit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                credit.rateUnit = "";
                credit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";




            }
            else
            {

                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = credit.valueDate;
                debit.currencyId = loan.CURRENCYID;
                debit.currencyRate = GetExchangeRate(credit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = credit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = creditGL;
                debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                //debit.casaAccountId = loan.CASAACCOUNTID;
                debit.casaAccountId = loan.CASAACCOUNTID2;
                debit.debitAmount = Math.Abs(postedAmount);
                debit.creditAmount = 0;
                debit.sourceBranchId = loan.BRANCHID;
                debit.destinationBranchId = loan.BRANCHID;
                debit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                debit.rateUnit = "";
                debit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";




                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = debit.valueDate;
                credit.currencyId = context.TBL_COMPANY.Where(x => x.COMPANYID == loan.COMPANYID).Select(x => x.CURRENCYID).FirstOrDefault();
                credit.currencyRate = GetExchangeRate(debit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = debit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loan.PRODUCTID).INTERESTINCOMEEXPENSEGL.Value;
                credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                //credit.casaAccountId = casa.CASAACCOUNTID;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(postedAmount);
                credit.sourceBranchId = loan.BRANCHID;
                credit.destinationBranchId = loan.BRANCHID;
                credit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                credit.rateUnit = "";
                credit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";


            }




            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions, false, twoFADetails);


            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostTerminateAndRebookEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL,int creditGL, string description, TwoFactorAutheticationViewModel twoFactorAuth)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            model.date = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;


            credit.glAccountId = creditGL; //context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = credit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = credit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;


            debit.glAccountId = debitGL;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            //debit.sourceBranchId = casa.BRANCHID;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            PostTransaction(inputTransactions, false, twoFactorAuth);

            return null;

        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostTerminateAndRebookEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, string description, TwoFactorAutheticationViewModel twoFactorAuth)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            model.date = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            

            credit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = credit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = credit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            
            debit.glAccountId = debitGL;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            //debit.sourceBranchId = casa.BRANCHID;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            PostTransaction(inputTransactions, false, twoFactorAuth);

            return null;

        }


        public FinanceTransactionViewModel PostEarnUnEarnedFeeOperationEntries(DailyInterestAccrualViewModel model, decimal postedAmount, string description, int operationId, int loanSystemTypeId)
        {
            //FinanceTransactionViewModel loanTransaction = new FinanceTransactionViewModel();

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true
            };

            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            var chardedFeeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(x => x.CHARGEFEEID == model.chargedFeeId && x.DETAILTYPEID == (short)ChargeFeeDetailTypeEnum.Tax && x.REQUIREAMORTISATION == true).FirstOrDefault();

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();



            var loan = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);



            if (postedAmount > 0)
            {

                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = debit.valueDate;
                debit.currencyId = context.TBL_COMPANY.Where(x => x.COMPANYID == loan.COMPANYID).Select(x => x.CURRENCYID).FirstOrDefault();
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = debit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = chardedFeeDetails.GLACCOUNTID1.Value;
                debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(postedAmount);
                debit.creditAmount = 0;
                debit.sourceBranchId = loan.BRANCHID;
                debit.destinationBranchId = loan.BRANCHID;
                debit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                debit.rateUnit = "";
                debit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";




                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = credit.valueDate;
                credit.currencyId = loan.CURRENCYID;
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = credit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = chardedFeeDetails.GLACCOUNTID2.Value;
                credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(postedAmount);
                credit.sourceBranchId = loan.BRANCHID;
                credit.destinationBranchId = loan.BRANCHID;
                credit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                credit.rateUnit = "";
                credit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";


            }
            else
            {

                debit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                debit.description = description; // "Loan Disbursment Amount";
                debit.valueDate = model.date;//generalSetup.GetApplicationDate();
                debit.transactionDate = credit.valueDate;
                debit.currencyId = loan.CURRENCYID;
                debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
                debit.isApproved = true;
                debit.postedBy = model.createdBy;
                debit.approvedBy = model.createdBy;
                debit.approvedDate = credit.transactionDate;
                debit.approvedDateTime = DateTime.Now;
                debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                debit.companyId = model.companyId;
                debit.glAccountId = chardedFeeDetails.GLACCOUNTID2.Value;
                debit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                debit.casaAccountId = null;
                debit.debitAmount = Math.Abs(postedAmount);
                debit.creditAmount = 0;
                debit.sourceBranchId = loan.BRANCHID;
                debit.destinationBranchId = loan.BRANCHID;
                debit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                debit.rateUnit = "";
                debit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";




                credit.operationId = operationId;//(int)OperationsEnum.LoanRepayment;
                credit.description = description; // "Loan Disbursment Amount";
                credit.valueDate = model.date;//generalSetup.GetApplicationDate();
                credit.transactionDate = debit.valueDate;
                credit.currencyId = context.TBL_COMPANY.Where(x => x.COMPANYID == loan.COMPANYID).Select(x => x.CURRENCYID).FirstOrDefault();
                credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
                credit.isApproved = true;
                credit.postedBy = model.createdBy;
                credit.approvedBy = model.createdBy;
                credit.approvedDate = debit.transactionDate;
                credit.approvedDateTime = DateTime.Now;
                credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
                credit.companyId = model.companyId;
                credit.glAccountId = chardedFeeDetails.GLACCOUNTID1.Value;
                credit.sourceReferenceNumber = loan.LOANREFERENCENUMBER;
                //credit.casaAccountId = casa.CASAACCOUNTID;
                credit.casaAccountId = null;
                credit.debitAmount = 0;
                credit.creditAmount = Math.Abs(postedAmount);
                credit.sourceBranchId = loan.BRANCHID;
                credit.destinationBranchId = loan.BRANCHID;
                credit.rateCode = context.TBL_CURRENCY_RATECODE.Where(x => x.RATECODEID == loan.NOSTRORATECODEID).Select(x => x.RATECODE).FirstOrDefault() ?? "";
                credit.rateUnit = "";
                credit.currencyCrossCode = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loan.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault() ?? "";



            }




            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions, false, twoFADetails);


            //financeTransaction.PostTransaction(loanTransaction);

            // Audit Section ---------------------------            


            return null;

        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel PostTerminateAndRebookDoubleEntries(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int debitGL, int creditGL, string description, TwoFactorAutheticationViewModel twoFactorAuth)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            model.date = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;


            credit.glAccountId = creditGL; // context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = credit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = credit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = debitGL;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            //debit.sourceBranchId = casa.BRANCHID;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            PostTransaction(inputTransactions, false, twoFactorAuth);

            return null;

        }


        [OperationBehavior(TransactionScopeRequired = true)]
        public FinanceTransactionViewModel TerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {

            var twoFADetails = new TwoFactorAutheticationViewModel
            {
                skipAuthentication = true,
            };

            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            DateTime appDate = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID2 && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = appDate;//generalSetup.GetApplicationDate();
            debit.transactionDate = appDate; // debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.postedDateTime = appDate;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = appDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;


            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = casa.BRANCHID;
            debit.destinationBranchId = casa.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = appDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = appDate; // credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.postedDateTime = appDate;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = appDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;


            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = casa.BRANCHID;
            credit.destinationBranchId = casa.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            //PostTransaction(inputTransactions);
            PostTransaction(inputTransactions, false, twoFADetails);

            return null;

        }


        public List<FinanceTransactionViewModel> BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {

            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            DateTime appDate = generalSetup.GetApplicationDate();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            //var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);
            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID2 && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanTermination;
            debit.description = description;
            debit.valueDate = appDate;//generalSetup.GetApplicationDate();
            debit.transactionDate = appDate; // debit.valueDate;
            debit.currencyId = casa.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.postedDateTime = appDate;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = appDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;


            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = casa.CASAACCOUNTID;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanTermination;
            credit.description = description;
            credit.valueDate = appDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = appDate; // credit.valueDate;
            credit.currencyId = casa.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.postedDateTime = appDate;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = appDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;


            credit.glAccountId = creditGL;
            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);


            return inputTransactions;

        }


        //[OperationBehavior(TransactionScopeRequired = true)]
        //public FinanceTransactionViewModel BuildTerminateAndRebookPosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        //{

        //    var twoFADetails = new TwoFactorAutheticationViewModel
        //    {
        //        skipAuthentication = true,
        //    };

        //    var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

        //    FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
        //    FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

        //    var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

        //    debit.operationId = (int)OperationsEnum.LoanTermination;
        //    debit.description = description;
        //    debit.valueDate = model.date;//generalSetup.GetApplicationDate();
        //    debit.transactionDate = debit.valueDate;
        //    debit.currencyId = casa.CURRENCYID;
        //    debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
        //    debit.isApproved = true;
        //    debit.postedBy = model.createdBy;
        //    debit.approvedBy = model.createdBy;
        //    debit.approvedDate = debit.transactionDate;
        //    debit.approvedDateTime = DateTime.Now;
        //    debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    debit.companyId = model.companyId;


        //    debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == casa.PRODUCTID).PRINCIPALBALANCEGL.Value;
        //    debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
        //    debit.casaAccountId = casa.CASAACCOUNTID;
        //    debit.debitAmount = postedAmount;
        //    debit.creditAmount = 0;
        //    debit.sourceBranchId = casa.BRANCHID;
        //    debit.destinationBranchId = casa.BRANCHID;


        //    FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

        //    credit.operationId = (int)OperationsEnum.LoanTermination;
        //    credit.description = description;
        //    credit.valueDate = model.date;//generalSetup.GetApplicationDate();
        //    credit.transactionDate = credit.valueDate;
        //    credit.currencyId = casa.CURRENCYID;
        //    credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
        //    credit.isApproved = true;
        //    credit.postedBy = model.createdBy;
        //    credit.approvedBy = model.createdBy;
        //    credit.approvedDate = credit.transactionDate;
        //    credit.approvedDateTime = DateTime.Now;
        //    credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
        //    credit.companyId = model.companyId;


        //    credit.glAccountId = creditGL;
        //    credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
        //    credit.casaAccountId = null;
        //    credit.debitAmount = 0;
        //    credit.creditAmount = postedAmount;
        //    credit.sourceBranchId = casa.BRANCHID;
        //    credit.destinationBranchId = casa.BRANCHID;


        //    List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
        //    inputTransactions.Add(debit);
        //    inputTransactions.Add(credit);

        //    PostTransaction(inputTransactions);
        //    PostTransaction(inputTransactions, false, twoFADetails);

        //    return null;

        //}

        public List<FinanceTransactionViewModel> BuildRecapitalisationAccuredInterestReceivablePosting(int loanId, LoanPaymentRestructureScheduleInputViewModel model, decimal postedAmount, int creditGL, string description)
        {
            var loanData = this.context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).FirstOrDefault();

            //FinanceTransactionViewModel terminateAndRebookTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();

            var casa = this.context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == loanData.CASAACCOUNTID && x.COMPANYID == model.companyId);

            debit.operationId = (int)OperationsEnum.LoanRecapitilization;
            debit.description = description;
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = loanData.CURRENCYID;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;


            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == loanData.PRODUCTID).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            debit.casaAccountId = null;
            debit.debitAmount = postedAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = loanData.BRANCHID;
            debit.destinationBranchId = loanData.BRANCHID;
            debit.valueDate = generalSetup.GetApplicationDate();


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();

            credit.operationId = (int)OperationsEnum.LoanRecapitilization;
            credit.description = description;
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = loanData.CURRENCYID;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = creditGL;

            credit.sourceReferenceNumber = loanData.LOANREFERENCENUMBER;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = postedAmount;
            credit.sourceBranchId = loanData.BRANCHID;
            credit.destinationBranchId = loanData.BRANCHID;
            credit.valueDate = generalSetup.GetApplicationDate();


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            //PostTransaction(inputTransactions);

            return inputTransactions;

        }
        
        public FinanceTransactionViewModel PostDailyInterestSuspension(DailyInterestAccrualViewModel model, int loanId, DateTime applicationDate, int staffId)

        {
            //FinanceTransactionViewModel dailyInterestAccrualTransaction = new FinanceTransactionViewModel();
            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.InterestSuspension;
            debit.description = "Interest Suspension";
            debit.valueDate = model.date;//generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = model.currencyId;
            debit.currencyRate = GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = staffId;
            debit.approvedBy = staffId;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = applicationDate;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);
            debit.glAccountId = product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = model.referenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = (decimal)model.dailyAccuralAmount;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.branchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.InterestSuspension;
            credit.description = "Interest Suspension";
            credit.valueDate = model.date;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = model.currencyId;
            credit.currencyRate = GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = staffId;
            credit.approvedBy = staffId;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = applicationDate;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            var InterestSuspensionGL = 8;////to be change when interestsuspenses is created
            credit.glAccountId = InterestSuspensionGL; ///product.InterestIncomeExpenseGL.Value;

            credit.sourceReferenceNumber = model.referenceNumber;
            credit.casaAccountId = null;
            credit.debitAmount = 0;
            credit.creditAmount = (decimal)model.dailyAccuralAmount;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.branchId;


            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);
            PostTransaction(inputTransactions);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDailyInterestAccrual,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Interest Suspension Posting: {model.referenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = model.date,//generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();
            return null;

        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public string TempPostTransaction(List<FinanceTransactionViewModel> inputTransactions)
        {
            var batchCode = CommonHelpers.GenerateRandomDigitCode(10);

            var transactionCount = (inputTransactions.Count());

            if (transactionCount < 2) //transaction.transactionDetails.Count() < 2
                throw new ConditionNotMetException("Specify both debit and credit transactions");

            List<TBL_FINANCE_TRANSACTION> transactions = new List<TBL_FINANCE_TRANSACTION>();

            var debitSum = inputTransactions.Sum(x => x.debitAmount);
            var creditSum = inputTransactions.Sum(x => x.creditAmount);
            if (debitSum != creditSum)
                throw new ConditionNotMetException("Total Debit Amount should equal Total Credit Amount");

            foreach (var item in inputTransactions)
            {

                if (item.debitAmount != 0 && item.creditAmount != 0)
                    throw new ConditionNotMetException("Debit or Credit Amount should be 0");

                if (item.debitAmount < 0)
                    throw new ConditionNotMetException("Debit Amount should NOT be less than 0");

                if (item.creditAmount < 0)
                    throw new ConditionNotMetException("Credit Amount should NOT be less than 0");

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {
                    if (item.casaAccountId == null)
                        throw new ConditionNotMetException($"Specify the CASA Account Number in this transaction for GL Code {glInfo.ACCOUNTCODE}");

                    //UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);


                    var casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == item.casaAccountId).FirstOrDefault();

                    if (casa == null)
                        throw new ConditionNotMetException($"CASA account number {item.sourceReferenceNumber} does not exist in the CASA table for this transaction for GL Code {glInfo.ACCOUNTCODE}");

                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);

                    if (referenceCount <= 0)
                        throw new ConditionNotMetException($"Loan reference number {item.sourceReferenceNumber} does not exist in the loan table for this transaction for GL Code {glInfo.ACCOUNTCODE}");
                }
                //else
                //{
                //    item.casaAccountId = null;
                //}

            }

            //api call
            //var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            //if (setup.USE_THIRD_PARTY_INTEGRATION)
            //{
            //    TransactionPosting tran = new TransactionPosting(context);
            //    bool data = false;

            //    Task.Run(async () => { data = await tran.APITransactionPosting(inputTransactions); }).GetAwaiter().GetResult();

            //    if (data == true)
            //    {
            //        TempPostTransactionSub(batchCode, inputTransactions, transactions);
            //        UpdateCustomTransactions(batchCode);
            //    }
            //    else
            //    {
            //        //display message
            //        throw new SecureException($"Transaction Failed.");
            //    }

            //}
            //else
            TempPostTransactionSub(batchCode, inputTransactions, transactions);

            this.context.TBL_FINANCE_TRANSACTION.AddRange(transactions);
            context.SaveChanges();

            return batchCode;
        }

        [OperationBehavior(TransactionScopeRequired = true)]
        public void TempPostTransactionSub(string batchCode, List<FinanceTransactionViewModel> inputTransactions, List<TBL_FINANCE_TRANSACTION> transactions)
        {
            foreach (var item in inputTransactions)
            {

                var glInfo = context.TBL_CHART_OF_ACCOUNT.FirstOrDefault(x => x.GLACCOUNTID == item.glAccountId);

                GLClassEnum glClass = (GLClassEnum)glInfo.GLCLASSID;

                if (glClass == GLClassEnum.CASA)
                {

                    UpdateCASABalances(item.casaAccountId.Value, item.debitAmount, item.creditAmount);
                }
                else if (glClass == GLClassEnum.LoanSchedule)
                {
                    item.casaAccountId = null;
                    int referenceCount = 0;

                    referenceCount = context.TBL_LOAN.Count(x => x.LOANREFERENCENUMBER == item.sourceReferenceNumber);
                }
                else
                { item.casaAccountId = null; }

                TBL_FINANCE_TRANSACTION trans = new TBL_FINANCE_TRANSACTION();

                trans.BATCHCODE = batchCode;
                //trans.OPERATIONID = mainItem.operationId;
                trans.OPERATIONID = (int)item.operationId;
                trans.DESCRIPTION = item.description;
                trans.VALUEDATE = item.valueDate;
                trans.POSTEDDATE = item.transactionDate;
                trans.CURRENCYID = item.currencyId;
                trans.CURRENCYRATE = item.currencyRate;
                trans.POSTEDDATETIME = DateTime.Now;
                trans.ISAPPROVED = item.isApproved;
                trans.POSTEDBY = item.postedBy;
                trans.APPROVEDBY = item.approvedBy;
                trans.APPROVEDDATE = item.approvedDate;
                trans.APPROVEDDATETIME = item.approvedDateTime;
                trans.SOURCEAPPLICATIONID = item.sourceApplicationId;
                trans.COMPANYID = item.companyId;
                trans.GLACCOUNTID = item.glAccountId;
                trans.SOURCEREFERENCENUMBER = item.sourceReferenceNumber;
                trans.CASAACCOUNTID = item.casaAccountId;
                trans.DEBITAMOUNT = item.debitAmount;
                trans.CREDITAMOUNT = item.creditAmount;
                trans.SOURCEBRANCHID = item.sourceBranchId;
                trans.DESTINATIONBRANCHID = item.destinationBranchId;

                transactions.Add(trans);

            }
        }

        public bool BulkIntegrationPosting(FinanceTransactionStagingViewModel model)

        {
            //model.branchId = 100;
            //model.staffId = 1;
            //var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = model.operationId;
            debit.description = model.description;
            debit.valueDate = model.valueDate; //generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = (short)model.currencyId;
            debit.currencyRate = model.currencyRate;//GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = (int)SystemStaff.System;
            debit.approvedBy = (int)SystemStaff.System;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;
            debit.glAccountId = model.debitGlAccountId;//product.INTERESTRECEIVABLEPAYABLEGL.Value;
            debit.sourceReferenceNumber = model.sourceReferenceNumber;//product.PRODUCTCODE;
            debit.casaAccountId = model.debitCasaAccountId;
            debit.debitAmount = model.actualAmountCollected;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.sourceBranchId;
            debit.destinationBranchId = model.destinationBranchId;
            debit.batchId = model.batchId;

            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = model.operationId;
            credit.description = model.description;
            credit.valueDate = model.valueDate;//generalSetup.GetApplicationDate();
            credit.transactionDate = credit.valueDate;
            credit.currencyId = (short)model.currencyId;
            credit.currencyRate = model.currencyRate;//GetExchangeRate(credit.valueDate, credit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = (int)SystemStaff.System;
            credit.approvedBy = (int)SystemStaff.System;
            credit.approvedDate = credit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;
            credit.glAccountId = model.creditGlAccountId;//product.INTERESTINCOMEEXPENSEGL.Value;

            credit.sourceReferenceNumber = model.sourceReferenceNumber;
            credit.casaAccountId = model.creditCasaAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = model.actualAmountCollected;
            credit.sourceBranchId = model.sourceBranchId;
            credit.destinationBranchId = model.destinationBranchId;
            credit.batchId = model.batchId;

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            inputTransactions.Add(debit);
            inputTransactions.Add(credit);

            var batchPost = PostTransaction(inputTransactions, true);

            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.BulkIntegrationPosting,
            //    STAFFID = (int)SystemStaff.HQ,//model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"{ model.description}: {model.sourceReferenceNumber}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = model.valueDate,//generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            if (batchPost != null)
            {
                var result = true;
                return result;
            }
            return false;

        }


        public TrialBalanceViewModel GetExportedTrialBalanceSummary(ReportSearchEntity entity, int companyId)
        {
            return GenerateTrialBalanceData(entity, companyId);
        }

        public TrialBalanceViewModel GenerateTrialBalanceData(ReportSearchEntity entity, int companyId)
        {
            var record = GetTrialBalanceSummary(entity, companyId).ToList();
            if (record == null)
                throw new ConditionNotMetException("Record Not Found For Download");

            return GenerateTrialBalance(record.ToList(), entity);

        }

        public List<TrialBalanceViewModel> GetTrialBalanceSummary(ReportSearchEntity entity, int companyId)
        {
            #region PREVIOUS CODE....
            //using (var context = new FinTrakBankingContext())
            //{

            //    //int customChartOfAccountId;
            //    //  TrialBalanceViewModel glAttributes;
            //    List<TrialBalanceViewModel> trialBal;

            //    if (entity.customChartOfAccountId != 0)
            //    {

            //        var glAttributes = (//from ft in context.TBL_FINANCE_TRANSACTION
            //                            //join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                            //  join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID
            //                           from ca in context.TBL_CHART_OF_ACCOUNT
            //                           join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
            //                           //  join ft in context.TBL_FINANCE_TRANSACTION on ca.GLACCOUNTID equals ft.GLACCOUNTID
            //                           join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE
            //                           where cca.CUSTOMACCOUNTID == (entity.customChartOfAccountId == 0 ? cca.CUSTOMACCOUNTID : entity.customChartOfAccountId)
            //                           select new TrialBalanceViewModel
            //                           {
            //                               glAccountId = ca.GLACCOUNTID,
            //                               accountName = cca.ACCOUNTID + " | " + ca.ACCOUNTNAME + " | " + cca.PLACEHOLDERID + " | " + cca.CURRENCYCODE,
            //                               currency = cca.CURRENCYCODE,
            //                               customChartOfAccountId = cca.CUSTOMACCOUNTID,
            //                               currencyId = cur.CURRENCYID


            //                           }).FirstOrDefault();





            //        // using (FinTrakBankingContext context = new FinTrakBankingContext())
            //        // {

            //        trialBal = (from ft in context.TBL_FINANCE_TRANSACTION
            //                    join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                    join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID

            //                    where (DbFunctions.TruncateTime(ft.VALUEDATE) >= DbFunctions.TruncateTime(entity.startDate) || entity.startDate == null) && DbFunctions.TruncateTime(ft.VALUEDATE) <= DbFunctions.TruncateTime(entity.endDate)
            //                     //   where ( entity.startDate != null ? DbFunctions.TruncateTime(ft.VALUEDATE) >= DbFunctions.TruncateTime(entity.startDate) :  null) && DbFunctions.TruncateTime(ft.VALUEDATE) <= DbFunctions.TruncateTime(entity.endDate)
            //                     //  && ft.COMPANYID == companyId && ft.GLACCOUNTID ==( glAttributes.glAccountId == 0 ? ft.GLACCOUNTID : glAttributes.glAccountId)  && ft.CURRENCYID == glAttributes.currencyId
            //                     && ft.COMPANYID == companyId && ft.GLACCOUNTID == glAttributes.glAccountId && ft.CURRENCYID == glAttributes.currencyId

            //                    group ft by new { ft.GLACCOUNTID, ca.ACCOUNTCODE, ca.ACCOUNTNAME, ft.CURRENCYID } into groupedQ

            //                    // entity.glAccountId && glAttributes.CURRENCYID
            //                    select new TrialBalanceViewModel()
            //                    {

            //                        glAccountId = groupedQ.Key.GLACCOUNTID,
            //                        accountCode = groupedQ.Key.ACCOUNTCODE,
            //                        accountName = groupedQ.Key.ACCOUNTNAME,
            //                        currencyCode = groupedQ.Key.CURRENCYID,
            //                        totalCredit = groupedQ.Sum(x => x.CREDITAMOUNT),
            //                        totalDebit = groupedQ.Sum(x => x.DEBITAMOUNT)



            //                    }).ToList();
            //        //.Select(x => {

            //        //    var totalCredit = fff.Where(m => m.GLACCOUNTID == x.glAccountId && m.TBL_CURRENCY.CURRENCYNAME == x.currency).Select(m => m.CREDITAMOUNT);
            //        //    x.totalCredit = totalCredit.Sum();

            //        //    var totalDebit = fff.Where(m => m.GLACCOUNTID == x.glAccountId && m.TBL_CURRENCY.CURRENCYNAME == x.currency).Select(m => m.DEBITAMOUNT).Sum();
            //        //    x.totalDebit = totalDebit;




            //        //    return x;

            //        //}).Distinct().ToList();




            //        // }
            //    }
            //    else
            //    {




            //        var glAttributes = (//from ft in context.TBL_FINANCE_TRANSACTION
            //                            //join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                            //  join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID
            //                        from ca in context.TBL_CHART_OF_ACCOUNT
            //                        join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
            //                        //  join ft in context.TBL_FINANCE_TRANSACTION on ca.GLACCOUNTID equals ft.GLACCOUNTID
            //                        join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE

            //                        select new TrialBalanceViewModel
            //                        {
            //                            glAccountId = ca.GLACCOUNTID,
            //                            accountName = cca.ACCOUNTID + " | " + ca.ACCOUNTNAME + " | " + cca.PLACEHOLDERID + " | " + cca.CURRENCYCODE,
            //                            currency = cca.CURRENCYCODE,
            //                            customChartOfAccountId = cca.CUSTOMACCOUNTID,
            //                            currencyId = cur.CURRENCYID


            //                        }).ToList();

            //        var glAccountIds = (//from ft in context.TBL_FINANCE_TRANSACTION
            //                            //join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                            //  join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID
            //                        from ca in context.TBL_CHART_OF_ACCOUNT
            //                        join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
            //                        //  join ft in context.TBL_FINANCE_TRANSACTION on ca.GLACCOUNTID equals ft.GLACCOUNTID
            //                        join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE
            //                        select ca.GLACCOUNTID);

            //        var currencyIds = (//from ft in context.TBL_FINANCE_TRANSACTION
            //                           //join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                           //  join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID
            //                     from ca in context.TBL_CHART_OF_ACCOUNT
            //                     join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
            //                     //  join ft in context.TBL_FINANCE_TRANSACTION on ca.GLACCOUNTID equals ft.GLACCOUNTID
            //                     join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE
            //                     select cur.CURRENCYID);





            //        // using (FinTrakBankingContext context = new FinTrakBankingContext())
            //        // {

            //        trialBal = (from ft in context.TBL_FINANCE_TRANSACTION
            //                    join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
            //                    join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID

            //                    where (DbFunctions.TruncateTime(ft.VALUEDATE) >= DbFunctions.TruncateTime(entity.startDate) || entity.startDate == null) && DbFunctions.TruncateTime(ft.VALUEDATE) <= DbFunctions.TruncateTime(entity.endDate)
            //                       //   where ( entity.startDate != null ? DbFunctions.TruncateTime(ft.VALUEDATE) >= DbFunctions.TruncateTime(entity.startDate) :  null) && DbFunctions.TruncateTime(ft.VALUEDATE) <= DbFunctions.TruncateTime(entity.endDate)
            //                       && ft.COMPANYID == companyId && glAccountIds.Contains(ft.GLACCOUNTID) && currencyIds.Contains(ft.CURRENCYID)
            //                    //   && ftt.COMPANYID == companyId && SqlFunctions.StringConvert((double)ftt.GLACCOUNTID).Contains(glAttributes.glAccountId)  && ftt.CURRENCYID == glAttributes.curr

            //                    group ft by new { ft.GLACCOUNTID, ca.ACCOUNTCODE, ca.ACCOUNTNAME, ft.CURRENCYID } into groupedQ

            //                    // entity.glAccountId && glAttributes.CURRENCYID
            //                    select new TrialBalanceViewModel()
            //                    {

            //                        glAccountId = groupedQ.Key.GLACCOUNTID,
            //                        accountCode = groupedQ.Key.ACCOUNTCODE,
            //                        accountName = groupedQ.Key.ACCOUNTNAME,
            //                        currencyCode = groupedQ.Key.CURRENCYID,
            //                        totalCredit = groupedQ.Sum(x => x.CREDITAMOUNT),
            //                        totalDebit = groupedQ.Sum(x => x.DEBITAMOUNT)



            //                    }).ToList();

            //    }

            //    return trialBal;
            //}
            #endregion


            using (var context = new FinTrakBankingContext())
            {
                List<TrialBalanceViewModel> trialBal;

                var glAttributes = (
                                from ca in context.TBL_CHART_OF_ACCOUNT
                                join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
                                join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE

                                select new TrialBalanceViewModel
                                {
                                    glAccountId = ca.GLACCOUNTID,
                                    accountName = cca.ACCOUNTID + " | " + ca.ACCOUNTNAME + " | " + cca.PLACEHOLDERID + " | " + cca.CURRENCYCODE,
                                    accountId = cca.ACCOUNTID,
                                    currency = cca.CURRENCYCODE,
                                    customChartOfAccountId = cca.CUSTOMACCOUNTID,
                                    currencyId = cur.CURRENCYID


                                }).ToList();

                var glAccountIds = (
                                from ca in context.TBL_CHART_OF_ACCOUNT
                                join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
                                join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE
                                select ca.GLACCOUNTID);

                var currencyIds = (
                             from ca in context.TBL_CHART_OF_ACCOUNT
                             join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
                             join cur in context.TBL_CURRENCY on cca.CURRENCYCODE equals cur.CURRENCYCODE
                             select cur.CURRENCYID);


                trialBal = (from ft in context.TBL_FINANCE_TRANSACTION
                            join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
                            join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID

                            where (DbFunctions.TruncateTime(ft.VALUEDATE) >= DbFunctions.TruncateTime(entity.startDate) || entity.startDate == null) && DbFunctions.TruncateTime(ft.VALUEDATE) <= DbFunctions.TruncateTime(entity.endDate)

                               && ft.COMPANYID == companyId && glAccountIds.Contains(ft.GLACCOUNTID) && currencyIds.Contains(ft.CURRENCYID)

                            group ft by new { ft.GLACCOUNTID, ca.ACCOUNTCODE, ca.ACCOUNTNAME, ft.CURRENCYID } into groupedQ

                            select new TrialBalanceViewModel()
                            {
                                glAccountId = groupedQ.Key.GLACCOUNTID,
                                accountCode = groupedQ.Key.ACCOUNTCODE,
                                accountName = groupedQ.Key.ACCOUNTNAME,
                                currencyId = groupedQ.Key.CURRENCYID,
                                totalCredit = groupedQ.Sum(x => x.CREDITAMOUNT),
                                totalDebit = groupedQ.Sum(x => x.DEBITAMOUNT)

                            }).ToList();

                foreach (var item in trialBal)
                {
                    item.accountId = glAttributes.FirstOrDefault(i => i.glAccountId == item.glAccountId && i.currencyId == item.currencyId).accountId;
                    //item.currency = glAttributes.FirstOrDefault(i => i.glAccountId == item.glAccountId).currency;
                    item.currency = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == item.currencyId).CURRENCYCODE;

                    item.currencyRate = context.TBL_FINANCE_TRANSACTION.FirstOrDefault(i => i.GLACCOUNTID == item.glAccountId && i.CURRENCYID == item.currencyId).CURRENCYRATE;

                    item.totalDebitInBaseCurrency = item.totalDebit * (decimal)item.currencyRate;
                    item.totalCreditInBaseCurrency = item.totalCredit * (decimal)item.currencyRate;

                    item.balance = item.totalCredit - item.totalDebit;
                    item.balanceInBaseCurrency = item.totalCreditInBaseCurrency - item.totalDebitInBaseCurrency;

                    if (item.balance < 0)
                    {
                        item.balanceType = "Dr";
                        item.debitBalance = Math.Abs(item.balance.Value);
                        item.creditBalance = 0;

                        item.debitBalanceInBaseCurrency = Math.Abs(item.balanceInBaseCurrency.Value);
                        item.creditBalanceInBaseCurrency = 0;
                    }
                    else
                    {
                        item.balanceType = "Cr";
                        item.creditBalance = Math.Abs(item.balance.Value);
                        item.debitBalance = 0;

                        item.creditBalanceInBaseCurrency = Math.Abs(item.balanceInBaseCurrency.Value);
                        item.debitBalanceInBaseCurrency = 0;
                    }
                }

                return trialBal;
            }
        }

        private TrialBalanceViewModel GenerateTrialBalance(List<TrialBalanceViewModel> loanInput, ReportSearchEntity entity)
        {

            Byte[] fileBytes = null;
            TrialBalanceViewModel data = new TrialBalanceViewModel();

            if (loanInput != null)
            {
                using (ExcelPackage pck = new ExcelPackage())
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Trial Balance");

                    ws.Cells[1, 1].Value = "ACCOUNT ID";
                    ws.Cells[1, 2].Value = "ACCOUNT CODE";
                    ws.Cells[1, 3].Value = "ACCOUNT NAME";
                    ws.Cells[1, 4].Value = "CURRENCY";
                    ws.Cells[1, 5].Value = "CRNCY DR BAL";
                    ws.Cells[1, 6].Value = "CRNCY CR BAL";
                    ws.Cells[1, 7].Value = "RATE";
                    ws.Cells[1, 8].Value = "NAIRA DR. BAL.";
                    ws.Cells[1, 9].Value = "NAIRA CR. BAL.";

                    //ws.Cells[1, 7].Value = "AMOUNT"; 
                    for (int i = 2; i <= loanInput.Count + 1; i++)
                    {
                        var record = loanInput[i - 2];
                        ws.Cells[i, 1].Value = record.accountId;
                        ws.Cells[i, 2].Value = record.accountCode;
                        ws.Cells[i, 3].Value = record.accountName;
                        ws.Cells[i, 4].Value = record.currency;
                        ws.Cells[i, 5].Value = record.debitBalance;
                        ws.Cells[i, 6].Value = record.creditBalance;
                        ws.Cells[i, 7].Value = record.currencyRate;
                        ws.Cells[i, 8].Value = record.debitBalanceInBaseCurrency;
                        ws.Cells[i, 9].Value = record.creditBalanceInBaseCurrency;
                    }

                    fileBytes = pck.GetAsByteArray();
                    data.reportData = fileBytes;
                    data.templateTypeName = "Trial_Balance";
                }

            }

            return data;
        }

        public List<TrialBalanceViewModel> GetGLandAccountName()
        {
            using (var context = new FinTrakBankingContext())
            {

                var glAttributes = (//from ft in context.TBL_FINANCE_TRANSACTION
                                    //join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
                                    //  join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID
                                  from ca in context.TBL_CHART_OF_ACCOUNT
                                  join cca in context.TBL_CUSTOM_CHART_OF_ACCOUNT on ca.ACCOUNTCODE equals cca.PLACEHOLDERID
                                  //  join ft in context.TBL_FINANCE_TRANSACTION on ca.GLACCOUNTID equals ft.GLACCOUNTID
                                  //  join cur in context.TBL_CURRENCY on ft.CURRENCYID equals cur.CURRENCYID

                                  select new TrialBalanceViewModel
                                  {
                                      glAccountId = ca.GLACCOUNTID,
                                      accountName = cca.ACCOUNTID + " | " + ca.ACCOUNTNAME + " | " + cca.PLACEHOLDERID + " | " + cca.CURRENCYCODE,
                                      currency = cca.CURRENCYCODE,
                                      customChartOfAccountId = cca.CUSTOMACCOUNTID


                                  }).ToList();

                return glAttributes;
            }
        }

    }
}