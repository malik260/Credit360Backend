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

        public IEnumerable<BulkDisbursementSetupPackageViewModel> GetAllBulkDisbursementPackageByGroupCustomerId(int groupCustomerId)
        {
            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_PACKAGE
                        where a.GROUPCUSTOMERID == groupCustomerId && a.DELETED == false
                        select new BulkDisbursementSetupPackageViewModel
                       {
                           startDate = a.STARTDATE,
                           endDate = a.ENDDATE,
                           groupCustomerId = a.GROUPCUSTOMERID,
                           packageDescription = a.PACKAGEDESCRIPTION,
                            customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.GROUPCUSTOMERID).FirstOrDefault().FIRSTNAME,
                        }).ToList();
           return data;

        }

        public IEnumerable<BulkDisbursementSetupPackageViewModel> GetAllBulkDisbursementPackageByCompany()
        {
            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_PACKAGE
                       // where a.DELETED == false
                        select new BulkDisbursementSetupPackageViewModel
                        {
                            startDate = a.STARTDATE,
                            endDate = a.ENDDATE,
                            groupCustomerId =  a.GROUPCUSTOMERID,
                            packageDescription = a.PACKAGEDESCRIPTION,
                            customerName = context.TBL_CUSTOMER.Where(c=>c.CUSTOMERID == a.GROUPCUSTOMERID).FirstOrDefault().FIRSTNAME,
                            
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupPackageViewModel> GetBulkDisbursementPackageById(int disbursementPackageId)
        {
            return GetAllBulkDisbursementPackageByGroupCustomerId(disbursementPackageId).Where(x => x.disbursementPackageId == disbursementPackageId);
        }

        public bool AddBulkDisbursementPackage(BulkDisbursementSetupPackageViewModel model)
        {
            var customerData = context.TBL_CUSTOMER.Find(model.groupCustomerId);
            if(customerData == null) { throw new ConditionNotMetException("The customer does not exist"); }

            var data = new TBL_LOAN_BULK_DISBURSE_PACKAGE
            {
                COMPANYID = (short)model.companyId,
                STARTDATE = model.startDate,
                ENDDATE = model.endDate,
                GROUPCUSTOMERID = model.groupCustomerId,
                PACKAGEDESCRIPTION = model.packageDescription,
                CREATEDBY = model.createdBy,
                LASTUPDATEDBY = model.lastUpdatedBy,
                DATETIMECREATED = _genSetup.GetApplicationDate(),
                DELETED = false,
                DELETEDBY = 0,
                DATETIMEDELETED = _genSetup.GetApplicationDate()
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementPackageAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Bulk Disbursement Package for '{customerData.CUSTOMERCODE}'  ",
                // IPADDRESS = model.userIPAddress,
                // URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_LOAN_BULK_DISBURSE_PACKAGE.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------



            return context.SaveChanges() != 0;
        }


        public bool AddMultipleBulkDisbursementPackage(List<BulkDisbursementSetupPackageViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (BulkDisbursementSetupPackageViewModel model in models)
            {
                AddBulkDisbursementPackage(model);
            }
            return true;
        }


        public bool UpdateBulkDisbursementPackage(int disbursementPackageId, BulkDisbursementSetupPackageViewModel model)
        {
            var customerData = context.TBL_CUSTOMER.Find(model.groupCustomerId);
            if (customerData == null) { throw new ConditionNotMetException("The customer does not exist"); }

            var data = this.context.TBL_LOAN_BULK_DISBURSE_PACKAGE.Find(disbursementPackageId);
            if (data == null) return false;
            data.STARTDATE = model.startDate;
            data.ENDDATE = model.endDate;
            data.GROUPCUSTOMERID = model.groupCustomerId;
            data.PACKAGEDESCRIPTION = model.packageDescription;
            data.COMPANYID = (short)model.companyId;
            data.CREATEDBY = model.createdBy;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMECREATED = _genSetup.GetApplicationDate();
            data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
            data.DELETED = false;
            data.DELETEDBY = 0;
            data.DATETIMEDELETED = _genSetup.GetApplicationDate();

            //Audit Section ---------------------------

            var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementPackageUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Bulk Disbursement package with code '{customerData.CUSTOMERCODE}' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.auditTrail.AddAuditTrail(audit);
                // end of Audit section -------------------------------

                return context.SaveChanges() != 0;

            }

        public bool DeleteBulkDisbursementPackage(int disbursementPackageId, UserInfo user)
        {
            var data = this.context.TBL_LOAN_BULK_DISBURSE_PACKAGE.Find(disbursementPackageId);
            data.DELETED = true;
            data.DELETEDBY = user.staffId;
            // Audit Section ---------------------------
            var customerData = context.TBL_LOAN_BULK_DISBURSE_PACKAGE.Where(x => x.GROUPCUSTOMERID == data.GROUPCUSTOMERID).FirstOrDefault();
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementPackageDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.companyId,
                DETAIL = $"Updated Bulk Disbursement with code '{customerData.COMPANYID}'",
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
        
        #region 

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByPackageId(int disbursementPackageId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.DISBURSEMENTPACKAGEID == disbursementPackageId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disbursementPackageId = (int)a.DISBURSEMENTPACKAGEID,
                            productId = a.PRODUCTID,
                            tenor = a.TENOR,
                            scheduleMethodId = (short)a.SCHEDULEMETHODID,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            includeProductFees = a.INCLUDEPRODUCTFEES,
                            approvalStatusId = (short)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByProductId(int productId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.PRODUCTID == productId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disbursementPackageId = (int)a.DISBURSEMENTPACKAGEID,
                            productId = a.PRODUCTID,
                            tenor = a.TENOR,
                            scheduleMethodId = (short)a.SCHEDULEMETHODID,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            includeProductFees = a.INCLUDEPRODUCTFEES,
                            approvalStatusId = (short)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementSchemeByDisburseSchemeId(int disburseSchemeId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.DISBURSESCHEMEID == disburseSchemeId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disbursementPackageId = (int)a.DISBURSEMENTPACKAGEID,
                            productId = a.PRODUCTID,
                            tenor = a.TENOR,
                            scheduleMethodId = (short)a.SCHEDULEMETHODID,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            includeProductFees = a.INCLUDEPRODUCTFEES,
                            approvalStatusId = (short)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeViewModel> GetAllBulkDisbursementScheme(int companyId)
        {

            var data = (from a in context.TBL_LOAN_BULK_DISBURSE_SCHEME
                        where a.COMPANYID == companyId
                        select new BulkDisbursementSetupSchemeViewModel
                        {
                            disbursementPackageId = (int)a.DISBURSEMENTPACKAGEID,
                            productId = a.PRODUCTID,
                            tenor = a.TENOR,
                            scheduleMethodId = (short)a.SCHEDULEMETHODID,
                            interestRate = a.INTERESTRATE,
                            productPriceIndexId = (int)a.PRODUCTPRICEINDEXID,
                            includeProductFees = a.INCLUDEPRODUCTFEES,
                            approvalStatusId = (short)a.APPROVALSTATUSID
                        }).ToList();
            return data;

        }


        public bool AddBulkDisbursementScheme(BulkDisbursementSetupSchemeViewModel model)
        {
            var customerData = context.TBL_CUSTOMER.Find(model.staffId);
            if (customerData == null) { throw new ConditionNotMetException("The scheme does not exist"); }

            var data = new TBL_LOAN_BULK_DISBURSE_SCHEME
            {
                DISBURSEMENTPACKAGEID = (int)model.disbursementPackageId,
                PRODUCTID = model.productId,
                TENOR = model.tenor,
                SCHEDULEMETHODID = (short)model.scheduleMethodId,
                INTERESTRATE = model.interestRate,
                PRODUCTPRICEINDEXID = model.productPriceIndexId,
                INCLUDEPRODUCTFEES = model.includeProductFees,
                APPROVALSTATUSID = (short)model.approvalStatusId
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementSchemeAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Bulk Disbursement Scheme for '{customerData.CUSTOMERCODE}'  ",
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
            var customerData = context.TBL_CUSTOMER.Find(model.staffId);
            if (customerData == null) { throw new ConditionNotMetException("The customer does not exist"); }

            var data = this.context.TBL_LOAN_BULK_DISBURSE_SCHEME.Find(disbursementSchemeId);
            if (data == null) return false;
            data.DISBURSEMENTPACKAGEID = (int)model.disbursementPackageId;
            data.PRODUCTID = model.productId;
            data.TENOR = model.tenor;
            data.SCHEDULEMETHODID = (short)model.scheduleMethodId;
            data.INTERESTRATE = model.interestRate;
            data.PRODUCTPRICEINDEXID = model.productPriceIndexId;
            data.INCLUDEPRODUCTFEES = model.includeProductFees;
            data.APPROVALSTATUSID = (short)model.approvalStatusId;

            //Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementSchemeUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Bulk Disbursement scheme with code '{customerData.CUSTOMERCODE}' ",
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
            
            // Audit Section ---------------------------
            var customerData = context.TBL_LOAN_BULK_DISBURSE_SCHEME.Where(x => x.DISBURSEMENTPACKAGEID == data.DISBURSEMENTPACKAGEID).FirstOrDefault();
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CustomerproductFeeUpdated,
                STAFFID = user.staffId,
                BRANCHID = (short)user.companyId,
                DETAIL = $"Deleted Bulk Disbursement Scheme with code '{customerData.COMPANYID}'",
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

        #region 

        public IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetAllBulkDisbursementSchemeFeesDisburseSchemeId(int disburseSchemeId)
        {

            var data = (from a in context.TBL_BULK_DISBURS_SCH_FEES
                        where a.DISBURSESCHEMEID == disburseSchemeId
                        select new BulkDisbursementSetupSchemeFeesViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            chargeFeeId = a.DISBURSESCHEMEID,
                            hasConcession = a.HASCONCESSION,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetAllBulkDisbursementSchemeFees()
        {

            var data = (from a in context.TBL_BULK_DISBURS_SCH_FEES
                        select new BulkDisbursementSetupSchemeFeesViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            chargeFeeId = a.DISBURSESCHEMEID,
                            hasConcession = a.HASCONCESSION,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                        }).ToList();
            return data;

        }

        public IEnumerable<BulkDisbursementSetupSchemeFeesViewModel> GetBulkDisbursementSchemeFeesById(int schemeFeeId)
        {
            var data = (from a in context.TBL_BULK_DISBURS_SCH_FEES
                        where a.DISBURSESCHEMEID == schemeFeeId
                        select new BulkDisbursementSetupSchemeFeesViewModel
                        {
                            disburseSchemeId = a.DISBURSESCHEMEID,
                            chargeFeeId = a.DISBURSESCHEMEID,
                            hasConcession = a.HASCONCESSION,
                            approvalStatusId = (short)a.APPROVALSTATUSID,
                        }).ToList();
            return data;
        }

        public bool AddBulkDisbursementSchemeFees(BulkDisbursementSetupSchemeFeesViewModel model)
        {
            var data = new TBL_BULK_DISBURS_SCH_FEES
            {
                DISBURSESCHEMEID = model.disburseSchemeId,
                CHARGEFEEID = model.chargeFeeId,
                HASCONCESSION = model.hasConcession,
                APPROVALSTATUSID = (short)model.approvalStatusId
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementFeesAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Bulk Disbursement Scheme for fees",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_BULK_DISBURS_SCH_FEES.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }


        public bool AddMultipleBulkDisbursementSchemeFees(List<BulkDisbursementSetupSchemeFeesViewModel> models)
        {
            if (models.Count <= 0)
                return false;

            foreach (BulkDisbursementSetupSchemeFeesViewModel model in models)
            {
                AddBulkDisbursementSchemeFees(model);
            }
            return true;
        }


        public bool UpdateBulkDisbursementSchemeFees(int schemeFeeId, BulkDisbursementSetupSchemeFeesViewModel model)
        {

            var data = this.context.TBL_BULK_DISBURS_SCH_FEES.Find(schemeFeeId);
            if (data == null) return false;
            data.DISBURSESCHEMEID = model.disburseSchemeId;
            data.CHARGEFEEID = model.chargeFeeId;
            data.HASCONCESSION = model.hasConcession;
            data.APPROVALSTATUSID = (short)model.approvalStatusId;

            //Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementFeesUpdated,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Bulk Disbursement scheme fees with code ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // end of Audit section -------------------------------

            return context.SaveChanges() != 0;

        }

        public bool DeleteBulkDisbursementSchemeFees(int schemeFeeId, UserInfo user)
        {
            var data = this.context.TBL_BULK_DISBURS_SCH_FEES.Find(schemeFeeId);

            // Audit Section ---------------------------
            // var customerData = context.TBL_BULK_DISBURS_SCH_FEES.Where(x => x.DISBURSEMENTPACKAGEID == data.DISBURSEMENTPACKAGEID).FirstOrDefault();
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementFeesDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.companyId,
                DETAIL = $"Deleted Bulk Disbursement Scheme fees",
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
    }
}
