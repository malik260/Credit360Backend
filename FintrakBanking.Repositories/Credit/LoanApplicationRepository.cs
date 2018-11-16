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
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
using System.Configuration;
using System.Data.Entity;
using FinTrakBanking.ThirdPartyIntegration.CustomerInfo;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.ViewModels.Customer;

namespace FintrakBanking.Repositories.Credit
{
    public partial class LoanApplicationRepository : ILoanApplicationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workflow;
        private ICasaRepository casa;
        private ICreditLimitValidationsRepository creditLimitValidationsRepository;
        private ICustomerCollateralRepository collateral;
        private IFinanceTransactionRepository fina;
        private CustomerDetails _customerIntegration;

        private IApprovalLevelStaffRepository approvalLevel;

        public int response { get; set; }
        public bool isGroupLoan { get; set; }
        public TBL_LOAN_APPLICATION loanData { get; set; }

        public LoanApplicationRepository(
            IAuditTrailRepository _auditTrail,
            ICasaRepository _casa,
            ICustomerCollateralRepository _collateral,
            IGeneralSetupRepository _genSetup,
            FinTrakBankingContext _context,
            CustomerDetails _customerIntegration,
            IApprovalLevelStaffRepository _approvallevel,
            IWorkflow _workflow,
            IFinanceTransactionRepository fina,
            ICreditLimitValidationsRepository _creditLimitValidationsRepository
            )
        {
            this.collateral = _collateral;
            this.fina = fina;
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.casa = _casa;
            this.collateral = _collateral;
            approvalLevel = _approvallevel;
            workflow = _workflow;
            this.creditLimitValidationsRepository = _creditLimitValidationsRepository;
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
                            casaAccountId = a.CASAACCOUNTID,
                            requireCollateral = a.REQUIRECOLLATERAL,
                            interestRate = a.INTERESTRATE,
                            loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                            loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            customerGroupId = a.CUSTOMERGROUPID,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            isCollateralBacked = a.REQUIRECOLLATERAL,
                            tenor = a.APPLICATIONTENOR,
                            productClassId = a.PRODUCTCLASSID,
                            loanApplicationId = a.LOANAPPLICATIONID,


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
                                    purchaseOrderNumber = i.PURCHASEORDERNUMBER
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
                            createdBy = a.CREATEDBY,
                            applicationDate = a.APPLICATIONDATE,
                            applicationTenor = a.APPLICATIONTENOR,
                            applicationAmount = a.APPLICATIONAMOUNT,
                            dateTimeCreated = a.DATETIMECREATED,
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
                if (casaAcct != 0 || casaAcct != null)
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
                                                         invoiceCurrencyCode = i.TBL_CURRENCY.CURRENCYCODE,
                                                         approvaStatusId = i.APPROVALSTATUSID,
                                                         approvalStatusName = i.TBL_LOAN_APPLICATION_DETL_STA.STATUSNAME,
                                                         principalName = i.TBL_LOAN_PRINCIPAL.NAME,
                                                         principalAccount = i.TBL_LOAN_PRINCIPAL.ACCOUNTNUMBER,
                                                         principalRegNo = i.TBL_LOAN_PRINCIPAL.PRINCIPALSREGNUMBER,
                                                         principalId = i.PRINCIPALID,
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

        public IEnumerable<dynamic> GetLoanApplicationByRelationshipOfficerId(int relationshipOfficerId, int companyId)
        {
            var data = from a in context.TBL_LOAN_APPLICATION
                       where a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationInProgress
                       && a.COMPANYID == companyId && a.DELETED == false
                          && (a.CREATEDBY == relationshipOfficerId || a.RELATIONSHIPOFFICERID == relationshipOfficerId)
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
                           createdBy = a.CREATEDBY,
                           applicationDate = a.APPLICATIONDATE,
                           dateTimeCreated = a.DATETIMECREATED,
                           applicationTenor = Math.Round((double)a.APPLICATIONTENOR) * (12.0 / 365.0),
                           applicationAmount = a.APPLICATIONAMOUNT,
                           regionId = a.CAPREGIONID,
                           preliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                           collateralDetail = a.COLLATERALDETAIL,
                           loanInformation = a.LOANINFORMATION,
                           productClassId = a.PRODUCTCLASSID,
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.targetId
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
                        productClassTypeId = data.PRODUCTCLASSTYPEID
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
                            createdBy = a.CREATEDBY,
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
        public IEnumerable<CustomerViewModels> GetCustomerByApplicationId(int applicationId)
        {
            var customers = (from a in context.TBL_LOAN_APPLICATION_DETAIL 
                        join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                       where a.LOANAPPLICATIONID == applicationId 
                       select new CustomerViewModels
                       {
                           customerId = a.CUSTOMERID,
                           fullName = b.FIRSTNAME + " " + b.LASTNAME
                       }).Distinct().ToList();

            return customers;
        }
        public CustomerApplicationTransactionsViewModels GetCustomerTransactions(int customerId, int applicationId)
        {
            var fields = new CustomerApplicationTransactionsViewModels();

            var first = (from a in context.TBL_LOAN_APPLICATION_TRANS
                             where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId
                         select new CustomerTransactionsViewModels
                             {
                                 cust_Id = a.CUSTOMERTRANSACTIONID.ToString(),
                                 period = a.PERIOD,
                                 productName = a.PRODUCTNAME,
                                 accountNumber =a.ACCOUNTNUMBER,
                                 max_Credit_Balance = a.MAXIMUMCREDITBALANCE,
                                 max_Debit_Balance = a.MAXIMUMDEBITBALANCE,
                                 min_Credit_Balance = a.MINIMUMCREDITBALANCE,
                                 min_Debit_Balance = a.MINIMUMDEBITBALANCE,
                                 credit_Turnover = a.CREDITTURNOVER,
                                 debit_Turnover = a.DEBITTURNOVER,
                             }).ToList();
            var second = (from a in context.TBL_LOAN_APPLICATION_TRANS2
                         where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId
                          select new CustomerTransactionsViewModels
                         {
                             cust_Id = a.CUSTOMERTRANSACTIONID2.ToString(),
                             period = a.PERIOD,
                             productName = a.PRODUCTNAME,
                             accountNumber = a.ACCOUNTNUMBER,
                             interest = a.INTEREST,
                             float_Charge = a.FLOATCHARGE,
                         }).ToList();

            fields.firstTransaction = first;
            fields.secondTransaction = second;

            return fields;
        }
        // PLEASE RENAME THIS METHOD NAME TO BE MORE DESCRIPTIVE like LoanApplicationChecklistValidation
        public LoanApplicationUpdateMessage UpdateApprovalStatusForApplication(int applicationId, int staffId)//, object entity)
        {
            int targetId = 0;
            string str = string.Empty;
            bool isCheckListDone = true;
            int checkListIndex = (int)ChecklistErrorEnum.GoodChecklist;
            LoanApplicationUpdateMessage result = new LoanApplicationUpdateMessage();

            var loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == applicationId);
            bool isCamsolJobRequestSent = false;
            if (loanApplicationDetails.Any())
            {
                var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanApplication).ToList();

                foreach (var detail in loanApplicationDetails)
                {
                    var checklistTypes = from a in context.TBL_CHECKLIST_TYPE select a;
                    foreach (var checklistType in checklistTypes) // through checklist types
                    {
                        if (checklistType.ISPRODUCT_BASED)
                            targetId = detail.LOANAPPLICATIONDETAILID;
                        else
                            targetId = detail.LOANAPPLICATIONDETAILID;

                        var checklistDetails = from a in context.TBL_CHECKLIST_DEFINITION
                                               join b in context.TBL_CHECKLIST_DETAIL on a.CHECKLISTDEFINITIONID
                                               equals b.CHECKLISTDEFINITIONID
                                               where b.TARGETID == targetId
                && b.TARGETTYPEID == (checklistType.ISPRODUCT_BASED ? (short)CheckListTargetTypeEnum.LoanApplicationProductChecklist : (short)CheckListTargetTypeEnum.LoanApplicationCustomerChecklist)
                && a.CHECKLIST_TYPEID == checklistType.CHECKLIST_TYPEID && a.OPERATIONID == (int)OperationsEnum.LoanApplication
                && ids.Contains((int)a.APPROVALLEVELID)
                                               select b;

                        var productId = checklistType.ISPRODUCT_BASED ? (short?)detail.APPROVEDPRODUCTID : null;

                        var checklistDefinitions = (from a in context.TBL_CHECKLIST_DEFINITION
                                                    join b in context.TBL_CHECKLIST_ITEM on a.CHECKLISTITEMID equals b.CHECKLISTITEMID
                                                    where ids.Contains((int)a.APPROVALLEVELID) && a.CHECKLIST_TYPEID == checklistType.CHECKLIST_TYPEID
                                                    && a.OPERATIONID == (int)OperationsEnum.LoanApplication && a.PRODUCTID == productId
                                                    select a).AsQueryable();

                        var camsolJobRequests = (from r in context.TBL_JOB_REQUEST join j in context.TBL_JOB_TYPE on r.JOBTYPEID equals j.JOBTYPEID
                                                 where r.OPERATIONSID == (short)OperationsEnum.LoanApplication && targetId == detail.LOANAPPLICATIONDETAILID && j.JOBTYPEID == (short)JobTypeEnum.camsolCheck
                                                 select r).ToList();

                        if (camsolJobRequests.Count > 0)
                            isCamsolJobRequestSent = true;

                        var cc = checklistDefinitions.Count();
                        var bb = checklistDetails.Count();

                        if (checklistDefinitions.Count() != checklistDetails.Count()) // checking for completion
                        {
                            isCheckListDone = false;
                            str = str + Environment.NewLine + checklistType.CHECKLIST_TYPE_NAME + " " + " is not complete";
                            checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
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

                    var rmSuggestion = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                                        where a.LOANAPPLICATIONDETAILID == detail.LOANAPPLICATIONDETAILID && (detail.CONDITIONPRECIDENT == null
                                        || detail.CONDITIONSUBSEQUENT == null || a.TRANSACTIONDYNAMICS == null)
                                        select a);

                    if (rmSuggestion.Any())
                    {
                        isCheckListDone = false;
                        str = str + " Kindly Complete The RM Suggestions Record" + Environment.NewLine;
                        checkListIndex = (int)ChecklistErrorEnum.IncompleteChecklist;
                    }

                    // if (isCheckListDone == false) break;

                } // foreach loanApplicationDetails

            } // loanApplicationDetails.Any()

            if (!isCamsolJobRequestSent)
                throw new ConditionNotMetException("Job Request must be sent to CAMSOL before you can proceed.");

            if (isCheckListDone && SubmitLoanApplicationForCam(applicationId, staffId, checkListIndex))
            {
                LoadCustomerTurnover(
                   applicationId,
                   loanApplicationDetails.Select(x => x.CUSTOMERID).Distinct().ToList(),
                   (short)staffId
                );

                return new LoanApplicationUpdateMessage
                {
                    isdone = isCheckListDone, // true
                    messageStr = str,
                    checkListIndex = (int)ChecklistErrorEnum.GoodChecklist, // okay
                };
            }

            return new LoanApplicationUpdateMessage
            {
                isdone = isCheckListDone,
                messageStr = str,
                checkListIndex = checkListIndex,
            };

        }

        public bool SubmitLoanApplicationForCam(int applicationId, int staffId, int checkListIndex)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);

            if (appl.PRODUCT_CLASS_PROCESSID == (int)ProductClassProcessEnum.ProductBased && checkListIndex == (int)ChecklistErrorEnum.NegetiveChecklist)
            {
                appl.PRODUCT_CLASS_PROCESSID = (int)ProductClassProcessEnum.CAMBased;
            }

            appl.APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ChecklistCompleted;

            int? receiverLevelId = null;
            receiverLevelId = GetFirstReceiverLevel(staffId, (int)OperationsEnum.CAM, appl.PRODUCTCLASSID);

            workflow.StaffId = staffId;
            workflow.ToStaffId = staffId; //
            workflow.NextLevelId = receiverLevelId; // BREAKING!
            workflow.OperationId = (int)OperationsEnum.CAM;
            workflow.TargetId = appl.LOANAPPLICATIONID;
            workflow.CompanyId = appl.COMPANYID;
            workflow.ProductClassId = appl.PRODUCTCLASSID;
            workflow.StatusId = (int)ApprovalStatusEnum.Pending;
            workflow.Comment = "New loan application";

            return workflow.LogActivity();
        }

        public int? GetFirstReceiverLevel(int staffId, int operationId, short? productClassId, bool next = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);

            var levels = context.TBL_APPROVAL_GROUP_MAPPING.Where(x => x.OPERATIONID == operationId && x.PRODUCTCLASSID == productClassId)
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

            var staffRoleLevels = levels.Where(x => x.staffRoleId == staff.STAFFROLEID);
            var staffRoleLevelIds = staffRoleLevels.Select(x => x.levelId);
            var staffRoleLevelId = staffRoleLevelIds.FirstOrDefault();

            if (next == false) return staffRoleLevelId;
            int index = levels.FindIndex(x => x.levelId == staffRoleLevelId);
            var nextLevelId = levels.Skip(index + 1).Take(1).Select(x => x.levelId).FirstOrDefault();

            return nextLevelId;
        }

        public string GetRefrenceNumber()
        {
            var millisecond = DateTime.Now.Millisecond;
            string refnumber = CommonHelpers.GetLoanReferanceNumber().ToString()
                + "" + CommonHelpers.AppendZeroString(millisecond, 3);
            return refnumber.ToString();
        }

        public LoanApplicationViewModel AddLoanApplication(LoanApplicationViewModel loan)
        {
            if (loan.relationshipOfficerId != 0)
            {
                var limit = creditLimitValidationsRepository.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId).limit;
                var loanAmt = loan.LoanApplicationDetail.Sum(x => x.exchangeAmount);
                loan.applicationAmount = loanAmt;
                if (limit != 0)
                {
                    if (loanAmt > (decimal)limit)
                    {
                        throw new SecureException($"RM Limit Exceeded. The limit of this RM is {limit}");
                    }
                }
            }

            loanData = context.TBL_LOAN_APPLICATION.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId).FirstOrDefault();
            var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId);

            if (loan.editMode == true && UpdateLoanApplicationDetail(loan)) return loan;

            if (loanDetail.Count() == 0 || loan.isNewApplication)
            {
                if (loanData == null)
                {
                    if (string.IsNullOrEmpty(loan.applicationReferenceNumber))
                    {
                        loan.applicationReferenceNumber = GetRefrenceNumber();
                    }
                    //loan.applicationReferenceNumber = GetRefrenceNumber();
                    // CommonHelpers.GetLoanReferanceNumber().ToString();

                    AddloanApplicationSub(loan);
                }

                if (loan.LoanApplicationDetail.Count > 0)
                {
                    AddLoanApplicationDetail(loan.LoanApplicationDetail, loan.createdBy);
                }

            }
            else
            {
                var limit = creditLimitValidationsRepository.ValidateCreditLimitByRMBM((short)loan.relationshipOfficerId).limit;
                var tdata = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loan.loanApplicationId);
                var total = tdata.Sum(o => o.PROPOSEDAMOUNT);

                // var totalAmount = tdata.Sum(o => (o.PROPOSEDAMOUNT * o.EXCHANGERATE));
                //loan.applicationAmount = totalAmount;
                if (limit != 0)
                {
                    if (total != 0)
                    {
                        if (total > (decimal)limit)
                        {
                            throw new SecureException($"RM Limit Exceeded. The limit of this RM is {limit}");
                        }
                    }
                }

                UpdateLoanApplication(loan);
            }

            try
            {
                response = context.SaveChanges();
            }
            catch (Exception ex)
            {
                //
            }


            var returndate = GetLoanApplicationByLoanRefrenceNo(loanData.APPLICATIONREFERENCENUMBER, loanData.COMPANYID);

            if (response > 0 && !loan.isNewApplication)
            {
                returndate.closeApplication = true;
                return returndate;
            }
            return returndate;

        }

        private void AddloanApplicationSub(LoanApplicationViewModel loan)
        {


            short productClassProcessId = 0;
            short? productClassId = null;
            isGroupLoan = false;
            response = 0;
            int loanId = 0;
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
                if (dat.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.CAMBased)
                {
                    productClassId = null;
                    productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;
                }
                if (dat.PRODUCT_CLASS_PROCESSID == (short)ProductClassProcessEnum.ProductBased)
                {
                    productClassId = loan.productClassId;
                    productClassProcessId = dat.PRODUCT_CLASS_PROCESSID;
                }
            }
            decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + loan.proposedAmount;
            var loanStatusId = (short)LoanStatusEnum.Inactive;

            loanData = new TBL_LOAN_APPLICATION
            {
                REQUIRECOLLATERAL = loan.requireCollateral,
                TOTALEXPOSUREAMOUNT = totalAmount,
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
                LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = loan.submittedForAppraisal,
                OPERATIONID = (int)OperationsEnum.CAM,
                LOANAPPLICATIONTYPEID = loan.loanTypeId,
                COLLATERALDETAIL = loan.collateralDetail
            };

            if (isGroupLoan)
            {
                loanData.CUSTOMERGROUPID = loan.customerGroupId;
                loanData.CUSTOMERID = null;
            }
            else
            {
                loanData.CUSTOMERID = loan.customerId;
                loanData.CUSTOMERGROUPID = null;
            }

            if(loan.loanPreliminaryEvaluationId != null && loan.loanPreliminaryEvaluationId != 0)
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
                IPADDRESS = loan.userIPAddress,
                URL = loan.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loan.loanApplicationId
            };

            this.auditTrail.AddAuditTrail(audit);
        }

        private bool UpdateLoanApplicationDetail(LoanApplicationViewModel loan)
        {
            UpdateLoanApplication(loan); // update main

            var detail = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
            var update = loan.LoanApplicationDetail.SingleOrDefault();
            if (update == null) throw new SecureException("Sequence contain not single! " + loan.LoanApplicationDetail.Count());


            // LEFT TO RIGHT MAPPING
            detail.SUBSECTORID = update.subSectorId;
            detail.PROPOSEDAMOUNT = update.proposedAmount;
            detail.PROPOSEDINTERESTRATE = (double)update.proposedInterestRate;
            detail.PROPOSEDPRODUCTID = update.proposedProductId;
            detail.PROPOSEDTENOR = ConvertTenorToDays(update.proposedTenor,update.tenorModeId);
            detail.REPAYMENTTERMS = update.repaymentTerm;
            detail.LOANPURPOSE = update.loanPurpose;
            detail.PRODUCTPRICEINDEXID = update.productPriceIndexId;
            detail.PRODUCTPRICEINDEXRATE = update.productPriceIndexRate;
            detail.CASAACCOUNTID = update.casaAccountId;
            detail.EQUITYCASAACCOUNTID = update.equityCasaAccountId;
            detail.CURRENCYID = update.currencyId;
            detail.TENORFREQUENCYTYPEID = update.tenorModeId;

            var productClassId = detail.TBL_PRODUCT.PRODUCTCLASSID;

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

            if (productClassId == (int)ProductClassEnum.FirstEdu)
            {
                var edu = context.TBL_LOAN_APPLICATION_DETL_EDU.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
                var eduUpdate = update.educationLoan;
                edu.NUMBER_OF_STUDENTS = eduUpdate.numberOfStudent;
                edu.AVERAGE_SCHOOL_FEES = eduUpdate.averageSchoolFees;
                edu.TOTAL_PREVIOUS_TERM_SCHOL_FEES = eduUpdate.totalPreviousTermSchoolFees;
            }

            if (productClassId == (int)ProductClassEnum.FirstTrader)
            {
                var trader = context.TBL_LOAN_APPLICATION_DETL_TRA.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == loan.loanApplicationDetailId);
                var traderUpdate = update.traderLoan;
                trader.MARKETID = traderUpdate.marketId;
                trader.AVERAGE_MONTHLY_TURNOVER = traderUpdate.averageMonthlyTurnover;
                trader.SOLDITEMS = traderUpdate.soldItems;
            }

            if (context.SaveChanges() == 0) throw new SecureException("Nothing was updated!");

            return true;
        }

        private int ConvertTenorToDays(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId)
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = (proposedTenor * 365) / 12; break;
                case (int)TenorMode.Yearly: tenor = (proposedTenor * 365); break;
            }
            return tenor;
        }

        private void UpdateLoanApplication(LoanApplicationViewModel loan)
        {
            var application = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.TBL_LOAN_APPLICATION.LOANAPPLICATIONID == loan.loanApplicationId).ToList();

            decimal totalAmount = GetCustomerTotalOutstandingBalance((int)loan.customerId) + application.Sum(a => a.PROPOSEDAMOUNT);

            decimal totalApplicationAmount = loan.applicationAmount;
            foreach (var item in application)
            {
                var exchangeValue = ((decimal)item.PROPOSEDAMOUNT * (decimal)item.EXCHANGERATE);
                totalApplicationAmount = totalApplicationAmount + exchangeValue;
            }

            this.loanData.REQUIRECOLLATERAL = loan.requireCollateral;
            this.loanData.TOTALEXPOSUREAMOUNT = totalAmount;
            this.loanData.INTERESTRATE = loan.interestRate;
            this.loanData.APPLICATIONDATE = genSetup.GetApplicationDate();
            this.loanData.LOANINFORMATION = loan.loanInformation;
            this.loanData.ISRELATEDPARTY = loan.isRelatedParty;
            this.loanData.ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed;
            this.loanData.CREATEDBY = (int)loan.createdBy;
            this.loanData.DATETIMECREATED = genSetup.GetApplicationDate();
            this.loanData.SYSTEMDATETIME = DateTime.Now;
            this.loanData.CASAACCOUNTID = loan.casaAccountId;
            this.loanData.APPLICATIONAMOUNT = totalApplicationAmount;
            this.loanData.APPLICATIONTENOR = application.Max(c => c.PROPOSEDTENOR);
            this.loanData.COLLATERALDETAIL = loan.collateralDetail;
            this.loanData.CAPREGIONID = loan.regionId;
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

        private void InvoiceDetails(List<InvoiceDetailViewModel> entity, int createdBy)
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
                CERTIFICATENO = c.certificateNumber

            });
            context.TBL_LOAN_APPLICATION_DETL_INV.AddRange(data);
        }

        private void AddLoanApplicationDetail(List<LoanApplicationDetailViewModel> entity, int createdBy)
        {
            foreach (var a in entity)
            {
                if (a.proposedTenor == 0)
                {
                    throw new SecureException("Tenor can not be ZERO (0)");
                }
                int loanId = this.loanData == null ? 0 : this.loanData.LOANAPPLICATIONID;
                int tenor = 0;

                switch (a.tenorModeId)
                {
                    case (int)TenorMode.Daily: tenor = a.proposedTenor; break;
                    case (int)TenorMode.Monthly: tenor = (a.proposedTenor * 365) / 12; break;
                    case (int)TenorMode.Yearly: tenor = (a.proposedTenor * 365); break;
                }

                var data = new TBL_LOAN_APPLICATION_DETAIL
                {
                    APPROVEDAMOUNT = a.proposedAmount,
                    APPROVEDINTERESTRATE = (double)a.proposedInterestRate,
                    APPROVEDPRODUCTID = a.proposedProductId,
                    APPROVEDTENOR = tenor, //Convert.ToInt32(Math.Round(((decimal)(a.proposedTenor / 12) * (decimal)365))),

                    EXCHANGERATE = a.exchangeRate,
                    CURRENCYID = a.currencyId,
                    CUSTOMERID = a.customerId,
                    LOANAPPLICATIONID = loanId,
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
                };

                context.TBL_LOAN_APPLICATION_DETAIL.Add(data);

                if (a.invoiceDetails.Any() && a.productClassId == (short)ProductClassEnum.InvoiceDiscountingFacility)
                {
                    InvoiceDetails(a.invoiceDetails, createdBy);
                }
                if (a.educationLoan != null && a.productClassId == (short)ProductClassEnum.FirstEdu)
                {
                    EducationLoan(a.educationLoan, a.loanApplicationDetailId, createdBy);
                }

                if (a.traderLoan != null && a.productClassId == (short)ProductClassEnum.FirstTrader)
                {
                    TradderLoan(a.traderLoan, a.loanApplicationDetailId, createdBy);
                }
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



            }
        }

        public LoanApplicationDetailViewModel GetLoanApplicationDetailFields(int detailId)
        {
            var fields = new LoanApplicationDetailViewModel();

            var d = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == detailId);
            //var inv = context.TBL_LOAN_APPLICATION_DETL_INV.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == detailId);

            fields = new LoanApplicationDetailViewModel
            {
                proposedAmount = d.PROPOSEDAMOUNT,
                proposedInterestRate = d.PROPOSEDINTERESTRATE,
                proposedProductId = d.PROPOSEDPRODUCTID,
                proposedTenor = d.PROPOSEDTENOR,
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
                crmsFundingSourceId = d.CRMSFUNDINGSOURCEID,
                crmsPaymentSourceId = d.CRMSREPAYMENTSOURCEID,
                crmsFundingSourceCategory = d.CRMSFUNDINGSOURCECATEGORY,
                productPriceIndexId = d.PRODUCTPRICEINDEXID,
                productPriceIndexRate = d.PRODUCTPRICEINDEXRATE,
                
                tenorModeId = d.TENORFREQUENCYTYPEID,
            };

            var invoiceDetails = (from a in context.TBL_LOAN_APPLICATION_DETL_INV where a.LOANAPPLICATIONDETAILID == detailId
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
                           productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                       }).ToList();

            var bondDetails = (from a in context.TBL_LOAN_APPLICATION_DETL_BG where a.LOANAPPLICATIONDETAILID == detailId
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

            var educationLoan = (from a in context.TBL_LOAN_APPLICATION_DETL_EDU where a.LOANAPPLICATIONDETAILID == detailId
                        select new EducationLoanViewModel
                        {
                            numberOfStudent = a.NUMBER_OF_STUDENTS,
                            averageSchoolFees = a.AVERAGE_SCHOOL_FEES,
                            totalPreviousTermSchoolFees = a.TOTAL_PREVIOUS_TERM_SCHOL_FEES,

                        }).FirstOrDefault();


            var traderLoan = (from a in context.TBL_LOAN_APPLICATION_DETL_TRA where a.LOANAPPLICATIONDETAILID == detailId
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

        private void ProductFees(List<ProductFeesViewModel> fees, int loanApplicationId, int createdBy)
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

        public bool UpdateLoanDetailFees(ProductFeesViewModel fees, int loanApplicationId, int createdBy)
        {
            var data = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(c => c.LOANCHARGEFEEID == fees.loanChargeFeeId).FirstOrDefault();

            data.RECOMMENDED_FEERATEVALUE = fees.rate;
            data.HASCONSESSION = fees.hasConsession;
            data.CONSESSIONREASON = fees.consessionReason;
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
        public bool DeleteLoanApplication(int loanApplicationId)
        {
            var dataapplication = context.TBL_LOAN_APPLICATION.Where(d => d.LOANAPPLICATIONID == loanApplicationId).FirstOrDefault();
            var datadetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == dataapplication.LOANAPPLICATIONID).ToList();

            if (datadetail != null)
            {
                foreach (var data in datadetail)
                {
                    //int loanApplicationId = 0;
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


                    context.TBL_LOAN_APPLICATION_DETAIL.Remove(data);
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


                context.TBL_LOAN_APPLICATION_DETAIL.Remove(data);
                loanApplicationId = data.LOANAPPLICATIONID;

                var loan = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == loanApplicationDetailId).ToList();
                if (loan.Count() == 1)
                {
                    var loanApp = context.TBL_LOAN_APPLICATION.Where(la => la.LOANAPPLICATIONID == loanApplicationId);
                    context.TBL_LOAN_APPLICATION.Remove(loanApp.FirstOrDefault());
                }

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
            var data = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == loanApplicatioinCollateralId).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                //loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID
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
                IPADDRESS = entity.FirstOrDefault().userIPAddress,
                URL = entity.FirstOrDefault().applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
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


        public IEnumerable<LoanApplicationDetailViewModel> SearchLoanApplicationDetails(int companyId, string searchQuery)
        {
            searchQuery = searchQuery.Trim().ToLower();

            var allApplicationDetails = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                         join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                                         where a.APPLICATIONREFERENCENUMBER.ToLower().Contains(searchQuery)
                                         //  || c.FIRSTNAME.ToLower().StartsWith(searchQuery)
                                         //  || c.CUSTOMERCODE.ToLower().Contains(searchQuery)
                                         //|| c.MIDDLENAME.ToLower().StartsWith(searchQuery)
                                         //|| c.LASTNAME.ToLower().StartsWith(searchQuery)
                                         //|| a.TBL_CASA.PRODUCTACCOUNTNUMBER==searchQuery
                                         select new LoanApplicationDetailViewModel
                                         {
                                             loanApplicationId = d.LOANAPPLICATIONID,
                                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                             customerId = c.CUSTOMERID,
                                             firstName = c.FIRSTNAME,
                                             middleName = c.MIDDLENAME,
                                             lastName = c.LASTNAME,
                                             customerCode = c.CUSTOMERCODE,
                                             loanApplicationDetailId = d.LOANAPPLICATIONDETAILID,
                                             approvedProductId = d.APPROVEDPRODUCTID,
                                             productName = d.TBL_PRODUCT.PRODUCTNAME,
                                             approvedTenor = d.APPROVEDTENOR,
                                             approvedAmount = d.APPROVEDAMOUNT,//
                                             approvedInterestRate = d.APPROVEDINTERESTRATE,
                                             productClassProcessId = a.PRODUCT_CLASS_PROCESSID,
                                             productClassId = (short?)a.PRODUCTCLASSID,
                                             applicationStatusId = a.APPLICATIONSTATUSID,
                                             applicationStatusPosition = a.TBL_LOAN_APPLICATION_STATUS.POSITION,
                                             approvalStatusId = a.APPROVALSTATUSID,
                                             applicationDate = a.APPLICATIONDATE,
                                             customerType = c.TBL_CUSTOMER_TYPE.NAME,
                                             branchName = a.TBL_BRANCH.BRANCHNAME,
                                             customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                             customerAccountNumber = a.TBL_CASA != null ? a.TBL_CASA.PRODUCTACCOUNTNUMBER : null,

                                             equityAmount = d.EQUITYAMOUNT,
                                             equityCasaAccountId = d.EQUITYCASAACCOUNTID,
                                             currencyId = d.CURRENCYID,
                                             currencyName = d.TBL_CURRENCY.CURRENCYNAME,
                                             currencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                             exchangeRate = d.EXCHANGERATE,
                                             subSectorId = d.SUBSECTORID,
                                             proposedAmount = d.PROPOSEDAMOUNT,
                                             proposedInterestRate = d.PROPOSEDINTERESTRATE,
                                             proposedProductId = d.PROPOSEDPRODUCTID,
                                             proposedProductName = d.TBL_PRODUCT.PRODUCTNAME,
                                         }).ToList();
            return allApplicationDetails;

        }

        #endregion "Loan Applications Awaiting Checklist"

        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            int[] operations = { (int)OperationsEnum.OfferLetterApproval, (int)OperationsEnum.CAM, (int)OperationsEnum.ContigentLoanBooking ,
           (int)OperationsEnum.ContingentLiabilityRenewal,(int)OperationsEnum.ContingentLiabilityUsage,(int)OperationsEnum.ContingentRequestBooking,
            (int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.LoanAvailment};

            searchString = searchString.Trim().ToLower();


            var applications = (from x in context.TBL_LOAN_APPLICATION
                                join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                join y in context.TBL_APPROVAL_TRAIL on x.LOANAPPLICATIONID equals y.TARGETID
                                where y.RESPONSESTAFFID == null
                                && operations.Contains(y.OPERATIONID)
                                && y.APPROVALSTATEID != (int)ApprovalState.Ended
                           && (x.APPLICATIONREFERENCENUMBER == searchString
                        || c.FIRSTNAME.ToLower().Contains(searchString)
                        || c.LASTNAME.ToLower().Contains(searchString)
                        || c.MIDDLENAME.ToLower().Contains(searchString)
                        || a.CREATEDBY == context.TBL_STAFF.Where(o => o.STAFFCODE == searchString.ToUpper()).Select(o => o.STAFFID).FirstOrDefault())
                                select new LoanApplicationViewModel
                                {
                                    firstName = c.FIRSTNAME,
                                    middleName = c.MIDDLENAME,
                                    lastName = c.LASTNAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = x.LOANAPPLICATIONID,
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
                                    applicationTenor = x.APPLICATIONTENOR,

                                    submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                    customerInfoValidated = x.CUSTOMERINFOVALIDATED,
                                    isRelatedParty = x.ISRELATEDPARTY,
                                    isPoliticallyExposed = x.ISPOLITICALLYEXPOSED,
                                    approvalStatusId = (short)x.APPROVALSTATUSID,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                    currentApprovalLevel = y.FROMAPPROVALLEVELID != null ? y.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a",
                                    approvalTrailId = y.APPROVALTRAILID,
                                    responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                                    applicationStatusId = x.APPLICATIONSTATUSID,
                                    applicationStatus = x.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
                                    branchName = x.TBL_BRANCH.BRANCHNAME,
                                    relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                                    relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                                    misCode = x.MISCODE,
                                    customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                    loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                    createdBy = x.CREATEDBY,
                                    loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
                                    operationId = x.OPERATIONID,
                                    // accountNumber = ca.PRODUCTACCOUNTNUMBER,
                                    isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == x.APPLICATIONREFERENCENUMBER).Any()
                                }).ToList();

            return applications;

            /*var applications = context.TBL_LOAN_APPLICATION
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER, g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
                    .Join(context.TBL_CASA, o => o.c.CUSTOMERID, s => s.CUSTOMERID, (o, s) => new { o, s })
                    .GroupJoin(context.TBL_APPROVAL_TRAIL.Where(t => t.RESPONSESTAFFID == null && t.APPROVALSTATEID != (int)ApprovalState.Ended), q => q.o.g.a.LOANAPPLICATIONID, t => t.TARGETID, (q, t) => new { q, t })
                    .SelectMany(x =>
                        x.t.DefaultIfEmpty(),
                        (x, y) => new LoanApplicationViewModel
                        {
                            firstName = x.q.o.c.FIRSTNAME,
                            middleName = x.q.o.c.MIDDLENAME,
                            lastName = x.q.o.c.LASTNAME,
                            customerCode = x.q.o.c.CUSTOMERCODE,
                            applicationReferenceNumber = x.q.o.g.a.APPLICATIONREFERENCENUMBER,

                            loanApplicationId = x.q.o.g.a.LOANAPPLICATIONID,
                            customerId = x.q.o.g.a.CUSTOMERID,
                            branchId = x.q.o.g.a.BRANCHID,
                            customerGroupId = x.q.o.g.a.CUSTOMERGROUPID,
                            loanTypeId = x.q.o.g.a.LOANAPPLICATIONTYPEID,
                            relationshipOfficerId = x.q.o.g.a.RELATIONSHIPOFFICERID,
                            relationshipManagerId = x.q.o.g.a.RELATIONSHIPMANAGERID,
                            applicationDate = x.q.o.g.a.APPLICATIONDATE,
                            applicationAmount = x.q.o.g.a.APPLICATIONAMOUNT,
                            approvedAmount = x.q.o.g.a.APPROVEDAMOUNT,
                            interestRate = x.q.o.g.a.INTERESTRATE,
                            applicationTenor = x.q.o.g.a.APPLICATIONTENOR,

                            submittedForAppraisal = x.q.o.g.a.SUBMITTEDFORAPPRAISAL,
                            customerInfoValidated = x.q.o.g.a.CUSTOMERINFOVALIDATED,
                            isRelatedParty = x.q.o.g.a.ISRELATEDPARTY,
                            isPoliticallyExposed = x.q.o.g.a.ISPOLITICALLYEXPOSED,
                            approvalStatusId = (short)x.q.o.g.a.APPROVALSTATUSID,
                            approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.q.o.g.a.APPROVALSTATUSID).APPROVALSTATUSNAME,

                            currentApprovalLevel = y == null ? "n/a" : y.TBL_APPROVAL_LEVEL1.LEVELNAME,
                            approvalTrailId = y == null ? 0 : y.APPROVALTRAILID,
                            responsiblePerson = y.TOSTAFFID == null ? "n/a" : y.TBL_STAFF1.STAFFCODE + " - " + y.TBL_STAFF1.FIRSTNAME + " " + y.TBL_STAFF1.MIDDLENAME + " " + y.TBL_STAFF1.LASTNAME,

                            applicationStatusId = x.q.o.g.a.APPLICATIONSTATUSID,
                            applicationStatus = x.q.o.g.a.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
                            branchName = x.q.o.g.a.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = x.q.o.g.a.TBL_STAFF.FIRSTNAME + " " + x.q.o.g.a.TBL_STAFF.MIDDLENAME + " " + x.q.o.g.a.TBL_STAFF.LASTNAME,
                            relationshipManagerName = x.q.o.g.a.TBL_STAFF1.FIRSTNAME + " " + x.q.o.g.a.TBL_STAFF1.MIDDLENAME + " " + x.q.o.g.a.TBL_STAFF1.LASTNAME,
                            misCode = x.q.o.g.a.MISCODE,
                            customerGroupName = x.q.o.g.a.CUSTOMERGROUPID.HasValue ? x.q.o.g.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            loanTypeName = x.q.o.g.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            createdBy = x.q.o.g.a.CREATEDBY,
                            loanPreliminaryEvaluationId = x.q.o.g.a.LOANPRELIMINARYEVALUATIONID,
                            operationId = x.q.o.g.a.OPERATIONID,
                            accountNumber = x.q.s.PRODUCTACCOUNTNUMBER,
                            isOfferLetterAvailable = context.TBL_OFFERLETTER.Where(ol => ol.APPLICATIONREFERENCENUMBER == x.q.o.g.a.APPLICATIONREFERENCENUMBER).Any()
                        })
                    .Where(x => x.applicationReferenceNumber == searchString
                        || x.firstName.ToLower().Contains(searchString)
                        || x.lastName.ToLower().Contains(searchString)
                        || x.middleName.ToLower().Contains(searchString)
                        || x.customerCode.ToLower().Contains(searchString)
                        || x.createdBy == context.TBL_STAFF.Where(o=>o.STAFFCODE== searchString.ToUpper()).Select(o=>o.STAFFID).FirstOrDefault()
                        )
                    ;

            var list = applications.ToList();
            applications = applications.OrderByDescending(x => x.approvalTrailId).GroupBy(x => x.applicationReferenceNumber).Select(x => x.FirstOrDefault());
            //var filteredList = applications.ToList();
            return applications; */

        }

        public IEnumerable<CreditApplicationViewModel> CommitteeCreditApplications(int applicationTypeId, int staffId)
        {
            List<int> ids;
            string applicationType;
            IQueryable<CreditApplicationViewModel> applications = null;

            if (applicationTypeId == 1)
            {
                applicationType = "Loan Origination";
                ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CAM).ToList();
                applications = context.TBL_LOAN_APPLICATION
                    .Join(context.TBL_LOAN_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER, g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
                    .Join(context.TBL_APPROVAL_TRAIL.Where(t => t.OPERATIONID == (int)OperationsEnum.CAM
                            && t.RESPONSESTAFFID == null && t.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ids.Contains((int)t.TOAPPROVALLEVELID)
                        ),
                        q => q.g.a.LOANAPPLICATIONID,
                        t => t.TARGETID, (q, t) => new { q, t })
                    .Select(x => new CreditApplicationViewModel
                    {
                        applicationType = applicationType,
                        firstName = x.q.c.FIRSTNAME,
                        middleName = x.q.c.MIDDLENAME,
                        lastName = x.q.c.LASTNAME,
                        customerCode = x.q.c.CUSTOMERCODE,
                        loanApplicationId = x.q.g.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.q.g.a.APPLICATIONREFERENCENUMBER,
                        applicationDate = x.q.g.a.APPLICATIONDATE,
                        customerGroupName = x.q.g.a.CUSTOMERGROUPID.HasValue ? x.q.g.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                    });
            }

            if (applicationTypeId == 2)
            {
                List<int> operationIds = new List<int>();
                operationIds.Add(46);
                operationIds.Add(71);
                operationIds.Add(79);
                applicationType = "Loan Management";
                ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanReviewApprovalAppraisal).ToList();
                applications = context.TBL_LMSR_APPLICATION
                    .Join(context.TBL_LMSR_APPLICATION_DETAIL, a => a.LOANAPPLICATIONID, d => d.LOANAPPLICATIONID, (a, d) => new { a, d })
                    .Join(context.TBL_CUSTOMER, g => g.d.CUSTOMERID, c => c.CUSTOMERID, (g, c) => new { g, c })
                    .Join(context.TBL_APPROVAL_TRAIL.Where(t => operationIds.Contains(t.OPERATIONID)
                        && t.APPROVALSTATEID != (int)ApprovalState.Ended
                        && t.RESPONSESTAFFID == null
                        && ids.Contains((int)t.TOAPPROVALLEVELID)
                        && (t.TOSTAFFID == null || t.TOSTAFFID == staffId)
                    ),
                        q => q.g.a.LOANAPPLICATIONID,
                        t => t.TARGETID, (q, t) => new { q, t })
                    .Select(x => new CreditApplicationViewModel
                    {
                        applicationType = applicationType,
                        firstName = x.q.c.FIRSTNAME,
                        middleName = x.q.c.MIDDLENAME,
                        lastName = x.q.c.LASTNAME,
                        customerCode = x.q.c.CUSTOMERCODE,
                        loanApplicationId = x.q.g.a.LOANAPPLICATIONID,
                        applicationReferenceNumber = x.q.g.a.APPLICATIONREFERENCENUMBER,
                        applicationDate = x.q.g.a.APPLICATIONDATE,
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
                           select b.PRODUCTCLASSID).FirstOrDefault();

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
                               productClassId = (int)ProductClassEnum.InvoiceDiscountingFacility
                           }).ToList();
                return inv;
            }
            else if (details == (short)ProductClassEnum.FirstTrader)
            {
                var trader = (from tra in context.TBL_LOAN_APPLICATION_DETL_TRA
                              where tra.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                              select new TraderLoanViewModel()
                              {
                                  traderId = tra.TRADDERID,
                                  loanApplicationDetailId = tra.LOANAPPLICATIONDETAILID,
                                  marketId = tra.MARKETID,
                                  marketName = tra.TBL_LOAN_MARKET.MARKETNAME,
                                  averageMonthlyTurnover = tra.AVERAGE_MONTHLY_TURNOVER,
                                  productClassId = (int)ProductClassEnum.FirstTrader,
                                  soldItems = tra.SOLDITEMS
                              }).ToList();
                return trader;
            }
            else if (details == (short)ProductClassEnum.FirstEdu)
            {
                var edu = (from e in context.TBL_LOAN_APPLICATION_DETL_EDU
                           where e.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                           select new EducationLoanViewModel()
                           {
                               educationId = e.EDUCATIONID,
                               loanApplicationDetailId = e.LOANAPPLICATIONDETAILID,
                               numberOfStudent = e.NUMBER_OF_STUDENTS,
                               averageSchoolFees = e.AVERAGE_SCHOOL_FEES,
                               totalPreviousTermSchoolFees = e.TOTAL_PREVIOUS_TERM_SCHOL_FEES,
                               productClassId = (int)ProductClassEnum.FirstEdu
                           }).ToList();
                return edu;
            }
            else if (details == (short)ProductClassEnum.BondAndGuarantees)
            {
                var bg = (from b in context.TBL_LOAN_APPLICATION_DETL_BG
                          where b.LOANAPPLICATIONDETAILID == loanApplicationDetailId
                          select new BondsAndGauranteeViewModel()
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
                              principalName = b.TBL_LOAN_PRINCIPAL.NAME,
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
            var dat = (from a in context.TBL_LOAN_APPLICATION_DETL_INV
                       join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                       where b.CUSTOMERID == data.customerId && a.INVOICENO == data.documentNo
                       select a).ToList();
            return dat.Any();

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
                createdBy = x.a.CREATEDBY,
                loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
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
                .Where(x => (x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.OfferLetterRejected || x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.ApplicationRejected)
                //&& x.REVIEW_TYPE == null // <------------------- INT of APPLICATIONSTATUSID to filter
                )
            .Select(x => new LoanApplicationViewModel
            {
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
                createdBy = x.CREATEDBY,
                misCode = x.MISCODE,

                applicationStatus = x.TBL_LOAN_APPLICATION_STATUS.APPLICATIONSTATUSNAME, // <----------------- new 
                relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME,
                relationshipManagerName = x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.MIDDLENAME + " " + x.TBL_STAFF1.LASTNAME,
                customerGroupName = x.CUSTOMERGROUPID.HasValue ? x.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                loanTypeName = x.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                loanPreliminaryEvaluationId = x.LOANPRELIMINARYEVALUATIONID,
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

        public string ReviewRequest(ForwardViewModel model)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(model.applicationId);

            if (context.TBL_LOAN_APPLICATION.Where(x => x.RELATEDREFERENCENUMBER == appl.APPLICATIONREFERENCENUMBER).Any())
            {
                return "This application is already re-initiated!";
            }

            var referenceNumber = GenerateLoanReferenceNumber();
            var applicationDate = genSetup.GetApplicationDate();
            bool wasApproved = appl.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved ? true : false;

            var request = context.TBL_LOAN_APPLICATION.Add(new TBL_LOAN_APPLICATION
            {
                PRODUCTCLASSID = appl.PRODUCTCLASSID,
                APPLICATIONREFERENCENUMBER = referenceNumber,
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
                CUSTOMERID = appl.CUSTOMERID,
                SUBMITTEDFORAPPRAISAL = appl.SUBMITTEDFORAPPRAISAL,
                OPERATIONID = appl.OPERATIONID,
                LOANAPPLICATIONTYPEID = appl.LOANAPPLICATIONTYPEID,
                PRODUCT_CLASS_PROCESSID = appl.PRODUCT_CLASS_PROCESSID,
            });

            var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
            foreach (var x in details)
            {
                context.TBL_LOAN_APPLICATION_DETAIL.Add(new TBL_LOAN_APPLICATION_DETAIL
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
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = applicationDate,
                });
            }

            if (context.SaveChanges() > 0) // <------------------------- skip to test
            {
                int i;
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
                }

                i = 0; // conditions
                var conditions = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                foreach (var x in conditions)
                {
                    context.TBL_LOAN_CONDITION_PRECEDENT.Add(new TBL_LOAN_CONDITION_PRECEDENT
                    {
                        CONDITION = x.CONDITION,
                        ISEXTERNAL = x.ISEXTERNAL,
                        ISSUBSEQUENT = x.ISSUBSEQUENT,
                        //LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                        LOANAPPLICATIONDETAILID = rejectedDetails[i], // ?
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = applicationDate,
                    });
                }

                i = 0; // fees
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
                }

                var cam = context.TBL_CREDIT_APPRAISAL_MEMORANDM.FirstOrDefault(x => x.LOANAPPLICATIONID == appl.LOANAPPLICATIONID);
                if (cam != null)
                {
                    var newcam = context.TBL_CREDIT_APPRAISAL_MEMORANDM.Add(new TBL_CREDIT_APPRAISAL_MEMORANDM
                    {
                        LOANAPPLICATIONID = request.LOANAPPLICATIONID,
                        COMPANYID = cam.COMPANYID,
                        CAMREF = referenceNumber,
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
                }

                var operationId = (int)OperationsEnum.CAM;
                workflow.StaffId = model.createdBy;
                workflow.OperationId = operationId;
                workflow.TargetId = request.LOANAPPLICATIONID;
                workflow.CompanyId = appl.COMPANYID;
                workflow.ProductClassId = appl.PRODUCTCLASSID;
                workflow.ProductId = null; // appl.PRODUCTID;
                workflow.StatusId = model.forwardAction;
                workflow.Comment = model.comment;
                workflow.Amount = appl.APPLICATIONAMOUNT;
                workflow.InvestmentGrade = appl.ISINVESTMENTGRADE;
                workflow.Tenor = appl.APPLICATIONTENOR;
                workflow.PoliticallyExposed = appl.ISPOLITICALLYEXPOSED;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Re-applied for loan with reference number: { referenceNumber }",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.applicationId
                };
                this.auditTrail.AddAuditTrail(audit);
                // End of Audit section ---------------------

                return context.SaveChanges() > 0 ? "New Loan Application Reference Number " + referenceNumber : string.Empty;
            }

            context.TBL_LOAN_APPLICATION_DETAIL.RemoveRange(details);
            context.TBL_LOAN_APPLICATION.Remove(request);

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

                fx = (fina.GetExchangeRate(item.DATETIMECREATED, item.CURRENCYID, companyId).buyingRate);

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
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationUpdate,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Updated loan application with reference Number: {loan.APPLICATIONREFERENCENUMBER}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
            workflow.ProductClassId = model.productClassId;
            workflow.ProductId = model.productId;
            workflow.NextLevelId = model.receiverLevelId;
            workflow.ToStaffId = model.receiverStaffId;
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

        public bool SaveCancelledApplcation(LoanApplicationViewModel data)
        {
            var ApprovalTrail = GetApprovalTrailByOperationIdAndTargetId((int)OperationsEnum.CAM, data.loanApplicationId, data.companyId, data.createdBy);
          var ApprovalStaffCount =  ApprovalTrail.Where(a=>a.requestStaffId != data.createdBy).Count();
            if (ApprovalStaffCount == 0 )
            {
                LaonApplcationCancelllationCompelted(data);

                if (context.SaveChanges() > 0)
                {
                    return true;
                }
                return false;
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
                    APPLICATIONSTATUSID = data.applicationStatusId
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

                //var audit = new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.LoanApplicationCancellation,
                //    STAFFID = data.createdBy,
                //    BRANCHID = (short)data.userBranchId,
                //    DETAIL = $"Cancellation request for Loan Application ID with '{data.loanApplicationId}' has been initiated. Reason being : {data.cancellationReason} ",
                //    IPADDRESS = data.userIPAddress,
                //    URL = data.applicationUrl,
                //    APPLICATIONDATE = genSetup.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now,
                //    TARGETID = application.LOANAPPLICATIONID,
                //};


                //this.auditTrail.AddAuditTrail(audit);
            }




            if (context.SaveChanges() > 0)
            {
                return true;
            }
            return false;
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
                                    createdBy = a.CREATEDBY,
                                    loanPreliminaryEvaluationId = a.LOANPRELIMINARYEVALUATIONID,
                                    operationId = a.OPERATIONID,
                                    tempApplicationCancellationId = t.TEMPAPPLICATIONCANCELLATIONID
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

        public bool GoForLoanApplicationCancellationApproval(LoanApplicationViewModel data)
        {
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = data.createdBy;
                workflow.CompanyId = data.companyId;
                workflow.StatusId = (short)data.approvalStatusId;
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
        private void UpdateLoanApplicationCancellationTempTable(LoanApplicationViewModel data, short statusId)
        {
            var val = context.TBL_TEMP_LOAN_APPLTN_CANCELTN.Where(x => x.TEMPAPPLICATIONCANCELLATIONID == data.tempApplicationCancellationId).Select(x => x).FirstOrDefault();
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
        private void LaonApplcationCancelllationInPregress(LoanApplicationViewModel data)
        {
            var val = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == data.loanApplicationId).Select(x => x).FirstOrDefault();
            val.APPLICATIONSTATUSID = (int)LoanApplicationStatusEnum.CancellationInProgress;
        }

        private void LogEmailAlertForLoanApplicationCancellation(string messageBody, string alertSubject, string recipients)
        {
            try
            {
                string recipient = recipients.Trim();

                string messageSubject = alertSubject;
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
            var data = (from a in context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == reference)
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.COMPANYID == companyId && a.DELETED == false && b.DELETED == false
                        select new LoanApplicationDetailViewModel
                        {
                            currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                            exchangeRate = b.EXCHANGERATE,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            proposedAmount = b.PROPOSEDAMOUNT,

                            //requireCollateral = a.REQUIRECOLLATERAL,
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
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            customerGroupId = (int?)a.CUSTOMERGROUPID,//.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            //customerAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER
                        });
            var result = data.ToList();
            var test = result.Count();

            return result;
        }

        public void LoadCustomerTurnover(int applicationId, List<int> customerIds, short staffId) // OBIE (Page 4)
        {
            int turnoverDuration = 48;
            var apiTransactions = new List<CustomerTurnoverViewModels>();
            var itx = new List<CustomerTurnoverViewModels>();

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
                Task.Run(async () => { apiTransactions = await _customerIntegration.GetCustomerTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();

                foreach (var transaction in apiTransactions)
                {
                    decimal amc = 0;
                    Decimal.TryParse(transaction.amc.Replace(",", ""), out amc);
                    decimal vat = 0;
                    Decimal.TryParse(transaction.vat.Replace(",", ""), out vat);
                    decimal management_Fee = 0;
                    Decimal.TryParse(transaction.management_Fee.Replace(",", ""), out management_Fee);
                    decimal commitment_Fees = 0;
                    Decimal.TryParse(transaction.commitment_Fees.Replace(",", ""), out commitment_Fees);
                    decimal com_Contigent_Liab = 0;
                    Decimal.TryParse(transaction.com_Contigent_Liab.Replace(",", ""), out com_Contigent_Liab);
                    decimal lc_Commission = 0;
                    Decimal.TryParse(transaction.lc_Commission.Replace(",", ""), out lc_Commission);
                    context.TBL_LOAN_APPLICATION_TRANS.Add(new TBL_LOAN_APPLICATION_TRANS
                    {
                        LOANAPPLICATIONID = (short)applicationId,
                        CUSTOMERID = (short)customer.CUSTOMERID,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        ACCOUNTNUMBER = transaction.foracid,
                        PERIOD = transaction.period,
                        PRODUCTNAME = "n/a",
                        MINIMUMDEBITBALANCE = transaction.min_Debit_Balance,
                        MAXIMUMDEBITBALANCE = transaction.max_Debit_Balance,
                        MINIMUMCREDITBALANCE = transaction.min_Credit_Balance,
                        MAXIMUMCREDITBALANCE = transaction.max_Credit_Balance,
                        DEBITTURNOVER = transaction.debit_Turnover,
                        CREDITTURNOVER = transaction.credit_Turnover,
                        SMSALERT = transaction.sms_Alert,
                        AMC  = amc ,
                        VAT = vat,
                        MANAGEMENTFEE = management_Fee,
                        COMMITMENTFEE = commitment_Fees,
                        CONTINGENTLIABILITYCOMM = com_Contigent_Liab,
                        LC_COMMISSION = lc_Commission,
                        CREATEDBY = staffId,
                        DATETIMECREATED = DateTime.Now,
                    });
                }
            }

            foreach (var customer in customers)
            {
                Task.Run(async () => { itx = await _customerIntegration.GetCustomerInterestTransactions(customer.CUSTOMERCODE, turnoverDuration); }).GetAwaiter().GetResult();


                foreach (var t in itx)
                {
                    decimal float_Charge = 0;
                    Decimal.TryParse(t.float_Charge.Replace(",", ""), out float_Charge);
                    decimal interest = 0;
                    Decimal.TryParse(t.interest.Replace(",", ""), out interest);

                    context.TBL_LOAN_APPLICATION_TRANS2.Add(new TBL_LOAN_APPLICATION_TRANS2
                    {
                        LOANAPPLICATIONID = (short)applicationId,
                        CUSTOMERID = (short)customer.CUSTOMERID,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        ACCOUNTNUMBER = t.foracid,
                        PERIOD = t.period,
                        PRODUCTNAME = "n/a",
                        FLOATCHARGE= float_Charge,
                        INTEREST = interest,
                        CREATEDBY = staffId,
                        DATETIMECREATED = DateTime.Now,
                    });
                }
            }

            if (context.SaveChanges() == 0) throw new SecureException("Customer turnover failed to load!");

        }
    }
}
