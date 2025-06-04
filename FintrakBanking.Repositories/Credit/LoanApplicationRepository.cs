using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
using System.Configuration;
using System.Data.Entity;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Customer;
using GemBox.Spreadsheet;
using System.IO;
using System.Data.Entity.Validation;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.Entities.StagingModels;
using Microsoft.Office.Interop.Excel;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext scontext;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workflow;
        private ICasaRepository casa;
        private ICreditLimitValidationsRepository limitValidation;
        private ICustomerCollateralRepository collateral;
        private IFinanceTransactionRepository fina;
        private IIntegrationWithFinacle integration;
        private IApprovalLevelStaffRepository approvalLevel;
        private CreditCommonRepository creditCommon;
        private int? workflowProductId = null;


        public int response { get; set; }
        public bool isGroupLoan { get; set; }
        public TBL_LOAN_APPLICATION loanData { get; set; }
        public TBL_EXCEPTIONAL_LOAN_APPLICATION exceptionalLoanData { get; set; }

        public LoanApplicationRepository(
            IAuditTrailRepository _auditTrail,
            ICustomerCollateralRepository _collateral,
            IFinanceTransactionRepository _fina,
            ICasaRepository _casa,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            FinTrakBankingStagingContext _scontext,
            IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workflow,
            IIntegrationWithFinacle _integration,
            ICreditLimitValidationsRepository _limitValidation,
            CreditCommonRepository _creditCommon

            )
        {
            auditTrail = _auditTrail;
            this.collateral = _collateral;
            this.fina = _fina;
            this.casa = _casa;
            this.genSetup = _genSetup;
            this.context = _context;
            this.scontext = _scontext;
            approvalLevel = _approvallevel;
            workflow = _workflow;
            this.integration = _integration;
            this.limitValidation = _limitValidation;
            this.creditCommon = _creditCommon;
        }

        // public

        public IEnumerable<ExistingLoanApplicationViewModel> ExistingLoanApplication(int customerId, int companyId)
        {
            var data = context.TBL_LOAN_APPLICATION.Where(c => c.CUSTOMERID == customerId && c.COMPANYID == companyId)
                .Select(c => new ExistingLoanApplicationViewModel()
                {
                    applicationDate = c.APPLICATIONDATE,
                    applicationReferenceNumber = c.APPLICATIONREFERENCENUMBER,
                    interestRate = c.INTERESTRATE,
                    loanTypeName = c.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                    branch = c.TBL_BRANCH.BRANCHNAME,
                    principalAmount = c.APPLICATIONAMOUNT,
                    tenor = c.APPLICATIONTENOR
                }).ToList();
            return data;
        }

        public CurrencyExchangeRateViewModel GetExchangeRate(DateTime date, short currencyId, int companyId)
        {
            var rate = fina.GetExchangeRate(date, currencyId, companyId);
            return rate;
        }

        private LoanApplicationViewModel GetLoanApplicationByLoanRefrenceNo(string loanApplicationRef, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        && a.APPLICATIONREFERENCENUMBER == loanApplicationRef
                        select new LoanApplicationViewModel
                        {
                            applicationAmount = a.APPLICATIONAMOUNT,
                            applicationDate = a.APPLICATIONDATE,
                            applicationStatus = a.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME,
                            applicationStatusId = (short)a.APPROVALSTATUSID,
                            applicationTenor = a.APPLICATIONTENOR,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            branchId = a.BRANCHID,
                            companyId = a.COMPANYID,
                            relatedReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            iblRequest = a.IBLREQUEST,
                            casaAccountId = a.CASAACCOUNTID,
                            requireCollateral = a.REQUIRECOLLATERAL,
                            interestRate = a.INTERESTRATE,
                            loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerGroupId = a.CUSTOMERGROUPID,
                            singleCustomerId = a.CUSTOMERID,
                            createdBy = a.CREATEDBY,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            isCollateralBacked = a.REQUIRECOLLATERAL,
                            tenor = a.APPLICATIONTENOR,
                            productClassId = a.PRODUCTCLASSID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            isadhocapplication = a.ISADHOCAPPLICATION,
                            loansWithOthers = a.LOANSWITHOTHERS,
                            ownershipStructure = a.OWNERSHIPSTRUCTURE,
                            loanApprovedLimitId = a.LOANAPPROVEDLIMITID,


                            LoanApplicationDetail = a.TBL_LOAN_APPLICATION_DETAIL.Where(b => b.LOANAPPLICATIONID == a.LOANAPPLICATIONID).Select(b => new LoanApplicationDetailViewModel
                            {

                                approvedAmount = b.APPROVEDAMOUNT,

                                approvedInterestRate = b.APPROVEDINTERESTRATE,
                                approvedProductId = b.APPROVEDPRODUCTID,
                                approvedTenor = b.APPROVEDTENOR,
                                currencyId = b.CURRENCYID,
                                currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                                casaAccountId = b.TBL_LOAN_APPLICATION.CASAACCOUNTID,
                                customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                                customerId = b.CUSTOMERID,
                                exchangeRate = b.EXCHANGERATE,
                                loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                                subSectorId = b.SUBSECTORID,
                                sectorName = b.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "/" + b.TBL_SUB_SECTOR.NAME,
                                applicationReferenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                loanApplicationId = b.LOANAPPLICATIONID,
                                proposedAmount = b.PROPOSEDAMOUNT,
                                proposedInterestRate = (double)b.PROPOSEDINTERESTRATE,
                                proposedProductId = b.PROPOSEDPRODUCTID,
                                proposedTenor = b.PROPOSEDTENOR, //Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                                statusId = b.STATUSID,
                                oldApplicationRefForRenewal = b.OLDAPPLICATIONREFFORRENEWAL,
                                proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                                productClassId = b.TBL_PRODUCT.PRODUCTCLASSID,
                                productClass = b.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                invoiceDetails = b.TBL_LOAN_APPLICATION_DETL_INV.Where(i => i.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(i => new InvoiceDetailViewModel
                                {
                                    approvalStatusId = i.APPROVALSTATUSID,
                                    contractNo = i.CONTRACTNO,
                                    invoiceCurrencyId = i.INVOICE_CURRENCYID,
                                    invoiceCurrencyName = i.TBL_CURRENCY.CURRENCYNAME,
                                    invoiceId = i.INVOICEID,
                                    invoiceAmount = i.INVOICE_AMOUNT,
                                    invoiceNo = i.INVOICENO,
                                    contractEndDate = i.CONTRACT_ENDDATE,
                                    contractStartDate = i.CONTRACT_STARTDATE,
                                    invoiceDate = i.INVOICE_DATE,
                                    principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                    principalId = i.PRINCIPALID,
                                    purchaseOrderNumber = i.PURCHASEORDERNUMBER,
                                    reValidated = i.REVALIDATED,
                                    entrySheetNumber = i.ENTRYSHEETNUMBER,
                                }).ToList(),
                                educationLoan = b.TBL_LOAN_APPLICATION_DETL_EDU.Where(i => i.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(x => new EducationLoanViewModel
                                {
                                    educationId = x.EDUCATIONID,
                                    loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                    numberOfStudent = x.NUMBER_OF_STUDENTS,
                                    averageSchoolFees = x.AVERAGE_SCHOOL_FEES,
                                    totalPreviousTermSchoolFees = x.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                    productClassId = context.TBL_PRODUCT_CLASS.Where(g => g.PRODUCTCLASSID == x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                    productClassName = context.TBL_PRODUCT_CLASS.Where(g => g.PRODUCTCLASSID == x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                }).FirstOrDefault(),
                                traderLoan = b.TBL_LOAN_APPLICATION_DETL_TRA.Where(i => i.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(y => new TraderLoanViewModel
                                {
                                    tradderId = y.TRADDERID,
                                    marketId = y.MARKETID,
                                    marketName = y.TBL_LOAN_MARKET.MARKETNAME,
                                    soldItems = y.SOLDITEMS,
                                    averageMonthlyTurnover = y.AVERAGE_MONTHLY_TURNOVER,
                                    loanApplicationDetailId = y.LOANAPPLICATIONDETAILID,

                                }).FirstOrDefault(),
                                bondDetails = b.TBL_LOAN_APPLICATION_DETL_BG.Where(i => i.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(d =>
                                              new BondsAndGuranty
                                              {
                                                  loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                                  principalId = d.PRINCIPALID,
                                                  bondAmount = d.AMOUNT,
                                                  bondCurrencyId = d.CURRENCYID,
                                                  contractStartDate = d.CONTRACT_STARTDATE,
                                                  contractEndDate = d.CONTRACT_ENDDATE,
                                                  isTenored = d.ISTENORED,
                                                  isBankFormat = d.ISBANKFORMAT,
                                                  casaAccountId = d.CASAACCOUNTID,
                                                  referenceNo = d.REFERENCENO,
                                              }).FirstOrDefault(),
                                productFees = b.TBL_LOAN_APPLICATION_DETL_FEE.Where(i => i.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID).Select(f => new ProductFeesViewModel
                                {
                                    feeId = f.CHARGEFEEID,
                                    loanApplicationDetailId = f.LOANAPPLICATIONDETAILID,
                                    consessionReason = f.CONSESSIONREASON,
                                    defaultfeeRateValue = f.DEFAULT_FEERATEVALUE,
                                    recommededFeeRateValue = f.RECOMMENDED_FEERATEVALUE,
                                    hasConsession = f.HASCONSESSION,
                                    feeName = f.TBL_CHARGE_FEE.CHARGEFEENAME
                                }).ToList()

                            }).ToList()
                        });
            var test = data.FirstOrDefault();
            return data.FirstOrDefault();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationDedubeCheck(int customerId, int companyId)
        {
            var data = GetLoanApplications(companyId).Where(c => c.customerId == customerId
           && c.approvalStatusId != (int)ApprovalStatusEnum.Approved && c.approvalStatusId != (int)ApprovalStatusEnum.Disapproved);
            return data.ToList();
        }

        private IQueryable<LoanApplicationViewModel> GetLoanApplications(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationViewModel
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID ?? 0,
                            customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            misCode = a.MISCODE,
                            teamMisCode = a.TEAMMISCODE,
                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                            customerGroupId = a.CUSTOMERGROUPID ?? 0,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            isEmployerRelated = a.ISEMPLOYERRELATED,
                            employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == a.RELATEDEMPLOYERID).EMPLOYER_NAME,
                            createdBy = a.OWNEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            applicationTenor = a.APPLICATIONTENOR,
                            applicationAmount = a.APPLICATIONAMOUNT,
                            dateTimeCreated = a.DATETIMECREATED,
                            isExternal = a.ISFROMEXTERNALSOURCE ?? false,
                            LoanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONID)
                             .Select(c => new LoanApplicationDetailViewModel()
                             {
                                 equityAmount = c.EQUITYAMOUNT,
                                 equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                 approvedAmount = c.APPROVEDAMOUNT,
                                 approvedInterestRate = c.APPROVEDINTERESTRATE,
                                 approvedProductId = c.APPROVEDPRODUCTID,
                                 approvedTenor = c.APPROVEDTENOR,
                                 currencyId = c.CURRENCYID,
                                 currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                 customerId = c.CUSTOMERID,
                                 exchangeRate = c.EXCHANGERATE,
                                 loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                 subSectorId = c.SUBSECTORID,
                                 loanApplicationId = c.LOANAPPLICATIONID,
                                 proposedAmount = c.PROPOSEDAMOUNT,
                                 proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                 proposedProductId = c.PROPOSEDPRODUCTID,
                                 proposedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                 oldApplicationRefForRenewal = c.OLDAPPLICATIONREFFORRENEWAL,
                                 //proposedTenor = Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                                 statusId = c.STATUSID
                             }).ToList()
                        });
            return data;
        }

        public IEnumerable<LoanApplicationViewModel> GetAllLoanApplications(int companyId)
        {
            return GetLoanApplications(companyId).ToList();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationJobs(int companyId, int levelId, int scope)
        {
            var applications = GetLoanApplications(companyId)
                .Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending); // scope 3 entire process

            if (scope == (int)ProcessViewScopeEnum.Group)
            {
                int levelGroupId;
                var level = context.TBL_APPROVAL_LEVEL.Find(levelId);

                if (level != null)
                {
                    levelGroupId = (int)level.GROUPID;

                    var groupApprovalLevelIds = context.TBL_APPROVAL_LEVEL
                        .Where(x => x.GROUPID == levelGroupId)
                        .Select(x => x.APPROVALLEVELID);

                    applications = applications.Where(x => groupApprovalLevelIds.Contains(x.approvalLevelId));
                }
            }

            if (scope == (int)ProcessViewScopeEnum.Level)
            {
                applications = applications.Where(x => x.approvalLevelId == levelId);
            }

            return applications
                .OrderByDescending(x => x.applicationDate)
                .ThenByDescending(x => x.loanApplicationId)
                .ToList();
        }

        public IEnumerable<LoanApplicationViewModel> GetLoanApplicationById(int loanApplicationId, int companyId)
        {
            return GetLoanApplications(companyId).Where(c => c.loanApplicationId == loanApplicationId).ToList();
        }

        public LoanApplicationViewModel GetSingleLoanApplicationById(int loanApplicationId, int companyId)
        {
            return GetLoanApplications(companyId).Where(c => c.loanApplicationId == loanApplicationId).FirstOrDefault();
        }

        public dynamic GetLoanAppById(int loanApplicationDetailId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.LOANAPPLICATIONID == loanApplicationDetailId && a.TBL_COMPANY.COMPANYID == companyId && a.DELETED == false
                        select new
                        {
                            applicationAmount = a.APPLICATIONAMOUNT,
                            tenor = a.APPLICATIONTENOR,
                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerId = a.CUSTOMERID,
                            customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            applicationDate = a.APPLICATIONDATE,   //FinTrakBankingContext
                            applicationRef = a.APPLICATIONREFERENCENUMBER
                        }).FirstOrDefault();
            return data;
        }

        public IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailById(int loanApplicationDetailId, int companyId)
        {
            List<jobLoanApplicationDetailViewModel> data = new List<jobLoanApplicationDetailViewModel>();
            try
            {
                int? casaAcct = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_LOAN_APPLICATION.CASAACCOUNTID;
                TBL_CASA casa = new TBL_CASA();
                var productNumber = "";
                var productName = "";
                if (casaAcct != null)
                {
                    casa = context.TBL_CASA.Where(x => x.CASAACCOUNTID == casaAcct).FirstOrDefault();
                    productName = casa.PRODUCTACCOUNTNAME;
                    productNumber = casa.PRODUCTACCOUNTNUMBER;
                }
                else
                {
                    productName = "N/A";
                    productNumber = "N/A";
                }
                data = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        join c in context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                        where a.TBL_LOAN_APPLICATION.COMPANYID == companyId && a.DELETED == false
                        && a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                        select new jobLoanApplicationDetailViewModel
                        {
                            requireCollateral = a.TBL_LOAN_APPLICATION.REQUIRECOLLATERAL,
                            approvedAmount = a.APPROVEDAMOUNT,
                            approvedInterestRate = a.APPROVEDINTERESTRATE,
                            approvedProductId = a.APPROVEDPRODUCTID,
                            approvedTenor = a.APPROVEDTENOR,
                            currencyId = a.CURRENCYID,
                            currencyName = a.TBL_CURRENCY.CURRENCYNAME,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                            casaAccountId = a.TBL_LOAN_APPLICATION.CASAACCOUNTID,
                            accountNumber = productNumber,
                            customerAccount = productName,
                            misCode = a.TBL_LOAN_APPLICATION.MISCODE,
                            teamMisCode = a.TBL_LOAN_APPLICATION.TEAMMISCODE,
                            applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanTypeName = a.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,

                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerId = a.CUSTOMERID,
                            exchangeRate = a.EXCHANGERATE,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            subSectorId = a.SUBSECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "/" + a.TBL_SUB_SECTOR.NAME,
                            branchName = a.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHNAME,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            proposedAmount = a.PROPOSEDAMOUNT,
                            proposedInterestRate = a.PROPOSEDINTERESTRATE,
                            proposedProductId = a.PROPOSEDPRODUCTID,
                            proposedTenor = a.PROPOSEDTENOR, //Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                            statusId = a.STATUSID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = a.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = a.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            relationshipOfficerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPMANAGERID,
                            customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            invoiceDiscountDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_INV.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new LoanApplicationDetailInvoiceViewModel
                                                     {
                                                         approvalComment = i.APPROVAL_COMMENT,
                                                         contractNumber = i.CONTRACTNO,
                                                         contractEndDate = i.CONTRACT_ENDDATE,
                                                         contractStartDate = i.CONTRACT_STARTDATE,
                                                         purchaseOrderNumber = i.PURCHASEORDERNUMBER,
                                                         invoiceAmount = i.INVOICE_AMOUNT,
                                                         invoiceNo = i.INVOICENO,
                                                         invoiceDate = i.INVOICE_DATE,
                                                         invoiceId = i.INVOICEID,
                                                         invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                         approvaStatusId = i.APPROVALSTATUSID,
                                                         revalidated = i.REVALIDATED,
                                                         approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                         principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                         principalAccount = i.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                         principalRegNo = i.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                         principalId = i.PRINCIPALID,
                                                         reValidated = i.REVALIDATED,
                                                         entrySheetNumber = i.ENTRYSHEETNUMBER,
                                                         // purchaseOrderNumber = i.PURCHASEORDERNUMBER
                                                     }).ToList(),
                            firstEducationtDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_EDU.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new EducationLoanViewModel
                                                     {
                                                         educationId = i.EDUCATIONID,
                                                         loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                         numberOfStudent = i.NUMBER_OF_STUDENTS,
                                                         averageSchoolFees = i.AVERAGE_SCHOOL_FEES,
                                                         totalPreviousTermSchoolFees = i.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                                         productClassId = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                                         productClassName = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                                     }).ToList(),
                            firstTradderDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_TRA.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new TraderLoanViewModel
                                                  {
                                                      tradderId = i.TRADDERID,
                                                      marketId = i.MARKETID,
                                                      soldItems = i.SOLDITEMS,
                                                      marketName = i.TBL_LOAN_MARKET.MARKETNAME,
                                                      averageMonthlyTurnover = i.AVERAGE_MONTHLY_TURNOVER,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      //productClassId = i.
                                                  }).ToList(),
                            //    loanCollateral = (from i in context.TBL_LOAN_APPLICATION_COLLATRL2.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                            //                      select new CollateralViewModel
                            //                      {
                            //                          collateralId = i.COLLATERALBASICDETAILID, 
                            //                          collateralCustomerId = (int)i.TBL_LOAN_APPLICATION_DETAIL.CUSTOMERID,
                            //                          collateralDetail = i.COLLATERALDETAIL,
                            //                          collateralValue = i.COLLATERALVALUE,
                            //                          stampToCoverAmount = i.STAMPEDTOCOVERAMOUNT,
                            //                          customerName = i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME
                            //                         + " " + i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME,
                            //                      }).ToList(),
                            bondsAndGaurantees = (from i in context.TBL_LOAN_APPLICATION_DETL_BG.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new BondsAndGauranteeViewModel
                                                  {
                                                      bondId = i.BONDID,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      principalId = i.PRINCIPALID,
                                                      amount = i.AMOUNT,
                                                      currencyId = i.CURRENCYID,
                                                      contractStartDate = i.CONTRACT_STARTDATE,
                                                      contractEndDate = i.CONTRACT_ENDDATE,
                                                      isTenored = i.ISTENORED,
                                                      isBankFormat = i.ISBANKFORMAT,
                                                      approvalStatusId = i.APPROVALSTATUSID,
                                                      approvalComment = i.APPROVAL_COMMENT,
                                                      approvedBy = i.APPROVEDBY,
                                                      approvedDateTime = i.APPROVEDDATETIME,
                                                      principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                      invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                      approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                  }).ToList(),

                        }).ToList();
            }
            catch (Exception ex)
            {

            }

            foreach (var i in data)
            {
                var relationshipOfficer = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipOfficerId).FirstOrDefault();
                var relationshipManager = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipManagerId).FirstOrDefault();
                i.relationshipOfficerName = relationshipOfficer.FIRSTNAME + " " + relationshipOfficer.MIDDLENAME + " " + relationshipOfficer.LASTNAME;
                i.relationshipManagerName = relationshipManager.FIRSTNAME + " " + relationshipManager.MIDDLENAME + " " + relationshipManager.LASTNAME;

            }
            return data;
        }

        public IEnumerable<jobLoanApplicationDetailViewModel> GetLoanApplicationDetailByLoanApplicationId(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        where a.TBL_LOAN_APPLICATION.COMPANYID == companyId && a.DELETED == false
                        && a.LOANAPPLICATIONID == loanApplicationId
                        select new jobLoanApplicationDetailViewModel
                        {
                            requireCollateral = a.TBL_LOAN_APPLICATION.REQUIRECOLLATERAL,
                            approvedAmount = a.APPROVEDAMOUNT,
                            approvedInterestRate = a.APPROVEDINTERESTRATE,
                            approvedProductId = a.APPROVEDPRODUCTID,
                            approvedTenor = a.APPROVEDTENOR,
                            currencyId = a.CURRENCYID,
                            currencyName = a.TBL_CURRENCY.CURRENCYNAME,
                            currencyCode = a.TBL_CURRENCY.CURRENCYCODE,
                            casaAccountId = a.TBL_LOAN_APPLICATION.CASAACCOUNTID,
                            accountNumber = a.TBL_LOAN_APPLICATION.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            customerAccount = a.TBL_LOAN_APPLICATION.TBL_CASA.PRODUCTACCOUNTNAME,
                            misCode = a.TBL_LOAN_APPLICATION.MISCODE,
                            teamMisCode = a.TBL_LOAN_APPLICATION.TEAMMISCODE,
                            applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanTypeName = a.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,

                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            customerId = a.CUSTOMERID,
                            exchangeRate = a.EXCHANGERATE,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            subSectorId = a.SUBSECTORID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "/" + a.TBL_SUB_SECTOR.NAME,
                            branchName = a.TBL_LOAN_APPLICATION.TBL_BRANCH.BRANCHNAME,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            proposedAmount = a.PROPOSEDAMOUNT,
                            proposedInterestRate = a.PROPOSEDINTERESTRATE,
                            proposedProductId = a.PROPOSEDPRODUCTID,
                            proposedTenor = a.PROPOSEDTENOR, //Convert.ToInt32(Math.Round(Convert.ToDecimal(c.PROPOSEDTENOR) * Convert.ToDecimal(12 / 365))),
                            statusId = a.STATUSID,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            productClassId = a.TBL_PRODUCT.PRODUCTCLASSID,
                            productClassName = a.TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            relationshipOfficerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.TBL_LOAN_APPLICATION.RELATIONSHIPMANAGERID,
                            customerType = a.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            invoiceDiscountDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_INV.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new LoanApplicationDetailInvoiceViewModel
                                                     {
                                                         approvalComment = i.APPROVAL_COMMENT,
                                                         contractNumber = i.CONTRACTNO,
                                                         contractEndDate = i.CONTRACT_ENDDATE,
                                                         contractStartDate = i.CONTRACT_STARTDATE,
                                                         purchaseOrderNumber = i.PURCHASEORDERNUMBER,
                                                         invoiceAmount = i.INVOICE_AMOUNT,
                                                         invoiceNo = i.INVOICENO,
                                                         invoiceDate = i.INVOICE_DATE,
                                                         invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                         approvaStatusId = i.APPROVALSTATUSID,
                                                         approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                         principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                         principalAccount = i.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                         principalRegNo = i.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                         principalId = i.PRINCIPALID,
                                                         reValidated = i.REVALIDATED,
                                                         entrySheetNumber = i.ENTRYSHEETNUMBER,
                                                         // purchaseOrderNumber = i.PURCHASEORDERNUMBER
                                                     }).ToList(),
                            firstEducationtDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_EDU.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                     select new EducationLoanViewModel
                                                     {
                                                         educationId = i.EDUCATIONID,
                                                         loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                         numberOfStudent = i.NUMBER_OF_STUDENTS,
                                                         averageSchoolFees = i.AVERAGE_SCHOOL_FEES,
                                                         totalPreviousTermSchoolFees = i.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                                                         productClassId = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSID,
                                                         productClassName = context.TBL_PRODUCT_CLASS.Where(x => x.PRODUCTCLASSID == i.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTCLASSID).FirstOrDefault().PRODUCTCLASSNAME,
                                                     }).ToList(),
                            firstTradderDetail = (from i in context.TBL_LOAN_APPLICATION_DETL_TRA.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new TraderLoanViewModel
                                                  {
                                                      tradderId = i.TRADDERID,
                                                      marketId = i.MARKETID,
                                                      soldItems = i.SOLDITEMS,
                                                      marketName = i.TBL_LOAN_MARKET.MARKETNAME,
                                                      averageMonthlyTurnover = i.AVERAGE_MONTHLY_TURNOVER,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      //productClassId = i.
                                                  }).ToList(),
                            loanCollateral = (from i in context.TBL_LOAN_APPLICATION_COLLATRL2.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                              select new CollateralViewModel
                                              {
                                                  collateralId = i.COLLATERALBASICDETAILID, // COLLATERALCUSTOMERID,
                                                  collateralCustomerId = (int)i.TBL_LOAN_APPLICATION_DETAIL.CUSTOMERID,//.CUSTOMERID, //COLLATERALCUSTOMERID,
                                                                                                                       // allowSharing = i.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                                                  collateralDetail = i.COLLATERALDETAIL,
                                                  //collateralCode = i.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                                                  collateralValue = i.COLLATERALVALUE,

                                                  stampToCoverAmount = i.STAMPEDTOCOVERAMOUNT,
                                                  //collateralTypeName = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                                  //collateralTypeId = i.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                                                  //collateralSubTypeId = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.TBL_COLLATERAL_TYPE_SUB.
                                                  //currencyCode = i.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
                                                  //valuationCycle = i.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                                                  //haircut = i.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                                                  customerName = i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME
                                                 + " " + i.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME,
                                                  //collateralSearchAmount = context.TBL_STATE.Where(x=>x.STATEID == i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault().TBL_CITY.STATEID).FirstOrDefault().COLLATERALSEARCHCHARGEAMOUNT,
                                                  //chartingAmount = context.TBL_STATE.Where(x => x.STATEID == i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_IMMOVE_PROPERTY.LastOrDefault().TBL_CITY.STATEID).LastOrDefault().CHARTINGAMOUNT,
                                                  //verificationAmount = context.TBL_STATE.Where(x => x.STATEID == i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault().TBL_CITY.STATEID).FirstOrDefault().COLLATERALSEARCHCHARGEAMOUNT,
                                                  //cityId = i.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault().CITYID,
                                                  // legalFeeTaken = i.LEGAL_FEE_TAKEN,
                                              }).ToList(),
                            bondsAndGaurantees = (from i in context.TBL_LOAN_APPLICATION_DETL_BG.Where(x => x.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID)
                                                  select new BondsAndGauranteeViewModel
                                                  {
                                                      bondId = i.BONDID,
                                                      loanApplicationDetailId = i.LOANAPPLICATIONDETAILID,
                                                      principalId = i.PRINCIPALID,
                                                      amount = i.AMOUNT,
                                                      currencyId = i.CURRENCYID,
                                                      contractStartDate = i.CONTRACT_STARTDATE,
                                                      contractEndDate = i.CONTRACT_ENDDATE,
                                                      isTenored = i.ISTENORED,
                                                      isBankFormat = i.ISBANKFORMAT,
                                                      approvalStatusId = i.APPROVALSTATUSID,
                                                      approvalComment = i.APPROVAL_COMMENT,
                                                      approvedBy = i.APPROVEDBY,
                                                      approvedDateTime = i.APPROVEDDATETIME,
                                                      principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                      invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                      approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                  }).ToList(),

                        }).ToList();
            foreach (var i in data)
            {
                var relationshipOfficer = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipOfficerId).FirstOrDefault();
                var relationshipManager = context.TBL_STAFF.Where(s => s.STAFFID == i.relationshipManagerId).FirstOrDefault();
                i.relationshipOfficerName = relationshipOfficer.FIRSTNAME + " " + relationshipOfficer.MIDDLENAME + " " + relationshipOfficer.LASTNAME;
                i.relationshipManagerName = relationshipManager.FIRSTNAME + " " + relationshipManager.MIDDLENAME + " " + relationshipManager.LASTNAME;

            }
            return data;
        }

        public bool SendApplicationToEdit(int loanApplicationId, int operationId, int accountOfficerId)
        {
            var staffs = genSetup.GetStaffRlieved(accountOfficerId);

            var loan = context.TBL_LOAN_APPLICATION.Find(loanApplicationId);
            if (loan == null)
            {
                throw new SecureException("This Loan doesn't exist on the System");
            }

            if (!(staffs.Contains(loan.OWNEDBY)))
            {
                throw new SecureException("You cannot modify a Loan you didn't initiate!");
            }

            var trail = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == loanApplicationId && t.OPERATIONID == operationId).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault();
            if (trail != null)
            {
                trail.APPROVALSTATEID = (int)ApprovalState.Ended;
                trail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;
                trail.VOTE = (int)ApprovalStatusEnum.Referred;
                trail.COMMENT += " Sent to Applications to edit Application with loan Reference Id: " + loan.APPLICATIONREFERENCENUMBER;
                trail.SYSTEMRESPONSEDATETIME = DateTime.Now;
                trail.RESPONSEDATE = DateTime.Now;
                trail.RESPONSESTAFFID = accountOfficerId;
                trail.REFEREBACKSTATEID = (int)ApprovalState.Ended;
            }
            var refers = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == loanApplicationId && t.OPERATIONID == operationId && t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred);
            foreach (var refer in refers)
            {
                refer.REFEREBACKSTATEID = (int)ApprovalState.Ended;
            }
            ArchiveLoanApplication(loanApplicationId, (int)OperationsEnum.LoanApplication, 0, accountOfficerId);
            loan.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress;
            loan.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
            var details = loan.TBL_LOAN_APPLICATION_DETAIL.ToList();
            foreach(var d in details)
            {
                d.STATUSID = (short)ApprovalStatusEnum.Processing;
            }
            return context.SaveChanges() > 0;
        }

        public IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId)
        {
            //var staffs = genSetup.GetStaffRlieved(relationshipOfficerId);// for further work

            var data = from a in context.TBL_LOAN_APPLICATION
                       where a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationInProgress
                       && a.COMPANYID == companyId && a.DELETED == false
                          && (a.OWNEDBY == relationshipOfficerId || a.RELATIONSHIPOFFICERID == relationshipOfficerId)
                          && a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.ApplicationInProgress
                          && a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending

                       orderby a.APPLICATIONDATE descending

                       select new
                       {
                           requireCollateral = a.REQUIRECOLLATERAL,
                           approvalStatusId = a.APPROVALSTATUSID,
                           loanApplicationId = a.LOANAPPLICATIONID,
                           applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                           loanAmount = a.APPLICATIONAMOUNT,
                           productClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                           customerId = a.CUSTOMERID ?? 0,
                           customerTypeId = a.TBL_CUSTOMER.CUSTOMERTYPEID,
                           customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                           customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                           companyId = a.COMPANYID,
                           branchId = (short)a.BRANCHID,
                           branchName = a.TBL_BRANCH.BRANCHNAME,
                           relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                           relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                           relationshipManagerId = a.RELATIONSHIPMANAGERID,
                           relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.LASTNAME,
                           misCode = a.MISCODE,
                           teamMisCode = a.TEAMMISCODE,
                           interestRate = a.INTERESTRATE,
                           isRelatedParty = a.ISRELATEDPARTY,
                           isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                           submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                           customerGroupId = a.CUSTOMERGROUPID ?? 0,
                           customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                           loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                           loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                           createdBy = a.OWNEDBY,
                           applicationDate = a.APPLICATIONDATE,
                           dateTimeCreated = a.DATETIMECREATED,
                           applicationTenor = Math.Round((double)a.APPLICATIONTENOR) * (12.0 / 365.0),
                           applicationAmount = a.APPLICATIONAMOUNT,
                           regionId = a.CAPREGIONID,
                           requireCollateralTypeId = a.REQUIRECOLLATERALTYPEID,
                           preliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                           collateralDetail = a.COLLATERALDETAIL,
                           loanInformation = a.LOANINFORMATION,
                           productClassId = a.PRODUCTCLASSID,
                           loantermSheetId = a.LOANTERMSHEETID,
                           ownershipStructure = a.OWNERSHIPSTRUCTURE,
                           loansWithOthers = a.LOANSWITHOTHERS,
                           isAdHocApplication = a.ISADHOCAPPLICATION,
                           approvedLimitId = a.LOANAPPROVEDLIMITID,
                           isEmployerRelated = a.ISEMPLOYERRELATED,
                           relatedEmployerId = a.RELATEDEMPLOYERID
                       };
            return data.ToList();
        }

        public async Task<bool> UpdateApprovalStatus(ApprovalViewModel entity)
        {
            var data = this.context.TBL_LOAN_APPLICATION.Find(entity.targetId);
            {
                //data.LoanStatusId = (short)entity.approvalStatusId;
                //data.ActedOnaBy = entity.staffId;
                //data.DateActedOn = genSetup.GetApplicaionDate();
                data.LOANAPPLICATIONID = (short)entity.approvalStatusId;
            }

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalStatusUpdated,
                STAFFID = entity.staffId,
                BRANCHID = (short)entity.BranchId,
                DETAIL =
                    $"Change Loan Application Status with reference number '{data.APPLICATIONREFERENCENUMBER}' to {GetLoanStatus((short)entity.approvalStatusId)}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.targetId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return await context.SaveChangesAsync() != 0;
        }

        public IEnumerable<ProductClassViewModel> GetProductClass()
        {
            return (from data in context.TBL_PRODUCT_CLASS
                    select new ProductClassViewModel()
                    {
                        productClassId = data.PRODUCTCLASSID,
                        productClassName = data.PRODUCTCLASSNAME,
                        productClassTypeId = data.PRODUCTCLASSTYPEID,
                        productTypeName = data.PRODUCTCLASSNAME,
                        productTypeId = data.PRODUCTCLASSID
                    });
        }

        public IEnumerable<LoanApplicationViewModel> FindLoanApplication(string referenceNumberOrName, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        where a.COMPANYID == companyId && a.DELETED == false
                        //&& (a.ApplicationReferenceNumber == referenceNumberOrName || $"{a.tbl_Customer.FirstName} {a.tbl_Customer.MiddleName} {a.tbl_Customer.LastName} {a.tbl_Customer.CustomerCode} ".Contains(referenceNumberOrName))
                        select new LoanApplicationViewModel
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = a.CUSTOMERID.Value,
                            loanInformation = a.LOANINFORMATION,
                            companyId = a.COMPANYID,
                            branchId = (short)a.BRANCHID,
                            //tenor = a.ApplicationTenor,
                            // tenorModeId = a.TenorModeId,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,

                            misCode = a.MISCODE,
                            //productId = (short)a.ProductId,
                            teamMisCode = a.TEAMMISCODE,

                            interestRate = a.INTERESTRATE,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                            //principalAmount = a.ApplicationAmount,
                            customerGroupId = a.CUSTOMERGROUPID.Value,
                            loanTypeId = a.LOANAPPLICATIONTYPEID,
                            //loanStatusId = a.LoanStatusId,
                            createdBy = a.OWNEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            dateTimeCreated = a.DATETIMECREATED
                        }).ToList();
            return data;
        }

        private string GetLoanStatus(short loanStatusId)
        {
            return this.context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == loanStatusId).SingleOrDefault()
                .ACCOUNTSTATUS;
        }

        public IEnumerable<CustomerViewModels> GetCustomerByApplicationId(int applicationId, string processtype)
        {
            List<CustomerViewModels> customers = new List<CustomerViewModels>();
            if (processtype == "LOS")
            {
                customers = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                             join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                             where a.LOANAPPLICATIONID == applicationId
                             select new CustomerViewModels
                             {
                                 customerId = a.CUSTOMERID,
                                 fullName = b.FIRSTNAME + " " + b.LASTNAME + "-" + b.CUSTOMERCODE,
                                 customerCode = b.CUSTOMERCODE
                             }).Distinct().ToList();

            }
            else if (processtype == "LMS")
            {
                customers = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                             join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                             where a.LOANAPPLICATIONID == applicationId
                             select new CustomerViewModels
                             {
                                 customerId = a.CUSTOMERID,
                                 fullName = b.FIRSTNAME + " " + b.LASTNAME + "-" + b.CUSTOMERCODE
                             }).Distinct().ToList();

            }
            return customers;
        }


        public List<accountsViewModels> GetCustomerTransactionsAccounts(int customerId, int applicationId, bool isLms = false)
        {
            var first = (from a in context.TBL_LOAN_APPLICATION_TRANS
                         where a.CUSTOMERID == customerId
                         select new accountsViewModels
                         {
                             accountNumber = a.ACCOUNTNUMBER,
                         }).Distinct().ToList();

            var second = (from a in context.TBL_LOAN_APPLICATION_TRANS2
                          where a.CUSTOMERID == customerId
                          select new accountsViewModels
                          {
                              accountNumber = a.ACCOUNTNUMBER,
                          }).Distinct().ToList();
            first = first.Union(second).ToList();

            return first;
        }

        public CustomerApplicationTransactionsViewModels GetCustomerTransactions(int customerId, int applicationId, bool isLms = false)
        {
            var fields = new CustomerApplicationTransactionsViewModels();

            var first = (from a in context.TBL_LOAN_APPLICATION_TRANS
                         where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.ISLMS == isLms
                         select new CustomerTransactionsViewModels
                         {
                             cust_Id = a.CUSTOMERTRANSACTIONID.ToString(),
                             period = a.PERIOD,
                             productName = a.PRODUCTNAME,
                             accountNumber = a.ACCOUNTNUMBER,
                             max_Credit_Balance = a.MAXIMUMCREDITBALANCE,
                             max_Debit_Balance = a.MAXIMUMDEBITBALANCE,
                             min_Credit_Balance = a.MINIMUMCREDITBALANCE,
                             min_Debit_Balance = a.MINIMUMDEBITBALANCE,
                             credit_Turnover = a.CREDITTURNOVER,
                             debit_Turnover = a.DEBITTURNOVER,
                             month = a.MONTH,
                             year = a.YEAR,
                             productAccountName = context.TBL_CASA.Where(o => o.CUSTOMERID == customerId).Select(o => o.PRODUCTACCOUNTNAME).FirstOrDefault(),

                         }).GroupBy(O => new { O.accountNumber, O.credit_Turnover, O.debit_Turnover, O.min_Credit_Balance, O.min_Debit_Balance }).Select(O => O.FirstOrDefault()).OrderByDescending(m => m.year).ThenByDescending(b => b.month).ToList();


            var second = (from a in context.TBL_LOAN_APPLICATION_TRANS2
                          where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.ISLMS == isLms
                          select new CustomerTransactionsViewModels
                          {
                              cust_Id = a.CUSTOMERTRANSACTIONID2.ToString(),
                              period = a.PERIOD,
                              productName = a.PRODUCTNAME,
                              accountNumber = a.ACCOUNTNUMBER,
                              interest = a.INTEREST,
                              float_Charge = a.FLOATCHARGE,
                              month = a.MONTH,
                              year = a.YEAR,
                              productAccountName = context.TBL_CASA.Where(o => o.CUSTOMERID == customerId).Select(o => o.PRODUCTACCOUNTNAME).FirstOrDefault(),
                          }).OrderByDescending(m => m.year).ThenByDescending(b => b.month).ToList();

            first.Add(new CustomerTransactionsViewModels
            {
                cust_Id = "TOTAL",
                max_Credit_Balance = first.Sum(t => t.max_Credit_Balance),
                max_Debit_Balance = first.Sum(t => t.max_Debit_Balance),
                min_Credit_Balance = first.Sum(t => t.min_Credit_Balance),
                min_Debit_Balance = first.Sum(t => t.min_Debit_Balance),
                credit_Turnover = first.Sum(t => t.credit_Turnover),
                debit_Turnover = first.Sum(t => t.debit_Turnover),
            });

            second.Add(new CustomerTransactionsViewModels
            {
                cust_Id = "TOTAL",
                interest = second.Sum(t => t.interest),
                float_Charge = second.Sum(t => t.float_Charge),
            });


            fields.firstTransaction = first;
            fields.secondTransaction = second;

            return fields;
        }

        public CustomerApplicationTransactionsViewModels GetCustomerTransactionsFiltered(int customerId, int applicationId, string accountnumber, int? fromYear, int fromMonth, int? toYear, int toMonth, bool isLms = false)
        {
            DateTime filterFromdate = new DateTime(); // = DateTime.Parse("1-" + fromMonth + "-" + fromYear);
            DateTime filterTodate = new DateTime();// = DateTime.Parse("1-" + toMonth + "-" + toYear);

            bool dateSelected = false;

            if (fromYear.HasValue)
            {
                dateSelected = true;

                try
                {
                    filterFromdate = new DateTime(fromYear.Value, fromMonth, 1);
                    filterTodate = new DateTime(toYear.Value, toMonth, 1);
                }
                catch (Exception)
                {
                    throw new ConditionNotMetException("Specify all date values to filter by date");
                }
            }

            var fields = new CustomerApplicationTransactionsViewModels();
            var first = new List<CustomerTransactionsViewModels>();
            var second = new List<CustomerTransactionsViewModels>();
            var firstRecord = new List<CustomerTransactionsViewModels>();
            var secondRecord = new List<CustomerTransactionsViewModels>();
            firstRecord = (from a in context.TBL_LOAN_APPLICATION_TRANS
                           where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.ISLMS == isLms
                           //&& a.ACCOUNTNUMBER == accountnumber
                           //&& (a.YEAR >= fromYear && a.MONTH >= fromMonth)
                           //&& (a.YEAR <= toYear && a.MONTH <= toMonth)
                           select new CustomerTransactionsViewModels
                           {
                               cust_Id = a.CUSTOMERTRANSACTIONID.ToString(),
                               period = a.PERIOD,
                               //periodDate  = DateTime.ParseExact("1-"+a.PERIOD,"dd-MM-yyyy",CultureInfo.InvariantCulture),
                               productName = a.PRODUCTNAME,
                               accountNumber = a.ACCOUNTNUMBER,
                               casaAccountId = context.TBL_CASA.Where(p => p.PRODUCTACCOUNTNUMBER == a.ACCOUNTNUMBER).Any() ? context.TBL_CASA.Where(p => p.PRODUCTACCOUNTNUMBER == a.ACCOUNTNUMBER).Select(o => o.CASAACCOUNTID).FirstOrDefault() : 0,
                               max_Credit_Balance = a.MAXIMUMCREDITBALANCE,
                               max_Debit_Balance = a.MAXIMUMDEBITBALANCE,
                               min_Credit_Balance = a.MINIMUMCREDITBALANCE,
                               min_Debit_Balance = a.MINIMUMDEBITBALANCE,
                               credit_Turnover = a.CREDITTURNOVER,
                               debit_Turnover = a.DEBITTURNOVER,
                               month = a.MONTH,
                               year = a.YEAR,
                           }).OrderBy(p => p.accountNumber).OrderBy(l => l.year).OrderBy(q => q.month).ToList();

            foreach (var rec in firstRecord)
            {
                rec.periodDate = new DateTime(rec.year.Value, rec.month.Value, 1); // DateTime.Parse(rec.period);// "1-" +a.PERIOD,
            }


            secondRecord = (from a in context.TBL_LOAN_APPLICATION_TRANS2
                            where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.ISLMS == isLms
                            //&& a.ACCOUNTNUMBER == accountnumber
                            //&& (a.YEAR >= fromYear && a.MONTH >= fromMonth)
                            //&& (a.YEAR <= toYear && a.MONTH <= toMonth)
                            select new CustomerTransactionsViewModels
                            {
                                // periodDate = DateTime.ParseExact("1-" + a.PERIOD, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                                cust_Id = a.CUSTOMERTRANSACTIONID2.ToString(),
                                period = a.PERIOD,
                                productName = a.PRODUCTNAME,
                                accountNumber = a.ACCOUNTNUMBER,
                                interest = a.INTEREST,
                                float_Charge = a.FLOATCHARGE,
                                month = a.MONTH,
                                year = a.YEAR,
                            }).OrderBy(p => p.accountNumber).OrderBy(l => l.year).OrderBy(q => q.month).ToList();
            foreach (var rec in secondRecord)
            {
                rec.periodDate = new DateTime(rec.year.Value, rec.month.Value, 1); //DateTime.Parse(rec.period);// "1-" +a.PERIOD,
            }

            first = (from a in firstRecord
                     where a.accountNumber == (accountnumber == "0" ? a.accountNumber : accountnumber)
                     && (a.periodDate >= (dateSelected == false ? a.periodDate : filterFromdate) && a.periodDate <= (dateSelected == false ? a.periodDate : filterTodate))

                     select new CustomerTransactionsViewModels
                     {
                         cust_Id = a.cust_Id,
                         period = a.period,
                         productName = a.productName,
                         accountNumber = a.accountNumber,
                         casaAccountId = a.casaAccountId,
                         max_Credit_Balance = a.max_Credit_Balance,
                         max_Debit_Balance = a.max_Debit_Balance,
                         min_Credit_Balance = a.min_Credit_Balance,
                         min_Debit_Balance = a.min_Debit_Balance,
                         credit_Turnover = a.credit_Turnover,
                         debit_Turnover = a.debit_Turnover,
                         month = a.month,
                         year = a.year,
                     }).ToList();

            second = (from a in secondRecord
                      where a.accountNumber == (accountnumber == "0" ? a.accountNumber : accountnumber)
                     && (a.periodDate >= (dateSelected == false ? a.periodDate : filterFromdate) && a.periodDate <= (dateSelected == false ? a.periodDate : filterTodate))

                      select new CustomerTransactionsViewModels
                      {
                          cust_Id = a.cust_Id,
                          period = a.period,
                          productName = a.productName,
                          accountNumber = a.accountNumber,
                          interest = a.interest,
                          float_Charge = a.float_Charge,
                          month = a.month,
                          year = a.year,
                      }).ToList();





            //foreach (var rec in second)
            //{
            //    rec.periodDate = DateTime.Parse("1-" + rec.period);// "1-" +a.PERIOD,
            //}
            //second = second.Where(op => op.periodDate >= filterFromdate && op.periodDate >= filterTodate).ToList();


            first.Add(new CustomerTransactionsViewModels
            {
                accountNumber = "TOTAL",
                //max_Credit_Balance = first.Sum(t => t.max_Credit_Balance),
                //max_Debit_Balance = first.Sum(t => t.max_Debit_Balance),
                //min_Credit_Balance = first.Sum(t => t.min_Credit_Balance),
                //min_Debit_Balance = first.Sum(t => t.min_Debit_Balance),
                credit_Turnover = first.Sum(t => t.credit_Turnover),
                debit_Turnover = first.Sum(t => t.debit_Turnover),
            });
            second.Add(new CustomerTransactionsViewModels
            {
                accountNumber = "TOTAL",
                interest = second.Sum(t => t.interest),
                float_Charge = second.Sum(t => t.float_Charge),
            });


            fields.firstTransaction = first;
            fields.secondTransaction = second;

            return fields;
        }

        public CustomerApplicationTransactionsViewModels GetCustomerTransactionsByFilterLogic(int customerId, int applicationId, int froma, int to, int fYear, int tYear, bool isLms = false)
        {
            var fields = new CustomerApplicationTransactionsViewModels();

            var first = (from a in context.TBL_LOAN_APPLICATION_TRANS
                         where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.MONTH >= froma && a.MONTH <= to && a.YEAR >= fYear && a.YEAR <= tYear && a.ISLMS == isLms
                         select new CustomerTransactionsViewModels
                         {
                             cust_Id = a.CUSTOMERTRANSACTIONID.ToString(),
                             period = a.PERIOD,
                             productName = a.PRODUCTNAME,
                             accountNumber = a.ACCOUNTNUMBER,
                             max_Credit_Balance = a.MAXIMUMCREDITBALANCE,
                             max_Debit_Balance = a.MAXIMUMDEBITBALANCE,
                             min_Credit_Balance = a.MINIMUMCREDITBALANCE,
                             min_Debit_Balance = a.MINIMUMDEBITBALANCE,
                             credit_Turnover = a.CREDITTURNOVER,
                             debit_Turnover = a.DEBITTURNOVER,
                             month = a.MONTH,
                             year = a.YEAR,
                             productAccountName = context.TBL_CASA.Where(o => o.CUSTOMERID == customerId).Select(o => o.PRODUCTACCOUNTNAME).FirstOrDefault(),
                         }).GroupBy(O => new { O.accountNumber, O.credit_Turnover, O.debit_Turnover, O.min_Credit_Balance, O.min_Debit_Balance }).Select(O => O.FirstOrDefault()).OrderByDescending(m => m.year).ThenByDescending(b => b.month).ToList();


            var second = (from a in context.TBL_LOAN_APPLICATION_TRANS2
                          where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.MONTH >= froma && a.MONTH <= to && a.YEAR >= fYear && a.YEAR <= tYear && a.ISLMS == isLms
                          select new CustomerTransactionsViewModels
                          {
                              cust_Id = a.CUSTOMERTRANSACTIONID2.ToString(),
                              period = a.PERIOD,
                              productName = a.PRODUCTNAME,
                              accountNumber = a.ACCOUNTNUMBER,
                              interest = a.INTEREST,
                              float_Charge = a.FLOATCHARGE,
                              month = a.MONTH,
                              year = a.YEAR,
                              productAccountName = context.TBL_CASA.Where(o => o.CUSTOMERID == customerId).Select(o => o.PRODUCTACCOUNTNAME).FirstOrDefault(),
                          }).OrderByDescending(m => m.year).ThenByDescending(b => b.month).ToList(); ;

            first.Add(new CustomerTransactionsViewModels
            {
                cust_Id = "TOTAL",
                max_Credit_Balance = first.Sum(t => t.max_Credit_Balance),
                max_Debit_Balance = first.Sum(t => t.max_Debit_Balance),
                min_Credit_Balance = first.Sum(t => t.min_Credit_Balance),
                min_Debit_Balance = first.Sum(t => t.min_Debit_Balance),
                credit_Turnover = first.Sum(t => t.credit_Turnover),
                debit_Turnover = first.Sum(t => t.debit_Turnover),
            });
            second.Add(new CustomerTransactionsViewModels
            {
                cust_Id = "TOTAL",
                interest = second.Sum(t => t.interest),
                float_Charge = second.Sum(t => t.float_Charge),
            });


            fields.firstTransaction = first;
            fields.secondTransaction = second;

            return fields;
        }


        public List<RatingAndRatioViewModel> GetCustomerRatios(int customerId, int applicationId, bool isLms = false)
        {

            //var aa = context.TBL_CUSTOMER_RATIOS
            //    .Select(o=> new RatingAndRatioViewModel {
            //        description = o.DESCRIPTION,
            //        value = o.VALUE,
            //    }).ToList();

            var fields = (from a in context.TBL_CUSTOMER_RATIOS
                          where a.CUSTOMERID == customerId && a.DELETED == false && a.LOANAPPLICATIONID == applicationId
                          select new RatingAndRatioViewModel
                          {
                              description = a.DESCRIPTION,
                              value = a.VALUE,

                          }).ToList();

            return fields;
        }

        // PLEASE RENAME THIS METHOD NAME TO BE MORE DESCRIPTIVE like LoanApplicationChecklistValidation
        public LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId, int staffId)//, object entity)
        {
            int targetId = 0;
            string str = string.Empty;
            bool jumpToDrawdown = false;
            bool isCheckListDone = true;
            int checkListIndex = (int)ChecklistErrorEnum.GoodChecklist;
            LoanApplicationUpdateMessage result = new LoanApplicationUpdateMessage();

            var application = context.TBL_LOAN_APPLICATION.Find(applicationId);
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);
            bool isCamsolJobRequestSent = false;
            bool isCollateralSearchJobRequestSent = false;

            try
            {
                if (loanApplicationDetails.Any())
                {
                    var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanApplication).ToList();

                    foreach (var detail in loanApplicationDetails)
                    {

                        /* Camsol Search Job Request */
                        var camsolJobRequestsSub = (from r in context.TBL_JOB_REQUEST
                                                    join j in context.TBL_JOB_TYPE on r.JOBTYPEID equals j.JOBTYPEID
                                                    where r.OPERATIONSID == (short)OperationsEnum.LoanApplication && r.TARGETID == detail.LOANAPPLICATIONDETAILID && j.JOBTYPEID == (short)JobTypeEnum.camsolCheck
                                                    select r);

                        var camsolJobRequests = camsolJobRequestsSub.ToList();

                        if (camsolJobRequests.Count > 0)
                            isCamsolJobRequestSent = true;

                        /* Collateral Search Job Request */
                        if (application.REQUIRECOLLATERALTYPEID == (int)RequireCollateralTypeEnum.ImmovablePropertyCollateral)
                        {
                            var legalRequestSub = context.TBL_JOB_REQUEST
                                .Where(x => x.TARGETID == detail.LOANAPPLICATIONDETAILID
                                && x.OPERATIONSID == (short)OperationsEnum.LoanApplication
                                && x.JOBTYPEID == (short)JobTypeEnum.legal
                                && x.JOB_SUB_TYPEID == (short)JobSubTypeEnum.CollateralRelated
                            );

                            var legalRequests = legalRequestSub.ToList();

                            if (legalRequests.Count() > 0)
                                isCollateralSearchJobRequestSent = true; //errorMessage = errorMessage + "Job Request to Legal for immovable property collateral is required! ";
                        }

                        /* Middle office Job Request for IDF */
                        var product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == detail.APPROVEDPRODUCTID);
                        //if (product.PRODUCTCLASSID == (short)ProductClassEnum.InvoiceDiscountingFacility)
                        //{
                        //    var middleOfficeRequests = (from r in context.TBL_JOB_REQUEST
                        //                                join j in context.TBL_JOB_TYPE on r.JOBTYPEID equals j.JOBTYPEID
                        //                                where r.OPERATIONSID == (short)OperationsEnum.LoanApplication && r.TARGETID == detail.LOANAPPLICATIONDETAILID && j.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification
                        //                                select r).ToList();

                        //    if (middleOfficeRequests.Count <= 0)
                        //        throw new ConditionNotMetException($"Job Request to relationship team for product '{product.PRODUCTNAME.ToLower()}' is required!");
                        //}

                        var checklistTypes = (from a in context.TBL_CHECKLIST_TYPE select a).ToList();
                        foreach (var checklistType in checklistTypes) // through checklist types
                        {
                            if (checklistType.ISPRODUCT_BASED)
                                targetId = detail.LOANAPPLICATIONDETAILID;
                            else
                                targetId = applicationId;

                            var checklistDetails = from a in context.TBL_CHECKLIST_DEFINITION
                                                   join b in context.TBL_CHECKLIST_DETAIL on a.CHECKLISTDEFINITIONID
                                                   equals b.CHECKLISTDEFINITIONID
                                                   where b.TARGETID == targetId
                                                   && b.TARGETTYPEID == (checklistType.ISPRODUCT_BASED ? (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist : (short)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist)
                                                   && a.CHECKLIST_TYPEID == checklistType.CHECKLIST_TYPEID && a.OPERATIONID == (int)OperationsEnum.LoanApplication
                                                   && b.CHECKLISTSTATUSID != null && b.DATETIMEUPDATED != null
                                                   && ids.Contains((int)a.APPROVALLEVELID)
                                                   select b;

                            var productId = checklistType.ISPRODUCT_BASED ? (short?)detail.APPROVEDPRODUCTID : null;

                            var checklistDefinitions = (from a in context.TBL_CHECKLIST_DEFINITION
                                                        join b in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals b.CHECKLISTITEMID
                                                        where ids.Contains((int)a.APPROVALLEVELID) && a.CHECKLIST_TYPEID == checklistType.CHECKLIST_TYPEID
                                                        && a.OPERATIONID == (int)OperationsEnum.LoanApplication && a.PRODUCTID == productId
                                                        select a).AsQueryable();

                            var definitionsCount = checklistDefinitions.Count();
                            var detailsCount = checklistDetails.Count();


                            if (checklistType.ISPRODUCT_BASED)
                            {
                                var count1 = checklistDefinitions.Count();
                                var count2 = checklistDetails.Count();

                                if (checklistType.CHECKLIST_TYPEID == (short)CheckListTypeEnum.RegulatoryChecklist)
                                    continue;

                                if (checklistDefinitions.Count() != checklistDetails.Count()) // checking for completion
                                {
                                    isCheckListDone = false;
                                    str = str + Environment.NewLine + checklistType.CHECKLIST_TYPE_NAME + " " + " is not complete";
                                    checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
                                }
                            }
                            else
                            {
                                var customerCount = (from a in loanApplicationDetails select a.CUSTOMERID).Distinct().Count();

                                var validationCount = definitionsCount * customerCount;

                                if (detailsCount != validationCount)
                                {
                                    if (checklistType.CHECKLIST_TYPEID == (short)CheckListTypeEnum.RegulatoryChecklist)
                                        continue;

                                    isCheckListDone = false;
                                    str = str + Environment.NewLine + checklistType.CHECKLIST_TYPE_NAME + " " + " is not complete";
                                    checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
                                }
                            }

                            var negativeChecklistDetails = checklistDetails.Where(c => c.CHECKLISTSTATUSID == (int)CheckListStatusEnum.No);
                            if (negativeChecklistDetails.Any())
                            {
                                isCheckListDone = false;
                                checkListIndex = (int)ChecklistErrorEnum.NegetiveChecklist;
                                str = str + "One or more item(s) did not meet up with the condition." + Environment.NewLine
                                            + " Please check your response to confirm." + Environment.NewLine;
                            }

                        } // foreach checklistTypes

                        // var rmSuggestion = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                        //                     where a.LOANAPPLICATIONDETAILID == detail.LOANAPPLICATIONDETAILID && (detail.CONDITIONPRECIDENT == null
                        //                     || detail.CONDITIONSUBSEQUENT == null || a.TRANSACTIONDYNAMICS == null)
                        //                     select a);

                        // if (rmSuggestion.Any())
                        // {
                        //     isCheckListDone = false;
                        //     str = str + " Kindly Complete The RM Suggestions Record" + Environment.NewLine;
                        //     checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
                        // }

                        if (isCheckListDone == false) break;

                    } // foreach loanApplicationDetails

                } // loanApplicationDetails.Any()

                //ValidateCollateralSearchJobRequests(applicationId, application.REQUIRECOLLATERALTYPEID, true);

                // if (!isCamsolJobRequestSent) throw new ConditionNotMetException("Job Request must be sent to CAMSOL before you can proceed.");

                if (application.REQUIRECOLLATERALTYPEID == (int)RequireCollateralTypeEnum.ImmovablePropertyCollateral)
                {
                    // if (isCollateralSearchJobRequestSent == false) throw new ConditionNotMetException("Job Request to Legal of type Collateral Related is required!");

                    //if (requests.Count() > 0) isCollateralSearchJobRequestSent = true; //errorMessage = errorMessage + "Job Request to Legal for immovable property collateral is required! ";
                }

                if (PushApplicationToDrawdown(application.APPLICATIONREFERENCENUMBER)) { jumpToDrawdown = true; }

                if (isCheckListDone && SubmitLoanApplicationForCam(applicationId, staffId, checkListIndex) == 1)
                {
                    var casa = context.TBL_CASA.Find(application.CASAACCOUNTID);
                    var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();

                    if (setup.USE_THIRD_PARTY_INTEGRATION)
                    {
                        //creditCommon.LoadCustomerRatios(
                        //     applicationId,
                        //     loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(),
                        //     staffId
                        //);

                        //creditCommon.LoadCustomerGroupRatios(
                        //     applicationId,
                        //     loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(),
                        //     staffId
                        //);

                        //creditCommon.GetCorporateCustomerRating(
                        //     applicationId,
                        //     loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(),
                        //     staffId
                        //);

                        //AddFacilityRating(loanApplicationDetails.ToList(), staffId);

                    }

                    return new LoanApplicationUpdateMessage
                    {
                        isdone = isCheckListDone, // true
                        messageStr = str,
                        checkListIndex = (int)ChecklistErrorEnum.GoodChecklist, // okay
                        jumpToDrawdown = jumpToDrawdown
                    };
                }

            }
            catch (Exception ex)
            {
                throw new SecureException(ex.ToString());
            }

            return new LoanApplicationUpdateMessage
            {
                isdone = isCheckListDone,
                messageStr = str,
                checkListIndex = checkListIndex,
                jumpToDrawdown = jumpToDrawdown
            };

        }

        public void LoadCustomerTurnover(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);

            if (loanApplicationDetails != null) {
                creditCommon.LoadCustomerTurnover(applicationId, loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(), staffId);
            }
        }

        public void LoadCustomerTurnoverLms(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LMSR_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId).ToList();

            if (loanApplicationDetails != null)
            {
                creditCommon.LoadCustomerTurnover(applicationId, loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(), staffId);
            }
        }

        public void GetCustomerRatiosFromBasel(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);

            if (loanApplicationDetails != null)
            {
                creditCommon.LoadCustomerRatios(applicationId, loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(), staffId);
            }
        }

        public void GetCustomerGroupRatiosFromBasel(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);

            if (loanApplicationDetails != null)
            {
                creditCommon.LoadCustomerGroupRatios(applicationId, loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(), staffId);
            }
        }

        public void GetCorporateCustomerRatingFromBasel(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);

            if (loanApplicationDetails != null)
            {
                creditCommon.GetCorporateCustomerRating(applicationId, loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(), staffId);
            }
        }

        public void GetFacilityRatingFromBasel(int applicationId, int staffId)
        {
            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);

            foreach (var item in loanApplicationDetails) {
                creditCommon.GetAutoLoansRetail(item.LOANAPPLICATIONDETAILID, item.CUSTOMERID, staffId);
                creditCommon.GetPersonalLoansRetail(item.LOANAPPLICATIONDETAILID, item.CUSTOMERID, staffId);
                creditCommon.GetCreditCardsRetail(item.LOANAPPLICATIONDETAILID, item.CUSTOMERID, staffId);
            }
        }

        public List<FacilityRatingViewModel> GetFacilityRating(int loanApplicationDetailId)
        {
            var facilityRating = context.TBL_FACILITY_RATING.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetailId)
                                        .OrderByDescending(O => O.FACILITYRATINGID).Select(O => new FacilityRatingViewModel
                                        {
                                            loanApplicationDetailId = O.LOANAPPLICATIONDETAILID,
                                            probability_of_Default = O.PROBABILITYOFDEFAULT,
                                            remark = O.REMARK,
                                            customer_ID = O.CUSTOMERCODE,
                                            dateTimeCreated = O.DATETIMECREATED,
                                            createdBy = O.CREATEDBY
                                        }).ToList();
            return facilityRating;
        }

        public short SubmitLoanApplicationForCam(int applicationId, int staffId, int checkListIndex)
        {

            /* 0 = failed 
            * 1 = successfully move to appraisal
            * 2 = successfuly moved to drawdown **/

            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);
            appl.TOTALEXPOSUREAMOUNT = 0;
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == applicationId).FirstOrDefault();

            if (appl.LOANAPPROVEDLIMITID > 0)
            {
                appl.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.BookingRequestInitiated;
                workflow.NextProcess(appl.COMPANYID, appl.OWNEDBY, (int)OperationsEnum.IndividualDrawdownRequest, appl.FLOWCHANGEID, appl.LOANAPPLICATIONID, null, "New approved application", true, false, false, false, null);
                context.SaveChanges();
                return 1;
            }

            if (appl.PRODUCT_CLASS_PROCESSID == (int)ProductClassProcessEnum.ProductBased && checkListIndex == (int)ChecklistErrorEnum.NegetiveChecklist)
            {
                appl.PRODUCT_CLASS_PROCESSID = (int)ProductClassProcessEnum.CAMBased;
            }

            appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ChecklistCompleted;

            int? receiverLevelId = null;
            int operationId;
            var product = context.TBL_PRODUCT.Find(detail.PROPOSEDPRODUCTID);
            var productBahaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == detail.PROPOSEDPRODUCTID).FirstOrDefault();

            if (appl.ISADHOCAPPLICATION == true)
            {
                operationId = (int)OperationsEnum.AdhocApproval;
                workflow.OperationId = operationId;
                appl.OPERATIONID = (int)OperationsEnum.AdhocApproval;
                receiverLevelId = GetFirstAdhocReceiverLevel(staffId, operationId, appl.PRODUCTCLASSID, false);
                workflow.NextLevelId = receiverLevelId;
                appl.DATEACTEDON = DateTime.Now;
                context.SaveChanges();
            }
            else if (PushApplicationToDrawdown(appl.APPLICATIONREFERENCENUMBER))
            {
                return 2;
            }
            else
            {
                operationId = appl.OPERATIONID; // (int)OperationsEnum.CreditAppraisal;
                workflow.OperationId = operationId;
                appl.OPERATIONID = operationId;
                workflow.ToStaffId = staffId;
                receiverLevelId = GetFirstReceiverLevel(staffId, operationId, appl.PRODUCTCLASSID, appl.PRODUCTID, appl.FLOWCHANGEID);
                workflow.NextLevelId = receiverLevelId; // BREAKING!
            }


            workflow.StaffId = staffId;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.ProductId = appl.PRODUCTID;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "New loan application";
            workflow.ExclusiveFlowChangeId = appl.FLOWCHANGEID;

            if (workflow.LogActivity()) return 1;
            else return 0;
        }

        private bool ValidateCollateralSearchJobRequests(int loanApplicationDetailId, int? requireCollateralTypeId = null, bool throwErrorMessage = false)
        {
            string errorMessage = String.Empty;
            List<TBL_JOB_REQUEST> requests = new List<TBL_JOB_REQUEST>();

            if (requireCollateralTypeId == (int)RequireCollateralTypeEnum.ImmovablePropertyCollateral)
            {
                requests = context.TBL_JOB_REQUEST
                    .Where(x => x.TARGETID == loanApplicationDetailId
                    && x.OPERATIONSID == (short)OperationsEnum.LoanApplication
                    && x.JOBTYPEID == (short)JobTypeEnum.legal
                    && x.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending
                ).ToList();

                if (requests.Count() > 0) errorMessage = errorMessage + "Job Request to Legal for immovable property collateral is required! ";
            }

            // var test = requests;

            if (throwErrorMessage) throw new SecureException(errorMessage);
            return requests.Count() > 0;
        }

        public int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, int? productId, int? exclusiveFlowChangeId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var mappingsOnProducts = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && (x.PRODUCTID == productId && x.PRODUCTID != null)
                           )
                           .ToList();

            var mappingsOnProductClass = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && (x.PRODUCTCLASSID == productClassId && x.PRODUCTCLASSID != null)
                               && x.PRODUCTID == null
                           )
                           .ToList();

            var mappingsOnOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == operationId
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();

            List<TBL_APPROVAL_GROUP_MAPPING> mappingsOnExclusiveOperations = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (exclusiveFlowChangeId > 0)
            {
                var flowChangePartern = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(exclusiveFlowChangeId);

                if (flowChangePartern != null)
                {
                    mappingsOnExclusiveOperations = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                               && x.OPERATIONID == flowChangePartern.OPERATIONID
                               && x.PRODUCTCLASSID == null
                               && x.PRODUCTID == null
                           )
                           .ToList();
                }
            }

            List<TBL_APPROVAL_GROUP_MAPPING> mappings = new List<TBL_APPROVAL_GROUP_MAPPING>();

            if (mappingsOnOperations.Any()) mappings = mappingsOnOperations;

            if (mappingsOnProductClass.Any()) mappings = mappingsOnProductClass;

            if (mappingsOnProducts.Any()) mappings = mappingsOnProducts;

            if (mappingsOnExclusiveOperations.Any()) mappings = mappingsOnExclusiveOperations;

            if (mappingsOnProducts.Any() == false && mappingsOnProductClass.Any() == false && mappingsOnOperations.Any() == false && mappingsOnExclusiveOperations.Any() == false)
            {
                var operation = context.TBL_OPERATIONS.Find(operationId);
                if (operation == null) throw new SecureException("Operation ID didn't match");
                if (productClassId != null)
                {
                    var productClass = context.TBL_PRODUCT_CLASS.Find(productClassId);
                    throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME + ", PRODUCT CLASS: " + productClass.PRODUCTCLASSNAME);
                }
                throw new SecureException("There is no approval workflow setup for the OPERATION: " + operation.OPERATIONNAME);
            }

            //var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId && x.PRODUCTID == productId)
            var levels = mappings.Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;


            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID).ToList();
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId).ToList();
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public int? GetFirstAdhocReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId)
                    .Join(context.TBL_APPROVAL_GROUP, m => m.GROUPID, g => g.GROUPID, (m, g) => new { m, g })
                    .Join(context.TBL_APPROVAL_LEVEL.Where(x => x.ISACTIVE == true),
                        mg => mg.g.GROUPID, l => l.GROUPID, (mg, l) => new
                        {
                            groupPosition = mg.m.POSITION,
                            levelPosition = l.POSITION,
                            levelId = l.APPROVALLEVELID,
                            levelName = l.LEVELNAME,
                            staffRoleId = l.STAFFROLEID,
                        })
                        .OrderBy(x => x.groupPosition)
                        .ThenBy(x => x.levelPosition)
                        .ToList()
                        ;

            var staffRoleLevelId = levels.FirstOrDefault().levelId;
            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public int? GetFirstLevelStaffId(int levelId, int userBranch)
        {
            if (levelId == 0) return 2;
            int staffId;
            var designatedStaff = (int?)context.TBL_APPROVAL_LEVEL_STAFF.Where(l => l.APPROVALLEVELID == levelId && l.DELETED == false).FirstOrDefault()?.STAFFID ?? 0;
            if (designatedStaff == 0)
            {
                var staffLevel = context.TBL_APPROVAL_LEVEL.Find(levelId);
                staffId = (int?)context.TBL_STAFF.Where(s => s.STAFFROLEID == staffLevel.STAFFROLEID && s.DELETED == false && s.BRANCHID == userBranch).FirstOrDefault()?.STAFFID ?? 0;
                return staffId;
            }
            return designatedStaff;
        }

        public string GetRefrenceNumber()
        {
            var millisecond = DateTime.Now.Millisecond;
            string refnumber = CommonHelpers.GetLoanReferanceNumber().ToString()
                + "" + CommonHelpers.AppendZeroString(millisecond, 3);
            return refnumber.ToString();
        }

        private void validateNonComformingProduct(LoanApplicationViewModel model, TBL_LOAN_APPLICATION parentData, List<TBL_LOAN_APPLICATION_DETAIL> lineRecords)
        {
            if (model.LoanApplicationDetail.Count() > 0)
            {
                var newlineRecord = model.LoanApplicationDetail.FirstOrDefault();

                if (newlineRecord.proposedProductId == 12)
                {
                    foreach (var line in lineRecords)
                    {
                        if (line.PROPOSEDPRODUCTID != newlineRecord.proposedProductId) { throw new ConditionNotMetException("Application already has products with different behaviour."); }
                    }
                }


                if (newlineRecord.proposedProductId != 12)
                {
                    List<short> productIds = new List<short>();
                    productIds = lineRecords.Select(x => x.PROPOSEDPRODUCTID).ToList();

                    if (productIds.Contains(12))
                    {
                        var productRecord = context.TBL_PRODUCT.Find(newlineRecord.proposedProductId);
                        throw new ConditionNotMetException("Application already has product " + productRecord.PRODUCTNAME + " with unique behaviour");
                    }

                }
            }
        }

        private bool PushApplicationToDrawdown(string applicationReferenceNumber)
        {
            //var newLineRecord = model.LoanApplicationDetail.FirstOrDefault();
            var headerRecords = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == applicationReferenceNumber);
            var headerrecord = headerRecords.FirstOrDefault();
            if (headerrecord == null) return false;

            var lineRecords = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == headerrecord.LOANAPPLICATIONID);
            var lineRecord = lineRecords.FirstOrDefault();
            var productBehaviour = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(headerrecord.FLOWCHANGEID);

            if (productBehaviour != null && productBehaviour.ISSKIPPROCESSENABLED == true)
            {
                foreach (var line in lineRecords)
                {
                    line.APPROVEDAMOUNT = line.PROPOSEDAMOUNT;
                    line.APPROVEDINTERESTRATE = line.PROPOSEDINTERESTRATE;
                    line.APPROVEDPRODUCTID = line.PROPOSEDPRODUCTID;
                    line.APPROVEDTENOR = line.PROPOSEDTENOR;
                    line.STATUSID = (short)ApprovalStatusEnum.Approved;

                    headerrecord.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
                    headerrecord.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
                }
                context.SaveChanges();
                return true;
            }
            return false;
        }

        //private bool PushApplicationToDrawdown(string applicationReferenceNumber)
        //{
        //    //var newLineRecord = model.LoanApplicationDetail.FirstOrDefault();
        //    var headerRecords = context.TBL_LOAN_APPLICATION.Where(x=>x.APPLICATIONREFERENCENUMBER == applicationReferenceNumber);
        //    var headerrecord = headerRecords.FirstOrDefault();
        //    var lineRecords = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == headerrecord.LOANAPPLICATIONID);
        //    var lineRecord = lineRecords.FirstOrDefault();
        //    var productBehaviour = context.TBL_PRODUCT_BEHAVIOUR.Where(x => x.PRODUCTID == lineRecords.FirstOrDefault().PROPOSEDPRODUCTID).FirstOrDefault();

        //    if (productBehaviour != null && productBehaviour.SKIPPROCESSFLOW == true)
        //    {
        //        foreach (var line in lineRecords)
        //        {
        //            line.APPROVEDAMOUNT = line.PROPOSEDAMOUNT;
        //            line.APPROVEDINTERESTRATE = line.PROPOSEDINTERESTRATE;
        //            line.APPROVEDPRODUCTID = line.PROPOSEDPRODUCTID;
        //            line.APPROVEDTENOR = line.PROPOSEDTENOR;
        //            line.STATUSID = (short)ApprovalStatusEnum.Approved;

        //            headerrecord.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.AvailmentCompleted;
        //            headerrecord.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
        //        }
        //        context.SaveChanges();
        //        return true;
        //    }
        //    return false;
        //}
        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            var customerId = customer.FirstOrDefault()?.customerId;
            var allGroupMappings = GetCustomerGroupMapping();
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (loanTypeId == (int)LoanTypeEnum.CustomerGroup && customer.Count() == 1)
            {
                var customerGroupMappings = new List<CustomerGroupMappingViewModel>();
                var mappings = allGroupMappings.Where(m => m.customerGroupId == customerId).ToList();

                if (mappings.Count() > 0)
                {
                    customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                }
            }
            else
            {
                var customerIsAGroupMember = allGroupMappings.Any(m => m.customerId == customerId);
                if (customerIsAGroupMember)
                {
                    var mappings = new List<CustomerGroupMappingViewModel>();
                    var customerGroups = allGroupMappings.Where(m => m.customerId == customerId).ToList();
                    var allGroupIds = customerGroups.Select(m => m.customerGroupId).Distinct().ToList();
                    foreach (var groupId in allGroupIds)
                    {
                        var mapping = allGroupMappings.Where(m => m.customerGroupId == groupId).ToList();
                        mappings.AddRange(mapping);
                    }
                    if (mappings.Count() > 0)
                    {
                        customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                    }
                }
            }

            foreach (var item in customer)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == item.customerId)?.CUSTOMERCODE.Trim();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                customerName = a.CUSTOMERNAME,
                                customerCode = a.CUSTOMERID.Trim(),
                                facilityType = a.ADJFACILITYTYPE,
                                approvedAmount = a.LOANAMOUNYTCY ?? 0,
                                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
                                currency = a.CURRENCYNAME,
                                currencyType = a.CURRENCYTYPE,
                                exposureTypeCodeString = a.EXPOSURETYPECODE,
                                adjFacilityTypeString = a.ADJFACILITYTYPE,
                                adjFacilityTypeCode = a.ADJFACILITYTYPEid.Trim(),
                                productIdString = a.PRODUCTID,
                                productCode = a.PRODUCTCODE,
                                productName = a.PRODUCTNAME,
                                currencyCode = a.ALPHACODE,
                                //existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                //proposedLimit = a.LOANAMOUNYLCY ?? 0,
                                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
                                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                bookingDate = a.BOOKINGDATE,
                                maturityDate = a.MATURITYDATE,
                                tenorString = a.TENOR,
                                //maturityDateString = a.MATURITYDATE,
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).ToList();

                if (exposure.Count() > 0)
                {
                    foreach (var e in exposure)
                    {
                        e.exposureTypeId = int.Parse(e.exposureTypeCodeString);
                        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
                        e.bookingDate = e.bookingDate?.Date;
                        e.maturityDate = e.maturityDate?.Date;
                        //e.productId = int.Parse(e.productIdString);
                        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);
                    }
                    exposures.AddRange(exposure);
                }


                //exposure = from a in context.TBL_LOAN
                //           join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                //           join b in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                //           where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //           select new CurrentCustomerExposure
                //           {
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               existingLimit = a.PRINCIPALAMOUNT,
                //               //proposedLimit = a.OUTSTANDINGPRINCIPAL,
                //               proposedLimit = 0,
                //               //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                //               outstandings = a.OUTSTANDINGPRINCIPAL,
                //               recommendedLimit = 0,
                //               PastDueObligationsInterest = a.PASTDUEINTEREST,
                //               PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //               reviewDate = DateTime.Now,
                //               prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //               loanStatus = "Running",
                //               referenceNumber = a.LOANREFERENCENUMBER,
                //               applicationStatusId = b.APPLICATIONSTATUSID
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = (from a in context.TBL_LOAN_REVOLVING
                //            join b in context.TBL_LOAN_APPLICATION on a.LOANREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //            where a.CUSTOMERID == item.customerId && a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                //            select new CurrentCustomerExposure
                //            {
                //                facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //                existingLimit = a.OVERDRAFTLIMIT,
                //                //proposedLimit = a.OVERDRAFTLIMIT,
                //                proposedLimit = 0,
                //                //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,   
                //                outstandings = a.OVERDRAFTLIMIT,
                //                recommendedLimit = 0,
                //                casaAccountId = a.CASAACCOUNTID,
                //                PastDueObligationsInterest = a.PASTDUEINTEREST,
                //                PastDueObligationsPrincipal = a.PASTDUEPRINCIPAL,
                //                reviewDate = DateTime.Now,
                //                prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                //                loanStatus = "Running",
                //                referenceNumber = a.LOANREFERENCENUMBER,
                //                applicationStatusId = b.APPLICATIONSTATUSID
                //            }).ToList();
                //.Select(x =>
                //{
                //    //var availableBalance = transRepo.GetCASABalance((int)x.casaAccountId).availableBalance;
                //    var availableBalance = context.TBL_CASA.FirstOrDefault(m=>m.CASAACCOUNTID == (int)x.casaAccountId).AVAILABLEBALANCE;

                //    if ( availableBalance >= 0)
                //    {
                //        x.outstandings = 0;
                //    }
                //    else
                //    {
                //        x.outstandings = Math.Abs(availableBalance); 
                //    }
                //    return x;
                //});

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                //           join b in context.TBL_LOAN_APPLICATION on a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                //           join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                //           where a.CUSTOMERID == item.customerId && a.TBL_LOAN_APPLICATION.COMPANYID == companyId && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                //           select new CurrentCustomerExposure
                //           {

                //               applicationStatusId = b.APPLICATIONSTATUSID,
                //               customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                //               customerCode = c.CUSTOMERCODE.Trim(),
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               approvedAmount = a.APPROVEDAMOUNT,
                //               currency = a.TBL_CURRENCY.CURRENCYNAME,
                //               //exposureTypeId = int.Parse(a.EXPOSURETYPECODE),
                //               //adjFacilityType = a.ADJFACILITYTYPE,
                //               productId = a.TBL_PRODUCT.PRODUCTID,
                //               productName = a.TBL_PRODUCT.PRODUCTNAME,
                //               outstandings = 0,
                //               pastDueObligationsPrincipal = 0,
                //               reviewDate = DateTime.Now,
                //               //bookingDate = DateTime.Parse(a.BOOKINGDATE),
                //               //maturityDate = DateTime.Parse(a.MATURITYDATE),
                //               loanStatus = "Processing",
                //               referenceNumber = b.APPLICATIONREFERENCENUMBER
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);


                //var staggingLoan = from a in stgCon.STG_LOAN_MART
                //                   where a.CUST_ID == customCode
                //                   select new CurrentCustomerExposure
                //                   {
                //                       facilityType = a.SCHM_TYPE,
                //                       existingLimit = a.FAC_GRANT_AMT,
                //                       //proposedLimit = a.FINAL_BALANCE,
                //                       proposedLimit = 0,
                //                       recommendedLimit = 0,
                //                       outstandings = a.FINAL_BALANCE,
                //                       PastDueObligationsInterest = a.INT_DUE,
                //                       PastDueObligationsPrincipal = a.DAYS_PAST_DUE,// 0,
                //                       reviewDate = DateTime.Now,
                //                       prudentialGuideline = a.USER_CLASSIFICATION == "1" ? "Performing" : "Non-Performing",
                //                       loanStatus = "Running"
                //                   };

                //exposures.Union(staggingLoan);

            }

            if (exposures.Count() == 0)
            {
                return exposures;
            }

            //exposures.Add(new CurrentCustomerExposure
            //{
            //    facilityType = "TOTAL",
            //    existingLimit = exposures.Sum(t => t.existingLimit),
            //    proposedLimit = exposures.Sum(t => t.proposedLimit),
            //    bookingDate = exposures.Max(t => t.bookingDate),
            //    maturityDate = exposures.Max(t => t.maturityDate),
            //    //approvedAmount = exposures.Sum(t => t.approvedAmount),
            //    recommendedLimit = exposures.Sum(t => t.recommendedLimit),
            //    outstandings = exposures.Sum(t => t.outstandings),
            //    PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
            //    pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
            //    reviewDate = DateTime.Now,
            //});

            return exposures;
        }
        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                        where a.DELETED == false
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                            customerGroupId = a.CUSTOMERGROUPID,
                                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                                            //createdBy = a.CreatedBy,
                                            customerId = a.CUSTOMERID,
                                            //dateTimeCreated = a.DateTimeCreated
                                        }).ToList();

            return customerGroupMapping;
        }

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId, short loantypeId) // not used!
        {
            return GetCurrentCustomerExposure(customerIds, loantypeId, companyId); // old ify impl
        }

        public List<CurrentCustomerExposure> GetExposures(TBL_LOAN_APPLICATION loanApplication)
        {
            var exposures = new List<CurrentCustomerExposure>();
            var customerIds = new List<CustomerExposure>();
            if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID, loanApplication.LOANAPPLICATIONTYPEID);
            }
            else
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERGROUPID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID, loanApplication.LOANAPPLICATIONTYPEID);
            }
            return exposures;
        }

        public LoanApplicationViewModel AddLoanApplication(LoanApplicationViewModel loan, bool isExceptionalLoan = false)
        {
            if (!isExceptionalLoan) {
                try
                {
                    ValidateLoanApplicationLimits(loan);
                }
                catch (SecureException ex)
                {
                    AddExceptionalLoanApplication(loan);
                    AddExceptionalLoanApplicationDetail(loan, ex.Message);
                    throw new SecureException($"{ex.Message} - Loan has been saved for Exceptional Workflow!");
                }
            }

            var additionalAmount = loan.LoanApplicationDetail.Where(x => x.deleted == false).Sum(x => x.exchangeAmount);
            var savedDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId && c.DELETED == false).ToList();

            decimal cumulativeSum = 0;
            foreach (var s in savedDetails) { cumulativeSum = cumulativeSum + (s.PROPOSEDAMOUNT * (decimal)s.EXCHANGERATE); }

            if (!isExceptionalLoan)
            {
                var validation = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId);
                if (validation.maximumAllowedLimit > 0) if ((cumulativeSum + additionalAmount) > (decimal)validation.limit) throw new SecureException($"RM Limit Exceeded. The limit of this RM is {validation.limit}");
            }

            loan.applicationAmount = cumulativeSum + additionalAmount;

            if (loan.editMode == true && UpdateLoanApplicationDetail(loan)) { return loan; }

            loanData = context.TBL_LOAN_APPLICATION.FirstOrDefault(l => l.APPLICATIONREFERENCENUMBER.Trim().ToLower() == loan.applicationReferenceNumber.Trim().ToLower() && l.DELETED == false);

            if (loan.productClassId == (short)ProductClassEnum.Creditcards)
            {
                validateNonComformingProduct(loan, loanData, savedDetails);
            }

            if (savedDetails.Count() == 0 || loan.isNewApplication)
            {

                if (loanData != null)
                {
                    loanData.APPLICATIONAMOUNT = loan.applicationAmount;
                    loanData.TOTALEXPOSUREAMOUNT = cumulativeSum + additionalAmount + (GetExposures(loanData).Sum(e => e.outstandingsLcy));
                    //loanData.TOTALEXPOSUREAMOUNT = cumulativeSum + additionalAmount + GetCustomerTotalOutstandingBalance((int)loan.customerId);
                    loanData.ISADHOCAPPLICATION = loan.isadhocapplication;
                    loanData.LOANAPPROVEDLIMITID = loan.loanApprovedLimitId;

                }

                if (loanData == null) // first time
                {
                    if (string.IsNullOrEmpty(loan.applicationReferenceNumber)) loan.applicationReferenceNumber = GetRefrenceNumber();
                    AddloanApplicationSub(loan);
                }

                if (loan.LoanApplicationDetail.Count > 0)
                {

                    var racReponse = AddLoanApplicationDetail(loan);
                    if (racReponse != null)
                    {
                        LoanApplicationViewModel model = new LoanApplicationViewModel();
                        model.loanApplicationId = (int)racReponse.loanApplicationId;
                        model.failedRacStartCam = true;
                        model.loanApplicationDetailId = (int)racReponse.loanApplicationDetailId;

                        //  trans.Commit();
                        return model;
                    }

                }
            }
            else
            {
                 var limit = limitValidation.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId).limit;
                 if ((limit != 0 && loan.applicationAmount != 0 && loan.applicationAmount > (decimal)limit)) throw new SecureException($"RM Limit Exceeded. The limit of this RM is {limit}");
                 UpdateLoanApplication(loan);
            }


            // if (response == 0)
            response = context.SaveChanges();

            var returndate = GetLoanApplicationByLoanRefrenceNo(loanData.APPLICATIONREFERENCENUMBER, loanData.COMPANYID);

            if(returndate != null)
            {
                int customerId = returndate.singleCustomerId != null ? returndate.singleCustomerId.Value : 0;
                var customer = context.TBL_CUSTOMER.Find(customerId);

                if (customer != null)
                {
                    var branchOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride).FirstOrDefault();
                    var sectorOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride).FirstOrDefault();
                    var customerOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.CustomerExposureLimitOverride).FirstOrDefault();

                    if (branchOverrideRequest != null)
                    {
                        branchOverrideRequest.ISUSED = true;
                        branchOverrideRequest.USEDBY = returndate.createdBy;
                        branchOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                    if (sectorOverrideRequest != null)
                    {
                        sectorOverrideRequest.ISUSED = true;
                        sectorOverrideRequest.USEDBY = returndate.createdBy;
                        sectorOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                    if (customerOverrideRequest != null)
                    {
                        customerOverrideRequest.ISUSED = true;
                        customerOverrideRequest.USEDBY = returndate.createdBy;
                        customerOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                }
                else
                {
                    int customerGroupId = (int)returndate.customerGroupId;
                    var customerGroup = context.TBL_CUSTOMER_GROUP.Find(customerGroupId);
                    var branchOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customerGroup.GROUPCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride).FirstOrDefault();
                    var sectorOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customerGroup.GROUPCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride).FirstOrDefault();
                    var customerOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customerGroup.GROUPCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == loan.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.CustomerExposureLimitOverride).FirstOrDefault();

                    if (branchOverrideRequest != null)
                    {
                        branchOverrideRequest.ISUSED = true;
                        branchOverrideRequest.USEDBY = returndate.createdBy;
                        branchOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                    if (sectorOverrideRequest != null)
                    {
                        sectorOverrideRequest.ISUSED = true;
                        sectorOverrideRequest.USEDBY = returndate.createdBy;
                        sectorOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                    if (customerOverrideRequest != null)
                    {
                        customerOverrideRequest.ISUSED = true;
                        customerOverrideRequest.USEDBY = returndate.createdBy;
                        customerOverrideRequest.SOURCE_REFERENCE_NUMBER = returndate.applicationReferenceNumber;
                        context.SaveChanges();
                    }
                }
            }
            

            if (response > 0 && !loan.isNewApplication)
            {
                //trans.Commit();
                returndate.closeApplication = true;

                //returndate.jumpedDestination = PushApplicationToDrawdown(loan, loanData.APPLICATIONREFERENCENUMBER);
            }


            // trans.Commit();
            return returndate;
            //  }


        }

        private int? GetWorkflowProductId(short productId)
        {
            if (context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.DELETED == false
                && x.OPERATIONID == (int)OperationsEnum.CreditAppraisal //&& x.PRODUCTCLASSID == model.productClassId
                && x.PRODUCTID == productId).Any()) return productId;
            return null;
        }

        public RacReturnInfoViewModel SaveRac(RacInformationViewModel rac, int? operationId, int productId, int? productClassId, int targetId, int staffId, int applicationId)
        {
            //try { 
                List<TBL_RAC_DEFINITION> definitions = new List<TBL_RAC_DEFINITION>();
                var msg = new RacReturnInfoViewModel();
                if (rac.form == null || rac.form.Count == 0) return null;
                var ids = rac.form.Select(x => x.criteriaId);

                definitions = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false
                   && ids.Contains(x.RACDEFINITIONID)
               ).ToList();

                // is tier related?, get default rac
                var isRacRelated = definitions.Where(o => o.RACCATEGORYTYPEID != null).Any();
                TBL_RAC_DEFINITION defaultTier = new TBL_RAC_DEFINITION();
                List<TBL_RAC_DEFINITION> defaultDefinition = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> racTiers = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> allTierRacs = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> defaultTierItems = new List<TBL_RAC_DEFINITION>();

                var b = definitions.FirstOrDefault();
                allTierRacs = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false && x.RACCATEGORYID == b.RACCATEGORYID).ToList();
                int lastRacIndex = 0;
                //List<int?> racCategoryTypeIds = allTierRacs.Select(x => x.RACCATEGORYTYPEID).ToList() ;

                var submission = new RacFormControlValue();

                if (isRacRelated == true)
                {
                    defaultTierItems = allTierRacs.Where(x => x.ISRACTIERCONTROLKEY == true).ToList();
                    
                    if (defaultTierItems.Count() <= 0) { throw new ConditionNotMetException("Control keys have not been setup for the RAC Tiers"); }

                    List<TBL_RAC_DEFINITION> matchedTierRac = new List<TBL_RAC_DEFINITION>();

                    var submissionRac = new RacFormControlValue().value;
                    foreach (var i in defaultTierItems)
                    {
                        submissionRac = (submissionRac == null) ? rac.form.FirstOrDefault(x => x.criteriaId == i.RACDEFINITIONID).value : submissionRac;

                        if (submission != null && ValidRacSubmission(i, submissionRac, operationId ?? 0, targetId))
                        {
                            matchedTierRac = allTierRacs.Where(x => x.RACCATEGORYTYPEID == i.RACCATEGORYTYPEID.Value)?.ToList();
                        };

                        if (matchedTierRac.Count() > 0)
                        {
                            defaultTier = matchedTierRac.FirstOrDefault();
                            lastRacIndex = defaultTierItems.IndexOf(i);
                            break;
                        }
                    }

                    if (matchedTierRac.Count() <= 0)
                    {
                        //throw new ConditionNotMetException("Could match RAC control key to any tier");
                        msg.loanApplicationDetailId = targetId;
                        msg.loanApplicationId = applicationId;
                        return msg;
                    }

                    defaultDefinition.AddRange(allTierRacs.Where(x => x.RACCATEGORYTYPEID == defaultTier.RACCATEGORYTYPEID).ToList());

                    racTiers = allTierRacs.Where(x => x.RACCATEGORYTYPEID != defaultTier.RACCATEGORYTYPEID).Select(x => x).OrderByDescending(a => a.RACCATEGORYTYPEID)
                                                                                                                          .ThenByDescending(a => a.RACITEMID).ToList();


                    definitions = defaultDefinition;
                }


                List<TBL_RAC_DETAIL> details = new List<TBL_RAC_DETAIL>();

                //int index;
                int ctr = 0;
                for (int i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    //index = i;

                    submission = rac.form.FirstOrDefault(x => x.criteriaId == definition.RACDEFINITIONID);

                    if (isRacRelated)
                    {

                        List<int> definitionId = new List<int>();
                        definitionId = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false && x.RACCATEGORYID == definition.RACCATEGORYID && x.RACITEMID == definition.RACITEMID).Select(d => d.RACDEFINITIONID).ToList();
                        //definitionId.AddRange(allTierRacs.Select(x => x.RACDEFINITIONID));

                        submission = rac.form.FirstOrDefault(x => definitionId.Contains(x.criteriaId));
                    }


                    if (submission == null) continue;

                    bool validation = ValidRacSubmission(definition, submission.value, operationId ?? 0, targetId);
                   
                    if (validation == false && ctr == 0)
                    {

                    throw new SecureException("Cannot Proceed. RAC not met for " + definition.TBL_RAC_ITEM.CRITERIA);
                    /* if (racTiers.Count() <= 0)
                     {
                         saveRacoptions(definitions, rac, operationId ?? 0, targetId, staffId);
                         msg.loanApplicationDetailId = targetId;
                         msg.loanApplicationId = applicationId;
                         return msg;
                     }

                     if (racTiers.Count() > 0 && ctr == 0)
                     {
                         var lastRacIndexTierItems = defaultTierItems[lastRacIndex + 1];
                         definitions = racTiers = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false
                         && ids.Contains(x.RACDEFINITIONID) && x.RACCATEGORYTYPEID == lastRacIndexTierItems.RACCATEGORYTYPEID && x.RACCATEGORYTYPEID != definition.RACCATEGORYTYPEID
                         ).Select(x => x).OrderByDescending(a => a.RACCATEGORYTYPEID).ThenByDescending(a => a.RACITEMID).ToList();

                     }

                     if (racTiers.Count() > 0 && ctr > 0)
                     {
                         saveRacoptions(definitions, rac, operationId ?? 0, targetId, staffId);
                         msg.loanApplicationDetailId = targetId;
                         msg.loanApplicationId = applicationId;
                         return msg;
                     }

                     ctr = ctr + 1;
                     continue;*/
                }
                    else if (validation == true)
                    {
                        details.Add(new TBL_RAC_DETAIL
                        {
                            RACDEFINITIONID = definition.RACDEFINITIONID,
                            OPERATIONID = operationId ?? 0,
                            TARGETID = targetId,
                            ACTUALVALUE = submission.value,
                            CREATEDBY = staffId,
                            DATETIMECREATED = DateTime.Now,
                        });

                        continue;
                    }

                    break;
                }

                context.TBL_RAC_DETAIL.AddRange(details);
           
                context.SaveChanges();
                return null;
            //}
            //catch (Exception ex)
            //{
            //    throw new SecureException("Error saving rac " +ex);
            //}


           

            //foreach (var definition in definitions)
            //{
            //    var submission = rac.form.FirstOrDefault(x => x.criteriaId == definition.RACDEFINITIONID);
            //    if (submission == null) continue;
            //    if (ValidRacSubmission(definition, submission.value, operationId, targetId) == false && ctr == 0)
            //    {
            //        //int index = definitions.FindIndex(x => x.RACDEFINITIONID == definition.RACDEFINITIONID);
            //        if (ctr == 0)
            //        {
            //            definitions = racTiers = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false
            //            && x.PRODUCTID == productId && x.ISRACTIERCONTROLKEY == true && x.RACCATEGORYTYPEID != defaultTier.RACCATEGORYTYPEID
            //            && x.RACCATEGORYID == defaultTier.RACCATEGORYID
            //            ).Select(x => x).OrderByDescending(i => i.RACCATEGORYTYPEID).ThenByDescending(i => i.RACITEMID).ToList();

            //            continue;
            //        }

            //        ctr = ctr + 1;
            //    }
            //    else { return applicationId; } // throw new SecureException("Cannot Proceed. RAC not met for " + definition.TBL_RAC_ITEM.CRITERIA); }

            //    //if (!ValidRacSubmission(definition, submission.value, operationId, targetId)) return applicationId;//throw new SecureException("Cannot Proceed as RAC not met for " + definition.TBL_RAC_ITEM.CRITERIA);
            //    details.Add(new TBL_RAC_DETAIL
            //    {
            //        RACDEFINITIONID = definition.RACDEFINITIONID,
            //        OPERATIONID = operationId,
            //        TARGETID = targetId,
            //        ACTUALVALUE = submission.value,
            //        CREATEDBY = staffId,
            //        DATETIMECREATED = DateTime.Now,
            //    });
            //}


        }

        public RacReturnInfoViewModel SaveExceptionalRac(RacInformationViewModel rac, int? operationId, int productId, int? productClassId, int targetId, int staffId, int applicationId)
        {
            try
            {
                List<TBL_RAC_DEFINITION> definitions = new List<TBL_RAC_DEFINITION>();
                var msg = new RacReturnInfoViewModel();
                if (rac.form == null || rac.form.Count == 0) return null;
                var ids = rac.form.Select(x => x.criteriaId);

                definitions = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false
                   && ids.Contains(x.RACDEFINITIONID)
               ).ToList();

                // is tier related?, get default rac
                var isRacRelated = definitions.Where(o => o.RACCATEGORYTYPEID != null).Any();
                TBL_RAC_DEFINITION defaultTier = new TBL_RAC_DEFINITION();
                List<TBL_RAC_DEFINITION> defaultDefinition = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> racTiers = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> allTierRacs = new List<TBL_RAC_DEFINITION>();
                List<TBL_RAC_DEFINITION> defaultTierItems = new List<TBL_RAC_DEFINITION>();

                var b = definitions.FirstOrDefault();
                allTierRacs = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false && x.RACCATEGORYID == b.RACCATEGORYID).ToList();
                int lastRacIndex = 0;
                //List<int?> racCategoryTypeIds = allTierRacs.Select(x => x.RACCATEGORYTYPEID).ToList() ;

                var submission = new RacFormControlValue();

                if (isRacRelated == true)
                {
                    defaultTierItems = allTierRacs.Where(x => x.ISRACTIERCONTROLKEY == true).ToList();
                    if (defaultTierItems.Count() <= 0) { throw new ConditionNotMetException("Control keys have not been setup for the RAC Tiers"); }

                    List<TBL_RAC_DEFINITION> matchedTierRac = new List<TBL_RAC_DEFINITION>();

                    var submissionRac = new RacFormControlValue().value;
                    foreach (var i in defaultTierItems)
                    {
                        submissionRac = (submissionRac == null) ? rac.form.FirstOrDefault(x => x.criteriaId == i.RACDEFINITIONID).value : submissionRac;

                        if (submission != null && ValidExceptionalRacSubmission(i, submissionRac, operationId ?? 0, targetId))
                        {
                            matchedTierRac = allTierRacs.Where(x => x.RACCATEGORYTYPEID == i.RACCATEGORYTYPEID.Value)?.ToList();
                        };

                        if (matchedTierRac.Count() > 0)
                        {
                            defaultTier = matchedTierRac.FirstOrDefault();
                            lastRacIndex = defaultTierItems.IndexOf(i);
                            break;
                        }
                    }

                    if (matchedTierRac.Count() <= 0)
                    {
                        //throw new ConditionNotMetException("Could match RAC control key to any tier");
                        msg.loanApplicationDetailId = targetId;
                        msg.loanApplicationId = applicationId;
                        return msg;
                    }

                    defaultDefinition.AddRange(allTierRacs.Where(x => x.RACCATEGORYTYPEID == defaultTier.RACCATEGORYTYPEID).ToList());

                    racTiers = allTierRacs.Where(x => x.RACCATEGORYTYPEID != defaultTier.RACCATEGORYTYPEID).Select(x => x).OrderByDescending(a => a.RACCATEGORYTYPEID)
                                                                                                                          .ThenByDescending(a => a.RACITEMID).ToList();
                    definitions = defaultDefinition;
                }

                List<TBL_EXCEPTIONAL_RAC_DETAIL> details = new List<TBL_EXCEPTIONAL_RAC_DETAIL>();

                int index; int ctr = 0;
                for (int i = 0; i < definitions.Count; i++)
                {
                    var definition = definitions[i];
                    index = i;

                    submission = rac.form.FirstOrDefault(x => x.criteriaId == definition.RACDEFINITIONID);

                    if (isRacRelated)
                    {
                        List<int> definitionId = new List<int>();
                        definitionId = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false && x.RACCATEGORYID == definition.RACCATEGORYID && x.RACITEMID == definition.RACITEMID).Select(d => d.RACDEFINITIONID).ToList();
                        //definitionId.AddRange(allTierRacs.Select(x => x.RACDEFINITIONID));

                        submission = rac.form.FirstOrDefault(x => definitionId.Contains(x.criteriaId));
                    }


                    if (submission == null) continue;

                    bool validation = ValidExceptionalRacSubmission(definition, submission.value, operationId ?? 0, targetId);

                    if (validation == false && ctr == 0)
                    {
                        if (racTiers.Count() <= 0)
                        {
                            saveExceptionalRacoptions(definitions, rac, operationId ?? (int)OperationsEnum.LoanApplication, targetId, staffId);
                            msg.loanApplicationDetailId = targetId;
                            msg.loanApplicationId = applicationId;
                            return msg;
                        }

                        if (racTiers.Count() > 0 && ctr == 0)
                        {
                            var lastRacIndexTierItems = defaultTierItems[lastRacIndex + 1];
                            definitions = racTiers = context.TBL_RAC_DEFINITION.Where(x => x.ISACTIVE == true && x.DELETED == false
                            && ids.Contains(x.RACDEFINITIONID) && x.RACCATEGORYTYPEID == lastRacIndexTierItems.RACCATEGORYTYPEID && x.RACCATEGORYTYPEID != definition.RACCATEGORYTYPEID
                            ).Select(x => x).OrderByDescending(a => a.RACCATEGORYTYPEID).ThenByDescending(a => a.RACITEMID).ToList();

                        }

                        if (racTiers.Count() > 0 && ctr > 0)
                        {
                            saveExceptionalRacoptions(definitions, rac, operationId ?? (int)OperationsEnum.LoanApplication, targetId, staffId);
                            msg.loanApplicationDetailId = targetId;
                            msg.loanApplicationId = applicationId;
                            return msg;
                        }

                        ctr = ctr + 1;
                        continue;
                    }
                    else if (validation == true)
                    {
                        details.Add(new TBL_EXCEPTIONAL_RAC_DETAIL
                        {
                            RACDEFINITIONID = definition.RACDEFINITIONID,
                            OPERATIONID = operationId ?? (int)OperationsEnum.LoanApplication,
                            TARGETID = targetId,
                            ACTUALVALUE = submission.value,
                            CREATEDBY = staffId,
                            DATETIMECREATED = DateTime.Now,
                        });

                        continue;
                    }

                    break;
                }

                context.TBL_EXCEPTIONAL_RAC_DETAIL.AddRange(details);
                context.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private List<TBL_RAC_DETAIL> saveRacoptions(List<TBL_RAC_DEFINITION> definitions, RacInformationViewModel rac, int operationId, int targetId, int staffId)
        {
            //FinTrakBankingContext racContext = new FinTrakBankingContext();
            List<TBL_RAC_DETAIL> details = new List<TBL_RAC_DETAIL>();
            foreach (var definition in definitions)
            {
                var submission = rac.form.FirstOrDefault(x => x.criteriaId == definition.RACDEFINITIONID);
                var detail = new TBL_RAC_DETAIL
                {
                    RACDEFINITIONID = definition.RACDEFINITIONID,
                    OPERATIONID = operationId,
                    TARGETID = targetId,
                    ACTUALVALUE = submission.value ?? "0",
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now,

                    //CHECKLISTSTATUS = 0,
                    //CHECKLISTSTATUS2 = 0,
                    //CHECKLISTSTATUS3 = 0
                };
                details.Add(detail);
            }

            context.TBL_RAC_DETAIL.AddRange(details);
            var result = context.SaveChanges();
            return details;
        }

        private List<TBL_EXCEPTIONAL_RAC_DETAIL> saveExceptionalRacoptions(List<TBL_RAC_DEFINITION> definitions, RacInformationViewModel rac, int operationId, int targetId, int staffId)
        {
            List<TBL_EXCEPTIONAL_RAC_DETAIL> details = new List<TBL_EXCEPTIONAL_RAC_DETAIL>();
            foreach (var definition in definitions)
            {
                var submission = rac.form.FirstOrDefault(x => x.criteriaId == definition.RACDEFINITIONID);
                var detail = new TBL_EXCEPTIONAL_RAC_DETAIL
                {
                    RACDEFINITIONID = definition.RACDEFINITIONID,
                    OPERATIONID = operationId,
                    TARGETID = targetId,
                    ACTUALVALUE = submission.value ?? "0",
                    CREATEDBY = staffId,
                    DATETIMECREATED = DateTime.Now,

                    //CHECKLISTSTATUS = 0,
                    //CHECKLISTSTATUS2 = 0,
                    //CHECKLISTSTATUS3 = 0
                };
                details.Add(detail);
            }

            context.TBL_EXCEPTIONAL_RAC_DETAIL.AddRange(details);
            var result = context.SaveChanges();
            return details;
        }

        private bool ValidRacSubmission(TBL_RAC_DEFINITION definition, string value, int operationId, int targetId)
        {
            //if (definition.ISREQUIRED == false) return true;

            //if (definition.REQUIREUPLOAD)
            //{
            //    var docContext = new Entities.DocumentModels.FinTrakBankingDocumentsContext();
            //    if (!docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
            //            && x.OPERATIONID == definition.OPERATIONID
            //            && x.TARGETID == targetId
            //    ).Any())
            //        return false;
            //}

            int integerConversion;
            int? integerValue = null;
            decimal? decimalValue = null;

            // text
            // numeric
            // select
            // radio
            // textarea


            switch (definition.RACINPUTTYPEID)
            {
                case 1:
                    if (!String.IsNullOrEmpty(value)) { return true; }
                    break;
                case 2:
                    if (value != null) decimalValue = decimal.Parse(value);
                    else decimalValue = 0;
                    break;
                case 3:
                    if (value != null)
                    {
                        int.TryParse(value, out integerConversion);
                        integerValue = integerConversion;
                    }
                    break;
                case 4:
                    if (value != null)
                    {
                        int.TryParse(value, out integerConversion);
                        integerValue = integerConversion;
                    }
                    break;
                case 5:
                    if (!String.IsNullOrEmpty(value)) return true;
                    break;
            }

            /*
            1   Equal To
            2   Greater Than
            3   Greater Than or Equal To
            4   Less Than
            5   Less Than or Equal To
            6   Not Equal To
            7   Boundary (Min & Max)
            */
            /*
            DEFINEDFUNCTIONID -- actual

            RACOPTIONID --
            CONTROLOPTIONID -- definition

            CONTROLAMOUNT --

            CONDITIONALOPERATORID --
            */

            if (integerValue != null) // selects
            {

                var optionItem = context.TBL_RAC_OPTION_ITEM.FirstOrDefault(x => x.RACOPTIONITEMID == definition.CONTROLOPTIONID);
                if (optionItem != null) {
                    if (integerValue == optionItem.KEY) return true; }
                //return false;
                return true; //changed to true for IBL RAC
            }
            else if (decimalValue != null) // amount
            {
                switch (definition.CONDITIONALOPERATORID)
                {
                    case 1:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue == definition.CONTROLAMOUNT;
                        else return decimalValue == ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 2:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue != definition.CONTROLAMOUNT;
                        else return decimalValue > ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 3:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue > definition.CONTROLAMOUNT;
                        else return decimalValue >= ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 4:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue >= definition.CONTROLAMOUNT;
                        else return decimalValue < ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 5:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue < definition.CONTROLAMOUNT;
                        else return decimalValue <= ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 6:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue <= definition.CONTROLAMOUNT;
                        else return decimalValue != ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 7:
                        if (definition.DEFINEDFUNCTIONID == 1) return (decimalValue >= definition.CONTROLAMOUNT && decimalValue <= definition.CONTROLAMOUNTMAX);
                        else return decimalValue != ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    default:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue == definition.CONTROLAMOUNT;
                        else return decimalValue == ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                }
            }
            else
            {
                return !String.IsNullOrEmpty(value);
            }
        }

        private bool ValidExceptionalRacSubmission(TBL_RAC_DEFINITION definition, string value, int operationId, int targetId)
        {
            int integerConversion;
            int? integerValue = null;
            decimal? decimalValue = null;

            switch (definition.RACINPUTTYPEID)
            {
                case 1:
                    if (!String.IsNullOrEmpty(value)) { return true; }
                    break;
                case 2:
                    if (value != null) { decimalValue = decimal.Parse(value); return true; }
                    else decimalValue = 0;
                    break;
                case 3:
                    if (value != null)
                    {
                        int.TryParse(value, out integerConversion);
                        integerValue = integerConversion;
                    }
                    break;
                case 4:
                    if (value != null)
                    {
                        int.TryParse(value, out integerConversion);
                        integerValue = integerConversion;
                    }
                    break;
                case 5:
                    if (!String.IsNullOrEmpty(value)) return true;
                    break;
            }

          
            if (integerValue != null) // selects
            {
                var optionItem = context.TBL_RAC_OPTION_ITEM.FirstOrDefault(x => x.RACOPTIONITEMID == definition.CONTROLOPTIONID);
                if (optionItem != null) { if (integerValue == optionItem.KEY) return true; }
                return false;
            }
            else if (decimalValue != null) // amount
            {
                switch (definition.CONDITIONALOPERATORID)
                {
                    case 1:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue == definition.CONTROLAMOUNT;
                        else return decimalValue == ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 2:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue != definition.CONTROLAMOUNT;
                        else return decimalValue > ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 3:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue > definition.CONTROLAMOUNT;
                        else return decimalValue >= ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 4:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue >= definition.CONTROLAMOUNT;
                        else return decimalValue < ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 5:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue < definition.CONTROLAMOUNT;
                        else return decimalValue <= ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 6:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue <= definition.CONTROLAMOUNT;
                        else return decimalValue != ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    case 7:
                        if (definition.DEFINEDFUNCTIONID == 1) return (decimalValue >= definition.CONTROLAMOUNT && decimalValue <= definition.CONTROLAMOUNTMAX);
                        else return decimalValue != ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                    default:
                        if (definition.DEFINEDFUNCTIONID == 1) return decimalValue == definition.CONTROLAMOUNT;
                        else return decimalValue == ReturnNotImplementedException(definition.DEFINEDFUNCTIONID); // TODO...
                }
            }
            else
            {
                return !String.IsNullOrEmpty(value);
            }
        }

        private decimal? ReturnNotImplementedException(int DEFINEDFUNCTIONID) // TODO...
        {
            throw new NotImplementedException();
        }

        private bool isProductBasedWorkflowApplicable = false;
        private bool isProductClassBasedWorkflowApplicable = false;
        private void determineWorkFlowAdjustment(int productId, int productClassId)
        {
            if (context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.PRODUCTID == productId
                                                            && x.OPERATIONID == (short)OperationsEnum.CreditAppraisal
                                                            && x.DELETED == false).Any()) isProductBasedWorkflowApplicable = true;
            if (context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.PRODUCTCLASSID == productClassId
                                                            && x.OPERATIONID == (short)OperationsEnum.CreditAppraisal
                                                            && x.DELETED == false).Any()) isProductClassBasedWorkflowApplicable = true;
        }

        private bool GetCustomerIsRelatedParty(int customerId)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);
            var customerGroup = new TBL_CUSTOMER_GROUP();
            if (customer == null)
            {
                customerGroup = context.TBL_CUSTOMER_GROUP.Find(customerId);
                if (customerGroup != null)
                {
                    var mappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(m => m.DELETED != true && m.CUSTOMERGROUPID == customerGroup.CUSTOMERGROUPID);
                    var customers = new List<TBL_CUSTOMER>();
                    foreach (var map in mappings)
                    {
                        customers.Add(map.TBL_CUSTOMER);
                    }

                    if (customers.Exists(c => c.ISREALATEDPARTY == true))
                    {
                        return true;
                    } else
                    {
                        return false;
                    }
                }
            }
            return customer.ISREALATEDPARTY;
        }

        private bool GetCustomerIsPoliticallyExposed(int customerId)
        {
            var customer = context.TBL_CUSTOMER.Find(customerId);
            var customerGroup = new TBL_CUSTOMER_GROUP();
            if (customer == null)
            {
                customerGroup = context.TBL_CUSTOMER_GROUP.Find(customerId);
                if (customerGroup != null)
                {
                    var mappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(m => m.DELETED != true && m.CUSTOMERGROUPID == customerGroup.CUSTOMERGROUPID);
                    var customers = new List<TBL_CUSTOMER>();
                    foreach (var map in mappings)
                    {
                        customers.Add(map.TBL_CUSTOMER);
                    }

                    if (customers.Exists(c => c.ISPOLITICALLYEXPOSED == true))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return customer.ISPOLITICALLYEXPOSED;
        }

        private void AddExceptionalLoanApplication(LoanApplicationViewModel loan)
        {
            short productClassProcessId = 0;
            short? productClassId = null;
            isGroupLoan = false;
            response = 0;
            //int loanId = 0;
            var proposedProductId = loan.LoanApplicationDetail.FirstOrDefault().proposedProductId;
            workflowProductId = GetWorkflowProductId(proposedProductId);

            if (loan.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                isGroupLoan = true;
            }

            int? casaAccountId = null;
            //string refNumber = GenerateLoanReference(loan.customerId.Value);GetRefrenceNumber
            string loanAppReference = GetRefrenceNumber();

            if (loan.customerAccount != "N/A")
            {
                casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);
            }

            var dat = context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == loan.productClassId).FirstOrDefault();

            if (dat != null)
            {
                productClassId = loan.productClassId;
                productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;

            }
          
            if (loan.flowchangeId != null && loan.flowchangeId > 0)
            {
                var newWorkflowBaseRecord = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(loan.flowchangeId);
                if (newWorkflowBaseRecord != null)
                {
                    loan.exclusiveOperationId = newWorkflowBaseRecord.OPERATIONID;
                }
            }

            exceptionalLoanData = new TBL_EXCEPTIONAL_LOAN_APPLICATION()
            {
                REQUIRECOLLATERAL = loan.requireCollateral,
                //TOTALEXPOSUREAMOUNT = totalAmount,
                PRODUCTCLASSID = productClassId,
                
                APPLICATIONREFERENCENUMBER = loanAppReference, // loan.applicationReferenceNumber,
                PRODUCT_CLASS_PROCESSID = productClassProcessId,
                COMPANYID = loan.companyId,
                BRANCHID = (short)loan.branchId,
                RELATIONSHIPOFFICERID = loan.createdBy,
                RELATIONSHIPMANAGERID = loan.createdBy,
                MISCODE = loan.misCode,
                TEAMMISCODE = loan.teamMisCode,
                INTERESTRATE = loan.interestRate,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                LOANINFORMATION = loan.loanInformation,
                ISRELATEDPARTY = loan.isRelatedParty,
                ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed,
                OWNEDBY = (int)loan.createdBy,
                CREATEDBY = (int)loan.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                CUSTOMERGROUPID = loan.customerGroupId,
                CASAACCOUNTID = loan.casaAccountId,
                APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONAMOUNT = loan.applicationAmount,
                APPLICATIONTENOR = loan.proposedTenor,
                ISINVESTMENTGRADE = loan.isInvestmentGrade,
                CAPREGIONID = loan.regionId,
                REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                LOANTERMSHEETID = loan.loantermSheetId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                FLOWCHANGEID = loan.flowchangeId,
                OPERATIONID = ((loan.exclusiveOperationId == null) || (loan.exclusiveOperationId == 0)) ? (int)OperationsEnum.CreditAppraisal : (int)loan.exclusiveOperationId,
                LOANAPPLICATIONTYPEID = loan.loanTypeId,
                COLLATERALDETAIL = loan.collateralDetail,
                ISADHOCAPPLICATION = loan.isadhocapplication,
                LOANSWITHOTHERS = loan.loansWithOthers,
                OWNERSHIPSTRUCTURE = loan.ownershipStructure,
                LOANAPPROVEDLIMITID = loan.loanApprovedLimitId,
                PRODUCTID = proposedProductId,
                //PRODUCTID = workflowProductId, 
                ISEMPLOYERRELATED = loan.isEmployerRelated,
                RELATEDEMPLOYERID = loan.relatedEmployerId
            };

            //loanData.TOTALEXPOSUREAMOUNT = loan.LoanApplicationDetail.Sum(x => x.exchangeAmount) + (GetExposures(loanData).Sum(e => e.outstandingsLcy));

            if (isGroupLoan)
            {
                exceptionalLoanData.CUSTOMERGROUPID = loan.customerGroupId;
                exceptionalLoanData.CUSTOMERID = null;
                exceptionalLoanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerGroupId);
                exceptionalLoanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerGroupId);
            }
            else
            {
                exceptionalLoanData.CUSTOMERID = loan.customerId;
                exceptionalLoanData.CUSTOMERGROUPID = null;
                exceptionalLoanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerId);
                exceptionalLoanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerId);
            }
            if (loan.loanPreliminaryEvaluationId != null && loan.loanPreliminaryEvaluationId != 0)
            {
                var pen = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loan.loanPreliminaryEvaluationId);
                pen.SENTFORLOANAPPLICATION = true;
            }

            context.TBL_EXCEPTIONAL_LOAN_APPLICATION.Add(exceptionalLoanData);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = loan.createdBy,
                BRANCHID = (short)loan.userBranchId,
                DETAIL = $"Saved exceptional loan with reference number: {loan.applicationReferenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = loan.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loan.loanApplicationId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            context.TBL_AUDIT.Add(audit);
            context.SaveChanges();
            //this.auditTrail.AddAuditTrail(audit);
        }

        private void AddloanApplicationSub(LoanApplicationViewModel loan)
        {
            try
            {
                short productClassProcessId = 0;
                short? productClassId = null;
                isGroupLoan = false;
                response = 0;
                int loanId = 0;
                var proposedProductId = loan.LoanApplicationDetail.FirstOrDefault().proposedProductId;

                // ValidateLoanApplicationLimits(loan); // init only
                workflowProductId = GetWorkflowProductId(proposedProductId);

                if (loan.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
                {
                    isGroupLoan = true;
                }
                int? casaAccountId = null;
                string refNumber = GenerateLoanReference(loan.customerId.Value);
                if (loan.customerAccount != "N/A")
                {
                    casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);
                }

                var dat = context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == loan.productClassId).FirstOrDefault();
                if (dat != null)
                {
                    productClassId = loan.productClassId;
                    productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;
                    //if (dat.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased)
                    //{
                    //    //productClassId = null;
                    //    //if(loan.productId == (short))
                    //    productClassId = loan.productClassId;
                    //    productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;
                    //    //if(dat.TBL_PRODUCT.p)
                    //}
                    //if (dat.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.ProductBased)
                    //{
                    //    productClassId = loan.productClassId;
                    //    productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;
                    //}

                }
                //decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + (loan.LoanApplicationDetail.Sum(x => x.exchangeAmount));
                //var loanStatusId = (short)LoanStatusEnum.Inactive;

                if (loan.flowchangeId != null && loan.flowchangeId > 0)
                {
                    var newWorkflowBaseRecord = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(loan.flowchangeId);
                    if (newWorkflowBaseRecord != null)
                    {
                        loan.exclusiveOperationId = newWorkflowBaseRecord.OPERATIONID;
                    }
                }


                loanData = new TBL_LOAN_APPLICATION
                {
                    REQUIRECOLLATERAL = loan.requireCollateral,
                    //TOTALEXPOSUREAMOUNT = totalAmount,
                    PRODUCTCLASSID = productClassId,
                    APPLICATIONREFERENCENUMBER = loan.applicationReferenceNumber,
                    PRODUCT_CLASS_PROCESSID = productClassProcessId,
                    COMPANYID = loan.companyId,
                    BRANCHID = (short)loan.branchId,
                    RELATIONSHIPOFFICERID = loan.createdBy,
                    RELATIONSHIPMANAGERID = loan.createdBy,
                    MISCODE = loan.misCode,
                    TEAMMISCODE = loan.teamMisCode,
                    INTERESTRATE = loan.interestRate,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    LOANINFORMATION = loan.loanInformation,
                    ISRELATEDPARTY = loan.isRelatedParty,
                    ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed,
                    OWNEDBY = (int)loan.createdBy,
                    CREATEDBY = (int)loan.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    CUSTOMERGROUPID = loan.customerGroupId,
                    CASAACCOUNTID = loan.casaAccountId,
                    APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    APPLICATIONAMOUNT = loan.applicationAmount,
                    APPLICATIONTENOR = loan.proposedTenor,
                    ISINVESTMENTGRADE = loan.isInvestmentGrade,
                    CAPREGIONID = loan.regionId,
                    REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                    LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                    LOANTERMSHEETID = loan.loantermSheetId,
                    CUSTOMERID = loan.customerId,
                    SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                    FLOWCHANGEID = loan.flowchangeId,
                    OPERATIONID = ((loan.exclusiveOperationId == null) || (loan.exclusiveOperationId == 0)) ? (int)OperationsEnum.CreditAppraisal : (int)loan.exclusiveOperationId,
                    LOANAPPLICATIONTYPEID = loan.loanTypeId,
                    COLLATERALDETAIL = loan.collateralDetail,
                    ISADHOCAPPLICATION = loan.isadhocapplication,
                    LOANSWITHOTHERS = loan.loansWithOthers,
                    OWNERSHIPSTRUCTURE = loan.ownershipStructure,
                    LOANAPPROVEDLIMITID = loan.loanApprovedLimitId,
                    PRODUCTID = workflowProductId,
                    ISEMPLOYERRELATED = loan.isEmployerRelated,
                    RELATEDEMPLOYERID = loan.relatedEmployerId,
                    TERMSHEETID = loan.loantermSheetId,
                    IBLREQUEST = loan.iblRequest
                };
                loanData.TOTALEXPOSUREAMOUNT = loan.LoanApplicationDetail.Sum(x => x.exchangeAmount) + (GetExposures(loanData).Sum(e => e.outstandingsLcy));

                if (isGroupLoan)
                {
                    loanData.CUSTOMERGROUPID = loan.customerGroupId;
                    loanData.CUSTOMERID = null;
                    loanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerGroupId);
                    loanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerGroupId);
                }
                else
                {
                    loanData.CUSTOMERID = loan.customerId;
                    loanData.CUSTOMERGROUPID = null;
                    loanData.ISRELATEDPARTY = GetCustomerIsRelatedParty((int)loan.customerId);
                    loanData.ISPOLITICALLYEXPOSED = GetCustomerIsPoliticallyExposed((int)loan.customerId);
                }
                if (loan.loanPreliminaryEvaluationId != null && loan.loanPreliminaryEvaluationId != 0)
                {
                    var pen = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loan.loanPreliminaryEvaluationId);
                    pen.SENTFORLOANAPPLICATION = true;
                }


                context.TBL_LOAN_APPLICATION.Add(loanData);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                    STAFFID = loan.createdBy,
                    BRANCHID = (short)loan.userBranchId,
                    DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = loan.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = loan.loanApplicationId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),

                };

                this.auditTrail.AddAuditTrail(audit);
            }catch(Exception e)
            {
                throw e;
            }
        }

        private bool UpdateLoanApplicationDetail(LoanApplicationViewModel loan)
        {
            UpdateLoanApplication(loan); // update main

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId && x.DELETED == false);
            var update = loan.LoanApplicationDetail.SingleOrDefault();
            if (update == null) throw new SecureException("Sequence contain not single! " + loan.LoanApplicationDetail.Count());

            if (update.productFees != null)
            {
                if (update.productFees.Count > 0)
                {
                    UpdateLoanDetailFees(update.productFees, loan.loanApplicationDetailId, loan.createdBy);
                }
            }
            // LEFT TO RIGHT MAPPING
            detail.SUBSECTORID = update.subSectorId;
            detail.EXCHANGERATE = update.exchangeRate;
            detail.PROPOSEDAMOUNT = update.proposedAmount;
            detail.APPROVEDAMOUNT = update.proposedAmount;
            detail.PROPOSEDINTERESTRATE = (double)update.proposedInterestRate;
            detail.APPROVEDINTERESTRATE = (double)update.proposedInterestRate;
            detail.PROPOSEDPRODUCTID = update.proposedProductId;
            detail.APPROVEDPRODUCTID = update.proposedProductId;
            detail.PROPOSEDTENOR = ConvertTenorToDays(update.proposedTenor, update.tenorModeId);
            detail.APPROVEDTENOR = ConvertTenorToDays(update.proposedTenor, update.tenorModeId);
            detail.REPAYMENTSCHEDULEID = update.repaymentScheduleId;
            detail.REPAYMENTTERMS = update.repaymentTerm;
            detail.LOANPURPOSE = update.loanPurpose;
            detail.PRODUCTPRICEINDEXID = update.productPriceIndexId;
            detail.PRODUCTPRICEINDEXRATE = update.productPriceIndexRate;
            detail.CASAACCOUNTID = update.casaAccountId;
            detail.OPERATINGCASAACCOUNTID = update.operatingCasaAccountId;
            detail.EQUITYCASAACCOUNTID = update.equityCasaAccountId;
            detail.CURRENCYID = update.currencyId;
            detail.TENORFREQUENCYTYPEID = update.tenorModeId;
            detail.ISTAKEOVERAPPLICATION = update.isTakeOverApplication;
            detail.LOANDETAILREVIEWTYPEID = update.loanDetailReviewTypeId;
            detail.ISMORATORIUM = update.isMoratorium;
            detail.INTERESTREPAYMENT = update.interestRepayment;
            detail.INTERESTREPAYMENTID = update.interestRepaymentId;
            detail.MORATORIUM = update.moratorium;
            detail.APPROVEDLINELIMIT = update.approvedLineLimit;
            detail.DATETIMEUPDATED = DateTime.Now;
            detail.LASTUPDATEDBY = loan.createdBy;
            detail.APPROVEDTRADECYCLEID = update.approvedTradeCycleId;
            detail.OLDAPPLICATIONREFFORRENEWAL = update.oldApplicationRefForRenewal;
            detail.PROPERTYTYPEID = update.propertyTypeId;
            detail.PROPERTYTITLE = update.propertyTitle;
            detail.PROPERTYPRICE = update.propertyPrice;
            detail.DOWNPAYMENT = update.downPayment;

            var currentProduct = context.TBL_PRODUCT.Find(detail.APPROVEDPRODUCTID);

            var productClassId = currentProduct.PRODUCTCLASSID;

            if (update.repaymentScheduleId <= 0 && (currentProduct.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability))
            {
                throw new SecureException("Please select a repayment pattern for the product " + update.productName);
            }


            if (productClassId == (int)ProductClassEnum.InvoiceDiscountingFacility && update.invoiceDetails.Any())
            {
                var invoice = context.TBL_LOAN_APPLICATION_DETL_INV.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
                var invoiceUpdate = update.invoiceDetails.FirstOrDefault();
                invoice.PRINCIPALID = invoiceUpdate.principalId;
                invoice.INVOICE_CURRENCYID = invoiceUpdate.invoiceCurrencyId;
                invoice.INVOICE_AMOUNT = invoiceUpdate.invoiceAmount;
                invoice.CONTRACT_STARTDATE = invoiceUpdate.contractStartDate;
                invoice.CONTRACT_ENDDATE = invoiceUpdate.contractEndDate;
                invoice.CONTRACTNO = invoiceUpdate.contractNo;
                invoice.PURCHASEORDERNUMBER = invoiceUpdate.purchaseOrderNumber;
                invoice.CERTIFICATENO = invoiceUpdate.certificateNumber;
                invoice.INVOICENO = invoiceUpdate.invoiceNo;
                invoice.INVOICE_DATE = invoiceUpdate.invoiceDate;
                invoice.INVOICE_AMOUNT = invoiceUpdate.invoiceAmount;
                invoice.REVALIDATED = invoiceUpdate.reValidated;
                invoice.ENTRYSHEETNUMBER = invoiceUpdate.entrySheetNumber;
            }

            if (productClassId == (int)ProductClassEnum.BondAndGuarantees)
            {
                var bond = context.TBL_LOAN_APPLICATION_DETL_BG.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
                var bondUpdate = update.bondDetails;
                bond.PRINCIPALID = bondUpdate.principalId;
                bond.AMOUNT = bondUpdate.bondAmount;
                bond.CURRENCYID = bondUpdate.bondCurrencyId;
                bond.CONTRACT_STARTDATE = bondUpdate.contractStartDate;
                bond.CONTRACT_ENDDATE = bondUpdate.contractEndDate;
                bond.ISTENORED = bondUpdate.isTenored;
                bond.ISBANKFORMAT = bondUpdate.isBankFormat;
                bond.CASAACCOUNTID = bondUpdate.casaAccountId;
                bond.REFERENCENO = bondUpdate.referenceNo;
            }

            //else
            //{
            //    throw new SecureException("No fee is defined for this product(s)");
            //}

            //if (productClassId == (int)ProductClassEnum.FirstEdu)
            //{
            //    var edu = context.TBL_LOAN_APPLICATION_DETL_EDU.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
            //    var eduUpdate = update.educationLoan;
            //    edu.NUMBER_OF_STUDENTS = eduUpdate.numberOfStudent;
            //    edu.AVERAGE_SCHOOL_FEES = eduUpdate.averageSchoolFees;
            //    edu.TOTAL_PREVIOUS_TERM_SCHOL_FEES = eduUpdate.totalPreviousTermSchoolFees;
            //}

            //if (productClassId == (int)ProductClassEnum.FirstTrader)
            //{
            //    var trader = context.TBL_LOAN_APPLICATION_DETL_TRA.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
            //    var traderUpdate = update.traderLoan;
            //    trader.MARKETID = traderUpdate.marketId;
            //    trader.AVERAGE_MONTHLY_TURNOVER = traderUpdate.averageMonthlyTurnover;
            //    trader.SOLDITEMS = traderUpdate.soldItems;
            //}

            if (context.SaveChanges() > 0)
            {
                var loanApplicationdetailIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == detail.LOANAPPLICATIONDETAILID).Select(l => l.LOANAPPLICATIONDETAILID).ToList();
                var facilityStampDutyIds = new List<int>();
                if (loanApplicationdetailIds.Count > 0)
                {
                    foreach (var detailId in loanApplicationdetailIds)
                    {
                        var stampDutyId = context.TBL_FACILITY_STAMP_DUTY.Where(s => s.LOANAPPLICATIONDETAILID == detailId).FirstOrDefault();
                        if(stampDutyId != null) facilityStampDutyIds.Add(stampDutyId.FACILITYSTAMPDUTYID);
                    }
                    if (facilityStampDutyIds.Count > 0)
                    {
                        foreach (var dutyId in facilityStampDutyIds)
                        {
                            var facilityDuty = context.TBL_FACILITY_STAMP_DUTY.Find(dutyId);
                            if (facilityDuty != null)
                            {
                                if (detail.PROPOSEDTENOR >= 360)
                                {
                                    facilityDuty.DELETED = true;
                                }
                                else
                                {
                                    facilityDuty.DELETED = false;
                                }

                            }

                        }
                    }
                }


                var racDetail = context.TBL_RAC_DETAIL.Where(r => r.TARGETID == detail.LOANAPPLICATIONDETAILID).ToList();
                if (loan.rac != null && racDetail.Count() == 0)
                {
                    var recResponse = SaveRac(loan.rac, loan.rac?.operationId, (int)loan.rac.productId, loan.rac.productClassId, detail.LOANAPPLICATIONDETAILID, loan.createdBy, detail.LOANAPPLICATIONID);
                    if (recResponse != null) return true;
                }
            }
            else
            {
                throw new SecureException("Nothing was updated!");
            }
            return true;
        }

        private int ConvertTenorToDays(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId) // UPDATED
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = proposedTenor * 30; break;
                case (int)TenorMode.Yearly: tenor = proposedTenor * 365; break;
            }
            return tenor;
        }

        private int ConvertTenorDaysToTenor(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId) // UPDATED
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = proposedTenor / 30; break;
                case (int)TenorMode.Yearly: tenor = proposedTenor / 365; break;
            }
            return tenor;
        }

        private void UpdateLoanApplication(LoanApplicationViewModel loan)
        {
            var application = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loan.loanApplicationId && c.DELETED == false).ToList();

            //decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + application.Sum(a => a.PROPOSEDAMOUNT * (decimal)a.EXCHANGERATE);
            var facility = application.FirstOrDefault();
            decimal totalApplicationAmount = 0; //loan.applicationAmount;
            foreach (var item in application)
            {
                var exchangeValue = (item.PROPOSEDAMOUNT * (decimal)item.EXCHANGERATE);
                totalApplicationAmount = totalApplicationAmount + exchangeValue;
            }

            this.loanData = context.TBL_LOAN_APPLICATION.Find(loan.loanApplicationId);
            decimal totalAmount = application.Sum(a => a.PROPOSEDAMOUNT * (decimal)a.EXCHANGERATE) + (GetExposures(loanData).Sum(e => e.outstandingsLcy));
            this.loanData.REQUIRECOLLATERAL = loan.requireCollateral;
            this.loanData.TOTALEXPOSUREAMOUNT = totalAmount;
            this.loanData.INTERESTRATE = loan.interestRate;
            //this.loanData.APPLICATIONDATE = genSetup.GetApplicationDate();
            this.loanData.LOANINFORMATION = loan.loanInformation;
            this.loanData.ISRELATEDPARTY = loan.isRelatedParty;
            this.loanData.ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed;
            this.loanData.LASTUPDATEDBY = (int)loan.createdBy;
            this.loanData.DATETIMEUPDATED = DateTime.Now;
            this.loanData.SYSTEMDATETIME = DateTime.Now;
            this.loanData.CASAACCOUNTID = loan.casaAccountId;
            this.loanData.APPLICATIONAMOUNT = totalApplicationAmount;
            this.loanData.APPLICATIONTENOR = application.Max(c => c.PROPOSEDTENOR);
            this.loanData.COLLATERALDETAIL = loan.collateralDetail;
            this.loanData.CAPREGIONID = loan.regionId;
            this.loanData.REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId;
            this.loanData.LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId;
            this.loanData.LOANTERMSHEETID = loan.loantermSheetId;
            this.loanData.ISADHOCAPPLICATION = loan.isadhocapplication;
            this.loanData.LOANAPPROVEDLIMITID = loan.loanApprovedLimitId;
            this.loanData.LOANSWITHOTHERS = loan.loansWithOthers;
            this.loanData.OWNERSHIPSTRUCTURE = loan.ownershipStructure;
            this.loanData.PRODUCTID = GetWorkflowProductId(facility.APPROVEDPRODUCTID);
            this.loanData.PRODUCTCLASSID = context.TBL_PRODUCT.FirstOrDefault(p => facility.APPROVEDPRODUCTID == p.PRODUCTID).PRODUCTCLASSID;
            this.loanData.PRODUCT_CLASS_PROCESSID = context.TBL_PRODUCT.FirstOrDefault(p => facility.APPROVEDPRODUCTID == p.PRODUCTID).TBL_PRODUCT_CLASS.PRODUCT_CLASS_PROCESSID;
            this.loanData.ISEMPLOYERRELATED = loan.isEmployerRelated;
            this.loanData.RELATEDEMPLOYERID = loan.relatedEmployerId;
            //this.loanData.IBLREQUEST = loan.iblRequest;

            if (loan.LoanApplicationDetail.Count > 0)
            {
                var exclusiveOperationId = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == loan.flowchangeId)?.OPERATIONID;
                this.loanData.OPERATIONID = ((exclusiveOperationId == null) || (exclusiveOperationId == 0)) ? (int)OperationsEnum.CreditAppraisal : (int)exclusiveOperationId;
                this.loanData.FLOWCHANGEID = loan.flowchangeId;
            }
        }

        private void TradderLoan(TraderLoanViewModel entity, int loanApplicationId, int createdBy)
        {
            var data = new TBL_LOAN_APPLICATION_DETL_TRA()
            {
                AVERAGE_MONTHLY_TURNOVER = entity.averageMonthlyTurnover,
                MARKETID = entity.marketId,
                CREATEDBY = createdBy,
                SOLDITEMS = entity.soldItems,
                LOANAPPLICATIONDETAILID = loanApplicationId,
                DATETIMECREATED = DateTime.Now

            };
            context.TBL_LOAN_APPLICATION_DETL_TRA.Add(data);
        }

        private void EducationLoan(EducationLoanViewModel entity, int loanApplicationDetailsId, int createdBy)
        {
            var data = new TBL_LOAN_APPLICATION_DETL_EDU()
            {
                AVERAGE_SCHOOL_FEES = entity.averageSchoolFees,
                LOANAPPLICATIONDETAILID = loanApplicationDetailsId,
                NUMBER_OF_STUDENTS = entity.numberOfStudent,
                TOTAL_PREVIOUS_TERM_SCHOL_FEES = entity.schoolFeesCollected,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now
            };
            context.TBL_LOAN_APPLICATION_DETL_EDU.Add(data);

        }

        private void InvoiceDetails(List<InvoiceDetailViewModel> entity, int createdBy, int loanApplicationId, int customerId)
        {
            var data = entity.Select(c => new TBL_LOAN_APPLICATION_DETL_INV()
            {
                CONTRACT_ENDDATE = c.contractEndDate,
                CONTRACT_STARTDATE = c.contractStartDate,
                INVOICENO = c.invoiceNo,
                CONTRACTNO = c.contractNo,
                INVOICE_AMOUNT = c.invoiceAmount,
                INVOICE_CURRENCYID = c.invoiceCurrencyId,
                INVOICE_DATE = c.invoiceDate,
                LOANAPPLICATIONDETAILID = c.loanApplicationDetailId,
                PRINCIPALID = c.principalId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                PURCHASEORDERNUMBER = c.purchaseOrderNumber,
                CERTIFICATENO = c.certificateNumber,
                REVALIDATED = c.reValidated,
                ENTRYSHEETNUMBER = c.entrySheetNumber

            }).ToList();

            //foreach (var item in data) // invoice reuse check
            //{

            //        if (context.TBL_LOAN_APPLICATION_DETL_INV.Where(x =>
            //            x.INVOICENO == item.INVOICENO &&
            //            x.TBL_LOAN_APPLICATION_DETAIL.CUSTOMERID == customerId &&
            //            x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID != loanApplicationId
            //            ).Any())
            //        {
            //            throw new SecureException("This invoice number have been used in another application!");
            //        }

            //}

            context.TBL_LOAN_APPLICATION_DETL_INV.AddRange(data);
        }

        private void AddExceptionalInvoiceDetails(List<InvoiceDetailViewModel> entity, int createdBy, int loanApplicationId, int customerId)
        {
            var data = entity.Select(c => new TBL_EXCEPTIONAL_LOAN_APPL_DETL_INV()
            {
                CONTRACT_ENDDATE = c.contractEndDate,
                CONTRACT_STARTDATE = c.contractStartDate,
                INVOICENO = c.invoiceNo,
                CONTRACTNO = c.contractNo,
                INVOICE_AMOUNT = c.invoiceAmount,
                INVOICE_CURRENCYID = c.invoiceCurrencyId,
                INVOICE_DATE = c.invoiceDate,
                EXCEPTIONALLOANAPPLDETAILID = c.loanApplicationDetailId,
                PRINCIPALID = c.principalId,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                PURCHASEORDERNUMBER = c.purchaseOrderNumber,
                CERTIFICATENO = c.certificateNumber,
                REVALIDATED = c.reValidated,
                ENTRYSHEETNUMBER = c.entrySheetNumber

            }).ToList();

            context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_INV.AddRange(data);
        }

        public bool ValidateDuplicateLoanApplication(LoanApplicationViewModel loan)
        {
            var createdBy = loan.createdBy;
            var a = loan.LoanApplicationDetail.FirstOrDefault();

            if (a.repaymentScheduleId <= 0)
            {
                throw new SecureException("Please select a repayment pattern");
            }

            if (a.proposedTenor == 0)
            {
                throw new SecureException("Tenor can not be ZERO (0)");
            }
            int applicationId = this.loanData == null ? 0 : this.loanData.LOANAPPLICATIONID; // ?
            int tenor = ConvertTenorToDays(a.proposedTenor, a.tenorModeId);

            var data = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDAMOUNT = a.proposedAmount,
                APPROVEDINTERESTRATE = (double)a.proposedInterestRate,
                APPROVEDPRODUCTID = a.proposedProductId,
                APPROVEDTENOR = tenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                EXCHANGERATE = a.exchangeRate,
                CURRENCYID = a.currencyId,
                CUSTOMERID = a.customerId,
                LOANAPPLICATIONID = applicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,

                EQUITYCASAACCOUNTID = a.equityCasaAccountId,
                EQUITYAMOUNT = a.equityAmount,

                PROPOSEDAMOUNT = a.proposedAmount,
                PROPOSEDINTERESTRATE = (int)a.proposedInterestRate,
                PROPOSEDPRODUCTID = a.proposedProductId,
                PROPOSEDTENOR = tenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),
                DELETED = false,
                SUBSECTORID = a.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = a.loanPurpose,
                CASAACCOUNTID = a.casaAccountId,
                OPERATINGCASAACCOUNTID = a.operatingCasaAccountId,
                REPAYMENTSCHEDULEID = a.repaymentScheduleId,
                REPAYMENTTERMS = a.repaymentTerm,
                CRMSFUNDINGSOURCEID = a.crmsFundingSourceId,
                CRMSREPAYMENTSOURCEID = a.crmsPaymentSourceId,
                CRMSFUNDINGSOURCECATEGORY = a.crmsFundingSourceCategory,
                CRMS_ECCI_NUMBER = a.crms_ECCI_Number,
                FIELD1 = a.fieldOne,
                FIELD2 = a.fieldTwo,
                FIELD3 = a.fieldThree,
                PRODUCTPRICEINDEXID = a.productPriceIndexId,
                PRODUCTPRICEINDEXRATE = a.productPriceIndexRate,
                TENORFREQUENCYTYPEID = a.tenorModeId,
                CRMSVALIDATED = false,
                ISTAKEOVERAPPLICATION = a.isTakeOverApplication,
                //LOANAPPLICATIONDETAILID = a.loanApplicationDetailId
            };


            var loanExist = context.TBL_LOAN_APPLICATION_DETAIL.Any(o => o.APPROVEDAMOUNT == data.APPROVEDAMOUNT
                    && o.APPROVEDINTERESTRATE == data.APPROVEDINTERESTRATE && o.APPROVEDTENOR == data.APPROVEDTENOR && o.CURRENCYID == data.CURRENCYID && o.CUSTOMERID == data.CUSTOMERID
                    && o.SUBSECTORID == data.SUBSECTORID && o.CREATEDBY == data.CREATEDBY && o.DELETED != true);

            return loanExist;
            //if (loanExist == true)
            //{
            //    return true;
            //}

            //return false;
        }

        private void AddExceptionalLoanApplicationDetail(LoanApplicationViewModel loan, string breachedLimitName)
        {
            var createdBy = loan.createdBy;
            var a = loan.LoanApplicationDetail.FirstOrDefault();
            var product = context.TBL_PRODUCT.Find(a.proposedProductId);

            if (a.repaymentScheduleId <= 0 && (product.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability))
            {
                throw new SecureException("Please select a repayment pattern");
            }

            if (a.proposedTenor == 0)
            {
                throw new SecureException("Tenor can not be ZERO (0)");
            }

            int applicationId = this.exceptionalLoanData == null ? 0 : this.exceptionalLoanData.EXCEPTIONALLOANAPPLICATIONID;
            int tenor = ConvertTenorToDays(a.proposedTenor, a.tenorModeId);

            var data = new TBL_EXCEPTIONAL_LOAN_APPL_DETAIL()
            {
                APPROVEDAMOUNT = a.proposedAmount,
                APPROVEDINTERESTRATE = (double)a.proposedInterestRate,
                APPROVEDPRODUCTID = a.proposedProductId,
                APPROVEDTENOR = tenor, 

                EXCHANGERATE = a.exchangeRate,
                CURRENCYID = a.currencyId,
                CUSTOMERID = a.customerId,
                EXCEPTIONALLOANAPPLICATIONID = applicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,

                EQUITYCASAACCOUNTID = a.equityCasaAccountId,
                EQUITYAMOUNT = a.equityAmount,

                PROPOSEDAMOUNT = a.proposedAmount,
                PROPOSEDINTERESTRATE = (int)a.proposedInterestRate,
                PROPOSEDPRODUCTID = a.proposedProductId,
                PROPOSEDTENOR = tenor, 
                DELETED = false,
                SUBSECTORID = a.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = a.loanPurpose,
                CASAACCOUNTID = a.casaAccountId,
                OPERATINGCASAACCOUNTID = a.operatingCasaAccountId,
                REPAYMENTSCHEDULEID = a.repaymentScheduleId,
                REPAYMENTTERMS = a.repaymentTerm,
                CRMSFUNDINGSOURCEID = a.crmsFundingSourceId,
                CRMSREPAYMENTSOURCEID = a.crmsPaymentSourceId,
                CRMSFUNDINGSOURCECATEGORY = a.crmsFundingSourceCategory,
                CRMS_ECCI_NUMBER = a.crms_ECCI_Number,
                FIELD1 = a.fieldOne,
                FIELD2 = a.fieldTwo,
                FIELD3 = a.fieldThree,
                PRODUCTPRICEINDEXID = a.productPriceIndexId,
                PRODUCTPRICEINDEXRATE = a.productPriceIndexRate,
                TENORFREQUENCYTYPEID = a.tenorModeId,
                CRMSVALIDATED = false,
                ISTAKEOVERAPPLICATION = a.isTakeOverApplication,
                ISLINEFACILITY = a.isLineFacility,
                LOANDETAILREVIEWTYPEID = a.loanDetailReviewTypeId,
                
                ISMORATORIUM = a.isMoratorium,
                INTERESTREPAYMENT = a.interestRepayment,
                INTERESTREPAYMENTID = a.interestRepaymentId,
                MORATORIUM = a.moratorium,
                APPROVEDLINELIMIT = a.approvedLineLimit,
                APPROVALSTATUSID = (int) ApprovalStatusEnum.Processing,
                BREACHEDLIMITNAME = breachedLimitName,
                OLDAPPLICATIONREFFORRENEWAL = a.oldApplicationRefForRenewal
            };

            context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL.Add(data);

            if (a.invoiceDetails.Any() && a.productClassId == (short)ProductClassEnum.InvoiceDiscountingFacility)
            {
                AddExceptionalInvoiceDetails(a.invoiceDetails, createdBy, applicationId, a.customerId);
            }
         
            if (a.bondDetails != null && a.productClassId == (short)ProductClassEnum.BondAndGuarantees)
            {
                AddExceptionalBondDetails(a.bondDetails, a.loanApplicationDetailId, createdBy);
            }

            if (a.syndicatedLoan != null && a.syndicatedLoan.Count > 0)
            {
                AddExceptionalSyndicatedDetails(a.syndicatedLoan, a.loanApplicationDetailId, createdBy);
            }

            if (a.productFees != null)
            {
                if (a.productFees.Count > 0)
                {
                    AddExceptionalProductFees(a.productFees, a.loanApplicationDetailId, createdBy);
                }
            }
            else
            {
                throw new SecureException("No fee is defined for this product(s)");
            }

            try
            {
                response = context.SaveChanges();

                if (response > 0)
                {

                    var recResponse = SaveExceptionalRac(loan.rac, loan.rac?.operationId, (int)loan.rac.productId, loan.rac.productClassId, data.EXCEPTIONALLOANAPPLDETAILID, loan.createdBy, data.EXCEPTIONALLOANAPPLICATIONID);
                    //if (recResponse != null) return recResponse;
                }

                var exceptional = new ExceptionalLoanViewModel()
                {
                    comment = "Saved Exceptional Loan Application",
                    loanApplicationDetailId = data.EXCEPTIONALLOANAPPLDETAILID,
                    createdBy = createdBy,
                    companyId = loan.companyId,
                    forwardAction = (int) ApprovalStatusEnum.Processing,
                    operationId = (int) OperationsEnum.ExceptionalLoan
                };

                // send to workflow
                var res = GoForApprovalExceptionalLoan(exceptional);
               
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        private RacReturnInfoViewModel AddLoanApplicationDetail(LoanApplicationViewModel loan)//List<LoanApplicationDetailViewModel> entity, int createdBy)
        {
            var createdBy = loan.createdBy;
            //foreach (var a in entity)
            //{
            var a = loan.LoanApplicationDetail.FirstOrDefault();

            var product = context.TBL_PRODUCT.Find(a.proposedProductId);

            if (a.repaymentScheduleId <= 0 && (product.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability))
            {
                throw new SecureException("Please select a repayment pattern");
            }

            if (a.proposedTenor == 0)
            {
                throw new SecureException("Tenor can not be ZERO (0)");
            }
            int applicationId = this.loanData == null ? 0 : this.loanData.LOANAPPLICATIONID; // ?
            int tenor = ConvertTenorToDays(a.proposedTenor, a.tenorModeId);


            var data = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDAMOUNT = a.proposedAmount,
                APPROVEDINTERESTRATE = (double)a.proposedInterestRate,
                APPROVEDPRODUCTID = a.proposedProductId,
                APPROVEDTENOR = tenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                EXCHANGERATE = a.exchangeRate,
                CURRENCYID = a.currencyId,
                CUSTOMERID = a.customerId,
                LOANAPPLICATIONID = applicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,

                EQUITYCASAACCOUNTID = a.equityCasaAccountId,
                EQUITYAMOUNT = a.equityAmount,

                PROPOSEDAMOUNT = a.proposedAmount,
                PROPOSEDINTERESTRATE = (int)a.proposedInterestRate,
                PROPOSEDPRODUCTID = a.proposedProductId,
                PROPOSEDTENOR = tenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),
                DELETED = false,
                SUBSECTORID = a.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = a.loanPurpose,
                CASAACCOUNTID = a.casaAccountId,
                OPERATINGCASAACCOUNTID = a.operatingCasaAccountId,
                REPAYMENTSCHEDULEID = a.repaymentScheduleId,
                REPAYMENTTERMS = a.repaymentTerm,
                CRMSFUNDINGSOURCEID = a.crmsFundingSourceId,
                CRMSREPAYMENTSOURCEID = a.crmsPaymentSourceId,
                CRMSFUNDINGSOURCECATEGORY = a.crmsFundingSourceCategory,
                CRMS_ECCI_NUMBER = a.crms_ECCI_Number,
                FIELD1 = a.fieldOne,
                FIELD2 = a.fieldTwo,
                FIELD3 = a.fieldThree,
                PRODUCTPRICEINDEXID = a.productPriceIndexId,
                PRODUCTPRICEINDEXRATE = a.productPriceIndexRate,
                TENORFREQUENCYTYPEID = a.tenorModeId,
                CRMSVALIDATED = false,
                ISTAKEOVERAPPLICATION = a.isTakeOverApplication,
                ISLINEFACILITY = a.isLineFacility,
                LOANDETAILREVIEWTYPEID = a.loanDetailReviewTypeId,
                APPROVEDTRADECYCLEID = a.approvedTradeCycleId,
                ISMORATORIUM = a.isMoratorium,
                INTERESTREPAYMENT = a.interestRepayment,
                INTERESTREPAYMENTID = a.interestRepaymentId,
                MORATORIUM = a.moratorium,
                APPROVEDLINELIMIT = a.approvedLineLimit,
                OLDAPPLICATIONREFFORRENEWAL = a.oldApplicationRefForRenewal,
                PROPERTYTYPEID = a.propertyTypeId,
                PROPERTYTITLE = a.propertyTitle,
                PROPERTYPRICE = a.propertyPrice,
                DOWNPAYMENT = a.downPayment
            };

            //var loanExist = context.TBL_LOAN_APPLICATION_DETAIL.Any(o => o.APPROVEDAMOUNT == data.APPROVEDAMOUNT
            //        && o.APPROVEDINTERESTRATE == data.APPROVEDINTERESTRATE && o.APPROVEDTENOR == data.APPROVEDTENOR && o.CURRENCYID == data.CURRENCYID && o.CUSTOMERID == data.CUSTOMERID
            //        && o.SUBSECTORID == data.SUBSECTORID && o.CREATEDBY == data.CREATEDBY && o.DELETED != true);

            //if (loanExist == true) throw new SecureException("This loan application has already been saved!");

            context.TBL_LOAN_APPLICATION_DETAIL.Add(data);

            if (a.invoiceDetails.Any() && a.productClassId == (short)ProductClassEnum.InvoiceDiscountingFacility)
            {
                InvoiceDetails(a.invoiceDetails, createdBy, applicationId, a.customerId);
            }
            //if (a.educationLoan != null && a.productClassId == (short)ProductClassEnum.FirstEdu)
            //{
            //    EducationLoan(a.educationLoan, a.loanApplicationDetailId, createdBy);
            //}

            //if (a.traderLoan != null && a.productClassId == (short)ProductClassEnum.FirstTrader)
            //{
            //    TradderLoan(a.traderLoan, a.loanApplicationDetailId, createdBy);
            //}
            if (a.bondDetails != null && a.productClassId == (short)ProductClassEnum.BondAndGuarantees)
            {
                BondDetails(a.bondDetails, a.loanApplicationDetailId, createdBy);
            }
            if (a.syndicatedLoan != null && a.syndicatedLoan.Count > 0)
            {
                SyndicatedDetails(a.syndicatedLoan, a.loanApplicationDetailId, createdBy);
            }
            if (a.productFees != null)
            {
                if (a.productFees.Count > 0)
                {
                    ProductFees(a.productFees, a.loanApplicationDetailId, createdBy);
                }
            }
            else
            {
                throw new SecureException("No fee is defined for this product(s)");
            }
            // }
            try
            {
                response = context.SaveChanges();
                if (response > 0)
                {
                    if (loan.rac != null)
                    {
                        var recResponse = SaveRac(loan.rac, loan.rac?.operationId, (int)loan.rac.productId, loan.rac.productClassId, data.LOANAPPLICATIONDETAILID, loan.createdBy, data.LOANAPPLICATIONID);
                        if (recResponse != null) return recResponse;
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

            return null;
        }

        public LoanApplicationViewModel GetExceptionalLoanApplicationById(int exceptionalLoanDetailId)
        {
            var loan = (from a in context.TBL_EXCEPTIONAL_LOAN_APPLICATION
                        join b in context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL on a.EXCEPTIONALLOANAPPLICATIONID equals b.EXCEPTIONALLOANAPPLICATIONID
                        where b.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                        select new LoanApplicationViewModel()
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            //TOTALEXPOSUREAMOUNT = totalAmount,
                            productClassId = a.PRODUCTCLASSID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            productClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                            companyId = a.COMPANYID,
                            branchId = a.BRANCHID,
                            userBranchId = a.BRANCHID,
                            createdBy = a.RELATIONSHIPOFFICERID,
                            //createdBy = a.RELATIONSHIPMANAGERID,
                            misCode = a.MISCODE,
                            teamMisCode = a.TEAMMISCODE,
                            interestRate = a.INTERESTRATE,
                            //APPLICATIONDATE = genSetup.GetApplicationDate(),
                            loanInformation = a.LOANINFORMATION,
                            isRelatedParty = a.ISRELATEDPARTY,
                            isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                          
                            customerGroupId = a.CUSTOMERGROUPID,
                            casaAccountId = a.CASAACCOUNTID,
                            
                            applicationAmount = a.APPLICATIONAMOUNT,
                            proposedTenor = a.APPLICATIONTENOR,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            regionId = a.CAPREGIONID,
                            requireCollateralTypeId = a.REQUIRECOLLATERALTYPEID,
                            loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                            loantermSheetId = a.LOANTERMSHEETID,
                            customerId = a.CUSTOMERID,
                            submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                            flowchangeId = a.FLOWCHANGEID,
                            exclusiveOperationId = a.OPERATIONID,
                            loanTypeId = a.LOANAPPLICATIONTYPEID,
                            collateralDetail = a.COLLATERALDETAIL,
                            isadhocapplication = a.ISADHOCAPPLICATION,
                            loansWithOthers = a.LOANSWITHOTHERS,
                            ownershipStructure = a.OWNERSHIPSTRUCTURE,
                            loanApprovedLimitId = a.LOANAPPROVEDLIMITID,
                            //workflowProductId = a.PRODUCTID,
                            isEmployerRelated = a.ISEMPLOYERRELATED,
                            relatedEmployerId = a.RELATEDEMPLOYERID,
                            rac = (from c in context.TBL_EXCEPTIONAL_RAC_DETAIL
                                   where c.TARGETID == exceptionalLoanDetailId
                                   select new RacInformationViewModel
                                   {
                                       operationId = c.OPERATIONID,
                                       productClassId = a.PRODUCTCLASSID,
                                       racDefinitionId = c.RACDEFINITIONID,
                                       actualValue = c.ACTUALVALUE,
                                       createdBy = c.CREATEDBY,
                                       productId = a.PRODUCTID
                                   }).FirstOrDefault(),

                            LoanApplicationDetail = (from d in context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL
                                                    where d.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                                                    select new LoanApplicationDetailViewModel
                                                    {
                                                        proposedAmount = d.PROPOSEDAMOUNT,
                                                        proposedInterestRate = d.PROPOSEDINTERESTRATE,
                                                        proposedProductId = d.PROPOSEDPRODUCTID,
                                                        proposedTenor = d.APPROVEDTENOR,
                                                        approvedAmount = d.APPROVEDAMOUNT,
                                                        approvedInterestRate = d.APPROVEDINTERESTRATE,
                                                        approvedProductId = d.APPROVEDPRODUCTID,
                                                        approvedTenor = d.APPROVEDTENOR,
                                                        exchangeRate = d.EXCHANGERATE,
                                                        currencyId = d.CURRENCYID,
                                                        customerId = d.CUSTOMERID,
                                                        equityCasaAccountId = d.EQUITYCASAACCOUNTID,
                                                        equityAmount = d.EQUITYAMOUNT,
                                                        subSectorId = d.SUBSECTORID,
                                                        //sectorId = (short)d.TBL_SUB_SECTOR.SECTORID,
                                                        loanPurpose = d.LOANPURPOSE,
                                                        casaAccountId = d.CASAACCOUNTID,
                                                        repaymentTerm = d.REPAYMENTTERMS,
                                                        repaymentScheduleId = d.REPAYMENTSCHEDULEID,
                                                        isTakeOverApplication = d.ISTAKEOVERAPPLICATION,
                                                        crmsFundingSourceId = d.CRMSFUNDINGSOURCEID,
                                                        crmsPaymentSourceId = d.CRMSREPAYMENTSOURCEID,
                                                        crmsFundingSourceCategory = d.CRMSFUNDINGSOURCECATEGORY,
                                                        productPriceIndexId = d.PRODUCTPRICEINDEXID,
                                                        productPriceIndexRate = d.PRODUCTPRICEINDEXRATE,
                                                        operatingCasaAccountId = d.OPERATINGCASAACCOUNTID,
                                                        loanDetailReviewTypeId = d.LOANDETAILREVIEWTYPEID,

                                                        tenorModeId = d.TENORFREQUENCYTYPEID,
                                                        flowChangeId = d.TBL_EXCEPTIONAL_LOAN_APPLICATION.FLOWCHANGEID,
                                                        isLineFacility = d.ISLINEFACILITY,
                                                        approvedLineLimit = d.APPROVEDLINELIMIT,
                                                        interestRepaymentId = d.INTERESTREPAYMENTID,
                                                        interestRepayment = d.INTERESTREPAYMENT,
                                                        isMoratorium = d.ISMORATORIUM,
                                                        moratorium = d.MORATORIUM,
                                                        invoiceDetails = (from a in context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_INV
                                                                          where a.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                                                                          select new InvoiceDetailViewModel
                                                                          {
                                                                              invoiceId = a.INVOICEID,
                                                                              //loanApplicationDetailId = a.EXCEPTIONALLOANAPPLDETAILID,
                                                                              principalId = a.PRINCIPALID,
                                                                              //principalName = a.TBL_LOAN_PRINCIPAL.NAME,
                                                                              invoiceNo = a.INVOICENO,
                                                                              contractNo = a.CONTRACTNO,
                                                                              invoiceDate = a.INVOICE_DATE,
                                                                              invoiceAmount = a.INVOICE_AMOUNT,
                                                                              invoiceCurrencyId = a.INVOICE_CURRENCYID,
                                                                              //invoiceCurrencyName = a.TBL_CURRENCY.CURRENCYNAME,
                                                                              contractStartDate = a.CONTRACT_STARTDATE,
                                                                              contractEndDate = a.CONTRACT_ENDDATE,
                                                                              approvalStatusId = a.APPROVALSTATUSID,
                                                                              purchaseOrderNumber = a.PURCHASEORDERNUMBER,
                                                                              reValidated = a.REVALIDATED,
                                                                              entrySheetNumber = a.ENTRYSHEETNUMBER,
                                                                              productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                                                                          }).ToList(),
                                                        bondDetails = (from a in context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_BG
                                                                       where a.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                                                                       select new BondsAndGuranty
                                                                       {
                                                                           //loanApplicationDetailId = a.EXCEPTIONALLOANAPPLDETAILID,
                                                                           principalId = a.PRINCIPALID,
                                                                           bondAmount = a.AMOUNT,
                                                                           bondCurrencyId = a.CURRENCYID,
                                                                           contractStartDate = a.CONTRACT_STARTDATE,
                                                                           contractEndDate = a.CONTRACT_ENDDATE,
                                                                           isTenored = a.ISTENORED,
                                                                           isBankFormat = a.ISBANKFORMAT,
                                                                           casaAccountId = a.CASAACCOUNTID,
                                                                           referenceNo = a.REFERENCENO,
                                                                       }).FirstOrDefault(),
                                                        syndicatedLoan = (from a in context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_SYN
                                                                          where a.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                                                                          select new SyndicatedLoanDetailViewModel
                                                                          {
                                                                              bankCode = a.BANKCODE,
                                                                              bankName = a.BANKNAME,
                                                                              amountContributed = a.AMOUNTCONTRIBUTED,
                                                                              typeId = (short)a.PARTY_TYPEID,
                                                                              //loanApplicationDetailId = a.EXCEPTIONALLOANAPPLDETAILID,
                                                                              //createdBy = a.CREATEDBY,
                                                                          }).ToList(),
                                                        productFees = (from a in context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_FEE
                                                                       where a.EXCEPTIONALLOANAPPLDETAILID == exceptionalLoanDetailId
                                                                       select new ProductFeesViewModel
                                                                       {
                                                                           //loanApplicationDetailId = a.EXCEPTIONALLOANAPPLDETAILID,
                                                                           feeId = a.CHARGEFEEID,
                                                                           rate = a.RECOMMENDED_FEERATEVALUE,
                                                                           hasConsession = a.HASCONSESSION,
                                                                       }).ToList(),
                                                    }).ToList(),
                        }).FirstOrDefault();

            return loan;
        }

        public IEnumerable<LoanApplicationDetailViewModel> GetExceptionalLoansForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.ExceptionalLoan;
            var levelIds = genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var exceptionalLoansForApproval = (from d in context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL
                                               join e in context.TBL_EXCEPTIONAL_LOAN_APPLICATION on d.EXCEPTIONALLOANAPPLICATIONID equals e.EXCEPTIONALLOANAPPLICATIONID
                                               join t in context.TBL_APPROVAL_TRAIL on d.EXCEPTIONALLOANAPPLDETAILID equals t.TARGETID
                                               where d.DELETED == false && t.OPERATIONID == (int)OperationsEnum.ExceptionalLoan
                                                &&
                                                ((
                                                d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                                && t.APPROVALSTATEID != (int)ApprovalState.Ended
                                                && t.RESPONSESTAFFID == null
                                                && (levelIds.Contains((int)t.TOAPPROVALLEVELID) && t.LOOPEDSTAFFID == null)
                                                && (t.TOSTAFFID == null || t.TOSTAFFID == staffId)
                                                )
                                                ||
                                                (
                                                d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved
                                                && t.APPROVALSTATEID == (int)ApprovalState.Ended
                                                && t.RESPONSESTAFFID == null && d.CREATEDBY == staffId
                                                ))

                                               select new LoanApplicationDetailViewModel
                                              {
                                                  customerName = context.TBL_CUSTOMER.Where(c=>c.CUSTOMERID == e.CUSTOMERID).Select(c=>c.FIRSTNAME + " "+ c.MIDDLENAME + " "+ c.LASTNAME).FirstOrDefault(),
                                                  loanApplicationId = e.EXCEPTIONALLOANAPPLICATIONID,
                                                  dateTimeCreated = d.DATETIMECREATED,
                                                  proposedAmount = d.PROPOSEDAMOUNT,
                                                  proposedInterestRate = d.PROPOSEDINTERESTRATE,
                                                  proposedProductId = d.PROPOSEDPRODUCTID,
                                                  applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                                  //proposedTenor = d.APPROVEDTENOR,
                                                  approvedAmount = d.APPROVEDAMOUNT,
                                                  approvedInterestRate = d.APPROVEDINTERESTRATE,
                                                  approvedProductId = d.APPROVEDPRODUCTID,
                                                  approvedTenor = d.APPROVEDTENOR,
                                                  exchangeRate = d.EXCHANGERATE,
                                                  currencyId = d.CURRENCYID,
                                                  customerId = d.CUSTOMERID,
                                                  equityCasaAccountId = d.EQUITYCASAACCOUNTID,
                                                  equityAmount = d.EQUITYAMOUNT,
                                                  subSectorId = d.SUBSECTORID,
                                                  //sectorId = (short)d.TBL_SUB_SECTOR.SECTORID,
                                                  loanPurpose = d.LOANPURPOSE,
                                                  casaAccountId = d.CASAACCOUNTID,
                                                  repaymentTerm = d.REPAYMENTTERMS,
                                                  repaymentScheduleId = d.REPAYMENTSCHEDULEID,
                                                  isTakeOverApplication = d.ISTAKEOVERAPPLICATION,
                                                  crmsFundingSourceId = d.CRMSFUNDINGSOURCEID,
                                                  crmsPaymentSourceId = d.CRMSREPAYMENTSOURCEID,
                                                  crmsFundingSourceCategory = d.CRMSFUNDINGSOURCECATEGORY,
                                                  productPriceIndexId = d.PRODUCTPRICEINDEXID,
                                                  productPriceIndexRate = d.PRODUCTPRICEINDEXRATE,
                                                        loanDetailReviewTypeId = d.LOANDETAILREVIEWTYPEID,
                                            operatingCasaAccountId = d.OPERATINGCASAACCOUNTID,
                                                  tenorModeId = d.TENORFREQUENCYTYPEID,
                                                  flowChangeId = d.TBL_EXCEPTIONAL_LOAN_APPLICATION.FLOWCHANGEID,
                                                  isLineFacility = d.ISLINEFACILITY,
                                                  approvedLineLimit = d.APPROVEDLINELIMIT,
                                                  interestRepaymentId = d.INTERESTREPAYMENTID,
                                                  interestRepayment = d.INTERESTREPAYMENT,
                                                  isMoratorium = d.ISMORATORIUM,
                                                  moratorium = d.MORATORIUM,
                                                  proposedTenor = d.PROPOSEDTENOR,
                                                  //approvalStatusId = d.APPROVALSTATUSID,
                                                  breachedLimitName = d.BREACHEDLIMITNAME,
                                                  loanApplicationDetailId = d.EXCEPTIONALLOANAPPLDETAILID,
                                                  
                                                  approvalStatusId = t.APPROVALSTATUSID,
                                                  approvalTrailId = t.APPROVALTRAILID,
                                                  currentApprovalLevelId = t.TOAPPROVALLEVELID,
                                                  currentApprovalLevel = t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                                  approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(a => a.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                              }).GroupBy(d => d.loanApplicationDetailId)
                                                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault()).ToList();
            foreach(var x in exceptionalLoansForApproval)
            {
                var templateExist = context.TBL_DOC_TEMPLATE_DETAIL.Where(p => p.TARGETID == x.loanApplicationId && p.OPERATIONID == (int)OperationsEnum.ExceptionalLoan).ToList();
                if (templateExist.Any())
                {
                    x.isTemplateUploaded = true;
                }
                else
                {
                    x.isTemplateUploaded = false;
                }
                 
            }
            return exceptionalLoansForApproval;
        }

        public WorkflowResponse GoForApprovalExceptionalLoan(ExceptionalLoanViewModel model)
        {
            int operationId = (int)OperationsEnum.ExceptionalLoan; 
            var cs = context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL.Find(model.loanApplicationDetailId);

            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved)
            {
                model.forwardAction = (int)ApprovalStatusEnum.Processing;
            }
            else
            {
                model.forwardAction = (int)ApprovalStatusEnum.Disapproved;
            }

            // WORKFLOW
            workflow.OperationId = operationId;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.loanApplicationDetailId;
            workflow.CompanyId = model.companyId;

            //workflow.Vote = model.vote;
            //workflow.NextLevelId = null;
            //workflow.ToStaffId = null;
            workflow.StatusId = (int)model.forwardAction;
            workflow.Comment = model.comment;

            //workflow.BusinessUnitId = c?.BUSINESSUNTID;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
            context.SaveChanges();
            string loanAppReference = "";
            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // UPDATE APPLICATION
            if (cs != null) { cs.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

            if (workflow.NewState == (int)ApprovalState.Ended)
            {
                if (workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
                {
                        cs.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        workflow.SetResponse = true;
                        loanAppReference = SaveExceptionalLoanApplication(model.loanApplicationDetailId);
                }
                else
                {
                    cs.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                }

            }

            context.SaveChanges();
            workflow.Response.responseMessage = loanAppReference;
            return workflow.Response;
        }

        public String ResponseMessage(WorkflowResponse response, string itemHeading)
        {
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    return "The " + itemHeading + " request has been APPROVED successfully";
                }
                else
                {
                    return "The " + itemHeading + " request has been DISAPPROVED successfully";
                }
            }

        }

        public string SaveExceptionalLoanApplication(int loanApplicationDetailId)
        {
            var loanApplication = GetExceptionalLoanApplicationById(loanApplicationDetailId);

            // save loan application
            var savedLoanApplication = AddLoanApplication(loanApplication, true);
            //var result = SendApplicationToEdit(savedLoanApplication.loanApplicationId, (int) OperationsEnum.CreditAppraisal, savedLoanApplication.createdBy);
            return savedLoanApplication.applicationReferenceNumber;
        }

        public LoanApplicationDetailViewModel GetLoanApplicationDetailFields(int detailId)
        {
            var fields = new LoanApplicationDetailViewModel();

            var d = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == detailId);
            //var inv = context.TBL_LOAN_APPLICATION_DETL_INV.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == detailId);
            var product = context.TBL_PRODUCT.Find(d.APPROVEDPRODUCTID);
            fields = new LoanApplicationDetailViewModel
            {
                proposedAmount = d.PROPOSEDAMOUNT,
                proposedInterestRate = d.PROPOSEDINTERESTRATE,
                proposedProductId = d.PROPOSEDPRODUCTID,
                proposedTenor = d.APPROVEDTENOR,
                approvedAmount = d.APPROVEDAMOUNT,
                approvedInterestRate = d.APPROVEDINTERESTRATE,
                approvedProductId = d.APPROVEDPRODUCTID,
                approvedTenor = d.APPROVEDTENOR,
                exchangeRate = d.EXCHANGERATE,
                currencyId = d.CURRENCYID,
                customerId = d.CUSTOMERID,
                equityCasaAccountId = d.EQUITYCASAACCOUNTID,
                equityAmount = d.EQUITYAMOUNT,
                subSectorId = d.SUBSECTORID,
                sectorId = (short)d.TBL_SUB_SECTOR.SECTORID,
                loanPurpose = d.LOANPURPOSE,
                casaAccountId = d.CASAACCOUNTID,
                repaymentTerm = d.REPAYMENTTERMS,
                repaymentScheduleId = d.REPAYMENTSCHEDULEID,
                isTakeOverApplication = d.ISTAKEOVERAPPLICATION,
                crmsFundingSourceId = d.CRMSFUNDINGSOURCEID,
                crmsPaymentSourceId = d.CRMSREPAYMENTSOURCEID,
                crmsFundingSourceCategory = d.CRMSFUNDINGSOURCECATEGORY,
                productPriceIndexId = d.PRODUCTPRICEINDEXID,
                productPriceIndexRate = d.PRODUCTPRICEINDEXRATE,
                operatingCasaAccountId = d.OPERATINGCASAACCOUNTID,
                loanDetailReviewTypeId = d.LOANDETAILREVIEWTYPEID,
                tenorModeId = d.TENORFREQUENCYTYPEID,
                flowChangeId = d.TBL_LOAN_APPLICATION.FLOWCHANGEID,
                isLineFacility = d.ISLINEFACILITY,
                iblRequest = d.TBL_LOAN_APPLICATION.IBLREQUEST,
                approvedLineLimit = d.APPROVEDLINELIMIT,
                interestRepaymentId = d.INTERESTREPAYMENTID,
                interestRepayment = d.INTERESTREPAYMENT,
                isMoratorium = d.ISMORATORIUM,
                moratorium = d.MORATORIUM,
                productClassId = product.PRODUCTCLASSID,
                productTypeId = product.PRODUCTTYPEID,
                approvedTradeCycleId = d.APPROVEDTRADECYCLEID,
                propertyTypeId = d.PROPERTYTYPEID,
                propertyTitle = d.PROPERTYTITLE,
                propertyPrice = d.PROPERTYPRICE,
                downPayment = d.DOWNPAYMENT
            };

            var proposedTenor = ConvertTenorDaysToTenor(fields.proposedTenor, fields.tenorModeId);
            fields.proposedTenor = fields.approvedTenor = proposedTenor;

            var invoiceDetails = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
                                  where a.LOANAPPLICATIONDETAILID == detailId
                                  select new InvoiceDetailViewModel
                                  {
                                      invoiceId = a.INVOICEID,
                                      loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                      principalId = a.PRINCIPALID,
                                      principalName = a.TBL_LOAN_PRINCIPAL.NAME,
                                      invoiceNo = a.INVOICENO,
                                      contractNo = a.CONTRACTNO,
                                      invoiceDate = a.INVOICE_DATE,
                                      invoiceAmount = a.INVOICE_AMOUNT,
                                      invoiceCurrencyId = a.INVOICE_CURRENCYID,
                                      invoiceCurrencyName = a.TBL_CURRENCY.CURRENCYNAME,
                                      contractStartDate = a.CONTRACT_STARTDATE,
                                      contractEndDate = a.CONTRACT_ENDDATE,
                                      approvalStatusId = a.APPROVALSTATUSID,
                                      purchaseOrderNumber = a.PURCHASEORDERNUMBER,
                                      reValidated = a.REVALIDATED,
                                      entrySheetNumber = a.ENTRYSHEETNUMBER,
                                      productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                                  }).ToList();

            var bondDetails = (from a in context.TBL_LOAN_APPLICATION_DETL_BG
                               where a.LOANAPPLICATIONDETAILID == detailId
                               select new BondsAndGuranty
                               {
                                   loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                   principalId = a.PRINCIPALID,
                                   bondAmount = a.AMOUNT,
                                   bondCurrencyId = a.CURRENCYID,
                                   contractStartDate = a.CONTRACT_STARTDATE,
                                   contractEndDate = a.CONTRACT_ENDDATE,
                                   isTenored = a.ISTENORED,
                                   isBankFormat = a.ISBANKFORMAT,
                                   casaAccountId = a.CASAACCOUNTID,
                                   referenceNo = a.REFERENCENO,

                               }).FirstOrDefault();

            var educationLoan = (from a in context.TBL_LOAN_APPLICATION_DETL_EDU
                                 where a.LOANAPPLICATIONDETAILID == detailId
                                 select new EducationLoanViewModel
                                 {
                                     numberOfStudent = a.NUMBER_OF_STUDENTS,
                                     averageSchoolFees = a.AVERAGE_SCHOOL_FEES,
                                     totalPreviousTermSchoolFees = a.TOTAL_PREVIOUS_TERM_SCHOL_FEES,

                                 }).FirstOrDefault();


            var traderLoan = (from a in context.TBL_LOAN_APPLICATION_DETL_TRA
                              where a.LOANAPPLICATIONDETAILID == detailId
                              select new TraderLoanViewModel
                              {
                                  loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                  marketId = a.MARKETID,
                                  marketName = a.TBL_LOAN_MARKET.MARKETNAME,
                                  averageMonthlyTurnover = a.AVERAGE_MONTHLY_TURNOVER,
                                  soldItems = a.SOLDITEMS

                              }).FirstOrDefault();

            fields.invoiceDetails = invoiceDetails;
            fields.bondDetails = bondDetails;
            fields.educationLoan = educationLoan;
            fields.traderLoan = traderLoan;

            return fields;
        }

        private void ProductFees(List<ProductFeesViewModel> fees, int loanApplicationDetailId, int createdBy)
        {
            var data = fees.Select(c => new TBL_LOAN_APPLICATION_DETL_FEE()
            {
                CHARGEFEEID = c.feeId,
                RECOMMENDED_FEERATEVALUE = c.rate,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                HASCONSESSION = false,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                LOANAPPLICATIONDETAILID = c.loanApplicationDetailId,
                DEFAULT_FEERATEVALUE = c.rate
            });

            context.TBL_LOAN_APPLICATION_DETL_FEE.AddRange(data);

        }

        private void AddExceptionalProductFees(List<ProductFeesViewModel> fees, int loanApplicationDetailId, int createdBy)
        {
            var data = fees.Select(c => new TBL_EXCEPTIONAL_LOAN_APPL_DETL_FEE()
            {
                CHARGEFEEID = c.feeId,
                RECOMMENDED_FEERATEVALUE = c.rate,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                HASCONSESSION = false,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                EXCEPTIONALLOANAPPLDETAILID = c.loanApplicationDetailId,
                DEFAULT_FEERATEVALUE = c.rate
            });

            context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_FEE.AddRange(data);
        }

        public bool UpdateLoanDetailFees(List<ProductFeesViewModel> fees, int loanApplicationDetailId, int createdBy)
        {

            foreach (var fee in fees)
            {
                var savedFee = context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(c => c.LOANAPPLICATIONDETAILID == loanApplicationDetailId && c.CHARGEFEEID == fee.feeId);
                if (savedFee != null)
                {
                    savedFee.RECOMMENDED_FEERATEVALUE = fee.rate;
                }
            }

            return context.SaveChanges() > 0;
        }

        private void BondDetails(BondsAndGuranty entity, int loanApplicationId, int createdBy)
        {
            int? princId;
            if (entity.principalId == -1)
            {
                princId = null;
            }
            else
            {
                princId = entity.principalId;

            }
            var data = new TBL_LOAN_APPLICATION_DETL_BG()
            {
                AMOUNT = entity.bondAmount,
                CONTRACT_ENDDATE = entity.contractEndDate,
                CONTRACT_STARTDATE = entity.contractStartDate,
                ISBANKFORMAT = entity.isBankFormat,
                ISTENORED = entity.isTenored,
                CURRENCYID = entity.bondCurrencyId,
                REFERENCENO = entity.referenceNo,
                CASAACCOUNTID = entity.casaAccountId,
                PRINCIPALID = princId,
                DATETIMECREATED = DateTime.Now,
                LOANAPPLICATIONDETAILID = loanApplicationId,
                CREATEDBY = createdBy,
                PRINCIPALNAME = entity.principalName
            };
            context.TBL_LOAN_APPLICATION_DETL_BG.Add(data);
        }

        private void AddExceptionalBondDetails(BondsAndGuranty entity, int loanApplicationId, int createdBy)
        {
            int? princId;
            if (entity.principalId == -1)
            {
                princId = null;
            }
            else
            {
                princId = entity.principalId;
            }

            var data = new TBL_EXCEPTIONAL_LOAN_APPL_DETL_BG()
            {
                AMOUNT = entity.bondAmount,
                CONTRACT_ENDDATE = entity.contractEndDate,
                CONTRACT_STARTDATE = entity.contractStartDate,
                ISBANKFORMAT = entity.isBankFormat,
                ISTENORED = entity.isTenored,
                CURRENCYID = entity.bondCurrencyId,
                REFERENCENO = entity.referenceNo,
                CASAACCOUNTID = entity.casaAccountId,
                PRINCIPALID = princId,
                DATETIMECREATED = DateTime.Now,
                EXCEPTIONALLOANAPPLDETAILID = loanApplicationId,
                CREATEDBY = createdBy,
                PRINCIPALNAME = entity.principalName
            };

            context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_BG.Add(data);
        }

        private void AddExceptionalSyndicatedDetails(List<SyndicatedLoanDetailViewModel> entity, int loanApplicationId, int createdBy)
        {
            var data = entity.Select(a => new TBL_EXCEPTIONAL_LOAN_APPL_DETL_SYN()
            {
                BANKCODE = a.bankCode,
                BANKNAME = a.bankName,
                AMOUNTCONTRIBUTED = a.amountContributed,
                PARTY_TYPEID = a.typeId,
                DELETED = false,
                DATETIMECREATED = DateTime.Now,
                EXCEPTIONALLOANAPPLDETAILID = loanApplicationId,
                CREATEDBY = createdBy
            });

            context.TBL_EXCEPTIONAL_LOAN_APPL_DETL_SYN.AddRange(data);
        }

        private void SyndicatedDetails(List<SyndicatedLoanDetailViewModel> entity, int loanApplicationId, int createdBy)
        {
            var data = entity.Select(a => new TBL_LOAN_APPLICATION_DETL_SYN()
            {

                BANKCODE = a.bankCode,
                BANKNAME = a.bankName,
                AMOUNTCONTRIBUTED = a.amountContributed,
                PARTY_TYPEID = a.typeId,
                DELETED = false,
                DATETIMECREATED = DateTime.Now,
                LOANAPPLICATIONDETAILID = loanApplicationId,
                CREATEDBY = createdBy
            });
            context.TBL_LOAN_APPLICATION_DETL_SYN.AddRange(data);
        }
        //public int AddCustomerCreditBureauCharge(LoanCreditBereauViewModel entity)
        //{
        //    var previousSearch = this.GetCustomerCreditBureauReportLog(entity.customerId);
        //    bool hascrms = false;
        //    foreach (var i in previousSearch)
        //    {
        //        if (i.creditBureauId == (short)CreditBureauEnum.CRMS) hascrms = true;
        //    };
        //    if (previousSearch.Count() >= 2 && !hascrms && entity.creditBureauId != (short)CreditBureauEnum.CRMS)
        //        throw new SecureException("Only three search options allowed and must inlude CRMS.\n Please check CRMS");

        //    if (previousSearch.Count() >= 3)
        //        throw new SecureException("You have reached that maximum credit bureau search for this customer");

        //    var data = new TBL_CUSTOMER_CREDIT_BUREAU()
        //    {
        //        COMPANYDIRECTORID = entity.companyDirectorId,
        //        CHARGEAMOUNT = entity.chargeAmount,
        //        CREDITBUREAUID = entity.creditBureauId,
        //        CUSTOMERID = entity.customerId,
        //        ISREPORTOKAY = entity.isReportOkay,
        //        USEDINTEGRATION = entity.usedIntegration,
        //        //ISCOMPLETED = entity.isComplete,
        //        DATECOMPLETED = entity.dateCompleted,
        //        DATETIMECREATED = DateTime.Now,
        //        CREATEDBY = entity.createdBy
        //    };
        //    context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);
        //    if (context.SaveChanges() > 0) return data.CUSTOMERCREDITBUREAUID;
        //    else return 0;
        //}

        public bool ArchiveLoanApplication(int loanAppliactionId, int operationId, short applicationStatus, int archivedBy)
        {
            short applicationStatusId = 0;
            var app = context.TBL_LOAN_APPLICATION.FirstOrDefault(l => l.LOANAPPLICATIONID == loanAppliactionId);
            if (app == null)
            {
                throw new SecureException("Loan Application doesn't exist!");
            }

            if (applicationStatus == 0)
            {
                applicationStatusId = app.APPLICATIONSTATUSID;
            }
            else
            {
                applicationStatusId = applicationStatus;
            }
            //var appArch = context.TBL_LOAN_APPLICATION_ARCHIVE.FirstOrDefault(l => l.LOANAPPLICATIONID == loanAppliactionId);
            //if (appArch != null)
            //{
            //    return true;
            //    //throw new SecureException("Loan Application already exist!");
            //}

            var details = app.TBL_LOAN_APPLICATION_DETAIL.ToList();
            TBL_LOAN_APPLICATION_ARCHIVE loanApplArchive = new TBL_LOAN_APPLICATION_ARCHIVE();
            loanApplArchive.ARCHIVEDBY = archivedBy;
            loanApplArchive.ARCHIVEDATE = DateTime.Now;
            loanApplArchive.LOANAPPLICATIONID = app.LOANAPPLICATIONID;
            loanApplArchive.APPLICATIONREFERENCENUMBER = app.APPLICATIONREFERENCENUMBER;
            loanApplArchive.LOANPRELIMINARYEVALUATIONID = app.LOANPRELIMINARYEVALUATIONID;
            loanApplArchive.LOANTERMSHEETID = app.LOANTERMSHEETID;
            loanApplArchive.COMPANYID = app.COMPANYID;
            loanApplArchive.CUSTOMERID = app.CUSTOMERID;
            loanApplArchive.BRANCHID = app.BRANCHID;
            loanApplArchive.CUSTOMERGROUPID = app.CUSTOMERGROUPID;
            loanApplArchive.LOANAPPLICATIONTYPEID = app.LOANAPPLICATIONTYPEID;
            loanApplArchive.RELATIONSHIPOFFICERID = app.RELATIONSHIPOFFICERID;
            loanApplArchive.RELATIONSHIPMANAGERID = app.RELATIONSHIPMANAGERID;
            loanApplArchive.CASAACCOUNTID = app.CASAACCOUNTID;
            loanApplArchive.APPLICATIONDATE = app.APPLICATIONDATE;
            loanApplArchive.INTERESTRATE = app.INTERESTRATE;
            loanApplArchive.APPLICATIONTENOR = app.APPLICATIONTENOR;
            loanApplArchive.OPERATIONID = app.OPERATIONID;
            loanApplArchive.PRODUCTCLASSID = app.PRODUCTCLASSID;
            loanApplArchive.PRODUCTID = app.PRODUCTID;
            loanApplArchive.PRODUCT_CLASS_PROCESSID = app.PRODUCT_CLASS_PROCESSID;
            loanApplArchive.APPLICATIONAMOUNT = app.APPLICATIONAMOUNT;
            loanApplArchive.APPROVEDAMOUNT = app.APPROVEDAMOUNT;
            loanApplArchive.TOTALEXPOSUREAMOUNT = app.TOTALEXPOSUREAMOUNT;
            loanApplArchive.APIREQUESTID = app.APIREQUESTID;
            loanApplArchive.LOANINFORMATION = app.LOANINFORMATION;
            loanApplArchive.MISCODE = app.MISCODE;
            loanApplArchive.TEAMMISCODE = app.MISCODE;
            loanApplArchive.ISINVESTMENTGRADE = app.ISINVESTMENTGRADE;
            loanApplArchive.ISRELATEDPARTY = app.ISRELATEDPARTY;
            loanApplArchive.ISPOLITICALLYEXPOSED = app.ISPOLITICALLYEXPOSED;
            loanApplArchive.ISPROJECTRELATED = app.ISPROJECTRELATED;
            loanApplArchive.ISONLENDING = app.ISONLENDING;
            loanApplArchive.ISINTERVENTIONFUNDS = app.ISINTERVENTIONFUNDS;
            loanApplArchive.ISORRBASEDAPPROVAL = app.ISORRBASEDAPPROVAL;
            loanApplArchive.WITHINSTRUCTION = app.WITHINSTRUCTION;
            loanApplArchive.DOMICILIATIONNOTINPLACE = app.DOMICILIATIONNOTINPLACE;
            loanApplArchive.CREATEDBY = app.CREATEDBY;
            loanApplArchive.DATETIMECREATED = app.DATETIMECREATED;
            loanApplArchive.LASTUPDATEDBY = app.LASTUPDATEDBY;
            loanApplArchive.DATETIMEUPDATED = app.DATETIMEUPDATED;
            loanApplArchive.DELETED = app.DELETED;
            loanApplArchive.DELETEDBY = app.DELETEDBY;
            loanApplArchive.DATETIMEDELETED = app.DATETIMEDELETED;
            loanApplArchive.SYSTEMDATETIME = app.SYSTEMDATETIME;
            loanApplArchive.APPROVALSTATUSID = app.APPROVALSTATUSID;
            loanApplArchive.APPLICATIONSTATUSID = applicationStatusId;
            loanApplArchive.FINALAPPROVAL_LEVELID = app.FINALAPPROVAL_LEVELID;
            loanApplArchive.TRANCHEAPPROVAL_LEVELID = app.TRANCHEAPPROVAL_LEVELID;
            loanApplArchive.NEXTAPPLICATIONSTATUSID = app.NEXTAPPLICATIONSTATUSID;
            loanApplArchive.DATEACTEDON = app.DATEACTEDON;
            loanApplArchive.ACTEDONBY = app.ACTEDONBY;
            loanApplArchive.RISKRATINGID = app.RISKRATINGID;
            loanApplArchive.SUBMITTEDFORAPPRAISAL = app.SUBMITTEDFORAPPRAISAL;
            loanApplArchive.CUSTOMERINFOVALIDATED = app.CUSTOMERINFOVALIDATED;
            loanApplArchive.APPROVEDDATE = app.APPROVEDDATE;
            loanApplArchive.AVAILMENTDATE = app.AVAILMENTDATE;
            loanApplArchive.DISPUTED = app.DISPUTED;
            loanApplArchive.REQUIRECOLLATERAL = app.REQUIRECOLLATERAL;
            loanApplArchive.COLLATERALDETAIL = app.COLLATERALDETAIL;
            loanApplArchive.CAPREGIONID = app.CAPREGIONID;
            loanApplArchive.REQUIRECOLLATERALTYPEID = app.REQUIRECOLLATERALTYPEID;
            loanApplArchive.RELATEDREFERENCENUMBER = app.RELATEDREFERENCENUMBER;
            loanApplArchive.ISCHECKLISTLOADED = app.ISCHECKLISTLOADED;
            loanApplArchive.ISADHOCAPPLICATION = app.ISADHOCAPPLICATION;
            loanApplArchive.LOANSWITHOTHERS = app.LOANSWITHOTHERS;
            loanApplArchive.OWNERSHIPSTRUCTURE = app.OWNERSHIPSTRUCTURE;
            loanApplArchive.LOANAPPROVEDLIMITID = app.LOANAPPROVEDLIMITID;
            loanApplArchive.FLOWCHANGEID = app.FLOWCHANGEID;
            loanApplArchive.ISMULTIPLEPRODUCTDRAWDOWN = app.ISMULTIPLEPRODUCTDRAWDOWN;
            loanApplArchive.APPROVEDLINESTATUSID = app.APPROVEDLINESTATUSID;
            loanApplArchive.OWNEDBY = app.OWNEDBY;
            loanApplArchive.ARCHIVINGOPERATIONID = operationId;
            context.TBL_LOAN_APPLICATION_ARCHIVE.Add(loanApplArchive);
            foreach (var f in details)
            {
                ArchiveLoanApplicationDetails(f.LOANAPPLICATIONDETAILID);
            }
            return context.SaveChanges() > 0;
        }

        public void ArchiveLoanApplicationDetails(int loanApplicationDetailId)
        {
            var detailRow = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            if (detailRow == null)
            {
                throw new SecureException("Facility doesn't exist!");
            }

            //var detailRowArch = context.TBL_LOAN_APPLICATION_DETL_ARCH.Where(d=>d.LOANAPPLICATIONDETAILID == loanApplicationDetailId).FirstOrDefault();
            //if (detailRowArch == null)
            //{
            TBL_LOAN_APPLICATION_DETL_ARCH addLoanApplDetailsArchive = new TBL_LOAN_APPLICATION_DETL_ARCH();

            addLoanApplDetailsArchive.ARCHIVEDATE = DateTime.Today;
            addLoanApplDetailsArchive.LOANAPPLICATIONDETAILID = detailRow.LOANAPPLICATIONDETAILID;
            addLoanApplDetailsArchive.LOANAPPLICATIONID = detailRow.LOANAPPLICATIONID;
            addLoanApplDetailsArchive.CUSTOMERID = detailRow.CUSTOMERID;
            addLoanApplDetailsArchive.PROPOSEDPRODUCTID = detailRow.PROPOSEDPRODUCTID;
            addLoanApplDetailsArchive.PROPOSEDTENOR = detailRow.PROPOSEDTENOR;
            addLoanApplDetailsArchive.PROPOSEDINTERESTRATE = detailRow.PROPOSEDINTERESTRATE;
            addLoanApplDetailsArchive.PROPOSEDAMOUNT = detailRow.PROPOSEDAMOUNT;
            addLoanApplDetailsArchive.APPROVEDPRODUCTID = detailRow.APPROVEDPRODUCTID;
            addLoanApplDetailsArchive.APPROVEDTENOR = detailRow.APPROVEDTENOR;
            addLoanApplDetailsArchive.APPROVEDINTERESTRATE = detailRow.APPROVEDINTERESTRATE;
            addLoanApplDetailsArchive.APPROVEDAMOUNT = detailRow.APPROVEDAMOUNT;
            addLoanApplDetailsArchive.CURRENCYID = detailRow.CURRENCYID;
            addLoanApplDetailsArchive.EXCHANGERATE = detailRow.EXCHANGERATE;
            addLoanApplDetailsArchive.SUBSECTORID = detailRow.SUBSECTORID;
            addLoanApplDetailsArchive.STATUSID = detailRow.STATUSID;
            addLoanApplDetailsArchive.LOANPURPOSE = detailRow.LOANPURPOSE;
            addLoanApplDetailsArchive.CREATEDBY = detailRow.CREATEDBY;
            addLoanApplDetailsArchive.DATETIMECREATED = detailRow.DATETIMECREATED;
            addLoanApplDetailsArchive.LASTUPDATEDBY = detailRow.LASTUPDATEDBY;
            addLoanApplDetailsArchive.DATETIMEUPDATED = detailRow.DATETIMEUPDATED;
            addLoanApplDetailsArchive.DELETED = detailRow.DELETED;
            addLoanApplDetailsArchive.DELETEDBY = detailRow.DELETEDBY;
            addLoanApplDetailsArchive.DATETIMEDELETED = detailRow.DATETIMEDELETED;
            addLoanApplDetailsArchive.EQUITYAMOUNT = detailRow.EQUITYAMOUNT;
            addLoanApplDetailsArchive.HASDONECHECKLIST = detailRow.HASDONECHECKLIST;
            addLoanApplDetailsArchive.EQUITYCASAACCOUNTID = detailRow.EQUITYCASAACCOUNTID;
            addLoanApplDetailsArchive.CONSESSIONAPPROVALSTATUSID = detailRow.CONSESSIONAPPROVALSTATUSID;
            addLoanApplDetailsArchive.CONSESSIONREASON = detailRow.CONSESSIONREASON;
            addLoanApplDetailsArchive.ISPOLITICALLYEXPOSED = detailRow.ISPOLITICALLYEXPOSED;
            addLoanApplDetailsArchive.REPAYMENTTERMS = detailRow.REPAYMENTTERMS;
            addLoanApplDetailsArchive.REPAYMENTSCHEDULEID = detailRow.REPAYMENTSCHEDULEID;
            addLoanApplDetailsArchive.EFFECTIVEDATE = detailRow.EFFECTIVEDATE;
            addLoanApplDetailsArchive.ISTAKEOVERAPPLICATION = detailRow.ISTAKEOVERAPPLICATION;
            addLoanApplDetailsArchive.EXPIRYDATE = detailRow.EXPIRYDATE;
            addLoanApplDetailsArchive.CASAACCOUNTID = detailRow.CASAACCOUNTID;
            addLoanApplDetailsArchive.OPERATINGCASAACCOUNTID = detailRow.OPERATINGCASAACCOUNTID;
            addLoanApplDetailsArchive.SECUREDBYCOLLATERAL = detailRow.SECUREDBYCOLLATERAL;
            addLoanApplDetailsArchive.CRMSCOLLATERALTYPEID = detailRow.CRMSCOLLATERALTYPEID;
            addLoanApplDetailsArchive.MORATORIUMDURATION = detailRow.MORATORIUMDURATION;
            addLoanApplDetailsArchive.CRMSFUNDINGSOURCEID = detailRow.CRMSFUNDINGSOURCEID;
            addLoanApplDetailsArchive.CRMSREPAYMENTSOURCEID = detailRow.CRMSREPAYMENTSOURCEID;
            addLoanApplDetailsArchive.CRMSFUNDINGSOURCECATEGORY = detailRow.CRMSFUNDINGSOURCECATEGORY;
            addLoanApplDetailsArchive.CRMS_ECCI_NUMBER = detailRow.CRMS_ECCI_NUMBER;
            addLoanApplDetailsArchive.CRMSCODE = detailRow.CRMSCODE;
            addLoanApplDetailsArchive.CRMSREPAYMENTAGREEMENTID = detailRow.CRMSREPAYMENTAGREEMENTID;
            addLoanApplDetailsArchive.CRMSVALIDATED = detailRow.CRMSVALIDATED;
            addLoanApplDetailsArchive.CRMSDATE = detailRow.CRMSDATE;
            addLoanApplDetailsArchive.TRANSACTIONDYNAMICS = detailRow.TRANSACTIONDYNAMICS;
            addLoanApplDetailsArchive.CONDITIONPRECIDENT = detailRow.CONDITIONPRECIDENT;
            addLoanApplDetailsArchive.CONDITIONSUBSEQUENT = detailRow.CONDITIONSUBSEQUENT;
            addLoanApplDetailsArchive.FIELD1 = detailRow.FIELD1;
            addLoanApplDetailsArchive.PRODUCTPRICEINDEXRATE = detailRow.PRODUCTPRICEINDEXRATE;
            addLoanApplDetailsArchive.PRODUCTPRICEINDEXID = detailRow.PRODUCTPRICEINDEXID;
            addLoanApplDetailsArchive.FIELD2 = detailRow.FIELD2;
            addLoanApplDetailsArchive.FIELD3 = detailRow.FIELD3;
            addLoanApplDetailsArchive.ISSPECIALISED = detailRow.ISSPECIALISED;
            addLoanApplDetailsArchive.TENORFREQUENCYTYPEID = detailRow.TENORFREQUENCYTYPEID;
            addLoanApplDetailsArchive.LOANDETAILREVIEWTYPEID = detailRow.LOANDETAILREVIEWTYPEID;
            addLoanApplDetailsArchive.ISFACILITYCREATED = detailRow.ISFACILITYCREATED;
            addLoanApplDetailsArchive.ISFEETAKEN = detailRow.ISFEETAKEN;
            addLoanApplDetailsArchive.TAKEFEETYPEID = detailRow.TAKEFEETYPEID;
            addLoanApplDetailsArchive.APPROVEDLINESTATUSID = detailRow.APPROVEDLINESTATUSID;
            addLoanApplDetailsArchive.INTERESTREPAYMENT = detailRow.INTERESTREPAYMENT;
            addLoanApplDetailsArchive.INTERESTREPAYMENTID = detailRow.INTERESTREPAYMENTID;
            addLoanApplDetailsArchive.MORATORIUM = detailRow.MORATORIUM;
            addLoanApplDetailsArchive.ISMORATORIUM = detailRow.ISMORATORIUM;
            addLoanApplDetailsArchive.APPROVEDLINELIMIT = detailRow.APPROVEDLINELIMIT;
            this.context.TBL_LOAN_APPLICATION_DETL_ARCH.Add(addLoanApplDetailsArchive);
            //}
            //return context.SaveChanges() != 0;
        }



        //lms operation ======================================
        public bool ArchiveLmsLoanApplication(int loanAppliactionId, int operationId, short applicationStatus, int archivedBy)
        {
            short applicationStatusId = 0;
            var app = context.TBL_LMSR_APPLICATION.FirstOrDefault(l => l.LOANAPPLICATIONID == loanAppliactionId);
            if (app == null)
            {
                throw new SecureException("Loan Application doesn't exist!");
            }

            if (applicationStatus == 0)
            {
                applicationStatusId = app.APPLICATIONSTATUSID;
            }
            else
            {
                applicationStatusId = applicationStatus;
            }
            
            var details = app.TBL_LMSR_APPLICATION_DETAIL.ToList();
            TBL_LMSR_APPLICATION_ARCHIVE loanApplArchive = new TBL_LMSR_APPLICATION_ARCHIVE();
            loanApplArchive.LOANAPPLICATIONID = app.LOANAPPLICATIONID;
            loanApplArchive.APPLICATIONREFERENCENUMBER = app.APPLICATIONREFERENCENUMBER;
            loanApplArchive.RELATEDREFERENCENUMBER = app.RELATEDREFERENCENUMBER;
            loanApplArchive.COMPANYID = app.COMPANYID;
            loanApplArchive.CUSTOMERID = app.CUSTOMERID;
            loanApplArchive.BRANCHID = app.BRANCHID;
            loanApplArchive.CUSTOMERGROUPID = app.CUSTOMERGROUPID;
            loanApplArchive.APPLICATIONDATE = app.APPLICATIONDATE;
            loanApplArchive.CREATEDBY = app.CREATEDBY;
            loanApplArchive.DATETIMECREATED = app.DATETIMECREATED;
            loanApplArchive.LASTUPDATEDBY = app.LASTUPDATEDBY;
            loanApplArchive.DATETIMEUPDATED = app.DATETIMEUPDATED;
            loanApplArchive.DELETED = app.DELETED;
            loanApplArchive.DELETEDBY = app.DELETEDBY;
            loanApplArchive.DATETIMEDELETED = app.DATETIMEDELETED;
            loanApplArchive.SYSTEMDATETIME = app.SYSTEMDATETIME;
            loanApplArchive.APPROVALSTATUSID = app.APPROVALSTATUSID;
            loanApplArchive.APPLICATIONSTATUSID = applicationStatusId;
            loanApplArchive.FINALAPPROVAL_LEVELID = app.FINALAPPROVAL_LEVELID;
            loanApplArchive.NEXTAPPLICATIONSTATUSID = app.NEXTAPPLICATIONSTATUSID;
            loanApplArchive.APPROVEDDATE = app.APPROVEDDATE;
            loanApplArchive.AVAILMENTDATE = app.AVAILMENTDATE;
            loanApplArchive.DISPUTED = app.DISPUTED;
            loanApplArchive.REQUIRECOLLATERAL = app.REQUIRECOLLATERAL;
            loanApplArchive.CAPREGIONID = app.CAPREGIONID;
            loanApplArchive.OPERATIONID = app.OPERATIONID;
            loanApplArchive.RISKRATINGID = app.RISKRATINGID;
            loanApplArchive.PRODUCTCLASSID = app.PRODUCTCLASSID;
            loanApplArchive.PRODUCTID = app.PRODUCTID;
            loanApplArchive.PRODUCT_CLASS_PROCESSID = app.PRODUCT_CLASS_PROCESSID;
            loanApplArchive.APPROVEDAMOUNT = app.APPROVEDAMOUNT;
            loanApplArchive.ISPROJECTRELATED = app.ISPROJECTRELATED;
            loanApplArchive.ISONLENDING = app.ISONLENDING;
            loanApplArchive.ISINTERVENTIONFUNDS = app.ISINTERVENTIONFUNDS;
            loanApplArchive.WITHINSTRUCTION = app.WITHINSTRUCTION;
            loanApplArchive.DOMICILIATIONNOTINPLACE = app.DOMICILIATIONNOTINPLACE;
            loanApplArchive.ARCHIVEDATE = DateTime.Now;
            loanApplArchive.OWNEDBY = app.CREATEDBY;
            context.TBL_LMSR_APPLICATION_ARCHIVE.Add(loanApplArchive);
            foreach (var f in details)
            {
                ArchiveLmsLoanApplicationDetails(f.LOANREVIEWAPPLICATIONID);
            }
            return context.SaveChanges() > 0;
        }

        public void ArchiveLmsLoanApplicationDetails(int loanApplicationDetailId)
        {
            var detailRow = context.TBL_LMSR_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            if (detailRow == null)
            {
                throw new SecureException("Facility doesn't exist!");
            }

           
            TBL_LMSR_APPLICATION_DETL_ARCH addLoanApplDetailsArchive = new TBL_LMSR_APPLICATION_DETL_ARCH();
            addLoanApplDetailsArchive.LOANREVIEWAPPLICATIONID = detailRow.LOANREVIEWAPPLICATIONID;
            addLoanApplDetailsArchive.LOANID = detailRow.LOANID;
            addLoanApplDetailsArchive.OPERATIONID = detailRow.OPERATIONID;
            addLoanApplDetailsArchive.CUSTOMERID = detailRow.CUSTOMERID;
            addLoanApplDetailsArchive.REPAYMENTTERMS = detailRow.REPAYMENTTERMS;
            addLoanApplDetailsArchive.REPAYMENTSCHEDULEID = detailRow.REPAYMENTSCHEDULEID;
            addLoanApplDetailsArchive.REVIEWDETAILS = detailRow.REVIEWDETAILS;
            addLoanApplDetailsArchive.REVIEWSTAGEID = detailRow.REVIEWSTAGEID;
            addLoanApplDetailsArchive.APPROVALSTATUSID = detailRow.APPROVALSTATUSID;
            addLoanApplDetailsArchive.CREATEDBY = detailRow.CREATEDBY;
            addLoanApplDetailsArchive.DATETIMECREATED = detailRow.DATETIMECREATED;
            addLoanApplDetailsArchive.LOANAPPLICATIONID = detailRow.LOANAPPLICATIONID;
            addLoanApplDetailsArchive.LOANSYSTEMTYPEID = detailRow.LOANSYSTEMTYPEID;
            addLoanApplDetailsArchive.PRODUCTID = detailRow.PRODUCTID;
            addLoanApplDetailsArchive.PROPOSEDAMOUNT = detailRow.PROPOSEDAMOUNT;
            addLoanApplDetailsArchive.PROPOSEDTENOR = detailRow.PROPOSEDTENOR;
            addLoanApplDetailsArchive.PROPOSEDINTERESTRATE = detailRow.PROPOSEDINTERESTRATE;
            addLoanApplDetailsArchive.APPROVEDTENOR = detailRow.APPROVEDTENOR;
            addLoanApplDetailsArchive.APPROVEDINTERESTRATE = detailRow.APPROVEDINTERESTRATE;
            addLoanApplDetailsArchive.APPROVEDAMOUNT = detailRow.APPROVEDAMOUNT;
            addLoanApplDetailsArchive.OPERATIONPERFORMED = detailRow.OPERATIONPERFORMED;
            addLoanApplDetailsArchive.CUSTOMERPROPOSEDAMOUNT = detailRow.CUSTOMERPROPOSEDAMOUNT;
            addLoanApplDetailsArchive.MANAGEMENTPOSITION = detailRow.MANAGEMENTPOSITION;
            addLoanApplDetailsArchive.DELETED = detailRow.DELETED;
            addLoanApplDetailsArchive.CURRENCYID = detailRow.CURRENCYID;
            addLoanApplDetailsArchive.ARCHIVEDATE = DateTime.Now;
            this.context.TBL_LMSR_APPLICATION_DETL_ARCH.Add(addLoanApplDetailsArchive);
        }


        public bool DeleteLoanApplication(int loanApplicationId)
        {
            var dataapplication = context.TBL_LOAN_APPLICATION.Where(d => d.LOANAPPLICATIONID == loanApplicationId).FirstOrDefault();
            var datadetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == dataapplication.LOANAPPLICATIONID).ToList();

            if (datadetail != null)
            {
                foreach (var data in datadetail)
                {
                    DeleteLoanApplicationDetail(data.LOANAPPLICATIONDETAILID);
                    //int loanApplicationId = 0;
                    //var fees = data.TBL_LOAN_APPLICATION_DETL_FEE;
                    //if (fees.Count > 0)
                    //{
                    //    context.TBL_LOAN_APPLICATION_DETL_FEE.RemoveRange(fees);
                    //}

                    //if (data.TBL_LOAN_APPLICATION_DETL_EDU.Any())
                    //{
                    //    context.TBL_LOAN_APPLICATION_DETL_EDU.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_EDU);
                    //}

                    //if (data.TBL_LOAN_APPLICATION_DETL_BG.Any())
                    //{
                    //    context.TBL_LOAN_APPLICATION_DETL_BG.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_BG);
                    //}

                    //if (data.TBL_LOAN_APPLICATION_DETL_INV.Any())
                    //{
                    //    context.TBL_LOAN_APPLICATION_DETL_INV.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_INV);
                    //}

                    //if (data.TBL_LOAN_APPLICATION_DETL_TRA.Any())
                    //{
                    //    context.TBL_LOAN_APPLICATION_DETL_TRA.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_TRA);
                    //}


                    //context.TBL_LOAN_APPLICATION_DETAIL.Remove(data);
                    // loanApplicationId = data.LOANAPPLICATIONID;

                    //var loan = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == loanApplicationId).ToList();
                    //if (loan.Count() == 1)
                    //{
                    //    var loanApp = context.TBL_LOAN_APPLICATION.Where(la => la.LOANAPPLICATIONID == loanApplicationId);
                    //    context.TBL_LOAN_APPLICATION.Remove(loanApp.FirstOrDefault());
                    //}
                }
                if (dataapplication != null)
                {
                    if (dataapplication.TBL_LOAN_APPLICATION_ARCHIVE.Any())
                    {
                        context.TBL_LOAN_APPLICATION_ARCHIVE.RemoveRange(dataapplication.TBL_LOAN_APPLICATION_ARCHIVE);
                    }
                    if (dataapplication.TBL_LOAN_APPLICATION_COLLATERL.Any())
                    {
                        context.TBL_LOAN_APPLICATION_COLLATERL.RemoveRange(dataapplication.TBL_LOAN_APPLICATION_COLLATERL);
                    }
                    if (dataapplication.TBL_LOAN_APPLICATION_COLLATRL2.Any())
                    {
                        context.TBL_LOAN_APPLICATION_COLLATRL2.RemoveRange(dataapplication.TBL_LOAN_APPLICATION_COLLATRL2);
                    }
                    context.TBL_LOAN_APPLICATION.Remove(dataapplication);
                }
            }
            return context.SaveChanges() > 0;
        }

        public bool DeleteLoanApplicationDetail(int loanApplicationDetailId)
        {
            var data = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONDETAILID == loanApplicationDetailId).FirstOrDefault();

            if (data != null)
            {
                int loanApplicationId = 0;
                var fees = data.TBL_LOAN_APPLICATION_DETL_FEE;
                if (fees.Count > 0)
                {
                    context.TBL_LOAN_APPLICATION_DETL_FEE.RemoveRange(fees);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_EDU.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_EDU.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_EDU);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_BG.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_BG.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_BG);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_INV.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_INV.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_INV);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_TRA.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_TRA.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_TRA);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_LOG.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_LOG.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_LOG);
                }

                if (data.TBL_LOAN_APPLICATION_DETL_CON.Any())
                {
                    context.TBL_LOAN_APPLICATION_DETL_CON.RemoveRange(data.TBL_LOAN_APPLICATION_DETL_CON);
                }

                if (context.TBL_LOAN_APPLICATION_DETL_SYN.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var syndications = context.TBL_LOAN_APPLICATION_DETL_SYN.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_APPLICATION_DETL_SYN.RemoveRange(syndications);
                }

                if (context.TBL_LOAN_APPLICATION_DETL_TRA.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var tradders = context.TBL_LOAN_APPLICATION_DETL_TRA.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_APPLICATION_DETL_TRA.RemoveRange(tradders);
                }

                if (context.TBL_LOAN_CONDITION_PRECEDENT.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var conds = context.TBL_LOAN_CONDITION_PRECEDENT.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_CONDITION_PRECEDENT.RemoveRange(conds);
                }

                if (context.TBL_LOAN_TRANSACTION_DYNAMICS.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var trans = context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_TRANSACTION_DYNAMICS.RemoveRange(trans);
                }

                if (context.TBL_LOAN_APPLICATION_COLLATERL.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var collaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_APPLICATION_COLLATERL.RemoveRange(collaterals);
                }

                if (context.TBL_LOAN_APPLICATION_DETL_ARCH.Any(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId))
                {
                    var archs = context.TBL_LOAN_APPLICATION_DETL_ARCH.Where(s => s.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
                    context.TBL_LOAN_APPLICATION_DETL_ARCH.RemoveRange(archs);
                }


                context.TBL_LOAN_APPLICATION_DETAIL.Remove(data);
                //loanApplicationId = data.LOANAPPLICATIONID;

                //var loan = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == loanApplicationDetailId).ToList();
                //if (loan.Count() == 1)
                //{
                //    var loanApp = context.TBL_LOAN_APPLICATION.Where(la => la.LOANAPPLICATIONID == loanApplicationId);
                //    context.TBL_LOAN_APPLICATION.Remove(loanApp.FirstOrDefault());
                //}

            }
            return context.SaveChanges() > 0;
        }

        public bool UpdateCreditBureauCustomerReportStatus(bool status, LoanCreditBureauViewModel model)
        {
            var data = context.TBL_CUSTOMER_CREDIT_BUREAU.Where(c => c.CREDITBUREAUID == model.creditBureauId && c.CUSTOMERID == model.customerId).FirstOrDefault();

            if (data != null)
                data.ISREPORTOKAY = status;

            return context.SaveChanges() > 0;
        }

        public bool UpdateMultipleCreditBureauCustomerReportStatus(bool status, List<LoanCreditBureauViewModel> model)
        {
            foreach (var item in model)
            {
                if (UpdateCreditBureauCustomerReportStatus(status, item) == false) return false;
            }

            return true;
        }

        //public IEnumerable<CreditBereauViewModel> GetCreditBureauInformation()
        //{
        //    var creditBureauList = from a in context.TBL_CREDIT_BUREAU
        //                           where a.INUSE
        //                           select new CreditBereauViewModel
        //                           {
        //                               creditBureauId = a.CREDITBUREAUID,
        //                               creditBureauName = a.CREDITBUREAUNAME,
        //                               corporateChargeAmount = a.CORPORATE_CHARGEAMOUNT,
        //                               retailChargeAmount = a.INDIVIDUAL_CHARGEAMOUNT,
        //                               inUse = a.INUSE,
        //                               isMandatory = a.ISMANDATORY,
        //                               useIntegration = a.USEINTEGRATION,
        //                               appliedSearchForLoan = false,
        //                               hasFile = false,
        //                               fileName = string.Empty,
        //                           };
        //    return creditBureauList;
        //}

        //public List<LoanCreditBereauViewModel> GetCustomerCreditBureauReportLog(int customerId)
        //{
        //    var customerLoanCreditBureauData = from a in context.TBL_CUSTOMER_CREDIT_BUREAU
        //                                       where a.CUSTOMERID == customerId && a.DELETED == false //&& a.DATETIMECREATED.Day <= ((DateTime.Now - a.DATETIMECREATED).TotalDays  - 30)
        //                                       select new LoanCreditBereauViewModel
        //                                       {
        //                                           companyDirectorId = a.COMPANYDIRECTORID,
        //                                           companyDirectorName = a.TBL_CUSTOMER_COMPANY_DIRECTOR.FIRSTNAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.MIDDLENAME + " " + a.TBL_CUSTOMER_COMPANY_DIRECTOR.SURNAME,
        //                                           chargeAmount = a.CHARGEAMOUNT,
        //                                           customerId = a.CUSTOMERID,
        //                                           creditBureauId = a.CREDITBUREAUID,
        //                                           isReportOkay = a.ISREPORTOKAY,
        //                                           usedIntegration = a.USEDINTEGRATION,
        //                                           dateCompleted = (DateTime)a.DATECOMPLETED,
        //                                           dateTimeCreated = a.DATETIMECREATED,
        //                                           searchCount = 0,
        //                                           uploadCount = 0,
        //                                           createdBy = a.CREATEDBY
        //                                       };

        //    return customerLoanCreditBureauData.ToList();
        //}


        public IEnumerable<LoanApplicationCollateralViewModel> GetLoanApplicationCollateral(int loanApplicatioinCollateralId)
        {
            var data = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == loanApplicatioinCollateralId && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                approvalStatusId = c.APPROVALSTATUSID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID.Value,
                //collateralReleaseStatusId=c.TBL_COLLATERAL_CUSTOMER.COLLATERALRELEASESTATUSID,
                //collateralReleaseStatusName = c.TBL_COLLATERAL_CUSTOMER.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Find((int)CollateralReleaseStatus.InVault).COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Find(c.TBL_COLLATERAL_CUSTOMER.COLLATERALRELEASESTATUSID).COLLATERALRELEASESTATUSNAME,

            }).OrderByDescending(x => x.loanAppCollateralId);
            return data.ToList();
        }

        public bool AddLoanApplicationCollateral(List<LoanApplicationCollateralViewModel> entity)
        {
            var unmapped = new List<TBL_LOAN_APPLICATION_COLLATERL>();
            foreach (var ent in entity)
            {
                var dat = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c =>
                c.COLLATERALCUSTOMERID == ent.collateralCustomerId && c.LOANAPPLICATIONID == ent.loanApplicationId)
                .FirstOrDefault();
                if (dat == null)
                {
                    unmapped.Add(new TBL_LOAN_APPLICATION_COLLATERL
                    {
                        COLLATERALCUSTOMERID = ent.collateralCustomerId,
                        CREATEDBY = ent.createdBy,
                        //LOANAPPLICATIONDETAILID = ent.loanApplicationDetailId,
                        LOANAPPLICATIONID = ent.loanApplicationId
                    });
                }
            }


            var data = unmapped.Select(item => new TBL_LOAN_APPLICATION_COLLATERL
            {
                //LOANAPPLICATIONDETAILID = item.LOANAPPLICATIONDETAILID,
                COLLATERALCUSTOMERID = item.COLLATERALCUSTOMERID,
                LOANAPPLICATIONID = item.LOANAPPLICATIONID,
                CREATEDBY = item.CREATEDBY,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
            });
            context.TBL_LOAN_APPLICATION_COLLATERL.AddRange(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = entity.FirstOrDefault().createdBy,
                BRANCHID = (short)entity.FirstOrDefault().userBranchId,
                DETAIL = $"Added collateral loan application with reference Number: {entity.FirstOrDefault().applicationReferenceNumber}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                // URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.FirstOrDefault().loanAppCollateralId
            };

            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() > 0;
        }

        //private void ApplicationCollateralRef(List<LoanApplicationCollateralRefNoViewModel> entity)
        //{
        //   var item = entity.Select(c => new tbl_Loan_Application_Collateral()
        //    {
        //        DocumentNumber = c.documentNumber,
        //        IsBankAccount = c.isBankAccount,
        //        Worth = c.worth,
        //        CustomerCollateralId = c.customerCollateralId
        //    });
        //    context.tbl_Loan_Application_Collateral.AddRange(item);
        //}

        private string GenerateLoanReference(int customerId)
        {
            string code = "";
            int data = 0;
            if (customerId > 2)
            {
                var grp = this.context.TBL_CUSTOMER_GROUP.Where(x => x.CUSTOMERGROUPID == customerId);
                if (grp.Any())
                {
                    code = grp.First().GROUPCODE;
                }
                data = ((this.context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
            }
            else
            {
                var cust = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId);
                if (cust.Any())
                {
                    code = cust.First().CUSTOMERCODE;
                }
                data = ((context.TBL_LOAN_APPLICATION.Count(x => x.CUSTOMERID == customerId)) + 1);
            }

            return $"{code}{CommonHelpers.GenerateZeroString(5) + data.ToString().Right(5)}";
        }

        public bool CheckExistingCertificateOfOwnership(string certificateOfOwnership, int companyId)
        {
            bool isExisting = false;
            var collate = collateral.GetCustomerCollateral(companyId).Where(c => c.collateralCode == certificateOfOwnership);
            if (collate.Any())
            {
                return isExisting = true;
            }
            return isExisting;
        }

        #region "Loan Applications Awaiting Checklist"

        public IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsAwaitingCheckList(int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.ApplicationCompleted
                        && b.STATUSID == (short)LoanApplicationDetailsStatusEnum.Pending
                        && b.HASDONECHECKLIST == false
                        && a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            // proposedTenor = (from f in context.TBL_LOAN_APPLICATION_DETAIL where f.LOANAPPLICATIONID == b.LOANAPPLICATIONID select f.PROPOSEDTENOR).Max() ,
                            proposedAmount = (from f in context.TBL_LOAN_APPLICATION_DETAIL where f.LOANAPPLICATIONID == b.LOANAPPLICATIONID select f.PROPOSEDAMOUNT).Sum(),
                            proposedInterestRate = b.PROPOSEDINTERESTRATE
                        });

            return data;
        }
        public LoanApplicationDetailViewModel GetSingleLoanApplicationsDetails(int loanApplicationDetailId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where b.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                        && a.COMPANYID == companyId && a.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            productClassProcessId = b.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                            productClassId = (short?)b.TBL_LOAN_APPLICATION.PRODUCTCLASSID,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            conditionPrecedent = b.CONDITIONPRECIDENT,
                            conditionSubsequent = b.CONDITIONSUBSEQUENT,
                            transactionDynamics = b.TRANSACTIONDYNAMICS
                            // LoanCreditBereauReport = GetCustomerLoanCreditBureauReportChargesByApplicationId(b.CUSTOMERID, a.LOANAPPLICATIONID).ToList()
                        }).FirstOrDefault();
            return data;
        }
        public IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.LOANAPPLICATIONID == loanApplicationId
                        && a.APPLICATIONSTATUSID == (short)LoanApplicationStatusEnum.ApplicationInProgress
                        && b.STATUSID == (short)LoanApplicationDetailsStatusEnum.Pending
                        // && b.HASDONECHECKLIST == false
                        && a.COMPANYID == companyId && a.DELETED == false && b.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            proposedProductName = (a.FLOWCHANGEID == null || a.FLOWCHANGEID <= 0 || a.FLOWCHANGEID == (short)FlowChangeEnum.FAM) ? b.TBL_PRODUCT.PRODUCTNAME : b.TBL_PRODUCT.PRODUCTNAME + "(" + context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(x => x.FLOWCHANGEID == a.FLOWCHANGEID).PLACEHOLDER + ")",
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            proposedInterestRate = b.APPROVEDINTERESTRATE,
                            productClassProcessId = b.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                            productClassId = (short?)b.TBL_LOAN_APPLICATION.PRODUCTCLASSID,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            conditionPrecedent = b.CONDITIONPRECIDENT,
                            conditionSubsequent = b.CONDITIONSUBSEQUENT,
                            transactionDynamics = b.TRANSACTIONDYNAMICS
                            // LoanCreditBereauReport = GetCustomerLoanCreditBureauReportChargesByApplicationId(b.CUSTOMERID, a.LOANAPPLICATIONID).ToList()
                        }).ToList();
            return data;
        }


        public IEnumerable<LoanApplicationDetailViewModel> GetAllLoanApplicationsDetailsById(int loanApplicationId, int companyId)
        {

            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL
                        on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.LOANAPPLICATIONID == loanApplicationId
                        && a.COMPANYID == companyId && a.DELETED == false
                        && (b.STATUSID == (int)ApprovalStatusEnum.Approved
                        || b.STATUSID == (int)ApprovalStatusEnum.Processing)
                        select new LoanApplicationDetailViewModel()
                        {
                            loanApplicationId = b.LOANAPPLICATIONID,
                            // applicationRefNo = a.APPLICATIONREFERENCENUMBER,
                            //customerId = b.CUSTOMERID,
                            //customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedAmount = b.PROPOSEDAMOUNT,
                        }).ToList();
            return data;
        }

        private IQueryable<LoanApplicationDetailViewModel> GetLoanApplicationsDetails(int companyId)
        {
            var data = (from b in context.TBL_LOAN_APPLICATION_DETAIL
                        join a in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                        where a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                        && a.COMPANYID == companyId && a.DELETED == false && b.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            firstName = b.TBL_CUSTOMER.FIRSTNAME,
                            middleName = b.TBL_CUSTOMER.MIDDLENAME,
                            lastName = b.TBL_CUSTOMER.LASTNAME,
                            customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            approvedProductName = b.TBL_PRODUCT1.PRODUCTNAME,
                            approvedAmount = b.APPROVEDAMOUNT,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            productClassProcessId = b.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                            productClassId = (short?)b.TBL_LOAN_APPLICATION.PRODUCTCLASSID,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER
                        });
            return data;
        }
        private IQueryable<LoanApplicationDetailViewModel> GetLmsLoanApplicationsDetails(int companyId)
        {
            var data = (from b in context.TBL_LMSR_APPLICATION_DETAIL
                        join a in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                        where a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                        && a.COMPANYID == companyId && a.DELETED == false && b.DELETED == false
                        select new LoanApplicationDetailViewModel()
                        {
                            requireCollateral = a.REQUIRECOLLATERAL,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            firstName = b.TBL_CUSTOMER.FIRSTNAME,
                            middleName = b.TBL_CUSTOMER.MIDDLENAME,
                            lastName = b.TBL_CUSTOMER.LASTNAME,
                            customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                            loanApplicationDetailId = b.LOANREVIEWAPPLICATIONID,
                            proposedProductId = b.PRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            approvedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            approvedAmount = b.APPROVEDAMOUNT,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? context.TBL_CUSTOMER_GROUP.Where(o => o.CUSTOMERGROUPID == a.CUSTOMERGROUPID).FirstOrDefault().GROUPNAME : "",
                            // customerAccountNumber =  b.TBL_CUSTOMER.TBL_CASA.PRODUCTACCOUNTNUMBER
                        });
            return data;
        }

        public List<LoanApplicationDetailViewModel> GetLmsLoanApplicationDetailsById(int loanApplicationId, int companyId)
        {
            return GetLmsLoanApplicationsDetails(companyId).Where(x => x.loanApplicationId == loanApplicationId).ToList();
        }

        public List<LoanApplicationDetailViewModel> GetLoanApplicationDetailsById(int loanApplicationId, int companyId)
        {
            return GetLoanApplicationsDetails(companyId).Where(x => x.loanApplicationId == loanApplicationId).ToList();
        }

        //public IEnumerable<LoanApplicationDetailViewModel> SearchLoanApplicationDetails(int companyId, string searchQuery)
        //{
        //    searchQuery = searchQuery?.Trim()?.ToLower();

        //    var allApplicationDetails = (from d in context.TBL_LOAN_APPLICATION_DETAIL
        //                                 join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
        //                                 join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
        //                                 where a.APPLICATIONREFERENCENUMBER.Trim().Contains(searchQuery)
        //                                   || c.FIRSTNAME.ToLower().StartsWith(searchQuery)
        //                                   || c.CUSTOMERCODE.ToLower().Contains(searchQuery)
        //                                 || c.MIDDLENAME.ToLower().StartsWith(searchQuery)
        //                                 || c.LASTNAME.ToLower().StartsWith(searchQuery)
        //                                 || a.TBL_CASA.PRODUCTACCOUNTNUMBER == searchQuery
        //                                 select new LoanApplicationDetailViewModel
        //                                 {
        //                                     loanApplicationId = d.LOANAPPLICATIONID,
        //                                     applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
        //                                     customerId = c.CUSTOMERID,
        //                                     firstName = c.FIRSTNAME,
        //                                     middleName = c.MIDDLENAME,
        //                                     lastName = c.LASTNAME,
        //                                     customerCode = c.CUSTOMERCODE,
        //                                     loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
        //                                     approvedProductId = d.APPROVEDPRODUCTID,
        //                                     productName = d.TBL_PRODUCT.PRODUCTNAME,
        //                                     approvedTenor = d.APPROVEDTENOR,
        //                                     approvedAmount = d.APPROVEDAMOUNT,
        //                                     approvedInterestRate = d.APPROVEDINTERESTRATE,
        //                                     productClassProcessId = a.PRODUCT_CLASS_PROCESSID,
        //                                     productClassId = (short?)a.PRODUCTCLASSID,
        //                                     applicationStatusId = a.APPLICATIONSTATUSID,
        //                                     applicationStatusPosition = a.TBL_LOAN_APPLICATION_STATUS.POSITION,
        //                                     approvalStatusId = a.APPROVALSTATUSID,
        //                                     applicationDate = a.APPLICATIONDATE,
        //                                     customerType = c.TBL_CUSTOMER_TYPE.NAME,
        //                                     branchName = a.TBL_BRANCH.BRANCHNAME,
        //                                     customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
        //                                     customerAccountNumber = a.TBL_CASA != null ? a.TBL_CASA.PRODUCTACCOUNTNUMBER : null,
        //                                     equityAmount = d.EQUITYAMOUNT,
        //                                     equityCasaAccountId = d.EQUITYCASAACCOUNTID,
        //                                     currencyId = d.CURRENCYID,
        //                                     currencyName = d.TBL_CURRENCY.CURRENCYNAME,
        //                                     currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
        //                                     exchangeRate = d.EXCHANGERATE,
        //                                     subSectorId = d.SUBSECTORID,
        //                                     proposedAmount = d.PROPOSEDAMOUNT,
        //                                     proposedInterestRate = d.PROPOSEDINTERESTRATE,
        //                                     proposedProductId = d.PROPOSEDPRODUCTID,
        //                                     proposedProductName = d.TBL_PRODUCT.PRODUCTNAME
        //                                 }).ToList();
        //    return allApplicationDetails;

        //}

        #endregion "Loan Applications Awaiting Checklist"

        //public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        //{
        //    int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CreditAppraisal, (int)OperationsEnum.ContigentLoanBooking ,
        //   (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
        //    (int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.LoanAvailment, (int)OperationsEnum.CreditCardsCashBacked, (int)OperationsEnum.CreditCardsCleanCards,
        //    (int)OperationsEnum.CreditCardsSalaryBacked, (int)OperationsEnum.TemporaryOverdraftRequest, (int)OperationsEnum.CashCollaterizedRequest};

        //    searchString = searchString.Trim().ToLower();


        //    var applications = (from x in context.TBL_LOAN_APPLICATION
        //                        join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
        //                        join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
        //                        // join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
        //                        //y.RESPONSESTAFFID == null && operations.Contains(y.OPERATIONID)
        //                        //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
        //                        where (x.APPLICATIONREFERENCENUMBER == searchString
        //                        || c.FIRSTNAME.ToLower().Contains(searchString)
        //                        || c.LASTNAME.ToLower().Contains(searchString)
        //                        || c.MIDDLENAME.ToLower().Contains(searchString)
        //                        || a.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE.ToLower() == searchString.ToLower()).Select(o => o.STAFFID).FirstOrDefault())
        //                        select new LoanApplicationViewModel
        //                        {
        //                            firstName = c.FIRSTNAME,
        //                            middleName = c.MIDDLENAME,
        //                            lastName = c.LASTNAME,
        //                            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
        //                            customerCode = c.CUSTOMERCODE,
        //                            applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
        //                            loanApplicationId = x.LOANAPPLICATIONID,
        //                            customerId = c.CUSTOMERID,
        //                            branchId = c.BRANCHID,
        //                            customerGroupId = x.CUSTOMERGROUPID,
        //                            loanTypeId = x.LOANAPPLICATIONTYPEID,
        //                            relationshipOfficerId = x.RELATIONSHIPOFFICERID,
        //                            relationshipManagerId = x.RELATIONSHIPMANAGERID,
        //                            applicationDate = x.APPLICATIONDATE,
        //                            applicationAmount = x.APPLICATIONAMOUNT,
        //                            approvedAmount = x.APPROVEDAMOUNT,
        //                            interestRate = x.INTERESTRATE,
        //                            applicationTenor = x.APPLICATIONTENOR,

        //                            submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
        //                            customerInfoValidated = x.CUSTOMERINFOVALIDATED,
        //                            isRelatedParty = x.ISRELATEDPARTY,
        //                            isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
        //                            approvalStatusId = (short)x.APPROVALSTATUSID,
        //                            approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

        //                            currentApprovalLevel = (from t in context.TBL_APPROVAL_TRAIL join al in context.TBL_APPROVAL_LEVEL on t.TOAPPROVALLEVELID equals al.APPROVALLEVELID where t.TARGETID == x.LOANAPPLICATIONID && t.OPERATIONID ==x.OPERATIONID orderby t.APPROVALTRAILID descending select al.LEVELNAME).FirstOrDefault(), //y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
        //                            approvalTrailId = (from t in context.TBL_APPROVAL_TRAIL join al in context.TBL_APPROVAL_LEVEL on t.TOAPPROVALLEVELID equals al.APPROVALLEVELID where t.TARGETID == x.LOANAPPLICATIONID && t.OPERATIONID == x.OPERATIONID orderby t.APPROVALTRAILID descending select t.APPROVALTRAILID).FirstOrDefault(),

        //                            //currentApprovalLevel = context.TBL_APPROVAL_TRAIL.Join(context.TBL_APPROVAL_LEVEL, at => at.TOAPPROVALLEVELID, al => al.APPROVALLEVELID, (at, al) => new { al.LEVELNAME }).Where(o => o.TARGETID == x.LOANAPPLICATIONID && o.OPERATIONID == x.OPERATIONID), //y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

        //                            // y.APPROVALTRAILID,
        //                            //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,
        //                            responsiblePerson = (from t in context.TBL_APPROVAL_TRAIL join st in context.TBL_STAFF on t.TOSTAFFID equals st.STAFFID where t.TARGETID == x.LOANAPPLICATIONID && t.OPERATIONID == x.OPERATIONID orderby t.APPROVALTRAILID descending select  st.FIRSTNAME + " " + st.LASTNAME).FirstOrDefault() ?? "n/a",
        //                           // y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

        //                            applicationStatusId = x.APPLICATIONSTATUSID,
        //                            applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
        //                            branchName = x.TBL_BRANCH.BRANCHNAME,
        //                            relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
        //                            relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
        //                            misCode = x.MISCODE,
        //                            customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
        //                            loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                            createdBy = x.CREATEDBY,
        //                            loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
        //                            loanTermSheetId = x.LOANTERMSHEETID,
        //                            operationId = x.OPERATIONID,
        //                            // accountNumber = ca.PRODUCTACCOUNTNUMBER,
        //                            isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == x.APPLICATIONREFERENCENUMBER).Any()
        //                        }).ToList();

        //    applications = applications.Where(x => x.applicationReferenceNumber != "-")
        //                    .GroupBy(p => p.applicationReferenceNumber)
        //                        .Select(g => g.First())
        //                            .ToList();

        //    return applications;

        //    /*var applications = context.TBL_LOAN_APPLICATION
        //            .Join(context.TBL_LOAN_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
        //            .Join(context.TBL_CUSTOMER, g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
        //            .Join(context.TBL_CASA, o => o.c.CUSTOMERID, s => s.CUSTOMERID, (o, s) => new { o, s })
        //            .GroupJoin(context.TBL_APPROVAL_TRAIL.Where(t => t.RESPONSESTAFFID == null && t.APPROVALSTATEID != (int)ApprovalState.Ended), q => q.o.g.a.LOANAPPLICATIONID, t => t.TARGETID, (q, t) => new { q, t })
        //            .SelectMany(x =>
        //                x.t.DefaultIfEmpty(),
        //                (x, y) => new LoanApplicationViewModel
        //                {
        //                    firstName = x.q.o.c.FIRSTNAME,
        //                    middleName = x.q.o.c.MIDDLENAME,
        //                    lastName = x.q.o.c.LASTNAME,
        //                    customerCode = x.q.o.c.CUSTOMERCODE,
        //                    applicationReferenceNumber = x.q.o.g.a.APPLICATIONREFERENCENUMBER,

        //                    loanApplicationId = x.q.o.g.a.LOANAPPLICATIONID,
        //                    customerId = x.q.o.g.a.CUSTOMERID,
        //                    branchId = x.q.o.g.a.BRANCHID,
        //                    customerGroupId = x.q.o.g.a.CUSTOMERGROUPID,
        //                    loanTypeId = x.q.o.g.a.LOANAPPLICATIONTYPEID,
        //                    relationshipOfficerId = x.q.o.g.a.RELATIONSHIPOFFICERID,
        //                    relationshipManagerId = x.q.o.g.a.RELATIONSHIPMANAGERID,
        //                    applicationDate = x.q.o.g.a.APPLICATIONDATE,
        //                    applicationAmount = x.q.o.g.a.APPLICATIONAMOUNT,
        //                    approvedAmount = x.q.o.g.a.APPROVEDAMOUNT,
        //                    interestRate = x.q.o.g.a.INTERESTRATE,
        //                    applicationTenor = x.q.o.g.a.APPLICATIONTENOR,

        //                    submittedForAppraisal = x.q.o.g.a.SUBMITTEDFORAPPRAISAL,
        //                    customerInfoValidated = x.q.o.g.a.CUSTOMERINFOVALIDATED,
        //                    isRelatedParty = x.q.o.g.a.ISRELATEDPARTY,
        //                    isPoliticallyExposed = x.q.o.g.a.ISPOLITICALLYEXPOSED,
        //                    approvalStatusId = (short)x.q.o.g.a.APPROVALSTATUSID,
        //                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.q.o.g.a.APPROVALSTATUSID).APPROVALSTATUSNAME,

        //                    currentApprovalLevel = y == null ? "n/a" : y.TBL_APPROVAL_LEVEL1.LEVELNAME,
        //                    approvalTrailId = y == null ? 0 : y.APPROVALTRAILID,
        //                    responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

        //                    applicationStatusId = x.q.o.g.a.APPLICATIONSTATUSID,
        //                    applicationStatus = x.q.o.g.a.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
        //                    branchName = x.q.o.g.a.TBL_BRANCH.BRANCHNAME,
        //                    relationshipOfficerName = x.q.o.g.a.TBL_STAFF.FIRSTNAME + " " + x.q.o.g.a.TBL_STAFF.MIDDLENAME + " " + x.q.o.g.a.TBL_STAFF.LASTNAME,
        //                    relationshipManagerName = x.q.o.g.a.TBL_STAFF1.FIRSTNAME + " " + x.q.o.g.a.TBL_STAFF1.MIDDLENAME + " " + x.q.o.g.a.TBL_STAFF1.LASTNAME,
        //                    misCode = x.q.o.g.a.MISCODE,
        //                    customerGroupName = x.q.o.g.a.CUSTOMERGROUPID.HasValue ? x.q.o.g.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
        //                    loanTypeName = x.q.o.g.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
        //                    createdBy = x.q.o.g.a.CREATEDBY,
        //                    loanPreliminaryEvaluationId = x.q.o.g.a.LOANPRELIMINARYEVALUATIONID,
        //                    operationId = x.q.o.g.a.OPERATIONID,
        //                    accountNumber = x.q.s.PRODUCTACCOUNTNUMBER,
        //                    isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == x.q.o.g.a.APPLICATIONREFERENCENUMBER).Any()
        //                })
        //            .Where(x => x.applicationReferenceNumber == searchString
        //                || x.firstName.ToLower().Contains(searchString)
        //                || x.lastName.ToLower().Contains(searchString)
        //                || x.middleName.ToLower().Contains(searchString)
        //                || x.customerCode.ToLower().Contains(searchString)
        //                || x.createdBy == context.TBL_STAFF.Where(o=>o.STAFFCODE== searchString.ToUpper()).Select(o=>o.STAFFID).FirstOrDefault()
        //                )
        //            ;

        //    var list = applications.ToList();
        //    applications = applications.OrderByDescending(x => x.approvalTrailId).GroupBy(x => x.applicationReferenceNumber).Select(x => x.FirstOrDefault());
        //    //var filteredList = applications.ToList();
        //    return applications; */

        //}



        //================================ by benjamin =======================

        public List<LoanApplicationViewModel> Search(string searchString, int staffId = 0)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var now = DateTime.Now;
                TBL_STAFF_RELIEF relieverStaff = new TBL_STAFF_RELIEF();
                var relifestaff = 0;
                if (staffId != 0)
                {
                    relieverStaff = context.TBL_STAFF_RELIEF
                           .FirstOrDefault(x => x.DELETED == false
                               && x.STAFFID == staffId
                               && x.STARTDATE <= now
                               && x.ENDDATE >= now
                               && x.ISACTIVE == true
                           );
                    if (relieverStaff != null)
                    {
                        relifestaff = relieverStaff.RELIEFSTAFFID;
                    }
                }
                else
                {
                    relifestaff = 0;
                }


                int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CreditAppraisal,(int)OperationsEnum.CreditCardsCashBacked,
                     (int)OperationsEnum.CreditCardsCleanCards, (int)OperationsEnum.AdhocApproval,
                    (int)OperationsEnum.CreditCardsSalaryBacked, (int)OperationsEnum.TemporaryOverdraftRequest, (int)OperationsEnum.CashCollaterizedRequest };

                //int[] operationsLms = {
                //    (int)OperationsEnum.ContigentLoanBooking ,
                //   (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
                //    (int)OperationsEnum.CommercialLoanBooking};


                searchString = searchString.Trim().ToLower();


                var applications = (from x in context.TBL_LOAN_APPLICATION
                                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                    join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                    let bizUnit = context.TBL_PROFILE_BUSINESS_UNIT.FirstOrDefault(u => u.BUSINESSUNITID == c.BUSINESSUNTID)
                                    let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.OWNEDBY)
                                    let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == x.FLOWCHANGEID)
                                    //join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
                                    where
                               //y.RESPONSESTAFFID == null
                               // && operations.Contains(y.OPERATIONID)
                               //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
                               // && 
                               (x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                            || c.FIRSTNAME.ToLower().Contains(searchString)
                            || c.LASTNAME.ToLower().Contains(searchString)
                            || c.MIDDLENAME.ToLower().Contains(searchString)
                            || bizUnit.BUSINESSUNITSHORTCODE.ToLower().Contains(searchString)
                            || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                    select new LoanApplicationViewModel
                                    {
                                        firstName = c.FIRSTNAME,
                                        middleName = c.MIDDLENAME,
                                        lastName = c.LASTNAME,
                                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                        customerCode = c.CUSTOMERCODE,
                                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                        loanApplicationId = x.LOANAPPLICATIONID,
                                        loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                        productName = p.PRODUCTNAME,
                                        customerId = c.CUSTOMERID,
                                        branchId = c.BRANCHID,
                                        customerGroupId = x.CUSTOMERGROUPID,
                                        loanTypeId = x.LOANAPPLICATIONTYPEID,
                                        relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                        relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                        creatorName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                        applicationDate = x.APPLICATIONDATE,
                                        applicationAmount = x.APPLICATIONAMOUNT,
                                        bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                        bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                        //applicationAmount = x.APPLICATIONAMOUNT,
                                        approvedAmount = x.APPROVEDAMOUNT,
                                        interestRate = x.INTERESTRATE,
                                        applicationTenor = x.APPLICATIONTENOR,
                                        productClassId = x.PRODUCTCLASSID,
                                        productId = (short)(x.PRODUCTID ?? 0),
                                        loanInformation = x.LOANINFORMATION,
                                        productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                        submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                        customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                        isRelatedParty = x.ISRELATEDPARTY,
                                        isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                        isInvestmentGrade = x.ISINVESTMENTGRADE,
                                        isProjectRelatedLoan = x.ISPROJECTRELATED,
                                        isOnLending = x.ISONLENDING,
                                        isInterventionFunds = x.ISINTERVENTIONFUNDS,
                                        isORRBasedApproval = x.ISORRBASEDAPPROVAL,
                                        isadhocapplication = x.ISADHOCAPPLICATION,
                                        approvalStatusId = (short)x.APPROVALSTATUSID,
                                        approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                        isSkipAppraisalEnabled = jumpsToDrawDown != null ? jumpsToDrawDown.ISSKIPPROCESSENABLED : false,
                                        //currentApprovalLevel = context.TBL_APPROVAL_TRAIL.Where(o=>o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e=>e.FROMAPPROVALLEVELID != null ? e.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a").OrderByDescending(r => r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                        //approvalTrailId = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e => e.APPROVALTRAILID).OrderByDescending(r => r).FirstOrDefault(),//y.APPROVALTRAILID,
                                        //responsiblePerson = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(y => y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME).OrderByDescending(r=>r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

                                        //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                        applicationStatusId = x.APPLICATIONSTATUSID,
                                        applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                        branchName = x.TBL_BRANCH.BRANCHNAME,
                                        relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                        relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                        misCode = x.MISCODE,
                                        customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                        loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                        createdBy = x.OWNEDBY,
                                        loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                                        operationId = x.OPERATIONID,
                                        owner = x.OWNEDBY == staffId ? true : relifestaff != 0 ? true : false,
                                        // accountNumber = ca.PRODUCTACCOUNTNUMBER,
                                        isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),
                                        isFacilityCreated = a.ISFACILITYCREATED,
                                        apiRequestId = x.APIREQUESTID,
                                        isEmployerRelated = x.ISEMPLOYERRELATED,
                                        employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == x.RELATEDEMPLOYERID).EMPLOYER_NAME
                                    }).ToList();

                var groupApplications = (from x in context.TBL_LOAN_APPLICATION
                                         join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                         join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                                         join c in context.TBL_CUSTOMER_GROUP on x.CUSTOMERGROUPID equals c.CUSTOMERGROUPID
                                         let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.OWNEDBY)
                                         let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == x.FLOWCHANGEID)
                                         //join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
                                         where
                                    //y.RESPONSESTAFFID == null
                                    // && operations.Contains(y.OPERATIONID)
                                    //    && y.APPROVALSTATEID != (int)ApprovalState.Ended
                                    // && 
                                    (x.APPLICATIONREFERENCENUMBER == searchString
                                 || c.GROUPNAME.ToLower().Contains(searchString)
                                 || c.GROUPCODE.ToLower().Contains(searchString)
                                 || c.GROUPDESCRIPTION.ToLower().Contains(searchString)
                                 || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                         select new LoanApplicationViewModel
                                         {
                                             //firstName = c.FIRSTNAME,
                                             //middleName = c.MIDDLENAME,
                                             //lastName = c.LASTNAME,
                                             customerName = c.GROUPNAME,
                                             customerCode = c.GROUPCODE,
                                             applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                             loanApplicationId = x.LOANAPPLICATIONID,
                                             loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                             productName = p.PRODUCTNAME,
                                             customerId = null,
                                             branchId = x.BRANCHID,
                                             bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                             bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANAPPLICATIONDETAILID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                             customerGroupId = x.CUSTOMERGROUPID,
                                             loanTypeId = x.LOANAPPLICATIONTYPEID,
                                             relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                             relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                             applicationDate = x.APPLICATIONDATE,
                                             applicationAmount = x.APPLICATIONAMOUNT,
                                             approvedAmount = x.APPROVEDAMOUNT,
                                             interestRate = x.INTERESTRATE,
                                             applicationTenor = x.APPLICATIONTENOR,
                                             productClassId = x.PRODUCTCLASSID,
                                             productId = (short)(x.PRODUCTID ?? 0),
                                             loanInformation = x.LOANINFORMATION,

                                             productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                             submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                             customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                             isRelatedParty = x.ISRELATEDPARTY,
                                             isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                             isInvestmentGrade = x.ISINVESTMENTGRADE,
                                             isProjectRelatedLoan = x.ISPROJECTRELATED,
                                             isOnLending = x.ISONLENDING,
                                             isInterventionFunds = x.ISINTERVENTIONFUNDS,
                                             isORRBasedApproval = x.ISORRBASEDAPPROVAL,
                                             isadhocapplication = x.ISADHOCAPPLICATION,
                                             approvalStatusId = (short)x.APPROVALSTATUSID,
                                             approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                             isSkipAppraisalEnabled = jumpsToDrawDown != null ? jumpsToDrawDown.ISSKIPPROCESSENABLED : false,

                                             //currentApprovalLevel = context.TBL_APPROVAL_TRAIL.Where(o=>o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e=>e.FROMAPPROVALLEVELID != null ? e.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a").OrderByDescending(r => r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                             //approvalTrailId = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(e => e.APPROVALTRAILID).OrderByDescending(r => r).FirstOrDefault(),//y.APPROVALTRAILID,
                                             //responsiblePerson = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).Select(y => y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME).OrderByDescending(r=>r).FirstOrDefault(), // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

                                             //responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                             applicationStatusId = x.APPLICATIONSTATUSID,
                                             applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                             branchName = x.TBL_BRANCH.BRANCHNAME,
                                             relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                             creatorName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                             relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                             misCode = x.MISCODE,
                                             customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                             loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                             createdBy = x.OWNEDBY,
                                             loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                                             operationId = x.OPERATIONID,
                                             owner = x.OWNEDBY == staffId ? true : relifestaff != 0 ? true : false,
                                             // accountNumber = ca.PRODUCTACCOUNTNUMBER,
                                             isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),
                                             isFacilityCreated = a.ISFACILITYCREATED,
                                             apiRequestId = x.APIREQUESTID,
                                             isEmployerRelated = x.ISEMPLOYERRELATED,
                                             employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == x.RELATEDEMPLOYERID).EMPLOYER_NAME
                                         }).ToList();

                //var allRecord = applications.Union(groupApplications).GroupBy(a => a.loanApplicationId).Select(a => a.FirstOrDefault()).ToList();
                var allRecord = applications.Union(groupApplications).GroupBy(a => a.loanApplicationId).Select(a => a.FirstOrDefault()).ToList();


                //int[] bookedLoanOperations = { (int) OperationsEnum.RevolvingLoanBooking, (int) OperationsEnum.TermLoanBooking, (int) OperationsEnum.ContigentLoanBooking };

                foreach (var x in allRecord)
                {
                    x.operationName = GetWorkFlowName(x.operationId.Value, x.productClassId, x.productId);
                    var appRecord = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.loanApplicationId && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
                    if (appRecord != null)
                    {
                        var singleRec = appRecord;
                        var status = context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == x.applicationStatusId).APPLICATIONSTATUSNAME;
                        if (singleRec.FROMAPPROVALLEVELID == singleRec.TOAPPROVALLEVELID)
                        {
                            if (singleRec.LOOPEDSTAFFID > 0)
                            {
                                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == singleRec.LOOPEDSTAFFID);
                                x.currentApprovalLevel = context.TBL_STAFF_ROLE.FirstOrDefault(r => r.STAFFROLEID == staff.STAFFROLEID).STAFFROLENAME;
                                x.responsiblePerson = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;

                            }
                            else
                            {
                                x.currentApprovalLevel = singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME;
                                x.responsiblePerson = singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;
                            }
                        }
                        else
                        {
                            x.currentApprovalLevel = singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "CLICK VIEW FOR DRAWDOWN DETAILS" : x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationRejected ? "REJECTED APPLICATION" : status; // y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                            x.responsiblePerson = singleRec.TOSTAFFID == null ? (singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : (x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : (x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "CLICK VIEW FOR DRAWDOWN DETAILS" : x.applicationStatusId == (short)LoanApplicationStatusEnum.ApplicationRejected ? "REJECTED APPLICATION" : status))) : singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                        }
                        x.currentApprovalLevelId = singleRec.TOAPPROVALLEVELID > 0 ? singleRec.TOAPPROVALLEVELID : 0;
                        x.approvalTrailId = singleRec.APPROVALTRAILID;//y.APPROVALTRAILID,
                        //x.responsiblePerson = singleRec.TOSTAFFID == null ? singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a" : singleRec.TBL_STAFF1.STAFFCODE + " - " + singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",

                        x.toStaffId = singleRec.TOSTAFFID;
                        x.currentOperationId = singleRec.OPERATIONID;
                    }
                    else
                    {
                        if (x.bookingRequestId > 0 || x.isSkipAppraisalEnabled)
                        {
                            x.currentApprovalLevel = "CLICK VIEW FOR DRAWDOWN DETAILS";
                            x.responsiblePerson = "CLICK VIEW FOR DRAWDOWN DETAILS";
                        }
                        else
                        {
                            x.currentApprovalLevel = x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : "NOT YET IN APPRAISAL";
                            x.responsiblePerson = x.isFacilityCreated == true ? "FIRST TRANCHE DISBURSEMENT HAS OCCURRED" : "WITH ACCOUNT OFFICER";
                        }
                    }
                    VerifyDrawDownStatus(x);
                }

                return allRecord;
            }
        }


        public List<LoanReviewApplicationViewModel> LmsSearch(string searchString, int staffId = 0)
        {
            
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var now = DateTime.Now;
                    TBL_STAFF_RELIEF relieverStaff = new TBL_STAFF_RELIEF();
                    var relifestaff = 0;
                    if (staffId != 0)
                    {
                        relieverStaff = context.TBL_STAFF_RELIEF
                               .FirstOrDefault(x => x.DELETED == false
                                   && x.STAFFID == staffId
                                   && x.STARTDATE <= now
                                   && x.ENDDATE >= now
                                   && x.ISACTIVE == true
                               );
                        if (relieverStaff != null)
                        {
                            relifestaff = relieverStaff.RELIEFSTAFFID;
                        }
                    }
                    else
                    {
                        relifestaff = 0;
                    }


                    int[] operationsLms = {
                    (int)OperationsEnum.ContigentLoanBooking ,
                   (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
                    (int)OperationsEnum.CommercialLoanBooking};


                    searchString = searchString.Trim().ToLower();


                    var applications = (from x in context.TBL_LMSR_APPLICATION
                                        join a in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                        join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                        join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                        let bizUnit = context.TBL_PROFILE_BUSINESS_UNIT.FirstOrDefault(u => u.BUSINESSUNITID == c.BUSINESSUNTID)
                                        let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.CREATEDBY)
                                        where

                                   (x.APPLICATIONREFERENCENUMBER.Trim() == searchString
                                || c.FIRSTNAME.ToLower().Contains(searchString)
                                || c.LASTNAME.ToLower().Contains(searchString)
                                || c.MIDDLENAME.ToLower().Contains(searchString)
                                || bizUnit.BUSINESSUNITSHORTCODE.ToLower().Contains(searchString)
                                || x.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                        select new LoanReviewApplicationViewModel
                                        {
                                            firstName = c.FIRSTNAME,
                                            middleName = c.MIDDLENAME,
                                            lastName = c.LASTNAME,
                                            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                            customerCode = c.CUSTOMERCODE,
                                            applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                            loanApplicationId = x.LOANAPPLICATIONID,
                                            loanApplicationDetailId = a.LOANREVIEWAPPLICATIONID,
                                            productName = p.PRODUCTNAME,
                                            customerId = c.CUSTOMERID,
                                            branchId = c.BRANCHID,
                                            customerGroupId = x.CUSTOMERGROUPID,
                                            relationshipOfficerId = x.CREATEDBY,
                                            relationshipManagerId = x.CREATEDBY,
                                            creatorName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                            applicationDate = x.APPLICATIONDATE,
                                            applicationAmount = (decimal)x.APPROVEDAMOUNT,
                                            bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANREVIEWAPPLICATIONID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                            bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANREVIEWAPPLICATIONID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                            approvedAmount = (decimal)x.APPROVEDAMOUNT,
                                            interestRate = a.APPROVEDINTERESTRATE,
                                            applicationTenor = a.APPROVEDTENOR,
                                            productClassId = x.PRODUCTCLASSID,
                                            productId = (short)(x.PRODUCTID ?? 0),
                                            loanInformation = a.REVIEWDETAILS,
                                            productClassProcessId = (short)x.PRODUCT_CLASS_PROCESSID,

                                            
                                            approvalStatusId = (short)x.APPROVALSTATUSID,
                                            approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                            applicationStatusId = x.APPLICATIONSTATUSID,
                                            applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                            branchName = x.TBL_BRANCH.BRANCHNAME,
                                            relationshipOfficerName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                            relationshipManagerName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                            misCode = context.TBL_STAFF.Where(x => x.STAFFID == x.CREATEDBY).Select(x => x.MISCODE).FirstOrDefault(),
                                            customerGroupName = x.CUSTOMERGROUPID.HasValue ? context.TBL_CUSTOMER_GROUP.Where(x => x.CUSTOMERGROUPID == x.CUSTOMERGROUPID).Select(x => x.GROUPNAME).FirstOrDefault() : "",
                                            loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                            createdBy = x.CREATEDBY,
                                            operationId = x.OPERATIONID,
                                            owner = x.CREATEDBY == staffId ? true : false,
                                            isOnLending = x.ISONLENDING,
                                            isInterventionFunds = x.ISINTERVENTIONFUNDS,
                                            employer = ""
                                        }).ToList();

                    var groupApplications = (from x in context.TBL_LMSR_APPLICATION
                                             join a in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                             join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                                             join c in context.TBL_CUSTOMER_GROUP on x.CUSTOMERGROUPID equals c.CUSTOMERGROUPID
                                             let creatorStaff = context.TBL_STAFF.FirstOrDefault(f => f.STAFFID == x.CREATEDBY)
                                             where

                                                (x.APPLICATIONREFERENCENUMBER == searchString
                                             || c.GROUPNAME.ToLower().Contains(searchString)
                                             || c.GROUPCODE.ToLower().Contains(searchString)
                                             || c.GROUPDESCRIPTION.ToLower().Contains(searchString)
                                             || x.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                             select new LoanReviewApplicationViewModel
                                             {
                                                 customerName = c.GROUPNAME,
                                                 customerCode = c.GROUPCODE,
                                                 applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                                 loanApplicationId = x.LOANAPPLICATIONID,
                                                 loanApplicationDetailId = a.LOANREVIEWAPPLICATIONID,
                                                 productName = p.PRODUCTNAME,
                                                 customerId = c.CUSTOMERGROUPID,
                                                 branchId = x.BRANCHID,
                                                 customerGroupId = x.CUSTOMERGROUPID,
                                                 relationshipOfficerId = x.CREATEDBY,
                                                 relationshipManagerId = x.CREATEDBY,
                                                 creatorName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                                 applicationDate = x.APPLICATIONDATE,
                                                 applicationAmount = (decimal)x.APPROVEDAMOUNT,
                                                 bookingOperationId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANREVIEWAPPLICATIONID).Select(r => r.OPERATIONID).FirstOrDefault(),
                                                 bookingRequestId = context.TBL_LOAN_BOOKING_REQUEST.Where(r => r.LOANAPPLICATIONDETAILID == a.LOANREVIEWAPPLICATIONID).Select(r => r.LOAN_BOOKING_REQUESTID).FirstOrDefault(),
                                                 approvedAmount = (decimal)x.APPROVEDAMOUNT,
                                                 interestRate = a.APPROVEDINTERESTRATE,
                                                 applicationTenor = a.APPROVEDTENOR,
                                                 productClassId = x.PRODUCTCLASSID,
                                                 productId = (short)(x.PRODUCTID ?? 0),
                                                 loanInformation = a.REVIEWDETAILS,
                                                 productClassProcessId = (short)x.PRODUCT_CLASS_PROCESSID,

                                                 approvalStatusId = (short)x.APPROVALSTATUSID,
                                                 approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                                 applicationStatusId = x.APPLICATIONSTATUSID,
                                                 applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(),
                                                 branchName = x.TBL_BRANCH.BRANCHNAME,
                                                 relationshipOfficerName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                                 relationshipManagerName = creatorStaff.FIRSTNAME + " " + creatorStaff.MIDDLENAME + " " + creatorStaff.LASTNAME,
                                                 misCode = context.TBL_STAFF.Where(x => x.STAFFID == x.CREATEDBY).Select(x => x.MISCODE).FirstOrDefault(),
                                                 customerGroupName = x.CUSTOMERGROUPID.HasValue ? context.TBL_CUSTOMER_GROUP.Where(x => x.CUSTOMERGROUPID == x.CUSTOMERGROUPID).Select(x => x.GROUPNAME).FirstOrDefault() : "",
                                                 loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                                 createdBy = x.CREATEDBY,
                                                 operationId = x.OPERATIONID,
                                                 owner = x.CREATEDBY == staffId ? true : false,
                                                 isOnLending = x.ISONLENDING,
                                                 isInterventionFunds = x.ISINTERVENTIONFUNDS,

                                                 employer = ""
                                             }).ToList();

                    var allRecord = applications.Union(groupApplications).GroupBy(a => a.loanApplicationId).Select(a => a.FirstOrDefault()).ToList();
                    foreach (var x in allRecord)
                    {
                        x.operationName = GetWorkFlowName(x.operationId.Value, x.productClassId, x.productId);
                        var appRecord = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.loanApplicationId && operationsLms.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
                        if (appRecord != null)
                        {
                            var singleRec = appRecord;
                            var status = context.TBL_LOAN_APPLICATION_STATUS.FirstOrDefault(s => s.APPLICATIONSTATUSID == x.applicationStatusId).APPLICATIONSTATUSNAME;
                            if (singleRec.FROMAPPROVALLEVELID == singleRec.TOAPPROVALLEVELID)
                            {
                                if (singleRec.LOOPEDSTAFFID > 0)
                                {
                                    var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == singleRec.LOOPEDSTAFFID);
                                    x.currentApprovalLevel = context.TBL_STAFF_ROLE.FirstOrDefault(r => r.STAFFROLEID == staff.STAFFROLEID).STAFFROLENAME;
                                    x.responsiblePerson = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;

                                }
                                else
                                {
                                    x.currentApprovalLevel = singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME;
                                    x.responsiblePerson = singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;
                                }
                            }
                            x.currentApprovalLevelId = singleRec.TOAPPROVALLEVELID > 0 ? singleRec.TOAPPROVALLEVELID : 0;
                            x.approvalTrailId = singleRec.APPROVALTRAILID;
                            x.toStaffId = singleRec.TOSTAFFID;
                            x.currentOperationId = singleRec.OPERATIONID;
                        }

                    }

                    return allRecord;
                }
            
        }

        private bool VerifyDrawDownStatus(LoanApplicationViewModel loan)
        {
            var isAtDrawdown = (loan.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted
                             || loan.applicationStatusId == (short)LoanApplicationStatusEnum.BookingRequestInitiated
                             || loan.applicationStatusId == (short)LoanApplicationStatusEnum.BookingRequestCompleted
                             || loan.applicationStatusId == (short)LoanApplicationStatusEnum.LoanBookingInProgress
                             || loan.applicationStatusId == (short)LoanApplicationStatusEnum.LoanBookingCompleted
                             || loan.currentApprovalLevel.Contains("CLICK VIEW FOR DRAWDOWN DETAILS")); 
            if (isAtDrawdown)
            {
                loan.currentApprovalLevel = "CLICK VIEW FOR DRAWDOWN DETAILS";
                loan.responsiblePerson = "CLICK VIEW FOR DRAWDOWN DETAILS";
            }
            return isAtDrawdown;
        }

        private string GetWorkFlowName(int operationId, int? productClassId, int? productId)
        {
            var operationName = String.Empty;

            if (operationId == (int)OperationsEnum.CreditAppraisal)
            {
                if (productId > 0)
                {
                    var workflowMapping = context.TBL_APPROVAL_GROUP_MAPPING.Where(m => m.OPERATIONID == operationId && m.PRODUCTCLASSID == productClassId && m.PRODUCTID == productId).ToList();
                    if (workflowMapping != null)
                    {
                        operationName = context.TBL_PRODUCT.FirstOrDefault(o => o.PRODUCTID == productId).PRODUCTNAME;
                        return operationName;
                    }
                }
                if (productClassId > 0)
                {
                    var workflowMapping = context.TBL_APPROVAL_GROUP_MAPPING.Where(m => m.OPERATIONID == operationId && m.PRODUCTCLASSID == productClassId && m.PRODUCTID == null).ToList();
                    if (workflowMapping != null)
                    {
                        operationName = context.TBL_PRODUCT_CLASS.FirstOrDefault(o => o.PRODUCTCLASSID == productClassId).PRODUCTCLASSNAME;
                        return operationName;
                    }
                }
            }
            operationName = context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID == operationId).OPERATIONNAME;
            return operationName;
        }

        public List<LoanApplicationViewModel> SearchDrawDown(string searchString, int staffId = 0)
        {
            int[] operations = { (int)OperationsEnum.CreditCardDrawdownRequest,(int)OperationsEnum.IndividualDrawdownRequest,
                (int)OperationsEnum.CorporateDrawdownRequest,(int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.ContigentLoanBooking,
                (int)OperationsEnum.RevolvingLoanBooking,(int)OperationsEnum.TermLoanBooking,(int)OperationsEnum.ForeignExchangeLoanBooking,
                (int)OperationsEnum.RevolvingTranchDisbursement
            };
            searchString = searchString.Trim().ToLower();
            var applications = (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join r in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                where
                           (x.APPLICATIONREFERENCENUMBER == searchString
                        || c.FIRSTNAME.ToLower().Contains(searchString)
                        || c.LASTNAME.ToLower().Contains(searchString)
                        || c.MIDDLENAME.ToLower().Contains(searchString)
                        || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                select new LoanApplicationViewModel
                                {
                                    firstName = c.FIRSTNAME,
                                    middleName = c.MIDDLENAME,
                                    lastName = c.LASTNAME,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    customerId = c.CUSTOMERID,
                                    bookingRequestId = r.LOAN_BOOKING_REQUESTID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    bookingOperationId = r.OPERATIONID,
                                    branchId = c.BRANCHID,
                                    customerGroupId = x.CUSTOMERGROUPID,
                                    loanTypeId = x.LOANAPPLICATIONTYPEID,
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = x.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    applicationTenor = x.APPLICATIONTENOR,
                                    productClassId = x.PRODUCTCLASSID,
                                    loanInformation = x.LOANINFORMATION,
                                    productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                    submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                    customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                    isRelatedParty = x.ISRELATEDPARTY,
                                    isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                    approvalStatusId = (short)x.APPROVALSTATUSID,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    applicationStatusId = x.APPLICATIONSTATUSID,
                                    applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                    branchName = x.TBL_BRANCH.BRANCHNAME,
                                    relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                    relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                    misCode = x.MISCODE,
                                    customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                    loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                    createdBy = x.OWNEDBY,
                                    loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                                    operationId = x.OPERATIONID,
                                    //owner = x.CREATEDBY == staffId ? true : relifestaff != 0 ? true : false,
                                    isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),
                                    isFacilityCreated = a.ISFACILITYCREATED
                                }).ToList();

            var groupApplications = (from x in context.TBL_LOAN_APPLICATION
                                     join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                     join c in context.TBL_CUSTOMER_GROUP on x.CUSTOMERGROUPID equals c.CUSTOMERGROUPID
                                     join r in context.TBL_LOAN_BOOKING_REQUEST on a.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID
                                     where
                                (x.APPLICATIONREFERENCENUMBER == searchString
                             || c.GROUPNAME.ToLower().Contains(searchString)
                             || c.GROUPCODE.ToLower().Contains(searchString)
                             || c.GROUPDESCRIPTION.ToLower().Contains(searchString)
                             || x.OWNEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                     select new LoanApplicationViewModel
                                     {
                                         customerName = c.GROUPNAME,
                                         customerCode = c.GROUPCODE,
                                         applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                         loanApplicationId = x.LOANAPPLICATIONID,
                                         customerId = null,
                                         branchId = x.BRANCHID,
                                         customerGroupId = x.CUSTOMERGROUPID,
                                         loanTypeId = x.LOANAPPLICATIONTYPEID,
                                         relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                         relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                         applicationDate = x.APPLICATIONDATE,
                                         applicationAmount = x.APPLICATIONAMOUNT,
                                         approvedAmount = x.APPROVEDAMOUNT,
                                         interestRate = x.INTERESTRATE,
                                         applicationTenor = x.APPLICATIONTENOR,
                                         loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                         bookingOperationId = r.OPERATIONID,
                                         productClassId = x.PRODUCTCLASSID,
                                         productClassProcessId = x.TBL_PRODUCT_CLASS_PROCESS.PRODUCT_CLASS_PROCESSID,
                                         submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                         customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                         isRelatedParty = x.ISRELATEDPARTY,
                                         isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                         approvalStatusId = (short)x.APPROVALSTATUSID,
                                         approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                         applicationStatusId = x.APPLICATIONSTATUSID,
                                         applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new 
                                         branchName = x.TBL_BRANCH.BRANCHNAME,
                                         relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                         relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                         misCode = x.MISCODE,
                                         customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                         loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                         createdBy = x.OWNEDBY,
                                         loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                                         operationId = x.OPERATIONID,
                                         bookingRequestId = r.LOAN_BOOKING_REQUESTID,
                                         //owner = x.CREATEDBY == staffId ? true : relifestaff != 0 ? true : false,
                                         isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),
                                         isFacilityCreated = a.ISFACILITYCREATED
                                     }).ToList();
            var allRecord = applications.Union(groupApplications).ToList();

            foreach (var x in allRecord)
            {
                var appRecord = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.bookingRequestId && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).FirstOrDefault();
                if (appRecord != null)
                {
                    var singleRec = appRecord;
                    x.currentApprovalLevel = singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : x.isFacilityCreated == true ? "DISBURSED" : x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "DRAWDOWN" : "N/A";
                    x.approvalTrailId = singleRec.APPROVALTRAILID;

                    x.responsiblePerson = singleRec.TOSTAFFID == null ? singleRec.TOAPPROVALLEVELID != null ? singleRec.TBL_APPROVAL_LEVEL1.LEVELNAME : x.isFacilityCreated == true ? "DISBURSED" : x.applicationStatusId == (short)LoanApplicationStatusEnum.AvailmentCompleted ? "DRAWDOWN" : "N/A" : singleRec.TBL_STAFF1.FIRSTNAME + " " + singleRec.TBL_STAFF1.MIDDLENAME + " " + singleRec.TBL_STAFF1.LASTNAME;// y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                    x.currentOperationId = singleRec.OPERATIONID;
                }
                else
                {
                    x.currentApprovalLevel = x.isFacilityCreated == true ? "DISBURSED" : "N/A";
                    x.responsiblePerson = x.isFacilityCreated == true ? "DISBURSED" : "N/A";
                }

            }
            return allRecord;

        }

        public List<WorkflowTrackerViewModel> SearchBookedLoans(int loanApplicationDetailId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var approvalOperations = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Select(f => f.OPERATIONID).ToList();
                var applicationStatuses = new int[] { (int)LoanApplicationStatusEnum.AvailmentCompleted, (int)LoanApplicationStatusEnum.CAMCompleted };
                int[] operations = new int[] {  (int)OperationsEnum.TermLoanBooking, (int)OperationsEnum.IndividualDrawdownRequest, (int)OperationsEnum.CorporateDrawdownRequest,
                                                (int)OperationsEnum.CreditCardDrawdownRequest,
                                                (int)OperationsEnum.RevolvingLoanBooking, (int)OperationsEnum.ContigentLoanBooking};
                int[] otherApprovalOperations = new int[] { (int)OperationsEnum.AdhocApproval, (int)OperationsEnum.OfferLetterApproval};
                approvalOperations.AddRange(otherApprovalOperations);
                int[] approvals = new int[] { (int)ApprovalStatusEnum.Approved, (int)ApprovalStatusEnum.Disapproved, (int)ApprovalStatusEnum.Authorised };
                List<WorkflowTrackerViewModel> approvalRecord = new List<WorkflowTrackerViewModel>();
                
                var drawdownsNotYetInitiated = (
                              from d in context.TBL_LOAN_APPLICATION_DETAIL
                              join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                              join a in context.TBL_APPROVAL_TRAIL on e.LOANAPPLICATIONID equals a.TARGETID
                              join b in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals b.APPROVALSTATUSID
                              join c in context.TBL_APPROVAL_STATE on a.APPROVALSTATEID equals c.APPROVALSTATEID
                              join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                              join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                              let isInitiated = context.TBL_LOAN_BOOKING_REQUEST.Any(r => r.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID)
                              let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == e.FLOWCHANGEID)
                              let creator = e.TBL_STAFF
                              where (
                              d.LOANAPPLICATIONDETAILID == loanApplicationDetailId && approvalOperations.Contains(a.OPERATIONID) && !isInitiated
                              && ((a.APPROVALSTATEID == (int)ApprovalState.Ended && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                              && applicationStatuses.Contains(e.APPLICATIONSTATUSID)) || (jumpsToDrawDown != null && jumpsToDrawDown.ISSKIPPROCESSENABLED))
                              )
                              orderby a.APPROVALTRAILID descending
                              select (new WorkflowTrackerViewModel
                              {
                                  approvalStatusId = a.APPROVALSTATUSID,
                                  operationName = a.TBL_OPERATIONS.OPERATIONNAME,
                                  currentLevel = creator.TBL_STAFF_ROLE.STAFFROLENAME,
                                  approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                                  applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                  requestStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                                  responseStaffName = (a.RESPONSESTAFFID == null) ? "N/A" : a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                  responsibleStaffName = creator.LASTNAME + " " + creator.MIDDLENAME + " " + creator.FIRSTNAME,
                                  comment = a.COMMENT,
                                  systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                                  requestApprovalLevel = a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  responseApprovalLevel = a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  customerName = d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                  customerDivisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                  //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID == d.CUSTOMERID).Select(cu=>cu.LASTNAME + " " + cu.MIDDLENAME + cu.LASTNAME).FirstOrDefault(),
                                  responseDate = a.RESPONSEDATE.HasValue ? (DateTime)a.RESPONSEDATE : (DateTime)(DateTime.Now),
                                  arrivalDate = a.ARRIVALDATE,
                                  applicationDate = e.DATETIMECREATED,
                                  amount = d.APPROVEDAMOUNT,
                                  TargetId = a.TARGETID,
                                  branchName = e.TBL_BRANCH.BRANCHNAME + " (" + e.TBL_BRANCH.BRANCHCODE + ") ",
                                  loanApplicationId = d.LOANAPPLICATIONID,
                                  loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                  loanBookingRequestId = 0,
                                  approvalTrailId = a.APPROVALTRAILID,
                                  productNames = p.PRODUCTNAME,
                                  apiRequestId = e.APIREQUESTID,
                                  loopedStaffId = a.LOOPEDSTAFFID,
                                  toStaffId = a.TOSTAFFID,
                                  fromApprovalLevelId = a.FROMAPPROVALLEVELID,
                                  toApprovalLevelId = a.TOAPPROVALLEVELID,
                                  currentApprovalLevelId = a.TOAPPROVALLEVELID,
                                  requestStaffId = a.REQUESTSTAFFID,
                                  isSkipAppraisalEnabled = jumpsToDrawDown != null ? jumpsToDrawDown.ISSKIPPROCESSENABLED : false,
                              })
                           ).OrderByDescending(o => o.approvalTrailId).ToList();

                var record = (
                              from d in context.TBL_LOAN_APPLICATION_DETAIL
                              join bo in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals bo.LOANAPPLICATIONDETAILID
                              join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                              let jumpsToDrawDown = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(f => f.FLOWCHANGEID == e.FLOWCHANGEID)
                              let current = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == bo.LOAN_BOOKING_REQUESTID && operations.Contains(t.OPERATIONID)).
                                            OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault()
                              join cust in context.TBL_CUSTOMER on d.CUSTOMERID equals cust.CUSTOMERID
                              join p in context.TBL_PRODUCT on bo.PRODUCTID equals p.PRODUCTID
                              where 
                              (d.LOANAPPLICATIONDETAILID == loanApplicationDetailId)
                              orderby bo.LOAN_BOOKING_REQUESTID
                              select (new WorkflowTrackerViewModel
                              {
                                  approvalStatusId = current.APPROVALSTATUSID,
                                  operationName = current.TBL_OPERATIONS.OPERATIONNAME,
                                  currentLevel = context.TBL_APPROVAL_LEVEL.Where(cl => cl.APPROVALLEVELID == current.TOAPPROVALLEVELID).Select(rec => rec.LEVELNAME).FirstOrDefault(),
                                  approvalStatus = current.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                  approvalState = current.TBL_APPROVAL_STATE.APPROVALSTATE,
                                  applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER,
                                  requestStaffName = current.TBL_STAFF.LASTNAME + " " + current.TBL_STAFF.MIDDLENAME + " " + current.TBL_STAFF.FIRSTNAME,
                                  responseStaffName = (current.RESPONSESTAFFID == null) ? "N/A" : current.TBL_STAFF1.LASTNAME + " " + current.TBL_STAFF1.MIDDLENAME + " " + current.TBL_STAFF1.FIRSTNAME,
                                  //responsibleStaffName = (current.TOSTAFFID == null) ? "N/A" : current.TBL_STAFF2.LASTNAME + " " + current.TBL_STAFF2.MIDDLENAME + " " + current.TBL_STAFF2.FIRSTNAME,
                                  //reliefStaffName = (current.TOSTAFFID == null) ? "N/A" : current.TBL_STAFF3.LASTNAME + " " + current.TBL_STAFF3.MIDDLENAME + " " + current.TBL_STAFF3.FIRSTNAME,
                                  comment = current.COMMENT,
                                  systemArrivalDate = current.SYSTEMARRIVALDATETIME,
                                  systemResponseDate = current.SYSTEMRESPONSEDATETIME,
                                  requestApprovalLevel = current.TBL_APPROVAL_LEVEL.LEVELNAME,
                                  responseApprovalLevel = current.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                  customerName = d.TBL_CUSTOMER.LASTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.FIRSTNAME,
                                  customerDivisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == cust.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                  //customerName = d.CUSTOMERID == null ? d.TBL_CUSTOMER_GROUP.GROUPNAME : context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID == d.CUSTOMERID).Select(cu=>cu.LASTNAME + " " + cu.MIDDLENAME + cu.LASTNAME).FirstOrDefault(),
                                  responseDate = current.RESPONSEDATE.HasValue ? (DateTime)current.RESPONSEDATE : (DateTime)(DateTime.Now),
                                  arrivalDate = current.ARRIVALDATE,
                                  applicationDate = bo.DATETIMECREATED,
                                  amount = bo.AMOUNT_REQUESTED,
                                  TargetId = current.TARGETID,
                                  branchName = e.TBL_BRANCH.BRANCHNAME + " (" + e.TBL_BRANCH.BRANCHCODE + ") ",
                                  loanApplicationId = d.LOANAPPLICATIONID,
                                  loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                  loanBookingRequestId = bo.LOAN_BOOKING_REQUESTID,
                                  approvalTrailId = current.APPROVALTRAILID,
                                  productNames = p.PRODUCTNAME,
                                  apiRequestId = e.APIREQUESTID,
                                  loopedStaffId = current.LOOPEDSTAFFID,
                                  toStaffId = current.TOSTAFFID,
                                  fromApprovalLevelId = current.FROMAPPROVALLEVELID,
                                  toApprovalLevelId = current.TOAPPROVALLEVELID,
                                  currentApprovalLevelId = current.TOAPPROVALLEVELID,
                                  requestStaffId = current.REQUESTSTAFFID,
                                  isSkipAppraisalEnabled = jumpsToDrawDown != null ? jumpsToDrawDown.ISSKIPPROCESSENABLED : false,
                                  comments = (from a in context.TBL_APPROVAL_TRAIL
                                             join r in context.TBL_LOAN_BOOKING_REQUEST on a.TARGETID equals r.LOAN_BOOKING_REQUESTID
                                             where operations.Contains(a.OPERATIONID) && r.LOAN_BOOKING_REQUESTID == bo.LOAN_BOOKING_REQUESTID
                                              select new ApprovalTrailViewModel
                                             {
                                                approvalTrailId = a.APPROVALTRAILID,
                                                comment = a.COMMENT,
                                                targetId = a.TARGETID,
                                                operationId = a.OPERATIONID,
                                                arrivalDate = a.ARRIVALDATE,
                                                systemArrivalDateTime = a.SYSTEMARRIVALDATETIME,
                                                responseDate = a.RESPONSEDATE,
                                                systemResponseDateTime = a.SYSTEMRESPONSEDATETIME,
                                                responseStaffId = a.RESPONSESTAFFID,
                                                requestStaffId = a.REQUESTSTAFFID,
                                                loopedStaffId = a.LOOPEDSTAFFID,
                                                toStaffId = a.TOSTAFFID,
                                                fromApprovalLevelId = a.FROMAPPROVALLEVELID,
                                                fromApprovalLevelName = a.FROMAPPROVALLEVELID == null ? a.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                                                toApprovalLevelName = a.TOAPPROVALLEVELID == null ? "N/A" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                                toApprovalLevelId = a.TOAPPROVALLEVELID,
                                                approvalStateId = a.APPROVALSTATEID,
                                                approvalStatusId = a.APPROVALSTATUSID,
                                                approvalState = a.TBL_APPROVAL_STATE.APPROVALSTATE,
                                                approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                                fromStaffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.FIRSTNAME,
                                                toStaffName = a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.MIDDLENAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                                  //toStaffName = (current.TOSTAFFID == null) ? "N/A" : current.TBL_STAFF2.LASTNAME + " " + current.TBL_STAFF2.MIDDLENAME + " " + current.TBL_STAFF2.FIRSTNAME,
                                              }).OrderByDescending(x => x.approvalTrailId).ToList()
                                })).OrderByDescending(o => o.approvalTrailId).ToList();

                //var test = preBookingrecord.ToList();
                var records = drawdownsNotYetInitiated.Union(record).ToList();
                records = setReportFields(records);
                //var records = record.OrderByDescending(a => a.approvalTrailId).ToList();
                return records;
                //int serial = 1;
                //foreach (var item in records.ToList())
                //{
                //    int count = serial;

                //    serial += 1;
                //    item.serial = count;
                //    approvalRecord.Add(item);
                //}
                ////var test = approvalRecord.ToList();

                //return approvalRecord.OrderBy(o => o.serial).ToList();
            }
        }

        private List<WorkflowTrackerViewModel> setReportFields(List<WorkflowTrackerViewModel> records)
        {
            var staffs = context.TBL_STAFF.ToList();
            foreach (var record in records)
            {
                if (record.toStaffId > 0)
                {
                    var toStaff = staffs.FirstOrDefault(s => s.STAFFID == record.toStaffId);
                    record.responsibleStaffName = toStaff.LASTNAME + " " + toStaff.MIDDLENAME + " " + toStaff.FIRSTNAME;
                }
                if (record.fromApprovalLevelId == null)
                {
                    var staff = staffs.FirstOrDefault(s => s.STAFFID == record.requestStaffId);
                    record.requestApprovalLevel = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                }
                if (record.fromApprovalLevelId == record.toApprovalLevelId && record.toStaffId > 0)
                {
                    var staff = staffs.FirstOrDefault(s => s.STAFFID == record.requestStaffId);
                    record.requestApprovalLevel = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                }

                if (record.fromApprovalLevelId == record.toApprovalLevelId && record.loopedStaffId > 0)
                {
                    var staff = staffs.FirstOrDefault(s => s.STAFFID == record.loopedStaffId);
                    record.currentLevel = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                    record.responsibleStaffName = staff.LASTNAME + " " + staff.MIDDLENAME + " " + staff.FIRSTNAME;
                }
                if (record?.comments != null)
                {
                    foreach (var c in record.comments)
                    {
                        if (c.toStaffId > 0)
                        {
                            var toStaff = staffs.FirstOrDefault(s => s.STAFFID == c.toStaffId);
                            c.toStaffName = toStaff.LASTNAME + " " + toStaff.MIDDLENAME + " " + toStaff.FIRSTNAME;
                        }
                        if (c.fromApprovalLevelId == null)
                        {
                            var staff = staffs.FirstOrDefault(s => s.STAFFID == c.requestStaffId);
                            c.fromApprovalLevelName = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                        }
                        if (c.fromApprovalLevelId == c.toApprovalLevelId && c.toStaffId > 0)
                        {
                            var staff = staffs.FirstOrDefault(s => s.STAFFID == c.requestStaffId);
                            c.fromApprovalLevelName = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                        }

                        if (record.fromApprovalLevelId == record.toApprovalLevelId && record.loopedStaffId > 0)
                        {
                            var staff = staffs.FirstOrDefault(s => s.STAFFID == record.loopedStaffId);
                            c.toApprovalLevelName = staff.TBL_STAFF_ROLE.STAFFROLENAME;
                            c.toStaffName = staff.LASTNAME + " " + staff.MIDDLENAME + " " + staff.FIRSTNAME;
                        }
                    }
                }
                
            }
            return records;
        }

        public IEnumerable<LoanApplicationViewModel> LoanSearch(string searchString)
        {
            var search = searchString.Trim().ToLower();

            var applications = (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                where (x.APPLICATIONREFERENCENUMBER.Contains(search)
                                      || c.FIRSTNAME.ToLower().Contains(search)
                                      || c.LASTNAME.ToLower().Contains(search)
                                      || c.MIDDLENAME.ToLower().Contains(search))
                                    select new LoanApplicationViewModel
                                    {
                                    firstName = c.FIRSTNAME,
                                    middleName = c.MIDDLENAME,
                                    lastName = c.LASTNAME,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    proposedAmount = a.PROPOSEDAMOUNT,
                                    approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                    customerId = c.CUSTOMERID,
                                    branchId = c.BRANCHID,
                                    customerGroupId = x.CUSTOMERGROUPID,
                                    loanTypeId = x.LOANAPPLICATIONTYPEID,
                                    relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    applicationDate = x.APPLICATIONDATE,
                                    applicationAmount = x.APPLICATIONAMOUNT,
                                    approvedAmount = x.APPROVEDAMOUNT,
                                    interestRate = x.INTERESTRATE,
                                    applicationTenor = x.APPLICATIONTENOR                      
                                    }).ToList();

            return applications;
            
        }

        public IEnumerable<CreditApplicationViewModel> CommitteeCreditApplications(int applicationTypeId, int staffId)
        {
            List<int> ids = new List<int>();
            string applicationType;
            IQueryable<CreditApplicationViewModel> applications = null;

            List<int> ExclusiveOperations = (from flow in context.TBL_LOAN_APPLICATN_FLOW_CHANGE select flow.OPERATIONID).ToList();

            

            if (applicationTypeId == 1)
            {
                ExclusiveOperations.Add((int)OperationsEnum.CreditAppraisal);
                foreach (var i in ExclusiveOperations)
                {
                    ids.AddRange(genSetup.GetStaffApprovalLevelIds(staffId, i).ToList());
                }
                applicationType = "Loan Origination";
                //ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CreditAppraisal).ToList();
                applications = context.TBL_LOAN_APPLICATION
                    //.Join(context.TBL_LOAN_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER, a => a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID, c => c.CUSTOMERID, (a, c) => new { a, c })
                    .Join(context.TBL_APPROVAL_TRAIL.Where(t => ExclusiveOperations.Contains(t.OPERATIONID)
                            && t.RESPONSESTAFFID == null && t.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ids.Contains((int)t.TOAPPROVALLEVELID)
                        ),
                        q => q.a.LOANAPPLICATIONID,
                        t => t.TARGETID, (q, t) => new { q, t })
                    .Select(x => new CreditApplicationViewModel
                    {
                        applicationType = applicationType,
                        firstName = x.q.c.FIRSTNAME,
                        middleName = x.q.c.MIDDLENAME,
                        lastName = x.q.c.LASTNAME,
                        customerCode = x.q.c.CUSTOMERCODE,
                        customerId = x.q.c.CUSTOMERID,
                        loanApplicationId = x.q.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.q.a.APPLICATIONREFERENCENUMBER,
                        applicationDate = x.q.a.APPLICATIONDATE,
                        //customerId = x.q.a.CUSTOMERID.Value,
                        operationId = x.q.a.OPERATIONID,
                        customerGroupName = x.q.a.CUSTOMERGROUPID.HasValue ? x.q.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                    });
            }

            if (applicationTypeId == 2)
            {
                List<int> operationIds = new List<int>();
                operationIds.Add(46);
                operationIds.Add(71);
                operationIds.Add(79);

                //ExclusiveOperations.AddRange(operationIds);
                //foreach (var i in ExclusiveOperations)
                //{
                //    ids.AddRange(genSetup.GetStaffApprovalLevelIds(staffId, i).ToList());
                //}
                applicationType = "Loan Management";
                ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanReviewApprovalAppraisal).ToList();
                applications = context.TBL_LMSR_APPLICATION
                    //.Join(context.TBL_LMSR_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER, a => a.CUSTOMERID, c => c.CUSTOMERID, (a, c) => new { a, c })
                    .Join(context.TBL_APPROVAL_TRAIL.Where(t => operationIds.Contains(t.OPERATIONID)
                        && t.APPROVALSTATEID != (int)ApprovalState.Ended
                        && t.RESPONSESTAFFID == null
                        && ids.Contains((int)t.TOAPPROVALLEVELID)
                        && (t.TOSTAFFID == null || t.TOSTAFFID == staffId)
                    ),
                        q => q.a.LOANAPPLICATIONID,
                        t => t.TARGETID, (q, t) => new { q, t })
                    .Select(x => new CreditApplicationViewModel
                    {
                        applicationType = applicationType,
                        firstName = x.q.c.FIRSTNAME,
                        middleName = x.q.c.MIDDLENAME,
                        lastName = x.q.c.LASTNAME,
                        customerCode = x.q.c.CUSTOMERCODE,
                        customerId = x.q.c.CUSTOMERID,
                        loanApplicationId = x.q.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.q.a.APPLICATIONREFERENCENUMBER,
                        applicationDate = x.q.a.APPLICATIONDATE,
                        customerGroupName = "",
                    });
            }

            return applications.ToList();
        }

        public dynamic GetLoanApplicationDetailsProductProgram(int loanApplicationDetailId)
        {
            var details = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_APPLICATION
                           on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select a.TBL_PRODUCT1.PRODUCTCLASSID).FirstOrDefault();

            var typeId = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                          join b in context.TBL_LOAN_APPLICATION
                          on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                          where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                          select a.TBL_PRODUCT.PRODUCTTYPEID).FirstOrDefault();


            if (typeId == (short)LoanProductTypeEnum.SyndicatedTermLoan)
            {
                var syndication = (from j in context.TBL_LOAN_APPLICATION_DETL_SYN
                                   where j.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                                   select new SyndicatedLoanDetailViewModel()
                                   {
                                       syndicationId = j.SYNDICATIONID,
                                       bankCode = j.BANKCODE,
                                       bankName = j.BANKNAME,
                                       amountContributed = j.AMOUNTCONTRIBUTED,
                                       loanApplicationDetailId = j.LOANAPPLICATIONDETAILID,
                                       typeId = (short)j.PARTY_TYPEID,
                                       typeName = context.TBL_LOAN_SYNDICATION_PARTY_TYP.Where(f => f.PARTY_TYPEID == j.PARTY_TYPEID).Select(i => i.PARTY_TYPENAME).FirstOrDefault(),
                                       productClassId = (int)ProductClassEnum.Corporate

                                   }).ToList();
                return syndication;
            }

            if (details == (short)ProductClassEnum.InvoiceDiscountingFacility)
            {
                var inv = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
                           where a.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select new InvoiceDetailViewModel()
                           {
                               invoiceId = a.INVOICEID,
                               loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                               principalId = a.PRINCIPALID,
                               principalName = a.TBL_LOAN_PRINCIPAL.NAME,
                               invoiceNo = a.INVOICENO,
                               contractNo = a.CONTRACTNO,
                               invoiceDate = a.INVOICE_DATE,
                               invoiceAmount = a.INVOICE_AMOUNT,
                               invoiceCurrencyId = a.INVOICE_CURRENCYID,
                               invoiceCurrencyName = a.TBL_CURRENCY.CURRENCYNAME,
                               contractStartDate = a.CONTRACT_STARTDATE,
                               contractEndDate = a.CONTRACT_ENDDATE,
                               approvalStatusId = a.APPROVALSTATUSID,
                               purchaseOrderNumber = a.PURCHASEORDERNUMBER,
                               reValidated = a.REVALIDATED,
                               entrySheetNumber = a.ENTRYSHEETNUMBER,
                               productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                           }).ToList();
                return inv;
            }
            //else if (details == (short)ProductClassEnum.FirstTrader)
            //{
            //    var trader = (from tra in context.TBL_LOAN_APPLICATION_DETL_TRA
            //                  where tra.LOANAPPLICATIONDETAILID == loanApplicationDetailId
            //                  select new TraderLoanViewModel()
            //                  {
            //                      traderId = tra.TRADDERID,
            //                      loanApplicationDetailId = tra.LOANAPPLICATIONDETAILID,
            //                      marketId = tra.MARKETID,
            //                      marketName = tra.TBL_LOAN_MARKET.MARKETNAME,
            //                      averageMonthlyTurnover = tra.AVERAGE_MONTHLY_TURNOVER,
            //                      productClassId = (int)ProductClassEnum.FirstTrader,
            //                      soldItems = tra.SOLDITEMS
            //                  }).ToList();
            //    return trader;
            //}
            //else if (details == (short)ProductClassEnum.FirstEdu)
            //{
            //    var edu = (from e in context.TBL_LOAN_APPLICATION_DETL_EDU
            //               where e.LOANAPPLICATIONDETAILID == loanApplicationDetailId
            //               select new EducationLoanViewModel()
            //               {
            //                   educationId = e.EDUCATIONID,
            //                   loanApplicationDetailId = e.LOANAPPLICATIONDETAILID,
            //                   numberOfStudent = e.NUMBER_OF_STUDENTS,
            //                   averageSchoolFees = e.AVERAGE_SCHOOL_FEES,
            //                   totalPreviousTermSchoolFees = e.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
            //                   productClassId = (int)ProductClassEnum.FirstEdu
            //               }).ToList();
            //    return edu;
            //}
            else if (details == (short)ProductClassEnum.BondAndGuarantees)
            {

                var bg = (from b in context.TBL_LOAN_APPLICATION_DETL_BG
                              //join p in context.TBL_LOAN_PRINCIPAL on b.PRINCIPALID equals p.PRINCIPALID
                          where b.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                          select new BondsAndGauranteeViewModel
                          {
                              bondId = b.BONDID,
                              loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                              principalId = b.PRINCIPALID,
                              amount = b.AMOUNT,
                              currencyId = b.CURRENCYID,
                              contractStartDate = b.CONTRACT_STARTDATE,
                              contractEndDate = b.CONTRACT_ENDDATE,
                              isTenored = b.ISTENORED,
                              isBankFormat = b.ISBANKFORMAT,
                              referenceNo = b.REFERENCENO,
                              approvalStatusId = b.APPROVALSTATUSID,

                              principalName = context.TBL_LOAN_PRINCIPAL.FirstOrDefault(x => x.PRINCIPALID == (int)b.PRINCIPALID) == null ? b.PRINCIPALNAME : b.TBL_LOAN_PRINCIPAL.NAME,
                              //principalNameOthers = b.PRINCIPALNAME,
                              // principalName = (b.PRINCIPALID == null) ? b.PRINCIPALNAME : b.TBL_LOAN_PRINCIPAL.NAME,

                              //principalName = (b.PRINCIPALID != null) ? b.TBL_LOAN_PRINCIPAL.NAME : b.PRINCIPALNAME,
                              //principalNameOthers = b.PRINCIPALNAME,
                              invoiceCurrencyCode = b.TBL_CURRENCY.CURRENCYCODE,
                              approvalStatusName = b.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                              productClassId = (int)ProductClassEnum.BondAndGuarantees
                          }).ToList();
                return bg;
            }
            return null;
        }


        public ValidateDataViewModel ValidateDocumentDate(ValidateDataViewModel data)
        {
            var dat = context.TBL_PRODUCT.Where(c => c.PRODUCTID == data.productId).FirstOrDefault();
            int days = DateTime.Now.Subtract(data.date).Days - 1;
            return new ValidateDataViewModel
            {
                dayCount = days,
                dayInterval = dat.EXPIRYPERIOD,
                InvoiceStatus = (days > 0 && dat.EXPIRYPERIOD >= days) ? true : false,
            };

        }

        public ValidateNumberViewModel ValidateDocumentNumber(ValidateNumberViewModel data)
        {
            var dat = context.TBL_LOAN_APPLICATION_DETL_INV
                .Where(c => c.PRINCIPALID == (int)data.principalId && c.INVOICENO == data.documentNo
                && c.PURCHASEORDERNUMBER == data.purchaseOrderNumber && c.CONTRACTNO == data.contractNumber)
                .FirstOrDefault();

            return new ValidateNumberViewModel
            {
                purchaseOrderNumber = data.purchaseOrderNumber,
                documentNo = data.documentNo,
                invoiceStatus = (dat == null) ? false : true,
                principalId = data.principalId,
                productId = data.productId
            };

        }

        public bool ValidateInvoiceDetails(ValidateNumberViewModel data)
        {
            //var dat = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
            //           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
            //           where b.CUSTOMERID == data.customerId && (a.INVOICENO == data.documentNo
            //             || a.PURCHASEORDERNUMBER == data.purchaseOrderNumber || a.CONTRACTNO == data.contractNumber ||
            //             a.CERTIFICATENO == data.certificateNumber)
            //           select a).ToList();
            var invoice = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           where b.CUSTOMERID == data.customerId && a.INVOICENO == data.documentNo
                           select a).FirstOrDefault();

            if (data.reValidated == true && invoice != null)
            {
                invoice.REVALIDATED = true;
                context.SaveChanges();
                return true;
            }

            return invoice == null;
        }

        //public List<InvoiceDetailViewModel> ValidateBulkLoanInvoice(byte[] file)
        //{
        //    var uploads = GetBulkLoanInvoice(file);
        //    if (uploads.Count() > 0)
        //    {
        //        return uploads;
        //    }
        //    return null;
        //}


        public List<InvoiceDetailViewModel> GetBulkLoanInvoice(byte[] file, UserInfo user)
        {
            List<InvoiceDetailViewModel> bulkEntries = new List<InvoiceDetailViewModel>();

            //Limited unlicenced key : SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY"); 
            SpreadsheetInfo.SetLicense("E1H4-YMDW-014G-BAQ5");

            MemoryStream ms = new MemoryStream(file);

            ExcelFile ef = ExcelFile.Load(ms, LoadOptions.XlsxDefault);

            //ExcelWorksheet ws = ef.Worksheets.ActiveWorksheet;
            ExcelWorksheet ws = ef.Worksheets[0]; //.ActiveWorksheet;
            CellRange range = ef.Worksheets.ActiveWorksheet.GetUsedCellRange(true);

            for (int j = range.FirstRowIndex; j <= range.LastRowIndex; j++)
            {
                InvoiceDetailViewModel currentLine = new InvoiceDetailViewModel();
                for (int i = range.FirstColumnIndex; i <= range.LastColumnIndex; i++)
                {
                    ExcelCell cell = range[j - range.FirstRowIndex, i - range.FirstColumnIndex];

                    string cellName = CellRange.RowColumnToPosition(j, i);
                    string cellRow = ExcelRowCollection.RowIndexToName(j);
                    string cellColumn = ExcelColumnCollection.ColumnIndexToName(i);
                    if (Convert.ToInt32(cellRow) == 1) continue;

                    switch (cellColumn)
                    {
                        case "A":
                            currentLine.contractNo = cell.Value.ToString();
                            continue;
                        case "B":
                            currentLine.purchaseOrderNumber = cell.Value.ToString();
                            continue;                       
                        case "C":
                            currentLine.entrySheetNumber = cell.Value.ToString();
                            continue;
                        case "D":
                            currentLine.invoiceDate = Convert.ToDateTime(cell.Value);
                            continue;
                        case "E":
                            currentLine.invoiceNo = cell.Value.ToString();
                            continue;
                        case "F":
                            currentLine.invoiceAmount = Convert.ToDecimal(cell.Value);
                            continue;

                    }
                }
                    bulkEntries.Add(currentLine);
            };
            bulkEntries.RemoveAt(0);
            return bulkEntries;
        }


        #region All Operation Applications

        // TO BE MADE UNIVERSAL ? stages, scope

        public IQueryable<LoanApplicationViewModel> GetLoanApplicationsByOperation(int operationId, int? classId, int branchId, int staffId)
        {
            bool isHeadOffice = (branchId == 1) ? true : false;

            var ids = genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var applications = context.TBL_LOAN_APPLICATION
                .Where(x =>
                //(isHeadOffice || x.BRANCHID == branchId)
                x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved // <-------------------------------------hard codes!!!
                && x.PRODUCTCLASSID == (short?)classId
                && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.BondAndGuaranteesInProgress // <--------hard codes!!!
            )
            .Join(
                context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationId
                && ids.Contains((int)x.TOAPPROVALLEVELID) && x.RESPONSESTAFFID == null
                ),
                a => a.LOANAPPLICATIONID,
                b => b.TARGETID,
                (a, b) => new { a, b })
            .Select(x => new LoanApplicationViewModel
            {
                //groupRoleId = y.TBL_APPROVAL_LEVEL1.TBL_APPROVAL_GROUP.ROLEID,
                loanApplicationId = x.a.LOANAPPLICATIONID,
                applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
                customerId = x.a.CUSTOMERID,
                branchId = x.a.BRANCHID,
                productClassId = x.a.PRODUCTCLASSID,
                productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                customerGroupId = x.a.CUSTOMERGROUPID,
                loanTypeId = x.a.LOANAPPLICATIONTYPEID,
                relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
                applicationDate = x.a.APPLICATIONDATE,
                applicationAmount = x.a.APPLICATIONAMOUNT,
                approvedAmount = x.a.APPROVEDAMOUNT,
                interestRate = x.a.INTERESTRATE,
                applicationTenor = x.a.APPLICATIONTENOR,
                lastComment = x.b.COMMENT,
                currentApprovalStateId = x.b.APPROVALSTATEID,
                currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
                //  currentApprovalLevel = x.b.APVL_LVL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                approvalTrailId = x.b == null ? 0 : x.b.APPROVALTRAILID, // for inner sequence ordering
                loanInformation = x.a.LOANINFORMATION,
                submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
                isRelatedParty = x.a.ISRELATEDPARTY,
                isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
                approvalStatusId = (short)x.a.APPROVALSTATUSID,
                applicationStatusId = x.a.APPLICATIONSTATUSID,
                branchName = x.a.TBL_BRANCH.BRANCHNAME,
                relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
                misCode = x.a.MISCODE,
                customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeName = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                createdBy = x.a.OWNEDBY,
                loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
                loantermSheetId = x.a.LOANTERMSHEETID,
                customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "N/A",
                operationId = x.a.OPERATIONID,
            })
            .GroupBy(d => d.loanApplicationId)
            .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault())
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId)
            ;

            var test = applications.ToList();
            return applications;
        }

        #endregion All Operation Applications

        public IQueryable<LoanApplicationViewModel> GetRejectedLoanApplications(UserInfo user)
        {
            bool isHeadOffice = (user.BranchId == 1) ? true : false;

            var applications = context.TBL_LOAN_APPLICATION
                .Where(x => (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterRejected || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationRejected
                || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted)
                //&& x.REVIEW_TYPE == null // <------------------- INT of APPLICATIONSTATUSID to filter
                )
            .Select(x => new LoanApplicationViewModel
            {
                loanApplicationId = x.LOANAPPLICATIONID,
                applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                relatedReferenceNumber = x.RELATEDREFERENCENUMBER,
                iblRequest = x.IBLREQUEST,
                customerId = x.CUSTOMERID,
                branchId = x.BRANCHID,
                branchName = x.TBL_BRANCH.BRANCHNAME,
                productClassId = x.PRODUCTCLASSID,
                productClassName = x.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                customerGroupId = x.CUSTOMERGROUPID,
                loanTypeId = x.LOANAPPLICATIONTYPEID,
                relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.RELATIONSHIPMANAGERID,
                applicationDate = x.APPLICATIONDATE,
                applicationAmount = x.APPLICATIONAMOUNT,
                approvedAmount = x.APPROVEDAMOUNT,
                interestRate = x.INTERESTRATE,
                applicationTenor = x.APPLICATIONTENOR,
                loanInformation = x.LOANINFORMATION,
                submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                isRelatedParty = x.ISRELATEDPARTY,
                isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                approvalStatusId = (short)x.APPROVALSTATUSID,
                applicationStatusId = x.APPLICATIONSTATUSID,
                createdBy = x.OWNEDBY,
                misCode = x.MISCODE,

                applicationStatus = x.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
                relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                loantermSheetId = x.LOANTERMSHEETID,
                customerName = x.CUSTOMERID.HasValue ? x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME : "N/A",

                details = x.TBL_LOAN_APPLICATION_DETAIL.Select(o => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = o.LOANAPPLICATIONDETAILID,
                    applicationId = o.LOANAPPLICATIONID,
                    customerId = o.TBL_CUSTOMER.CUSTOMERID,
                    obligorName = o.TBL_CUSTOMER.FIRSTNAME + " " + o.TBL_CUSTOMER.MIDDLENAME + " " + o.TBL_CUSTOMER.LASTNAME,
                    currencyCode = o.TBL_CURRENCY.CURRENCYCODE,
                    //proposedProductName = o.TBL_PRODUCT.PRODUCTNAME,
                    proposedTenor = o.PROPOSEDTENOR,
                    proposedRate = o.PROPOSEDINTERESTRATE,
                    proposedAmount = o.PROPOSEDAMOUNT,
                    proposedProductId = o.PROPOSEDPRODUCTID,
                    approvedProductName = o.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                    approvedTenor = o.APPROVEDTENOR,
                    approvedRate = o.APPROVEDINTERESTRATE,
                    approvedAmount = o.APPROVEDAMOUNT,
                    //convertedApprovedAmount = o.ApprovedAmount * Convert.ToDecimal(o.ExchangeRate),
                    approvedProductId = o.APPROVEDPRODUCTID,

                    statusId = o.STATUSID,
                    exchangeRate = o.EXCHANGERATE,
                })
            })
            .OrderByDescending(x => x.applicationDate)
            .ThenByDescending(x => x.loanApplicationId)
            ;

            //var test = applications.ToList();
            return applications;
        }

        public IQueryable<LoanReviewApplicationViewModel> GetRejectedReviewLoanApplications(UserInfo user)
        {

            bool isHeadOffice = (user.BranchId == 1) ? true : false;

            var applications = context.TBL_LMSR_APPLICATION
                .Where(x => (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterRejected || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationRejected
                || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted)
                )
            .Select(x => new LoanReviewApplicationViewModel
            {
                applicationStatusId = x.APPLICATIONSTATUSID,
                applicationDate = x.APPLICATIONDATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                approvalStatusId = (int)x.APPROVALSTATUSID,
                createdByName = context.TBL_STAFF.Where(s => s.STAFFID == x.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
                loanReviewApplicationId = x.LOANAPPLICATIONID,
                referenceNumber = x.APPLICATIONREFERENCENUMBER,
                relatedReferenceNumber = x.RELATEDREFERENCENUMBER,
                
                branchId = x.BRANCHID,
                branchName = x.TBL_BRANCH.BRANCHNAME,
                customerId = (int)x.CUSTOMERID,
                operationId = x.OPERATIONID,
                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,

                customerGroupName = context.TBL_CUSTOMER_GROUP.Where(c => c.CUSTOMERGROUPID == x.CUSTOMERGROUPID).Select(c => c.GROUPNAME).FirstOrDefault() ?? "",

                loanTypeName = context.TBL_LOAN_APPLICATION_TYPE.Where(l => l.LOANAPPLICATIONTYPEID == x.LOANAPPLICATIONTYPEID).Select(l => l.LOANAPPLICATIONTYPENAME).FirstOrDefault() ?? "N/A",
                facility = x.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LMSR_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
                approvedAmount = x.APPROVEDAMOUNT == null ? 0 : x.APPROVEDAMOUNT,
                productClassProcessId = x.PRODUCT_CLASS_PROCESSID == null ? 0 : x.PRODUCT_CLASS_PROCESSID,
                divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                globalsla = !context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(c => c.GLOBALSLA).Any() ? 0 : context.TBL_PRODUCT_CLASS.Where(c => c.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(c => c.GLOBALSLA).FirstOrDefault(),

                dateTimeCreated = x.DATETIMECREATED,
                operationTypeName = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(),

                creditOperationType = context.TBL_LMSR_APPLICATION_DETAIL.Where(s => s.LOANAPPLICATIONID == x.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Count() > 1
                     ? "Multiple"
                     : context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID ==
                             context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).OPERATIONID
                         ).OPERATIONNAME,

                facilityType = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault(s => s.LOANAPPLICATIONID == x.LOANAPPLICATIONID && s.DELETED != true && s.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).TBL_PRODUCT.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                applicationDetails = x.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.DELETED == false)
                    .Select(d => new applicationDetails
                    {
                        detailId = d.LOANREVIEWAPPLICATIONID,
                        loanApplicationId = d.LOANAPPLICATIONID,
                        operationId = d.OPERATIONID,
                        operationName = d.TBL_OPERATIONS.OPERATIONNAME,
                        reviewDetails = d.REVIEWDETAILS,
                        reviewStageId = d.REVIEWSTAGEID,
                        loanId = d.LOANID,
                        loanSystemTypeId = d.LOANSYSTEMTYPEID,
                        loanSystemTypeName = d.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                        productId = d.PRODUCTID,
                        customerId = d.CUSTOMERID,
                        obligorName = d.TBL_CUSTOMER.FIRSTNAME + " " + d.TBL_CUSTOMER.MIDDLENAME + " " + d.TBL_CUSTOMER.LASTNAME,
                        proposedTenor = d.PROPOSEDTENOR,
                        proposedRate = d.PROPOSEDINTERESTRATE,
                        proposedAmount = d.PROPOSEDAMOUNT,
                        approvedTenor = d.APPROVEDTENOR,
                        approvedRate = d.APPROVEDINTERESTRATE,
                        approvedAmount = d.APPROVEDAMOUNT,
                        customerProposedAmount = d.CUSTOMERPROPOSEDAMOUNT,
                        statusId = d.APPROVALSTATUSID,
                        accountName = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNAME,
                        accountNumber = d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN.Where(M => M.TERMLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_REVOLVING.Where(M => M.REVOLVINGLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : d.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability ? context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_CONTINGENT.Where(M => M.CONTINGENTLOANID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER : context.TBL_CASA.Where(O => O.CASAACCOUNTID == (context.TBL_LOAN_APPLICATION_DETAIL.Where(M => M.LOANAPPLICATIONDETAILID == d.LOANID).FirstOrDefault().CASAACCOUNTID)).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                        terms = d.REPAYMENTTERMS,
                        schedule = context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault() == null ? "" : context.TBL_REPAYMENT_TERM.Where(r => r.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).Select(r => r.REPAYMENTTERMDETAIL).FirstOrDefault(),
                    })

            }).OrderByDescending(d => d.loanReviewApplicationId);

            return applications;
        }

        public IQueryable<LoanApplicationViewModel> GetRejectedLoanApplicationsArch(UserInfo user)
        {
            bool isHeadOffice = (user.BranchId == 1) ? true : false;
            var reliefStaffIds = genSetup.GetStaffRlieved(user.staffId);

            var applications = context.TBL_LOAN_APPLICATION_ARCHIVE
                .Where(x => (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterRejected
                || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationRejected
                || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted
                )
                && ((reliefStaffIds.Contains(x.CREATEDBY)) || (x.OWNEDBY == user.staffId))
                )
            .Select(x => new LoanApplicationViewModel
            {
                applicationDate = (DateTime)x.APPLICATIONDATE,
                loanApplicationId = x.LOANAPPLICATIONID,
                applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                relatedReferenceNumber = x.RELATEDREFERENCENUMBER,
                
                customerId = x.CUSTOMERID,
                branchId = x.BRANCHID,
                branchName = x.TBL_BRANCH.BRANCHNAME,
                productClassId = x.PRODUCTCLASSID,
                productClassName = x.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                customerGroupId = x.CUSTOMERGROUPID,
                loanTypeId = x.LOANAPPLICATIONTYPEID,
                relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                relationshipManagerId = x.RELATIONSHIPMANAGERID,
                applicationDateArch = x.APPLICATIONDATE,
                applicationAmount = x.APPLICATIONAMOUNT,
                approvedAmount = x.APPROVEDAMOUNT,
                interestRate = x.INTERESTRATE,
                applicationTenor = x.APPLICATIONTENOR,
                loanInformation = x.LOANINFORMATION,
                submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                isRelatedParty = x.ISRELATEDPARTY,
                isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                approvalStatusId = (short)x.APPROVALSTATUSID,
                applicationStatusId = x.APPLICATIONSTATUSID,
                createdBy = x.CREATEDBY,
                misCode = x.MISCODE,

                applicationStatus = x.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
                relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                loantermSheetId = x.LOANTERMSHEETID,
                customerName = x.CUSTOMERID.HasValue ? x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME : "N/A",

                details = context.TBL_LOAN_APPLICATION_DETAIL.Where(o=>o.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => new ApprovedLoanDetailViewModel
                {
                    loanApplicationDetailId = o.LOANAPPLICATIONDETAILID,
                    applicationId = o.LOANAPPLICATIONID,
                    customerId = o.TBL_CUSTOMER.CUSTOMERID,
                    obligorName = o.TBL_CUSTOMER.FIRSTNAME + " " + o.TBL_CUSTOMER.MIDDLENAME + " " + o.TBL_CUSTOMER.LASTNAME,
                    currencyCode = o.TBL_CURRENCY.CURRENCYCODE,
                    //proposedProductName = o.TBL_PRODUCT.PRODUCTNAME,
                    proposedTenor = o.PROPOSEDTENOR,
                    proposedRate = o.PROPOSEDINTERESTRATE,
                    proposedAmount = o.PROPOSEDAMOUNT,
                    proposedProductId = o.PROPOSEDPRODUCTID,
                    approvedProductName = o.TBL_PRODUCT1.PRODUCTNAME, // <----------take note of 1
                    approvedTenor = o.APPROVEDTENOR,
                    approvedRate = o.APPROVEDINTERESTRATE,
                    approvedAmount = o.APPROVEDAMOUNT,
                    //convertedApprovedAmount = o.ApprovedAmount * Convert.ToDecimal(o.ExchangeRate),
                    approvedProductId = o.APPROVEDPRODUCTID,

                    statusId = o.STATUSID,
                    exchangeRate = o.EXCHANGERATE,
                })
            })
            .OrderByDescending(x => x.loanApplicationId)
            //.ThenByDescending(x => x.loanApplicationId)
            ;

            var test = applications.ToList();
            return applications;
        }

        public string ReviewRequest(ForwardViewModel model)
        {
            var applArchive = context.TBL_LOAN_APPLICATION_ARCHIVE.Where(ar=>ar.LOANAPPLICATIONID == model.applicationId).FirstOrDefault();
            var appl = context.TBL_LOAN_APPLICATION.Where(a => a.LOANAPPLICATIONID == model.applicationId).Select(a=>a).FirstOrDefault();
            List<int> LoanApplicationStatus = new List<int> { (int)LoanApplicationStatusEnum.ApplicationRejected, (int)LoanApplicationStatusEnum.OfferLetterRejected, (int)LoanApplicationStatusEnum.CancellationInProgress, (int)LoanApplicationStatusEnum.CancellationCompleted };
            
            if (context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == appl.APPLICATIONREFERENCENUMBER && !LoanApplicationStatus.Contains(x.APPLICATIONSTATUSID)).Any())
            {
                return "This application is already re-initiated!";
            }

            if ((context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == model.applicationId && !LoanApplicationStatus.Contains(x.APPLICATIONSTATUSID)).Any()) && applArchive == null)
            {
                ArchiveLoanApplication(appl.LOANAPPLICATIONID, appl.OPERATIONID, 0, model.createdBy);
            }

            

            //var referenceNumber = GenerateLoanReferenceNumber();
            var applicationDate = genSetup.GetApplicationDate();
            var entity = context.TBL_LOAN_APPLICATION.Find(appl.LOANAPPLICATIONID);
            entity.DATETIMECREATED = applicationDate;
            entity.FINALAPPROVAL_LEVELID = null;
            entity.NEXTAPPLICATIONSTATUSID = null;
            entity.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            entity.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress;
            entity.DELETED = false;
            if (appl.ISADHOCAPPLICATION == true)
            {
                appl.OPERATIONID = (int)OperationsEnum.AdhocApproval;
                var receiverLevelId = GetFirstAdhocReceiverLevel(entity.OWNEDBY, appl.OPERATIONID, appl.PRODUCTCLASSID, false);
                workflow.NextLevelId = receiverLevelId;
                appl.DATEACTEDON = DateTime.Now;
                context.SaveChanges();
            }

            //bool wasApproved = appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved ? true : false;

            /* var request = context.TBL_LOAN_APPLICATION.Add(new TBL_LOAN_APPLICATION
             {
                 PRODUCTCLASSID = appl.PRODUCTCLASSID,
                 APPLICATIONREFERENCENUMBER = appl.APPLICATIONREFERENCENUMBER,
                 RELATEDREFERENCENUMBER = appl.APPLICATIONREFERENCENUMBER,
                 COMPANYID = appl.COMPANYID,
                 BRANCHID = appl.BRANCHID,
                 RELATIONSHIPOFFICERID = appl.RELATIONSHIPOFFICERID,
                 RELATIONSHIPMANAGERID = appl.RELATIONSHIPMANAGERID,
                 MISCODE = appl.MISCODE,
                 TEAMMISCODE = appl.TEAMMISCODE,
                 INTERESTRATE = appl.INTERESTRATE,
                 APPLICATIONDATE = appl.APPLICATIONDATE,
                 LOANINFORMATION = appl.LOANINFORMATION,
                 ISRELATEDPARTY = appl.ISRELATEDPARTY,
                 ISPOLITICALLYEXPOSED = appl.ISPOLITICALLYEXPOSED,
                 CREATEDBY = model.createdBy,
                 DATETIMECREATED = applicationDate,
                 SYSTEMDATETIME = DateTime.Now,
                 CUSTOMERGROUPID = appl.CUSTOMERGROUPID,
                 CASAACCOUNTID = appl.CASAACCOUNTID,
                 APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CAMInProgress,
                 APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                 APPLICATIONAMOUNT = appl.APPLICATIONAMOUNT,
                 APPROVEDAMOUNT = appl.APPLICATIONAMOUNT,
                 TOTALEXPOSUREAMOUNT = appl.TOTALEXPOSUREAMOUNT,
                 APPLICATIONTENOR = appl.APPLICATIONTENOR,
                 ISINVESTMENTGRADE = appl.ISINVESTMENTGRADE,
                 LOANPRELIMINARYEVALUATIONID = appl.LOANPRELIMINARYEVALUATIONID,
                 LOANTERMSHEETID = appl.LOANTERMSHEETID,
                 CUSTOMERID = appl.CUSTOMERID,
                 SUBMITTEDFORAPPRAISAL = appl.SUBMITTEDFORAPPRAISAL,
                 OPERATIONID = appl.OPERATIONID,
                 LOANAPPLICATIONTYPEID = appl.LOANAPPLICATIONTYPEID,
                 PRODUCT_CLASS_PROCESSID = appl.PRODUCT_CLASS_PROCESSID,
             });*/

            /*var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
            foreach (var x in details)
            {
                context.TBL_LOAN_APPLICATION_DETAIL.Add(new TBL_LOAN_APPLICATION_DETAIL // TODO update the list below!
                {
                    LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                    APPROVEDAMOUNT = wasApproved ? x.APPROVEDAMOUNT : x.PROPOSEDAMOUNT,
                    APPROVEDINTERESTRATE = wasApproved ? x.APPROVEDINTERESTRATE : x.PROPOSEDINTERESTRATE,
                    APPROVEDPRODUCTID = wasApproved ? x.APPROVEDPRODUCTID : x.PROPOSEDPRODUCTID,
                    APPROVEDTENOR = wasApproved ? x.APPROVEDTENOR : x.PROPOSEDTENOR,
                    EXCHANGERATE = x.EXCHANGERATE,
                    CURRENCYID = x.CURRENCYID,
                    CUSTOMERID = x.CUSTOMERID,
                    STATUSID = x.STATUSID,
                    LOANPURPOSE = x.LOANPURPOSE,
                    PROPOSEDAMOUNT = x.PROPOSEDAMOUNT,
                    PROPOSEDINTERESTRATE = x.PROPOSEDINTERESTRATE,
                    PROPOSEDPRODUCTID = x.PROPOSEDPRODUCTID,
                    PROPOSEDTENOR = x.PROPOSEDTENOR,
                    SUBSECTORID = x.SUBSECTORID,
                    CASAACCOUNTID = x.CASAACCOUNTID,
                    OPERATINGCASAACCOUNTID = x.OPERATINGCASAACCOUNTID,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = applicationDate,
                });
            }*/

            if (context.SaveChanges() > 0) // <------------------------- skip to test
            {
                /*int i;
                var rejectedDetails = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == request.LOANAPPLICATIONID)
                    .Select(x => x.LOANAPPLICATIONDETAILID)
                    .ToArray();

                    i = 0; // collateral
                    var collats = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                    foreach (var x in collats)
                    {
                        context.TBL_LOAN_APPLICATION_COLLATERL.Add(new TBL_LOAN_APPLICATION_COLLATERL
                        {
                            LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                            //LOANAPPLICATIONDETAILID = rejectedDetails[i], // ?
                            COLLATERALCUSTOMERID = x.COLLATERALCUSTOMERID,
                            LOANAPPCOLLATERALID = x.LOANAPPCOLLATERALID,
                            CREATEDBY = model.createdBy,
                            DATETIMECREATED = applicationDate,
                            SYSTEMDATETIME = DateTime.Now,
                        });
                        i++;
                    }*/

                   /* i = 0; // conditions
                    var conditions = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                 foreach (var x in conditions)
                 {
                    var condition = new TBL_LOAN_CONDITION_PRECEDENT
                    {
                        CONDITION = x.CONDITION,
                        ISEXTERNAL = x.ISEXTERNAL,
                        ISSUBSEQUENT = x.ISSUBSEQUENT,
                        RESPONSE_TYPEID = x.RESPONSE_TYPEID,
                        //LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                        LOANAPPLICATIONDETAILID = rejectedDetails[i], // ?
                        CREATEDBY = model.createdBy, 
                        DATETIMECREATED = applicationDate,
                    };

                    context.TBL_LOAN_CONDITION_PRECEDENT.Add(condition);
                 }*/

                /*i = 0; // fees
                var fees = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                foreach (var x in fees)
                {
                    context.TBL_LOAN_APPLICATION_DETL_FEE.Add(new TBL_LOAN_APPLICATION_DETL_FEE
                    {
                        LOANAPPLICATIONDETAILID = rejectedDetails[i],
                        CHARGEFEEID = x.CHARGEFEEID,
                        HASCONSESSION = x.HASCONSESSION,
                        CONSESSIONREASON = x.CONSESSIONREASON,
                        DEFAULT_FEERATEVALUE = x.DEFAULT_FEERATEVALUE,
                        RECOMMENDED_FEERATEVALUE = x.RECOMMENDED_FEERATEVALUE,
                        APPROVALSTATUSID = x.APPROVALSTATUSID,
                        DELETED = false,
                        DATETIMECREATED = applicationDate,
                        CREATEDBY = model.createdBy,
                    });
                }*/

                /*var cam = context.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                if (cam != null)
                {
                    var newcam = context.TBL_CREDIT_APPRAISAL_MEMORANDM.Add(new TBL_CREDIT_APPRAISAL_MEMORANDM
                    {
                        LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                        COMPANYID = cam.COMPANYID,
                        CAMREF = appl.APPLICATIONREFERENCENUMBER,
                        ISCOMPLETED = false,
                        RISKRATED = cam.RISKRATED,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = applicationDate,
                    });

                    var docs = context.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Where(x => x.APPRAISALMEMORANDUMID == cam.APPRAISALMEMORANDUMID);
                    foreach (var x in docs)
                    {
                        context.TBL_CREDIT_APPRAISAL_MEMO_DOCU.Add(new TBL_CREDIT_APPRAISAL_MEMO_DOCU
                        {
                            CAMDOCUMENTATION = x.CAMDOCUMENTATION,
                            APPRAISALMEMORANDUMID = cam.APPRAISALMEMORANDUMID,
                            APPROVALLEVELID = x.APPROVALLEVELID,
                            CREATEDBY = model.createdBy,
                            DATETIMECREATED = applicationDate,
                        });
                    }
                }*/

                //var operationId = (int)OperationsEnum.CreditAppraisal;
                workflow.StaffId = model.createdBy;
                workflow.OperationId = appl.OPERATIONID;
                workflow.TargetId = appl.LOANAPPLICATIONID;
                workflow.CompanyId = appl.COMPANYID;
                workflow.ProductClassId = appl.PRODUCTCLASSID;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                workflow.ToStaffId = model.createdBy;
                workflow.Amount = appl.APPLICATIONAMOUNT;
                workflow.InvestmentGrade = appl.ISINVESTMENTGRADE;
                workflow.Tenor = appl.APPLICATIONTENOR;
                workflow.PoliticallyExposed = appl.ISPOLITICALLYEXPOSED;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.ProductId = appl.PRODUCTID;
                workflow.ExclusiveFlowChangeId = appl.FLOWCHANGEID;
                workflow.LogActivity();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Re-applied for loan with reference number: { appl.APPLICATIONREFERENCENUMBER }",
                    IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    TARGETID = model.applicationId
                };
                this.auditTrail.AddAuditTrail(audit);
                // End of Audit section ---------------------

                return context.SaveChanges() > 0 ? "New Loan Application Reference Number " + appl.APPLICATIONREFERENCENUMBER : string.Empty;
            }

            //context.TBL_LOAN_APPLICATION_DETAIL.RemoveRange(details);
            //context.TBL_LOAN_APPLICATION.Remove(request);

            context.SaveChanges();

            return string.Empty;
        }

        private string GenerateLoanReferenceNumber()
        {
            TimeSpan epochTicks = new TimeSpan(new DateTime(1970, 1, 1).Ticks);
            TimeSpan unixTicks = new TimeSpan(DateTime.UtcNow.Ticks) - epochTicks;
            double unixTime = (int)unixTicks.TotalSeconds;
            return unixTime.ToString();
        }

        public dynamic GetCollateralRequirements(int applicationID, int? collateralCurrencyId, int companyId)
        {
            double colValue = 0.0;
            var collateralRate = context.TBL_PRODUCT_BEHAVIOUR.Select(n => new
            {
                productId = n.PRODUCTID,
                lcy = n.COLLATERAL_LCY_LIMIT,
                fcy = n.COLLATERAL_FCY_LIMIT,
                productLimit = n.PRODUCT_LIMIT
            }).ToList();

            var loanApp = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationID).ToList();
            var totatlLoan = loanApp.Sum(c => c.PROPOSEDAMOUNT);
            string loanCurrency = string.Empty;
            double fx = 0.0;
            foreach (var item in loanApp)
            {
                loanCurrency = item.TBL_CURRENCY.CURRENCYCODE;

                fx = (fina.GetExchangeRate(item.DATETIMECREATED, item.CURRENCYID, companyId).sellingRate);

                double rate;
                var productBehaviour = collateralRate.Where(p => p.productId == item.PROPOSEDPRODUCTID).FirstOrDefault();
                if (productBehaviour != null)
                {
                    double maximumExtendableValuePercentage = 0.0;
                    if (collateralCurrencyId == null)
                    {
                        maximumExtendableValuePercentage = productBehaviour.lcy ?? 0.0;
                    }
                    else if (collateralCurrencyId == item.CURRENCYID)
                    {
                        maximumExtendableValuePercentage = productBehaviour.lcy ?? 0.0;
                    }
                    else if (collateralCurrencyId != item.CURRENCYID)
                    {
                        maximumExtendableValuePercentage = productBehaviour.fcy ?? 0.0;
                    }
                    rate = (maximumExtendableValuePercentage > 100) ? (maximumExtendableValuePercentage / 100) : (100 / maximumExtendableValuePercentage);
                    colValue += ((double)rate * (double)item.PROPOSEDAMOUNT);
                }

            }
            return new { loanAmount = totatlLoan, loanCurrency = loanCurrency, fx = fx, requiredCollateral = colValue };
        }

        public bool UpdateLoanApplicationDetails(LoanApplicationDatailViewModel entity, UserInfo user)
        {
            decimal newAmount;
            var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONDETAILID == entity.applicationDetailedId).FirstOrDefault();

            var loan = context.TBL_LOAN_APPLICATION.Where(d => d.LOANAPPLICATIONID == entity.loanApplicationId).FirstOrDefault();

            if (loan != null)
            {
                decimal propusedAmount = loanDetails.PROPOSEDAMOUNT;
                decimal loanAmount = loan.APPLICATIONAMOUNT;
                newAmount = loanAmount - propusedAmount;

                decimal totalAmount = 0; // (GetCustomerTotalOutstandingBalance(loanDetails.CUSTOMERID) - propusedAmount) + entity.proposedAmount;
                loan.APPLICATIONAMOUNT = newAmount + entity.proposedAmount;
                loan.TOTALEXPOSUREAMOUNT = newAmount + entity.proposedAmount;
                loanDetails.PROPOSEDTENOR = entity.proposedTenor;
                loanDetails.APPROVEDTENOR = entity.proposedTenor;
                loanDetails.PROPOSEDAMOUNT = entity.proposedAmount;
                loanDetails.APPROVEDAMOUNT = entity.proposedAmount;
                loanDetails.LOANPURPOSE = loanDetails.LOANPURPOSE;
                loanDetails.ISLINEFACILITY = loanDetails.ISLINEFACILITY;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationUpdate,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Updated loan application with reference Number: {loan.APPLICATIONREFERENCENUMBER}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.applicationDetailedId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() > 0;
        }

        public decimal GetCustomerTotalOutstandingBalance(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);
            decimal loanBalance = 0;
            decimal overdraftBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN
                               where a.CUSTOMERID == customerId
                               select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }
            else
            {
                loanBalance = 0;
            }

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING
                               where a.CUSTOMERID == customerId
                               select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }
            else
            {
                overdraftBalance = 0;
            }

            decimal totalBalance = loanBalance + overdraftBalance;

            return totalBalance;
        }

        public bool ProductFeesConcession(ProductFeesViewModel fees, UserInfo user)
        {
            var entity = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(c => c.CHARGEFEEID == fees.feeId && c.LOANAPPLICATIONDETAILID == fees.loanApplicationDetailId).FirstOrDefault();

            entity.RECOMMENDED_FEERATEVALUE = fees.rate;
            entity.DATETIMEUPDATED = DateTime.Now;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.HASCONSESSION = true;
            entity.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
            entity.CONSESSIONREASON = fees.consessionReason;
            entity.LOANAPPLICATIONDETAILID = fees.loanApplicationDetailId;
            entity.RECOMMENDED_FEERATEVALUE = fees.rate;



            // Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
            //        STAFFID = fees.staffId,
            //        BRANCHID = (short)fees.userBranchId,
            //        DETAIL = $"Concession request",
            //        IPADDRESS = fees.userIPAddress,
            //        URL = fees.applicationUrl,
            //        APPLICATIONDATE = genSetup.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now,
            //        TARGETID = fees.loanApplicationDetailId 
            //    };
            //    this.auditTrail.AddAuditTrail(audit);

            return context.SaveChanges() > 0;

        }


        public List<ProductFeeViewModel> GetLoanApplicationProductFees(int loanApplicationDeatilId)
        {
            var loanAppProdFee = (from fa in context.TBL_LOAN_APPLICATION_DETL_FEE
                                  where fa.LOANAPPLICATIONDETAILID == loanApplicationDeatilId
                                  && fa.DELETED == false
                                  select new ProductFeeViewModel
                                  {
                                      feeName = fa.TBL_CHARGE_FEE.CHARGEFEENAME,
                                      loanChargeFeeId = fa.LOANCHARGEFEEID,
                                      loanApplicationDetailId = fa.LOANAPPLICATIONDETAILID,
                                      chargeFeeId = fa.CHARGEFEEID,
                                      hasConsession = fa.HASCONSESSION,
                                      consessionReason = fa.CONSESSIONREASON,
                                      approvalStatusId = fa.APPROVALSTATUSID,
                                      defaultfeeRateValue = fa.DEFAULT_FEERATEVALUE,
                                      recommededFeeRateValue = fa.RECOMMENDED_FEERATEVALUE,
                                      feeAmount = 0,
                                      feeIntervalName = fa.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                                      isIntegralFee = fa.TBL_CHARGE_FEE.ISINTEGRALFEE,
                                      isRecurring = fa.TBL_CHARGE_FEE.RECURRING,

                                  }).ToList();
            return loanAppProdFee;
        }

        public IEnumerable<ProductFeesViewModel> GetLoanApplicationFees(int loanDetailId)
        {
            var data = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(c => c.LOANAPPLICATIONDETAILID == loanDetailId).Select(c => new ProductFeesViewModel
            {
                defaultfeeRateValue = c.DEFAULT_FEERATEVALUE,
                rate = c.RECOMMENDED_FEERATEVALUE,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                feeId = c.CHARGEFEEID,
                feeName = c.TBL_CHARGE_FEE.CHARGEFEENAME,
                customerName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME + " " + c.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME,
                productName = c.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME
            });
            return data;
        }

        public IEnumerable<LoanApplicationViewModel> SearchForLoan(string searchString)
        {
            var applications = (from a in context.TBL_LOAN_APPLICATION
                                join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                join d in context.TBL_CASA on c.CUSTOMERID equals d.CUSTOMERID
                                where (a.APPLICATIONREFERENCENUMBER == searchString
                                                       || d.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchString.ToLower())
                                                       || c.FIRSTNAME.ToLower().Contains(searchString.ToLower())
                                                       || c.MAIDENNAME.ToLower().Contains(searchString.ToLower())
                                                       || c.MIDDLENAME.ToLower().Contains(searchString.ToLower())
                                                       || c.CUSTOMERCODE == searchString)

                                select new LoanApplicationViewModel
                                {
                                    customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                    customerCode = c.CUSTOMERCODE,
                                    loanApplicationId = a.LOANAPPLICATIONID,
                                    loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                                    applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    customerId = a.CUSTOMERID,
                                    applicationAmount = b.PROPOSEDAMOUNT,
                                    interestRate = b.PROPOSEDINTERESTRATE,
                                    applicationTenor = b.PROPOSEDTENOR,
                                    productName = b.TBL_PRODUCT.PRODUCTNAME,
                                    submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                                    customerInfoValidated = a.CUSTOMERINFOVALIDATED,
                                    isRelatedParty = a.ISRELATEDPARTY,
                                    branchName = a.TBL_BRANCH.BRANCHNAME,
                                    relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                    loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                    operationId = a.OPERATIONID,
                                    accountNumber = d.PRODUCTACCOUNTNUMBER,
                                });

            return applications.GroupBy(x => x.loanApplicationDetailId).Select(d => d.FirstOrDefault()).ToList();
        }

        public WorkflowResponse RerouteWorkflowTarget(ForwardViewModel model)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.ProductClassId = model.productClassId > 0 ? model.productClassId : null;
            workflow.ProductId = model.productId > 0 ? model.productId : null;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId > 0 ? model.receiverStaffId : null;
            workflow.StatusId = model.forwardAction;
            workflow.Comment = model.comment;
            workflow.LogActivity();
            return workflow.Response;
        }

        public WorkflowResponse RouteWorkflowTarget(ForwardViewModel model)
        {
            workflow.StaffId = model.createdBy;
            workflow.OperationId = model.operationId;
            workflow.TargetId = model.applicationId;
            workflow.CompanyId = model.companyId;
            workflow.Comment = model.comment;
            workflow.ToStaffId = model.receiverStaffId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ExternalInitialization = true;
            workflow.StatusId = (short)ApprovalStatusEnum.Pending;
            workflow.Amount = model.amount;

            workflow.LogActivity();

            context.SaveChanges();

            return workflow.Response;
        }


        public List<LoanApplicationViewModel> GetLoanApplication(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_APPLICATION
                                   join r in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals r.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where (a.APPLICATIONREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanApplicationViewModel
                                   {
                                       loanApplicationId = a.LOANAPPLICATIONID,
                                       customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                       productName = r.TBL_PRODUCT.PRODUCTNAME,
                                       applicationAmount = a.APPLICATIONAMOUNT,
                                       applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER
                                   });
            return allFilteredLoan.ToList();
        }
        private IQueryable<WorkflowTrackerViewModel> GetApprovalTrail(int companyId, int staffId, int targetId, int operationId)
        {
            var loggedsStaff = context.TBL_STAFF.Find(staffId);
            var result = (from a in context.TBL_APPROVAL_TRAIL
                              // join b in context.TBL_APPROVAL_LEVEL on a.FROMAPPROVALLEVELID equals b.APPROVALLEVELID
                              //  join c in context.TBL_APPROVAL_GROUP on b.GROUPID equals c.GROUPID
                              // join d in context.TBL_APPROVAL_GROUP_MAPPING on c.GROUPID equals d.GROUPID
                              // join e in context.TBL_OPERATIONS on d.OPERATIONID equals e.OPERATIONID

                              // join i in context.TBL_STAFF on a.REQUESTSTAFFID equals i.STAFFID
                              //join j in context.TBL_STAFF on a.RESPONSESTAFFID equals j.STAFFID into apprStaff
                              // from j in apprStaff.DefaultIfEmpty()
                              // join k in context.TBL_APPROVAL_STATUS on a.APPROVALSTATUSID equals k.APPROVALSTATUSID
                          where a.COMPANYID == companyId
                          where a.TARGETID == targetId && a.OPERATIONID == operationId
                          select new WorkflowTrackerViewModel
                          {
                              arrivalDate = a.ARRIVALDATE,
                              responseApprovalLevel = a.TOAPPROVALLEVELID.HasValue ? a.TBL_APPROVAL_LEVEL1.LEVELNAME : "N/A",
                              responseDate = a.SYSTEMRESPONSEDATETIME ?? DateTime.Now,
                              systemArrivalDate = a.SYSTEMARRIVALDATETIME,
                              systemResponseDate = a.SYSTEMRESPONSEDATETIME,
                              requestStaffId = a.REQUESTSTAFFID,
                              responseStaffName = !a.TOAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL1.LEVELNAME,
                              comment = a.COMMENT,
                              requestStaffName = a.TBL_STAFF.FIRSTNAME != null ? a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME : null,
                              requestApprovalLevel = !a.FROMAPPROVALLEVELID.HasValue ? "Initiation" : a.TBL_APPROVAL_LEVEL.LEVELNAME,
                              TargetId = a.TARGETID,
                              // operationId = e.OPERATIONID,
                              // operationName = e.OPERATIONNAME,
                              //approvalStatus = context.TBL_APPROVAL_STATUS.Where(x=>x.APPROVALSTATUSID == a.APPROVALSTATUSID).FirstOrDefault().APPROVALSTATUSNAME
                              approvalStatus = a.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME
                          }).Distinct();


            var response = result.ToList();
            return result;
        }

        public IEnumerable<WorkflowTrackerViewModel> GetApprovalTrailByOperationIdAndTargetId(int operationId, int targetId, int companyId, int staffId)
        {
            var result = GetApprovalTrail(companyId, staffId, targetId, operationId).ToList();
            return result.Distinct();
        }

        public int SaveCancelledApplcation(LoanApplicationViewModel data)
        {

            var appl = context.TBL_LOAN_APPLICATION.Find(data.loanApplicationId);
            var ApprovalTrail = GetApprovalTrailByOperationIdAndTargetId(appl.OPERATIONID, data.loanApplicationId, data.companyId, data.createdBy);
            //var ApprovalTrail = GetApprovalTrailByOperationIdAndTargetId((int)OperationsEnum.CreditAppraisal, data.loanApplicationId, data.companyId, data.createdBy);
            var ApprovalStaffCount = ApprovalTrail.Where(a => a.requestStaffId != data.createdBy).Count();
            var staffRecord = context.TBL_STAFF.Find(data.createdBy);
            var userAdminRole = context.TBL_STAFF_ROLE.Where(x => x.STAFFROLENAME.ToUpper().Contains("USER ADMIN")).FirstOrDefault();
            if (ApprovalStaffCount == 0 || (staffRecord != null && staffRecord.STAFFROLEID == userAdminRole?.STAFFROLEID))
            {
                LaonApplcationCancelllationCompelted(data);
                UpdateLoanApplicationCollateralTable(data.loanApplicationId, (int)ApprovalStatusEnum.Approved, data.createdBy);

                if (context.SaveChanges() > 0)
                {
                    return 1;
                }
                return 0;
            }
            var isCancellatuionInProgress = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress).Any();
                if (isCancellatuionInProgress == true)
                    throw new ConditionNotMetException(" This Loan is currently under going cancellation process");

                var isCancellatuionCompleted = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted).Any();
                if (isCancellatuionCompleted == true)
                    throw new ConditionNotMetException(" This Loan has already been cancelled");

                var application = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
                if (application != null)
                {
                    var cancelledApplication = new TBL_TEMP_LOAN_APPLTN_CANCELTN
                    {
                        LOANAPPLICATIONID = data.loanApplicationId,
                        CANCELLATIONREASON = data.cancellationReason,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                        CREATEDBY = data.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        ISCURRENT = true,
                        APPLICATIONSTATUSID = data.applicationStatusId,
                        LASTUPDATEDBY = data.createdBy,
                        DATETIMEUPDATED = genSetup.GetApplicationDate()
                    };

                    context.TBL_TEMP_LOAN_APPLTN_CANCELTN.Add(cancelledApplication);

                    if (context.SaveChanges() > 0)
                    {
                        data.tempApplicationCancellationId = cancelledApplication.TEMPAPPLICATIONCANCELLATIONID;
                    }

                    LaonApplcationCancelllationInPregress(data);

                    //EMAIL TO NOTIFY STACK HOLDERS ON PENDING CANCELLATION
                    var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                    string messageBoby = $"Dear Team, <br /><br />This is to bring your attention a request for loan application cancellation has been initiated by {staffName} on {data.applicationReferenceNumber} application refernence number. The Loan application is currently under going approval. <br /><br />";
                    string alertSubject = $"Loan Application Cancellation Approval Notification";
                    LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanApplicationId));

                    workflow.StaffId = data.createdBy;
                    workflow.CompanyId = data.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = data.tempApplicationCancellationId;
                    workflow.Comment = $"Cancellation request for Loan Application ID with '{data.loanApplicationId}' has been initiated. Reason being : {data.cancellationReason}  ";
                    workflow.OperationId = (int)OperationsEnum.LoanApplicationCancellation;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                }

            if (context.SaveChanges() > 0)
            {
                return 2;
            }
            return 0;
        }


        public int SaveLMSCancelledApplcation(LoanReviewApplicationViewModel data)
        {

            var appl = context.TBL_LMSR_APPLICATION.Find(data.loanReviewApplicationId);
            var ApprovalTrail = GetApprovalTrailByOperationIdAndTargetId(appl.OPERATIONID, data.loanReviewApplicationId, data.companyId, data.createdBy);
            var ApprovalStaffCount = ApprovalTrail.Where(a => a.requestStaffId != data.createdBy).Count();
            var staffRecord = context.TBL_STAFF.Find(data.createdBy);
            var userAdminRole = context.TBL_STAFF_ROLE.Where(x => x.STAFFROLENAME.ToUpper().Contains("USER ADMIN")).FirstOrDefault();
            if (ApprovalStaffCount == 0 || (staffRecord != null && staffRecord.STAFFROLEID == userAdminRole?.STAFFROLEID))
            {
                LmsLaonApplcationCancelllationCompelted(data);

                if (context.SaveChanges() > 0)
                {
                    return 1;
                }
                return 0;
            }
            var isCancellatuionInProgress = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanReviewApplicationId && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationInProgress).Any();
            if (isCancellatuionInProgress == true)
                throw new ConditionNotMetException(" This Loan is currently under going cancellation process");

            var isCancellatuionCompleted = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanReviewApplicationId && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CancellationCompleted).Any();
            if (isCancellatuionCompleted == true)
                throw new ConditionNotMetException(" This Loan has already been cancelled");

            var application = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanReviewApplicationId).Select(x => x).FirstOrDefault();
            if (application != null)
            {
                var cancelledApplication = new TBL_TEMP_LMSR_APPLTN_CANCELTN
                {
                    LOANAPPLICATIONID = data.loanReviewApplicationId,
                    CANCELLATIONREASON = data.cancellationReason,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    CREATEDBY = data.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    ISCURRENT = true,
                    APPLICATIONSTATUSID = data.applicationStatusId,
                    LASTUPDATEDBY = data.createdBy,
                    DATETIMEUPDATED = genSetup.GetApplicationDate()
                };

                context.TBL_TEMP_LMSR_APPLTN_CANCELTN.Add(cancelledApplication);

                if (context.SaveChanges() > 0)
                {
                    data.tempApplicationCancellationId = cancelledApplication.TEMPAPPLICATIONCANCELLATIONID;
                }

                LmsLaonApplcationCancelllationInPregress(data);

                //EMAIL TO NOTIFY STACK HOLDERS ON PENDING CANCELLATION
                var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                string messageBoby = $"Dear Team, <br /><br />This is to bring to your attention that a request for LMS application cancellation has been initiated by {staffName} on {appl.APPLICATIONREFERENCENUMBER} application refernence number. The Loan application is currently under going approval. <br /><br />";
                string alertSubject = $"Loan Application Cancellation Approval Notification";
                LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanReviewApplicationId));

                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = data.tempApplicationCancellationId;
                workflow.Comment = $"Cancellation request for Loan Application ID with '{data.loanReviewApplicationId}' has been initiated. Reason being : {data.cancellationReason}  ";
                workflow.OperationId = (int)OperationsEnum.LmsLoanApplicationCancellation;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

            if (context.SaveChanges() > 0)
            {
                return 2;
            }
            return 0;
        }
        public List<LoanApplicationViewModel> GetAllRequestsForLoanCancellation(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanApplicationCancellation).ToList();


            var applications = (from t in context.TBL_TEMP_LOAN_APPLTN_CANCELTN
                                join a in context.TBL_LOAN_APPLICATION on t.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                join g in context.TBL_CUSTOMER on a.CUSTOMERID equals g.CUSTOMERID
                                join atrail in context.TBL_APPROVAL_TRAIL on t.TEMPAPPLICATIONCANCELLATIONID equals atrail.TARGETID
                                where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                        && atrail.OPERATIONID == (int)OperationsEnum.LoanApplicationCancellation
                                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                        && atrail.RESPONSESTAFFID == null
                                select new LoanApplicationViewModel
                                {
                                    applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    customerName = g.FIRSTNAME + "" + g.MIDDLENAME + "" + g.LASTNAME,
                                    loanApplicationId = a.LOANAPPLICATIONID,
                                    applicationDate = a.APPLICATIONDATE,
                                    applicationAmount = a.APPLICATIONAMOUNT,
                                    approvedAmount = a.APPROVEDAMOUNT,
                                    interestRate = a.INTERESTRATE,
                                    applicationTenor = a.APPLICATIONTENOR,
                                    submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                                    customerInfoValidated = a.CUSTOMERINFOVALIDATED,
                                    isRelatedParty = a.ISRELATEDPARTY,
                                    isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                                    approvalStatusId = (short)a.APPROVALSTATUSID,
                                    //  approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    applicationStatusId = a.APPLICATIONSTATUSID,
                                    branchName = a.TBL_BRANCH.BRANCHNAME,
                                    createdBy = a.OWNEDBY,
                                    loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                                    loantermSheetId = a.LOANTERMSHEETID,
                                    operationId = a.OPERATIONID,
                                    tempApplicationCancellationId = t.TEMPAPPLICATIONCANCELLATIONID
                                });

            var list = applications.ToList();

            return list;

        }

        public List<LoanReviewApplicationViewModel> GetAllLmsRequestsForLoanCancellation(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LmsLoanApplicationCancellation).ToList();


            var applications = (from t in context.TBL_TEMP_LMSR_APPLTN_CANCELTN
                                join a in context.TBL_LMSR_APPLICATION on t.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join d in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                join g in context.TBL_CUSTOMER on a.CUSTOMERID equals g.CUSTOMERID
                                join atrail in context.TBL_APPROVAL_TRAIL on t.TEMPAPPLICATIONCANCELLATIONID equals atrail.TARGETID
                                where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                        && atrail.OPERATIONID == (int)OperationsEnum.LmsLoanApplicationCancellation
                                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                        && atrail.RESPONSESTAFFID == null
                                select new LoanReviewApplicationViewModel
                                {
                                    applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                    customerName = g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME,
                                    loanApplicationId = a.LOANAPPLICATIONID,
                                    applicationDate = a.APPLICATIONDATE,
                                    applicationAmount = d.PROPOSEDAMOUNT,
                                    approvedAmount = d.APPROVEDAMOUNT,
                                    interestRate = d.APPROVEDINTERESTRATE,
                                    applicationTenor = d.APPROVEDTENOR,
                                    approvalStatusId = (short)a.APPROVALSTATUSID,
                                    //  approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == a.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    applicationStatusId = a.APPLICATIONSTATUSID,
                                    branchName = a.TBL_BRANCH.BRANCHNAME,
                                    createdBy = a.CREATEDBY,
                                    operationId = a.OPERATIONID,
                                    tempApplicationCancellationId = t.TEMPAPPLICATIONCANCELLATIONID,
                                    
                                });

            var list = applications.ToList();

            return list;

        }

        public LoanApplicationViewModel ViewLaonApplicationCancellationDetails(LoanApplicationViewModel values)
        {
            var data = Search(values.applicationReferenceNumber);
            if (data != null)
            {
                var newData = data.FirstOrDefault(x => x.applicationReferenceNumber == values.applicationReferenceNumber);
                newData.operationId = values.operationId;
                return newData;
            }
            return new LoanApplicationViewModel();
        }

        public LoanReviewApplicationViewModel ViewLmsLaonApplicationCancellationDetails(LoanReviewApplicationViewModel values)
        {
            var data = LmsSearch(values.applicationReferenceNumber);
            if (data != null)
            {
                var newData = data.FirstOrDefault(x => x.applicationReferenceNumber == values.applicationReferenceNumber);
                newData.operationId = values.operationId;
                return newData;
            }
            return new LoanReviewApplicationViewModel();
        }

        public bool GoForLoanApplicationCancellationApproval(LoanApplicationViewModel data)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = data.tempApplicationCancellationId;
                workflow.Comment = data.comment;
                workflow.OperationId = (int)OperationsEnum.LoanApplicationCancellation;
                workflow.DeferredExecution = true;
                //workflow.ExternalInitialization = true;
                workflow.LogActivity();

                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (data.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            UpdateLoanApplicationCancellationTempTable(data, (short)workflow.StatusId);
                            LaonApplcationCancelllationDisapproved(data);

                            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
                            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                            string messageBoby = $"Dear Team, <br /><br />This is to bring your attention the loan with {data.applicationReferenceNumber} application refernence number which was going through approval for cancellation has been successfully disapproved by {staffName}. <br /><br />";
                            string alertSubject = $"Loan Application Cancellation Approval Notification";
                            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanApplicationId));
                        }
                        else
                        {
                            UpdateLoanApplicationCancellationTempTable(data, (short)workflow.StatusId);
                            UpdateLoanApplicationCollateralTable(data.loanApplicationId, (short)workflow.StatusId, data.createdBy);
                            LaonApplcationCancelllationCompelted(data);

                            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
                            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                            string messageBoby = $"Dear Team, <br /><br />This is to bring your attention the loan with {data.applicationReferenceNumber} application refernence number which was going through approval for cancellation has been successfully approved by {staffName}. <br /><br />";
                            string alertSubject = $"Loan Application Cancellation Approval Notification";
                            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanApplicationId));
                        }
                    }

                    responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }

        }

        public bool GoForLmsLoanApplicationCancellationApproval(LoanReviewApplicationViewModel data)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = data.tempApplicationCancellationId;
                workflow.Comment = data.comment;
                workflow.OperationId = (int)OperationsEnum.LmsLoanApplicationCancellation;
                workflow.DeferredExecution = true;
                //workflow.ExternalInitialization = true;
                workflow.LogActivity();

                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (data.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                        {
                            UpdateLmsLoanApplicationCancellationTempTable(data, (short)workflow.StatusId);
                            LmsLaonApplcationCancelllationDisapproved(data);

                            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
                            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                            string messageBoby = $"Dear Team, <br /><br />This is to bring to your attention that the loan review with {data.referenceNumber} application refernence number which was going through approval for cancellation has been successfully disapproved by {staffName}. <br /><br />";
                            string alertSubject = $"Loan Review Application Cancellation Approval Notification";
                            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanApplicationId));
                        }
                        else
                        {
                            UpdateLmsLoanApplicationCancellationTempTable(data, (short)workflow.StatusId);
                            UpdateLoanApplicationCollateralTable(data.loanApplicationId, (short)workflow.StatusId, data.createdBy);
                            LmsLaonApplcationCancelllationCompelted(data);

                            //NOTIFY STAKE HOLDER OF THE TOTAL CANCELLATION
                            var staffName = this.context.TBL_STAFF.Where(x => x.STAFFID == data.createdBy).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault();
                            string messageBoby = $"Dear Team, <br /><br />This is to bring your attention the loan with {data.referenceNumber} application refernence number which was going through approval for cancellation has been successfully approved by {staffName}. <br /><br />";
                            string alertSubject = $"Loan Application Cancellation Approval Notification";
                            LogEmailAlertForLoanApplicationCancellation(messageBoby, alertSubject, GetLoanApplicationEmailRecipients(data.loanApplicationId));
                        }
                    }

                    responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }

        }
        private void UpdateLoanApplicationCancellationTempTable(LoanApplicationViewModel data, short statusId)
        {
            var val = context.TBL_TEMP_LOAN_APPLTN_CANCELTN.Where(x => x.TEMPAPPLICATIONCANCELLATIONID == data.tempApplicationCancellationId).Select(x => x).FirstOrDefault();
            val.ISCURRENT = false;
            val.APPROVALSTATUSID = statusId;
            val.LASTUPDATEDBY = data.createdBy;
            val.DATETIMEUPDATED = genSetup.GetApplicationDate();
            //val.APPLICATIONSTATUSID = data.applicationStatusId;

            
        }

        private void UpdateLoanApplicationCollateralTable(int loanApplicationId, short statusId, int user)
        {
            var val = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Select(x => x).ToList();
            if (val.Count() > 0)
            {
                foreach(var i in val)
                {
                    i.APPROVALSTATUSID = statusId;
                    i.LASTUPDATEDBY = user;
                    i.DATETIMEUPDATED = genSetup.GetApplicationDate();
                    i.DELETED = true;
                    i.DELETEDBY = user;
                    i.DATETIMEDELETED = DateTime.Now;
                }
                
            }
            
        }

        private void UpdateLmsLoanApplicationCancellationTempTable(LoanReviewApplicationViewModel data, short statusId)
        {
            var val = context.TBL_TEMP_LMSR_APPLTN_CANCELTN.Where(x => x.TEMPAPPLICATIONCANCELLATIONID == data.tempApplicationCancellationId).Select(x => x).FirstOrDefault();
            val.ISCURRENT = false;
            val.APPROVALSTATUSID = statusId;
            val.LASTUPDATEDBY = data.createdBy;
            val.DATETIMEUPDATED = genSetup.GetApplicationDate();
            // val.APPLICATIONSTATUSID = data.applicationStatusId;
        }

        private void LaonApplcationCancelllationCompelted(LoanApplicationViewModel data)
        {
            var val = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
            val.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationCompleted;
            val.LASTUPDATEDBY = data.createdBy;
            val.DATETIMEUPDATED = DateTime.Now;
            ArchiveLoanApplication(data.loanApplicationId, (int)OperationsEnum.LoanApplicationCancellation, val.APPLICATIONSTATUSID, data.createdBy);
            var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == data.createdBy);
            var lastTrail = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == val.LOANAPPLICATIONID && t.OPERATIONID == val.OPERATIONID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault();
            if (lastTrail != null)
            {
                lastTrail.RESPONSEDATE = genSetup.GetApplicationDate();
                lastTrail.SYSTEMRESPONSEDATETIME = DateTime.Now;
                lastTrail.RESPONSESTAFFID = data.createdBy;
                lastTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                lastTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                lastTrail.COMMENT += " Loan was Cancelled by " + staff.STAFFCODE + " Reason being : " + data.cancellationReason;
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationCancellation,
                STAFFID = data.createdBy,
                BRANCHID = (short)data.userBranchId,
                DETAIL = $"Cancellation request for Loan Application ID with '{data.loanApplicationId}' has been completed. Reason being : {data.cancellationReason} ",
                IPADDRESS = data.userIPAddress,
                URL = data.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = val.LOANAPPLICATIONID,
            };


            this.auditTrail.AddAuditTrail(audit);
        }

        private void LmsLaonApplcationCancelllationCompelted(LoanReviewApplicationViewModel data)
        {
            if(data.loanApplicationId == 0)
            {
                data.loanApplicationId = data.loanReviewApplicationId;
            }
            var val = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
            val.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationCompleted;
            val.LASTUPDATEDBY = data.createdBy;
            val.DATETIMEUPDATED = DateTime.Now;
            ArchiveLmsLoanApplication(data.loanApplicationId, (int)OperationsEnum.LmsLoanApplicationCancellation, val.APPLICATIONSTATUSID, data.createdBy);
            var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == data.createdBy);
            var lastTrail = context.TBL_APPROVAL_TRAIL.Where(t => t.TARGETID == val.LOANAPPLICATIONID && t.OPERATIONID == val.OPERATIONID).OrderByDescending(t => t.APPROVALTRAILID).FirstOrDefault();
            if (lastTrail != null)
            {
                lastTrail.RESPONSEDATE = genSetup.GetApplicationDate();
                lastTrail.SYSTEMRESPONSEDATETIME = DateTime.Now;
                lastTrail.RESPONSESTAFFID = data.createdBy;
                lastTrail.APPROVALSTATEID = (int)ApprovalState.Ended;
                lastTrail.APPROVALSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                lastTrail.COMMENT += " Lms Loan was Cancelled by " + staff.STAFFCODE + " Reason being : " + data.cancellationReason;
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationCancellation,
                STAFFID = data.createdBy,
                BRANCHID = (short)data.userBranchId,
                DETAIL = $"Cancellation request for Lms Application ID with '{data.loanApplicationId}' has been completed. Reason being : {data.cancellationReason} ",
                IPADDRESS = data.userIPAddress,
                URL = data.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = val.LOANAPPLICATIONID,
            };


            this.auditTrail.AddAuditTrail(audit);
        }

        private void LaonApplcationCancelllationDisapproved(LoanApplicationViewModel data)
        {
            var value = context.TBL_TEMP_LOAN_APPLTN_CANCELTN.Where(x => x.TEMPAPPLICATIONCANCELLATIONID == data.tempApplicationCancellationId).Select(x => x).FirstOrDefault();
            if (value != null)
            {
                var val = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
                val.APPLICATIONSTATUSID = value.APPLICATIONSTATUSID;
            }
        }

        private void LmsLaonApplcationCancelllationDisapproved(LoanReviewApplicationViewModel data)
        {
            var value = context.TBL_TEMP_LMSR_APPLTN_CANCELTN.Where(x => x.TEMPAPPLICATIONCANCELLATIONID == data.tempApplicationCancellationId).Select(x => x).FirstOrDefault();
            if (value != null)
            {
                var val = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanReviewApplicationId).Select(x => x).FirstOrDefault();
                val.APPLICATIONSTATUSID = value.APPLICATIONSTATUSID;
            }
        }
        private void LaonApplcationCancelllationInPregress(LoanApplicationViewModel data)
        {
            var val = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
            val.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationInProgress;
        }

        private void LmsLaonApplcationCancelllationInPregress(LoanReviewApplicationViewModel data)
        {
            var val = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanReviewApplicationId).Select(x => x).FirstOrDefault();
            val.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationInProgress;
        }

        private void LogEmailAlertForLoanApplicationCancellation(string messageBody, string alertSubject, string recipients)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = recipients.Trim();
                string messageSubject = title;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which are approaching their due date. <br /><br />";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime
            };

            context.TBL_MESSAGE_LOG.Add(message);

        }

        private string GetLoanApplicationEmailRecipients(int targetId)
        {
            string recipientEmailAddresses = string.Empty;
            int? staffId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == targetId).Select(x => x.REQUESTSTAFFID).FirstOrDefault();
            if (staffId != null)
            {
                return context.TBL_STAFF.Where(x => x.STAFFID == staffId).Select(x => x.EMAIL).FirstOrDefault();

            }
            else
            {
                int? approvalLevelId = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == targetId).OrderByDescending(x => x.SYSTEMARRIVALDATETIME).Select(x => x.FROMAPPROVALLEVELID).FirstOrDefault();
                var staffIds = context.TBL_APPROVAL_LEVEL.Where(x => x.APPROVALLEVELID == approvalLevelId).Select(x =>
                new StaffInfoViewModel
                {
                    staffId = (int)x.STAFFROLEID
                }).ToList();
                foreach (var a in staffIds)
                    recipientEmailAddresses = context.TBL_STAFF.Where(x => x.STAFFID == a.staffId).Select(x => x.EMAIL).FirstOrDefault() + ";";
                var emails = recipientEmailAddresses.TrimEnd(';');
                return emails;
            }

        }

        public List<TransactionDynamicsViewModel> GetTrnasactionDynamics(int loanApplicationId)
        {
            int detailId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Select(x => x.LOANAPPLICATIONDETAILID).FirstOrDefault();
            return (from x in context.TBL_LOAN_TRANSACTION_DYNAMICS
                    where x.LOANAPPLICATIONDETAILID == detailId
                    select new TransactionDynamicsViewModel
                    {
                        dynamicsId = x.DYNAMICSID,
                        dynamics = x.DYNAMICS,
                        loanApplicationDetailId = x.LOANAPPLICATIONDETAILID
                    }).ToList();
        }
        public List<ConditionPrecedentViewModel> GetConditionPrecidents(int loanApplicationId)
        {
            int detailId = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Select(x => x.LOANAPPLICATIONDETAILID).FirstOrDefault();
            return (from x in context.TBL_LOAN_CONDITION_PRECEDENT
                    where x.LOANAPPLICATIONDETAILID == detailId
                    select new ConditionPrecedentViewModel
                    {
                        loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                        condition = x.CONDITION
                    }).ToList();
        }
        public List<ConditionPrecedentViewModel> GetLMSConditionPrecidents(int loanApplicationId)
        {
            int detailId = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplicationId).Select(x => x.LOANREVIEWAPPLICATIONID).FirstOrDefault();
            return (from x in context.TBL_LMSR_CONDITION_PRECEDENT
                    where x.LOANREVIEWAPPLICATIONID == detailId
                    select new ConditionPrecedentViewModel
                    {
                        loanApplicationDetailId = x.LOANREVIEWAPPLICATIONID,
                        condition = x.CONDITION
                    }).ToList();
        }
        public bool updateSuggestionsLoanApplicationdetail(LoanApplicationDetailViewModel model)
        {
            if (model == null) return false;

            var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.loanApplicationDetailId);
            if (loanDetail != null)
            {
                loanDetail.CONDITIONPRECIDENT = model.conditionPrecedent;
                loanDetail.CONDITIONSUBSEQUENT = model.conditionSubsequent;
                loanDetail.TRANSACTIONDYNAMICS = model.transactionDynamics;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationUpdate,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated loan application detail with with applicationdetailId : {model.loanApplicationDetailId}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = model.loanApplicationDetailId
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() > 0;
        }

        public IEnumerable<LookupViewModel> GetAllCRMSFundingSource()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.FundingSource).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllCRMSRepaymentSource()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.RepaymentSourceType).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllCRMSRepaymentAgreementType()
        {
            return context.TBL_CRMS_REGULATORY.Where(x => x.CRMSTYPEID == (int)RegulatoryTypeEnum.RepaymentAgreementType).Select(x => new LookupViewModel()
            {
                lookupId = (short)x.CRMSREGULATORYID,
                lookupName = x.CODE + "-" + x.DESCRIPTION
            }).ToList();
        }

        public IEnumerable<LookupViewModel> GetAllSyndicationType()
        {
            return context.TBL_LOAN_SYNDICATION_PARTY_TYP.Select(x => new LookupViewModel()
            {
                lookupId = (short)x.PARTY_TYPEID,
                lookupName = x.PARTY_TYPENAME
            }).ToList();
        }

        public IEnumerable<LoanApplicationDetailViewModel> GetLoanApplicationDetailsByReference(string reference, int companyId)
        {
            reference = reference.Trim().ToLower();
            var data = (from a in context.TBL_LOAN_APPLICATION
            //var data = (from a in context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == reference)
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.COMPANYID == companyId && a.DELETED == false && b.DELETED == false
                        &&
                        (
                            a.APPLICATIONREFERENCENUMBER.Trim().ToLower() == reference
                        ||  a.TBL_CUSTOMER_GROUP.GROUPNAME.Trim().ToLower().Contains(reference.Trim())
                        ||  a.TBL_CUSTOMER.FIRSTNAME.Trim().ToLower().Contains(reference.Trim())
                        ||  a.TBL_CUSTOMER.MIDDLENAME.Trim().ToLower().Contains(reference.Trim())
                        ||  a.TBL_CUSTOMER.LASTNAME.Trim().ToLower().Contains(reference.Trim())
                        )
                        select new LoanApplicationDetailViewModel
                        {
                            currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                            exchangeRate = b.EXCHANGERATE,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            approvedProductId = b.APPROVEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            sectorId = (short)b.TBL_SUB_SECTOR.SECTORID,
                            subSectorId = b.SUBSECTORID,
                            approvedProductName = context.TBL_PRODUCT.Where(o=>o.PRODUCTID==b.APPROVEDPRODUCTID).Select(o=>o.PRODUCTNAME).FirstOrDefault(),
                            currencyId = b.CURRENCYID,
                            //requireCollateral = a.REQUIRECOLLATERAL,
                            repaymentScheduleId = b.REPAYMENTSCHEDULEID,
                            isTakeOverApplication = b.ISTAKEOVERAPPLICATION,
                            repaymentTerm = b.REPAYMENTTERMS,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            firstName = b.TBL_CUSTOMER.FIRSTNAME,
                            middleName = b.TBL_CUSTOMER.MIDDLENAME,
                            lastName = b.TBL_CUSTOMER.LASTNAME,
                            customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            productClassProcessId = b.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                            productClassId = (short?)b.TBL_PRODUCT.PRODUCTCLASSID,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            customerTypeId = b.TBL_CUSTOMER.CUSTOMERTYPEID,
                            loanDetailReviewTypeId = b.LOANDETAILREVIEWTYPEID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            customerGroupId = (int?)a.CUSTOMERGROUPID,//.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            approvalStatusId = a.APPROVALSTATUSID,
                            productPriceIndexId = b.PRODUCTPRICEINDEXID
                            //customerAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER
                        });
            var result = data.ToList();
            //foreach (var f in result)
            //{

            //}
            // var test = result.Count();
            return result;
        }

        public IEnumerable<LoanApplicationDetailViewModel> SearchApprovedLoanApplicationDetails(string reference, int companyId)
        {
            var data = GetLoanApplicationDetailsByReference(reference, companyId).Where(a => a.approvalStatusId == (int)ApprovalStatusEnum.Approved).ToList(); ;
            var mappings = context.TBL_LOAN_APPLICATION_COLLATERL.Where(m => m.DELETED == false).ToList();
            var filtered = data.Where(d => mappings.Exists(m => d.loanApplicationDetailId == m.LOANAPPLICATIONDETAILID));
            var result = filtered.ToList();
            return result;
        }

        public CurrentCustomerExposure GetCurrentCustomerExposure(int customerId)
        {
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            CurrentCustomerExposure totalExposures = new CurrentCustomerExposure();

            //if (operationId == (int)OperationsEnum.CreditAppraisal)
            //    details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.APPROVEDPRODUCTID }).ToList();
            //else
            //    details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.PRODUCTID }).ToList();

            //foreach (var detail in details)
            //{
            exposure = context.TBL_LOAN
                    .Where(x => x.CUSTOMERID == customerId && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
                        proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                // Same for revolving and contegent facility ...

                exposure = context.TBL_LOAN_REVOLVING
                    .Where(x => x.CUSTOMERID == customerId && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                        proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

                if (exposure.Count() > 0) exposures.AddRange(exposure);


            exposure = context.TBL_LOAN_CONTINGENT
                .Where(x => x.CUSTOMERID == customerId && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                .Select(g => new CurrentCustomerExposure
                {
                    facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                    existingLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    proposedLimit = g.Sum(x => x.CONTINGENTAMOUNT),
                    recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                    reviewDate = DateTime.Now,
                    loanStatus = "Running"
                });

            if (exposure.Count() > 0) exposures.AddRange(exposure);


            totalExposures = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
                prudentialGuideline = String.Empty,
                loanStatus = String.Empty,
            };

            return totalExposures;
        }

        public CurrentCustomerExposure GetCurrentCompanyExposure()
        {
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();
            CurrentCustomerExposure totalExposures = new CurrentCustomerExposure();

            //if (operationId == (int)OperationsEnum.CreditAppraisal)
            //    details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.APPROVEDPRODUCTID }).ToList();
            //else
            //    details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.PRODUCTID }).ToList();

            //foreach (var detail in details)
            //{
            //exposure = context.TBL_LOAN
            //        .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
            //        .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
            //        .Select(g => new CurrentCustomerExposure
            //        {
            //            facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
            //            existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
            //            proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
            //            recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //            PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
            //            PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
            //            reviewDate = DateTime.Now,
            //            prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
            //            loanStatus = "Running"
            //        });

            exposure = from a in context.TBL_GLOBAL_EXPOSURE
                       select new CurrentCustomerExposure
                       {
                           facilityType = a.ADJFACILITYTYPE,
                           existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                           proposedLimit = a.LOANAMOUNYLCY ?? 0,
                           //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                           outstandings = a.TOTALEXPOSURE ?? 0,
                           recommendedLimit = 0,
                           //PastDueObligationsInterest = a.PASTDUEINTEREST,
                           pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                           reviewDate = DateTime.Now,
                           //prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
                           loanStatus = a.CBNCLASSIFICATION,
                           referenceNumber = a.REFERENCENUMBER,
                       };

            if (exposure.Count() > 0) exposures.AddRange(exposure);

            //exposure = from a in context.EXTERNAL_ALERT
            //           where a.CURRENCYTYPE.Contains("FCY")
            //           select new CurrentCustomerExposure
            //           {
            //               facilityType = a.ADJFACILITYTYPE,
            //               existingLimit = a.PRINCIPALOUTSTANDINGBALLCY * (decimal.Parse(a.FXRATE)) ?? 0,
            //               proposedLimit = a.LOANAMOUNTLCY * (decimal.Parse(a.FXRATE)) ?? 0,
            //               //recommendedLimit = a.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //               outstandings = a.TOTALEXPOSURE * (decimal.Parse(a.FXRATE))?? 0,
            //               recommendedLimit = 0,
            //               //PastDueObligationsInterest = a.PASTDUEINTEREST,
            //               PastDueObligationsPrincipal = a.UNPAIDOBLIGATIONAMOUNT * (decimal.Parse(a.FXRATE)) ?? 0,
            //               reviewDate = DateTime.Now,
            //               //prudentialGuideline = a.TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME,
            //               loanStatus = a.CBNCLASSIFICATION,
            //               referenceNumber = a.REFERENCENUMBER,
            //           };

            //if (exposure.Count() > 0) exposures.AddRange(exposure);

            // Same for revolving and contegent facility ...

            //exposure = context.TBL_LOAN_REVOLVING
            //    .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
            //    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
            //    .Select(g => new CurrentCustomerExposure
            //    {
            //        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
            //        existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
            //        proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
            //        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
            //        PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
            //        reviewDate = DateTime.Now,
            //        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
            //        loanStatus = "Running"
            //    });

            //if (exposure.Count() > 0) exposures.AddRange(exposure);


            //exposure = context.TBL_LOAN_CONTINGENT
            //    .Where(x => x.LOANSTATUSID == (int)LoanStatusEnum.Active)
            //    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
            //    .Select(g => new CurrentCustomerExposure
            //    {
            //        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
            //        existingLimit = g.Sum(x => x.CONTINGENTAMOUNT),
            //        proposedLimit = g.Sum(x => x.CONTINGENTAMOUNT),
            //        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //        reviewDate = DateTime.Now,
            //        loanStatus = "Running"
            //    });

            //if (exposure.Count() > 0) exposures.AddRange(exposure);


            totalExposures = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
                prudentialGuideline = String.Empty,
                loanStatus = String.Empty,
            };

            return totalExposures;
        }

        public void ValidateLoanApplicationLimits(LoanApplicationViewModel application)
        {
            var details = application.LoanApplicationDetail;
            int branchId = (int)application.branchId;
            int customerId = (int)application.customerId;
            var customer = context.TBL_CUSTOMER.Find(customerId);
            int productId = details.SingleOrDefault()?.proposedProductId ?? 0;
            decimal applicationAmount = details.Sum(x => x.proposedAmount * (decimal)x.exchangeRate); // proposedAmount should be approvedAmount after application

            TBL_OVERRIDE_DETAIL branchOverrideRequest = null;
            TBL_OVERRIDE_DETAIL sectorOverrideRequest = null;
            TBL_OVERRIDE_DETAIL customerOverrideRequest = null;

            if (customer != null) {
                branchOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == application.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride).FirstOrDefault();
                sectorOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == application.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride).FirstOrDefault();
                customerOverrideRequest = context.TBL_OVERRIDE_DETAIL.Where(c => c.CUSTOMERCODE == customer.CUSTOMERCODE && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.ISUSED == false && c.CREATEDBY == application.createdBy && c.OVERRIDE_ITEMID == (int)OverrideItem.CustomerExposureLimitOverride).FirstOrDefault();
            }

            if (branchOverrideRequest != null)
            {
                //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                //request.ISUSED = true;
            }
            else
            {
                // branch limits
                foreach (var facility in details)
                {
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var branchValidation = limitValidation.ValidateNPLByBranch((short)branchId);
                        decimal branchNplAmount = (decimal)branchValidation.outstandingBalance;
                        var branch = context.TBL_BRANCH.Find(branchId);
                        if (branch.NPL_LIMIT > 0 && branch.NPL_LIMIT < (branchNplAmount + applicationAmount)) throw new SecureException("Branch NPL Limit exceeded!");
                    }
                }
            }

            if (sectorOverrideRequest != null)
            {
                //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                //request.ISUSED = true;
            }
            else
            {
                foreach(var facility in details)
                {
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var sector = context.TBL_SUB_SECTOR.Find(facility.subSectorId);
                        var sectorName = context.TBL_SECTOR.Find(sector.SECTORID).NAME ?? "N/A";
                        var sectorValidation = limitValidation.ValidateNPLBySector(facility.subSectorId);
                        if (sectorValidation != null)
                        {
                            decimal sectorAmount = (decimal)sectorValidation.outstandingBalance + (facility.proposedAmount * (decimal)facility.exchangeRate);
                            decimal sectorsAmount = (decimal)sectorValidation.outstandingSectorsBalance + (facility.proposedAmount * (decimal)facility.exchangeRate);
                            decimal percentageTotalExposure = decimal.Round((sectorAmount / sectorsAmount), 5, MidpointRounding.AwayFromZero);
                            if (percentageTotalExposure > 0 && percentageTotalExposure >= sectorValidation.maximumAllowedLimit)
                                throw new SecureException("Sector Limit for sector, " + sectorName + " exceeded!");
                            //if (sectorValidation.maximumAllowedLimit > 0 && sectorValidation.maximumAllowedLimit <= sectorAmount) throw new SecureException("Sector Limit for sector, " + facility.sectorName + " exceeded!");
                        }
                    } 
                }

            }
            if (limitValidation.ProductLimitExceeded(productId, details.SingleOrDefault()?.proposedAmount ?? 0))
            {
                throw new SecureException("Product Limit exceeded!");
            }

            var exposure = GetCurrentCompanyExposure();
            var proposedExposure = exposure.outstandings + applicationAmount;
            //var proposedExposure = exposure.proposedLimit + applicationAmount;
            var company = context.TBL_COMPANY.Find(application.companyId);
            if (proposedExposure >= company.SHAREHOLDERSFUND)
            {
                throw new SecureException("Company Limit Exceeded");
            }

            if (limitValidation.ValidateIsInsiderCustomer(customerId))
            {
                foreach (var facility in details)
                {
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var insiderLimit = limitValidation.ValidateNPLByInsiderCustomer();
                        var insiderExposure = insiderLimit.outstandingBalance + (double)applicationAmount;
                        if (insiderExposure >= (double)insiderLimit.maximumAllowedLimit)
                        {
                            throw new SecureException("Insider Limit Exceeded");
                        }
                    }
                }
            }

            if(limitValidation.IsDirectorRelatedGroup(application.customerGroupId) || limitValidation.CustomerIsDirector(application.customerId))
            {
                foreach (var facility in details)
                {
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var directorLimit = limitValidation.ValidateNPLByDirectors(application);
                        var directorExposure = (double)applicationAmount + directorLimit.outstandingBalance;
                        if (directorExposure >= (double)directorLimit.maximumAllowedLimit)
                        {
                            throw new SecureException("Director Limit Exceeded");
                        }
                    }
                }

            }

            if (customerOverrideRequest != null)
            {
                //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                //request.ISUSED = true;
            }
            else
            {
                foreach (var facility in details)
                {
                    if (facility != null && (facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.Renewal && facility.loanDetailReviewTypeId != (int)LoanDetailReviewTypeEnum.RenewalWithDecrease))
                    {
                        var singleObligor = limitValidation.ValidateSingleObligorLimit(application);
                        var proposedObligorLimit = singleObligor.outstandingBalance + (double)applicationAmount;
                        if (proposedObligorLimit >= (double)singleObligor.maximumAllowedLimit)
                        {
                            throw new SecureException("Single Obligor Limit Exceeded");
                        }
                    }
                }
                
            }
            
            var applications = application.LoanApplicationDetail.FirstOrDefault();
            decimal incomingAmount = details.Sum(x => x.exchangeAmount);
            if (applications != null)
            {
                var currency = context.TBL_CURRENCY.Find(applications.currencyId);
                if (currency.CURRENCYCODE.Trim() != "NGN")
                {
                    var currencyLimits = limitValidation.ValidateNPLByCurrency(application);
                    if (currencyLimits != null)
                    {
                        var proposedCurrencyLimit = currencyLimits.outstandingBalance + (double)incomingAmount;
                        if ((double)currencyLimits.maximumAllowedLimit != 0 && proposedCurrencyLimit >= (double)currencyLimits.maximumAllowedLimit)
                        {
                            throw new SecureException("Currency Limit Exceeded");
                        }
                    }
                }
            }

            if (application.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                var groupLimitsTwenty = limitValidation.ValidateNPLByGroupFirstTwenty(application);
                var proposedGroupLimit = groupLimitsTwenty.outstandingBalance + (double)incomingAmount;
                if ((double)groupLimitsTwenty.maximumAllowedLimit != 0 && proposedGroupLimit >= (double)groupLimitsTwenty.maximumAllowedLimit)
                {
                    throw new SecureException("Group Limit for the first 20 Group Customers exporsures Exceeded");
                }
            }
            
            if (application.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            {
                var groupLimitHundred = limitValidation.ValidateNPLByGroupFirstHundred(application);
                var proposedGroupLimitHundred = groupLimitHundred.outstandingBalance + (double)incomingAmount; 
                if ((double)groupLimitHundred.maximumAllowedLimit != 0 && proposedGroupLimitHundred >= (double)groupLimitHundred.maximumAllowedLimit)
                {
                    throw new SecureException("Group Limit for the first 100 Group Customers exporsures Exceeded");
                }
            }

        }

        public CurrentCustomerExposure GetTotalBankExposure()
        {
            decimal? totalBankExposure = 0;
            decimal? exposureProposedLimit = context.TBL_GLOBAL_EXPOSURE.Sum(l => l.TOTALEXPOSURE);
            //decimal? exposureProposedLimit = context.TBL_LOAN.Where(l => l.LOANSTATUSID == (short)LoanStatusEnum.Active)?.Sum(l => l.OUTSTANDINGPRINCIPAL);
            if (exposureProposedLimit.HasValue) totalBankExposure += exposureProposedLimit;

            //exposureProposedLimit = 0;
            //exposureProposedLimit = context.TBL_LOAN_REVOLVING.Where(l => l.LOANSTATUSID == (short)LoanStatusEnum.Active)?.Sum(l => l.OVERDRAFTLIMIT);
            //if (exposureProposedLimit.HasValue) totalBankExposure += exposureProposedLimit;

            exposureProposedLimit = 0;
            exposureProposedLimit = context.TBL_LOAN_CONTINGENT.Where(l => l.LOANSTATUSID == (short)LoanStatusEnum.Active).Sum(l => (decimal?)l.CONTINGENTAMOUNT) ?? 0;
            if (exposureProposedLimit.HasValue) totalBankExposure += exposureProposedLimit.Value;



            return new CurrentCustomerExposure
            {
                totalBankExposure = totalBankExposure,
                companyLimit = context.TBL_COMPANY.FirstOrDefault().COMPANYLIMIT
            };
        }

        public bool ModifyFacility(FacilityModificationViewModel model, int loanApplicationDetailId)
        {
            int saved;
            using (var trans = context.Database.BeginTransaction())
            {

                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
                var loan = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
                if (facility != null && loan != null)
                {
                    if (model.fees != null)
                    {
                        if (model.fees.Count > 0)
                        {
                            UpdateLoanDetailFees(model.fees, loanApplicationDetailId, model.createdBy);
                        }
                    }
                    var difference = model.approvedAmount - facility.APPROVEDAMOUNT;
                    ArchiveLoanApplication(facility.LOANAPPLICATIONID, (int)OperationsEnum.CreditAppraisal, 0, model.createdBy);
                    model.approvedTenor = ConvertTenorToDays(model.approvedTenor, model.tenorModeId);
                    facility.APPROVEDPRODUCTID = model.approvedProductId;
                    facility.PROPOSEDPRODUCTID = model.approvedProductId;
                    facility.APPROVEDINTERESTRATE = model.approvedInterestRate;
                    facility.PROPOSEDINTERESTRATE = model.approvedInterestRate;
                    facility.APPROVEDTENOR = model.approvedTenor;
                    facility.PROPOSEDTENOR = model.approvedTenor;
                    facility.TENORFREQUENCYTYPEID = model.tenorModeId;
                    facility.SUBSECTORID = model.subSectorId;
                    facility.LOANDETAILREVIEWTYPEID = model.loanDetailReviewTypeId;
                    facility.APPROVEDAMOUNT = model.approvedAmount;
                    facility.PROPOSEDAMOUNT = model.approvedAmount;
                    loan.APPLICATIONAMOUNT = loan.TBL_LOAN_APPLICATION_DETAIL.Sum(d => d.APPROVEDAMOUNT * (decimal)d.EXCHANGERATE);
                    loan.TOTALEXPOSUREAMOUNT += difference;
                    loan.DATETIMEUPDATED = DateTime.Now;
                    loan.LASTUPDATEDBY = model.createdBy;
                }
                saved = context.SaveChanges();
                trans.Commit();
            }
            return saved > 0;
        }

        private void ValidateFacilityModification(TBL_LOAN_APPLICATION_DETAIL facility, FacilityModificationViewModel model)
        {
            if (model.approvedAmount > facility.APPROVEDAMOUNT)
            {
                throw new ConditionNotMetException("Amount Cannot be greater than Approved amount");
            }
            if (model.approvedAmount > facility.APPROVEDAMOUNT)
            {
                throw new ConditionNotMetException("Tenor Cannot be greater than Approved Tenor");
            }
        }
        

        public bool UpdateLoanApplicationTags(LoanApplicationTagsViewModel model, int id, UserInfo user)
        {
            
            var entity = this.context.TBL_LOAN_APPLICATION.Find(id);
            var loanApplicationdetailIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == id).Select(l => l.LOANAPPLICATIONDETAILID).ToList();
            var facilityStampDutyIds = new List<int>();
            if (entity != null)
            {
                entity.ISPROJECTRELATED = model.isProjectRelated;
                entity.ISONLENDING = model.isOnLending;
                entity.ISINTERVENTIONFUNDS = model.isInterventionFunds;
                entity.WITHINSTRUCTION = model.withInstruction;
                entity.DOMICILIATIONNOTINPLACE = model.domiciliationNotInPlace;
                entity.ISAGRICRELATED = model.isAgricRelated;
                entity.ISSYNDICATED = model.isSyndicated;
                entity.IBLRENEWAL = model.iblRenewal;
                
                entity.LASTUPDATEDBY = user.createdBy;
                entity.DATETIMEUPDATED = DateTime.Now;
            }

            if (loanApplicationdetailIds.Count > 0)
            {
                foreach (var detailId in loanApplicationdetailIds)
                {
                    bool stampDutyApplicable = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == detailId).Select(l => l.STAMPDUTYAPPLICABLE == true).FirstOrDefault();
                    if (stampDutyApplicable)
                    {
                        var stampDutyId = context.TBL_FACILITY_STAMP_DUTY.Where(s => s.LOANAPPLICATIONDETAILID == detailId).FirstOrDefault().FACILITYSTAMPDUTYID;
                        facilityStampDutyIds.Add(stampDutyId);
                    }
                }
                if(facilityStampDutyIds.Count > 0)
                {
                    foreach (var dutyId in facilityStampDutyIds)
                    {
                        var facilityDuty = context.TBL_FACILITY_STAMP_DUTY.Find(dutyId);
                        if (facilityDuty != null)
                        {
                            if ( model.isOnLending == true)
                            {                         
                              facilityDuty.DELETED = true;
                            }
                            else
                            {
                                facilityDuty.DELETED = false;
                            }
                            
                        }
                        
                    }
                }
            }

            if (model.isProjectRelated == false)
            {
                var existing_contractor_tiering = context.TBL_CONTRACTOR_TIERING.Where(x => x.LOANAPPLICATIONID == id).ToList();
                if (existing_contractor_tiering != null)
                {
                    context.TBL_CONTRACTOR_TIERING.RemoveRange(existing_contractor_tiering);
                };
            }

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.staffId).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationTagChange,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LOAN_APPLICATN Loan Application tag Updated by '{auditStaff}' with Id  {user.createdBy}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public LoanApplicationTagsViewModel GetLoanApplicationTags(int id)
        {
            var entity = context.TBL_LOAN_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == id && x.DELETED == false);
            if (entity == null) return new LoanApplicationTagsViewModel();
            return new LoanApplicationTagsViewModel
            {
                isProjectRelated = entity.ISPROJECTRELATED,
                isOnLending = entity.ISONLENDING,
                isInterventionFunds = entity.ISINTERVENTIONFUNDS,
                withInstruction = entity.WITHINSTRUCTION,
                domiciliationNotInPlace = entity.DOMICILIATIONNOTINPLACE,
                isAgricRelated = entity.ISAGRICRELATED,
                isSyndicated = entity.ISSYNDICATED,
                iblRenewal = entity.IBLRENEWAL
            };
        }

        public LoanApplicationTagsLMSViewModel GetLoanApplicationTagsLMS(int id)
        {
            var entity = context.TBL_LMSR_APPLICATION.FirstOrDefault(x => x.LOANAPPLICATIONID == id && x.DELETED == false);

            return new LoanApplicationTagsLMSViewModel
            {
                isProjectRelated = entity.ISPROJECTRELATED,
                isOnLending = entity.ISONLENDING,
                isInterventionFunds = entity.ISINTERVENTIONFUNDS,
                withInstruction = entity.WITHINSTRUCTION,
                domiciliationNotInPlace = entity.DOMICILIATIONNOTINPLACE,
                isAgricRelated = entity.ISAGRICRELATED,
                isSyndicated = entity.ISSYNDICATED,
            };
        }

        public bool UpdateLoanApplicationTagsLMS(LoanApplicationTagsLMSViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LMSR_APPLICATION.Find(id);
            if (entity != null)
            {
                entity.ISPROJECTRELATED = model.isProjectRelated;
                entity.ISONLENDING = model.isOnLending;
                entity.ISINTERVENTIONFUNDS = model.isInterventionFunds;
                entity.WITHINSTRUCTION = model.withInstruction;
                entity.DOMICILIATIONNOTINPLACE = model.domiciliationNotInPlace;
                entity.ISAGRICRELATED = model.isAgricRelated;
                entity.ISSYNDICATED = model.isSyndicated;

                entity.LASTUPDATEDBY = user.createdBy;
                entity.DATETIMEUPDATED = DateTime.Now;
            }
           

            return context.SaveChanges() != 0;
        }

        public IEnumerable<RevisedProcessFlowModel> getFacilityApplicationRevisedProcessFlow()
        {
            var revisedProcessFlow = (from c in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                                      select new RevisedProcessFlowModel
                                      {
                                          flowchangeId = c.FLOWCHANGEID,
                                          placeHolder = c.PLACEHOLDER,
                                          productClassId = c.PRODUCTCLASSID,
                                          productId = c.PRODUCTID,
                                          destinationUrl = c.DESTINATIONURL,
                                          skipProcessFlowEnabled = c.ISSKIPPROCESSENABLED,
                                          operationId = c.OPERATIONID,
                                          hasOperationBasedRac = c.HASOPERATIONBASEDRAC,
                                          dateTimeCreated = c.DATETIMECREATED,
                                          createdBy = c.CREATEDBY
                                      }).ToList();

            return revisedProcessFlow;
        }

        public IEnumerable<RevisedProcessFlowModel> getFacilityApplicationRevisedProcessFlowByProductClassId(short productClassId, short productId, short productTypeId)
        {
            var productTypeFlow = (from c in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                                   where c.PRODUCTTYPEID == productTypeId && c.PRODUCTID == null && c.PRODUCTCLASSID == null
                                   select new RevisedProcessFlowModel
                                   {
                                       flowchangeId = c.FLOWCHANGEID,
                                       placeHolder = c.PLACEHOLDER,
                                       productClassId = c.PRODUCTCLASSID,
                                       productId = c.PRODUCTID,
                                       productTypeId = c.PRODUCTTYPEID,
                                       destinationUrl = c.DESTINATIONURL,
                                       skipProcessFlowEnabled = c.ISSKIPPROCESSENABLED,
                                       operationId = c.OPERATIONID,
                                       label = c.LABEL,
                                       dateTimeCreated = c.DATETIMECREATED,
                                       createdBy = c.CREATEDBY
                                   }).ToList();

            var productClassFlow = (from c in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                                    where c.PRODUCTCLASSID == productClassId && c.PRODUCTID == null && c.PRODUCTTYPEID == null
                                    select new RevisedProcessFlowModel
                                    {
                                        flowchangeId = c.FLOWCHANGEID,
                                        placeHolder = c.PLACEHOLDER,
                                        productClassId = c.PRODUCTCLASSID,
                                        productId = c.PRODUCTID,
                                        productTypeId = c.PRODUCTTYPEID,
                                        destinationUrl = c.DESTINATIONURL,
                                        skipProcessFlowEnabled = c.ISSKIPPROCESSENABLED,
                                        operationId = c.OPERATIONID,
                                        label = c.LABEL,
                                        dateTimeCreated = c.DATETIMECREATED,
                                        createdBy = c.CREATEDBY
                                    }).ToList();

            var productFlow = (from c in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                               where c.PRODUCTCLASSID == null && c.PRODUCTTYPEID == null && c.PRODUCTID == productId
                               select new RevisedProcessFlowModel
                               {
                                   flowchangeId = c.FLOWCHANGEID,
                                   placeHolder = c.PLACEHOLDER,
                                   productClassId = c.PRODUCTCLASSID,
                                   productId = c.PRODUCTID,
                                   productTypeId = c.PRODUCTTYPEID,
                                   destinationUrl = c.DESTINATIONURL,
                                   skipProcessFlowEnabled = c.ISSKIPPROCESSENABLED,
                                   operationId = c.OPERATIONID,
                                   label = c.LABEL,
                                   dateTimeCreated = c.DATETIMECREATED,
                                   createdBy = c.CREATEDBY
                               }).ToList();

            if (productClassFlow.Count() > 0) return productClassFlow;
            if (productTypeFlow.Count() > 0) return productTypeFlow;
            else return productFlow;
        }

        public RevisedProcessFlowModel getCashCollaterizedProcessFlowBy()
        {
            var cashCollaterzedFlow = (from c in context.TBL_LOAN_APPLICATN_FLOW_CHANGE
                                   where c.FLOWCHANGEID == (short)FlowChangeEnum.CASHCOLLATERIZED
                                   //where c.PLACEHOLDER.ToLower() == "cash collaterized" || c.PLACEHOLDER.ToLower() == "cash collaterised" || c.PLACEHOLDER.ToLower() == "cash-collaterized"
                                       select new RevisedProcessFlowModel
                                      {
                                       flowchangeId = c.FLOWCHANGEID,
                                       placeHolder = c.PLACEHOLDER,
                                       productClassId = c.PRODUCTCLASSID,
                                       productId = c.PRODUCTID,
                                       productTypeId = c.PRODUCTTYPEID,
                                       destinationUrl = c.DESTINATIONURL,
                                       skipProcessFlowEnabled = c.ISSKIPPROCESSENABLED,
                                       operationId = c.OPERATIONID,
                                       label = c.LABEL,
                                       dateTimeCreated = c.DATETIMECREATED,
                                       createdBy = c.CREATEDBY
                                   }).FirstOrDefault();

            return cashCollaterzedFlow;
        }

        public IEnumerable<LoanApplicationViewModel> GetFacilityByApplicationId(int loanApplicationId)
        {
            var queryOne = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                            where x.LOANAPPLICATIONID == loanApplicationId
                            select new LoanApplicationViewModel
                            {
                                productId = x.PROPOSEDPRODUCTID,
                                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PROPOSEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                facilityAmount = x.APPROVEDAMOUNT,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                currencyName = x.TBL_CURRENCY.CURRENCYNAME,
                                exchangeRate = x.EXCHANGERATE,
                                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                approvedProductId = x.APPROVEDPRODUCTID,
                                proposedProductName = x.TBL_PRODUCT.PRODUCTNAME,
                                proposedTenor = x.PROPOSEDTENOR,
                                proposedInterestRate = x.PROPOSEDINTERESTRATE,
                                proposedAmount = x.PROPOSEDAMOUNT,
                                sectorId = (short)x.TBL_SUB_SECTOR.SECTORID,
                                subSectorId = x.SUBSECTORID,
                                approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                currencyId = x.CURRENCYID,
                                repaymentScheduleId = x.REPAYMENTSCHEDULEID.Value,
                                isTakeOverApplication = x.ISTAKEOVERAPPLICATION,
                                repaymentTerm = x.REPAYMENTTERMS,
                                applicationReferenceNumber = x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                customerId = x.CUSTOMERID,
                                firstName = x.TBL_CUSTOMER.FIRSTNAME,
                                middleName = x.TBL_CUSTOMER.MIDDLENAME,
                                lastName = x.TBL_CUSTOMER.LASTNAME,
                                customerCode = x.TBL_CUSTOMER.CUSTOMERCODE,
                                proposedProductId = x.PROPOSEDPRODUCTID,
                                productClassProcessId = x.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                                productClassId = x.TBL_PRODUCT.PRODUCTCLASSID,
                                customerType = x.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                customerGroupId = x.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,//.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                customerGroupName = x.TBL_LOAN_APPLICATION.CUSTOMERGROUPID.HasValue ? x.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            }).ToList();

            var queryTwo = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                            where x.LOANREVIEWAPPLICATIONID == loanApplicationId
                            select new LoanApplicationViewModel
                            {
                                productId = x.PRODUCTID,
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                facilityAmount = x.APPROVEDAMOUNT,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                proposedProductName = x.TBL_PRODUCT.PRODUCTNAME,
                                proposedTenor = x.PROPOSEDTENOR,
                                proposedInterestRate = x.PROPOSEDINTERESTRATE,
                                proposedAmount = x.PROPOSEDAMOUNT,
                                approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                repaymentTerm = x.REPAYMENTTERMS,
                                customerId = x.CUSTOMERID,
                                firstName = x.TBL_CUSTOMER.FIRSTNAME,
                                middleName = x.TBL_CUSTOMER.MIDDLENAME,
                                lastName = x.TBL_CUSTOMER.LASTNAME,
                                customerCode = x.TBL_CUSTOMER.CUSTOMERCODE,
                                productClassId = x.TBL_PRODUCT.PRODUCTCLASSID,
                                customerType = x.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                loanApplicationDetailId = x.LOANREVIEWAPPLICATIONID,
                            }).ToList();
            var finalQuery = queryOne.Union(queryTwo);
            return finalQuery.ToList();
        }

        public LoanApplicationViewModel GetFacilityByApplicationDetailIdLos(int loanApplicationDetailId)
        {
            var queryOne = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                            where x.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                            select new LoanApplicationViewModel
                            {
                                productId = x.PROPOSEDPRODUCTID,
                                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PROPOSEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                facilityAmount = x.APPROVEDAMOUNT,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                currencyName = x.TBL_CURRENCY.CURRENCYNAME,
                                exchangeRate = x.EXCHANGERATE,
                                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                approvedProductId = x.APPROVEDPRODUCTID,
                                proposedProductName = x.TBL_PRODUCT.PRODUCTNAME,
                                proposedTenor = x.PROPOSEDTENOR,
                                proposedInterestRate = x.PROPOSEDINTERESTRATE,
                                proposedAmount = x.PROPOSEDAMOUNT,
                                sectorId = (short)x.TBL_SUB_SECTOR.SECTORID,
                                subSectorId = x.SUBSECTORID,
                                approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                currencyId = x.CURRENCYID,
                                repaymentScheduleId = x.REPAYMENTSCHEDULEID.Value,
                                isTakeOverApplication = x.ISTAKEOVERAPPLICATION,
                                repaymentTerm = x.REPAYMENTTERMS,
                                applicationReferenceNumber = x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                customerId = x.CUSTOMERID,
                                firstName = x.TBL_CUSTOMER.FIRSTNAME,
                                middleName = x.TBL_CUSTOMER.MIDDLENAME,
                                lastName = x.TBL_CUSTOMER.LASTNAME,
                                customerCode = x.TBL_CUSTOMER.CUSTOMERCODE,
                                proposedProductId = x.PROPOSEDPRODUCTID,
                                productClassProcessId = x.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                                productClassId = x.TBL_PRODUCT.PRODUCTCLASSID,
                                customerType = x.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                customerGroupId = x.TBL_LOAN_APPLICATION.CUSTOMERGROUPID,//.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                customerGroupName = x.TBL_LOAN_APPLICATION.CUSTOMERGROUPID.HasValue ? x.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            }).FirstOrDefault();

            return queryOne;
        }

        public LoanApplicationViewModel GetFacilityByApplicationDetailIdLms(int loanApplicationDetailId)
        {
            var queryTwo = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                            where x.LOANREVIEWAPPLICATIONID == loanApplicationDetailId
                            select new LoanApplicationViewModel
                            {
                                productId = x.PRODUCTID,
                                productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                facilityAmount = x.APPROVEDAMOUNT,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                proposedProductName = x.TBL_PRODUCT.PRODUCTNAME,
                                proposedTenor = x.PROPOSEDTENOR,
                                proposedInterestRate = x.PROPOSEDINTERESTRATE,
                                proposedAmount = x.PROPOSEDAMOUNT,
                                approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == x.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                repaymentTerm = x.REPAYMENTTERMS,
                                customerId = x.CUSTOMERID,
                                firstName = x.TBL_CUSTOMER.FIRSTNAME,
                                middleName = x.TBL_CUSTOMER.MIDDLENAME,
                                lastName = x.TBL_CUSTOMER.LASTNAME,
                                customerCode = x.TBL_CUSTOMER.CUSTOMERCODE,
                                productClassId = x.TBL_PRODUCT.PRODUCTCLASSID,
                                customerType = x.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                loanApplicationDetailId = x.LOANREVIEWAPPLICATIONID,
                            }).FirstOrDefault();
            
            return queryTwo;
        }


        public bool LoanApplicationFlowChange(int loanApplicationId)
        {
            var detail = context.TBL_LOAN_APPLICATION.Where(o => o.LOANAPPLICATIONID == loanApplicationId).Select(o => o).FirstOrDefault();
            var flowChange = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Where(o => o.PLACEHOLDER == "FAM").FirstOrDefault();

            if (flowChange == null) { throw new ConditionNotMetException("Workflow Change to reroute not found. Contact admin." ); }

            detail.PRODUCT_CLASS_PROCESSID = 1;
            detail.FLOWCHANGEID = flowChange?.FLOWCHANGEID;
            detail.OPERATIONID = flowChange.OPERATIONID ;

            return context.SaveChanges() > 0;
        }

        public bool DeleteLoanApplicationThatFailedRAC(int loanApplicationDetailId, int deletedBy)
        {
            var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == loanApplicationDetailId).Select(o => o).FirstOrDefault();
            if (detail == null)
            {
                detail.DELETED = true;
                detail.DATETIMEDELETED = genSetup.GetApplicationDate();
                detail.DELETEDBY = deletedBy;
                return true;
            }
            context.TBL_LOAN_APPLICATION_DETAIL.Remove(detail);
            //detail.DELETED = true;
            //detail.DATETIMEDELETED = genSetup.GetApplicationDate();
            //detail.DELETEDBY = deletedBy;
            //return true;
            return context.SaveChanges() > 0;
        }

        public LoanApplicationFlowChangeViewModel GetLoanAppicationFlowChange(int id)
        {
            var entity = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.FirstOrDefault(x => x.FLOWCHANGEID == id && x.DELETED == false);

            return new LoanApplicationFlowChangeViewModel
            {
                FlowChangeId = entity.FLOWCHANGEID,
                label = entity.LABEL,
                placeHolder = entity.PLACEHOLDER,
                skipflow = entity.ISSKIPPROCESSENABLED,
                productClassId = entity.PRODUCTCLASSID,
                productId = entity.PRODUCTID,
                operationId = entity.OPERATIONID,
                //interestPayment = entity.INTERESTPAYMENT,
                destinationUrl = entity.DESTINATIONURL,
                productTypeId = entity.PRODUCTTYPEID,
                documentOperation= 0, //entity.DOCUMENTOPERATION,
            };
        }

        public IEnumerable<LoanApplicationFlowChangeViewModel> GetLoanApplicationFlowChange()
        {
            return context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Where(x => x.DELETED == false)
                .Select(x => new LoanApplicationFlowChangeViewModel
                {
                    FlowChangeId = x.FLOWCHANGEID,
                    documentOperation= 0, //x.DOCUMENTOPERATION,
                    label = x.LABEL,
                    placeHolder=x.PLACEHOLDER,
                    //interestPayment = x.INTERESTPAYMENT,
                    operationId=x.OPERATIONID,
                    destinationUrl=x.DESTINATIONURL == null ? "N/A" : x.DESTINATIONURL,
                    productTypeId=x.PRODUCTTYPEID,
                    productType = context.TBL_PRODUCT_TYPE.Where(pt => pt.PRODUCTTYPEID == x.PRODUCTTYPEID).Select( s => s.PRODUCTTYPENAME).FirstOrDefault() == null ? "N/A" : context.TBL_PRODUCT_TYPE.Where(pt => pt.PRODUCTTYPEID == x.PRODUCTTYPEID).Select(s => s.PRODUCTTYPENAME).FirstOrDefault(),
                    productClass = context.TBL_PRODUCT_CLASS.Where(pc => pc.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(s => s.PRODUCTCLASSNAME).FirstOrDefault() == null ? "N/A" : context.TBL_PRODUCT_CLASS.Where(pc => pc.PRODUCTCLASSID == x.PRODUCTCLASSID).Select(s => s.PRODUCTCLASSNAME).FirstOrDefault(),
                    productClassId =x.PRODUCTCLASSID,
                    skipflow=x.ISSKIPPROCESSENABLED,
                    skipFlo = x.ISSKIPPROCESSENABLED == true ? "YES" : "NO",
                    operation =context.TBL_OPERATIONS.Where(o=>o.OPERATIONID==o.OPERATIONID).Select(s=>s.OPERATIONNAME).FirstOrDefault(),
                    
                })
                .ToList();
        }

        public bool AddLoanApplicationFlowChange(LoanApplicationFlowChangeViewModel model)
        {
            var entity = new TBL_LOAN_APPLICATN_FLOW_CHANGE
            {
                //DOCUMENTOPERATION = model.documentOperation,
                LABEL = model.label,
                PLACEHOLDER = model.placeHolder,
                ISSKIPPROCESSENABLED = model.skipflow,
                PRODUCTCLASSID = model.productClassId,
                OPERATIONID = model.operationId,
                DESTINATIONURL = model.destinationUrl,
                PRODUCTTYPEID = model.productTypeId,
                //INTERESTPAYMENT = (int)model.interestPayment,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = model.createdBy,
                DELETED = false

            };
            context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationFlowChangeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_LOAN_APPLICATN_FLOW_CHANGE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLoanApplicationFlowChange(LoanApplicationFlowChangeViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(id);
            entity.LABEL = model.label;
           // entity.PLACEHOLDER = model.placeHolder;
            entity.ISSKIPPROCESSENABLED = model.skipflow;
            entity.PRODUCTCLASSID = model.productClassId;
            entity.OPERATIONID = model.operationId;
            entity.DESTINATIONURL = model.destinationUrl;
            //entity.INTERESTPAYMENT = (int)model.interestPayment;
            //entity.DOCUMENTOPERATION = model.documentOperation;
            entity.PRODUCTTYPEID = model.productTypeId;
            var auditStaff = context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE).FirstOrDefault();
            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationFlowChangeUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LOAN_APPLICATN_FLOW_CHANGE '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.FLOWCHANGEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;


        }
       
        public bool DeleteLoanApplicationFlowChange(int id, UserInfo user)
        {
            var entity = this.context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Find(id);
            entity.DELETED = true;
            entity.DATETIMEDELETED = genSetup.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationFlowChangeDeleted, //still missing its value
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LOAN_APPLICATN_FLOW_CHANGE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.FLOWCHANGEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        // for lien operations
        public LoanApplicationLienViewModel GetApplicationDetailLien(int id)
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    where x.APPLICATIONDETAILLIENID == id
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED
                    }).FirstOrDefault();
        }

        public IEnumerable<LoanApplicationLienViewModel> GetLienByCollateralId(int collateralId)
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    where x.APPLICATIONDETAILLIENID == collateralId
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED
                    }).ToList();
        }

        public IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLien()
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED
                    }).ToList();
        }


        public IEnumerable<LoanApplicationLienViewModel> GetLienByApplicationDetailId(int applicationDetailId)
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    where x.APPLICATIONDETAILID == applicationDetailId && x.DELETED == false
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED,
                        collateralId = x.COLLATERALCUSTOMERID
                    }).ToList();
        }

        public IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLienByIsReleased(bool isReleased)
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    where x.ISRELEASED == isReleased && x.DELETED == false
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED
                    }).ToList();
        }

        public IEnumerable<LoanApplicationLienViewModel> GetApplicationDetailLienByAccountNo(string accountNo)
        {
            return (from x in context.TBL_APPLICATIONDETAIL_LIEN
                    where x.ACCOUNTNO.Trim() == accountNo.Trim() && x.DELETED == false
                    select new LoanApplicationLienViewModel
                    {
                        applicationDetailLienId = x.APPLICATIONDETAILLIENID,
                        applicationDetailId = x.APPLICATIONDETAILID,
                        amount = x.AMOUNT,
                        accountNo = x.ACCOUNTNO,
                        isReleased = x.ISRELEASED
                    }).ToList();
        }

        public bool AddLoanApplicationDetailLien(LoanApplicationLienViewModel model)
        {
               var entity = new TBL_APPLICATIONDETAIL_LIEN
                {
                    APPLICATIONDETAILID = model.applicationDetailId,
                    AMOUNT = model.amount,
                    ACCOUNTNO = model.accountNo,
                    ISRELEASED = false,
                    COLLATERALCUSTOMERID = model.collateralId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate()
                };

                var id = context.TBL_APPLICATIONDETAIL_LIEN.Add(entity);

                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.auditTrail.AddAuditTrail(new TBL_AUDIT
                {
                    //AUDITTYPEID = (short)AuditTypeEnum.LienProposed,
                    AUDITTYPEID = (short)AuditTypeEnum.LienPlaced,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_APPLICATIONDETAIL_LIEN '{entity.ToString()}' created by {auditStaff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                  
                });
                // Audit Section end ------------------------

               return context.SaveChanges() != 0;
        }
    
        public bool UpdateLoanApplicationDetailLien(LoanApplicationLienViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_APPLICATIONDETAIL_LIEN.Find(id);
            if (entity != null)
            {
                entity.APPLICATIONDETAILID = model.applicationDetailId;
                entity.AMOUNT = model.amount;
                entity.ACCOUNTNO = model.accountNo;
                entity.ISRELEASED = model.isReleased;

                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.auditTrail.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ProjectSiteReportUpdated,// lien edited
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"TBL_APPLICATIONDETAIL_LIEN '{entity.ToString()}' was updated by {auditStaff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    TARGETID = entity.APPLICATIONDETAILLIENID
                });
                // Audit Section end ------------------------
            }

            return context.SaveChanges() != 0;
        }

        public bool DeleteLoanApplicationDetailLien(int id, UserInfo user)
        {
            var entity = this.context.TBL_APPLICATIONDETAIL_LIEN.Find(id);
            entity.DELETED = true;
            entity.ISRELEASED = true;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LienReleased,
                //AUDITTYPEID = (short)AuditTypeEnum.LienUnproposed,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_APPLICATIONDETAIL_LIEN '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.APPLICATIONDETAILLIENID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanDetailReviewTypeViewModel> GetAllLoanDetailReviewTypes()
        {
            var reviewTypes = (from x in context.TBL_LOAN_DETAIL_REVIEW_TYPE
                                select new LoanDetailReviewTypeViewModel
                                {
                                    loanDetailReviewTypeId = x.LOANDETAILREVIEWTYPEID,
                                    loanDetailReviewTypeName = x.LOANDETAILREVIEWTYPENAME,
                                }).ToList();

            return reviewTypes;
        }

        public IEnumerable<PropertyTypeViewModel> GetAllPropertyTypes()
        {
            var propertyTypes = (from x in context.TBL_PROPERTY_TYPE
                               select new PropertyTypeViewModel
                               {
                                   propertyTypeId = x.PROPERTYTYPEID,
                                   propertyTypeName = x.PROPERTYNAME,
                               }).ToList();

            return propertyTypes;
        }

        public IEnumerable<ApprovedTradeCycleViewModel> GetAllApprovedTradeCycles()
        {
            var approvedCycles = (from x in context.TBL_APPROVED_TRADE_CYCLE
                                  select new ApprovedTradeCycleViewModel
                               {
                                   approvedTradeCycleId = x.APPROVEDTRADECYCLEID,
                                   approvedTradeCycleDays = x.APPROVEDTRADECYCLEDAYS,
                               }).ToList();

            return approvedCycles;
        }

        public IEnumerable<RetailRecoveryCustomerTransactionsViewModels> GetRetailRecoveryReporting(DateTime startDate, DateTime endDate, int accreditedConsultantId, string customer)
        {
            IEnumerable<RetailRecoveryCustomerTransactionsViewModels> records = null; 
            List<RetailRecoveryCustomerTransactionsViewModels> dataTermLoan = null;
            List<RetailRecoveryCustomerTransactionsViewModels> dataRevolvingLoan = null;

            List<RetailRecoveryCustomerTransactionsViewModels> dataDigitalExposureLoan = null;
            List<RetailRecoveryCustomerTransactionsViewModels> dataExposureLoan = null;

            if (customer != null)
            {
                 dataTermLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                    join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                    join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                    join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                    join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                    join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                    join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                    join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                    join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                    join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                    where
                                    lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                    && lr.ISFULLYRECOVERED == false
                                    && pr.EXCLUDEFROMLITIGATION == false
                                    && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                    && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                    && lr.SOURCE.ToLower() == "retail"
                                    && lr.CUSTOMERID == customer

                                    select new RetailRecoveryCustomerTransactionsViewModels
                                    {
                                        totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                        loanApplicationId = lp.LOANAPPLICATIONID,
                                        currencyId = ld.CURRENCYID,
                                        accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                        accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                        accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                        expCompletionDate = lr.EXPCOMPLETIONDATE,
                                        creditAppraisalOperationId = lp.OPERATIONID,
                                        loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                        loanId = ln.TERMLOANID,
                                        customerId = ln.CUSTOMERID,
                                        productId = ln.PRODUCTID,
                                        productTypeId = pr.PRODUCTTYPEID,
                                        casaAccountId = ln.CASAACCOUNTID,
                                        casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                        casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                        branchId = ln.BRANCHID,
                                        totalExposure = lp.TOTALEXPOSUREAMOUNT,
                                        totalAmountRecovery = (decimal?)lr.TOTALAMOUNTRECOVERY ?? 0,
                                        loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                        applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                        principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                        pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                        interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                        interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                        principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                        interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                        misCode = ln.MISCODE,
                                        teamMiscode = ln.TEAMMISCODE,
                                        interestRate = ln.INTERESTRATE,
                                        effectiveDate = ln.EFFECTIVEDATE,
                                        maturityDate = ln.MATURITYDATE,
                                        bookingDate = ln.BOOKINGDATE,
                                        principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                        principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                        operationId = ln.OPERATIONID,
                                        loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                        equityContribution = ln.EQUITYCONTRIBUTION,
                                        subSectorId = ln.SUBSECTORID,
                                        subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                        sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                        profileLoan = ln.PROFILELOAN,
                                        customerCode = cu.CUSTOMERCODE,
                                        loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                        customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                        branchName = br.BRANCHNAME,
                                        relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                        relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                        productName = pr.PRODUCTNAME,
                                        approvedAmount = ld.APPROVEDAMOUNT,
                                        creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                    }).ToList();

                 dataExposureLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                where
                                lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                && lr.ISFULLYRECOVERED == false
                                && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && lr.SOURCE.ToLower() == "retail"
                                && lr.CUSTOMERID == customer

                                select new RetailRecoveryCustomerTransactionsViewModels
                                {
                                    accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                    accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                    accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                    expCompletionDate = lr.EXPCOMPLETIONDATE,
                                    creditAppraisalOperationId = (int)lr.OPERATIONID,
                                    loanId = ln.ID,
                                    casaAccount = ln.ACCOUNTNUMBER,
                                    casaAccountName = "CURRENT ACCOUNT",
                                    totalExposure = (decimal)ln.TOTALEXPOSURE,
                                    totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                    totalUnsettledAmount = ln.TOTALUNSETTLEDAMOUNT,
                                    loanReferenceNumber = ln.REFERENCENUMBER,
                                    applicationReferenceNumber = ln.REFERENCENUMBER,
                                    misCode = ln.ACCOUNTOFFICERCODE,
                                    teamMiscode = ln.TEAMCODE,
                                    principalAmount = (decimal)ln.PRINCIPALOUTSTANDINGBALLCY,
                                    operationId = lr.OPERATIONID,
                                    sectorName = ln.CBNSECTOR,
                                    customerCode = ln.CUSTOMERID,
                                    branchCode = ln.BRANCHCODE,
                                    customerName = ln.CUSTOMERNAME,
                                    productCode = ln.PRODUCTCODE,
                                    branchName = ln.BRANCHNAME,
                                    relationshipOfficerName = ln.ACCOUNTOFFICERNAME,
                                    relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                    productName = ln.PRODUCTNAME,
                                    approvedAmount = (decimal)ln.LOANAMOUNYLCY,
                                    creatorName = ln.ACCOUNTOFFICERNAME,
                                }).ToList();

                foreach (var xx in dataExposureLoan)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERID).FirstOrDefault();
                    xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                    xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
                }

                 dataDigitalExposureLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                        join ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                        where
                                        lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                        && lr.ISFULLYRECOVERED == false
                                        && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                        && lr.SOURCE.ToLower() == "retail"
                                        && lr.CUSTOMERID == customer

                                        select new RetailRecoveryCustomerTransactionsViewModels
                                        {
                                            accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                            accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                            accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                            expCompletionDate = lr.EXPCOMPLETIONDATE,
                                            creditAppraisalOperationId = (int)lr.OPERATIONID,
                                            loanId = ln.ID,
                                            casaAccount = ln.ACCOUNTNUMBER,
                                            casaAccountName = "CURRENT ACCOUNT",
                                            totalExposure = (decimal)ln.TOTALEXPOSURE,
                                            totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                            totalUnsettledAmount = ln.TOTALUNSETTLEDAMOUNT,
                                            loanReferenceNumber = ln.REFERENCENUMBER,
                                            applicationReferenceNumber = ln.REFERENCENUMBER,
                                            misCode = ln.ACCOUNTOFFICERCODE,
                                            teamMiscode = ln.TEAMCODE,
                                            principalAmount = (decimal)ln.PRINCIPALOUTSTANDINGBALLCY,
                                            operationId = lr.OPERATIONID,
                                            sectorName = ln.CBNSECTOR,
                                            customerCode = ln.CUSTOMERID,
                                            productCode = ln.PRODUCTCODE,
                                            customerName = ln.CUSTOMERNAME,
                                            branchName = ln.BRANCHNAME,
                                            relationshipOfficerName = ln.ACCOUNTOFFICERNAME,
                                            relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                            productName = ln.PRODUCTNAME,
                                            approvedAmount = (decimal)ln.LOANAMOUNYLCY,
                                            creatorName = ln.ACCOUNTOFFICERNAME,
                                            branchCode = ln.BRANCHCODE,
                                        }).ToList();

                foreach (var xx in dataDigitalExposureLoan)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERID).FirstOrDefault();
                    xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                    xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
                }

                dataRevolvingLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where
                                         lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                         && lr.ISFULLYRECOVERED == false
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && lr.SOURCE.ToLower() == "retail"
                                         && lr.CUSTOMERID == customer

                                         select new RetailRecoveryCustomerTransactionsViewModels
                                         {
                                             totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                             loanApplicationId = lp.LOANAPPLICATIONID,
                                             totalAmountRecovery = (decimal?)lr.TOTALAMOUNTRECOVERY ?? 0,
                                             totalExposure = lp.TOTALEXPOSUREAMOUNT,
                                             currencyId = ld.CURRENCYID,
                                             accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                             accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                             accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                             expCompletionDate = lr.EXPCOMPLETIONDATE,
                                             creditAppraisalOperationId = lp.OPERATIONID,
                                             loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                             loanId = ln.REVOLVINGLOANID,
                                             customerId = ln.CUSTOMERID,
                                             productId = ln.PRODUCTID,
                                             productTypeId = pr.PRODUCTTYPEID,
                                             casaAccountId = ln.CASAACCOUNTID,
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             branchId = ln.BRANCHID,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                             misCode = ln.MISCODE,
                                             teamMiscode = ln.TEAMMISCODE,
                                             interestRate = ln.INTERESTRATE,
                                             effectiveDate = ln.EFFECTIVEDATE,
                                             maturityDate = ln.MATURITYDATE,
                                             bookingDate = ln.BOOKINGDATE,
                                             operationId = ln.OPERATIONID,
                                             loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                             subSectorId = ln.SUBSECTORID,
                                             subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                             sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                             customerCode = cu.CUSTOMERCODE,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             branchName = br.BRANCHNAME,
                                             relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                             relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                             productName = pr.PRODUCTNAME,
                                             approvedAmount = ld.APPROVEDAMOUNT,
                                             creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                         }).ToList();
            }
            else {

                dataExposureLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                    join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                    where
                                    lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                    && lr.ISFULLYRECOVERED == false
                                    && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                    && lr.SOURCE.ToLower() == "retail"

                                    select new RetailRecoveryCustomerTransactionsViewModels
                                    {
                                        accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                        accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                        accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                        expCompletionDate = lr.EXPCOMPLETIONDATE,
                                        creditAppraisalOperationId = (int)lr.OPERATIONID,
                                        loanId = ln.ID,
                                        casaAccount = ln.ACCOUNTNUMBER,
                                        casaAccountName = "CURRENT ACCOUNT",
                                        totalExposure = (decimal)ln.TOTALEXPOSURE,
                                        totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                        loanReferenceNumber = ln.REFERENCENUMBER,
                                        applicationReferenceNumber = ln.REFERENCENUMBER,
                                        totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                        misCode = ln.ACCOUNTOFFICERCODE,
                                        teamMiscode = ln.TEAMCODE,
                                        principalAmount = (decimal)ln.PRINCIPALOUTSTANDINGBALLCY,
                                        operationId = lr.OPERATIONID,
                                        sectorName = ln.CBNSECTOR,
                                        customerCode = ln.CUSTOMERID,
                                        branchCode = ln.BRANCHCODE,
                                        customerName = ln.CUSTOMERNAME,
                                        productCode = ln.PRODUCTCODE,
                                        branchName = ln.BRANCHNAME,
                                        relationshipOfficerName = ln.ACCOUNTOFFICERNAME,
                                        relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                        productName = ln.PRODUCTNAME,
                                        approvedAmount = (decimal)ln.LOANAMOUNYLCY,
                                        creatorName = ln.ACCOUNTOFFICERNAME,
                                    }).ToList();

                foreach (var xx in dataExposureLoan)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERID).FirstOrDefault();
                    xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                    xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
                }

                dataDigitalExposureLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                           join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                           where
                                           lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                           && lr.ISFULLYRECOVERED == false
                                           && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                           && lr.SOURCE.ToLower() == "retail"

                                           select new RetailRecoveryCustomerTransactionsViewModels
                                           {
                                               accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                               accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                               accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                               expCompletionDate = lr.EXPCOMPLETIONDATE,
                                               creditAppraisalOperationId = (int)lr.OPERATIONID,
                                               loanId = ln.ID,
                                               casaAccount = ln.ACCOUNTNUMBER,
                                               casaAccountName = "CURRENT ACCOUNT",
                                               totalExposure = (decimal)ln.TOTALEXPOSURE,
                                               totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                               loanReferenceNumber = ln.REFERENCENUMBER,
                                               applicationReferenceNumber = ln.REFERENCENUMBER,
                                               totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                               misCode = ln.ACCOUNTOFFICERCODE,
                                               teamMiscode = ln.TEAMCODE,
                                               principalAmount = (decimal)ln.PRINCIPALOUTSTANDINGBALLCY,
                                               operationId = lr.OPERATIONID,
                                               sectorName = ln.CBNSECTOR,
                                               customerCode = ln.CUSTOMERID,
                                               productCode = ln.PRODUCTCODE,
                                               customerName = ln.CUSTOMERNAME,
                                               branchName = ln.BRANCHNAME,
                                               relationshipOfficerName = ln.ACCOUNTOFFICERNAME,
                                               relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                               productName = ln.PRODUCTNAME,
                                               approvedAmount = (decimal)ln.LOANAMOUNYLCY,
                                               creatorName = ln.ACCOUNTOFFICERNAME,
                                               branchCode = ln.BRANCHCODE,
                                           }).ToList();

                foreach (var xx in dataDigitalExposureLoan)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERID).FirstOrDefault();
                    xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                    xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
                }

                dataTermLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                where
                                lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                && lr.ISFULLYRECOVERED == false
                                && pr.EXCLUDEFROMLITIGATION == false
                                && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && lr.SOURCE.ToLower() == "retail"

                                select new RetailRecoveryCustomerTransactionsViewModels
                                {
                                    loanApplicationId = lp.LOANAPPLICATIONID,
                                    currencyId = ld.CURRENCYID,
                                    accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                    accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                    accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                    expCompletionDate = lr.EXPCOMPLETIONDATE,
                                    creditAppraisalOperationId = lp.OPERATIONID,
                                    loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                    loanId = ln.TERMLOANID,
                                    customerId = ln.CUSTOMERID,
                                    productId = ln.PRODUCTID,
                                    productTypeId = pr.PRODUCTTYPEID,
                                    casaAccountId = ln.CASAACCOUNTID,
                                    casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                    casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                    branchId = ln.BRANCHID,
                                    totalExposure = lp.TOTALEXPOSUREAMOUNT,
                                    totalAmountRecovery = (decimal?)lr.TOTALAMOUNTRECOVERY ?? 0,
                                    totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                    loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                    applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                    principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                    pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                    interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                    interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                    principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                    interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                    misCode = ln.MISCODE,
                                    teamMiscode = ln.TEAMMISCODE,
                                    interestRate = ln.INTERESTRATE,
                                    effectiveDate = ln.EFFECTIVEDATE,
                                    maturityDate = ln.MATURITYDATE,
                                    bookingDate = ln.BOOKINGDATE,
                                    principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                    principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                    operationId = ln.OPERATIONID,
                                    loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                    equityContribution = ln.EQUITYCONTRIBUTION,
                                    subSectorId = ln.SUBSECTORID,
                                    subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                    sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                    profileLoan = ln.PROFILELOAN,
                                    customerCode = cu.CUSTOMERCODE,
                                    loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                    customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                    branchName = br.BRANCHNAME,
                                    relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                    relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                    productName = pr.PRODUCTNAME,
                                    approvedAmount = ld.APPROVEDAMOUNT,
                                    creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                }).ToList();

                dataRevolvingLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                     join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                     join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                     join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                     join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                     join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                     join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                     join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                     join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                     join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                     where
                                     lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                     && lr.ISFULLYRECOVERED == false
                                     && pr.EXCLUDEFROMLITIGATION == false
                                     && lr.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                     && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                     && lr.SOURCE.ToLower() == "retail"

                                     select new RetailRecoveryCustomerTransactionsViewModels
                                     {
                                         loanApplicationId = lp.LOANAPPLICATIONID,
                                         totalAmountRecovery = (decimal?)lr.TOTALAMOUNTRECOVERY ?? 0,
                                         totalExposure = lp.TOTALEXPOSUREAMOUNT,
                                         totalUnsettledAmount = lr.TOTALAMOUNTRECOVERY,
                                         currencyId = ld.CURRENCYID,
                                         accreditedConsultantName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.NAME).FirstOrDefault(),
                                         accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == lr.ACCREDITEDCONSULTANT).Select(x => x.FIRMNAME).FirstOrDefault(),
                                         accreditedConsultantId = lr.ACCREDITEDCONSULTANT,
                                         expCompletionDate = lr.EXPCOMPLETIONDATE,
                                         creditAppraisalOperationId = lp.OPERATIONID,
                                         loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                         loanId = ln.REVOLVINGLOANID,
                                         customerId = ln.CUSTOMERID,
                                         productId = ln.PRODUCTID,
                                         productTypeId = pr.PRODUCTTYPEID,
                                         casaAccountId = ln.CASAACCOUNTID,
                                         casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                         branchId = ln.BRANCHID,
                                         loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                         misCode = ln.MISCODE,
                                         teamMiscode = ln.TEAMMISCODE,
                                         interestRate = ln.INTERESTRATE,
                                         effectiveDate = ln.EFFECTIVEDATE,
                                         maturityDate = ln.MATURITYDATE,
                                         bookingDate = ln.BOOKINGDATE,
                                         operationId = ln.OPERATIONID,
                                         loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                         subSectorId = ln.SUBSECTORID,
                                         subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                         sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                         customerCode = cu.CUSTOMERCODE,
                                         loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                         customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                         branchName = br.BRANCHNAME,
                                         relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                         relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                         productName = pr.PRODUCTNAME,
                                         approvedAmount = ld.APPROVEDAMOUNT,
                                         creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                     }).ToList();
            }

            var termLoanData = dataTermLoan.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.customerId);
            var revolvingLoanData = dataRevolvingLoan.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.customerId);
            var unionRecord = termLoanData.Union(revolvingLoanData).Union(dataExposureLoan).Union(dataDigitalExposureLoan);
            var data = unionRecord.ToList();

            foreach (var record in data)
            {
               records = (from a in context.TBL_FINANCE_TRANSACTION
                             where 
                             a.SOURCEREFERENCENUMBER == record.loanReferenceNumber 
                             && (DbFunctions.TruncateTime(a.APPROVEDDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.APPROVEDDATE) <= DbFunctions.TruncateTime(endDate))
                             select new RetailRecoveryCustomerTransactionsViewModels
                             {
                                 totalUnsettledAmount = record.totalUnsettledAmount,
                                 totalExposure = record.totalExposure,
                                 totalAmountRecovery = record.totalAmountRecovery,
                                 startDate = startDate,
                                 endDate = endDate,
                                 productName = record.productName,
                                 accountNumber = record.casaAccount,
                                 productAccountName = record.casaAccountName,
                                 accreditedConsultantName = record.accreditedConsultantName,
                                 accreditedConsultantCompany = record.accreditedConsultantCompany,
                                 expCompletionDate = record.expCompletionDate,
                                 loanReferenceNumber = record.loanReferenceNumber,
                                 applicationReferenceNumber = record.applicationReferenceNumber,
                                 customerName = record.customerName,
                                 debitAmount = a.DEBITAMOUNT,
                                 creditAmount = a.CREDITAMOUNT,
                                 valueDate = a.VALUEDATE,
                                 postedDate = a.POSTEDDATE,
                                 description = a.DESCRIPTION,
                                 loanReference = record.loanReferenceNumber
                             }).ToList();

            }
            return records;
        }


        public bool AddApprovalFromSubsidiary(HeadOfficeFacilityApprovalViewModel model)
        {
            try {
                bool response = false;

                if (model != null)
                {
                    var inputRecords = new TBL_SUB_BASICTRANSACTION()
                    {
                        LOANAPPLICATIONID = model.loanApplicationId,
                        LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                        APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                        RELATEDREFERENCENUMBER = model.relatedReferenceNumber,
                        SUBSIDIARYID = model.subsidiaryId,
                        CUSTOMERID = model.customerId,
                        CUSTOMERGLOBALID = model.customerGlobalId,
                        APPLICATIONDATE = model.applicationDate,
                        INTERESTRATE = model.interestRate,
                        APPLICATIONTENOR = model.applicationTenor,
                        APPROVALSTATUSID = model.approvalStatusId,
                        APPROVALLEVELID = model.approvalLevelId,
                        APPROVALLEVELGLOBALCODE = model.approvalLevelGlobalCode,
                        TOSTAFFID = model.toStaffId,
                        APPLICATIONSTATUSID = model.applicationStatusId,
                        OPERATIONNAME = model.operationName,
                        PRODUCTCLASSNAME = model.productClassName,
                        PRODUCTNAME = model.productName,
                        PRODUCT_CLASS_PROCESS = model.productClassProcess,
                        LOANAPPLICATIONTYPENAME = model.loanApplicationTypeName,
                        FIRSTNAME = model.firstName,
                        MIDDLENAME = model.middleName,
                        LASTNAME = model.lastName,
                        SYSTEMARRIVALDATETIME = model.systemArrivalDateTime,
                        BUSINESSUNITSHORTCODE = model.businessUnitShortCode,
                        APPLICATIONAMOUNT = model.applicationAmount,
                        TOTALEXPOSUREAMOUNT = model.totalExposureAmount,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = model.dateTimeCreated,
                        LASTUPDATEDBY = model.loanApplicationId,
                        DATETIMEUPDATED = model.dateTimeUpdated,
                        DELETED = model.deleted,
                        DELETEDBY = model.deletedBy,
                        DATETIMEDELETED = model.dateTimeDeleted,
                        SYSTEMDATETIME = model.systemDateTime,
                        CREATEDBYNAME = model.createdByName,
                        COUNTRYCODE = model.countryCode,
                        STAFFROLECODE = model.staffRoleCode,
                        TARGETID = model.targetId,
                        OPERATIONID = model.operationId,
                        ACTEDON = model.actedOn
                    };

                    context.TBL_SUB_BASICTRANSACTION.Add(inputRecords);
                    response = context.SaveChanges() > 0;
                }

                return response;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
       

        public bool AddLMSApprovalFromSubsidiary(HeadOfficeFacilityApprovalViewModel model)
        {
            try {
                bool response = false;

                if (model != null)
                {
                    var inputRecords = new TBL_LMS_SUB_BASICTRANSACTION()
                    {
                        LOANAPPLICATIONID = model.loanApplicationId,
                        LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                        APPLICATIONREFERENCENUMBER = model.applicationReferenceNumber,
                        RELATEDREFERENCENUMBER = model.relatedReferenceNumber,
                        SUBSIDIARYID = model.subsidiaryId,
                        CUSTOMERID = model.customerId,
                        CUSTOMERGLOBALID = model.customerGlobalId,
                        APPLICATIONDATE = model.applicationDate,
                        INTERESTRATE = model.interestRate,
                        APPLICATIONTENOR = model.applicationTenor,
                        APPROVALSTATUSID = model.approvalStatusId,
                        APPROVALLEVELID = model.approvalLevelId,
                        APPROVALLEVELGLOBALCODE = model.approvalLevelGlobalCode,
                        TOSTAFFID = model.toStaffId,
                        APPLICATIONSTATUSID = model.applicationStatusId,
                        OPERATIONNAME = model.operationName,
                        PRODUCTCLASSNAME = model.productClassName,
                        PRODUCTNAME = model.productName,
                        PRODUCT_CLASS_PROCESS = model.productClassProcess,
                        LOANAPPLICATIONTYPENAME = model.loanApplicationTypeName,
                        FIRSTNAME = model.firstName,
                        MIDDLENAME = model.middleName,
                        LASTNAME = model.lastName,
                        SYSTEMARRIVALDATETIME = model.systemArrivalDateTime,
                        BUSINESSUNITSHORTCODE = model.businessUnitShortCode,
                        APPLICATIONAMOUNT = model.applicationAmount,
                        TOTALEXPOSUREAMOUNT = model.totalExposureAmount,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = model.dateTimeCreated,
                        LASTUPDATEDBY = model.loanApplicationId,
                        DATETIMEUPDATED = model.dateTimeUpdated,
                        DELETED = model.deleted,
                        DELETEDBY = model.deletedBy,
                        DATETIMEDELETED = model.dateTimeDeleted,
                        SYSTEMDATETIME = model.systemDateTime,
                        CREATEDBYNAME = model.createdByName,
                        COUNTRYCODE = model.countryCode,
                        STAFFROLECODE = model.staffRoleCode,
                        TARGETID = model.targetId,
                        OPERATIONID = model.operationId,
                        ACTEDON = model.actedOn
                    };

                    context.TBL_LMS_SUB_BASICTRANSACTION.Add(inputRecords);
                    response = context.SaveChanges() > 0;
                }

                return response;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        
    }
}
