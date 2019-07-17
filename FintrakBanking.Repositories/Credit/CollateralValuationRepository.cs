using FintrakBanking.Common.CustomException;
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
                OPERATIONID = (int)OperationsEnum.CollateralValuationRequest,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
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


        public List<CollateralValuationViewModel> GetAllCollateralValuerIformation()
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       select new CollateralValuationViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer =  _context.TBL_ACCREDITEDCONSULTANT.Where(o=>o.ACCREDITEDCONSULTANTID==x.VALUERID).Select(o=>o.NAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuerComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,

                       };
            return data.ToList();
        }

        public List<CollateralValuationViewModel> GetAllCollateralValuerIformation(int id)
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       where x.COLLATERALVALUATIONID == id
                       select new CollateralValuationViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer = _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.NAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuerComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,

                       };
            return data.ToList();
        }
        public bool AddCollateralValurerInfo(CollateralValuationViewModel model)
        {
            try
            {
                var exist = _context.TBL_VALUATION_REPORT.Where(x => x.ACCOUNTNUMBER == model.accountNumber
                                                            && x.WHT == model.wht && x.CREATEDBY == model.createdBy
                                                            && x.COLLATERALVALUATIONID == model.collateralValuationId
                                                            && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved).Any();

            if (exist == true) throw new SecureException("This record is already saved!");

            var entity = new TBL_VALUATION_REPORT()
            {
                VALUERID = model.valuerId,
                COLLATERALVALUATIONID = model.collateralValuationId,
                VALUATIONFEE = model.valuationFee,
                ACCOUNTNUMBER = model.accountNumber,
                WHT = model.wht,
                VALUERCOMMENT = model.valuerComment,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _general.GetApplicationDate(),
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing
            };

            var newEntity = _context.TBL_VALUATION_REPORT.Add(entity);

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

           
                return _context.SaveChanges() > 0;

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
                           approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == O.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           approvalStatusId = O.APPROVALSTATUSID
                       };
            return data.ToList();
        }

        public bool GoForCollateralValuationApproval(CollateralValuationViewModel entity)
        {

            var valuations = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALCUSTOMERID == entity.collateralCustomerId && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).Select(o => o).ToList();

            foreach (var valuation in valuations)
            {
                valuation.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            if (valuations != null)
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

            return _context.SaveChanges() != 0;
        }

        public IEnumerable<CollateralValuationViewModel> GetAllValuationRequest(int collteralId)
        {
            var res = from C in _context.TBL_COLLATERAL_CUSTOMER
                      join V in _context.TBL_COLLATERAL_VALUATION
                      on C.COLLATERALCUSTOMERID equals V.COLLATERALCUSTOMERID
                      where V.COLLATERALCUSTOMERID == collteralId
                      select new CollateralValuationViewModel
                      {
                          valuationComment = V.VALUATIONCOMMENT,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          approvalStatusId = V.APPROVALSTATUSID,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == V.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
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

        public bool SubmitApproval(CollateralValuationViewModel model)
        {
            bool responce = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
                _workflow.StaffId = model.createdBy;
                _workflow.CompanyId = model.companyId;
                _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                _workflow.TargetId = model.collateralCustomerId;
                _workflow.Comment = model.comment;
                _workflow.OperationId = (int)OperationsEnum.CollateralValuationRequest;
                _workflow.DeferredExecution = true;
                _workflow.LogActivity();
                try
                {
                    if (_workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var valuations = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALCUSTOMERID == model.collateralCustomerId && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing).Select(o => o).ToList();

                        foreach (var valuation in valuations)
                        {
                            valuation.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                            var valuerReport = _context.TBL_VALUATION_REPORT.Where(o => o.COLLATERALVALUATIONID == valuation.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();
                            if (valuerReport == null) continue;
                            valuerReport.APPROVALSTATUSID= (int)ApprovalStatusEnum.Approved;
                        }



                    }

                    responce = _context.SaveChanges() > 0;
                    transaction.Commit();

                    return responce;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }
    }
}
