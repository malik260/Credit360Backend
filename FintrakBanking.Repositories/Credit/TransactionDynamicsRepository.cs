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
using System.Net.Http;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    public class TransactionDynamicsRepository : ITransactionDynamicsRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public TransactionDynamicsRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddTransactionDynamics(TransactionDynamicsViewModel model)
        {
            if (model.isExternal == null)
            {
                model.isExternal = false;
            }
            var data = new TBL_LOAN_TRANSACTION_DYNAMICS
            {
                DYNAMICS = model.dynamics,
                CREATEDBY = model.createdBy,
                //LOANAPPLICATIONID = model.loanApplicationId,
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                DATETIMECREATED = general.GetApplicationDate(),
                POSITION = model.position,
                ISEXTERNAL = model.isExternal,
            };

            context.TBL_LOAN_TRANSACTION_DYNAMICS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanTransactionDynamics(int id, TransactionDynamicsViewModel model)
        {
            var data = this.context.TBL_LOAN_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }

            data.DYNAMICS = model.dynamics;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.POSITION = model.position;
            data.ISEXTERNAL = model.isExternal;
            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TransactionDynamicsViewModel> GetAllTransactionDynamics()
        {
            return this.context.TBL_LOAN_TRANSACTION_DYNAMICS
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new TransactionDynamicsViewModel
                    {
                        loanDynamicsId = c.LOANDYNAMICSID,
                        dynamicsId = c.DYNAMICSID == null ? 0 : (int)c.DYNAMICSID,
                        dynamics = c.DYNAMICS,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                        loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                        position = c.POSITION,
                        isExternal = c.ISEXTERNAL,
                    });
        }

        public IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByDetailId(int detailId)
        {
            return this.GetAllTransactionDynamics().Where(x => x.loanApplicationDetailId == detailId).OrderBy(a=> a.position);
        }

        public bool RemoveLoanTransactionDynamics(int id, UserInfo model)
        {
            var data = this.context.TBL_LOAN_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LOAN_TRANSACTION_DYNAMICS.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Transaction Dynamics '{ data.DYNAMICS }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public List<TransactionDynamicsViewModel> AddSelectedTransactionDynamics(SelectedIdsViewModel entity)
        {
            var loanconditions = context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(x => x.DYNAMICSID != null
                && x.LOANAPPLICATIONDETAILID == entity.detailId
            );

            var conditions = context.TBL_TRANSACTION_DYNAMICS.Where(x => entity.selectedIds.Contains(x.DYNAMICSID)).ToList();
            foreach (var c in conditions)
            {
                if (!loanconditions.Any(x => x.DYNAMICSID == (int)c.DYNAMICSID))
                {
                    if (c.ISEXTERNAL == null)
                    {
                        c.ISEXTERNAL = false;
                    }
                    context.TBL_LOAN_TRANSACTION_DYNAMICS.Add(new TBL_LOAN_TRANSACTION_DYNAMICS
                    {
                        DYNAMICS = c.DYNAMICS,
                        DYNAMICSID = c.DYNAMICSID,
                        CREATEDBY = c.CREATEDBY,
                        LOANAPPLICATIONDETAILID = entity.detailId,
                        DATETIMECREATED = general.GetApplicationDate(),
                        POSITION = 1,
                        ISEXTERNAL = c.ISEXTERNAL,
                        
                    });
                }
            }
            context.SaveChanges();

            var deletableIds = loanconditions.Where(x => x.DYNAMICSID != null && !entity.selectedIds.Contains((int)x.DYNAMICSID)).Select(x => x.LOANDYNAMICSID);
            context.TBL_LOAN_TRANSACTION_DYNAMICS.RemoveRange(
                context.TBL_LOAN_TRANSACTION_DYNAMICS.Where(x => deletableIds.Contains((int)x.LOANDYNAMICSID))
            );
            context.SaveChanges();

            return GetTransactionDynamicsByDetailId(entity.detailId).ToList();
        }

        #region CP Template

        public IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsTemplate()
        {
            return this.context.TBL_TRANSACTION_DYNAMICS
            .Select(c => new TransactionDynamicsViewModel
            {
                dynamicsId = c.DYNAMICSID,
                dynamics = c.DYNAMICS,
                productId = c.PRODUCTID,
                productName = c.TBL_PRODUCT.PRODUCTNAME,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
                isExternal = c.ISEXTERNAL==null?false : c.ISEXTERNAL,
            });
        }

        public bool AddTransactionDynamicsTemplate(TransactionDynamicsViewModel model)
        {
            if (model.isExternal == null)
            {
                model.isExternal = false;
            }
            var data = new TBL_TRANSACTION_DYNAMICS
            {
                DYNAMICS = model.dynamics,
                PRODUCTID = (short)model.productId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                ISEXTERNAL = model.isExternal,
            };

            context.TBL_TRANSACTION_DYNAMICS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Transaction Dynamics template '{ model.dynamicsId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateTransactionDynamicsTemplate(TransactionDynamicsViewModel model, int dynamicsId)
        {
            var data = this.context.TBL_TRANSACTION_DYNAMICS.Find(dynamicsId);
            if (data == null)
            {
                return false;
            }

            data.DYNAMICS = model.dynamics;
            data.PRODUCTID = (short)model.productId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = DateTime.Now;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();
            data.ISEXTERNAL = model.isExternal;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Transaction Dynamics template '{ model.dynamicsId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByDetailId(int detailId)
        {
            var ids = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONDETAILID == detailId)
                .Select(x => x.APPROVEDPRODUCTID)
                .Distinct();

            var dynamics = this.context.TBL_TRANSACTION_DYNAMICS.Where(x => ids.Contains((short)x.PRODUCTID))
            .Select(c => new TransactionDynamicsViewModel
            {
                dynamicsId = c.DYNAMICSID,
                dynamics = c.DYNAMICS,
                loanApplicationDetailId = c.PRODUCTID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
                isExternal = c.ISEXTERNAL
            });

            return dynamics.ToList();
        }

        public List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByDetailIdLms(int detailId)
        {
            var ids = context.TBL_LMSR_APPLICATION_DETAIL
                .Where(x => x.LOANREVIEWAPPLICATIONID == detailId)
                .Select(x => x.PRODUCTID)
                .Distinct();

            var dynamics = this.context.TBL_TRANSACTION_DYNAMICS.Where(c => ids.Contains((short)c.PRODUCTID))
            .Select(c => new TransactionDynamicsViewModel
            {
                dynamicsId = c.DYNAMICSID,
                dynamics = c.DYNAMICS,
                loanApplicationDetailId = c.PRODUCTID,
                dateTimeCreated = c.DATETIMECREATED,
                dateTimeUpdated = c.DATETIMEUPDATED,
                isExternal = c.ISEXTERNAL,
                operationId = c.OPERATIONID,
                isCheckListSpecific = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == c.OPERATIONID).Select(o => o.ISCHECKLISTSPECIFIC).FirstOrDefault().Value
            }).ToList();

            return dynamics;
        }

        public List<TransactionDynamicsViewModel> GetTransactionDynamicsDefaultByApplicationIdAndOperationLms(int detailId, int? operationId)
        {
            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Find(detailId);
            if (operationId != null)
            {
                var operation = context.TBL_OPERATIONS.Find(operationId);
                if (operation.ISCHECKLISTSPECIFIC != null && operation.ISCHECKLISTSPECIFIC.Value)
                {
                    var output = GetTransactionDynamicsDefaultByDetailIdLms(applicationDetail.LOANREVIEWAPPLICATIONID);
                    return output.Where(x => x.operationId == operationId).ToList();

                }
            }
            return GetTransactionDynamicsDefaultByDetailIdLms(applicationDetail.LOANREVIEWAPPLICATIONID);
        }

        #endregion CP Template

        #region LMS approval process

        public List<TransactionDynamicsViewModel> AddSelectedTransactionDynamicsLms(SelectedIdsViewModel entity)
        {
            var loanconditions = context.TBL_LMSR_TRANSACTION_DYNAMICS.Where(x => x.DYNAMICSID != null
                && x.LOANREVIEWAPPLICATIONID == entity.detailId
            );

            var conditions = context.TBL_TRANSACTION_DYNAMICS.Where(x => entity.selectedIds.Contains(x.DYNAMICSID)).ToList();
            foreach (var c in conditions)
            {
                if (!loanconditions.Any(x => x.DYNAMICSID == (int)c.DYNAMICSID))
                {
                    if (c.ISEXTERNAL == null)
                    {
                        c.ISEXTERNAL = false;
                    }
                    context.TBL_LMSR_TRANSACTION_DYNAMICS.Add(new TBL_LMSR_TRANSACTION_DYNAMICS
                    {
                        DYNAMICS = c.DYNAMICS,
                        DYNAMICSID = c.DYNAMICSID,
                        CREATEDBY = c.CREATEDBY,
                        LOANREVIEWAPPLICATIONID = entity.detailId,
                        DATETIMECREATED = general.GetApplicationDate(),
                        POSITION = 1,
                        ISEXTERNAL = c.ISEXTERNAL,
                        
                    });
                }
            }
            context.SaveChanges();

            var deletableIds = loanconditions.Where(x => x.DYNAMICSID != null && !entity.selectedIds.Contains((int)x.DYNAMICSID)).Select(x => x.LOANDYNAMICSID);
            context.TBL_LMSR_TRANSACTION_DYNAMICS.RemoveRange(
                context.TBL_LMSR_TRANSACTION_DYNAMICS.Where(x => deletableIds.Contains((int)x.LOANDYNAMICSID))
            );
            context.SaveChanges();

            return GetTransactionDynamicsByDetailIdLms(entity.detailId).ToList();
        }

        public bool RemoveLoanTransactionDynamicsLms(int id, UserInfo model)
        {
            var data = this.context.TBL_LMSR_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }
            context.TBL_LMSR_TRANSACTION_DYNAMICS.Remove(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.staffId,
                BRANCHID = (short)model.BranchId,
                DETAIL = $"Remove Transaction Dynamics '{ data.DYNAMICS }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool EditLoanTransactionDynamicsLms(int id, TransactionDynamicsViewModel model)
        {
            var data = this.context.TBL_LMSR_TRANSACTION_DYNAMICS.Find(id);
            if (data == null)
            {
                return false;
            }

            data.DYNAMICS = model.dynamics;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.LOANREVIEWAPPLICATIONID = model.loanApplicationDetailId;
            data.DATETIMEUPDATED = DateTime.Now;
            data.POSITION = model.position;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.ISEXTERNAL = model.isExternal;

            context.Entry(data).State = System.Data.Entity.EntityState.Modified;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool AddTransactionDynamicsLms(TransactionDynamicsViewModel model)
        {
            if (model.isExternal == null)
            {
                model.isExternal = false;
            }
            var data = new TBL_LMSR_TRANSACTION_DYNAMICS
            {
                DYNAMICS = model.dynamics,
                CREATEDBY = model.createdBy,
                POSITION = model.position,
                //LOANAPPLICATIONID = model.loanApplicationId,
                LOANREVIEWAPPLICATIONID = model.loanApplicationDetailId,
                DATETIMECREATED = general.GetApplicationDate(),
                ISEXTERNAL = model.isExternal==null? false : model.isExternal,
            };

            context.TBL_LMSR_TRANSACTION_DYNAMICS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.TransactionDynamicsAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Transaction Dynamics '{ model.dynamicsId }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<TransactionDynamicsViewModel> GetTransactionDynamicsByDetailIdLms(int detailId)
        {
            return this.GetAllTransactionDynamicsLms().Where(x => x.loanApplicationDetailId == detailId).OrderBy(a => a.position); // lms
        }

        public IEnumerable<TransactionDynamicsViewModel> GetAllTransactionDynamicsLms()
        {
            return this.context.TBL_LMSR_TRANSACTION_DYNAMICS
                .Join(
                    context.TBL_STAFF,
                    c => c.CREATEDBY,
                    s => s.STAFFID,
                    (c, s) => new TransactionDynamicsViewModel
                    {
                        
                        loanDynamicsId = c.LOANDYNAMICSID,
                        dynamicsId = c.DYNAMICSID == null ? 0 : (int)c.DYNAMICSID,
                        dynamics = c.DYNAMICS,
                        position = c.POSITION,
                        staffName = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                        loanApplicationId = c.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID,
                        loanApplicationDetailId = c.LOANREVIEWAPPLICATIONID,
                        dateTimeCreated = c.DATETIMECREATED,
                        dateTimeUpdated = c.DATETIMEUPDATED,
                        isExternal = c.ISEXTERNAL,
                    });
        }

        #endregion LMS approval process


        #region SUGGESTED conditions

        public bool AddSuggestedConditions(SuggestedConditionsViewModel entity)
        {
            if (entity == null)
            {
                return false;
            }

            var suggestedCondition = new TBL_SUGGESTED_CONDITION
            {
                LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId,
                SUGGESTIONTYPEID = entity.suggestionTypeId,
                //APPLICATIONID = entity.applicationId,
                DESCRIPTION = entity.description,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = entity.dateTimeCreated
            };
            context.TBL_SUGGESTED_CONDITIONS.Add(suggestedCondition);
            return context.SaveChanges() != 0;
        }

        public List<SuggestedConditionsViewModel> GetSuggestedConditions(int applicationDetailId)
        {
            return context.TBL_SUGGESTED_CONDITIONS.Where(c => c.LOANAPPLICATIONDETAILID == applicationDetailId).Select(c => new SuggestedConditionsViewModel
            {
                suggestionid = c.SUGGESTEDCONDITIONID,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                suggestionTypeId = c.SUGGESTIONTYPEID,
                description = c.DESCRIPTION,
                //applicationId = c.APPLICATIONID,
            }).ToList();
        }

        public List<SuggestedConditionsViewModel> GetSuggestedConditionsByApplicationId(int applicationId)
        {
            // get all detalids in the appplication
            var allDetailIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == applicationId)
                .Select(l => l.LOANAPPLICATIONDETAILID).ToList();
            var conditions2 = (from id in allDetailIds
                               join c in context.TBL_SUGGESTED_CONDITIONS on id equals c.LOANAPPLICATIONDETAILID
                               orderby c.DATETIMECREATED
                               select new SuggestedConditionsViewModel
                               {
                                   suggestionid = c.SUGGESTEDCONDITIONID,
                                   loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                   suggestionTypeId = c.SUGGESTIONTYPEID,
                                   description = c.DESCRIPTION,
                                   //applicationId = c.APPLICATIONID,
                               }).ToList();
            return conditions2;
        }

        public bool UpdateSuggestedConditions(int id, SuggestedConditionsViewModel entity)
        {
            if (entity == null)
            {
                return false;
            }

            var suggestedCondition = context.TBL_SUGGESTED_CONDITIONS.Find(entity.suggestionid);
            if (suggestedCondition != null)
            {
                suggestedCondition.SUGGESTEDCONDITIONID = entity.suggestionid;
                suggestedCondition.LOANAPPLICATIONDETAILID = entity.loanApplicationDetailId;
                suggestedCondition.SUGGESTIONTYPEID = entity.suggestionTypeId;
                //suggestedCondition.APPLICATIONID = entity.applicationId;
                suggestedCondition.DESCRIPTION = entity.description;
                suggestedCondition.CREATEDBY = entity.createdBy;
                suggestedCondition.DATETIMECREATED = entity.dateTimeCreated;
                suggestedCondition.LASTUPDATEDBY = entity.lastUpdatedBy;
                suggestedCondition.DATETIMEUPDATED = entity.dateTimeUpdated;
            }
            return context.SaveChanges() != 0;
        }

        public bool RemoveSuggestedConditions(int id, UserInfo user)
        {
            var suggestedCondition = context.TBL_SUGGESTED_CONDITIONS.Find(id);
            if (suggestedCondition == null)
            {
                return false;
            }

            context.TBL_SUGGESTED_CONDITIONS.Remove(suggestedCondition);
            return context.SaveChanges() != 0;
        }

        #endregion SUGGESTED conditions

    }
}
