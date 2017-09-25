using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using System.Data;
using System.Collections;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ILimitRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LimitRepository : ILimitRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;

        public LimitRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
        }

        #region Limits
        public IEnumerable<LimitViewModel> GetAllLimit(int companyId)
        {
            var data = (from a in _context.tbl_Limit
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
            var data = (from a in _context.tbl_Limit
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
                CreatedBy = model.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate()
            };

            _context.tbl_Limit.Add(data);

            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitAdded,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Added Limit '{ data.LimitName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateLimit(int limitId, LimitViewModel model)
        {
            var data = _context.tbl_Limit.Find(limitId);
            if (data == null) return false;

            data.LimitName = model.limitName;
            data.LimitValueTypeId = model.limitValueTypeId;
            data.LimitMetricId = model.limitMetricId;
            data.CompanyId = model.companyId;
            data.LastUpdatedBy = model.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitUpdated,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Updated Limit : '{ data.LimitName }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool DeleteLimit(int limitId, UserInfo user)
        {
            var data = _context.tbl_Limit.Find(limitId);
            if (data != null)
            {
                data.Deleted = true;
                data.DeletedBy = user.staffId;
                data.DateTimeDeleted = _genSetup.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var limit = _context.tbl_Limit.SingleOrDefault(x => x.LimitId == limitId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Limit : '{ limit?.LimitName }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion


        #region Limits Details
        public IEnumerable<LimitDetailViewModel> GetAllLimitDetail(int id)
        {
            var targetTable = new List<TargetViewModel>();
            if (id == (int)LimitType.Obligor)
            {
                targetTable = (from a in _context.tbl_Customer
                               select new TargetViewModel
                               {
                                   targetId = a.CustomerId,
                                   targetName = a.CustomerCode + " - " + a.FirstName + " " + a.LastName
                               }).ToList();

            }
         if (id == (int)LimitType.Sector)
            {
                targetTable = (from a in _context.tbl_Sector
                               select new TargetViewModel
                               {
                                   targetId = a.SectorId,
                                   targetName =a.Code + " - " + a.Name
                               }).ToList();
            }
            if (id == (int)LimitType.Branch)
            {
                targetTable = (from a in _context.tbl_Branch
                               select new TargetViewModel
                               {
                                   targetId = a.BranchId,
                                   targetName = a.BranchCode + " - " + a.BranchName
                               }).ToList();
            }
           if (id == (int)LimitType.RelationshipManager)
            {
                targetTable = (from a in _context.tbl_Staff
                               select new TargetViewModel
                               {
                                   targetId = a.StaffId,
                                   targetName =a.StaffCode +" - "+ a.FirstName + " " + a.LastName
                               }).ToList();
            }
            if (id == (int)LimitType.PrelimemaryEvaluationNote)
            {
                targetTable.Add(new TargetViewModel
                {
                    targetId = -1,
                    targetName = "PEN Limit"
                });
            }
            if (id == (int)LimitType.CustomerGroup)
            {
                targetTable = (from a in _context.tbl_Customer_Group
                               select new TargetViewModel
                               {
                                   targetId = a.CustomerGroupId,
                                   targetName =a.GroupCode +"  - "+ a.GroupName
                               }).ToList();
            }
            //var data = (from a in _context.tbl_Limit_Detail
            //            join b in targetTable on a.TargetId equals b.targetId
            //            where a.Deleted == false && a.LimitTypeId == id
            //            orderby a.LimitDetailId
            //            select new LimitDetailViewModel
            //            {
            //                limitDetailId = a.LimitDetailId,
            //                limitId = a.LimitId,
            //                limitName = a.tbl_Limit.LimitName,
            //                limitTypeId = a.LimitTypeId,
            //                limitTypeName = a.tbl_Limit_Type.LimitTypeName,
            //                minimumValue = a.MinimumValue,
            //                maximumValue = a.MaximumValue,
            //                targetId = a.TargetId,
            //                targetName = b.targetName,//targetTable.Where(x => x.targetId == a.TargetId).FirstOrDefault().targetName,
            //                limitFrequencyTypeId = a.LimitFrequencyTypeId,
            //                limitFrequencyTypeName = a.tbl_Frequency_Type.Mode,
            //                dateTimeCreated = a.DateTimeCreated,
            //                createdBy = a.CreatedBy
            //            }).ToList();
            var data = (from a in _context.tbl_Limit_Detail
                        where a.Deleted == false && a.LimitTypeId == id
                        orderby a.LimitDetailId
                        select a).ToList();
            var kk = new List<LimitDetailViewModel>();
            if (data != null)
            {

                foreach (var a in data)
                {
                    var target = "n/a";
                    var getTarget = targetTable.Where(x => x.targetId == a.TargetId).FirstOrDefault().targetName;
                    if (getTarget != null)
                    {
                        target = getTarget;
                    }
                    var b = new LimitDetailViewModel();
                    b.limitDetailId = a.LimitDetailId;
                    b.limitId = a.LimitId;
                    b.limitName = a.tbl_Limit.LimitName;
                    b.limitTypeId = a.LimitTypeId;
                    b.limitTypeName = a.tbl_Limit_Type.LimitTypeName;
                    b.minimumValue = a.MinimumValue;
                    b.maximumValue = a.MaximumValue;
                    b.targetId = a.TargetId;
                    b.targetName = target;
                    b.limitFrequencyTypeId = a.LimitFrequencyTypeId;
                    b.limitFrequencyTypeName = a.tbl_Frequency_Type.Mode;
                    b.dateTimeCreated = a.DateTimeCreated;
                    b.createdBy = a.CreatedBy;
                    kk.Add(b);
                }

            }

            return kk;
            }

            public List<LimitDetailViewModel> GetLimitDetailById(int limitDetailId)
            {
                var data = (from a in _context.tbl_Limit_Detail
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
                    CreatedBy = model.createdBy,
                    DateTimeCreated = _genSetup.GetApplicationDate()
                };

                _context.tbl_Limit_Detail.Add(data);

                // Audit Section ---------------------------
                var auditLimitDetail = _context.tbl_Limit.FirstOrDefault(x => x.LimitId == model.limitId);
                var auditLimitType = _context.tbl_Limit_Type.FirstOrDefault(x => x.LimitTypeId == model.limitTypeId);
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LimitDetailAdded,
                    StaffId = model.createdBy,
                    BranchId = model.userBranchId,
                    Detail = $"Added Limit Deatil for limit: '{ auditLimitDetail?.LimitName }' with type: '{auditLimitType?.LimitTypeName}' and values between: '{model.minimumValue} - {model.maximumValue}'",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                _auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
                return _context.SaveChanges() != 0;
            }

            public bool AddMultipleLimitDetail(List<LimitDetailViewModel> model)
            {
                if (model != null)
                {
                    foreach (var item in model)
                    {
                        var data = new tbl_Limit_Detail
                        {
                            LimitTypeId = item.limitTypeId,
                            LimitId = item.limitId,
                            MaximumValue = item.maximumValue,
                            MinimumValue = item.minimumValue,
                            TargetId = item.targetId,
                            LimitFrequencyTypeId = item.limitFrequencyTypeId,
                            CreatedBy = item.createdBy,
                            DateTimeCreated = _genSetup.GetApplicationDate()
                        };

                        _context.tbl_Limit_Detail.Add(data);

                        // Audit Section ---------------------------
                        var auditLimitDetail = _context.tbl_Limit.FirstOrDefault(x => x.LimitId == item.limitId);
                        var auditLimitType = _context.tbl_Limit_Type.FirstOrDefault(x => x.LimitTypeId == item.limitTypeId);
                        var audit = new tbl_Audit
                        {
                            AuditTypeId = (short)AuditTypeEnum.LimitDetailAdded,
                            StaffId = item.createdBy,
                            BranchId = item.userBranchId,
                            Detail =
                                $"Added Limit Deatil for limit: '{auditLimitDetail?.LimitName}' with type: '{auditLimitType?.LimitTypeName}' and values between: '{item.minimumValue} - {item.maximumValue}'",
                            IPAddress = item.userIPAddress,
                            Url = item.applicationUrl,
                            ApplicationDate = _genSetup.GetApplicationDate(),
                            SystemDateTime = DateTime.Now
                        };

                        _auditTrail.AddAuditTrail(audit);

                        //end of Audit section -----------------------
                    }

                    return _context.SaveChanges() != 0;
                }
                else
                {
                    return false;
                }
            }

            public bool UpdateLimitDetail(int limitDetailId, LimitDetailViewModel model)
            {
                var data = _context.tbl_Limit_Detail.Find(limitDetailId);
                if (data == null) return false;

                data.LimitTypeId = model.limitTypeId;
                data.LimitId = model.limitId;
                data.MaximumValue = model.maximumValue;
                data.MinimumValue = model.minimumValue;
                data.TargetId = model.targetId;
                data.LimitFrequencyTypeId = model.limitFrequencyTypeId;
                data.LastUpdatedBy = model.createdBy;
                data.DateTimeUpdated = _genSetup.GetApplicationDate();

                // Audit Section ---------------------------
                var auditLimitDetail = _context.tbl_Limit.FirstOrDefault(x => x.LimitId == data.LimitId);
                var auditLimitType = _context.tbl_Limit_Type.FirstOrDefault(x => x.LimitTypeId == data.LimitTypeId);
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LimitDetailUpdated,
                    StaffId = model.createdBy,
                    BranchId = model.userBranchId,
                    Detail = $"Updated '{ auditLimitDetail?.LimitName }' limit on type: '{auditLimitType?.LimitTypeName}' ",
                    IPAddress = model.userIPAddress,
                    Url = model.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                _auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
                return _context.SaveChanges() != 0;
            }

            public bool DeleteLimitDetail(int limitDetailId, UserInfo user)
            {
                var data = _context.tbl_Limit_Detail.Find(limitDetailId);
                if (data == null) return _context.SaveChanges() != 0;
                data.Deleted = true;
                data.DeletedBy = user.staffId;
                data.DateTimeDeleted = _genSetup.GetApplicationDate();

                // Audit Section ---------------------------
                var auditLimitDetail = _context.tbl_Limit.FirstOrDefault(x => x.LimitId == data.LimitId);
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.LimitDetailDeleted,
                    StaffId = user.staffId,
                    BranchId = (short)user.BranchId,
                    Detail = $"Deleted Limit detail '{auditLimitDetail?.LimitName}'. ",
                    IPAddress = user.userIPAddress,
                    Url = user.applicationUrl,
                    ApplicationDate = _genSetup.GetApplicationDate(),
                    SystemDateTime = DateTime.Now
                };

                _auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------
                return _context.SaveChanges() != 0;
            }
            #endregion

            #region Limits Metric
            public IEnumerable<LimitMetricViewModel> GetAllLimitMetric()
            {
                var data = (from a in _context.tbl_Limit_Metric
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
                var data = (from a in _context.tbl_Limit_Type
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
                var data = (from a in _context.tbl_Limit_Value_Type
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
                var data = (from a in _context.tbl_Frequency_Type
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
