using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IProductFeeRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ProductFeeRepository : IProductFeeRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;

        public ProductFeeRepository(FinTrakBankingContext _context,
                                        IGeneralSetupRepository genSetup, IGeneralSetupRepository _genSetup,
                                        IAuditTrailRepository _auditTrail)
        {
            this.context = _context;
            this.genSetup = _genSetup;
            auditTrail = _auditTrail;
        }

        private bool SaveAll()
        {
            return this.context.SaveChanges() > 0;
        }

        public int AddMultipleProductFee(List<ProductFeeViewModel> productFees)
        {
            if (productFees.Count <= 0)
                return -1;

            foreach (ProductFeeViewModel item in productFees)
            {
                AddTempProductFee(item);
            }

            return 1;
        }


        public int AddTempProductFee(ProductFeeViewModel productFee)
        {
            var dataExist = this.context.tbl_Temp_Product_Fee.FirstOrDefault(x => x.ProductId == productFee.productId && x.FeeId == productFee.feeId && x.Deleted == true); // .Find(accountId);

            var tempProductFeeEntity = dataExist;

            if (dataExist == null)
            {
                tempProductFeeEntity = new tbl_Temp_Product_Fee()
                {
                    ProductId = productFee.productId,
                    FeeId = productFee.feeId,
                    CompanyId = productFee.companyId,

                    RateValue = productFee.rateValue,
                    DependentAmount = productFee.dependentAmount,

                    CreatedBy = productFee.createdBy,
                    DateTimeCreated = genSetup.GetApplicaionDate(),
                    Deleted = false
                };

                this.context.tbl_Temp_Product_Fee.Add(tempProductFeeEntity);
                // Audit Section ---------------------------
                var productName = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productFee.productId).ProductName;
                var audit = new tbl_Audit
                {
                    AuditTypeId = (short)AuditTypeEnum.ProductFeeAdded,
                    StaffId = productFee.createdBy,
                    BranchId = (short)productFee.userBranchId,
                    Detail = $"Initiated adding Fee: { productFee.feeName } for product {productName} with amount {productFee.rateValue} ",
                    IPAddress = productFee.userIPAddress,
                    Url = productFee.applicationUrl,
                    ApplicationDate = genSetup.GetApplicaionDate(),
                    SystemDateTime = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------
            }
            else
            {
                tempProductFeeEntity.RateValue = productFee.rateValue;
                tempProductFeeEntity.DependentAmount = productFee.dependentAmount;

                tempProductFeeEntity.Deleted = false;
            }

            var status = this.SaveAll();

            if (status)
                return tempProductFeeEntity.ProductFeeId;
            else
                return -1;
        }

        public void ApproveProductFee(int productId, UserInfo user)
        {
            var productFeeModel = context.tbl_Temp_Product_Fee.Where(x => x.ProductId == productId 
                                                                        && x.Deleted == false
                                                                        && x.IsCurrent == true);
            var productToUpdate = context.tbl_Product.Find(productId);

            foreach( var p in productFeeModel)
            {
                var product = new tbl_Product_Fee()
                {
                    ProductId = p.ProductId,
                    FeeId = p.FeeId,
                    CompanyId = p.CompanyId,

                    RateValue = p.RateValue,
                    DependentAmount = p.DependentAmount,

                    CreatedBy = p.CreatedBy,
                    DateTimeCreated = genSetup.GetApplicaionDate(),
                    Deleted = false
                };
                context.tbl_Product_Fee.Add(product);
                p.IsCurrent = false;
                p.Deleted = true;
            }
            
            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductFeeAdded,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Approved Fee for product '{productToUpdate.ProductName}' with product code'{productToUpdate.ProductCode}'",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            //return this.SaveAll();
        }

        public bool DeleteMultipleProductFee(List<int> productFeeIds)
        {
            if (productFeeIds.Count <= 0)
                return false;

            var dataList = (from a in context.tbl_Product_Fee
                            where productFeeIds.ToList().Contains(a.ProductFeeId)
                            select a);

            foreach (tbl_Product_Fee data in dataList)
            {
                data.Deleted = true;
                data.DateTimeDeleted = DateTime.Now;
            }

            return this.SaveAll();
        }

        public bool DeleteProductFee(int productFeeId, UserInfo user)
        {
            var data = this.context.tbl_Product_Fee.Find(productFeeId);

            if (data == null)
                return false;

            data.Deleted = true;
            //accountModel.DeletedBy = ;
            data.DateTimeDeleted = genSetup.GetApplicaionDate();
            // Audit Section ---------------------------
            var productFee = this.context.tbl_Fee.FirstOrDefault(x => x.FeeId == data.FeeId);
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductFeeDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted tbl_Product Fee: {productFee.FeeName} to product {data.tbl_Product} ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            return this.SaveAll();
        }

        public IEnumerable<ProductFeeViewModel> GetFeeByProduct(int productId)
        {
            return (from data in context.tbl_Product_Fee
                    where data.ProductId == productId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ProductFeeViewModel()
                    {
                        productFeeId = data.ProductFeeId,
                        productId = (short)data.ProductId,
                        feeId = data.FeeId,
                        feeName = data.tbl_Fee.FeeName,
                        feeIntervalName = data.tbl_Fee.tbl_Fee_Interval.FeeIntervalName,
                        feeTargetName = data.tbl_Fee.tbl_Fee_Target.FeeTargetName,
                        feeTypeName = data.tbl_Fee.tbl_Fee_Type.FeeTypeName,
                        glAccountCode = data.tbl_Fee.tbl_Chart_Of_Account.AccountCode,
                        glAccountName = data.tbl_Fee.tbl_Chart_Of_Account.AccountName,
                        companyId = data.CompanyId,

                        rateValue = data.RateValue,
                        dependentAmount = data.DependentAmount,

                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated,
                    });
        }

        public bool DoesProductFeeExist(int productFeeId)
        {
            return context.tbl_Product_Fee.Any(x => x.ProductFeeId == productFeeId);
        }

        public ProductFeeViewModel GetProductFee(int productFeeId)
        {
            return (from data in context.tbl_Product_Fee
                    where data.ProductFeeId == productFeeId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ProductFeeViewModel()
                    {
                        productFeeId = data.ProductFeeId,
                        productId = (short)data.ProductId,
                        feeId = data.FeeId,
                        feeName = data.tbl_Fee.FeeName,
                        companyId = data.CompanyId,

                        rateValue = data.RateValue,
                        dependentAmount = data.DependentAmount,

                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated,
                    }).FirstOrDefault();
        }

        public ProductFeeViewModel GetTempProductFee(int productFeeId)
        {
            return (from data in context.tbl_Temp_Product_Fee
                    where data.ProductFeeId == productFeeId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                    select new ProductFeeViewModel()
                    {
                        productFeeId = data.ProductFeeId,
                        productId = (short)data.ProductId,
                        feeId = data.FeeId,
                        feeName = data.tbl_Fee.FeeName,
                        companyId = data.CompanyId,

                        rateValue = data.RateValue,
                        dependentAmount = data.DependentAmount,

                        createdBy = data.CreatedBy,
                        dateTimeCreated = data.DateTimeCreated,
                    }).FirstOrDefault();
        }

        public IEnumerable<FeeViewModel> GetUnmappedFeeToProduct(int productId)
        {           
            var dataList = (from data in context.tbl_Product_Fee
                            where data.ProductId == productId && data.Deleted == false //orderby account.AccountCode ascending, account.AccountName ascending
                            select data.FeeId).ToList();           

            var fee = (from data in context.tbl_Fee
                       where data.Deleted == false // && !dataList.Contains(data.FeeId)
                       select new FeeViewModel
                       {
                           feeId = data.FeeId,
                           feeName = data.FeeName,
                           accountCategoryId = data.AccountCategoryId,
                           accountCategoryName = data.tbl_Account_Category.AccountCategoryName,
                           feeTypeId = data.FeeTypeId,
                           feeTypeName = data.tbl_Fee_Type.FeeTypeName,
                           feeTypeByAmountRequired = data.tbl_Fee_Type.ByAmountRequired,
                           byAmountRequired = data.tbl_Fee_Type.ByAmountRequired,
                           feeIntervalId = data.FeeIntervalId,
                           feeIntervalName = data.tbl_Fee_Interval.FeeIntervalName,
                           productTypeId = data.ProductTypeId,
                           productTypeName = data.tbl_Product_Type.ProductTypeName,
                           feeTargetId = data.FeeTargetId,
                           feeTargetName = data.tbl_Fee_Target.FeeTargetName,
                           glAccountId = data.GLAccountId,
                           glAccountCode = data.tbl_Chart_Of_Account.AccountCode,
                           includeCutOffDay = data.IncludeCutOffDay,
                           cutOffDay = data.CutOffDay,
                           companyId = data.CompanyId,
                           feeDate = data.FeeDate,
                           createdBy = data.CreatedBy,
                       });

            if (dataList.Any())
            {
                fee = fee.Where(x => !dataList.Contains(x.feeId));
            }

            return fee;
        }

        //public IEnumerable<FeeViewModel> GetUnmappedFeesToTempProduct(int productId)
        //{
        //    var dataList = (from data in context.tbl_Temp_Product_Fee
        //                    where data.ProductId == productId && data.Deleted == false
        //                    select data.FeeId).ToList();

        //    var fee = (from data in context.tbl_Temp_Fee
        //               where data.Deleted == false // && !dataList.Contains(data.FeeId)
        //               select new FeeViewModel
        //               {
        //                   feeId = data.FeeId,
        //                   feeName = data.FeeName,
        //                   accountCategoryId = data.AccountCategoryId,
        //                   accountCategoryName = data.tbl_Account_Category.AccountCategoryName,
        //                   feeTypeId = data.FeeTypeId,
        //                   feeTypeName = data.tbl_Fee_Type.FeeTypeName,
        //                   feeTypeByAmountRequired = data.tbl_Fee_Type.ByAmountRequired,
        //                   byAmountRequired = data.tbl_Fee_Type.ByAmountRequired,
        //                   feeIntervalId = data.FeeIntervalId,
        //                   feeIntervalName = data.tbl_Fee_Interval.FeeIntervalName,
        //                   productTypeId = data.ProductTypeId,
        //                   productTypeName = data.tbl_Product_Type.ProductTypeName,
        //                   feeTargetId = data.FeeTargetId,
        //                   feeTargetName = data.tbl_Fee_Target.FeeTargetName,
        //                   glAccountId = data.GLAccountId,
        //                   glAccountCode = data.tbl_Chart_Of_Account.AccountCode,
        //                   includeCutOffDay = data.IncludeCutOffDay,
        //                   cutOffDay = data.CutOffDay,
        //                   companyId = data.CompanyId,
        //                   feeDate = data.FeeDate,
        //                   createdBy = data.CreatedBy,
        //               });

        //    if (dataList.Any())
        //    {
        //        fee = fee.Where(x => !dataList.Contains(x.feeId));
        //    }

        //    return fee;
        //}

        public bool UpdateProductFee(int productFeeId, ProductFeeViewModel productFee)
        {
            var productFeeEntity = this.context.tbl_Product_Fee.Find(productFeeId);

            if (productFeeEntity == null)
                return false;

            productFeeEntity.RateValue = productFee.rateValue;
            productFeeEntity.DependentAmount = productFee.dependentAmount;

            productFeeEntity.LastUpdatedBy = productFee.lastUpdatedBy;
            productFeeEntity.DateTimeUpdated = genSetup.GetApplicaionDate();

            // Audit Section ---------------------------
            var product = this.context.tbl_Product.FirstOrDefault(x => x.ProductId == productFee.productId).ProductName;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.ProductFeeUpdated,
                StaffId = productFee.createdBy,
                BranchId = (short)productFee.userBranchId,
                Detail = $"Updated tbl_Product Fee: { productFee.feeName } to product {product} with amount {productFee.rateValue} ",
                IPAddress = productFee.userIPAddress,
                Url = productFee.applicationUrl,
                ApplicationDate = genSetup.GetApplicaionDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            return this.SaveAll();
        }
    }
}