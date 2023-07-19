using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        List<AlertsViewModel> alerts = new List<AlertsViewModel>();
        AlertsViewModel alert = new AlertsViewModel();


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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                 URL = model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            var entity = new TBL_COLLATERAL_VALUATION_PRE()
            {
                //COLLATERALCUSTOMERID = model.collateralCustomerId,
                COLLATERALVALUATIONID = model.collateralValuationId,
                VALUATIONREQUESTTYPEID = model.valuationRequestTypeId,
                VALUATIONCOMMENT = model.valuationComment,
                REFERENCENUMBER = referenceNumber,
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                 URL =model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                 URL = model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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

        public List<ValuationPrerequisiteViewModel> GetCollateralValuerIformation(int id)
        {
            var data = (from x in _context.TBL_VALUATION_REPORT
                       where x.DELETED == false && x.COLLATERALVALUATIONID == id
                        orderby x.VALUATIONREPORTID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer =  _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.FIRMNAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           whtAmount = x.WHTAMOUNT,
                           valuationComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,
                           operationId = (int) OperationsEnum.CollateralValuationRequest,
                           omv = x.OMV,
                           fsv = x.FSV
                       }).ToList();
            return data;
        }


        public ValuationPrerequisiteViewModel GetCollateralValuerIformations(int id)
        {
            var data = (from x in _context.TBL_VALUATION_REPORT
                        where x.DELETED == false && x.COLLATERALVALUATIONID == id
                        orderby x.VALUATIONREPORTID descending
                        select new ValuationPrerequisiteViewModel
                        {
                            valuerId = x.VALUERID,
                            valuer = _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.FIRMNAME).FirstOrDefault(),
                            collateralValuationId = x.COLLATERALVALUATIONID,
                            valuationFee = x.VALUATIONFEE,
                            accountNumber = x.ACCOUNTNUMBER,
                            wht = x.WHT,
                            whtAmount = x.WHTAMOUNT,
                            valuationComment = x.VALUERCOMMENT,
                            valuationReportId = x.VALUATIONREPORTID,
                            operationId = (int)OperationsEnum.CollateralValuationRequest,
                            omv = x.OMV,
                            fsv = x.FSV
                        }).FirstOrDefault();
            return data;
        }

        public List<ValuationPrerequisiteViewModel> GetAllCollateralValuerIformation()
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       where x.DELETED == false
                       orderby x.VALUATIONREPORTID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer = _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.FIRMNAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           valuationComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,
                           operationId = (int)OperationsEnum.CollateralValuationRequest,
                           omv = x.OMV,
                           fsv = x.FSV,
                           whtAmount = x.WHTAMOUNT
                       };
            return data.ToList();
        }

        public ValuationPrerequisiteViewModel GetAllCollateralValuerIformationById(int id)
        {
            var data = from x in _context.TBL_VALUATION_REPORT
                       where x.VALUATIONREPORTID == id
                       && x.DELETED == false
                       orderby x.VALUATIONREPORTID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuerId = x.VALUERID,
                           valuer = _context.TBL_ACCREDITEDCONSULTANT.Where(o => o.ACCREDITEDCONSULTANTID == x.VALUERID).Select(o => o.FIRMNAME).FirstOrDefault(),
                           collateralValuationId = x.COLLATERALVALUATIONID,
                           valuationFee = x.VALUATIONFEE,
                           accountNumber = x.ACCOUNTNUMBER,
                           wht = x.WHT,
                           whtAmount = x.WHTAMOUNT,
                           valuationComment = x.VALUERCOMMENT,
                           valuationReportId = x.VALUATIONREPORTID,
                           omv = x.OMV,
                           fsv = x.FSV

                       };
            return data.FirstOrDefault();
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
                OMV = model.omv,
                FSV = model.fsv,
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                 URL = model.applicationUrl,
                APPLICATIONDATE = _general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
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

        public bool UpdateCollateralValurerInfo(ValuationPrerequisiteViewModel model)
        {
            try
            {
                var exist = _context.TBL_VALUATION_REPORT.Find(model.valuationReportId);

                if (exist != null)
                {
                    exist.VALUERID = model.valuerId;
                    exist.COLLATERALVALUATIONID = model.collateralValuationId;
                    exist.VALUATIONFEE = model.valuationFee;
                    exist.ACCOUNTNUMBER = model.accountNumber;
                    exist.WHT = model.wht;
                    exist.WHTAMOUNT = model.whtAmount;
                    exist.VALUERCOMMENT = model.valuationComment;
                    exist.OMV = model.omv;
                    exist.FSV = model.fsv;
                }
                
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public bool UpdateCollateralNarration(ValuationPrerequisiteViewModel model)
        {
            try
            {
                var exist = _context.TBL_COLLATERAL_VALUATION.Find(model.valuationReportId);

                if (exist != null)
                {
                    exist.NARRATION = model.narration;
                }

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
            var result1 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
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
                       }).ToList();

            // fetching REFFERED valuation prerequisite
            // fetching DISAPPROVED valuation prerequisite
            var result2 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                           join atrail in _context.TBL_APPROVAL_TRAIL on O.VALUATIONPREREQUISITEID equals atrail.TARGETID
                           where O.COLLATERALVALUATIONID == collateralValuationId
                           //&& atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved
                           //&& atrail.RESPONSESTAFFID == null && atrail.APPROVALSTATEID != (short) ApprovalState.Ended
                           //&& atrail.LOOPEDSTAFFID == staffId
                           //&& ids.Contains((int) atrail.TOAPPROVALLEVELID)
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
                               approvalTrailId = atrail.APPROVALTRAILID,
                               loopedStaffId = atrail.LOOPEDSTAFFID,
                               approvalComment = atrail.COMMENT,
                               approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                           }).GroupBy(l => l.valuationPrerequisiteId).Select(g => g.OrderByDescending(l => l.approvalTrailId).FirstOrDefault())
                           .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && l.loopedStaffId == staffId)).ToList();

            // fetching DISAPPROVED valuation prerequisite
            //var result3 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
            //            join atrail in _context.TBL_APPROVAL_TRAIL on O.VALUATIONPREREQUISITEID equals atrail.TARGETID
            //            where O.COLLATERALVALUATIONID == collateralValuationId
            //            //&& atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Disapproved || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
            //            //&& atrail.RESPONSESTAFFID == null
            //            //&& ids.Contains((int)atrail.TOAPPROVALLEVELID)
            //            && atrail.OPERATIONID == (int)OperationsEnum.CollateralValuationRequest
            //             orderby O.VALUATIONPREREQUISITEID descending
            //            select new ValuationPrerequisiteViewModel
            //            {
            //                valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
            //                collateralValuationId = O.COLLATERALVALUATIONID,
            //                valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
            //                valuationComment = O.VALUATIONCOMMENT,
            //                operationId = O.OPERATIONID,
            //                customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID.Value).FirstOrDefault(),
            //                approvalStatusId = atrail.APPROVALSTATUSID,
            //                approvalComment = atrail.COMMENT,
            //                approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
            //            }).ToList();


            var result = result1.Union(result2);
            return result.ToList();
        }


        public List<ValuationPrerequisiteViewModel> GetAllValuationPrerequisitesListById(int staffId, int collateralValuationId)
        {
            var collateralId = _context.TBL_COLLATERAL_VALUATION.Where(x => x.COLLATERALVALUATIONID == collateralValuationId)
                                                                    .Select(x => x.COLLATERALCUSTOMERID).FirstOrDefault();

            var result1 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                           where O.COLLATERALVALUATIONID == collateralValuationId
                           orderby O.VALUATIONPREREQUISITEID descending
                           select new ValuationPrerequisiteViewModel
                           {
                               valuationPrerequisiteId = O.VALUATIONPREREQUISITEID,
                               collateralValuationId = O.COLLATERALVALUATIONID,
                               valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(x => x.VALUATIONREQUESTTYPEID == O.VALUATIONREQUESTTYPEID).Select(x => x.VALUATIONREQUESTTYPE).FirstOrDefault(),
                               valuationComment = O.VALUATIONCOMMENT,
                               operationId = O.OPERATIONID,
                               customerId = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => x.CUSTOMERID.Value).FirstOrDefault(),
                               approvalStatusId = O.APPROVALSTATUSID,
                               approvalComment = O.VALUATIONCOMMENT,
                               approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == o.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                           }).ToList();

            var result2 = (from O in _context.TBL_COLLATERAL_VALUATION_PRE
                           join atrail in _context.TBL_APPROVAL_TRAIL on O.VALUATIONPREREQUISITEID equals atrail.TARGETID
                           where O.COLLATERALVALUATIONID == collateralValuationId
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
                               approvalTrailId = atrail.APPROVALTRAILID,
                               loopedStaffId = atrail.LOOPEDSTAFFID,
                               approvalComment = atrail.COMMENT,
                               approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                           }).GroupBy(l => l.valuationPrerequisiteId).Select(g => g.OrderByDescending(l => l.approvalTrailId).FirstOrDefault())
                           .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                && l.loopedStaffId == staffId)).ToList();

            var result = result1.Union(result2);
            return result.ToList();
        }

        public WorkflowResponse GoForCollateralValuationApproval(ValuationPrerequisiteViewModel entity)
        {
            var prerequisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.VALUATIONPREREQUISITEID == entity.valuationPrerequisiteId && (O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)).Select(O => O).FirstOrDefault();

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
                    _workflow.Comment = "Request for collateral valuation approval";
                    _workflow.OperationId = (int) OperationsEnum.CollateralValuationRequest;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();
                }
            }
            catch (Exception ex) { }

            _context.SaveChanges();
            _workflow.Response.responseMessage = prerequisite?.REFERENCENUMBER;
            return _workflow.Response;
        }

        public String ResponseMessage(WorkflowResponse response, string itemHeading)
        {
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    return "The " + itemHeading + " request has been APPROVED successfully";
                }
                else
                {
                    return "The " + itemHeading + " request has been DISAPPROVED successfully";
                }
            }

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
                          divisionCode = (from p in _context.TBL_PROFILE_BUSINESS_UNIT join c in _context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == c.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                          divisionShortCode = (from p in _context.TBL_PROFILE_BUSINESS_UNIT join c in _context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == C.COLLATERALCUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                          valuationComment = V.VALUATIONCOMMENT,
                          valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                          collateralCode = C.COLLATERALCODE,
                          collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                          valuationPrerequisiteId = V.VALUATIONPREREQUISITEID,
                          valuationReason = T.VALUATIONREASON,
                          collateralValuationId = V.COLLATERALVALUATIONID,
                          operationId = V.OPERATIONID,
                          approvalStatusId = atrail.APPROVALSTATUSID,
                          referenceNumber = V.REFERENCENUMBER,
                          approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                          collateralCustomerId = C.COLLATERALCUSTOMERID,
                          systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,

                      };

            return res.ToList();
        }


        public IEnumerable<ValuationPrerequisiteViewModel> GetAllValuationRequestList()
        {
            var res = (from C in _context.TBL_COLLATERAL_CUSTOMER
                       join T in _context.TBL_COLLATERAL_VALUATION on C.COLLATERALCUSTOMERID equals T.COLLATERALCUSTOMERID
                       join V in _context.TBL_COLLATERAL_VALUATION_PRE on T.COLLATERALVALUATIONID equals V.COLLATERALVALUATIONID
                       join atrail in _context.TBL_APPROVAL_TRAIL on V.VALUATIONPREREQUISITEID equals atrail.TARGETID
                       where 
                       C.DELETED == false 
                       && (V.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved 
                       || V.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                       || V.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                       || V.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)
                       
                       orderby V.VALUATIONPREREQUISITEID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           valuationComment = V.VALUATIONCOMMENT,
                           customerName = _context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == C.CUSTOMERID).Select(cc => cc.FIRSTNAME + " " + cc.MIDDLENAME + " " + cc.LASTNAME).FirstOrDefault(),
                           valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == V.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                           collateralCode = C.COLLATERALCODE,
                           collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                           valuationPrerequisiteId = V.VALUATIONPREREQUISITEID,
                           valuationReason = T.VALUATIONREASON,
                           valuationName = T.VALUATIONNAME,
                           valuationFee = _context.TBL_VALUATION_REPORT.Where(r => r.COLLATERALVALUATIONID == T.COLLATERALVALUATIONID).Select(r => r.VALUATIONFEE).FirstOrDefault(),
                           collateralValuationId = V.COLLATERALVALUATIONID,
                           operationId = V.OPERATIONID,
                           targetId = atrail.TARGETID,
                           approvalTrailId = atrail.APPROVALTRAILID,
                           referenceNumber = V.REFERENCENUMBER,
                           approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                           collateralCustomerId = C.COLLATERALCUSTOMERID,
                           createdByName = _context.TBL_STAFF.Where(cc => cc.STAFFID == V.CREATEDBY).Select(cc => cc.FIRSTNAME + " " + cc.MIDDLENAME + " " + cc.LASTNAME).FirstOrDefault(),
                       }).ToList();
            var data = res.GroupBy(x => x.targetId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.targetId); ;
            return data;
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
            var staffs = _general.GetStaffRlieved(staffId);
            //_context.Configuration.ProxyCreationEnabled = false;

            var res = (from valPre in _context.TBL_COLLATERAL_VALUATION_PRE
                       join q in _context.TBL_COLLATERAL_VALUATION on valPre.COLLATERALVALUATIONID equals q.COLLATERALVALUATIONID
                       join C in _context.TBL_COLLATERAL_CUSTOMER on q.COLLATERALCUSTOMERID equals C.COLLATERALCUSTOMERID
                       join atrail in _context.TBL_APPROVAL_TRAIL on valPre.VALUATIONPREREQUISITEID equals atrail.TARGETID
                       join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                       //where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                       where atrail.RESPONSESTAFFID == null
                        && atrail.LOOPEDSTAFFID == null
                        && atrail.APPROVALSTATEID != (int)ApprovalState.Ended
                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                        && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                        && atrail.OPERATIONID == (int)OperationsEnum.CollateralValuationRequest
                        && atrail.LOOPEDSTAFFID == null
                       orderby valPre.VALUATIONPREREQUISITEID descending
                       select new ValuationPrerequisiteViewModel
                       {
                           divisionCode = (from p in _context.TBL_PROFILE_BUSINESS_UNIT join c in _context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == c.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                           divisionShortCode = (from p in _context.TBL_PROFILE_BUSINESS_UNIT join c in _context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == C.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                           narration = q.NARRATION,
                            customerName = cus.FIRSTNAME + " " + cus.LASTNAME + " " + cus.MAIDENNAME,
                            customerId = cus.CUSTOMERID,
                            customerAccount = _context.TBL_CASA.Where(c=>c.CUSTOMERID == cus.CUSTOMERID).Select(c=>c.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                            collateralCode = C.COLLATERALCODE,
                            collateralType = _context.TBL_COLLATERAL_TYPE.Where(O => O.COLLATERALTYPEID == C.COLLATERALTYPEID).Select(O => O.COLLATERALTYPENAME).FirstOrDefault(),
                            collateralValue = C.COLLATERALVALUE,
                            collateralValuationId = q.COLLATERALVALUATIONID,
                            collateralCustomerId = C.COLLATERALCUSTOMERID,
                            currentApprovalLevelId = atrail.TOAPPROVALLEVELID,
                            valuationComment = valPre.VALUATIONCOMMENT,
                            valuationName = q.VALUATIONNAME,
                            valuationReason = q.VALUATIONREASON,
                            operationId = valPre.OPERATIONID,
                            valuationPrerequisiteId = valPre.VALUATIONPREREQUISITEID,
                            referenceNumber = valPre.REFERENCENUMBER,
                            approvalStatusId = atrail.APPROVALSTATUSID,
                            approvalTrailId = atrail.APPROVALTRAILID,
                            valuationRequestTypeId = valPre.VALUATIONREQUESTTYPEID,
                            //approvalStatusId = valPre.APPROVALSTATUSID,
                            approvalComment = atrail.COMMENT,
                            systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                            approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == valPre.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),
                       }).ToList();

            foreach(var v in res)
            {
                var detailIds = _context.TBL_LOAN_APPLICATION_COLLATERL.Where(p => p.COLLATERALCUSTOMERID == v.collateralCustomerId && p.DELETED == false).Select(p => p.LOANAPPLICATIONDETAILID);
                var facilities = _context.TBL_LOAN_APPLICATION_DETAIL.Where(d => detailIds.Contains(d.LOANAPPLICATIONDETAILID)).ToList();
                v.facilityAmount = facilities.Sum(p => p.APPROVEDAMOUNT * (decimal)p.EXCHANGERATE);
            }

            //return res.GroupBy(O => O.collateralValuationId).Select(O => O.FirstOrDefault()).ToList();
            return res;
        }

        public List<ValuationPrerequisiteViewModel> SearchForCollateralValuation(string searchString)
        {
            int[] operations = { (int)OperationsEnum.CollateralValuationRequest };

            searchString = searchString.Trim().ToLower();

            var valuations = (from valPre in _context.TBL_COLLATERAL_VALUATION_PRE
                                join q in _context.TBL_COLLATERAL_VALUATION on valPre.COLLATERALVALUATIONID equals q.COLLATERALVALUATIONID
                                join C in _context.TBL_COLLATERAL_CUSTOMER on q.COLLATERALCUSTOMERID equals C.COLLATERALCUSTOMERID
                                join atrail in _context.TBL_APPROVAL_TRAIL on valPre.VALUATIONPREREQUISITEID equals atrail.TARGETID
                                join cus in _context.TBL_CUSTOMER on C.CUSTOMERID equals cus.CUSTOMERID
                                where ((atrail.OPERATIONID == (int)OperationsEnum.CollateralValuationRequest)
                                && (valPre.REFERENCENUMBER.Trim().ToLower().Contains(searchString))
                                || cus.CUSTOMERCODE.Contains(searchString)
                                || cus.FIRSTNAME.ToLower().Contains(searchString)
                                || cus.MIDDLENAME.ToLower().Contains(searchString)
                                || cus.LASTNAME.ToLower().Contains(searchString)
                                )
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
                                    referenceNumber = valPre.REFERENCENUMBER,
                                    approvalStatusId = (int) atrail.APPROVALSTATUSID,
                                    approvalTrailId = atrail.APPROVALTRAILID,
                                    valuationRequestTypeId = valPre.VALUATIONREQUESTTYPEID,
                                    arrivalDate = atrail.ARRIVALDATE,
                                    approvalComment = atrail.COMMENT,
                                    approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME.ToUpper()).FirstOrDefault(),
                                    valuationRequestType = _context.TBL_VALUATION_REQUEST_TYPE.Where(O => O.VALUATIONREQUESTTYPEID == valPre.VALUATIONREQUESTTYPEID).Select(O => O.VALUATIONREQUESTTYPE).FirstOrDefault(),

                                    currentApprovalLevelId = atrail.TOAPPROVALLEVELID,
                                    currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? ((atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && atrail.LOOPEDSTAFFID != null) ? _context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == atrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : _context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME) : "N/A",
                                    responsiblePerson = atrail.TOSTAFFID == null ? "N/A" : atrail.TBL_STAFF1.STAFFCODE + " - " + atrail.TBL_STAFF1.FIRSTNAME + " " + atrail.TBL_STAFF1.MIDDLENAME + " " + atrail.TBL_STAFF1.LASTNAME,
                                    createdBy = (int) q.CREATEDBY,
                                }).GroupBy(a => a.valuationPrerequisiteId).Select(g => g.OrderByDescending(l => l.approvalTrailId)
                                    .FirstOrDefault())
                                    .ToList();
            
            List<ValuationPrerequisiteViewModel> vals = new List<ValuationPrerequisiteViewModel>();
            vals.AddRange(valuations);

            foreach (var v in vals)
            {
                var detailIds = _context.TBL_LOAN_APPLICATION_COLLATERL.Where(p => p.COLLATERALCUSTOMERID == v.collateralCustomerId && p.DELETED == false).Select(p => p.LOANAPPLICATIONDETAILID);
                var facilities = _context.TBL_LOAN_APPLICATION_DETAIL.Where(d => detailIds.Contains(d.LOANAPPLICATIONDETAILID)).ToList();
                v.facilityAmount = facilities.Sum(p => p.APPROVEDAMOUNT * (decimal)p.EXCHANGERATE);
            }
            return vals;
        }

        public WorkflowResponse SubmitApproval(ValuationPrerequisiteViewModel model)
        {
            bool response = false;
            var prereqisite = new TBL_COLLATERAL_VALUATION_PRE();

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
                    if (_workflow.NewState != (int)ApprovalState.Ended)
                    {
                        var approvingStaff = _context.TBL_STAFF.Find(model.createdBy);
                        var staffRole = _context.TBL_STAFF_ROLE.Where(r => r.STAFFROLEID == approvingStaff.STAFFROLEID).FirstOrDefault();
                        prereqisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.VALUATIONPREREQUISITEID == model.valuationPrerequisiteId && O.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing).Select(O => O).FirstOrDefault();
                        var valuerReport = _context.TBL_VALUATION_REPORT.Where(o => o.COLLATERALVALUATIONID == prereqisite.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();

                        if (staffRole.STAFFROLECODE == "VAL CR DOC OFF")
                        {
                            var update = _context.TBL_COLLATERAL_VALUATION_PRE.Find(prereqisite.VALUATIONPREREQUISITEID);
                            if(update.NUMBEROFTIMESAPPROVE == null)
                            {
                                update.NUMBEROFTIMESAPPROVE = 0;
                            }
                            update.NUMBEROFTIMESAPPROVE = (update.NUMBEROFTIMESAPPROVE + 1);
                            _context.SaveChanges();
                        }

                        if ((staffRole.STAFFROLECODE == "CRDT DOC GH" || staffRole.STAFFROLECODE == "CR DOC MGR") && (prereqisite.NUMBEROFTIMESAPPROVE <= 1 || prereqisite.NUMBEROFTIMESAPPROVE == null) && (valuerReport.OMV < 1 || valuerReport.OMV == null))
                        {
                            var collateral = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALVALUATIONID == valuerReport.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();
                            var valuer = _context.TBL_COLLATERAL_VALUER.Where(o => o.COLLATERALVALUERID == valuerReport.VALUERID).Select(o => o.NAME).FirstOrDefault();

                            var valuationOfficer = _context.TBL_STAFF.Find(valuerReport.CREATEDBY);
                            var accountOfficer = _context.TBL_STAFF.Find(collateral.CREATEDBY);
                            var valuerDetail = _context.TBL_ACCREDITEDCONSULTANT.Find(valuerReport.VALUERID);

                            var rem = _context.TBL_STAFF.Find(accountOfficer.SUPERVISOR_STAFFID);
                            var customerCollateral = _context.TBL_COLLATERAL_CUSTOMER.Find(collateral.COLLATERALCUSTOMERID);
                            var collateralType = _context.TBL_COLLATERAL_TYPE.Find(customerCollateral.COLLATERALTYPEID);
                            var customer = _context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == customerCollateral.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault();
                            var collAddress = _context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(c => c.COLLATERALCUSTOMERID == customerCollateral.COLLATERALCUSTOMERID).Select(c => c.PROPERTYADDRESS).FirstOrDefault();
                            var staffFullName = accountOfficer?.FIRSTNAME + " " + accountOfficer?.LASTNAME;

                            var letter = _context.TBL_ALERT_TITLE.Where(x=>x.BINDINGMETHOD == "CollateralValuationNotification").Select(x=>x).FirstOrDefault();
                            var letterBody = letter?.TEMPLATE;
                            var letterTitle = letter?.TITLE;
                            letterBody = letterBody.Replace("@{{month}}", DateTime.Now.Month.ToString());
                            letterBody = letterBody.Replace("@{{year}}", DateTime.Now.Year.ToString());
                            letterBody = letterBody.Replace("@{{referenceNumber}}", prereqisite?.REFERENCENUMBER);
                            letterBody = letterBody.Replace("@{{letterDate}}", DateTime.Now.ToString("dd-MM-yyyy"));
                            letterBody = letterBody.Replace("@{{nameAndAddress}}", valuerDetail?.FIRMNAME + "</br>" + valuerDetail?.ADDRESS);
                            letterBody = letterBody.Replace("@{{assetType}}", collateralType?.COLLATERALTYPENAME);
                            letterBody = letterBody.Replace("@{{customerName}}", customer);
                            letterBody = letterBody.Replace("@{{collateralAddress}}", collAddress);
                            letterBody = letterBody.Replace("@{{initiator}}", staffFullName);
                            letterBody = letterBody.Replace("@{{initiatorPhone}}", accountOfficer?.PHONE);

                            letterTitle = letterTitle.Replace("@{{customerName}}", customer.ToUpper());
                            letterTitle = letterTitle.Replace("@{{valuerName}}", valuerDetail?.FIRMNAME.ToUpper());

                            var emailList = accountOfficer?.EMAIL + ";" + rem?.EMAIL + ";" + valuerDetail?.EMAILADDRESS + ";" + valuationOfficer?.EMAIL + ";" + letter?.DEFAULTEMAIL;
                            alert.receiverEmailList.Add(emailList);
                            LogEmailAlert(letterBody, letterTitle, alert.receiverEmailList, "98007", 98007, letter.BINDINGMETHOD);
                        }

                        if (staffRole.STAFFROLECODE == "VAL CR DOC OFF" && prereqisite.NUMBEROFTIMESAPPROVE > 1 && valuerReport.OMV > 0 && valuerReport.FSV > 0)
                        {
                            var collateral = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALVALUATIONID == valuerReport.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();
                            var valuer = _context.TBL_COLLATERAL_VALUER.Where(o => o.COLLATERALVALUERID == valuerReport.VALUERID).Select(o => o.NAME).FirstOrDefault();

                            var valuationOfficer = _context.TBL_STAFF.Find(valuerReport.CREATEDBY);
                            var accountOfficer = _context.TBL_STAFF.Find(collateral.CREATEDBY);
                            var valuerDetail = _context.TBL_ACCREDITEDCONSULTANT.Find(valuerReport.VALUERID);

                            var rem = _context.TBL_STAFF.Find(accountOfficer.SUPERVISOR_STAFFID);
                            var customerCollateral = _context.TBL_COLLATERAL_CUSTOMER.Find(collateral.COLLATERALCUSTOMERID);
                            var collateralType = _context.TBL_COLLATERAL_TYPE.Find(customerCollateral.COLLATERALTYPEID);
                            var customer = _context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == customerCollateral.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault();
                            var collAddress = _context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(c => c.COLLATERALCUSTOMERID == customerCollateral.COLLATERALCUSTOMERID).Select(c => c.PROPERTYADDRESS).FirstOrDefault();
                            var staffFullName = accountOfficer?.FIRSTNAME + " " + accountOfficer?.LASTNAME;

                            var tempResult = string.Empty;
                            var omv = string.Format("{0:#,##.00}", Convert.ToDecimal(valuerReport?.OMV));
                            var fsv = string.Format("{0:#,##.00}", Convert.ToDecimal(valuerReport?.FSV));
                            var fee = string.Format("{0:#,##.00}", Convert.ToDecimal(valuerReport?.VALUATIONFEE));
                            tempResult = $@"
                             <table cellpadding='0' cellspacing='0' border='1' width='800px'>
                                <tr>
                                    <td><b>PROPERTY ADDRESS</b></td>
                                    <td><b>OMV</b></td>
                                    <td><b>FSV</b></td>
                                    <td><b>VALUATION FEE TO BE DEBITED(NGN)</b></td>
                                </tr>
                             ";
                            tempResult = tempResult + $@"
                                <tr>
                                    <td>{collAddress}</td>
                                    <td>{$"{omv}"}</td>
                                    <td>{$"{fsv}"}</td>
                                    <td>{$"{fee}"}</td>
                                </tr>
                                ";
                    tempResult = tempResult + $"</table><br/>";

                    var letter = _context.TBL_ALERT_TITLE.Where(x => x.BINDINGMETHOD == "CollateralValuationSecondNotification").Select(x => x).FirstOrDefault();
                            var letterBody = letter?.TEMPLATE;
                            var letterTitle = letter?.TITLE;
                            letterBody = letterBody.Replace("@{{customerName}}", customer);
                            letterBody = letterBody.Replace("@{{valuationDetail}}", tempResult);
                            letterBody = letterBody.Replace("@{{initiator}}", staffFullName);
                            letterTitle = letterTitle.Replace("@{{customerName}}", customer.ToUpper());

                            var emailList = accountOfficer?.EMAIL + ";" + rem?.EMAIL + ";" + valuerDetail?.EMAILADDRESS + ";" + valuationOfficer?.EMAIL + ";" + letter?.DEFAULTEMAIL;
                            alert.receiverEmailList.Add(emailList);
                            LogEmailAlert(letterBody, letterTitle, alert.receiverEmailList, "98007", 98007, letter.BINDINGMETHOD);
                        }
                    }
                        
                    if (_workflow.NewState == (int) ApprovalState.Ended)
                    {
                        prereqisite = _context.TBL_COLLATERAL_VALUATION_PRE.Where(O => O.VALUATIONPREREQUISITEID == model.valuationPrerequisiteId && O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing).Select(O => O).FirstOrDefault();

                        //foreach (var prereqisite in prereqisites)
                        //{
                        prereqisite.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                        var valuerReport = _context.TBL_VALUATION_REPORT.Where(o => o.COLLATERALVALUATIONID == prereqisite.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();
                        
                        if (valuerReport != null) 
                            valuerReport.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                        //}

                        var collateral = _context.TBL_COLLATERAL_VALUATION.Where(o => o.COLLATERALVALUATIONID == valuerReport.COLLATERALVALUATIONID).Select(o => o).FirstOrDefault();
                        var valuer = _context.TBL_COLLATERAL_VALUER.Where(o => o.COLLATERALVALUERID == valuerReport.VALUERID).Select(o => o.NAME).FirstOrDefault();

                        var valuationOfficer = _context.TBL_STAFF.Find(valuerReport.CREATEDBY);
                        var accountOfficer = _context.TBL_STAFF.Find(collateral.CREATEDBY);
                        var valuerDetail = _context.TBL_ACCREDITEDCONSULTANT.Find(valuerReport.VALUERID);

                        var rem = _context.TBL_STAFF.Find(accountOfficer.SUPERVISOR_STAFFID);
                        var customerCollateral = _context.TBL_COLLATERAL_CUSTOMER.Find(collateral.COLLATERALCUSTOMERID);
                        var collateralType = _context.TBL_COLLATERAL_TYPE.Find(customerCollateral.COLLATERALTYPEID);
                        var customer = _context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == customerCollateral.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault();
                        var collAddress = _context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(c => c.COLLATERALCUSTOMERID == customerCollateral.COLLATERALCUSTOMERID).Select(c => c.PROPERTYADDRESS).FirstOrDefault();

                        var updateValues = _context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(c => c.COLLATERALCUSTOMERID == customerCollateral.COLLATERALCUSTOMERID).Select(c => c).FirstOrDefault();
                        updateValues.FORCEDSALEVALUE = valuerReport.FSV;
                        updateValues.OPENMARKETVALUE = valuerReport.OMV;
                        updateValues.VALUATIONAMOUNT = valuerReport.FSV;
                        customerCollateral.COLLATERALVALUE = (decimal)valuerReport.FSV;

                        var staffFullName = accountOfficer?.FIRSTNAME + " " + accountOfficer?.LASTNAME +" "+ accountOfficer?.PHONE;
                        var letter = _context.TBL_ALERT_TITLE.Where(x => x.BINDINGMETHOD == "CollateralValuationSecondNotification").Select(x => x).FirstOrDefault();
                        var letterBody = letter?.TEMPLATE;
                        var letterTitle = letter?.TITLE;
                        letterBody = letterBody.Replace("@{{month}}", DateTime.Now.Month.ToString());
                        letterBody = letterBody.Replace("@{{year}}", DateTime.Now.Year.ToString());
                        letterBody = letterBody.Replace("@{{referenceNumber}}", prereqisite?.REFERENCENUMBER);
                        letterBody = letterBody.Replace("@{{letterDate}}", DateTime.Now.ToString("dd-MM-yyyy"));
                        letterBody = letterBody.Replace("@{{nameAndAddress}}", valuerDetail?.FIRMNAME + "</br>" + valuerDetail?.ADDRESS);
                        letterBody = letterBody.Replace("@{{assetType}}", collateralType?.COLLATERALTYPENAME);
                        letterBody = letterBody.Replace("@{{customerName}}", customer);
                        letterBody = letterBody.Replace("@{{collateralAddress}}", collAddress);
                        letterBody = letterBody.Replace("@{{initiator}}", staffFullName);
                        letterBody = letterBody.Replace("@{{initiatorPhone}}", accountOfficer?.PHONE);

                        letterTitle = letterTitle.Replace("@{{customerName}}", customer.ToUpper());
                        letterTitle = letterTitle.Replace("@{{initiator}}", staffFullName.ToUpper());

                        var emailList = accountOfficer?.EMAIL + ";" + rem?.EMAIL + ";" + valuerDetail?.EMAILADDRESS + ";" + valuationOfficer?.EMAIL + ";" + letter?.DEFAULTEMAIL;
                        alert.receiverEmailList.Add(emailList);
                        LogEmailAlert(letterBody, letterTitle, alert.receiverEmailList, "98007", 98007, letter.BINDINGMETHOD);
                    }

                    response = _context.SaveChanges() > 0;
                    transaction.Commit();
                    _workflow.Response.responseMessage = prereqisite?.REFERENCENUMBER;
                    return _workflow.Response;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
                //return false;
            }
        }


        private string GetBusinessUsersEmailsToGroupHead(string accountOfficerMIsCode)
        {
            string emailList = "";

            var accountOfficer = _context.TBL_STAFF.Where(x => x.MISCODE.ToLower() == accountOfficerMIsCode.ToLower()).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = _context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = _context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = _context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }

        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = title;
                string messageContent = messageBody;
                //string templateUrl = context.TBL_ALERT_GENERAL_TEMPLATE.Find(1).TEMPLATEBODY; //"~/EmailTemp/Monitoring.html";
                //string mailBody = templateUrl.Replace("{Description}", messageContent);  //EmailHelpers.PopulateBody(messageContent, templateUrl); 
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = messageContent,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = referenceCode,
                    targetId = targetId,
                    operationMethod = operationMehtod,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                new SecureException(ex.ToString());
            }
        }
        private void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId,
                OPERATIONMETHOD = model.operationMethod
            };

            _context.TBL_MESSAGE_LOG.Add(message);
            _context.SaveChanges();
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                _audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return _context.SaveChanges() > 0;
            }

            return false;
        }

    }
}
