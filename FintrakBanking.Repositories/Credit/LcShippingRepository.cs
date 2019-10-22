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
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.credit
{
    public class LcShippingRepository : ILcShippingRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LcShippingRepository(
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

        public IEnumerable<LcShippingViewModel> GetLcShippings()
        {
            return context.TBL_LC_SHIPPING.Where(x => x.DELETED == false)
                .Select(x => new LcShippingViewModel
                {
                    lcShippingId = x.LCSHIPPINGID,
                    lcIssuanceId = x.LCISSUANCEID,
                    partyName = x.PARTYNAME,
                    partyAddress = x.PARTYADDRESS,
                    portOfDischarge = x.PORTOFDISCHARGE,
                    portOfShipment = x.PORTOFSHIPMENT,
                    latestShipmentDate = x.LATESTSHIPMENTDATE,
                    isPartShipmentAllowed = x.ISPARTSHIPMENTALLOWED,
                    isTransShipmentAllowed = x.ISTRANSSHIPMENTALLOWED,
                })
                .ToList();
        }

        public IEnumerable<LcShippingViewModel> GetLcShippingsByIssuanceId(int lcIssuanceId)
        {
            return context.TBL_LC_SHIPPING.Where(x => x.LCISSUANCEID == lcIssuanceId && x.DELETED == false)
                .Select(x => new LcShippingViewModel
                {
                    lcShippingId = x.LCSHIPPINGID,
                    lcIssuanceId = x.LCISSUANCEID,
                    partyName = x.PARTYNAME,
                    partyAddress = x.PARTYADDRESS,
                    portOfDischarge = x.PORTOFDISCHARGE,
                    portOfShipment = x.PORTOFSHIPMENT,
                    latestShipmentDate = x.LATESTSHIPMENTDATE,
                    isPartShipmentAllowed = x.ISPARTSHIPMENTALLOWED,
                    isTransShipmentAllowed = x.ISTRANSSHIPMENTALLOWED,
                })
                .ToList();
        }

        public LcShippingViewModel GetLcShipping(int id)
        {
            var entity = context.TBL_LC_SHIPPING.FirstOrDefault(x => x.LCSHIPPINGID == id && x.DELETED == false);

            return new LcShippingViewModel
            {
                lcShippingId = entity.LCSHIPPINGID,
                lcIssuanceId = entity.LCISSUANCEID,
                partyName = entity.PARTYNAME,
                partyAddress = entity.PARTYADDRESS,
                portOfDischarge = entity.PORTOFDISCHARGE,
                portOfShipment = entity.PORTOFSHIPMENT,
                latestShipmentDate = entity.LATESTSHIPMENTDATE,
                isPartShipmentAllowed = entity.ISPARTSHIPMENTALLOWED,
                isTransShipmentAllowed = entity.ISTRANSSHIPMENTALLOWED,
            };
        }

        public bool AddLcShipping(LcShippingViewModel model)
        {
            var entity = new TBL_LC_SHIPPING
            {
                LCISSUANCEID = model.lcIssuanceId,
                PARTYNAME = model.partyName,
                PARTYADDRESS = model.partyAddress,
                PORTOFDISCHARGE = model.portOfDischarge,
                PORTOFSHIPMENT = model.portOfShipment,
                LATESTSHIPMENTDATE = model.latestShipmentDate,
                ISPARTSHIPMENTALLOWED = model.isPartShipmentAllowed,
                ISTRANSSHIPMENTALLOWED = model.isTransShipmentAllowed,
                // COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_LC_SHIPPING.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Lc Shipping '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateLcShipping(LcShippingViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_SHIPPING.Find(id);
            entity.LCISSUANCEID = model.lcIssuanceId;
            entity.PARTYNAME = model.partyName;
            entity.PARTYADDRESS = model.partyAddress;
            entity.PORTOFDISCHARGE = model.portOfDischarge;
            entity.PORTOFSHIPMENT = model.portOfShipment;
            entity.LATESTSHIPMENTDATE = model.latestShipmentDate;
            entity.ISPARTSHIPMENTALLOWED = model.isPartShipmentAllowed;
            entity.ISTRANSSHIPMENTALLOWED = model.isTransShipmentAllowed;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Shipping '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LCSHIPPINGID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteLcShipping(int id, UserInfo user)
        {
            var entity = this.context.TBL_LC_SHIPPING.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LcShippingDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Lc Shipping '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LCSHIPPINGID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }        

    }
}

           // kernel.Bind<ILcShippingRepository>().To<LcShippingRepository>();
           // LcShippingAdded = ???, LcShippingUpdated = ???, LcShippingDeleted = ???,
