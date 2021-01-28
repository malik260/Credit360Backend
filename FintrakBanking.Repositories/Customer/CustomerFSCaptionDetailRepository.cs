using FintrakBanking.Common;
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

            var exisitingDeletedRecord = context.TBL_CUSTOMER_FS_CAPTION_DETAIL.FirstOrDefault(x => x.FSCAPTIONID == entity.fsCaptionId
               && x.CUSTOMERID == entity.customerId && x.DELETED == true);

            if (exisitingDeletedRecord != null)
            {
                exisitingDeletedRecord.DELETED = false;
                exisitingDeletedRecord.AMOUNT = entity.amount;
                exisitingDeletedRecord.DATETIMEUPDATED = DateTime.Now;
                exisitingDeletedRecord.LASTUPDATEDBY = entity.createdBy;
                exisitingDeletedRecord.FSDATE = entity.fsDate;
                exisitingDeletedRecord.TEXTVALUE = entity.textValue;

                var captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == exisitingDeletedRecord.FSCAPTIONID);
                var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
                var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == exisitingDeletedRecord.CUSTOMERID);
                var customer = $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

                var auditRecord = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = entity.userBranchId,
                    DETAIL = $"Added FS Caption Detail for customer {customer} and caption {caption} . Amount is { exisitingDeletedRecord?.AMOUNT:#,##0} with date {exisitingDeletedRecord?.FSDATE:dd/MM/yyyy}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
            }
            else
            {
                var data = new TBL_CUSTOMER_FS_CAPTION_DETAIL
                {
                    CUSTOMERID = entity.customerId,
                    FSCAPTIONID = entity.fsCaptionId,
                    FSDATE = entity.fsDate,
                    AMOUNT = entity.amount,
                    DELETED = false,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = _genSetup.GetApplicationDate(),
                    TEXTVALUE = entity.textValue
                };

                if (entity.textValue != "" && entity.textValue != null) {
                    data.AMOUNT = 0;
                }

            context.TBL_CUSTOMER_FS_CAPTION_DETAIL.Add(data);

            // Audit Section ---------------------------
            var captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
            var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
            var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
            var customer = $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailAdded,
                STAFFID = entity.createdBy,
                BRANCHID = entity.userBranchId,
                DETAIL = $"Added FS Caption Detail for customer {customer} and caption {caption} . Amount is { data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
            }
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
                var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
                var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
                var customer =
                    $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deleted FS Caption Detail for customer {customer} and caption {caption}. Amount is {data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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
                        join b in context.TBL_CUSTOMER_FS_CAPTION on a.FSCAPTIONID equals b.FSCAPTIONID
                        join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where a.CUSTOMERID == customerId && b.FSCAPTIONGROUPID == fsCaptionGroupId
                        && a.FSDATE == fsDate && a.DELETED == false //&& b.ISRATIO == false 
                        orderby a.FSDATE, b.POSITION, b.FSCAPTIONGROUPID
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CUSTOMERID,
                            customerCode = c.CUSTOMERCODE,
                            fsdetailId = a.FSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = b.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                            fsCaptionPosition = b.POSITION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<CustomerFSCaptionDetailViewModel> GetAllMappedCustomerFsCaptionDetail(int customerId, DateTime fsDate)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                        join b in context.TBL_CUSTOMER_FS_CAPTION on a.FSCAPTIONID equals b.FSCAPTIONID
                        join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where a.CUSTOMERID == customerId //&& b.FSCAPTIONGROUPID == fsCaptionGroupId
                        && a.FSDATE == fsDate && a.DELETED == false //&& b.ISRATIO == false 
                        orderby a.FSDATE, b.POSITION, b.FSCAPTIONGROUPID
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CUSTOMERID,
                            customerCode = c.CUSTOMERCODE,
                            fsdetailId = a.FSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = b.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                            textValue = a.TEXTVALUE,
                            fsCaptionPosition = b.POSITION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<CustomerFSCaptionDetailViewModel> GetMappedCustomerFsCaptions(int customerId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                        where a.CUSTOMERID == customerId
                        && a.TBL_CUSTOMER_FS_CAPTION.ISRATIO == false
                        && a.DELETED == false
                        orderby a.FSDATE descending, a.TBL_CUSTOMER_FS_CAPTION.TBL_CUSTOMER_FS_CAPTION_GROUP.POSITION, a.TBL_CUSTOMER_FS_CAPTION.POSITION
                        select new CustomerFSCaptionDetailViewModel
                        {
                            customerId = a.CUSTOMERID,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            fsdetailId = a.FSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                            fsGroupName = a.TBL_CUSTOMER_FS_CAPTION.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME,
                            fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
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
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                           fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
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
            var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
            var customerInfo = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == data.CUSTOMERID);
            var customer = $"Cutomer with code: {customerInfo?.CUSTOMERCODE} ({customerInfo?.FIRSTNAME}  {customerInfo?.LASTNAME})";

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSCaptionDetailUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = entity.userBranchId,
                DETAIL = $"Updated FS Caption Detail for customer {customer} and caption {caption} . Amount is { data?.AMOUNT.ToString("#,##0") } with date {data?.FSDATE.ToString("dd/MM/yyyy")}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
            var data = (from a in context.TBL_CUSTOMER_GRP_FS_CAPTN_DET
                        where a.CUSTOMERGROUPID == customerGroupId && a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID == fsCaptionGroupId
                        && a.FSDATE == fsDate
                        && a.TBL_CUSTOMER_FS_CAPTION.ISRATIO == false
                        && a.DELETED == false
                        orderby a.FSDATE, a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID
                        select new CustomerGroupFSCaptionDetailViewModel
                        {
                            customerGroupId = a.CUSTOMERGROUPID,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            fsdetailId = a.GROUPFSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                            fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public IEnumerable<CustomerGroupFSCaptionDetailViewModel> GetMappedCustomerGroupFsCaptions(int customerGroupId)
        {
            var data = (from a in context.TBL_CUSTOMER_GRP_FS_CAPTN_DET
                        where a.CUSTOMERGROUPID == customerGroupId
                        && a.TBL_CUSTOMER_FS_CAPTION.ISRATIO == false
                        && a.DELETED == false
                        orderby a.FSDATE, a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID
                        select new CustomerGroupFSCaptionDetailViewModel
                        {
                            customerGroupId = a.CUSTOMERGROUPID,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            fsdetailId = a.GROUPFSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                            fsCaptionPosition = a.TBL_CUSTOMER_FS_CAPTION.POSITION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public CustomerGroupFSCaptionDetailViewModel GetCustomerGroupFSCaptionDetailById(int fsdetailId)
        {
            var data = (from a in context.TBL_CUSTOMER_GRP_FS_CAPTN_DET
                        where a.GROUPFSDETAILID == fsdetailId
                        select new CustomerGroupFSCaptionDetailViewModel
                        {
                            customerGroupId = a.CUSTOMERGROUPID,
                            customerGroupCode = a.TBL_CUSTOMER_GROUP.GROUPCODE,
                            fsdetailId = a.GROUPFSDETAILID,
                            fsCaptionId = a.FSCAPTIONID,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                            fsDate = a.FSDATE,
                            amount = a.AMOUNT,
                          fsCaptionPosition =  a.TBL_CUSTOMER_FS_CAPTION.POSITION,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).FirstOrDefault();

            return data;
        }

        public bool AddCustomerGroupFSCaptionDetail(CustomerGroupFSCaptionDetailViewModel entity)
        {
            try
            {
                var exisitingDeletedRecord = context.TBL_CUSTOMER_GRP_FS_CAPTN_DET.FirstOrDefault(x => x.FSCAPTIONID == entity.fsCaptionId
                && x.CUSTOMERGROUPID == entity.customerGroupId && x.DELETED == true);

                TBL_CUSTOMER_FS_CAPTION captionInfo;
                string caption;
                TBL_CUSTOMER_GROUP customerGrpInfo;
                string customerGrp;

                if (exisitingDeletedRecord != null)
                {
                    exisitingDeletedRecord.DELETED = false;
                    exisitingDeletedRecord.AMOUNT = entity.amount;
                    exisitingDeletedRecord.DATETIMEUPDATED = DateTime.Now;
                    exisitingDeletedRecord.LASTUPDATEDBY = entity.createdBy;
                    exisitingDeletedRecord.FSDATE = entity.fsDate;

                    captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == exisitingDeletedRecord.FSCAPTIONID);
                    caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
                    customerGrpInfo = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == exisitingDeletedRecord.CUSTOMERGROUPID);
                    customerGrp = $"Cutomer Group with code: {customerGrpInfo?.GROUPCODE}";

                    var auditRecord = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupFSCaptionDetailUpdated,
                        STAFFID = entity.createdBy,
                        BRANCHID = entity.userBranchId,
                        DETAIL = $"Added FS Caption Detail for customer group {customerGrp} and caption {caption} . Amount is { exisitingDeletedRecord?.AMOUNT:#,##0} with date {exisitingDeletedRecord?.FSDATE:dd/MM/yyyy}",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };
                }
                else
                {
                    var data = new TBL_CUSTOMER_GRP_FS_CAPTN_DET
                    {
                        CUSTOMERGROUPID = entity.customerGroupId,
                        FSCAPTIONID = entity.fsCaptionId,
                        FSDATE = entity.fsDate,
                        AMOUNT = entity.amount,

                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = _genSetup.GetApplicationDate()
                    };

                    context.TBL_CUSTOMER_GRP_FS_CAPTN_DET.Add(data);

                    // Audit Section ---------------------------

                    captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
                    caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
                    customerGrpInfo = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == data.CUSTOMERGROUPID);
                    customerGrp = $"Cutomer Group with code: {customerGrpInfo?.GROUPCODE}";

                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupFSCaptionDetailAdded,
                        STAFFID = entity.createdBy,
                        BRANCHID = entity.userBranchId,
                        DETAIL = $"Added FS Caption Detail for customer group {customerGrp} and caption {caption} . Amount is { data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = entity.applicationUrl,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName(),
                        APPLICATIONDATE = _genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now
                    };

                    auditTrail.AddAuditTrail(audit);
                    //end of Audit section -------------------------------

                }

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateCustomerGroupFSCaptionDetail(int fsdetailId, CustomerGroupFSCaptionDetailViewModel entity)
        {
            var data = context.TBL_CUSTOMER_GRP_FS_CAPTN_DET.Find(fsdetailId);
            if (data == null) return false;

            data.CUSTOMERGROUPID = entity.customerGroupId;
            data.FSCAPTIONID = entity.fsCaptionId;
            data.FSDATE = entity.fsDate;
            data.AMOUNT = entity.amount;

            data.LASTUPDATEDBY = entity.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var captionInfo = context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
            var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
            var customerGrpInfo = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == data.CUSTOMERGROUPID);
            var customerGrp = $"Cutomer Group with code: {customerGrpInfo?.GROUPCODE}";

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupFSCaptionDetailUpdated,
                STAFFID = entity.createdBy,
                BRANCHID = entity.userBranchId,
                DETAIL = $"Updated FS Caption Detail for customer group {customerGrp} and caption {caption} . Amount is { data?.AMOUNT.ToString("#,##0") } with date {data?.FSDATE.ToString("dd/MM/yyyy")}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteCustomerGroupFSCaptionDetail(int fsdetailId, UserInfo user)
        {
            var data = context.TBL_CUSTOMER_GRP_FS_CAPTN_DET.Find(fsdetailId);
            if (data != null)
            {
                data.DELETED = true;
                data.DELETEDBY = user.createdBy;
                data.DATETIMEDELETED = _genSetup.GetApplicationDate();

                // Audit Section ---------------------------

                var captionInfo =
                    context.TBL_CUSTOMER_FS_CAPTION.FirstOrDefault(x => x.FSCAPTIONID == data.FSCAPTIONID);
                var caption = $"{captionInfo?.FSCAPTIONNAME} ({captionInfo?.FSCAPTIONNAME})";
                var customerGrpInfo = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == data.CUSTOMERGROUPID);
                var customerGrp =
                    $"Cutomer Group with code: {customerGrpInfo?.GROUPCODE}";

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerGroupFSCaptionDetailDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deleted FS Caption Detail for customer {customerGrp} and caption {caption}. Amount is {data?.AMOUNT:#,##0} with date {data?.FSDATE:dd/MM/yyyy}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
            }

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddMultipleCustomerGroupFSCaptionDetail(List<CustomerGroupFSCaptionDetailViewModel> entities)
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