using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class ValuationReportRepository : IValuationReportRepository
    {
        private FinTrakBankingContext _context;
        private IGeneralSetupRepository _general;
        private IAuditTrailRepository _audit;

        public ValuationReportRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            _context = context;
            _general = general;
            _audit = audit;
        }

        public ValuationReportViewModel AddValuationReport(ValuationReportViewModel model)
        {
            var entity = new TBL_VALUATION_REPORT()
            {
                ACCOUNTNUMBER = model.accountNumber,
                APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending,
                COLLATERALVALUATIONID = model.collateralValuationId,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                WHT = model.WHT,
                VALUATIONFEE = model.valuationFee,
                VALUATIONREPORTID = model.valuationReportId,
                VALUERCOMMENT = model.valuerComment,
                VALUERID = model.valuerId,
                DATETIMECREATED = _general.GetApplicationDate()
            };

            var newEntity = _context.TBL_VALUATION_REPORT.Add(entity);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.CallateralValuationReportAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Valuation Report for  '{ model.valuationReportId }' is added by {model.staffId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this._audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            try {
                _context.SaveChanges();

                return new ValuationReportViewModel()
                {
                    accountNumber = newEntity.ACCOUNTNUMBER,
                    approvalStatusId = newEntity.APPROVALSTATUSID,
                    collateralValuationId = newEntity.COLLATERALVALUATIONID,
                    companyId = newEntity.COMPANYID,
                    createdBy = newEntity.CREATEDBY,
                    WHT = newEntity.WHT,
                    valuationFee = newEntity.VALUATIONFEE,
                    valuationReportId = newEntity.VALUATIONREPORTID,
                    valuerComment = newEntity.VALUERCOMMENT,
                    valuerId = newEntity.VALUERID,
                    dateTimeCreated = newEntity.DATETIMECREATED
                };
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public List<ValuationReportViewModel> GetAllValuationReports()
        {
            var data = from O in _context.TBL_VALUATION_REPORT
                       select new ValuationReportViewModel
                       {
                           accountNumber = O.ACCOUNTNUMBER,
                           approvalStatusId = O.APPROVALSTATUSID,
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           companyId = O.COMPANYID,
                           createdBy = O.CREATEDBY,
                           WHT = O.WHT,
                           valuationFee = O.VALUATIONFEE,
                           valuationReportId = O.VALUATIONREPORTID,
                           valuerComment = O.VALUERCOMMENT,
                           valuerId = O.VALUERID,
                           dateTimeCreated = O.DATETIMECREATED,
                       };
            return data.ToList();
        }
    }
}
