using FintrakBanking.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Text; 
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using System.Linq;
using FintrakBanking.Common.Enum; 
using FintrakBanking.ViewModels.Customer;
using System.ComponentModel.Composition;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Customer
{
 
    public class CustomerFSCaptionRepository : ICustomerFSCaptionRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSCaptionRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        private string GenerateCaptionCode(short fsCaptionGroupId)
        {
            var data = this.context.TBL_CUSTOMER_FS_CAPTION.Count(x => x.FSCAPTIONGROUPID == fsCaptionGroupId);
            int counter = data + 1;
            var code = string.Format("{0}", counter.ToString().PadLeft(3, '0'));
            return code;
        }

        public bool AddCustomerFSCaption(CustomerFSCaptionViewModel entity)
        {
            var data = new TBL_CUSTOMER_FS_CAPTION
            {
                FSCAPTIONNAME = entity.fsCaptionName,
                POSITION = entity.position,
                FSCAPTIONGROUPID = entity.fsCaptionGroupId,
                ISRATIO = entity.isRatio,
                CREATEDBY = (int)entity.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            context.TBL_CUSTOMER_FS_CAPTION.Add(data);

            // Audit Section ---------------------------
            var auditInfo = context.TBL_CUSTOMER_FS_CAPTION_GROUP.FirstOrDefault(x => x.FSCAPTIONGROUPID == data.FSCAPTIONGROUPID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer FS Caption : { data.FSCAPTIONNAME } in group ( {auditInfo.FSCAPTIONGROUPNAME })",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.FSCAPTIONID 
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool ValidateFSCaption(string captionName)
        {
            var exist = from a in context.TBL_CUSTOMER_FS_CAPTION where a.FSCAPTIONNAME == captionName select a;
            if (exist.Any())
            {
                return true;
            }
            return false;
        }

        public bool DeleteCustomerFSCaption(int fsCaptionId, UserInfo user)
        {
            var data = context.TBL_CUSTOMER_FS_CAPTION.Find(fsCaptionId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.createdBy;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var auditInfo = context.TBL_CUSTOMER_FS_CAPTION_GROUP.FirstOrDefault(x => x.FSCAPTIONGROUPID == data.FSCAPTIONGROUPID);
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted Customer FS Caption: { data.FSCAPTIONNAME } in group ( {auditInfo.FSCAPTIONGROUPNAME })",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetCustomerFSCaptionByGroupId(short fsCaptionGroupId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where   a.DELETED == false  && a.FSCAPTIONGROUPID == fsCaptionGroupId 
                        orderby a.FSCAPTIONGROUPID, a.POSITION
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCAPTIONID,                            
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            position = a.POSITION,
                            fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            fsCaptionName = a.FSCAPTIONNAME + " - " + a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            isRatio = a.ISRATIO,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetCustomerFSCaptions()
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where a.DELETED == false //&& a.ISRATIO == false 
                        orderby a.FSCAPTIONGROUPID, a.POSITION
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            position = a.POSITION,
                            fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            fsCaptionName = a.FSCAPTIONNAME + " - " + a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            isRatio = a.ISRATIO,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }


        public CustomerFSCaptionViewModel GetCustomerFSCaptionById(int fsCaptionId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where a.FSCAPTIONID == fsCaptionId
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.FSCAPTIONNAME,
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            isRatio = a.ISRATIO,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY, 
                        }).FirstOrDefault();
            return data;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetUnmappedCustomerFSCaption(short fsCaptionGroupId, int customerId, DateTime fsDate)
        {
            var dataList = (from data in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                            where data.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID == fsCaptionGroupId && data.CUSTOMERID == customerId 
                            && data.FSDATE == fsDate  && data.DELETED == false 
                            select data.FSCAPTIONID).ToList();

            var captions = (from a in context.TBL_CUSTOMER_FS_CAPTION
                       where a.FSCAPTIONGROUPID == fsCaptionGroupId && a.DELETED == false // && !dataList.Contains(data.ProductProductFeeId)
                       select new CustomerFSCaptionViewModel
                       {
                           fsCaptionId = a.FSCAPTIONID,
                           fsCaptionName = a.FSCAPTIONNAME,
                           fsCaptionGroupId = a.FSCAPTIONGROUPID,
                           isRatio = a.ISRATIO,
                           fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                           dateTimeCreated = a.DATETIMECREATED,
                           createdBy = a.CREATEDBY
                       });

            if (dataList.Any())
            {
                captions = captions.Where(x => !dataList.Contains(x.fsCaptionId));
            }

            return captions;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetUnmappedCustomerGroupFSCaption(short fsCaptionGroupId, int customerGroupId, DateTime fsDate)
        {
            var dataList = (from data in context.TBL_CUSTOMER_GRP_FS_CAPTN_DET
                            where data.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID == fsCaptionGroupId && data.CUSTOMERGROUPID == customerGroupId
                            && data.FSDATE == fsDate && data.DELETED == false 
                            select data.FSCAPTIONID).ToList();

            var captions = (from a in context.TBL_CUSTOMER_FS_CAPTION
                            where a.FSCAPTIONGROUPID == fsCaptionGroupId && a.DELETED == false// && !dataList.Contains(data.ProductProductFeeId)
                            select new CustomerFSCaptionViewModel
                            {
                                fsCaptionId = a.FSCAPTIONID,
                                fsCaptionName = a.FSCAPTIONNAME,
                                fsCaptionGroupId = a.FSCAPTIONGROUPID,
                                fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                                isRatio = a.ISRATIO,
                                dateTimeCreated = a.DATETIMECREATED,
                                createdBy = a.CREATEDBY
                            });

            if (dataList.Any())
            {
                captions = captions.Where(x => !dataList.Contains(x.fsCaptionId));
            }

            return captions;
        }

        public bool UpdateCustomerFSCaption(int fsCaptionId, CustomerFSCaptionViewModel entity)
        {
            var data = this.context.TBL_CUSTOMER_FS_CAPTION.Find(fsCaptionId);
            if (data == null) return false;

            data.FSCAPTIONNAME = entity.fsCaptionName;
            data.FSCAPTIONGROUPID = entity.fsCaptionGroupId;
            data.ISRATIO = entity.isRatio;
            data.POSITION = entity.position;
            data.LASTUPDATEDBY = (int)entity.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var auditInfo = context.TBL_CUSTOMER_FS_CAPTION_GROUP.FirstOrDefault(x => x.FSCAPTIONGROUPID == data.FSCAPTIONGROUPID);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer FS Caption : { data.FSCAPTIONNAME }  in group ( {auditInfo.FSCAPTIONGROUPNAME })",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

    }
}
