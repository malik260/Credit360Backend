using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public partial class RemedialAssetsReport
    {
        public IEnumerable<GlobalExposureApplicationViewModel> OutOfCourtSettlement(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERCODE
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.LOANREFERENCE  equals b.LOANREFERENCE
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new GlobalExposureApplicationViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();
               
                return dataLoan;
            }

        }
        public IEnumerable<GlobalExposureApplicationViewModel> CollateralSales(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERCODE
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new GlobalExposureApplicationViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }
        public IEnumerable<RemedialAssetReportViewModel> RecoveryAgentUpdate(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataExposure = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && l.DELETED == false

                                select new RemedialAssetReportViewModel
                                {
                                    accreditedConsultant = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCREDITEDCONSULTANTID,
                                    AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    Telephone = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    DateOfAssignment = l.DATEASSIGNED,
                                    Email = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().EMAILADDRESS,
                                    casaAccount = ln.ACCOUNTNUMBER,
                                }).ToList();

                var dataDigitalExposure = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                    join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                    join ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                    where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                    && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                    && l.DELETED == false

                                    select new RemedialAssetReportViewModel
                                    {
                                        accreditedConsultant = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCREDITEDCONSULTANTID,
                                        AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                        AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                        NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                        Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                        Telephone = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                        ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                        AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                        DateOfAssignment = l.DATEASSIGNED,
                                        Email = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().EMAILADDRESS,
                                        casaAccount = ln.ACCOUNTNUMBER,
                                    }).ToList();

                var dataLoan = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && l.DELETED == false

                                select new RemedialAssetReportViewModel
                                {
                                    accreditedConsultant = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCREDITEDCONSULTANTID,
                                    AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    Telephone = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    DateOfAssignment = l.DATEASSIGNED,
                                    Email = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().EMAILADDRESS,
                                    casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                }).ToList();

                var dataRevolvingLoan = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                             && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && l.DELETED == false

                                         select new RemedialAssetReportViewModel
                                         {
                                             accreditedConsultant = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCREDITEDCONSULTANTID,
                                             AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                             AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                             NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                             Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                             Telephone = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                             ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                             AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                             DateOfAssignment = l.DATEASSIGNED,
                                             Email = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().EMAILADDRESS,
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         }).ToList();


                var unionAll = dataLoan.Union(dataRevolvingLoan).Union(dataExposure).Union(dataDigitalExposure);
                var groupRecord = unionAll.GroupBy(x => x.accreditedConsultant).Select(g => g.OrderByDescending(b => b.accreditedConsultant).FirstOrDefault());
                
                foreach (var i in groupRecord)
                {
                    i.ListOfAccountsAssigned = i.ListOfAccountsAssigned + "," + i.casaAccount;
                }

                return groupRecord;

                
            }

        }
        public IEnumerable<RemedialAssetReportViewModel> RecoveryCommission(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataExposure = (from l in context.TBL_LOAN_RECOVERY_COMMISSION_BATCH
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCENUMBER equals lr.LOANREFERENCE
                                join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = ln.ACCOUNTNUMBER,
                                    AccountName = "CURRENT ACCOUNT",
                                    RecoveryAgentName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    TotalExposure = ln.TOTALEXPOSURE, 
                                    AccountRecovered = l.AMOUNTRECOVERED,
                                    DateOfRecovery = lr.RECEIPTDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    PercentageCommissionPaid = lr.PERCENTAGECOMMISSION,
                                    CommissionPayable = (l.AMOUNTRECOVERED - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100))) - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100)),
                                }).ToList();

                var dataDigitalExposure = (from l in context.TBL_LOAN_RECOVERY_COMMISSION_BATCH
                                    join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCENUMBER equals lr.LOANREFERENCE
                                    join ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                    where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                    && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))

                                    select new RemedialAssetReportViewModel
                                    {
                                        AccountNumber = ln.ACCOUNTNUMBER,
                                        AccountName = "CURRENT ACCOUNT",
                                        RecoveryAgentName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                        TotalExposure = ln.TOTALEXPOSURE,
                                        AccountRecovered = l.AMOUNTRECOVERED,
                                        DateOfRecovery = lr.RECEIPTDATE,
                                        AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                        PercentageCommissionPaid = lr.PERCENTAGECOMMISSION,
                                        CommissionPayable = (l.AMOUNTRECOVERED - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100))) - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100)),
                                    }).ToList();

                var dataLoan = (from l in context.TBL_LOAN_RECOVERY_COMMISSION_BATCH
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCENUMBER equals lr.LOANREFERENCE
                                join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                    AccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                    RecoveryAgentName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    TotalExposure = lp.TOTALEXPOSUREAMOUNT, //context.TBL_GLOBAL_EXPOSURE.Where(x => x.CUSTOMERID == l.CUSTOMERID.ToString()).Sum(x => x.TOTALEXPOSURE),
                                    AccountRecovered = l.AMOUNTRECOVERED,
                                    DateOfRecovery = lr.RECEIPTDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    PercentageCommissionPaid = lr.PERCENTAGECOMMISSION,
                                    CommissionPayable = (l.AMOUNTRECOVERED - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100))) - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100)),
                                }).ToList();

                var dataRevolvingLoan = (from l in context.TBL_LOAN_RECOVERY_COMMISSION_BATCH
                                         join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCENUMBER equals lr.LOANREFERENCE
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                               && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                               && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

                                         select new RemedialAssetReportViewModel
                                         {
                                             AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             AccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             RecoveryAgentName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                             TotalExposure = lp.TOTALEXPOSUREAMOUNT, //context.TBL_GLOBAL_EXPOSURE.Where(x => x.CUSTOMERID == l.CUSTOMERID.ToString()).Sum(x => x.TOTALEXPOSURE),
                                             AccountRecovered = l.AMOUNTRECOVERED,
                                             DateOfRecovery = lr.RECEIPTDATE,
                                             AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                             PercentageCommissionPaid = lr.PERCENTAGECOMMISSION,
                                             CommissionPayable = (l.AMOUNTRECOVERED - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100))) - (l.AMOUNTRECOVERED * (lr.PERCENTAGECOMMISSION / 100)),
                                         }).ToList();


                var groupRecord = dataLoan.Union(dataRevolvingLoan).Union(dataExposure).Union(dataDigitalExposure);

                return groupRecord;

            }

        }
        public IEnumerable<RemedialAssetReportViewModel> RecoveryAgentPerformance(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataExposure = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && l.DELETED == false
                                && l.SOURCE.ToLower() == "remedial"

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    TelephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    DateOfEngagement = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().DATEOFENGAGEMENT,
                                    ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    DateOfAssignment = l.DATEASSIGNED,
                                    AccountBalance = 0,
                                    casaAccount = ln.ACCOUNTNUMBER,
                                }).ToList();

                var dataDigitalExposure = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                    join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                    join ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                    where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                    && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                    && l.DELETED == false
                                    && l.SOURCE.ToLower() == "remedial"

                                    select new RemedialAssetReportViewModel
                                    {
                                        AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                        AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                        NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                        Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                        TelephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                        DateOfEngagement = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().DATEOFENGAGEMENT,
                                        ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                        AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                        DateOfAssignment = l.DATEASSIGNED,
                                        AccountBalance = 0,
                                        casaAccount = ln.ACCOUNTNUMBER,
                                    }).ToList();

                var dataLoan = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                join ln in context.TBL_LOAN on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && l.DELETED == false
                                && l.SOURCE.ToLower() == "remedial"

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    TelephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    DateOfEngagement = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().DATEOFENGAGEMENT,
                                    ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                    AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                    DateOfAssignment = l.DATEASSIGNED,
                                    AccountBalance = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().LEDGERBALANCE,
                                    casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                }).ToList();

                var dataRevolvingLoan = (from l in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on l.LOANREFERENCE equals lr.LOANREFERENCE
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANREFERENCE equals ln.LOANREFERENCENUMBER
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(l.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                             && DbFunctions.TruncateTime(l.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                             && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && l.DELETED == false
                                             && l.SOURCE.ToLower() == "remedial"

                                         select new RemedialAssetReportViewModel
                                         {

                                             AccountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                             AccountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                             NameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                             Address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                             TelephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == l.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                             ExpectedRecoveryDate = (DateTime)l.EXPCOMPLETIONDATE,
                                             AmountRecovered = lr.ISFULLYRECOVERED == true ? lr.TOTALRECOVERYAMOUNT : lr.RECOVEREDAMOUNT,
                                             DateOfAssignment = l.DATEASSIGNED,
                                             AccountBalance = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().LEDGERBALANCE,
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         }).ToList();


                var groupRecord = dataLoan.Union(dataRevolvingLoan).Union(dataExposure).Union(dataDigitalExposure);
                return groupRecord;

            }

        }
        public IEnumerable<GlobalExposureApplicationViewModel> LitigationRecoveries(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_GLOBAL_EXPOSURE on ln.LOANREFERENCE equals a.REFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERCODE
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.LOANREFERENCE equals b.LOANREFERENCE
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new GlobalExposureApplicationViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                var dataDigitalLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_GLOBAL_EXPOSURE on ln.LOANREFERENCE equals a.REFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERCODE
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.LOANREFERENCE equals b.LOANREFERENCE
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new GlobalExposureApplicationViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                var allData = dataLoan.Union(dataDigitalLoan);

                return allData;
            }

        }
        public IEnumerable<RemedialAssetReportViewModel> RevalidationOfFullAndFinalSettlement(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from l in context.TBL_LOAN_REVIEW_OPERATION
                                join ln in context.TBL_LOAN on l.LOANID equals ln.TERMLOANID
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(l.DATECREATED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATECREATED) <= DbFunctions.TruncateTime(endDate))
                                && l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && l.OPERATIONTYPEID == (int)OperationsEnum.FullAndFinalCompleteWriteOff

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                                    CustomerId = cu.CUSTOMERCODE,
                                    CustomerName = lp.CUSTOMERGROUPID==null ? (cu.FIRSTNAME+""+cu.MIDDLENAME+""+cu.LASTNAME) : context.TBL_CUSTOMER_GROUP.Where(g=>g.CUSTOMERGROUPID == (int)lp.CUSTOMERGROUPID).Select(g=>g.GROUPNAME).FirstOrDefault(),
                                    FullAndFinalAmountApproved = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == l.LOANREVIEWAPPLICATIONID).FirstOrDefault().APPROVEDAMOUNT,
                                    maturityDate = l.MATURITYDATE,
                                    effectiveDate = l.EFFECTIVEDATE,
                                    ExpectedPaymentPeriod = ""+l.EFFECTIVEDATE.Date+" "+l.MATURITYDATE.Value,
                                    AgeOfLastCredit = 0,
                                    Classification = "",
                                }).ToList();

                var dataRevolvingLoan = (from l in context.TBL_LOAN_REVIEW_OPERATION
                                         join ln in context.TBL_LOAN_REVOLVING on l.LOANID equals ln.REVOLVINGLOANID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(l.DATECREATED) >= DbFunctions.TruncateTime(startDate)
                                             && DbFunctions.TruncateTime(l.DATECREATED) <= DbFunctions.TruncateTime(endDate))
                                             && l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && l.OPERATIONTYPEID == (int)OperationsEnum.FullAndFinalCompleteWriteOff

                                         select new RemedialAssetReportViewModel
                                         {
                                             AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                                             CustomerId = cu.CUSTOMERCODE,
                                             CustomerName = lp.CUSTOMERGROUPID == null ? (cu.FIRSTNAME + "" + cu.MIDDLENAME + "" + cu.LASTNAME) : context.TBL_CUSTOMER_GROUP.Where(g => g.CUSTOMERGROUPID == (int)lp.CUSTOMERGROUPID).Select(g => g.GROUPNAME).FirstOrDefault(),
                                             FullAndFinalAmountApproved = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == l.LOANREVIEWAPPLICATIONID).FirstOrDefault().APPROVEDAMOUNT,
                                             maturityDate = l.MATURITYDATE,
                                             effectiveDate = l.EFFECTIVEDATE,
                                             ExpectedPaymentPeriod = "" + l.EFFECTIVEDATE.Date + " " + l.MATURITYDATE.Value,
                                             AgeOfLastCredit = 0,
                                             Classification = "",
                                         }).ToList();


                var groupRecord = dataLoan.Union(dataRevolvingLoan);
                return groupRecord;

            }

        }
        public IEnumerable<RemedialAssetReportViewModel> IdleAssetsSales(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY 
                                join ln in context.TBL_LOAN on lr.LOANID equals ln.TERMLOANID
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(lr.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(lr.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                && lr.COLLECTIONMODE.ToLower() == "collateral"

                                select new RemedialAssetReportViewModel
                                {
                                    CollateralDescription = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == lr.COLLATERALCODE).FirstOrDefault().COLLATERALSUMMARY,
                                    Branch = br.BRANCHNAME,
                                    Region = "",
                                    StatusOfPerction = "",
                                    DateOfValuation = DateTime.Now,
                                    OMV = 0.00,
                                    FSV = 0.00,
                                    NBV = "",
                                    AmountSold = 0.00,
                                    PAndLImpact = 0.00,
                                    DateSold = "",
                                }).ToList();

                var dataRevolvingLoan = (from lr in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY 
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANID equals ln.REVOLVINGLOANID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(lr.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                         && DbFunctions.TruncateTime(lr.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                         && lr.COLLECTIONMODE.ToLower() == "collateral"

                                         select new RemedialAssetReportViewModel
                                         {
                                             CollateralDescription = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == lr.COLLATERALCODE).FirstOrDefault().COLLATERALSUMMARY,
                                             Branch = br.BRANCHNAME,
                                             Region = "",
                                             StatusOfPerction = "",
                                             DateOfValuation = DateTime.Now,
                                             OMV = 0.00,
                                             FSV = 0.00,
                                             NBV = "",
                                             AmountSold = 0.00,
                                             PAndLImpact = 0.00,
                                             DateSold = "",
                                         }).ToList();


                var groupRecord = dataLoan.Union(dataRevolvingLoan);
                return groupRecord;

            }

        }
        public IEnumerable<RemedialAssetReportViewModel> FullAndFinalSettlementAndWaivers(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from l in context.TBL_LOAN_REVIEW_OPERATION
                                join ln in context.TBL_LOAN on l.LOANID equals ln.TERMLOANID
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                where (DbFunctions.TruncateTime(l.DATECREATED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(l.DATECREATED) <= DbFunctions.TruncateTime(endDate))
                                && l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && l.OPERATIONTYPEID == (int)OperationsEnum.FullAndFinalCompleteWriteOff

                                select new RemedialAssetReportViewModel
                                {
                                    AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                                    AccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNAME,
                                    FacilityType = pr.PRODUCTNAME,
                                    CollateralDescription = "",
                                    FullAndFinalAmountApproved = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == l.LOANREVIEWAPPLICATIONID).FirstOrDefault().APPROVEDAMOUNT,
                                    TotalExposure = context.TBL_GLOBAL_EXPOSURE.Where(e=>e.CUSTOMERID == cu.CUSTOMERID.ToString()).Sum(e=>e.TOTALEXPOSURE),
                                    Classification = "",
                                    Provision = "",
                                    ResidualBalance = 0.00,
                                    MonthlyExpectedPayment = 0.00,
                                    maturityDate = l.MATURITYDATE,
                                    effectiveDate = l.EFFECTIVEDATE,
                                    AgeOfLastCredit = 0,
                                }).ToList();

                var dataRevolvingLoan = (from l in context.TBL_LOAN_REVIEW_OPERATION
                                         join ln in context.TBL_LOAN_REVOLVING on l.LOANID equals ln.REVOLVINGLOANID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where (DbFunctions.TruncateTime(l.DATECREATED) >= DbFunctions.TruncateTime(startDate)
                                             && DbFunctions.TruncateTime(l.DATECREATED) <= DbFunctions.TruncateTime(endDate))
                                             && l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && l.OPERATIONTYPEID == (int)OperationsEnum.FullAndFinalCompleteWriteOff

                                         select new RemedialAssetReportViewModel
                                         {
                                             AccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNUMBER,
                                             AccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).FirstOrDefault().PRODUCTACCOUNTNAME,
                                             FacilityType = pr.PRODUCTNAME,
                                             CollateralDescription = "",
                                             FullAndFinalAmountApproved = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == l.LOANREVIEWAPPLICATIONID).FirstOrDefault().APPROVEDAMOUNT,
                                             TotalExposure = context.TBL_GLOBAL_EXPOSURE.Where(e => e.CUSTOMERID == cu.CUSTOMERID.ToString()).Sum(e => e.TOTALEXPOSURE),
                                             Classification = "",
                                             Provision = "",
                                             ResidualBalance = 0.00,
                                             MonthlyExpectedPayment = 0.00,
                                             maturityDate = l.MATURITYDATE,
                                             effectiveDate = l.EFFECTIVEDATE,
                                             AgeOfLastCredit = 0,
                                         }).ToList();


                var groupRecord = dataLoan.Union(dataRevolvingLoan);
                return groupRecord;

            }

        }
    }
}
