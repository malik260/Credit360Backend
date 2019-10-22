using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FintrakBanking.Common.CustomException;

using System.Data.Entity.Validation;
using System.Linq;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Setups.General
{
    public class MonitoringSetupRepository : IMonitoringSetupRepository
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private FinTrakBankingContext context;

        public MonitoringSetupRepository(
            IAuditTrailRepository _auditTrail,
            IGeneralSetupRepository genSetup, 
            FinTrakBankingContext _context
            )
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this._genSetup = genSetup;
        }

        private bool SaveAll()
        {
            try
            {
                return this.context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException ex)
            {
                string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                throw new DbEntityValidationException(errorMessages);
            }
        }
        public bool AddMonitoringSetup(MonitoringSetupViewModel entity)
        {
            try
            {
                var MonitoringSetup = new TBL_MONITORING_ALERT_SETUP 
                {
                    MONITORING_ITEM_NAME = entity.monitoringItemName,
                    MESSAGE_TEMPLATE = entity.messageTemplate,
                    MESSAGETYPEID = entity.messageTypeId,
                  //  NOTIFICATION_PERIOD = entity.notificationPeriod,
                   // PRODUCTID = entity.productId,
                };

                this.context.TBL_MONITORING_ALERT_SETUP.Add(MonitoringSetup);
                //context.SaveChanges();

                // Audit Section ----------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.MonitoringSetupAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = "Added new tbl_MonitoringSetup ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                };

                auditTrail.AddAuditTrail(audit);
                return SaveAll();
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }
           

        //public bool DeleteMonitoringSetup(int MonitoringSetupId)
        //{
        //    var MonitoringSetup = this.context.TBL_MONITORING_SETUP.Find(MonitoringSetupId);
        //    MonitoringSetup.DELETED = true;
        //    return SaveAll();
        //}


        public IEnumerable<MonitoringSetupViewModel> GetAllMonitoringSetup()
        {
            var MonitoringSetup = (from d in context.TBL_MONITORING_ALERT_SETUP
                                   select new MonitoringSetupViewModel()
                              {
                                  monitoringItemId = d.MONITORING_ITEMID,
                                  monitoringItemName = d.MONITORING_ITEM_NAME,
                                 // notificationPeriod = d.NOTIFICATION_PERIOD,
                                  messageTypeId = d.MESSAGETYPEID,
                                  messageTemplate = d.MESSAGE_TEMPLATE,
                                  messageTypeName = context.TBL_MESSAGE_LOG_TYPE.Where(o=>o.MESSAGETYPEID==d.MESSAGETYPEID).Select(o=>o.MESSAGETYPENAME).FirstOrDefault(),
                                ///  productId = d.TBL_PRODUCT.PRODUCTID,
                                 // productName = d.TBL_PRODUCT.PRODUCTDESCRIPTION,

                                   }).ToList();
            return MonitoringSetup;
        }



        public IEnumerable<MonitoringSetupViewModel> GetAllMessageType() 
        {
            var MonitoringSetup = (from d in context.TBL_MESSAGE_LOG_TYPE
                                   select new MonitoringSetupViewModel()
                                   {
                                       messageTypeId = d.MESSAGETYPEID,
                                       messageTypeName = d.MESSAGETYPENAME,

                                   }).ToList();
            return MonitoringSetup;
        }


        public IEnumerable<MonitoringSetupViewModel> GetAllProduct()
        {
            var MonitoringSetup = (from d in context.TBL_PRODUCT 
                                   select new MonitoringSetupViewModel()
                                   {
                                       productId = d.PRODUCTID,
                                       productName = d.PRODUCTNAME,

                                   }).ToList();
            return MonitoringSetup;
        }

        public MonitoringSetupViewModel GetMonitoringSetup(int MonitoringSetupId)
        {
            var MonitoringSetup = (from d in context.TBL_MONITORING_ALERT_SETUP
                                   where d.MONITORING_ITEMID == MonitoringSetupId
                              select new MonitoringSetupViewModel()
                              {
                                  monitoringItemId = d.MONITORING_ITEMID,
                                  monitoringItemName = d.MONITORING_ITEM_NAME,
                                 // notificationPeriod = d.NOTIFICATION_PERIOD,
                                  messageTypeId = d.MESSAGETYPEID,
                                  messageTemplate = d.MESSAGE_TEMPLATE,
                               //   productId = d.PRODUCTID,
                              }).SingleOrDefault();
            return MonitoringSetup;
        }

        public bool UpdateMonitoringSetup(int MonitoringSetupId, MonitoringSetupViewModel entity)
        {
            var MonitoringSetup = context.TBL_MONITORING_ALERT_SETUP.Find(MonitoringSetupId);

            MonitoringSetup.MONITORING_ITEM_NAME = entity.monitoringItemName;
            MonitoringSetup.MESSAGE_TEMPLATE = entity.messageTemplate;
          //  MonitoringSetup.NOTIFICATION_PERIOD = entity.notificationPeriod;
            MonitoringSetup.MESSAGETYPEID = entity.messageTypeId;
            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.MonitoringSetupUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_MonitoringSetup with Id: {entity.monitoringItemId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
            };

            auditTrail.AddAuditTrail(audit);
            return SaveAll();
        }
    }
}