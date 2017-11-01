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
                FSCAPTIONCODE = GenerateCaptionCode(entity.fsCaptionGroupId),
                FSCAPTIONNAME = entity.fsCaptionName,
                FSCAPTIONGROUPID = entity.fsCaptionGroupId,
                PARENTIDFSCAPTIONID = entity.parentIdFSCaptionId,
                ACCOUNTCATEGORYID = entity.accountCategoryId,
                FSTYPEID = entity.fsTypeId,
                POSITION = entity.position,
                REFNOTE = entity.refNote,
                ISTOTALLINE = entity.isTotalLine,
                REPORTCOLOUR = entity.reportColour,
                MULTIPLIER = entity.multiplier,               

                CREATEDBY = (int)entity.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            context.TBL_CUSTOMER_FS_CAPTION.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added Customer FS Caption : { data.FSCAPTIONNAME } with code: {data.FSCAPTIONCODE}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = data.FSCAPTIONID 
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
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
                DETAIL = $"Deleted Customer FS Caption: { data.FSCAPTIONNAME } with Code: { data.FSCAPTIONCODE } in group ( {auditInfo.FSCAPTIONGROUPNAME })",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetCustomerFSCaption(short fsCaptionGroupId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where a.FSCAPTIONGROUPID == fsCaptionGroupId && a.DELETED == false
                        orderby a.POSITION
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionCode = a.FSCAPTIONCODE,
                            fsCaptionName = a.FSCAPTIONNAME,
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            parentIdFSCaptionId = a.PARENTIDFSCAPTIONID,
                            parentIdFSCaptionName =  a.TBL_CUSTOMER_FS_CAPTION2 != null ? a.TBL_CUSTOMER_FS_CAPTION2.FSCAPTIONNAME : "",
                            accountCategoryId = a.ACCOUNTCATEGORYID,
                            accountCategoryName = a.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            fsTypeId = a.FSTYPEID,
                            fsTypeName = a.TBL_FINANCIAL_STATEMENT_TYPE.FSTYPENAME,
                            position = a.POSITION,
                            refNote = a.REFNOTE,
                            isTotalLine = a.ISTOTALLINE,
                            reportColour = a.REPORTCOLOUR,
                            multiplier = a.MULTIPLIER,                            

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
                            fsCaptionCode = a.FSCAPTIONCODE,
                            fsCaptionName = a.FSCAPTIONNAME,
                            fsCaptionGroupId = a.FSCAPTIONGROUPID,
                            fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            parentIdFSCaptionId = a.PARENTIDFSCAPTIONID,
                            parentIdFSCaptionName = a.TBL_CUSTOMER_FS_CAPTION2 != null ? a.TBL_CUSTOMER_FS_CAPTION2.FSCAPTIONNAME : "",
                            accountCategoryId = a.ACCOUNTCATEGORYID,
                            accountCategoryName = a.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            fsTypeId = a.FSTYPEID,
                            fsTypeName = a.TBL_FINANCIAL_STATEMENT_TYPE.FSTYPENAME,
                            position = a.POSITION,
                            refNote = a.REFNOTE,
                            isTotalLine = a.ISTOTALLINE,
                            reportColour = a.REPORTCOLOUR,
                            multiplier = a.MULTIPLIER,

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
                       where a.FSCAPTIONGROUPID == fsCaptionGroupId && a.ISTOTALLINE == false && a.DELETED == false // && !dataList.Contains(data.ProductProductFeeId)
                       orderby a.FSTYPEID, a.POSITION
                       select new CustomerFSCaptionViewModel
                       {
                           fsCaptionId = a.FSCAPTIONID,
                           fsCaptionCode = a.FSCAPTIONCODE,
                           fsCaptionName = a.FSCAPTIONNAME,
                           fsCaptionGroupId = a.FSCAPTIONGROUPID,
                           fsCaptionGroupName = a.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                           parentIdFSCaptionId = a.PARENTIDFSCAPTIONID,
                           parentIdFSCaptionName = a.TBL_CUSTOMER_FS_CAPTION2 != null ? a.TBL_CUSTOMER_FS_CAPTION2.FSCAPTIONNAME : "",
                           accountCategoryId = a.ACCOUNTCATEGORYID,
                           accountCategoryName = a.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                           fsTypeId = a.FSTYPEID,
                           fsTypeName = a.TBL_FINANCIAL_STATEMENT_TYPE.FSTYPENAME,
                           position = a.POSITION,
                           refNote = a.REFNOTE,
                           isTotalLine = a.ISTOTALLINE,
                           reportColour = a.REPORTCOLOUR,
                           multiplier = a.MULTIPLIER,

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
            data.PARENTIDFSCAPTIONID = entity.parentIdFSCaptionId;
            data.ACCOUNTCATEGORYID = entity.accountCategoryId;
            data.FSTYPEID = entity.fsTypeId;
            data.POSITION = entity.position;
            data.REFNOTE = entity.refNote;
            data.ISTOTALLINE = entity.isTotalLine;
            data.REPORTCOLOUR = entity.reportColour;
            data.MULTIPLIER = entity.multiplier;

            data.LASTUPDATEDBY = (int)entity.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated Customer FS Caption : { data.FSCAPTIONNAME } with code: {data.FSCAPTIONCODE}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
    }
}
