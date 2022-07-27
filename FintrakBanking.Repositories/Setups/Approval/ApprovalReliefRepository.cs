using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;

using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.Approval
{
    public class ApprovalReliefRepository : IApprovalReliefRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository auditTrail;
        private IWorkflow workFlow;

        public ApprovalReliefRepository(
            FinTrakBankingContext context,
            IGeneralSetupRepository general,
            IAuditTrailRepository audit,
            IWorkflow _workflow
            )
        {
            this.context = context;
            this.general = general;
            this.auditTrail = audit;
            workFlow = _workflow;
        }
        public async Task<ApprovalReliefViewModel> AddApprovalRelief(ApprovalReliefViewModel model)
        {
            //var data = new TBL_STAFF_RELIEF
            //{
            //    STAFFID = model.relievedStaffId,
            //    RELIEFSTAFFID = model.reliefStaffId,
            //    RELIEFREASON = model.reliefReason,
            //    STARTDATE = model.startDate,
            //    ENDDATE = model.endDate,
            //    ISACTIVE = model.isActive,
            //    DATETIMECREATED = general.GetApplicationDate(),
            //    CREATEDBY = (int)model.createdBy
            //};

                    bool output = false;
                    TBL_TEMP_STAFF_RELIEF tempStaffRelief;

            if (model.relievedStaffId <= 0)
            {
                throw new ConditionNotMetException("Please select a staff to relieve");
            }

            if(model.reliefStaffId <= 0)
            {
                throw new ConditionNotMetException("Please select a relieving staff");
            }


            tempStaffRelief = new TBL_TEMP_STAFF_RELIEF()
                        {
                            STAFFID = model.relievedStaffId,
                            RELIEFSTAFFID = model.reliefStaffId,
                            RELIEFREASON = model.reliefReason,
                            STARTDATE = model.startDate,
                            ENDDATE = model.endDate,
                           // ISACTIVE = model.isActive,
                            ISACTIVE = DateTime.Now.CompareTo(model.endDate) < 0,
                            CREATEDBY = (int)model.createdBy,
                            OPERATION = "insert",
                            DATETIMECREATED = DateTime.Now,
                            APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                            ISCURRENT = true,
                            DELETED = false,
                        };

                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.StaffReliefAdded,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)model.userBranchId,
                        DETAIL = $"Added '{model.reliefStaffName}' as Staff Relief for: '{model.staffName}'",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        TARGETID = model.reliefId,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };
                    using (var trans = context.Database.BeginTransaction())
                    {
                        try
                        {
                            context.TBL_TEMP_STAFF_RELIEF.Add(tempStaffRelief);
                            this.auditTrail.AddAuditTrail(audit);
                            output = context.SaveChanges() > 0;

                            var entity = new ApprovalViewModel
                            {
                                staffId = model.createdBy,
                                companyId = model.companyId,
                                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                                comment = "Please approve this Approval Relief",
                                targetId = tempStaffRelief.TEMPRELIEFID,
                                operationId = (int)OperationsEnum.StaffReliefCreation,
                                BranchId = model.userBranchId,
                                externalInitialization = true
                            };

                            var response = workFlow.LogForApproval(entity);

                            if (response)
                            {
                                trans.Commit();

                                if (output)
                                {
                                    return new ApprovalReliefViewModel { reliefId = tempStaffRelief.TEMPRELIEFID};
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new SecureException(ex.Message);
                        }
                    }
            return new ApprovalReliefViewModel();

        }

        public void UpdateAllApprovalRelief(int companyId)
        {
            try
            {
                var relisfData = context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false)
                .OrderByDescending(x => x.RELIEFID).ToList();
                foreach (var r in relisfData)
                {
                    r.ISACTIVE = DateTime.Now.CompareTo(r.ENDDATE) < 0;
                }
                context.SaveChanges();
                
            }
            catch (Exception e)
            {
                throw e;
            }


        }


        public IEnumerable<ApprovalReliefViewModel> GetAllApprovalRelief(int companyId)
        {
            try
            {
                var relisfData = context.TBL_STAFF_RELIEF
                .Where(x => x.DELETED == false)
                .OrderByDescending(x => x.RELIEFID).ToList();
                foreach (var r in relisfData)
                {
                    r.ISACTIVE = DateTime.Now.CompareTo(r.ENDDATE) < 0;
                }
                context.SaveChanges();
                var reliefs = (from x in context.TBL_STAFF_RELIEF where x.DELETED == false

                       select new ApprovalReliefViewModel
                       {
                           reliefId = x.RELIEFID,
                           relievedStaffId = x.STAFFID,
                           reliefStaffId = x.RELIEFSTAFFID,
                           staffName = context.TBL_STAFF.Where(s => s.STAFFID == x.STAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                           reliefStaffName = context.TBL_STAFF.Where(s => s.STAFFID == x.RELIEFSTAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                           reliefReason = x.RELIEFREASON,
                           startDate = x.STARTDATE,
                           endDate = x.ENDDATE,
                           isActive = x.ISACTIVE,
                           //isActive = DateTime.Now.CompareTo(x.ENDDATE) < 0,

                       }).ToList();
                return reliefs;
           }
            catch(Exception e)
            {
                throw e;
            }

            
        }
        public async Task<bool> UpdateApprovalRelief(int reliefId, ApprovalReliefViewModel model)
        {
            bool output = false;
            var targetReliefId = 0;

            var existingTempApprovalRelief = context.TBL_TEMP_STAFF_RELIEF
                   .FirstOrDefault(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.RELIEFID == model.reliefId);
            var unApprovedApprovalRelief = context.TBL_TEMP_STAFF_RELIEF
                    .Where(x => x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                    && x.RELIEFID == model.reliefId && x.ISCURRENT == true);
            TBL_TEMP_STAFF_RELIEF tempApprovalRelief = new TBL_TEMP_STAFF_RELIEF();

            if (unApprovedApprovalRelief.Any())
            {
                throw new SecureException("Approval Relief is already undergoing approval");
            }
            if (existingTempApprovalRelief != null)
            {
                var tempApprovalReliefToUpdate = existingTempApprovalRelief;
                tempApprovalReliefToUpdate.RELIEFSTAFFID = model.reliefStaffId;
                tempApprovalReliefToUpdate.RELIEFREASON = model.reliefReason;
                tempApprovalReliefToUpdate.STARTDATE = model.startDate;
                tempApprovalReliefToUpdate.ENDDATE = model.endDate;
                tempApprovalReliefToUpdate.ISACTIVE = DateTime.Now.CompareTo(model.endDate) < 0;
                //tempApprovalReliefToUpdate.ISACTIVE = model.isActive;
                tempApprovalReliefToUpdate.LASTUPDATEDBY = (int)model.createdBy;
                tempApprovalReliefToUpdate.ISCURRENT = true;
                tempApprovalReliefToUpdate.DATETIMEUPDATED = DateTime.Now;
                tempApprovalReliefToUpdate.DELETED = false;
                tempApprovalReliefToUpdate.OPERATION = "update";
                tempApprovalReliefToUpdate.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
                tempApprovalReliefToUpdate.RELIEFID = (int)model.reliefId;
                context.TBL_TEMP_STAFF_RELIEF.Add(tempApprovalReliefToUpdate);
                context.SaveChanges();
            }
            else
            {
                tempApprovalRelief = new TBL_TEMP_STAFF_RELIEF()
                {
                    STAFFID = model.relievedStaffId,
                    RELIEFSTAFFID = model.reliefStaffId,
                    RELIEFREASON = model.reliefReason,
                    STARTDATE = model.startDate,
                    ENDDATE = model.endDate,
                    //ISACTIVE = model.isActive,
                    ISACTIVE = DateTime.Now.CompareTo(model.endDate) < 0,
                    //LASTUPDATEDBY = model.lastUpdatedBy,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    ISCURRENT = true,
                    DELETED = false,
                    OPERATION = "update",
                };
                context.TBL_TEMP_STAFF_RELIEF.Add(tempApprovalRelief);
            }

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffReliefUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Staff Relief of '{model.staffName}'",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.reliefId,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    this.auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                    output = context.SaveChanges() > 0;

                    targetReliefId = existingTempApprovalRelief?.TEMPRELIEFID ?? tempApprovalRelief.TEMPRELIEFID;

                    workFlow.StaffId = model.createdBy;
                    workFlow.CompanyId = model.companyId;
                    workFlow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workFlow.TargetId = targetReliefId;
                    workFlow.Comment = $"Update approval request for Staff Relief : {targetReliefId}";
                    workFlow.OperationId = (int)OperationsEnum.StaffReliefCreation;
                    workFlow.DeferredExecution = true;
                    workFlow.ExternalInitialization = true;
                    var response = workFlow.LogActivity();
                    context.SaveChanges();

                    //var entity = new ApprovalViewModel
                    //{
                    //    staffId = model.createdBy,
                    //    companyId = model.companyId,
                    //    approvalStatusId = (int)ApprovalStatusEnum.Processing,
                    //    targetId = targetReliefId,
                    //    operationId = (int)OperationsEnum.StaffReliefCreation,
                    //    BranchId = model.userBranchId,
                    //    externalInitialization = true
                    //};
                    //var response = workFlow.LogForApproval(entity);


                    if (response)
                    {
                        trans.Commit();

                        return output;
                    }
                    trans.Rollback();
                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }


        public IEnumerable<ApprovalReliefViewModel> GetApprovalReliefAwaitingApprovals(int staffId, int companyId)
        {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.StaffReliefCreation).ToList();

            var charge = (from a in context.TBL_TEMP_STAFF_RELIEF
                           join t in context.TBL_APPROVAL_TRAIL on a.TEMPRELIEFID equals t.TARGETID
                           where (t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                           && a.ISCURRENT == true
                           && a.DELETED == false
                           && t.RESPONSESTAFFID == null
                           && t.OPERATIONID == (int)OperationsEnum.StaffReliefCreation
                           && ids.Contains((int)t.TOAPPROVALLEVELID)
                          
                           select new ApprovalReliefViewModel
                           {
                               reliefId = a.TEMPRELIEFID,
                               relievedStaffId = a.STAFFID,
                               reliefStaffId = a.RELIEFSTAFFID,
                               staffName =  context.TBL_STAFF.Where(s => s.STAFFID == a.STAFFID)
                                                 .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                 .FirstOrDefault().name ?? "",
                               reliefStaffName = context.TBL_STAFF.Where(s => s.STAFFID == a.RELIEFSTAFFID)
                                                 .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                 .FirstOrDefault().name ?? "",
                               reliefReason = a.RELIEFREASON,
                               startDate = a.STARTDATE,
                               endDate = a.ENDDATE,
                               isActive = a.ISACTIVE,
                           }).ToList();

   
    //        var charge = context.TBL_TEMP_STAFF_RELIEF
    //.Join(context.TBL_APPROVAL_TRAIL,
    //    temp => temp.TEMPRELIEFID,
    //    trial => trial.TARGETID,
    //    (temp, trial) => new { TBL_TEMP_STAFF_RELIEF = temp, TBL_APPROVAL_TRAIL = trial })
    //.Where(o =>
    //   o.TBL_APPROVAL_TRAIL.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing 
    //   || o.TBL_APPROVAL_TRAIL.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
    //    && o.TBL_TEMP_STAFF_RELIEF.ISCURRENT == true
    //                          && o.TBL_APPROVAL_TRAIL.RESPONSESTAFFID == null
    //                          && o.TBL_APPROVAL_TRAIL.OPERATIONID == (int)OperationsEnum.StaffReliefCreation
    //                      && ids.Contains((int)o.TBL_APPROVAL_TRAIL.TOAPPROVALLEVELID))
    //.Select(a => new ApprovalReliefViewModel
    //{
    //    reliefId = a.TBL_TEMP_STAFF_RELIEF.TEMPRELIEFID,
    //    relievedStaffId = a.TBL_TEMP_STAFF_RELIEF.STAFFID,
    //    reliefStaffId = a.TBL_TEMP_STAFF_RELIEF.RELIEFSTAFFID,
    //    staffName = context.TBL_STAFF.Where(s => s.STAFFID == a.TBL_TEMP_STAFF_RELIEF.STAFFID)
    //                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
    //                                            .FirstOrDefault().name ?? "",
    //    reliefStaffName = context.TBL_STAFF.Where(s => s.STAFFID == a.TBL_TEMP_STAFF_RELIEF.RELIEFSTAFFID)
    //                                            .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
    //                                            .FirstOrDefault().name ?? "",
    //    reliefReason = a.TBL_TEMP_STAFF_RELIEF.RELIEFREASON,
    //    startDate = (DateTime)a.TBL_TEMP_STAFF_RELIEF.STARTDATE,
    //    endDate = (DateTime)a.TBL_TEMP_STAFF_RELIEF.ENDDATE,
    //    isActive = a.TBL_TEMP_STAFF_RELIEF.ISACTIVE,
    //})
    //.GroupBy(x => x.reliefId).Select(g => g.FirstOrDefault());

    //        var data = charge.ToList();

            return charge;
        }
        private bool ApproveStaffRelief(int targetId, short approvalStatusId, UserInfo user)
        {
            //var tempApprovalRelief = (from a in context.TBL_TEMP_STAFF_RELIEF where a.TEMPRELIEFID == targetId select a).FirstOrDefault();
            var tempApprovalRelief = context.TBL_TEMP_STAFF_RELIEF.Find(targetId);
            TBL_STAFF_RELIEF targetApprovalRelief;
            TBL_STAFF_RELIEF targetApprovalReliefForAudit;
            string previousReliefForAudit = string.Empty;
            if (tempApprovalRelief.RELIEFID > 0) 
            {
                targetApprovalRelief = context.TBL_STAFF_RELIEF.Find(tempApprovalRelief.RELIEFID);
                targetApprovalReliefForAudit = targetApprovalRelief;
                if (targetApprovalRelief != null)
                {
                    targetApprovalRelief.RELIEFREASON = tempApprovalRelief.RELIEFREASON;
                    targetApprovalRelief.RELIEFSTAFFID = tempApprovalRelief.RELIEFSTAFFID;
                    targetApprovalRelief.STAFFID = tempApprovalRelief.STAFFID;
                    targetApprovalRelief.STARTDATE = tempApprovalRelief.STARTDATE;
                    targetApprovalRelief.ENDDATE = tempApprovalRelief.ENDDATE;
                    targetApprovalRelief.ISACTIVE = tempApprovalRelief.ISACTIVE;
                    targetApprovalRelief.LASTUPDATEDBY = tempApprovalRelief.CREATEDBY;
                    targetApprovalRelief.DATETIMEUPDATED = DateTime.Now;
                    previousReliefForAudit = targetApprovalReliefForAudit.ToString();
                };
            }
            else
            {
                targetApprovalRelief = new TBL_STAFF_RELIEF()
                {
                    RELIEFREASON = tempApprovalRelief.RELIEFREASON,
                    RELIEFSTAFFID = tempApprovalRelief.RELIEFSTAFFID,
                    STAFFID = tempApprovalRelief.STAFFID,
                    STARTDATE = tempApprovalRelief.STARTDATE,
                    ENDDATE = tempApprovalRelief.ENDDATE,
                    ISACTIVE = tempApprovalRelief.ISACTIVE,
                    CREATEDBY = tempApprovalRelief.CREATEDBY,
                    DATETIMECREATED = DateTime.Now,
                };
                context.TBL_STAFF_RELIEF.Add(targetApprovalRelief);
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ApprovalReliefApproved,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = "Approved Approval Relief " + previousReliefForAudit,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            try
            {


                auditTrail.AddAuditTrail(audit);
                // Audit Section ---------------------------
                var response = context.SaveChanges() > 0;
                tempApprovalRelief.RELIEFID = targetApprovalRelief.RELIEFID;
                tempApprovalRelief.APPROVALSTATUSID = approvalStatusId;
                tempApprovalRelief.ISCURRENT = false;

                context.SaveChanges();
                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }
        public bool GoForApproval(ApprovalViewModel entity)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.StaffId = entity.staffId;
                    workFlow.CompanyId = entity.companyId;
                    workFlow.StatusId = ((short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved) ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                    workFlow.TargetId = entity.targetId;
                    workFlow.Comment = entity.comment;
                    workFlow.OperationId = (int)OperationsEnum.StaffReliefCreation;

                    workFlow.LogActivity();

                    var b = workFlow.NextLevelId ?? 0;
                    if (b == 0 && workFlow.NewState != (int)ApprovalState.Ended) // check if this is the last level
                    {
                        trans.Rollback();
                        throw new SecureException("Approval Failed");
                    }

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        var response = ApproveStaffRelief(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        var tempApprovalRelief = (from a in context.TBL_TEMP_STAFF_RELIEF where a.TEMPRELIEFID == entity.targetId select a).FirstOrDefault();
                        if (tempApprovalRelief != null)
                        {
                            tempApprovalRelief.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                            tempApprovalRelief.ISCURRENT = true;
                            tempApprovalRelief.DATETIMEUPDATED = DateTime.Now;
                        }
                        context.SaveChanges();
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        public IEnumerable<ApprovalReliefViewModel> GetAllStaffRelief(int companyId, int staffId)
        {

            return context.TBL_STAFF_RELIEF
                .Where(x => x.STAFFID == staffId && x.DELETED == false)
                .OrderByDescending(x => x.RELIEFID)
                .Select(x => new ApprovalReliefViewModel
                {
                    reliefId = x.RELIEFID,
                    relievedStaffId = x.STAFFID,
                    reliefStaffId = x.RELIEFSTAFFID,
                    staffName = context.TBL_STAFF.Where(s => s.STAFFID == x.STAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                    reliefStaffName = context.TBL_STAFF.Where(s => s.STAFFID == x.RELIEFSTAFFID)
                                                .Select(s => new { name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME + " - " + s.STAFFCODE })
                                                .FirstOrDefault().name ?? "",
                    reliefReason = x.RELIEFREASON,
                    startDate = x.STARTDATE,
                    endDate = x.ENDDATE,
                    isActive = x.ISACTIVE,
                    approvedBy = context.TBL_STAFF.Where(o => o.STAFFID == x.CREATEDBY).Select(i => i.LASTNAME + " " + i.MIDDLENAME + " " + i.FIRSTNAME).FirstOrDefault(),
                });
        }

        public bool AddStaffRelief(ApprovalReliefViewModel model)
        {
            bool output = false;
            var staffRecord = context.TBL_STAFF.Where(o => o.STAFFID == model.relievedStaffId).Select(i => i.LASTNAME + " " + i.MIDDLENAME + " " + i.FIRSTNAME).FirstOrDefault();
            var reliefStaffRecord = context.TBL_STAFF.Where(o => o.STAFFID == model.reliefStaffId).Select(i => i.LASTNAME + " " + i.MIDDLENAME + " " + i.FIRSTNAME).FirstOrDefault();

            TBL_STAFF_RELIEF data;

            data = new TBL_STAFF_RELIEF()
            {
                STAFFID = model.relievedStaffId,
                RELIEFSTAFFID = model.reliefStaffId,
                RELIEFREASON = model.reliefReason,
                STARTDATE = model.startDate,
                ENDDATE = model.endDate,
                ISACTIVE = model.isActive,
                DATETIMECREATED = general.GetApplicationDate(),
                CREATEDBY = (int)model.createdBy
            };


            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.StaffReliefAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added '{staffRecord}' as Staff Relief for: '{reliefStaffRecord}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = model.reliefId
            };

            context.TBL_STAFF_RELIEF.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            return context.SaveChanges() > 0;
        }

    }
}
