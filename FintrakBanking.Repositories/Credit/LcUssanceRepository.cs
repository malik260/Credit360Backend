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
                    lcMaturityDate = x.LCUSSANCEMATURITYDATE
                })
                .ToList();
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
                    lcMaturityDate = entity.LCUSSANCEMATURITYDATE
                };
            }

            return null;
        }

        public IEnumerable<LcIssuanceViewModel> GetLcIssuancesForUssance()
        {
            var lcs = (from x in context.TBL_LC_ISSUANCE where
                        (x.DELETED == false && x.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.LcIssuanceCompleted)
                        //join y in context.TBL_LC_USSANCE.Where(y => y.DELETED == false)
                        //on x.LCISSUANCEID equals y.LCISSUANCEID
                            select new LcIssuanceViewModel()
                            {
                                lcIssuanceId = x.LCISSUANCEID,
                                beneficiaryName = x.BENEFICIARYNAME,
                                totalApprovedAmount = x.TOTALAPPROVEDAMOUNT,
                                totalApprovedAmountCurrencyId = x.TOTALAPPROVEDAMOUNTCURRENCYID,
                                availableAmountCurrencyId = x.AVAILABLEAMOUNTCURRENCYID,
                                cashBuildUpAvailable = x.CASHBUILDUPAVAILABLE,
                                cashBuildUpReferenceNumber = (string)x.CASHBUILDUPREFERENCETYPE,
                                cashBuildUpReferenceType = (string)x.CASHBUILDUPREFERENCENUMBER,
                                percentageToCover = x.PERCENTAGETOCOVER,
                                letterOfCreditTypeId = x.LETTEROFCREDITTYPEID,
                                isDraftRequired = x.ISDRAFTREQUIRED,
                                beneficiaryAddress = x.BENEFICIARYADDRESS,
                                beneficiaryEmail = x.BENEFICIARYEMAIL,
                                customerId = x.CUSTOMERID,
                                fundSourceId = x.FUNDSOURCEID,
                                fundSourceDetails = x.FUNDSOURCEDETAILS,
                                formNumber = x.FORMMNUMBER,
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
                                dateTimeCreated = (DateTime)x.DATETIMECREATED,
                                //lcUssanceId = y.LCUSSANCEID,
                                //ussanceAmount = y.USSANCEAMOUNT,
                                //ussanceRate = (int)y.USSANCERATE,
                                //ussanceTenor = (int)y.USSANCETENOR,
                                //lcEffectiveDate = (DateTime)y.LCUSSANCEEFFECTIVEDATE,
                                //lcMaturityDate = (DateTime)y.LCUSSANCEMATURITYDATE
                            })
                            .ToList();
                        return lcs;
        }

        public LcUssanceViewModel AddLcUssance(LcUssanceViewModel model)
        {
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
            var createdlcUssance = context.TBL_LC_USSANCE.FirstOrDefault(lc => lc.LCISSUANCEID == model.lcIssuanceId);
            model.lcUssanceId = createdlcUssance.LCUSSANCEID;
            return model;
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

            var querytest1 = (from a in context.TBL_LC_ISSUANCE
                              where
                                a.DELETED == false
                                && a.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress
                                && a.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                              select a).ToList();

            var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
                              where
                                (b.OPERATIONID == operationId)
                                && b.APPROVALSTATEID != (int)ApprovalState.Ended
                                && b.RESPONSESTAFFID == null
                                && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                                && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                              select b).ToList();

            var query = (from a in context.TBL_LC_ISSUANCE
                         where
                            (a.DELETED == false
                            && a.LCUSSANCESTATUSID == (int)LoanApplicationStatusEnum.lcUssanceInProgress)
                         orderby a.LCISSUANCEID
                         join b in context.TBL_APPROVAL_TRAIL on a.LCISSUANCEID equals b.TARGETID
                         where
                            (
                            (b.OPERATIONID == operationId)
                            && b.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RESPONSESTAFFID == null
                            && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                            && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                            )
                         select new LcIssuanceApprovalViewModel()
                         {
                             lcIssuanceId = a.LCISSUANCEID,
                             isDraftRequired = a.ISDRAFTREQUIRED,
                             lcReferenceNumber = a.LCREFERENCENUMBER,
                             letterOfCreditTypeId = a.LETTEROFCREDITTYPEID,
                             beneficiaryName = a.BENEFICIARYNAME,
                             totalApprovedAmount = a.TOTALAPPROVEDAMOUNT,
                             totalApprovedAmountCurrencyId = a.TOTALAPPROVEDAMOUNTCURRENCYID,
                             availableAmountCurrencyId = a.AVAILABLEAMOUNTCURRENCYID,
                             cashBuildUpAvailable = a.CASHBUILDUPAVAILABLE,
                             cashBuildUpReferenceNumber = (string)a.CASHBUILDUPREFERENCETYPE,
                             cashBuildUpReferenceType = (string)a.CASHBUILDUPREFERENCENUMBER,
                             percentageToCover = a.PERCENTAGETOCOVER,
                             beneficiaryAddress = a.BENEFICIARYADDRESS,
                             beneficiaryEmail = a.BENEFICIARYEMAIL,
                             customerId = a.CUSTOMERID,
                             fundSourceId = a.FUNDSOURCEID,
                             fundSourceDetails = a.FUNDSOURCEDETAILS,
                             formNumber = a.FORMMNUMBER,
                             beneficiaryPhoneNumber = a.BENEFICIARYPHONENUMBER,
                             beneficiaryBank = a.BENEFICIARYBANK,
                             currencyId = a.CURRENCYID,
                             proformaInvoiceId = a.PROFORMAINVOICEID,
                             availableAmount = a.AVAILABLEAMOUNT,
                             letterOfCreditAmount = a.LETTEROFCREDITAMOUNT,
                             letterOfcreditExpirydate = a.LETTEROFCREDITEXPIRYDATE,
                             invoiceDate = a.INVOICEDATE,
                             invoiceDueDate = a.INVOICEDUEDATE,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             approvalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             //customerName = context.TBL_CUSTOMER.Find(a.CUSTOMERID).FIRSTNAME + context.TBL_CUSTOMER.Find(a.CUSTOMERID).LASTNAME,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATETIMECREATED,
                             //lcUssanceId = c.LCUSSANCEID,
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
