using FintrakBanking.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using System.Linq;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ICustomerFSCaptionDetailRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomerFSCaptionDetailRepository : ICustomerFSCaptionDetailRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSCaptionDetailRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }


        public bool AddCustomerFSCaptionDetail(CustomerFSCaptionDetailViewModel entity)
        {
            var data = new tbl_Customer_FS_Caption_Detail
            {
                CustomerId = entity.customerId,
                FSCaptionId = entity.fsCaptionId,
                FSDate = entity.fsDate,
                Amount = entity.amount,                            

                CreatedBy = (int)entity.createdBy,
                DateTimeCreated = _genSetup.GetApplicaionDate()
            };

            context.tbl_Customer_FS_Caption_Detail.Add(data);

            // Audit Section ---------------------------
            var captionInfo = context.tbl_Customer_FS_Caption.FirstOrDefault(x => x.FSCaptionId == data.FSCaptionId);
            var caption = $"{captionInfo.FSCaptionName} ({captionInfo.FSCaptionCode})";
            var customerInfo = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == data.CustomerId);
            var customer = $"Cutomer with code: {customerInfo.CustomerCode} ({customerInfo.FirstName}  {customerInfo.LastName})";            

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionDetailAdded,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added FS Caption Detail for customer {customer} and caption {caption} . Amount is { data.Amount.ToString("#,##0") } with date {data.FSDate.ToString("dd/MM/yyyy")}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        ///TODO: Implement a more efficient method
        public bool AddMultipleCustomerFSCaptionDetail(List<CustomerFSCaptionDetailViewModel> entities)
        {
            if (entities.Count <= 0)
                return false;

            foreach (CustomerFSCaptionDetailViewModel entity in entities)
                AddCustomerFSCaptionDetail(entity);

            return true;
        }

        public bool DeleteCustomerFSCaptionDetail(int fsdetailId, UserInfo user)
        {
            var data = context.tbl_Customer_FS_Caption_Detail.Find(fsdetailId);
            data.Deleted = true;
            data.DeletedBy = (int)user.createdBy;
            data.DateTimeDeleted = _genSetup.GetApplicaionDate();


            // Audit Section ---------------------------

            var captionInfo = context.tbl_Customer_FS_Caption.FirstOrDefault(x => x.FSCaptionId == data.FSCaptionId);
            var caption = $"{captionInfo.FSCaptionName} ({captionInfo.FSCaptionCode})";
            var customerInfo = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == data.CustomerId);
            var customer = $"Cutomer with code: {customerInfo.CustomerCode} ({customerInfo.FirstName}  {customerInfo.LastName})";

            //var auditInfo = (from a in context.TblCustomerFsCaptionDetail
            //                 where a.FsdetailId == data.FsdetailId
            //                 select new
            //                 {
            //                     CustomerInfo = $"Cutomer with code: {a.tbl_Customer.CustomerCode} ({a.tbl_Customer.FirstName}  { a.tbl_Customer.LastName})",
            //                     CaptionInfo = $"{a.Fscaption.FscaptionName} ({a.Fscaption.FscaptionCode})"
            //                 }).FirstOrDefault();

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionDetailDeleted,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted FS Caption Detail for customer {customer} and caption {caption}. Amount is { data.Amount.ToString("#,##0") } with date {data.FSDate.ToString("dd/MM/yyyy")}",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteMultileCustomerFSCaptionDetail(List<int> fsdetailIds, UserInfo user)
        {
            if (fsdetailIds.Count <= 0)
                return false;

            foreach (int fsdetailId in fsdetailIds)
                DeleteCustomerFSCaptionDetail(fsdetailId, user);

            return true;
        }

        public IEnumerable<CustomerFSCaptionDetailViewModel> GetCustomerFSCaptionDetail(int customerId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption_Detail
                        where a.CustomerId == customerId && a.tbl_Customer_FS_Caption.IsTotalLine == false && a.Deleted == false
                        orderby a.FSDate, a.tbl_Customer_FS_Caption.FSCaptionGroupId, a.tbl_Customer_FS_Caption.FSTypeId, a.tbl_Customer_FS_Caption.Position
                        select new CustomerFSCaptionDetailViewModel
                        {                                                        
                            customerId = a.CustomerId,
                            customerCode = a.tbl_Customer.CustomerCode,
                            fsdetailId = a.FSDetailId,
                            fsCaptionId = a.FSCaptionId,
                            fsCaptionName = a.tbl_Customer_FS_Caption.FSCaptionName,
                            fsCaptionPosition = a.tbl_Customer_FS_Caption.Position,
                            accountCategoryName = a.tbl_Customer_FS_Caption.tbl_Account_Category.AccountCategoryName,
                            fsTypeName = a.tbl_Customer_FS_Caption.tbl_Financial_Statement_Type.FSTypeName,
                            fsDate = a.FSDate,
                            amount = a.Amount,

                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).ToList();
            return data;
        }

        public CustomerFSCaptionDetailViewModel GetCustomerFSCaptionDetailById(int fsdetailId)
        {
            var data = (from a in context.tbl_Customer_FS_Caption_Detail
                        where a.FSDetailId == fsdetailId
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CustomerId,
                            customerCode = a.tbl_Customer.CustomerCode,
                            fsdetailId = a.FSDetailId,
                            fsCaptionId = a.FSCaptionId,
                            fsCaptionName = a.tbl_Customer_FS_Caption.FSCaptionName,
                            fsCaptionPosition = a.tbl_Customer_FS_Caption.Position,
                            accountCategoryName = a.tbl_Customer_FS_Caption.tbl_Account_Category.AccountCategoryName,
                            fsTypeName = a.tbl_Customer_FS_Caption.tbl_Financial_Statement_Type.FSTypeName,
                            fsDate = a.FSDate,
                            amount = a.Amount,

                            dateTimeCreated = a.DateTimeCreated,
                            createdBy = a.CreatedBy
                        }).FirstOrDefault();

            return data;
        }

        public bool UpdateCustomerFSCaptionDetail(int fsdetailId, CustomerFSCaptionDetailViewModel entity)
        {
            var data = this.context.tbl_Customer_FS_Caption_Detail.Find(fsdetailId);
            if (data == null) return false;

            data.CustomerId = entity.customerId;
            data.FSCaptionId = entity.fsCaptionId;
            data.FSDate = entity.fsDate;
            data.Amount = entity.amount;

            data.LastUpdatedBy = entity.createdBy;
            data.DateTimeUpdated = _genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var captionInfo = context.tbl_Customer_FS_Caption.FirstOrDefault(x => x.FSCaptionId == data.FSCaptionId);
            var caption = $"{captionInfo.FSCaptionName} ({captionInfo.FSCaptionCode})";
            var customerInfo = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == data.CustomerId);
            var customer = $"Cutomer with code: {customerInfo.CustomerCode} ({customerInfo.FirstName}  {customerInfo.LastName})";

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CustomerFSCaptionDetailUpdated,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated FS Caption Detail for customer {customer} and caption {caption} . Amount is { data.Amount.ToString("#,##0") } with date {data.FSDate.ToString("dd/MM/yyyy")}",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = _genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }               

        
    }
}
