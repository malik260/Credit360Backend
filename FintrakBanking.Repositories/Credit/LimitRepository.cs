using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ILimitRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LimitRepository : ILimitRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public LimitRepository(FinTrakBankingContext _context,
                                IGeneralSetupRepository genSetup, 
                                IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        #region Limits
        public IEnumerable<LimitViewModel> GetAllLimit(int companyId)
        {
            var data = (from a in context.tbl_Limit
                        where a.Deleted == false && a.CompanyId == companyId
                        orderby a.LimitValueTypeId
                        select new LimitViewModel
                        {
                            limitId = a.LimitId,
                            limitName = a.LimitName,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            limitMetricId = a.LimitMetricId,
                            limitMetric = a.tbl_Limit_Metric.LimitMetricName,
                            limitValueTypeId = a.LimitValueTypeId,
                            limitValueType = a.tbl_Limit_Value_Type.LimitValueTypeName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<LimitViewModel> GetLimitById(int limitId)
        {
            var data = (from a in context.tbl_Limit
                        where a.Deleted == false && a.LimitId == limitId
                        orderby a.LimitValueTypeId
                        select new LimitViewModel
                        {
                            limitId = a.LimitId,
                            limitName = a.LimitName,
                            companyId = a.CompanyId,
                            companyName = a.tbl_Company.Name,
                            limitMetricId = a.LimitMetricId,
                            limitMetric = a.tbl_Limit_Metric.LimitMetricName,
                            limitValueTypeId = a.LimitValueTypeId,
                            limitValueType = a.tbl_Limit_Value_Type.LimitValueTypeName,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public bool AddLimit(LimitViewModel model)
        {
            var data = new tbl_Limit
            {
                LimitName = model.limitName,
                LimitValueTypeId = model.limitValueTypeId,
                LimitMetricId = model.limitMetricId,
                CompanyId = model.companyId,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = _genSetup.GetApplicaionDate()
            };

            context.tbl_Limit.Add(data);

            // Audit Section ---------------------------
            
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Limit '{ data.LimitName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool UpdateLimit(int limitId, LimitViewModel model)
        {
            var data = this.context.tbl_Limit.Find(limitId);
            if (data == null) return false;

            data.LimitName = model.limitName;
            data.LimitValueTypeId = model.limitValueTypeId;
            data.LimitMetricId = model.limitMetricId;
            data.CompanyId = model.companyId;
            data.LastUpdatedBy = (int)model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Limit : '{ data.LimitName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteLimit(int limitId, UserInfo user)
        {
            var data = context.tbl_Limit.Find(limitId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var limit = this.context.tbl_Limit.SingleOrDefault(x => x.LimitId == limitId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Limit : '{ limit.LimitName }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
        #endregion


        #region Limits Details
        public IEnumerable<LimitDetailViewModel> GetAllLimitDetail()
        {
            var data = (from a in context.tbl_Limit_Detail
                        where a.Deleted == false 
                        orderby a.LimitDetailId
                        select new LimitDetailViewModel
                        {
                            limitDetailId = a.LimitDetailId,
                            limitId = a.LimitId,
                            limitName = a.tbl_Limit.LimitName,
                            limitTypeId = a.LimitTypeId,
                            limitTypeName = a.tbl_Limit_Type.LimitTypeName,
                            minimumValue = a.MinimumValue,
                            maximumValue = a.MaximumValue,
                            targetId = a.TargetId,
                            limitFrequencyTypeId = a.LimitFrequencyTypeId,
                            limitFrequencyTypeName = a.tbl_Frequency_Type.Mode,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public List<LimitDetailViewModel> GetLimitDetailById(int limitDetailId)
        {
            var data = (from a in context.tbl_Limit_Detail
                        where a.Deleted == false && a.LimitDetailId == limitDetailId
                        orderby a.MinimumValue
                        select new LimitDetailViewModel
                        {
                            limitDetailId = a.LimitDetailId,
                            limitId = a.LimitId,
                            limitName = a.tbl_Limit.LimitName,
                            limitTypeId = a.LimitTypeId,
                            limitTypeName = a.tbl_Limit_Type.LimitTypeName,
                            minimumValue = a.MinimumValue,
                            maximumValue = a.MaximumValue,
                            targetId = a.TargetId,
                            limitFrequencyTypeId = a.LimitFrequencyTypeId,
                            limitFrequencyTypeName = a.tbl_Frequency_Type.Mode,
                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public bool AddLimitDetail(LimitDetailViewModel model)
        {
            var data = new tbl_Limit_Detail
            {
                LimitTypeId = model.limitTypeId,
                LimitId = model.limitId,
                MaximumValue = model.maximumValue,
                MinimumValue = model.minimumValue,
                TargetId = model.targetId,
                LimitFrequencyTypeId = model.limitFrequencyTypeId,
                CreatedBy = (int)model.createdBy,
                DateTimeCreated = _genSetup.GetApplicaionDate()
        };

            context.tbl_Limit_Detail.Add(data);

            // Audit Section ---------------------------
            var audit_limit_detail = (context.tbl_Limit.FirstOrDefault(x => x.LimitId == model.limitId));
            var audit_limit_type = (context.tbl_Limit_Type.FirstOrDefault(x => x.LimitTypeId == model.limitTypeId));
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDetailAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Limit Deatil for limit: '{ audit_limit_detail.LimitName }' with type: '{audit_limit_type.LimitTypeName}' and values between: '{model.minimumValue} - {model.maximumValue}'",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool UpdateLimitDetail(int limitDetailId, LimitDetailViewModel model)
        {
            var data = this.context.tbl_Limit_Detail.Find(limitDetailId);
            if (data == null) return false;

            data.LimitTypeId = model.limitTypeId;
            data.LimitId = model.limitId;
            data.MaximumValue = model.maximumValue;
            data.MinimumValue = model.minimumValue;
            data.TargetId = model.targetId;
            data.LimitFrequencyTypeId = model.limitFrequencyTypeId;
            data.LastUpdatedBy = (int)model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var audit_limit_detail = (context.tbl_Limit.FirstOrDefault(x => x.LimitId == data.LimitId));
            var audit_limit_type = (context.tbl_Limit_Type.FirstOrDefault(x => x.LimitTypeId == data.LimitTypeId));
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDetailUpdated,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated '{ audit_limit_detail.LimitName }' limit on type: '{audit_limit_type.LimitTypeName}' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteLimitDetail(int limitDetailId, UserInfo user)
        {
            var data = context.tbl_Limit_Detail.Find(limitDetailId);
            data.Deleted = true;
            data.DeletedBy = (int)user.staffId;
            data.DateTimeDeleted = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var audit_limit_detail = (context.tbl_Limit.FirstOrDefault(x => x.LimitId == data.LimitId));
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDetailDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Limit detail '{ audit_limit_detail.LimitName }'. ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
        #endregion

        #region Limits Metric
        public IEnumerable<LimitMetricViewModel> GetAllLimitMetric()
        {
            var data = (from a in context.tbl_Limit_Metric
                            //where a.Deleted == false
                        orderby a.LimitMetricId
                        select new LimitMetricViewModel
                        {
                            limitMetricId = a.LimitMetricId,
                            limitMetricName = a.LimitMetricName,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion

        #region Limits Type
        public IEnumerable<LimitTypeViewModel> GetAllLimitType()
        {
            var data = (from a in context.tbl_Limit_Type
                            //where a.Deleted == false
                        orderby a.LimitTypeId
                        select new LimitTypeViewModel
                        {
                            limitTypeId = a.LimitTypeId,
                            limitTypeName = a.LimitTypeName,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion
        
        #region Limits Value Type
        public IEnumerable<LimitValueTypeViewModel> GetAllLimitValueType()
        {
            var data = (from a in context.tbl_Limit_Value_Type
                        //where a.Deleted == false
                        orderby a.LimitValueTypeId
                        select new LimitValueTypeViewModel
                        {
                            limitValueTypeId = a.LimitValueTypeId,
                            limitValueTypeName = a.LimitValueTypeName,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion

        #region Frequency Type
        public IEnumerable<FrequencyTypeViewModel> GetAllFrequencyType()
        {
            var data = (from a in context.tbl_Frequency_Type 
                            //where a.Deleted == false
                        orderby a.Mode
                        select new FrequencyTypeViewModel
                        {
                             frequencyTypeId = a.FrequencyTypeId,
                             mode = a.Mode,
                             description = a.Description,
                             value = a.Value,
                             isVisible = a.IsVisible 
                        }).ToList();
            return data;
        }
        #endregion

    }
}
