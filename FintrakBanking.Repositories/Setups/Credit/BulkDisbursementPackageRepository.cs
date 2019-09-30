using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FintrakBanking.Interfaces.Setups;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using System.ComponentModel.Composition;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;


namespace FintrakBanking.Repositories.Setups.Credit
{

    public class BulkDisbursementPackageRepository : IBulkDisbursementPackageRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository _genSetup;
        private IAuditTrailRepository auditTrail;
       
        public BulkDisbursementPackageRepository(FinTrakBankingContext _context,
                                                    IGeneralSetupRepository genSetup,
                                                    IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this._genSetup = genSetup;
            auditTrail = _auditTrail;
        }
        
        #region 

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisburseSchemeByApplicationReferenceNumber(string applicationReferenceNumber)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                       // where a.APPLICATIONREFERENCENUMBER == applicationReferenceNumber
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            //LOANAPPLICATIONDETAIL = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == a.LOANAPPLICATIONDETAILID).Select(c=> new TBL_LOAN_APPLICATION_DETAIL {

                            //}).FirstOrDefault(),
                            schemeCode = a.SCHEMECODE,
                            productId = a.PRODUCTID,
                            currencyId = a.CURRENCYID,
                            currencyName = context.TBL_CURRENCY.Where(r => r.CURRENCYID == a.CURRENCYID).FirstOrDefault().CURRENCYNAME,
                            facilityName = context.TBL_PRODUCT.Where(c => c.PRODUCTID == a.PRODUCTID).FirstOrDefault().PRODUCTNAME,               
                            tenor = a.TENOR,
                            scheduleMethodId = (int)a.SCHEDULEMETHODID,
                            schemeName = a.SCHEMENAME,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            scheduleName = context.TBL_LOAN_SCHEDULE_TYPE.Where(c => c.SCHEDULETYPEID == a.SCHEDULEMETHODID).FirstOrDefault().SCHEDULETYPENAME,
                            //approvalStatusId = (int)a.APPROVALSTATUSID
                        }).ToList();
            
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByProductId(int productId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.PRODUCTID == productId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                            interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                            principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            schemeCode = a.SCHEMECODE,
                            productId = a.PRODUCTID,
                            currencyId = a.CURRENCYID,
                            currencyName = context.TBL_CURRENCY.Where(r => r.CURRENCYID == a.CURRENCYID).FirstOrDefault().CURRENCYNAME,
                            facilityName = context.TBL_PRODUCT.Where(c => c.PRODUCTID == a.PRODUCTID).FirstOrDefault().PRODUCTNAME,
                            tenor = a.TENOR,
                            scheduleMethodId = (int)a.SCHEDULEMETHODID,
                            schemeName = a.SCHEMENAME,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            scheduleName = context.TBL_LOAN_SCHEDULE_TYPE.Where(c => c.SCHEDULETYPEID == a.SCHEDULEMETHODID).FirstOrDefault().SCHEDULETYPENAME,
                            //approvalStatusId = (int)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByDisburseSchemeId(int disburseSchemeId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.DISBURSESCHEMEID == disburseSchemeId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                            interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                            principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                            loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                            schemeCode = a.SCHEMECODE,
                            productId = a.PRODUCTID,
                            facilityName = context.TBL_PRODUCT.Where(c => c.PRODUCTID == a.PRODUCTID).FirstOrDefault().PRODUCTNAME,
                            tenor = a.TENOR,
                            dateTimeCreated= a.DATETIMECREATED,
                            scheduleMethodId = (int)a.SCHEDULEMETHODID,
                            schemeName = a.SCHEMENAME,
                            interestRate = a.INTERESTRATE,
                            currencyId = a.CURRENCYID,
                            currencyName = context.TBL_CURRENCY.Where(r => r.CURRENCYID == a.CURRENCYID).FirstOrDefault().CURRENCYNAME,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            scheduleName = context.TBL_LOAN_SCHEDULE_TYPE.Where(c => c.SCHEDULETYPEID == a.SCHEDULEMETHODID).FirstOrDefault().SCHEDULETYPENAME,
                            //approvalStatusId = (int)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementScheme()
        {
            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                                join x in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals x.LOANAPPLICATIONDETAILID
                                join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                
                                select new BulkDisbursementSetupSchemeViewModel
                                {
                                    disburseSchemeId = a.DISBURSESCHEMEID,
                                    scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                                    interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                                    principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    schemeCode = a.SCHEMECODE,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    applicationReferenceNumber = b.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = b.LOANAPPLICATIONID,
                                    productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                                    scheduleMethodId = (int)a.SCHEDULEMETHODID,
                                    productId = a.PRODUCTID,
                                    tenor = a.TENOR,
                                    schemeName = a.SCHEMENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    interestRate = a.INTERESTRATE,
                                    currencyId = a.CURRENCYID,
                                    currencyName = context.TBL_CURRENCY.Where(r => r.CURRENCYID == a.CURRENCYID).FirstOrDefault().CURRENCYNAME,
                                    facilityName = context.TBL_PRODUCT.Where(d => d.PRODUCTID == a.PRODUCTID).FirstOrDefault().PRODUCTNAME,
                                    scheduleName = context.TBL_LOAN_SCHEDULE_TYPE.Where(l => l.SCHEDULETYPEID == a.SCHEDULEMETHODID).FirstOrDefault().SCHEDULETYPENAME
                                })?.ToList();

                     return data;                         

        }


        public bool AddBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel model)
        {
            var schedulTypeData = context.TBL_LOAN_SCHEDULE_TYPE.Where(s => s.SCHEDULETYPEID == model.scheduleMethodId).FirstOrDefault();
            if (schedulTypeData == null) {
                throw new ConditionNotMetException("The LOANAPPLICATIONDETAILID does not exist");
            }

            var productData = context.TBL_PRODUCT.Where(p => p.PRODUCTID == model.productId);
            if (productData == null) {
                throw new ConditionNotMetException("The PRODUCTID does not exist");
            }

            var schemeCode = CommonHelpers.GenerateRandomDigitCode(10);

            var data = new TBL_LOAN_BULK_DISBURSE_SCHEME
            {
                LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                SCHEDULEDAYCOUNTCONVENTIONID = model.scheduleDayCountConventionId,
                INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId,
                PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId,
                SCHEMECODE = schemeCode, //model.schemeCode,
                PRODUCTID = model.productId,
                TENOR = model.tenor,
                SCHEDULEMETHODID = (short)model.scheduleMethodId,
                INTERESTRATE = model.interestRate,
                PRODUCTPRICEINDEXID = model.productPriceIndexId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                COMPANYID = (short)model.companyId,
                SCHEMENAME = model.schemeName,
                CURRENCYID = model.currencyId
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementSchemeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Bulk Disbursement Scheme for   ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_LOAN_BULK_DISBURSE_SCHEME.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }


        public bool AddMultipleBulkDisbursementScheme(List<BulkDisbursementSetupSchemeViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (BulkDisbursementSetupSchemeViewModel model in models)
            {
                AddBulkDisbursementScheme(model);
            }
            return true;
        }


        public bool UpdateBulkDisbursementScheme(int disbursementSchemeId, BulkDisbursementSetupSchemeViewModel model)
        {
            var loanApplicationDetailData = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId);
            if (loanApplicationDetailData == null)
            {
                throw new ConditionNotMetException("The LOANAPPLICATIONDETAILID does not exist");
            }

            var priceIndexData = context.TBL_PRODUCT_PRICE_INDEX.Where(pi => pi.PRODUCTPRICEINDEXID == model.productPriceIndexId);
            if (priceIndexData == null)
            {
                throw new ConditionNotMetException("The PRODUCTPRICEINDEXID does not exist");
            }

            var productData = context.TBL_PRODUCT.Where(p => p.PRODUCTID == model.productId);
            if (productData == null)
            {
                throw new ConditionNotMetException("The PRODUCTID does not exist");
            }

            var data = this.context.TBL_LOAN_BULK_DISBURSE_SCHEME.Find(disbursementSchemeId);
            if (data == null) return false;
            data.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
            data.SCHEDULEDAYCOUNTCONVENTIONID = model.scheduleDayCountConventionId;
            data.INTERESTFREQUENCYTYPEID = model.interestFrequencyTypeId;
            data.PRINCIPALFREQUENCYTYPEID = model.principalFrequencyTypeId;
            data.SCHEMECODE = model.schemeCode;
            data.PRODUCTID = model.productId;
            data.TENOR = model.tenor;
            data.SCHEDULEMETHODID = (short)model.scheduleMethodId;
            data.INTERESTRATE = model.interestRate;
            data.PRODUCTPRICEINDEXID = model.productPriceIndexId;
            data.APPROVALSTATUSID = (short)model.approvalStatusId;
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.CURRENCYID = model.currencyId;

            //Audit Section -------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementSchemeUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Bulk Disbursement scheme with code '{model.createdBy}' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
               
            };
            this.auditTrail.AddAuditTrail(audit);
            // end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        public bool DeleteBulkDisbursementScheme(int disbursementPackageId, UserInfo user)
        {
            var data = this.context.TBL_LOAN_BULK_DISBURSE_SCHEME.Find(disbursementPackageId);
            data.DELETED = true;
            data.DELETEDBY = user.staffId;
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerproductFeeUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.companyId,
                DETAIL = $"Deleted Bulk Disbursement Scheme with id '{data.DISBURSESCHEMEID}'",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }
        #endregion

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> SearchLoanApplicationDetails(int companyId, string searchQuery)
        {
            searchQuery = searchQuery?.Trim()?.ToLower();

            var allApplicationDetails = (from d in context.TBL_LOAN_APPLICATION_DETAIL
                                         join a in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                         join c in context.TBL_LOAN_BULK_DISBURSE_SCHEME on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                         where a.APPLICATIONREFERENCENUMBER.Trim().Contains(searchQuery.Trim())
                                         //  || c.FIRSTNAME.ToLower().StartsWith(searchQuery)
                                         //  || c.CUSTOMERCODE.ToLower().Contains(searchQuery)
                                         //|| c.MIDDLENAME.ToLower().StartsWith(searchQuery)
                                         //|| c.LASTNAME.ToLower().StartsWith(searchQuery)
                                         || a.TBL_CASA.PRODUCTACCOUNTNUMBER == searchQuery
                                         select new BulkDisbursementSetupSchemeViewModel
                                         {   
                                             
                                             applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                             
                                         })?.ToList();
            return allApplicationDetails;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> SchemeSearch(string searchString)
        {
            var search = searchString.Trim().ToLower();

            var applications = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                                join x in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals x.LOANAPPLICATIONDETAILID
                                join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                where (a.SCHEMECODE.Contains(search)
                                      || a.SCHEMENAME.ToLower().Contains(search))
                                select new BulkDisbursementSetupSchemeViewModel
                                {
                                    disburseSchemeId = a.DISBURSESCHEMEID,
                                    scheduleDayCountConventionId = a.SCHEDULEDAYCOUNTCONVENTIONID,
                                    interestFrequencyTypeId = a.INTERESTFREQUENCYTYPEID,
                                    principalFrequencyTypeId = a.PRINCIPALFREQUENCYTYPEID,
                                    loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                                    schemeCode = a.SCHEMECODE,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    applicationReferenceNumber = b.APPLICATIONREFERENCENUMBER,
                                    loanApplicationId = b.LOANAPPLICATIONID,
                                    productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                                    scheduleMethodId = (int)a.SCHEDULEMETHODID,
                                    productId = a.PRODUCTID,
                                    facilityName = context.TBL_PRODUCT.Where(c => c.PRODUCTID == a.PRODUCTID).FirstOrDefault().PRODUCTNAME,
                                    tenor = a.TENOR,
                                    approvedAmount = x.APPROVEDAMOUNT,
                                    schemeName = a.SCHEMENAME,
                                    interestRate = a.INTERESTRATE,
                                    currencyId = a.CURRENCYID,
                                    currencyName = context.TBL_CURRENCY.Where(r=>r.CURRENCYID == a.CURRENCYID).FirstOrDefault().CURRENCYNAME,
                                    scheduleName = context.TBL_LOAN_SCHEDULE_TYPE.Where(c => c.SCHEDULETYPEID == a.SCHEDULEMETHODID).FirstOrDefault().SCHEDULETYPENAME,
                                }).ToList();

            return applications;

        }

    }
}
