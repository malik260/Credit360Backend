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
            var data = this.context.tbl_Customer_FS_Caption.Count(x => x.FSCaptionGroupId == fsCaptionGroupId);
            int counter = data + 1;
            var code = string.Format("{0}", counter.ToString().PadLeft(3, '0'));
            return code;
        }

        public bool AddCustomerFSCaption(CustomerFSCaptionViewModel entity)
        {
            var data = new tbl_Customer_FS_Caption
            {                
                FSCaptionCode = GenerateCaptionCode(entity.fsCaptionGroupId),
                FSCaptionName = entity.fsCaptionName,
                FSCaptionGroupId = entity.fsCaptionGroupId,
                ParentIdFSCaptionId = entity.parentIdFSCaptionId,
                AccountCategoryId = entity.accountCategoryId,
                FSTypeId = entity.fsTypeId,
                Position = entity.position,
                RefNote = entity.refNote,
                IsTotalLine = entity.isTotalLine,
                ReportColour = entity.reportColour,
                Multiplier = entity.multiplier,               

                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate()
            };

            context.tbl_Customer_FS_Caption.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added Customer FS Caption : { data.FSCaptionName } with code: {data.FSCaptionCode}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now,
                TargetId = data.FSCaptionId 
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteCustomerFSCaption(int fsCaptionId, UserInfo user)
        {
            var data = context.tbl_Customer_FS_Caption.Find(fsCaptionId);
            data.Deleted = true;
            data.DeletedBy = (int)user.createdBy;
            data.DateTimeDeleted = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var auditInfo = context.tbl_Customer_FS_Caption_Group.FirstOrDefault(x => x.FSCaptionGroupId == data.FSCaptionGroupId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted Customer FS Caption: { data.FSCaptionName } with Code: { data.FSCaptionCode } in group ( {auditInfo.FSCaptionGroupName })",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetCustomerFSCaption(short fsCaptionGroupId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption
                        where a.FSCaptionGroupId == fsCaptionGroupId && a.Deleted == false
                        orderby a.Position
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCaptionId,
                            fsCaptionCode = a.FSCaptionCode,
                            fsCaptionName = a.FSCaptionName,
                            fsCaptionGroupId = a.FSCaptionGroupId,
                            fsCaptionGroupName = a.tbl_Customer_FS_Caption_Group.FSCaptionGroupName,
                            parentIdFSCaptionId = a.ParentIdFSCaptionId,
                            parentIdFSCaptionName =  a.tbl_Customer_FS_Caption2 != null ? a.tbl_Customer_FS_Caption2.FSCaptionName : "",
                            accountCategoryId = a.AccountCategoryId,
                            accountCategoryName = a.tbl_Account_Category.AccountCategoryName,
                            fsTypeId = a.FSTypeId,
                            fsTypeName = a.tbl_Financial_Statement_Type.FSTypeName,
                            position = a.Position,
                            refNote = a.RefNote,
                            isTotalLine = a.IsTotalLine,
                            reportColour = a.ReportColour,
                            multiplier = a.Multiplier,                            

                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public CustomerFSCaptionViewModel GetCustomerFSCaptionById(int fsCaptionId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption
                        where a.FSCaptionId == fsCaptionId
                        select new CustomerFSCaptionViewModel
                        {
                            fsCaptionId = a.FSCaptionId,
                            fsCaptionCode = a.FSCaptionCode,
                            fsCaptionName = a.FSCaptionName,
                            fsCaptionGroupId = a.FSCaptionGroupId,
                            fsCaptionGroupName = a.tbl_Customer_FS_Caption_Group.FSCaptionGroupName,
                            parentIdFSCaptionId = a.ParentIdFSCaptionId,
                            parentIdFSCaptionName = a.tbl_Customer_FS_Caption2 != null ? a.tbl_Customer_FS_Caption2.FSCaptionName : "",
                            accountCategoryId = a.AccountCategoryId,
                            accountCategoryName = a.tbl_Account_Category.AccountCategoryName,
                            fsTypeId = a.FSTypeId,
                            fsTypeName = a.tbl_Financial_Statement_Type.FSTypeName,
                            position = a.Position,
                            refNote = a.RefNote,
                            isTotalLine = a.IsTotalLine,
                            reportColour = a.ReportColour,
                            multiplier = a.Multiplier,

                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy, 
                        }).FirstOrDefault();
            return data;
        }

        public IEnumerable<CustomerFSCaptionViewModel> GetUnmappedCustomerFSCaption(short fsCaptionGroupId, int customerId, DateTime fsDate)
        {
            var dataList = (from data in context.tbl_Customer_FS_Caption_Detail
                            where data.tbl_Customer_FS_Caption.FSCaptionGroupId == fsCaptionGroupId && data.CustomerId == customerId 
                            && data.FSDate == fsDate  && data.Deleted == false 
                            select data.FSCaptionId).ToList();

            var captions = (from a in context.tbl_Customer_FS_Caption
                       where a.FSCaptionGroupId == fsCaptionGroupId && a.IsTotalLine == false && a.Deleted == false // && !dataList.Contains(data.ProductProductFeeId)
                       orderby a.FSTypeId, a.Position
                       select new CustomerFSCaptionViewModel
                       {
                           fsCaptionId = a.FSCaptionId,
                           fsCaptionCode = a.FSCaptionCode,
                           fsCaptionName = a.FSCaptionName,
                           fsCaptionGroupId = a.FSCaptionGroupId,
                           fsCaptionGroupName = a.tbl_Customer_FS_Caption_Group.FSCaptionGroupName,
                           parentIdFSCaptionId = a.ParentIdFSCaptionId,
                           parentIdFSCaptionName = a.tbl_Customer_FS_Caption2 != null ? a.tbl_Customer_FS_Caption2.FSCaptionName : "",
                           accountCategoryId = a.AccountCategoryId,
                           accountCategoryName = a.tbl_Account_Category.AccountCategoryName,
                           fsTypeId = a.FSTypeId,
                           fsTypeName = a.tbl_Financial_Statement_Type.FSTypeName,
                           position = a.Position,
                           refNote = a.RefNote,
                           isTotalLine = a.IsTotalLine,
                           reportColour = a.ReportColour,
                           multiplier = a.Multiplier,

                           dateTimeCreated = a.DateTimeCreated,
                           createdBy = a.CreatedBy
                       });

            if (dataList.Any())
            {
                captions = captions.Where(x => !dataList.Contains(x.fsCaptionId));
            }

            return captions;
        }

        public bool UpdateCustomerFSCaption(int fsCaptionId, CustomerFSCaptionViewModel entity)
        {
            var data = this.context.tbl_Customer_FS_Caption.Find(fsCaptionId);
            if (data == null) return false;

            data.FSCaptionName = entity.fsCaptionName;
            data.FSCaptionGroupId = entity.fsCaptionGroupId;
            data.ParentIdFSCaptionId = entity.parentIdFSCaptionId;
            data.AccountCategoryId = entity.accountCategoryId;
            data.FSTypeId = entity.fsTypeId;
            data.Position = entity.position;
            data.RefNote = entity.refNote;
            data.IsTotalLine = entity.isTotalLine;
            data.ReportColour = entity.reportColour;
            data.Multiplier = entity.multiplier;

            data.LastUpdatedBy = (int)entity.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated Customer FS Caption : { data.FSCaptionName } with code: {data.FSCaptionCode}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
    }
}
