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

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgments()
        {
            return (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Processing
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
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForApproval(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AtcLodgementApproval).ToList();

            return (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ATCLODGMENTID equals atrail.TARGETID
                    where x.DELETED == false && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing 
                     && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                     && atrail.RESPONSESTAFFID == null
                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                     && atrail.OPERATIONID == (int)OperationsEnum.AtcLodgementApproval
                    select new AtcLodgmentViewModel
                    {
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
                        approvalStatusId = x.APPROVALSTATUSID,
                        dateCreated = x.DATETIMECREATED,
                        operationId = atrail.OPERATIONID,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                    }).OrderByDescending(o => o.atcLodgmentId)
             .ToList();
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcReleaseForApproval(int staffId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.AtcReleaseApproval).ToList();

            return (from x in context.TBL_ATC_LODGMENT
                    join r in context.TBL_ATC_RELEASE on x.ATCLODGMENTID equals r.ATCLODGMENTID
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on r.ATCLODGMENTID equals atrail.TARGETID
                    where (x.DELETED == false && atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                     && atrail.RESPONSESTAFFID == null
                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                     && atrail.OPERATIONID == (int)OperationsEnum.AtcReleaseApproval
                    select new AtcLodgmentViewModel
                    {
                        atcReleaseId = r.ATCRELEASEID,
                        atcLodgmentId = x.ATCLODGMENTID,
                        customerId = x.CUSTOMERID,
                        atcTypeId = x.ATCTYPEID,
                        atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
                        description = x.DESCRIPTION,
                        depot = x.DEPOT,
                        unitValue = x.UNITVALUE,
                        unitNumber = x.UNITNUMBER,
                        operationId = atrail.OPERATIONID,
                        unitToRelease = r.UNITTORELEASE,
                        certificateNumber = x.CERTIFICATENUMBER,
                        statusId = x.STATUSID,
                        approvalStatusId = x.APPROVALSTATUSID,
                        dateCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                    }).OrderByDescending(o => o.atcReleaseId)
             .ToList();
        }

        public bool SubmitApproval(AtcReleaseViewModel model)
        {
            bool responce = false;

            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.atcLodgmentId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.AtcReleaseApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {

                        var document = context.TBL_ATC_RELEASE.Where(o => o.ATCLODGMENTID == model.atcLodgmentId && 
                                                                    (o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred))
                                                                    .FirstOrDefault();
                        //var doc = (from ar in context.TBL_ATC_RELEASE
                        //           join al in context.TBL_ATC_LODGMENT on ar.ATCLODGMENTID equals al.ATCLODGMENTID
                        //           where ar.ATCLODGMENTID == model.atcLodgmentId && 
                        //           (ar.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || ar.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                        //           select al).FirstOrDefault();
                        var doc =  context.TBL_ATC_LODGMENT.Where( o => o.ATCLODGMENTID == model.atcLodgmentId).FirstOrDefault();
                        if (document != null && doc != null)
                        {
                            document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
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
                                ATCLODGMENTID = model.atcLodgmentId

                            };

                            context.TBL_ATC_RELEASE_ARCHIVE.Add(entity);
                        }
             
                        

                    }

                    responce = context.SaveChanges() > 0;
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

        public bool SubmitLodgementApproval(AtcLodgmentViewModel model)
        {
            bool responce = false;

                using (var transaction = context.Database.BeginTransaction())
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
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
                                document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                                document.DATETIMEAPPROVED = general.GetApplicationDate();
                            }

                        }

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

        public bool AddAtcRelease(IEnumerable<AtcReleaseViewModel> model)
        {
            foreach(var atc in model)
            {
                //var balance = atc.unitNumber - (atc.unitBalance + atc.unitToRelease);   //don't understand this calculation
                var balance = atc.unitNumber - atc.unitToRelease;   //my balance should be updated by subtracting unitToRelease from unitNumber
                var data = (from ar in context.TBL_ATC_RELEASE
                            join al in context.TBL_ATC_LODGMENT on ar.ATCLODGMENTID equals al.ATCLODGMENTID
                            select ar).ToList();
                var dataDistinct = data.Where(d => d.ATCLODGMENTID == atc.atcLodgmentId).FirstOrDefault();

                if (dataDistinct == null || (dataDistinct != null && (dataDistinct.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || dataDistinct.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved)))
                {
                    var entity = new TBL_ATC_RELEASE
                    {
                        ATCLODGMENTID = atc.atcLodgmentId,
                        UNITBALANCE = balance,
                        UNITNUMBER = atc.unitNumber,
                        UNITTORELEASE = atc.unitToRelease,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                        // COMPANYID = model.companyId,
                        CREATEDBY = atc.createdBy,
                        DATETIMECREATED = general.GetApplicationDate(),
                    };

                    var id = context.TBL_ATC_RELEASE.Add(entity);

                    if (context.SaveChanges() > 0)
                    {
                        workflow.StaffId = atc.createdBy;
                        workflow.CompanyId = atc.companyId;
                        workflow.StatusId = (int)ApprovalStatusEnum.Processing;
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
                        IPADDRESS = atc.userIPAddress,
                        URL = atc.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    });
                    // Audit Section end ------------------------
                }
                else return false;

            }

            return context.SaveChanges() != 0;
        }
        public bool AddAtcLodgment(AtcLodgmentViewModel model)
        {
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
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                BRANCHID = model.branchId,
                CURRENCYID = model.currencyId,
                NUMBEROFBAGS = model.numberOfBags
            };

            var id = context.TBL_ATC_LODGMENT.Add(entity);

            return context.SaveChanges() != 0;
        }

        public bool atclodgmentApproval(AtcLodgmentViewModel model)
        {
            var data = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == model.atcLodgmentId && o.OPERATIONID == (int)OperationsEnum.AtcLodgementApproval)
                                                    .FirstOrDefault();


            var entity = context.TBL_ATC_LODGMENT.Where(o => o.ATCLODGMENTID == model.atcLodgmentId)
                        .FirstOrDefault();

            if (data != null) return false;

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.Comment = "Request for ATC Lodgement approval";
            workflow.OperationId = (int)OperationsEnum.AtcLodgementApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.TargetId = model.atcLodgmentId;
            workflow.LogActivity();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Atc Lodgment '{model.description}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------

            if(context.SaveChanges() != 0)
            {
                entity.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            }

            return context.SaveChanges() != 0;
        }

        public bool UpdateAtcLodgment(AtcLodgmentViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_ATC_LODGMENT.Find(id);
            entity.CUSTOMERID = model.customerId;
            entity.ATCTYPEID = model.atcTypeId;
            entity.DESCRIPTION = model.description;
            entity.DEPOT = model.depot;
            entity.UNITVALUE = model.unitValue;
            entity.UNITNUMBER = model.unitNumber;
            //entity.CERTIFICATENUMBER = model.certificateNumber;
            entity.STATUSID = model.statusId;
            entity.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
            entity.BRANCHID = model.branchId;
            entity.CURRENCYID = model.currencyId;
            entity.NUMBEROFBAGS = model.numberOfBags;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            //// Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.AtcLodgmentUpdated,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Atc Lodgment '{entity.DESCRIPTION}' was updated by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.ATCLODGMENTID
            //});
            //// Audit Section end ------------------------

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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ATCLODGMENTID
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ATCLODGMENTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<AtcLodgmentViewModel> GetAtcLodgmentForRelease()
        {
            //return (from x in context.TBL_ATC_LODGMENT
            //        join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
            //        where x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.UNITNUMBER > 0
            //        select new AtcLodgmentViewModel
            //        {
            //            atcLodgmentId = x.ATCLODGMENTID,
            //            customerId = x.CUSTOMERID,
            //            atcTypeId = x.ATCTYPEID,
            //            description = x.DESCRIPTION,
            //            depot = x.DEPOT,
            //            unitValue = x.UNITVALUE,
            //            unitNumber = x.UNITNUMBER,
            //            numberOfBags = x.NUMBEROFBAGS,
            //            certificateNumber = x.CERTIFICATENUMBER,
            //            atcType = context.TBL_ATC_TYPE.Where(o => o.ATCTYPEID == x.ATCTYPEID).Select(o => o.ACTTYPENAME).FirstOrDefault(),
            //            statusId = x.STATUSID,
            //            approvalStatusId = x.APPROVALSTATUSID,
            //            dateCreated = x.DATETIMECREATED,
            //            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
            //            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
            //            customerCode = c.CUSTOMERCODE,
            //            branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),

            //        }).OrderByDescending(o=>o.atcLodgmentId)
            // .ToList();
            return (from x in context.TBL_ATC_LODGMENT
                    join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.UNITNUMBER > 0
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
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == x.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault()

                    }).OrderByDescending(o => o.atcLodgmentId)
             .ToList();
        }

        public IEnumerable<AtcReleaseViewModel>GetAtcLodgmentForReleaseList()
        {
            return (from ar in context.TBL_ATC_RELEASE
                    join al in context.TBL_ATC_LODGMENT on ar.ATCLODGMENTID equals al.ATCLODGMENTID
                    join c in context.TBL_CUSTOMER on al.CUSTOMERID equals c.CUSTOMERID
                    where ar.DELETED ==
                           false && (ar.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                    select new AtcReleaseViewModel
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
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = entity.ATCTYPEID
                });
          
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }
    }
}

// kernel.Bind<IAtcLodgmentRepository>().To<AtcLodgmentRepository>();
// AtcLodgmentAdded = ???, AtcLodgmentUpdated = ???, AtcLodgmentDeleted = ???,
