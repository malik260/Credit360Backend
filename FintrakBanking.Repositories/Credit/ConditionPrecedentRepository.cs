using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    public class ConditionPrecedentRepository : IConditionPrecedentRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public ConditionPrecedentRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByDetailId(int detailId)
        {
            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(detailId);
            var sectorId = context.TBL_SUB_SECTOR.Where(s => s.SUBSECTORID == applicationDetail.SUBSECTORID).Select(s=>s.SUBSECTORID).FirstOrDefault();
            return GetConditionPrecedentDefaultByProductId(applicationDetail.APPROVEDPRODUCTID, sectorId, applicationDetail.SUBSECTORID);
        }

        public List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByDetailIdLms(int detailId)
        {
            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Find(detailId);
            return GetConditionPrecedentDefaultByProductId(applicationDetail.PRODUCTID,1,1);
        }

        public List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByProductId(int? productId,int sectorId,int subSectorId)
        {
            var conditions = this.context.TBL_CONDITION_PRECEDENT.Where(x => x.PRODUCTID == productId || x.SECTORID == sectorId || x.SUBSECTORID == subSectorId).ToList();
            var test = conditions.Select(c => new ConditionPrecedentViewModel
            {
                conditionId = c.CONDITIONID,
                condition = c.CONDITION,
                isExternal = c.ISEXTERNAL,
                isSubsequent = c.ISSUBSEQUENT,
                isDocument = c.ISDOCUMENT,
                corporate = c.CORPORATE,
                retail = c.RETAIL,
                productId = c.PRODUCTID,
                product = context.TBL_PRODUCT.Where(p => p.PRODUCTID == productId).Select(p => p.PRODUCTNAME).FirstOrDefault() ?? "N/A" ,
                timelineId = c.TIMELINEID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
                operationId = c.OPERATIONID,
                isCheckListSpecific = context.TBL_OPERATIONS.FirstOrDefault(o=>o.OPERATIONID==c.OPERATIONID)?.ISCHECKLISTSPECIFIC.Value
            })
            .OrderBy(x => x.isSubsequent).ThenByDescending(x => x.isExternal);

            return test.ToList();
            //return conditions.ToList();
        }

        private List<ConditionPrecedentViewModel> GetSectorConditionPrecedentDefault()
        {
            var conditions = this.context.TBL_CONDITION_PRECEDENT.Where(x => x.SECTORID != null || x.SUBSECTORID != null).ToList();
            var test = conditions.Select(c => new ConditionPrecedentViewModel
            {
                conditionId = c.CONDITIONID,
                condition = c.CONDITION,
                isExternal = c.ISEXTERNAL,
                isSubsequent = c.ISSUBSEQUENT,
                isDocument = c.ISDOCUMENT,
                corporate = c.CORPORATE,
                retail = c.RETAIL,
                productId = c.PRODUCTID,
                product = context.TBL_PRODUCT.Where(p => p.PRODUCTID == c.PRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault() ?? "N/A",
                timelineId = c.TIMELINEID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
                operationId = c.OPERATIONID,
                isCheckListSpecific = context.TBL_OPERATIONS.FirstOrDefault(o => o.OPERATIONID == c.OPERATIONID)?.ISCHECKLISTSPECIFIC.Value
            })
            .OrderBy(x => x.isSubsequent).ThenByDescending(x => x.isExternal);

            return test.ToList();
            //return conditions.ToList();
        }

        public bool AddConditionPrecedent(ConditionPrecedentViewModel model)
        {
            var data = new TBL_LOAN_CONDITION_PRECEDENT
            {
                CONDITION = model.condition,
                ISEXTERNAL = (bool)model.isExternal,
                ISSUBSEQUENT = model.isSubsequent,
                CREATEDBY = model.createdBy,
                ISDOCUMENT = model.isDocument,
                //LOANAPPLICATIONID = model.loanApplicationId,
                TIMELINEID = model.timelineId,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                RESPONSE_TYPEID = 1,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LOAN_CONDITION_PRECEDENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent '{ model.conditionId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public List<ConditionPrecedentViewModel> AddSelectedConditionPrecedent(SelectedIdsViewModel entity)
        {
            var loanconditions = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.CONDITIONID != null
                && x.LOANAPPLICATIONDETAILID == entity.detailId

            );

            var conditions = context.TBL_CONDITION_PRECEDENT.Where(x => entity.selectedIds.Contains(x.CONDITIONID)).ToList();
            foreach (var c in conditions)
            {
                if (!loanconditions.Any(x => x.CONDITIONID == (int)c.CONDITIONID))
                {
                    context.TBL_LOAN_CONDITION_PRECEDENT.Add(new TBL_LOAN_CONDITION_PRECEDENT
                    {
                        CONDITION = c.CONDITION,
                        CONDITIONID = c.CONDITIONID,
                        ISEXTERNAL = (bool)c.ISEXTERNAL,
                        ISSUBSEQUENT = c.ISSUBSEQUENT,
                        CREATEDBY = c.CREATEDBY,
                        //CHECKLISTVALIDATED=false,
                        ISDOCUMENT = c.ISDOCUMENT,
                        TIMELINEID = c.TIMELINEID,
                        LOANAPPLICATIONDETAILID = entity.detailId,
                        RESPONSE_TYPEID = c.RESPONSE_TYPEID,
                        DATETIMECREATED = general.GetApplicationDate(),
                    });
                }
            }
            context.SaveChanges();

            var deletableIds = loanconditions.Where(x => x.CONDITIONID != null && !entity.selectedIds.Contains((int)x.CONDITIONID)).Select(x => x.LOANCONDITIONID);
            context.TBL_LOAN_CONDITION_DEFERRAL.RemoveRange(
                context.TBL_LOAN_CONDITION_DEFERRAL.Where(x => deletableIds.Contains((int)x.LOANCONDITIONID))
            );
            context.TBL_LOAN_CONDITION_PRECEDENT.RemoveRange(
                context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => deletableIds.Contains((int)x.LOANCONDITIONID))
            );
            context.SaveChanges();

            return GetConditionPrecedentByDetailId(entity.detailId).ToList();
        }

        public bool EditLoanConditionPrecedent(int id, ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = (bool)model.isExternal;
            data.ISSUBSEQUENT = (bool)model.isSubsequent;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            data.TIMELINEID = model.timelineId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.ISDOCUMENT = model.isDocument;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Condition Precedent '{ model.conditionId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedent()
        {
            var x = this.context.TBL_LOAN_CONDITION_PRECEDENT
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new ConditionPrecedentViewModel
                    {
                        loanConditionId = c.LOANCONDITIONID, 
                        condition = c.CONDITION,
                        conditionId = c.CONDITIONID == null ? 0 : (int)c.CONDITIONID,
                        isExternal = c.ISEXTERNAL,
                        isSubsequent = c.ISSUBSEQUENT,
                        isDocument = c.ISDOCUMENT,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                        loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                        timelineId = c.TIMELINEID,
                        responseTypeId = c.RESPONSE_TYPEID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                        //sectorId = s.
                        
                        
                    });

            var test = x.ToList();

            return x;
        }

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByDetailId(int detailId)
        {
            return this.GetAllConditionPrecedent().Where(x => x.loanApplicationDetailId == detailId);
        }

        public List<ConditionPrecedentViewModel> GetConditionPrecedentDefaultByApplicationIdAndOperationLms(int detailId, int? operationId)
        {
            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Find(detailId);
           
            var customer = context.TBL_CUSTOMER.Find(applicationDetail.CUSTOMERID);
            List<ConditionPrecedentViewModel> sectorConditions = new List<ConditionPrecedentViewModel>();

            sectorConditions.AddRange(GetSectorConditionPrecedentDefault().Where(x => x.subSectorId == customer?.SUBSECTORID).ToList());

            if (operationId != null)
            {
                var operation = context.TBL_OPERATIONS.Find(operationId);
                if (operation.ISCHECKLISTSPECIFIC != null)
                {
                    var output = GetConditionPrecedentDefaultByProductId(applicationDetail.PRODUCTID,1,1).Where(x => x.operationId == operationId).ToList();

                    return output.Union(sectorConditions).ToList();
                }
            }
             
            return GetConditionPrecedentDefaultByProductId(applicationDetail.PRODUCTID,1,1).Union(sectorConditions).ToList();
        }

        #region CP Template

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentTemplate()
        {
            return this.context.TBL_CONDITION_PRECEDENT.Where(x=> x.DELETED == false)
            .Select(c => new ConditionPrecedentViewModel
            {
                conditionId = c.CONDITIONID,
                condition = c.CONDITION,
                isExternal = c.ISEXTERNAL,
                isSubsequent = c.ISSUBSEQUENT,
                isDocument = c.ISDOCUMENT,
                corporate = c.CORPORATE,
                retail = c.RETAIL,
                productId = c.PRODUCTID,
                responseTypeId = c.RESPONSE_TYPEID,
                timelineId = c.TIMELINEID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
            });
        }

        public bool AddConditionPrecedentTemplate(ConditionPrecedentViewModel model)
        {
            var data = new TBL_CONDITION_PRECEDENT
            {
                CONDITION = model.condition,
                ISEXTERNAL = model.isExternal,
                ISSUBSEQUENT = model.isSubsequent,
                ISDOCUMENT = model.isDocument,
                PRODUCTID = model.productId,
                TIMELINEID = model.timelineId,
                SECTORID = model.sectorId,
                SUBSECTORID = model.subSectorId,
                CORPORATE = true,
                RETAIL = false,
                RESPONSE_TYPEID = model.responseTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_CONDITION_PRECEDENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent template '{ model.conditionId }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateConditionPrecedentTemplate(ConditionPrecedentViewModel model, int conditionId)
        {
            var data = this.context.TBL_CONDITION_PRECEDENT.Find(conditionId);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = model.isExternal;
            data.ISSUBSEQUENT = model.isSubsequent;
            data.PRODUCTID = (short)model.productId;
            data.TIMELINEID = model.timelineId;
            data.ISDOCUMENT = model.isDocument;
            //data.CORPORATE = model.corporate;
            //data.RETAIL = model.retail;
            data.RESPONSE_TYPEID = model.responseTypeId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Condition Precedent template '{ model.conditionId }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveLoanConditionPrecedent(int id, UserInfo model)
        {
            var data = this.context.TBL_LOAN_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LOAN_CONDITION_PRECEDENT.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Condition Precedent '{ data.CONDITION }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        #endregion CP Template

        #region Timeline for Compliance

        public IEnumerable<ComplianceTimelineViewModel> GetComplianceTimelineTemplate()
        {
            return this.context.TBL_COMPLIANCE_TIMELINE.Where(x => x.DELETED == false)
            .Select(c => new ComplianceTimelineViewModel
            {
                timelineId = c.TIMELINEID,
                timeline = c.TIMELINE,
                duration = c.DURATIONINDAYS,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
            });
        }

        public bool AddComplianceTimelineTemplate(ComplianceTimelineViewModel model)
        {
            var data = new TBL_COMPLIANCE_TIMELINE
            {
                TIMELINE = model.timeline,
                DURATIONINDAYS = model.duration,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_COMPLIANCE_TIMELINE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ComplianceTimelineAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent template '{ model.timelineId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateComplianceTimelineTemplate(ComplianceTimelineViewModel model, int timelineId)
        {
            var data = this.context.TBL_COMPLIANCE_TIMELINE.Find(timelineId);
            if (data == null)
            {
                return false;
            }

            data.TIMELINE = model.timeline;
            data.DURATIONINDAYS = model.duration;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ComplianceTimelineUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Condition Precedent template '{ model.timelineId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveComplianceTimelineTemplate(UserInfo user, int timelineId)
        {
            var data = this.context.TBL_COMPLIANCE_TIMELINE.Find(timelineId);
            if (data == null)
            {
                return false;
            }

            data.DELETED = true;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = user.staffId;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ComplianceTimelineUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete timeline for compliance'{ timelineId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteConditionPrecedentTemplate(UserInfo user, int id)
        {
            var data = this.context.TBL_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }
            data.DELETED = true;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = user.staffId;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Delete Condition Precedent '{ id }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        #endregion Timeline for Compliance

        #region LMS approval process

        public bool RemoveLoanConditionPrecedentLms(int id, UserInfo model)
        {
            var data = this.context.TBL_LMSR_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LMSR_CONDITION_PRECEDENT.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Condition Precedent '{ data.CONDITION }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanConditionPrecedentLms(int id, ConditionPrecedentViewModel model)
        {
            var data = this.context.TBL_LMSR_CONDITION_PRECEDENT.Find(id);
            if (data == null)
            {
                return false;
            }

            data.CONDITION = model.condition;
            data.ISEXTERNAL = (bool)model.isExternal;
            data.ISSUBSEQUENT = (bool)model.isSubsequent;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.LOANREVIEWAPPLICATIONID = model.loanApplicationDetailId;
            data.TIMELINEID = model.timelineId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.ISDOCUMENT = model.isDocument;
            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Condition Precedent '{ model.conditionId }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public List<ConditionPrecedentViewModel> AddSelectedConditionPrecedentLms(SelectedIdsViewModel entity)
        {
            var loanconditions = context.TBL_LMSR_CONDITION_PRECEDENT.Where(x => x.CONDITIONID != null
                && x.LOANREVIEWAPPLICATIONID == entity.detailId
            );

            var conditions = context.TBL_CONDITION_PRECEDENT.Where(x => entity.selectedIds.Contains(x.CONDITIONID)).ToList();
            foreach (var c in conditions)
            {
                if (!loanconditions.Any(x => x.CONDITIONID == (int)c.CONDITIONID))
                {
                    context.TBL_LMSR_CONDITION_PRECEDENT.Add(new TBL_LMSR_CONDITION_PRECEDENT
                    {
                        CONDITION = c.CONDITION,
                        CONDITIONID = c.CONDITIONID,
                        ISEXTERNAL = (bool)c.ISEXTERNAL,
                        ISSUBSEQUENT = c.ISSUBSEQUENT,
                        ISDOCUMENT = c.ISDOCUMENT,
                        CREATEDBY = c.CREATEDBY,
                        TIMELINEID = c.TIMELINEID,
                        LOANREVIEWAPPLICATIONID = entity.detailId,
                        RESPONSE_TYPEID = c.RESPONSE_TYPEID,
                        DATETIMECREATED = general.GetApplicationDate(),
                    });
                }
            }
            context.SaveChanges();

            var deletableIds = loanconditions.Where(x => x.CONDITIONID != null && !entity.selectedIds.Contains((int)x.CONDITIONID)).Select(x => x.LOANCONDITIONID);
            //context.TBL_LMSR_CONDITION_DEFERRAL.RemoveRange(
            //    context.TBL_LOAN_CONDITION_DEFERRAL.Where(x => deletableIds.Contains((int)x.LOANCONDITIONID))
            //);
            context.TBL_LMSR_CONDITION_PRECEDENT.RemoveRange(
                context.TBL_LMSR_CONDITION_PRECEDENT.Where(x => deletableIds.Contains((int)x.LOANCONDITIONID))
            );
            context.SaveChanges();

            return GetConditionPrecedentByDetailIdLms(entity.detailId).ToList();
        }

        public bool AddConditionPrecedentLms(ConditionPrecedentViewModel model)
        {
            var data = new TBL_LMSR_CONDITION_PRECEDENT
            {
                CONDITION = model.condition,
                ISEXTERNAL = (bool)model.isExternal,
                ISSUBSEQUENT = model.isSubsequent,
                CREATEDBY = model.createdBy,
                ISDOCUMENT = model.isDocument,
                //LOANAPPLICATIONID = model.loanApplicationId,
                TIMELINEID = model.timelineId,
                LOANREVIEWAPPLICATIONID = model.loanApplicationDetailId,
                RESPONSE_TYPEID = 1,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LMSR_CONDITION_PRECEDENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Condition Precedent '{ model.conditionId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL =model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByDetailIdLms(int detailId)
        {
            return this.GetAllConditionPrecedentLms().Where(x => x.loanApplicationDetailId == detailId);
        }

        public IEnumerable<ConditionPrecedentViewModel> GetAllConditionPrecedentLms()
        {
            var x = this.context.TBL_LMSR_CONDITION_PRECEDENT
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new ConditionPrecedentViewModel
                    {
                        loanConditionId = c.LOANCONDITIONID,
                        condition = c.CONDITION,
                        conditionId = c.CONDITIONID == null ? 0 : (int)c.CONDITIONID,
                        isExternal = c.ISEXTERNAL,
                        isSubsequent = c.ISSUBSEQUENT,
                        isDocument = c.ISDOCUMENT,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                        loanApplicationDetailId = c.LOANREVIEWAPPLICATIONID,
                        timelineId = c.TIMELINEID,
                        responseTypeId = c.RESPONSE_TYPEID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                    });

            var test = x.ToList();

            return x;
        }


        #endregion LMS approval process


        #region Additional Comments

        public List<AdditionalCommentViewModel> GetAdditionalComment(int applicationId, int callerId, int staffId)
        {
            return context.TBL_LOAN_APPLICATION_COMMENT.Where(x => x.DELETED == false && x.LOANAPPLICATIONID == applicationId && x.OPERATIONID == callerId)
            .Select(c => new AdditionalCommentViewModel
            {
                id = c.LOANCOMMENTID,
                callerId = c.OPERATIONID,
                additionalComment = c.COMMENTS,
                applicationId = c.LOANAPPLICATIONID,
                owner = c.CREATEDBY == staffId,
            })
            .ToList();
        }

        public bool AddAdditionalComment(AdditionalCommentViewModel model)
        {
            var data = new TBL_LOAN_APPLICATION_COMMENT
            {
                COMMENTS=model.additionalComment,
                LOANAPPLICATIONID=model.applicationId,
                OPERATIONID=model.callerId,
                DATETIMECREATED = general.GetApplicationDate(),
                CREATEDBY = model.createdBy,
            };

            context.TBL_LOAN_APPLICATION_COMMENT.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Additional Comment Condition  ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditAdditionalComment(int id, AdditionalCommentViewModel model)
        {
            var data = this.context.TBL_LOAN_APPLICATION_COMMENT.Find(id);
            if (data == null) return false;

            data.COMMENTS = model.additionalComment;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Additional Comment Condition' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool RemoveAdditionalComment(int id, UserInfo user)
        {
            var data = this.context.TBL_LOAN_APPLICATION_COMMENT.Find(id);
            if (data == null) return false;

            data.DELETED = true;
            data.DATETIMEDELETED = DateTime.Now;
            data.DELETEDBY = user.createdBy;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.ConditionPrecedentUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Additional Comment Condition' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        #endregion Additional Comments

    }
}
