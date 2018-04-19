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
            var data = (from a in _context.TBL_LIMIT
                        where a.DELETED == false && a.COMPANYID == companyId
                        orderby a.LIMITVALUETYPEID
                        select new LimitViewModel
                        {
                            limitId = a.LIMITID,
                            limitName = a.LIMITNAME,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            limitMetricId = a.LIMITMETRICID,
                            limitMetric = a.TBL_LIMIT_METRIC.LIMITMETRICNAME,
                            limitValueTypeId = a.LIMITVALUETYPEID,
                            limitValueType = a.TBL_LIMIT_VALUE_TYPE.LIMITVALUETYPENAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<LimitViewModel> GetLimitById(int limitId)
        {
            var data = (from a in _context.TBL_LIMIT
                        where a.DELETED == false && a.LIMITID == limitId
                        orderby a.LIMITVALUETYPEID
                        select new LimitViewModel
                        {
                            limitId = a.LIMITID,
                            limitName = a.LIMITNAME,
                            companyId = a.COMPANYID,
                            companyName = a.TBL_COMPANY.NAME,
                            limitMetricId = a.LIMITMETRICID,
                            limitMetric = a.TBL_LIMIT_METRIC.LIMITMETRICNAME,
                            limitValueTypeId = a.LIMITVALUETYPEID,
                            limitValueType = a.TBL_LIMIT_VALUE_TYPE.LIMITVALUETYPENAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public bool AddLimit(LimitViewModel model)
        {
            var data = new TBL_LIMIT
            {
                LIMITNAME = model.limitName,
                LIMITVALUETYPEID = model.limitValueTypeId,
                LIMITMETRICID = model.limitMetricId,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            _context.TBL_LIMIT.Add(data);

            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added Limit '{ data.LIMITNAME }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateLimit(int limitId, LimitViewModel model)
        {
            var data = _context.TBL_LIMIT.Find(limitId);
            if (data == null) return false;

            data.LIMITNAME = model.limitName;
            data.LIMITVALUETYPEID = model.limitValueTypeId;
            data.LIMITMETRICID = model.limitMetricId;
            data.COMPANYID = model.companyId;
            data.LASTUPDATEDBY = model.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated Limit : '{ data.LIMITNAME }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool DeleteLimit(int limitId, UserInfo user)
        {
            var data = _context.TBL_LIMIT.Find(limitId);
            if (data != null)
            {
                data.DELETED = true;
                data.DELETEDBY = user.staffId;
                data.DATETIMEDELETED = _genSetup.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var limit = _context.TBL_LIMIT.SingleOrDefault(x => x.LIMITID == limitId);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Limit : '{ limit?.LIMITNAME }' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                targetTable = (from a in _context.TBL_CUSTOMER
                               select new TargetViewModel
                               {
                                   targetId = a.CUSTOMERID,
                                   targetName = a.CUSTOMERCODE + " - " + a.FIRSTNAME + " " + a.LASTNAME
                               }).ToList();

            }
            if (id == (int)LimitType.Sector)
            {
                targetTable = (from a in _context.TBL_SECTOR
                               select new TargetViewModel
                               {
                                   targetId = a.SECTORID,
                                   targetName = a.CODE + " - " + a.NAME
                               }).ToList();
            }
            if (id == (int)LimitType.Branch)
            {
                targetTable = (from a in _context.TBL_BRANCH
                               select new TargetViewModel
                               {
                                   targetId = a.BRANCHID,
                                   targetName = a.BRANCHCODE + " - " + a.BRANCHNAME
                               }).ToList();
            }
            if (id == (int)LimitType.RelationshipManager)
            {
                targetTable = (from a in _context.TBL_STAFF
                               select new TargetViewModel
                               {
                                   targetId = a.STAFFID,
                                   targetName = a.STAFFCODE + " - " + a.FIRSTNAME + " " + a.LASTNAME
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
                targetTable = (from a in _context.TBL_CUSTOMER_GROUP
                               select new TargetViewModel
                               {
                                   targetId = a.CUSTOMERGROUPID,
                                   targetName = a.GROUPCODE + "  - " + a.GROUPNAME
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
            var data = (from a in _context.TBL_LIMIT_DETAIL
                        where a.DELETED == false && a.LIMITTYPEID == id
                        orderby a.LIMITDETAILID
                        select a).ToList();
            var kk = new List<LimitDetailViewModel>();
            if (data != null)
            {

                foreach (var a in data)
                {
                    var target = "n/a";
                    var getTarget = targetTable.Where(x => x.targetId == a.TARGETID).FirstOrDefault().targetName;
                    if (getTarget != null)
                    {
                        target = getTarget;
                    }
                    var b = new LimitDetailViewModel();
                    b.limitDetailId = a.LIMITDETAILID;
                    b.limitId = a.LIMITID;
                    b.limitName = a.TBL_LIMIT.LIMITNAME;
                    b.limitTypeId = a.LIMITTYPEID;
                    b.limitTypeName = a.TBL_LIMIT_TYPE.LIMITTYPENAME;
                    b.minimumValue = a.MINIMUMVALUE;
                    b.maximumValue = a.MAXIMUMVALUE;
                    b.targetId = a.TARGETID;
                    b.targetName = target;
                    b.limitFrequencyTypeId = a.LIMITFREQUENCYTYPEID;
                    b.limitFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE;
                    b.dateTimeCreated = a.DATETIMECREATED;
                    b.createdBy = a.CREATEDBY;
                    kk.Add(b);
                }

            }

            return kk;
        }

        public List<LimitDetailViewModel> GetLimitDetailById(int limitDetailId)
        {
            var data = (from a in _context.TBL_LIMIT_DETAIL
                        where a.DELETED == false && a.LIMITDETAILID == limitDetailId
                        orderby a.MINIMUMVALUE
                        select new LimitDetailViewModel
                        {
                            limitDetailId = a.LIMITDETAILID,
                            limitId = a.LIMITID,
                            limitName = a.TBL_LIMIT.LIMITNAME,
                            limitTypeId = a.LIMITTYPEID,
                            limitTypeName = a.TBL_LIMIT_TYPE.LIMITTYPENAME,
                            minimumValue = a.MINIMUMVALUE,
                            maximumValue = a.MAXIMUMVALUE,
                            targetId = a.TARGETID,
                            limitFrequencyTypeId = a.LIMITFREQUENCYTYPEID,
                            limitFrequencyTypeName = a.TBL_FREQUENCY_TYPE.MODE,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public bool AddLimitDetail(LimitDetailViewModel model)
        {
            var data = new TBL_LIMIT_DETAIL
            {
                LIMITTYPEID = model.limitTypeId,
                LIMITID = model.limitId,
                MAXIMUMVALUE = model.maximumValue,
                MINIMUMVALUE = model.minimumValue,
                TARGETID = model.targetId,
                LIMITFREQUENCYTYPEID = model.limitFrequencyTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            _context.TBL_LIMIT_DETAIL.Add(data);

            // Audit Section ---------------------------
            var auditLimitDetail = _context.TBL_LIMIT.FirstOrDefault(x => x.LIMITID == model.limitId);
            var auditLimitType = _context.TBL_LIMIT_TYPE.FirstOrDefault(x => x.LIMITTYPEID == model.limitTypeId);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitDetailAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added Limit Deatil for limit: '{ auditLimitDetail?.LIMITNAME }' with type: '{auditLimitType?.LIMITTYPENAME}' and values between: '{model.minimumValue} - {model.maximumValue}'",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                    var data = new TBL_LIMIT_DETAIL
                    {
                        LIMITTYPEID = item.limitTypeId,
                        LIMITID = item.limitId,
                        MAXIMUMVALUE = item.maximumValue,
                        MINIMUMVALUE = item.minimumValue,
                        TARGETID = item.targetId,
                        LIMITFREQUENCYTYPEID = item.limitFrequencyTypeId,
                        CREATEDBY = item.createdBy,
                        DATETIMECREATED = _genSetup.GetApplicationDate()
                    };

                    _context.TBL_LIMIT_DETAIL.Add(data);

                    // Audit Section ---------------------------
                    var auditLimitDetail = _context.TBL_LIMIT.FirstOrDefault(x => x.LIMITID == item.limitId);
                    var auditLimitType = _context.TBL_LIMIT_TYPE.FirstOrDefault(x => x.LIMITTYPEID == item.limitTypeId);
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.LimitDetailAdded,
                        STAFFID = item.createdBy,
                        BRANCHID = item.userBranchId,
                        DETAIL =
                            $"Added Limit Deatil for limit: '{auditLimitDetail?.LIMITNAME}' with type: '{auditLimitType?.LIMITTYPENAME}' and values between: '{item.minimumValue} - {item.maximumValue}'",
                        IPADDRESS = item.userIPAddress,
                        URL = item.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
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
            var data = _context.TBL_LIMIT_DETAIL.Find(limitDetailId);
            if (data == null) return false;

            data.LIMITTYPEID = model.limitTypeId;
            data.LIMITID = model.limitId;
            data.MAXIMUMVALUE = model.maximumValue;
            data.MINIMUMVALUE = model.minimumValue;
            data.TARGETID = model.targetId;
            data.LIMITFREQUENCYTYPEID = model.limitFrequencyTypeId;
            data.LASTUPDATEDBY = model.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var auditLimitDetail = _context.TBL_LIMIT.FirstOrDefault(x => x.LIMITID == data.LIMITID);
            var auditLimitType = _context.TBL_LIMIT_TYPE.FirstOrDefault(x => x.LIMITTYPEID == data.LIMITTYPEID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitDetailUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated '{ auditLimitDetail?.LIMITNAME }' limit on type: '{auditLimitType?.LIMITTYPENAME}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool DeleteLimitDetail(int limitDetailId, UserInfo user)
        {
            var data = _context.TBL_LIMIT_DETAIL.Find(limitDetailId);
            if (data == null) return _context.SaveChanges() != 0;
            data.DELETED = true;
            data.DELETEDBY = user.staffId;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var auditLimitDetail = _context.TBL_LIMIT.FirstOrDefault(x => x.LIMITID == data.LIMITID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitDetailDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Limit detail '{auditLimitDetail?.LIMITNAME}'. ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion

        #region Limits Metric
        public IEnumerable<LimitMetricViewModel> GetAllLimitMetric()
        {
            var data = (from a in _context.TBL_LIMIT_METRIC
                            //where a.Deleted == false
                        orderby a.LIMITMETRICID
                        select new LimitMetricViewModel
                        {
                            limitMetricId = a.LIMITMETRICID,
                            limitMetricName = a.LIMITMETRICNAME,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion

        #region Limits Type
        public IEnumerable<LimitTypeViewModel> GetAllLimitType()
        {
            var data = (from a in _context.TBL_LIMIT_TYPE
                            //where a.Deleted == false
                        orderby a.LIMITTYPEID
                        select new LimitTypeViewModel
                        {
                            limitTypeId = a.LIMITTYPEID,
                            limitTypeName = a.LIMITTYPENAME,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion

        #region Limits Value Type
        public IEnumerable<LimitValueTypeViewModel> GetAllLimitValueType()
        {
            var data = (from a in _context.TBL_LIMIT_VALUE_TYPE
                            //where a.Deleted == false
                        orderby a.LIMITVALUETYPEID
                        select new LimitValueTypeViewModel
                        {
                            limitValueTypeId = a.LIMITVALUETYPEID,
                            limitValueTypeName = a.LIMITVALUETYPENAME,
                            //DateTimeCreated = a.DateTimeCreated,
                            //CreatedBy = a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion

        #region Frequency Type
        public IEnumerable<FrequencyTypeViewModel> GetAllFrequencyType()
        {
            var data = (from a in _context.TBL_FREQUENCY_TYPE
                            //where a.Deleted == false
                        orderby a.MODE
                        select new FrequencyTypeViewModel
                        {
                            frequencyTypeId = a.FREQUENCYTYPEID,
                            mode = a.MODE,
                            description = a.DESCRIPTION,
                            value = a.VALUE,
                            isVisible = a.ISVISIBLE
                        }).ToList();
            return data;
        }
        #endregion

        #region Obligor Limit 
        public IEnumerable<ObligorLimitViewModel> GetAllObligorLimit()
        {
            var data = (from a in _context.TBL_CUSTOMER_RISK_RATING
                        select new ObligorLimitViewModel
                        {
                            riskRatingId = a.RISKRATINGID,
                            riskRating = a.RISKRATING,
                            description = a.DESCRIPTION,
                            companyId = a.COMPANYID,
                            isInvestmentGrade = a.ISINVESTMENTGRADE,
                            maxShareholderPercentage = a.MAX_SHAREHOLDER_FUND_PERCENTAG,
                        }).ToList();
            return data;
        }
        public bool AddUpdateRiskRating(ObligorLimitViewModel entity)
        {
            if (entity != null)
            {
                try
                {

                    TBL_CUSTOMER_RISK_RATING risk;
                    if (entity.riskRatingId > 0)
                    {
                        risk = _context.TBL_CUSTOMER_RISK_RATING.Find(entity.riskRatingId);
                        if (risk != null)
                        {
                            risk.RISKRATING = entity.riskRating;
                            risk.DESCRIPTION = entity.description;
                            risk.ISINVESTMENTGRADE = entity.isInvestmentGrade;
                            risk.MAX_SHAREHOLDER_FUND_PERCENTAG = entity.maxShareholderPercentage;
                        }
                    }
                    else
                    {
                        risk = new TBL_CUSTOMER_RISK_RATING()
                        {
                            RISKRATING = entity.riskRating,
                            DESCRIPTION = entity.description,
                            ISINVESTMENTGRADE = entity.isInvestmentGrade,
                            MAX_SHAREHOLDER_FUND_PERCENTAG = entity.maxShareholderPercentage,
                            COMPANYID = entity.companyId
                        };
                        _context.TBL_CUSTOMER_RISK_RATING.Add(risk);
                    }
                    // Audit Section ----------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = (short)entity.userBranchId,
                        DETAIL = "Added/Modified Staff Role",
                        IPADDRESS = entity.userIPAddress,
                        URL = entity.applicationUrl,
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    this._auditTrail.AddAuditTrail(audit);

                    var response = _context.SaveChanges() != 0;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            return false;
        }
        public bool ValidateRiskRating(string riskRating)
        {
            return _context.TBL_CUSTOMER_RISK_RATING.Where(x => x.RISKRATING == riskRating).Any();
        }
        #endregion
    }
}
