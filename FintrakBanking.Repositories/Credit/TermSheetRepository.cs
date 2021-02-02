using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    public class TermSheetRepository : ITermSheetRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public TermSheetRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<TermSheetViewModel> GetTermSheets( int staffId)
        {
            return context.TBL_TERM_SHEET.Where(x => x.DELETED == false)
                .Select(x => new TermSheetViewModel
                {
                    termSheetId = x.TERMSHEETID,
                    termSheetCode = x.TERMSHEETCODE,
                    borrower = x.BORROWER,
                    facilityAmount = x.FACILITYAMOUNT,
                    facilityType = x.FACILITYTYPE,
                    purpose = x.PURPOSE,
                    tenor = x.TENOR,
                    permittedAccount = x.PERMITTEDACCOUNT,
                    debtServiceReserveAccount = x.DEBTSERVICERESERVEACCOUNT,
                    cancellation = x.CANCELLATION,
                    principalRepayment = x.PRINCIPALREPAYMENT,
                    interestPayment = x.INTERESTPAYMENT,
                    computationOfInterest = x.COMPUTATIONOFINTEREST,
                    repaymentSource = x.REPAYMENTSOURCE,
                    availability = x.AVAILABILITY,
                    currencyOfDisbursement = x.CURRENCYOFDISBURSEMENT,
                    documentation = x.DOCUMENTATION,
                    drawdown = x.DRAWDOWN,
                    earlyRepaymentOfPrincipal = x.EARLYREPAYMENTOFPRINCIPAL,
                    interestRate = x.INTERESTRATE,
                    pricing = x.PRICING,
                    managementFees = x.MANAGEMENTFEES,
                    facilityFee = x.FACILITYFEE,
                    processingFee = x.PROCESSINGFEE,
                    securityCondition = x.SECURITYCONDITION,
                    transactionDynamics = x.TRANSACTIONDYNAMICS,
                    conditionsPrecedentToUtilisation = x.CONDITIONSPRECEDENTTOUTILISATION,
                    otherCondition = x.OTHERCONDITION,
                    taxes = x.TAXES,
                    presentationsAndWarrantees = x.PRESENTATIONSANDWARRANTEES,
                    covenants = x.COVENANTS,
                    eventsOfDefault = x.EVENTSOFDEFAULT,
                    transferability = x.TRANSFERABILITY,
                    governingLawAndJurisdiction = x.GOVERNINGLAWANDJURISDICTION,
                    owner = x.CREATEDBY == staffId ? true : false
                })
                .ToList();
        }

        public IEnumerable<LookupViewModel> GetCustomerTermSheets(int customerId)
        {
            var data = (from t in context.TBL_TERM_SHEET
                       where t.CUSTOMERID == customerId
                       select new LookupViewModel()
                       {
                           lookupId = (short)t.TERMSHEETID,
                           lookupName = t.TERMSHEETCODE
                       }).ToList();
            return data;
        }


        public IEnumerable<LookupViewModel> GetCustomerTermSheetsCorrection()
        {
            var data = (from t in context.TBL_TERM_SHEET
                        where t.DELETED == false
                        select new LookupViewModel()
                        {
                            lookupId = (short)t.TERMSHEETID,
                            lookupName = t.TERMSHEETCODE
                        }).ToList();
            return data;
        }

        public IEnumerable<TermSheetViewModel> GetCustomerTermSheetsByCode(int termSheetCode)
        {
            var data = context.TBL_TERM_SHEET.Where(x => x.DELETED == false && x.TERMSHEETID == termSheetCode)
                .Select(x => new TermSheetViewModel
                {
                    termSheetId = x.TERMSHEETID,
                    termSheetCode = x.TERMSHEETCODE,
                    borrower = x.BORROWER,
                    facilityAmount = x.FACILITYAMOUNT,
                    facilityType = x.FACILITYTYPE,
                    purpose = x.PURPOSE,
                    tenor = x.TENOR,
                    permittedAccount = x.PERMITTEDACCOUNT,
                    debtServiceReserveAccount = x.DEBTSERVICERESERVEACCOUNT,
                    cancellation = x.CANCELLATION,
                    principalRepayment = x.PRINCIPALREPAYMENT,
                    interestPayment = x.INTERESTPAYMENT,
                    computationOfInterest = x.COMPUTATIONOFINTEREST,
                    repaymentSource = x.REPAYMENTSOURCE,
                    availability = x.AVAILABILITY,
                    currencyOfDisbursement = x.CURRENCYOFDISBURSEMENT,
                    documentation = x.DOCUMENTATION,
                    drawdown = x.DRAWDOWN,
                    earlyRepaymentOfPrincipal = x.EARLYREPAYMENTOFPRINCIPAL,
                    interestRate = x.INTERESTRATE,
                    pricing = x.PRICING,
                    managementFees = x.MANAGEMENTFEES,
                    facilityFee = x.FACILITYFEE,
                    processingFee = x.PROCESSINGFEE,
                    securityCondition = x.SECURITYCONDITION,
                    transactionDynamics = x.TRANSACTIONDYNAMICS,
                    conditionsPrecedentToUtilisation = x.CONDITIONSPRECEDENTTOUTILISATION,
                    otherCondition = x.OTHERCONDITION,
                    taxes = x.TAXES,
                    presentationsAndWarrantees = x.PRESENTATIONSANDWARRANTEES,
                    covenants = x.COVENANTS,
                    eventsOfDefault = x.EVENTSOFDEFAULT,
                    transferability = x.TRANSFERABILITY,
                    governingLawAndJurisdiction = x.GOVERNINGLAWANDJURISDICTION,
                }).ToList();

            return data;
        }

        public TermSheetViewModel GetTermSheet(int id)
        {
            var entity = context.TBL_TERM_SHEET.FirstOrDefault(x => x.TERMSHEETID == id && x.DELETED == false);

            return new TermSheetViewModel
            {
                termSheetId = entity.TERMSHEETID,
                borrower = entity.BORROWER,
                facilityAmount = entity.FACILITYAMOUNT,
                facilityType = entity.FACILITYTYPE,
                purpose = entity.PURPOSE,
                tenor = entity.TENOR,
                permittedAccount = entity.PERMITTEDACCOUNT,
                debtServiceReserveAccount = entity.DEBTSERVICERESERVEACCOUNT,
                cancellation = entity.CANCELLATION,
                principalRepayment = entity.PRINCIPALREPAYMENT,
                interestPayment = entity.INTERESTPAYMENT,
                computationOfInterest = entity.COMPUTATIONOFINTEREST,
                repaymentSource = entity.REPAYMENTSOURCE,
                availability = entity.AVAILABILITY,
                currencyOfDisbursement = entity.CURRENCYOFDISBURSEMENT,
                documentation = entity.DOCUMENTATION,
                drawdown = entity.DRAWDOWN,
                earlyRepaymentOfPrincipal = entity.EARLYREPAYMENTOFPRINCIPAL,
                interestRate = entity.INTERESTRATE,
                pricing = entity.PRICING,
                managementFees = entity.MANAGEMENTFEES,
                facilityFee = entity.FACILITYFEE,
                processingFee = entity.PROCESSINGFEE,
                securityCondition = entity.SECURITYCONDITION,
                transactionDynamics = entity.TRANSACTIONDYNAMICS,
                conditionsPrecedentToUtilisation = entity.CONDITIONSPRECEDENTTOUTILISATION,
                otherCondition = entity.OTHERCONDITION,
                taxes = entity.TAXES,
                presentationsAndWarrantees = entity.PRESENTATIONSANDWARRANTEES,
                covenants = entity.COVENANTS,
                eventsOfDefault = entity.EVENTSOFDEFAULT,
                transferability = entity.TRANSFERABILITY,
                governingLawAndJurisdiction = entity.GOVERNINGLAWANDJURISDICTION,
            };
        }

        public bool AddTermSheet(TermSheetViewModel model)
        {
            var entity = new TBL_TERM_SHEET
            {
                BORROWER = model.borrower,
                FACILITYAMOUNT = model.facilityAmount,
                FACILITYTYPE = model.facilityType,
                PURPOSE = model.purpose,
                TENOR = model.tenor,
                PERMITTEDACCOUNT = model.permittedAccount,
                DEBTSERVICERESERVEACCOUNT = model.debtServiceReserveAccount,
                CANCELLATION = model.cancellation,
                PRINCIPALREPAYMENT = model.principalRepayment,
                INTERESTPAYMENT = model.interestPayment,
                COMPUTATIONOFINTEREST = model.computationOfInterest,
                REPAYMENTSOURCE = model.repaymentSource,
                AVAILABILITY = model.availability,
                CURRENCYOFDISBURSEMENT = model.currencyOfDisbursement,
                DOCUMENTATION = model.documentation,
                DRAWDOWN = model.drawdown,
                EARLYREPAYMENTOFPRINCIPAL = model.earlyRepaymentOfPrincipal,
                INTERESTRATE = model.interestRate,
                PRICING = model.pricing,
                MANAGEMENTFEES = model.managementFees,
                FACILITYFEE = model.facilityFee,
                PROCESSINGFEE = model.processingFee,
                SECURITYCONDITION = model.securityCondition,
                TRANSACTIONDYNAMICS = model.transactionDynamics,
                CONDITIONSPRECEDENTTOUTILISATION = model.conditionsPrecedentToUtilisation,
                OTHERCONDITION = model.otherCondition,
                TAXES = model.taxes,
                PRESENTATIONSANDWARRANTEES = model.presentationsAndWarrantees,
                COVENANTS = model.covenants,
                EVENTSOFDEFAULT = model.eventsOfDefault,
                TRANSFERABILITY = model.transferability,
                GOVERNINGLAWANDJURISDICTION = model.governingLawAndJurisdiction,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                
            };

            var refNumber = CommonHelpers.GenerateRandomDigitCode(10);
            entity.TERMSHEETCODE = refNumber;
            context.TBL_TERM_SHEET.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TermSheetAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Term Sheet '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateTermSheet(TermSheetViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_TERM_SHEET.Find(id);
            entity.BORROWER = model.borrower;
            entity.FACILITYAMOUNT = model.facilityAmount;
            entity.FACILITYTYPE = model.facilityType;
            entity.PURPOSE = model.purpose;
            entity.TENOR = model.tenor;
            entity.PERMITTEDACCOUNT = model.permittedAccount;
            entity.DEBTSERVICERESERVEACCOUNT = model.debtServiceReserveAccount;
            entity.CANCELLATION = model.cancellation;
            entity.PRINCIPALREPAYMENT = model.principalRepayment;
            entity.INTERESTPAYMENT = model.interestPayment;
            entity.COMPUTATIONOFINTEREST = model.computationOfInterest;
            entity.REPAYMENTSOURCE = model.repaymentSource;
            entity.AVAILABILITY = model.availability;
            entity.CURRENCYOFDISBURSEMENT = model.currencyOfDisbursement;
            entity.DOCUMENTATION = model.documentation;
            entity.DRAWDOWN = model.drawdown;
            entity.EARLYREPAYMENTOFPRINCIPAL = model.earlyRepaymentOfPrincipal;
            entity.INTERESTRATE = model.interestRate;
            entity.PRICING = model.pricing;
            entity.MANAGEMENTFEES = model.managementFees;
            entity.FACILITYFEE = model.facilityFee;
            entity.PROCESSINGFEE = model.processingFee;
            entity.SECURITYCONDITION = model.securityCondition;
            entity.TRANSACTIONDYNAMICS = model.transactionDynamics;
            entity.CONDITIONSPRECEDENTTOUTILISATION = model.conditionsPrecedentToUtilisation;
            entity.OTHERCONDITION = model.otherCondition;
            entity.TAXES = model.taxes;
            entity.PRESENTATIONSANDWARRANTEES = model.presentationsAndWarrantees;
            entity.COVENANTS = model.covenants;
            entity.EVENTSOFDEFAULT = model.eventsOfDefault;
            entity.TRANSFERABILITY = model.transferability;
            entity.GOVERNINGLAWANDJURISDICTION = model.governingLawAndJurisdiction;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TermSheetUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Term Sheet '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.TERMSHEETID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteTermSheet(int id, UserInfo user)
        {
            var entity = this.context.TBL_TERM_SHEET.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TermSheetDeleted, //still missing its value
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Term Sheet '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.TERMSHEETID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

    }
}

// kernel.Bind<ITermSheetRepository>().To<TermSheetRepository>();
// TermSheetAdded = ???, TermSheetUpdated = ???, TermSheetDeleted = ???,
