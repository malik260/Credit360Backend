using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class FacilityDetailSummary : IFacilityDetailSummary
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ILoanScheduleRepository loanSchedule;
        private ILoanCovenantRepository loanCovenant;
        private IFinanceTransactionRepository financeTransaction;
        private ICasaLienRepository casaLien;
        private ICustomerRepository customers;
        private IOverRideRepository overrider;

        public FacilityDetailSummary(

            FinTrakBankingContext _context,
        IGeneralSetupRepository _generalSetup,
        IAuditTrailRepository _auditTrail,
        ILoanScheduleRepository _loanSchedule,
        ILoanCovenantRepository _loanCovenant,
        IFinanceTransactionRepository _financeTransaction,
        ICasaLienRepository _casaLien,
        ICustomerRepository _customers,
        IOverRideRepository _overrider
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            loanSchedule = _loanSchedule;
            loanCovenant = _loanCovenant;
            financeTransaction = _financeTransaction;
        }
        public List<CollateralViewModel> Collateral(int loanId)
        {
            var data = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                        join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                        join ct in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                        join cs in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                        where x.LOANID == loanId
                        select new CollateralViewModel
                        {
                            collateralType = ct.COLLATERALTYPENAME,
                            collateralSubTypeName = cs.COLLATERALSUBTYPENAME,
                            collateralCode = c.COLLATERALCODE,
                            collateralValue = c.COLLATERALVALUE,
                            haircut=c.HAIRCUT

                        }).ToList();
            return data;
        }

        public LoanViewModel FacilityDetail(int loanId)
        {
            LoanViewModel result;
            var data = this.context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.REVOLVINGLOANID == loanId);
            if (data != null)
            {
                result = GetDisbursedODByODId(loanId);
            }
            else
            {
                result = GetDisbursedLoanByLoan(loanId);
            }
            return result;
        }

        public List<LoanChargeFeeViewModel> GuarantorDetail(int loanId)
        {
            throw new NotImplementedException();
        }

        public List<LoanChargeFeeViewModel> LoanChargeFee(int loanId)
        {
            var data = (from c in context.TBL_LOAN_FEE
                        where c.LOANID == loanId //&& c.Deleted == false
                        select new LoanChargeFeeViewModel
                        {
                            loanChargeFeeId = c.LOANCHARGEFEEID,
                            loanId = c.LOANID,
                            chargeFeeId = c.CHARGEFEEID,
                            chargeFeeName = c.TBL_CHARGE_FEE.CHARGEFEENAME,
                            feeRateValue = c.FEERATEVALUE,
                            feeDependentAmount = c.FEEDEPENDENTAMOUNT,
                            feeAmount = c.FEEAMOUNT,
                            feeIntervalId = c.TBL_CHARGE_FEE.FEEINTERVALID,
                            feeIntervalName = c.TBL_CHARGE_FEE.TBL_FEE_INTERVAL.FEEINTERVALNAME,
                        }).ToList();
            return data;
        }

        public List<LoanCovenantDetailViewModel> LoanCovenantDetail(int loanId)
        {
            var data = (from a in context.TBL_LOAN_COVENANT_DETAIL
                        where a.LOANID == loanId && a.DELETED == false
                        select new LoanCovenantDetailViewModel
                        {
                            loanCovenantDetailId = a.LOANCOVENANTDETAILID,
                            covenantDetail = a.COVENANTDETAIL,
                            loanId = a.LOANID,
                            covenantTypeId = (short)a.COVENANTTYPEID,
                            frequencyTypeId = (short)a.FREQUENCYTYPEID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE,
                            casaAccountId = a.CASAACCOUNTID
                        }).ToList();
            return data;
        }

        public List<LoanPaymentSchedulePeriodicViewModel> LoanSchedule(int loanId)
        {
            var loanSchedule = (from sch in context.TBL_LOAN_SCHEDULE_PERIODIC
                                where sch.LOANID == loanId
                                select new LoanPaymentSchedulePeriodicViewModel
                                {
                                    loanId = sch.LOANID,
                                    paymentNumber = sch.PAYMENTNUMBER,
                                    paymentDate = sch.PAYMENTDATE,
                                    startPrincipalAmount = (double)sch.STARTPRINCIPALAMOUNT,
                                    periodPaymentAmount = (double)sch.PERIODPAYMENTAMOUNT,
                                    periodInterestAmount = (double)sch.PERIODINTERESTAMOUNT,
                                    periodPrincipalAmount = (double)sch.PERIODPRINCIPALAMOUNT,
                                    endPrincipalAmount = (double)sch.ENDPRINCIPALAMOUNT,
                                    interestRate = sch.INTERESTRATE,
                                    amortisedStartPrincipalAmount = (double)sch.AMORTISEDSTARTPRINCIPALAMOUNT,
                                    amortisedPeriodPaymentAmount = (double)sch.AMORTISEDPERIODPAYMENTAMOUNT,
                                    amortisedPeriodInterestAmount = (double)sch.AMORTISEDPERIODINTERESTAMOUNT,
                                    amortisedPeriodPrincipalAmount = (double)sch.AMORTISEDPERIODPRINCIPALAMOUNT,
                                    amortisedEndPrincipalAmount = (double)sch.AMORTISEDENDPRINCIPALAMOUNT,
                                    effectiveInterestRate = sch.EFFECTIVEINTERESTRATE
                                }).ToList();
            return loanSchedule;
        }


        private LoanViewModel GetDisbursedODByODId(int loanId)
        {

            var loanDetails = (from a in context.TBL_LOAN_REVOLVING
                               join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                               join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                               join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                               join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                               join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                               where a.REVOLVINGLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.REVOLVINGLOANID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   loanApplicationId = lp.LOANAPPLICATIONID,
                                   productTypeId = pr.PRODUCTTYPEID,
                                   productName = pr.PRODUCTNAME,
                                   productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.OVERDRAFTLIMIT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   isDisbursed = a.ISDISBURSED,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = tt.OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                   customerGroupId = lp.CUSTOMERGROUPID,
                                   loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                   loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                   //outstandingPrincipal = availableBalance,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = a.TBL_CURRENCY.CURRENCYNAME,
                               }).FirstOrDefault();
            return loanDetails;
        }
        private LoanViewModel GetDisbursedLoanByLoan(int loanId)
        {
            var loanDetails = (from a in context.TBL_LOAN
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                               join f in context.TBL_PRODUCT on a.PRODUCTID equals f.PRODUCTID
                               join pt in context.TBL_PRODUCT_TYPE on f.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                               join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                               join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                               join cur in context.TBL_CURRENCY on a.CURRENCYID equals cur.CURRENCYID
                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                               join ro in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals ro.STAFFID
                               join rm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals rm.STAFFID
                               where a.TERMLOANID == loanId && a.ISDISBURSED == true
                               select new LoanViewModel
                               {
                                   loanId = a.TERMLOANID,
                                   loanApplicationId = a.LOANAPPLICATIONDETAILID,
                                   customerId = a.CUSTOMERID,
                                   customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                   customerCode = b.CUSTOMERCODE,
                                   productId = a.PRODUCTID,
                                   companyId = a.COMPANYID,
                                   casaAccountId = a.CASAACCOUNTID,
                                   branchId = a.BRANCHID,
                                   branchName = br.BRANCHNAME,
                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                   applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",
                                   principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID != null ? (short)a.PRINCIPALFREQUENCYTYPEID : (short)0,
                                   pricipalFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                                   interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID != null ? (short)a.INTERESTFREQUENCYTYPEID : (short)0,
                                   interestFrequencyTypeName = a.TBL_FREQUENCY_TYPE1.MODE,
                                   productTypeId = f.PRODUCTTYPEID,
                                   productName = f.PRODUCTNAME,
                                   productTypeName = pt.PRODUCTTYPENAME,
                                   principalNumberOfInstallment = a.PRINCIPALNUMBEROFINSTALLMENT,
                                   interestNumberOfInstallment = a.INTERESTNUMBEROFINSTALLMENT,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = ro.FIRSTNAME + " " + ro.MIDDLENAME + " " + ro.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = rm.FIRSTNAME + " " + rm.MIDDLENAME + " " + rm.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMiscode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   effectiveDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   bookingDate = a.BOOKINGDATE,
                                   principalAmount = a.PRINCIPALAMOUNT,
                                   principalInstallmentLeft = a.PRINCIPALINSTALLMENTLEFT,
                                   interestInstallmentLeft = a.INTERESTINSTALLMENTLEFT,
                                   approvalStatusId = a.APPROVALSTATUSID,
                                   approvedBy = a.APPROVEDBY,
                                   approverComment = a.APPROVERCOMMENT,
                                   dateApproved = a.DATEAPPROVED,
                                   loanStatusId = a.LOANSTATUSID,
                                   scheduleTypeId = a.SCHEDULETYPEID,
                                   // scheduleTypeName = a.TBL_LOAN_SCHEDULE_TYPE.SCHEDULETYPENAME,
                                   isDisbursed = a.ISDISBURSED,
                                   disbursedBy = a.DISBURSEDBY,
                                   disburserComment = a.DISBURSERCOMMENT,
                                   disburseDate = a.DISBURSEDATE,
                                   operationId = a.OPERATIONID,
                                   operationName = context.TBL_OPERATIONS.FirstOrDefault(x => x.OPERATIONID == a.OPERATIONID).OPERATIONNAME,
                                   subSectorName = a.TBL_SUB_SECTOR.NAME,
                                   sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                   casaAccountNumber = c.PRODUCTACCOUNTNUMBER,
                                   productAccountName = c.PRODUCTACCOUNTNAME,
                                   customerGroupId = e.CUSTOMERGROUPID,
                                   loanTypeId = e.LOANAPPLICATIONTYPEID,
                                   //loanTypeName = e.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   equityContribution = a.EQUITYCONTRIBUTION,
                                   firstPrincipalPaymentDate = a.FIRSTPRINCIPALPAYMENTDATE,
                                   firstInterestPaymentDate = a.FIRSTINTERESTPAYMENTDATE,
                                   outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                   outstandingInterest = a.OUTSTANDINGINTEREST,
                                   principalAdditionCount = a.PRINCIPALADDITIONCOUNT ?? 0,
                                   principalReductionCount = a.PRINCIPALREDUCTIONCOUNT ?? 0,
                                   fixedPrincipal = a.FIXEDPRINCIPAL,
                                   profileLoan = a.PROFILELOAN,
                                   dischargeLetter = a.DISCHARGELETTER,
                                   suspendInterest = a.SUSPENDINTEREST,
                                   customerSensitivityLevelId = b.CUSTOMERSENSITIVITYLEVELID,
                                   createdBy = a.CREATEDBY,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   // isCamsol = context.TBL_LOAN_CAMSOL.Where(x => x.LOANID == a.TERMLOANID).Any(),
                                   exchangeRate = a.EXCHANGERATE,
                                   currencyId = a.CURRENCYID,
                                   currency = cur.CURRENCYNAME
                               }).FirstOrDefault();

            return loanDetails;

        }

        public List<LoanViewModel> LoanSearch(int productTypeId, string searchQuery)
        {

           // var applicationDate = generalSetup.GetApplicationDate();

            try
            {
                List<LoanViewModel> allFilteredLoan = null;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                }

                if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
                {
                    if (productTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
                    {
                        allFilteredLoan = SearchTermLoan(searchQuery);
                    }
                    else if (productTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
                    {
                        allFilteredLoan = SearchRevolvingLoan(searchQuery);
                    }
                    else if (productTypeId == (int)LoanSystemTypeEnum.ContingentLiability)
                    {
                        allFilteredLoan = SearchContigentLoan(searchQuery);
                    }
                }

                //var x = allFilteredLoan.ToList();

                return allFilteredLoan;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<LoanViewModel> SearchTermLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName =  a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1
                                   });
            return allFilteredLoan.ToList();
        }
        private List<LoanViewModel> SearchRevolvingLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1
                                   });
            return allFilteredLoan.ToList();
        }

        private List<LoanViewModel> SearchContigentLoan(string searchQuery)
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_CONTINGENT
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true && (a.LOANREFERENCENUMBER.Contains(searchQuery) ||
                                   b.CUSTOMERCODE.ToLower().Contains(searchQuery) ||
                                   b.FIRSTNAME.ToLower().Contains(searchQuery) ||
                                   b.LASTNAME.ToLower().Contains(searchQuery) ||
                                   c.PRODUCTACCOUNTNUMBER.ToLower().Contains(searchQuery))
                                   select new LoanViewModel
                                   {
                                       loanId = a.CONTINGENTLOANID,
                                       customerId = a.CUSTOMERID,
                                       productId = a.PRODUCTID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = 1,
                                       principalAmount = a.CONTINGENTAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       // isPerforming = a.USER_PRUDENTIAL_GUIDE_STATUSID == 1
                                   });
            return allFilteredLoan.ToList();
        }

        
        public List<ProductType> ProductType()
        {
            List<int> productTypeId = new List<int> { (int)LoanProductTypeEnum.TermLoan, (int)LoanProductTypeEnum.RevolvingLoan, (int)LoanProductTypeEnum.ContingentLiability };
            return (from x in context.TBL_PRODUCT_TYPE
                    where productTypeId.Contains(x.PRODUCTTYPEID)
                    select new ProductType
                    {
                        productTypeId = x.PRODUCTTYPEID,
                        prodcutTypeName = x.PRODUCTTYPENAME
                    }).ToList();
        }
    }
}
