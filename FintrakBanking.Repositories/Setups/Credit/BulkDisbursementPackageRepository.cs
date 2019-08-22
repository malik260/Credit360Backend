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

        //public IEnumerable<CustomerProductFeeViewModel> GetAllCustomerProductFees(int companyId)
        //{
        //    var data = (from a in context.TBL_CUSTOMER_PRODUCT_FEE
        //                where a.DELETED == false
        //                select new CustomerProductFeeViewModel
        //                {
        //                    chargeFeeId = a.CHARGEFEEID,
        //                    chargeFee = a.TBL_CHARGE_FEE.CHARGEFEENAME,
        //                    customerId = a.CUSTOMERID,
        //                    customerProductFeeId = a.CUSTOMER_PRODUCT_FEEID,
        //                    dependentAmount = a.DEPENDENTAMOUNT,
        //                    rateValue = a.RATEVALUE,
        //                    productId = a.PRODUCTID,
        //                    product = a.TBL_PRODUCT.PRODUCTNAME,
        //                    productCode = a.TBL_PRODUCT.PRODUCTCODE,
        //                    companyId = a.COMPANYID,
        //                    companyName = a.TBL_COMPANY.NAME,
        //                    dateTimeCreated = a.DATETIMECREATED,
        //                    createdBy = a.CREATEDBY
        //                }).ToList();
        //    return data;

        //}
        //public IEnumerable<CustomerProductFeeViewModel> GetCustomerProductFeeByCustomerId(int companyId, int customerId)
        //{
        //    return GetAllCustomerProductFees(companyId).Where(x => x.customerId == customerId);
        //}

        //public IEnumerable<CustomerProductFeeViewModel> GetCustomerProductFeeByProductId(int companyId, int productId)
        //{
        //    return GetAllCustomerProductFees(companyId).Where(x => x.customerId == productId);
        //}

        public bool AddBulkDisbursementPackage(BulkDisbursementSetupPackageViewModel model)
        {
            var customerData = context.TBL_CUSTOMER.Find(model.groupCustomerId);
            if(customerData == null) { throw new ConditionNotMetException("The customer does not exist"); }

            var data = new TBL_LOAN_BULK_DISBURSE_PACKAGE
            {
                COMPANYID = (short)model.companyId,

                //DELETED = false,
                //DATETIMECREATED = _genSetup.GetApplicationDate(),
                //CREATEDBY = (int)model.createdBy
            };

            //Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.BulkDisbursementPackageAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Bulk Disbursement Package for '{customerData.CUSTOMERCODE}'  ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            context.TBL_LOAN_BULK_DISBURSE_PACKAGE.Add(data);
            this.auditTrail.AddAuditTrail(audit);
            //end of Audit section -------------------------------

            return context.SaveChanges() != 0;
        }


        //public bool AddMultipleCustomerProductFees(List<CustomerProductFeeViewModel> models)
        //{
        //    if (models.Count <= 0)
        //        return false;

        //    foreach (CustomerProductFeeViewModel model in models)
        //    {
        //        AddCustomerProductFee(model);
        //    }
        //    return true;
        //}


        //public bool UpdateCustomerProductFee(int customerProductFeeId, CustomerProductFeeViewModel model)
        //{
        //    var data = this.context.TBL_CUSTOMER_PRODUCT_FEE.Find(customerProductFeeId);
        //    if (data == null) return false;
        //    data.RATEVALUE = (int)model.rateValue;
        //    data.DEPENDENTAMOUNT = model.dependentAmount;
        //    data.CUSTOMERID = model.customerId;
        //    data.PRODUCTID = (short)model.productId;
        //    data.DATETIMEUPDATED = _genSetup.GetApplicationDate();
        //    data.LASTUPDATEDBY = (int)model.createdBy;
        //    //Audit Section ---------------------------

        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.CustomerproductFeeUpdated,
        //        STAFFID = model.createdBy,
        //        BRANCHID = (short)model.userBranchId,
        //        DETAIL = $"Updated Customer Product Fee for customer with code '{data.TBL_CUSTOMER.CUSTOMERCODE}' on Product '{data.TBL_PRODUCT.PRODUCTNAME}' and rate '{data.RATEVALUE}' ",
        //        IPADDRESS = model.userIPAddress,
        //        URL = model.applicationUrl,
        //        APPLICATIONDATE = _genSetup.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    };
        //    this.auditTrail.AddAuditTrail(audit);
        //    // end of Audit section -------------------------------

        //    return context.SaveChanges() != 0;

        //}

        //public bool DeleteCustomerProductFee(int customerProductFeeId, UserInfo user)
        //{
        //    var data = this.context.TBL_CUSTOMER_PRODUCT_FEE.Find(customerProductFeeId);
        //    data.DELETED = true;
        //    data.DELETEDBY = (int)user.staffId;
        //    data.DATETIMEDELETED = _genSetup.GetApplicationDate();

        //    // Audit Section ---------------------------
        //    var customerData = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == data.CUSTOMERID).FirstOrDefault();
        //    var productData = context.TBL_PRODUCT.Where(x => x.PRODUCTID == data.PRODUCTID).FirstOrDefault();
        //    var audit = new TBL_AUDIT
        //    {
        //        AUDITTYPEID = (short)AuditTypeEnum.CustomerproductFeeUpdated,
        //        STAFFID = user.createdBy,
        //        BRANCHID = (short)user.BranchId,
        //        DETAIL = $"Updated Customer Product Fee for customer with code '{customerData.CUSTOMERCODE}' on Product '{productData.PRODUCTNAME}' and rate '{data.RATEVALUE}' ",
        //        IPADDRESS = user.userIPAddress,
        //        URL = user.applicationUrl,
        //        APPLICATIONDATE = _genSetup.GetApplicationDate(),
        //        SYSTEMDATETIME = DateTime.Now
        //    };
        //    this.auditTrail.AddAuditTrail(audit);
        //    //end of Audit section -------------------------------

        //    return context.SaveChanges() != 0;
        //}
        #endregion

    }
}
