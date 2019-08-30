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

namespace FintrakBanking.Repositories.credit
{
    public class LcUssanceRepository : ILcUssanceRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LcUssanceRepository(
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
                    lcEffectiveDate = entity.LCUSSANCEEFFECTIVEDATE,
                    lcMaturityDate = entity.LCUSSANCEMATURITYDATE,
                    usanceAmountCurrencyId = entity.USANCEAMOUNTCURRENCYID
                };
            }

            return null;
        }

        public LcUssanceViewModel GetLcUssanceByLCIssuanceId(int lcIssuanceId)
        {
            var entity = context.TBL_LC_USSANCE.FirstOrDefault(x => x.LCISSUANCEID == lcIssuanceId && x.DELETED == false);

            if (entity != null)
            {
                return new LcUssanceViewModel
                {
                    lcIssuanceId = entity.LCISSUANCEID,
                    lcUssanceId = entity.LCUSSANCEID,
                    ussanceAmount = entity.USSANCEAMOUNT,
                    ussanceRate = entity.USSANCERATE,
                    ussanceTenor = entity.USSANCETENOR,
                    lcEffectiveDate = entity.LCUSSANCEEFFECTIVEDATE,
                    lcMaturityDate = entity.LCUSSANCEMATURITYDATE,
                    usanceAmountCurrencyId = entity.USANCEAMOUNTCURRENCYID
                };
            }

            return null;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssance(int staffId)
        {
            var usances = context.TBL_LC_USSANCE.ToList();
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
                                approvalTrailId = utrail.APPROVALTRAILID,
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
                                totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)usances.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
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
                                }).GroupBy(l => l.lcUssanceId).Select(l => l.OrderByDescending(t => t.approvalTrailId).FirstOrDefault())
                                .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && l.loopedStaffId == staffId)).ToList();

            var lcsNotStarted = (from x in context.TBL_LC_ISSUANCE
                                 join y in context.TBL_LC_USSANCE on x.LCISSUANCEID equals y.LCISSUANCEID into xy
                                 from u in xy.DefaultIfEmpty()
                                 where
                                (
                                x.DELETED == false
                                && x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                && x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted
                                )
                                 select new LcIssuanceApprovalViewModel()
                                 {
                                     lcIssuanceId = x.LCISSUANCEID,
                                     lcUssanceId = u.LCUSSANCEID,
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
                                     totalUsanceAmount = x.LETTEROFCREDITAMOUNT - ((decimal?)usances.Where(u => u.LCISSUANCEID == x.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT) ?? 0),
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
            return lcs;
        }

        public LcUssanceViewModel AddLcUssance(LcUssanceViewModel model)
        {
            ValidateUsanceAmount(model);
            var entity = new TBL_LC_USSANCE
            {
                LCISSUANCEID = model.lcIssuanceId,
                USSANCEAMOUNT = model.ussanceAmount,
                USSANCERATE = model.ussanceRate,
                USSANCETENOR = model.ussanceTenor,
                LCUSSANCEEFFECTIVEDATE = model.lcEffectiveDate,
                LCUSSANCEMATURITYDATE = model.lcMaturityDate,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                USANCEAMOUNTCURRENCYID = model.usanceAmountCurrencyId,
            };

            context.TBL_LC_USSANCE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_LC_USSANCE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------
            context.SaveChanges();
            var createdlcUssance = context.TBL_LC_USSANCE.FirstOrDefault(lc => lc.DATETIMECREATED == entity.DATETIMECREATED && lc.LCISSUANCEID == entity.LCISSUANCEID);
            model.lcUssanceId = createdlcUssance.LCUSSANCEID;
            return model;
        }

        private bool ValidateUsanceAmount(LcUssanceViewModel model)
        {
            var lc = context.TBL_LC_ISSUANCE.Find(model.lcIssuanceId);
            var totalReleasedAmount = context.TBL_LCRELEASE_AMOUNT.Where(r => r.LCISSUANCEID == model.lcIssuanceId).Sum(r => r.RELEASEAMOUNT);
            var availableAmount = lc.LCTOLERANCEVALUE - totalReleasedAmount;
            if (model.ussanceAmount > availableAmount)
            {
                throw new SecureException("Usance Amount cannot be greater than remainder tolerance amount" + availableAmount);
            }
            return true;
        }

        public bool UpdateLcUssance(LcUssanceViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_USSANCE.Find(id);
            entity.LCISSUANCEID = model.lcIssuanceId;
            entity.LCUSSANCEID = model.lcUssanceId;
            entity.USSANCEAMOUNT = model.ussanceAmount;
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCUSSANCEID
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LCUSSANCEID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LcIssuanceApprovalViewModel> GetLcIssuancesForUssanceApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.lcUssance;
            IQueryable<LcIssuanceApprovalViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();

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
                            && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
                            && ut.APPROVALSTATEID != (int)ApprovalState.Ended
                            && ut.RESPONSESTAFFID == null
                            && levelIds.Contains((int)ut.TOAPPROVALLEVELID)
                            && (ut.TOSTAFFID == null || ut.TOSTAFFID == staffId)
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             lcUssanceId = u.LCUSSANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalUsanceAmount = context.TBL_LC_USSANCE.Where(u => u.LCISSUANCEID == a.LCISSUANCEID && u.USANCEAPPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.lcUssanceCompleted).Sum(u => u.USSANCEAMOUNT),
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
                             approvalTrailId = ut == null ? 0 : ut.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = ut.TOSTAFFID,
                             approvalStatusId = (short)u.USANCEAPPROVALSTATUSID,
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

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.lcIssuanceId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications.ToList();
        }

    }
}

           // kernel.Bind<ILcShippingRepository>().To<LcShippingRepository>();
           // LcShippingAdded = ???, LcShippingUpdated = ???, LcShippingDeleted = ???,
