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
        private ICustomerFSCaptionDetailRepository _fsCaptionDetailRepo;

        public CustomerFSRatioRepository(FinTrakBankingContext _context,
            IGeneralSetupRepository genSetup, IAuditTrailRepository _auditTrail, ICustomerFSCaptionDetailRepository fsCaptionDetailRepo)
        {
            context = _context;
            _genSetup = genSetup;
            auditTrail = _auditTrail;
            _fsCaptionDetailRepo = fsCaptionDetailRepo;
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

        public IEnumerable<CustomerFSRatioCaptionViewModel> GetFSRatioCaptionByFSCaptionGroupId(int companyId, int fSCaptionGroupId)
        {
            var data = (from a in context.TBL_CUSTOMER_FS_CAPTION
                        where a.ISRATIO == true && a.DELETED == false && a.FSCAPTIONGROUPID == fSCaptionGroupId
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL =model.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
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

            var lastFourDates = customerFSDates.OrderBy(x => x).Take(4).ToList();

            int count = lastFourDates.Count;

            var ratioCaptions = (from a in context.TBL_CUSTOMER_FS_CAPTION
                                join b in context.TBL_CUSTOMER_FS_CAPTION_GROUP on a.FSCAPTIONGROUPID equals b.FSCAPTIONGROUPID
                                //where customerFSCaptionIds.Contains(a.FSCAPTIONID) 
                                orderby b.POSITION, a.POSITION 
                                select a).ToList();

            List<CustomerFSRatioCaptionReportViewModel> output = new List<CustomerFSRatioCaptionReportViewModel>();
            foreach (var item in ratioCaptions)
            {
                CustomerFSRatioCaptionReportViewModel value = new CustomerFSRatioCaptionReportViewModel();

                value.ratioCaptionId = (short)item.FSCAPTIONID;
                value.ratioCaptionName = item.FSCAPTIONNAME;
                value.fsGroupCaption = item.TBL_CUSTOMER_FS_CAPTION_GROUP?.FSCAPTIONGROUPNAME;
                value.fsDate1 = count >= 4 ? lastFourDates[count - 4] : new DateTime(1900, 1, 1);
                value.fsDate2 = count >= 3 ? lastFourDates[count - 3] : new DateTime(1900, 1, 1);
                value.fsDate3 = count >= 2 ? lastFourDates[count - 2] : new DateTime(1900, 1, 1);
                value.fsDate4 = count >= 1 ? lastFourDates[count - 1] : new DateTime(1900, 1, 1);
                value.ratioValue1 = count >= 4 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 4]) : "0.00";
                value.ratioValue2 = count >= 3 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 3]) : "0.00";
                value.ratioValue3 = count >= 2 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 2]) : "0.00";
                value.ratioValue4 = count >= 1 ? GetCustomerFSRatio(item.ISRATIO, customerId, (short)item.FSCAPTIONID, lastFourDates[count - 1]) : "0.00";

                ////if (Convert.ToDecimal(value.ratioValue1) > 0 || Convert.ToDecimal(value.ratioValue2) > 0 ||
                ////    Convert.ToDecimal(value.ratioValue3) > 0 || Convert.ToDecimal(value.ratioValue4) > 0)
                ////{

                //if (!string.IsNullOrEmpty(value.ratioValue1) && (value.ratioValue1 != "0.00") || !string.IsNullOrEmpty(value.ratioValue2) && (value.ratioValue2 != "0.00") ||
                //    !string.IsNullOrEmpty(value.ratioValue3) && (value.ratioValue3 != "0.00") || !string.IsNullOrEmpty(value.ratioValue4) && (value.ratioValue4 != "0.00"))
                //{
                //    output.Add(value);
                //}

                if (!string.IsNullOrEmpty(value.ratioValue1) || !string.IsNullOrEmpty(value.ratioValue2) ||
                    !string.IsNullOrEmpty(value.ratioValue3) || !string.IsNullOrEmpty(value.ratioValue4))
                {
                    output.Add(value);
                }
            }

            return output;

        }

        private string GetCustomerFSRatio(bool isRatio, int customerId, short fsCaptionId, DateTime fsDate)
        {
            // it is not ratio (not derived)
            if (isRatio == false)
            {
                var fsAmount = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                                where a.CUSTOMERID == customerId && a.FSDATE == fsDate && a.FSCAPTIONID == fsCaptionId
                                select a).FirstOrDefault();

                if (fsAmount != null)
                {
                    if (fsAmount.AMOUNT != 0)
                        return string.Format("{0:n}", fsAmount.AMOUNT);
                    else if (!string.IsNullOrEmpty(fsAmount.TEXTVALUE) && !string.IsNullOrWhiteSpace(fsAmount.TEXTVALUE))
                        return fsAmount.TEXTVALUE;
                }
            }

            // it is ratio (derived)
            if (isRatio)
            {
                //var detail = (from c in context.TBL_CUSTOMER_FS_RATIO_DETAIL
                //               join f in context.TBL_CUSTOMER_FS_CAPTION on c.FSCAPTIONID equals f.FSCAPTIONID
                //               join t in context.TBL_CUSTOMER_FS_CAPTION_DETAIL on c.FSCAPTIONID equals t.FSCAPTIONID
                //               where c.RATIOCAPTIONID == fsCaptionId && t.FSDATE == fsDate
                //               select new { c.FSCAPTIONID, c.DIVISORTYPEID, f.ISRATIO, t.AMOUNT }).ToList().OrderBy(O => O.DIVISORTYPEID).FirstOrDefault();

                var detail = (from c in context.TBL_CUSTOMER_FS_CAPTION
                              join d in context.TBL_CUSTOMER_FS_CAPTION_DETAIL on c.FSCAPTIONID equals d.FSCAPTIONID
                              where c.FSCAPTIONID == fsCaptionId && d.FSDATE == fsDate && d.DELETED == false
                              select new { c.FSCAPTIONID, c.FSCAPTIONNAME, c.ISRATIO, d.AMOUNT }).FirstOrDefault();

                if (detail != null)
                {
                    decimal sum = 0, newSum = 0;
                    
                    // Indicative Decisions
                    if (detail.FSCAPTIONNAME.ToLower().Contains("indicative decision"))
                    {
                        if ((double)detail.AMOUNT <= 1.5)
                        {
                            return "ACCEPT";
                        }
                        else
                        {
                            return "DECLINED";
                        }
                    }

                    return string.Format("{0:n}", detail.AMOUNT);
                }

                return string.Format("{0:n}", CalculateFSRatioValue(customerId, fsCaptionId, fsDate));
            }

            // neither ratio nor non ratio
            return string.Format("{0:n}", 0);
        }

        public decimal CalculateFSRatioValueForDerived(CustomerFSCaptionDetailViewModel entity)
        {
            //var sum = 0;
            var sum = CalculateFSRatioValue(entity.customerId, (short) entity.fsCaptionId, entity.fsDate);
            return sum;
            //entity.amount = sum;
            //_fsCaptionDetailRepo.AddCustomerFSCaptionDetail(entity);
        }

        //private string GetCustomerFSRatio(bool isRatio, int customerId, short fsCaptionId, DateTime fsDate)
        //{
        //    // it is not ratio (not derived)
        //    if (isRatio == false)
        //    {
        //        var fsAmount = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
        //                        where a.CUSTOMERID == customerId && a.FSDATE == fsDate && a.FSCAPTIONID == fsCaptionId
        //                        select a).FirstOrDefault();

        //        if (fsAmount != null) {
        //            if (fsAmount.AMOUNT != 0)
        //                return string.Format("{0:n}", fsAmount.AMOUNT);
        //            else if (!string.IsNullOrEmpty(fsAmount.TEXTVALUE) && !string.IsNullOrWhiteSpace(fsAmount.TEXTVALUE))
        //                return fsAmount.TEXTVALUE;
        //        }
        //        //else
        //        //    return "0.00";
        //    }

        //    // it is ratio (derived)
        //    if (isRatio) {
        //        var details = (from c in context.TBL_CUSTOMER_FS_RATIO_DETAIL
        //                       join f in context.TBL_CUSTOMER_FS_CAPTION on c.FSCAPTIONID equals f.FSCAPTIONID
        //                       join t in context.TBL_CUSTOMER_FS_CAPTION_DETAIL on c.FSCAPTIONID equals t.FSCAPTIONID
        //                       where c.RATIOCAPTIONID == fsCaptionId && t.FSDATE == fsDate
        //                       select new { c.FSCAPTIONID, c.DIVISORTYPEID, f.ISRATIO}).ToList().OrderBy(O => O.DIVISORTYPEID).ToList();

        //        if (details.Count > 0) {
        //            decimal sum = 0, newSum = 0;

        //            foreach (var detail in details) {
        //                if (detail.ISRATIO) {

        //                    // testing 
        //                    //var newDetails = (from c in context.TBL_CUSTOMER_FS_RATIO_DETAIL
        //                    //               join f in context.TBL_CUSTOMER_FS_CAPTION on c.FSCAPTIONID equals f.FSCAPTIONID
        //                    //               join t in context.TBL_CUSTOMER_FS_CAPTION_DETAIL on c.FSCAPTIONID equals t.FSCAPTIONID
        //                    //               where c.RATIOCAPTIONID == detail.FSCAPTIONID && t.FSDATE == fsDate
        //                    //               select new { c.FSCAPTIONID, c.DIVISORTYPEID, f.ISRATIO }).ToList().OrderBy(O => O.DIVISORTYPEID).ToList();

        //                    //if (newDetails.Count > 0) {
        //                    //    foreach (var newDetail in newDetails)
        //                    //    {
        //                    //        if (newDetail.ISRATIO) {
        //                    //            var newCalculatedValue = CalculateFSRatioValue(customerId, (short)detail.FSCAPTIONID, fsDate);
        //                    //            newSum = CalculateFSRatioValueDerived(newSum, detail.DIVISORTYPEID, newCalculatedValue);
        //                    //        }
        //                    //        else
        //                    //        {
        //                    //            var captionDetail = (from O in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
        //                    //                                 where O.FSCAPTIONID == detail.FSCAPTIONID && O.CUSTOMERID == customerId
        //                    //                                 select O).FirstOrDefault();

        //                    //            if (captionDetail != null && captionDetail.AMOUNT > 0)
        //                    //            {
        //                    //                newSum = CalculateFSRatioValueDerived(newSum, detail.DIVISORTYPEID, captionDetail.AMOUNT);
        //                    //            }
        //                    //        }
        //                    //    }

        //                    //    sum = newSum;
        //                    //}
        //                    //else {
        //                        var calculatedValue = CalculateFSRatioValue(customerId, (short)detail.FSCAPTIONID, fsDate);
        //                        sum = CalculateFSRatioValueDerived(sum, detail.DIVISORTYPEID, calculatedValue);
        //                    //}

        //                }
        //                else {
        //                    var captionDetail = (from O in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
        //                                        where O.FSCAPTIONID == detail.FSCAPTIONID && O.CUSTOMERID == customerId
        //                                        select O).FirstOrDefault();

        //                    if (captionDetail != null && captionDetail.AMOUNT > 0) {
        //                        sum = CalculateFSRatioValueDerived(sum, detail.DIVISORTYPEID, captionDetail.AMOUNT);
        //                    }
        //                }
        //            }

        //            // Indicative Decisions
        //            if (fsCaptionId == 29) {
        //                // var computedValue = CalculateFSRatioValue(customerId, 28, fsDate);
        //                if ((double) sum <= 1.5) {
        //                    return "OK"; 
        //                }
        //                else {
        //                    return "DECLINED";
        //                }
        //            }

        //            return string.Format("{0:n}", sum);
        //        }

        //        return string.Format("{0:n}", CalculateFSRatioValue(customerId, fsCaptionId, fsDate));
        //    }

        //    // neither ratio nor non ratio
        //    return string.Format("{0:n}", 0);
        //}

        private decimal CalculateFSRatioValueDerived(decimal sum, short divisorTypeId, decimal calculatedValue)
        {
            if (divisorTypeId == 1) {
                if (sum == 0) {
                    sum = 1 * calculatedValue;
                }
                else {
                    sum = sum * calculatedValue;
                }
            }
            else if (divisorTypeId == 2) {
                if (sum == 0) {
                    sum = calculatedValue == 0 ? 0 : 1 * (1 / calculatedValue);
                }
                else {
                    sum = calculatedValue == 0 ? 0 : sum * (1 / calculatedValue);
                }
            }
            else if (divisorTypeId == 3) {
                    sum = sum + calculatedValue;
            }
            else {
                    sum = sum - calculatedValue;
            }

            return sum;
        }

        private decimal CalculateFSRatioValue(int customerId, short fsCaptionId, DateTime fsDate)
        {
            var customerFS = (from a in context.TBL_CUSTOMER_FS_CAPTION_DETAIL
                             where a.CUSTOMERID == customerId && a.FSDATE == fsDate && a.DELETED == false
                             select a).ToList();

            var ratios = (from a in context.TBL_CUSTOMER_FS_RATIO_DETAIL
                         where a.RATIOCAPTIONID == fsCaptionId && a.DELETED == false
                         select a).ToList();

            var numeratorInfo = (from a in ratios
                                join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                                where a.DIVISORTYPEID == 1
                                select new { a.MULTIPLIER, b.AMOUNT }).ToList();

            double numerator = 0;
            if (numeratorInfo.Count() > 0)
                numerator = (from a in numeratorInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();

            var demoninatorInfo = (from a in ratios
                                  join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                                  where a.DIVISORTYPEID == 2
                                  select new { a.MULTIPLIER, b.AMOUNT }).ToList();

            double demoninator = 0;
            if (demoninatorInfo.Count() > 0)
                demoninator = (from a in demoninatorInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();

            // extending the financial 
            var additionInfo = (from a in ratios
                               join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                               where a.DIVISORTYPEID == 3
                               select new { a.MULTIPLIER, b.AMOUNT }).ToList();

            double addition = 0;
            if (additionInfo.Count() > 0)
                addition = (from a in additionInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();

            var subtractionInfo = (from a in ratios
                                  join b in customerFS on a.FSCAPTIONID equals b.FSCAPTIONID
                                  where a.DIVISORTYPEID == 4
                                  select new { a.MULTIPLIER, b.AMOUNT }).ToList();

            double subtraction = 0;
            if (subtractionInfo.Count() > 0)
                subtraction = (from a in subtractionInfo select a.MULTIPLIER * (double)a.AMOUNT).Sum();


            if (demoninator == 0)
                return (decimal) (numerator + addition - subtraction);
            else
                return (((decimal)numerator / (decimal)demoninator) + (decimal)addition - (decimal)subtraction);
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
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
