using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Repositories.credit
{
    public class LcUssanceRepository : ILcUssanceRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IFinanceTransactionRepository fina;
        private IWorkflow workflow;

        public LcUssanceRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IFinanceTransactionRepository _fina,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.fina = _fina;
            this.workflow = _workflow;
        }


        public IEnumerable<LcUssanceViewModel> GetLcUssances()
        {
            return context.TBL_LC_USSANCE.Where(x => x.DELETED == false)
                .Select(x => new LcUssanceViewModel
                {
                    lcIssuanceId = x.LCISSUANCEID,
                    lcUssanceId = x.LCUSSANCEID,
                    ussanceAmount = x.USSANCEAMOUNT,
                    ussanceRate = x.USSANCERATE,
                    ussanceTenor = x.USSANCETENOR,
                    lcEffectiveDate = x.LCUSSANCEEFFECTIVEDATE,
                    lcMaturityDate = x.LCUSSANCEMATURITYDATE,
                    usanceAmountCurrencyId = x.USANCEAMOUNTCURRENCYID
                })
                .ToList();
        }

        public List<LcUssanceViewModel> GetLcUssanceExtensionsByLcUsanceId(int lcUsanceId)
        {
            var entity = (from u in context.TBL_LC_USSANCE
                          join ux in context.TBL_TEMP_LC_USSANCE on u.LCUSSANCEID equals ux.LCUSSANCEID
                          where u.LCUSSANCEID == lcUsanceId
                          && u.DELETED == false
                          select new LcUssanceViewModel
                          {
                              lcIssuanceId = u.LCISSUANCEID,
                              lcUssanceId = ux.LCUSSANCEID,
                              tempLcUsanceId = ux.TEMPLCUSSANCEID,
                              ussanceAmount = u.USSANCEAMOUNT,
                              ussanceRate = u.USSANCERATE,
                              oldUssanceTenor = ux.OLDUSSANCETENOR,
                              ussanceTenor = ux.NEWUSSANCETENOR,
                              lcEffectiveDate = u.LCUSSANCEEFFECTIVEDATE,
                              oldLcMaturityDate = ux.OLDLCUSSANCEMATURITYDATE,
                              lcMaturityDate = ux.NEWLCUSSANCEMATURITYDATE,
                              usanceAmountCurrencyId = u.USANCEAMOUNTCURRENCYID,
                              //comments = (from t in context.TBL_APPROVAL_TRAIL
                              //           where ux.TEMPLCUSSANCEID == t.TARGETID
                              //           && t.OPERATIONID == (int)OperationsEnum.LCIssuanceExtensionApproval
                              //            select new ApprovalTrailViewModel
                              //            {
                              //                approvalTrailId = t.APPROVALTRAILID,
                              //                comment = t.COMMENT,
                              //                targetId = t.TARGETID,
                              //                operationId = t.OPERATIONID,
                              //                arrivalDate = t.ARRIVALDATE,
                              //                systemArrivalDateTime = t.SYSTEMARRIVALDATETIME,
                              //                responseDate = t.RESPONSEDATE,
                              //                systemResponseDateTime = t.SYSTEMRESPONSEDATETIME,
                              //                responseStaffId = t.RESPONSESTAFFID,
                              //                requestStaffId = t.REQUESTSTAFFID,
                              //                loopedStaffId = t.LOOPEDSTAFFID,
                              //                toStaffId = t.TOSTAFFID,
                              //                fromApprovalLevelId = t.FROMAPPROVALLEVELID,
                              //                fromApprovalLevelName = t.FROMAPPROVALLEVELID == null ? t.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME : t.TBL_APPROVAL_LEVEL.LEVELNAME,
                              //                toApprovalLevelName = t.TOAPPROVALLEVELID == null ? "N/A" : t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                              //                toApprovalLevelId = t.TOAPPROVALLEVELID,
                              //                approvalStateId = t.APPROVALSTATEID,
                              //                approvalStatusId = t.APPROVALSTATUSID,
                              //                approvalState = t.TBL_APPROVAL_STATE.APPROVALSTATE,
                              //                approvalStatus = t.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                              //                fromStaffName = t.TBL_STAFF.LASTNAME + " " + t.TBL_STAFF.MIDDLENAME + " " + t.TBL_STAFF.FIRSTNAME,
                              //                toStaffName = t.TBL_STAFF1.LASTNAME + " " + t.TBL_STAFF1.MIDDLENAME + " " + t.TBL_STAFF1.FIRSTNAME,
                              //            }).OrderByDescending(x => x.approvalTrailId).ToList(),
                          }).ToList();

            return entity;
        }

        public LcUssanceViewModel GetLcUssanceExtensionByTempLcUsanceId(int tempLcUsanceId)
        {
            var entity = (from ux in context.TBL_TEMP_LC_USSANCE
                          join u in context.TBL_LC_USSANCE on ux.LCUSSANCEID equals u.LCUSSANCEID
                          where ux.TEMPLCUSSANCEID == tempLcUsanceId
                          && u.DELETED == false
                          select new LcUssanceViewModel
                          {
                              lcIssuanceId = u.LCISSUANCEID,
                              tempLcUsanceId = ux.TEMPLCUSSANCEID,
                              lcUssanceId = ux.LCUSSANCEID,
                              ussanceAmount = u.USSANCEAMOUNT,
                              ussanceRate = u.USSANCERATE,
                              oldUssanceTenor = ux.OLDUSSANCETENOR,
                              ussanceTenor = ux.NEWUSSANCETENOR,
                              lcEffectiveDate = u.LCUSSANCEEFFECTIVEDATE,
                              oldLcMaturityDate = ux.OLDLCUSSANCEMATURITYDATE,
                              lcMaturityDate = ux.NEWLCUSSANCEMATURITYDATE,
                              usanceAmountCurrencyId = u.USANCEAMOUNTCURRENCYID
                          }).FirstOrDefault();

            if (entity != null)
            {
                return entity;
            }

            return null;
        }

        public LcUssanceViewModel GetLcUssanceByLCUsanceId(int lcUsanceId)
        {
            var entity = context.TBL_LC_USSANCE.FirstOrDefault(x => x.LCUSSANCEID == lcUsanceId && x.DELETED == false);

            if (entity != null)
            {
                return new LcUssanceViewModel
                {
                    lcIssuanceId = entity.LCISSUANCEID,
                    lcUssanceId = entity.LCUSSANCEID,
                    ussanceAmount = entity.USSANCEAMOUNT,
                    ussanceRate = entity.USSANCERATE,
                    ussanceTenor = entity.USSANCETENOR,
                    oldUssanceTenor = entity.USSANCETENOR,
                    lcEffectiveDate = entity.LCUSSANCEEFFECTIVEDATE,
                    lcMaturityDate = entity.LCUSSANCEMATURITYDATE,
                    oldLcMaturityDate = entity.LCUSSANCEMATURITYDATE,
                    usanceAmountCurrencyId = entity.USANCEAMOUNTCURRENCYID,
                };
            }

            return null;
        }

        public List<LcUssanceViewModel> GetLcUssanceByLCIssuanceId(int lcIssuanceId)
        {
            return context.TBL_LC_USSANCE.Where(x => x.LCISSUANCEID == lcIssuanceId && x.DELETED == false)
                .Select(entity => new LcUssanceViewModel
                {
                    lcIssuanceId = entity.LCISSUANCEID,
                    lcUssanceId = entity.LCUSSANCEID,
                    ussanceAmount = entity.USSANCEAMOUNT,
                    ussanceRate = entity.USSANCERATE,
                    ussanceTenor = entity.USSANCETENOR,
                    lcEffectiveDate = entity.LCUSSANCEEFFECTIVEDATE,
                    lcMaturityDate = entity.LCUSSANCEMATURITYDATE,
                    usanceAmountCurrencyId = entity.USANCEAMOUNTCURRENCYID
                }).ToList();
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceExtension(int staffId)
        {
            var staffs = general.GetStaffRlieved(staffId).ToList();
            var lcsInProgress = (from x in context.TBL_LC_ISSUANCE
                                 join u in context.TBL_LC_USSANCE on x.LCISSUANCEID equals u.LCISSUANCEID
                                 join ux in context.TBL_TEMP_LC_USSANCE on u.LCUSSANCEID equals ux.LCUSSANCEID
                                 join t in context.TBL_APPROVAL_TRAIL on ux.TEMPLCUSSANCEID equals t.TARGETID into uxt
                                 from utrail in uxt.DefaultIfEmpty()
                                 where
                                (
                                x.DELETED == false
                                && utrail.RESPONSESTAFFID == null
                                && (utrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved || (utrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && staffs.Contains(utrail.LOOPEDSTAFFID ?? 0)))
                                && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && utrail.OPERATIONID == (int)OperationsEnum.LCUsanceExtensionApproval
                                && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress
                                && ux.USANCEEXTENSIONAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress
                                )
                                 select new LcIssuanceApprovalViewModel()
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     lcUssanceId = u.LCUSSANCEID,
                                     tempLcUsanceId = ux.TEMPLCUSSANCEID,
                                     lcApprovalTrailId = utrail.APPROVALTRAILID,
                                     approvalStatusId = utrail.APPROVALSTATUSID,
                                     loopedStaffId = utrail.LOOPEDSTAFFID,
                                     beneficiaryName = x.BENEFICIARYNAME,
                                     totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = x.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = x.LCTOLERANCEVALUE,
                                     totalUsanceAmountLocal = x.TOTALUSANCEAMOUNTLOCAL,
                                     //totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                     releaseAmount = x.RELEASEDAMOUNT,
                                     letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                     isDraftRequired = x.ISDRAFTREQUIRED,
                                     beneficiaryAddress = x.BENEFICIARYADDRESS,
                                     beneficiaryEmail = x.BENEFICIARYEMAIL,
                                     customerId = x.CUSTOMERID,
                                     customerName = x.TBL_CUSTOMER.FIRSTNAME + x.TBL_CUSTOMER.MIDDLENAME + x.TBL_CUSTOMER.LASTNAME,
                                     fundSourceId = x.FUNDSOURCEID,
                                     fundSourceDetails = x.FUNDSOURCEDETAILS,
                                     formMNumber = x.FORMMNUMBER,
                                     beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = x.BENEFICIARYBANK,
                                     currencyId = x.CURRENCYID,
                                     proformaInvoiceId = x.PROFORMAINVOICEID,
                                     availableAmount = x.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = x.INVOICEDATE,
                                     invoiceDueDate = x.INVOICEDUEDATE,
                                     lcReferenceNumber = x.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)x.DATETIMECREATED
                                 })
                                // .GroupBy(l => l.tempLcUsanceId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                                //.Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                //|| (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                //&& staffs.Contains(l.loopedStaffId ?? 0)))
                                .ToList();

            var lcsNotStarted = (from x in context.TBL_LC_ISSUANCE
                                 join u in context.TBL_LC_USSANCE on x.LCISSUANCEID equals u.LCISSUANCEID
                                 join ux in context.TBL_TEMP_LC_USSANCE on u.LCUSSANCEID equals ux.LCUSSANCEID into uxy
                                 from ux in uxy.DefaultIfEmpty()
                                 let ongoingUsExtExists = (ux.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending && ux != null)
                                 where
                                (
                                x.DELETED == false
                                && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && staffs.Contains(u.CREATEDBY ?? 0)
                                && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted
                                && (staffs.Contains(ux.CREATEDBY) || ux == null)
                                && (ongoingUsExtExists || ux == null || !ongoingUsExtExists)
                                )
                                 select new LcIssuanceApprovalViewModel()
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     lcUssanceId = u.LCUSSANCEID,
                                     tempLcUsanceId = ongoingUsExtExists ? ux.TEMPLCUSSANCEID : 0,
                                     beneficiaryName = x.BENEFICIARYNAME,
                                     totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = x.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = x.LCTOLERANCEVALUE,
                                     totalUsanceAmountLocal = x.TOTALUSANCEAMOUNTLOCAL,
                                     //totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                     releaseAmount = x.RELEASEDAMOUNT,
                                     letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                     isDraftRequired = x.ISDRAFTREQUIRED,
                                     beneficiaryAddress = x.BENEFICIARYADDRESS,
                                     beneficiaryEmail = x.BENEFICIARYEMAIL,
                                     customerId = x.CUSTOMERID,
                                     customerName = x.TBL_CUSTOMER.FIRSTNAME + x.TBL_CUSTOMER.MIDDLENAME + x.TBL_CUSTOMER.LASTNAME,
                                     fundSourceId = x.FUNDSOURCEID,
                                     fundSourceDetails = x.FUNDSOURCEDETAILS,
                                     formMNumber = x.FORMMNUMBER,
                                     beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = x.BENEFICIARYBANK,
                                     currencyId = x.CURRENCYID,
                                     proformaInvoiceId = x.PROFORMAINVOICEID,
                                     availableAmount = x.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = x.INVOICEDATE,
                                     invoiceDueDate = x.INVOICEDUEDATE,
                                     lcReferenceNumber = x.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)x.DATETIMECREATED
                                 })
                           .ToList();
            var lcs = lcsNotStarted.Union(lcsInProgress);
            lcs = lcs.Distinct().ToList();
            var companyId = context.TBL_COMPANY.FirstOrDefault().COMPANYID;
            foreach (var u in lcs)
            {
                var lcCurrRate = fina.GetExchangeRate(DateTime.Now, (short)u.currencyId, companyId);
                var lcTotalAmountAvailableForUsance = u.lcToleranceValue * (decimal)lcCurrRate.sellingRate;
                u.totalUsanceAmount = (lcTotalAmountAvailableForUsance - u.totalUsanceAmountLocal);
            }
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssance(int staffId)
        {
            var staffs = general.GetStaffRlieved(staffId).ToList();
            var lcsInProgress = (from x in context.TBL_LC_ISSUANCE
                       join u in context.TBL_LC_USSANCE on x.LCISSUANCEID equals u.LCISSUANCEID
                       join t in context.TBL_APPROVAL_TRAIL on u.LCUSSANCEID equals t.TARGETID into ut
                       from utrail in ut.DefaultIfEmpty() where 
                       (
                       x.DELETED == false
                       && utrail.OPERATIONID == (int)OperationsEnum.lcUssance
                       && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
                       )
                        select new LcIssuanceApprovalViewModel()
                            {
                                lcIssuanceId = x.LCISSUANCEID,
                                lcUssanceId = u.LCUSSANCEID,
                                lcApprovalTrailId = utrail.APPROVALTRAILID,
                                approvalStatusId = utrail.APPROVALSTATUSID,
                                loopedStaffId = utrail.LOOPEDSTAFFID,
                                beneficiaryName = x.BENEFICIARYNAME,
                                totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                percentageToCover = x.PERCENTAGETOCOVER,
                                lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                lcToleranceValue = x.LCTOLERANCEVALUE,
                                totalUsanceAmountLocal = x.TOTALUSANCEAMOUNTLOCAL,
                                //totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                releaseAmount = x.RELEASEDAMOUNT,
                                letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                isDraftRequired = x.ISDRAFTREQUIRED,
                                beneficiaryAddress = x.BENEFICIARYADDRESS,
                                beneficiaryEmail = x.BENEFICIARYEMAIL,
                                customerId = x.CUSTOMERID,
                                customerName = x.TBL_CUSTOMER.FIRSTNAME + x.TBL_CUSTOMER.MIDDLENAME + x.TBL_CUSTOMER.LASTNAME,
                                fundSourceId = x.FUNDSOURCEID,
                                fundSourceDetails = x.FUNDSOURCEDETAILS,
                                formMNumber = x.FORMMNUMBER,
                                beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                beneficiaryBank = x.BENEFICIARYBANK,
                                currencyId = x.CURRENCYID,
                                proformaInvoiceId = x.PROFORMAINVOICEID,
                                availableAmount = x.AVAILABLEAMOUNT,
                                letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                invoiceDate = x.INVOICEDATE,
                                invoiceDueDate = x.INVOICEDUEDATE,
                                lcReferenceNumber = x.LCREFERENCENUMBER,
                                dateTimeCreated = (DateTime)x.DATETIMECREATED
                            }).GroupBy(l => l.lcUssanceId).Select(l => l.OrderByDescending(t => t.lcApprovalTrailId).FirstOrDefault())
                                .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var lcsNotStarted = (from x in context.TBL_LC_ISSUANCE
                                 join y in context.TBL_LC_USSANCE on x.LCISSUANCEID equals y.LCISSUANCEID into xy
                                 let lcTotalUsanceCompleted = (x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted)
                                 from u in xy.DefaultIfEmpty()
                                 let newUs = (u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted && !lcTotalUsanceCompleted)
                                 where
                                (
                                x.DELETED == false
                                && (staffs.Contains(u.CREATEDBY ?? 0) || u == null)
                                && x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                && 
                                (u.USANCEAPPLICATIONSTATUSID == null || u == null || newUs)
                                //&& (u.USANCEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.lcUssanceInProgress || u == null)
                                //&& (u.USANCEAPPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.lcUssanceCompleted || u == null)
                                )
                                 select new LcIssuanceApprovalViewModel()
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     lcUssanceId = newUs ? 0 : u.LCUSSANCEID,
                                     beneficiaryName = x.BENEFICIARYNAME,
                                     totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                     totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                     availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                     cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                     cashBuildUpReferenceNumber = x.CASHBUILDUPREFERENCETYPE,
                                     cashBuildUpReferenceType = x.CASHBUILDUPREFERENCENUMBER,
                                     percentageToCover = x.PERCENTAGETOCOVER,
                                     lcTolerancePercentage = x.LCTOLERANCEPERCENTAGE,
                                     lcToleranceValue = x.LCTOLERANCEVALUE,
                                     totalUsanceAmountLocal = x.TOTALUSANCEAMOUNTLOCAL,
                                     //totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                                     releaseAmount = x.RELEASEDAMOUNT,
                                     letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                     isDraftRequired = x.ISDRAFTREQUIRED,
                                     beneficiaryAddress = x.BENEFICIARYADDRESS,
                                     beneficiaryEmail = x.BENEFICIARYEMAIL,
                                     customerId = x.CUSTOMERID,
                                     customerName = x.TBL_CUSTOMER.FIRSTNAME + x.TBL_CUSTOMER.MIDDLENAME + x.TBL_CUSTOMER.LASTNAME,
                                     fundSourceId = x.FUNDSOURCEID,
                                     fundSourceDetails = x.FUNDSOURCEDETAILS,
                                     formMNumber = x.FORMMNUMBER,
                                     beneficiaryPhoneNumber = x.BENEFICIARYPHONENUMBER,
                                     beneficiaryBank = x.BENEFICIARYBANK,
                                     currencyId = x.CURRENCYID,
                                     proformaInvoiceId = x.PROFORMAINVOICEID,
                                     availableAmount = x.AVAILABLEAMOUNT,
                                     letterOfCreditAmount = x.LETTEROFCREDITAMOUNT,
                                     letterOfcreditExpirydate = x.LETTEROFCREDITEXPIRYDATE,
                                     invoiceDate = x.INVOICEDATE,
                                     invoiceDueDate = x.INVOICEDUEDATE,
                                     lcReferenceNumber = x.LCREFERENCENUMBER,
                                     dateTimeCreated = (DateTime)x.DATETIMECREATED
                                 })
                           .ToList();
            var lcs = lcsNotStarted.Union(lcsInProgress);
            lcs = lcs.Distinct();
            var companyId = context.TBL_COMPANY.FirstOrDefault().COMPANYID;
            foreach (var u in lcs)
            {
                var lcCurrRate = fina.GetExchangeRate(DateTime.Now, (short)u.currencyId, companyId);
                var lcTotalAmountAvailableForUsance = u.lcToleranceValue * (decimal)lcCurrRate.sellingRate;
                u.totalUsanceAmount = (lcTotalAmountAvailableForUsance - u.totalUsanceAmountLocal);
            }
            return lcs.ToList();
        }

        public LcUssanceViewModel AddLcUssance(LcUssanceViewModel model)
        {
            model = ValidateUsanceAmount(model);
            var reference = CommonHelpers.GenerateRandomDigitCode(10);
            var entity = new TBL_LC_USSANCE
            {
                LCISSUANCEID = model.lcIssuanceId,
                USSANCEAMOUNT = model.ussanceAmount,
                USSANCEAMOUNTLOCAL = model.ussanceAmountLocal,
                USSANCERATE = model.ussanceRate,
                USSANCETENOR = model.ussanceTenor,
                LCUSSANCEEFFECTIVEDATE = model.lcEffectiveDate,
                LCUSSANCEMATURITYDATE = model.lcMaturityDate,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                USANCEAMOUNTCURRENCYID = model.usanceAmountCurrencyId,
                USANCEREF = reference
            };

            context.TBL_LC_USSANCE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcUsanceAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_LC_USSANCE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------
            context.SaveChanges();
            var createdlcUssance = context.TBL_LC_USSANCE.FirstOrDefault(lc => lc.USANCEREF == reference);
            model.lcUssanceId = createdlcUssance.LCUSSANCEID;
            return model;
        }

        public LcUssanceViewModel AddLcUssanceExtension(LcUssanceViewModel model)
        {
            var reference = CommonHelpers.GenerateRandomDigitCode(10);
            var us = context.TBL_LC_USSANCE.Find(model.lcUssanceId);
            if (us == null)
            {
                throw new SecureException("LC Usance for Extension cannot be null!");
            }
            var entity = new TBL_TEMP_LC_USSANCE
            {
                LCISSUANCEID = model.lcIssuanceId,
                LCUSSANCEID = model.lcUssanceId,
                OLDUSSANCETENOR = (int)us.USSANCETENOR,
                NEWUSSANCETENOR = (int)model.ussanceTenor,
                OLDLCUSSANCEMATURITYDATE = (DateTime)us.LCUSSANCEMATURITYDATE,
                NEWLCUSSANCEMATURITYDATE = (DateTime)model.lcMaturityDate,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                EXTENSIONREFNUMBER = reference
            };

            context.TBL_TEMP_LC_USSANCE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcUsanceExtensionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_LC_USSANCE EXTENSION '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------
            context.SaveChanges();
            var createdlcUssance = context.TBL_TEMP_LC_USSANCE.FirstOrDefault(lc => lc.EXTENSIONREFNUMBER == reference);
            model.tempLcUsanceId = createdlcUssance.TEMPLCUSSANCEID;
            return model;
        }

        private LcUssanceViewModel ValidateUsanceAmount(LcUssanceViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var company = context.TBL_COMPANY.FirstOrDefault();
            //var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT);
            var lcToleranceCurrencyRate = fina.GetExchangeRate(DateTime.Now, (short)lc.CURRENCYID, company.COMPANYID);
            var availableAmount = lc.LCTOLERANCEVALUE * (decimal)lcToleranceCurrencyRate.sellingRate;
            var lcAmountCurrencyRate = fina.GetExchangeRate(DateTime.Now, (short)model.usanceAmountCurrencyId, company.COMPANYID);
            var lcUssanceAmount = (decimal)lcAmountCurrencyRate.sellingRate * model.ussanceAmount;

            if (lcUssanceAmount > availableAmount)
            {
                throw new SecureException("Usance Amount cannot be greater than tolerance amount " + availableAmount);
            }
            model.ussanceAmountLocal = lcUssanceAmount;
            return model;
        }

        public bool UpdateLcUsanceExtension(LcUssanceViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_TEMP_LC_USSANCE.Find(id);
            entity.NEWUSSANCETENOR = (int)model.ussanceTenor;
            entity.NEWLCUSSANCEMATURITYDATE = (DateTime)model.lcMaturityDate;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcUsanceExtensionUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LC_USSANCE EXTENSION'{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCUSSANCEID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcUssance(LcUssanceViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_USSANCE.Find(id);
            //entity.LCISSUANCEID = model.lcIssuanceId;
            //entity.LCUSSANCEID = model.lcUssanceId;
            model = ValidateUsanceAmount(model);
            entity.USSANCEAMOUNT = model.ussanceAmount;
            entity.USSANCEAMOUNTLOCAL = model.ussanceAmountLocal;
            entity.USSANCERATE = model.ussanceRate;
            entity.USSANCETENOR = model.ussanceTenor;
            entity.LCUSSANCEEFFECTIVEDATE = model.lcEffectiveDate;
            entity.LCUSSANCEMATURITYDATE = model.lcMaturityDate;
            // COMPANYID = model.companyId,
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;
            entity.USANCEAMOUNTCURRENCYID = model.usanceAmountCurrencyId;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LC_USSANCE '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCUSSANCEID,
                  DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
             
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcUssance(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_USSANCE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LC_USSANCE '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LCUSSANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceExtensionApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.LCUsanceExtensionApproval;
            //IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            //var querytest1 = (from a in context.TBL_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false
            //                    && a.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
            //                    && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();

            var query = (from a in context.TBL_LC_ISSUANCE
                         join u in context.TBL_LC_USSANCE on a.LCISSUANCEID equals u.LCISSUANCEID
                         join ux in context.TBL_TEMP_LC_USSANCE on u.LCUSSANCEID equals ux.LCUSSANCEID
                         join ut in context.TBL_APPROVAL_TRAIL on ux.TEMPLCUSSANCEID equals ut.TARGETID
                         where
                            (
                            (ut.OPERATIONID == operationId)
                            && a.DELETED == false
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress
                            && ux.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            && ut.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ut.RESPONSESTAFFID == null
                            && levelIds.Contains((int)ut.TOAPPROVALLEVELID)
                            && ut.LOOPEDSTAFFID == null
                            && (ut.TOSTAFFID == null || staffs.Contains(ut.TOSTAFFID ?? 0))
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcUssanceId = u.LCUSSANCEID,
                             tempLcUsanceId = ux.TEMPLCUSSANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalUsanceAmountLocal = a.TOTALUSANCEAMOUNTLOCAL,
                             //totalUsanceAmount = a.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == a.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = (string)a.CASHBUILDUPREFERENCETYPE,
                             cashBuildUpReferenceType = (string)a.CASHBUILDUPREFERENCENUMBER,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             releaseAmount = a.RELEASEDAMOUNT,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.MIDDLENAME + a.TBL_CUSTOMER.LASTNAME,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             lastComment = ut.COMMENT,
                             currentApprovalStateId = ut.APPROVALSTATEID,
                             currentApprovalLevelId = ut.TOAPPROVALLEVELID,
                             currentApprovalLevel = ut.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = ut.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = ut == null ? 0 : ut.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = ut.TOSTAFFID,
                             approvalStatusId = ut.APPROVALSTATUSID,
                             applicationStatusId = u.USANCEAPPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED,
                         }).ToList();

            var referredQuery = (from a in context.TBL_LC_ISSUANCE
                         join u in context.TBL_LC_USSANCE on a.LCISSUANCEID equals u.LCISSUANCEID
                         join ux in context.TBL_TEMP_LC_USSANCE on u.LCUSSANCEID equals ux.LCUSSANCEID
                         join ut in context.TBL_APPROVAL_TRAIL on ux.TEMPLCUSSANCEID equals ut.TARGETID
                         where
                            (
                            (ut.OPERATIONID == operationId)
                            && a.DELETED == false
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcUsanceExtensionInProgress
                            && ux.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                            && ut.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ut.RESPONSESTAFFID == null
                            && !levelIds.Contains((int)ut.TOAPPROVALLEVELID)
                            && (staffs.Contains(ut.LOOPEDSTAFFID ?? 0))
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcUssanceId = u.LCUSSANCEID,
                             tempLcUsanceId = ux.TEMPLCUSSANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalUsanceAmountLocal = a.TOTALUSANCEAMOUNTLOCAL,
                             //totalUsanceAmount = a.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == a.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = (string)a.CASHBUILDUPREFERENCETYPE,
                             cashBuildUpReferenceType = (string)a.CASHBUILDUPREFERENCENUMBER,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             releaseAmount = a.RELEASEDAMOUNT,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.MIDDLENAME + a.TBL_CUSTOMER.LASTNAME,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             lastComment = ut.COMMENT,
                             currentApprovalStateId = ut.APPROVALSTATEID,
                             currentApprovalLevelId = ut.TOAPPROVALLEVELID,
                             currentApprovalLevel = ut.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = ut.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = ut == null ? 0 : ut.APPROVALTRAILID, // for inner sequence ordering
                             loopedStaffId = ut.LOOPEDSTAFFID,
                             approvalStatusId = ut.APPROVALSTATUSID,
                             applicationStatusId = u.USANCEAPPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED,
                         }).ToList();

            var companyId = context.TBL_COMPANY.FirstOrDefault().COMPANYID;
            foreach (var u in query)
            {
                var lcCurrRate = fina.GetExchangeRate(DateTime.Now, (short)u.currencyId, companyId);
                var lcTotalAmountAvailableForUsance = u.lcToleranceValue * (decimal)lcCurrRate.sellingRate;
                u.totalUsanceAmount = (lcTotalAmountAvailableForUsance - u.totalUsanceAmountLocal);
            }
            //applications = query.AsQueryable()
            //    .Where(x => x.currentApprovalLevelTypeId != 2)
            //    .GroupBy(d => d.lcUssanceId)
            //    .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            //return applications.ToList();
            var lcs = query.Union(referredQuery);
            lcs = lcs.Distinct();
            return lcs;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcUssance;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = general.GetStaffRlieved(staffId).ToList();

            //var querytest1 = (from a in context.TBL_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false
            //                    && a.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
            //                    && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();

            var query = (from a in context.TBL_LC_ISSUANCE
                         join u in context.TBL_LC_USSANCE on a.LCISSUANCEID equals u.LCISSUANCEID
                         join ut in context.TBL_APPROVAL_TRAIL on u.LCUSSANCEID equals ut.TARGETID
                         where
                            (
                            (ut.OPERATIONID == operationId)
                            && a.DELETED == false
                            && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                            && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
                            && ut.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ut.RESPONSESTAFFID == null
                            && levelIds.Contains((int)ut.TOAPPROVALLEVELID)
                            && ut.LOOPEDSTAFFID == null
                            && (ut.TOSTAFFID == null || staffs.Contains(ut.TOSTAFFID ?? 0))
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcUssanceId = u.LCUSSANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalUsanceAmountLocal = a.TOTALUSANCEAMOUNTLOCAL,
                             //totalUsanceAmount = a.LETTEROFCREDITAMOUNT - ((decimal?)context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == a.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = (string)a.CASHBUILDUPREFERENCETYPE,
                             cashBuildUpReferenceType = (string)a.CASHBUILDUPREFERENCENUMBER,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             lcTolerancePercentage = a.LCTOLERANCEPERCENTAGE,
                             lcToleranceValue = a.LCTOLERANCEVALUE,
                             releaseAmount = a.RELEASEDAMOUNT,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.MIDDLENAME + a.TBL_CUSTOMER.LASTNAME,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formMNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             lastComment = ut.COMMENT,
                             currentApprovalStateId = ut.APPROVALSTATEID,
                             currentApprovalLevelId = ut.TOAPPROVALLEVELID,
                             currentApprovalLevel = ut.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = ut.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             lcApprovalTrailId = ut == null ? 0 : ut.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = ut.TOSTAFFID,
                             approvalStatusId = ut.APPROVALSTATUSID,
                             applicationStatusId = u.USANCEAPPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED,
                             //ussanceAmount = c.USSANCEAMOUNT,
                             //ussanceRate = (int)c.USSANCERATE,
                             //ussanceTenor = (int)c.USSANCETENOR,
                             //lcEffectiveDate = (DateTime)c.LCUSSANCEEFFECTIVEDATE,
                             //lcMaturityDate = (DateTime)c.LCUSSANCEMATURITYDATE
                         }).ToList();
            var companyId = context.TBL_COMPANY.FirstOrDefault().COMPANYID;
            foreach (var u in query)
            {
                var lcCurrRate = fina.GetExchangeRate(DateTime.Now, (short)u.currencyId, companyId);
                var lcTotalAmountAvailableForUsance = u.lcToleranceValue * (decimal)lcCurrRate.sellingRate;
                u.totalUsanceAmount = (lcTotalAmountAvailableForUsance - u.totalUsanceAmountLocal);
            }
            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.lcApprovalTrailId).FirstOrDefault());

            return applications.ToList();
        }

    }
}

           // kernel.Bind<ILcShippingRepository>().To<LcShippingRepository>();
           // LcShippingAdded = ???, LcShippingUpdated = ???, LcShippingDeleted = ???,
