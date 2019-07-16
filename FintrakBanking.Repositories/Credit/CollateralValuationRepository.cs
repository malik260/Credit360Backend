using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CollateralValuationRepository : ICollateralValuationRepository
    {
        private FinTrakBankingContext _context;
        private IGeneralSetupRepository _general;
        private IAuditTrailRepository _audit;
        private IWorkflow _workflow;



        public CollateralValuationRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, IWorkflow workflow)
        {
            _context = context;
            _general = general;
            _audit = audit;
            _workflow = workflow;
        }

        public CollateralValuationViewModel AddCollateralValuation(CollateralValuationViewModel model)
        {

            var entity = new TBL_COLLATERAL_VALUATION()
            {
                COLLATERALCUSTOMERID = model.collateralCustomerId,
                VALUATIONREQUESTTYPEID = model.valuationRequestTypeId,
                VALUATIONCOMMENT = model.valuationComment,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _general.GetApplicationDate(),
                OPERATIONID = (int)OperationsEnum.CollateralValuationRequest
            };

            var newEntity = _context.TBL_COLLATERAL_VALUATION.Add(entity);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CallateralValuationAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Valuation for  '{ model.collateralValuationId }' is added by {model.staffId} ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            try
            {
                _context.SaveChanges();
                return new CollateralValuationViewModel()
                {
                    collateralCustomerId = newEntity.COLLATERALCUSTOMERID,
                    valuationRequestTypeId = newEntity.VALUATIONREQUESTTYPEID,
                    valuationComment = newEntity.VALUATIONCOMMENT,
                    companyId = newEntity.COMPANYID.Value,
                    createdBy = newEntity.CREATEDBY,
                    dateTimeCreated = newEntity.DATETIMECREATED,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<CollateralValuationViewModel> GetAllCollateralValuations(int collateralId)
        {
            var data = from O in _context.TBL_COLLATERAL_VALUATION
                       where O.COLLATERALCUSTOMERID == collateralId
                       select new CollateralValuationViewModel
                       {
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           collateralCustomerId = O.COLLATERALCUSTOMERID,
                           valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           valuationComment = O.VALUATIONCOMMENT,
                           operationId = O.OPERATIONID,
                           customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID).FirstOrDefault(),
                       };
            return data.ToList();
        }

        public bool GoForCollateralValuationApproval(CollateralValuationViewModel entity)
        {
            try
            {

                var document = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALCUSTOMERID == entity.collateralCustomerId).Select(o => o).FirstOrDefault();

                if (document != null)
                {

                    _workflow.StaffId = entity.createdBy;
                    _workflow.CompanyId = entity.companyId;
                    _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = entity.collateralCustomerId;
                    _workflow.Comment = "Request for collateral visitation approval";
                    _workflow.OperationId = (int)OperationsEnum.CollateralValuationRequest;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();
                }
            }
            catch (Exception ex) { }
            return _context.SaveChanges() != 0;
        }

        public IEnumerable<CollateralValuationViewModel> GetAllValuationRequest(int collteralId)
        {
            var res = from C in _context.TBL_COLLATERAL_CUSTOMER
                      join V in _context.TBL_COLLATERAL_VALUATION
                      on C.COLLATERALCUSTOMERID equals V.COLLATERALCUSTOMERID
                      where V.COLLATERALCUSTOMERID == collteralId
                      select new CollateralValuationViewModel {
                          valuationComment = V.VALUATIONCOMMENT,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          approvalStatusId = V.APPROVALSTATUSID,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o=>o.APPROVALSTATUSID==V.APPROVALSTATUSID).Select(o=>o.APPROVALSTATUSNAME).FirstOrDefault(),
                          collateralCustomerId = C.COLLATERALCUSTOMERID
                      };

            return res;
        }

        public IEnumerable<CollateralValuationViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId)
        {
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CollateralValuationRequest).ToList();


            var res = from C in _context.TBL_COLLATERAL_CUSTOMER
                      join atrail in _context.TBL_APPROVAL_TRAIL on C.COLLATERALCUSTOMERID equals atrail.TARGETID
                      join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                      where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                       && atrail.RESPONSESTAFFID == null
                       && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                       && atrail.OPERATIONID == (int)OperationsEnum.CollateralValuationRequest
                      select new CollateralValuationViewModel
                      {
                          customerName = cus.FIRSTNAME + " " + cus.LASTNAME + " " + cus.MAIDENNAME,
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          collateralValue = C.COLLATERALVALUE,
                          collateralCustomerId = C.COLLATERALCUSTOMERID
                      };

            return res;
        }
    }
}
