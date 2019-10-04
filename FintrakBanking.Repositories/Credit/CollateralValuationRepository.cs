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

        public bool UpdateValuationPrerequisite(int valuationPrerequisiteId, ValuationPrerequisiteViewModel model)
        {
            var data = _context.TBL_COLLATERAL_VALUATION_PRE.Find(valuationPrerequisiteId);

            if (data == null) { return false; }

            //if (data.LASTUPDATEDBY != model.lastUpdatedBy) // archive old
            //{
            //    _context.TBL_COLLATERAL_VALUATION_PRE.Add(new TBL_COLLATERAL_VALUATION_PRE
            //    {
            //        COLLATERALVALUATIONID = model.collateralValuationId,
            //        VALUATIONREQUESTTYPEID = model.valuationRequestTypeId,
            //        VALUATIONCOMMENT = model.valuationComment,
            //    });
            //}

            //data.COLLATERALVALUATIONID = model.collateralValuationId;
            data.VALUATIONREQUESTTYPEID = model.valuationRequestTypeId;
            data.VALUATIONCOMMENT = model.valuationComment;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = _general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short) AuditTypeEnum.ValuationPrerequisiteUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short) model.userBranchId,
                DETAIL = $"Updated Valuation Prerequisite for '{ model.valuationPrerequisiteId }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this._audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return _context.SaveChanges() != 0;
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
                       orderby x.VALUATIONREPORTID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer =  _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.NAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuationComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,
                           operationId = (int) OperationsEnum.CollateralValuationRequest
                       };
            return data.ToList();
        }

        public List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation(int id)
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       where x.COLLATERALVALUATIONID == id
                       orderby x.VALUATIONREPORTID descending
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
                WHTAMOUNT = model.whtAmount,
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
                       orderby O.COLLATERALVALUATIONID descending
                       select new CollateralValuationViewModel
                       {
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           //collateralValuationId = O.VALUATIONPREREQUISITEID,
                           collateralCustomerId = O.COLLATERALCUSTOMERID,
                           //valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           valuationReason = O.VALUATIONREASON,
                           valuationName = O.VALUATIONNAME,
                           createdBy = O.CREATEDBY,
                           createdByName = _context.TBL_STAFF.Where(s => s.STAFFID == O.CREATEDBY && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),
                           //operationId = O.OPERATIONID,
                           //customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID).FirstOrDefault(),
                           //approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == O.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           //approvalStatusId = O.APPROVALSTATUSID
                       };
            return data.ToList();
        }

        public List<ValuationPrerequisiteViewModel> GetAllValuationPrerequisitesById(int staffId, int collateralValuationId)
        {
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CollateralValuationRequest).ToList();

            var collateralId = _context.TBL_COLLATERAL_VALUATION.Where(x => x.COLLATERALVALUATIONID == collateralValuationId)
                                                                    .Select(x => x.COLLATERALCUSTOMERID).FirstOrDefault();

            // fetching PENDING valuation prerequisite
            var result = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                       where O.COLLATERALVALUATIONID == collateralValuationId
                       && O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending
                       orderby O.VALUATIONPREREQUISITEID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           //collateralCustomerId = O.COLLATERALCUSTOMERID,
                           valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           valuationComment = O.VALUATIONCOMMENT,
                           operationId = O.OPERATIONID,
                           customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID.Value).FirstOrDefault(),
                           approvalStatusId = O.APPROVALSTATUSID,
                           approvalComment = O.VALUATIONCOMMENT,
                           approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == o.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                       });

            // fetching REFFERED valuation prerequisite
            var result2 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                       join atrail in _context.TBL_APPROVAL_TRAIL on O.VALUATIONPREREQUISITEID equals atrail.TARGETID
                       where O.COLLATERALVALUATIONID == collateralValuationId
                       && atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Referred || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved
                       //&& atrail.RESPONSESTAFFID == null && atrail.APPROVALSTATEID != (short) ApprovalState.Ended
                       && atrail.LOOPEDSTAFFID == staffId
                       && ids.Contains((int) atrail.TOAPPROVALLEVELID)
                       && atrail.OPERATIONID == (int) OperationsEnum.CollateralValuationRequest
                        orderby O.VALUATIONPREREQUISITEID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
                           collateralValuationId = O.COLLATERALVALUATIONID,
                           valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           valuationComment = O.VALUATIONCOMMENT,
                           operationId = O.OPERATIONID,
                           customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID.Value).FirstOrDefault(),
                           approvalStatusId = atrail.APPROVALSTATUSID,
                           approvalComment = atrail.COMMENT,
                           approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                       });

            // fetching DISAPPROVED valuation prerequisite
            var result3 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                        join atrail in _context.TBL_APPROVAL_TRAIL on O.VALUATIONPREREQUISITEID equals atrail.TARGETID
                        where O.COLLATERALVALUATIONID == collateralValuationId
                        && atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Disapproved || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                        && atrail.RESPONSESTAFFID == null
                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                        && atrail.OPERATIONID == (int)OperationsEnum.CollateralValuationRequest
                         orderby O.VALUATIONPREREQUISITEID descending
                        select new ValuationPrerequisiteViewModel
                        {
                            valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
                            collateralValuationId = O.COLLATERALVALUATIONID,
                            valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                            valuationComment = O.VALUATIONCOMMENT,
                            operationId = O.OPERATIONID,
                            customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID.Value).FirstOrDefault(),
                            approvalStatusId = atrail.APPROVALSTATUSID,
                            approvalComment = atrail.COMMENT,
                            approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                        });


            result = result.Union(result2).Union(result3);
            return result.ToList();
        }

        public bool GoForCollateralValuationApproval(ValuationPrerequisiteViewModel entity)
        {
            var prerequisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.COLLATERALVALUATIONID == entity.collateralValuationId && O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing).Select(O => O).FirstOrDefault();
            //var prerequisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.VALUATIONPREREQUISITEID == entity.valuationPrerequisiteId && (O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)).Select(O => O).FirstOrDefault();

            try
            {

                if (prerequisite != null)
                {
                    prerequisite.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                    _workflow.StaffId = entity.createdBy;
                    _workflow.CompanyId = entity.companyId;
                    _workflow.StatusId = (int) ApprovalStatusEnum.Processing;
                    //_workflow.TargetId = entity.collateralValuationId;
                    _workflow.TargetId = entity.valuationPrerequisiteId;
                    _workflow.Comment = "Request for collateral visitation approval";
                    _workflow.OperationId = (int) OperationsEnum.CollateralValuationRequest;
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
                      join T in _context.TBL_COLLATERAL_VALUATION on C.COLLATERALCUSTOMERID equals T.COLLATERALCUSTOMERID
                      join V in _context.TBL_COLLATERAL_VALUATION_PRE on T.COLLATERALVALUATIONID equals V.COLLATERALVALUATIONID
                      join atrail in _context.TBL_APPROVAL_TRAIL on V.VALUATIONPREREQUISITEID equals atrail.TARGETID
                      where T.COLLATERALCUSTOMERID == collteralId && V.APPROVALSTATUSID != (int) ApprovalStatusEnum.Approved
                      orderby V.VALUATIONPREREQUISITEID descending
                      select new ValuationPrerequisiteViewModel
                      {
                          valuationComment = V.VALUATIONCOMMENT,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          //approvalStatusId = V.APPROVALSTATUSID,
                          //approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == V.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                          collateralValuationId = V.COLLATERALVALUATIONID,
                          operationId = V.OPERATIONID,
                          approvalStatusId = atrail.APPROVALSTATUSID,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                          collateralCustomerId = C.COLLATERALCUSTOMERID,
                          //collateralCustomerId = C.COLLATERALCUSTOMERID
                      };

            return res.ToList();
        }

        public List<ValuationPrerequisiteViewModel> GetCollateralValuationPrerequisiteById(int staffId, int valuationPrerequisiteId)
        {
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int) OperationsEnum.CollateralValuationRequest).ToList();

            var res = from valPre in _context.TBL_COLLATERAL_VALUATION_PRE
                      join q in _context.TBL_COLLATERAL_VALUATION on valPre.COLLATERALVALUATIONID equals q.COLLATERALVALUATIONID
                      join C in _context.TBL_COLLATERAL_CUSTOMER on q.COLLATERALCUSTOMERID equals C.COLLATERALCUSTOMERID
                      join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                      //join atrail in _context.TBL_APPROVAL_TRAIL on valPre.VALUATIONPREREQUISITEID equals atrail.TARGETID
                      join atrail in _context.TBL_APPROVAL_TRAIL on valuationPrerequisiteId equals atrail.TARGETID
                      where valPre.VALUATIONPREREQUISITEID == valuationPrerequisiteId && ids.Contains((int) atrail.TOAPPROVALLEVELID)
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
                          operationId = valPre.OPERATIONID,
                          valuationPrerequisiteId = valPre.VALUATIONPREREQUISITEID,
                          approvalStatusId = atrail.APPROVALSTATUSID,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == valPre.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == valPre.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                      };

            return res.ToList();
        }

        public IEnumerable<ValuationPrerequisiteViewModel> GetCollateralValuationRequestWaitingForApproval(int staffId)
        {
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int) OperationsEnum.CollateralValuationRequest).ToList();
            //_context.Configuration.ProxyCreationEnabled = false;

            var res = from valPre in _context.TBL_COLLATERAL_VALUATION_PRE
                      join q in _context.TBL_COLLATERAL_VALUATION on valPre.COLLATERALVALUATIONID equals q.COLLATERALVALUATIONID
                      join C in _context.TBL_COLLATERAL_CUSTOMER on q.COLLATERALCUSTOMERID equals C.COLLATERALCUSTOMERID
                      join atrail in _context.TBL_APPROVAL_TRAIL on valPre.VALUATIONPREREQUISITEID equals atrail.TARGETID
                      join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                      where (atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                       && atrail.RESPONSESTAFFID == null
                       && ids.Contains((int) atrail.TOAPPROVALLEVELID)
                       && atrail.OPERATIONID == (int) OperationsEnum.CollateralValuationRequest
                       orderby valPre.VALUATIONPREREQUISITEID descending
                      select new ValuationPrerequisiteViewModel
                      {
                          customerName = cus.FIRSTNAME + " " + cus.LASTNAME + " " + cus.MAIDENNAME,
                          customerId = cus.CUSTOMERID,
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          collateralValue = C.COLLATERALVALUE,
                          collateralValuationId = q.COLLATERALVALUATIONID,
                          collateralCustomerId = C.COLLATERALCUSTOMERID,
                          valuationComment = valPre.VALUATIONCOMMENT,
                          valuationName = q.VALUATIONNAME,
                          valuationReason = q.VALUATIONREASON,
                          operationId = valPre.OPERATIONID,
                          valuationPrerequisiteId = valPre.VALUATIONPREREQUISITEID,
                          approvalStatusId = atrail.APPROVALSTATUSID,
                          valuationRequestTypeId = valPre.VALUATIONREQUESTTYPEID,
                          //approvalStatusId = valPre.APPROVALSTATUSID,
                          approvalComment = atrail.COMMENT,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == valPre.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                      };

            //return res.GroupBy(O => O.collateralValuationId).Select(O => O.FirstOrDefault()).ToList();
            return res.ToList();
        }

        public bool SubmitApproval(ValuationPrerequisiteViewModel model)
        {
            bool response = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
                _workflow.StaffId = model.createdBy;
                _workflow.CompanyId = model.companyId;
                //_workflow.StatusId = (int) ApprovalStatusEnum.Processing;
                _workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                //_workflow.TargetId = model.collateralValuationId;
                _workflow.TargetId = model.valuationPrerequisiteId;
                _workflow.Comment = model.valuationComment;
                _workflow.OperationId = (int) OperationsEnum.CollateralValuationRequest;
                _workflow.DeferredExecution = true;
                _workflow.LogActivity();
                try
                {
                    if (_workflow.NewState == (int) ApprovalState.Ended)
                    {
                        var prereqisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.VALUATIONPREREQUISITEID == model.valuationPrerequisiteId && O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing).Select(O => O).FirstOrDefault();

                        //foreach (var prereqisite in prereqisites)
                        //{
                        prereqisite.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                        var valuerReport = _context.TBL_VALUATION_REPORT.Where(o => o.COLLATERALVALUATIONID == prereqisite.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();

                        if (valuerReport != null) 
                            valuerReport.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                        //}
                    }

                    response = _context.SaveChanges() > 0;
                    transaction.Commit();
                    return response;
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

        public bool UpdateValuationPrerequisiteStatus(int valuationPrerequisiteId, UserInfo user)
        {
            var prerequisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                        && O.VALUATIONPREREQUISITEID == valuationPrerequisiteId).FirstOrDefault();

            if (prerequisite != null)
            {
                prerequisite.APPROVALSTATUSID = (int)ApprovalStatusEnum.Referred;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ValuationPrerequisiteUpdated,
                    STAFFID = user.staffId,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = "Updated Valuation Prerequisite with code: " + prerequisite.VALUATIONPREREQUISITEID,
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
