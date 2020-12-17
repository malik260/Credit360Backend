using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Common.CustomException;
using FinTrakBanking.ThirdPartyIntegration;

namespace FintrakBanking.Repositories.CASA
{
    public class CasaRepository : ICasaRepository
    {
        private FinTrakBankingContext context;
        private ICreditLimitValidationsRepository creditLimitRepo;
        private IFinanceTransactionRepository transRepo;
        private IntegrationWithFlexcube integration;

        public CasaRepository(FinTrakBankingContext _context, ICreditLimitValidationsRepository _creditLimitRepo, IFinanceTransactionRepository _transRepo, IntegrationWithFlexcube _integration)
        {
            this.context = _context;
            this.creditLimitRepo = _creditLimitRepo;
            this.transRepo = _transRepo;
            this.integration = _integration;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public string GetCustomerAccountNumber(int casaAccountId)
        {
          return   context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == casaAccountId).PRODUCTACCOUNTNUMBER;
        }

        public int GetCasaAccountId(string accountNumber, int companyId)
        {
            int value = 0;
            int.TryParse(accountNumber, out value);

            var CasaAccount = (context.TBL_CASA.Where(d => d.OLDPRODUCTACCOUNTNUMBER3 == accountNumber ||
                d.OLDPRODUCTACCOUNTNUMBER2 == accountNumber || d.OLDPRODUCTACCOUNTNUMBER1 == accountNumber ||
                d.CASAACCOUNTID == (value) || d.PRODUCTACCOUNTNUMBER == accountNumber && d.COMPANYID == companyId)
                ).Select(d=>d.CASAACCOUNTID).FirstOrDefault();
            return CasaAccount;
        }

        public string GetAccountOwnerByAccountNumber(string accountNumber, int companyId)
        {
            var custName = (from data in context.TBL_CASA
                            join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                            where data.COMPANYID == companyId && data.PRODUCTACCOUNTNUMBER.Trim() == accountNumber.Trim()
                            select cust).FirstOrDefault();
            return custName.FIRSTNAME + " " + custName.LASTNAME;
        }

        public string GetAllCASAAccount(string casaAccountNumber, int companyId)
        {
            string accno = "";
            int casaAccountId = GetCasaAccountId(casaAccountNumber, companyId);
            var accounts = context.TBL_CASA.Where(x => x.CASAACCOUNTID == casaAccountId);

            foreach (var account in accounts)
            {
                if (account == null)
                {
                    accno += account.PRODUCTACCOUNTNUMBER + " - " + account.TBL_CURRENCY.CURRENCYCODE;
                }
                else
                {
                    accno += "," + account.PRODUCTACCOUNTNUMBER + " - " + account.TBL_CURRENCY.CURRENCYCODE;
                }

            }
            return accno;

        }

        public CasaBalanceViewModel GetCASABalance(string casaAccountNumber, int companyId)
        {
            //CasaBalanceViewModel model = new CasaBalanceViewModel();

            //int casaAccountId = GetCasaAccountId(casaAccountNumber, companyId);
            //var account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == casaAccountId);

            //if (account == null) {
            //    model.isCasaAccountDetailAvailable=false;
            //    throw new ConditionNotMetException("Invalid Account Number");
            //    //return model;
            //}
            if (casaAccountNumber!="")
            {
                return transRepo.GetCASABalance(casaAccountNumber, companyId);

            }
            return new CasaBalanceViewModel();
        }

        public IEnumerable<CasaAccountLienViewModel> GetAllCasaLiens(string accountNumber)
        {
            var result = context.TBL_CASA_LIEN.Where(O => O.PRODUCTACCOUNTNUMBER == accountNumber && O.ISLIENREMOVED == false).Select(O => new CasaAccountLienViewModel()
            {
                lienId = O.LIENID,
                description = O.DESCRIPTION,
                lienAmount = O.LIENAMOUNT,
                lienReferenceNumber = O.LIENREFERENCENUMBER,
                productAccountNumber = O.PRODUCTACCOUNTNUMBER,
                sourceReferenceNumber = O.SOURCEREFERENCENUMBER,
                lienTypeId = O.LIENTYPEID,
                dateCreated = O.DATETIMECREATED
            }).ToList();

            return result;
        }

        public IEnumerable<CasaLienTypeViewModel> GetAllCasaLienTypes(int companyId)
        {
            var result = context.TBL_CASA_LIEN_TYPE.Select(O => new CasaLienTypeViewModel()
            {
                lienTypeId = O.LIENTYPEID,
                lienTypeName = O.LIENTYPENAME
            }).ToList();

            return result;
        }

        public IEnumerable<CasaLoanViewModel> GetAllCasaLoans(int companyId, int casaAccountId)
        {
            var result = context.TBL_LOAN.Where(O => O.CASAACCOUNTID == casaAccountId && O.LOANSTATUSID != (int) LoanStatusEnum.Cancelled && O.LOANSTATUSID != (int)LoanStatusEnum.Terminated && O.LOANSTATUSID != (int)LoanStatusEnum.Completed).Select(O => new CasaLoanViewModel()
            {
                loanId = O.TERMLOANID,
                loanReferenceNumber = O.LOANREFERENCENUMBER
            }).ToList();

            var result1 = context.TBL_LOAN_CONTINGENT.Where(O => O.CASAACCOUNTID == casaAccountId && O.LOANSTATUSID != (int)LoanStatusEnum.Cancelled && O.LOANSTATUSID != (int)LoanStatusEnum.Terminated && O.LOANSTATUSID != (int)LoanStatusEnum.Completed).Select(O => new CasaLoanViewModel()
            {
                loanId = O.CONTINGENTLOANID,
                loanReferenceNumber = O.LOANREFERENCENUMBER
            }).ToList();

            var result2 = context.TBL_LOAN_REVOLVING.Where(O => O.CASAACCOUNTID == casaAccountId && O.LOANSTATUSID != (int)LoanStatusEnum.Cancelled && O.LOANSTATUSID != (int)LoanStatusEnum.Terminated && O.LOANSTATUSID != (int)LoanStatusEnum.Completed).Select(O => new CasaLoanViewModel()
            {
                loanId = O.REVOLVINGLOANID,
                loanReferenceNumber = O.LOANREFERENCENUMBER
            }).ToList();

            return result.Union(result1).Union(result2);
        }

        public bool AddCasaLien(CasaViewModel model)
        {
            context.TBL_CASA_LIEN.Add(new TBL_CASA_LIEN() {
                BRANCHID = model.branchId,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                DESCRIPTION = model.description,
                LIENAMOUNT = model.lienAmount,
                LIENREFERENCENUMBER = model.lienReferenceNumber,
                LIENTYPEID = model.lienTypeId.Value,
                PRODUCTACCOUNTNUMBER = model.productAccountNumber,
                SOURCEREFERENCENUMBER = model.sourceReferenceNumber,
                
            });

            return context.SaveChanges() > 0;
        }

        public IEnumerable<CasaViewModel> FindCustomerCasaLien(string accountNumberOrName, int companyId)
        {
            var data1 = (from data in context.TBL_CASA
                         join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                         where data.COMPANYID == companyId && (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) ||
                         cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                         cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                         select new CasaViewModel()
                         {
                             casaAccountId = data.CASAACCOUNTID,
                             productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                             productAccountName = data.PRODUCTACCOUNTNAME,
                             customerId = data.CUSTOMERID,
                             customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                             customerName = data.TBL_CUSTOMER.FIRSTNAME + " " + data.TBL_CUSTOMER.LASTNAME,
                             productId = data.PRODUCTID,
                             productCode = data.TBL_PRODUCT.PRODUCTCODE,
                             productName = data.TBL_PRODUCT.PRODUCTNAME,
                             companyId = data.COMPANYID,
                             branchId = data.BRANCHID,
                             currency = data.TBL_CURRENCY.CURRENCYNAME,
                             branchCode = data.TBL_BRANCH.BRANCHCODE,
                             branchName = data.TBL_BRANCH.BRANCHNAME,
                             isCurrentAccount = data.ISCURRENTACCOUNT,
                             tenor = data.TENOR ?? 0,
                             interestRate = data.INTERESTRATE ?? 0,
                             effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                             terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                             actionBy = data.ACTIONBY ?? 0,
                             actionDate = data.ACTIONDATE ?? General.DefaultDate,
                             accountStatusId = data.ACCOUNTSTATUSID,
                             operationId = data.OPERATIONID ?? 0,
                             availableBalance = data.AVAILABLEBALANCE,
                             ledgerBalance = data.LEDGERBALANCE,
                             relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                             misCode = data.MISCODE,
                             overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                             overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                             overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                             hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                             lienAmount = data.LIENAMOUNT,
                             hasLien = data.HASLIEN,
                             postNoStatusId = data.POSTNOSTATUSID,
                             oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                             oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                             oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                             //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0 
                             aprovalStatusId = data.APROVALSTATUSID,
                             sourceReferenceNumber = null,
                         });

            var data2 = (from l in context.TBL_LOAN
                         join c in context.TBL_CASA on l.CASAACCOUNTID equals c.CASAACCOUNTID
                         join cu in context.TBL_CUSTOMER on c.CUSTOMERID equals cu.CUSTOMERID
                         where l.LOANREFERENCENUMBER == accountNumberOrName && l.COMPANYID == companyId &&
                         l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                         select new CasaViewModel()
                         {
                             casaAccountId = c.CASAACCOUNTID,
                             productAccountNumber = c.PRODUCTACCOUNTNUMBER,
                             productAccountName = c.PRODUCTACCOUNTNAME,
                             customerId = c.CUSTOMERID,
                             customerCode = c.TBL_CUSTOMER.CUSTOMERCODE,
                             customerName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.LASTNAME,
                             productId = c.PRODUCTID,
                             productCode = c.TBL_PRODUCT.PRODUCTCODE,
                             productName = c.TBL_PRODUCT.PRODUCTNAME,
                             companyId = c.COMPANYID,
                             branchId = c.BRANCHID,
                             currency = c.TBL_CURRENCY.CURRENCYNAME,
                             branchCode = c.TBL_BRANCH.BRANCHCODE,
                             branchName = c.TBL_BRANCH.BRANCHNAME,
                             isCurrentAccount = c.ISCURRENTACCOUNT,
                             tenor = c.TENOR ?? 0,
                             interestRate = c.INTERESTRATE ?? 0,
                             effectiveDate = c.EFFECTIVEDATE ?? General.DefaultDate,
                             terminalDate = c.TERMINALDATE ?? General.DefaultDate,
                             actionBy = c.ACTIONBY ?? 0,
                             actionDate = c.ACTIONDATE ?? General.DefaultDate,
                             accountStatusId = c.ACCOUNTSTATUSID,
                             operationId = c.OPERATIONID ?? 0,
                             availableBalance = c.AVAILABLEBALANCE,
                             ledgerBalance = c.LEDGERBALANCE,
                             relationshipOfficerId = c.RELATIONSHIPOFFICERID ?? 0,
                             misCode = c.MISCODE,
                             overdraftAmount = c.OVERDRAFTAMOUNT ?? 0,
                             overdraftInterestRate = c.OVERDRAFTINTERESTRATE ?? 0,
                             overdraftExpiryDate = c.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                             hasOverdraft = c.HASOVERDRAFT.HasValue == true ? c.HASOVERDRAFT.Value : false,
                             lienAmount = c.LIENAMOUNT,
                             hasLien = c.HASLIEN,
                             postNoStatusId = c.POSTNOSTATUSID,
                             oldProductAccountNumber1 = c.OLDPRODUCTACCOUNTNUMBER1,
                             oldProductAccountNumber2 = c.OLDPRODUCTACCOUNTNUMBER2,
                             oldProductAccountNumber3 = c.OLDPRODUCTACCOUNTNUMBER3,
                             //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                             aprovalStatusId = c.APROVALSTATUSID,
                             sourceReferenceNumber = l.LOANREFERENCENUMBER,
                         });

            var data3 = (from l in context.TBL_LOAN_CONTINGENT
                         join c in context.TBL_CASA on l.CASAACCOUNTID equals c.CASAACCOUNTID
                         join cu in context.TBL_CUSTOMER on c.CUSTOMERID equals cu.CUSTOMERID
                         where l.LOANREFERENCENUMBER == accountNumberOrName && l.COMPANYID == companyId &&
                         l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                         select new CasaViewModel()
                         {
                             casaAccountId = c.CASAACCOUNTID,
                             productAccountNumber = c.PRODUCTACCOUNTNUMBER,
                             productAccountName = c.PRODUCTACCOUNTNAME,
                             customerId = c.CUSTOMERID,
                             customerCode = c.TBL_CUSTOMER.CUSTOMERCODE,
                             customerName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.LASTNAME,
                             productId = c.PRODUCTID,
                             productCode = c.TBL_PRODUCT.PRODUCTCODE,
                             productName = c.TBL_PRODUCT.PRODUCTNAME,
                             companyId = c.COMPANYID,
                             branchId = c.BRANCHID,
                             currency = c.TBL_CURRENCY.CURRENCYNAME,
                             branchCode = c.TBL_BRANCH.BRANCHCODE,
                             branchName = c.TBL_BRANCH.BRANCHNAME,
                             isCurrentAccount = c.ISCURRENTACCOUNT,
                             tenor = c.TENOR ?? 0,
                             interestRate = c.INTERESTRATE ?? 0,
                             effectiveDate = c.EFFECTIVEDATE ?? General.DefaultDate,
                             terminalDate = c.TERMINALDATE ?? General.DefaultDate,
                             actionBy = c.ACTIONBY ?? 0,
                             actionDate = c.ACTIONDATE ?? General.DefaultDate,
                             accountStatusId = c.ACCOUNTSTATUSID,
                             operationId = c.OPERATIONID ?? 0,
                             availableBalance = c.AVAILABLEBALANCE,
                             ledgerBalance = c.LEDGERBALANCE,
                             relationshipOfficerId = c.RELATIONSHIPOFFICERID ?? 0,
                             misCode = c.MISCODE,
                             overdraftAmount = c.OVERDRAFTAMOUNT ?? 0,
                             overdraftInterestRate = c.OVERDRAFTINTERESTRATE ?? 0,
                             overdraftExpiryDate = c.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                             hasOverdraft = c.HASOVERDRAFT.HasValue == true ? c.HASOVERDRAFT.Value : false,
                             lienAmount = c.LIENAMOUNT,
                             hasLien = c.HASLIEN,
                             postNoStatusId = c.POSTNOSTATUSID,
                             oldProductAccountNumber1 = c.OLDPRODUCTACCOUNTNUMBER1,
                             oldProductAccountNumber2 = c.OLDPRODUCTACCOUNTNUMBER2,
                             oldProductAccountNumber3 = c.OLDPRODUCTACCOUNTNUMBER3,
                             //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                             aprovalStatusId = c.APROVALSTATUSID,
                             sourceReferenceNumber = l.LOANREFERENCENUMBER
                         });

            var data4 = (from l in context.TBL_LOAN_REVOLVING
                         join c in context.TBL_CASA on l.CASAACCOUNTID equals c.CASAACCOUNTID
                         join cu in context.TBL_CUSTOMER on c.CUSTOMERID equals cu.CUSTOMERID
                         where l.LOANREFERENCENUMBER == accountNumberOrName && l.COMPANYID == companyId &&
                         l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && l.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                         select new CasaViewModel()
                         {
                             casaAccountId = c.CASAACCOUNTID,
                             productAccountNumber = c.PRODUCTACCOUNTNUMBER,
                             productAccountName = c.PRODUCTACCOUNTNAME,
                             customerId = c.CUSTOMERID,
                             customerCode = c.TBL_CUSTOMER.CUSTOMERCODE,
                             customerName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.LASTNAME,
                             productId = c.PRODUCTID,
                             productCode = c.TBL_PRODUCT.PRODUCTCODE,
                             productName = c.TBL_PRODUCT.PRODUCTNAME,
                             companyId = c.COMPANYID,
                             branchId = c.BRANCHID,
                             currency = c.TBL_CURRENCY.CURRENCYNAME,
                             branchCode = c.TBL_BRANCH.BRANCHCODE,
                             branchName = c.TBL_BRANCH.BRANCHNAME,
                             isCurrentAccount = c.ISCURRENTACCOUNT,
                             tenor = c.TENOR ?? 0,
                             interestRate = c.INTERESTRATE ?? 0,
                             effectiveDate = c.EFFECTIVEDATE ?? General.DefaultDate,
                             terminalDate = c.TERMINALDATE ?? General.DefaultDate,
                             actionBy = c.ACTIONBY ?? 0,
                             actionDate = c.ACTIONDATE ?? General.DefaultDate,
                             accountStatusId = c.ACCOUNTSTATUSID,
                             operationId = c.OPERATIONID ?? 0,
                             availableBalance = c.AVAILABLEBALANCE,
                             ledgerBalance = c.LEDGERBALANCE,
                             relationshipOfficerId = c.RELATIONSHIPOFFICERID ?? 0,
                             misCode = c.MISCODE,
                             overdraftAmount = c.OVERDRAFTAMOUNT ?? 0,
                             overdraftInterestRate = c.OVERDRAFTINTERESTRATE ?? 0,
                             overdraftExpiryDate = c.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                             hasOverdraft = c.HASOVERDRAFT.HasValue == true ? c.HASOVERDRAFT.Value : false,
                             lienAmount = c.LIENAMOUNT,
                             hasLien = c.HASLIEN,
                             postNoStatusId = c.POSTNOSTATUSID,
                             oldProductAccountNumber1 = c.OLDPRODUCTACCOUNTNUMBER1,
                             oldProductAccountNumber2 = c.OLDPRODUCTACCOUNTNUMBER2,
                             oldProductAccountNumber3 = c.OLDPRODUCTACCOUNTNUMBER3,
                             //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                             aprovalStatusId = c.APROVALSTATUSID,
                             sourceReferenceNumber = l.LOANREFERENCENUMBER
                         });

            return data1.Union(data2).Union(data3).Union(data4);
        }

        /// TODO: Implement server side filtering due to large number of records that may be returned
        public IEnumerable<CasaViewModel> FindAccount(string accountNumberOrName, int companyId)
        {
            var result = (from data in context.TBL_CASA
                        join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                        where data.COMPANYID == companyId && (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) ||
                        cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                        cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CasaViewModel()
                        {
                            casaAccountId = data.CASAACCOUNTID,
                            productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                            productAccountName = data.PRODUCTACCOUNTNAME,
                            customerId = data.CUSTOMERID,
                            customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                            customerName = data.TBL_CUSTOMER.FIRSTNAME + " " + data.TBL_CUSTOMER.LASTNAME,
                            productId = data.PRODUCTID,
                            productCode = data.TBL_PRODUCT.PRODUCTCODE,
                            productName = data.TBL_PRODUCT.PRODUCTNAME,
                            companyId = data.COMPANYID,
                            branchId = data.BRANCHID,
                            currency = data.TBL_CURRENCY.CURRENCYNAME,
                            branchCode = data.TBL_BRANCH.BRANCHCODE,
                            branchName = data.TBL_BRANCH.BRANCHNAME,
                            isCurrentAccount = data.ISCURRENTACCOUNT,
                            tenor = data.TENOR ?? 0,
                            interestRate = data.INTERESTRATE ?? 0,
                            effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                            terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                            actionBy = data.ACTIONBY ?? 0,
                            actionDate = data.ACTIONDATE ?? General.DefaultDate,
                            accountStatusId = data.ACCOUNTSTATUSID,
                            operationId = data.OPERATIONID ?? 0,
                            availableBalance = data.AVAILABLEBALANCE,
                            ledgerBalance = data.LEDGERBALANCE,
                            relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                            misCode = data.MISCODE,
                            overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                            overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                            overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                            hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                            lienAmount = data.LIENAMOUNT,
                            hasLien = data.HASLIEN,
                            postNoStatusId = data.POSTNOSTATUSID,
                            oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                            oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                            oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                            //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0 
                            aprovalStatusId = data.APROVALSTATUSID,
                        });

         
            return result;
        }

        public IEnumerable<CasaViewModel> GetGroupAccountNumberWithCustomerId(string accountNumberOrName, int customerId, int companyId)
        {
            int customerGroupId = context.TBL_CUSTOMER_GROUP_MAPPING.FirstOrDefault(x => x.CUSTOMERID == customerId).CUSTOMERGROUPID;

            return (from data in context.TBL_CASA
                    join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                    join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID
                    where data.COMPANYID == companyId && custGroup.CUSTOMERGROUPID == customerGroupId && (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) ||
                    cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                 cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = data.TBL_CUSTOMER.FIRSTNAME + " " + data.TBL_CUSTOMER.LASTNAME,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        currency = data.TBL_CURRENCY.CURRENCYNAME,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,
                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,
                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        misCode = data.MISCODE,
                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,
                    });
        }

        public IEnumerable<CasaViewModel> GetOverdraftAccountNumberWithCustomerId(string accountNumberOrName, int customerId, int companyId)
        {
            int customerGroupId = context.TBL_CUSTOMER_GROUP_MAPPING.FirstOrDefault(x => x.CUSTOMERID == customerId)?.CUSTOMERGROUPID ?? 0;
            List<CasaViewModel> result  = new List<CasaViewModel>();
            if (customerGroupId > 0)
            {
                result = (from data in context.TBL_CASA
                          join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                          join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID
                          join od in context.TBL_LOAN_REVOLVING on data.CASAACCOUNTID equals od.CASAACCOUNTID
                          where data.COMPANYID == companyId && custGroup.CUSTOMERGROUPID == customerGroupId
                          //&& (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) ||
                          //cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                          //cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                          select new CasaViewModel
                          {
                              casaAccountId = data.CASAACCOUNTID,
                              productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                              productAccountName = data.PRODUCTACCOUNTNAME,
                              customerId = data.CUSTOMERID,
                              //customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                              customerName = data.TBL_CUSTOMER.FIRSTNAME + " " + data.TBL_CUSTOMER.LASTNAME,
                              //productId = data.PRODUCTID,
                              //productCode = data.TBL_PRODUCT.PRODUCTCODE,
                              //productName = data.TBL_PRODUCT.PRODUCTNAME,
                              //companyId = data.COMPANYID,
                              //branchId = data.BRANCHID,
                              currency = data.TBL_CURRENCY.CURRENCYNAME,
                              //branchCode = data.TBL_BRANCH.BRANCHCODE,
                              //branchName = data.TBL_BRANCH.BRANCHNAME,
                              //isCurrentAccount = data.ISCURRENTACCOUNT,
                              //tenor = data.TENOR ?? 0,
                              //interestRate = data.INTERESTRATE ?? 0,
                              //effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                              //terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                              //actionBy = data.ACTIONBY ?? 0,
                              //actionDate = data.ACTIONDATE ?? General.DefaultDate,
                              //accountStatusId = data.ACCOUNTSTATUSID,
                              //operationId = data.OPERATIONID ?? 0,
                              //availableBalance = data.AVAILABLEBALANCE,
                              //ledgerBalance = data.LEDGERBALANCE,
                              //relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                              //misCode = data.MISCODE,
                              //overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                              //overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                              //overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                              //hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                              //lienAmount = data.LIENAMOUNT,
                              //hasLien = data.HASLIEN,
                              //postNoStatusId = data.POSTNOSTATUSID,
                              //oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                              //oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                              //oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                              ////aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                              //aprovalStatusId = data.APROVALSTATUSID,
                          }).ToList();

                return result;
            }
            else
            {
                result = (from data in context.TBL_CASA
                          join cust in context.TBL_CUSTOMER on data.CUSTOMERID equals cust.CUSTOMERID
                          //join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID
                          join od in context.TBL_LOAN_REVOLVING on data.CASAACCOUNTID equals od.CASAACCOUNTID
                          where data.COMPANYID == companyId && cust.CUSTOMERID == customerId
                          //&& (data.PRODUCTACCOUNTNUMBER.Contains(accountNumberOrName) ||
                          //cust.CUSTOMERCODE.Contains(accountNumberOrName) || cust.FIRSTNAME.Contains(accountNumberOrName) ||
                          //cust.LASTNAME.Contains(accountNumberOrName)) //orderby account.AccountCode ascending, account.AccountName ascending
                          select new CasaViewModel
                          {
                              casaAccountId = data.CASAACCOUNTID,
                              productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                              productAccountName = data.PRODUCTACCOUNTNAME,
                              customerId = data.CUSTOMERID,
                              //customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                              customerName = data.TBL_CUSTOMER.FIRSTNAME + " " + data.TBL_CUSTOMER.LASTNAME,
                              //productId = data.PRODUCTID,
                              //productCode = data.TBL_PRODUCT.PRODUCTCODE,
                              //productName = data.TBL_PRODUCT.PRODUCTNAME,
                              //companyId = data.COMPANYID,
                              //branchId = data.BRANCHID,
                              currency = data.TBL_CURRENCY.CURRENCYNAME,
                              //branchCode = data.TBL_BRANCH.BRANCHCODE,
                              //branchName = data.TBL_BRANCH.BRANCHNAME,
                              //isCurrentAccount = data.ISCURRENTACCOUNT,
                              //tenor = data.TENOR ?? 0,
                              //interestRate = data.INTERESTRATE ?? 0,
                              //effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                              //terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                              //actionBy = data.ACTIONBY ?? 0,
                              //actionDate = data.ACTIONDATE ?? General.DefaultDate,
                              //accountStatusId = data.ACCOUNTSTATUSID,
                              //operationId = data.OPERATIONID ?? 0,
                              //availableBalance = data.AVAILABLEBALANCE,
                              //ledgerBalance = data.LEDGERBALANCE,
                              //relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                              //misCode = data.MISCODE,
                              //overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                              //overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                              //overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                              //hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                              //lienAmount = data.LIENAMOUNT,
                              //hasLien = data.HASLIEN,
                              //postNoStatusId = data.POSTNOSTATUSID,
                              //oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                              //oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                              //oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                              ////aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                              //aprovalStatusId = data.APROVALSTATUSID,
                          }).ToList();

                return result;
            }
          
        }

        public CasaViewModel GetAccount(int casaAccountId)
        {
            return (from data in context.TBL_CASA
                    where data.CASAACCOUNTID == casaAccountId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,

                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,

                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        misCode = data.MISCODE,

                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,

                        refreshBatchId = data.REFRESHBATCHID,
                        lastRefreshDatetime = data.LASTREFRESHDATETIME,
                        createdBy = data.CREATEDBY ?? 0,
                        lastUpdatedBy = data.LASTUPDATEDBY ?? 0,
                        dateTimeCreated = (DateTime)data.DATETIMECREATED,
                        dateTimeUpdated = data.DATETIMEUPDATED,
                        deleted = data.DELETED,
                        deletedBy = data.DELETEDBY,
                        dateTimeDeleted = data.DATETIMEDELETED
                    }).FirstOrDefault();
        }

        public IEnumerable<CustomerCasaAcountsViewModel> GetAllCustomerAccount(int customerId, int applicationTypeId, int companyId)
        {
            IEnumerable<CustomerCasaAcountsViewModel> data = null;
            if (applicationTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                //data = (from a in  context.TBL_CUSTOMER_GROUP_MAPPING join b in context.TBL_CASA on a.CUSTOMERID equals b.CUSTOMERID 
                //            where a.CUSTOMERID == customerId && a.TBL_CUSTOMER.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                //            select new
                //            {
                //                casaAccountId = b.CASAACCOUNTID,
                //                productAccountNumber = b.PRODUCTACCOUNTNUMBER + "(" + b.PRODUCTACCOUNTNAME + " - " + b.TBL_CURRENCY.CURRENCYCODE + ")",
                //                productAccountName = b.PRODUCTACCOUNTNAME,
                //                availableBalance = b.AVAILABLEBALANCE
                //            }).Distinct();

                data = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                        join b in context.TBL_CASA on a.CUSTOMERID equals b.CUSTOMERID
                        where a.CUSTOMERGROUPID == customerId && a.TBL_CUSTOMER.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CustomerCasaAcountsViewModel
                        {
                            casaAccountId = b.CASAACCOUNTID,
                            productAccountNumber = b.PRODUCTACCOUNTNUMBER + "(" + b.PRODUCTACCOUNTNAME + " - " + b.TBL_CURRENCY.CURRENCYCODE + ")",
                            productAccountName = b.PRODUCTACCOUNTNAME,
                            availableBalance = b.AVAILABLEBALANCE
                        }).Distinct();
            }

            if (applicationTypeId == (int)LoanTypeEnum.Single)
            {
                // data = new List<CasaViewModel>();
                //var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                //if (setup.USE_THIRD_PARTY_INTEGRATION)
                //{
                //    var customerinfo = (from a in context.TBL_CUSTOMER
                //                        where a.CUSTOMERID == customerId
                //                        select new CasaViewModel
                //                        {
                //                            customerCode = a.CUSTOMERCODE,
                //                        }).ToList();
                //    if (customerinfo.Count > 0)
                //    {
                //        CustomerDetails customer = new CustomerDetails();
                //        Task.Run(async () => { data = await customer.GetCustomerAccountsBalance(customerinfo[0].customerCode); }).GetAwaiter().GetResult();
                //    }
                //    return data;
                //}
                //else
                //{
                data = (from a in context.TBL_CASA
                        where a.CUSTOMERID == customerId && a.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CustomerCasaAcountsViewModel

                        {
                            casaAccountId = a.CASAACCOUNTID,
                            productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                            productAccountName = a.PRODUCTACCOUNTNAME,
                            availableBalance = a.AVAILABLEBALANCE
                        }).Distinct();
                //}
                return data.OrderBy(x=> x.productAccountName);
            }



            return data.OrderBy(x => x.productAccountName); ;
        }

        public IEnumerable<CasaViewModel> GetAccountByCustomerId(int customerId)
        {
            return (from data in context.TBL_CASA
                    where data.CUSTOMERID == customerId //orderby account.AccountCode ascending, account.AccountName ascending
                    select new CasaViewModel()
                    {
                        casaAccountId = data.CASAACCOUNTID,
                        productAccountNumber = data.PRODUCTACCOUNTNUMBER,
                        productAccountName = data.PRODUCTACCOUNTNAME,
                        customerId = data.CUSTOMERID,
                        customerCode = data.TBL_CUSTOMER.CUSTOMERCODE,
                        productId = data.PRODUCTID,
                        productCode = data.TBL_PRODUCT.PRODUCTCODE,
                        productName = data.TBL_PRODUCT.PRODUCTNAME,
                        companyId = data.COMPANYID,
                        branchId = data.BRANCHID,
                        branchCode = data.TBL_BRANCH.BRANCHCODE,
                        branchName = data.TBL_BRANCH.BRANCHNAME,
                        isCurrentAccount = data.ISCURRENTACCOUNT,
                        tenor = data.TENOR ?? 0,
                        interestRate = data.INTERESTRATE ?? 0,
                        effectiveDate = data.EFFECTIVEDATE ?? General.DefaultDate,
                        terminalDate = data.TERMINALDATE ?? General.DefaultDate,
                        actionBy = data.ACTIONBY ?? 0,
                        actionDate = data.ACTIONDATE ?? General.DefaultDate,
                        accountStatusId = data.ACCOUNTSTATUSID,
                        operationId = data.OPERATIONID ?? 0,
                        availableBalance = data.AVAILABLEBALANCE,
                        ledgerBalance = data.LEDGERBALANCE,

                        relationshipOfficerId = data.RELATIONSHIPOFFICERID ?? 0,
                        relationshipManagerId = data.RELATIONSHIPMANAGERID ?? 0,
                        misCode = data.MISCODE,

                        overdraftAmount = data.OVERDRAFTAMOUNT ?? 0,
                        overdraftInterestRate = data.OVERDRAFTINTERESTRATE ?? 0,
                        overdraftExpiryDate = data.OVERDRAFTEXPIRYDATE ?? General.DefaultDate,
                        hasOverdraft = data.HASOVERDRAFT.HasValue == true ? data.HASOVERDRAFT.Value : false,
                        lienAmount = data.LIENAMOUNT,
                        hasLien = data.HASLIEN,
                        postNoStatusId = data.POSTNOSTATUSID,
                        oldProductAccountNumber1 = data.OLDPRODUCTACCOUNTNUMBER1,
                        oldProductAccountNumber2 = data.OLDPRODUCTACCOUNTNUMBER2,
                        oldProductAccountNumber3 = data.OLDPRODUCTACCOUNTNUMBER3,
                        //aprovalStatusId = data.AprovalStatusId.HasValue == true ? (short) data.AprovalStatusId.Value : (short) 0
                        aprovalStatusId = data.APROVALSTATUSID,

                        //refreshBatchId = data.RefreshBatchId,
                        //lastRefreshDatetime = data.LastRefreshDatetime,
                        //createdBy = data.CreatedBy ?? 0,
                        //lastUpdatedBy = data.LastUpdatedBy ?? 0,
                        //dateTimeCreated = data.DateTimeCreated,
                        //dateTimeUpdated = data.DateTimeUpdated,
                        //deleted = data.Deleted,
                        //deletedBy = data.DeletedBy,
                        //dateTimeDeleted = data.DateTimeDeleted
                    });
        }

        public IQueryable<CustomerSearchVM> SearchCustomer(int customerTypeId, int companyId, string searchQuery)
        {
            if (customerTypeId == 0) return null;
            IQueryable<CustomerSearchVM> allCustomer = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }
            if (customerTypeId < 3)
            {
                allCustomer = from cust in context.TBL_CUSTOMER
                              join acc in context.TBL_CASA on cust.CUSTOMERID equals acc.CUSTOMERID
                              join prod in context.TBL_PRODUCT on acc.PRODUCTID equals prod.PRODUCTID
                              where cust.DELETED == false && cust.COMPANYID == companyId
                              select new CustomerSearchVM
                              {
                                  customerId = cust.CUSTOMERID,
                                  accountNumber = acc.PRODUCTACCOUNTNUMBER,
                                  customerCode = cust.CUSTOMERCODE,
                                  firstName = cust.FIRSTNAME,
                                  lastName = cust.LASTNAME,
                                  middleName = cust.MAIDENNAME,
                                  relationshipManagerId = acc.RELATIONSHIPMANAGERID ?? 0,
                                  relationshipOfficerId = acc.RELATIONSHIPOFFICERID ?? 0
                              };

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allCustomer = allCustomer
                        .Where(x => x.firstName.ToLower().Contains(searchQuery)
                        || x.middleName.ToLower().Contains(searchQuery)
                        || x.lastName.ToLower().Contains(searchQuery)
                        || x.accountNumber.Contains(searchQuery));
                }
            }
            else
            {
                allCustomer = from cg in context.TBL_CUSTOMER_GROUP
                              join gm in context.TBL_CUSTOMER_GROUP_MAPPING
                              on cg.CUSTOMERGROUPID equals gm.CUSTOMERGROUPID
                              join casa in context.TBL_CASA
                              on gm.CUSTOMERID equals casa.CUSTOMERID
                              join prod in context.TBL_PRODUCT on casa.PRODUCTID equals prod.PRODUCTID
                              where cg.DELETED == false && gm.DELETED == false && casa.DELETED == false
                              select new CustomerSearchVM
                              {
                                  customerId = cg.CUSTOMERGROUPID,
                                  customerCode = cg.GROUPCODE,
                                  firstName = cg.GROUPNAME,
                                  lastName = string.Empty,
                                  accountNumber = casa.PRODUCTACCOUNTNUMBER,
                                  relationshipManagerId = casa.RELATIONSHIPMANAGERID ?? 0,
                                  relationshipOfficerId = casa.RELATIONSHIPOFFICERID ?? 0
                              };

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    allCustomer = allCustomer
                        .Where(x => x.firstName.ToLower().Contains(searchQuery)
                        || x.middleName.ToLower().Contains(searchQuery)
                        || x.lastName.ToLower().Contains(searchQuery)
                        || x.accountNumber.Contains(searchQuery));
                }
            }

            return allCustomer;
        }

        public IEnumerable<GroupCustomerMembersViewModel> GetGroupMembersByGroupId(int customerId, int companyId)
        {
            var customerGroupMapping = from b in context.TBL_CASA
                                       where b.CUSTOMERID == customerId && b.DELETED == false && b.COMPANYID == companyId
                                       select new GroupCustomerMembersViewModel
                                       {
                                           customerId = b.CUSTOMERID,
                                           customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                           lastName = b.TBL_CUSTOMER.LASTNAME,
                                           firstName = b.TBL_CUSTOMER.FIRSTNAME,
                                           customerTypeId = (short)b.TBL_CUSTOMER.CUSTOMERTYPEID,
                                           customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                       };

            return customerGroupMapping;
        }

        private IQueryable<CasaCustomerSearchViewModel> GetAllAccountLight()
        {
            var data = (from casa in context.TBL_CASA
                        join cust in context.TBL_CUSTOMER on casa.CUSTOMERID equals cust.CUSTOMERID
                        //join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                        join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        select new CasaCustomerSearchViewModel()
                        {
                            casaAccountId = casa.CASAACCOUNTID,
                            productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                            productAccountName = casa.PRODUCTACCOUNTNAME,
                            customerId = casa.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            productId = casa.TBL_PRODUCT.PRODUCTID,
                            productCode = casa.TBL_PRODUCT.PRODUCTCODE,
                            productName = casa.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = casa.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = casa.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            companyId = casa.COMPANYID,
                            branchId = casa.BRANCHID,
                            branchCode = casa.TBL_BRANCH.BRANCHCODE,
                            branchName = casa.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = cust.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = cust.TBL_STAFF.SUPERVISOR_STAFFID ?? 0,
                            subSectorId = sector.SUBSECTORID,
                            subSectorName = sector.NAME,
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerSectorId = sector.TBL_SECTOR.SECTORID,
                            customerSectorName = sector.TBL_SECTOR.NAME,
                            customerGroupId = custGroup.CUSTOMERGROUPID,
                            customerGroupName = custGroup.TBL_CUSTOMER_GROUP.GROUPNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER
                        });

            return data;
        }

        private IQueryable<CasaCustomerSearchViewModel> GetAllAccounts()
        {
            var data = (from casa in context.TBL_CASA
                        join cust in context.TBL_CUSTOMER on casa.CUSTOMERID equals cust.CUSTOMERID
                        //join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                        join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        select new CasaCustomerSearchViewModel()
                        {
                            casaAccountId = casa.CASAACCOUNTID,
                            productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                            productAccountName = casa.PRODUCTACCOUNTNAME,
                            customerId = casa.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            productId = casa.TBL_PRODUCT.PRODUCTID,
                            productCode = casa.TBL_PRODUCT.PRODUCTCODE,
                            productName = casa.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = casa.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = casa.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            companyId = casa.COMPANYID,
                            branchId = casa.BRANCHID,
                            branchCode = casa.TBL_BRANCH.BRANCHCODE,
                            branchName = casa.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = casa.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = casa.RELATIONSHIPMANAGERID ?? 0,
                            subSectorId = sector.SUBSECTORID,
                            subSectorName = sector.NAME,
                            customerSectorId = sector.TBL_SECTOR.SECTORID,
                            customerSectorName = sector.TBL_SECTOR.NAME,
                            customerGroupId = custGroup.CUSTOMERGROUPID,
                            customerGroupName = custGroup.TBL_CUSTOMER_GROUP.GROUPNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER,
                            registrationNumber = cust.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == cust.CUSTOMERID).REGISTRATIONNUMBER,
                            isBlackList = context.TBL_CUSTOMER_BLACKLIST.Any(x => x.CUSTOMERCODE == cust.CUSTOMERCODE),
                            isOnWatchList = context.TBL_LOAN_PRUDENTIALGUIDELINE.Any(x => x.TBL_LOAN.Any(l => l.CUSTOMERID == cust.CUSTOMERID && l.EXT_PRUDENT_GUIDELINE_STATUSID == (int)LoanPrudentialStatusEnum.WatchList)),
                            isCamsol = context.TBL_LOAN_CAMSOL.Any(x => context.TBL_LOAN.Any(l => l.TERMLOANID == x.LOANID && l.CUSTOMERID == cust.CUSTOMERID)),
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerTypeName = cust.TBL_CUSTOMER_TYPE.NAME,
                            completedInformation = cust.ACCOUNTCREATIONCOMPLETE,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == casa.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == casa.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == casa.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == casa.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == casa.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                        });

            return data;
        }

        public IQueryable<CasaCustomerSearchViewModel> SearchForCustomerAccount(int companyId, string searchQuery, int customerTypeId)
        {
            IQueryable<CasaCustomerSearchViewModel> allCustomers = null;

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                allCustomers = GetAllAccounts()
                    .Where(c => c.companyId == companyId)
                    .Where(ct => ct.customerTypeId == customerTypeId)
                    .Where(x => x.accountHolder.Contains(searchQuery)
               || x.customerCode.Contains(searchQuery)
               || x.productAccountNumber.Contains(searchQuery)
                ).GroupBy(c => c.customerId).Select(g => g.FirstOrDefault()).Take(10);
            }

            return allCustomers;
        }

        public CasaCustomerSearchViewModel GetCustomerAccountDetailsById(int customerId)
        {
            var data = (from casa in context.TBL_CASA
                        join cust in context.TBL_CUSTOMER on casa.CUSTOMERID equals cust.CUSTOMERID
                        join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                        join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        where cust.CUSTOMERID == customerId
                        select new CasaCustomerSearchViewModel()
                        {
                            casaAccountId = casa.CASAACCOUNTID,
                            productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                            productAccountName = casa.PRODUCTACCOUNTNAME,
                            customerId = casa.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            productId = casa.TBL_PRODUCT.PRODUCTID,
                            productCode = casa.TBL_PRODUCT.PRODUCTCODE,
                            productName = casa.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = casa.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = casa.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            companyId = casa.COMPANYID,
                            branchId = casa.BRANCHID,
                            branchCode = casa.TBL_BRANCH.BRANCHCODE,
                            branchName = casa.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = cust.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = cust.TBL_STAFF.SUPERVISOR_STAFFID ?? 0,
                            subSectorId = sector.SUBSECTORID,
                            subSectorName = sector.NAME,
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerSectorId = sector.TBL_SECTOR.SECTORID,
                            customerSectorName = sector.TBL_SECTOR.NAME,
                            customerGroupId = custGroup.CUSTOMERGROUPID,
                            customerGroupName = custGroup.TBL_CUSTOMER_GROUP.GROUPNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER
                        }).ToList();

            if(data.Count() <= 0)
            {
                data = (from casa in context.TBL_CASA
                         join cust in context.TBL_CUSTOMER on casa.CUSTOMERID equals cust.CUSTOMERID
                         join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                         join custGroup in context.TBL_CUSTOMER on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                         from custGroup in cGroup.DefaultIfEmpty()
                         where cust.CUSTOMERID == customerId
                         select new CasaCustomerSearchViewModel()
                         {
                             casaAccountId = casa.CASAACCOUNTID,
                             productAccountNumber = casa.PRODUCTACCOUNTNUMBER,
                             productAccountName = casa.PRODUCTACCOUNTNAME,
                             customerId = casa.CUSTOMERID,
                             customerCode = cust.CUSTOMERCODE,
                             accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                             productId = casa.TBL_PRODUCT.PRODUCTID,
                             productCode = casa.TBL_PRODUCT.PRODUCTCODE,
                             productName = casa.TBL_PRODUCT.PRODUCTNAME,
                             productClassId = casa.TBL_PRODUCT.PRODUCTCLASSID,
                             productClassName = casa.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                             companyId = casa.COMPANYID,
                             branchId = casa.BRANCHID,
                             branchCode = casa.TBL_BRANCH.BRANCHCODE,
                             branchName = casa.TBL_BRANCH.BRANCHNAME,
                             relationshipOfficerId = cust.RELATIONSHIPOFFICERID ?? 0,
                             relationshipManagerId = cust.TBL_STAFF.SUPERVISOR_STAFFID ?? 0,
                             subSectorId = sector.SUBSECTORID,
                             subSectorName = sector.NAME,
                             customerTypeId = cust.CUSTOMERTYPEID,
                             customerSectorId = sector.TBL_SECTOR.SECTORID,
                             customerSectorName = sector.TBL_SECTOR.NAME,
                             customerGroupId = custGroup.CUSTOMERID,
                             customerGroupName = custGroup.FIRSTNAME + " " + custGroup.LASTNAME ?? "None",
                             taxIdentificationNumber = cust.TAXNUMBER
                         }).ToList();
            }

            if (data.Count() <= 0)
            {
                data = (from cust in context.TBL_CUSTOMER
                            //join prod in context.tbl_Product on casa.ProductId equals prod.ProductId
                        join sector in context.TBL_SUB_SECTOR on cust.SUBSECTORID equals sector.SUBSECTORID
                        join custGroup in context.TBL_CUSTOMER on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        where cust.CUSTOMERID == customerId
                        select new CasaCustomerSearchViewModel()
                        {
                            customerId = cust.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            companyId = cust.COMPANYID,
                            branchId = cust.BRANCHID,
                            branchCode = cust.TBL_BRANCH.BRANCHCODE,
                            branchName = cust.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = cust.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = cust.TBL_STAFF.SUPERVISOR_STAFFID ?? 0,
                            subSectorId = sector.SUBSECTORID,
                            subSectorName = sector.NAME,
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerSectorId = sector.TBL_SECTOR.SECTORID,
                            customerSectorName = sector.TBL_SECTOR.NAME,
                            customerGroupId = custGroup.CUSTOMERID,
                            customerGroupName = custGroup.FIRSTNAME + " " + custGroup.LASTNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER
                        }).ToList();
            }

            if (data.Count() <= 0)
            {
                data = (from cust in context.TBL_CUSTOMER
                        join custGroup in context.TBL_CUSTOMER_GROUP_MAPPING on cust.CUSTOMERID equals custGroup.CUSTOMERID into cGroup
                        from custGroup in cGroup.DefaultIfEmpty()
                        where cust.CUSTOMERID == customerId
                        select new CasaCustomerSearchViewModel()
                        {
                            customerId = cust.CUSTOMERID,
                            customerCode = cust.CUSTOMERCODE,
                            accountHolder = cust.FIRSTNAME + " " + cust.LASTNAME,
                            companyId = cust.COMPANYID,
                            branchId = cust.BRANCHID,
                            branchCode = cust.TBL_BRANCH.BRANCHCODE,
                            branchName = cust.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = cust.RELATIONSHIPOFFICERID ?? 0,
                            relationshipManagerId = cust.TBL_STAFF.SUPERVISOR_STAFFID ?? 0,
                            customerTypeId = cust.CUSTOMERTYPEID,
                            customerGroupId = custGroup.CUSTOMERID,
                            customerGroupName = custGroup.TBL_CUSTOMER_GROUP.GROUPNAME ?? "None",
                            taxIdentificationNumber = cust.TAXNUMBER
                        }).ToList();
            }

            if (data.Count() > 0)
            {
                return data.FirstOrDefault();
            }

            return new CasaCustomerSearchViewModel { };
        }

        public IEnumerable<CasaBalanceViewModel> GetAllCustomerAccountByCustomerIdAndCurrency(int customerId, int companyId, int currencyId)
        {
            var data = (from a in context.TBL_CASA
                        where a.CUSTOMERID == customerId && a.COMPANYID == companyId && a.CURRENCYID == currencyId //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CasaBalanceViewModel

                        {
                            casaAccountId = a.CASAACCOUNTID,
                            productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                            accountNumber = a.PRODUCTACCOUNTNUMBER,
                            productAccountName = a.PRODUCTACCOUNTNAME,
                            availableBalance = a.AVAILABLEBALANCE, //transRepo.GetCASABalance(a.CASAACCOUNTID).availableBalance,
                            currencyId = a.CURRENCYID,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE
                        });
            //foreach (var item in data)
            //{
            //    item.availableBalance = transRepo.GetCASABalance(item.casaAccountId).availableBalance;
            //}

            return data.ToList();
        }

        public IEnumerable<CasaBalanceViewModel> GetAllCustomerAccountByCustomerId(int customerId, int companyId)
        {
            var data = (from a in context.TBL_CASA
                        where a.CUSTOMERID == customerId && a.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CasaBalanceViewModel

                        {
                            casaAccountId = a.CASAACCOUNTID,
                            productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                            accountNumber = a.PRODUCTACCOUNTNUMBER,
                            productAccountName = a.PRODUCTACCOUNTNAME,
                            availableBalance = a.AVAILABLEBALANCE, //transRepo.GetCASABalance(a.CASAACCOUNTID).availableBalance,
                            currencyId = a.CURRENCYID,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE
                        });
            //foreach (var item in data)
            //{
            //    item.availableBalance = transRepo.GetCASABalance(item.casaAccountId).availableBalance;
            //}

            return data.ToList();
        }

        public IEnumerable<CasaBalanceViewModel> GetBusinessAccounts( int companyId)
        {
            var data = (from a in context.TBL_CASA
                        where a.COMPANYID == companyId  //orderby account.AccountCode ascending, account.AccountName ascending
                        select new CasaBalanceViewModel

                        {
                            casaAccountId = a.CASAACCOUNTID,
                            productAccountNumber = a.PRODUCTACCOUNTNUMBER + "(" + a.PRODUCTACCOUNTNAME + " - " + a.TBL_CURRENCY.CURRENCYCODE + ")",
                            accountNumber = a.PRODUCTACCOUNTNUMBER,
                            productAccountName = a.PRODUCTACCOUNTNAME,
                            availableBalance = a.AVAILABLEBALANCE, //transRepo.GetCASABalance(a.CASAACCOUNTID).availableBalance,
                            currencyId = a.CURRENCYID,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE
                        });
            //foreach (var item in data)
            //{
            //    item.availableBalance = transRepo.GetCASABalance(item.casaAccountId).availableBalance;
            //}

            return data.ToList();
        }

        public void AddCustomerAccounts(string customerCode)
        {
            if (customerCode == null || customerCode == string.Empty) return;
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                integration.AddCustomerAccounts(customerCode);
            }
        }

    }
  
}