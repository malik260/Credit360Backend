using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
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
                VALUATIONNAME = model.valuationName,
                VALUATIONREASON = model.valuationReason,
                //COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _general.GetApplicationDate(),
                //OPERATIONID = (int)OperationsEnum.CollateralValuationRequest,
                //APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
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
                    //valuationRequestTypeId = newEntity.VALUATIONREQUESTTYPEID,
                    valuationName = newEntity.VALUATIONNAME,
                    valuationReason = newEntity.VALUATIONREASON,
                    //companyId = newEntity.COMPANYID.Value,
                    createdBy = newEntity.CREATEDBY,
                    dateTimeCreated = newEntity.DATETIMECREATED,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ValuationPrerequisiteViewModel AddValuationPrerequisite(ValuationPrerequisiteViewModel model)
        {

            var entity = new TBL_COLLATERAL_VALUATION_PRE()
            {
                //COLLATERALCUSTOMERID = model.collateralCustomerId,
                COLLATERALVALUATIONID = model.collateralValuationId,
                VALUATIONREQUESTTYPEID = model.valuationRequestTypeId,
                VALUATIONCOMMENT = model.valuationComment,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _general.GetApplicationDate(),
                OPERATIONID = (int)OperationsEnum.CollateralValuationRequest,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending
            };

            var newEntity = _context.TBL_COLLATERAL_VALUATION_PRE.Add(entity);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ValuationPrerequisiteAdded,
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
                return new ValuationPrerequisiteViewModel()
                {
                    //collateralCustomerId = newEntity.COLLATERALCUSTOMERID,
                    collateralValuationId = newEntity.COLLATERALVALUATIONID,
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

        public CollateralValuationViewModel GetCollateralValuation(int collteralValuationId)
        {
            var res = from C in _context.TBL_COLLATERAL_VALUATION
                      where C.COLLATERALVALUATIONID == collteralValuationId
                      select new CollateralValuationViewModel
                      {
                          collateralValuationId = C.COLLATERALVALUATIONID,
                          collateralCustomerId = C.COLLATERALCUSTOMERID,
                          valuationName = C.VALUATIONNAME,
                          valuationReason = C.VALUATIONREASON,
                          dateTimeCreated = C.DATETIMECREATED,
                          createdBy = C.CREATEDBY,
                      };

            return res.FirstOrDefault();
        }

        public List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation()
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer =  _context.TBL_ACCREDITEDCONSULTANT.Where(o=>o.ACCREDITEDCONSULTANTID==x.VALUERID).Select(o=>o.NAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuationComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,

                       };
            return data.ToList();
        }

        public List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation(int id)
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       where x.COLLATERALVALUATIONID == id
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer = _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.NAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuationComment = x.VALUERCOMMENT,
                           //valuerComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,

                       };
            return data.ToList();
        }
        public bool AddCollateralValurerInfo(ValuationPrerequisiteViewModel model)
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
                VALUERCOMMENT = model.valuationComment,
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
                           //collateralValuationId = O.VALUATIONPREREQUISITEID,
                           collateralCustomerId = O.COLLATERALCUSTOMERID,
                           //valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           valuationReason = O.VALUATIONREASON,
                           valuationName = O.VALUATIONNAME,
                           //operationId = O.OPERATIONID,
                           //customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID).FirstOrDefault(),
                           //approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == O.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           //approvalStatusId = O.APPROVALSTATUSID
                       };
            return data.ToList();
        }

        public List<ValuationPrerequisiteViewModel> GetAllValuationPrerequisitesById(int collateralValuationId)
        {
            var collateralId = _context.TBL_COLLATERAL_VALUATION.Where(x => x.COLLATERALVALUATIONID == collateralValuationId)
                                                                    .Select(x => x.COLLATERALCUSTOMERID).FirstOrDefault();

            var data = from O in _context.TBL_COLLATERAL_VALUATION_PRE
                       where O.COLLATERALVALUATIONID == collateralValuationId
                       select new ValuationPrerequisiteViewModel
                       {
                           valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           //collateralCustomerId = O.COLLATERALCUSTOMERID,
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
            var valuations = _context.TBL_COLLATERAL_VALUATION_PRE.Where(o => o.COLLATERALVALUATIONID == entity.collateralValuationId && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).Select(o => o).ToList();
            try { 
            foreach (var valuation in valuations)
            {
                valuation.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            if (valuations != null)
            {
                _workflow.StaffId = entity.createdBy;
                _workflow.CompanyId = entity.companyId;
                _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                _workflow.TargetId = entity.collateralValuationId;
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

        public IEnumerable<ValuationPrerequisiteViewModel> GetAllValuationRequest(int collteralId)
        {
            var res = from C in _context.TBL_COLLATERAL_CUSTOMER
                      join T in _context.TBL_COLLATERAL_VALUATION
                      on C.COLLATERALCUSTOMERID equals T.COLLATERALCUSTOMERID
                      join V in _context.TBL_COLLATERAL_VALUATION_PRE
                      on T.COLLATERALVALUATIONID equals V.COLLATERALVALUATIONID
                      where T.COLLATERALCUSTOMERID == collteralId
                      select new ValuationPrerequisiteViewModel
                      {
                          valuationComment = V.VALUATIONCOMMENT,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          approvalStatusId = V.APPROVALSTATUSID,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == V.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                          collateralValuationId = V.COLLATERALVALUATIONID
                          //collateralCustomerId = C.COLLATERALCUSTOMERID
                      };

            return res;
        }

        public IEnumerable<ValuationPrerequisiteViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId)
        {
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int) OperationsEnum.CollateralValuationRequest).ToList();
            //_context.Configuration.ProxyCreationEnabled = false;

            var res = from q in _context.TBL_COLLATERAL_VALUATION 
                join C in _context.TBL_COLLATERAL_CUSTOMER on q.COLLATERALCUSTOMERID equals C.COLLATERALCUSTOMERID
                      join atrail in _context.TBL_APPROVAL_TRAIL on q.COLLATERALVALUATIONID equals atrail.TARGETID
                      join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                      join valPre in _context.TBL_COLLATERAL_VALUATION_PRE on q.COLLATERALVALUATIONID equals valPre.COLLATERALVALUATIONID
                      where atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing
                       && atrail.RESPONSESTAFFID == null
                       && ids.Contains((int) atrail.TOAPPROVALLEVELID)
                       && atrail.OPERATIONID == (int) OperationsEnum.CollateralValuationRequest
                      select new ValuationPrerequisiteViewModel
                      {
                          customerName = cus.FIRSTNAME + " " + cus.LASTNAME + " " + cus.MAIDENNAME,
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          collateralValue = C.COLLATERALVALUE,
                          collateralValuationId = q.COLLATERALVALUATIONID,
                          collateralCustomerId = C.COLLATERALCUSTOMERID,
                          valuationComment = valPre.VALUATIONCOMMENT,
                          valuationName = q.VALUATIONNAME,
                          valuationReason = q.VALUATIONREASON,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == valPre.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                      };

            return res.GroupBy(O => O.collateralValuationId).Select(O => O.FirstOrDefault()).ToList();
        }

        public bool SubmitApproval(CollateralValuationViewModel model)
        {
            bool responce = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
                _workflow.StaffId = model.createdBy;
                _workflow.CompanyId = model.companyId;
                _workflow.StatusId = (int) ApprovalStatusEnum.Processing;
                _workflow.TargetId = model.collateralCustomerId;
                _workflow.Comment = model.valuerComment;
                _workflow.OperationId = (int) OperationsEnum.CollateralValuationRequest;
                _workflow.DeferredExecution = true;
                _workflow.LogActivity();
                try
                {
                    if (_workflow.NewState == (int) ApprovalState.Ended)
                    {
                        var prereqisites = _context.TBL_COLLATERAL_VALUATION_PRE.Where(o => o.COLLATERALVALUATIONID == model.collateralValuationId && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing).Select(o => o).ToList();

                        foreach (var prereqisite in prereqisites)
                        {
                            prereqisite.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                            var valuerReport = _context.TBL_VALUATION_REPORT.Where(o => o.COLLATERALVALUATIONID == prereqisite.VALUATIONPREREQUISITEID).Select(o => o).FirstOrDefault();
                            if (valuerReport == null) continue;
                            valuerReport.APPROVALSTATUSID= (int) ApprovalStatusEnum.Approved;
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

        public bool DeleteValuationPrerequisite(int valuationPrerequisiteId, UserInfo user)
        {
            var prerequisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending 
                                        && O.VALUATIONPREREQUISITEID ==  valuationPrerequisiteId).FirstOrDefault();

            if (prerequisite != null)
            {
                _context.TBL_COLLATERAL_VALUATION_PRE.Remove(prerequisite);

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short) AuditTypeEnum.ValuationPrerequisiteDeleted,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Deleted Valuation Prerequisite with code: " + prerequisite.VALUATIONPREREQUISITEID,
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                _audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return _context.SaveChanges() > 0;
            }

            return false;
        }
    }
}
