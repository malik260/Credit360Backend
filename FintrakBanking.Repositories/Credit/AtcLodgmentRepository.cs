using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.credit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.credit
{
    public class AtcLodgmentRepository : IAtcLodgmentRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public AtcLodgmentRepository(
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

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgments(int staffId)
        {

            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.AtcLodgementApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var data = (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && (x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                    select new AtcLodgmentViewModel
                    {
                        atcLodgmentId = x.ATCLODGMENTID,
                        customerId = x.CUSTOMERID,
                        atcTypeId = x.ATCTYPEID,
                        description = x.DESCRIPTION,
                        depot = x.DEPOT,
                        unitValue = x.UNITVALUE,
                        unitNumber = x.UNITNUMBER,
                        numberOfBags = x.NUMBEROFBAGS,
                        atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                        certificateNumber = x.CERTIFICATENUMBER,
                        statusId = x.STATUSID,
                        approvalStatusId = x.APPROVALSTATUSID,
                        dateCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        branchId = x.BRANCHID,
                        currencyId = x.CURRENCYID,
                        currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                       
                    }).OrderByDescending(o=>o.atcLodgmentId)
             .ToList();

            var data2 = (from x in context.TBL_ATC_LODGMENT
                         join trail in context.TBL_APPROVAL_TRAIL on x.ATCLODGMENTID equals trail.TARGETID
                         join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                         where  trail.OPERATIONID == (short)OperationsEnum.AtcLodgementApproval
                             && x.DELETED == false
                             && trail.TARGETID == x.ATCLODGMENTID
                         orderby trail.APPROVALTRAILID descending
                         select new AtcLodgmentViewModel
                        {
                            loopedStaffId = trail.LOOPEDSTAFFID,
                            atcLodgmentId = x.ATCLODGMENTID,
                            customerId = x.CUSTOMERID,
                            atcTypeId = x.ATCTYPEID,
                            description = x.DESCRIPTION,
                            depot = x.DEPOT,
                            unitValue = x.UNITVALUE,
                            unitNumber = x.UNITNUMBER,
                            numberOfBags = x.NUMBEROFBAGS,
                            atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                            certificateNumber = x.CERTIFICATENUMBER,
                            statusId = x.STATUSID,
                            approvalStatusId = trail.APPROVALSTATUSID,
                            dateCreated = x.DATETIMECREATED,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == trail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                            customerCode = c.CUSTOMERCODE,
                            branchId = x.BRANCHID,
                            currencyId = x.CURRENCYID,
                            currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                            branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),

                        }).ToList().GroupBy(x => x.atcLodgmentId).Select(x => x.FirstOrDefault()).Where((trail => (trail.approvalStatusId == (short)ApprovalStatusEnum.Referred
                                    && trail.loopedStaffId == initiator) || trail.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)).ToList();



            //var dataGroup = data2.GroupBy(x => x.atcLodgmentId).Select(x => x.FirstOrDefault()).ToList();

            data = data.Union(data2).ToList();

            return data;
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForApproval(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AtcLodgementApproval).ToList();
            var staffs = general.GetStaffRlieved(staffId);

            return (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ATCLODGMENTID equals atrail.TARGETID
                    where x.DELETED == false && (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing 
                     || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                     && atrail.RESPONSESTAFFID == null
                     && (ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null)
                     && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                     && atrail.OPERATIONID == (int)OperationsEnum.AtcLodgementApproval
                    select new AtcLodgmentViewModel
                    {
                        numberOfBags = x.NUMBEROFBAGS,
                        atcLodgmentId = x.ATCLODGMENTID,
                        customerId = x.CUSTOMERID,
                        atcTypeId = x.ATCTYPEID,
                        atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                        description = x.DESCRIPTION,
                        depot = x.DEPOT,
                        unitValue = x.UNITVALUE,
                        unitNumber = x.UNITNUMBER,
                        certificateNumber = x.CERTIFICATENUMBER,
                        statusId = x.STATUSID,
                        approvalStatusId = atrail.APPROVALSTATUSID,
                        dateCreated = x.DATETIMECREATED,
                        operationId = atrail.OPERATIONID,
                        currencyId = x.CURRENCYID,
                        currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                    }).OrderByDescending(o => o.atcLodgmentId)
             .ToList();
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentsByCustomerId(int customerId)
        {
            var result = (from atc in context.TBL_ATC_LODGMENT
                          join c in context.TBL_CUSTOMER on atc.CUSTOMERID equals c.CUSTOMERID
                          where atc.CUSTOMERID == customerId
                            && atc.DELETED == false
                            && atc.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                          orderby atc.ATCLODGMENTID descending
                          select new AtcLodgmentViewModel
                          {
                              atcLodgmentId = atc.ATCLODGMENTID,
                              customerId = atc.CUSTOMERID,
                              atcTypeId = atc.ATCTYPEID,
                              description = atc.DESCRIPTION,
                              depot = atc.DEPOT,
                              unitValue = atc.UNITVALUE,
                              unitNumber = atc.UNITNUMBER,
                              numberOfBags = atc.NUMBEROFBAGS,
                              atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == atc.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                              certificateNumber = atc.CERTIFICATENUMBER,
                              statusId = atc.STATUSID,
                              approvalStatusId = atc.APPROVALSTATUSID,
                              dateCreated = atc.DATETIMECREATED,
                              approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atc.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                              customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                              customerCode = c.CUSTOMERCODE,
                              branchId = atc.BRANCHID,
                              currencyId = atc.CURRENCYID,
                              currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == atc.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                              branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == atc.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                          }).ToList();

            return result;
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcReleaseForApproval(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AtcReleaseApproval).ToList();
            var staffs = general.GetStaffRlieved(staffId);

            return (from atrail in context.TBL_APPROVAL_TRAIL
                    join r in context.TBL_ATC_RELEASE on atrail.TARGETID equals r.ATCLODGMENTID
                    join x in context.TBL_ATC_LODGMENT on r.ATCLODGMENTID equals x.ATCLODGMENTID
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    where r.DELETED == false 
                     && (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                     && r.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                     && atrail.RESPONSESTAFFID == null
                     && ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null
                     && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                     && atrail.OPERATIONID == (int)OperationsEnum.AtcReleaseApproval
                    select new AtcLodgmentViewModel
                    {
                        numberOfBags = x.NUMBEROFBAGS,
                        atcReleaseId = r.ATCRELEASEID,
                        atcLodgmentId = r.ATCLODGMENTID,
                        customerId = x.CUSTOMERID,
                        atcTypeId = x.ATCTYPEID,
                        atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                        description = x.DESCRIPTION,
                        depot = x.DEPOT,
                        unitValue = x.UNITVALUE,
                        unitNumber = x.UNITNUMBER,
                        operationId = (int)OperationsEnum.AtcReleaseApproval,
                        unitToRelease = r.UNITTORELEASE,
                        certificateNumber = x.CERTIFICATENUMBER,
                        statusId = x.STATUSID,
                        approvalStatusId = atrail.APPROVALSTATUSID,
                        dateCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        dateReleased = x.DATETIMERELEASED
                    }).OrderByDescending(o => o.atcReleaseId)
             .ToList();
        }
        

        public WorkflowResponse SubmitReferredAtcBackIntoWorkflow(AtcReleaseViewModel model)
        {
            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (short)ApprovalStatusEnum.Processing;
            workflow.TargetId = model.atcLodgmentId;
            workflow.Comment = "Update has been Applied, Request for ATC approval";
            workflow.OperationId = (int)OperationsEnum.AtcReleaseApproval;
            workflow.DeferredExecution = true;
            workflow.LogActivity();
            var saved = context.SaveChanges() > 0;
            return workflow.Response;
        }

        public Tuple<WorkflowResponse, int> SubmitApproval(IEnumerable<AtcReleaseViewModel> model)
        {
            bool responce = false;
            
            using (var transaction = context.Database.BeginTransaction())
            {
                var ctr = 0;
                foreach (var mod in model)
                {
                    workflow.StaffId = mod.createdBy;
                    workflow.CompanyId = mod.companyId;
                    workflow.StatusId = mod.approvalStatusId == 2 ? (short)ApprovalStatusEnum.Processing : mod.approvalStatusId;
                    workflow.TargetId = mod.atcLodgmentId;
                    workflow.Comment = mod.comment;
                    workflow.NextLevelId = null;
                    workflow.OperationId = (int)OperationsEnum.AtcReleaseApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();

                    try
                    {
                        
                        if (workflow.NewState == (int)ApprovalState.Ended)
                        {
                            ctr = ctr + 1;
                            var document = context.TBL_ATC_RELEASE.Where(o => o.ATCLODGMENTID == mod.atcLodgmentId &&
                                                                    (o.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || o.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred))
                                                                    .FirstOrDefault();
                              
                            var doc = context.TBL_ATC_LODGMENT.Where(o => o.ATCLODGMENTID == mod.atcLodgmentId).FirstOrDefault();
                            if (document != null && doc != null)
                            {
                                // Update the date released the Lodgement
                                doc.DATETIMERELEASED = DateTime.Now;

                                document.APPROVALSTATUSID = mod.approvalStatusId;
                                if(mod.approvalStatusId == (short)ApprovalStatusEnum.Approved)
                                {
                                    document.UNITNUMBER = document.UNITBALANCE;
                                    doc.UNITNUMBER = document.UNITBALANCE;
                                    document.DATETIMEAPPROVED = general.GetApplicationDate();

                                    //Adding Data to TBL_RELEASE_ARCHIVE
                                    var entity = new TBL_ATC_RELEASE_ARCHIVE
                                    {
                                        UNITNUMBER = document.UNITBALANCE,
                                        ATCRELEASEID = document.ATCRELEASEID,
                                        UNITTORELEASE = document.UNITTORELEASE,
                                        UNITBALANCE = document.UNITBALANCE,
                                        DATETIMECREATED = document.DATETIMECREATED,
                                        APPROVALSTATUSID = document.APPROVALSTATUSID,
                                        CREATEDBY = document.CREATEDBY,
                                        ATCLODGMENTID = mod.atcLodgmentId

                                    };

                                    context.TBL_ATC_RELEASE_ARCHIVE.Add(entity);
                                }
                            }

                            
                        }
                    }

                    catch (Exception ex)
                    {

                        transaction.Rollback();
                        throw ex;
                    }
                    //return false;
                }

                responce = context.SaveChanges() > 0;
                transaction.Commit();

                return new Tuple<WorkflowResponse, int>  (workflow.Response,ctr);
            }
        }

        public WorkflowResponse SubmitLodgementApproval(AtcLodgmentViewModel model)
        {
            bool responce = false;

                using (var transaction = context.Database.BeginTransaction())
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = model.approvalStatusId == (short)ApprovalStatusEnum.Approved ? (short)ApprovalStatusEnum.Processing : model.approvalStatusId;
                    workflow.TargetId = model.atcLodgmentId;
                    workflow.Comment = model.comment;
                    workflow.OperationId = (int)OperationsEnum.AtcLodgementApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();
                    try
                    {
                        if (workflow.NewState == (int)ApprovalState.Ended)
                        {
                            var document = context.TBL_ATC_LODGMENT.Where(o => o.ATCLODGMENTID == model.atcLodgmentId).FirstOrDefault();
                        
                        if (document != null)
                            {
                                document.APPROVALSTATUSID = model.approvalStatusId;
                                document.DATETIMEAPPROVED = general.GetApplicationDate();
                            }

                        }

                        responce = context.SaveChanges() > 0;
                        transaction.Commit();

                        return workflow.Response;
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();


                        throw new SecureException(ex.Message);
                    }
                    //return false;
                }
            
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcType()
        {
            return (from x in context.TBL_ATC_TYPE
                    select new AtcLodgmentViewModel
                    {
                        atcTypeId = x.ATCTYPEID,
                        atcTypeName = x.ACTTYPENAME
                    })
             .ToList();

        }

        public AtcLodgmentViewModel GetAtcLodgment(int id)
        {
            var entity = context.TBL_ATC_LODGMENT.FirstOrDefault(x => x.ATCLODGMENTID == id && x.DELETED == false);

            return new AtcLodgmentViewModel
            {
                atcLodgmentId = entity.ATCLODGMENTID,
                customerId = entity.CUSTOMERID,
                atcTypeId = entity.ATCTYPEID,
                description = entity.DESCRIPTION,
                depot = entity.DEPOT,
                unitValue = entity.UNITVALUE,
                atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == entity.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                unitNumber = entity.UNITNUMBER,
                //   certificateNumber = entity.CERTIFICATENUMBER,
                dateCreated = entity.DATETIMECREATED,
                statusId = entity.STATUSID,
                approvalStatusId = entity.APPROVALSTATUSID,
                approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == entity.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),

            };
        }

        public AtcReleaseViewModel GetAtcRelease(int id)
        {
            //var entity = (from x in context.TBL_ATC_RELEASE
            //              join r in context.TBL_ATC_LODGMENT on x.ATCLODGMENTID equals r.ATCLODGMENTID
            //              where x.ATCLODGMENTID == id && x.DELETED == false
            //              select new AtcReleaseViewModel
            //              {
            //                  unitNumber = r.UNITNUMBER,
            //                  unitToRelease = x.UNITTORELEASE,
            //                  dateCreated = x.DATETIMECREATED,
            //                  releaseBalance = x.UNITBALANCE,
            //                  createdBy = x.CREATEDBY,
            //                  atcLodgmentId = x.ATCLODGMENTID,
            //                  atcReleaseId = x.ATCRELEASEID,
            //                  approvalStatusId = x.APPROVALSTATUSID,
            //                  approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
            //                  // unitBalance = x.UNITBALANCE
            //              });

            //foreach (var item in entity)
            //{
            //    item.unitRelease = item.unitRelease + item.unitToRelease;
            //    item.unitBalance = item.unitNumber - item.unitRelease;
            //}
            //return entity.ToList();
            var entity = (from x in context.TBL_ATC_RELEASE
                          join r in context.TBL_ATC_LODGMENT on x.ATCLODGMENTID equals r.ATCLODGMENTID
                          where x.ATCLODGMENTID == id && x.DELETED == false
                          select new AtcReleaseViewModel
                          {
                              unitNumber = r.UNITNUMBER,
                              unitToRelease = x.UNITTORELEASE,
                              dateCreated = x.DATETIMECREATED,
                              releaseBalance = x.UNITBALANCE,
                              createdBy = x.CREATEDBY,
                              atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == r.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                              atcLodgmentId = x.ATCLODGMENTID,
                              atcReleaseId = x.ATCRELEASEID,
                              approvalStatusId = x.APPROVALSTATUSID,
                              numberOfBags = r.NUMBEROFBAGS,
                              approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                              // unitBalance = x.UNITBALANCE
                          }).FirstOrDefault();

           
                entity.unitRelease = entity.unitRelease + entity.unitToRelease;
                entity.unitBalance = entity.unitNumber - entity.unitRelease;

            return entity;
        }

        public WorkflowResponse AddAtcRelease(IEnumerable<AtcReleaseViewModel> model)
        {
            //Doing a Check to confirm if none of the ATC in the incoming list is already being Processed 
            foreach(var atc in model)
            {

                if (atc.unitToRelease <= 0 || atc.unitToRelease > atc.unitNumber) throw new SecureException("Invalid Input For Unit to Release");

                var record = (from ar in context.TBL_ATC_RELEASE
                               where atc.atcLodgmentId == ar.ATCLODGMENTID
                               orderby ar.ATCLODGMENTID descending
                               select ar).FirstOrDefault();
                if (record == null || (record != null && (record.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved || record.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved)))
                {
                    continue;
                }
                else throw new SecureException("One or More records may currently be Undergoing Approval");
            }

            foreach(var atc in model)
            {
                //var balance = atc.unitNumber - (atc.unitBalance + atc.unitToRelease);   //don't understand this calculation
                var balance = atc.unitNumber - atc.unitToRelease;   //my balance should be updated by subtracting unitToRelease from unitNumber
                
                    var entity = new TBL_ATC_RELEASE
                    {
                        ATCLODGMENTID = atc.atcLodgmentId,
                        UNITBALANCE = balance,
                        UNITNUMBER = atc.unitNumber,
                        UNITTORELEASE = atc.unitToRelease,
                        APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                        // COMPANYID = model.companyId,
                        CREATEDBY = atc.createdBy,
                        DATETIMECREATED = general.GetApplicationDate(),
                    };

                    var id = context.TBL_ATC_RELEASE.Add(entity);

                    if (context.SaveChanges() > 0)
                    {
                        workflow.StaffId = atc.createdBy;
                        workflow.CompanyId = atc.companyId;
                        workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                        workflow.TargetId = id.ATCLODGMENTID;
                        workflow.Comment = "Request for ATC Release approval";
                        workflow.OperationId = (int)OperationsEnum.AtcReleaseApproval;
                        workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                        workflow.ExternalInitialization = true;
                        workflow.LogActivity();
                    }


                    var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == atc.createdBy).Select(x => x.STAFFCODE));
                    //// Audit Section ---------------------------
                    this.audit.AddAuditTrail(new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.AtcReleaseAdded,
                        STAFFID = atc.createdBy,
                        BRANCHID = (short)atc.userBranchId,
                        DETAIL = $"TBL_Atc Release of '{entity.UNITTORELEASE}' units created by {auditStaff}",
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = atc.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,

                       
                    });
                    // Audit Section end ------------------------

            }
            var saved = context.SaveChanges() > 0;
            return workflow.Response;
        }
        public bool AddAtcLodgment(AtcLodgmentViewModel model)
        {

            bool response = false;

            var entity = new TBL_ATC_LODGMENT
            {
                CUSTOMERID = model.customerId,
                ATCTYPEID = model.atcTypeId,
                DESCRIPTION = model.description,
                DEPOT = model.depot,
                UNITVALUE = model.unitValue,
                UNITNUMBER = model.unitNumber,
                //  CERTIFICATENUMBER = model.certificateNumber,
                STATUSID = model.statusId,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                BRANCHID = model.branchId,
                CURRENCYID = model.currencyId,
                NUMBEROFBAGS = model.numberOfBags
            };

            var id = context.TBL_ATC_LODGMENT.Add(entity);

            response = context.SaveChanges() > 0; 

            if (response)
            {
                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                //// Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_Atc Lodgment '{model.description}' created by {auditStaff}",               
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),      
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                });
                // Audit Section end ------------------------
            }



            return context.SaveChanges() != 0;
        }

        public bool atclodgeApproval(AtcLodgmentViewModel model)
        {

            if (model.approvalStatusId == (short)ApprovalStatusEnum.Referred)
            {
                bool responce = false;

                using (var transaction = context.Database.BeginTransaction())
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                    workflow.TargetId = model.atcLodgmentId;
                    workflow.Comment = "Update has been applied, Request for ATC lodgment Approval";
                    workflow.OperationId = (int)OperationsEnum.AtcLodgementApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();
                    try
                    {

                        responce = context.SaveChanges() > 0;
                        transaction.Commit();

                        return responce;
                    }
                    catch (Exception ex)
                    {

                        transaction.Rollback();


                        throw new SecureException(ex.Message);
                    }
                    //return false;
                }
            }

            else
            {
                var data = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == model.atcLodgmentId && o.OPERATIONID == (int)OperationsEnum.AtcLodgementApproval)
                                                   .FirstOrDefault();


                var entity = context.TBL_ATC_LODGMENT.Where(o => o.ATCLODGMENTID == model.atcLodgmentId)
                            .FirstOrDefault();

                if (data != null) return false;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                workflow.Comment = "Request for ATC Lodgement approval";
                workflow.OperationId = (int)OperationsEnum.AtcLodgementApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.TargetId = model.atcLodgmentId;
                workflow.LogActivity();

                if (context.SaveChanges() != 0)
                {
                    entity.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
                }

                return context.SaveChanges() != 0;
            }

        }

        public WorkflowResponse AtclodgmentApproval(IEnumerable<AtcLodgmentViewModel> model)
        {
           

            foreach (var atc in model)
            {
                atclodgeApproval(atc);
            }
            var saved = context.SaveChanges() > 0;
            return workflow.Response;
        }

        public bool SaveEditedATCRelease(AtcReleaseViewModel model, int id)
        {
            var unitBalance = model.unitNumber - model.unitToRelease;

            if(model.unitToRelease <= 0 || model.unitToRelease > model.unitNumber) throw new SecureException("Invalid Input For Unit to Release");

            var entity = this.context.TBL_ATC_RELEASE.Find(id);
            if(entity != null)
            {
                entity.UNITTORELEASE = model.unitToRelease;
                entity.UNITBALANCE = unitBalance;
                entity.LASTUPDATEDBY = model.createdBy;
                entity.DATETIMEUPDATED = general.GetApplicationDate();
            }
            try
            {
                return context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateAtcLodgment(AtcLodgmentViewModel model, int id, UserInfo user)
        {

            bool response = false; 

            var entity = this.context.TBL_ATC_LODGMENT.Find(id);
            entity.CUSTOMERID = model.customerId;
            entity.ATCTYPEID = model.atcTypeId;
            entity.DESCRIPTION = model.description;
            entity.DEPOT = model.depot;
            entity.UNITVALUE = model.unitValue;
            entity.UNITNUMBER = model.unitNumber;
            //entity.CERTIFICATENUMBER = model.certificateNumber;
            entity.STATUSID = model.statusId;
            entity.APPROVALSTATUSID = model.approvalStatusId;//(int)ApprovalStatusEnum.Pending;
            entity.BRANCHID = model.branchId;
            entity.CURRENCYID = model.currencyId;
            entity.NUMBEROFBAGS = model.numberOfBags;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            response = context.SaveChanges() > 0;

            if(response)
            {
                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentUpdated,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"TBL_Atc Lodgment '{entity.DESCRIPTION}' was updated by {auditStaff}",                 
                    URL = user.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                });
                // Audit Section end ------------------------
            }


            return context.SaveChanges() != 0;
        }

        public bool DeleteAtcLodgment(int id, UserInfo user)
        {
            var entity = this.context.TBL_ATC_LODGMENT.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Atc Lodgment '{entity.DESCRIPTION}' was deleted by {auditStaff}",
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),           
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteAtcRelease(int id, UserInfo user)
        {
            var entity = this.context.TBL_ATC_RELEASE.Find(id);
            entity.DELETED = true;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AtcReleaseDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Atc release '{entity.UNITTORELEASE}' was deleted by {auditStaff}",        
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),            
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForRelease()
        {
                    return (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && x.UNITNUMBER > 0
                    select new AtcLodgmentViewModel
                    
                    {
                        atcLodgmentId = x.ATCLODGMENTID,
                        customerId = x.CUSTOMERID,
                        atcTypeId = x.ATCTYPEID,
                        description = x.DESCRIPTION,
                        depot = x.DEPOT,
                        unitValue = x.UNITVALUE,
                        unitNumber = x.UNITNUMBER,
                        numberOfBags = x.NUMBEROFBAGS,
                        certificateNumber = x.CERTIFICATENUMBER,
                        atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                        statusId = x.STATUSID,
                        approvalStatusId = x.APPROVALSTATUSID,
                        dateCreated = x.DATETIMEAPPROVED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        currencyId = x.CURRENCYID,
                        currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        exchangeRate = context.TBL_CURRENCY_EXCHANGERATE.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.EXCHANGERATE).FirstOrDefault(),
                        dateReleased = x.DATETIMEUPDATED,
                    }).OrderByDescending(o => o.atcLodgmentId)
             .ToList();
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForReleaseList(int staffId)
        {

            //var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AtcReleaseApproval).ToList();
            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.AtcReleaseApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var model1 = (from ar in context.TBL_ATC_RELEASE
                    join al in context.TBL_ATC_LODGMENT on ar.ATCLODGMENTID equals al.ATCLODGMENTID
                    join c in context.TBL_CUSTOMER on al.CUSTOMERID equals c.CUSTOMERID
                    where ar.DELETED ==
                           false && (ar.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                    select new AtcLodgmentViewModel
                    {
                        atcReleaseId = ar.ATCRELEASEID,
                        dateCreated = ar.DATETIMECREATED,
                        unitToRelease = ar.UNITTORELEASE,
                        unitBalance = ar.UNITBALANCE,
                        unitNumber = ar.UNITNUMBER,
                        depot = al.DEPOT,
                        description = al.DESCRIPTION,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == al.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == ar.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault()

                    }).OrderByDescending(o => o.atcReleaseId).ToList();

            var model2 = (from ar in context.TBL_ATC_RELEASE
                         join al in context.TBL_ATC_LODGMENT on ar.ATCLODGMENTID equals al.ATCLODGMENTID
                         join c in context.TBL_CUSTOMER on al.CUSTOMERID equals c.CUSTOMERID
                         join atrail in context.TBL_APPROVAL_TRAIL on ar.ATCLODGMENTID equals atrail.TARGETID
                         where ar.DELETED == false && atrail.OPERATIONID == (int)OperationsEnum.AtcReleaseApproval
                            && atrail.TARGETID == ar.ATCLODGMENTID
                            orderby atrail.APPROVALTRAILID descending
                         select new AtcLodgmentViewModel
                         {
                             loopedStaffId = atrail.LOOPEDSTAFFID,
                             atcLodgmentId = ar.ATCLODGMENTID,
                             atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == al.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                             unitValue = al.UNITVALUE,
                             numberOfBags = al.NUMBEROFBAGS,
                             approvalStatusId = atrail.APPROVALSTATUSID,
                             atcReleaseId = ar.ATCRELEASEID,
                             dateCreated = ar.DATETIMECREATED,
                             unitToRelease = ar.UNITTORELEASE,
                             unitBalance = ar.UNITBALANCE,
                             unitNumber = ar.UNITNUMBER,
                             depot = al.DEPOT,
                             description = al.DESCRIPTION,
                             branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == al.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                             customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                             approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),

                         }).ToList().GroupBy(x => x.atcLodgmentId).Select(x => x.FirstOrDefault()).Where(trail => (trail.approvalStatusId == (short)ApprovalStatusEnum.Referred
                                    && trail.loopedStaffId == initiator) || trail.approvalStatusId == (short)ApprovalStatusEnum.Disapproved).ToList();
            

            var model = model1.Union(model2).ToList();

            return model;
        }

        public bool AddAtcType(AtcTypeViewModel model)
        {
            var entity = new TBL_ATC_TYPE
            {
                ACTTYPENAME = model.actTypeName,
            };

            context.TBL_ATC_TYPE.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.AtcTypeAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"ATC Type '{entity.ACTTYPENAME}' created by {auditStaff}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //});
            // Audit Section end ------------------------
            return context.SaveChanges() != 0;
        }


        public bool DeleteAtcType(int id, UserInfo user)
        {
           
                var entity = this.context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == id).Select(o => o).FirstOrDefault();
                context.TBL_ATC_TYPE.Remove(entity);


                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.AtcTypeDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"ATC Type '{entity.ACTTYPENAME}' was deleted by {auditStaff}",            
                    URL = user.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),              
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),


                });
          
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
    }
}

// kernel.Bind<IAtcLodgmentRepository>().To<AtcLodgmentRepository>();
// AtcLodgmentAdded = ???, AtcLodgmentUpdated = ???, AtcLodgmentDeleted = ???,
