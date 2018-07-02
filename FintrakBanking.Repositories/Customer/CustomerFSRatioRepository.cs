using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace FintrakBanking.Repositories.Customer
{

    public class CustomerFSRatioRepository : ICustomerFSRatioRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;

        public CustomerFSRatioRepository(FinTrakBankingContext _context,
            IGeneralSetupRepository genSetup, IAuditTrailRepository _auditTrail)
        {
            context = _context;
            _genSetup = genSetup;
            auditTrail = _auditTrail;
        }

        #region tbl_Customer FS Ratio Caption

        public IEnumerable<CustomerFSRatioCaptionViewModel> GetFSRatioCaption(int companyId)
        {


            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where a.ISRATIO == true && a.DELETED == false
                        select new CustomerFSRatioCaptionViewModel
                        {
                            ratioCaptionId = (short)a.FSCAPTIONID,
                            ratioCaptionName = a.FSCAPTIONNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public List<CustomerFSRatioCaptionViewModel> GetFSRatioCaptionById(short ratioCaptionId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                            //where a.RATIOCAPTIONID == ratioCaptionId // && a.Deleted == false
                            //orderby a.POSITION
                        select new CustomerFSRatioCaptionViewModel
                        {
                            //ratioCaptionId = a.RATIOCAPTIONID,
                            //ratioCaptionName = a.RATIOCAPTION,
                            //companyId = a.COMPANYID,
                            //companyName = a.TBL_COMPANY.NAME,
                            //annualised = a.ANNUALISED,
                            //position = a.POSITION,

                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public bool UpdateFSRatioCaption(short ratioCaptionId, CustomerFSRatioCaptionViewModel model)
        {
            //var data = context.TBL_CUSTOMER_FS_RATIO_CAPTION.Find(ratioCaptionId);
            //if (data == null) return false;

            //data.ANNUALISED = model.annualised;
            //data.COMPANYID = model.companyId;
            //data.RATIOCAPTION = model.ratioCaptionName;
            //data.POSITION = model.position;

            //data.LASTUPDATEDBY = (int)model.createdBy;
            //data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioCaptionUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                //DETAIL = $"Updated FS Ratio Caption : { data.RATIOCAPTION } with postion: {data.POSITION}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }

        public bool AddFSRatioCaption(CustomerFSRatioCaptionViewModel model)
        {
            //var data = new TBL_CUSTOMER_FS_RATIO_CAPTION
            //{
            //    ANNUALISED = model.annualised,
            //    COMPANYID = model.companyId,
            //    POSITION = model.position,
            //    RATIOCAPTION = model.ratioCaptionName,
            //    CREATEDBY = (int)model.createdBy,
            //    DATETIMECREATED = _genSetup.GetApplicationDate()
            //};

            ////context.TBL_CUSTOMER_FS_RATIO_CAPTION.Add(data);

            //// Audit Section ---------------------------
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioCaptionAdded,
            //    STAFFID = model.createdBy,
            //    BRANCHID = (short)model.userBranchId,
            //    DETAIL = $"Added FS Ratio Caption {data.RATIOCAPTION} and postion {data.POSITION}.",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = _genSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};

            //auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteFSRatioCaption(short ratioCaptionId, UserInfo user)
        {
            //var data = context.TBL_CUSTOMER_FS_RATIO_CAPTION.Find(ratioCaptionId);
            //data.DELETED = true;
            //data.DELETEDBY = (int)user.staffId;
            //data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioCaptionDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                //DETAIL = $"Deleted FS Ratio Caption: { data.RATIOCAPTION }. ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return context.SaveChanges() != 0;
        }
        #endregion

        #region tbl_Customer FS-Ratio-Detail
        public bool AddFSRatioDetail(CustomerFSRatioDetailViewModel model)
        {
            if (model != null)
            {
                var data = new TBL_CUSTOMER_FS_RATIO_DETAIL
                {
                    RATIOCAPTIONID = model.ratioCaptionId,
                    DIVISORTYPEID = (short)model.divisorTypeId,
                    FSCAPTIONID = (int)model.fscaptionId,
                    MULTIPLIER = (double)model.multiplier,
                    VALUETYPEID = (short)model.valueTypeId,
                    DESCRIPTION = context.TBL_CUSTOMER_FS_RATIO_VALUETYP.FirstOrDefault(x => x.VALUETYPEID == model.valueTypeId)?.VALUETYPENAME,
                    CREATEDBY = (int)model.createdBy,
                    DATETIMECREATED = _genSetup.GetApplicationDate()
                };

                context.TBL_CUSTOMER_FS_RATIO_DETAIL.Add(data);

                // Audit Section ---------------------------
                var auditDivisor = context.TBL_CUSTOMER_FS_RATIO_DIVI_TYP.FirstOrDefault(x => x.DIVISORTYPEID == data.DIVISORTYPEID)?.DIVISORTYPENAME;
                var auditValue = context.TBL_CUSTOMER_FS_RATIO_VALUETYP.FirstOrDefault(x => x.VALUETYPEID == data.VALUETYPEID)?.VALUETYPENAME;

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioDetailAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added FS Ratio Detail {data.TBL_CUSTOMER_FS_CAPTION} with divisor type '{auditDivisor}' and value typ '{auditValue }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                auditTrail.AddAuditTrail(audit);
            }

            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }

        public bool AddMultipleFSRatioDetail(List<CustomerFSRatioDetailViewModel> model)
        {
            if (model.Count <= 0)
                return false;

            foreach (CustomerFSRatioDetailViewModel entity in model)
                AddFSRatioDetail(entity);

            return true;
        }

        public IEnumerable<CustomerFSRatioDetailViewModel> GetFSRatioDetail(short ratioCaptionId, short fsCaptionGroupId, int companyId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_RATIO_DETAIL
                        join b in context.TBL_CUSTOMER_FS_CAPTION on a.RATIOCAPTIONID equals b.FSCAPTIONID
                        where a.RATIOCAPTIONID == ratioCaptionId && a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONGROUPID == fsCaptionGroupId && a.DELETED == false
                        select new CustomerFSRatioDetailViewModel

                        {
                            ratioDetailId = a.RATIODETAILID,
                            ratioCaptionId = (short)a.RATIOCAPTIONID,
                            divisorTypeId = a.DIVISORTYPEID,
                            valueTypeId = a.VALUETYPEID,
                            multiplier = a.MULTIPLIER,
                            fscaptionId = a.FSCAPTIONID,
                            ratioCaptionName = b.FSCAPTIONNAME,
                            fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION1.FSCAPTIONNAME,
                            valueTypeName = a.TBL_CUSTOMER_FS_RATIO_VALUETYP.VALUETYPENAME,
                            divisorTypeName = a.TBL_CUSTOMER_FS_RATIO_DIVI_TYP.DIVISORTYPENAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            createdBy = a.CREATEDBY
                        }).ToList();
            return data;
        }

        public CustomerFSRatioDetailViewModel GetFSRatioDetailById(int ratioDetailId)
        {
            var data = from a in context.TBL_CUSTOMER_FS_RATIO_DETAIL
                       join b in context.TBL_CUSTOMER_FS_CAPTION on a.RATIOCAPTIONID equals b.FSCAPTIONID
                       where a.RATIODETAILID == ratioDetailId && a.DELETED == false
                       select new CustomerFSRatioDetailViewModel
                       {
                           ratioDetailId = a.RATIODETAILID,
                           ratioCaptionId = (short)a.RATIOCAPTIONID,
                           divisorTypeId = a.DIVISORTYPEID,
                           valueTypeId = a.VALUETYPEID,
                           multiplier = a.MULTIPLIER,
                           fscaptionId = a.FSCAPTIONID,
                           ratioCaptionName = b.FSCAPTIONNAME,
                           fsCaptionName = a.TBL_CUSTOMER_FS_CAPTION.FSCAPTIONNAME,
                           valueTypeName = a.TBL_CUSTOMER_FS_RATIO_VALUETYP.VALUETYPENAME,
                           divisorTypeName = a.TBL_CUSTOMER_FS_RATIO_DIVI_TYP.DIVISORTYPENAME,
                           dateTimeCreated = a.DATETIMECREATED,
                           createdBy = a.CREATEDBY
                       };

            return data.SingleOrDefault();
        }

        public List<CustomerFSRatioCaptionReportViewModel> GetCustomerFSRatioValues(int customerId)
        {

            var customerFSDates = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                                   where a.CUSTOMERID == customerId
                                   orderby a.FSDATE descending
                                   select a.FSDATE).Distinct();

            //var customerFSCaptionIds = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
            //                       where a.CUSTOMERID == customerId                                   
            //                       select a.FSCAPTIONID);

            var lastFourDates = customerFSDates.Take(4).ToList();

            int count = lastFourDates.Count;

            var ratioCaptions = from a in context.TBL_CUSTOMER_FS_CAPTION
                                join b in context.TBL_CUSTOMER_FS_CAPTION_GROUP on a.FSCAPTIONGROUPID equals b.FSCAPTIONGROUPID
                                //where customerFSCaptionIds.Contains(a.FSCAPTIONID) 
                                orderby b.POSITION, a.POSITION 
                                select a;

            List<CustomerFSRatioCaptionReportViewModel> output = new List<CustomerFSRatioCaptionReportViewModel>();
            foreach (var item in ratioCaptions)
            {
                CustomerFSRatioCaptionReportViewModel value = new CustomerFSRatioCaptionReportViewModel();

                value.ratioCaptionId = (short)item.FSCAPTIONID;
                value.ratioCaptionName = item.FSCAPTIONNAME;
                value.fsGroupCaption = item.TBL_CUSTOMER_FS_CAPTION_GROUP.FSCAPTIONGROUPNAME;
                value.fsDate1 = count >= 4 ? lastFourDates[count - 4] : new DateTime(1900, 1, 1);
                value.fsDate2 = count >= 3 ? lastFourDates[count - 3] : new DateTime(1900, 1, 1);
                value.fsDate3 = count >= 2 ? lastFourDates[count - 2] : new DateTime(1900, 1, 1);
                value.fsDate4 = count >= 1 ? lastFourDates[count - 1] : new DateTime(1900, 1, 1);
                value.ratioValue1 = count >= 4 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 4]) : "0.00";
                value.ratioValue2 = count >= 3 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 3]) : "0.00";
                value.ratioValue3 = count >= 2 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 2]) : "0.00";
                value.ratioValue4 = count >= 1 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 1]) : "0.00";

                output.Add(value);
            }

            return output;

        }


        private string GetCustomerFSRatio(bool isRatio, int customerId, short fsCaptionId, DateTime fsDate)
        {
            if (isRatio == false)
            {
                var fsAmount = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                                where a.CUSTOMERID == customerId && a.FSDATE == fsDate && a.FSCAPTIONID == fsCaptionId
                                select a).FirstOrDefault();

                if (fsAmount != null)

                    return string.Format("{0:n}", fsAmount.AMOUNT);
                else
                    return "0.00";
            }

            var customerFS = from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                             where a.CUSTOMERID == customerId && a.FSDATE == fsDate
                             select a;

            var ratios = from a in context.TBL_CUSTOMER_FS_RATIO_DETAIL
                         where a.RATIOCAPTIONID == fsCaptionId
                         select a;

            var numeratorInfo = from a in ratios
                                join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                                where a.DIVISORTYPEID == 1
                                select new { a.MULTIPLIER, b.AMOUNT };

            double numerator = 0;
            if (numeratorInfo.Count() > 0)
                numerator = (from a in numeratorInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();

            var demoninatorInfo = from a in ratios
                                  join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                                  where a.DIVISORTYPEID == 2
                                  select new { a.MULTIPLIER, b.AMOUNT };

            double demoninator = 0;
            if (demoninatorInfo.Count() > 0)
                demoninator = (from a in demoninatorInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();

            if (demoninator == 0)
                return string.Format("{0:n}", numerator);
            else
                return string.Format("{0:0.00}", (decimal)numerator / (decimal)demoninator);
        }


        public bool UpdateFSRatioDetail(int ratioDetailId, CustomerFSRatioDetailViewModel model)
        {
            var data = context.TBL_CUSTOMER_FS_RATIO_DETAIL.Find(ratioDetailId);
            if (data == null) return false;

            data.RATIOCAPTIONID = model.ratioCaptionId;
            data.MULTIPLIER = (double)model.multiplier;
            data.VALUETYPEID = (short)model.valueTypeId;
            data.DIVISORTYPEID = (short)model.divisorTypeId;
            data.FSCAPTIONID = (int)model.fscaptionId;
            data.LASTUPDATEDBY = model.createdBy;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();

            // Audit Section ---------------------------
            var audit_divisor = (context.TBL_CUSTOMER_FS_RATIO_DIVI_TYP.FirstOrDefault(x => x.DIVISORTYPEID == data.DIVISORTYPEID)).DIVISORTYPENAME;
            var audit_value = (context.TBL_CUSTOMER_FS_RATIO_VALUETYP.FirstOrDefault(x => x.VALUETYPEID == data.VALUETYPEID)).VALUETYPENAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioDetailUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated FS Ratio Detail {data.TBL_CUSTOMER_FS_CAPTION} with divisor type '{audit_divisor}' and value typ '{audit_value }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;

        }

        public bool DeleteFSRatioDetail(int ratioDetailId, UserInfo user)
        {
            var data = context.TBL_CUSTOMER_FS_RATIO_DETAIL.Find(ratioDetailId);
            data.DELETED = true;
            data.DELETEDBY = (int)user.staffId;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();


            // Audit Section ---------------------------
            var audit_divisor = (context.TBL_CUSTOMER_FS_RATIO_DIVI_TYP.FirstOrDefault(x => x.DIVISORTYPEID == data.DIVISORTYPEID)).DIVISORTYPENAME;
            var audit_value = (context.TBL_CUSTOMER_FS_RATIO_VALUETYP.FirstOrDefault(x => x.VALUETYPEID == data.VALUETYPEID)).VALUETYPENAME;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerFSRatioDetailDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted FS Ratio Detail {data.TBL_CUSTOMER_FS_CAPTION} with divisor type '{audit_divisor}' and value type '{audit_value }' ",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return context.SaveChanges() != 0;
        }

        public bool DeleteMultipleFSRatioDetail(List<int> ratioDetailId, UserInfo user)
        {
            if (ratioDetailId.Count <= 0)
                return false;

            foreach (int fsdetailId in ratioDetailId)
                DeleteFSRatioDetail(fsdetailId, user);

            return true;
        }
        #endregion

        #region tbl_Customer FS Ratio Divisor Type
        public IEnumerable<CustomerFSRatioDivisorTypeViewModel> GetAllDivisorType()
        {
            var data = (from a in context.TBL_CUSTOMER_FS_RATIO_DIVI_TYP
                        where a.DELETED == false
                        select new CustomerFSRatioDivisorTypeViewModel
                        {
                            divisorTypeId = a.DIVISORTYPEID,
                            divisorTypeName = a.DIVISORTYPENAME,
                            //createdBy = a.CreatedBy.Value,
                            dateTimeCreated = a.DATETIMECREATED
                        }).ToList();
            return data;
        }
        #endregion

        #region tbl_Customer FS Ration Value Type
        public IEnumerable<CustomerFSRatioValueTypeViewModel> GetAllValueType()
        {
            var data = (from a in context.TBL_CUSTOMER_FS_RATIO_VALUETYP
                        where a.DELETED == false
                        select new CustomerFSRatioValueTypeViewModel
                        {
                            valueTypeId = a.VALUETYPEID,
                            valueTypeName = a.VALUETYPENAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            //createdBy = (int)a.CreatedBy
                        }).ToList();
            return data;
        }
        #endregion
    }
}
