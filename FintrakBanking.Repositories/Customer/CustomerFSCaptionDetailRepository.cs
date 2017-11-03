using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Customer
{
    public class CustomerFSCaptionDetailRepository : ICustomerFSCaptionDetailRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSCaptionDetailRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            context = _context;
            _genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        #region Customer FS Caption Detail
        public bool AddCustomerFSCaptionDetail(CustomerFSCaptionDetailViewModel entity)
        {
            //var targetEntity = context.tbl_Customer_FS_Caption_Detail.FirstOrDefault(x => x.FSCaptionId == entity.fsCaptionId);

            //if (targetEntity != null)
            //{
            //    targetEntity.Deleted = false;
            //    targetEntity.LastUpdatedBy = entity.createdBy;
            //    targetEntity.DateTimeUpdated = DateTime.Now;
            //}
            //else
            //{
            //}

            var data = new TBL_CUSTOMER_FS_CAPTION_DETAIL
            {
                CUSTOMERID = entity.customerId,
                FSCAPTIONID = entity.fsCaptionId,
                FSDATE = entity.fsDate,
                AMOUNT = entity.amount,

                CREATEDBY = entity.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Add(data);

            // Audit Section ---------------------------
            var captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
            var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONCODE})";
            var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
            var customer = $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailAdded,
                STAFFID = entity.createdBy,
                BRANCHID = entity.userBranchId,
                DETAIL = $"Added FS Caption Detail for customer {customer} and caption {caption} . Amount is { data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
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
            var data = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Find(fsdetailId);
            if (data != null)
            {
                data.DELETED = true;
                data.DELETEDBY = user.createdBy;
                data.DATETIMEDELETED = _genSetup.GetApplicationDate();

                // Audit Section ---------------------------

                var captionInfo =
                    context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
                var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONCODE})";
                var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
                var customer =
                    $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

                //var auditInfo = (from a in context.TblCustomerFsCaptionDetail
                //                 where a.FsdetailId == data.FsdetailId
                //                 select new
                //                 {
                //                     CustomerInfo = $"Cutomer with code: {a.tbl_Customer.CustomerCode} ({a.tbl_Customer.FirstName}  { a.tbl_Customer.LastName})",
                //                     CaptionInfo = $"{a.Fscaption.FscaptionName} ({a.Fscaption.FscaptionCode})"
                //                 }).FirstOrDefault();

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL =
                        $"Deleted FS Caption Detail for customer {customer} and caption {caption}. Amount is {data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                    IPADDRESS = user.userIPAddress,
                    URL = user.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
            }

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteMultipleCustomerFSCaptionDetail(List<int> fsdetailIds, UserInfo user)
        {
            if (fsdetailIds.Count <= 0)
                return false;

            foreach (int fsdetailId in fsdetailIds)
                DeleteCustomerFSCaptionDetail(fsdetailId, user);

            return true;
        }

        public IEnumerable<CustomerFSCaptionDetailViewModel> GetMappedCustomerFsCaptionDetail(int customerId, short fsCaptionGroupId, DateTime fsDate)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                        where a.CUSTOMERID == customerId && a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID == fsCaptionGroupId
                        && a.FSDATE == fsDate
                        && a.TBL_CUSTOMER_FS_CAPTION.ISTOTALLINE == false
                        && a.DELETED == false
                        orderby a.FSDATE, a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID,
                        a.TBL_CUSTOMER_FS_CAPTION.FSTYPEID, a.TBL_CUSTOMER_FS_CAPTION.POSITION
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CUSTOMERID,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            fsdetailId = a.FSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
                            accountCategoryName = a.TBL_CUSTOMER_FS_CAPTION.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            fsTypeName = a.TBL_CUSTOMER_FS_CAPTION.TBL_FINANCIAL_STATEMENT_TYPE.FSTYPENAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,

                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public CustomerFSCaptionDetailViewModel GetCustomerFSCaptionDetailById(int fsdetailId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                        where a.FSDETAILID == fsdetailId
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CUSTOMERID,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            fsdetailId = a.FSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
                            accountCategoryName = a.TBL_CUSTOMER_FS_CAPTION.TBL_ACCOUNT_CATEGORY.ACCOUNTCATEGORYNAME,
                            fsTypeName = a.TBL_CUSTOMER_FS_CAPTION.TBL_FINANCIAL_STATEMENT_TYPE.FSTYPENAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,

                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).FirstOrDefault();

            return data;
        }

        public bool UpdateCustomerFSCaptionDetail(int fsdetailId, CustomerFSCaptionDetailViewModel entity)
        {
            var data = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Find(fsdetailId);
            if (data == null) return false;

            data.CUSTOMERID = entity.customerId;
            data.FSCAPTIONID = entity.fsCaptionId;
            data.FSDATE = entity.fsDate;
            data.AMOUNT = entity.amount;

            data.LASTUPDATEDBY = entity.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
            var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONCODE})";
            var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
            var customer = $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = entity.userBranchId,
                DETAIL = $"Updated FS Caption Detail for customer {customer} and caption {caption} . Amount is { data?.AMOUNT.ToString("#,##0") } with date {data?.FSDATE.ToString("dd/MM/yyyy")}",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        #endregion Customer FS Caption Detail

        #region Customer Group FS Caption Detail 

        public IEnumerable<CustomerGroupFSCaptionDetailViewModel> GetMappedCustomerGroupFsCaptionDetail(int customerGroupId, short fsCaptionGroupId, DateTime fsDate)
        {
            throw new NotImplementedException();
        }

        public CustomerGroupFSCaptionDetailViewModel GetCustomerGroupFSCaptionDetailById(int fsdetailId)
        {
            throw new NotImplementedException();
        }

        public bool AddCustomerGroupFSCaptionDetail(CustomerGroupFSCaptionDetailViewModel entity)
        {
            throw new NotImplementedException();
        }

        public bool AddMultipleCustomerGroupFSCaptionDetail(List<CustomerGroupFSCaptionDetailViewModel> entities)
        {
            throw new NotImplementedException();
        }

        public bool UpdateCustomerGroupFSCaptionDetail(int fsdetailId, CustomerGroupFSCaptionDetailViewModel entity)
        {
            throw new NotImplementedException();
        }

        public bool DeleteCustomerGroupFSCaptionDetail(int fsdetailId, UserInfo user)
        {
            throw new NotImplementedException();
        }

        public bool DeleteMultipleCustomerGroupFSCaptionDetail(List<int> fsdetailIds, UserInfo user)
        {
            throw new NotImplementedException();
        }

        #endregion Customer Group FS Caption Detail
    }
}