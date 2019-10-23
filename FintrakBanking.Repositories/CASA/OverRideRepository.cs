using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.CASA
{

    public class OverRideRepository : IOverRideRepository
    {
        private readonly FinTrakBankingContext _context;
        private IAuditTrailRepository _auditTrail;
        private readonly IGeneralSetupRepository _genSetup;
        private IWorkflow _workFlow;

        public OverRideRepository(
            IAuditTrailRepository auditTrail,
            IWorkflow workFlow, IGeneralSetupRepository genSetup,
            FinTrakBankingContext context)
        {
            this._context = context;
            this._auditTrail = auditTrail;
            this._genSetup = genSetup;
            this._workFlow = workFlow;
        }

        public bool AddOverRideRequest(IEnumerable<OverrideDetailVeiwModel> entity)
        {
            bool status = false;

            foreach (var item in entity)
            {

                var data = new TBL_OVERRIDE_DETAIL
                {
                    CUSTOMERCODE = item.customerCode,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    CREATEDBY = item.createdBy,
                    REASON = item.reason,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    ISUSED = false,
                    OVERRIDE_ITEMID = item.overrideItemId,
                    SOURCE_REFERENCE_NUMBER = item.sourceReferenceNumber
                };
                _context.TBL_OVERRIDE_DETAIL.Add(data);

                // Audit Section ---------------------------            

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.OverrideRequest,
                    STAFFID = item.staffId,
                    BRANCHID = (short)item.userBranchId,
                    DETAIL = $"Override Request Initaited",
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = item.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                   
                   
                };
                this._auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                //using (var trans = _context.Database.BeginTransaction())
                //{
                    status = _context.SaveChanges() > 0;

                    _workFlow.StaffId = item.createdBy;
                    _workFlow.CompanyId = item.companyId;
                    _workFlow.StatusId = (int)ApprovalStatusEnum.Pending; 
                    _workFlow.TargetId = data.OVERRIDE_DETAILID;
                    _workFlow.Comment = "Initiation";
                    _workFlow.OperationId = (int)OperationsEnum.OverrideRequest;
                   // _workFlow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    _workFlow.ExternalInitialization = true;

                    var response = _workFlow.LogActivity();



                   // status = _context.SaveChanges() > 0;
                }
          //  }

            return status;
        } 

        public int EffectOverride(string customerCode, int overrideItemId, string sourceRef)
        {
            int result = 0;

            var data = _context.TBL_OVERRIDE_DETAIL.Where(e => e.ISUSED == false
                                                               && e.CUSTOMERCODE == customerCode
                                                               && e.APPROVALSTATUSID ==
                                                               (int) ApprovalStatusEnum.Approved
                                                               && e.OVERRIDE_ITEMID == overrideItemId).FirstOrDefault();
            if (data != null)
            {
                data.ISUSED = true;
                data.SOURCE_REFERENCE_NUMBER = sourceRef;
                data.DATETIMEUSED = _genSetup.GetApplicationDate();
                
               result = _context.SaveChanges();
               
            }

            return result;
        }

         

        public bool DeleteOverRideRequest(OverrideDetailVeiwModel entity)
        {
            var data = _context.TBL_OVERRIDE_DETAIL.Where(c => c.OVERRIDE_DETAILID == entity.overrideDetailId);
             
             _context.TBL_OVERRIDE_DETAIL .RemoveRange(data);
          return   _context.SaveChanges() > 0;
        }

        public IEnumerable<OverrideItemVeiwModel> GetAllOverRideItems()
        {
            var data = _context.TBL_OVERRIDE_ITEM.Where(c=>c.OVERRIDE_ITEMID != (int)OverrideItem.BlackbookOverride && c.OVERRIDE_ITEMID!=(int)OverrideItem.CAMSOL_Override).Select(c => new OverrideItemVeiwModel
            {
               itemId = c.OVERRIDE_ITEMID ,
                itemName = c.OVERIDE_ITEMNAME 
            });
            return data.ToList();
        }



        private IQueryable<OverrideDetailVeiwModel> AllOverRideRequest()
        {
            var data = _context.TBL_OVERRIDE_DETAIL;
            var cust = _context.TBL_CUSTOMER.Select(d => new
            {
                customer = d.LASTNAME + " " + d.FIRSTNAME + " " + d.MIDDLENAME,
                d.CUSTOMERCODE,
                d.CUSTOMERID
            });

            var over = from a in cust
                       join d in data on a.CUSTOMERCODE equals d.CUSTOMERCODE

                       select new OverrideDetailVeiwModel
                       {
                           approvedStatusId = d.APPROVALSTATUSID,
                           createdBy = d.CREATEDBY,
                           itemId = d.OVERRIDE_ITEMID,
                           itemName = d.TBL_OVERRIDE_ITEM.OVERIDE_ITEMNAME,
                           approvalStatus = d.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                           customerCode = d.CUSTOMERCODE,
                           customerName = a.customer,
                           sourceReferenceNumber = d.SOURCE_REFERENCE_NUMBER,
                           isUsed = d.ISUSED,
                           customerId = a.CUSTOMERID,
                           dateTimeCreated = d.DATETIMECREATED,
                           overrideDetailId = d.OVERRIDE_DETAILID
                       };

            return over;
        }

        public IEnumerable<OverrideDetailVeiwModel> GetAllOverRideRequest()
        {
            return AllOverRideRequest().OrderBy(c=> c.customerCode).ToList();            
        }

        public OverrideDetailVeiwModel GetOverRideRequestById(int id)
        {
           return AllOverRideRequest().FirstOrDefault(e => e.overrideDetailId == id);
        }

        public IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByOverRideItemsId(int id)
        {
            return AllOverRideRequest().Where(e => e.overrideItemId == id);
        }

        public IEnumerable<OverrideDetailVeiwModel> GetOverRideRequestByReferenceNumber(string refNo)
        {
            return AllOverRideRequest().Where(e => e.sourceReferenceNumber == refNo);
        }

        public bool UpdateOverRideRequest(OverrideDetailVeiwModel entity)
        {
            var data = _context.TBL_OVERRIDE_DETAIL.FirstOrDefault(c => c.OVERRIDE_DETAILID == entity.overrideDetailId);
            if (data != null)
            {
                data.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                data.CREATEDBY = entity.createdBy;
                //data.CUSTOMERID = entity.customerId;
                data.DATETIMECREATED = _genSetup.GetApplicationDate();
                data.ISUSED = entity.isUsed;
                data.OVERRIDE_ITEMID = entity.overrideItemId;
                data.SOURCE_REFERENCE_NUMBER = entity.sourceReferenceNumber;
            }

            return _context.SaveChanges() > 0;
        }


        public int GoForApproval(ApproveOverrideVeiwModel entity)
        {
            _workFlow.StaffId = entity.staffId;
            _workFlow.CompanyId = entity.companyId;
            _workFlow.StatusId = (int)ApprovalStatusEnum.Processing;
            _workFlow.TargetId = entity.overrideDetailId;
            _workFlow.Comment = entity.statusComment;
            _workFlow.OperationId = entity.operationId;
            _workFlow.DeferredExecution = true;
            _workFlow.LogActivity();
            
            return _workFlow.NewState ;
        }


        public bool ApproveOverride(ApproveOverrideVeiwModel entity )
        {
            bool result = false;
            using (var trans = _context.Database.BeginTransaction())
            {
                try
                {

                    if (GoForApproval(entity) == (int)ApprovalState.Ended)
                    {
                        var data = _context.TBL_OVERRIDE_DETAIL.FirstOrDefault(c => c.OVERRIDE_DETAILID == entity.overrideDetailId);
                        if (data != null)
                        {
                            data.APPROVALSTATUSID = (short)entity.approvedStatusId;

                            // Audit Section ---------------------------            

                            var audit1 = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.ApprovedOverrideRequest,
                                STAFFID = entity.staffId,
                                BRANCHID = (short)entity.userBranchId,
                                DETAIL = $" Override Request approval was effected successfully",
                                URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now,
                                DEVICENAME = CommonHelpers.GetDeviceName(),
                                OSNAME = CommonHelpers.FriendlyName()

                                
                            };
                            this._auditTrail.AddAuditTrail(audit1);

                            //end of Audit section -------------------------------

                            result = _context.SaveChanges() > 0;
                        }
                    }
                    else
                    {
                        // Audit Section ---------------------------            

                        var audit1 = new TBL_AUDIT
                        {
                            AUDITTYPEID = (short)AuditTypeEnum.AttemptedApproveOverrideRequest,
                            STAFFID = entity.staffId,
                            BRANCHID = (short)entity.userBranchId,
                            DETAIL = $" Override Request approval was effected successfully",
                            IPADDRESS = entity.userIPAddress,
                            URL = entity.applicationUrl,
                            APPLICATIONDATE = _genSetup.GetApplicationDate(),
                            SYSTEMDATETIME = DateTime.Now,
                            DEVICENAME = CommonHelpers.GetDeviceName(),
                            OSNAME = CommonHelpers.FriendlyName()
                        };
                        this._auditTrail.AddAuditTrail(audit1);

                        //end of Audit section -------------------------------
                        result = _context.SaveChanges() > 0;
                    }
                    if(result == true)

                    trans.Commit();
                }
                
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }


            }
            return result;
        }

        public IEnumerable<OverrideDetailVeiwModel> GetOverrideAwaitingApproval(int staffId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OverrideRequest).ToList();
            var data = (from o in _context.TBL_OVERRIDE_DETAIL
                        join a in _context.TBL_APPROVAL_TRAIL on o.OVERRIDE_DETAILID equals a.TARGETID
                        join c in _context.TBL_CUSTOMER on o.CUSTOMERCODE equals c.CUSTOMERCODE
                        where a.OPERATIONID == (int)OperationsEnum.OverrideRequest 
                            && (a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || a.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                            && ids.Contains((int)a.TOAPPROVALLEVELID)  
                            && a.RESPONSESTAFFID == null
                        select new OverrideDetailVeiwModel
                        {
                            approvedStatusId = o.APPROVALSTATUSID,
                            createdBy = o.CREATEDBY,
                            itemId = o.OVERRIDE_ITEMID,
                            reason = o.REASON,
                            customerbvn = c.CUSTOMERBVN,
                            itemName = o.TBL_OVERRIDE_ITEM.OVERIDE_ITEMNAME,
                            approvalStatus = o.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            customerCode = o.CUSTOMERCODE,
                            sourceReferenceNumber = o.SOURCE_REFERENCE_NUMBER,
                            isUsed = o.ISUSED,
                            customerId = c.CUSTOMERID,
                            operationId = a.OPERATIONID,
                            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MAIDENNAME,
                            dateTimeCreated = o.DATETIMECREATED,
                            overrideDetailId = o.OVERRIDE_DETAILID,
                            staffName = o.TBL_STAFF.LASTNAME + " " + o.TBL_STAFF.FIRSTNAME + " " + o.TBL_STAFF.MIDDLENAME
                        });      

            return data.ToList();
        }
    }
}
